// ADR-0005: projectile pool.
using Brave.Gameplay.Combat;
using UnityEngine;

namespace Brave.Gameplay.Pooling;

public sealed class ProjectilePool
{
    private readonly GenericPool<Projectile> _pool;
    public string WeaponSlug { get; }

    public ProjectilePool(string weaponSlug, Projectile prefab, int capacity, Transform parent)
    {
        WeaponSlug = weaponSlug;
        _pool = new GenericPool<Projectile>(prefab, capacity, parent);
    }

    public Projectile Acquire(Vector3 position, Vector2 direction)
    {
        var p = _pool.Acquire();
        p.transform.position = position;
        p.SetDirection(direction);
        return p;
    }

    public void Release(Projectile p) => _pool.Release(p);
}
