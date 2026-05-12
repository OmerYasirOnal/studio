// Big Onion: mid-boss with one signature attack. Slow tank with a long telegraph.
//
// ADR-0018: XZ-plane semantics. See SwarmerBehavior header for the mapping rationale.
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class EliteBehavior : EnemyBehavior
{
    private readonly float _telegraphSec;

    public EliteBehavior(float telegraphMs) => _telegraphSec = telegraphMs / 1000f;

    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector3 pos = enemy.transform.position;
        Vector2 dir;
        dir.x = playerPos.x - pos.x;
        dir.y = playerPos.y - pos.z;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        float step = enemy.Definition.moveSpeed * dt;
        pos.x += dir.x * step;
        pos.z += dir.y * step;          // input Y → world Z
        enemy.transform.position = pos;
        // TODO(Phase 5): periodic signature attack with _telegraphSec lead-in, AOE damage on land.
    }
}
