// Brave Bunny — Systems / LiveOps
// Wave 9: QuestService — 3 daily quests rolling at UTC midnight.
//
// Responsibilities:
//   * Roll a deterministic 3-quest set per UTC day from QuestPoolConfig SO.
//   * Reconcile in-memory state with persisted progress/claims in SaveData.QuestState.
//   * Receive IQuestProgressEvent events (kills / waves / boss / level / gold / duration)
//     and fan-out to each active quest.
//   * On Claim: grant reward via CurrencyService (or save delta directly when
//     no CurrencyService is registered yet, e.g. EditMode tests).
//
// Persistence: SaveData.QuestState carries rolledForDate + per-quest progress/claim
// flags. Same shape as DailyMissionsSection but kept independent so the older
// "missions" field can ship in parallel (BattlePass / DailyRewards agents may use
// it). The service migrates a save lazily — missing fields = fresh state.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using Brave.Systems.Save;

namespace Brave.Systems.LiveOps
{
    public interface IQuestService : IService
    {
        /// <summary>Returns today's 3 quests (may include nulls if the pool is empty).</summary>
        Quest?[] GetTodaysQuests();

        /// <summary>Dispatch a progress event to all active quests.</summary>
        void OnEvent(IQuestProgressEvent evt);

        /// <summary>Try to claim a quest reward. Returns the reward struct on success; <c>(None,0)</c> on failure.</summary>
        QuestReward Claim(string questId);

        /// <summary>Raised whenever a quest's state changes (progress / claim).</summary>
        event Action<Quest>? QuestUpdated;
    }

    public sealed class QuestService : IQuestService
    {
        private readonly ISaveService _save;
        private readonly QuestPoolConfig? _config;
        private readonly ICurrencyService? _currency;
        private readonly Func<DateTime> _utcNow;

        private Quest?[] _today = Array.Empty<Quest?>();
        private string _rolledForDate = string.Empty;

        public event Action<Quest>? QuestUpdated;

        public QuestService(ISaveService save, QuestPoolConfig? config, ICurrencyService? currency = null, Func<DateTime>? utcNow = null)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _config = config;
            _currency = currency;
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
            EnsureRolledForToday();
        }

        public Quest?[] GetTodaysQuests()
        {
            EnsureRolledForToday();
            return _today;
        }

        public void OnEvent(IQuestProgressEvent evt)
        {
            if (evt == null) return;
            EnsureRolledForToday();
            var any = false;
            for (var i = 0; i < _today.Length; i++)
            {
                var q = _today[i];
                if (q == null) continue;
                var before = q.CurrentCount;
                q.OnEvent(evt);
                if (q.CurrentCount != before)
                {
                    PersistInto(_save.Data.QuestState, q);
                    QuestUpdated?.Invoke(q);
                    any = true;
                }
            }
            // Progress ticks are not save-flushed per-event (matches the
            // achievement pattern: batched at RunEnd). _save.Save() is only
            // called on Claim.
            _ = any;
        }

        public QuestReward Claim(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return default;
            EnsureRolledForToday();
            for (var i = 0; i < _today.Length; i++)
            {
                var q = _today[i];
                if (q == null || !string.Equals(q.Id, questId, StringComparison.Ordinal)) continue;
                if (!q.TryMarkClaimed()) return default;
                GrantReward(q.Reward);
                PersistInto(_save.Data.QuestState, q);
                _save.Save();
                QuestUpdated?.Invoke(q);
                return q.Reward;
            }
            return default;
        }

        // ---------------- rollover + persistence ----------------

        private void EnsureRolledForToday()
        {
            var today = _utcNow().Date.ToString("yyyy-MM-dd");
            if (string.Equals(_rolledForDate, today, StringComparison.Ordinal) && _today.Length > 0) return;
            RollFor(today);
        }

        private void RollFor(string today)
        {
            var state = _save.Data.QuestState;
            var persistedDate = state.RolledForDate ?? string.Empty;
            var sameDay = string.Equals(persistedDate, today, StringComparison.Ordinal);

            // Roll quest set. When config is null (CI / EditMode w/o assets),
            // produce an empty array so consumers handle the no-quest case.
            var playerId = _save.Data.Player.Id ?? string.Empty;
            var rolled = _config != null
                ? QuestPool.RollDaily(_config, playerId, _utcNow())
                : new Quest?[QuestPool.QuestsPerDay];

            if (sameDay)
            {
                // Reconcile persisted entries by id.
                foreach (var entry in state.Entries)
                {
                    for (var i = 0; i < rolled.Length; i++)
                    {
                        var q = rolled[i];
                        if (q == null) continue;
                        if (!string.Equals(q.Id, entry.Id, StringComparison.Ordinal)) continue;
                        q.RestoreState(entry.Progress, entry.Claimed);
                    }
                }
            }
            else
            {
                // New UTC day → wipe persisted entries.
                state.Entries.Clear();
                state.RolledForDate = today;
            }

            // Ensure the save mirrors the (possibly new) roll.
            SyncEntries(state, rolled);

            _today = rolled;
            _rolledForDate = today;
        }

        private static void SyncEntries(SaveData.QuestState state, Quest?[] rolled)
        {
            // Keep entries that still match a rolled quest; add missing ones.
            var keep = new List<SaveData.QuestEntry>(rolled.Length);
            foreach (var entry in state.Entries)
            {
                for (var i = 0; i < rolled.Length; i++)
                {
                    var q = rolled[i];
                    if (q != null && string.Equals(q.Id, entry.Id, StringComparison.Ordinal))
                    {
                        keep.Add(entry);
                        break;
                    }
                }
            }
            state.Entries.Clear();
            state.Entries.AddRange(keep);

            for (var i = 0; i < rolled.Length; i++)
            {
                var q = rolled[i];
                if (q == null) continue;
                var found = false;
                foreach (var entry in state.Entries)
                {
                    if (string.Equals(entry.Id, q.Id, StringComparison.Ordinal)) { found = true; break; }
                }
                if (!found)
                {
                    state.Entries.Add(new SaveData.QuestEntry
                    {
                        Id = q.Id,
                        Progress = q.CurrentCount,
                        Claimed = q.Claimed,
                    });
                }
            }
        }

        private static void PersistInto(SaveData.QuestState state, Quest q)
        {
            foreach (var entry in state.Entries)
            {
                if (string.Equals(entry.Id, q.Id, StringComparison.Ordinal))
                {
                    entry.Progress = q.CurrentCount;
                    entry.Claimed = q.Claimed;
                    return;
                }
            }
            state.Entries.Add(new SaveData.QuestEntry
            {
                Id = q.Id,
                Progress = q.CurrentCount,
                Claimed = q.Claimed,
            });
        }

        private void GrantReward(QuestReward reward)
        {
            if (reward.Amount <= 0) return;
            if (_currency != null)
            {
                _currency.Add(reward.Currency, reward.Amount, persist: false);
                return;
            }
            // No currency service registered → mutate the wallet section directly so
            // EditMode tests can still observe the side-effect without spinning up
            // CurrencyService + Wallet plumbing.
            var c = _save.Data.Currencies;
            switch (reward.Currency)
            {
                case CurrencyType.Carrots: c.Carrots += reward.Amount; break;
                case CurrencyType.Stars: c.Stars += reward.Amount; break;
                case CurrencyType.SoulShards: c.SoulShards += reward.Amount; break;
            }
        }

        // ---------------- event-channel subscription helpers ----------------

        /// <summary>
        /// Wire engine event channels to this service. Returns an Action that
        /// unsubscribes — callers (typically GameContextBootstrap) hold it for
        /// teardown. Any null channel is silently skipped, so CI builds that
        /// haven't wired every SO still boot.
        /// </summary>
        public Action SubscribeEventChannels(
            EnemyKilledChannel? enemyKilled,
            LevelUpChannel? levelUp,
            BossDefeatedChannel? bossDefeated,
            PickupChannel? pickup)
        {
            Action<EnemyKilledEvent>? onEnemy = null;
            Action<LevelUpEvent>? onLevel = null;
            Action<BossDefeatedEvent>? onBoss = null;
            Action<PickupEvent>? onPickup = null;

            if (enemyKilled != null)
            {
                onEnemy = e => OnEvent(new EnemyKilledProgress(e.wasElite));
                enemyKilled.Subscribe(onEnemy);
            }
            if (levelUp != null)
            {
                onLevel = e => OnEvent(new LevelReachedProgress(e.newLevel));
                levelUp.Subscribe(onLevel);
            }
            if (bossDefeated != null)
            {
                onBoss = e => OnEvent(new BossDefeatedProgress(e.bossId ?? string.Empty));
                bossDefeated.Subscribe(onBoss);
            }
            if (pickup != null)
            {
                onPickup = e =>
                {
                    if (e.kind == PickupKind.GoldCoin) OnEvent(new GoldCollectedProgress(e.amount));
                };
                pickup.Subscribe(onPickup);
            }

            return () =>
            {
                if (enemyKilled != null && onEnemy != null) enemyKilled.Unsubscribe(onEnemy);
                if (levelUp != null && onLevel != null) levelUp.Unsubscribe(onLevel);
                if (bossDefeated != null && onBoss != null) bossDefeated.Unsubscribe(onBoss);
                if (pickup != null && onPickup != null) pickup.Unsubscribe(onPickup);
            };
        }
    }
}
