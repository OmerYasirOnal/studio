#nullable enable
// Brave Bunny — Wave 10 / Combo counter (kill-streak)
//
// ComboService tracks consecutive enemy kills inside a rolling time window
// (`FeelConfig.comboWindowSeconds`, default 2.0s). Every kill that arrives within
// the window since the previous kill bumps `currentStreak`. If no kill lands
// before the window expires, the streak breaks and resets to zero. Every
// transition fires `ComboChangedEvent` through the bound `ComboChangedChannel`.
//
// Out-of-scope (separate Wave 10 agents own these):
//   - Audio cues on tier rollover (audio agent listens to the channel)
//   - Crit / damage-multiplier ties (separate agent)
//   - Achievement triggers (separate agent)
//
// Allocation-free hot path: no closures, no string formatting, no LINQ. The
// service is a plain C# class so EditMode tests can exercise it without a
// MonoBehaviour. A thin `ComboServiceHost` MonoBehaviour pumps Tick() per frame
// at runtime — same pattern as HitstopServiceHost (Wave-4 convention).
//
// CLAUDE.md principle 6 (no magic numbers): the window and tier thresholds live
// on `FeelConfig`. Defaults below are seeded from the asset, never inlined.

using System;

using UnityEngine;

using Brave.Gameplay.Events;
using Brave.Gameplay.Feel;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Combo / kill-streak service contract. Implementations track streak state
    /// against a rolling window and emit <see cref="ComboChangedEvent"/> on every
    /// transition (increment, break).
    /// </summary>
    public interface IComboService
    {
        /// <summary>Current consecutive-kill streak. 0 when idle / broken.</summary>
        int CurrentStreak { get; }

        /// <summary>Peak streak reached so far this run.</summary>
        int PeakStreak { get; }

        /// <summary>Tier of <see cref="CurrentStreak"/> per FeelConfig thresholds (0–3).</summary>
        int CurrentTier { get; }

        /// <summary>Apply a single kill at the given run time. Returns the new streak count.</summary>
        int RegisterKill(float runSecondsNow);

        /// <summary>Tick the rolling window; breaks the streak when it expires.</summary>
        void Tick(float runSecondsNow);

        /// <summary>Reset state at run start. Does not fire an event.</summary>
        void Reset();
    }

    /// <summary>
    /// Default <see cref="IComboService"/>. Pure (no MonoBehaviour) so tests can
    /// drive deterministic clock values. <c>ComboServiceHost</c> wraps it for
    /// runtime use.
    /// </summary>
    public sealed class ComboService : IComboService
    {
        // Sentinel: there is no pending streak yet.
        private const float NoLastKill = -1f;

        private readonly FeelConfig _config;
        private readonly ComboChangedChannel? _channel;

        private int _currentStreak;
        private int _peakStreak;
        private float _lastKillTime = NoLastKill;

        public int CurrentStreak => _currentStreak;
        public int PeakStreak => _peakStreak;
        public int CurrentTier => TierFor(_currentStreak, _config);

        public ComboService(FeelConfig config, ComboChangedChannel? channel = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            _channel = channel;
        }

        /// <inheritdoc />
        public int RegisterKill(float runSecondsNow)
        {
            // If a previous streak is still in-window, continue it; otherwise this
            // kill starts a fresh streak at 1. The case where the previous window
            // has already expired but Tick() hasn't run yet is handled here too —
            // we don't want a stale kill from 10 seconds ago to contribute.
            if (_lastKillTime != NoLastKill &&
                (runSecondsNow - _lastKillTime) <= _config.comboWindowSeconds)
            {
                _currentStreak += 1;
            }
            else
            {
                _currentStreak = 1;
            }

            if (_currentStreak > _peakStreak) _peakStreak = _currentStreak;
            _lastKillTime = runSecondsNow;

            RaiseChange(runSecondsNow);
            return _currentStreak;
        }

        /// <inheritdoc />
        public void Tick(float runSecondsNow)
        {
            if (_currentStreak <= 0) return;
            if (_lastKillTime == NoLastKill) return;

            if ((runSecondsNow - _lastKillTime) > _config.comboWindowSeconds)
            {
                _currentStreak = 0;
                _lastKillTime = NoLastKill;
                // Break: fire with currentStreak=0 (and tier=0) — runSecondsAtChange=0
                // to match the contract on ComboChangedEvent.
                _channel?.Raise(new ComboChangedEvent(0, _peakStreak, 0, 0f));
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            _currentStreak = 0;
            _peakStreak = 0;
            _lastKillTime = NoLastKill;
        }

        /// <summary>Subscribe the service to a kill channel for fire-and-forget wiring.</summary>
        public void BindEnemyKilledChannel(EnemyKilledChannel? channel)
        {
            if (channel == null) return;
            channel.Subscribe(OnEnemyKilled);
        }

        public void UnbindEnemyKilledChannel(EnemyKilledChannel? channel)
        {
            if (channel == null) return;
            channel.Unsubscribe(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilledEvent e) => RegisterKill(e.runSeconds);

        private void RaiseChange(float runSecondsNow)
        {
            if (_channel == null) return;
            int tier = TierFor(_currentStreak, _config);
            _channel.Raise(new ComboChangedEvent(_currentStreak, _peakStreak, tier, runSecondsNow));
        }

        /// <summary>
        /// Resolve the tier (0/1/2/3) for a given streak count against the
        /// <see cref="FeelConfig"/> thresholds. Static so tests and UI can share.
        /// </summary>
        public static int TierFor(int streak, FeelConfig config)
        {
            if (streak <= 0) return 0;
            if (streak >= config.comboTier3Threshold) return 3;
            if (streak >= config.comboTier2Threshold) return 2;
            if (streak >= config.comboTier1Threshold) return 1;
            return 0;
        }
    }

    /// <summary>
    /// Runtime host that ticks <see cref="ComboService"/> from Unity's Update
    /// loop and wires it to the kill channel. Tests construct the service
    /// directly and skip this host.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ComboServiceHost : MonoBehaviour
    {
        [SerializeField] private FeelConfig? _config;
        [SerializeField] private ComboChangedChannel? _channel;
        [SerializeField] private EnemyKilledChannel? _killChannel;

        private ComboService? _service;
        private float _runStartTime;

        public ComboService? Service => _service;

        private void Awake()
        {
            if (_config == null) { enabled = false; return; }
            _service = new ComboService(_config, _channel);
            _service.BindEnemyKilledChannel(_killChannel);
            _runStartTime = Time.time;
        }

        private void OnDestroy()
        {
            _service?.UnbindEnemyKilledChannel(_killChannel);
            _service?.Reset();
        }

        private void Update()
        {
            if (_service == null) return;
            _service.Tick(Time.time - _runStartTime);
        }
    }
}
