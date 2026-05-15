// Brave Bunny — Systems / LiveOps
// Wave 9 — battle-pass scaffold (data layer).
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md § Battle pass / Season track.
//   * docs/06-tech-spec/03-save-system.md § BattlePassState save trigger.
// Owner: systems-engineer.
//
// Data class shared by the season-config SO and the runtime service. Lives in
// the Brave.Systems.LiveOps namespace so the UI assembly can read rewards
// without referencing the SO assembly directly. No magic numbers — currency
// type comes from <see cref="Brave.Systems.Progression.CurrencyType"/> and
// amount is authored by balance-engineer in the Season<N>Config asset.

#nullable enable

using System;
using Brave.Systems.Progression;
using UnityEngine;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// One reward cell on the battle-pass track. A tier carries TWO rewards
    /// (free row + premium row); <see cref="IsPremium"/> tags which row this
    /// cell belongs to. Authored in <c>BattlePassSeasonConfig</c> per-tier.
    /// </summary>
    [Serializable]
    public sealed class BattlePassReward
    {
        [Tooltip("Currency awarded when this cell is claimed. Maps to the global currency wallet.")]
        public CurrencyType currencyType = CurrencyType.Carrots;

        [Tooltip("Magnitude of the reward. Balance-engineer authors per-tier; no inline defaults.")]
        [Min(0)] public int amount;

        [Tooltip("True when this reward is on the premium row (requires premium-pass entitlement to claim).")]
        public bool isPremium;

        [Tooltip("Optional: kebab-case character slug for character-token rewards. " +
                 "When non-empty, the claim grants progress toward unlocking that character " +
                 "instead of (or in addition to) the currency above. Empty for pure-currency cells.")]
        public string characterTokenSlug = string.Empty;

        /// <summary>Pure-data ctor for tests / fakes.</summary>
        public BattlePassReward() { }

        public BattlePassReward(CurrencyType currencyType, int amount, bool isPremium,
            string characterTokenSlug = "")
        {
            this.currencyType = currencyType;
            this.amount = amount;
            this.isPremium = isPremium;
            this.characterTokenSlug = characterTokenSlug ?? string.Empty;
        }
    }
}
