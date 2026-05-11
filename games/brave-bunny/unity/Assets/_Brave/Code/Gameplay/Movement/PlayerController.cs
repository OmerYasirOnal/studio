#nullable enable
// Tech-spec 04 § Latency target. Joystick output -> velocity in the same frame; no physics.
// Stats sourced from CharacterDefinition + balance/characters.json. No magic numbers.

using System;

using UnityEngine;

using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Movement
{
    /// <summary>
    /// Reads the virtual joystick (or editor WASD) and translates it directly into a
    /// transform velocity. Implements US-13 pipeline shape: same-frame velocity write.
    /// No Rigidbody — swarmer collision uses our custom 2D radial-overlap test
    /// (tech-spec 05 § Collision).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick? joystick;
        [SerializeField] private CharacterDefinition? character;

        private float _moveSpeed;        // cached from baseStats * char_speed_mult on Awake
        private const float MaxMoveSpeed = 9.0f;   // formulas.md § 3 cap (2x Bunny baseline)
        private Vector2 _lastInput;
        private bool _frozen;

        public Vector2 Velocity { get; private set; }
        public Vector2 Facing   { get; private set; } = Vector2.right;

        private void Awake()
        {
            if (character == null)
            {
                Debug.LogError($"{nameof(PlayerController)}: CharacterDefinition not assigned", this);
                enabled = false;
                return;
            }

            _moveSpeed = Mathf.Min(character.baseStats.baseMoveSpeed, MaxMoveSpeed);
        }

        /// <summary>Called by RunStateMachine when leaving Run -> RunPaused (freeze) or revive (unfreeze).</summary>
        public void SetFrozen(bool frozen)
        {
            _frozen = frozen;
            if (frozen) Velocity = Vector2.zero;
        }

        /// <summary>Run hot path: no allocations. Reads joystick value once per frame.</summary>
        private void Update()
        {
            if (_frozen || joystick == null) return;

            Vector2 input = joystick.Value;
            _lastInput = input;

            Vector2 delta = input * _moveSpeed * Time.deltaTime;
            Vector3 pos = transform.position;
            pos.x += delta.x;
            pos.y += delta.y;
            transform.position = pos;

            Velocity = input * _moveSpeed;
            if (input.sqrMagnitude > 0.01f)
                Facing = input.normalized;
        }

        /// <summary>Used by AutoAttackController for targeting-priority front-arc preference.</summary>
        public Vector2 LastInput => _lastInput;
    }
}
