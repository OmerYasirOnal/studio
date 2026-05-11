// Brave Bunny — Systems / Progression
// Tech spec: docs/06-tech-spec/09-event-bus.md (CurrencyChangedChannel — Tier 3 SO channel)
//            docs/06-tech-spec/03-save-system.md (save trigger after every meta-loop currency change)
// CLAUDE.md principle 6: no magic numbers — exchange rates etc. come from data/balance/economy.json
// (consumed by callers; this service is a thin save-backed bank only).

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

public interface ICurrencyService : IService
{
    long Get(CurrencyType type);
    void Add(CurrencyType type, long delta, bool persist = false);
    bool TrySpend(CurrencyType type, long amount, bool persist = true);

    /// <summary>
    /// (type, newTotal, delta). Subscribed to by:
    /// - UI home-screen currency widgets (animations)
    /// - SaveService for trigger-driven persistence on meta-loop deltas
    /// - AnalyticsService for currency_delta events
    /// </summary>
    event Action<CurrencyType, long, long>? Changed;
}

/// <summary>
/// Wraps <see cref="CurrencyWallet"/> with explicit save-trigger control:
/// in-run gem pickups should batch (<c>persist:false</c>) and only flush at
/// RunEnd, whereas IAP/achievement claims persist immediately (per
/// 03-save-system.md trigger list). Forwards <see cref="Changed"/> to mirror
/// the <c>CurrencyChangedChannel</c> SO event in 09-event-bus.md.
/// </summary>
public sealed class CurrencyService : ICurrencyService
{
    private readonly ISaveService _save;
    private readonly CurrencyWallet _wallet;

    public event Action<CurrencyType, long, long>? Changed;

    public CurrencyService(ISaveService save, CurrencyWallet wallet)
    {
        _save = save;
        _wallet = wallet;
        _wallet.OnChanged += OnWalletChanged;
    }

    public long Get(CurrencyType type) => _wallet.Get(type);

    public void Add(CurrencyType type, long delta, bool persist = false)
    {
        if (delta == 0) return;
        _wallet.Add(type, delta);
        if (persist) _save.Save();
    }

    public bool TrySpend(CurrencyType type, long amount, bool persist = true)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (!_wallet.TrySpend(type, amount)) return false;
        if (persist) _save.Save();
        return true;
    }

    private void OnWalletChanged(CurrencyType type, long total, long delta) =>
        Changed?.Invoke(type, total, delta);
}
