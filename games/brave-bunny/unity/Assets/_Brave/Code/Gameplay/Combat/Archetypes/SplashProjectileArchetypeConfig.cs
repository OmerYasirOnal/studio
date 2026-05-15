#nullable enable
// ADR-0020 §Decision: SplashProjectileArchetypeConfig — Cob Mortar + Acorn Cannon L5.
// JSON keys: splash_units_base (top-level + L2/L5 perk "splash_units"), travel_ms (top-level + L2 perk).
// Cob Mortar L1: splash_units_base=1.5, travel_ms=1200. Acorn Cannon L5 perk grants 1.0 splash_units.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for splash-projectile weapons (Cob Mortar). Also
    /// used by Acorn Cannon once its L5 perk grants splash. Carries the
    /// per-weapon splash radius + travel time semantics.
    /// </summary>
    [BraveRegister("weapon.archetype.splash_projectile")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Splash Projectile",
                     fileName = "SplashProjectileArchetypeConfig",
                     order = 104)]
    public sealed class SplashProjectileArchetypeConfig : WeaponArchetypeConfig
    {
        [Header("Splash radius (world units) — weapon top-level baseline")]
        public float splashUnitsBase;

        [Header("Travel time (ms) — weapon top-level baseline (Cob Mortar lob arc)")]
        public int travelMs;

        [Header("Per-level overrides (ADR-0020 carry-forward) — EXACTLY 5 entries")]
        public float[] splashUnitsPerLevel = new float[LevelCount];
        public int[]   travelMsPerLevel    = new int[LevelCount];
    }
}
