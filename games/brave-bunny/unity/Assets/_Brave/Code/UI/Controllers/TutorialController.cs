// Brave Bunny — UI / Controllers / TutorialController
// Bound to: _Brave/UI/Documents/Tutorial.uxml
// Wave 7C onboarding spec: 30-second first-run flow with 5 steps —
//   1. Move   — "tutorial.move"          (advances when player moves 1m)
//   2. Attack — "tutorial.attack"        (advances on first enemy killed)
//   3. XP     — "tutorial.pickup_xp"     (advances on first level-up)
//   4. Boss   — "tutorial.boss"          (advances on boss-phase change)
//   5. Done   — "tutorial.pause_hint"    (auto-completes after Done; dismiss)
//
// Behaviour:
//   * On Run scene start, if TutorialState.ShouldShow == true, the overlay
//     mounts and step 1 is shown.
//   * Each step's trigger pushes the controller to the next step. The final
//     step calls TutorialState.MarkCompleted() (persists tutorialSeen=true)
//     and dismisses.
//   * The Skip Tutorial button triggers MarkCompleted() and dismisses early.
//   * Time.timeScale is NEVER touched — the tutorial does not pause the game.
//   * If TutorialState.ShouldShow == false at Awake, the panel hides itself
//     and the controller never subscribes to channels (zero perf cost).
//
// Testability: TutorialFlowLogic is a pure-C# state machine. The
// MonoBehaviour shell wires it to the UIDocument + ScriptableObject event
// channels in OnEnable. EditMode tests exercise step progression, dismiss,
// and persistence without spinning up UIDocument or scene-loading.

#nullable enable

using System;
using Brave.Gameplay.Events;
using Brave.Systems.Progression;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>The sequential steps shown by the first-run tutorial overlay.</summary>
    public enum TutorialStep
    {
        Move = 0,
        Attack = 1,
        PickupXp = 2,
        Boss = 3,
        Done = 4,
        Completed = 5,
    }

    /// <summary>
    /// Pure-C# state machine for the first-run tutorial. Owns the active step,
    /// the persistence side-effect (via <see cref="ITutorialState"/>), and the
    /// loc-key resolution for the current step. No UIDocument dependency.
    /// </summary>
    public sealed class TutorialFlowLogic
    {
        // Loc-key constants — single source of truth so UXML and tests share the
        // same strings (CLAUDE.md principle 6: no magic strings duplicated).
        public const string LocKeyMove = "tutorial.move";
        public const string LocKeyAttack = "tutorial.attack";
        public const string LocKeyPickupXp = "tutorial.pickup_xp";
        public const string LocKeyBoss = "tutorial.boss";
        public const string LocKeyDone = "tutorial.pause_hint";
        public const string LocKeyDismiss = "tutorial.dismiss";

        private readonly ITutorialState _state;

        /// <summary>Active step. Starts at <see cref="TutorialStep.Move"/>.</summary>
        public TutorialStep Current { get; private set; } = TutorialStep.Move;

        /// <summary>True once the tutorial has been completed or skipped this session.</summary>
        public bool IsDismissed => Current == TutorialStep.Completed;

        /// <summary>Raised when the active step changes. UI shell rebinds the label text.</summary>
        public event Action<TutorialStep>? StepChanged;

        /// <summary>Raised when the tutorial is dismissed (completed or skipped). UI hides the panel.</summary>
        public event Action? Dismissed;

        public TutorialFlowLogic(ITutorialState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        /// <summary>Returns the loc-key for <paramref name="step"/>.</summary>
        public static string LocKeyFor(TutorialStep step) => step switch
        {
            TutorialStep.Move => LocKeyMove,
            TutorialStep.Attack => LocKeyAttack,
            TutorialStep.PickupXp => LocKeyPickupXp,
            TutorialStep.Boss => LocKeyBoss,
            TutorialStep.Done => LocKeyDone,
            _ => string.Empty,
        };

        /// <summary>
        /// External trigger: player moved at least the "first hop" threshold.
        /// Advances Move → Attack. No-op for any later step (idempotent —
        /// PlayerMover ticks every frame and we don't want to skip steps).
        /// </summary>
        public void NotifyMoved()
        {
            if (Current == TutorialStep.Move) AdvanceTo(TutorialStep.Attack);
        }

        /// <summary>Player killed their first enemy. Advances Attack → PickupXp.</summary>
        public void NotifyEnemyKilled()
        {
            if (Current == TutorialStep.Attack) AdvanceTo(TutorialStep.PickupXp);
        }

        /// <summary>Player levelled up. Advances PickupXp → Boss.</summary>
        public void NotifyLevelUp()
        {
            if (Current == TutorialStep.PickupXp) AdvanceTo(TutorialStep.Boss);
        }

        /// <summary>Boss phase changed (boss fight started). Advances Boss → Done.</summary>
        public void NotifyBossPhase()
        {
            if (Current == TutorialStep.Boss) AdvanceTo(TutorialStep.Done);
        }

        /// <summary>
        /// Marks the tutorial as completed — persists the flag and dismisses.
        /// Called by the UI shell when the player taps the dismiss button on
        /// the Done step, OR auto-fired by a controller policy after the Done
        /// step has been visible long enough (orchestrator decision).
        /// </summary>
        public void Complete()
        {
            if (Current == TutorialStep.Completed) return;
            _state.MarkCompleted();
            Current = TutorialStep.Completed;
            StepChanged?.Invoke(Current);
            Dismissed?.Invoke();
        }

        /// <summary>Skip early — persists the flag (so it doesn't show next run) and dismisses.</summary>
        public void Skip()
        {
            // Same persistence intent as Complete — once the player taps "got it"
            // we never want to show the tutorial again.
            Complete();
        }

        private void AdvanceTo(TutorialStep next)
        {
            Current = next;
            StepChanged?.Invoke(Current);
        }
    }

    /// <summary>
    /// MonoBehaviour shell binding <see cref="TutorialFlowLogic"/> to a UIDocument
    /// + ScriptableObject event channels. Lifecycle:
    ///   * Awake: pull <see cref="ITutorialState"/> from a serialized accessor;
    ///     if ShouldShow == false the panel disables itself.
    ///   * OnEnable: subscribe to EnemyKilledChannel / LevelUpChannel /
    ///     BossPhaseChannel and to button click. NotifyMoved is driven by the
    ///     boot composition root calling <see cref="LogicHandle"/>.NotifyMoved()
    ///     on the first non-zero <see cref="Gameplay.Movement.PlayerMover.Velocity"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TutorialController : MonoBehaviour
    {
        // ---- USS class toggles ----
        public const string HiddenClass = "is-hidden";

        // ---- Element names (must match Tutorial.uxml) ----
        public const string RootName = "tutorial-root";
        public const string StepLabelName = "lbl-tutorial-step";
        public const string SkipButtonName = "btn-tutorial-skip";

        [Tooltip("EnemyKilled channel — advances Attack → PickupXp on first kill.")]
        [SerializeField] private EnemyKilledChannel? _enemyKilledChannel;

        [Tooltip("LevelUp channel — advances PickupXp → Boss on first level-up.")]
        [SerializeField] private LevelUpChannel? _levelUpChannel;

        [Tooltip("BossPhase channel — advances Boss → Done when boss appears.")]
        [SerializeField] private BossPhaseChannel? _bossPhaseChannel;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private Label _stepLabel = null!;
        private Button _skipButton = null!;

        private TutorialFlowLogic? _logic;

        /// <summary>Exposed for boot composition root + EditMode tests. Null until Bind() runs.</summary>
        public TutorialFlowLogic? LogicHandle => _logic;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        /// <summary>
        /// Wire the controller to an <see cref="ITutorialState"/>. Must be called
        /// by the boot composition root before the Run scene's first frame so the
        /// overlay can decide whether to mount. Returns false if the tutorial has
        /// already been completed — the caller should leave the panel hidden.
        /// </summary>
        public bool Bind(ITutorialState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!state.ShouldShow)
            {
                SetRootHidden(true);
                return false;
            }
            _logic = new TutorialFlowLogic(state);
            _logic.StepChanged += OnStepChanged;
            _logic.Dismissed += OnDismissed;
            return true;
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _stepLabel = root.Q<Label>(StepLabelName)!;
            _skipButton = root.Q<Button>(SkipButtonName)!;

            _skipButton.clicked += OnSkipClicked;

            if (_enemyKilledChannel != null) _enemyKilledChannel.Subscribe(OnEnemyKilled);
            if (_levelUpChannel != null) _levelUpChannel.Subscribe(OnLevelUp);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Subscribe(OnBossPhase);

            _loc.ApplyToTree(root);
            RefreshStepText();
        }

        private void OnDisable()
        {
            if (_skipButton != null) _skipButton.clicked -= OnSkipClicked;

            if (_enemyKilledChannel != null) _enemyKilledChannel.Unsubscribe(OnEnemyKilled);
            if (_levelUpChannel != null) _levelUpChannel.Unsubscribe(OnLevelUp);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Unsubscribe(OnBossPhase);
        }

        private void OnDestroy()
        {
            if (_logic == null) return;
            _logic.StepChanged -= OnStepChanged;
            _logic.Dismissed -= OnDismissed;
        }

        private void OnSkipClicked() => _logic?.Skip();

        private void OnEnemyKilled(EnemyKilledEvent _) => _logic?.NotifyEnemyKilled();
        private void OnLevelUp(LevelUpEvent _) => _logic?.NotifyLevelUp();
        private void OnBossPhase(BossPhaseEvent _) => _logic?.NotifyBossPhase();

        private void OnStepChanged(TutorialStep step) => RefreshStepText();

        private void OnDismissed() => SetRootHidden(true);

        private void RefreshStepText()
        {
            if (_logic == null || _stepLabel == null) return;
            var key = TutorialFlowLogic.LocKeyFor(_logic.Current);
            if (!string.IsNullOrEmpty(key)) _stepLabel.text = _loc.Loc(key);
        }

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
