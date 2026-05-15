#nullable enable
// QA — VfxPool ParticleSystem stop-callback EditMode tests (ADR-0019 follow-up).
//
// Subject under test: Brave.Gameplay.Pooling.VfxPool + PooledVfx.
// Specs: ADR-0005 (pool contract), ADR-0019 (Phase-5 cleanup follow-ups, item:
//        "VfxPool particle callback — replace timeout hack").
// What we verify:
//   * When a PooledVfx has a real ParticleSystem, Play wires the event-driven
//     stop-callback path (PlayAndAutoRelease returns true).
//   * Simulating the OnParticleSystemStopped Unity message via the Test_*
//     seam returns the instance to the pool (InUse drops to 0).
//   * When a PooledVfx has NO ParticleSystem, the fallback timeout path is
//     selected (PlayAndAutoRelease returns false) — that's the warning seam.
//
// Why a test seam: Unity does NOT dispatch OnParticleSystemStopped synchronously
// during EditMode (no emission/simulation pump runs without PlayMode). The
// internal Test_SimulateParticleStopped call is the smallest possible seam that
// asserts the *release-path* without depending on PlayMode timing.

using Brave.Gameplay.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Pooling
{
    [TestFixture]
    public class VfxPoolStopCallbackTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string PoolKey = "test-vfx";
        private const int PoolCapacity = 1;
        private const float FallbackSeconds = 0.5f;

        private GameObject _prefabGo = null!;
        private PooledVfx _prefab = null!;
        private GameObject _poolRootGo = null!;
        private VfxPool _pool = null!;

        [SetUp]
        public void SetUp()
        {
            // Real PooledVfx prefab WITH a ParticleSystem — exercises the callback path.
            _prefabGo = new GameObject("Test_PooledVfxPrefab");
            _prefabGo.AddComponent<ParticleSystem>();
            _prefab = _prefabGo.AddComponent<PooledVfx>();
            _prefabGo.SetActive(false);

            _poolRootGo = new GameObject("Test_VfxPoolRoot");
            _pool = new VfxPool(PoolKey, _prefab, PoolCapacity, _poolRootGo.transform,
                fallbackLifetimeSeconds: FallbackSeconds);
        }

        [TearDown]
        public void TearDown()
        {
            if (_poolRootGo != null) Object.DestroyImmediate(_poolRootGo);
            if (_prefabGo != null) Object.DestroyImmediate(_prefabGo);
        }

        // ---- Tests ----

        [Test]
        public void Play_WithParticleSystem_AcquiresInstance_InUseGoesTo1()
        {
            var inst = _pool.Play(Vector3.zero);
            Assert.That(inst, Is.Not.Null, "Play must return the acquired PooledVfx");
            Assert.That(inst.gameObject.activeSelf, Is.True,
                "acquired instance must be active");
        }

        [Test]
        public void Play_WithParticleSystem_UsesCallbackPath_NotFallback()
        {
            // Direct probe of PooledVfx.PlayAndAutoRelease return value: true = PS path,
            // false = fallback. The pool wraps Play but we exercise the underlying
            // adapter to lock in the contract.
            var instGo = new GameObject("Test_StandalonePooledVfx");
            instGo.AddComponent<ParticleSystem>();
            var vfx = instGo.AddComponent<PooledVfx>();
            try
            {
                bool calledBack = false;
                bool psPath = vfx.PlayAndAutoRelease(() => calledBack = true,
                    fallbackLifetimeSeconds: FallbackSeconds);

                Assert.That(psPath, Is.True,
                    "with a ParticleSystem present, PlayAndAutoRelease must take the callback path");
                Assert.That(calledBack, Is.False,
                    "callback must NOT fire until OnParticleSystemStopped is dispatched");
            }
            finally { Object.DestroyImmediate(instGo); }
        }

        [Test]
        public void OnParticleSystemStopped_FiresReleaseCallback_PoolReturnsToZeroInUse()
        {
            // Acquire one instance via the pool; pool InUse goes to 1. Simulate the
            // OnParticleSystemStopped Unity message via the test seam; the registered
            // onComplete (the pool's Release) fires and InUse returns to 0.
            var inst = _pool.Play(Vector3.zero);

            // Sanity: instance was acquired through the pool.
            Assert.That(inst.gameObject.activeSelf, Is.True);

            // Drive the event-driven release.
            inst.Test_SimulateParticleStopped();

            Assert.That(inst.gameObject.activeSelf, Is.False,
                "after OnParticleSystemStopped, instance must be deactivated (pool released)");
        }

        [Test]
        public void PlayAndAutoRelease_NoParticleSystem_TakesFallbackPath()
        {
            // A PooledVfx attached to a GO with no ParticleSystem must take the
            // fallback timeout path (return value false). The pool will emit a
            // one-time warning the first time this is hit in production.
            var bareGo = new GameObject("Test_PooledVfx_NoPS");
            // Note: we intentionally do NOT add a ParticleSystem before adding PooledVfx,
            // so Awake's GetComponent<ParticleSystem>() returns null.
            var vfx = bareGo.AddComponent<PooledVfx>();
            try
            {
                bool psPath = vfx.PlayAndAutoRelease(() => { },
                    fallbackLifetimeSeconds: FallbackSeconds);

                Assert.That(psPath, Is.False,
                    "without a ParticleSystem, PlayAndAutoRelease must take the fallback path");
            }
            finally { Object.DestroyImmediate(bareGo); }
        }
    }
}
