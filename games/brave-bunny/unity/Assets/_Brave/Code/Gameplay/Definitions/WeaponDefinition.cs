#nullable enable
// Tech-spec 02 § WeaponDefinition. Mirrors GDD 04-weapons.md.
// Weapon levels are EXACTLY 5; archetype drives Weapon concrete-class dispatch in combat.

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    public enum WeaponArchetype { Projectile, Area, Aura, Summon, Utility }
    public enum TargetingMode   { Nearest, Furthest, Random, SelfCentered, OrbitPlayer, RandomScreenPos }
    public enum SynergyTag      { Kinetic, Nature, Solar, Frost, Aura, Summon, Mech, Explosive, Bounce, Beam, Heavy }

    /// <summary>
    /// Static weapon config. 12 launch weapons; vertical slice ships 3. Universal pool per ADR-0001.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Weapon", fileName = "Weapon", order = 1)]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;
        public string displayName = string.Empty;
        public Sprite? icon;

        [Header("Behaviour")]
        public WeaponArchetype archetype = WeaponArchetype.Projectile;
        public TargetingMode targeting = TargetingMode.Nearest;
        public GameObject? projectilePrefab;       // null for aura / utility

        [Header("Level table — EXACTLY 5 entries (L1..L5)")]
        public WeaponLevelData[] levels = new WeaponLevelData[5];

        [Header("Evolution (null for non-evolving weapons)")]
        public WeaponEvolutionRecipe? evolution;

        [Header("Tags")]
        public SynergyTag[] synergyTags = Array.Empty<SynergyTag>();

        private void OnValidate()
        {
            if (levels == null || levels.Length != 5)
                Debug.LogError($"{slug}: weapons must have exactly 5 levels", this);
        }
    }
}
