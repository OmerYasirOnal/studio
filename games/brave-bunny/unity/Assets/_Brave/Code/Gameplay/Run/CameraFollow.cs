#nullable enable
// Brave Bunny — Gameplay / Run / CameraFollow
//
// MVP playability camera. Attach to the Run-scene MainCamera; on Awake the
// component captures (target.position - camera.position) as its world-space
// offset, then per-LateUpdate the camera smooth-damps toward target.position +
// offset and re-aims its forward at the target.
//
// Why LateUpdate: PlayerMover writes transform.position in Update(); LateUpdate
// guarantees we read the post-input position so the camera never trails by a
// frame (US-13 latency budget — joystick-to-camera within one frame).
//
// Allocation-free: SmoothDamp uses cached ref-Vector3 velocity state, no per-frame
// allocations. No coroutines, no lambdas in the hot path.
//
// Cross-refs:
//   * Editor/SceneSetup.cs — initial camera transform (0, 14.7, -10.4) rot (55,0,0).
//     The default offset of (10, 12, -8) supplied in the wave-12 brief is a
//     SerializeField on this component, overridable per-scene.
//   * docs/06-tech-spec/05-performance.md — LateUpdate path budget < 0.05 ms.
//   * docs/02-gdd/01-core-loop.md — top-down 3/4 perspective.

using UnityEngine;

namespace Brave.Gameplay.Run
{
    /// <summary>
    /// Smooth-damped follow camera for the Run scene. Reads the target's world
    /// position in LateUpdate and lerps the camera to (target + offset), then
    /// re-aims at the target. Allocation-free.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Follow target — assigned at scene setup or via SetTarget()")]
        [SerializeField] private Transform? target;

        [Tooltip("World-space offset from target to camera, in target's frame. "
                + "Default (10, 12, -8) gives a top-down 3/4 view per the wave-12 brief; "
                + "if zero at Awake, the offset is captured from the camera's initial pose.")]
        [SerializeField] private Vector3 offset = new Vector3(10f, 12f, -8f);

        [Tooltip("SmoothDamp time-to-target, seconds. Smaller = snappier follow. "
                + "0.12s balances responsiveness with anti-jitter for finger drags.")]
        [SerializeField, Min(0f)] private float smoothTime = 0.12f;

        [Tooltip("Maximum speed the camera will travel toward target (world u/s). "
                + "Mathf.Infinity = unbounded.")]
        [SerializeField] private float maxSpeed = Mathf.Infinity;

        [Tooltip("When true, the camera looks at the target every LateUpdate. "
                + "Disable to lock the initial rotation (e.g. for fixed-angle isometric).")]
        [SerializeField] private bool lookAtTarget = true;

        // Smooth-damp state — kept on the instance so allocations stay zero per-frame.
        private Vector3 _velocity = Vector3.zero;

        /// <summary>Most recent damp output. Useful for debugging / EditMode tests.</summary>
        public Vector3 LastDesiredPosition { get; private set; }

        /// <summary>Resolved offset used this run (post-Awake auto-capture if applicable).</summary>
        public Vector3 ResolvedOffset => offset;

        /// <summary>
        /// Runtime injection point. Used by RunSceneWiring / RunBootstrap once the
        /// hero is instantiated. Idempotent and allocation-free.
        /// </summary>
        public void SetTarget(Transform t)
        {
            target = t;
            _velocity = Vector3.zero;
        }

        private void Awake()
        {
            // If a target is already wired (drag in editor) and offset is zero,
            // auto-capture (target - self) so the camera's initial framing in the
            // scene file is preserved without forcing the level designer to type
            // the offset by hand.
            if (target != null && offset == Vector3.zero)
            {
                offset = transform.position - target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.position + offset;
            LastDesiredPosition = desired;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _velocity,
                smoothTime,
                maxSpeed,
                Time.deltaTime);

            if (lookAtTarget)
            {
                transform.LookAt(target.position);
            }
        }

        // ---- Pure helpers — testable without a Unity scene ---------------------------------

        /// <summary>
        /// Pure transform of (current, targetPos, offset) → desired camera position.
        /// EditMode tests assert on this without instantiating a MonoBehaviour.
        /// </summary>
        public static Vector3 ComputeDesiredPosition(Vector3 targetPosition, Vector3 worldOffset)
            => targetPosition + worldOffset;
    }
}
