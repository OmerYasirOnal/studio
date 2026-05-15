// Brave Bunny — Systems / Stats / LifetimeStatsService (Wave 10).
//
// Tallies the lifetime fields surfaced on the Player Profile screen:
//   * TotalRuns               — RunEnded
//   * TotalKills              — RunEnded.totalKills (banked at run-end)
//   * BestRunTimeSeconds      — RunEnded.runDurationSeconds (Win outcome only)
//   * BestWaveReached         — RunEnded.wavesCleared
//   * BossesDefeated          — BossDefeatedChannel (one tick per boss)
//   * EvolutionsTriggered     — WeaponEvolvedChannel (one tick per evolution)
//   * TotalPlaytimeSeconds    — accumulated via Time.realtimeSinceStartup delta
//                               between Begin() and RunEnded (banked at run-end).
//
// Pattern mirrors TelemetryEventBridge.cs (Systems/Telemetry) — a thin
// subscriber wired up by GameContextBootstrap with [SerializeField] channel
// references. Idempotent Subscribe/Unsubscribe. Pure logic class
// (LifetimeStatsLogic) is exposed so EditMode tests can drive it without
// touching ScriptableObjects.
//
// Spec refs:
//   * docs/06-tech-spec/03-save-system.md § stats payload schema.
//   * docs/06-tech-spec/09-event-bus.md § Tier-3 SO event channel pattern.
//   * docs/handoffs/wave10-loc-keys-needed.md (profile.* loc keys).

#nullable enable

using System;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using Brave.Systems.Context;
using Brave.Systems.Save;
using UnityEngine;

namespace Brave.Systems.Stats
{
    /// <summary>Public service contract — registered against GameContext.</summary>
    public interface ILifetimeStatsService : IService
    {
        /// <summary>Live snapshot — same reference as SaveData.Stats.</summary>
        SaveData.StatsSection Stats { get; }

        /// <summary>Call when a run starts; latches the realtime baseline for playtime tally.</summary>
        void NotifyRunStarted();
    }

    /// <summary>
    /// Pure-C# state machine: takes events, mutates a StatsSection, returns
    /// whether anything changed. No UnityEngine dependency — exercised by
    /// EditMode tests with hand-rolled events.
    /// </summary>
    public static class LifetimeStatsLogic
    {
        /// <summary>Apply a RunEnded payload to the tally section. Returns true iff anything changed.</summary>
        public static bool ApplyRunEnded(SaveData.StatsSection stats, RunEndReport? report,
            double playtimeDeltaSeconds)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            // Even a null report still counts the run + folds in playtime.
            var changed = false;

            stats.TotalRuns += 1;
            changed = true;

            if (playtimeDeltaSeconds > 0)
            {
                stats.TotalPlaytimeSeconds += playtimeDeltaSeconds;
            }

            if (report == null) return changed;

            if (report.totalKills > 0)
            {
                stats.TotalKills += report.totalKills;
            }
            if (report.wavesCleared > stats.BestWaveReached)
            {
                stats.BestWaveReached = report.wavesCleared;
            }
            // Best-run-time only counts Win outcomes — losses dominate the
            // distribution and would otherwise pin BestRunTimeSeconds to ~0.
            if (report.outcome == RunOutcome.Win
                && report.runDurationSeconds > stats.BestRunTimeSeconds)
            {
                stats.BestRunTimeSeconds = report.runDurationSeconds;
            }
            return changed;
        }

        /// <summary>Bump the bosses-defeated tally by one. Returns whether anything changed.</summary>
        public static bool ApplyBossDefeated(SaveData.StatsSection stats)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            stats.BossesDefeated += 1;
            return true;
        }

        /// <summary>Bump the evolutions-triggered tally by one. Returns whether anything changed.</summary>
        public static bool ApplyWeaponEvolved(SaveData.StatsSection stats)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            stats.EvolutionsTriggered += 1;
            return true;
        }
    }

    /// <summary>
    /// MonoBehaviour shell — subscribes to the three SO channels and folds
    /// each event into SaveData.Stats via <see cref="LifetimeStatsLogic"/>.
    /// Owns the playtime accumulator (Time.realtimeSinceStartup deltas).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LifetimeStatsService : MonoBehaviour, ILifetimeStatsService
    {
        // ---- Serialized channel references (wired from Boot.unity SO assets) ----
        [Header("Gameplay event channels")]
        [SerializeField] private RunEndedChannel? _runEndedChannel;
        [SerializeField] private BossDefeatedChannel? _bossDefeatedChannel;
        [SerializeField] private WeaponEvolvedChannel? _weaponEvolvedChannel;

        private ISaveService? _save;
        private bool _subscribed;

        // Playtime accumulator: latched at run-start (NotifyRunStarted), folded
        // in at run-end. Sentinel < 0 means "not running" — guards against
        // double-counting if RunEnded fires twice in a row.
        private const double NotRunning = -1.0;
        private double _runStartRealtime = NotRunning;

        // Time.realtimeSinceStartup is a Unity-engine accessor; the static
        // delegate lets EditMode tests substitute a virtual clock.
        public static Func<double> NowSeconds { get; set; } = DefaultNow;

        private static double DefaultNow() => Time.realtimeSinceStartup;

        public SaveData.StatsSection Stats =>
            _save?.Data.Stats ?? throw new InvalidOperationException(
                "LifetimeStatsService.Stats accessed before SaveService injection.");

        /// <summary>Test seam — wire SaveService + (optionally) channels without an inspector.</summary>
        public void ConfigureForTests(
            ISaveService save,
            RunEndedChannel? runEnded = null,
            BossDefeatedChannel? bossDefeated = null,
            WeaponEvolvedChannel? weaponEvolved = null)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _runEndedChannel = runEnded;
            _bossDefeatedChannel = bossDefeated;
            _weaponEvolvedChannel = weaponEvolved;
        }

        /// <summary>Bind a SaveService that was constructed outside the Unity lifecycle (e.g. CI tests).</summary>
        public void SetSaveService(ISaveService save) =>
            _save = save ?? throw new ArgumentNullException(nameof(save));

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        /// <summary>Idempotent — safe to call repeatedly.</summary>
        public void Subscribe()
        {
            if (_subscribed) return;
            if (_save == null)
            {
                var ctx = GameContextBootstrap.Context;
                if (ctx != null && ctx.TryGet<ISaveService>(out var resolved)) _save = resolved;
            }

            if (_runEndedChannel != null) _runEndedChannel.Subscribe(HandleRunEnded);
            if (_bossDefeatedChannel != null) _bossDefeatedChannel.Subscribe(HandleBossDefeated);
            if (_weaponEvolvedChannel != null) _weaponEvolvedChannel.Subscribe(HandleWeaponEvolved);

            _subscribed = true;
        }

        /// <summary>Mirror of <see cref="Subscribe"/>.</summary>
        public void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_runEndedChannel != null) _runEndedChannel.Unsubscribe(HandleRunEnded);
            if (_bossDefeatedChannel != null) _bossDefeatedChannel.Unsubscribe(HandleBossDefeated);
            if (_weaponEvolvedChannel != null) _weaponEvolvedChannel.Unsubscribe(HandleWeaponEvolved);
            _subscribed = false;
        }

        /// <summary>Call when a run begins; latches the realtime baseline for playtime accounting.</summary>
        public void NotifyRunStarted()
        {
            _runStartRealtime = NowSeconds();
        }

        // ---- Channel handlers (internal so tests can drive them directly) ----

        internal void HandleRunEnded(RunEndedEvent evt) => HandleRunEndedReport(evt.report);

        /// <summary>
        /// Public test seam: tests pass the report directly without constructing
        /// the event channel. Production code reaches this via <see cref="HandleRunEnded"/>.
        /// </summary>
        public void HandleRunEndedReport(RunEndReport? report)
        {
            if (_save == null) return;
            var delta = ComputePlaytimeDelta();
            var changed = LifetimeStatsLogic.ApplyRunEnded(_save.Data.Stats, report, delta);
            if (changed) _save.Save();
        }

        internal void HandleBossDefeated(BossDefeatedEvent _) => HandleBossDefeatedTick();

        /// <summary>Public test seam: equivalent to firing the channel once.</summary>
        public void HandleBossDefeatedTick()
        {
            if (_save == null) return;
            var changed = LifetimeStatsLogic.ApplyBossDefeated(_save.Data.Stats);
            if (changed) _save.Save();
        }

        internal void HandleWeaponEvolved(WeaponEvolvedEvent _) => HandleWeaponEvolvedTick();

        /// <summary>Public test seam: equivalent to firing the channel once.</summary>
        public void HandleWeaponEvolvedTick()
        {
            if (_save == null) return;
            var changed = LifetimeStatsLogic.ApplyWeaponEvolved(_save.Data.Stats);
            if (changed) _save.Save();
        }

        // ---- helpers ----

        private double ComputePlaytimeDelta()
        {
            if (_runStartRealtime <= NotRunning) return 0;
            var delta = NowSeconds() - _runStartRealtime;
            _runStartRealtime = NotRunning;
            return delta > 0 ? delta : 0;
        }

        // ---- Test diagnostics ----

        internal bool IsSubscribedForTests => _subscribed;
        internal bool IsAccumulatingForTests => _runStartRealtime > NotRunning;
    }
}
