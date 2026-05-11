// Brave Bunny — Systems / Analytics
// Typed event helpers — gameplay/UI callers use these instead of string keys
// to keep names+props consistent across the codebase. Add new helpers here
// rather than emitting raw AnalyticsEvent at call sites.

#nullable enable

using System.Collections.Generic;

namespace Brave.Systems.Analytics;

public static class AnalyticsEvents
{
    public static void RunEnded(IAnalyticsService svc, int kills, int carrotsEarned, int soulShardsEarned, float seconds, bool won)
    {
        svc.Track(new AnalyticsEvent("run_ended", new Dictionary<string, object>
        {
            ["kills"] = kills,
            ["carrots_earned"] = carrotsEarned,
            ["soul_shards_earned"] = soulShardsEarned,
            ["seconds"] = seconds,
            ["won"] = won,
        }));
    }

    public static void CharacterUnlocked(IAnalyticsService svc, string slug, string source /* "stars" | "achievement" | "iap" */)
    {
        svc.Track(new AnalyticsEvent("character_unlocked", new Dictionary<string, object>
        {
            ["slug"] = slug,
            ["source"] = source,
        }));
    }

    public static void IapPurchased(IAnalyticsService svc, string sku, string priceLocal)
    {
        svc.Track(new AnalyticsEvent("iap_purchased", new Dictionary<string, object>
        {
            ["sku"] = sku,
            ["price_local"] = priceLocal,
        }));
    }

    public static void AdShown(IAnalyticsService svc, string surface, bool watchedToCompletion)
    {
        svc.Track(new AnalyticsEvent("ad_shown", new Dictionary<string, object>
        {
            ["surface"] = surface,
            ["watched"] = watchedToCompletion,
        }));
    }

    public static void DailyStreakClaimed(IAnalyticsService svc, int streakDay)
    {
        svc.Track(new AnalyticsEvent("daily_streak_claimed", new Dictionary<string, object>
        {
            ["day"] = streakDay,
        }));
    }
}
