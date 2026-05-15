#nullable enable
// Wave 10: Stun — prevents enemy attacks but does NOT slow movement. Set via Cloud
// weapons. Visually a brief "!" particle (see StatusEffectVisuals).

using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Stuns the enemy: sets a <see cref="StatusEffectState.CanAttack"/> flag
    /// to false for the duration. Does NOT reduce move speed (use Slow/Freeze for that).</summary>
    [BraveRegister("status.stun")]
    public sealed class StunEffect : StatusEffect
    {
        public override string TypeName => "status.stun";

        public StunEffect() { }

        public StunEffect(int durationMs)
        {
            Configure(durationMs, magnitude: 0f);
        }

        public override void OnApply(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.SetCanAttack(false);
        }

        public override void OnTick(Enemy enemy, float dtSeconds) { /* no-op */ }

        public override void OnExpire(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.SetCanAttack(true);
        }
    }
}
