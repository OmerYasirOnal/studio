#nullable enable
// ADR-0020 §Decision: MineArchetypeConfig — Daisy Mine vertical-slice.
// JSON keys: arm_time_ms (top-level) + L3 perk "arm_time_ms".
// Daisy Mine L1..L5: 1000, 1000, 500, 500, 500 (L3 perk halves it).

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for area-mine weapons (Daisy Mine vertical-slice).
    /// Carries the arm-time-ms semantics that were previously hardcoded or
    /// missing from <see cref="Definitions.WeaponLevelData"/>.
    /// </summary>
    [BraveRegister("weapon.archetype.mine")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Mine",
                     fileName = "MineArchetypeConfig",
                     order = 102)]
    public sealed class MineArchetypeConfig : WeaponArchetypeConfig
    {
        [Header("Arm time (ms) — weapon top-level baseline")]
        public int armTimeMs;

        [Header("Per-level overrides (ADR-0020 carry-forward) — EXACTLY 5 entries")]
        public int[] armTimeMsPerLevel = new int[LevelCount];
    }
}
