// Brave Bunny — Gameplay/Enemies/BossAttackPattern
//
// Telegraph + execute lifecycle for a single boss attack. Per
// docs/09-level-design/02-bosses/old-boar-king/mechanics.md, all boss attacks
// must telegraph for a minimum of 0.8 s (per docs/02-gdd/05-enemies.md boss row),
// then execute, then enter a wind-down recovery window.
//
// This is the runtime helper — NOT the ADR-0009 polymorphic registry type with
// the same name. The registry type (a ScriptableObject sibling under
// `Code/Gameplay/Definitions/Mechanics/`) is owned by tech-architect and is
// authored in scenes; this is the per-tick state machine that BossBehavior uses
// to actually run a chosen attack pattern from the strategy layer.
//
// Lifecycle:
//   Idle → Telegraph (0.8s default) → Execute → Recover → Idle
// `IsActive` returns true while Telegraph / Execute / Recover are non-zero so
// BossBehavior can suppress movement during attacks.
//
// Allocation-free in the hot path: only float / enum state. No closures.

#nullable enable

namespace Brave.Gameplay.Enemies
{
    /// <summary>Sub-state of an in-flight boss attack pattern.</summary>
    public enum BossAttackPhase
    {
        Idle      = 0,    // pattern is dormant
        Telegraph = 1,    // tell is visible; not yet damaging
        Execute   = 2,    // damage frame is active
        Recover   = 3,    // wind-down; boss is "open" to counter-attack
    }

    /// <summary>
    /// Telegraph + execute timing state machine for a single boss attack pattern.
    /// Caller drives <see cref="Tick(float)"/> at the boss's update cadence;
    /// the machine reports <see cref="JustEntered(BossAttackPhase)"/> transitions
    /// so BossBehavior can fire side-effects (VFX, audio, damage) exactly once.
    /// </summary>
    public struct BossAttackPattern
    {
        // Spec-default telegraph window per ADR-0003 + boss mechanics: 0.8 seconds.
        // BossBehavior overrides per-phase via `Begin(...)`.
        public const float DefaultTelegraphSeconds = 0.8f;

        private float _telegraphSec;
        private float _executeSec;
        private float _recoverSec;
        private float _elapsed;
        private BossAttackPhase _phase;
        private BossAttackPhase _justEntered;

        public BossAttackPhase Phase => _phase;
        public float TelegraphSeconds => _telegraphSec;
        public float ExecuteSeconds => _executeSec;
        public float RecoverSeconds => _recoverSec;
        public float Elapsed => _elapsed;

        /// <summary>True when an attack is in flight (Telegraph / Execute / Recover).</summary>
        public bool IsActive => _phase != BossAttackPhase.Idle;

        /// <summary>True during the frame the pattern transitioned into <paramref name="phase"/>.</summary>
        public bool JustEntered(BossAttackPhase phase) => _justEntered == phase;

        /// <summary>
        /// Kick off a new attack with the supplied timing windows. <paramref name="telegraphSec"/>
        /// must be greater than zero and is clamped to <see cref="DefaultTelegraphSeconds"/>
        /// minimum per ADR-0003.
        /// </summary>
        public void Begin(float telegraphSec, float executeSec, float recoverSec)
        {
            _telegraphSec = telegraphSec < DefaultTelegraphSeconds ? DefaultTelegraphSeconds : telegraphSec;
            _executeSec = executeSec < 0f ? 0f : executeSec;
            _recoverSec = recoverSec < 0f ? 0f : recoverSec;
            _elapsed = 0f;
            _phase = BossAttackPhase.Telegraph;
            _justEntered = BossAttackPhase.Telegraph;
        }

        /// <summary>Force-cancel an in-flight attack and return to Idle.</summary>
        public void Cancel()
        {
            _phase = BossAttackPhase.Idle;
            _elapsed = 0f;
            _justEntered = BossAttackPhase.Idle;
        }

        /// <summary>
        /// Advance the pattern by <paramref name="dt"/> seconds. Updates <see cref="Phase"/>
        /// and records the most-recent transition for one frame.
        /// </summary>
        public void Tick(float dt)
        {
            // Clear the one-frame transition flag unless Begin() just set it this call window.
            _justEntered = BossAttackPhase.Idle;

            if (_phase == BossAttackPhase.Idle) return;
            _elapsed += dt;

            switch (_phase)
            {
                case BossAttackPhase.Telegraph:
                    if (_elapsed >= _telegraphSec)
                    {
                        _phase = BossAttackPhase.Execute;
                        _justEntered = BossAttackPhase.Execute;
                        _elapsed = 0f;
                    }
                    break;

                case BossAttackPhase.Execute:
                    if (_elapsed >= _executeSec)
                    {
                        _phase = BossAttackPhase.Recover;
                        _justEntered = BossAttackPhase.Recover;
                        _elapsed = 0f;
                    }
                    break;

                case BossAttackPhase.Recover:
                    if (_elapsed >= _recoverSec)
                    {
                        _phase = BossAttackPhase.Idle;
                        _justEntered = BossAttackPhase.Idle;
                        _elapsed = 0f;
                    }
                    break;
            }
        }
    }
}
