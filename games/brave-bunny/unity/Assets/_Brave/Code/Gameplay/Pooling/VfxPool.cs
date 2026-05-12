// ADR-0005: VFX particle-system pool.
using UnityEngine;

namespace Brave.Gameplay.Pooling;

/// <summary>
/// Wraps a <see cref="ParticleSystem"/>-driven VFX prefab. Adapter MonoBehaviour
/// (<see cref="PooledVfx"/>) makes the particle system <see cref="IPoolable"/>.
/// </summary>
public sealed class VfxPool
{
    private readonly GenericPool<PooledVfx> _pool;
    public string Key { get; }

    public VfxPool(string key, PooledVfx prefab, int capacity, Transform parent)
    {
        Key = key;
        _pool = new GenericPool<PooledVfx>(prefab, capacity, parent);
    }

    public PooledVfx Play(Vector3 position, Quaternion rotation = default)
    {
        var v = _pool.Acquire();
        v.transform.SetPositionAndRotation(position, rotation);
        v.PlayAndAutoRelease(() => _pool.Release(v));
        return v;
    }
}

/// <summary>Particle-system wrapper that auto-releases on completion.</summary>
[RequireComponent(typeof(ParticleSystem))]
public sealed class PooledVfx : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _ps;
    private System.Action _onComplete;

    private void Reset() => _ps = GetComponent<ParticleSystem>();

    public void Acquire() { /* no-op; PlayAndAutoRelease drives lifecycle */ }
    public void Release() { _onComplete = null; if (_ps != null) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }

    // IPoolable contract.
    public void OnGetFromPool() { Acquire(); }
    public void OnReturnToPool() { Release(); }

    public void PlayAndAutoRelease(System.Action onComplete)
    {
        _onComplete = onComplete;
        _ps.Play();
        // TODO(Phase 5): drive completion via OnParticleSystemStopped + main.stopAction = Callback.
    }
}
