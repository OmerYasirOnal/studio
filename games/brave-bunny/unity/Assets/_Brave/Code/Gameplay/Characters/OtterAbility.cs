#nullable enable
// Wave 10 — Otter passive ability "Slick": +15% projectile speed.
// Magnitude sourced from characters.json: characters[id=otter].ability.projectile_speed_bonus.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.slick")]
    public sealed class OtterAbility : CharacterAbility
    {
        public override string AbilityId => "slick";

        /// <summary>Additive projectile-speed multiplier (e.g. +0.15 → 1.15× projectile speed).</summary>
        public float ProjectileSpeedBonus = 0.15f;

        public float EffectiveProjectileSpeedMultiplier => 1f + ProjectileSpeedBonus;
    }
}
