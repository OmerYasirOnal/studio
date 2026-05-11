// Balance/00-formulas.md § 3: move_speed = base_move × character_speed_mult × (1 + speed_buff_total).
using Brave.Gameplay.Definitions;
using UnityEngine;

namespace Brave.Gameplay.Movement;

/// <summary>
/// Player movement controller. Reads from an <see cref="IInputProvider"/> and uses
/// the character's <see cref="CharacterStats.baseMoveSpeed"/> as the speed source.
/// </summary>
public sealed class PlayerMover : MonoBehaviour
{
    [SerializeField] private float _baseMoveUnitsPerSec = 4.5f;  // characters.json base_move_units_per_sec
    [SerializeField] private float _speedCap = 9.0f;             // balance § 3 cap

    private IInputProvider _input;
    private float _moveMultiplier = 1f;
    private float _speedBuffTotal = 0f;

    public void Initialise(IInputProvider input, CharacterStats stats)
    {
        _input = input;
        _baseMoveUnitsPerSec = stats.baseMoveSpeed;
        _moveMultiplier = 1f;  // character multiplier already pre-folded into baseMoveSpeed at hero-spawn
    }

    public void ApplyBuff(float speedBuffPercentDelta) => _speedBuffTotal += speedBuffPercentDelta;

    private void Update()
    {
        if (_input == null) return;
        float speed = Mathf.Min(_baseMoveUnitsPerSec * _moveMultiplier * (1f + _speedBuffTotal), _speedCap);
        transform.position = Mover.Step(transform.position, _input.StickDirection, speed, Time.deltaTime);
    }
}
