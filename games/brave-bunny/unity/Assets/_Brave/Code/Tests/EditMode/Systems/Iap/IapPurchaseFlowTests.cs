// QA — IapPurchaseFlow EditMode tests (Wave 9)
// Subject: Brave.Systems.Iap.IapPurchaseFlow
//
// Coverage:
//   (a) Successful purchase calls the grant sink for each token in Grants[];
//   (b) Successful purchase appends an opaque <sku>_<utc> receipt to SaveData;
//   (c) Cancelled / Failed purchase is a no-op (no grants, no receipt);
//   (d) Re-purchasing a one-time non-consumable is blocked by HasPurchased
//       (failure reason "alreadyOwned");
//   (e) Unknown SKU short-circuits to Failed without touching the IapService;
//   (f) PurchaseCompleted event fires exactly once per TryPurchase call.
//
// Uses InMemoryFileSystem so the SaveService roundtrips without disk I/O.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Iap;
using Brave.Systems.Save;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Iap
{
    [TestFixture]
    public class IapPurchaseFlowTests
    {
        private const string RootDir = "/virt/brave-iap";

        // ---- Fake collaborators ----

        private sealed class FakeIapService : IIapService
        {
            public PurchaseResult NextResult = PurchaseResult.Success;
            public int PurchaseCallCount;
            public string? LastSku;
            private readonly List<IapProduct> _catalog = new();
            public IReadOnlyList<IapProduct> Catalog => _catalog;

            public void LoadCatalog(IEnumerable<IapProduct> products)
            {
                _catalog.Clear();
                foreach (var p in products) _catalog.Add(p);
            }

            public void PurchaseProduct(string sku, System.Action<PurchaseResult, IapProduct?> onComplete)
            {
                PurchaseCallCount++;
                LastSku = sku;
                IapProduct? match = null;
                foreach (var p in _catalog) { if (p.Sku == sku) { match = p; break; } }
                onComplete(NextResult, match);
            }

            public void RestorePurchases(System.Action<PurchaseResult> onComplete) =>
                onComplete(PurchaseResult.Success);
        }

        private sealed class FakeGrants : IPurchaseGrantSink
        {
            public int StarsGranted;
            public int CarrotsGranted;
            public readonly List<string> CharactersUnlocked = new();
            public bool RemoveAdsSet;
            public bool BattlePassPremiumSet;

            public void GrantStars(int amount) => StarsGranted += amount;
            public void GrantCarrots(int amount) => CarrotsGranted += amount;
            public void UnlockCharacter(string slug) => CharactersUnlocked.Add(slug);
            public void SetRemoveAds(bool removed) => RemoveAdsSet = removed;
            public void SetBattlePassPremium(bool premium) => BattlePassPremiumSet = premium;
        }

        // ---- Fixtures ----

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;
        private FakeIapService _iap = null!;
        private FakeGrants _grants = null!;
        private IapCatalogConfig _catalog = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load();
            _iap = new FakeIapService();
            _grants = new FakeGrants();
            _catalog = ScriptableObject.CreateInstance<IapCatalogConfig>();
            _catalog.Products = new List<IapProduct>
            {
                new() { Sku = "stars_100",  PriceUsdCents = 199, StarsGranted = 100, Kind = "consumable",    Category = IapCategory.Currency },
                new() { Sku = "char_otter", PriceUsdCents = 299, Kind = "nonconsumable", Category = IapCategory.Characters, Grants = new[] { "character:otter" } },
                new() { Sku = "remove_ads", PriceUsdCents = 299, Kind = "nonconsumable", Category = IapCategory.Specials,   Grants = new[] { "removeAds" } },
                new() { Sku = "starter_pack", PriceUsdCents = 99, Kind = "nonconsumable", Category = IapCategory.Specials, Grants = new[] { "stars:100", "character:otter" } },
                new() { Sku = "bp_premium", PriceUsdCents = 499, Kind = "nonconsumable", Category = IapCategory.BattlePass, Grants = new[] { "battlePassPremium" } },
            };
            _iap.LoadCatalog(_catalog.Products);
        }

        private IapPurchaseFlow NewFlow() => new(_iap, _save, _grants, _catalog);

        // (a) — grant tokens applied on success
        [Test]
        public void Purchase_StarsPack_GrantsStarsFromStarsGrantedField()
        {
            var flow = NewFlow();
            IapPurchaseOutcome outcome = default;
            flow.TryPurchase("stars_100", o => outcome = o);

            Assert.That(outcome.Success, Is.True);
            Assert.That(_grants.StarsGranted, Is.EqualTo(100));
        }

        [Test]
        public void Purchase_StarterPack_GrantsAllTokens()
        {
            var flow = NewFlow();
            flow.TryPurchase("starter_pack");

            Assert.That(_grants.StarsGranted, Is.EqualTo(100));
            Assert.That(_grants.CharactersUnlocked, Has.Count.EqualTo(1));
            Assert.That(_grants.CharactersUnlocked[0], Is.EqualTo("otter"));
        }

        [Test]
        public void Purchase_BattlePassPremium_FlipsFlag()
        {
            var flow = NewFlow();
            flow.TryPurchase("bp_premium");
            Assert.That(_grants.BattlePassPremiumSet, Is.True);
        }

        [Test]
        public void Purchase_RemoveAds_FlipsFlag()
        {
            var flow = NewFlow();
            flow.TryPurchase("remove_ads");
            Assert.That(_grants.RemoveAdsSet, Is.True);
        }

        // (b) — receipt persisted
        [Test]
        public void Purchase_Success_AppendsReceiptToSaveData()
        {
            var flow = NewFlow();
            flow.TryPurchase("char_otter");

            Assert.That(_save.Data.PurchaseReceipts, Has.Count.EqualTo(1));
            Assert.That(_save.Data.PurchaseReceipts[0], Does.StartWith("char_otter_"));
        }

        [Test]
        public void Purchase_Success_PersistsSave()
        {
            var flow = NewFlow();
            flow.TryPurchase("char_otter");

            var reloaded = new SaveService(RootDir, _fs);
            reloaded.Load();
            Assert.That(reloaded.Data.PurchaseReceipts, Has.Count.EqualTo(1));
            Assert.That(reloaded.Data.PurchaseReceipts[0], Does.StartWith("char_otter_"));
        }

        // (c) — failed/cancelled purchase is a no-op
        [Test]
        public void Purchase_Cancelled_NoGrantsNoReceipt()
        {
            _iap.NextResult = PurchaseResult.Cancelled;
            var flow = NewFlow();
            IapPurchaseOutcome outcome = default;
            flow.TryPurchase("stars_100", o => outcome = o);

            Assert.That(outcome.Result, Is.EqualTo(PurchaseResult.Cancelled));
            Assert.That(_grants.StarsGranted, Is.EqualTo(0));
            Assert.That(_save.Data.PurchaseReceipts, Is.Empty);
        }

        [Test]
        public void Purchase_Failed_NoGrantsNoReceipt()
        {
            _iap.NextResult = PurchaseResult.Failed;
            var flow = NewFlow();
            flow.TryPurchase("stars_100");

            Assert.That(_grants.StarsGranted, Is.EqualTo(0));
            Assert.That(_save.Data.PurchaseReceipts, Is.Empty);
        }

        // (d) — one-time SKU blocked on second purchase
        [Test]
        public void Purchase_OneTimeAlreadyOwned_ShortCircuitsToFailed()
        {
            var flow = NewFlow();
            flow.TryPurchase("char_otter");                  // first time → success
            Assert.That(_iap.PurchaseCallCount, Is.EqualTo(1));

            IapPurchaseOutcome second = default;
            flow.TryPurchase("char_otter", o => second = o); // second time → blocked
            Assert.That(second.Result, Is.EqualTo(PurchaseResult.Failed));
            Assert.That(second.FailureReason, Is.EqualTo("alreadyOwned"));
            // FakeIapService.PurchaseProduct must NOT have been called again.
            Assert.That(_iap.PurchaseCallCount, Is.EqualTo(1));
            Assert.That(_save.Data.PurchaseReceipts, Has.Count.EqualTo(1));
        }

        [Test]
        public void HasPurchased_ReturnsFalseBeforeBuy_TrueAfter()
        {
            var flow = NewFlow();
            Assert.That(flow.HasPurchased("char_otter"), Is.False);
            flow.TryPurchase("char_otter");
            Assert.That(flow.HasPurchased("char_otter"), Is.True);
        }

        [Test]
        public void HasPurchased_ConsumableIsNotBlocked_CanBuyAgain()
        {
            var flow = NewFlow();
            flow.TryPurchase("stars_100");
            flow.TryPurchase("stars_100"); // consumable — should re-run

            Assert.That(_iap.PurchaseCallCount, Is.EqualTo(2));
            Assert.That(_grants.StarsGranted, Is.EqualTo(200));
        }

        // (e) — unknown SKU
        [Test]
        public void Purchase_UnknownSku_FailsWithoutCallingService()
        {
            var flow = NewFlow();
            IapPurchaseOutcome outcome = default;
            flow.TryPurchase("nonexistent", o => outcome = o);

            Assert.That(outcome.Result, Is.EqualTo(PurchaseResult.Failed));
            Assert.That(outcome.FailureReason, Is.EqualTo("unknownSku"));
            Assert.That(_iap.PurchaseCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Purchase_EmptySku_FailsImmediately()
        {
            var flow = NewFlow();
            IapPurchaseOutcome outcome = default;
            flow.TryPurchase(string.Empty, o => outcome = o);
            Assert.That(outcome.Result, Is.EqualTo(PurchaseResult.Failed));
            Assert.That(outcome.FailureReason, Is.EqualTo("emptySku"));
        }

        // (f) — completion event fires exactly once
        [Test]
        public void PurchaseCompleted_EventFiresOncePerCall()
        {
            var flow = NewFlow();
            var fired = 0;
            flow.PurchaseCompleted += _ => fired++;

            flow.TryPurchase("stars_100");
            Assert.That(fired, Is.EqualTo(1));

            flow.TryPurchase("char_otter");
            Assert.That(fired, Is.EqualTo(2));
        }
    }
}
