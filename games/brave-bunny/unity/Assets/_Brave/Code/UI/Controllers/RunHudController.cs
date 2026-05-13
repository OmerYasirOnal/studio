// Brave Bunny — UI / Controllers / RunHudController
// Bound to: _Brave/UI/Documents/RunHud.uxml
// Wireframe spec: docs/05-wireframes/05-run-hud.html
// User stories: US-13 joystick, US-16 pause, US-19 auto-attack, US-21 HUD
//               readability, US-24 currency drops, US-28 wave pressure.
//
// KPI (per wireframe): Joystick → bunny within 1 frame (16 ms). HUD readable
// in iPhone SE 3 thumb-occluded zones.
//
// Wiring contract (ADR-0021):
//   * BindState(IRunRuntimeState) is the preferred wiring path. RunBootstrap
//     calls it once after UXML is mounted. The HUD subscribes to StateChanged
//     and redraws the full view on each signal.
//   * When no binding is attached (or the binding is later cleared), the
//     controller falls back to per-frame Update() + RunHudStubRuntime so the
//     HUD renders plausible placeholder values in editor + EditMode tests.
//   * Pulse events (level-up, pickup, pause-button) still flow through the
//     existing ScriptableObject channels and the UIEvents bus — that contract
//     is unchanged from Phase-5 Wave-1.
//
// Allocation note (per Wave-5 conventions):
//   The per-frame update path uses class-toggles (AddToClassList /
//   RemoveFromClassList) for show/hide and a single string.Format for the
//   "mm:ss" timer + "HP/MaxHP" overlay. That is the Wave-5 baseline; the
//   stretch goal of zero-alloc formatting via Span<char> is tracked in the
//   hand-off note.

#nullable enable

using Brave.Gameplay.Events;
using Brave.Systems.Context;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class RunHudController : MonoBehaviour
    {
        // ---- USS class toggles (kebab-case per Wave-5 convention) ----
        public const string HiddenClass = "is-hidden";

        // ---- Element names (must match RunHud.uxml) ----
        public const string HpFillName = "hp-bar-fill";
        public const string XpFillName = "xp-bar-fill";
        public const string HpNumericName = "lbl-hp-numeric";
        public const string TimerLabelName = "lbl-timer";
        public const string WaveCounterName = "lbl-wave-counter";
        public const string LevelPillName = "lbl-level-pill";
        public const string WaveToastName = "lbl-wave-toast";
        public const string BossWarningName = "boss-warning";
        public const string PickupGoldAmountName = "lbl-pickup-gold-amount";
        public const string PickupHeartAmountName = "lbl-pickup-heart-amount";
        public const string PauseButtonName = "btn-pause";

        [Header("Gameplay event channels (optional — wired by Boot)")]
        [SerializeField] private LevelUpChannel? _levelUpChannel;
        [SerializeField] private PickupChannel? _pickupChannel;

        /// <summary>
        /// Per-frame state binding. Set via <see cref="BindState"/> or directly.
        /// When <c>null</c>, the controller falls back to <see cref="RunHudStubRuntime"/>.
        /// </summary>
        public IRunRuntimeState? State { get; set; }

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private readonly RunHudStubRuntime _fallbackState = new();
        private readonly HudElements _elements = new();

        // Retained so the fallback Update() path can show elapsed time even
        // without a RunController bound (editor play, standalone preview).
        private float _runStartTime;

        private int _accumulatedGoldPickup;
        private int _accumulatedHeartPickup;

        // ---- Event-driven binding (ADR-0021) ----

        /// <summary>
        /// Binds the HUD to a live <see cref="IRunRuntimeState"/> implementation.
        /// Call this after UXML is mounted (i.e. after OnEnable) so element refs are valid.
        /// Idempotent: rebinding unsubscribes the previous state first.
        /// On bind an immediate <see cref="Render"/> pass is performed so the HUD
        /// shows correct values before the first <see cref="IRunRuntimeState.StateChanged"/> fires.
        /// </summary>
        public void BindState(IRunRuntimeState state)
        {
            if (State != null) State.StateChanged -= OnStateChanged;
            State = state;
            State.StateChanged += OnStateChanged;
            // Immediate sync.
            Render(State, _elements);
        }

        private void OnStateChanged()
        {
            if (State != null) Render(State, _elements);
        }

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            _elements.BindFrom(_doc.rootVisualElement);
            _elements.PauseButton.clicked += OnPauseClicked;

            _runStartTime = Time.time;
            _accumulatedGoldPickup = 0;
            _accumulatedHeartPickup = 0;

            if (_levelUpChannel != null) _levelUpChannel.Subscribe(OnLevelUp);
            if (_pickupChannel != null) _pickupChannel.Subscribe(OnPickup);

            _loc.ApplyToTree(_doc.rootVisualElement);
        }

        private void OnDisable()
        {
            if (_levelUpChannel != null) _levelUpChannel.Unsubscribe(OnLevelUp);
            if (_pickupChannel != null) _pickupChannel.Unsubscribe(OnPickup);
            _elements.PauseButton.clicked -= OnPauseClicked;

            // Detach event subscription so we don't hold a reference after disable.
            if (State != null)
            {
                State.StateChanged -= OnStateChanged;
                State = null;
            }
        }

        private void Update()
        {
            // When a live state is bound via BindState(), redraws happen through
            // OnStateChanged() and we skip the per-frame poll to avoid double-work.
            // When no state is bound we fall back to polling the stub every frame
            // so the HUD renders plausible preview values in editor.
            if (State == null)
            {
                Render(_fallbackState, _elements);
            }
        }

        /// <summary>
        /// Pure render step — no dependencies on <see cref="UIDocument"/>,
        /// <see cref="Time"/>, or any MonoBehaviour state. EditMode tests call
        /// this directly against an in-memory <see cref="HudElements"/>
        /// instance.
        /// </summary>
        public static void Render(IRunRuntimeState state, HudElements el)
        {
            // ---- HP bar ----
            float maxHp = state.MaxHP <= 0f ? 1f : state.MaxHP;
            float hpRatio = Mathf.Clamp01(state.CurrentHP / maxHp);
            el.HpFill.style.width = new StyleLength(new Length(hpRatio * 100f, LengthUnit.Percent));
            el.HpNumeric.text = $"{Mathf.RoundToInt(state.CurrentHP)} / {Mathf.RoundToInt(state.MaxHP)}";

            // ---- XP bar + level pill ----
            float xpMax = state.XPToNextLevel <= 0f ? 1f : state.XPToNextLevel;
            float xpRatio = Mathf.Clamp01(state.CurrentXP / xpMax);
            el.XpFill.style.width = new StyleLength(new Length(xpRatio * 100f, LengthUnit.Percent));
            el.LevelPill.text = $"Lv {state.Level}";

            // ---- Wave + timer ----
            el.WaveCounter.text = $"Wave {state.WaveNumber}";
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(state.RunSecondsElapsed));
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            el.Timer.text = $"{m:D2}:{s:D2}";

            // ---- Boss-warning banner (class toggle, no per-frame allocation) ----
            SetHidden(el.BossWarning, !state.IsBossActive);
        }

        private static void SetHidden(VisualElement el, bool hidden)
        {
            if (hidden)
            {
                if (!el.ClassListContains(HiddenClass)) el.AddToClassList(HiddenClass);
            }
            else
            {
                if (el.ClassListContains(HiddenClass)) el.RemoveFromClassList(HiddenClass);
            }
        }

        public void ShowWaveToast(string locKey)
        {
            _elements.WaveToast.text = _loc.Loc(locKey);
            SetHidden(_elements.WaveToast, false);
            _elements.WaveToast.schedule.Execute(() => SetHidden(_elements.WaveToast, true)).StartingIn(2500);
        }

        private void OnLevelUp(LevelUpEvent evt)
        {
            _elements.LevelPill.text = $"Lv {evt.newLevel}";
            // XP-bar visual reset; the next Render() pass will refresh from the
            // canonical IRunRuntimeState.
            _elements.XpFill.style.width = new StyleLength(new Length(0f, LengthUnit.Percent));
        }

        private void OnPickup(PickupEvent evt)
        {
            switch (evt.kind)
            {
                case PickupKind.GoldCoin:
                    _accumulatedGoldPickup += evt.amount;
                    _elements.PickupGoldAmount.text = $"+ {_accumulatedGoldPickup}";
                    break;
                case PickupKind.Heart:
                    _accumulatedHeartPickup += evt.amount;
                    _elements.PickupHeartAmount.text = $"+ {_accumulatedHeartPickup}";
                    break;
                // XP gems show up via Render(); nothing to render in pickup feed.
            }
        }

        private void OnPauseClicked() => UIEvents.RaisePauseRunRequested();

        /// <summary>
        /// Bag of resolved HUD <see cref="VisualElement"/> refs. Lifetime-bound
        /// to the controller; reusable by EditMode tests which construct the
        /// bag manually (no UXML required).
        /// </summary>
        public sealed class HudElements
        {
            public VisualElement HpFill = null!;
            public VisualElement XpFill = null!;
            public Label HpNumeric = null!;
            public Label Timer = null!;
            public Label WaveCounter = null!;
            public Label LevelPill = null!;
            public Label WaveToast = null!;
            public VisualElement BossWarning = null!;
            public Label PickupGoldAmount = null!;
            public Label PickupHeartAmount = null!;
            public Button PauseButton = null!;

            public void BindFrom(VisualElement root)
            {
                HpFill = root.Q<VisualElement>(HpFillName)!;
                XpFill = root.Q<VisualElement>(XpFillName)!;
                HpNumeric = root.Q<Label>(HpNumericName)!;
                Timer = root.Q<Label>(TimerLabelName)!;
                WaveCounter = root.Q<Label>(WaveCounterName)!;
                LevelPill = root.Q<Label>(LevelPillName)!;
                WaveToast = root.Q<Label>(WaveToastName)!;
                BossWarning = root.Q<VisualElement>(BossWarningName)!;
                PickupGoldAmount = root.Q<Label>(PickupGoldAmountName)!;
                PickupHeartAmount = root.Q<Label>(PickupHeartAmountName)!;
                PauseButton = root.Q<Button>(PauseButtonName)!;
            }
        }
    }
}
