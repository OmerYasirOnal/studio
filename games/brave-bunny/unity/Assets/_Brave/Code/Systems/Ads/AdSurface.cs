// Brave Bunny — Systems / Ads
// Design source: docs/02-gdd/09-monetization-design.md (4-6 rewarded-ad surfaces)
// 08-economy.md "Allowed monetization surfaces" — ads are quality-of-life, not power.

#nullable enable

namespace Brave.Systems.Ads;

/// <summary>
/// The 4-6 launch rewarded-ad surfaces. Every surface ships with a hard cap
/// per session/day enforced by AdsService — never an uncapped ad funnel.
/// </summary>
public enum AdSurface
{
    Revive = 0,            // Run-end: revive once per run (first death only).
    DoubleEndRewards = 1,  // Run-end: 2x carrots from the tally.
    DailyChest = 2,        // Home: open the daily reward chest (1/day, includes the Stars chest).
    ExtraBanish = 3,       // Mid-run draft: reroll one draft option.
    MagnetBoost = 4,       // Mid-run: 30s magnet boost.
    FreePull = 5,          // Store: free cosmetic shard pull (1/day).
}
