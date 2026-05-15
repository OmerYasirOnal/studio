#nullable enable
// Wave 10: status effects (Slow, Burn, Poison, Freeze, Stun).
//
// Polymorphic base per ADR-0009: each concrete subclass carries a [BraveRegister]
// type-name so MechanicRegistry can roundtrip status-effect state through save
// (ADR-0008 future hook). For now the registry-discovery side is a TODO — the
// type-name is wired up so the round-trip lands when save-persistence ships.
//
// Allocation-free per-tick path: effects are pooled by StatusEffectApplier and
// Acquire/Release reset their state. No per-tick LINQ, boxing, or hash lookups.
// Magnitude/duration values are passed by the caller (AuraWeapon reads from
// AuraArchetypeConfig.slowPctBase, etc.) — never inlined here.

using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>
    /// Abstract base for a per-enemy status effect (Slow/Burn/Poison/Freeze/Stun).
    /// Subclasses implement <see cref="OnApply"/>, <see cref="OnTick"/>, and
    /// <see cref="OnExpire"/>; the <see cref="StatusEffectApplier"/> drives the
    /// lifecycle and reuses instances via a pool.
    /// </summary>
    public abstract class StatusEffect
    {
        /// <summary>Total duration of this effect in milliseconds. Set via <see cref="Configure"/>.</summary>
        public int durationMs;

        /// <summary>Effect-specific magnitude (slow pct, dmg-per-tick, etc.). Set via <see cref="Configure"/>.</summary>
        public float magnitude;

        /// <summary>Tick interval in milliseconds for DoT effects (0 = no per-tick callback).</summary>
        public int tickIntervalMs;

        /// <summary>Remaining time in milliseconds. Drains down to 0 then the effect expires.</summary>
        public int remainingMs;

        /// <summary>Time accumulated since the last tick fired, in milliseconds.</summary>
        public int sinceLastTickMs;

        /// <summary>Stable string identifier used by the applier to detect stacking/refresh.
        /// Concrete subclasses return a constant (e.g. "status.slow"). Aligned with ADR-0009
        /// [BraveRegister] type-names so future save round-trip uses the same token.</summary>
        public abstract string TypeName { get; }

        /// <summary>Hot configure — called by the pool on acquire (or by tests directly).</summary>
        public void Configure(int durationMs, float magnitude, int tickIntervalMs = 0)
        {
            this.durationMs = durationMs;
            this.magnitude = magnitude;
            this.tickIntervalMs = tickIntervalMs;
            this.remainingMs = durationMs;
            this.sinceLastTickMs = 0;
        }

        /// <summary>Pool reset — clears mutable state without resizing buffers.</summary>
        public virtual void Reset()
        {
            durationMs = 0;
            magnitude = 0f;
            tickIntervalMs = 0;
            remainingMs = 0;
            sinceLastTickMs = 0;
        }

        /// <summary>Refresh remaining duration when the same effect is re-applied to the
        /// same enemy. Default policy:
        ///   * Take the larger of (current remaining, new duration) — never shorten an
        ///     existing effect.
        ///   * Magnitude UPGRADES only (max of old/new). Magnitude is recorded on the
        ///     effect so OnExpire restores the correct delta; subclasses that mutate
        ///     per-enemy state on refresh must override and re-apply explicitly via
        ///     the <paramref name="enemy"/> reference.
        /// Subclasses with additive stacking semantics override this entirely.</summary>
        public virtual void Refresh(Enemy enemy, int newDurationMs, float newMagnitude)
        {
            if (newDurationMs > remainingMs) remainingMs = newDurationMs;
            if (newMagnitude > magnitude) magnitude = newMagnitude;
            durationMs = remainingMs;
            // Do NOT reset sinceLastTickMs — let in-flight DoT keep their cadence.
        }

        /// <summary>Fired once when the effect is first applied to <paramref name="enemy"/>.
        /// Used to install long-lived modifiers (speed multiplier, attack-block flag,
        /// material tint hook). Allocation-free.</summary>
        public abstract void OnApply(Enemy enemy);

        /// <summary>Fired per frame while the effect is active. <paramref name="dtSeconds"/>
        /// is the frame delta. Subclasses that don't need a per-frame hook may no-op;
        /// DoT effects accumulate damage via <see cref="sinceLastTickMs"/>.</summary>
        public abstract void OnTick(Enemy enemy, float dtSeconds);

        /// <summary>Fired once when the effect expires (duration ≤ 0) or is removed early.
        /// Subclasses must restore any modifier they installed in <see cref="OnApply"/>.</summary>
        public abstract void OnExpire(Enemy enemy);
    }
}
