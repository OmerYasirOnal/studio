#nullable enable
// ADR-0005 + tech-spec 05 § Pool pre-warm. Pre-allocated; never grows during run.
// Used by projectiles, enemies, pickups, VFX. Sizes from data/balance/pool-sizes.json.

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Brave.Gameplay.Pooling
{
    /// <summary>
    /// Generic component pool. The prefab must carry a <see cref="MonoBehaviour"/> implementing
    /// <see cref="IPoolable"/>. Capacity is fixed at warm-up; <see cref="Get"/> never instantiates
    /// past capacity (returns null instead so callers can drop the request).
    /// </summary>
    public sealed class ObjectPool<T> where T : MonoBehaviour, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform? _parent;
        private readonly Stack<T> _idle;
        private readonly int _capacity;
        private int _activeCount;

        public int Capacity => _capacity;
        public int ActiveCount => _activeCount;
        public int IdleCount => _idle.Count;

        public ObjectPool(T prefab, int capacity, Transform? parent = null)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            _prefab = prefab;
            _parent = parent;
            _capacity = capacity;
            _idle = new Stack<T>(capacity);
        }

        /// <summary>Instantiate all instances up-front. Called from RunIntro.OnEnter.</summary>
        public void Warm()
        {
            for (int i = _idle.Count + _activeCount; i < _capacity; i++)
            {
                T inst = UnityEngine.Object.Instantiate(_prefab, _parent);
                inst.gameObject.SetActive(false);
                _idle.Push(inst);
            }
        }

        /// <summary>Returns a fresh instance or null if pool is exhausted.</summary>
        public T? Get()
        {
            if (_idle.Count == 0)
            {
                if (_activeCount >= _capacity) return null;     // hard cap; never grow
                T inst = UnityEngine.Object.Instantiate(_prefab, _parent);
                _activeCount++;
                inst.OnGetFromPool();
                return inst;
            }

            T pooled = _idle.Pop();
            _activeCount++;
            pooled.OnGetFromPool();
            return pooled;
        }

        public void Return(T instance)
        {
            if (instance == null) return;
            instance.OnReturnToPool();
            _idle.Push(instance);
            _activeCount--;
        }

        /// <summary>Destroy every instance. Called at RunEnd exit.</summary>
        public void Teardown()
        {
            while (_idle.Count > 0)
                UnityEngine.Object.Destroy(_idle.Pop().gameObject);
            _activeCount = 0;
        }
    }
}
