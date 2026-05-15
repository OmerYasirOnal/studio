// Brave Bunny — UI / Controllers / QuitConfirmController
// Bound to: _Brave/UI/Documents/QuitConfirmDialog.uxml
// Wave 10 QoL — interposes between Pause-modal Quit and the actual scene exit.
//
// Lifecycle:
//   * PauseController routes its Quit button to QuitConfirmController.Show()
//     instead of directly loading MainMenu.
//   * Confirm: calls IQuitTarget.QuitRun (RunController.End(Quit, "player_quit"))
//     then ISceneLoader.Load("MainMenu"). TimeScale is restored before the
//     load so the next scene doesn't inherit a frozen clock.
//   * Cancel: hides the dialog and returns control to the pause modal (which
//     is still visible underneath).
//
// Pattern mirrors PauseModalLogic — a pure-C# inner state machine drives the
// EditMode-testable surface; the MonoBehaviour shell only wires UI Toolkit.

#nullable enable

using System;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>
    /// Abstraction over <c>RunController.End(RunOutcome.Quit, cause)</c>.
    /// EditMode tests stub this without touching the gameplay assembly.
    /// </summary>
    public interface IQuitTarget
    {
        void QuitRun(string cause);
    }

    /// <summary>
    /// Pure-C# state machine for the quit-confirm dialog. No UIDocument
    /// dependency — exercised in EditMode against fakes.
    /// </summary>
    public sealed class QuitConfirmLogic
    {
        public const string MainMenuSceneName = "MainMenu";
        public const string QuitCause = "player_quit";
        public const float RestoredTimeScale = 1f;

        private readonly ITimeScaleSource _time;
        private readonly ISceneLoader _scene;
        private readonly IQuitTarget? _runTarget;

        public bool IsVisible { get; private set; }

        /// <summary>Raised when the dialog becomes visible / hidden — UI shell wires its USS toggle here.</summary>
        public event Action<bool>? VisibilityChanged;

        public QuitConfirmLogic(ITimeScaleSource time, ISceneLoader scene, IQuitTarget? runTarget = null)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _runTarget = runTarget;
        }

        public void Show()
        {
            if (IsVisible) return;
            IsVisible = true;
            VisibilityChanged?.Invoke(true);
        }

        public void Cancel()
        {
            if (!IsVisible) return;
            IsVisible = false;
            VisibilityChanged?.Invoke(false);
        }

        /// <summary>Confirm the quit — end the run, restore timescale, load the menu.</summary>
        public void Confirm()
        {
            // Always end the dialog first so VisibilityChanged fires before
            // any scene change tears the UIDocument down.
            IsVisible = false;
            VisibilityChanged?.Invoke(false);

            _runTarget?.QuitRun(QuitCause);

            // Restore timescale so MainMenu doesn't inherit a frozen clock.
            _time.TimeScale = RestoredTimeScale;
            _scene.Load(MainMenuSceneName);
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class QuitConfirmController : MonoBehaviour
    {
        // ---- USS class toggles ----
        public const string HiddenClass = "is-hidden";

        // ---- Element names (must match QuitConfirmDialog.uxml) ----
        public const string RootName = "quit-confirm-root";
        public const string ConfirmButtonName = "btn-confirm";
        public const string CancelButtonName = "btn-cancel";

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private Button _btnConfirm = null!;
        private Button _btnCancel = null!;

        private QuitConfirmLogic _logic = null!;
        private IQuitTarget? _runTarget;

        /// <summary>Exposed so PauseController can invoke Show() without a scene-wide lookup.</summary>
        public QuitConfirmLogic Logic => _logic;

        /// <summary>Wire the run-target post-construction (boot scene). Optional in EditMode.</summary>
        public void Bind(IQuitTarget runTarget) => _runTarget = runTarget;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);

            _logic = new QuitConfirmLogic(new UnityTimeScaleSource(), new SceneManagerLoader(), _runTarget);
            _logic.VisibilityChanged += OnVisibilityChanged;
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _btnConfirm = root.Q<Button>(ConfirmButtonName)!;
            _btnCancel = root.Q<Button>(CancelButtonName)!;

            _btnConfirm.clicked += _logic.Confirm;
            _btnCancel.clicked += _logic.Cancel;

            // Hidden by default; PauseController routes its Quit button here.
            SetRootHidden(true);

            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            _btnConfirm.clicked -= _logic.Confirm;
            _btnCancel.clicked -= _logic.Cancel;
        }

        private void OnDestroy()
        {
            if (_logic != null) _logic.VisibilityChanged -= OnVisibilityChanged;
        }

        /// <summary>Public entry point — PauseController calls Show() instead of loading MainMenu.</summary>
        public void Show() => _logic.Show();

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
