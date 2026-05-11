// Brave Bunny — Systems / Progression
// Design source: docs/02-gdd/08-economy.md (currency model, exchange-rate caveats)
// Tech spec: docs/06-tech-spec/09-event-bus.md (CurrencyChangedChannel — Tier 3)

#nullable enable

using System;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

/// <summary>
/// Thin wrapper over <see cref="SaveData.Currencies"/> that surfaces the
/// "currency changed" event used by the UI wallet tiles. Negative amounts
/// throw — overspend bugs fail loud.
/// </summary>
public sealed class CurrencyWallet
{
    private readonly SaveData.CurrenciesSection _currencies;

    public event Action<CurrencyType, long, long>? OnChanged; // (type, newTotal, delta)

    public CurrencyWallet(SaveData.CurrenciesSection currencies) { _currencies = currencies; }

    public long Get(CurrencyType type) => type switch
    {
        CurrencyType.Carrots => _currencies.Carrots,
        CurrencyType.Stars => _currencies.Stars,
        CurrencyType.SoulShards => _currencies.SoulShards,
        _ => 0,
    };

    public void Add(CurrencyType type, long delta)
    {
        if (delta == 0) return;
        var next = Get(type) + delta;
        if (next < 0) throw new InvalidOperationException($"CurrencyWallet: {type} would underflow ({next}).");
        Set(type, next);
        OnChanged?.Invoke(type, next, delta);
    }

    public bool TrySpend(CurrencyType type, long amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (Get(type) < amount) return false;
        Add(type, -amount);
        return true;
    }

    private void Set(CurrencyType type, long value)
    {
        switch (type)
        {
            case CurrencyType.Carrots: _currencies.Carrots = value; break;
            case CurrencyType.Stars: _currencies.Stars = value; break;
            case CurrencyType.SoulShards: _currencies.SoulShards = value; break;
        }
    }
}
