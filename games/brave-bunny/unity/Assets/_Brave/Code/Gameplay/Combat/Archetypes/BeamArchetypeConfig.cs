#nullable enable
// ADR-0020 §Decision: BeamArchetypeConfig — base case for beam weapons (Sunbeam
// vertical-slice). Empty for now; future home for beam_width_units,
// sweep_lock_seconds per ADR-0020.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for beam weapons. No archetype-specific fields yet
    /// — Sunbeam's beam-width + sweep-lock perks ride per-level on
    /// <see cref="Definitions.WeaponLevelData"/>. Reserved for shared beam
    /// constants surfaced by future weapons (Solar Halo evolution etc.).
    /// </summary>
    [BraveRegister("weapon.archetype.beam")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Beam",
                     fileName = "BeamArchetypeConfig",
                     order = 101)]
    public sealed class BeamArchetypeConfig : WeaponArchetypeConfig
    {
    }
}
