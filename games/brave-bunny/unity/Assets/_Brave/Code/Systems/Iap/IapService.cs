// Brave Bunny — Systems / IAP
// CLAUDE.md allows Unity IAP (revenue side, not generation side).
// Editor mode is NoOp — returns Success immediately so dev flow is unblocked.
// Tech spec: 03-save-system.md trigger — "IAP purchase confirmed" → SaveService.Save().

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;

namespace Brave.Systems.Iap;

public interface IIapService : IService
{
    IReadOnlyList<IapProduct> Catalog { get; }
    void LoadCatalog(IEnumerable<IapProduct> products);
    void PurchaseProduct(string sku, Action<PurchaseResult, IapProduct?> onComplete);
    void RestorePurchases(Action<PurchaseResult> onComplete);
}

/// <summary>
/// Thin facade over Unity IAP. The real implementation lives behind a
/// platform define (UNITY_PURCHASING). Editor build short-circuits to
/// <see cref="PurchaseResult.Success"/> so designers can iterate the IAP
/// surfaces without sandbox accounts.
/// </summary>
public sealed class IapService : IIapService
{
    private const int MaxPriceUsdCents = 1999; // Hard cap per 08-economy.md.

    private readonly List<IapProduct> _catalog = new();

    public IReadOnlyList<IapProduct> Catalog => _catalog;

    public void LoadCatalog(IEnumerable<IapProduct> products)
    {
        _catalog.Clear();
        foreach (var p in products)
        {
            if (p.PriceUsdCents > MaxPriceUsdCents)
                throw new InvalidOperationException($"IapService: SKU {p.Sku} priced above $19.99 cap.");
            _catalog.Add(p);
        }
    }

    public void PurchaseProduct(string sku, Action<PurchaseResult, IapProduct?> onComplete)
    {
#if UNITY_EDITOR
        var product = _catalog.Find(p => p.Sku == sku);
        onComplete(PurchaseResult.Success, product);
#else
        throw new NotImplementedException("Wire Unity IAP store listener post-Phase-5.");
#endif
    }

    public void RestorePurchases(Action<PurchaseResult> onComplete)
    {
#if UNITY_EDITOR
        onComplete(PurchaseResult.Success);
#else
        throw new NotImplementedException("Wire Unity IAP restore flow post-Phase-5.");
#endif
    }
}
