#nullable enable
// Brave Bunny — Hit Feedback Juice
// HitFlash: tint an enemy MeshRenderer white for ~60ms on damage, then restore. Uses a
// shared MaterialPropertyBlock so we never instantiate per-enemy materials (would break
// SRP batching and balloon draw calls past tech-spec 05's cap of 80).
//
// Attached to the Enemy prefab; one instance per enemy. The flash window is started by
// calling Flash() from the hit-application site. We don't subscribe to a damage event
// directly — the call site already has the renderer context and the enemy reference.

using UnityEngine;

namespace Brave.Gameplay.Feel
{
    /// <summary>
    /// Per-enemy hit-flash: temporarily overrides the renderer's color property with
    /// <see cref="FeelConfig.flashColor"/> for <see cref="FeelConfig.FlashSeconds"/>.
    /// Allocation-free: the <see cref="MaterialPropertyBlock"/> is reused.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitFlashController : MonoBehaviour
    {
        [SerializeField] private FeelConfig? _config;
        [SerializeField] private Renderer? _renderer;

        private MaterialPropertyBlock? _mpb;
        private int _colorPropertyId;
        private Color _restColor = Color.white;
        private float _restoreAtUnscaledTime;
        private bool _flashing;
        private bool _propertyResolved;

        /// <summary><c>true</c> while a flash is being held.</summary>
        public bool IsFlashing => _flashing;

        public void BindConfig(FeelConfig config) => _config = config;
        public void BindRenderer(Renderer renderer) => _renderer = renderer;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
            ResolvePropertyId();
        }

        private void OnEnable()
        {
            // Reset flash state when the GameObject is acquired from the enemy pool.
            _flashing = false;
            _restoreAtUnscaledTime = 0f;
            ApplyColor(_restColor);
        }

        private void ResolvePropertyId()
        {
            if (_config == null || string.IsNullOrEmpty(_config.flashColorPropertyName))
            {
                _colorPropertyId = Shader.PropertyToID("_BaseColor");
            }
            else
            {
                _colorPropertyId = Shader.PropertyToID(_config.flashColorPropertyName);
            }
            _propertyResolved = true;
        }

        /// <summary>
        /// Start a flash window. If a flash is already active, restarts it (latest call wins).
        /// Safe to call from the hit hot-path: zero allocations, single per-instance MPB.
        /// </summary>
        public void Flash()
        {
            if (_config == null || _renderer == null) return;
            if (!_propertyResolved) ResolvePropertyId();

            ApplyColor(_config.flashColor);
            _restoreAtUnscaledTime = Time.unscaledTime + _config.FlashSeconds;
            _flashing = true;
        }

        private void Update()
        {
            if (!_flashing) return;
            if (Time.unscaledTime >= _restoreAtUnscaledTime)
            {
                ApplyColor(_restColor);
                _flashing = false;
            }
        }

        private void ApplyColor(Color c)
        {
            if (_renderer == null) return;
            _mpb ??= new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorPropertyId, c);
            _renderer.SetPropertyBlock(_mpb);
        }

        /// <summary>Testing hook: simulate Update() at a given unscaled-time without Unity's loop.</summary>
        internal void Tick(float unscaledNow)
        {
            if (!_flashing) return;
            if (unscaledNow >= _restoreAtUnscaledTime)
            {
                ApplyColor(_restColor);
                _flashing = false;
            }
        }
    }
}
