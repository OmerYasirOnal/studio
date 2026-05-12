#nullable enable
// Wave 4: receiver-side helper that applies a damage amount to an EnemyBase / EnemyHealth.
// Lives alongside DamageCalculator/DamageFormula (which compute the *amount*); this class
// is the lightweight *application* surface called from the projectile hit-path.
//
// Enemy death wiring (XP gem drop, death VFX, pool return) is OUT OF SCOPE for the Wave 4
// vertical slice. EnemyHealth.Die already fires registered IDeathListeners; the pool-return
// is the listener chain's responsibility per existing code comment. Follow-up surfaced in
// the hand-off.

using UnityEngine;

using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Stateless helper for applying a pre-computed damage value to an enemy. Wraps
    /// <see cref="EnemyHealth.TakeHit"/> so callers (projectiles, mines, beams) don't have
    /// to construct a <see cref="HitInfo"/> by hand at every call site.
    /// </summary>
    public static class DamageApplier
    {
        /// <summary>Apply <paramref name="damage"/> to the enemy. Returns <c>true</c> when the
        /// hit was the killing blow (HP dropped to ≤ 0 as a result of this hit).</summary>
        public static bool TryApply(EnemyBase enemy, float damage, Vector3 hitPoint, int sourceId)
        {
            if (enemy == null) return false;
            EnemyHealth health = enemy.Health;
            if (health == null) return false;
            if (!health.IsAlive) return false;

            float hpBefore = health.Hp;
            var info = new HitInfo(damage, hitPoint, isCrit: false, sourceId: sourceId,
                targetId: enemy.GetInstanceID());
            health.TakeHit(info);

            // Killing-blow signal: HP crossed zero on this hit. (Death-listeners chain handles
            // XP/loot/pool-return — we don't duplicate that work here.)
            return hpBefore > 0f && health.Hp <= 0f;
        }

        /// <summary>Pure-arithmetic variant for unit tests that don't want a MonoBehaviour.
        /// Returns the new HP after applying <paramref name="damage"/> (floor 0).</summary>
        public static float NewHpAfter(float hpBefore, float damage)
        {
            float next = hpBefore - damage;
            return next < 0f ? 0f : next;
        }

        /// <summary>Killing-blow predicate matching <see cref="TryApply"/>'s contract.</summary>
        public static bool IsKillingBlow(float hpBefore, float damage)
            => hpBefore > 0f && (hpBefore - damage) <= 0f;
    }
}
