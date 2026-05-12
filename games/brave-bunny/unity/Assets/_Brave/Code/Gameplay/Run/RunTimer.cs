#nullable enable
// GDD 01 § Run length 7-10 min. Boss approach at 7:00; boss spawn at 8:00.
// Hard timeout cap at 8:00 per game CLAUDE.md run-length contract.

using System;

using UnityEngine;

namespace Brave.Gameplay.Run
{
    /// <summary>
    /// Run-clock. Drives the wave schedule + boss approach + timeout. Pauses honor
    /// <c>Time.timeScale</c> automatically since we use <c>Time.deltaTime</c>.
    /// </summary>
    public sealed class RunTimer : MonoBehaviour
    {
        /// <summary>Boss approach raised at this run-time (seconds). GDD 01 anchor.</summary>
        public const float BossApproachAtSeconds = 7f * 60f;

        /// <summary>Boss spawns at this run-time (seconds). GDD 01 anchor.</summary>
        public const float BossSpawnAtSeconds = 8f * 60f;

        /// <summary>Hard timeout (seconds). Run ends as a timeout if hero is still alive.</summary>
        public const float TimeoutSeconds = 480f;       // 8 min cap

        private bool _running;
        private bool _bossApproachFired;
        private bool _bossSpawnFired;

        public float RunSeconds { get; private set; }

        /// <summary>Alias used by RunController.</summary>
        public float Seconds => RunSeconds;

        public event Action? BossApproachReached;
        public event Action? BossSpawnReached;
        public event Action? TimeoutReached;

        /// <summary>Raised once the run has officially ended (timeout or explicit Stop).</summary>
        public event Action? RunEnded;

        public void StartRun()
        {
            RunSeconds = 0f;
            _bossApproachFired = false;
            _bossSpawnFired = false;
            _running = true;
        }

        public void StopRun()
        {
            if (!_running) return;
            _running = false;
            RunEnded?.Invoke();
        }

        // Convenience aliases used by RunController.
        public void Start() => StartRun();
        public void Stop() => StopRun();
        public void Pause() => _running = false;
        public void Resume() => _running = true;

        /// <summary>Manual tick — caller advances the clock by <paramref name="dt"/>. Returns the new RunSeconds.</summary>
        public float Tick(float dt)
        {
            if (_running)
            {
                RunSeconds += dt;
                if (!_bossApproachFired && RunSeconds >= BossApproachAtSeconds)
                { _bossApproachFired = true; BossApproachReached?.Invoke(); }
                if (!_bossSpawnFired && RunSeconds >= BossSpawnAtSeconds)
                { _bossSpawnFired = true; BossSpawnReached?.Invoke(); }
                if (RunSeconds >= TimeoutSeconds)
                {
                    _running = false;
                    TimeoutReached?.Invoke();
                    RunEnded?.Invoke();
                }
            }
            return RunSeconds;
        }

        private void Update()
        {
            if (!_running) return;
            RunSeconds += Time.deltaTime;

            if (!_bossApproachFired && RunSeconds >= BossApproachAtSeconds)
            {
                _bossApproachFired = true;
                BossApproachReached?.Invoke();
            }

            if (!_bossSpawnFired && RunSeconds >= BossSpawnAtSeconds)
            {
                _bossSpawnFired = true;
                BossSpawnReached?.Invoke();
            }

            if (RunSeconds >= TimeoutSeconds)
            {
                _running = false;
                TimeoutReached?.Invoke();
            }
        }
    }
}
