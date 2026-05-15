#nullable enable
// Wave 10: StatusEffectVisuals — per-effect VFX bridge.
//
// Hooks into the existing VfxPool (ADR-0005) rather than spawning ad-hoc particle systems.
// One pool key per visual:
//   * Burn   → "status.burn"    — orange flame particle
//   * Poison → "status.poison"  — green bubble particle
//   * Freeze → "status.freeze"  — blue tint via material property + ice-crystal particle
//   * Stun   → "status.stun"    — yellow "!" overhead pop
//   * Slow   → "status.slow"    — light-blue trail (subtle; debug-overlay-only by default)
//
// The visuals MonoBehaviour reads StatusEffectApplier.GetOrCreateState(enemy) each frame
// and turns the IsBurning / IsPoisoned / IsFrozen flags into Play() calls on the
// corresponding VfxPool. Pool wiring is injected by RunController; tests do not exercise
// the VFX path (verified at smoke-test layer in PlayMode later).

using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Per-enemy visual driver. Attach to each enemy prefab alongside <see cref="Enemy"/>;
    /// the spawner injects the per-effect <see cref="VfxPool"/> references via <see cref="Initialise"/>.
    /// Reads the per-enemy <see cref="StatusEffectState"/> from <see cref="StatusEffectApplier"/>
    /// each frame and plays the matching VFX when flags transition.</summary>
    [DisallowMultipleComponent]
    public sealed class StatusEffectVisuals : MonoBehaviour
    {
        // Material-property name for the freeze tint. Read by the shader on the
        // enemy's renderer. No magic string in shader code — surfaced here so the
        // art-bible can rename without churning gameplay.
        public const string FreezeTintProperty = "_FreezeTint";

        // RGBA colours per effect. Sourced from art-bible §3 (cold blue, hot orange,
        // toxic green). Kept here as named constants so tweaking is one-file.
        public static readonly Color BurnColor   = new(1.0f, 0.45f, 0.10f, 1f); // warm orange
        public static readonly Color PoisonColor = new(0.30f, 0.85f, 0.20f, 1f); // toxic green
        public static readonly Color FreezeColor = new(0.50f, 0.80f, 1.00f, 1f); // ice blue
        public static readonly Color StunColor   = new(1.00f, 0.95f, 0.20f, 1f); // alert yellow
        public static readonly Color SlowColor   = new(0.70f, 0.90f, 1.00f, 1f); // pale blue

        [SerializeField] private Renderer? _targetRenderer;

        private VfxPool? _burnPool;
        private VfxPool? _poisonPool;
        private VfxPool? _freezePool;
        private VfxPool? _stunPool;

        private Enemy? _enemy;
        private MaterialPropertyBlock? _mpb;

        // Last-frame snapshot of the state flags so we only fire VFX on TRANSITIONS,
        // not every frame (allocation-free; bool comparisons are cheap).
        private bool _wasBurning;
        private bool _wasPoisoned;
        private bool _wasFrozen;
        private bool _wasStunned;

        /// <summary>Inject the four per-effect VFX pools. RunController calls this once
        /// per enemy after dequeue.</summary>
        public void Initialise(VfxPool burnPool, VfxPool poisonPool, VfxPool freezePool, VfxPool stunPool)
        {
            _burnPool = burnPool;
            _poisonPool = poisonPool;
            _freezePool = freezePool;
            _stunPool = stunPool;
            _enemy = GetComponent<Enemy>();
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_enemy == null) return;
            var state = StatusEffectApplier.GetOrCreateState(_enemy);

            // Burn — fire once on rising edge; the pool drains itself.
            if (state.IsBurning && !_wasBurning) _burnPool?.Play(transform.position);
            // Poison — same edge-triggered model.
            if (state.IsPoisoned && !_wasPoisoned) _poisonPool?.Play(transform.position);
            // Stun — short overhead pop.
            if (!state.CanAttack && !_wasStunned) _stunPool?.Play(transform.position);

            // Freeze — material-property tint while flag is held high.
            if (_targetRenderer != null && _mpb != null)
            {
                _targetRenderer.GetPropertyBlock(_mpb);
                _mpb.SetColor(FreezeTintProperty, state.IsFrozen ? FreezeColor : Color.white);
                _targetRenderer.SetPropertyBlock(_mpb);
            }
            if (state.IsFrozen && !_wasFrozen) _freezePool?.Play(transform.position);

            _wasBurning  = state.IsBurning;
            _wasPoisoned = state.IsPoisoned;
            _wasFrozen   = state.IsFrozen;
            _wasStunned  = !state.CanAttack;
        }
    }
}
