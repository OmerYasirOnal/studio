#nullable enable
// Tech-spec 02 § PassiveDefinition. 6 launch passives; each with EXACTLY 5 levels.
// Charm-evolution rule: a passive at L5 enables the matching weapon evolution.

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    public enum PassiveStat
    {
        MagnetRadius      = 0,
        MaxHP             = 1,
        Regen             = 2,
        GlobalDamage      = 3,
        CritChance        = 4,
        ProjectileCount   = 5,
        MoveSpeed         = 6,
        Cooldown          = 7,
    }

    /// <summary>
    /// Static passive (charm) config. Generated from <c>data/balance/passives.json</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Passive", fileName = "Passive", order = 3)]
    public sealed class PassiveDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;
        public string displayName = string.Empty;
        public Sprite? icon;

        [Header("Level table — EXACTLY 5 entries (L1..L5)")]
        public PassiveLevelData[] levels = new PassiveLevelData[5];

        private void OnValidate()
        {
            if (levels == null || levels.Length != 5)
                Debug.LogError($"{slug}: passives must have exactly 5 levels", this);
        }
    }

    [Serializable]
    public struct PassiveLevelData
    {
        public PassiveStatModifier[] modifiers;
        public string upgradeFlavor;
    }

    [Serializable]
    public struct PassiveStatModifier
    {
        public PassiveStat stat;
        public float deltaPercent;     // additive percent unless flagged as flat
        public bool isFlat;
    }
}
