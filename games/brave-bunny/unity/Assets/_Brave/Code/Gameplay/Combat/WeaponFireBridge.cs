// Brave Bunny — Gameplay / Combat
//
// Static cross-asmdef bridge for "weapon fired" notifications. Brave.Gameplay
// cannot reference Brave.Systems (the asmdef layering is one-way:
// Systems→Gameplay), so AutoAttackController publishes weapon-fire pulses on
// this static delegate and any Systems-side listener (e.g.
// Brave.Systems.Audio.GameplayAudioBindings) subscribes at scene-wire time.
//
// Design:
//   * Pure C# static — no MonoBehaviour, no allocations, no SO asset.
//   * Single multicast delegate; listeners are responsible for un-subscribing on
//     teardown (RunSceneWiring does this in OnDestroy).
//   * Never throws on a null listener — try/catch wrapped per-invocation so a
//     single bad subscriber can't break the firing loop.
//
// Follow-up: when a dedicated WeaponFireChannel SO ScriptableObject is added
// (see GameplayAudioBindings.cs §"TODO follow-up"), AutoAttackController will
// raise it instead and this bridge can be deleted.

#nullable enable

using System;
using UnityEngine;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Static cross-asmdef bridge: AutoAttackController publishes, Systems
    /// (audio bindings) subscribes. See file header for rationale.
    /// </summary>
    public static class WeaponFireBridge
    {
        /// <summary>
        /// Raised every time a weapon fires. <c>archetype</c> is the lowercase
        /// archetype slug ("projectile" / "area" / "aura" / "summon" / "utility");
        /// <c>worldPosition</c> is the firing origin.
        /// </summary>
        public static event Action<string, Vector3>? Fired;

        /// <summary>
        /// Publish a weapon-fire pulse. Safe to call when no listeners are bound
        /// (no-op). Exceptions thrown by individual subscribers are swallowed and
        /// logged so a misbehaving listener cannot break the firing loop.
        /// </summary>
        public static void Notify(string archetype, Vector3 worldPosition)
        {
            var handler = Fired;
            if (handler == null) return;
            foreach (var d in handler.GetInvocationList())
            {
                try { ((Action<string, Vector3>)d)(archetype, worldPosition); }
                catch (Exception e)
                {
                    Debug.LogError($"[WeaponFireBridge] subscriber threw: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Test-only reset hook used by EditMode tests; clears subscribers. Public
        /// (rather than internal) because the EditMode test asmdef does not declare
        /// InternalsVisibleTo on Brave.Gameplay (project-wide convention — see
        /// MechanicRegistryTests).
        /// </summary>
        public static void ResetForTests() => Fired = null;
    }
}
