#nullable enable
// ADR-0020 §Decision: ProjectileArchetypeConfig — base case for kinetic projectile
// weapons (Carrot Boomerang vertical-slice, Pebble Sling, Acorn Cannon, Whirligig).
// Empty for now; future home for pierce_default, bounce_default scalars per ADR-0020.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for projectile weapons. No archetype-specific fields yet
    /// — all projectile config lives on <see cref="Definitions.WeaponLevelData"/>.
    /// Reserved for future per-archetype constants (e.g. default pierce / bounce
    /// counts) shared across all projectile weapons.
    /// </summary>
    [BraveRegister("weapon.archetype.projectile")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Projectile",
                     fileName = "ProjectileArchetypeConfig",
                     order = 100)]
    public sealed class ProjectileArchetypeConfig : WeaponArchetypeConfig
    {
    }
}
