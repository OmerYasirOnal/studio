// ADR-0005: pooling is mandatory for every spawnable. Zero allocations in run hot loop.
using System.Collections.Generic;
using UnityEngine;

namespace Brave.Gameplay.Pooling;

/// <summary>
/// Type-T object pool for Unity Components. Pre-warmed at <c>RunIntro</c> entry per
/// tech-spec 08; pool size from <c>data/balance/pool-sizes.json</c>. Pool is non-growing
/// by default — overflows are treated as a bug, not silently expanded.
/// </summary>
public sealed class GenericPool<T> where T : Component, IPoolable
{
    private readonly Stack<T> _free;
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly bool _allowGrowth;

    public int Capacity { get; }
    public int InUse { get; private set; }

    public GenericPool(T prefab, int capacity, Transform parent = null, bool allowGrowth = false)
    {
        _prefab = prefab;
        _parent = parent;
        Capacity = capacity;
        _allowGrowth = allowGrowth;
        _free = new Stack<T>(capacity);
        PreWarm(capacity);
    }

    private void PreWarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var inst = Object.Instantiate(_prefab, _parent);
            inst.gameObject.SetActive(false);
            _free.Push(inst);
        }
    }

    public T Acquire()
    {
        T inst;
        if (_free.Count > 0)
        {
            inst = _free.Pop();
        }
        else if (_allowGrowth)
        {
            inst = Object.Instantiate(_prefab, _parent);
        }
        else
        {
            throw new System.InvalidOperationException(
                $"Pool<{typeof(T).Name}> exhausted at capacity {Capacity}; expand pool-sizes.json or set allowGrowth.");
        }

        inst.gameObject.SetActive(true);
        inst.Acquire();
        InUse++;
        return inst;
    }

    public void Release(T inst)
    {
        if (inst == null) return;
        inst.Release();
        inst.gameObject.SetActive(false);
        if (_parent != null) inst.transform.SetParent(_parent, worldPositionStays: false);
        _free.Push(inst);
        InUse--;
    }
}
