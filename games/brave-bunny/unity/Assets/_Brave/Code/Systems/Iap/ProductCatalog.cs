// Brave Bunny — Systems / IAP
// Source of truth: data/balance/economy.json §iap_catalog
// CLAUDE.md principle 6 — no magic numbers in code. Catalog rows are loaded from JSON at boot.
// Tech spec: docs/02-gdd/09-monetization-design.md (SKU lineup + price ladder)
// ADR-0010: Monthly Bunny Card 35 stars/day × 30 = 1050 stars at $4.99 is locked.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brave.Systems.Iap;

/// <summary>
/// Loads <see cref="IapProduct"/> rows from <c>data/balance/economy.json</c>
/// (resolved at runtime via <c>Resources.Load&lt;TextAsset&gt;("balance/economy")</c>
/// or an Editor copy under <c>Assets/_Brave/Data/Balance/economy.json</c>).
/// Caller (typically <see cref="IapService"/>) consumes <see cref="Products"/>.
/// </summary>
public sealed class ProductCatalog
{
    /// <summary>Hard cap from 08-economy.md / ADR-0010. Catalog parsing rejects anything above.</summary>
    public const int MaxPriceUsdCents = 1999;

    private readonly List<IapProduct> _products = new();
    public IReadOnlyList<IapProduct> Products => _products;

    /// <summary>Parse the <c>iap_catalog</c> array from the economy.json text payload.</summary>
    public void LoadFromJson(string json)
    {
        _products.Clear();
        if (string.IsNullOrEmpty(json)) return;

        JObject root;
        try { root = JObject.Parse(json); }
        catch (JsonException e)
        {
            Debug.LogError($"[ProductCatalog] economy.json parse failed: {e.Message}");
            return;
        }

        var arr = root["iap_catalog"] as JArray;
        if (arr == null)
        {
            Debug.LogWarning("[ProductCatalog] No 'iap_catalog' array in economy.json.");
            return;
        }

        foreach (var node in arr)
        {
            var sku = (string?)node["sku"];
            if (string.IsNullOrWhiteSpace(sku)) continue;

            var priceUsd = (double?)node["price_usd"] ?? 0;
            var priceCents = (int)Math.Round(priceUsd * 100.0);

            if (priceCents > MaxPriceUsdCents)
            {
                Debug.LogWarning($"[ProductCatalog] SKU '{sku}' price ${priceUsd} exceeds $19.99 cap — skipped.");
                continue;
            }

            var product = new IapProduct
            {
                Sku = sku!,
                DisplayName = (string?)node["display_name"] ?? sku!,
                PriceUsdCents = priceCents,
                StarsGranted = (int?)node["stars"] ?? 0,
                Kind = (bool?)node["subscription"] == true ? "subscription"
                     : (bool?)node["one_time"] == true ? "nonconsumable"
                     : "consumable",
                BonusFlags = node["extras"] is JArray extras
                    ? extras.ToObject<string[]>() ?? Array.Empty<string>()
                    : Array.Empty<string>(),
            };
            _products.Add(product);
        }
    }

    /// <summary>Look up a single SKU; returns null on miss.</summary>
    public IapProduct? Find(string sku)
    {
        for (var i = 0; i < _products.Count; i++)
            if (string.Equals(_products[i].Sku, sku, StringComparison.Ordinal)) return _products[i];
        return null;
    }
}
