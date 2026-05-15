// Brave Bunny — Systems / LiveOps
// Wave 9 — battle-pass scaffold.
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md § Battle pass / Season track.
//   * docs/10-balance/ § Per-season tier thresholds (linear-30 default).
// Owner: systems-engineer.
//
// ScriptableObject authored per-season by balance-engineer + game-designer.
// Carries the tier XP thresholds and the two reward rows (free + premium).
// All numbers live in the asset (CLAUDE.md principle 6 — no magic numbers in
// code). The runtime BattlePassService consumes this SO via constructor or
// SerializeField injection; the SO never mutates.

#nullable enable

using UnityEngine;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// Per-season immutable configuration. 30 tiers fixed (matches storefront
    /// expectations and the wireframe in docs/05-wireframes/09-battle-pass.html).
    /// Tier XP thresholds are CUMULATIVE — tier 1 unlocks at <c>tierXpThresholds[0]</c>
    /// total XP, tier 2 at <c>tierXpThresholds[1]</c>, etc.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/LiveOps/BattlePassSeasonConfig",
        fileName = "Season1Config", order = 50)]
    public sealed class BattlePassSeasonConfig : ScriptableObject
    {
        /// <summary>Fixed track length for v1. New seasons reuse this layout.</summary>
        public const int TierCount = 30;

        [Header("Identity")]
        [Tooltip("Unique season identifier. Used as the save-data key so XP/tier from a " +
                 "prior season does not bleed into the next when the SO is swapped.")]
        public string seasonId = "season-1";

        [Tooltip("ISO-8601 UTC date the season opens. UI shows a countdown; the service " +
                 "does NOT auto-grant XP before this date.")]
        public string startDate = "2026-05-15";

        [Tooltip("ISO-8601 UTC date the season closes. After this date, GrantXp becomes a " +
                 "no-op and Claim still works for already-earned tiers (post-season grace).")]
        public string endDate = "2026-07-15";

        [Header("Track (length = TierCount)")]
        [Tooltip("Cumulative XP required to reach each tier (index 0 = tier 1). " +
                 "Default: linear 1000, 2000, …, 30_000.")]
        public int[] tierXpThresholds = new int[TierCount];

        [Tooltip("Free-row reward for each tier (index 0 = tier 1).")]
        public BattlePassReward[] freeRewards = new BattlePassReward[TierCount];

        [Tooltip("Premium-row reward for each tier (index 0 = tier 1). Claimable only " +
                 "while the premium pass is active for this season.")]
        public BattlePassReward[] premiumRewards = new BattlePassReward[TierCount];

        // ---- Convenience accessors ----

        /// <summary>True when the track arrays are sized exactly to TierCount.</summary>
        public bool IsWellFormed =>
            tierXpThresholds != null && tierXpThresholds.Length == TierCount
            && freeRewards != null && freeRewards.Length == TierCount
            && premiumRewards != null && premiumRewards.Length == TierCount;

        /// <summary>
        /// Compute the tier index (1..TierCount) reached for a given cumulative XP total.
        /// Returns 0 when XP &lt; <c>tierXpThresholds[0]</c>. Clamps to TierCount on overflow.
        /// </summary>
        public int TierForXp(int xp)
        {
            if (tierXpThresholds == null || tierXpThresholds.Length == 0) return 0;
            int tier = 0;
            for (int i = 0; i < tierXpThresholds.Length; i++)
            {
                if (xp >= tierXpThresholds[i]) tier = i + 1;
                else break;
            }
            return tier;
        }

        /// <summary>Return the free-row reward for a 1-based tier ordinal, or null when out of range.</summary>
        public BattlePassReward? FreeRewardAtTier(int tier)
        {
            if (tier < 1 || tier > TierCount) return null;
            if (freeRewards == null || tier > freeRewards.Length) return null;
            return freeRewards[tier - 1];
        }

        /// <summary>Return the premium-row reward for a 1-based tier ordinal, or null when out of range.</summary>
        public BattlePassReward? PremiumRewardAtTier(int tier)
        {
            if (tier < 1 || tier > TierCount) return null;
            if (premiumRewards == null || tier > premiumRewards.Length) return null;
            return premiumRewards[tier - 1];
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only helper used by <c>OnValidate</c> to (re)seed a fresh SO with the
        /// linear 1000, 2000, …, 30_000 threshold default. Balance-engineer can then
        /// tune per-tier from there.
        /// </summary>
        [ContextMenu("Seed linear thresholds (1000 step)")]
        private void SeedLinearThresholds()
        {
            const int step = 1000; // GDD §02-meta-loop.md default.
            tierXpThresholds = new int[TierCount];
            for (int i = 0; i < TierCount; i++) tierXpThresholds[i] = (i + 1) * step;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
