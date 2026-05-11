using System;

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
    public float critRate;          // 0..1 (Bunny baseline 0.05)
    public float critDamage;        // multiplier added to 1.0 on crit (Bunny baseline 1.0)
    public float magnetMultiplier;  // multiplier on pickup magnet radius (Bunny baseline 1.0)
    public float xpGemValueBonus;   // additive percent (Owl baseline 0.10)
}
