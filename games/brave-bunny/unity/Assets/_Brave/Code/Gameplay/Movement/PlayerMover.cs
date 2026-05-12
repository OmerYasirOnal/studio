#nullable enable
// Player movement. Top-down 3/4 perspective (camera looks down the XZ ground plane;
// see Editor/SceneSetup.cs which positions MainCamera at (0, 14.7, -10.4) rot (55, 0, 0)).
//
// Input model — tech-spec 04 § Virtual joystick contract:
//   * Primary: an IInputProvider (the on-screen VirtualJoystick wired by the boot
//     composition root). Magnitude in [0,1].
//   * Editor fallback (and any platform with a connected keyboard): WASD / Arrow keys
//     via Unity Input System Keyboard.current. Diagonal input is normalised so a
//     diagonal press doesn't exceed cardinal speed.
//
// Speed source — balance/characters.json § base_move_units_per_sec (Bunny baseline 4.5).
// We read from CharacterDefinition.baseStats.baseMoveSpeed at Awake / via Configure.
// No magic numbers — if no CharacterDefinition is assigned the mover refuses to enable.

using UnityEngine;
using UnityEngine.InputSystem;

using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Movement
{
    /// <summary>
    /// Top-down player movement controller. One-frame input → velocity write per US-13
    /// latency pipeline. Allocation-free in Update.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PlayerMover : MonoBehaviour
    {
        [Header("Stats — must be assigned at edit time")]
        [SerializeField] private CharacterDefinition? character;

        [Header("Input — assigned via boot composition root")]
        [SerializeField] private MonoBehaviour? inputProviderBehaviour; // must implement IInputProvider

        private IInputProvider? _input;
        private float _moveSpeed;       // cached from CharacterDefinition.baseStats.baseMoveSpeed
        private bool _frozen;

        /// <summary>Last-applied velocity vector in world-XZ. Useful for animation/VFX consumers.</summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>Last non-zero facing direction (XZ). Defaults to +X (camera-right at scene start).</summary>
        public Vector3 Facing { get; private set; } = Vector3.right;

        private void Awake()
        {
            if (character == null)
            {
                Debug.LogError($"{nameof(PlayerMover)}: CharacterDefinition not assigned — disabling.", this);
                enabled = false;
                return;
            }

            _moveSpeed = character.baseStats.baseMoveSpeed;
            if (_moveSpeed <= 0f)
            {
                Debug.LogError(
                    $"{nameof(PlayerMover)}: '{character.slug}'.baseStats.baseMoveSpeed is {_moveSpeed} — " +
                    "balance JSON not imported into CharacterDefinition. Run 'Brave > Generate Balance SOs from JSON'.",
                    this);
                enabled = false;
                return;
            }

            if (inputProviderBehaviour is IInputProvider provider)
                _input = provider;
        }

        /// <summary>
        /// Runtime injection point. The boot composition root calls this once after the Player
        /// hero is instantiated, passing the joystick instance + the resolved character SO.
        /// </summary>
        public void Configure(IInputProvider input, CharacterDefinition characterDefinition)
        {
            _input = input;
            character = characterDefinition;
            _moveSpeed = characterDefinition.baseStats.baseMoveSpeed;
            enabled = _moveSpeed > 0f;
        }

        /// <summary>Pause / level-up draft freezes velocity without disabling this component.</summary>
        public void SetFrozen(bool frozen)
        {
            _frozen = frozen;
            if (frozen) Velocity = Vector3.zero;
        }

        private void Update()
        {
            if (_frozen) return;

            Vector2 raw = ReadInput();
            Vector3 velocity = ComputeVelocity(raw, _moveSpeed);

            // Apply velocity for this frame. No Rigidbody — see tech-spec 05 § Collision.
            Vector3 pos = transform.position;
            pos.x += velocity.x * Time.deltaTime;
            pos.z += velocity.z * Time.deltaTime;
            transform.position = pos;

            Velocity = velocity;
            if (velocity.sqrMagnitude > 0f)
                Facing = velocity / velocity.magnitude;
        }

        /// <summary>
        /// Combines the joystick provider with keyboard WASD/Arrow fallback. Joystick wins when
        /// it's producing non-zero input; otherwise we sample the keyboard. Both feed the same
        /// normalised [-1,1] Vector2 contract.
        /// </summary>
        private Vector2 ReadInput()
        {
            // 1) Joystick — primary on touch devices, the production input path.
            if (_input != null)
            {
                Vector2 stick = _input.StickDirection;
                if (stick.sqrMagnitude > 0f) return stick;
            }

            // 2) Keyboard fallback — editor & desktop. Unity Input System keyboard read is
            //    allocation-free (Keyboard.current is a singleton, ReadValue returns a float).
            Keyboard? kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            float x = 0f;
            float y = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    y += 1f;
            return new Vector2(x, y);
        }

        // ---- Pure helpers — testable without a Unity scene ---------------------------------

        /// <summary>
        /// Pure transform of (input2D, speed) → world-XZ velocity.
        ///   * Input X maps to world.x; input Y maps to world.z (top-down camera convention).
        ///   * Input magnitude > 1 is normalised to 1 so diagonal WASD doesn't out-speed cardinals.
        ///   * Output magnitude is clamped to <c>speed</c>.
        /// </summary>
        public static Vector3 ComputeVelocity(Vector2 input, float speed)
        {
            float sqr = input.x * input.x + input.y * input.y;
            if (sqr <= 0f || speed <= 0f) return Vector3.zero;

            if (sqr > 1f)
            {
                float invMag = 1f / Mathf.Sqrt(sqr);
                input.x *= invMag;
                input.y *= invMag;
            }
            return new Vector3(input.x * speed, 0f, input.y * speed);
        }
    }
}
