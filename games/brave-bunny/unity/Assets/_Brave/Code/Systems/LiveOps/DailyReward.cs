// Brave Bunny — Systems / LiveOps
// Wave 9: daily login rewards. Plain DTO returned by IDailyRewardService.Peek/Claim
// so callers (UI, telemetry) can render a card without depending on the SO.
// Design source: docs/02-gdd/02-meta-loop.md (7-day rotating calendar)

#nullable enable

using Brave.Systems.Progression;

namespace Brave.Systems.LiveOps;

/// <summary>
/// One day's reward on the 7-day rotating calendar. Built by
/// <see cref="DailyRewardConfig"/> and returned by
/// <see cref="IDailyRewardService.PeekToday"/> / <see cref="IDailyRewardService.Claim"/>.
/// Milestones (day 7) get distinct UI styling.
/// </summary>
public sealed class DailyReward
{
    /// <summary>Day index on the cycle (1..7).</summary>
    public int Day { get; }

    /// <summary>Currency type to grant. Maps to <see cref="CurrencyWallet"/>.</summary>
    public CurrencyType CurrencyType { get; }

    /// <summary>Amount to grant. Always positive.</summary>
    public int Amount { get; }

    /// <summary>True for the final-day mega-reward (e.g. summon ticket). UI uses this for special framing.</summary>
    public bool IsMilestone { get; }

    public DailyReward(int day, CurrencyType currencyType, int amount, bool isMilestone)
    {
        Day = day;
        CurrencyType = currencyType;
        Amount = amount;
        IsMilestone = isMilestone;
    }
}
