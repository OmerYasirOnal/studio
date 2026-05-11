#nullable enable
// Tech-spec 02 § EnemyDefinition. Stats are minute-1 baselines; biome-level ScalingCurve
// applies the per-minute delta at runtime.

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Designer-facing static config for a single enemy variant. Lives under
    /// <c>Assets/_Brave/Data/Balance/Enemies/</c>; generated from <c>enemies.json</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Enemy", fileName = "Enemy", order = 2)]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;
        public EnemyRole role = EnemyRole.Swarmer;
        public BiomeDefinition? biome;             // null = any biome (rare)
        public GameObject? prefab;

        [Header("Stats (minute-1 baseline)")]
        public float baseHP = 10f;                 // hp
        public float contactDamage = 5f;           // hp/touch
        public float rangedDamage;                 // 0 if not ranged
        public float moveSpeed = 2.5f;             // units/sec (raw, NOT multiplier)
        public float defenseMultiplier;            // 0..0.75 clamp (damage reduction)

        [Header("Drops + telegraph")]
        public DropTable drops = new DropTable();
        public string telegraphSfxKey = string.Empty;
        public float telegraphWindowSeconds;       // 0 for swarmers

        private void OnValidate()
        {
            if (role != EnemyRole.Swarmer && telegraphWindowSeconds <= 0f)
                Debug.LogWarning($"{slug}: non-swarmer should have a telegraph window", this);
            if (defenseMultiplier < 0f || defenseMultiplier > 0.75f)
                Debug.LogError($"{slug}: defenseMultiplier must be in [0, 0.75]", this);
        }
    }

    /// <summary>Probability bundle for an enemy kill's loot drop. All chances in [0, 1].</summary>
    [Serializable]
    public struct DropTable
    {
        public float xpGemSmallChance;
        public float xpGemMediumChance;
        public float xpGemLargeChance;
        public float goldCoinChance;
        public float heartChance;
        public int soulShardsOnKill;       // 0 trash, 1 elite, 3-30 boss
    }
}
