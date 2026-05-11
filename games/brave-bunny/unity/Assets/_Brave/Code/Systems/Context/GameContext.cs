// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/09-event-bus.md (service locator pattern; NOT a singleton)
// Owner: systems-engineer

#nullable enable

using System;
using System.Collections.Generic;

namespace Brave.Systems.Context;

/// <summary>
/// App-lifetime service locator. Constructed once by
/// <see cref="GameContextBootstrap"/> in <c>Boot.unity</c> and passed to every
/// consumer via constructor injection or <c>[SerializeField]</c>. There is no
/// static <c>Instance</c> property — by design (see 09-event-bus.md, "banned
/// patterns" table). Lookup is O(1) by interface type.
/// </summary>
public sealed class GameContext
{
    private readonly Dictionary<Type, object> _services = new(capacity: 32);

    /// <summary>Register a service implementation under interface <typeparamref name="T"/>.</summary>
    public void Register<T>(T impl) where T : class
    {
        if (impl is null) throw new ArgumentNullException(nameof(impl));
        _services[typeof(T)] = impl;
    }

    /// <summary>Resolve a registered service. Throws if missing — wiring bugs fail loud at Boot.</summary>
    public T Get<T>() where T : class
    {
        if (!_services.TryGetValue(typeof(T), out var raw))
        {
            throw new InvalidOperationException(
                $"GameContext: service {typeof(T).FullName} not registered. " +
                "Check GameContextBootstrap.Awake() wiring order.");
        }
        return (T)raw;
    }

    /// <summary>Soft resolve. Returns false instead of throwing when unregistered.</summary>
    public bool TryGet<T>(out T impl) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var raw))
        {
            impl = (T)raw;
            return true;
        }
        impl = null!;
        return false;
    }

    /// <summary>Test-only: enumerate every registered service for diagnostics or shutdown sweeps.</summary>
    public IEnumerable<object> AllServices()
    {
        foreach (var kv in _services) yield return kv.Value;
    }

    /// <summary>Clear all registrations. Intended for EditMode tests, not production.</summary>
    public void Clear() => _services.Clear();
}
