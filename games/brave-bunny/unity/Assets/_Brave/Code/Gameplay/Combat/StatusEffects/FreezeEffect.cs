#nullable enable
// Wave 10: Freeze — hard-stop speed to 0 + visual tint. Stronger than Slow; non-stacking
// (overlapping freezes refresh the longer duration rather than compounding).

using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Hard freeze — sets the enemy's speed multiplier to 0 and applies a
    /// frozen-state flag visuals consult for the blue tint. Restores normal speed on expire.</summary>
    [BraveRegister("status.freeze")]
    public sealed class FreezeEffect : StatusEffect
    {
        public override string TypeName => "status.freeze";

        // Cached pre-freeze speed multiplier so OnExpire can restore exactly,
        // independent of any Slow effects active concurrently.
        private float _preFreezeMultiplier;

        public FreezeEffect() { }

        public FreezeEffect(int durationMs)
        {
            // magnitude is unused for Freeze (always sets speed to 0); duration only.
            Configure(durationMs, magnitude: 0f);
        }

        public override void OnApply(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            _preFreezeMultiplier = state.SpeedMultiplier;
            state.SetSpeedMultiplier(0f);
            state.MarkFrozen(true);
        }

        public override void OnTick(Enemy enemy, float dtSeconds) { /* no per-frame work */ }

        public override void OnExpire(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.SetSpeedMultiplier(_preFreezeMultiplier);
            state.MarkFrozen(false);
            _preFreezeMultiplier = 1f;
        }

        public override void Reset()
        {
            base.Reset();
            _preFreezeMultiplier = 1f;
        }
    }
}
