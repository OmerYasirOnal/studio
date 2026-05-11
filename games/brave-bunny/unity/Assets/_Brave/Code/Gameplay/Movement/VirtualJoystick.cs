#nullable enable
// Tech-spec 04 § Virtual joystick contract. Dynamic placement in bottom-left quadrant.
// Stick radius = clamp(0.18 * shorterEdge, 50, 84) pt. Dead zone 8% of stick radius.

using System;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Brave.Gameplay.Movement
{
    /// <summary>
    /// Dynamic-placement virtual joystick. Spawns on pointer-down in the bottom-left quadrant,
    /// produces a normalized Vector2 in [-1, 1]^2. Wired to the Player action map's Move (Value).
    /// </summary>
    public sealed class VirtualJoystick : MonoBehaviour
    {
        [SerializeField] private RectTransform? baseRing;
        [SerializeField] private RectTransform? thumbKnob;
        [SerializeField] private CanvasGroup? canvasGroup;

        // Stick geometry — formula from tech-spec 04
        private const float RadiusFactor   = 0.18f;
        private const float MinRadius      = 50f;
        private const float MaxRadius      = 84f;
        private const float DeadZonePct    = 0.08f;

        // Spawn quadrant — bottom-left
        private const float SpawnQuadX = 0.5f;
        private const float SpawnQuadY = 0.6f;

        // Fade timings (ms)
        private const float FadeInSeconds  = 0.080f;
        private const float FadeOutSeconds = 0.200f;

        private float _stickRadius;
        private float _deadZone;
        private Vector2 _spawnScreenPos;
        private bool _active;
        private Vector2 _value;

        public Vector2 Value => _value;
        public bool IsActive => _active;

        private void Awake()
        {
            float shorterEdge = Mathf.Min(Screen.width, Screen.height);
            _stickRadius = Mathf.Clamp(RadiusFactor * shorterEdge, MinRadius, MaxRadius);
            _deadZone    = _stickRadius * DeadZonePct;

            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        /// <summary>Called by InputService on pointer-down events.</summary>
        public void OnPointerDown(Vector2 screenPos)
        {
            // Reject pointer-downs outside the bottom-left spawn quadrant
            if (screenPos.x > Screen.width * SpawnQuadX) return;
            if (screenPos.y > Screen.height * SpawnQuadY) return;

            _spawnScreenPos = screenPos;
            _active = true;

            if (baseRing != null) baseRing.position = screenPos;
            if (thumbKnob != null) thumbKnob.position = screenPos;
            if (canvasGroup != null) canvasGroup.alpha = 1f;  // TODO: ease over FadeInSeconds via tween service
        }

        /// <summary>Called every frame while the touch is held.</summary>
        public void OnPointerDrag(Vector2 screenPos)
        {
            if (!_active) return;

            Vector2 offset = screenPos - _spawnScreenPos;
            float magnitude = offset.magnitude;

            if (magnitude < _deadZone)
            {
                _value = Vector2.zero;
            }
            else
            {
                float clampedMag = Mathf.Min(magnitude, _stickRadius);
                Vector2 dir = offset / magnitude;
                _value = dir * (clampedMag / _stickRadius);
                if (thumbKnob != null) thumbKnob.position = _spawnScreenPos + dir * clampedMag;
            }
        }

        /// <summary>Called on pointer-up; output zeroes and the ring fades out.</summary>
        public void OnPointerUp()
        {
            _active = false;
            _value = Vector2.zero;
            if (canvasGroup != null) canvasGroup.alpha = 0f;  // TODO: ease over FadeOutSeconds
        }
    }
}
