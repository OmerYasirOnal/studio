// Brave Bunny — Systems / Progression
// Design source: docs/02-gdd/02-meta-loop.md (daily streak: 7-day cycle, 2-day skip tolerance)
// Tech spec: 03-save-system.md trigger — "Achievement claimed" pattern is reused for streak claims.

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

public interface IDailyStreakService : IService
{
    bool IsClaimable(DateTime utcNow);
    int CurrentStreakDay { get; }
    void Claim(DateTime utcNow);
}

/// <summary>
/// Tracks daily-streak claim eligibility per 02-meta-loop.md.
/// - Day increments on first claim within a new UTC day.
/// - Missing up to 2 consecutive UTC days does not reset the streak.
/// - Missing 3+ days resets to day 1.
/// </summary>
public sealed class DailyStreakService : IDailyStreakService
{
    private const int CycleLength = 7;
    private const int SkipToleranceDays = 2;

    private readonly ISaveService _save;

    public DailyStreakService(ISaveService save) { _save = save; }

    public int CurrentStreakDay => _save.Data.DailyStreak.CurrentDay;

    public bool IsClaimable(DateTime utcNow)
    {
        var streak = _save.Data.DailyStreak;
        var today = utcNow.Date;
        if (streak.LastClaimUtcDate is null) return true;
        if (!DateTime.TryParse(streak.LastClaimUtcDate, out var last)) return true;
        return today > last.Date;
    }

    public void Claim(DateTime utcNow)
    {
        if (!IsClaimable(utcNow)) return;
        var streak = _save.Data.DailyStreak;
        var today = utcNow.Date;
        var dayDelta = streak.LastClaimUtcDate != null && DateTime.TryParse(streak.LastClaimUtcDate, out var last)
            ? (today - last.Date).Days
            : 1;

        if (dayDelta <= 1) streak.CurrentDay = (streak.CurrentDay % CycleLength) + 1;
        else if (dayDelta <= 1 + SkipToleranceDays) streak.CurrentDay = (streak.CurrentDay % CycleLength) + 1; // soft-detect tolerance
        else streak.CurrentDay = 1; // reset

        streak.LastClaimUtcDate = today.ToString("o");
        _save.Save();
    }
}
