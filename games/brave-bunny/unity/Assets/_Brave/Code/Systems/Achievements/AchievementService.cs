// Brave Bunny — Systems / Achievements (Wave 10).
//
// AchievementService owns the runtime set of 20 Achievement instances, fans
// gameplay events into each, persists progress to SaveData.Achievements, and
// fires AchievementUnlockedEvent the moment a threshold is crossed. The toast
// + panel controllers subscribe to the channel for UI presentation.
//
// Design choices:
//   * One service for ALL achievements (no per-kind class explosion at the
//     dispatch layer — the kind enum + factory in AchievementCatalogConfig
//     handles instantiation). The 20 subclasses live in Achievement.cs.
//   * Progress persists in the existing SaveData.Achievements<string, AchievementEntry>
//     dictionary — no schema change.
//   * Save() is called only on Claim (matches the AchievementService /
//     QuestService pattern of "batched at RunEnd, eager on Claim").
//   * Subscribes to: EnemyKilledChannel, RunEndedChannel, BossDefeatedChannel,
//     LevelUpChannel, WeaponEvolvedChannel. AchievementUnlockedChannel is
//     OUTPUT-side and may be null in CI (service then quietly skips raising).
//
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md (achievement spec).
//   * docs/06-tech-spec/03-save-system.md trigger "Achievement claimed".

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using Brave.Systems.Save;

namespace Brave.Systems.Achievements
{
    public interface IAchievementService : IService
    {
        IReadOnlyList<Achievement> All { get; }
        Achievement? Get(string id);
        int GetProgress(string id);
        bool IsUnlocked(string id);
        bool IsClaimed(string id);

        /// <summary>Manual progress increment (for achievements without a wired event channel).</summary>
        bool AddProgress(string id, int delta);

        /// <summary>Manual threshold raise (level / wave-style absolute progress).</summary>
        bool SetAtLeast(string id, int value);

        /// <summary>Notify a damage tick for the Untouchable per-run tracker.</summary>
        void NotifyDamageTaken(int amount);

        /// <summary>Try to claim the reward. Returns the granted (currency, amount) tuple; (0,0) on failure.</summary>
        (CurrencyType currency, int amount) TryClaim(string id);

        /// <summary>Raised whenever an achievement's progress or claim flag changes.</summary>
        event Action<Achievement>? AchievementChanged;
    }

    public sealed class AchievementService : IAchievementService
    {
        private readonly ISaveService _save;
        private readonly AchievementCatalogConfig? _config;
        private readonly ICurrencyService? _currency;
        private readonly AchievementUnlockedChannel? _unlockedChannel;

        private readonly List<Achievement> _all = new();
        private readonly Dictionary<string, Achievement> _byId = new(StringComparer.Ordinal);

        public IReadOnlyList<Achievement> All => _all;

        public event Action<Achievement>? AchievementChanged;

        public AchievementService(
            ISaveService save,
            AchievementCatalogConfig? config,
            ICurrencyService? currency = null,
            AchievementUnlockedChannel? unlockedChannel = null)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _config = config;
            _currency = currency;
            _unlockedChannel = unlockedChannel;
            Hydrate();
        }

        // ---------------- hydration / persistence ----------------

        private void Hydrate()
        {
            _all.Clear();
            _byId.Clear();
            if (_config == null) return;
            foreach (var def in _config.Entries)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                if (_byId.ContainsKey(def.id)) continue; // dedupe
                var a = AchievementCatalogConfig.Create(def);
                if (_save.Data.Achievements.TryGetValue(def.id, out var entry))
                {
                    a.Restore(entry.Progress, entry.Claimed);
                }
                _all.Add(a);
                _byId[def.id] = a;
            }
        }

        private void Persist(Achievement a)
        {
            if (!_save.Data.Achievements.TryGetValue(a.Id, out var entry))
            {
                entry = new SaveData.AchievementEntry();
                _save.Data.Achievements[a.Id] = entry;
            }
            entry.Progress = a.CurrentCount;
            entry.Claimed = a.Claimed;
            if (a.Unlocked && entry.CompletedAt == null)
                entry.CompletedAt = DateTime.UtcNow.ToString("o");
        }

        // ---------------- query ----------------

        public Achievement? Get(string id) =>
            _byId.TryGetValue(id, out var a) ? a : null;

        public int GetProgress(string id) =>
            _byId.TryGetValue(id, out var a) ? a.CurrentCount : 0;

        public bool IsUnlocked(string id) =>
            _byId.TryGetValue(id, out var a) && a.Unlocked;

        public bool IsClaimed(string id) =>
            _byId.TryGetValue(id, out var a) && a.Claimed;

        // ---------------- manual progress ----------------

        public bool AddProgress(string id, int delta)
        {
            if (!_byId.TryGetValue(id, out var a)) return false;
            if (a.Unlocked || delta <= 0) return false;
            var wasUnlocked = a.Unlocked;
            // Use the public reset/restore pathway: add via Restore(+delta).
            var newCount = a.CurrentCount + delta;
            a.Restore(newCount, a.Claimed);
            Persist(a);
            AchievementChanged?.Invoke(a);
            if (!wasUnlocked && a.Unlocked) FireUnlocked(a);
            return a.Unlocked;
        }

        public bool SetAtLeast(string id, int value)
        {
            if (!_byId.TryGetValue(id, out var a)) return false;
            if (a.Unlocked || value <= a.CurrentCount) return false;
            var wasUnlocked = a.Unlocked;
            a.Restore(value, a.Claimed);
            Persist(a);
            AchievementChanged?.Invoke(a);
            if (!wasUnlocked && a.Unlocked) FireUnlocked(a);
            return a.Unlocked;
        }

        public void NotifyDamageTaken(int amount)
        {
            if (amount <= 0) return;
            if (Get(UntouchableAchievement.DefaultId) is UntouchableAchievement u)
                u.CurrentRunDamage += amount;
        }

        // ---------------- claim ----------------

        public (CurrencyType currency, int amount) TryClaim(string id)
        {
            if (!_byId.TryGetValue(id, out var a)) return (default, 0);
            if (!a.Unlocked || a.Claimed) return (default, 0);
            if (!a.MarkClaimed()) return (default, 0);

            // Grant reward (currency service preferred; fallback to direct wallet write).
            var amount = a.Def.rewardAmount;
            var currency = a.Def.rewardCurrency;
            if (amount > 0)
            {
                if (_currency != null)
                {
                    _currency.Add(currency, amount, persist: false);
                }
                else
                {
                    var c = _save.Data.Currencies;
                    switch (currency)
                    {
                        case CurrencyType.Carrots: c.Carrots += amount; break;
                        case CurrencyType.Stars: c.Stars += amount; break;
                        case CurrencyType.SoulShards: c.SoulShards += amount; break;
                    }
                }
            }

            Persist(a);
            _save.Save(); // 03-save-system.md trigger: "Achievement claimed".
            AchievementChanged?.Invoke(a);
            return (currency, amount);
        }

        // ---------------- event channel routing ----------------

        public void OnEnemyKilled(in EnemyKilledEvent evt)
        {
            for (var i = 0; i < _all.Count; i++)
            {
                var a = _all[i];
                if (a.Unlocked) continue;
                var beforeCount = a.CurrentCount;
                var changed = a.OnEnemyKilled(in evt);
                if (changed || a.CurrentCount != beforeCount)
                {
                    Persist(a);
                    AchievementChanged?.Invoke(a);
                }
                if (changed && a.Unlocked) FireUnlocked(a);
            }
        }

        public void OnRunEnded(in RunEndedEvent evt)
        {
            for (var i = 0; i < _all.Count; i++)
            {
                var a = _all[i];
                if (a.Unlocked)
                {
                    // Still reset the untouchable per-run tally even if already unlocked.
                    if (a is UntouchableAchievement u) u.CurrentRunDamage = 0;
                    continue;
                }
                var beforeCount = a.CurrentCount;
                var changed = a.OnRunEnded(in evt);
                if (changed || a.CurrentCount != beforeCount)
                {
                    Persist(a);
                    AchievementChanged?.Invoke(a);
                }
                if (changed && a.Unlocked) FireUnlocked(a);
            }
        }

        public void OnBossDefeated(in BossDefeatedEvent evt)
        {
            for (var i = 0; i < _all.Count; i++)
            {
                var a = _all[i];
                if (a.Unlocked) continue;
                var beforeCount = a.CurrentCount;
                var changed = a.OnBossDefeated(in evt);
                if (changed || a.CurrentCount != beforeCount)
                {
                    Persist(a);
                    AchievementChanged?.Invoke(a);
                }
                if (changed && a.Unlocked) FireUnlocked(a);
            }
        }

        public void OnLevelUp(in LevelUpEvent evt)
        {
            for (var i = 0; i < _all.Count; i++)
            {
                var a = _all[i];
                if (a.Unlocked) continue;
                var beforeCount = a.CurrentCount;
                var changed = a.OnLevelUp(in evt);
                if (changed || a.CurrentCount != beforeCount)
                {
                    Persist(a);
                    AchievementChanged?.Invoke(a);
                }
                if (changed && a.Unlocked) FireUnlocked(a);
            }
        }

        public void OnWeaponEvolved(in WeaponEvolvedEvent evt)
        {
            for (var i = 0; i < _all.Count; i++)
            {
                var a = _all[i];
                if (a.Unlocked) continue;
                var beforeCount = a.CurrentCount;
                var changed = a.OnWeaponEvolved(in evt);
                if (changed || a.CurrentCount != beforeCount)
                {
                    Persist(a);
                    AchievementChanged?.Invoke(a);
                }
                if (changed && a.Unlocked) FireUnlocked(a);
            }
        }

        private void FireUnlocked(Achievement a)
        {
            if (_unlockedChannel == null) return;
            _unlockedChannel.Raise(new AchievementUnlockedEvent(
                a.Id,
                a.Def.EffectiveDisplayKey,
                (int)a.Def.rewardCurrency,
                a.Def.rewardAmount));
        }

        // ---------------- wiring helpers ----------------

        /// <summary>
        /// Wire engine event channels to this service. Returns an Action that
        /// unsubscribes — callers (typically GameContextBootstrap) hold it for
        /// teardown. Null channels are silently skipped.
        /// </summary>
        public Action SubscribeEventChannels(
            EnemyKilledChannel? enemyKilled,
            RunEndedChannel? runEnded,
            BossDefeatedChannel? bossDefeated,
            LevelUpChannel? levelUp,
            WeaponEvolvedChannel? weaponEvolved)
        {
            Action<EnemyKilledEvent>? onEnemy = null;
            Action<RunEndedEvent>? onRunEnd = null;
            Action<BossDefeatedEvent>? onBoss = null;
            Action<LevelUpEvent>? onLevelUp = null;
            Action<WeaponEvolvedEvent>? onEvo = null;

            if (enemyKilled != null)
            {
                onEnemy = e => OnEnemyKilled(in e);
                enemyKilled.Subscribe(onEnemy);
            }
            if (runEnded != null)
            {
                onRunEnd = e => OnRunEnded(in e);
                runEnded.Subscribe(onRunEnd);
            }
            if (bossDefeated != null)
            {
                onBoss = e => OnBossDefeated(in e);
                bossDefeated.Subscribe(onBoss);
            }
            if (levelUp != null)
            {
                onLevelUp = e => OnLevelUp(in e);
                levelUp.Subscribe(onLevelUp);
            }
            if (weaponEvolved != null)
            {
                onEvo = e => OnWeaponEvolved(in e);
                weaponEvolved.Subscribe(onEvo);
            }

            return () =>
            {
                if (enemyKilled != null && onEnemy != null) enemyKilled.Unsubscribe(onEnemy);
                if (runEnded != null && onRunEnd != null) runEnded.Unsubscribe(onRunEnd);
                if (bossDefeated != null && onBoss != null) bossDefeated.Unsubscribe(onBoss);
                if (levelUp != null && onLevelUp != null) levelUp.Unsubscribe(onLevelUp);
                if (weaponEvolved != null && onEvo != null) weaponEvolved.Unsubscribe(onEvo);
            };
        }
    }
}
