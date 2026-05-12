#nullable enable
// Wave 4 vertical-slice projectile pool. Owns a pre-warmed ObjectPool<Projectile> and
// exposes a Spawn/Return surface the AutoAttackController consumes directly. ADR-0005:
// every spawnable is pool-recycled; no Instantiate/Destroy after Awake's warm-up.
//
// Naming: this is the Carrot-Boomerang-specific pool today; when Sunbeam / Daisy Mine
// land they get their own component-pool wrappers (per dispatch: "or `Carrot`-specific
// is fine for now"). A future refactor unifies under a single ProjectileSpawner once the
// shared cross-archetype API solidifies.

using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour wrapper around <see cref="ObjectPool{T}"/> for <see cref="Projectile"/>.
    /// Pre-instantiates <see cref="initialPoolSize"/> projectiles on Awake; subsequent
    /// <see cref="Spawn"/> / <see cref="Return"/> calls are allocation-free.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CarrotProjectilePool : MonoBehaviour
    {
        // UI/perf constant — pool capacity. Not a balance value. Sized for the worst-case
        // vertical-slice firing pattern: ~1 cast/sec × max-projectile-lifetime (a few seconds)
        // × splash levels still leaves plenty of headroom at 64.
        [SerializeField] private int initialPoolSize = 64;

        [SerializeField] private Projectile? projectilePrefab;

        private ObjectPool<Projectile>? _pool;
        private readonly List<Projectile> _activeScratch = new(capacity: 64);

        public int Capacity => _pool?.Capacity ?? 0;
        public int ActiveCount => _pool?.ActiveCount ?? 0;
        public int IdleCount => _pool?.IdleCount ?? 0;

        /// <summary>True when the pool is warmed and ready to spawn.</summary>
        public bool IsReady => _pool != null;

        private void Awake()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError(
                    $"{nameof(CarrotProjectilePool)}: projectilePrefab not assigned — disabling.",
                    this);
                enabled = false;
                return;
            }
            InitialiseInternal(projectilePrefab, initialPoolSize, transform);
        }

        /// <summary>Runtime injection — used by the boot composition root + EditMode tests
        /// (which can't rely on Awake firing on a manually-constructed GameObject).</summary>
        public void Initialise(Projectile prefab, int capacity, Transform? parent = null)
        {
            projectilePrefab = prefab;
            initialPoolSize = capacity;
            InitialiseInternal(prefab, capacity, parent ?? transform);
        }

        private void InitialiseInternal(Projectile prefab, int capacity, Transform parent)
        {
            _pool = new ObjectPool<Projectile>(prefab, capacity, parent);
            _pool.Warm();
            // Bind the pool to every pre-warmed projectile so Despawn() can return cleanly.
            // We pop all idle into the scratch list, bind, then push them back.
            int count = _pool.IdleCount;
            for (int i = 0; i < count; i++)
            {
                Projectile? p = _pool.Get();
                if (p == null) break;
                p.BindPool(_pool);
                _activeScratch.Add(p);
            }
            for (int i = _activeScratch.Count - 1; i >= 0; i--)
                _pool.Return(_activeScratch[i]);
            _activeScratch.Clear();
        }

        /// <summary>Acquire a projectile from the pool and launch it. Returns null if the pool
        /// is exhausted (which is treated as a content-config bug per ObjectPool.Get docs —
        /// callers may log and drop the shot).</summary>
        public Projectile? Spawn(Vector3 position, Vector3 direction, float speed, float damage, float lifetime)
        {
            if (_pool == null) return null;
            Projectile? p = _pool.Get();
            if (p == null) return null;
            p.LaunchLinear(position, direction, speed, damage, lifetime);
            return p;
        }

        /// <summary>Return a projectile to the pool. Safe to call multiple times; the pool
        /// stack ignores duplicates only at the ObjectPool layer (calling-side discipline).</summary>
        public void Return(Projectile p) => _pool?.Return(p);
    }
}
