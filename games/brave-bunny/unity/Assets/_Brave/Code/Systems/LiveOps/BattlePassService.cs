// Brave Bunny — Systems / LiveOps
// Wave 9 — battle-pass scaffold (runtime service).
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md § Battle pass.
//   * docs/06-tech-spec/03-save-system.md § Save triggers — every claim + every
//     XP grant that advances a tier triggers ISaveService.Save().
// Owner: systems-engineer.
//
// Responsibilities:
//   * Track per-season XP, derive current tier from BattlePassSeasonConfig.
//   * Award XP via GrantXp(int). Capped at the top-tier threshold so XP can't
//     overflow past tier 30. No-ops after endDate (post-season grace).
//   * Resolve claims: free row anytime when tier reached; premium row only when
//     PremiumActive == true. Returns a populated BattlePassReward (or null on
//     reject) — currency dispensing is the caller's responsibility so this
//     service stays free of CurrencyService coupling.
//   * On season swap (different seasonId in the config), call
//     BattlePassState.BeginNewSeason — claimed/xp/tier reset.
//
// Premium gating contract (CLAUDE.md constraints):
//   * Claim(tier, isPremium=true) returns null when PremiumActive == false.
//   * Once PremiumActive flips to true, ALL previously earned premium tiers
//     become claimable — the service does NOT auto-deduct on grant time. The
//     UI is expected to surface the retroactive claim affordance.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;
using Brave.Systems.Save;
using UnityEngine;

namespace Brave.Systems.LiveOps
{
    /// <summary>Outcome of a single <see cref="IBattlePassService.Claim"/> call.</summary>
    public enum BattlePassClaimResult
    {
        /// <summary>Tier was unreached, out of range, premium row without entitlement, or already claimed.</summary>
        Rejected = 0,
        /// <summary>Reward granted; <c>reward</c> out-param is populated.</summary>
        Granted = 1,
    }

    /// <summary>Service contract — see <see cref="BattlePassService"/>.</summary>
    public interface IBattlePassService : IService
    {
        /// <summary>Active season identifier (from the bound <see cref="BattlePassSeasonConfig"/>).</summary>
        string CurrentSeasonId { get; }

        /// <summary>Cumulative XP earned this season (monotonic until reset).</summary>
        int CurrentXp { get; }

        /// <summary>Highest 1-based tier ordinal currently reached. 0 when no XP earned.</summary>
        int CurrentTier { get; }

        /// <summary>True when the premium pass has been activated for this season.</summary>
        bool IsPremiumActive { get; }

        /// <summary>Add XP to the current season. Saves when the tier advances or premium auto-flips.</summary>
        void GrantXp(int xp);

        /// <summary>Activate the premium row for this season. Saves. Idempotent.</summary>
        void ActivatePremium();

        /// <summary>
        /// Resolve a tier claim. <paramref name="isPremium"/> picks the row.
        /// Returns the <see cref="BattlePassReward"/> on success (caller is responsible
        /// for dispensing currency / tokens via CurrencyService) or null on rejection.
        /// </summary>
        BattlePassReward? Claim(int tier, bool isPremium);

        /// <summary>True when the given tier is already on the claimed list for the given row.</summary>
        bool IsTierClaimed(int tier, bool isPremium);

        /// <summary>Raised after a successful Claim — UI swaps to "claimed" state.</summary>
        event Action<int, bool>? TierClaimed;

        /// <summary>Raised after a tier advance — UI plays the celebration animation.</summary>
        event Action<int>? TierAdvanced;
    }

    /// <summary>Concrete service. Persists via the shared <see cref="ISaveService"/>.</summary>
    public sealed class BattlePassService : IBattlePassService
    {
        private readonly ISaveService _save;
        private readonly BattlePassSeasonConfig _config;
        private readonly Func<DateTime>? _utcNowOverride;

        public BattlePassService(ISaveService save, BattlePassSeasonConfig config,
            Func<DateTime>? utcNowOverride = null)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
            _utcNowOverride = utcNowOverride;
            ReconcileSeasonBinding();
        }

        // ---- IBattlePassService ----

        public string CurrentSeasonId => State.SeasonId;
        public int CurrentXp => State.CurrentXp;
        public int CurrentTier => State.CurrentTier;
        public bool IsPremiumActive => State.PremiumActive;

        public event Action<int, bool>? TierClaimed;
        public event Action<int>? TierAdvanced;

        public void GrantXp(int xp)
        {
            if (xp <= 0) return;
            ReconcileSeasonBinding();

            // Post-season grace: no XP after endDate, but Claim() still works on
            // tiers already reached. Returns silently so the caller (RunController)
            // doesn't need to know the season is closed.
            if (IsSeasonEnded()) return;

            int previousTier = State.CurrentTier;
            int cap = ComputeXpCap();
            int newXp = State.CurrentXp + xp;
            if (cap > 0 && newXp > cap) newXp = cap;
            State.CurrentXp = newXp;

            int newTier = _config.TierForXp(newXp);
            if (newTier != State.CurrentTier)
            {
                State.CurrentTier = newTier;
                _save.Save(); // 03-save-system.md trigger: "Battle-pass tier advanced".
                if (newTier > previousTier) TierAdvanced?.Invoke(newTier);
            }
        }

        public void ActivatePremium()
        {
            ReconcileSeasonBinding();
            if (State.PremiumActive) return;
            State.PremiumActive = true;
            _save.Save(); // 03-save-system.md trigger: "Premium pass activated".
        }

        public BattlePassReward? Claim(int tier, bool isPremium)
        {
            ReconcileSeasonBinding();

            if (tier < 1 || tier > BattlePassSeasonConfig.TierCount) return null;
            if (tier > State.CurrentTier) return null;            // not yet earned
            if (IsTierClaimed(tier, isPremium)) return null;       // already taken

            // Premium gating: row=premium requires entitlement. Both free and
            // premium rewards still PERSIST as earned-but-unclaimed-premium so
            // they can be retroactively claimed once premium activates. We just
            // refuse the claim NOW.
            if (isPremium && !State.PremiumActive) return null;

            var reward = isPremium
                ? _config.PremiumRewardAtTier(tier)
                : _config.FreeRewardAtTier(tier);

            if (reward == null) return null;

            var claimedList = isPremium ? State.ClaimedPremiumTiers : State.ClaimedFreeTiers;
            claimedList.Add(tier);
            _save.Save(); // 03-save-system.md trigger: "Battle-pass tier claimed".

            TierClaimed?.Invoke(tier, isPremium);
            return reward;
        }

        public bool IsTierClaimed(int tier, bool isPremium)
        {
            var list = isPremium ? State.ClaimedPremiumTiers : State.ClaimedFreeTiers;
            return list != null && list.Contains(tier);
        }

        // ---- internals ----

        private BattlePassState State => _save.Data.BattlePassState;

        /// <summary>
        /// Ensure the save's BattlePassState matches the bound <see cref="BattlePassSeasonConfig"/>.
        /// When the seasonId differs the state is reset (claimed/xp/tier cleared, premium dropped).
        /// </summary>
        private void ReconcileSeasonBinding()
        {
            var state = State;
            if (state.SeasonId == _config.seasonId) return;
            state.BeginNewSeason(_config.seasonId);
            _save.Save(); // 03-save-system.md trigger: "Battle-pass season swapped".
        }

        /// <summary>Top-tier XP cap so cumulative XP can't overflow past the last threshold.</summary>
        private int ComputeXpCap()
        {
            var thresholds = _config.tierXpThresholds;
            if (thresholds == null || thresholds.Length == 0) return 0;
            return thresholds[thresholds.Length - 1];
        }

        /// <summary>True when the configured endDate parses to a past UTC date.</summary>
        private bool IsSeasonEnded()
        {
            if (string.IsNullOrEmpty(_config.endDate)) return false;
            if (!DateTime.TryParse(_config.endDate, null,
                    System.Globalization.DateTimeStyles.AssumeUniversal, out var end))
            {
                Debug.LogWarning($"BattlePassService: unparseable endDate '{_config.endDate}'.");
                return false;
            }
            var now = _utcNowOverride?.Invoke() ?? DateTime.UtcNow;
            return now >= end.ToUniversalTime();
        }
    }
}
