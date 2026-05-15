// Brave Bunny — UI / Bindings / IRunRuntimeState
//
// Canonical single interface consumed by RunHudController (polling + event-driven).
// gameplay-engineer implements this on RunController so the HUD works with live data
// without knowing MonoBehaviour internals.
//
// Reconciliation note (ADR-0021):
//   Wave-5 added the polling contract (8 properties, no events).
//   Wave-6 hud-wire added an event-driven contract (StateChanged + 6 fields) in a
//   separate namespace. Those two are merged here — one interface, one namespace.
//   The duplicate Gameplay/Run/IRunRuntimeState.cs was NOT merged; it never existed
//   on main. RunHudController.BindState() is the preferred wiring path; per-frame
//   Update() polling is kept as a fallback for scenes without a bound state.
//
// Contract notes:
//   * Allocation-free getters. Concrete impls must not allocate per read.
//   * StateChanged is a single coarse-grained event — HUD redraws the entire view.
//     Per-field granularity is not needed given the HUD Update budget (< 0.1 ms).
//   * UI handles the "null state" case via RunHudStubRuntime so production code
//     may swap in a real impl on Boot without UI refactors.
//
// Cross-refs:
//   * docs/05-wireframes/05-run-hud.html — visual elements that consume these.
//   * docs/06-tech-spec/05-performance.md — 60 fps cap; UI may not allocate.
//   * docs/02-gdd/07-progression.md — XP/level definitions.
//   * docs/decisions/ADR-0021-hud-binding-contract.md — design decision record.

#nullable enable

using System;
using Brave.Gameplay.Run;

namespace Brave.UI.Bindings
{
    /// <summary>
    /// Read-only snapshot of the live run state. Consumed by
    /// <see cref="Brave.UI.Controllers.RunHudController"/> both via per-frame polling
    /// and the event-driven <see cref="StateChanged"/> path. Implementations must be
    /// allocation-free per read.
    /// </summary>
    public interface IRunRuntimeState
    {
        // ---- HP ----

        /// <summary>Player hit points remaining this run (0..MaxHP).</summary>
        float CurrentHP { get; }

        /// <summary>Player max hit points (post-passives, post-buffs).</summary>
        float MaxHP { get; }

        /// <summary>Current HP as a 0-1 fraction of max HP. Convenience for bar fills.</summary>
        float CurrentHpNormalized { get; }

        // ---- XP / level ----

        /// <summary>XP accumulated *within the current level* (resets on level-up).</summary>
        float CurrentXP { get; }

        /// <summary>XP threshold to reach <c>Level + 1</c> from the current level.</summary>
        float XPToNextLevel { get; }

        /// <summary>Total XP earned this run (raw cumulative points, never resets).</summary>
        int XpPoints { get; }

        /// <summary>Player level (1-based; starts at 1).</summary>
        int Level { get; }

        // ---- Wave / run progression ----

        /// <summary>Active wave ordinal (1-based; matches <c>waves.json</c>). Alias: <see cref="WaveNumber"/>.</summary>
        int WaveNumber { get; }

        /// <summary>Seconds elapsed in this run (honours pause). Alias: <see cref="RunSecondsElapsed"/>.</summary>
        float RunSecondsElapsed { get; }

        /// <summary>True while a boss is on-screen or being telegraphed; drives the warning banner.</summary>
        bool IsBossActive { get; }

        // ---- Kill tracking ----

        /// <summary>Total enemies killed this run.</summary>
        int KillCount { get; }

        // ---- Pause state ----

        /// <summary>True while the run is in the Paused sub-state.</summary>
        bool Paused { get; }

        // ---- Run-end report ----

        /// <summary>
        /// The populated run-end report, or <c>null</c> while the run is in progress.
        /// Set in <c>RunController.End()</c> immediately before <c>RunEndedChannel</c> is
        /// raised, so UI controllers may read it either via subscription to the channel or
        /// by polling after the run-end state transition. Allocation-free read.
        ///
        /// Default-implemented as <c>null</c> so stub/test fakes that pre-date the
        /// run-end-report pipeline keep compiling. Real impls (e.g. <c>RunController</c>)
        /// override this.
        /// </summary>
        RunEndReport? CurrentRunEndReport => null;

        // ---- Event-driven binding ----

        /// <summary>
        /// Raised whenever any field above changes. HUD subscribers redraw the full
        /// view on this signal. Implementations raise this in mutators (SetHp, AddXp,
        /// SetWave, RecordKill, Pause/Resume).
        /// </summary>
        event Action StateChanged;
    }
}
