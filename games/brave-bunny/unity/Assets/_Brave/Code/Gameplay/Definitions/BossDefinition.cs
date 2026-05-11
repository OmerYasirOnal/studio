#nullable enable
// Tech-spec 02 § BossDefinition. Bosses have exactly 3 phases; pattern strings resolve via
// the same MechanicRegistry (ADR-0009).

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Static boss config. One per biome (mid + end). Phases are gated by HP percent.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Boss", fileName = "Boss", order = 5)]
    public sealed class BossDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;
        public BiomeDefinition? biome;
        public GameObject? prefab;

        [Header("Stats")]
        public float baseHP = 4000f;            // mid-run 4000, end-run 12000 baseline

        [Header("Phases — EXACTLY 3")]
        public BossPhase[] phases = new BossPhase[3];

        [Header("Drops (3-5 soul shards + character shard pull)")]
        public DropTable bossDrops = new DropTable();

        private void OnValidate()
        {
            if (phases == null || phases.Length != 3)
                Debug.LogError($"{slug}: bosses must define exactly 3 phases", this);
        }
    }

    [Serializable]
    public struct BossPhase
    {
        public float hpGatePercent;       // 1.0, 0.66, 0.33
        public string attackPatternId;    // resolves to a BossAttackPattern subclass (ADR-0009)
        public float moveSpeedMultiplier;
    }
}
