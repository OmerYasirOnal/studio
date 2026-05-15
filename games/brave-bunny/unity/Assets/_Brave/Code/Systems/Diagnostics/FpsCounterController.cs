// Brave Bunny — Systems / Diagnostics / FpsCounterController
// Wave 10 QoL — top-right FPS counter, gated behind SettingsService.DevModeEnabled.
//
// Lifecycle:
//   * Awake: builds a tiny IMGUI-free UI Toolkit Label parented to a host
//     UIDocument (the Run HUD root is fine — wire via inspector).
//   * Update: samples a rolling-window FPS value (locally — no dependency on
//     Brave.Diagnostics so this stays inside the Brave.Systems.Diagnostics
//     asmdef and ships in player builds).
//   * Subscribes to SettingsService.OnChanged so toggling DevMode at runtime
//     flips visibility without a restart.
//
// DevMode toggle is hidden behind SettingsService — there's no UI element
// yet. Cheat via PlayerPrefs / save edit in build for now (Wave 10 deferred:
// add an in-Settings DevMode checkbox once the secret-tap input is wired).

#nullable enable

using System;
using Brave.Systems.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.Systems.Diagnostics
{
    /// <summary>
    /// Pure-C# FPS sampler — 60-frame ring buffer, zero per-frame GC.
    /// Public so EditMode tests can poke samples directly and assert the
    /// averaged readout.
    /// </summary>
    public sealed class FpsCounterLogic
    {
        public const int RingSize = 60;

        private readonly float[] _ring = new float[RingSize];
        private int _head;
        private int _filled;
        private float _accum;

        /// <summary>Most recent rolling-average FPS. 0 until at least one sample arrives.</summary>
        public float CurrentFps { get; private set; }

        /// <summary>Push one unscaled delta-time sample (seconds). Computes a rolling FPS.</summary>
        public void Sample(float unscaledDeltaTime)
        {
            if (unscaledDeltaTime <= 0f) return;
            _accum -= _ring[_head];
            _ring[_head] = unscaledDeltaTime;
            _accum += unscaledDeltaTime;
            _head = (_head + 1) % RingSize;
            if (_filled < RingSize) _filled++;
            CurrentFps = _filled / _accum;
        }

        /// <summary>Reset the ring (used when toggling visibility off → on).</summary>
        public void Reset()
        {
            Array.Clear(_ring, 0, RingSize);
            _head = 0;
            _filled = 0;
            _accum = 0f;
            CurrentFps = 0f;
        }
    }

    /// <summary>
    /// MonoBehaviour shell — owns the FPS Label and toggles it based on
    /// <see cref="ISettingsService.Current"/>.DevModeEnabled. Attach to a host
    /// GameObject in the Run scene (or Boot, if the counter should persist).
    /// </summary>
    public sealed class FpsCounterController : MonoBehaviour
    {
        // ---- USS class toggles ----
        public const string HiddenClass = "is-hidden";

        // ---- element-naming ----
        public const string LabelName = "lbl-fps-counter";

        [Tooltip("Host UIDocument — the FPS Label is parented to its rootVisualElement. " +
                 "Wire the Run HUD's UIDocument here.")]
        [SerializeField] private UIDocument? _host;

        private Label? _label;
        private FpsCounterLogic _logic = null!;
        private ISettingsService? _settings;
        private bool _visible;

        /// <summary>Exposed for EditMode tests — driving Sample() and asserting CurrentFps.</summary>
        public FpsCounterLogic Logic => _logic;

        private void Awake()
        {
            _logic = new FpsCounterLogic();
        }

        /// <summary>
        /// Boot-scene wiring hook. Called after the SettingsService comes online.
        /// </summary>
        public void Bind(ISettingsService settings)
        {
            if (_settings != null) _settings.OnChanged -= OnSettingsChanged;
            _settings = settings;
            _settings.OnChanged += OnSettingsChanged;
            ApplyVisibility(settings.Current.DevModeEnabled);
        }

        private void OnEnable()
        {
            EnsureLabel();
        }

        private void OnDisable()
        {
            if (_settings != null) _settings.OnChanged -= OnSettingsChanged;
        }

        private void Update()
        {
            if (!_visible) return;
            _logic.Sample(Time.unscaledDeltaTime);
            if (_label != null) _label.text = Mathf.RoundToInt(_logic.CurrentFps).ToString();
        }

        private void OnSettingsChanged(SettingsData data) => ApplyVisibility(data.DevModeEnabled);

        private void ApplyVisibility(bool dev)
        {
            _visible = dev;
            EnsureLabel();
            if (_label == null) return;
            if (dev)
            {
                _label.RemoveFromClassList(HiddenClass);
                _logic.Reset();
            }
            else
            {
                if (!_label.ClassListContains(HiddenClass)) _label.AddToClassList(HiddenClass);
            }
        }

        private void EnsureLabel()
        {
            if (_host == null || _host.rootVisualElement == null) return;
            if (_label != null) return;

            _label = new Label("0")
            {
                name = LabelName,
                pickingMode = PickingMode.Ignore,
            };
            _label.AddToClassList("fps-counter");
            // Inline style ensures the counter shows up even when no USS rule
            // matches — tiny top-right overlay above the HUD.
            _label.style.position = Position.Absolute;
            _label.style.top = 8f;
            _label.style.right = 12f;
            _label.style.fontSize = 18f;
            _label.style.color = new StyleColor(new Color(1f, 1f, 0.4f, 0.95f));
            _label.style.unityTextOutlineWidth = 1f;
            _label.style.unityTextOutlineColor = new StyleColor(new Color(0f, 0f, 0f, 0.85f));
            _host.rootVisualElement.Add(_label);
            if (!_visible) _label.AddToClassList(HiddenClass);
        }
    }
}
