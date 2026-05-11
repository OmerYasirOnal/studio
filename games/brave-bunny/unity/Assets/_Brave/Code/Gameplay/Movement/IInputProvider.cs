using UnityEngine;

namespace Brave.Gameplay.Movement;

/// <summary>
/// Abstraction over the input source so the run hot path doesn't care whether the joystick
/// is on-screen, gamepad, or recorded for replay. Per tech-spec 04 (Input system).
/// </summary>
public interface IInputProvider
{
    /// <summary>Joystick direction; magnitude 0..1.</summary>
    Vector2 StickDirection { get; }

    /// <summary>True for the frame the pause button was pressed.</summary>
    bool PausePressed { get; }

    /// <summary>True for the frame the ability button was pressed (reserved for v1.1).</summary>
    bool AbilityPressed { get; }
}
