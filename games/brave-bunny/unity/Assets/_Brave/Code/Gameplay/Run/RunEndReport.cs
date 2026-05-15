// Brave Bunny — Gameplay/Run/RunEndReport
//
// Per-run totals banked at run-end and shown on the tally screen. Built by
// RunController.End() from running tallies tracked across the run. Raised on
// RunEndedChannel for UI consumption (Gameplay/Events/RunEndedChannel.cs).
//
// Design choices:
//   * Serializable so it can be JSON-encoded by the meta-progression
//     agent's save service without an extra DTO.
//   * Reference type (class) so the live "current" report on IRunRuntimeState
//     can be null before End() is called and non-null after — a readonly struct
//     would force a sentinel.
//   * weaponIdsUsed is a string[] of slugs (kebab-case) — designer-facing
//     identifiers from WeaponDefinition.slug. Empty when no weapons are equipped
//     (e.g. naked-run tests).
//   * Preserves the original readonly-struct field names (carrotsEarned,
//     enemiesKilled, runSeconds) as constructor parameters via the legacy
//     factory so any in-flight branch consuming the old shape still compiles.
//
// Spec refs:
//   * docs/02-gdd/01-core-loop.md § Run end resolutions.
//   * docs/05-wireframes/08-run-end-tally.html — visual fields consume these.
//   * docs/06-tech-spec/03-save-triggers.md — RunEnd is a save trigger.

#nullable enable

using System;
using UnityEngine;

namespace Brave.Gameplay.Run
{
    /// <summary>Why the run ended. String-typed for save-file forward-compat per ADR-0008.</summary>
    public static class RunEndCause
    {
        public const string HpZero       = "hp_zero";
        public const string BossDefeated = "boss_defeated";
        public const string WaveComplete = "wave_complete";
        public const string PlayerQuit   = "player_quit";
        public const string Timeout      = "timeout";
    }

    /// <summary>
    /// Per-run totals banked at run-end. Populated by <see cref="RunController.End(RunResult)"/>
    /// (and the <see cref="RunOutcome"/> overload) from running tallies kept across the
    /// run; raised on <c>RunEndedChannel</c> for UI + persisted by ISaveService at RunEnd
    /// state-entry (tech-spec 03 save triggers).
    /// </summary>
    [Serializable]
    public sealed class RunEndReport
    {
        // ---- Outcome ----

        /// <summary>End-state of the run (Win / Lose / Timeout / Quit).</summary>
        public RunOutcome outcome;

        /// <summary>Legacy alias used by DeathChannel + older save-file readers (Victory/Death/Quit).</summary>
        public RunResult result;

        /// <summary>String tag for cause-of-end. One of <see cref="RunEndCause"/> values.</summary>
        public string deathCause = string.Empty;

        // ---- Duration ----

        /// <summary>Seconds elapsed in the run (honors pause).</summary>
        public float runDurationSeconds;

        // ---- Combat tallies ----

        /// <summary>Total enemies killed this run (sum of swarmers + elites + bosses).</summary>
        public int totalKills;

        /// <summary>Subset of <see cref="totalKills"/> that were elite-flagged enemies.</summary>
        public int elitesKilled;

        /// <summary>Subset of <see cref="totalKills"/> that were boss-flagged enemies.</summary>
        public int bossesKilled;

        // ---- Wave progression ----

        /// <summary>Highest wave ordinal reached (1-based).</summary>
        public int wavesCleared;

        // ---- Progression ----

        /// <summary>Highest character level reached during the run.</summary>
        public int finalLevel;

        /// <summary>Total raw XP earned during the run (cumulative; never resets).</summary>
        public int xpGained;

        /// <summary>Total gold (carrots) earned during the run.</summary>
        public int goldGained;

        /// <summary>Soul shards earned (meta-progression currency).</summary>
        public int soulShardsEarned;

        /// <summary>Battle-pass XP earned this run.</summary>
        public int passXpEarned;

        // ---- Loadout snapshot ----

        /// <summary>Slugs of weapons equipped at run-end (kebab-case, designer-facing).</summary>
        public string[] weaponIdsUsed = Array.Empty<string>();

        /// <summary>Slug of the character used for this run (kebab-case).</summary>
        public string characterId = string.Empty;

        /// <summary>
        /// Parameterless constructor for designer-facing serialization. Use
        /// <see cref="FromLegacy"/> for the original tally signature.
        /// </summary>
        public RunEndReport() { }

        // ---- Legacy compatibility ----

        /// <summary>
        /// Build a report from the original Phase-5 readonly-struct field set
        /// (result / runSeconds / enemiesKilled / elitesKilled / bossesKilled /
        /// carrotsEarned / soulShardsEarned / passXpEarned / finalLevel). Lets any
        /// in-flight consumer keep its call-site untouched while migrating.
        /// </summary>
        public static RunEndReport FromLegacy(
            RunResult result, float runSeconds,
            int enemiesKilled, int elitesKilled, int bossesKilled,
            int carrotsEarned, int soulShardsEarned, int passXpEarned, int finalLevel)
        {
            var outcome = OutcomeFromResult(result);
            return new RunEndReport
            {
                result             = result,
                outcome            = outcome,
                deathCause         = DefaultCauseFor(outcome),
                runDurationSeconds = runSeconds,
                totalKills         = enemiesKilled,
                elitesKilled       = elitesKilled,
                bossesKilled       = bossesKilled,
                goldGained         = carrotsEarned,
                soulShardsEarned   = soulShardsEarned,
                passXpEarned       = passXpEarned,
                finalLevel         = finalLevel,
                xpGained           = 0,
                wavesCleared       = 0,
            };
        }

        // ---- Helpers ----

        /// <summary>Map the legacy <see cref="RunResult"/> tri-state to <see cref="RunOutcome"/>.</summary>
        public static RunOutcome OutcomeFromResult(RunResult r) => r switch
        {
            RunResult.Victory => RunOutcome.Win,
            RunResult.Death   => RunOutcome.Lose,
            RunResult.Quit    => RunOutcome.Quit,
            _                 => RunOutcome.Lose,
        };

        /// <summary>Map <see cref="RunOutcome"/> back to the legacy <see cref="RunResult"/> tri-state.</summary>
        public static RunResult ResultFromOutcome(RunOutcome o) => o switch
        {
            RunOutcome.Win     => RunResult.Victory,
            RunOutcome.Lose    => RunResult.Death,
            RunOutcome.Timeout => RunResult.Death,
            RunOutcome.Quit    => RunResult.Quit,
            _                  => RunResult.Death,
        };

        /// <summary>Pick the default <see cref="RunEndCause"/> for a given outcome.</summary>
        public static string DefaultCauseFor(RunOutcome o) => o switch
        {
            RunOutcome.Win     => RunEndCause.BossDefeated,
            RunOutcome.Lose    => RunEndCause.HpZero,
            RunOutcome.Timeout => RunEndCause.Timeout,
            RunOutcome.Quit    => RunEndCause.PlayerQuit,
            _                  => RunEndCause.HpZero,
        };
    }
}
