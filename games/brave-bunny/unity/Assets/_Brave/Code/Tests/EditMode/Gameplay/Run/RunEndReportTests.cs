// QA — RunEndReport capture EditMode tests (Phase 5 Wave 6).
//
// Subjects under test:
//   * Brave.Gameplay.Run.RunController.BuildRunEndReport (projection)
//   * Brave.Gameplay.Run.RunController.End (channel emission + CurrentRunEndReport)
//   * Brave.Gameplay.Run.RunEndReport (struct/class field invariants)
//
// Spec refs:
//   * docs/02-gdd/01-core-loop.md § Run end resolutions.
//   * docs/06-tech-spec/03-save-triggers.md — RunEnd is a save trigger; report must
//     be populated before the save service reads it.
//   * docs/decisions/0021-hud-binding-contract.md — IRunRuntimeState single canonical.
//
// Pattern notes:
//   * RunController is a MonoBehaviour; we instantiate it on a throwaway GameObject and
//     drive mutators directly (no scene). DestroyImmediate in teardown.
//   * Event channels are SO assets; we create instance copies via ScriptableObject.CreateInstance
//     (the SaveServiceFileStoreTests pattern). Wired via reflection on private [SerializeField]s
//     so we never expose them publicly just for tests.

#nullable enable

using System.Reflection;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Run
{
    [TestFixture]
    public class RunEndReportTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string WeaponSlugCarrot = "carrot-boomerang";
        private const string WeaponSlugThorn = "thorn-whip";
        private const int Kills = 17;
        private const int Elites = 3;
        private const int Bosses = 1;
        private const int XpGained = 250;
        private const int GoldGained = 88;
        private const int SoulShards = 4;
        private const int PassXp = 33;
        private const int FinalLevel = 6;
        private const int WaveOrdinal = 5;

        // ---- per-test scratch ----
        private GameObject? _go;
        private RunController? _controller;
        private RunEndedChannel? _runEndedChannel;
        private EnemyKilledChannel? _enemyKilledChannel;
        private LevelUpChannel? _levelUpChannel;
        private PickupChannel? _pickupChannel;
        private DeathChannel? _deathChannel;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Test_RunController");
            _controller = _go.AddComponent<RunController>();
            _runEndedChannel = ScriptableObject.CreateInstance<RunEndedChannel>();
            _enemyKilledChannel = ScriptableObject.CreateInstance<EnemyKilledChannel>();
            _levelUpChannel = ScriptableObject.CreateInstance<LevelUpChannel>();
            _pickupChannel = ScriptableObject.CreateInstance<PickupChannel>();
            _deathChannel = ScriptableObject.CreateInstance<DeathChannel>();

            WirePrivateField("_runEndedChannel", _runEndedChannel);
            WirePrivateField("_enemyKilledChannel", _enemyKilledChannel);
            WirePrivateField("_levelUpChannel", _levelUpChannel);
            WirePrivateField("_pickupChannel", _pickupChannel);
            WirePrivateField("_deathChannel", _deathChannel);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_runEndedChannel != null) Object.DestroyImmediate(_runEndedChannel);
            if (_enemyKilledChannel != null) Object.DestroyImmediate(_enemyKilledChannel);
            if (_levelUpChannel != null) Object.DestroyImmediate(_levelUpChannel);
            if (_pickupChannel != null) Object.DestroyImmediate(_pickupChannel);
            if (_deathChannel != null) Object.DestroyImmediate(_deathChannel);
            _go = null;
            _controller = null;
        }

        private void WirePrivateField(string name, Object value)
        {
            var field = typeof(RunController).GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"RunController has no field '{name}'");
            field!.SetValue(_controller, value);
        }

        // ---- Round-trip projection ----

        [Test]
        public void BuildRunEndReport_CapturesAllRunningTotals()
        {
            // Simulate a run by driving mutators directly (no scene).
            // Kills: regular + elite + boss
            for (int i = 0; i < Kills - Elites - Bosses; i++) _controller!.RecordKill();
            for (int i = 0; i < Elites; i++) _controller!.RecordEliteKill();
            for (int i = 0; i < Bosses; i++) _controller!.RecordBossKill();

            // XP / level
            _controller!.AddXp(XpGained);
            for (int i = 1; i < FinalLevel; i++) _controller!.LevelUp(); // L1 -> L<FinalLevel>

            // Wave progression
            _controller!.SetWave(WaveOrdinal);

            // Currency
            _controller!.AddGold(GoldGained);
            _controller!.AddSoulShards(SoulShards);
            _controller!.AddPassXp(PassXp);

            // Loadout
            _controller!.RegisterEquippedWeapon(WeaponSlugCarrot);
            _controller!.RegisterEquippedWeapon(WeaponSlugThorn);
            _controller!.RegisterEquippedWeapon(WeaponSlugCarrot); // dedup

            var report = _controller!.BuildRunEndReport(RunOutcome.Lose);

            Assert.That(report.outcome, Is.EqualTo(RunOutcome.Lose));
            Assert.That(report.result, Is.EqualTo(RunResult.Death));
            Assert.That(report.deathCause, Is.EqualTo(RunEndCause.HpZero));
            Assert.That(report.totalKills, Is.EqualTo(Kills));
            Assert.That(report.elitesKilled, Is.EqualTo(Elites));
            Assert.That(report.bossesKilled, Is.EqualTo(Bosses));
            Assert.That(report.wavesCleared, Is.EqualTo(WaveOrdinal));
            Assert.That(report.finalLevel, Is.EqualTo(FinalLevel));
            Assert.That(report.xpGained, Is.EqualTo(XpGained));
            Assert.That(report.goldGained, Is.EqualTo(GoldGained));
            Assert.That(report.soulShardsEarned, Is.EqualTo(SoulShards));
            Assert.That(report.passXpEarned, Is.EqualTo(PassXp));
            Assert.That(report.weaponIdsUsed,
                Is.EquivalentTo(new[] { WeaponSlugCarrot, WeaponSlugThorn }),
                "Weapon slugs must be de-duplicated.");
            Assert.That(report.runDurationSeconds, Is.GreaterThanOrEqualTo(0f),
                "Run duration is timer-driven and never negative.");
        }

        [Test]
        public void BuildRunEndReport_DeathCause_DefaultsForOutcome()
        {
            var winReport = _controller!.BuildRunEndReport(RunOutcome.Win);
            Assert.That(winReport.deathCause, Is.EqualTo(RunEndCause.BossDefeated));

            var quitReport = _controller!.BuildRunEndReport(RunOutcome.Quit);
            Assert.That(quitReport.deathCause, Is.EqualTo(RunEndCause.PlayerQuit));

            var timeoutReport = _controller!.BuildRunEndReport(RunOutcome.Timeout);
            Assert.That(timeoutReport.deathCause, Is.EqualTo(RunEndCause.Timeout));
        }

        [Test]
        public void BuildRunEndReport_ExplicitCause_OverridesDefault()
        {
            var report = _controller!.BuildRunEndReport(RunOutcome.Win, cause: RunEndCause.WaveComplete);
            Assert.That(report.deathCause, Is.EqualTo(RunEndCause.WaveComplete));
            Assert.That(report.outcome, Is.EqualTo(RunOutcome.Win));
        }

        [Test]
        public void BuildRunEndReport_NoEquippedWeapons_EmptyArray()
        {
            var report = _controller!.BuildRunEndReport(RunOutcome.Lose);
            Assert.That(report.weaponIdsUsed, Is.Not.Null);
            Assert.That(report.weaponIdsUsed.Length, Is.EqualTo(0));
        }

        [Test]
        public void BuildRunEndReport_NoCharacter_EmptyCharacterId()
        {
            // SetUp didn't wire a CharacterDefinition — characterId must default to "".
            var report = _controller!.BuildRunEndReport(RunOutcome.Lose);
            Assert.That(report.characterId, Is.EqualTo(string.Empty));
        }

        [Test]
        public void RegisterEquippedWeapon_NullOrEmpty_IsNoOp()
        {
            _controller!.RegisterEquippedWeapon("");
            _controller!.RegisterEquippedWeapon(null!);
            var report = _controller!.BuildRunEndReport(RunOutcome.Lose);
            Assert.That(report.weaponIdsUsed.Length, Is.EqualTo(0));
        }

        // ---- Channel emission ----

        [Test]
        public void End_FiresRunEndedChannel_WithPopulatedReport()
        {
            _controller!.RecordKill();
            _controller!.RecordKill();
            _controller!.AddGold(42);
            _controller!.RegisterEquippedWeapon(WeaponSlugCarrot);

            RunEndedEvent? captured = null;
            void Handler(RunEndedEvent e) => captured = e;
            _runEndedChannel!.Subscribe(Handler);
            try
            {
                _controller!.End(RunResult.Death);
            }
            finally { _runEndedChannel!.Unsubscribe(Handler); }

            Assert.That(captured, Is.Not.Null, "RunEndedChannel must be raised on End().");
            Assert.That(captured!.Value.report, Is.Not.Null);
            Assert.That(captured!.Value.report.totalKills, Is.EqualTo(2));
            Assert.That(captured!.Value.report.goldGained, Is.EqualTo(42));
            Assert.That(captured!.Value.report.outcome, Is.EqualTo(RunOutcome.Lose));
            Assert.That(captured!.Value.report.deathCause, Is.EqualTo(RunEndCause.HpZero));
            Assert.That(captured!.Value.report.weaponIdsUsed,
                Is.EqualTo(new[] { WeaponSlugCarrot }));
        }

        [Test]
        public void End_SetsCurrentRunEndReport_OnRuntimeState()
        {
            // Before End() the live report is null.
            Assert.That(_controller!.CurrentRunEndReport, Is.Null,
                "CurrentRunEndReport must be null while the run is in progress.");

            _controller!.RecordKill();
            _controller!.End(RunResult.Death);

            Assert.That(_controller!.CurrentRunEndReport, Is.Not.Null,
                "End() must populate CurrentRunEndReport for UI consumption.");
            Assert.That(_controller!.CurrentRunEndReport!.totalKills, Is.EqualTo(1));
            Assert.That(_controller!.CurrentRunEndReport!.outcome, Is.EqualTo(RunOutcome.Lose));
        }

        [Test]
        public void End_AlsoFiresLegacyDeathChannel()
        {
            DeathEvent? death = null;
            void Handler(DeathEvent e) => death = e;
            _deathChannel!.Subscribe(Handler);
            try
            {
                _controller!.RecordKill();
                _controller!.End(RunResult.Victory);
            }
            finally { _deathChannel!.Unsubscribe(Handler); }

            Assert.That(death, Is.Not.Null, "Legacy DeathChannel must still fire (back-compat).");
            Assert.That(death!.Value.cause, Is.EqualTo(DeathCause.Victory));
            Assert.That(death!.Value.enemiesKilled, Is.EqualTo(1));
        }

        [Test]
        public void End_RaisesStateChanged_OnceAtEnd()
        {
            int changeCount = 0;
            ((Brave.UI.Bindings.IRunRuntimeState)_controller!).StateChanged += () => changeCount++;

            int before = changeCount;
            _controller!.End(RunResult.Quit);
            int after = changeCount;

            Assert.That(after - before, Is.GreaterThanOrEqualTo(1),
                "StateChanged must fire so the HUD redraws on run-end.");
        }

        // ---- Outcome / result mapping ----

        [Test]
        public void OutcomeFromResult_MapsAllVariants()
        {
            Assert.That(RunEndReport.OutcomeFromResult(RunResult.Victory), Is.EqualTo(RunOutcome.Win));
            Assert.That(RunEndReport.OutcomeFromResult(RunResult.Death),   Is.EqualTo(RunOutcome.Lose));
            Assert.That(RunEndReport.OutcomeFromResult(RunResult.Quit),    Is.EqualTo(RunOutcome.Quit));
        }

        [Test]
        public void ResultFromOutcome_MapsAllVariants()
        {
            Assert.That(RunEndReport.ResultFromOutcome(RunOutcome.Win),     Is.EqualTo(RunResult.Victory));
            Assert.That(RunEndReport.ResultFromOutcome(RunOutcome.Lose),    Is.EqualTo(RunResult.Death));
            Assert.That(RunEndReport.ResultFromOutcome(RunOutcome.Timeout), Is.EqualTo(RunResult.Death));
            Assert.That(RunEndReport.ResultFromOutcome(RunOutcome.Quit),    Is.EqualTo(RunResult.Quit));
        }
    }
}
