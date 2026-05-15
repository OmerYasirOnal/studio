// Brave Bunny — Wave 10 / Combo counter
// Payload + channel for the kill-streak ("combo") system.
//
// Owner: gameplay agent (Wave 10). Subscribers: UI (ComboBadgeController), and
// later audio + analytics. Fired by ComboService on every increment AND on
// streak break (currentStreak=0). Allocation-free: readonly struct, pass-by-value.

using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Snapshot of the current combo / kill-streak state. Raised on increment AND on break.
    /// <c>currentStreak == 0</c> indicates the streak just broke (window expired). Otherwise
    /// <c>currentStreak</c> is the cumulative count of kills within the rolling window.
    /// </summary>
    public readonly struct ComboChangedEvent
    {
        /// <summary>Current streak count. 0 = the streak just ended (window expired).</summary>
        public readonly int currentStreak;
        /// <summary>Peak streak reached in the current run so far (monotonic per run).</summary>
        public readonly int peakStreak;
        /// <summary>1 = silver, 2 = gold, 3 = rainbow; 0 = below tier-1 threshold or broken.</summary>
        public readonly int tier;
        /// <summary>Time (in run seconds) of the kill that triggered the change. 0 on break.</summary>
        public readonly float runSecondsAtChange;

        public ComboChangedEvent(int currentStreak, int peakStreak, int tier, float runSecondsAtChange)
        {
            this.currentStreak = currentStreak;
            this.peakStreak = peakStreak;
            this.tier = tier;
            this.runSecondsAtChange = runSecondsAtChange;
        }
    }

    /// <summary>
    /// SO channel that emits <see cref="ComboChangedEvent"/> values from <c>ComboService</c>.
    /// Designers wire this asset to subscribers (UI controller, audio cue dispatcher).
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Events/ComboChanged", fileName = "ComboChangedChannel", order = 6)]
    public sealed class ComboChangedChannel : EventChannel<ComboChangedEvent> { }
}
