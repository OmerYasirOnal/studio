#nullable enable
// Tech-spec 02 § WeaponDefinition.levels — per-level row.

using System;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// One row of <see cref="WeaponDefinition.levels"/>. All values sourced from
    /// <c>data/balance/weapons.json</c>; never inline magic numbers in combat code.
    /// </summary>
    [Serializable]
    public struct WeaponLevelData
    {
        public float damage;            // DMG per hit (at this level)
        public float fireRate;          // seconds between fires
        public float range;             // world units
        public int projectiles;         // count per fire
        public string upgradeFlavor;    // 1-line designer description for the draft card
    }
}
