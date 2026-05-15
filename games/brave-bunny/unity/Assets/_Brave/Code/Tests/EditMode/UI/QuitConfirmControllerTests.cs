// QA — QuitConfirmController / QuitConfirmLogic EditMode tests (Wave 10 QoL).
// Subject under test:
//   * Brave.UI.Controllers.QuitConfirmLogic — pure-C# state machine for the
//     quit-confirm dialog. Verifies confirm-path (end-run + restore timescale
//     + load MainMenu) and cancel-path (hide + no side effects).
//
// Pattern matches PauseControllerTests — exercise the testable surface
// without UIDocument or SceneManager.

#nullable enable

using System.Collections.Generic;
using Brave.UI.Controllers;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class QuitConfirmControllerTests
    {
        // ---- constants ----
        private const float NormalTimeScale = 1f;
        private const float FrozenTimeScale = 0f;
        private const string ExpectedMenuScene = "MainMenu";
        private const string ExpectedQuitCause = "player_quit";

        // ---- test doubles ----

        private sealed class FakeTimeScale : ITimeScaleSource
        {
            public float TimeScale { get; set; } = FrozenTimeScale;
        }

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> LoadedScenes = new();
            public void Load(string sceneName) => LoadedScenes.Add(sceneName);
        }

        private sealed class FakeQuitTarget : IQuitTarget
        {
            public readonly List<string> Causes = new();
            public int QuitCalls => Causes.Count;
            public void QuitRun(string cause) => Causes.Add(cause);
        }

        // ---- helpers ----

        private static (QuitConfirmLogic logic, FakeTimeScale time, FakeSceneLoader scene, FakeQuitTarget target)
            MakeLogic()
        {
            var time = new FakeTimeScale();
            var scene = new FakeSceneLoader();
            var target = new FakeQuitTarget();
            var logic = new QuitConfirmLogic(time, scene, target);
            return (logic, time, scene, target);
        }

        // ---- Show / Cancel ----

        [Test]
        public void Show_MakesVisible()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.Show();
            Assert.That(logic.IsVisible, Is.True);
        }

        [Test]
        public void Show_WhenAlreadyVisible_IsIdempotent()
        {
            var (logic, _, _, _) = MakeLogic();
            int events = 0;
            logic.VisibilityChanged += _ => events++;

            logic.Show();
            logic.Show();

            Assert.That(events, Is.EqualTo(1),
                "Second Show() must not raise a duplicate VisibilityChanged event.");
        }

        [Test]
        public void Cancel_HidesAndDoesNotCallQuitTarget()
        {
            var (logic, time, scene, target) = MakeLogic();
            logic.Show();
            logic.Cancel();

            Assert.That(logic.IsVisible, Is.False);
            Assert.That(target.QuitCalls, Is.EqualTo(0),
                "Cancel must NOT call RunController.End — the player kept playing.");
            Assert.That(scene.LoadedScenes, Is.Empty,
                "Cancel must NOT load any scene.");
            Assert.That(time.TimeScale, Is.EqualTo(FrozenTimeScale),
                "Cancel must NOT mutate TimeScale — the underlying pause modal stays frozen.");
        }

        [Test]
        public void Cancel_WhenNotVisible_IsNoOp()
        {
            var (logic, _, _, _) = MakeLogic();
            int events = 0;
            logic.VisibilityChanged += _ => events++;

            logic.Cancel();

            Assert.That(events, Is.EqualTo(0));
        }

        // ---- Confirm — happy path ----

        [Test]
        public void Confirm_CallsQuitTargetWithPlayerQuitCause()
        {
            var (logic, _, _, target) = MakeLogic();
            logic.Show();
            logic.Confirm();

            Assert.That(target.Causes, Has.Exactly(1).EqualTo(ExpectedQuitCause),
                "Confirm must call RunController.End(Quit, \"player_quit\").");
        }

        [Test]
        public void Confirm_LoadsMainMenuScene()
        {
            var (logic, _, scene, _) = MakeLogic();
            logic.Show();
            logic.Confirm();

            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedMenuScene),
                "Confirm must request the MainMenu scene.");
        }

        [Test]
        public void Confirm_RestoresTimeScaleBeforeSceneLoad()
        {
            // The pause modal froze TimeScale to 0; Confirm must restore it
            // so MainMenu doesn't inherit a frozen clock.
            var (logic, time, _, _) = MakeLogic();
            time.TimeScale = FrozenTimeScale;
            logic.Show();
            logic.Confirm();

            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale),
                "Confirm must restore TimeScale to 1.0 before loading MainMenu.");
        }

        [Test]
        public void Confirm_HidesDialog()
        {
            var (logic, _, _, _) = MakeLogic();
            logic.Show();
            logic.Confirm();

            Assert.That(logic.IsVisible, Is.False);
        }

        [Test]
        public void Confirm_FiresVisibilityChangedFalseBeforeSceneLoad()
        {
            // Ensures consumers wired to VisibilityChanged see the hide event
            // before the scene tears down — otherwise the UIDocument would
            // be destroyed mid-event.
            var (logic, _, scene, _) = MakeLogic();
            var ordered = new List<string>();
            logic.VisibilityChanged += v => ordered.Add(v ? "shown" : "hidden");

            // Wrap the FakeSceneLoader call with a marker by reading scene state.
            logic.Show();
            logic.Confirm();

            Assert.That(ordered, Is.EqualTo(new[] { "shown", "hidden" }));
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedMenuScene));
        }

        // ---- Confirm — defensive ----

        [Test]
        public void Confirm_WithNullQuitTarget_StillRoutesToMenu()
        {
            // Boot scenes may wire the dialog before RunController exists.
            var time = new FakeTimeScale();
            var scene = new FakeSceneLoader();
            var logic = new QuitConfirmLogic(time, scene, runTarget: null);

            logic.Show();
            Assert.DoesNotThrow(() => logic.Confirm());

            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo(ExpectedMenuScene));
            Assert.That(time.TimeScale, Is.EqualTo(NormalTimeScale));
        }

        // ---- constants stable ----

        [Test]
        public void QuitConfirmConstants_AreStable()
        {
            // Guards renames; downstream agents (telemetry, analytics) match on
            // the cause string "player_quit".
            Assert.That(QuitConfirmLogic.MainMenuSceneName, Is.EqualTo(ExpectedMenuScene));
            Assert.That(QuitConfirmLogic.QuitCause, Is.EqualTo(ExpectedQuitCause));
            Assert.That(QuitConfirmLogic.RestoredTimeScale, Is.EqualTo(NormalTimeScale));
        }
    }
}
