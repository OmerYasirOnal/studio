#nullable enable
// Wave 10 — Tortoise passive ability "Shell": +50% HP, -20% move-speed.
// Magnitudes sourced from characters.json: characters[id=tortoise].ability.{hp_mult_bonus,move_mult_bonus}.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.shell")]
    public sealed class TortoiseAbility : CharacterAbility
    {
        public override string AbilityId => "shell";

        /// <summary>Additive HP bonus multiplier (e.g. +0.50 → 1.5× HP).</summary>
        public float HpMultiplierBonus = 0.50f;

        /// <summary>Additive move-speed delta (e.g. -0.20 → 0.8× speed).</summary>
        public float MoveSpeedMultiplierBonus = -0.20f;

        public float EffectiveHpMultiplier => 1f + HpMultiplierBonus;
        public float EffectiveMoveSpeedMultiplier => 1f + MoveSpeedMultiplierBonus;
    }
}
