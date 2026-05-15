// Brave Bunny — Systems / IAP / IapCatalogConfig
// ScriptableObject companion to the existing ProductCatalog (which sources rows
// from data/balance/economy.json). This SO is the canonical Editor-friendly
// version designers iterate on — Shop UI binds against it directly. The two
// catalogs are kept in sync by Editor.BalanceJsonImporter (Wave 9 follow-up).
//
// CLAUDE.md principle 6 (no magic numbers): all SKUs, prices, and grants live
// here or in economy.json — never inline in shop UI / purchase-flow code.
//
// Tech spec: docs/02-gdd/09-monetization-design.md (SKU lineup + price ladder)
//            docs/06-tech-spec/03-save-system.md (purchase → SaveService.Save())
// ADR-0010 caps Monthly Bunny Card at $4.99 / 1050 stars over 30 days.

#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Brave.Systems.Iap
{
    /// <summary>
    /// Tab buckets the Shop UI groups products under. The order here is also
    /// the visual order in Shop.uxml — keep in sync with the tabbar buttons.
    /// </summary>
    public enum IapCategory
    {
        Currency = 0,
        Characters = 1,
        Specials = 2,
        BattlePass = 3,
    }

    /// <summary>
    /// Editor-edited catalog of in-app products. Loaded at boot by IapService
    /// (via <c>LoadCatalog(config.Products)</c>) and consumed by ShopController.
    /// Mirrors the JSON catalog row-for-row; the SO is authoritative inside the
    /// Unity Editor so designers can iterate without round-tripping economy.json.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/IAP/IapCatalogConfig", fileName = "IapCatalogConfig", order = 20)]
    public sealed class IapCatalogConfig : ScriptableObject
    {
        /// <summary>Hard cap from 08-economy.md / ADR-0010. The Editor validator rejects above.</summary>
        public const int MaxPriceUsdCents = 1999;

        [Tooltip("All purchasable SKUs. Order is the per-tab display order in Shop.uxml.")]
        public List<IapProduct> Products = new();

        /// <summary>Filter helper used by the Shop UI to populate a tab.</summary>
        public IEnumerable<IapProduct> ForCategory(IapCategory category)
        {
            for (var i = 0; i < Products.Count; i++)
            {
                if (Products[i] == null) continue;
                if (Products[i].Category == category) yield return Products[i];
            }
        }

        /// <summary>Look up a product by SKU; returns null on miss.</summary>
        public IapProduct? Find(string sku)
        {
            if (string.IsNullOrEmpty(sku)) return null;
            for (var i = 0; i < Products.Count; i++)
            {
                if (Products[i] != null && Products[i].Sku == sku) return Products[i];
            }
            return null;
        }

#if UNITY_EDITOR
        // Editor-side guardrail mirroring ProductCatalog.LoadFromJson validation.
        // Triggers on any inspector edit; surfaces price-cap or duplicate-SKU drift
        // before the asset ships.
        private void OnValidate()
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < Products.Count; i++)
            {
                var p = Products[i];
                if (p == null) continue;
                if (p.PriceUsdCents > MaxPriceUsdCents)
                    Debug.LogWarning($"[IapCatalogConfig] SKU '{p.Sku}' priced ${p.PriceUsdCents / 100f:F2} exceeds $19.99 cap.");
                if (!string.IsNullOrEmpty(p.Sku) && !seen.Add(p.Sku))
                    Debug.LogWarning($"[IapCatalogConfig] Duplicate SKU '{p.Sku}' at index {i}.");
            }
        }
#endif
    }
}
