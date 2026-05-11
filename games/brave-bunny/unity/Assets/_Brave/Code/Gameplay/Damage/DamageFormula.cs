// Implements docs/10-balance/00-formulas.md § 1 (Damage) + § 2 (Crit roll).
// All numbers sourced from CharacterStats + WeaponLevelData + EnemyDefinition; no magic constants.
using Brave.Gameplay.Definitions;
using UnityEngine;

namespace Brave.Gameplay.Damage;

/// <summary>
/// Pure damage-formula helper. Inputs are structs; output is a final float damage value
/// and a crit flag. The caller composes a <see cref="HitContext"/>.
/// </summary>
public static class DamageFormula
{
    /// <summary>Clamp from balance/00-formulas.md § 11.</summary>
    public const float DefenseMultMax = 0.75f;
    public const float CritRateMax = 0.95f;
    public const float DamageFloor = 1f;

    /// <summary>
    /// Compute final damage. <paramref name="critRoll"/> is a uniform float in [0,1) supplied
    /// by the caller (so the random source remains injectable for tests + deterministic replay).
    /// </summary>
    public static float Compute(
        WeaponLevelData weapon,
        CharacterStats hero,
        float enemyDefenseMult,
        float critRoll,
        out bool isCrit)
    {
        float baseDmg = weapon.damage;
        float charMult = Mathf.Max(0f, hero.damageMultiplier);
        float weaponLevelMult = 1f;        // pre-baked into weapon.damage from per-level table

        float critRate = Mathf.Clamp(hero.critRate, 0f, CritRateMax);
        isCrit = critRoll < critRate;
        float critMult = isCrit ? 1f + hero.critDamage : 1f;

        float defense = Mathf.Clamp(enemyDefenseMult, 0f, DefenseMultMax);

        float dmg = baseDmg * charMult * weaponLevelMult * critMult * (1f - defense);
        return Mathf.Max(DamageFloor, dmg);
    }
}
