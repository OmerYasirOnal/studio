#nullable enable
// Brave Bunny — Hit Feedback Juice
// Cinemachine is NOT in Packages/manifest.json (checked 2026-05-16). Falling back to a
// direct camera-offset shake: each Update we add a small sin/cos perturbation to the
// camera's local position, decayed over the shake duration. Amplitudes come from
// FeelConfig (screenshake_amp_low / _med / _high) — feel.json units are "screen-fraction",
// which we convert into world units via the camera's orthographic size (or vertical FOV
// at the camera's z distance for perspective).
//
// Triggered on:
//   - basic / elite / boss kills (via the existing EnemyKilledChannel)
//   - boss-phase events (BossPhaseChannel)
//   - player-hurt (direct call from the player-hurt service when wired)

using UnityEngine;

using Brave.Gameplay.Events;

namespace Brave.Gameplay.Feel
{
    /// <summary>Magnitude buckets matching <c>FeelConfig</c>.</summary>
    public enum ScreenShakeMagnitude
    {
        Low  = 0,
        Med  = 1,
        High = 2,
    }

    /// <summary>
    /// Direct-offset screen shake. Camera-anchored; restores to its rest position on
    /// expiry. Pure-math implementation (no Cinemachine) — Cinemachine is not in the
    /// project's package manifest as of 2026-05-16.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreenShakeController : MonoBehaviour
    {
        [SerializeField] private FeelConfig? _config;
        [SerializeField] private Camera? _camera;
        [SerializeField] private EnemyKilledChannel? _killChannel;
        [SerializeField] private BossPhaseChannel? _bossPhaseChannel;

        private Vector3 _restLocalPosition;
        private float _shakeStartUnscaledTime;
        private float _shakeDurationSeconds;
        private float _shakeWorldAmplitude;
        private float _shakeFrequencyHz;
        private bool _shaking;
        private bool _cameraCaptured;

        public bool IsShaking => _shaking;
        public float CurrentAmplitudeWorld => _shakeWorldAmplitude;

        public void BindConfig(FeelConfig config) => _config = config;
        public void BindCamera(Camera camera) { _camera = camera; CaptureRest(); }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
            CaptureRest();
        }

        private void OnEnable()
        {
            if (_killChannel != null) _killChannel.Subscribe(OnEnemyKilled);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Subscribe(OnBossPhase);
        }

        private void OnDisable()
        {
            if (_killChannel != null) _killChannel.Unsubscribe(OnEnemyKilled);
            if (_bossPhaseChannel != null) _bossPhaseChannel.Unsubscribe(OnBossPhase);
        }

        private void CaptureRest()
        {
            if (_camera == null || _cameraCaptured) return;
            _restLocalPosition = _camera.transform.localPosition;
            _cameraCaptured = true;
        }

        /// <summary>
        /// Trigger a shake at the given magnitude bucket. Amplitudes are read from
        /// <see cref="FeelConfig"/>; duration defaults to <c>screenshakeDurationSeconds</c>
        /// but can be overridden per-call (e.g. boss-kill = longer hold).
        /// </summary>
        public void Shake(ScreenShakeMagnitude magnitude, float? durationOverrideSeconds = null)
        {
            if (_config == null || _camera == null) return;
            CaptureRest();

            float ampFraction = magnitude switch
            {
                ScreenShakeMagnitude.High => _config.screenshakeAmpHigh,
                ScreenShakeMagnitude.Med  => _config.screenshakeAmpMed,
                _                          => _config.screenshakeAmpLow,
            };

            float worldAmp = FractionToWorldUnits(ampFraction);
            float duration = durationOverrideSeconds ?? _config.screenshakeDurationSeconds;

            // Coalesce: if a stronger or longer shake is requested mid-shake, take the max.
            if (_shaking)
            {
                _shakeWorldAmplitude = Mathf.Max(_shakeWorldAmplitude, worldAmp);
                float endNew = Time.unscaledTime + duration;
                float endCur = _shakeStartUnscaledTime + _shakeDurationSeconds;
                if (endNew > endCur)
                {
                    _shakeStartUnscaledTime = Time.unscaledTime;
                    _shakeDurationSeconds = duration;
                }
                return;
            }

            _shakeStartUnscaledTime = Time.unscaledTime;
            _shakeDurationSeconds = duration;
            _shakeWorldAmplitude = worldAmp;
            _shakeFrequencyHz = _config.screenshakeFrequencyHz;
            _shaking = true;
        }

        private float FractionToWorldUnits(float screenFraction)
        {
            if (_camera == null) return screenFraction;
            // For an orthographic camera, "screen vertical" = 2 * orthographicSize world units.
            if (_camera.orthographic)
                return screenFraction * _camera.orthographicSize * 2f;
            // For perspective, use the rough view-height at ~10 units in front of the camera.
            const float referenceDistance = 10f;
            float verticalWorld = 2f * referenceDistance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return screenFraction * verticalWorld;
        }

        private void Update()
        {
            if (!_shaking || _camera == null) return;
            float now = Time.unscaledTime;
            float elapsed = now - _shakeStartUnscaledTime;
            if (elapsed >= _shakeDurationSeconds)
            {
                _camera.transform.localPosition = _restLocalPosition;
                _shaking = false;
                return;
            }

            // Linear decay; sin/cos perturbation on X/Y for a 2D-game-feel jitter.
            float t = elapsed / _shakeDurationSeconds;
            float decay = 1f - t;
            float phase = elapsed * _shakeFrequencyHz * Mathf.PI * 2f;
            float dx = Mathf.Sin(phase) * _shakeWorldAmplitude * decay;
            float dy = Mathf.Cos(phase * 1.3f) * _shakeWorldAmplitude * decay;
            _camera.transform.localPosition = _restLocalPosition + new Vector3(dx, dy, 0f);
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
            => Shake(e.wasElite ? ScreenShakeMagnitude.Med : ScreenShakeMagnitude.Low);

        private void OnBossPhase(BossPhaseEvent _)
            => Shake(ScreenShakeMagnitude.High);
    }
}
