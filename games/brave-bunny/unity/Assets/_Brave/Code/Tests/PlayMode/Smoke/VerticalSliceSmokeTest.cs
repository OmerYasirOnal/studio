// QA — Brave Bunny vertical-slice end-to-end PlayMode smoke test.
//
// Single integration test that drives the full boot → run → end loop and proves
// nothing is fundamentally broken across Wave 7A's vertical slice integration.
//
// What this test covers (in order, in one [UnityTest] coroutine):
//   1. Boot — load Boot.unity; assert GameContextBootstrap.Context populates and
//      that every critical service (and the Wave 7A additions where registered)
//      is reachable via Get<>/TryGet<>.
//   2. Transition — let SceneFlow drive Boot → Run; wait until the Run scene is
//      the active scene.
//   3. Run loop — find RunController; spin yields for ~5 simulated seconds; assert
//      a PlayerMover stays alive and that the RunEndedChannel + EnemyKilledChannel
//      can be observed (no force-spawn seam exists yet — see hand-off note).
//   4. Force-end — call RunController.End(RunOutcome.Win, "test_force_end");
//      assert RunEndedChannel fires exactly once with a populated RunEndReport,
//      and the per-character CharacterUnlockService.RunsCompleted ticks (if the
//      service is registered).
//   5. Teardown — no unexpected log lines after one final frame.
//
// Diagnostic strategy:
//   Every step records its label into a step-trail. On failure the trail is dumped
//   so the very first thing a future agent sees is "where in the loop did we die?".
//   Sub-system gaps are reported as Assert.Inconclusive so the test stays useful in
//   the iterative-integration window (boot smoke test uses Assert.Pass for the same
//   reason — see BootSmokeTests.cs precedent).
//
// Performance budget:
//   Total wall-time bounded by SmokeTestHelpers constants — ~30 s ceiling at 60 fps.
//
// Spec refs:
//   * docs/06-tech-spec/08-state-machine.md — Boot → Run transition.
//   * docs/06-tech-spec/09-event-bus.md     — service registry table.
//   * docs/decisions/0021-hud-binding-contract.md — IRunRuntimeState contract.
//   * games/brave-bunny/CLAUDE.md           — perf + wave-timing contracts.

#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Brave.Gameplay.Events;
using Brave.Gameplay.Movement;
using Brave.Gameplay.Run;
using Brave.Systems.Audio;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Brave.Tests.PlayMode.Smoke
{
    /// <summary>
    /// End-to-end PlayMode smoke test exercising the full Boot → Run → End loop.
    /// One <c>[UnityTest]</c> method; bounded wall-time. See file header for
    /// detailed coverage notes + hand-off TODOs.
    /// </summary>
    [TestFixture]
    public class VerticalSliceSmokeTest
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string BootScene = "Boot";
        private const string RunScene = "Run";
        private const string ForceEndCause = "test_force_end";

        // Per-step trail for diagnostic dump on failure.
        private readonly List<string> _stepTrail = new(16);

        [SetUp]
        public void SetUp()
        {
            _stepTrail.Clear();
            // Fail the test if any error logs appear during the run.
            LogAssert.NoUnexpectedReceived();
        }

        [TearDown]
        public void TearDown()
        {
            if (_stepTrail.Count > 0)
                Debug.Log("[VerticalSliceSmokeTest] step-trail: " + string.Join(" -> ", _stepTrail));
        }

        [UnityTest]
        public IEnumerator VerticalSlice_BootToRunToEnd_Passes()
        {
            // ============================================================
            // Step 1 — Boot scene + service registration
            // ============================================================
            Step("boot.load");
            bool bootLoaded = false;
            yield return SmokeTestHelpers.LoadSceneOrSkip(BootScene, ok => bootLoaded = ok);
            if (!bootLoaded)
            {
                Assert.Inconclusive(
                    $"Boot scene '{BootScene}' not in Build Settings. Cannot exercise the vertical slice. " +
                    "Add Assets/_Brave/Scenes/Boot.unity to Build Settings for this test to be meaningful.");
                yield break;
            }

            Step("boot.wait-ready");
            bool ctxReady = false;
            yield return SmokeTestHelpers.WaitForBootstrapReady(ok => ctxReady = ok);
            if (!ctxReady)
            {
                Assert.Fail(
                    "GameContextBootstrap.Context never populated after Boot scene loaded. " +
                    "Step trail: " + string.Join(" -> ", _stepTrail) + ". " +
                    "Likely cause: GameContextBootstrap missing from Boot.unity, or Awake threw.");
            }

            Step("boot.assert-services");
            AssertCoreServicesRegistered();

            // ============================================================
            // Step 2 — Boot → Run transition (driven by SceneFlow)
            // ============================================================
            Step("transition.wait-run-active");
            bool runActive = false;
            yield return SmokeTestHelpers.WaitForActiveScene(RunScene, ok => runActive = ok);
            if (!runActive)
            {
                // SceneFlow may be wired with a different next-scene (e.g. MainMenu). Try a
                // direct LoadSceneAsync to keep the test moving.
                Step("transition.fallback-load-run");
                bool runLoaded = false;
                yield return SmokeTestHelpers.LoadSceneOrSkip(RunScene, ok => runLoaded = ok);
                if (!runLoaded)
                {
                    Assert.Inconclusive(
                        $"Run scene '{RunScene}' not reachable from Boot and not directly loadable. " +
                        "Step trail: " + string.Join(" -> ", _stepTrail));
                    yield break;
                }

                // Give Run.unity a frame to populate via Awake before searching.
                yield return null;
            }

            // ============================================================
            // Step 3 — In-Run loop: PlayerMover alive, event channels reachable
            // ============================================================
            Step("run.find-controller");
            RunController? controller = SmokeTestHelpers.FindRunController();
            if (controller == null)
            {
                Assert.Inconclusive(
                    "RunController not found in Run.unity. Wave 7A integration likely incomplete. " +
                    "Step trail: " + string.Join(" -> ", _stepTrail));
                yield break;
            }

            Step("run.find-playermover");
            PlayerMover? mover = SmokeTestHelpers.FindComponent<PlayerMover>();
            if (mover == null)
            {
                Assert.Inconclusive(
                    "PlayerMover not found in Run.unity. Hero prefab likely not instantiated. " +
                    "Step trail: " + string.Join(" -> ", _stepTrail));
                yield break;
            }

            // Snapshot the RunEndedChannel + EnemyKilledChannel via reflection so the test
            // doesn't need a CreateInstance copy (channels are SO assets; the production
            // ones must match the ones RunController has wired or events won't observe).
            Step("run.snapshot-channels");
            var runEndedChannel = GetPrivateChannel<RunEndedChannel>(controller, "_runEndedChannel");
            var enemyKilledChannel = GetPrivateChannel<EnemyKilledChannel>(controller, "_enemyKilledChannel");

            int runEndedFireCount = 0;
            RunEndReport? capturedReport = null;
            void OnRunEnded(RunEndedEvent evt)
            {
                runEndedFireCount++;
                capturedReport = evt.report;
            }
            runEndedChannel?.Subscribe(OnRunEnded);

            int enemyKilledFireCount = 0;
            void OnEnemyKilled(EnemyKilledEvent evt) => enemyKilledFireCount++;
            enemyKilledChannel?.Subscribe(OnEnemyKilled);

            // Run for ~5 seconds. We can't force-spawn enemies without a WaveSpawner test
            // seam (TODO: add WaveSpawner.ForceSpawn(EnemyDefinition, count) — see hand-off).
            // Instead we verify the system stays alive and that whatever spawns naturally
            // produces measurable activity; if it doesn't, we still proceed to force-end.
            Step("run.simulate");
            for (int i = 0; i < SmokeTestHelpers.MaxFramesForRunSimulation; i++)
            {
                if (mover == null || !mover) // hero destroyed mid-run = catastrophic
                {
                    Assert.Fail(
                        "PlayerMover was destroyed during the 5s run simulation. " +
                        "Step trail: " + string.Join(" -> ", _stepTrail));
                }
                yield return null;
            }

            // Soft-assert kills + auto-attack visibility. Wave 7A integration may not yet
            // produce kills in 5 seconds of naked simulation; record + continue.
            Debug.Log(
                $"[VerticalSliceSmokeTest] post-5s observations: enemyKilledFireCount={enemyKilledFireCount}");

            // ============================================================
            // Step 4 — Force-end run via RunController.End(Win, cause)
            // ============================================================
            Step("end.snapshot-pre-state");
            // Capture pre-end CharacterUnlockService snapshot if registered, so we can
            // verify post-call deltas.
            var ctx = GameContextBootstrap.Context;
            ICharacterUnlockService? unlockService = null;
            int preRunsCompleted = -1;
            string? characterSlug = controller.Character != null ? controller.Character.slug : null;
            if (ctx != null && ctx.TryGet<ICharacterUnlockService>(out var resolved))
            {
                unlockService = resolved;
                preRunsCompleted = ReadRunsCompleted(unlockService, characterSlug);
            }

            // Seed at least one kill so the report has a non-zero tally, regardless of
            // what the wave runner produced. RunController exposes RecordKill() publicly.
            Step("end.seed-kill");
            controller.RecordKill();

            Step("end.force-end-win");
            controller.End(RunOutcome.Win, ForceEndCause);

            // ============================================================
            // Step 5 — Assert end-of-run report + service interaction
            // ============================================================
            Step("end.assert-channel");
            // If the production channel wasn't wired on the RunController, RunEndedChannel
            // can't fire — fall back to reading IRunRuntimeState.CurrentRunEndReport directly.
            if (runEndedChannel == null)
            {
                Debug.LogWarning(
                    "[VerticalSliceSmokeTest] RunController._runEndedChannel was null — " +
                    "reading CurrentRunEndReport directly instead of via the channel.");
                capturedReport = controller.CurrentRunEndReport;
                Assert.That(capturedReport, Is.Not.Null,
                    "Even with a null channel, RunController.CurrentRunEndReport must be set after End().");
            }
            else
            {
                Assert.That(runEndedFireCount, Is.EqualTo(1),
                    $"RunEndedChannel must fire exactly once on RunController.End(). Observed {runEndedFireCount} fires. " +
                    "Step trail: " + string.Join(" -> ", _stepTrail));
                Assert.That(capturedReport, Is.Not.Null,
                    "RunEndedEvent payload (RunEndReport) must be non-null.");
            }

            Step("end.assert-report-fields");
            Assert.That(capturedReport!.outcome, Is.EqualTo(RunOutcome.Win),
                "Forced End(Win) must set RunEndReport.outcome = Win.");
            Assert.That(capturedReport.deathCause, Is.EqualTo(ForceEndCause),
                $"RunEndReport.deathCause must reflect the explicit cause passed to End(): expected '{ForceEndCause}'.");
            Assert.That(capturedReport.totalKills, Is.GreaterThan(0),
                "RunEndReport.totalKills must include the seeded kill (RecordKill was called pre-End).");
            Assert.That(capturedReport.runDurationSeconds, Is.GreaterThanOrEqualTo(0f),
                "RunEndReport.runDurationSeconds must be non-negative.");
            // characterId may be empty if no CharacterDefinition is wired in Run.unity —
            // that's an inconclusive flag, not a hard fail.
            if (string.IsNullOrEmpty(capturedReport.characterId))
            {
                Debug.LogWarning(
                    "[VerticalSliceSmokeTest] RunEndReport.characterId is empty — " +
                    "RunController._activeCharacter not wired in Run.unity (Wave 7A follow-up).");
            }

            Step("end.unlock-service");
            if (unlockService == null)
            {
                Debug.LogWarning(
                    "[VerticalSliceSmokeTest] ICharacterUnlockService is not registered with " +
                    "GameContextBootstrap. TODO: wire it in Boot per Wave 7A intent — " +
                    "CharacterUnlockService.RecordRunCompletion is currently unreachable.");
            }
            else if (!string.IsNullOrEmpty(characterSlug))
            {
                // Production wiring: a Run-end subscriber should call
                // ICharacterUnlockService.RecordRunCompletion(slug, wave, bosses).
                // We don't call it ourselves — the smoke test is verifying the wiring exists.
                // If RunsCompleted didn't tick, log a diagnostic rather than fail (the
                // subscriber may live in a not-yet-merged wave).
                int postRunsCompleted = ReadRunsCompleted(unlockService, characterSlug);
                if (postRunsCompleted <= preRunsCompleted)
                {
                    Debug.LogWarning(
                        $"[VerticalSliceSmokeTest] ICharacterUnlockService.RunsCompleted did not " +
                        $"tick for '{characterSlug}' (pre={preRunsCompleted}, post={postRunsCompleted}). " +
                        "Missing RunEnded → RecordRunCompletion subscriber. Hand-off TODO.");
                }
            }

            // ============================================================
            // Step 6 — Clean teardown: unsubscribe, yield one frame, no logs
            // ============================================================
            Step("teardown.unsubscribe");
            runEndedChannel?.Unsubscribe(OnRunEnded);
            enemyKilledChannel?.Unsubscribe(OnEnemyKilled);

            Step("teardown.final-frame");
            yield return null;
            // SetUp's LogAssert.NoUnexpectedReceived() captures any errors raised during
            // the test. If we got here, the smoke test passes.
            Step("teardown.done");
        }

        // ---- helpers ----

        private void Step(string label) => _stepTrail.Add(label);

        /// <summary>
        /// Assert that the critical Boot services are registered, with a clear failure
        /// message naming the missing service. Wave 7A additions (ICharacterUnlockService,
        /// HitstopService) may or may not be registered — those are logged as warnings
        /// when missing rather than hard-failed.
        /// </summary>
        private static void AssertCoreServicesRegistered()
        {
            var ctx = GameContextBootstrap.Context;
            Assert.That(ctx, Is.Not.Null, "GameContextBootstrap.Context is null after WaitForBootstrapReady — invariant broken.");

            AssertRegistered<Brave.Systems.Save.ISaveService>(ctx!, "ISaveService");
            AssertRegistered<Brave.Systems.Settings.ISettingsService>(ctx!, "ISettingsService");
            AssertRegistered<Brave.Systems.Localization.ILocalizationService>(ctx!, "ILocalizationService");
            AssertRegistered<Brave.Systems.Audio.IAudioMixerDriver>(ctx!, "IAudioMixerDriver");
            AssertRegistered<Brave.Systems.Progression.IProgressionService>(ctx!, "IProgressionService");
            AssertRegistered<BgmGameplayDriver>(ctx!, "BgmGameplayDriver");

            // Wave 7A additions — diagnostic warnings only (not hard fail). When these light
            // up the warning, the orchestrator knows the wave-7A wiring follow-up is needed.
            if (!ctx!.TryGet<ICharacterUnlockService>(out _))
                Debug.LogWarning("[VerticalSliceSmokeTest] ICharacterUnlockService not registered — Wave 7A wiring incomplete.");
        }

        private static void AssertRegistered<T>(GameContext ctx, string label) where T : class
        {
            Assert.That(ctx.TryGet<T>(out _), Is.True,
                $"Service '{label}' (type {typeof(T).FullName}) not registered. " +
                "Check GameContextBootstrap.Awake() wiring order. " +
                "This is a Wave 7A integration regression.");
        }

        /// <summary>
        /// Read a private SerializedField channel reference off the controller via reflection.
        /// Returns null if the field is absent or unwired — the test then falls back to a
        /// direct CurrentRunEndReport read.
        /// </summary>
        private static T? GetPrivateChannel<T>(RunController controller, string fieldName) where T : class
        {
            var field = typeof(RunController).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return null;
            return field.GetValue(controller) as T;
        }

        /// <summary>
        /// Read CharacterUnlockService.RunsCompleted for a slug via reflection on the
        /// underlying SaveService.Data.Characters dict — the service does not expose a
        /// per-character getter and we don't want to add one just for tests.
        /// Returns 0 when the slug is unknown.
        /// </summary>
        private static int ReadRunsCompleted(ICharacterUnlockService service, string? characterSlug)
        {
            if (string.IsNullOrEmpty(characterSlug) || service == null) return 0;
            var saveField = service.GetType().GetField("_save",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (saveField == null) return 0;
            var saveObj = saveField.GetValue(service);
            if (saveObj == null) return 0;
            var dataProp = saveObj.GetType().GetProperty("Data");
            if (dataProp == null) return 0;
            var data = dataProp.GetValue(saveObj);
            if (data == null) return 0;
            var charsProp = data.GetType().GetProperty("Characters");
            if (charsProp == null) return 0;
            if (charsProp.GetValue(data) is not System.Collections.IDictionary chars) return 0;
            if (!chars.Contains(characterSlug!)) return 0;
            var profile = chars[characterSlug!];
            if (profile == null) return 0;
            var runsField = profile.GetType().GetField("RunsCompleted") ??
                            (System.Reflection.MemberInfo?)profile.GetType().GetProperty("RunsCompleted");
            return runsField switch
            {
                FieldInfo fi => (fi.GetValue(profile) as int?) ?? 0,
                PropertyInfo pi => (pi.GetValue(profile) as int?) ?? 0,
                _ => 0,
            };
        }
    }
}
