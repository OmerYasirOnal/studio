// Brave Bunny — Gameplay/Events/AchievementUnlockedEvent + AchievementUnlockedChannel (Wave 10).
//
// Tech-spec 09 § Tier 3 typed ScriptableObject event channel. Fired by
// AchievementService the moment a tracked achievement crosses its required
// threshold (one-shot per achievement id). Consumers:
//   * UI — AchievementToastController pops a 3s toast at top of screen.
//   * Analytics (future) — tag the unlock for funnel reports.
//
// Payload is a readonly struct (pass-by-value, zero GC) per tech-spec 09.

#nullable enable

using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Payload broadcast on <see cref="AchievementUnlockedChannel"/> at the moment
    /// the achievement's progress meets its required count. The fields carry the
    /// stable id (matches <c>SaveData.Achievements</c> key + catalog id) so UI
    /// can resolve loc keys (<c>achievement.&lt;id&gt;.name</c>) without taking a
    /// dep on the Brave.Systems.Achievements assembly definition.
    /// </summary>
    public readonly struct AchievementUnlockedEvent
    {
        /// <summary>Stable slug (e.g. "first-boss-kill"). Matches catalog id + save key.</summary>
        public readonly string achievementId;

        /// <summary>Loc key for the display name (typically "achievement.&lt;id&gt;.name").</summary>
        public readonly string displayLocKey;

        /// <summary>Reward currency type code (0=Carrots, 1=Stars, 2=SoulShards). Mirrors CurrencyType ordinals.</summary>
        public readonly int rewardCurrencyCode;

        /// <summary>Reward amount granted on claim. 0 when no reward.</summary>
        public readonly int rewardAmount;

        public AchievementUnlockedEvent(string achievementId, string displayLocKey, int rewardCurrencyCode, int rewardAmount)
        {
            this.achievementId = achievementId;
            this.displayLocKey = displayLocKey;
            this.rewardCurrencyCode = rewardCurrencyCode;
            this.rewardAmount = rewardAmount;
        }
    }

    /// <summary>SO channel asset — wired into AchievementService + UI toast controller.</summary>
    [CreateAssetMenu(menuName = "Brave/Events/AchievementUnlocked", fileName = "AchievementUnlockedChannel", order = 8)]
    public sealed class AchievementUnlockedChannel : EventChannel<AchievementUnlockedEvent> { }
}
