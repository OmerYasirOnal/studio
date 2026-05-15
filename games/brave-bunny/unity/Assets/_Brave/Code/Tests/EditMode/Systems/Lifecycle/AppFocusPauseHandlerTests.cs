// QA — AppFocusPauseLogic EditMode tests (Wave 10 QoL).
// Subject under test:
//   * Brave.Systems.Lifecycle.AppFocusPauseLogic — pure-C# gating logic for
//     "should app-focus-loss pause the run?". Verifies:
//       - focus-loss while in the Run scene raises the pause intent
//       - focus-loss in MainMenu / Loadout / Home is a no-op
//       - focus-gained is always a no-op (Resume is player-driven)
//
// Pattern: matches PauseControllerTests — exercise the logic class against
// fakes, no MonoBehaviour required.

#nullable enable

using Brave.Systems.Lifecycle;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.LifecycleTests
{
    [TestFixture]
    public class AppFocusPauseHandlerTests
    {
        // ---- constants (no magic strings — CLAUDE.md principle 6) ----
        private const string RunScene = "Run";
        private const string MainMenuScene = "MainMenu";
        private const string LoadoutScene = "Loadout";
        private const string HomeScene = "Home";

        // ---- test doubles ----

        private sealed class FakeSceneProbe : IActiveSceneProbe
        {
            public string ActiveScene = RunScene;
            public string GetActiveSceneName() => ActiveScene;
        }

        // ---- helpers ----

        private static (AppFocusPauseLogic logic, FakeSceneProbe probe, IntCounter pauseCalls)
            MakeLogic(string activeScene = RunScene)
        {
            var probe = new FakeSceneProbe { ActiveScene = activeScene };
            var counter = new IntCounter();
            var logic = new AppFocusPauseLogic(probe, counter.Bump);
            return (logic, probe, counter);
        }

        private sealed class IntCounter
        {
            public int Value;
            public void Bump() => Value++;
        }

        // ---- focus-loss during Run pauses ----

        [Test]
        public void FocusLoss_DuringRunScene_RaisesPauseIntent()
        {
            var (logic, _, counter) = MakeLogic(RunScene);
            logic.HandleFocusChanged(hasFocus: false);

            Assert.That(counter.Value, Is.EqualTo(1),
                "Focus loss inside the Run scene must raise PauseRunRequested exactly once.");
            Assert.That(logic.LastFocusLossPaused, Is.True);
        }

        // ---- focus-loss in menus does NOT pause ----

        [Test]
        public void FocusLoss_InMainMenu_DoesNotRaisePauseIntent()
        {
            var (logic, _, counter) = MakeLogic(MainMenuScene);
            logic.HandleFocusChanged(hasFocus: false);

            Assert.That(counter.Value, Is.EqualTo(0),
                "Focus loss in MainMenu must NOT raise PauseRunRequested.");
            Assert.That(logic.LastFocusLossPaused, Is.False);
        }

        [Test]
        public void FocusLoss_InLoadoutScene_DoesNotRaisePauseIntent()
        {
            var (logic, _, counter) = MakeLogic(LoadoutScene);
            logic.HandleFocusChanged(hasFocus: false);

            Assert.That(counter.Value, Is.EqualTo(0));
            Assert.That(logic.LastFocusLossPaused, Is.False);
        }

        [Test]
        public void FocusLoss_InHomeScene_DoesNotRaisePauseIntent()
        {
            var (logic, _, counter) = MakeLogic(HomeScene);
            logic.HandleFocusChanged(hasFocus: false);

            Assert.That(counter.Value, Is.EqualTo(0));
        }

        // ---- focus-gained is always a no-op ----

        [Test]
        public void FocusGained_InRunScene_IsNoOp()
        {
            var (logic, _, counter) = MakeLogic(RunScene);
            logic.HandleFocusChanged(hasFocus: true);

            Assert.That(counter.Value, Is.EqualTo(0),
                "Focus gained must NOT raise PauseRunRequested — Resume is player-driven.");
            Assert.That(logic.LastFocusLossPaused, Is.False);
        }

        [Test]
        public void FocusGained_InMainMenu_IsNoOp()
        {
            var (logic, _, counter) = MakeLogic(MainMenuScene);
            logic.HandleFocusChanged(hasFocus: true);

            Assert.That(counter.Value, Is.EqualTo(0));
        }

        // ---- successive focus-loss events while still in Run ----

        [Test]
        public void FocusLoss_TwiceDuringRun_RaisesPauseTwice()
        {
            // Idempotency of the pause modal itself is the PauseController's
            // concern — the focus handler is a thin pass-through. We assert
            // both events propagate so debouncing happens at the consumer.
            var (logic, _, counter) = MakeLogic(RunScene);
            logic.HandleFocusChanged(hasFocus: false);
            logic.HandleFocusChanged(hasFocus: false);

            Assert.That(counter.Value, Is.EqualTo(2));
        }

        // ---- scene change between focus events ----

        [Test]
        public void FocusLoss_AfterReturnedToMenu_IsNoOp()
        {
            // Player paused in Run → quit to menu → swiped away. Second
            // focus-loss while in MainMenu must not bounce a pause intent.
            var (logic, probe, counter) = MakeLogic(RunScene);
            logic.HandleFocusChanged(hasFocus: false);
            Assert.That(counter.Value, Is.EqualTo(1));

            probe.ActiveScene = MainMenuScene;
            logic.HandleFocusChanged(hasFocus: false);
            Assert.That(counter.Value, Is.EqualTo(1),
                "After returning to MainMenu, focus-loss must not re-raise the pause intent.");
        }

        // ---- constants exposed for downstream callers ----

        [Test]
        public void RunSceneNameConstant_MatchesPauseControllerSceneName()
        {
            // The gating constant must match the scene name used elsewhere in
            // the codebase. Guarded so a rename in one place breaks tests loudly.
            Assert.That(AppFocusPauseLogic.RunSceneName, Is.EqualTo(RunScene));
        }
    }
}
