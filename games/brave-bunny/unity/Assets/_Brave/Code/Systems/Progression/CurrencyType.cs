// Brave Bunny — Systems / Progression
// Design source: docs/02-gdd/08-economy.md (three-currency model)

#nullable enable

namespace Brave.Systems.Progression;

/// <summary>
/// The three currencies. Each has a single primary sink per the
/// "one sink per currency" rule in 08-economy.md §Design philosophy.
/// </summary>
public enum CurrencyType
{
    /// <summary>Soft gold. Earned in-run. Primary sink: character meta-level upgrades.</summary>
    Carrots = 0,

    /// <summary>Premium. Earned via IAP / battle pass / achievements. Primary sink: character unlocks.</summary>
    Stars = 1,

    /// <summary>Run-banked. Dropped by elites + bosses. Primary sink: runes (v1.1 post-launch).</summary>
    SoulShards = 2,
}
