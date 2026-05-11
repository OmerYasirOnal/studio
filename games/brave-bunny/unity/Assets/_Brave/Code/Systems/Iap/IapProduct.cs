// Brave Bunny — Systems / IAP
// Design source: docs/02-gdd/08-economy.md (SKU price ladder: $0.99 → $19.99 cap)
// Catalog rows are owned by data/balance/economy.json (per CLAUDE.md principle 6).

#nullable enable

using System;
using Newtonsoft.Json;

namespace Brave.Systems.Iap;

/// <summary>
/// One row of the IAP catalog. Mirrors the price-ladder table in 08-economy.md.
/// Stars-per-dollar is derived, not stored. Hard cap of $19.99 enforced by
/// validation in <see cref="IapService"/> on catalog load.
/// </summary>
[Serializable]
public sealed class IapProduct
{
    [JsonProperty("sku")] public string Sku = string.Empty;
    [JsonProperty("displayName")] public string DisplayName = string.Empty;
    [JsonProperty("priceUsdCents")] public int PriceUsdCents;     // 99, 499, 999, 1999
    [JsonProperty("starsGranted")] public int StarsGranted;
    [JsonProperty("kind")] public string Kind = "consumable";     // "consumable" | "nonconsumable"
    [JsonProperty("bonusFlags")] public string[] BonusFlags = Array.Empty<string>();
}
