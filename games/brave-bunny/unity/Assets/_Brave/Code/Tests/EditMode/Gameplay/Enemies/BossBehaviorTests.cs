// QA — Old Boar King boss behaviour EditMode tests.
// Subjects under test:
//   * Brave.Gameplay.Enemies.BossPhaseState — HP-gated phase transitions
//   * Brave.Gameplay.Enemies.BossAttackPattern — telegraph window + execute lifecycle
//   * Brave.Gameplay.Enemies.Behaviors.BossBehavior — phase events, defeat hook,
//     channel raises
// Specs:
//   * docs/09-level-design/02-bosses/old-boar-king/mechanics.md § Phases + Telegraphs
//   * docs/decisions/0003-hitstop-timings.md § Boss phase change 150ms (telegraph 0.8s min)
//   * docs/decisions/0006-enemy-hp-recalibration.md § Phase gates 66% / 33%
// Why:
//   * Phase transitions must fire exactly once per HP-gate crossing, on the
//     correct side of the threshold.
//   * Telegraph window must be >= 0.8 s (ADR-0003 boss attack tell minimum).
//   * Defeat must raise BossDefeatedChannel + EnemyKilledChannel + invoke
//     the run-end hook with cause="boss_defeated".
// Notes:
//   * Construction uses ScriptableObject.CreateInstance + new GameObject — valid
//     in EditMode. Each test cleans up its scratch objects.
//   * No magic numbers: phase thresholds, telegraph timing, dt are named constants.

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Enemies.Behaviors;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Enemies
{
    [TestFixture]
    public class BossBehaviorTests
    {
        // ---- Constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float ChargeGate = 0.66f;        // ADR-0006: phase-2 gate
        private const float SlamGate   = 0.33f;        // ADR-0006: phase-3 gate
        private const float TickDt     = 1f / 30f;     // EnemyTicker runs at 30 Hz
        private const float TelegraphSeconds = 0.8f;   // ADR-0003 boss attack tell minimum
        private const float ExecuteSeconds   = 0.3f;
        private const float RecoverSeconds   = 0.6f;
        private const float MaxHp = 3000f;             // ADR-0006: Old Boar King total HP
        private const float Epsilon = 0.0001f;
        private const string BossId = "old-boar-king";

        private EnemyDefinition? _def;
        private GameObject? _bossGo;
        private Enemy? _boss;

        [SetUp]
        public void SetUp()
        {
            _def = ScriptableObject.CreateInstance<EnemyDefinition>();
            _def.slug = BossId;
            _def.role = EnemyRole.Boss;
            _def.baseHP = MaxHp;
            _def.moveSpeed = 2.0f;

            _bossGo = new GameObject("TestBoss");
            _boss = _bossGo.AddComponent<Enemy>();
            _boss!.Configure(_def, MaxHp, behavior: null!, owner: null!);
        }

        [TearDown]
        public void TearDown()
        {
            if (_bossGo != null) Object.DestroyImmediate(_bossGo);
            if (_def != null) Object.DestroyImmediate(_def);
        }

        // ---- Helpers ----

        /// <summary>Build a HitContext for an arbitrary damage amount. Other fields are inert.</summary>
        private static Brave.Gameplay.Damage.HitContext HitFor(float amount) =>
            new Brave.Gameplay.Damage.HitContext(
                sourceId: 0,
                targetId: 0,
                amount: amount,
                isCrit: false,
                isKillingBlow: amount >= MaxHp,
                hitPoint: Vector3.zero,
                type: Brave.Gameplay.Damage.DamageType.Kinetic);

        private static BossConfig MakeConfig() => new()
        {
            BossId = BossId,
            ChargePhaseGate = ChargeGate,
            SlamPhaseGate = SlamGate,
            TelegraphSeconds = TelegraphSeconds,
            ExecuteSeconds = ExecuteSeconds,
            RecoverSeconds = RecoverSeconds,
            // Push cadences so the attack loop doesn't fire during a single-tick test.
            ApproachCadence = 99f,
            ChargeCadence = 99f,
            SlamCadence = 99f,
        };

        // ---- BossPhaseState ----

        [Test]
        public void PhaseState_StartsInApproach_AtFullHp()
        {
            var state = new BossPhaseState();
            state.Initialise(ChargeGate, SlamGate);
            Assert.That(state.Current, Is.EqualTo(BossPhase.Approach));
            Assert.That(state.HpPhaseFor(1.0f), Is.EqualTo(BossPhase.Approach));
        }

        [Test]
        public void PhaseState_TransitionsToCharge_AtChargeGate()
        {
            var state = new BossPhaseState();
            state.Initialise(ChargeGate, SlamGate);

            // Above gate: no transition.
            Assert.That(state.EvaluateTransition(ChargeGate + 0.01f), Is.Null);
            Assert.That(state.Current, Is.EqualTo(BossPhase.Approach));

            // At the gate: transition fires.
            var promoted = state.EvaluateTransition(ChargeGate);
            Assert.That(promoted, Is.EqualTo(BossPhase.Charge));
            Assert.That(state.Current, Is.EqualTo(BossPhase.Charge));
        }

        [Test]
        public void PhaseState_TransitionsToSlam_AtSlamGate()
        {
            var state = new BossPhaseState();
            state.Initialise(ChargeGate, SlamGate);

            state.EvaluateTransition(ChargeGate);   // Approach -> Charge
            Assert.That(state.Current, Is.EqualTo(BossPhase.Charge));

            var promoted = state.EvaluateTransition(SlamGate);
            Assert.That(promoted, Is.EqualTo(BossPhase.Slam));
            Assert.That(state.Current, Is.EqualTo(BossPhase.Slam));
        }

        [Test]
        public void PhaseState_DoesNotRegress_WhenHpRecovers()
        {
            var state = new BossPhaseState();
            state.Initialise(ChargeGate, SlamGate);
            state.EvaluateTransition(SlamGate - 0.01f);     // jump straight to Slam
            Assert.That(state.Current, Is.EqualTo(BossPhase.Slam));

            // Even if HP recovers above the charge gate, the phase machine sticks at Slam.
            var promoted = state.EvaluateTransition(1.0f);
            Assert.That(promoted, Is.Null);
            Assert.That(state.Current, Is.EqualTo(BossPhase.Slam));
        }

        [Test]
        public void PhaseState_RecoverOverlay_DoesNotBreakHpGatedProgression()
        {
            var state = new BossPhaseState();
            state.Initialise(ChargeGate, SlamGate);
            state.EnterRecover();
            Assert.That(state.Current, Is.EqualTo(BossPhase.Recover));

            // ExitRecover snaps back to the HP-implied phase.
            state.ExitRecover(ChargeGate - 0.01f);
            Assert.That(state.Current, Is.EqualTo(BossPhase.Charge));
        }

        // ---- BossAttackPattern ----

        [Test]
        public void AttackPattern_BeginEnforcesMinimum_0_8s_Telegraph()
        {
            var attack = new BossAttackPattern();
            // Caller asks for 0.2s; pattern must clamp to the 0.8s minimum per ADR-0003.
            attack.Begin(telegraphSec: 0.2f, executeSec: ExecuteSeconds, recoverSec: RecoverSeconds);
            Assert.That(attack.TelegraphSeconds,
                Is.EqualTo(BossAttackPattern.DefaultTelegraphSeconds).Within(Epsilon),
                "ADR-0003: boss attack telegraph must be at least 0.8 s");
            Assert.That(attack.Phase, Is.EqualTo(BossAttackPhase.Telegraph));
            Assert.That(attack.IsActive, Is.True);
        }

        [Test]
        public void AttackPattern_RunsThrough_Telegraph_Execute_Recover_Idle()
        {
            var attack = new BossAttackPattern();
            attack.Begin(TelegraphSeconds, ExecuteSeconds, RecoverSeconds);

            // Drain the telegraph window in 30 Hz ticks.
            int telegraphTicks = Mathf.CeilToInt(TelegraphSeconds / TickDt) + 1;
            for (int i = 0; i < telegraphTicks; i++) attack.Tick(TickDt);
            Assert.That(attack.Phase, Is.EqualTo(BossAttackPhase.Execute),
                "After {0}s of ticks the attack must be in Execute", telegraphTicks * TickDt);

            int executeTicks = Mathf.CeilToInt(ExecuteSeconds / TickDt) + 1;
            for (int i = 0; i < executeTicks; i++) attack.Tick(TickDt);
            Assert.That(attack.Phase, Is.EqualTo(BossAttackPhase.Recover));

            int recoverTicks = Mathf.CeilToInt(RecoverSeconds / TickDt) + 1;
            for (int i = 0; i < recoverTicks; i++) attack.Tick(TickDt);
            Assert.That(attack.Phase, Is.EqualTo(BossAttackPhase.Idle));
            Assert.That(attack.IsActive, Is.False);
        }

        [Test]
        public void AttackPattern_TelegraphWindow_NotPrematurelyEnded()
        {
            // Mid-telegraph at 0.5 s should still report Telegraph (not Execute) —
            // the player must have the full 0.8 s tell.
            var attack = new BossAttackPattern();
            attack.Begin(TelegraphSeconds, ExecuteSeconds, RecoverSeconds);

            int halfwayTicks = Mathf.FloorToInt((TelegraphSeconds * 0.5f) / TickDt);
            for (int i = 0; i < halfwayTicks; i++) attack.Tick(TickDt);

            Assert.That(attack.Phase, Is.EqualTo(BossAttackPhase.Telegraph),
                "Telegraph must persist for the full window — premature exit blocks dodge readability");
        }

        // ---- BossBehavior ----

        [Test]
        public void BossBehavior_RaisesPhaseChannel_OnceAtChargeGate()
        {
            var phaseChannel = ScriptableObject.CreateInstance<BossPhaseChannel>();
            try
            {
                int phaseRaiseCount = 0;
                int lastPhase = -1;
                System.Action<BossPhaseEvent> listener = e => { phaseRaiseCount++; lastPhase = e.newPhase; };
                phaseChannel.Subscribe(listener);

                var behavior = new BossBehavior(MakeConfig(), phaseChannel);

                // Drain HP below charge gate.
                _boss!.ApplyHit(HitFor(MaxHp * (1f - ChargeGate + 0.01f)));
                behavior.Tick(_boss!, Vector2.zero, TickDt);

                Assert.That(phaseRaiseCount, Is.EqualTo(1),
                    "Exactly one phase event must fire on first gate-crossing");
                // BossPhase.Charge -> event payload = 2 (1-based newPhase).
                Assert.That(lastPhase, Is.EqualTo((int)BossPhase.Charge + 1));

                phaseChannel.Unsubscribe(listener);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(phaseChannel);
            }
        }

        [Test]
        public void BossBehavior_RaisesDefeatedChannel_OnDeath_WithCause_boss_defeated()
        {
            var phaseChannel = ScriptableObject.CreateInstance<BossPhaseChannel>();
            var defeatedChannel = ScriptableObject.CreateInstance<BossDefeatedChannel>();
            var enemyKilledChannel = ScriptableObject.CreateInstance<EnemyKilledChannel>();
            try
            {
                int defeatCount = 0;
                int killCount = 0;
                string? cause = null;
                string? defeatedBossId = null;

                System.Action<BossDefeatedEvent> defeatedListener = e =>
                {
                    defeatCount++;
                    defeatedBossId = e.bossId;
                };
                System.Action<EnemyKilledEvent> killListener = _ => killCount++;
                defeatedChannel.Subscribe(defeatedListener);
                enemyKilledChannel.Subscribe(killListener);

                var behavior = new BossBehavior(
                    MakeConfig(),
                    phaseChannel,
                    defeatedChannel,
                    enemyKilledChannel,
                    runSecondsGetter: () => 420f,
                    onDefeated: c => cause = c);

                // Kill the boss in one hit.
                _boss!.ApplyHit(HitFor(MaxHp + 1f));
                behavior.Tick(_boss!, Vector2.zero, TickDt);

                Assert.That(behavior.IsDefeated, Is.True);
                Assert.That(defeatCount, Is.EqualTo(1), "BossDefeatedChannel must fire exactly once on death");
                Assert.That(killCount, Is.EqualTo(1), "EnemyKilledChannel must fire so kill subscribers see boss death");
                Assert.That(defeatedBossId, Is.EqualTo(BossId));
                Assert.That(cause, Is.EqualTo(RunEndCause.BossDefeated),
                    "Run-end hook must receive cause=\"boss_defeated\" per scope");

                defeatedChannel.Unsubscribe(defeatedListener);
                enemyKilledChannel.Unsubscribe(killListener);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(phaseChannel);
                ScriptableObject.DestroyImmediate(defeatedChannel);
                ScriptableObject.DestroyImmediate(enemyKilledChannel);
            }
        }

        [Test]
        public void BossBehavior_DoesNotFireDefeated_Twice_OnSubsequentTicks()
        {
            var defeatedChannel = ScriptableObject.CreateInstance<BossDefeatedChannel>();
            try
            {
                int defeatCount = 0;
                System.Action<BossDefeatedEvent> listener = _ => defeatCount++;
                defeatedChannel.Subscribe(listener);

                var behavior = new BossBehavior(MakeConfig(), defeatedChannel: defeatedChannel);

                _boss!.ApplyHit(HitFor(MaxHp + 1f));
                behavior.Tick(_boss!, Vector2.zero, TickDt);
                behavior.Tick(_boss!, Vector2.zero, TickDt);     // second tick after death
                behavior.Tick(_boss!, Vector2.zero, TickDt);

                Assert.That(defeatCount, Is.EqualTo(1),
                    "Defeat chain must guard against repeated invocations on subsequent ticks");

                defeatedChannel.Unsubscribe(listener);
            }
            finally
            {
                ScriptableObject.DestroyImmediate(defeatedChannel);
            }
        }
    }
}
