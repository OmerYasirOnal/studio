#nullable enable
// Brave Bunny — Wave 10 / Combo Badge UI Controller
// Bound to: _Brave/UI/Documents/ComboBadge.uxml
// Channel:  Brave.Gameplay.Events.ComboChangedChannel
// Config:   Brave.Gameplay.Feel.FeelConfig.combo* (window, tier thresholds, fade)
//
// Behaviour:
//   * On every ComboChangedEvent with currentStreak >= tier-1 threshold, pop the
//     badge in (remove .is-hidden + add .combo-pop one-shot class) and update
//     the count label + tier class.
//   * On break (currentStreak == 0), schedule a fade-out after
//     FeelConfig.comboFadeOutSeconds, then hide the root.
//   * Below tier-1 threshold (streak 1 or 2) the badge stays hidden — only
//     "real" combos surface so the HUD doesn't churn on every single kill.
//
// CLAUDE.md principle 6: all timings + thresholds come from FeelConfig. Loc
// strings come from the LocalizationProvider via loc-key attributes in UXML.
//
// Allocation-free: no per-frame work; the controller is event-driven. The
// label uses cached "x{N}" string formatting via a small reusable
// `System.Text.StringBuilder` — Unity's UIElements label is the only allocator
// we can't avoid (it copies the string into its internal buffer).

using System.Text;

using Brave.Gameplay.Events;
using Brave.Gameplay.Feel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class ComboBadgeController : MonoBehaviour
    {
        // ---- USS class names (kebab-case per Wave-5 convention) ----
        public const string HiddenClass = "is-hidden";
        public const string PopClass = "combo-pop";
        public const string TierZeroClass = "combo-tier-0";
        public const string TierOneClass = "combo-tier-1";
        public const string TierTwoClass = "combo-tier-2";
        public const string TierThreeClass = "combo-tier-3";

        // ---- Element names (must match ComboBadge.uxml) ----
        public const string RootName = "combo-badge-root";
        public const string BadgeName = "combo-badge";
        public const string CountLabelName = "lbl-combo-count";

        // ---- Inspector wiring ----
        [Header("Event channel (subscribe to ComboService output)")]
        [SerializeField] private ComboChangedChannel? _channel;

        [Header("Config (window + tier thresholds + fade timing)")]
        [SerializeField] private FeelConfig? _config;

        // ---- Resolved view refs ----
        private UIDocument _doc = null!;
        private readonly BadgeElements _elements = new();
        private readonly StringBuilder _countBuf = new(8);
        private IVisualElementScheduledItem? _pendingFadeOut;

        // ---- Public hooks for tests + Boot wiring ----

        /// <summary>Bind a channel imperatively (Boot pathway). Idempotent.</summary>
        public void BindChannel(ComboChangedChannel channel)
        {
            if (_channel != null) _channel.Unsubscribe(OnComboChanged);
            _channel = channel;
            _channel.Subscribe(OnComboChanged);
        }

        /// <summary>Bind config imperatively (Boot pathway / tests).</summary>
        public void BindConfig(FeelConfig config) => _config = config;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _elements.BindFrom(_doc.rootVisualElement);
            // Always start hidden — only show on first qualifying streak event.
            SetHidden(_elements.Root, true);
            if (_channel != null) _channel.Subscribe(OnComboChanged);
        }

        private void OnDisable()
        {
            if (_channel != null) _channel.Unsubscribe(OnComboChanged);
            _pendingFadeOut?.Pause();
            _pendingFadeOut = null;
        }

        private void OnComboChanged(ComboChangedEvent evt)
        {
            if (_config == null) return;
            Render(evt, _elements, _config, _countBuf);

            // Handle the fade-out schedule for the break case. The pure Render()
            // helper applies the immediate class state; the timed fade-out is
            // owned by the MonoBehaviour because UIElements' scheduler is the
            // mechanism for delayed updates without per-frame polling.
            if (evt.currentStreak <= 0)
            {
                _pendingFadeOut?.Pause();
                long delayMs = (long)(_config.comboFadeOutSeconds * 1000f);
                _pendingFadeOut = _elements.Root
                    .schedule
                    .Execute(() => SetHidden(_elements.Root, true))
                    .StartingIn(delayMs);
            }
            else
            {
                // New increment cancels a pending fade-out.
                _pendingFadeOut?.Pause();
                _pendingFadeOut = null;
            }
        }

        /// <summary>
        /// Pure render step — testable without a panel. Applies the tier class,
        /// updates the count label, toggles visibility based on the streak value.
        /// </summary>
        public static void Render(ComboChangedEvent evt, BadgeElements el, FeelConfig config, StringBuilder buf)
        {
            int tier = evt.currentStreak <= 0
                ? 0
                : Brave.Gameplay.Combat.ComboService.TierFor(evt.currentStreak, config);

            // Apply tier class (mutually exclusive).
            ApplyTierClass(el.Badge, tier);

            // Below tier 1: keep hidden (don't churn on a single kill). The fade-out
            // for the break case is scheduled by the MonoBehaviour caller.
            if (tier == 0)
            {
                // Keep current visibility — break case will schedule the hide.
                // For the "kill 1, kill 2" sub-threshold path keep the badge hidden.
                if (evt.currentStreak > 0)
                {
                    SetHidden(el.Root, true);
                }
                return;
            }

            // Build "x{N}" without allocating a fresh string each time.
            buf.Clear();
            buf.Append('x');
            buf.Append(evt.currentStreak);
            el.Count.text = buf.ToString();

            SetHidden(el.Root, false);
            // Trigger the pop transition (USS .combo-pop is a one-shot class — we
            // add then immediately remove next frame; the transition runs from
            // .combo-pop's transform-scale to the resting state).
            if (!el.Badge.ClassListContains(PopClass))
            {
                el.Badge.AddToClassList(PopClass);
                el.Badge.schedule.Execute(() => el.Badge.RemoveFromClassList(PopClass)).StartingIn(1);
            }
        }

        private static void ApplyTierClass(VisualElement el, int tier)
        {
            // Remove all tier classes first, then apply the resolved one.
            if (el.ClassListContains(TierZeroClass)) el.RemoveFromClassList(TierZeroClass);
            if (el.ClassListContains(TierOneClass)) el.RemoveFromClassList(TierOneClass);
            if (el.ClassListContains(TierTwoClass)) el.RemoveFromClassList(TierTwoClass);
            if (el.ClassListContains(TierThreeClass)) el.RemoveFromClassList(TierThreeClass);

            switch (tier)
            {
                case 1: el.AddToClassList(TierOneClass); break;
                case 2: el.AddToClassList(TierTwoClass); break;
                case 3: el.AddToClassList(TierThreeClass); break;
                default: el.AddToClassList(TierZeroClass); break;
            }
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

        /// <summary>
        /// Resolved view refs. Tests construct an instance directly without UXML
        /// and call <see cref="Render"/> against it.
        /// </summary>
        public sealed class BadgeElements
        {
            public VisualElement Root = null!;
            public VisualElement Badge = null!;
            public Label Count = null!;

            public void BindFrom(VisualElement root)
            {
                Root = root.Q<VisualElement>(RootName)!;
                Badge = root.Q<VisualElement>(BadgeName)!;
                Count = root.Q<Label>(CountLabelName)!;
            }
        }
    }
}
