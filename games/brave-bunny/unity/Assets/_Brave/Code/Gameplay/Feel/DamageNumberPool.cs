#nullable enable
// Brave Bunny — Hit Feedback Juice
// Pool of floating damage-number widgets. Builds on the existing GenericPool<T> pattern
// (ADR-0005, Pooling/GenericPool.cs) — pre-warmed; non-growing by default; zero
// allocations on the steady-state hit path.

using UnityEngine;

using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Feel
{
    /// <summary>
    /// Pool host for <see cref="DamageNumberWidget"/> instances. Pre-warms at Awake
    /// (Initialise) and hands out free widgets to <see cref="DamageNumberSpawner"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageNumberPool : MonoBehaviour
    {
        [SerializeField] private DamageNumberWidget? _prefab;
        [SerializeField] private int _capacity = 32;
        [SerializeField] private Transform? _parent;
        [SerializeField] private bool _allowGrowth;

        private GenericPool<DamageNumberWidget>? _pool;

        public int Capacity => _pool?.Capacity ?? 0;
        public int InUse => _pool?.InUse ?? 0;

        /// <summary>Code-driven setup (tests + bootstrap). Idempotent.</summary>
        public void Initialise(DamageNumberWidget prefab, int capacity, Transform? parent = null, bool allowGrowth = false)
        {
            _prefab = prefab;
            _capacity = capacity;
            _parent = parent != null ? parent : transform;
            _allowGrowth = allowGrowth;
            _pool = new GenericPool<DamageNumberWidget>(prefab, capacity, _parent, allowGrowth);
        }

        private void Awake()
        {
            if (_pool != null) return;          // already Initialised in code
            if (_prefab == null) { enabled = false; return; }
            _pool = new GenericPool<DamageNumberWidget>(_prefab, _capacity, _parent != null ? _parent : transform, _allowGrowth);
        }

        /// <summary>Acquire a widget from the pool. Throws if exhausted and growth is disabled.</summary>
        public DamageNumberWidget Acquire()
        {
            if (_pool == null) throw new System.InvalidOperationException("DamageNumberPool not initialised.");
            return _pool.Acquire();
        }

        /// <summary>Return a widget to the pool. Safe to call with a null/unowned widget.</summary>
        public void Release(DamageNumberWidget widget)
        {
            if (_pool == null || widget == null) return;
            _pool.Release(widget);
        }
    }
}
