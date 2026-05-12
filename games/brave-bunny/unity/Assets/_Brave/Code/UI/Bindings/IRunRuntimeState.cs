// Brave Bunny — UI / Bindings / IRunRuntimeState
//
// Read-only contract the Run-HUD consumes each frame. gameplay-engineer will
// implement this against the canonical RunService (HP/XP/wave/timer/boss
// telegraph) in a future dispatch. The UI side ships first so the HUD is
// developable + previewable + testable in editor without the runtime.
//
// Contract notes:
//   * 8 properties — no methods, no events. The UI polls each frame.
//   * Allocation-free getters. Concrete impls must not allocate per read.
//   * UI handles the "null state" case (placeholder values for editor preview)
//     so production code may swap in a real impl on Boot without UI refactors.
//
// Cross-refs:
//   * docs/05-wireframes/05-run-hud.html — visual elements that consume these.
//   * docs/06-tech-spec/05-performance.md — 60 fps cap; UI may not allocate.
//   * docs/02-gdd/07-progression.md — XP/level definitions.
//   * Wave-5 ui-engineer dispatch — this file is the contract.

#nullable enable

namespace Brave.UI.Bindings
{
    /// <summary>
    /// Read-only snapshot of the live run state, polled per-frame by
    /// <see cref="Brave.UI.Controllers.RunHudController"/>. Implementations
    /// must be allocation-free per read.
    /// </summary>
    public interface IRunRuntimeState
    {
        /// <summary>Player hit points remaining this run (0..MaxHP).</summary>
        float CurrentHP { get; }

        /// <summary>Player max hit points (post-passives, post-buffs).</summary>
        float MaxHP { get; }

        /// <summary>XP accumulated *within the current level* (resets on level-up).</summary>
        float CurrentXP { get; }

        /// <summary>XP threshold to reach <c>Level + 1</c> from the current level.</summary>
        float XPToNextLevel { get; }

        /// <summary>Player level (1-based; starts at 1).</summary>
        int Level { get; }

        /// <summary>Active wave ordinal (1-based; matches <c>waves.json</c>).</summary>
        int WaveNumber { get; }

        /// <summary>Seconds elapsed in this run (paused state freezes via Time.timeScale upstream).</summary>
        float RunSecondsElapsed { get; }

        /// <summary>True while a boss is on-screen or being telegraphed; drives the warning banner.</summary>
        bool IsBossActive { get; }
    }
}
