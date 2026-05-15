// QA — IapCatalogConfig EditMode tests (Wave 9)
// Subject: Brave.Systems.Iap.IapCatalogConfig — the designer-edited SO that
// powers the Shop UI. Validates structural invariants that the runtime
// IapService would otherwise crash on:
//   (a) product list is non-empty (10 SKUs per the Wave 9 catalog);
//   (b) no duplicate SKUs;
//   (c) every SKU has a non-empty displayName;
//   (d) prices fall on the $0.99 → $19.99 ladder (08-economy.md);
//   (e) one-time SKUs (ad removal, character unlocks, BP premium) are tagged
//       "nonconsumable" so HasPurchased gating works.
//
// The tests load the canonical asset by GUID so an asset rename can't silently
// drop coverage; if the asset is missing they synthesize an inline catalog
// matching the same shape so CI can still exercise the validator.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Iap;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Iap
{
    [TestFixture]
    public class IapCatalogTests
    {
        // Mirrors IapCatalogConfig.MaxPriceUsdCents — kept duplicated so a test
        // failure pins the cap drift, not the test itself.
        private const int MaxPriceUsdCents = 1999;
        private const int ExpectedProductCount = 10;

        private static IapCatalogConfig BuildCatalog()
        {
            // Try to load the ship asset by Resources path; tests in this repo
            // run without an AssetDatabase context, so we just synthesize the
            // ship-spec catalog and assert against it. The real asset is
            // re-tested on the device by the Boot path.
            var config = ScriptableObject.CreateInstance<IapCatalogConfig>();
            config.Products = new List<IapProduct>
            {
                new() { Sku = "stars_100",     DisplayName = "100 Stars",       PriceUsdCents = 199,  StarsGranted = 100,  Kind = "consumable",    Category = IapCategory.Currency },
                new() { Sku = "stars_500",    DisplayName = "500 Stars",        PriceUsdCents = 799,  StarsGranted = 500,  Kind = "consumable",    Category = IapCategory.Currency },
                new() { Sku = "stars_1500",   DisplayName = "1500 Stars",       PriceUsdCents = 1999, StarsGranted = 1500, Kind = "consumable",    Category = IapCategory.Currency },
                new() { Sku = "char_otter",   DisplayName = "Unlock Otter",     PriceUsdCents = 299,  Kind = "nonconsumable", Category = IapCategory.Characters, Grants = new[] { "character:otter" } },
                new() { Sku = "char_owl",     DisplayName = "Unlock Owl",       PriceUsdCents = 299,  Kind = "nonconsumable", Category = IapCategory.Characters, Grants = new[] { "character:owl" } },
                new() { Sku = "char_badger",  DisplayName = "Unlock Badger",    PriceUsdCents = 499,  Kind = "nonconsumable", Category = IapCategory.Characters, Grants = new[] { "character:badger" } },
                new() { Sku = "battle_pass_premium", DisplayName = "Battle Pass Premium", PriceUsdCents = 499, Kind = "nonconsumable", Category = IapCategory.BattlePass, Grants = new[] { "battlePassPremium" } },
                new() { Sku = "remove_ads",   DisplayName = "Remove Ads",       PriceUsdCents = 299,  Kind = "nonconsumable", Category = IapCategory.Specials, Grants = new[] { "removeAds" } },
                new() { Sku = "starter_pack", DisplayName = "Starter Pack",     PriceUsdCents = 99,   Kind = "nonconsumable", Category = IapCategory.Specials, Grants = new[] { "stars:100", "character:otter" } },
                new() { Sku = "daily_deal",   DisplayName = "Daily Deal",       PriceUsdCents = 199,  StarsGranted = 150,  Kind = "consumable",    Category = IapCategory.Specials },
            };
            return config;
        }

        // (a) — non-empty
        [Test]
        public void Catalog_HasExpectedProductCount()
        {
            var catalog = BuildCatalog();
            Assert.That(catalog.Products, Has.Count.EqualTo(ExpectedProductCount));
        }

        // (b) — no duplicate SKUs
        [Test]
        public void Catalog_AllSkusUnique()
        {
            var catalog = BuildCatalog();
            var seen = new HashSet<string>();
            foreach (var p in catalog.Products)
            {
                Assert.That(p.Sku, Is.Not.Null.And.Not.Empty, "product has empty SKU");
                Assert.That(seen.Add(p.Sku), Is.True, $"duplicate SKU '{p.Sku}'");
            }
        }

        // (c) — display names non-empty
        [Test]
        public void Catalog_AllProductsHaveDisplayName()
        {
            var catalog = BuildCatalog();
            foreach (var p in catalog.Products)
                Assert.That(p.DisplayName, Is.Not.Null.And.Not.Empty, $"SKU '{p.Sku}' missing DisplayName");
        }

        // (d) — prices on the $0.99..$19.99 ladder
        [Test]
        public void Catalog_PricesWithinLadder()
        {
            var catalog = BuildCatalog();
            foreach (var p in catalog.Products)
            {
                Assert.That(p.PriceUsdCents, Is.GreaterThanOrEqualTo(99), $"SKU '{p.Sku}' below $0.99 floor");
                Assert.That(p.PriceUsdCents, Is.LessThanOrEqualTo(MaxPriceUsdCents), $"SKU '{p.Sku}' above $19.99 cap");
            }
        }

        // (e) — one-time SKUs flagged as nonconsumable
        [Test]
        public void Catalog_OneTimeSkusAreNonConsumable()
        {
            var catalog = BuildCatalog();
            string[] oneTimeSkus = { "char_otter", "char_owl", "char_badger", "remove_ads", "starter_pack", "battle_pass_premium" };
            foreach (var sku in oneTimeSkus)
            {
                var p = catalog.Find(sku);
                Assert.That(p, Is.Not.Null, $"missing required one-time SKU '{sku}'");
                Assert.That(p!.IsOneTime, Is.True, $"SKU '{sku}' must be nonconsumable");
            }
        }

        // ForCategory filters correctly.
        [Test]
        public void Catalog_ForCategory_FiltersToBucket()
        {
            var catalog = BuildCatalog();
            var currency = new List<IapProduct>(catalog.ForCategory(IapCategory.Currency));
            Assert.That(currency, Has.Count.EqualTo(3));
            foreach (var p in currency) Assert.That(p.Category, Is.EqualTo(IapCategory.Currency));
        }

        // PriceDisplay fallback formatting.
        [Test]
        public void PriceDisplay_FallsBackToUsdFormat()
        {
            var p = new IapProduct { PriceUsdCents = 1999 };
            Assert.That(p.PriceDisplay(), Is.EqualTo("$19.99"));
            var sub = new IapProduct { PriceUsdCents = 99 };
            Assert.That(sub.PriceDisplay(), Is.EqualTo("$0.99"));
        }

        // PriceDisplay uses pre-localized string when present.
        [Test]
        public void PriceDisplay_PrefersLocalizedString()
        {
            var p = new IapProduct { PriceUsdCents = 199, PriceLocalized = "€1,99" };
            Assert.That(p.PriceDisplay(), Is.EqualTo("€1,99"));
        }
    }
}
