// Brave Bunny — Gameplay/Enemies/Behaviors/BossBehavior
//
// Per-tick AI strategy for the Old Boar King. Implements EnemyBehavior so the
// existing AI ticker can drive it through the Enemy strategy layer
// (see SwarmerBehavior / EliteBehavior for the per-tick contract).
//
// Per-instance state (phase machine + in-flight attack) is held here because
// only one boss exists at a time (per GDD: one boss per biome at a time, and
// ADR-0020 WaveSpawner uses bossCapacity=1). The pool returns the same boss
// instance via IDeathListener (see EnemyPoolReturnOnDeath); we re-`Configure`
// the BossBehavior at spawn-time which calls `Reset()`.
//
// Phase machine sourced from BossPhaseState (HP-gated). Attack lifecycle uses
// BossAttackPattern (0.8s telegraph min per ADR-0003). On phase transition,
// raises BossPhaseChannel for the audio bindings + HUD; on defeat, raises
// BossDefeatedChannel + EnemyKilledChannel + invokes RunController.End.
//
// All tunables (phase HP gates, telegraph durations, speeds) come from the
// supplied BossConfig — never inlined per CLAUDE.md principle 6.
//
// Spec refs:
//   * docs/09-level-design/02-bosses/old-boar-king/mechanics.md
//   * docs/decisions/0003-hitstop-timings.md
//   * docs/decisions/0006-enemy-hp-recalibration.md
//   * docs/decisions/0020-weapon-archetype-config-and-boss-enum.md

#nullable enable

using System;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Events;
using UnityEngine;

namespace Brave.Gameplay.Enemies.Behaviors
{
    /// <summary>
    /// Designer-tunable configuration bundle for the boss fight. Built from
    /// <c>BossDefinition</c> + <c>data/balance/enemies.json</c>; held by
    /// <see cref="BossBehavior"/> so the strategy is self-contained.
    /// </summary>
    public sealed class BossConfig
    {
        /// <summary>Designer slug, e.g. "old-boar-king".</summary>
        public string BossId = string.Empty;

        // ---- Phase HP gates (0..1 fractions of MaxHp). Read from BossDefinition.phases. ----
        public float ChargePhaseGate = 0.66f;
        public float SlamPhaseGate = 0.33f;

        // ---- Movement speeds per phase (world units / sec). Read from enemies.json ----
        public float ApproachSpeed = 2.0f;
        public float ChargeSpeed   = 4.0f;
        public float SlamSpeed     = 2.5f;

        // ---- Attack timings (seconds). ADR-0003 sets 0.8s telegraph minimum for the boss. ----
        public float TelegraphSeconds = 0.8f;
        public float ExecuteSeconds   = 0.3f;
        public float RecoverSeconds   = 0.6f;

        // ---- Per-phase attack cadence (seconds between attacks). ----
        public float ApproachCadence = 2.0f;
        public float ChargeCadence   = 3.5f;
        public float SlamCadence     = 1.6f;

        /// <summary>Damage dealt on contact (Approach phase body-slam).</summary>
        public float ContactDamage = 35f;

        /// <summary>AOE damage dealt by the Slam shockwave.</summary>
        public float AoeDamage = 50f;
    }

    /// <summary>
    /// Per-instance boss AI strategy. Created fresh per boss spawn; one
    /// instance per active boss (single-active per ADR-0020 bossCapacity=1).
    /// Allocation-free in <see cref="Tick"/> — no closures, no list growth.
    /// </summary>
    public sealed class BossBehavior : EnemyBehavior
    {
        private readonly BossConfig _config;
        private readonly BossPhaseChannel? _phaseChannel;
        private readonly BossDefeatedChannel? _defeatedChannel;
        private readonly EnemyKilledChannel? _enemyKilledChannel;
        private readonly Func<float>? _runSecondsGetter;
        private readonly Action<string>? _onDefeated;  // run-end hook (RunController.End(Win, "boss_defeated"))

        private BossPhaseState _phaseState;
        private BossAttackPattern _attack;
        private float _attackCooldown;
        private bool _defeated;

        public BossPhase CurrentPhase => _phaseState.Current;
        public BossAttackPhase CurrentAttackPhase => _attack.Phase;
        public bool IsDefeated => _defeated;
        public BossConfig Config => _config;

        public BossBehavior(
            BossConfig config,
            BossPhaseChannel? phaseChannel = null,
            BossDefeatedChannel? defeatedChannel = null,
            EnemyKilledChannel? enemyKilledChannel = null,
            Func<float>? runSecondsGetter = null,
            Action<string>? onDefeated = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _phaseChannel = phaseChannel;
            _defeatedChannel = defeatedChannel;
            _enemyKilledChannel = enemyKilledChannel;
            _runSecondsGetter = runSecondsGetter;
            _onDefeated = onDefeated;

            Reset();
        }

        /// <summary>
        /// Reset per-instance state at spawn / pool-return. Phase machine starts at
        /// <see cref="BossPhase.Approach"/>; the cooldown is seeded so the first attack
        /// doesn't fire on the very first tick (player needs time to see the boss).
        /// </summary>
        public void Reset()
        {
            _phaseState.Initialise(_config.ChargePhaseGate, _config.SlamPhaseGate);
            _attack.Cancel();
            _attackCooldown = _config.ApproachCadence;
            _defeated = false;
        }

        /// <summary>
        /// Per-tick AI: 1) walk toward player at phase-scaled speed (when not attacking),
        /// 2) advance the in-flight attack lifecycle, 3) start a new attack when cooldown
        /// expires, 4) check for phase transitions / defeat against current HP.
        /// </summary>
        public override void Tick(Enemy enemy, Vector2 playerPos, float dt)
        {
            if (_defeated) return;
            if (enemy == null) return;

            // 1) Death gate — runs FIRST so it fires even if Enemy.ApplyHit already
            //    flipped IsAlive on the previous AI tick frame. Once HP hits 0, fire
            //    the defeat chain exactly once.
            if (enemy.Hp <= 0f)
            {
                FireDefeated(enemy);
                return;
            }

            if (!enemy.IsAlive) return;

            // 2) HP-gated phase transition. Promote phase + raise BossPhaseChannel
            //    once per gate crossing.
            float hpFrac = enemy.MaxHp <= 0f ? 0f : enemy.Hp / enemy.MaxHp;
            BossPhase? promoted = _phaseState.EvaluateTransition(hpFrac);
            if (promoted.HasValue)
            {
                RaisePhaseChanged(promoted.Value);
            }

            // 3) Drive the attack lifecycle.
            _attack.Tick(dt);

            // 4) Movement is suppressed during an active attack (telegraph / execute /
            //    recover). When idle, walk toward the player at phase-scaled speed.
            if (!_attack.IsActive)
            {
                StepTowardPlayer(enemy, playerPos, dt);

                // 5) Cool down and trigger the next attack.
                _attackCooldown -= dt;
                if (_attackCooldown <= 0f)
                {
                    _attack.Begin(
                        telegraphSec: _config.TelegraphSeconds,
                        executeSec:   _config.ExecuteSeconds,
                        recoverSec:   _config.RecoverSeconds);
                    _attackCooldown = CadenceFor(_phaseState.Current);
                }
            }
        }

        private void StepTowardPlayer(Enemy enemy, Vector2 playerPos, float dt)
        {
            Vector3 pos = enemy.transform.position;
            // ADR-0018 XZ-plane: playerPos.x → world.x, playerPos.y → world.z.
            Vector2 dir;
            dir.x = playerPos.x - pos.x;
            dir.y = playerPos.y - pos.z;
            float sq = dir.x * dir.x + dir.y * dir.y;
            if (sq < 0.0001f) return;
            float invLen = 1f / Mathf.Sqrt(sq);
            dir.x *= invLen;
            dir.y *= invLen;
            float speed = SpeedFor(_phaseState.Current);
            float step = speed * dt;
            pos.x += dir.x * step;
            pos.z += dir.y * step;
            enemy.transform.position = pos;
        }

        private float SpeedFor(BossPhase phase) => phase switch
        {
            BossPhase.Approach => _config.ApproachSpeed,
            BossPhase.Charge   => _config.ChargeSpeed,
            BossPhase.Slam     => _config.SlamSpeed,
            _                  => _config.ApproachSpeed,
        };

        private float CadenceFor(BossPhase phase) => phase switch
        {
            BossPhase.Approach => _config.ApproachCadence,
            BossPhase.Charge   => _config.ChargeCadence,
            BossPhase.Slam     => _config.SlamCadence,
            _                  => _config.ApproachCadence,
        };

        private void RaisePhaseChanged(BossPhase newPhase)
        {
            if (_phaseChannel == null) return;
            // BossPhaseEvent.newPhase is 1-based per existing event payload contract.
            _phaseChannel.Raise(new BossPhaseEvent(
                newPhase: (int)newPhase + 1,
                bossSlugHash: _config.BossId.GetHashCode()));
        }

        private void FireDefeated(Enemy enemy)
        {
            _defeated = true;
            _attack.Cancel();
            float runSeconds = _runSecondsGetter?.Invoke() ?? 0f;
            Vector3 pos = enemy.transform.position;

            _defeatedChannel?.Raise(new BossDefeatedEvent(
                bossId: _config.BossId,
                bossSlugHash: _config.BossId.GetHashCode(),
                runSeconds: runSeconds,
                position: pos));

            // Also fire EnemyKilledChannel so kill-count / audio / hitstop subscribers
            // pick up the boss death through the standard pathway.
            _enemyKilledChannel?.Raise(new EnemyKilledEvent(
                enemySlugHash: _config.BossId.GetHashCode(),
                position: pos,
                wasElite: false,
                runSeconds: runSeconds));

            // Run-end hook — RunController.End(RunOutcome.Win, "boss_defeated").
            _onDefeated?.Invoke(Brave.Gameplay.Run.RunEndCause.BossDefeated);
        }
    }
}
