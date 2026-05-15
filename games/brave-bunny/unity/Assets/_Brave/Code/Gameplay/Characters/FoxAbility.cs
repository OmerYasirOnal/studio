#nullable enable
// Wave 10 — Fox passive ability "Cunning": +100% crit damage.
// Magnitude sourced from characters.json: characters[id=fox].ability.crit_dmg_bonus.
//
// Out-of-scope: DamageCalculator owns the actual crit math (crit agent). This
// ability exposes CritDamageBonus so the calculator reads it via the existing
// pipe instead of duplicating the multiplier path.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.cunning")]
    public sealed class FoxAbility : CharacterAbility
    {
        public override string AbilityId => "cunning";

        /// <summary>Additive crit-damage multiplier delta (e.g. +1.0 → +100%).</summary>
        public float CritDamageBonus = 1.00f;

        /// <summary>Convenience: total crit multiplier (1.0 base + bonus).</summary>
        public float CritMultiplier => 1f + CritDamageBonus;
    }
}
