// Brave Bunny — Systems / IAP
// Design source: docs/02-gdd/08-economy.md (SKU price ladder: $0.99 → $19.99 cap)
// Catalog rows are owned jointly by:
//   * data/balance/economy.json  → parsed by ProductCatalog at boot (live ops)
//   * IapCatalogConfig.asset     → designer-edited SO consumed by Shop UI (Wave 9)
// Both surfaces map onto this single POCO; the SO holds extra UI-only fields
// (Category, Localized price, Grants) that the JSON ledger doesn't carry.

#nullable enable

using System;
using Newtonsoft.Json;

namespace Brave.Systems.Iap
{
    /// <summary>
    /// One row of the IAP catalog. Mirrors the price-ladder table in 08-economy.md.
    /// Stars-per-dollar is derived, not stored. Hard cap of $19.99 enforced by
    /// validation in <see cref="IapService"/> and <see cref="IapCatalogConfig"/>.
    /// </summary>
    [Serializable]
    public sealed class IapProduct
    {
        // ---------- Identity / pricing (shared with economy.json) ----------

        [JsonProperty("sku")] public string Sku = string.Empty;
        [JsonProperty("displayName")] public string DisplayName = string.Empty;

        /// <summary>USD price in cents — 99, 299, 499, 799, 999, 1999.</summary>
        [JsonProperty("priceUsdCents")] public int PriceUsdCents;

        /// <summary>Stars granted on successful purchase (currency packs + starter pack).</summary>
        [JsonProperty("starsGranted")] public int StarsGranted;

        /// <summary>"consumable" | "nonconsumable" | "subscription". Matches Unity IAP product types.</summary>
        [JsonProperty("kind")] public string Kind = "consumable";

        /// <summary>Misc tagging — surfaced as "extras" in economy.json.</summary>
        [JsonProperty("bonusFlags")] public string[] BonusFlags = Array.Empty<string>();

        // ---------- UI-only fields (populated by IapCatalogConfig SO) ----------

        /// <summary>Shop tab this SKU appears under (Currency / Characters / Specials / Battle Pass).</summary>
        [JsonProperty("category")] public IapCategory Category = IapCategory.Currency;

        /// <summary>
        /// Pre-formatted price string for the player's locale. When empty the UI
        /// falls back to <c>$X.YY</c> derived from <see cref="PriceUsdCents"/>.
        /// Wired by the platform IAP plugin post-soft-launch (TODO: store metadata).
        /// </summary>
        [JsonProperty("priceLocalized")] public string PriceLocalized = string.Empty;

        /// <summary>
        /// What the purchase grants. Each token is interpreted by IapPurchaseFlow:
        ///   "stars:N"           → grant N Stars
        ///   "carrots:N"         → grant N Carrots
        ///   "character:<slug>"  → unlock the given character slug
        ///   "removeAds"         → flip the no-ads flag
        ///   "battlePassPremium" → mark BP premium owned for current season
        /// </summary>
        [JsonProperty("grants")] public string[] Grants = Array.Empty<string>();

        // ---------- Convenience helpers ----------

        /// <summary>True for one-time / non-consumable SKUs (ad removal, character unlocks, BP premium).</summary>
        public bool IsOneTime => string.Equals(Kind, "nonconsumable", StringComparison.OrdinalIgnoreCase);

        /// <summary>True for repeatable consumable SKUs (currency packs, daily deals).</summary>
        public bool IsConsumable => string.Equals(Kind, "consumable", StringComparison.OrdinalIgnoreCase);

        /// <summary>USD price formatted as <c>$X.YY</c> — fallback when PriceLocalized is empty.</summary>
        public string PriceDisplay()
        {
            if (!string.IsNullOrEmpty(PriceLocalized)) return PriceLocalized;
            // TODO(loc): replace with platform-provided locale-aware string once
            // the live IAP plugin lands (post-soft-launch). For now this matches
            // the price-ladder formatting in 08-economy.md.
            var dollars = PriceUsdCents / 100;
            var cents = PriceUsdCents % 100;
            return $"${dollars}.{cents:D2}";
        }
    }
}
