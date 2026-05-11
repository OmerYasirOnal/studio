// ADR-0005: pickup pools (XP gem / coin / heart) — one pool per kind.
using Brave.Gameplay.Events;
using UnityEngine;

namespace Brave.Gameplay.Pooling;

public sealed class PickupPool
{
    private readonly GenericPool<Pickup> _pool;
    public PickupKind Kind { get; }

    public PickupPool(PickupKind kind, Pickup prefab, int capacity, Transform parent)
    {
        Kind = kind;
        _pool = new GenericPool<Pickup>(prefab, capacity, parent);
    }

    public Pickup Drop(Vector3 position, int amount)
    {
        var p = _pool.Acquire();
        p.transform.position = position;
        p.Configure(Kind, amount, this);
        return p;
    }

    public void Release(Pickup p) => _pool.Release(p);
}

public sealed class Pickup : MonoBehaviour, IPoolable
{
    public PickupKind Kind { get; private set; }
    public int Amount { get; private set; }
    private PickupPool _owner;

    public void Configure(PickupKind kind, int amount, PickupPool owner)
    {
        Kind = kind; Amount = amount; _owner = owner;
    }

    public void Acquire() { /* reset visuals */ }
    public void Release() { _owner = null; Amount = 0; }

    /// <summary>Called by magnet logic when the player picks the item up.</summary>
    public void OnCollected() => _owner?.Release(this);
}
