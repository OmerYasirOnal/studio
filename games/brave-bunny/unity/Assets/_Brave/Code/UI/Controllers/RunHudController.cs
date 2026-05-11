// Brave Bunny — UI / Controllers / RunHudController
// Bound to: _Brave/UI/Documents/RunHud.uxml
// Wireframe spec: docs/05-wireframes/05-run-hud.html
// User stories: US-13 joystick, US-16 pause, US-19 auto-attack, US-21 HUD
//               readability, US-24 currency drops, US-28 wave pressure.
//
// KPI (per wireframe): Joystick → bunny within 1 frame (16 ms). HUD readable
// in iPhone SE 3 thumb-occluded zones.
//
// Wiring contract: this controller subscribes to whichever ScriptableObject
// event channels the gameplay layer has wired into the Boot scene. To keep
// the UI assembly buildable BEFORE the gameplay-engineer ships the canonical
// `HpChangedChannel` / `XpChangedChannel` / `RunTimerChannel`, we expose
// SerializeField slots — controllers gracefully no-op when not assigned.

#nullable enable

using Brave.UI.Bindings;
using Brave.UI.Theming;
using Brave.Gameplay.Events;
using Brave.Systems.Context;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class RunHudController : MonoBehaviour
    {
        [Header("Gameplay event channels (optional — wired by Boot)")]
        [SerializeField] private LevelUpChannel? _levelUpChannel;
        [SerializeField] private PickupChannel? _pickupChannel;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;

        // Cached element refs — looked up once on enable.
        private VisualElement _hpFill = null!;
        private VisualElement _xpFill = null!;
        private Label _timerLabel = null!;
        private Label _levelBadge = null!;
        private Label _waveToast = null!;
        private Label _pickupGoldAmount = null!;
        private Label _pickupHeartAmount = null!;

        private float _runStartTime;
        private int _accumulatedGoldPickup;
        private int _accumulatedHeartPickup;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _hpFill = root.Q<VisualElement>("hp-bar-fill")!;
            _xpFill = root.Q<VisualElement>("xp-bar-fill")!;
            _timerLabel = root.Q<Label>("lbl-timer")!;
            _levelBadge = root.Q<Label>("lbl-level-badge")!;
            _waveToast = root.Q<Label>("lbl-wave-toast")!;
            _pickupGoldAmount = root.Q<Label>("lbl-pickup-gold-amount")!;
            _pickupHeartAmount = root.Q<Label>("lbl-pickup-heart-amount")!;

            root.Q<Button>("btn-pause")!.clicked += OnPauseClicked;

            _runStartTime = Time.time;
            _accumulatedGoldPickup = 0;
            _accumulatedHeartPickup = 0;

            if (_levelUpChannel != null) _levelUpChannel.Subscribe(OnLevelUp);
            if (_pickupChannel != null) _pickupChannel.Subscribe(OnPickup);

            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            if (_levelUpChannel != null) _levelUpChannel.Unsubscribe(OnLevelUp);
            if (_pickupChannel != null) _pickupChannel.Unsubscribe(OnPickup);
        }

        private void Update()
        {
            // Timer tick — recomputed locally; gameplay layer pushes pauses
            // through RunService which freezes Time.timeScale.
            var elapsed = Time.time - _runStartTime;
            var m = Mathf.FloorToInt(elapsed / 60f);
            var s = Mathf.FloorToInt(elapsed % 60f);
            _timerLabel.text = $"{m:D2}:{s:D2}";
        }

        /// <summary>Called by gameplay-engineer's HP service whenever HP changes.</summary>
        public void SetHp(float ratio01)
        {
            _hpFill.style.width = new StyleLength(new Length(Mathf.Clamp01(ratio01) * 100f, LengthUnit.Percent));
        }

        /// <summary>Called whenever XP changes; ratio is xpIntoLevel / xpForNextLevel.</summary>
        public void SetXp(float ratio01)
        {
            _xpFill.style.width = new StyleLength(new Length(Mathf.Clamp01(ratio01) * 100f, LengthUnit.Percent));
        }

        public void ShowWaveToast(string locKey)
        {
            _waveToast.text = _loc.Loc(locKey);
            _waveToast.style.display = DisplayStyle.Flex;
            _waveToast.schedule.Execute(() => _waveToast.style.display = DisplayStyle.None).StartingIn(2500);
        }

        private void OnLevelUp(LevelUpEvent evt)
        {
            _levelBadge.text = $"Lv {evt.newLevel}";
            // XP bar resets to xpRemainder/nextThreshold; gameplay-engineer's
            // ProgressionService is the canonical source. UI only displays.
            SetXp(0f);
        }

        private void OnPickup(PickupEvent evt)
        {
            switch (evt.kind)
            {
                case PickupKind.GoldCoin:
                    _accumulatedGoldPickup += evt.amount;
                    _pickupGoldAmount.text = $"+ {_accumulatedGoldPickup}";
                    break;
                case PickupKind.Heart:
                    _accumulatedHeartPickup += evt.amount;
                    _pickupHeartAmount.text = $"+ {_accumulatedHeartPickup}";
                    break;
                // XP gems show up via SetXp; nothing to render in pickup feed.
            }
        }

        private void OnPauseClicked() => UIEvents.RaisePauseRunRequested();
    }
}
