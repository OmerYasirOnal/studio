// Brave Bunny — Systems / Ads
// CLAUDE.md allows Unity Ads / AdMob (revenue side, not generation side).
// Editor mode is NoOp — returns Watched immediately so dev flow is unblocked.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;

namespace Brave.Systems.Ads;

public interface IAdsService : IService
{
    bool IsAvailable(AdSurface surface);
    void Show(AdSurface surface, Action<AdResult> onComplete);
}

/// <summary>
/// Rewarded-ads facade. Wraps Unity Ads / AdMob at runtime; Editor returns
/// <see cref="AdResult.Watched"/> after one frame so QA can verify reward
/// payout flow without ad SDK setup. Per-surface daily caps live in
/// data/balance/economy.json and are enforced by callers, not here.
/// </summary>
public sealed class AdsService : IAdsService
{
    private readonly Dictionary<AdSurface, bool> _loaded = new();

    public bool IsAvailable(AdSurface surface)
    {
#if UNITY_EDITOR
        return true;
#else
        return _loaded.TryGetValue(surface, out var ready) && ready;
#endif
    }

    public void Show(AdSurface surface, Action<AdResult> onComplete)
    {
#if UNITY_EDITOR
        onComplete(AdResult.Watched);
#else
        throw new NotImplementedException("Wire Unity Ads / AdMob rewarded listener post-Phase-5.");
#endif
    }
}
