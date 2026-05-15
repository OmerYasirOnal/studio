#nullable enable
// ADR-0020 §Decision: AuraArchetypeConfig — Frost Whisper + Honey Aura.
// JSON keys: slow_pct_base (top-level + L2/L4 perk "slow_pct"), tick lifetime (covers Honey Aura tick duration).
// Frost Whisper L1: slow_pct_base=0.10.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for aura weapons (Frost Whisper, Honey Aura). Carries
    /// the slow-percentage scalar that the runtime aura-tick applies to enemies
    /// inside the aura radius, plus a per-tick lifetime field future-proofed
    /// for non-rate-driven aura ticks.
    /// </summary>
    [BraveRegister("weapon.archetype.aura")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Aura",
                     fileName = "AuraArchetypeConfig",
                     order = 105)]
    public sealed class AuraArchetypeConfig : WeaponArchetypeConfig
    {
        [Header("Slow percentage [0..1] — weapon top-level baseline")]
        public float slowPctBase;

        [Header("Tick lifetime (ms) — weapon top-level baseline (Honey Aura tick duration)")]
        public int tickLifetimeMs;

        [Header("Per-level overrides (ADR-0020 carry-forward) — EXACTLY 5 entries")]
        public float[] slowPctPerLevel        = new float[LevelCount];
        public int[]   tickLifetimeMsPerLevel = new int[LevelCount];
    }
}
