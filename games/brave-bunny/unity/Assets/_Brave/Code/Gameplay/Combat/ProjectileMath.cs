#nullable enable
// Pure projectile-arithmetic helpers, extracted for unit testability per the PlayerMover
// .ComputeVelocity precedent. No Unity Component instantiation required.

using UnityEngine;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Stateless projectile math. Lives outside <see cref="Projectile"/> so EditMode tests
    /// can exercise the arithmetic without instantiating a MonoBehaviour.
    /// </summary>
    public static class ProjectileMath
    {
        // ---- UI / perf constants — explicitly NOT balance numbers (per dispatch self-review). ----

        /// <summary>Fraction of the weapon's fire-rate window that a projectile spends in flight.
        /// 1/2 keeps projectiles readable on screen but well before the next cast lands.</summary>
        public const float RangeTravelFractionOfRate = 0.5f;

        /// <summary>Lower-bound projectile-travel duration. Guards against fireRate→0 collapse.</summary>
        public const float MinTravelSeconds = 0.05f;

        /// <summary>Below this squared magnitude we treat the facing vector as zero.</summary>
        public const float FacingEpsilonSqr = 1e-6f;

        /// <summary>Defensive ceiling on burst-casts in a single tick — prevents an infinite
        /// loop if a degenerate fireRate slips through the Awake guard. 32 is well above any
        /// reasonable cast cadence (Sunbeam's 0.15 s rate × 60 fps frame = ~7 casts/frame max).</summary>
        public const int MaxCastsPerTick = 32;

        /// <summary>Decay a remaining-lifetime counter by <paramref name="dt"/>. Returned value
        /// can be ≤ 0, in which case the caller despawns the projectile.</summary>
        public static float DecayLifetime(float lifetimeRemaining, float dt) => lifetimeRemaining - dt;

        /// <summary>True when the projectile should be returned to its pool (lifetime exhausted).</summary>
        public static bool ShouldExpire(float lifetimeRemaining) => lifetimeRemaining <= 0f;

        /// <summary>Pure step: new world position after travelling at speed for dt seconds
        /// along the normalised <paramref name="dir"/>. Y-axis is preserved (we operate on XZ).</summary>
        public static Vector3 Step(Vector3 pos, Vector3 dir, float speed, float dt)
        {
            pos.x += dir.x * speed * dt;
            pos.y += dir.y * speed * dt;
            pos.z += dir.z * speed * dt;
            return pos;
        }
    }
}
