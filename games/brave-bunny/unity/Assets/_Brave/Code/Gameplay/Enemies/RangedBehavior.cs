// Archer Mole: kite + fire. Maintains kite_distance_units from player; fires when in window.
//
// ADR-0018: XZ-plane semantics. See SwarmerBehavior header for the mapping rationale.
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
        Vector3 pos = enemy.transform.position;
        // self in caller-space XY (world x, world z):
        Vector2 self;
        self.x = pos.x;
        self.y = pos.z;
        float distance = Vector2.Distance(self, playerPos);

        Vector2 dir;
        if (distance < _kiteDistance) dir = (self - playerPos).normalized;          // back away
        else if (distance > _fireWindowMax) dir = (playerPos - self).normalized;     // close gap
        else dir = Vector2.zero;                                                     // hold + fire

        if (dir != Vector2.zero)
        {
            float step = enemy.Definition.moveSpeed * dt;
            pos.x += dir.x * step;
            pos.z += dir.y * step;      // input Y → world Z
            enemy.transform.position = pos;
        }

        // TODO(Phase 5): telegraph + fire spawn through EnemyProjectilePool.
    }
}
