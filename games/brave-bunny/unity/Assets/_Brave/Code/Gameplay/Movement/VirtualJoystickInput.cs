// Tech-spec 04: dynamic virtual joystick — appears under the touch-down position,
// proportional to shorter screen edge.
using UnityEngine;

namespace Brave.Gameplay.Movement;

/// <summary>
/// Concrete <see cref="IInputProvider"/> backed by an on-screen dynamic joystick.
/// Reads touches via Unity Input System and maps to a normalised <see cref="Vector2"/>.
/// </summary>
public sealed class VirtualJoystickInput : MonoBehaviour, IInputProvider
{
    [SerializeField] private float _radiusScreenFraction = 0.10f;  // tunable; cross-checked vs tech-spec 04
    [SerializeField] private float _deadzone = 0.10f;

    private Vector2 _stick;
    private bool _pausePressed;
    private bool _abilityPressed;

    public Vector2 StickDirection => _stick;
    public bool PausePressed => _pausePressed;
    public bool AbilityPressed => _abilityPressed;

    private void Update()
    {
        // TODO(Phase 5): wire to Unity Input System touch.* actions; integrate with Hud pause button.
        _stick = Vector2.zero;
        _pausePressed = false;
        _abilityPressed = false;
    }
}
