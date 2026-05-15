// Brave Bunny — Gameplay / Run
//
// Static cross-asmdef bridge for "run ended → meta systems" notifications.
// Brave.Gameplay cannot reference Brave.Systems (asmdef layering is one-way:
// Systems→Gameplay), so RunController publishes the populated RunEndReport on
// this static delegate and Systems-side listeners (BgmGameplayDriver,
// CharacterUnlockService) subscribe at scene-wire time.
//
// Rationale: RunEndedChannel (the SO event channel) is already used by UI
// (RunEndTallyController). Adding more raw subscribers to that channel would
// couple Systems-side services to the SO asset lifecycle. The static bridge
// keeps the Systems wiring purely code-side and lets the SO channel stay a
// UI-only signal.
//
// Companion: WeaponFireBridge.cs — same pattern, different signal.

#nullable enable

using System;
using UnityEngine;

namespace Brave.Gameplay.Run
{
    /// <summary>
    /// Static cross-asmdef bridge: RunController publishes the run-end report,
    /// Systems-side meta services (BGM driver, character unlocks) subscribe.
    /// See file header for rationale.
    /// </summary>
    public static class RunEndIntegrationBridge
    {
        /// <summary>
        /// Raised once per run-end after the channel-based <c>RunEndedChannel</c>
        /// fires. Carries the populated <see cref="RunEndReport"/> so subscribers
        /// can derive outcome, character slug, boss kills, etc.
        /// </summary>
        public static event Action<RunEndReport>? RunEnded;

        /// <summary>
        /// Publish a run-end pulse. Safe to call with no listeners (no-op).
        /// Exceptions thrown by individual subscribers are swallowed and
        /// logged so a misbehaving listener cannot break the run-end flow.
        /// </summary>
        public static void Notify(RunEndReport report)
        {
            if (report == null) return;
            var handler = RunEnded;
            if (handler == null) return;
            foreach (var d in handler.GetInvocationList())
            {
                try { ((Action<RunEndReport>)d)(report); }
                catch (Exception e)
                {
                    Debug.LogError($"[RunEndIntegrationBridge] subscriber threw: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Test-only reset hook; clears subscribers. Public (rather than internal)
        /// because the EditMode test asmdef does not declare InternalsVisibleTo on
        /// Brave.Gameplay (project-wide convention — see MechanicRegistryTests).
        /// </summary>
        public static void ResetForTests() => RunEnded = null;
    }
}
