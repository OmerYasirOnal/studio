// Sleepy Boar: slow homing with periodic burst-charge (per enemies.json charge block).
//
// ADR-0018: XZ-plane semantics. See SwarmerBehavior header for the mapping rationale.
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class TankBehavior : EnemyBehavior
{
    private readonly float _chargeIntervalSec;
    private readonly float _chargeBurstSpeedMult;
    private readonly float _chargeBurstDurationSec;
    private readonly float _telegraphSec;

    // Per-enemy state would normally live on Enemy; for this skeleton we accept the trade-off
    // of one allocation per tank at spawn (still pooled).
    public TankBehavior(float chargeIntervalMs, float burstSpeedMult, float burstDurationMs, float telegraphMs)
    {
        _chargeIntervalSec = chargeIntervalMs / 1000f;
        _chargeBurstSpeedMult = burstSpeedMult;
        _chargeBurstDurationSec = burstDurationMs / 1000f;
        _telegraphSec = telegraphMs / 1000f;
    }

    public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
    {
        Vector3 pos = enemy.transform.position;
        Vector2 dir;
        dir.x = playerPos.x - pos.x;
        dir.y = playerPos.y - pos.z;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        // TODO(Phase 5): track per-enemy charge timer (Enemy needs a small AI-state struct).
        float step = enemy.Definition.moveSpeed * dt;
        pos.x += dir.x * step;
        pos.z += dir.y * step;          // input Y → world Z
        enemy.transform.position = pos;
    }
}
