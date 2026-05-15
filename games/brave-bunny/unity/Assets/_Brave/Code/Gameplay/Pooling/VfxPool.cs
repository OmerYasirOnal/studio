// ADR-0005: VFX particle-system pool.
// ADR-0019 follow-up: VFX return-to-pool is driven by ParticleSystem.OnParticleSystemStopped
// (event-driven) instead of a hardcoded timeout. Non-ParticleSystem prefabs (e.g. sprite-flash)
// fall back to a configurable timeout with a one-time warning per VfxPool key.
using System.Collections;
using UnityEngine;

namespace Brave.Gameplay.Pooling;

/// <summary>
/// Wraps a <see cref="ParticleSystem"/>-driven VFX prefab. Adapter MonoBehaviour
/// (<see cref="PooledVfx"/>) makes the particle system <see cref="IPoolable"/>.
/// </summary>
public sealed class VfxPool
{
    private readonly GenericPool<PooledVfx> _pool;
    private bool _fallbackWarned;
    public string Key { get; }

    /// <summary>Fallback lifetime used when the prefab has no ParticleSystem. Per-pool so
    /// sprite-flash effects with different lifetimes can be configured separately.</summary>
    public float FallbackLifetimeSeconds { get; }

    public VfxPool(string key, PooledVfx prefab, int capacity, Transform parent,
        float fallbackLifetimeSeconds = 1.0f)
    {
        Key = key;
        FallbackLifetimeSeconds = fallbackLifetimeSeconds;
        _pool = new GenericPool<PooledVfx>(prefab, capacity, parent);
    }

    public PooledVfx Play(Vector3 position, Quaternion rotation = default)
    {
        var v = _pool.Acquire();
        v.transform.SetPositionAndRotation(position, rotation);
        bool psPath = v.PlayAndAutoRelease(() => _pool.Release(v), FallbackLifetimeSeconds);
        if (!psPath && !_fallbackWarned)
        {
            _fallbackWarned = true;
            Debug.LogWarning(
                $"VfxPool '{Key}': prefab has no ParticleSystem — falling back to {FallbackLifetimeSeconds:0.###}s timeout. " +
                "Add a ParticleSystem (with main.stopAction=Callback handled by PooledVfx) to drive event-based release.");
        }
        return v;
    }
}

/// <summary>Particle-system wrapper that auto-releases on completion.
/// <para>Primary path: a <see cref="ParticleSystem"/> with <c>main.stopAction = Callback</c>
/// invokes <see cref="OnParticleSystemStopped"/> when emission finishes, which fires the
/// pooled <c>onComplete</c> callback. This is allocation-free (Unity dispatches the
/// SendMessage internally).</para>
/// <para>Fallback path: when no ParticleSystem is present (e.g. a sprite-flash effect),
/// <see cref="PlayAndAutoRelease"/> starts a single coroutine that waits the configured
/// timeout and then fires the callback. VfxPool logs a one-time warning on first use.</para></summary>
public sealed class PooledVfx : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _ps;
    private System.Action _onComplete;
    private Coroutine _fallbackRoutine;

    private void Reset()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void Awake()
    {
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        if (_ps != null)
        {
            // ADR-0019 follow-up: drive completion via callback, not a hardcoded timeout.
            var main = _ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }
    }

    public void Acquire() { /* no-op; PlayAndAutoRelease drives lifecycle */ }

    public void Release()
    {
        if (_fallbackRoutine != null)
        {
            StopCoroutine(_fallbackRoutine);
            _fallbackRoutine = null;
        }
        _onComplete = null;
        if (_ps != null) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // IPoolable contract.
    public void OnGetFromPool() { Acquire(); }
    public void OnReturnToPool() { Release(); }

    /// <summary>Plays the VFX and arranges for <paramref name="onComplete"/> to fire once
    /// the effect has finished. Returns <c>true</c> when the event-driven (ParticleSystem)
    /// path is used; <c>false</c> when the fallback timeout path is used.</summary>
    public bool PlayAndAutoRelease(System.Action onComplete, float fallbackLifetimeSeconds)
    {
        _onComplete = onComplete;
        if (_ps != null)
        {
            // Ensure the callback fires — Awake already set this but a hot-swapped PS or
            // a prefab variant could have a default-stop action. Idempotent.
            var main = _ps.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            _ps.Play();
            return true;
        }

        // Fallback: no ParticleSystem (e.g. sprite-flash effect). Use a coroutine timeout.
        if (_fallbackRoutine != null) StopCoroutine(_fallbackRoutine);
        _fallbackRoutine = StartCoroutine(FallbackRelease(fallbackLifetimeSeconds));
        return false;
    }

    private IEnumerator FallbackRelease(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        yield return new WaitForSeconds(seconds);
        _fallbackRoutine = null;
        InvokeComplete();
    }

    /// <summary>Unity message: invoked when the ParticleSystem stops (because
    /// main.stopAction == Callback). This is the event-driven release path.</summary>
    private void OnParticleSystemStopped()
    {
        // Guard: ignore stop-events when no callback is armed (Release already fired,
        // or Stop was called as part of pool teardown).
        InvokeComplete();
    }

    /// <summary>Test seam: directly trigger the OnParticleSystemStopped path without
    /// constructing a live ParticleSystem. EditMode tests use this to assert the
    /// callback flow without spinning up PlayMode. Public because the test assembly
    /// is a separate asmdef from Brave.Gameplay (no InternalsVisibleTo wired).</summary>
    public void Test_SimulateParticleStopped() => InvokeComplete();

    private void InvokeComplete()
    {
        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }
}
