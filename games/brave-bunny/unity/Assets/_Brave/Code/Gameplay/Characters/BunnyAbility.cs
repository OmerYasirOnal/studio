#nullable enable
// Wave 10 — Bunny passive ability "Hop": +10% move-speed permanent passive.
// Magnitude sourced from characters.json: characters[id=bunny].ability.move_mult_bonus.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.hop")]
    public sealed class BunnyAbility : CharacterAbility
    {
        public override string AbilityId => "hop";

        /// <summary>Move-speed multiplier delta. Default mirrors characters.json baseline.</summary>
        public float MoveSpeedMultiplierBonus = 0.10f;

        /// <summary>Effective multiplier applied to base move speed (1.0 + bonus).</summary>
        public float EffectiveMoveSpeedMultiplier => 1f + MoveSpeedMultiplierBonus;
    }
}
