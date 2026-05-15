// Brave Bunny — Systems / LiveOps
// Wave 9: ScriptableObject holding the 7-day reward table for daily login.
// Per CLAUDE.md principle 6 (no magic numbers) — the schedule lives in the
// SO asset at Assets/_Brave/Data/Definitions/LiveOps/DailyRewardConfig.asset,
// not in code. Balance-engineer can re-tune without recompiling.
//
// Design source: docs/02-gdd/02-meta-loop.md (daily login: 7-day cycle, day-7 milestone)

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Progression;
using UnityEngine;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// 7-row reward schedule for the daily login calendar. Indexed 1..7 in the
    /// inspector but stored 0-based in <see cref="entries"/>; helper methods
    /// guard the off-by-one.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/LiveOps/DailyRewardConfig", fileName = "DailyRewardConfig", order = 20)]
    public sealed class DailyRewardConfig : ScriptableObject
    {
        /// <summary>The fixed cycle length in days. Day index loops 1..7 → 1..</summary>
        public const int CycleLength = 7;

        [Serializable]
        public sealed class Entry
        {
            [Tooltip("Currency to grant for this day.")]
            public CurrencyType currencyType = CurrencyType.Carrots;

            [Tooltip("Amount of the currency to grant. Must be > 0.")]
            [Min(1)] public int amount = 50;

            [Tooltip("Milestone (final-day) styling. Typically day 7 only.")]
            public bool isMilestone;
        }

        [Header("Day-by-day reward table — exactly 7 rows (day 1 = index 0)")]
        [Tooltip("Day 1 → entries[0], Day 7 → entries[6]. Inspector ordering must match.")]
        [SerializeField] private List<Entry> entries = new();

        /// <summary>Resolve the reward for a given day index in [1..7]. Throws on out-of-range.</summary>
        public DailyReward GetReward(int day)
        {
            if (day < 1 || day > CycleLength)
                throw new ArgumentOutOfRangeException(nameof(day), $"Day {day} not in [1..{CycleLength}].");
            if (entries == null || entries.Count < CycleLength)
                throw new InvalidOperationException(
                    $"DailyRewardConfig must have exactly {CycleLength} entries — found {entries?.Count ?? 0}.");

            var e = entries[day - 1];
            return new DailyReward(day, e.currencyType, e.amount, e.isMilestone);
        }

        /// <summary>Editor + test seam: replace the entry list wholesale. Production callers use the inspector.</summary>
        public void SetEntriesForTest(IReadOnlyList<Entry> source)
        {
            entries = new List<Entry>(source);
        }
    }
}
