// QA — LifetimeStatsService EditMode tests (Wave 10).
//
// Subject under test:
//   * Brave.Systems.Stats.LifetimeStatsLogic — the pure-C# tally functions.
//     We assert each event mutation independently against a fresh StatsSection.
//   * Brave.Systems.Stats.LifetimeStatsService — the MonoBehaviour façade.
//     Driven via the public test seams (ConfigureForTests + HandleXxxTick) so
//     we never need to spin up a Unity scene.
//
// Pattern: matches TelemetryEventBridgeTests + TutorialControllerTests —
// in-memory SaveService (InMemoryFileSystem) verifies the field round-trips
// through the JSON wire format per ADR-0008 forward-compat.

#nullable enable

using Brave.Gameplay.Run;
using Brave.Systems.Save;
using Brave.Systems.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Stats
{
    [TestFixture]
    public class LifetimeStatsServiceTests
    {
        // ---- constants (CLAUDE.md principle 6 — no magic numbers) ----
        private const string SaveRootDir = "/tmp/brave-lifetime-stats-tests";
        private const int DefaultKills = 73;
        private const int DefaultWaves = 12;
        private const float DefaultRunSeconds = 245.5f;
        private const double FakeStart = 100.0;
        private const double FakeEnd   = 130.0;
        private const double DeltaPlaytimeSeconds = FakeEnd - FakeStart;

        // ---- helpers ----

        private static (SaveService save, LifetimeStatsService svc) MakeService()
        {
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load();
            var svc = new GameObject("lifetime-stats").AddComponent<LifetimeStatsService>();
            svc.ConfigureForTests(save);
            // No channels wired — tests exercise the public test seams directly.
            return (save, svc);
        }

        private static RunEndReport MakeWinReport(int kills, int waves, float runSeconds) => new RunEndReport
        {
            outcome             = RunOutcome.Win,
            result              = RunResult.Victory,
            runDurationSeconds  = runSeconds,
            totalKills          = kills,
            wavesCleared        = waves,
            characterId         = "bunny",
            deathCause          = RunEndCause.BossDefeated,
        };

        private static RunEndReport MakeLoseReport(int kills, int waves, float runSeconds) => new RunEndReport
        {
            outcome             = RunOutcome.Lose,
            result              = RunResult.Death,
            runDurationSeconds  = runSeconds,
            totalKills          = kills,
            wavesCleared        = waves,
            characterId         = "bunny",
            deathCause          = RunEndCause.HpZero,
        };

        // ---- pure logic: RunEnded tally ----

        [Test]
        public void ApplyRunEnded_NullReport_StillTicksRunCounter()
        {
            var stats = new SaveData.StatsSection();
            var changed = LifetimeStatsLogic.ApplyRunEnded(stats, null, playtimeDeltaSeconds: 0);
            Assert.That(changed, Is.True);
            Assert.That(stats.TotalRuns, Is.EqualTo(1),
                "Even a null report counts as a completed run for tally purposes.");
        }

        [Test]
        public void ApplyRunEnded_AddsKillsAndWaves()
        {
            var stats = new SaveData.StatsSection();
            LifetimeStatsLogic.ApplyRunEnded(stats, MakeWinReport(DefaultKills, DefaultWaves, DefaultRunSeconds), 0);
            Assert.That(stats.TotalKills, Is.EqualTo(DefaultKills));
            Assert.That(stats.BestWaveReached, Is.EqualTo(DefaultWaves));
            Assert.That(stats.BestRunTimeSeconds, Is.EqualTo(DefaultRunSeconds).Within(0.001f));
            Assert.That(stats.TotalRuns, Is.EqualTo(1));
        }

        [Test]
        public void ApplyRunEnded_LossDoesNotUpdateBestTime()
        {
            // Best-run-time only counts Win outcomes per design — losses dominate
            // distribution and would otherwise pin it to ~0 quickly.
            var stats = new SaveData.StatsSection();
            LifetimeStatsLogic.ApplyRunEnded(stats, MakeLoseReport(10, 3, DefaultRunSeconds), 0);
            Assert.That(stats.BestRunTimeSeconds, Is.EqualTo(0f),
                "Loss outcomes must not update BestRunTimeSeconds.");
            Assert.That(stats.BestWaveReached, Is.EqualTo(3),
                "BestWaveReached still updates on loss — wave reached is wave reached.");
        }

        [Test]
        public void ApplyRunEnded_BestWaveOnlyClimbs()
        {
            var stats = new SaveData.StatsSection { BestWaveReached = 20 };
            LifetimeStatsLogic.ApplyRunEnded(stats, MakeWinReport(5, 12, 100f), 0);
            Assert.That(stats.BestWaveReached, Is.EqualTo(20),
                "BestWaveReached must never regress.");
        }

        [Test]
        public void ApplyRunEnded_AccumulatesPlaytimeDelta()
        {
            var stats = new SaveData.StatsSection();
            LifetimeStatsLogic.ApplyRunEnded(stats, MakeWinReport(0, 0, 0f), DeltaPlaytimeSeconds);
            Assert.That(stats.TotalPlaytimeSeconds, Is.EqualTo(DeltaPlaytimeSeconds).Within(0.001));
            LifetimeStatsLogic.ApplyRunEnded(stats, MakeWinReport(0, 0, 0f), DeltaPlaytimeSeconds);
            Assert.That(stats.TotalPlaytimeSeconds, Is.EqualTo(DeltaPlaytimeSeconds * 2).Within(0.001));
        }

        // ---- pure logic: boss / evolution tallies ----

        [Test]
        public void ApplyBossDefeated_BumpsCounter()
        {
            var stats = new SaveData.StatsSection { BossesDefeated = 4 };
            LifetimeStatsLogic.ApplyBossDefeated(stats);
            Assert.That(stats.BossesDefeated, Is.EqualTo(5));
        }

        [Test]
        public void ApplyWeaponEvolved_BumpsCounter()
        {
            var stats = new SaveData.StatsSection { EvolutionsTriggered = 11 };
            LifetimeStatsLogic.ApplyWeaponEvolved(stats);
            Assert.That(stats.EvolutionsTriggered, Is.EqualTo(12));
        }

        // ---- service end-to-end (handlers persist via SaveService) ----

        [Test]
        public void Service_HandleBossDefeatedTick_PersistsToSave()
        {
            var (save, svc) = MakeService();
            svc.HandleBossDefeatedTick();
            Assert.That(save.Data.Stats.BossesDefeated, Is.EqualTo(1));
            Assert.That(save.Data.LastSavedAt, Is.Not.Null.And.Not.Empty,
                "HandleBossDefeatedTick must persist via SaveService.Save().");
            Object.DestroyImmediate(svc.gameObject);
        }

        [Test]
        public void Service_HandleWeaponEvolvedTick_PersistsToSave()
        {
            var (save, svc) = MakeService();
            svc.HandleWeaponEvolvedTick();
            svc.HandleWeaponEvolvedTick();
            Assert.That(save.Data.Stats.EvolutionsTriggered, Is.EqualTo(2));
            Object.DestroyImmediate(svc.gameObject);
        }

        [Test]
        public void Service_HandleRunEndedReport_FoldsKillsWavesPlaytime()
        {
            var (save, svc) = MakeService();
            // Swap the clock so the delta is deterministic — without the swap,
            // Time.realtimeSinceStartup ticks unpredictably during tests.
            LifetimeStatsService.NowSeconds = () => FakeStart;
            svc.NotifyRunStarted();
            LifetimeStatsService.NowSeconds = () => FakeEnd;

            svc.HandleRunEndedReport(MakeWinReport(DefaultKills, DefaultWaves, DefaultRunSeconds));

            Assert.That(save.Data.Stats.TotalRuns, Is.EqualTo(1));
            Assert.That(save.Data.Stats.TotalKills, Is.EqualTo(DefaultKills));
            Assert.That(save.Data.Stats.BestWaveReached, Is.EqualTo(DefaultWaves));
            Assert.That(save.Data.Stats.BestRunTimeSeconds, Is.EqualTo(DefaultRunSeconds).Within(0.001f));
            Assert.That(save.Data.Stats.TotalPlaytimeSeconds, Is.EqualTo(DeltaPlaytimeSeconds).Within(0.001));

            // Reset clock to avoid cross-test contamination.
            LifetimeStatsService.NowSeconds = () => 0;
            Object.DestroyImmediate(svc.gameObject);
        }

        [Test]
        public void Service_HandleRunEndedReport_RoundTripsThroughSaveService()
        {
            // ADR-0008 forward-compat: the new totalPlaytimeSeconds + bestWaveReached
            // fields must round-trip through the JSON wire format.
            var fs = new InMemoryFileSystem();
            var save = new SaveService(SaveRootDir, fs);
            save.Load();
            var svc = new GameObject("lifetime-stats-rt").AddComponent<LifetimeStatsService>();
            svc.ConfigureForTests(save);

            LifetimeStatsService.NowSeconds = () => FakeStart;
            svc.NotifyRunStarted();
            LifetimeStatsService.NowSeconds = () => FakeEnd;
            svc.HandleRunEndedReport(MakeWinReport(DefaultKills, DefaultWaves, DefaultRunSeconds));
            svc.HandleBossDefeatedTick();
            svc.HandleWeaponEvolvedTick();

            // Simulate process restart against the same fs.
            var save2 = new SaveService(SaveRootDir, fs);
            save2.Load();

            Assert.That(save2.Data.Stats.TotalRuns, Is.EqualTo(1));
            Assert.That(save2.Data.Stats.TotalKills, Is.EqualTo(DefaultKills));
            Assert.That(save2.Data.Stats.BestWaveReached, Is.EqualTo(DefaultWaves));
            Assert.That(save2.Data.Stats.BossesDefeated, Is.EqualTo(1));
            Assert.That(save2.Data.Stats.EvolutionsTriggered, Is.EqualTo(1));
            Assert.That(save2.Data.Stats.TotalPlaytimeSeconds, Is.EqualTo(DeltaPlaytimeSeconds).Within(0.001),
                "totalPlaytimeSeconds must survive the JSON round-trip.");

            LifetimeStatsService.NowSeconds = () => 0;
            Object.DestroyImmediate(svc.gameObject);
        }

        [Test]
        public void Service_NullSaveOnHandlers_DoesNotThrow()
        {
            // No SaveService injected → handlers are a no-op (defensive).
            var svc = new GameObject("lifetime-stats-null").AddComponent<LifetimeStatsService>();
            Assert.DoesNotThrow(() => svc.HandleBossDefeatedTick());
            Assert.DoesNotThrow(() => svc.HandleWeaponEvolvedTick());
            Assert.DoesNotThrow(() => svc.HandleRunEndedReport(MakeWinReport(1, 1, 1f)));
            Object.DestroyImmediate(svc.gameObject);
        }
    }
}
