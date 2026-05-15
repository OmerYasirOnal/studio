// Brave Bunny — Systems / Achievements (Wave 10).
//
// ScriptableObject catalogue describing the 20 launch achievements. Authored
// in the inspector at Data/Definitions/Achievements/AchievementCatalog.asset.
// Per CLAUDE.md principle 6: every threshold + reward lives in this SO (no
// magic numbers inside Achievement.cs subclasses).
//
// Design source: docs/02-gdd/02-meta-loop.md.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Progression;
using UnityEngine;

namespace Brave.Systems.Achievements
{
    /// <summary>
    /// Maps an <see cref="AchievementDef"/> to one of the concrete
    /// <see cref="Achievement"/> subclasses. Stays as a string-typed key on the
    /// catalog row so designers can reorder the inspector list freely; the
    /// service uses this token to instantiate the correct subclass.
    /// </summary>
    public enum AchievementKind
    {
        FirstBossKill = 0,
        Slayer = 1,
        Survivor = 2,
        Untouchable = 3,
        Evolutionist = 4,
        Completionist = 5,
        StreakMaster = 6,
        CritLord = 7,
        TreasureHunter = 8,
        StarCollector = 9,
        Variety = 10,
        IronPlayer = 11,
        Marathon = 12,
        SpeedRun = 13,
        PremiumBuyer = 14,
        Generous = 15,
        Loyal = 16,
        QuestMaster = 17,
        WorldTour = 18,
        Bossbane = 19,
    }

    /// <summary>
    /// Inspector-friendly catalog row. Mirrors what one Achievement instance
    /// needs to know at construction time. Loc keys follow the agreed Wave 10
    /// convention: <c>achievement.&lt;id&gt;.name</c> + <c>.description</c>.
    /// </summary>
    [Serializable]
    public sealed class AchievementDef
    {
        [Tooltip("Stable slug. Becomes the SaveData.Achievements key + the .name/.description loc key suffix.")]
        public string id = string.Empty;

        [Tooltip("Which Achievement subclass to instantiate.")]
        public AchievementKind kind;

        [Tooltip("Target count (kills / survives / seconds / waves / collects).")]
        [Min(1)] public int requiredCount = 1;

        [Tooltip("Reward currency on claim.")]
        public CurrencyType rewardCurrency = CurrencyType.Stars;

        [Tooltip("Reward amount on claim (0 = no currency reward).")]
        [Min(0)] public int rewardAmount;

        [Tooltip("Loc key for the display name. Defaults to achievement.<id>.name when empty.")]
        public string displayKey = string.Empty;

        [Tooltip("Loc key for the description. Defaults to achievement.<id>.description when empty.")]
        public string descriptionKey = string.Empty;

        [Tooltip("Optional second threshold (e.g. Speed Run uses this for the time-cap in seconds).")]
        public int secondaryThreshold;

        /// <summary>Effective name loc key — fallback to convention pattern.</summary>
        public string EffectiveDisplayKey =>
            string.IsNullOrEmpty(displayKey) ? $"achievement.{id}.name" : displayKey;

        /// <summary>Effective description loc key — fallback to convention pattern.</summary>
        public string EffectiveDescriptionKey =>
            string.IsNullOrEmpty(descriptionKey) ? $"achievement.{id}.description" : descriptionKey;
    }

    /// <summary>
    /// Wave-10 master catalog: the 20 launch achievements. Order in the
    /// inspector controls panel display order.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Achievements/Catalog Config", fileName = "AchievementCatalogConfig", order = 11)]
    public sealed class AchievementCatalogConfig : ScriptableObject
    {
        [Tooltip("Master achievement list. Service instantiates one Achievement per row.")]
        public List<AchievementDef> entries = new();

        /// <summary>Read-only enumerable for service consumption / tests.</summary>
        public IReadOnlyList<AchievementDef> Entries => entries;

        /// <summary>Factory: build a concrete Achievement for the given catalog row.</summary>
        public static Achievement Create(AchievementDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.kind switch
            {
                AchievementKind.FirstBossKill => new FirstBossKillAchievement(def),
                AchievementKind.Slayer => new SlayerAchievement(def),
                AchievementKind.Survivor => new SurvivorAchievement(def),
                AchievementKind.Untouchable => new UntouchableAchievement(def),
                AchievementKind.Evolutionist => new EvolutionistAchievement(def),
                AchievementKind.Completionist => new CompletionistAchievement(def),
                AchievementKind.StreakMaster => new StreakMasterAchievement(def),
                AchievementKind.CritLord => new CritLordAchievement(def),
                AchievementKind.TreasureHunter => new TreasureHunterAchievement(def),
                AchievementKind.StarCollector => new StarCollectorAchievement(def),
                AchievementKind.Variety => new VarietyAchievement(def),
                AchievementKind.IronPlayer => new IronPlayerAchievement(def),
                AchievementKind.Marathon => new MarathonAchievement(def),
                AchievementKind.SpeedRun => new SpeedRunAchievement(def),
                AchievementKind.PremiumBuyer => new PremiumBuyerAchievement(def),
                AchievementKind.Generous => new GenerousAchievement(def),
                AchievementKind.Loyal => new LoyalAchievement(def),
                AchievementKind.QuestMaster => new QuestMasterAchievement(def),
                AchievementKind.WorldTour => new WorldTourAchievement(def),
                AchievementKind.Bossbane => new BossbaneAchievement(def),
                _ => throw new ArgumentOutOfRangeException(nameof(def.kind), def.kind, "Unknown achievement kind"),
            };
        }
    }
}
