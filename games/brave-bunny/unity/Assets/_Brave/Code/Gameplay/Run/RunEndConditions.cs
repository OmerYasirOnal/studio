#nullable enable
// GDD 01 § Failure loop + Win condition. Three resolutions: win (boss dead), lose (hp 0),
// timeout (hard 8:00 cap). The RunStateMachine consumes the verdict and transitions to
// RunEnd with the appropriate run-end payload.

using System;

namespace Brave.Gameplay.Run
{
    /// <summary>The way a run ends. Routed to the tally screen.</summary>
    public enum RunOutcome
    {
        Win     = 0,        // end-boss defeated
        Lose    = 1,        // hero HP reached 0 and no revive
        Timeout = 2,        // 8-minute hard cap reached
        Quit    = 3,        // player chose quit-to-menu from pause
    }

    /// <summary>
    /// Encapsulates the three end-of-run checks. Each tick, the RunController calls
    /// <see cref="EvaluateOutcome"/>; if it returns non-null, transition to RunEnd.
    /// </summary>
    public sealed class RunEndConditions
    {
        private readonly Func<float> _heroHpGetter;
        private readonly Func<bool> _endBossAliveGetter;
        private readonly Func<bool> _endBossEverSpawnedGetter;
        private readonly RunTimer _timer;

        public RunEndConditions(Func<float> heroHp, Func<bool> endBossAlive,
            Func<bool> endBossEverSpawned, RunTimer timer)
        {
            _heroHpGetter = heroHp;
            _endBossAliveGetter = endBossAlive;
            _endBossEverSpawnedGetter = endBossEverSpawned;
            _timer = timer;
        }

        /// <summary>Returns the run outcome or null if the run should continue.</summary>
        public RunOutcome? EvaluateOutcome()
        {
            // Order matters: hero HP 0 short-circuits.
            if (_heroHpGetter() <= 0f) return RunOutcome.Lose;

            // Boss defeated -> win (only if it ever spawned).
            if (_endBossEverSpawnedGetter() && !_endBossAliveGetter())
                return RunOutcome.Win;

            // Hard timeout.
            if (_timer.RunSeconds >= RunTimer.TimeoutSeconds)
                return RunOutcome.Timeout;

            return null;
        }
    }
}
