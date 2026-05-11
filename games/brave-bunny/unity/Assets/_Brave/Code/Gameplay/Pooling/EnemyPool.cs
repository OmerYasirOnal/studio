// ADR-0005: per-archetype enemy pool.
using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Pooling;

/// <summary>
/// Concrete pool for <see cref="Enemy"/>. One pool per enemy prefab (slug).
/// </summary>
public sealed class EnemyPool
{
    private readonly GenericPool<Enemy> _pool;
    public string Slug { get; }

    public EnemyPool(string slug, Enemy prefab, int capacity, Transform parent)
    {
        Slug = slug;
        _pool = new GenericPool<Enemy>(prefab, capacity, parent);
    }

    public Enemy Acquire(Vector3 position)
    {
        var e = _pool.Acquire();
        e.transform.position = position;
        return e;
    }

    public void Release(Enemy e) => _pool.Release(e);

    public int InUse => _pool.InUse;
    public int Capacity => _pool.Capacity;
}
