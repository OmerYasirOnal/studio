// Brave Bunny — Systems / Ads
// Outcome shape returned to the ad-surface caller.

#nullable enable

namespace Brave.Systems.Ads;

public enum AdResult
{
    Watched = 0,      // User watched to completion → grant the reward.
    Skipped = 1,      // User dismissed before completion → no reward.
    NotAvailable = 2, // No fill / network unreachable.
    Errored = 3,      // SDK error.
}
