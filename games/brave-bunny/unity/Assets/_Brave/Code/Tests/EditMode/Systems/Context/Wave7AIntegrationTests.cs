// QA — Wave 7A integration EditMode tests.
//
// Scope:
//   * GameContextBootstrap registers the 6 Wave 7A services after Awake:
//       - CharacterUnlockService / ICharacterUnlockService
//       - HitstopService
//       - DamageNumberSpawner + DamageNumberPool
//       - ScreenShakeController
//       - BgmGameplayDriver (regression — Wave 7A audio agent already wired it)
//   * RunController.End() raises RunEndedChannel AND publishes the report on the
//     RunEndIntegrationBridge static delegate (which Boot subscribes to).
//   * GameContextBootstrap.DispatchRunEndToMetaServices invokes BgmGameplayDriver
//     .EnterRunEnd with the correct win flag and CharacterUnlockService
//     .RecordRunCompletion with the correct slug / wave / boss-kill count.
//
// Test strategy:
//   * For service-registration: instantiate the GameContextBootstrap MonoBehaviour
//     on a throwaway GameObject. The bootstrap's Awake() builds a real service graph
//     including SaveService (Application.persistentDataPath — safe in EditMode).
//     FeelConfig + DamageNumberWidget prefab refs are injected via reflection on
//     the [SerializeField] backing fields, mirroring the RunEndReportTests pattern.
//   * For RunController.End → bridge: reuse the RunEndReportTests SetUp pattern
//     (RunController on a fresh GO, channels via ScriptableObject.CreateInstance).
//     Hook a static subscriber on RunEndIntegrationBridge.Fired before calling End()
//     and assert the captured report matches.
//   * For DispatchRunEndToMetaServices: call the static pure helper directly with
//     a FakeMusicStateMachine-backed BgmGameplayDriver and an InMemoryFileSystem-
//     backed CharacterUnlockService. No MonoBehaviour or scene needed.
//
// Spec refs:
//   * docs/06-tech-spec/09-event-bus.md § service registry.
//   * docs/02-gdd/02-meta-loop.md § Character unlock ladder.
//   * docs/08-audio-bible/01-bgm-spec.md § Run-end snapshot transitions.

#nullable enable

using System.Collections.Generic;
using System.Reflection;
using Brave.Gameplay.Events;
using Brave.Gameplay.Feel;
using Brave.Gameplay.Run;
using Brave.Systems.Audio;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Brave.Tests.EditMode.Systems.Context
{
    [TestFixture]
    public class Wave7AIntegrationTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string TestCharacterSlug = "bunny";
        private const int TestWavesCleared = 12;
        private const int TestBossesKilled = 1;
        private const int TestKills = 50;
        private const string SentinelBossSlug = "boss";

        [SetUp]
        public void SetUp()
        {
            // Static bridge subscriber lists persist across tests if not cleared.
            // Reset both bridges so each test starts from a known clean slate.
            RunEndIntegrationBridge.ResetForTests();
            Brave.Gameplay.Combat.WeaponFireBridge.ResetForTests();
        }

        // =========================================================================
        // (A) GameContextBootstrap registers all Wave 7A services
        // =========================================================================

        [Test]
        public void Bootstrap_RegistersAllWave7AServices()
        {
            // arrange — build a FeelConfig + damage-number widget prefab so the
            // bootstrap's null-guard doesn't skip the juice-services block.
            var feelConfig = ScriptableObject.CreateInstance<FeelConfig>();
            var widgetGo = new GameObject("TestDmgWidget");
            var widget = widgetGo.AddComponent<DamageNumberWidget>();

            // Build the host GO with the component DISABLED so AddComponent doesn't fire
            // Awake before we've populated the SerializeFields. Then enable + manually
            // invoke Awake via reflection so the lifecycle runs exactly once.
            var go = new GameObject("Test_Bootstrap");
            go.SetActive(false);
            var bootstrap = go.AddComponent<GameContextBootstrap>();
            SetPrivateField(bootstrap, "_feelConfig", feelConfig);
            SetPrivateField(bootstrap, "_damageNumberWidgetPrefab", widget);

            // Reset the static Context so we observe THIS bootstrap's registration set,
            // not residue from a prior test that may have invoked Awake earlier.
            ResetStaticContext();

            try
            {
                // act — invoke Awake() directly; the inactive GO ensures the engine
                // hasn't already fired it.
                var awake = typeof(GameContextBootstrap).GetMethod("Awake",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(awake, Is.Not.Null, "GameContextBootstrap.Awake() not found via reflection.");
                awake!.Invoke(bootstrap, null);

                var ctx = GameContextBootstrap.Context;
                Assert.That(ctx, Is.Not.Null, "GameContextBootstrap.Awake did not populate Context.");

                // assert — every Wave 7A service is resolvable.
                Assert.That(ctx.TryGet<ICharacterUnlockService>(out _), Is.True,
                    "ICharacterUnlockService not registered.");
                Assert.That(ctx.TryGet<CharacterUnlockService>(out _), Is.True,
                    "CharacterUnlockService (concrete) not registered.");
                Assert.That(ctx.TryGet<HitstopService>(out _), Is.True,
                    "HitstopService not registered.");
                Assert.That(ctx.TryGet<DamageNumberSpawner>(out _), Is.True,
                    "DamageNumberSpawner not registered.");
                Assert.That(ctx.TryGet<DamageNumberPool>(out _), Is.True,
                    "DamageNumberPool not registered.");
                Assert.That(ctx.TryGet<ScreenShakeController>(out _), Is.True,
                    "ScreenShakeController not registered.");
                Assert.That(ctx.TryGet<BgmGameplayDriver>(out _), Is.True,
                    "BgmGameplayDriver not registered (regression — was wired in Wave 7A audio agent).");
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(widgetGo);
                Object.DestroyImmediate(feelConfig);
            }
        }

        // =========================================================================
        // (B) RunController.End publishes on RunEndIntegrationBridge
        // =========================================================================

        [Test]
        public void RunControllerEnd_PublishesReportOnIntegrationBridge()
        {
            // arrange
            var go = new GameObject("Test_RunControllerEnd");
            var controller = go.AddComponent<RunController>();
            var runEndedChannel = ScriptableObject.CreateInstance<RunEndedChannel>();
            var deathChannel = ScriptableObject.CreateInstance<DeathChannel>();
            SetPrivateField(controller, "_runEndedChannel", runEndedChannel);
            SetPrivateField(controller, "_deathChannel", deathChannel);

            RunEndIntegrationBridge.ResetForTests();
            RunEndReport? captured = null;
            int captureCount = 0;
            System.Action<RunEndReport> handler = r => { captured = r; captureCount++; };
            RunEndIntegrationBridge.RunEnded += handler;

            try
            {
                // act — drive the End(outcome, cause) overload so we control deathCause exactly.
                controller.SetWave(TestWavesCleared);
                for (int i = 0; i < TestKills; i++) controller.RecordKill();
                controller.RecordBossKill();
                controller.End(RunOutcome.Win, RunEndCause.BossDefeated);

                // assert
                Assert.That(captureCount, Is.EqualTo(1),
                    "RunEndIntegrationBridge.Fired must fire exactly once per End() call.");
                Assert.That(captured, Is.Not.Null);
                Assert.That(captured!.outcome, Is.EqualTo(RunOutcome.Win));
                Assert.That(captured.deathCause, Is.EqualTo(RunEndCause.BossDefeated));
                Assert.That(captured.wavesCleared, Is.EqualTo(TestWavesCleared));
                Assert.That(captured.bossesKilled, Is.EqualTo(TestBossesKilled));
            }
            finally
            {
                RunEndIntegrationBridge.RunEnded -= handler;
                RunEndIntegrationBridge.ResetForTests();
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(runEndedChannel);
                Object.DestroyImmediate(deathChannel);
            }
        }

        // =========================================================================
        // (C) DispatchRunEndToMetaServices forwards to BGM + CharacterUnlockService
        // =========================================================================

        [Test]
        public void DispatchRunEndToMetaServices_WinReport_CallsEnterRunEndTrue_AndRecordsRunCompletion()
        {
            // arrange — fake music + real unlock service backed by in-memory save.
            var music = new FakeMusicStateMachine();
            var bgm = new BgmGameplayDriver(music);

            var unlocks = new FakeCharacterUnlockService();

            var report = new RunEndReport
            {
                outcome = RunOutcome.Win,
                deathCause = RunEndCause.BossDefeated,
                characterId = TestCharacterSlug,
                wavesCleared = TestWavesCleared,
                bossesKilled = TestBossesKilled,
                totalKills = TestKills,
            };

            // act
            GameContextBootstrap.DispatchRunEndToMetaServices(report, bgm, unlocks);

            // assert — BGM was told to enter run-end-win.
            Assert.That(music.Calls, Does.Contain(nameof(IMusicStateMachine.EnterRunEnd)));
            Assert.That(music.LastRunEndWin, Is.True,
                "EnterRunEnd was called but with win=false; expected win=true for RunOutcome.Win.");

            // assert — unlock service recorded the run + boss defeat.
            Assert.That(unlocks.RunCompletionCalls.Count, Is.EqualTo(1));
            var call = unlocks.RunCompletionCalls[0];
            Assert.That(call.slug, Is.EqualTo(TestCharacterSlug));
            Assert.That(call.wavesReached, Is.EqualTo(TestWavesCleared));
            Assert.That(call.bossesDefeatedThisRun, Is.EqualTo(TestBossesKilled));

            Assert.That(unlocks.BossDefeatCalls.Count, Is.EqualTo(1),
                "deathCause=boss_defeated + bossesKilled>0 must trigger RecordBossDefeated once.");
            Assert.That(unlocks.BossDefeatCalls[0].bossSlug, Is.EqualTo(SentinelBossSlug));
            Assert.That(unlocks.BossDefeatCalls[0].characterSlug, Is.EqualTo(TestCharacterSlug));
        }

        [Test]
        public void DispatchRunEndToMetaServices_LoseReport_CallsEnterRunEndFalse_AndSkipsBossDefeat()
        {
            // arrange
            var music = new FakeMusicStateMachine();
            var bgm = new BgmGameplayDriver(music);
            var unlocks = new FakeCharacterUnlockService();

            var report = new RunEndReport
            {
                outcome = RunOutcome.Lose,
                deathCause = RunEndCause.HpZero,
                characterId = TestCharacterSlug,
                wavesCleared = 4,
                bossesKilled = 0,
            };

            // act
            GameContextBootstrap.DispatchRunEndToMetaServices(report, bgm, unlocks);

            // assert
            Assert.That(music.LastRunEndWin, Is.False,
                "RunOutcome.Lose must map to EnterRunEnd(win:false).");
            Assert.That(unlocks.RunCompletionCalls.Count, Is.EqualTo(1));
            Assert.That(unlocks.BossDefeatCalls.Count, Is.EqualTo(0),
                "Lose + no bosses must not call RecordBossDefeated.");
        }

        [Test]
        public void DispatchRunEndToMetaServices_NullReport_NoOps()
        {
            var music = new FakeMusicStateMachine();
            var bgm = new BgmGameplayDriver(music);
            var unlocks = new FakeCharacterUnlockService();

            Assert.DoesNotThrow(() =>
                GameContextBootstrap.DispatchRunEndToMetaServices(null!, bgm, unlocks));
            Assert.That(music.Calls, Is.Empty);
            Assert.That(unlocks.RunCompletionCalls, Is.Empty);
        }

        [Test]
        public void DispatchRunEndToMetaServices_EmptyCharacterId_RecordsBgmButSkipsUnlocks()
        {
            var music = new FakeMusicStateMachine();
            var bgm = new BgmGameplayDriver(music);
            var unlocks = new FakeCharacterUnlockService();

            var report = new RunEndReport
            {
                outcome = RunOutcome.Win,
                deathCause = RunEndCause.BossDefeated,
                characterId = string.Empty,    // naked-run / pre-loadout
                bossesKilled = 1,
            };

            GameContextBootstrap.DispatchRunEndToMetaServices(report, bgm, unlocks);

            Assert.That(music.LastRunEndWin, Is.True, "BGM must still transition even when slug is empty.");
            Assert.That(unlocks.RunCompletionCalls, Is.Empty,
                "Empty character slug must not call RecordRunCompletion.");
            Assert.That(unlocks.BossDefeatCalls, Is.Empty,
                "Empty character slug must not call RecordBossDefeated.");
        }

        // =========================================================================
        // helpers
        // =========================================================================

        private static void SetPrivateField(object target, string fieldName, object? value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, $"{target.GetType().Name} has no field '{fieldName}'");
            field!.SetValue(target, value);
        }

        /// <summary>
        /// Reset the static GameContextBootstrap.Context so each test observes a fresh
        /// service-graph instance. The auto-property has a private setter, so we walk
        /// down to the compiler-generated backing field directly. Safe to call when no
        /// prior bootstrap ran.
        /// </summary>
        private static void ResetStaticContext()
        {
            // C# auto-property backing field is named "&lt;Context&gt;k__BackingField".
            const string backingFieldName = "<Context>k__BackingField";
            var field = typeof(GameContextBootstrap).GetField(backingFieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, null);
        }

        // ---- fakes ----

        private sealed class FakeMusicStateMachine : IMusicStateMachine
        {
            public readonly List<string> Calls = new();
            public string? LastBiome;
            public bool? LastRunEndWin;

            public void EnterHome() => Calls.Add(nameof(EnterHome));
            public void EnterLobby() => Calls.Add(nameof(EnterLobby));
            public void EnterRun(string biomeSlug) { Calls.Add(nameof(EnterRun)); LastBiome = biomeSlug; }
            public void EnterBoss() => Calls.Add(nameof(EnterBoss));
            public void EnterRunEnd(bool win) { Calls.Add(nameof(EnterRunEnd)); LastRunEndWin = win; }
        }

        private sealed class FakeCharacterUnlockService : ICharacterUnlockService
        {
            public readonly List<(string slug, int wavesReached, int bossesDefeatedThisRun)> RunCompletionCalls = new();
            public readonly List<(string bossSlug, string characterSlug)> BossDefeatCalls = new();

            // Field-event auto-implements add/remove; we never raise it in this fake.
            public event System.Action<string>? CharacterUnlocked;

            public bool IsUnlocked(string slug) => false;
            public IReadOnlyList<string> GetUnlockedCharacterIds() => System.Array.Empty<string>();
            public IReadOnlyList<string> EvaluateAll() => System.Array.Empty<string>();

            public void RecordRunCompletion(string characterSlug, int waveReached, int bossesDefeatedThisRun)
            {
                RunCompletionCalls.Add((characterSlug, waveReached, bossesDefeatedThisRun));
                // Silence "unused" warning on the auto-event in case the analyzer flags it.
                _ = CharacterUnlocked;
            }

            public void RecordBossDefeated(string bossSlug, string characterSlug)
                => BossDefeatCalls.Add((bossSlug, characterSlug));

            public bool TryPurchase(string slug, CurrencyWallet wallet) => false;
        }
    }
}
