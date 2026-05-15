// Brave Bunny — Systems / LiveOps
// Wave 9: daily login rewards (7-day rotating calendar).
// Design source: docs/02-gdd/02-meta-loop.md (daily login)
// Tech spec: docs/06-tech-spec/03-save-system.md trigger — "Daily reward claimed"
//            uses the same pattern as Achievement.Claim (Save() on grant).
// ADR-0008: new SaveData.DailyRewardState field is forward-compat; missing key
//           in v1 saves deserializes as default-safely (currentDay=1, lifetime=0).
//
// Contract:
//   * CanClaim(utcNow) — true iff lastClaimUtc < utcNow.Date.
//   * Claim(utcNow)    — grants today's reward via IProgressionService.Wallet,
//                        advances currentDay (1..7 → 1..), increments lifetime,
//                        stamps lastClaimUtc, calls ISaveService.Save().
//   * PeekToday()      — non-mutating; returns the DailyReward at currentDay.
//   * Soft-reset on day-8: (currentDay % 7) + 1 wraps 7 → 1.
//   * Missed days still increment lifetime on the next claim (calendar position
//     advances regardless of gap; no penalty per Wave 9 brief — distinct from
//     DailyStreakService which DOES enforce the 2-day tolerance).

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using Brave.Systems.Save;

namespace Brave.Systems.LiveOps;

public interface IDailyRewardService : IService
{
    /// <summary>True iff a new UTC day has begun since the last claim (or never claimed).</summary>
    bool CanClaim(DateTime utcNow);

    /// <summary>
    /// Grants today's reward (no-op + null if not claimable). Advances cycle,
    /// stamps lastClaimUtc, increments lifetime, persists.
    /// </summary>
    DailyReward? Claim(DateTime utcNow);

    /// <summary>Non-mutating peek at today's reward (for UI calendar render).</summary>
    DailyReward PeekToday();

    /// <summary>Today's cycle day in [1..7]. Read-only convenience for UI.</summary>
    int CurrentDay { get; }

    /// <summary>Lifetime number of claims across all cycles. Read-only.</summary>
    int LifetimeClaims { get; }
}

/// <summary>
/// Persists daily-login progress via <see cref="ISaveService"/> and grants
/// rewards via <see cref="IProgressionService.Wallet"/>. UTC-only — never
/// reads <see cref="DateTime.Now"/>. The 7-day reward table is supplied by
/// <see cref="DailyRewardConfig"/> (ScriptableObject, set in Boot).
/// </summary>
public sealed class DailyRewardService : IDailyRewardService
{
    private readonly ISaveService _save;
    private readonly IProgressionService _progression;
    private readonly DailyRewardConfig _config;

    public DailyRewardService(ISaveService save, IProgressionService progression, DailyRewardConfig config)
    {
        _save = save ?? throw new ArgumentNullException(nameof(save));
        _progression = progression ?? throw new ArgumentNullException(nameof(progression));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public int CurrentDay => Clamp(_save.Data.DailyRewardState.CurrentDay);

    public int LifetimeClaims => _save.Data.DailyRewardState.LifetimeClaims;

    public bool CanClaim(DateTime utcNow)
    {
        var state = _save.Data.DailyRewardState;
        if (string.IsNullOrEmpty(state.LastClaimUtc)) return true;
        if (!DateTime.TryParse(state.LastClaimUtc, out var last)) return true;
        // Both compared at UTC midnight; new UTC day = claimable.
        return utcNow.ToUniversalTime().Date > last.ToUniversalTime().Date;
    }

    public DailyReward PeekToday() => _config.GetReward(CurrentDay);

    public DailyReward? Claim(DateTime utcNow)
    {
        if (!CanClaim(utcNow)) return null;

        var reward = _config.GetReward(CurrentDay);

        // Grant via wallet — single side-effect on progression state.
        _progression.Wallet.Add(reward.CurrencyType, reward.Amount);

        // Advance cycle: 1→2, …, 7→1 (soft-reset on day-8).
        var state = _save.Data.DailyRewardState;
        state.CurrentDay = (Clamp(state.CurrentDay) % DailyRewardConfig.CycleLength) + 1;
        state.LastClaimUtc = utcNow.ToUniversalTime().Date.ToString("o");
        state.LifetimeClaims++;

        _save.Save(); // 03-save-system.md trigger: "Daily reward claimed".
        return reward;
    }

    private static int Clamp(int day)
    {
        // Defensive: corrupted save shouldn't crash the UI. Wrap into [1..7].
        if (day < 1) return 1;
        if (day > DailyRewardConfig.CycleLength) return ((day - 1) % DailyRewardConfig.CycleLength) + 1;
        return day;
    }
}
