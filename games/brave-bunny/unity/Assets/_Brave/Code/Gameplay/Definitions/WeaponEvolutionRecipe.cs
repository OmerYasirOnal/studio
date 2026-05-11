#nullable enable
// Tech-spec 02 § WeaponEvolutionRecipe. Evolution consumes a max-level passive (ADR-0007).

using System;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Optional evolution table — null means the weapon does not evolve.
    /// Six of twelve launch weapons evolve.
    /// </summary>
    [Serializable]
    public sealed class WeaponEvolutionRecipe
    {
        public PassiveDefinition? ingredient;       // L5 passive required
        public WeaponDefinition? resultWeapon;      // evolved form
    }
}
