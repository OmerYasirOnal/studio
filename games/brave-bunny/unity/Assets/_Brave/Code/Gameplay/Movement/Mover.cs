using UnityEngine;

namespace Brave.Gameplay.Movement;

/// <summary>
/// Stateless velocity-application helper. Used by both the hero (via <see cref="PlayerMover"/>)
/// and enemy behaviour strategies. Bypasses Unity Physics on swarmers per tech-spec 05.
/// </summary>
public static class Mover
{
    /// <summary>
    /// Apply a velocity for one frame and return the new world position. Caller passes
    /// pre-clamped speed (per balance/00-formulas.md § 3 cap of 9.0 u/s).
    /// </summary>
    public static Vector3 Step(Vector3 worldPos, Vector2 direction, float speedUnitsPerSec, float dt)
    {
        if (direction.sqrMagnitude > 1f) direction.Normalize();
        Vector3 delta = new(direction.x * speedUnitsPerSec * dt, direction.y * speedUnitsPerSec * dt, 0f);
        return worldPos + delta;
    }
}
