// QA — PauseController / PauseModalLogic EditMode tests.
// Subject under test:
//   * Brave.UI.Controllers.PauseModalLogic — pure-C# state machine driving the
//     pause modal. Verifies Time.timeScale freeze/restore, scene-routing on
//     Restart / Quit, IPauseTarget forwarding, and UIEvents subscription
//     contract for PauseRunRequested.
//
// Pattern: matches RunHudControllerTests — exercise the testable surface
// without instantiating UIDocument or scene-loading.

using System.Collections.Generic;
using Brave.UI.Bindings;
using Brave.UI.Controllers;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class PauseControllerTests
    {
        // ---- constants (no magic numbers — CLAUDE.md principle 6) ----
        private const float NormalTimeScale = 1f;
        private const float SlowMoTimeScale = 0.5f;
        private const float FrozenTimeScale = 0f;
        private const string ExpectedRunScene = "Run";
        private const string ExpectedMenuScene = "MainMenu";

        // ---- test doubles ----

        private sealed class FakeTimeScale : ITimeScaleSource
        {
            public float TimeScale { get; set; } = NormalTimeScale;
        }

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> LoadedScenes = new();
            public void Load(string sceneName) => LoadedScenes.Add(sceneName);
        }

        private sealed class FakePauseTarget : IPauseTarget
        {
            public int PauseCalls;
            public int ResumeCalls;
            public void Pause() => PauseCalls++;
            public void ResumeFromPause() => ResumeCalls++;
        }

        // ---- helpers ----

        private static (PauseModalLogic logic, FakeTimeScale time, FakeSceneLoader scene, FakePauseTarget target)
            MakeLogic(float startScale = NormalTimeScale)
        {
            var time = new FakeTimeScale { TimeScale = startScale };
            var scene = new FakeSceneLoader();
            var target = new FakePauseTarget();
            var logic = new PauseModalLogic(time, scene, target);
            return (logic, time, scene, target);
        }

        // ---- Time.timeScale freeze/restore ----

        [Test]
        public void Show_FreezesTimeScaleToZero()
        {
            var (logic, time, _, _) = MakeLogic();
            logic.Show();
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale),
                "Show() must freeze TimeScale to 0 so Update loops halt.");
            Assert.That(logic.IsPaused, Is.True);
        }

        [Test]
        public void Resume_RestoresPriorTimeScale()
        {
            var (logic, time, _, _) = MakeLogic(NormalTimeScale);
            logic.Show();
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale));

            logic.Resume();
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale),
                "Resume() must restore the prior TimeScale (1.0 by default).");
            Assert.That(logic.IsPaused, Is.False);
        }

        [Test]
        public void Resume_RestoresNonStandardPriorScale()
        {
            // If a slow-mo modifier was active when the player paused, Resume
            // must put us back into slow-mo — not blast back to 1.0.
            var (logic, time, _, _) = MakeLogic(SlowMoTimeScale);
            logic.Show();
            logic.Resume();
            Assert.That(time.TimeScale, Is.EqualTo(SlowMoTimeScale).Within(0.001f),
                "Prior slow-mo TimeScale must round-trip through Show/Resume.");
        }

        [Test]
        public void Show_WhenAlreadyPaused_NoOp()
        {
            var (logic, time, _, target) = MakeLogic();
            logic.Show();
            logic.Show(); // second call
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale));
            Assert.That(target.PauseCalls, Is.EqualTo(1),
                "Idempotent: second Show() must not re-pause the run target.");
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotMutateTimeScale()
        {
            var (logic, time, _, _) = MakeLogic(NormalTimeScale);
            logic.Resume();
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale));
            Assert.That(logic.IsPaused, Is.False);
        }

        [Test]
        public void Toggle_AlternatesShowAndResume()
        {
            var (logic, time, _, _) = MakeLogic();
            logic.Toggle();
            Assert.That(logic.IsPaused, Is.True);
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale));
            logic.Toggle();
            Assert.That(logic.IsPaused, Is.False);
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale));
        }

        // ---- IPauseTarget forwarding ----

        [Test]
        public void Show_ForwardsToRunTargetPause()
        {
            var (logic, _, _, target) = MakeLogic();
            logic.Show();
            Assert.That(target.PauseCalls, Is.EqualTo(1),
                "Show() must call RunController.Pause() via the IPauseTarget hook.");
        }

        [Test]
        public void Resume_ForwardsToRunTargetResume()
        {
            var (logic, _, _, target) = MakeLogic();
            logic.Show();
            logic.Resume();
            Assert.That(target.ResumeCalls, Is.EqualTo(1));
        }

        [Test]
        public void Logic_WithNullTarget_DoesNotThrow()
        {
            // The target is optional — Boot scenes wired before RunController exists must work.
            var time = new FakeTimeScale();
            var scene = new FakeSceneLoader();
            var logic = new PauseModalLogic(time, scene, runTarget: null);
            Assert.DoesNotThrow(() => logic.Show());
            Assert.DoesNotThrow(() => logic.Resume());
        }

        // ---- scene routing on Quit / Restart ----

        [Test]
        public void QuitToMenu_LoadsMainMenuScene()
        {
            var (logic, _, scene, _) = MakeLogic();
            logic.Show();
            logic.QuitToMenu();
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedMenuScene),
                "Quit must request the MainMenu scene.");
        }

        [Test]
        public void RestartRun_LoadsRunScene()
        {
            var (logic, _, scene, _) = MakeLogic();
            logic.Show();
            logic.RestartRun();
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedRunScene),
                "Restart must request the Run scene.");
        }

        [Test]
        public void Quit_FromPaused_RestoresTimeScaleFirst()
        {
            // Without this, the next scene inherits TimeScale=0 and dies.
            var (logic, time, scene, _) = MakeLogic();
            logic.Show();
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale));
            logic.QuitToMenu();
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale),
                "Quit must restore TimeScale before triggering the scene load.");
            Assert.That(logic.IsPaused, Is.False);
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedMenuScene));
        }

        [Test]
        public void Restart_FromPaused_RestoresTimeScaleFirst()
        {
            var (logic, time, scene, _) = MakeLogic();
            logic.Show();
            logic.RestartRun();
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale),
                "Restart must restore TimeScale before triggering the scene load.");
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedRunScene));
        }

        // ---- visibility event ----

        [Test]
        public void VisibilityChanged_FiresTrueOnShowAndFalseOnResume()
        {
            var (logic, _, _, _) = MakeLogic();
            var events = new List<bool>();
            logic.VisibilityChanged += events.Add;

            logic.Show();
            logic.Resume();

            Assert.That(events, Is.EqualTo(new[] { true, false }),
                "VisibilityChanged must fire (true) on Show and (false) on Resume.");
        }

        // ---- UIEvents.PauseRunRequested subscription (the production contract) ----

        [Test]
        public void UIEvents_PauseRunRequested_RoutesToShow()
        {
            // PauseController subscribes UIEvents.PauseRunRequested → Show().
            // We verify the contract by wiring a handler that calls Show() and
            // confirming the event raises it; this is what the controller does
            // in OnEnable.
            UIEvents.ResetAllSubscribers();

            var (logic, time, _, _) = MakeLogic();
            UIEvents.PauseRunRequested += logic.Show;

            UIEvents.RaisePauseRunRequested();

            Assert.That(logic.IsPaused, Is.True, "UIEvents.PauseRunRequested must trigger Show().");
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale));

            UIEvents.ResetAllSubscribers();
        }

        // ---- scene name constants are not magic-number free-text in callers ----

        [Test]
        public void SceneNameConstants_AreStable()
        {
            // Guards against accidental renames; downstream agents (build engineer,
            // QA play-mode tests) depend on these names matching Build Settings.
            Assert.That(PauseModalLogic.RunSceneName, Is.EqualTo(ExpectedRunScene));
            Assert.That(PauseModalLogic.MainMenuSceneName, Is.EqualTo(ExpectedMenuScene));
            Assert.That(PauseModalLogic.SettingsScreenName, Is.EqualTo("Settings"));
            Assert.That(PauseModalLogic.PausedTimeScale, Is.EqualTo(FrozenTimeScale));
        }
    }
}
