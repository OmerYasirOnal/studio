#nullable enable
// Wave 10: Burn DoT — periodic damage. Magnitude = damage per tick. Tick interval
// configurable; default 500ms when caller doesn't override.

using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Damage-over-time effect. Every <see cref="StatusEffect.tickIntervalMs"/>
    /// milliseconds applies <see cref="StatusEffect.magnitude"/> damage to the target.
    /// Damage IS reduced by armor (use <see cref="PoisonEffect"/> for armor-piercing).</summary>
    [BraveRegister("status.burn")]
    public class BurnEffect : StatusEffect
    {
        public override string TypeName => "status.burn";

        public BurnEffect() { }

        public BurnEffect(int durationMs, float magnitude, int tickIntervalMs)
        {
            Configure(durationMs, magnitude, tickIntervalMs);
        }

        public override void OnApply(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.MarkBurning(true);
        }

        public override void OnTick(Enemy enemy, float dtSeconds)
        {
            // Tick accumulator drives discrete DoT pulses; the Applier advances
            // sinceLastTickMs and calls ApplyTickDamage when it crosses the threshold.
            // We split the hook so subclasses (Poison) can override the damage formula
            // without re-doing the accounting.
        }

        /// <summary>Apply one discrete DoT pulse — called by the Applier when
        /// <see cref="StatusEffect.sinceLastTickMs"/> ≥ <see cref="StatusEffect.tickIntervalMs"/>.
        /// Overridden by <see cref="PoisonEffect"/> to ignore armor.</summary>
        public virtual void ApplyTickDamage(Enemy enemy)
        {
            // For now: route through the same applier accounting that Slow/Stun use,
            // so PlayMode hooks land in one place. Magnitude is damage-per-tick.
            float dmg = magnitude;
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.RecordDamageTick(dmg);
            // Apply to live HP via Enemy.ApplyHit — preserves the existing damage path
            // so DamageApplier / HitResult schema (owned by crit agent) is untouched.
            if (enemy != null && enemy.IsAlive)
            {
                var hit = new HitContext(
                    sourceId: 0,
                    targetId: 0,
                    amount: dmg,
                    isCrit: false,
                    isKillingBlow: false,
                    hitPoint: enemy.transform.position,
                    type: DamageType.Solar);
                enemy.ApplyHit(in hit);
            }
        }

        public override void OnExpire(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.MarkBurning(false);
        }
    }
}
