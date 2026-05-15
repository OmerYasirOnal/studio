#nullable enable
// Wave 10 — Badger passive ability "Tenacity": +25% damage when HP < 30%.
// Magnitudes sourced from characters.json: characters[id=badger].ability.{damage_bonus,hp_threshold}.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.tenacity")]
    public sealed class BadgerAbility : CharacterAbility
    {
        public override string AbilityId => "tenacity";

        /// <summary>Additive damage-multiplier delta while threshold is met (e.g. +0.25).</summary>
        public float DamageBonus = 0.25f;

        /// <summary>HP fraction (0..1) below which the bonus activates.</summary>
        public float HpThresholdFraction = 0.30f;

        /// <summary>Returns the active damage multiplier given current/max HP.
        /// Returns 1.0 when above threshold, (1+DamageBonus) below. MaxHp must be &gt; 0.</summary>
        public float GetDamageMultiplier(float currentHp, float maxHp)
        {
            if (maxHp <= 0f) return 1f;
            float fraction = currentHp / maxHp;
            return fraction < HpThresholdFraction ? 1f + DamageBonus : 1f;
        }
    }
}
