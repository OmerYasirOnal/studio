// Hop-Slime / Bee-Buzz / Daisy-Bite: simple homing toward player.
using Brave.Gameplay.Movement;
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class SwarmerBehavior : EnemyBehavior
{
    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector2 selfPos = enemy.transform.position;
        Vector2 dir = playerPos - selfPos;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        float speed = enemy.Definition.moveSpeed;
        enemy.transform.position = Mover.Step(enemy.transform.position, dir, speed, dt);
    }
}
