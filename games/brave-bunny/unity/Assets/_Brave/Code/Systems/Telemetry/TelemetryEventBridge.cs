// Brave Bunny — Systems / Telemetry
//
// Subscribes to gameplay event channels and translates each into a
// TelemetryEvent for LocalTelemetryService. Mirrors the GameplayAudioBindings
// pattern (Brave.Systems.Audio) — MonoBehaviour with [SerializeField] channel
// refs, lives in the Run scene, idempotent Subscribe/Unsubscribe.
//
// Cross-refs:
//   * Brave.Systems.Telemetry.LocalTelemetryService — sink.
//   * Brave.Gameplay.Events.{RunEndedChannel,LevelUpChannel,EnemyKilledChannel,
//                            BossDefeatedChannel,DeathChannel}.
//   * docs/06-tech-spec/09-event-bus.md — Tier-3 SO event channel pattern.
//
// Scope:
//   * Captures the five soft-launch retention signals: run_start, run_end,
//     level_up, boss_kill, death — plus app_pause / app_quit so the day's
//     buffer is always flushed before the OS reclaims the process.
//   * Purchase events are NOT routed through here — Brave.Systems.Iap calls
//     LocalTelemetryService.Log(...) directly with the sku + price. This bridge
//     stays focused on Gameplay-side channels (Iap is in Systems and can hold
//     its own ILocalTelemetryService reference without crossing asmdef edges).
//
// Run-start emission:
//   * No dedicated RunStartedChannel exists yet. We expose NotifyRunStarted()
//     so RunController.Begin() can call it directly until a channel is added.
//     Documented as a TODO + the same seam pattern GameplayAudioBindings uses.

#nullable enable

using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using Brave.Systems.Context;
using UnityEngine;

namespace Brave.Systems.Telemetry
{
    /// <summary>
    /// Translates gameplay events into TelemetryEvents and forwards them to
    /// <see cref="ILocalTelemetryService"/>. Wired in the Run scene; survives
    /// scene unload via <see cref="OnDisable"/> unsubscribe.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TelemetryEventBridge : MonoBehaviour, IService
    {
        // ---- Serialized channel references (wired from Run scene SO assets) ----
        [Header("Gameplay event channels")]
        [SerializeField] private RunEndedChannel? _runEndedChannel;
        [SerializeField] private LevelUpChannel? _levelUpChannel;
        [SerializeField] private EnemyKilledChannel? _enemyKilledChannel;
        [SerializeField] private BossDefeatedChannel? _bossDefeatedChannel;
        [SerializeField] private DeathChannel? _deathChannel;

        private ILocalTelemetryService? _telemetry;
        private bool _subscribed;

        /// <summary>Inject the telemetry sink before <see cref="OnEnable"/>.</summary>
        public void SetTelemetry(ILocalTelemetryService telemetry) => _telemetry = telemetry;

        /// <summary>Test seam: wire telemetry + channels without an inspector.</summary>
        public void ConfigureForTests(
            ILocalTelemetryService telemetry,
            RunEndedChannel? runEnded = null,
            LevelUpChannel? levelUp = null,
            EnemyKilledChannel? enemyKilled = null,
            BossDefeatedChannel? bossDefeated = null,
            DeathChannel? death = null)
        {
            _telemetry = telemetry;
            _runEndedChannel = runEnded;
            _levelUpChannel = levelUp;
            _enemyKilledChannel = enemyKilled;
            _bossDefeatedChannel = bossDefeated;
            _deathChannel = death;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        /// <summary>Idempotent — safe to call repeatedly.</summary>
        public void Subscribe()
        {
            if (_subscribed) return;

            // Late-bind from GameContext if Boot didn't inject one.
            if (_telemetry == null)
            {
                var ctx = GameContextBootstrap.Context;
                if (ctx != null && ctx.TryGet<ILocalTelemetryService>(out var resolved))
                    _telemetry = resolved;
            }

            if (_runEndedChannel != null)     _runEndedChannel.Subscribe(HandleRunEnded);
            if (_levelUpChannel != null)      _levelUpChannel.Subscribe(HandleLevelUp);
            if (_enemyKilledChannel != null)  _enemyKilledChannel.Subscribe(HandleEnemyKilled);
            if (_bossDefeatedChannel != null) _bossDefeatedChannel.Subscribe(HandleBossDefeated);
            if (_deathChannel != null)        _deathChannel.Subscribe(HandleDeath);

            _subscribed = true;
        }

        /// <summary>Mirror of <see cref="Subscribe"/>.</summary>
        public void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_runEndedChannel != null)     _runEndedChannel.Unsubscribe(HandleRunEnded);
            if (_levelUpChannel != null)      _levelUpChannel.Unsubscribe(HandleLevelUp);
            if (_enemyKilledChannel != null)  _enemyKilledChannel.Unsubscribe(HandleEnemyKilled);
            if (_bossDefeatedChannel != null) _bossDefeatedChannel.Unsubscribe(HandleBossDefeated);
            if (_deathChannel != null)        _deathChannel.Unsubscribe(HandleDeath);
            _subscribed = false;
        }

        // ---- Channel handlers (internal so tests can drive them directly) ----

        internal void HandleRunEnded(RunEndedEvent evt)
        {
            if (_telemetry == null) return;
            var report = evt.report;
            if (report == null)
            {
                _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.RunEnd));
                return;
            }
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.RunEnd, new Dictionary<string, object>
            {
                ["outcome"]        = report.outcome.ToString(),
                ["cause"]          = report.deathCause ?? string.Empty,
                ["seconds"]        = report.runDurationSeconds,
                ["kills"]          = report.totalKills,
                ["elites_killed"]  = report.elitesKilled,
                ["bosses_killed"]  = report.bossesKilled,
                ["waves_cleared"]  = report.wavesCleared,
                ["final_level"]    = report.finalLevel,
                ["xp_gained"]      = report.xpGained,
                ["gold_gained"]    = report.goldGained,
                ["character_id"]   = report.characterId ?? string.Empty,
            }));
        }

        internal void HandleLevelUp(LevelUpEvent evt)
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.LevelUp, new Dictionary<string, object>
            {
                ["level"] = evt.newLevel,
            }));
        }

        /// <summary>Boss kills are routed via <see cref="BossDefeatedChannel"/> — the
        /// per-enemy <see cref="EnemyKilledChannel"/> handler exists so future telemetry
        /// can hook trash kills if needed, but in this build it is intentionally a no-op
        /// to keep the JSONL file size bounded (200+ kills/run would dominate the day's
        /// payload). The bridge stays subscribed so the wiring is visible at inspector time.</summary>
        internal void HandleEnemyKilled(EnemyKilledEvent _) { /* no-op by design — see XML comment */ }

        internal void HandleBossDefeated(BossDefeatedEvent evt)
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.BossKill, new Dictionary<string, object>
            {
                ["boss_id"]     = evt.bossId ?? string.Empty,
                ["run_seconds"] = evt.runSeconds,
            }));
        }

        internal void HandleDeath(DeathEvent evt)
        {
            if (_telemetry == null) return;
            // Skip Victory — that fact is already captured in run_end with outcome=Win.
            if (evt.cause == DeathCause.Victory) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.Death, new Dictionary<string, object>
            {
                ["cause"]          = evt.cause.ToString(),
                ["run_seconds"]    = evt.runSeconds,
                ["enemies_killed"] = evt.enemiesKilled,
            }));
        }

        // ---- Public hooks (callers that lack a dedicated channel) ----

        /// <summary>RunController.Begin() calls this once per run. Until a RunStartedChannel
        /// ships (out-of-scope for this wave), the run-start event is dispatched directly.</summary>
        public void NotifyRunStarted(string characterId)
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.RunStart, new Dictionary<string, object>
            {
                ["character_id"] = characterId ?? string.Empty,
            }));
        }

        /// <summary>Brave.Systems.Iap calls this on a confirmed purchase. Lives here (vs
        /// in Iap) so the JSON shape stays consistent with the rest of the file.</summary>
        public void NotifyPurchase(string sku, string priceLocalDisplay)
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.Purchase, new Dictionary<string, object>
            {
                ["sku"]   = sku ?? string.Empty,
                ["price"] = priceLocalDisplay ?? string.Empty,
            }));
        }

        // ---- App lifecycle ----

        private void OnApplicationPause(bool pause)
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(pause ? TelemetryEventTypes.AppPause : TelemetryEventTypes.AppResume));
            // Always flush on pause so iOS / Android killing the suspended process
            // doesn't lose the day's events.
            _telemetry.Flush();
        }

        private void OnApplicationQuit()
        {
            if (_telemetry == null) return;
            _telemetry.Log(new TelemetryEvent(TelemetryEventTypes.AppQuit));
            _telemetry.Flush();
        }

        // ---- Test diagnostics ----

        internal ILocalTelemetryService? TelemetryForTests => _telemetry;
        internal bool IsSubscribedForTests => _subscribed;
    }
}
