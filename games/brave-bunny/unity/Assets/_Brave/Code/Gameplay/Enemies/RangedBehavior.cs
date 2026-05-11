// Archer Mole: kite + fire. Maintains kite_distance_units from player; fires when in window.
using Brave.Gameplay.Movement;
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class RangedBehavior : EnemyBehavior
{
    private readonly float _kiteDistance;
    private readonly float _fireWindowMin;
    private readonly float _fireWindowMax;
    private readonly float _telegraphSec;
    private readonly float _projectileSpeed;

    public RangedBehavior(float kiteDistance, Vector2 fireWindow, float telegraphMs, float projectileSpeed)
    {
        _kiteDistance = kiteDistance;
        _fireWindowMin = fireWindow.x;
        _fireWindowMax = fireWindow.y;
        _telegraphSec = telegraphMs / 1000f;
        _projectileSpeed = projectileSpeed;
    }

    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector2 selfPos = enemy.transform.position;
        float distance = Vector2.Distance(selfPos, playerPos);
        Vector2 dir;
        if (distance < _kiteDistance) dir = (selfPos - playerPos).normalized;          // back away
        else if (distance > _fireWindowMax) dir = (playerPos - selfPos).normalized;     // close gap
        else dir = Vector2.zero;                                                        // hold + fire

        if (dir != Vector2.zero)
            enemy.transform.position = Mover.Step(enemy.transform.position, dir, enemy.Definition.moveSpeed, dt);

        // TODO(Phase 5): telegraph + fire spawn through EnemyProjectilePool.
    }
}
