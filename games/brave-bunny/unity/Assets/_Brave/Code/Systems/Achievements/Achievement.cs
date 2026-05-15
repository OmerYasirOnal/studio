// Brave Bunny — Systems / Achievements (Wave 10).
//
// Defines the abstract Achievement base and the 20 concrete launch achievements.
// Each subclass overrides the event handlers it cares about and updates
// internal progress through the base helpers. Thresholds live on the
// AchievementCatalogConfig SO entry (no inline magic numbers per CLAUDE.md
// principle 6). The service binds an AchievementDef into each instance at
// construction.
//
// Design source: docs/02-gdd/02-meta-loop.md (achievement spec).

#nullable enable

using System;
using Brave.Gameplay.Events;

namespace Brave.Systems.Achievements
{
    /// <summary>
    /// Per-achievement progress event surface. Each method is a no-op on the base
    /// class so concrete achievements override only what they care about. The
    /// service iterates the active set and fans each gameplay event through.
    /// </summary>
    public abstract class Achievement
    {
        /// <summary>Catalog row that owns thresholds + reward (data-driven, no magic numbers).</summary>
        public readonly AchievementDef Def;

        /// <summary>Stable id (matches SaveData.Achievements key + catalog id).</summary>
        public string Id => Def.id;

        /// <summary>Required progress count from the catalog row.</summary>
        public int RequiredCount => Def.requiredCount;

        /// <summary>Current progress count (mutated through Add/SetAt-Least helpers).</summary>
        public int CurrentCount { get; protected set; }

        /// <summary>Set once the threshold is crossed; further events are ignored.</summary>
        public bool Unlocked { get; protected set; }

        /// <summary>Reward already moved into the wallet (claimed via the panel).</summary>
        public bool Claimed { get; protected set; }

        protected Achievement(AchievementDef def)
        {
            Def = def ?? throw new ArgumentNullException(nameof(def));
        }

        // ---------------- progress helpers ----------------

        /// <summary>Add delta to the counter; returns true if this call crossed the threshold.</summary>
        protected bool AddProgress(int delta)
        {
            if (Unlocked || delta <= 0) return false;
            CurrentCount += delta;
            if (CurrentCount >= RequiredCount)
            {
                CurrentCount = RequiredCount;
                Unlocked = true;
                return true;
            }
            return false;
        }

        /// <summary>Raise the counter to at-least value; returns true if this crossed the threshold.</summary>
        protected bool SetAtLeast(int value)
        {
            if (Unlocked || value <= CurrentCount) return false;
            CurrentCount = value > RequiredCount ? RequiredCount : value;
            if (CurrentCount >= RequiredCount)
            {
                Unlocked = true;
                return true;
            }
            return false;
        }

        /// <summary>One-shot trigger achievements (boss kill, first evolution): mark unlocked.</summary>
        protected bool TriggerOnce()
        {
            if (Unlocked) return false;
            CurrentCount = RequiredCount;
            Unlocked = true;
            return true;
        }

        // ---------------- event handlers (override what you care about) ----------------

        public virtual bool OnEnemyKilled(in EnemyKilledEvent evt) => false;
        public virtual bool OnRunEnded(in RunEndedEvent evt) => false;
        public virtual bool OnBossDefeated(in BossDefeatedEvent evt) => false;
        public virtual bool OnLevelUp(in LevelUpEvent evt) => false;
        public virtual bool OnWeaponEvolved(in WeaponEvolvedEvent evt) => false;

        // ---------------- persistence seam ----------------

        /// <summary>Hydrate counter + claim flag from save. Marks Unlocked if counter ≥ required.</summary>
        public void Restore(int progress, bool claimed)
        {
            CurrentCount = progress > RequiredCount ? RequiredCount : (progress < 0 ? 0 : progress);
            Claimed = claimed;
            Unlocked = CurrentCount >= RequiredCount || claimed;
        }

        /// <summary>Mark the reward as taken. Idempotent.</summary>
        public bool MarkClaimed()
        {
            if (!Unlocked || Claimed) return false;
            Claimed = true;
            return true;
        }
    }

    // ===================================================================
    // 20 launch achievements — one subclass per row in AchievementCatalog.
    // The id strings here are *defaults*; the catalog overrides on bind so the
    // designer can rename without recompiling. The dispatch is by subclass type.
    // ===================================================================

    /// <summary>1. First Boss Kill — single boss defeat triggers unlock.</summary>
    public sealed class FirstBossKillAchievement : Achievement
    {
        public const string DefaultId = "first-boss-kill";
        public FirstBossKillAchievement(AchievementDef def) : base(def) { }
        public override bool OnBossDefeated(in BossDefeatedEvent evt) => TriggerOnce();
    }

    /// <summary>2. Slayer — 1000 enemy kills total (lifetime).</summary>
    public sealed class SlayerAchievement : Achievement
    {
        public const string DefaultId = "slayer";
        public SlayerAchievement(AchievementDef def) : base(def) { }
        public override bool OnEnemyKilled(in EnemyKilledEvent evt) => AddProgress(1);
    }

    /// <summary>3. Survivor — reach wave 50 in a run (waveReached from RunEndReport).</summary>
    public sealed class SurvivorAchievement : Achievement
    {
        public const string DefaultId = "survivor";
        public SurvivorAchievement(AchievementDef def) : base(def) { }
        public override bool OnRunEnded(in RunEndedEvent evt)
        {
            if (evt.report == null) return false;
            return SetAtLeast(evt.report.wavesCleared);
        }
    }

    /// <summary>4. Untouchable — finish a run without taking damage.
    /// RunEndReport has no damageTaken field yet (cross-team contract); damage
    /// is fed manually via <c>IAchievementService.NotifyDamageTaken()</c> by the
    /// player-health bridge. OnRunEnded checks the per-run damage tally that the
    /// service maintains.</summary>
    public sealed class UntouchableAchievement : Achievement
    {
        public const string DefaultId = "untouchable";
        public int CurrentRunDamage { get; set; }
        public UntouchableAchievement(AchievementDef def) : base(def) { }
        public override bool OnRunEnded(in RunEndedEvent evt)
        {
            if (evt.report == null) return false;
            // Only counts if the run was a Win + zero damage tallied this run.
            if (CurrentRunDamage == 0 &&
                evt.report.outcome == Brave.Gameplay.Run.RunOutcome.Win)
            {
                var crossed = TriggerOnce();
                CurrentRunDamage = 0;
                return crossed;
            }
            CurrentRunDamage = 0;
            return false;
        }
    }

    /// <summary>5. Evolutionist — first weapon evolution triggered.</summary>
    public sealed class EvolutionistAchievement : Achievement
    {
        public const string DefaultId = "evolutionist";
        public EvolutionistAchievement(AchievementDef def) : base(def) { }
        public override bool OnWeaponEvolved(in WeaponEvolvedEvent evt) => TriggerOnce();
    }

    /// <summary>6. Completionist — manual increment (claim all daily rewards once). Hooked by DailyRewardService.</summary>
    public sealed class CompletionistAchievement : Achievement
    {
        public const string DefaultId = "completionist";
        public CompletionistAchievement(AchievementDef def) : base(def) { }
        // Increment via IAchievementService.AddProgress("completionist", n) — DailyRewardService bridge.
    }

    /// <summary>7. Streak Master — combo of 20 (manual, no event channel yet). Wired through AddProgress.</summary>
    public sealed class StreakMasterAchievement : Achievement
    {
        public const string DefaultId = "streak-master";
        public StreakMasterAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>8. Crit Lord — 300 lifetime crits. Crit channel still pending; manual increment.</summary>
    public sealed class CritLordAchievement : Achievement
    {
        public const string DefaultId = "crit-lord";
        public CritLordAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>9. Treasure Hunter — 10000 gold lifetime. Pickup channel still pending; manual increment.</summary>
    public sealed class TreasureHunterAchievement : Achievement
    {
        public const string DefaultId = "treasure-hunter";
        public TreasureHunterAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>10. Star Collector — 100 stars lifetime. Currency channel pending; manual increment.</summary>
    public sealed class StarCollectorAchievement : Achievement
    {
        public const string DefaultId = "star-collector";
        public StarCollectorAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>11. Variety — use 6 different weapons. Tracked via manual AddProgress.</summary>
    public sealed class VarietyAchievement : Achievement
    {
        public const string DefaultId = "variety";
        public VarietyAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>12. Iron Player — 1 hour total playtime (seconds). Stats.runDurationSeconds aggregated.</summary>
    public sealed class IronPlayerAchievement : Achievement
    {
        public const string DefaultId = "iron-player";
        public IronPlayerAchievement(AchievementDef def) : base(def) { }
        public override bool OnRunEnded(in RunEndedEvent evt)
        {
            if (evt.report == null || evt.report.runDurationSeconds <= 0) return false;
            // Add seconds to the counter; threshold is the required total in seconds.
            return AddProgress(UnityEngine.Mathf.RoundToInt(evt.report.runDurationSeconds));
        }
    }

    /// <summary>13. Marathon — single run lasting > threshold seconds (e.g. 480 = 8 min).</summary>
    public sealed class MarathonAchievement : Achievement
    {
        public const string DefaultId = "marathon";
        public MarathonAchievement(AchievementDef def) : base(def) { }
        public override bool OnRunEnded(in RunEndedEvent evt)
        {
            if (evt.report == null) return false;
            if (evt.report.runDurationSeconds > RequiredCount) return TriggerOnce();
            return false;
        }
    }

    /// <summary>14. Speed Run — clear wave-30 in &lt; 300s. RequiredCount carries the wave target;
    /// the 5-min ceiling is the catalog's secondaryThreshold field.</summary>
    public sealed class SpeedRunAchievement : Achievement
    {
        public const string DefaultId = "speed-run";
        public SpeedRunAchievement(AchievementDef def) : base(def) { }
        public override bool OnRunEnded(in RunEndedEvent evt)
        {
            if (evt.report == null) return false;
            if (evt.report.wavesCleared >= RequiredCount &&
                evt.report.runDurationSeconds > 0f &&
                evt.report.runDurationSeconds <= Def.secondaryThreshold)
            {
                return TriggerOnce();
            }
            return false;
        }
    }

    /// <summary>15. Premium Buyer — purchased the battle-pass. Driven by IapService bridge → AddProgress.</summary>
    public sealed class PremiumBuyerAchievement : Achievement
    {
        public const string DefaultId = "premium-buyer";
        public PremiumBuyerAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>16. Generous — 500 stars donated to a character unlock. CharacterUnlockService bridge.</summary>
    public sealed class GenerousAchievement : Achievement
    {
        public const string DefaultId = "generous";
        public GenerousAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>17. Loyal — login 7 days in a row. DailyStreakService bridge.</summary>
    public sealed class LoyalAchievement : Achievement
    {
        public const string DefaultId = "loyal";
        public LoyalAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>18. Quest Master — 30 daily quests claimed total. QuestService bridge.</summary>
    public sealed class QuestMasterAchievement : Achievement
    {
        public const string DefaultId = "quest-master";
        public QuestMasterAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>19. World Tour — clear all 3 wired biomes. BiomeRegistry bridge sends 1 per fresh biome clear.</summary>
    public sealed class WorldTourAchievement : Achievement
    {
        public const string DefaultId = "world-tour";
        public WorldTourAchievement(AchievementDef def) : base(def) { }
    }

    /// <summary>20. Bossbane — defeat any boss 10 times (lifetime).</summary>
    public sealed class BossbaneAchievement : Achievement
    {
        public const string DefaultId = "bossbane";
        public BossbaneAchievement(AchievementDef def) : base(def) { }
        public override bool OnBossDefeated(in BossDefeatedEvent evt) => AddProgress(1);
    }
}
