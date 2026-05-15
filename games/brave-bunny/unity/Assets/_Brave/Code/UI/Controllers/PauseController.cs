// Brave Bunny — UI / Controllers / PauseController
// Bound to: _Brave/UI/Documents/Pause.uxml
// User stories: US-16 pause anywhere, US-11 audio prefs (Settings link).
//
// Lifecycle:
//   * Subscribes to UIEvents.PauseRunRequested (raised by HUD pause btn, or
//     by input bindings — Escape on editor, hardware back on Android).
//   * On Show(): records prior Time.timeScale, freezes to 0, displays panel,
//     calls RunController.Pause() via the optional serialized reference if wired.
//   * Resume: restores prior timeScale, hides panel.
//   * Restart Run: loads "Run" scene fresh (Single).
//   * Quit to Menu: loads "MainMenu" scene (Single).
//   * Settings: pushes "Settings" screen via UIEvents.PushScreen.
//
// State + transitions live on a pure-C# inner class (PauseModalLogic) so the
// scene-routing path and Time.timeScale handling can be exercised in EditMode
// tests without spinning up a UIDocument. The MonoBehaviour shell wires the
// logic to UI Toolkit + Input.

#nullable enable

using System;
using Brave.Gameplay.Run;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>
    /// Minimal scene-load indirection so EditMode tests can verify which scene
    /// would have been loaded on Quit / Restart without touching SceneManager.
    /// </summary>
    public interface ISceneLoader
    {
        void Load(string sceneName);
    }

    /// <summary>Production scene loader — delegates to <see cref="SceneManager"/>.</summary>
    public sealed class SceneManagerLoader : ISceneLoader
    {
        public void Load(string sceneName) => SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// Abstraction over <see cref="UnityEngine.Time"/>.timeScale so EditMode tests
    /// can inspect the freeze/restore handshake without mutating the global clock.
    /// </summary>
    public interface ITimeScaleSource
    {
        float TimeScale { get; set; }
    }

    /// <summary>Production timescale source — proxies <see cref="UnityEngine.Time"/>.</summary>
    public sealed class UnityTimeScaleSource : ITimeScaleSource
    {
        public float TimeScale
        {
            get => Time.timeScale;
            set => Time.timeScale = value;
        }
    }

    /// <summary>
    /// Optional run-state hook — RunController implements this implicitly via
    /// its Pause/ResumeFromPause methods. We define a tiny interface so the
    /// EditMode tests don't need a MonoBehaviour to stand in for it.
    /// </summary>
    public interface IPauseTarget
    {
        void Pause();
        void ResumeFromPause();
    }

    /// <summary>
    /// Pure-C# state machine for the pause modal — no UnityEngine.UIDocument
    /// dependency, fully testable. Owns:
    ///   * Time.timeScale freeze/restore handshake (via ITimeScaleSource)
    ///   * Optional IPauseTarget forwarding (so RunController.Pause is called)
    ///   * Scene-routing intent (Restart / Quit) via ISceneLoader
    /// </summary>
    public sealed class PauseModalLogic
    {
        public const float PausedTimeScale = 0f;
        public const float DefaultRunTimeScale = 1f;
        public const string RunSceneName = "Run";
        public const string MainMenuSceneName = "MainMenu";
        public const string SettingsScreenName = "Settings";

        private readonly ITimeScaleSource _time;
        private readonly ISceneLoader _scene;
        private readonly IPauseTarget? _runTarget;

        private float _priorTimeScale = DefaultRunTimeScale;

        public bool IsPaused { get; private set; }

        /// <summary>Raised when the modal becomes visible / hidden. UI shell wires its USS toggle here.</summary>
        public event Action<bool>? VisibilityChanged;

        public PauseModalLogic(ITimeScaleSource time, ISceneLoader scene, IPauseTarget? runTarget = null)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _runTarget = runTarget;
        }

        public void Show()
        {
            if (IsPaused) return;
            _priorTimeScale = _time.TimeScale;
            _time.TimeScale = PausedTimeScale;
            IsPaused = true;
            _runTarget?.Pause();
            VisibilityChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                VisibilityChanged?.Invoke(false);
                return;
            }
            _time.TimeScale = _priorTimeScale;
            IsPaused = false;
            _runTarget?.ResumeFromPause();
            VisibilityChanged?.Invoke(false);
        }

        public void Toggle()
        {
            if (IsPaused) Resume();
            else Show();
        }

        public void RestartRun()
        {
            RestoreTimeScale();
            _scene.Load(RunSceneName);
        }

        public void QuitToMenu()
        {
            RestoreTimeScale();
            _scene.Load(MainMenuSceneName);
        }

        /// <summary>Restore timescale immediately (used before scene reloads or on disable).</summary>
        public void RestoreTimeScale()
        {
            if (!IsPaused) return;
            _time.TimeScale = _priorTimeScale;
            IsPaused = false;
        }
    }

    /// <summary>Adapts a <see cref="RunController"/> to the <see cref="IPauseTarget"/> contract.</summary>
    public sealed class RunControllerPauseTarget : IPauseTarget
    {
        private readonly RunController _run;
        public RunControllerPauseTarget(RunController run) { _run = run; }
        public void Pause() => _run.Pause();
        public void ResumeFromPause() => _run.ResumeFromPause();
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class PauseController : MonoBehaviour
    {
        // ---- USS class toggles ----
        public const string HiddenClass = "is-hidden";

        // ---- Element names (must match Pause.uxml) ----
        public const string RootName = "pause-root";
        public const string ResumeButtonName = "btn-resume";
        public const string SettingsButtonName = "btn-settings";
        public const string RestartButtonName = "btn-restart";
        public const string QuitButtonName = "btn-quit";

        [Tooltip("Optional — the live RunController. When wired, Pause/Resume are " +
                 "forwarded so the run state machine reflects the modal lifecycle.")]
        [SerializeField] private RunController? _runController;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private Button _btnResume = null!;
        private Button _btnSettings = null!;
        private Button _btnRestart = null!;
        private Button _btnQuit = null!;

        private PauseModalLogic _logic = null!;

        /// <summary>True while the pause modal is showing and time is frozen.</summary>
        public bool IsPaused => _logic != null && _logic.IsPaused;

        /// <summary>Exposed for input-binding agents (Escape / hardware-back) and the HUD pause button.</summary>
        public PauseModalLogic Logic => _logic;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);

            IPauseTarget? target = _runController != null
                ? new RunControllerPauseTarget(_runController)
                : null;
            _logic = new PauseModalLogic(new UnityTimeScaleSource(), new SceneManagerLoader(), target);
            _logic.VisibilityChanged += OnVisibilityChanged;
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _btnResume = root.Q<Button>(ResumeButtonName)!;
            _btnSettings = root.Q<Button>(SettingsButtonName)!;
            _btnRestart = root.Q<Button>(RestartButtonName)!;
            _btnQuit = root.Q<Button>(QuitButtonName)!;

            _btnResume.clicked += _logic.Resume;
            _btnSettings.clicked += OnSettingsClicked;
            _btnRestart.clicked += _logic.RestartRun;
            _btnQuit.clicked += _logic.QuitToMenu;

            UIEvents.PauseRunRequested += OnPauseRequested;

            // Modal is hidden at boot; HUD pause button or input toggles it.
            SetRootHidden(true);

            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            _btnResume.clicked -= _logic.Resume;
            _btnSettings.clicked -= OnSettingsClicked;
            _btnRestart.clicked -= _logic.RestartRun;
            _btnQuit.clicked -= _logic.QuitToMenu;
            UIEvents.PauseRunRequested -= OnPauseRequested;

            // If the panel is being torn down while paused, restore time so the
            // next scene doesn't inherit a frozen clock.
            _logic?.RestoreTimeScale();
        }

        private void OnDestroy()
        {
            if (_logic != null) _logic.VisibilityChanged -= OnVisibilityChanged;
        }

        private void Update()
        {
            // Lightweight Escape-to-toggle binding for the editor. Mobile devices
            // route through UIEvents.PauseRunRequested (HUD pause button) or the
            // platform's hardware-back handler.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _logic.Toggle();
            }
        }

        private void OnPauseRequested() => _logic.Show();

        private void OnSettingsClicked() => UIEvents.RaisePushScreen(PauseModalLogic.SettingsScreenName);

        private void OnVisibilityChanged(bool visible) => SetRootHidden(!visible);

        private void SetRootHidden(bool hidden)
        {
            var root = _doc?.rootVisualElement;
            if (root == null) return;
            if (hidden)
            {
                if (!root.ClassListContains(HiddenClass)) root.AddToClassList(HiddenClass);
            }
            else
            {
                if (root.ClassListContains(HiddenClass)) root.RemoveFromClassList(HiddenClass);
            }
        }
    }
}
