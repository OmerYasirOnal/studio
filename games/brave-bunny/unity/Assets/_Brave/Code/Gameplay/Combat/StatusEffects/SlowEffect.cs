#nullable enable
// Wave 10: Slow status effect. Applied by Aura weapons (Frost Whisper, Honey Aura).
// Magnitude is in [0..1] — fraction of base speed REMOVED. magnitude=0.4f → speed * 0.6f.

using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Multiplicative speed-reduction effect. Reduces enemy speed by
    /// <see cref="StatusEffect.magnitude"/> (fraction, 0..1) for <see cref="StatusEffect.durationMs"/>.
    /// Restores the speed multiplier on expire.</summary>
    [BraveRegister("status.slow")]
    public sealed class SlowEffect : StatusEffect
    {
        public override string TypeName => "status.slow";

        public SlowEffect() { }

        /// <summary>Convenience ctor for AuraWeapon's call-site.</summary>
        public SlowEffect(int durationMs, float magnitude)
        {
            Configure(durationMs, magnitude);
        }

        public override void OnApply(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            // Multiplicative: speed multiplier collapses by (1 - magnitude). Clamp ≥ 0.
            float reduction = 1f - magnitude;
            if (reduction < 0f) reduction = 0f;
            state.MultiplySpeed(reduction);
        }

        public override void OnTick(Enemy enemy, float dtSeconds) { /* no per-tick work */ }

        public override void OnExpire(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            float reduction = 1f - magnitude;
            if (reduction <= 0f) reduction = 0.0001f; // avoid div-by-zero on Freeze edge cases
            state.DivideSpeed(reduction);
        }

        /// <summary>Apply the magnitude DELTA when refresh upgrades the slow strength so
        /// OnExpire's restore math stays consistent. If the new magnitude is weaker we
        /// leave the existing reduction untouched (no double-restore).</summary>
        public override void Refresh(Enemy enemy, int newDurationMs, float newMagnitude)
        {
            if (newMagnitude > magnitude && enemy != null)
            {
                // Upgrade in-place: multiply by (1 - new) / (1 - old). When OnExpire later
                // divides by (1 - new), the net is a full restore of the original speed.
                float oldReduction = 1f - magnitude;
                float newReduction = 1f - newMagnitude;
                if (oldReduction <= 0f) oldReduction = 0.0001f;
                if (newReduction < 0f) newReduction = 0f;
                float delta = newReduction / oldReduction;
                var state = StatusEffectApplier.GetOrCreateState(enemy);
                state.MultiplySpeed(delta);
            }
            base.Refresh(enemy, newDurationMs, newMagnitude);
        }
    }
}
