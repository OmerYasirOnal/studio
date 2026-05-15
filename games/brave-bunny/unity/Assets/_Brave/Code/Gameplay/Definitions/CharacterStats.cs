using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions;

/// <summary>
/// Per-character baseline stats. Sourced from <c>data/balance/characters.json</c>.
/// Per tech-spec 02 data-model: HP, DmgMult, MoveMult, MagnetMult, CritRate, CritDmg.
/// </summary>
[Serializable]
public struct CharacterStats
{
    public float baseHP;            // hit points (Bunny baseline 100)
    public float baseMoveSpeed;     // units/second (Bunny baseline 4.5)
    public float damageMultiplier;  // multiplier vs weapon DMG (Bunny baseline 1.0)

    /// <summary>
    /// Critical-hit chance per attack, 0..1 (a.k.a. "CritChance" in HUD/loadout).
    /// Bunny baseline 0.05. Clamped to [0, <see cref="Brave.Gameplay.Damage.DamageFormula.CritRateMax"/>]
    /// at roll-time in <see cref="Brave.Gameplay.Damage.DamageFormula.Compute"/>.
    /// </summary>
    [Range(0f, 1f)] public float critRate;

    /// <summary>
    /// Bonus damage added on crit — final multiplier is <c>1 + critDamage</c>.
    /// Default 1.0 → 2× damage on crit (the "CritMultiplier" 1.5× to 3× window per
    /// data/balance/characters.schema.md). Sourced from <c>characters.json</c>.
    /// </summary>
    public float critDamage;

    public float magnetMultiplier;  // multiplier on pickup magnet radius (Bunny baseline 1.0)
    public float xpGemValueBonus;   // additive percent (Owl baseline 0.10)
}
