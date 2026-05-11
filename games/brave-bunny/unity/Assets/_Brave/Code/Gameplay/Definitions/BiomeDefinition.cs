#nullable enable
// Tech-spec 02 § BiomeDefinition. Owns scaling curves, enemy variants, both bosses.

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// One biome's static catalog: visuals, palette, wave schedule, scaling curves,
    /// and enemy variant tables. Five biomes at launch; vertical slice ships 1.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Biome", fileName = "Biome", order = 4)]
    public sealed class BiomeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;           // "carrot-fields", "honey-swamp", ...
        public string displayName = string.Empty;
        public Sprite? thumbnail;

        [Header("World")]
        public GameObject? environmentPrefab;        // pre-merged chunks
        public Color paletteAccent = Color.white;    // HUD tinting
        public Material? toonLutMaterial;            // per ADR-0002

        [Header("Spawning")]
        public WaveDefinition? waves;
        public BossDefinition? midBoss;
        public BossDefinition? endBoss;

        [Header("Variants")]
        public EnemyDefinition[] swarmerVariants = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] tankVariants    = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] rangedVariants  = Array.Empty<EnemyDefinition>();
        public EnemyDefinition? eliteVariant;

        [Header("Per-minute scaling (linear)")]
        public ScalingCurve hpScaling;
        public ScalingCurve spawnDensityScaling;
    }

    /// <summary>Linear per-minute scaling. Capped to <see cref="capValue"/>.</summary>
    [Serializable]
    public struct ScalingCurve
    {
        public float perMinuteDelta;
        public float minuteOneBaseline;
        public float capValue;

        /// <summary>Pure function — no Unity dependency. Safe to call from Burst jobs.</summary>
        public float SampleAtMinute(float minute)
        {
            float raw = minuteOneBaseline + perMinuteDelta * Mathf.Max(0f, minute - 1f);
            return Mathf.Min(raw, capValue);
        }
    }
}
