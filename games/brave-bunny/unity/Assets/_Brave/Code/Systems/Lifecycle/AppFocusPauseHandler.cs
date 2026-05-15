// Brave Bunny — Systems / Lifecycle / AppFocusPauseHandler
// Wave 10 QoL — auto-pause on app focus loss.
//
// Subscribes to UnityEngine.Application.focusChanged (the correct API for
// app focus on both desktop and mobile — NOT focusGained, which doesn't exist
// as a public event). When focus is lost (player swiped away / received a
// phone call / pressed home), and we're inside the Run scene, the handler
// raises UIEvents.PauseRunRequested so the PauseController (Wave 7A) brings
// up the modal and freezes Time.timeScale.
//
// Gating: focus-loss in MainMenu / Loadout / Home is a no-op — we only want
// to interrupt active gameplay. The active scene is checked through an
// abstraction (IActiveSceneProbe) so EditMode tests can drive the handler
// without juggling SceneManager.
//
// Tech-spec: docs/06-tech-spec/03-save-system.md (Settings autosave on
// background) defers to the SaveService; this handler only owns the
// pause-on-focus-loss intent — saving rides whatever channel the save
// service is already listening on.

#nullable enable

using System;
using Brave.UI.Bindings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brave.Systems.Lifecycle
{
    /// <summary>
    /// Thin abstraction over <see cref="SceneManager.GetActiveScene"/>.name so
    /// the focus-loss → pause logic can be exercised in EditMode without
    /// touching <see cref="SceneManager"/>.
    /// </summary>
    public interface IActiveSceneProbe
    {
        string GetActiveSceneName();
    }

    /// <summary>Production scene probe — proxies <see cref="SceneManager"/>.</summary>
    public sealed class SceneManagerProbe : IActiveSceneProbe
    {
        public string GetActiveSceneName() => SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Pure-C# state machine for "should app-focus-loss pause the run?".
    /// Owns the gating policy (Run scene only) and the side-effect of raising
    /// the pause intent. The MonoBehaviour shell forwards
    /// <see cref="Application.focusChanged"/> events into <see cref="HandleFocusChanged"/>.
    /// </summary>
    public sealed class AppFocusPauseLogic
    {
        /// <summary>Scene name that gates the pause-on-focus-loss behaviour.</summary>
        public const string RunSceneName = "Run";

        private readonly IActiveSceneProbe _scene;
        private readonly Action _raisePauseIntent;

        /// <summary>True when the last observed focus change paused a run.</summary>
        public bool LastFocusLossPaused { get; private set; }

        public AppFocusPauseLogic(IActiveSceneProbe scene, Action raisePauseIntent)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _raisePauseIntent = raisePauseIntent ?? throw new ArgumentNullException(nameof(raisePauseIntent));
        }

        /// <summary>
        /// Called for every focus-changed event. Pauses iff:
        ///   * focus was lost (hasFocus == false), AND
        ///   * the active scene is <see cref="RunSceneName"/>.
        /// </summary>
        public void HandleFocusChanged(bool hasFocus)
        {
            LastFocusLossPaused = false;
            if (hasFocus) return; // Gained focus is a no-op — Resume is player-driven.

            var sceneName = _scene.GetActiveSceneName();
            if (!string.Equals(sceneName, RunSceneName, StringComparison.Ordinal)) return;

            _raisePauseIntent();
            LastFocusLossPaused = true;
        }
    }

    /// <summary>
    /// MonoBehaviour shell — wires <see cref="Application.focusChanged"/> to
    /// <see cref="AppFocusPauseLogic"/>. Place one instance in the Boot scene
    /// (it survives via the existing GameContextBootstrap parent).
    /// </summary>
    public sealed class AppFocusPauseHandler : MonoBehaviour
    {
        private AppFocusPauseLogic _logic = null!;

        /// <summary>Exposed for EditMode integration tests / Wave 7A pause agent.</summary>
        public AppFocusPauseLogic Logic => _logic;

        private void Awake()
        {
            _logic = new AppFocusPauseLogic(new SceneManagerProbe(), UIEvents.RaisePauseRunRequested);
        }

        private void OnEnable()
        {
            Application.focusChanged += OnFocusChanged;
        }

        private void OnDisable()
        {
            Application.focusChanged -= OnFocusChanged;
        }

        private void OnFocusChanged(bool hasFocus) => _logic.HandleFocusChanged(hasFocus);
    }
}
