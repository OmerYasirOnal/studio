// Brave Bunny — Systems / LiveOps
// Wave 9: Daily quest/mission system (3 quests rotating at UTC midnight).
//
// Design source: docs/02-gdd/02-meta-loop.md (daily missions cadence)
// Tech spec:    docs/06-tech-spec/03-save-system.md (questState payload field)
// CLAUDE.md principle 6: no magic numbers — all targets/rewards live in
// QuestPoolConfig SO, never inlined.
//
// Quest is a small in-memory object the QuestService rolls each day from
// templates. It tracks progress against an integer counter and exposes an
// IQuestProgressEvent dispatch so subclasses can listen on the typed event
// channels (EnemyKilledChannel, LevelUpChannel, ...). Persistence is owned
// by QuestService — Quest itself is a pure POCO.

#nullable enable

using System;
using Brave.Gameplay.Events;

namespace Brave.Systems.LiveOps
{
    /// <summary>Quest category — drives which event channel feeds progress.</summary>
    public enum QuestType
    {
        KillEnemies = 0,
        SurviveWaves = 1,
        DefeatBoss = 2,
        ReachLevel = 3,
        CollectGold = 4,
        RunDuration = 5,
    }

    /// <summary>Difficulty tag — QuestPool picks 1 of each per day.</summary>
    public enum QuestDifficulty { Easy = 0, Medium = 1, Hard = 2 }

    /// <summary>Currency + amount granted on claim (mirrors QuestPoolConfig template).</summary>
    public readonly struct QuestReward
    {
        public readonly Brave.Systems.Progression.CurrencyType Currency;
        public readonly int Amount;
        public QuestReward(Brave.Systems.Progression.CurrencyType currency, int amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }

    /// <summary>Marker for any event a Quest might consume (kept open for future events).</summary>
    public interface IQuestProgressEvent { }

    // ----- Adapter payloads (zero-alloc, mirror engine events) -----

    public readonly struct EnemyKilledProgress : IQuestProgressEvent
    {
        public readonly bool wasElite;
        public EnemyKilledProgress(bool wasElite) { this.wasElite = wasElite; }
    }

    public readonly struct WaveClearedProgress : IQuestProgressEvent
    {
        public readonly int wavesCleared;
        public WaveClearedProgress(int wavesCleared) { this.wavesCleared = wavesCleared; }
    }

    public readonly struct BossDefeatedProgress : IQuestProgressEvent
    {
        public readonly string bossId;
        public BossDefeatedProgress(string bossId) { this.bossId = bossId; }
    }

    public readonly struct LevelReachedProgress : IQuestProgressEvent
    {
        public readonly int newLevel;
        public LevelReachedProgress(int newLevel) { this.newLevel = newLevel; }
    }

    public readonly struct GoldCollectedProgress : IQuestProgressEvent
    {
        public readonly int amount;
        public GoldCollectedProgress(int amount) { this.amount = amount; }
    }

    public readonly struct RunDurationProgress : IQuestProgressEvent
    {
        public readonly float seconds;
        public RunDurationProgress(float seconds) { this.seconds = seconds; }
    }

    /// <summary>
    /// Abstract base for a single quest instance. Subclasses translate a
    /// concrete <see cref="IQuestProgressEvent"/> into <c>currentCount</c>
    /// deltas. <see cref="Id"/> is stable across a UTC day so the save can
    /// reconcile persisted progress on reload.
    /// </summary>
    public abstract class Quest
    {
        /// <summary>Stable template slug (e.g. "kill_30_enemies_easy").</summary>
        public string Id { get; }
        public QuestType Type { get; }
        public QuestDifficulty Difficulty { get; }
        public int RequiredCount { get; }
        public int CurrentCount { get; private set; }
        public bool Claimed { get; private set; }
        public QuestReward Reward { get; }
        /// <summary>Localization key for the quest title (e.g. "quest.kill_enemies.title").</summary>
        public string TitleLocKey { get; }

        protected Quest(
            string id,
            QuestType type,
            QuestDifficulty difficulty,
            int requiredCount,
            QuestReward reward,
            string titleLocKey)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("id required", nameof(id));
            if (requiredCount <= 0) throw new ArgumentOutOfRangeException(nameof(requiredCount));
            Id = id;
            Type = type;
            Difficulty = difficulty;
            RequiredCount = requiredCount;
            Reward = reward;
            TitleLocKey = titleLocKey ?? string.Empty;
        }

        /// <summary>True once <see cref="CurrentCount"/> &gt;= <see cref="RequiredCount"/>.</summary>
        public bool IsComplete => CurrentCount >= RequiredCount;

        /// <summary>True iff complete and not yet claimed.</summary>
        public bool IsClaimable => IsComplete && !Claimed;

        /// <summary>0..1 inclusive progress ratio.</summary>
        public float Progress01 => RequiredCount <= 0
            ? 0f
            : Math.Min(1f, (float)CurrentCount / RequiredCount);

        /// <summary>Test/persistence hook — restore progress without going through events.</summary>
        public void RestoreState(int currentCount, bool claimed)
        {
            CurrentCount = Math.Max(0, Math.Min(currentCount, RequiredCount));
            Claimed = claimed;
        }

        /// <summary>Bump the counter; clamped at <see cref="RequiredCount"/>.</summary>
        protected void AddProgress(int delta)
        {
            if (Claimed) return;
            if (delta <= 0) return;
            CurrentCount = Math.Min(RequiredCount, CurrentCount + delta);
        }

        /// <summary>Set absolute value (used by RunDuration / ReachLevel which compare-and-set).</summary>
        protected void SetProgressAtLeast(int value)
        {
            if (Claimed) return;
            if (value <= CurrentCount) return;
            CurrentCount = Math.Min(RequiredCount, value);
        }

        /// <summary>Apply an event payload — subclasses pattern-match the struct.</summary>
        public abstract void OnEvent(IQuestProgressEvent evt);

        /// <summary>Mark as claimed; idempotent. Returns false if not claimable.</summary>
        internal bool TryMarkClaimed()
        {
            if (Claimed || !IsComplete) return false;
            Claimed = true;
            return true;
        }
    }

    // ---------- Concrete quest types ----------

    public sealed class KillEnemiesQuest : Quest
    {
        public KillEnemiesQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey)
            : base(id, QuestType.KillEnemies, d, required, reward, locKey) { }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is EnemyKilledProgress) AddProgress(1);
        }
    }

    public sealed class SurviveWavesQuest : Quest
    {
        public SurviveWavesQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey)
            : base(id, QuestType.SurviveWaves, d, required, reward, locKey) { }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is WaveClearedProgress w) SetProgressAtLeast(w.wavesCleared);
        }
    }

    public sealed class DefeatBossQuest : Quest
    {
        /// <summary>Optional boss slug filter; empty = any boss counts.</summary>
        public string BossFilter { get; }

        public DefeatBossQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey, string bossFilter = "")
            : base(id, QuestType.DefeatBoss, d, required, reward, locKey)
        {
            BossFilter = bossFilter ?? string.Empty;
        }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is BossDefeatedProgress b)
            {
                if (BossFilter.Length > 0 && !string.Equals(BossFilter, b.bossId, StringComparison.Ordinal)) return;
                AddProgress(1);
            }
        }
    }

    public sealed class ReachLevelQuest : Quest
    {
        public ReachLevelQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey)
            : base(id, QuestType.ReachLevel, d, required, reward, locKey) { }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is LevelReachedProgress l) SetProgressAtLeast(l.newLevel);
        }
    }

    public sealed class CollectGoldQuest : Quest
    {
        public CollectGoldQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey)
            : base(id, QuestType.CollectGold, d, required, reward, locKey) { }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is GoldCollectedProgress g) AddProgress(g.amount);
        }
    }

    public sealed class RunDurationQuest : Quest
    {
        public RunDurationQuest(string id, QuestDifficulty d, int required, QuestReward reward, string locKey)
            : base(id, QuestType.RunDuration, d, required, reward, locKey) { }

        public override void OnEvent(IQuestProgressEvent evt)
        {
            if (evt is RunDurationProgress r) SetProgressAtLeast((int)Math.Floor(r.seconds));
        }
    }
}
