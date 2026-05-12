#nullable enable
// Tech-spec 04 § Virtual joystick contract — dynamic placement, normalised [-1,+1] output.
// This is the lightweight IInputProvider implementation that PlayerMover consumes when no
// full-fledged VirtualJoystick UI prefab is wired. It reads touch directly from
// UnityEngine.InputSystem.Touchscreen.current — NOT the legacy Input.touches API.
//
// Geometry note: maxDragRadiusPx is a UI-geometry concern (how far the user's finger has
// to travel to reach full deflection), not a balance/tuning concern, so it lives in code
// as a SerializedField, not in data/balance/*.json. The richer sibling
// `VirtualJoystick.cs` derives its radius from the device's shorter screen edge per the
// tech-spec; this lighter input provider is for the minimum-viable Run-scene wiring
// before that UI prefab lands.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Brave.Gameplay.Movement
{
    /// <summary>
    /// Concrete <see cref="IInputProvider"/> backed by Unity Input System touch input.
    /// Tracks the active primary touch's drag offset from its initial press position and
    /// converts it into a normalised <see cref="Vector2"/> in [-1,+1] per axis.
    /// Falls back to <see cref="Vector2.zero"/> when no <see cref="Touchscreen"/> is present
    /// (editor / desktop) so PlayerMover's keyboard fallback can take over.
    /// </summary>
    public sealed class VirtualJoystickInput : MonoBehaviour, IInputProvider
    {
        // Max drag radius (screen pixels). UI geometry — NOT a balance value.
        // 100px ≈ a comfortable thumb-reach on a 5.4" display; matches MaxRadius
        // upper-bound in the sibling VirtualJoystick.cs and the wireframe spec
        // (docs/05-wireframes/ — joystick footprint ~80–100px diameter).
        [SerializeField] private float maxDragRadiusPx = 100f;

        private Vector2 _stick;
        private bool _pausePressed;
        private bool _abilityPressed;

        // Per-touch state — captured on press, mutated on drag.
        private bool _trackingTouch;
        private Vector2 _touchOrigin;

        public Vector2 StickDirection => _stick;
        public bool PausePressed => _pausePressed;
        public bool AbilityPressed => _abilityPressed;

        private void Update()
        {
            // Reset one-frame flags before sampling.
            _pausePressed = false;
            _abilityPressed = false;

            Touchscreen ts = Touchscreen.current;
            if (ts == null)
            {
                // Editor / desktop — no touch hardware. PlayerMover's keyboard
                // fallback will handle input. Surface zero so we don't fight it.
                _stick = Vector2.zero;
                _trackingTouch = false;
                return;
            }

            // primaryTouch is allocation-free — it's a TouchControl, not a List.
            TouchControl t = ts.primaryTouch;
            TouchPhase phase = t.phase.ReadValue();

            switch (phase)
            {
                case TouchPhase.Began:
                    _touchOrigin = t.position.ReadValue();
                    _trackingTouch = true;
                    _stick = Vector2.zero;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_trackingTouch)
                    {
                        Vector2 delta = t.position.ReadValue() - _touchOrigin;
                        _stick = ScreenDeltaToNormalized(delta, maxDragRadiusPx);
                    }
                    else
                    {
                        _stick = Vector2.zero;
                    }
                    break;

                default: // Ended, Canceled, None
                    _trackingTouch = false;
                    _stick = Vector2.zero;
                    break;
            }
        }

        // ---- Pure helper — testable without a Unity scene ---------------------------------

        /// <summary>
        /// Map a screen-space drag delta into a normalised joystick vector.
        ///   * Output magnitude is clamped to 1.0 (drags beyond <paramref name="maxRadius"/>
        ///     read as full deflection rather than over-saturating).
        ///   * (0,0) input → (0,0) output (no NaN at the origin).
        ///   * Negative or zero <paramref name="maxRadius"/> is treated as a no-op (zero).
        ///   * Allocation-free — only struct math, no <c>new()</c>.
        /// </summary>
        public static Vector2 ScreenDeltaToNormalized(Vector2 delta, float maxRadius)
        {
            if (maxRadius <= 0f) return Vector2.zero;

            float sqr = delta.x * delta.x + delta.y * delta.y;
            if (sqr <= 0f) return Vector2.zero;

            float maxRadiusSqr = maxRadius * maxRadius;
            if (sqr >= maxRadiusSqr)
            {
                // Beyond the ring — clamp to unit length in the same direction.
                float invMag = 1f / Mathf.Sqrt(sqr);
                return new Vector2(delta.x * invMag, delta.y * invMag);
            }

            // Inside the ring — linear proportion 0..1.
            float invRadius = 1f / maxRadius;
            return new Vector2(delta.x * invRadius, delta.y * invRadius);
        }
    }
}
