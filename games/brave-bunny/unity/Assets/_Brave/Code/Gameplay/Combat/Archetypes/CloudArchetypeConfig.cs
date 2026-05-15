#nullable enable
// ADR-0020 §Decision: CloudArchetypeConfig — Thunder Cloud.
// JSON keys: cloud_lifetime_ms (top-level + L4 perk), zaps_per_cloud (top-level + L2 perk).
// Thunder Cloud L1: cloud_lifetime_ms=4000, zaps_per_cloud=3.

using UnityEngine;

namespace Brave.Gameplay.Combat.Archetypes
{
    /// <summary>
    /// Archetype sidecar for cloud-zap weapons (Thunder Cloud).
    /// </summary>
    [BraveRegister("weapon.archetype.cloud")]
    [CreateAssetMenu(menuName = "Brave/Archetype Config/Cloud",
                     fileName = "CloudArchetypeConfig",
                     order = 103)]
    public sealed class CloudArchetypeConfig : WeaponArchetypeConfig
    {
        [Header("Cloud lifetime (ms) — weapon top-level baseline")]
        public int cloudLifetimeMs;

        [Header("Zaps per cloud — weapon top-level baseline")]
        public int zapsPerCloud;

        [Header("Per-level overrides (ADR-0020 carry-forward) — EXACTLY 5 entries")]
        public int[] cloudLifetimeMsPerLevel = new int[LevelCount];
        public int[] zapsPerCloudPerLevel    = new int[LevelCount];
    }
}
