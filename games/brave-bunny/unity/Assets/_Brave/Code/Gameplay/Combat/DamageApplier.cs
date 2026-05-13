#nullable enable
// Wave 4: receiver-side helper that applies a damage amount to an EnemyBase / EnemyHealth.
// Lives alongside DamageCalculator/DamageFormula (which compute the *amount*); this class
// is the lightweight *application* surface called from the projectile hit-path.
//
// ADR-0019 item 3: on killing blow, DamageApplier now finds all IDeathListener components
// on the target GameObject and invokes OnDeath. EnemyHealth.TakeHit handles its own
// Enemies.IDeathListener chain first; the Combat.IDeathListener chain (pool-return etc.)
// fires after, so drops/XP from the EnemyHealth chain can still spawn before the object
// is deactivated.

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
        /// hit was the killing blow (HP dropped to ≤ 0 as a result of this hit).
        /// On killing blow, all <see cref="IDeathListener"/> components on the enemy's
        /// GameObject are invoked exactly once (idempotency: if HP was already ≤ 0 before
        /// this call, no listeners are fired).</summary>
        public static bool TryApply(EnemyBase enemy, float damage, Vector3 hitPoint, int sourceId)
        {
            if (enemy == null) return false;
            EnemyHealth health = enemy.Health;
            if (health == null) return false;
            if (!health.IsAlive) return false;   // already dead — idempotency guard

            float hpBefore = health.Hp;
            var info = new HitInfo(damage, hitPoint, isCrit: false, sourceId: sourceId,
                targetId: enemy.GetInstanceID());
            health.TakeHit(info);

            bool killingBlow = hpBefore > 0f && health.Hp <= 0f;
            if (killingBlow)
            {
                // ADR-0019 item 3: fire Combat.IDeathListener components (e.g. pool-return).
                var listeners = enemy.GetComponents<IDeathListener>();
                for (int i = 0; i < listeners.Length; i++)
                    listeners[i].OnDeath(enemy.gameObject);
            }
            return killingBlow;
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
