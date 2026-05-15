#nullable enable
// ADR-0020: Weapon archetype-config sidecar ScriptableObject.
//
// Abstract base for the per-weapon, per-archetype configuration carried alongside
// WeaponDefinition. Each concrete subclass owns only the fields its archetype
// actually needs (arm-time for mines, cloud-lifetime for clouds, etc.) — see
// ADR-0020 §Decision for the full mapping.
//
// Polymorphism uses ADR-0009's [BraveRegister] type-name registry rather than
// SerializeReference (rejected per ADR-0009 for save-compat + editor-stability).
// WeaponDefinition holds a single nullable reference to the base class; the
// BalanceJsonImporter creates the matching concrete subclass per the weapon's
// JSON archetype string + key-presence disambiguator.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Abstract base for weapon archetype-config sidecar SOs (ADR-0020).
    /// Concrete subclasses live alongside this file; each carries a
    /// <see cref="BraveRegisterAttribute"/> so MechanicRegistry resolves them
    /// at boot. Per-level perk overrides live on the subclass (e.g.
    /// <c>armTimeMsPerLevel</c>), mirroring <see cref="Definitions.WeaponLevelData"/>'s
    /// per-level carry-forward semantics.
    /// </summary>
    public abstract class WeaponArchetypeConfig : ScriptableObject
    {
        /// <summary>Number of weapon levels — fixed at 5 per ADR-0001 / tech-spec 02.</summary>
        public const int LevelCount = 5;
    }
}
