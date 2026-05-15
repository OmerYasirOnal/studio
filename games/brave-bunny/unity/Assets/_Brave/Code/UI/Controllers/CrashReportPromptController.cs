// Brave Bunny — UI / Controllers / CrashReportPromptController
// Bound to: _Brave/UI/Documents/CrashReportPrompt.uxml
// Wave 11 — GDPR-friendly opt-in dialog asking the user whether to send
// anonymous crash reports on the next launch.
//
// Lifecycle:
//   * Boot wiring inspects CrashReporter.HasUnsentReports + a
//     "prompt-acknowledged" flag (PlayerPrefs key) and invokes Show() iff a
//     crash exists AND the user has not yet answered.
//   * Yes:    optInTarget.SetCrashOptIn(true).
//   * No:     optInTarget.SetCrashOptIn(false) (default; stops re-prompting).
//   * Later:  acknowledged=false; re-ask on next crash. We still set
//             "prompt-shown-once-for-this-report=true" so the same crash
//             doesn't re-prompt within the same session.
//
// Pattern mirrors QuitConfirmController — a pure-C# inner state machine drives
// the EditMode-testable surface; the MonoBehaviour shell only wires UI Toolkit.

#nullable enable

using System;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>
    /// Boot-wired sink for the player's opt-in choice. Boot adapts this to
    /// <c>ISettingsService.SetCrashOptInEnabled(value); Commit();</c> without
    /// the UI assembly depending on Brave.Systems.Diagnostics.
    /// </summary>
    public interface ICrashOptInTarget
    {
        void SetCrashOptIn(bool enabled);
    }

    /// <summary>The three discrete user choices the prompt surfaces.</summary>
    public enum CrashPromptChoice
    {
        Pending = 0,
        OptIn = 1,
        Decline = 2,
        Defer = 3,
    }

    /// <summary>
    /// Pure-C# state machine for the crash-report prompt. No UIDocument
    /// dependency — exercised in EditMode against fakes.
    /// </summary>
    public sealed class CrashReportPromptLogic
    {
        private readonly ICrashOptInTarget? _target;

        public bool IsVisible { get; private set; }
        public CrashPromptChoice LastChoice { get; private set; } = CrashPromptChoice.Pending;

        public event Action<bool>? VisibilityChanged;
        public event Action<CrashPromptChoice>? ChoiceMade;

        public CrashReportPromptLogic(ICrashOptInTarget? target = null)
        {
            _target = target;
        }

        public void Show()
        {
            if (IsVisible) return;
            IsVisible = true;
            VisibilityChanged?.Invoke(true);
        }

        public void OptIn() => Commit(CrashPromptChoice.OptIn, enabled: true);
        public void Decline() => Commit(CrashPromptChoice.Decline, enabled: false);

        /// <summary>"Ask me later" — does not push a setting change.</summary>
        public void Defer()
        {
            if (!IsVisible) return;
            IsVisible = false;
            LastChoice = CrashPromptChoice.Defer;
            VisibilityChanged?.Invoke(false);
            ChoiceMade?.Invoke(CrashPromptChoice.Defer);
        }

        private void Commit(CrashPromptChoice choice, bool enabled)
        {
            if (!IsVisible) return;
            IsVisible = false;
            LastChoice = choice;
            _target?.SetCrashOptIn(enabled);
            VisibilityChanged?.Invoke(false);
            ChoiceMade?.Invoke(choice);
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class CrashReportPromptController : MonoBehaviour
    {
        // ---- USS class toggles ----
        public const string HiddenClass = "is-hidden";

        // ---- Element names (must match CrashReportPrompt.uxml) ----
        public const string RootName = "crash-report-prompt-root";
        public const string YesButtonName = "btn-yes";
        public const string NoButtonName = "btn-no";
        public const string LaterButtonName = "btn-later";

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private Button _btnYes = null!;
        private Button _btnNo = null!;
        private Button _btnLater = null!;

        private CrashReportPromptLogic _logic = null!;
        private ICrashOptInTarget? _target;

        public CrashReportPromptLogic Logic => _logic;

        /// <summary>Boot-scene wiring hook — supplies the settings sink.</summary>
        public void Bind(ICrashOptInTarget target)
        {
            _target = target;
            // Rebuild logic so the target captures into the closure.
            _logic = new CrashReportPromptLogic(_target);
            _logic.VisibilityChanged += OnVisibilityChanged;
        }

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            // _doc.rootVisualElement is null in Awake on some Unity versions —
            // SafeAreaUtility attaches lazily in OnEnable via the document.
            _logic = new CrashReportPromptLogic(_target);
            _logic.VisibilityChanged += OnVisibilityChanged;
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            if (root == null) return;

            _btnYes = root.Q<Button>(YesButtonName)!;
            _btnNo = root.Q<Button>(NoButtonName)!;
            _btnLater = root.Q<Button>(LaterButtonName)!;

            _btnYes.clicked += _logic.OptIn;
            _btnNo.clicked += _logic.Decline;
            _btnLater.clicked += _logic.Defer;

            SetRootHidden(true);
            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            if (_btnYes != null) _btnYes.clicked -= _logic.OptIn;
            if (_btnNo != null) _btnNo.clicked -= _logic.Decline;
            if (_btnLater != null) _btnLater.clicked -= _logic.Defer;
        }

        private void OnDestroy()
        {
            if (_logic != null) _logic.VisibilityChanged -= OnVisibilityChanged;
        }

        /// <summary>Public entry point — boot wiring calls Show() when a crash report is pending.</summary>
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
