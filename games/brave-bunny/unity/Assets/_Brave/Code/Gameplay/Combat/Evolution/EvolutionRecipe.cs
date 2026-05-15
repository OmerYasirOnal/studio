#nullable enable
// Wave 9 — Weapon Evolution.
// Plain data class describing a single evolution recipe (base weapon + charm → evolved weapon).
//
// ADR-0007 (Evolution charm consumption): consumeCharm is `true` at launch for all 8 recipes.
//   The bool is kept on the recipe (rather than baked into the service) so future cosmetics
//   ("permanent charm") can flip a single record without code edits.
//
// Slug-based (vs SO-reference) so the recipe can be authored from JSON without forcing the
// importer to resolve cross-asset references during a single-pass import. The
// WeaponEvolutionService looks up WeaponDefinition / PassiveDefinition by slug at evolution
// time using inventories the run already maintains.

using System;

namespace Brave.Gameplay.Combat.Evolution
{
    /// <summary>
    /// One row from <c>data/balance/evolutions.json</c>.
    /// Authored either inline by tests or generated from JSON into an
    /// <c>EvolutionRecipeAsset</c> SO at editor time.
    /// </summary>
    [Serializable]
    public sealed class EvolutionRecipe
    {
        public string baseWeaponId = string.Empty;
        public string requiredCharmId = string.Empty;
        public string evolvedWeaponId = string.Empty;

        /// <summary>Always 5 at launch; tests can override.</summary>
        public int requiredWeaponLevel = 5;

        /// <summary>Always 5 at launch; tests can override.</summary>
        public int requiredCharmLevel = 5;

        /// <summary>ADR-0007: true for all launch recipes. Set false only for future cosmetic charms.</summary>
        public bool consumeCharm = true;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(baseWeaponId)
                && !string.IsNullOrEmpty(requiredCharmId)
                && !string.IsNullOrEmpty(evolvedWeaponId)
                && requiredWeaponLevel > 0
                && requiredCharmLevel > 0;
        }
    }
}
