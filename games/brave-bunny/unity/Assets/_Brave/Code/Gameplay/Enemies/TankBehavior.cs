// Sleepy Boar: slow homing with periodic burst-charge (per enemies.json charge block).
using Brave.Gameplay.Movement;
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
        Vector2 dir = (playerPos - (Vector2)enemy.transform.position).normalized;
        // TODO(Phase 5): track per-enemy charge timer (Enemy needs a small AI-state struct).
        float speed = enemy.Definition.moveSpeed;
        enemy.transform.position = Mover.Step(enemy.transform.position, dir, speed, dt);
    }
}
