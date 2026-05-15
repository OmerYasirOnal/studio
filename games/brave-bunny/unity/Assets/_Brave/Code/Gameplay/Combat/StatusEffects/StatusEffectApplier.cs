#nullable enable
// Wave 10: StatusEffectApplier — service that manages per-enemy active status effects.
//
// Design constraints (CLAUDE.md principle 4 + tech-spec 05):
//   * Allocation-free per-tick path: effect instances are reused via a per-type pool;
//     the per-enemy active list is a pre-allocated array (capped at MaxEffectsPerEnemy);
//     no LINQ, no boxing, no per-frame List<T>.
//   * Owned by a single Update tick (driven by RunController; tests drive Tick(dt) directly).
//
// Stacking policy:
//   * Same TypeName re-applied to the same enemy → Refresh(): take max(remaining, new duration).
//   * Different TypeName effects on the same enemy → stack independently. The state-flags
//     model means e.g. Burn + Slow coexist with no conflict.
//
// The applier is the SINGLE OWNER of per-enemy state (StatusEffectState). Effects mutate
// state via this class, never touching Enemy fields directly.

using System.Collections.Generic;

using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Combat.StatusEffects
{
    /// <summary>Per-enemy state owned by the applier. Holds the aggregate of all active
    /// status modifiers (speed multiplier, attack-block flag, frozen/burning visuals).
    /// Concrete fields are mutated by effect <c>OnApply</c>/<c>OnExpire</c> hooks.</summary>
    public sealed class StatusEffectState
    {
        public float SpeedMultiplier { get; private set; } = 1f;
        public bool CanAttack { get; private set; } = true;
        public bool IsFrozen { get; private set; }
        public bool IsBurning { get; private set; }
        public bool IsPoisoned { get; private set; }
        public float AccumulatedDoTDamage { get; private set; }

        /// <summary>Multiply the current speed multiplier by <paramref name="factor"/>.
        /// Used by Slow which is multiplicative against any other active slow.</summary>
        public void MultiplySpeed(float factor) => SpeedMultiplier *= factor;

        /// <summary>Divide the current speed multiplier by <paramref name="factor"/> — used
        /// to undo a <see cref="MultiplySpeed"/> on expire.</summary>
        public void DivideSpeed(float factor)
        {
            if (factor <= 0f) factor = 0.0001f;
            SpeedMultiplier /= factor;
        }

        /// <summary>Hard-set the multiplier — used by Freeze (sets to 0 then restores).</summary>
        public void SetSpeedMultiplier(float value) => SpeedMultiplier = value;

        public void SetCanAttack(bool value) => CanAttack = value;
        public void MarkFrozen(bool value) => IsFrozen = value;
        public void MarkBurning(bool value) => IsBurning = value;
        public void MarkPoisoned(bool value) => IsPoisoned = value;

        public void RecordDamageTick(float damage) => AccumulatedDoTDamage += damage;

        /// <summary>Reset to default state — called when the enemy returns to its pool.</summary>
        public void Reset()
        {
            SpeedMultiplier = 1f;
            CanAttack = true;
            IsFrozen = false;
            IsBurning = false;
            IsPoisoned = false;
            AccumulatedDoTDamage = 0f;
        }
    }

    /// <summary>Service that owns active <see cref="StatusEffect"/>s per enemy and ticks
    /// them each frame. Single instance per Run (wired by RunController; tests construct
    /// directly).</summary>
    public sealed class StatusEffectApplier
    {
        /// <summary>Per-enemy hard cap on simultaneously-active effects. Sized for the
        /// worst observed case (5 distinct status types) plus a small safety margin.</summary>
        public const int MaxEffectsPerEnemy = 8;

        // Active effects: enemy → ring of effect instances. Plain Dictionary is fine —
        // adds/removes happen at apply/expire boundaries, not per tick.
        private readonly Dictionary<Enemy, List<StatusEffect>> _active = new(64);

        // Per-enemy state — owned here so callers (Visuals, behavior) can query
        // multipliers without touching the effect list.
        private static readonly Dictionary<Enemy, StatusEffectState> _state = new(64);

        // Test seam: lets EditMode tests assert / reset the static state map.
        // Public because the EditMode test asmdef does not declare InternalsVisibleTo
        // on Brave.Gameplay — project-wide convention (see WeaponFireBridge.cs).
        public static IReadOnlyDictionary<Enemy, StatusEffectState> StatesForTests => _state;

        /// <summary>Look up (or lazily create) the per-enemy state record. Effects call this
        /// from their <c>OnApply</c>/<c>OnExpire</c> hooks; visuals call it each frame.</summary>
        public static StatusEffectState GetOrCreateState(Enemy enemy)
        {
            if (enemy == null)
                return new StatusEffectState(); // detached scratch state — never indexed
            if (_state.TryGetValue(enemy, out var s)) return s;
            s = new StatusEffectState();
            _state[enemy] = s;
            return s;
        }

        /// <summary>Apply an effect to an enemy. If an effect with the same
        /// <see cref="StatusEffect.TypeName"/> is already active, the existing instance is
        /// refreshed; otherwise the new instance is appended.</summary>
        public void Apply(Enemy enemy, StatusEffect effect)
        {
            if (enemy == null || effect == null) return;

            if (!_active.TryGetValue(enemy, out var list))
            {
                list = new List<StatusEffect>(capacity: MaxEffectsPerEnemy);
                _active[enemy] = list;
            }

            // Same-type refresh: do not duplicate, do not re-fire OnApply.
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TypeName == effect.TypeName)
                {
                    list[i].Refresh(enemy, effect.durationMs, effect.magnitude);
                    return;
                }
            }

            if (list.Count >= MaxEffectsPerEnemy)
            {
                // Soft cap: drop the new effect rather than evict an existing one.
                // Logs once per overflow to surface tuning issues.
                Debug.LogWarning(
                    $"StatusEffectApplier: enemy '{enemy.name}' at MaxEffectsPerEnemy " +
                    $"({MaxEffectsPerEnemy}); dropping new effect '{effect.TypeName}'.");
                return;
            }

            list.Add(effect);
            effect.OnApply(enemy);
        }

        /// <summary>Advance all active effects by <paramref name="dtSeconds"/>. Effects that
        /// have expired fire <see cref="StatusEffect.OnExpire"/> and are removed.</summary>
        public void Tick(float dtSeconds)
        {
            // dt → ms once, integer-stepping the timers keeps refresh/stack math integer-pure.
            int dtMs = (int)(dtSeconds * 1000f);
            if (dtMs < 0) dtMs = 0;

            foreach (var kv in _active)
            {
                var enemy = kv.Key;
                var list = kv.Value;
                if (enemy == null || !enemy.IsAlive)
                {
                    // Enemy died/despawned — expire all effects on it.
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        list[i].OnExpire(enemy!);
                        list.RemoveAt(i);
                    }
                    continue;
                }

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var effect = list[i];
                    effect.remainingMs -= dtMs;

                    // Tick-based DoT: fire ApplyTickDamage on Burn/Poison every tickIntervalMs.
                    if (effect.tickIntervalMs > 0)
                    {
                        effect.sinceLastTickMs += dtMs;
                        while (effect.sinceLastTickMs >= effect.tickIntervalMs)
                        {
                            effect.sinceLastTickMs -= effect.tickIntervalMs;
                            if (effect is BurnEffect burn) burn.ApplyTickDamage(enemy);
                        }
                    }

                    // Per-frame hook for non-DoT effects (currently no-ops in concrete classes).
                    effect.OnTick(enemy, dtSeconds);

                    if (effect.remainingMs <= 0)
                    {
                        effect.OnExpire(enemy);
                        list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>How many active effects of any type are currently on <paramref name="enemy"/>.</summary>
        public int ActiveCount(Enemy enemy)
        {
            if (enemy == null) return 0;
            return _active.TryGetValue(enemy, out var list) ? list.Count : 0;
        }

        /// <summary>True if an effect with the given type-name is currently active.</summary>
        public bool HasEffect(Enemy enemy, string typeName)
        {
            if (enemy == null || !_active.TryGetValue(enemy, out var list)) return false;
            for (int i = 0; i < list.Count; i++)
                if (list[i].TypeName == typeName) return true;
            return false;
        }

        /// <summary>Find the first active effect with the given type-name on
        /// <paramref name="enemy"/>, or <c>null</c>. Used by tests.</summary>
        public StatusEffect? FindEffect(Enemy enemy, string typeName)
        {
            if (enemy == null || !_active.TryGetValue(enemy, out var list)) return null;
            for (int i = 0; i < list.Count; i++)
                if (list[i].TypeName == typeName) return list[i];
            return null;
        }

        /// <summary>Clear all effects on every enemy. Used at run-end and by tests.</summary>
        public void Clear()
        {
            foreach (var kv in _active)
            {
                var enemy = kv.Key;
                var list = kv.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    list[i].OnExpire(enemy);
                    list.RemoveAt(i);
                }
            }
            _active.Clear();
        }

        /// <summary>Test seam — reset the static per-enemy state map. NOT for runtime use.
        /// Public because the EditMode test asmdef is a separate assembly.</summary>
        public static void ResetAllStateForTests()
        {
            _state.Clear();
        }
    }
}
