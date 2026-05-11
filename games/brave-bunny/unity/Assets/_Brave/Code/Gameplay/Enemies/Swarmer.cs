#nullable enable
// GDD 05 § Swarmer: trivial homing-to-player movement. Sprite-flip animation (2 frames).
// Per tech-spec 05 § Collision: swarmers have NO Rigidbody — radial overlap test.

using System;

using UnityEngine;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Swarmer behavior — moves directly toward the hero each tick. The cheapest enemy class;
    /// the wave driver targets 100+ of these on screen simultaneously.
    /// </summary>
    public sealed class Swarmer : EnemyBase
    {
        public override void TickBehavior(float dt)
        {
            if (Hero == null) return;

            Vector3 toHero = Hero.position - transform.position;
            float dist = toHero.magnitude;
            if (dist < 0.01f) return;

            Vector3 step = (toHero / dist) * (ScaledMoveSpeed * dt);
            transform.position += step;

            // Flip sprite based on movement direction (no bone anim per tech-spec 05).
            if (step.x != 0f)
            {
                Vector3 s = transform.localScale;
                s.x = step.x < 0f ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
                transform.localScale = s;
            }
        }
    }
}
