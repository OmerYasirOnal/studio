// Brave Bunny — Gameplay/Events/RunEndedEvent + RunEndedChannel
//
// Tech-spec 09 § Tier 3: typed ScriptableObject event channel for cross-asmdef
// loose coupling. Fired by RunController.End() once the report is populated.
//
// Subscribers:
//   * UI/Controllers/RunEndTallyController (Brave.UI) — renders the tally view.
//   * Systems/Save/SaveService (Brave.Systems) — persists meta-progression deltas.
//
// Both subscribers also read the live report from IRunRuntimeState.CurrentRunEndReport
// to avoid event-vs-state race conditions in the editor.
//
// Payload note: RunEndReport is a class (not a readonly struct) — see RunEndReport.cs
// header for the rationale. The channel passes a reference; subscribers must treat it
// as read-only.

#nullable enable

using Brave.Gameplay.Run;
using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Payload broadcast on <see cref="RunEndedChannel"/> when a run terminates. Wraps the
    /// populated <see cref="RunEndReport"/> for subscribers.
    /// </summary>
    public readonly struct RunEndedEvent
    {
        /// <summary>The populated end-of-run report. Never null when raised by RunController.</summary>
        public readonly RunEndReport report;

        public RunEndedEvent(RunEndReport report)
        {
            this.report = report;
        }
    }

    /// <summary>SO channel asset — designers wire this into RunController + RunEndTallyController.</summary>
    [CreateAssetMenu(menuName = "Brave/Events/RunEnded", fileName = "RunEndedChannel", order = 5)]
    public sealed class RunEndedChannel : EventChannel<RunEndedEvent> { }
}
