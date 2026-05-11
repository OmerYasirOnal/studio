// Big Onion: mid-boss with one signature attack. Slow tank with a long telegraph.
using Brave.Gameplay.Movement;
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class EliteBehavior : EnemyBehavior
{
    private readonly float _telegraphSec;

    public EliteBehavior(float telegraphMs) => _telegraphSec = telegraphMs / 1000f;

    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector2 dir = (playerPos - (Vector2)enemy.transform.position).normalized;
        enemy.transform.position = Mover.Step(enemy.transform.position, dir, enemy.Definition.moveSpeed, dt);
        // TODO(Phase 5): periodic signature attack with _telegraphSec lead-in, AOE damage on land.
    }
}
