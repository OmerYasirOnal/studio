// Hop-Slime / Bee-Buzz / Daisy-Bite: simple homing toward player.
//
// ADR-0018: XZ-plane semantics. Caller still passes a Vector2 `playerPos` (caller-space
// 2D coords — see EnemyBehavior.Tick), but the world is XZ (top-down camera per
// SceneSetup.cs), matching PlayerMover. Input X → world.x, input Y → world.z.
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class SwarmerBehavior : EnemyBehavior
{
    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector3 pos = enemy.transform.position;
        // playerPos is XY in caller-space → compare to world (x, z) on the ground plane.
        Vector2 dir;
        dir.x = playerPos.x - pos.x;
        dir.y = playerPos.y - pos.z;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        float step = enemy.Definition.moveSpeed * dt;
        pos.x += dir.x * step;
        pos.z += dir.y * step;          // input Y → world Z
        enemy.transform.position = pos;
    }
}
