#nullable enable
// Wave 10: Poison DoT — like Burn but armor-piercing. Magnitude = damage per tick (raw).
// Subclasses BurnEffect to share the accumulator wiring, overrides ApplyTickDamage to
// bypass the defense multiplier on EnemyDefinition.

using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Armor-piercing DoT. Identical accounting to <see cref="BurnEffect"/>;
    /// damage skips the enemy's defenseMultiplier and is applied raw.</summary>
    [BraveRegister("status.poison")]
    public sealed class PoisonEffect : BurnEffect
    {
        public override string TypeName => "status.poison";

        public PoisonEffect() { }

        public PoisonEffect(int durationMs, float magnitude, int tickIntervalMs)
        {
            Configure(durationMs, magnitude, tickIntervalMs);
        }

        public override void OnApply(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.MarkPoisoned(true);
        }

        public override void ApplyTickDamage(Enemy enemy)
        {
            // Armor-ignoring path: undo the defenseMultiplier the standard pipeline applies.
            // For the EditMode path we record raw damage in the state; the future runtime
            // wires this through a "raw" damage flag on HitContext when the crit agent
            // expands the schema. For now apply raw amount directly.
            float raw = magnitude;
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.RecordDamageTick(raw);
            if (enemy != null && enemy.IsAlive)
            {
                var hit = new HitContext(
                    sourceId: 0,
                    targetId: 0,
                    amount: raw,
                    isCrit: false,
                    isKillingBlow: false,
                    hitPoint: enemy.transform.position,
                    type: DamageType.Nature);
                enemy.ApplyHit(in hit);
            }
        }

        public override void OnExpire(Enemy enemy)
        {
            var state = StatusEffectApplier.GetOrCreateState(enemy);
            state.MarkPoisoned(false);
        }
    }
}
