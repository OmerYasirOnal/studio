// Concrete boss from docs/09-level-design/02-bosses/old-boar-king/mechanics.md
// Phase 1: charge + sweep. Phase 2: + hop attack + minion summon. Phase 3: + stomp + rage charge.
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public sealed class OldBoarKing : BossBase
{
    [Header("Attack timings (frames @ 60 fps from boss mechanics doc)")]
    [SerializeField] private int _chargeTelegraphFrames = 30;
    [SerializeField] private int _sweepTelegraphFrames  = 18;
    [SerializeField] private int _hopTelegraphFrames    = 24;
    [SerializeField] private int _stompTelegraphFrames  = 36;
    [SerializeField] private int _rageTelegraphFrames   = 12;

    [Header("Phase 2 / 3 cadences (seconds)")]
    [SerializeField] private float _hopCadenceSec   = 6f;
    [SerializeField] private float _stompCadenceSec = 5f;
    [SerializeField] private float _rageCadenceSec  = 7f;

    private float _phase2EnterSec;
    private bool _phase2SummonOnce;
    private bool _phase2SummonTwice;

    protected override void OnPhaseChanged(int newPhase)
    {
        switch (newPhase)
        {
            case 1: /* awake-and-grumpy: charge + sweep */ break;
            case 2:
                // TODO(Phase 5): spawn tree-stump props per arena.md.
                _phase2EnterSec = Time.time;
                _phase2SummonOnce = false;
                _phase2SummonTwice = false;
                SummonMinions();
                break;
            case 3:
                // TODO(Phase 5): spawn ground-crack decals; enable stomp + rage attacks.
                break;
        }
    }

    private void SummonMinions()
    {
        // TODO(Phase 5): summon 4 hop-slimes in N/E/S/W burst around boss.
    }
}
