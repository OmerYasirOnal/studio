// Brave Bunny — Systems / IAP / IapPurchaseFlow
// Wave 9: mediates between Shop UI → IapService → grants delivery → SaveService.
//
// Flow:
//   1. UI calls TryPurchase(sku) (probably from a button click in ShopController).
//   2. Flow checks the one-time-purchase guard (for nonconsumable SKUs already in
//      SaveData.PurchaseReceipts → blocks duplicate buy of ad-removal etc.).
//   3. Flow delegates to IIapService.PurchaseProduct (mocked in Editor, real on
//      device once Unity IAP listener lands post-soft-launch).
//   4. On Success: parses Grants[], applies them (currency / character unlock /
//      flags), persists a receipt token (<sku>_<utc>) into SaveData, calls Save.
//   5. On Cancelled / Failed: no-op, the OnComplete callback receives the result.
//
// Receipt format is an opaque string sufficient for one-time-purchase guards
// locally; the platform-issued receipts are stored separately by Unity IAP
// once the live listener is wired (post-soft-launch).
//
// Tech spec: docs/06-tech-spec/03-save-system.md (Save trigger: IAP confirmed)
//            docs/02-gdd/09-monetization-design.md (grant ladder)
// ADR-0010: Monthly Bunny Card grant cadence ($4.99 / 30 days / 1050 stars).

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using UnityEngine;

namespace Brave.Systems.Iap
{
    /// <summary>Result envelope returned to UI / tests after a TryPurchase call.</summary>
    public readonly struct IapPurchaseOutcome
    {
        public readonly PurchaseResult Result;
        public readonly string Sku;
        public readonly string ReceiptId;
        public readonly string? FailureReason;

        public IapPurchaseOutcome(PurchaseResult result, string sku, string receiptId, string? reason = null)
        {
            Result = result;
            Sku = sku ?? string.Empty;
            ReceiptId = receiptId ?? string.Empty;
            FailureReason = reason;
        }

        public bool Success => Result == PurchaseResult.Success;
    }

    /// <summary>
    /// Hooks consumed by <see cref="IapPurchaseFlow"/> to deliver SKU grants.
    /// Tests inject a no-op implementation so they don't need a full progression
    /// stack; production binds <see cref="ProductionPurchaseGrants"/> at boot.
    /// </summary>
    public interface IPurchaseGrantSink
    {
        void GrantStars(int amount);
        void GrantCarrots(int amount);
        void UnlockCharacter(string slug);
        void SetRemoveAds(bool removed);
        void SetBattlePassPremium(bool premium);
    }

    /// <summary>Production sink that routes grants through the live services.</summary>
    public sealed class ProductionPurchaseGrants : IPurchaseGrantSink
    {
        private readonly ICurrencyService _currency;
        private readonly ISaveService _save;

        public ProductionPurchaseGrants(ICurrencyService currency, ISaveService save)
        {
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
            _save = save ?? throw new ArgumentNullException(nameof(save));
        }

        public void GrantStars(int amount)
        {
            if (amount <= 0) return;
            // IAP grants persist immediately per 03-save-system.md trigger list.
            _currency.Add(CurrencyType.Stars, amount, persist: true);
        }

        public void GrantCarrots(int amount)
        {
            if (amount <= 0) return;
            _currency.Add(CurrencyType.Carrots, amount, persist: true);
        }

        public void UnlockCharacter(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return;
            if (!_save.Data.Characters.TryGetValue(slug, out var profile))
            {
                profile = new CharacterProfile { Level = 1, Xp = 0 };
                _save.Data.Characters[slug] = profile;
            }
            profile.Owned = true;
            profile.Unlocked = true;
            profile.UnlockedAt = DateTime.UtcNow.ToString("o");
            _save.Save();
        }

        public void SetRemoveAds(bool removed)
        {
            // No dedicated SaveData flag yet — the no-ads state is derived from
            // PurchaseReceipts containing the "removeAds" SKU. AdsService reads
            // the receipt list at boot. See ShopController.HasPurchased.
            // (Persisting the receipt itself is done by IapPurchaseFlow.)
        }

        public void SetBattlePassPremium(bool premium)
        {
            _save.Data.BattlePass.PremiumOwned = premium;
            _save.Save();
        }
    }

    /// <summary>
    /// Coordinates the purchase pipeline. Stateless aside from a handful of
    /// service references — safe to construct once at boot.
    /// </summary>
    public sealed class IapPurchaseFlow
    {
        private readonly IIapService _iap;
        private readonly ISaveService _save;
        private readonly IPurchaseGrantSink _grants;
        private readonly IapCatalogConfig? _catalogConfig;

        /// <summary>Raised on the final outcome of every <see cref="TryPurchase"/> call.</summary>
        public event Action<IapPurchaseOutcome>? PurchaseCompleted;

        public IapPurchaseFlow(
            IIapService iap,
            ISaveService save,
            IPurchaseGrantSink grants,
            IapCatalogConfig? catalogConfig = null)
        {
            _iap = iap ?? throw new ArgumentNullException(nameof(iap));
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _grants = grants ?? throw new ArgumentNullException(nameof(grants));
            _catalogConfig = catalogConfig;
        }

        /// <summary>True iff the local receipts log already contains a token for this SKU.</summary>
        public bool HasPurchased(string sku)
        {
            if (string.IsNullOrEmpty(sku)) return false;
            var receipts = _save.Data.PurchaseReceipts;
            if (receipts == null) return false;
            for (var i = 0; i < receipts.Count; i++)
            {
                var r = receipts[i];
                if (string.IsNullOrEmpty(r)) continue;
                // Receipt format: "<sku>_<utc_timestamp>" — match on the prefix.
                if (r.Length > sku.Length && r[sku.Length] == '_' && r.StartsWith(sku, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Kick off a purchase. The flow short-circuits to
        /// <see cref="PurchaseResult.Failed"/> with "alreadyOwned" if the SKU is
        /// a one-time non-consumable already in the receipts log.
        /// </summary>
        public void TryPurchase(string sku, Action<IapPurchaseOutcome>? onComplete = null)
        {
            if (string.IsNullOrEmpty(sku))
            {
                Complete(new IapPurchaseOutcome(PurchaseResult.Failed, sku ?? string.Empty, string.Empty, "emptySku"), onComplete);
                return;
            }

            var product = ResolveProduct(sku);
            if (product == null)
            {
                Complete(new IapPurchaseOutcome(PurchaseResult.Failed, sku, string.Empty, "unknownSku"), onComplete);
                return;
            }

            if (product.IsOneTime && HasPurchased(sku))
            {
                Complete(new IapPurchaseOutcome(PurchaseResult.Failed, sku, string.Empty, "alreadyOwned"), onComplete);
                return;
            }

            _iap.PurchaseProduct(sku, (result, resolved) =>
            {
                // The IapService stub in editor returns Success — production
                // listener returns Success | Cancelled | Failed | Pending.
                if (result != PurchaseResult.Success)
                {
                    Complete(new IapPurchaseOutcome(result, sku, string.Empty, result.ToString()), onComplete);
                    return;
                }

                // 1. Deliver grants encoded in the SKU.
                ApplyGrants(product);

                // 2. Persist a local receipt (opaque <sku>_<utc>).
                var receipt = $"{sku}_{DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}";
                _save.Data.PurchaseReceipts ??= new List<string>();
                _save.Data.PurchaseReceipts.Add(receipt);
                _save.Save();

                Complete(new IapPurchaseOutcome(PurchaseResult.Success, sku, receipt), onComplete);
            });
        }

        // ---------- helpers ----------

        private IapProduct? ResolveProduct(string sku)
        {
            // Prefer the SO (Editor-authored, richest fields). Fall back to the
            // IapService runtime catalog which was loaded from economy.json.
            var fromSo = _catalogConfig?.Find(sku);
            if (fromSo != null) return fromSo;
            for (var i = 0; i < _iap.Catalog.Count; i++)
                if (_iap.Catalog[i].Sku == sku) return _iap.Catalog[i];
            return null;
        }

        private void ApplyGrants(IapProduct product)
        {
            // Stars implied by the row's StarsGranted field — preserve legacy
            // economy.json semantics even when Grants[] is empty.
            if (product.StarsGranted > 0) _grants.GrantStars(product.StarsGranted);

            if (product.Grants == null) return;
            for (var i = 0; i < product.Grants.Length; i++)
            {
                var token = product.Grants[i];
                if (string.IsNullOrEmpty(token)) continue;
                ApplyGrantToken(token);
            }
        }

        private void ApplyGrantToken(string token)
        {
            // Format: "<kind>[:<arg>]". Unknown tokens log and skip — safer than
            // throwing mid-purchase after money has changed hands.
            var sepIdx = token.IndexOf(':');
            var kind = sepIdx < 0 ? token : token.Substring(0, sepIdx);
            var arg = sepIdx < 0 ? string.Empty : token.Substring(sepIdx + 1);

            switch (kind)
            {
                case "stars":
                    if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) && s > 0)
                        _grants.GrantStars(s);
                    break;
                case "carrots":
                    if (int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var c) && c > 0)
                        _grants.GrantCarrots(c);
                    break;
                case "character":
                    if (!string.IsNullOrEmpty(arg)) _grants.UnlockCharacter(arg);
                    break;
                case "removeAds":
                    _grants.SetRemoveAds(true);
                    break;
                case "battlePassPremium":
                    _grants.SetBattlePassPremium(true);
                    break;
                default:
                    Debug.LogWarning($"[IapPurchaseFlow] Unknown grant token '{token}' — skipped.");
                    break;
            }
        }

        private void Complete(IapPurchaseOutcome outcome, Action<IapPurchaseOutcome>? onComplete)
        {
            onComplete?.Invoke(outcome);
            PurchaseCompleted?.Invoke(outcome);
        }
    }
}
