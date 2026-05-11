// Brave Bunny — Systems / Progression
// Design source: docs/02-gdd/02-meta-loop.md (50 achievements at launch)
// Tech spec: 03-save-system.md save trigger — "Achievement claimed".

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

public interface IAchievementService : IService
{
    int GetProgress(string slug);
    bool IsClaimed(string slug);
    void AddProgress(string slug, int delta);
    bool TryClaim(string slug, int target);
}

/// <summary>
/// Tracks the 50 launch achievements per 02-meta-loop.md. The catalog (target
/// thresholds + rewards) is owned by data/balance/economy.json; this service
/// only stores per-player progress + claim state in the save.
/// </summary>
public sealed class AchievementService : IAchievementService
{
    private readonly ISaveService _save;

    public AchievementService(ISaveService save) { _save = save; }

    public int GetProgress(string slug) =>
        _save.Data.Achievements.TryGetValue(slug, out var e) ? e.Progress : 0;

    public bool IsClaimed(string slug) =>
        _save.Data.Achievements.TryGetValue(slug, out var e) && e.Claimed;

    public void AddProgress(string slug, int delta)
    {
        if (delta <= 0) return;
        var entry = GetOrCreate(slug);
        if (entry.Claimed) return;
        entry.Progress += delta;
        // No save call — progress ticks every kill; saves are batched at RunEnd per 03-save-system.md.
    }

    public bool TryClaim(string slug, int target)
    {
        var entry = GetOrCreate(slug);
        if (entry.Claimed) return false;
        if (entry.Progress < target) return false;
        entry.Claimed = true;
        entry.CompletedAt ??= DateTime.UtcNow.ToString("o");
        _save.Save(); // 03-save-system.md trigger: "Achievement claimed"
        return true;
    }

    private SaveData.AchievementEntry GetOrCreate(string slug)
    {
        if (!_save.Data.Achievements.TryGetValue(slug, out var entry))
        {
            entry = new SaveData.AchievementEntry();
            _save.Data.Achievements[slug] = entry;
        }
        return entry;
    }
}
