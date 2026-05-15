#nullable enable
// Brave Bunny — Hit Feedback Juice
// ADR-0003 (canonical lock): hitstop durations are ms-precise per trigger.
//
// This service is the *global* hitstop applier. It coalesces overlapping requests so a
// rapid burst of hits doesn't double-pause the world — the longest pending resume-time
// wins. Per-frame work is O(1); no allocations.
//
// Test plan: HitstopServiceTests asserts:
//   - Apply(seconds) zeros Time.timeScale and restores it after the window
//   - Apply()-during-active extends the resume time but does not re-zero
//   - Tick(unscaledTime) restores once now >= resumeAt
//
// Hookup: the brief specifies subscription to OnEnemyDamaged / OnPlayerHit. Those
// channels don't yet exist project-wide; instead this service exposes a direct
// imperative API (Apply / ApplyForTrigger) plus auto-subscribes to the existing
// EnemyKilledChannel for kill-stop. Direct-call sites (DamageApplier, player hurt)
// can be wired in a follow-up without touching this service.

using System;

using UnityEngine;

using Brave.Gameplay.Damage;
using Brave.Gameplay.Events;

namespace Brave.Gameplay.Feel
{
    /// <summary>
    /// Single global hitstop applier. Subclasses Hitstop's MonoBehaviour pattern but
    /// runs as a pure service (Unity-agnostic Tick API for tests). One instance per
    /// run; lives on the [Bootstrap] GameObject or is created by code at run start.
    /// </summary>
    public sealed class HitstopService
    {
        private readonly FeelConfig _config;
        private readonly FeelDefinition? _definition;

        private float _resumeAtUnscaledTime;
        private float _previousTimeScale = 1f;
        private bool _active;
        private float _holdTimeScale;

        /// <summary><c>true</c> while a hitstop window is currently held.</summary>
        public bool IsActive => _active;

        /// <summary>Resume time (in unscaled seconds) for the active window. 0 when idle.</summary>
        public float ResumeAtUnscaledTime => _resumeAtUnscaledTime;

        public HitstopService(FeelConfig config, FeelDefinition? definition = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            _definition = definition;
            _holdTimeScale = Mathf.Clamp01(config.hitstopTimeScale);
        }

        /// <summary>
        /// Apply a hitstop window of <paramref name="durationSeconds"/> starting at
        /// <paramref name="unscaledNow"/>. Coalescing: if the service is already
        /// active, the window is *extended* (longest wins) — the time-scale is not
        /// re-applied. Sub-zero/zero durations are ignored.
        /// </summary>
        public void Apply(float durationSeconds, float unscaledNow)
        {
            if (durationSeconds <= 0f) return;

            float resumeAt = unscaledNow + durationSeconds;

            if (!_active)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = _holdTimeScale;
                _active = true;
                _resumeAtUnscaledTime = resumeAt;
                return;
            }

            // Coalesce: extend the window if the new resume is later than the pending one.
            if (resumeAt > _resumeAtUnscaledTime)
                _resumeAtUnscaledTime = resumeAt;
        }

        /// <summary>
        /// Apply using the per-trigger ms lookup in <see cref="FeelDefinition"/> (ADR-0003).
        /// Falls back to <see cref="FeelConfig.HitstopSeconds"/> when no definition is bound.
        /// </summary>
        public void ApplyForTrigger(HitstopTrigger trigger, float unscaledNow)
        {
            float seconds = _definition != null
                ? _definition.DurationMsFor(trigger) * 0.001f
                : _config.HitstopSeconds;
            Apply(seconds, unscaledNow);
        }

        /// <summary>
        /// Restore the time-scale if the active window has expired. Call from
        /// MonoBehaviour Update() with <see cref="UnityEngine.Time.unscaledTime"/>.
        /// </summary>
        public void Tick(float unscaledNow)
        {
            if (!_active) return;
            if (unscaledNow >= _resumeAtUnscaledTime)
            {
                Time.timeScale = _previousTimeScale;
                _active = false;
                _resumeAtUnscaledTime = 0f;
            }
        }

        /// <summary>
        /// Force-restore the time-scale immediately. Used on run-end / scene unload so
        /// a queued window doesn't leak into the menu.
        /// </summary>
        public void Cancel()
        {
            if (!_active) return;
            Time.timeScale = _previousTimeScale;
            _active = false;
            _resumeAtUnscaledTime = 0f;
        }

        /// <summary>Subscribe to the kill channel; applies the basic-kill window automatically.</summary>
        public void BindEnemyKilledChannel(EnemyKilledChannel? channel)
        {
            if (channel == null) return;
            channel.Subscribe(OnEnemyKilled);
        }

        /// <summary>Unsubscribe on shutdown so domain-reload doesn't double-bind.</summary>
        public void UnbindEnemyKilledChannel(EnemyKilledChannel? channel)
        {
            if (channel == null) return;
            channel.Unsubscribe(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            var trigger = e.wasElite ? HitstopTrigger.EliteKill : HitstopTrigger.BasicEnemyKill;
            ApplyForTrigger(trigger, Time.unscaledTime);
        }
    }

    /// <summary>
    /// Thin MonoBehaviour host that ticks the pure <see cref="HitstopService"/> from
    /// Unity's Update loop. Tests construct the service directly without this host.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitstopServiceHost : MonoBehaviour
    {
        [SerializeField] private FeelConfig? _config;
        [SerializeField] private FeelDefinition? _definition;
        [SerializeField] private EnemyKilledChannel? _killChannel;

        private HitstopService? _service;
        public HitstopService? Service => _service;

        private void Awake()
        {
            if (_config == null) { enabled = false; return; }
            _service = new HitstopService(_config, _definition);
            _service.BindEnemyKilledChannel(_killChannel);
        }

        private void OnDestroy()
        {
            _service?.UnbindEnemyKilledChannel(_killChannel);
            _service?.Cancel();
        }

        private void Update() => _service?.Tick(Time.unscaledTime);
    }
}
