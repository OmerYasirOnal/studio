#nullable enable
// ADR-0020 §Decision: SummonArchetypeConfig — Tumbleweed, Whirligig orbit-lifetime.
// JSON keys: lifetime_ms (top-level + L2 perk "lifetime_ms").
// Tumbleweed L1: lifetime_ms=4000.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for summon weapons (Tumbleweed, Whirligig orbit-lifetime).
    /// Carries the summon-entity lifetime that the runtime spawner uses to despawn
    /// the entity after its in-world duration elapses.
    /// </summary>
    [BraveRegister("weapon.archetype.summon")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Summon",
                     fileName = "SummonArchetypeConfig",
                     order = 106)]
    public sealed class SummonArchetypeConfig : WeaponArchetypeConfig
    {
        [Header("Summon lifetime (ms) — weapon top-level baseline")]
        public int lifetimeMs;

        [Header("Per-level overrides (ADR-0020 carry-forward) — EXACTLY 5 entries")]
        public int[] lifetimeMsPerLevel = new int[LevelCount];
    }
}
