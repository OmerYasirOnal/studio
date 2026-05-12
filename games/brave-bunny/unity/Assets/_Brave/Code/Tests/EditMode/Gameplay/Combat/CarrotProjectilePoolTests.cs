// QA — CarrotProjectilePool EditMode tests (Wave 4 vertical slice).
// Subject under test: Brave.Gameplay.Combat.CarrotProjectilePool
// Specs: ADR-0005 (pre-warmed pool; no Instantiate/Destroy on the hot path),
//        docs/06-tech-spec/05-runtime-architecture.md § Pool sizes.
// What we verify here:
//   * Initialise() pre-warms exactly N projectiles (no growth past capacity)
//   * Spawn round-trips identity to Return — pool reuses the same instances
//   * Active/idle counts stay consistent across many Spawn/Return cycles
//   * No instantiation past Awake (steady-state Spawn does not allocate prefab clones)
// Pattern: EnemyBehaviorXZMovementTests precedent — construct a sacrificial GameObject as a
//          stand-in "prefab", Instantiate copies it. Cleanup in TearDown to avoid bleed.

using System.Collections.Generic;

using Brave.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class CarrotProjectilePoolTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const int PoolCapacity = 8;        // small enough for fast tests, large enough to exhaust
        private const float Speed = 10f;
        private const float Damage = 1.2f;
        private const float Lifetime = 0.5f;
        private static readonly Vector3 Origin = new(0f, 0f, 0f);
        private static readonly Vector3 Dir = new(1f, 0f, 0f);

        private GameObject? _prefabGo;
        private Projectile? _prefab;
        private GameObject? _poolGo;
        private CarrotProjectilePool? _pool;

        [SetUp]
        public void SetUp()
        {
            _prefabGo = new GameObject("Test_ProjectilePrefab");
            _prefab = _prefabGo.AddComponent<Projectile>();
            _prefabGo.SetActive(false);   // prefabs are typically inactive in the asset DB

            _poolGo = new GameObject("Test_CarrotProjectilePool");
            _pool = _poolGo.AddComponent<CarrotProjectilePool>();
            _pool.Initialise(_prefab, PoolCapacity, _poolGo.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_poolGo != null) Object.DestroyImmediate(_poolGo);
            if (_prefabGo != null) Object.DestroyImmediate(_prefabGo);
        }

        // ---- Warm-up ----

        [Test]
        public void Initialise_PreWarms_CapacityIdle()
        {
            // After Initialise + Awake-binding, all instances are returned to idle and the
            // pool reports the capacity as idle, zero active.
            Assert.That(_pool!.IsReady, Is.True);
            Assert.That(_pool.Capacity, Is.EqualTo(PoolCapacity));
            Assert.That(_pool.IdleCount, Is.EqualTo(PoolCapacity));
            Assert.That(_pool.ActiveCount, Is.EqualTo(0));
        }

        // ---- Spawn / Return ----

        [Test]
        public void Spawn_ReturnsActiveProjectile_AndAdvancesActiveCount()
        {
            Projectile? p = _pool!.Spawn(Origin, Dir, Speed, Damage, Lifetime);
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.gameObject.activeSelf, Is.True);
            Assert.That(_pool.ActiveCount, Is.EqualTo(1));
            Assert.That(_pool.IdleCount, Is.EqualTo(PoolCapacity - 1));
        }

        [Test]
        public void SpawnReturn_RoundTrip_PreservesIdentity()
        {
            // The same instance comes back out on the next Spawn — that's the whole point of
            // a pool. (Stack ordering: LIFO. Last-returned is first-acquired.)
            Projectile? a = _pool!.Spawn(Origin, Dir, Speed, Damage, Lifetime);
            _pool.Return(a!);
            Projectile? b = _pool.Spawn(Origin, Dir, Speed, Damage, Lifetime);
            Assert.That(b, Is.SameAs(a));
        }

        [Test]
        public void Spawn_ExhaustsAtCapacity_ThenReturnsNull()
        {
            // Acquire all `PoolCapacity` instances. The (PoolCapacity+1)-th must return null
            // (per ObjectPool.Get's hard-cap contract). Pool MUST NOT grow silently.
            var taken = new List<Projectile>(PoolCapacity);
            for (int i = 0; i < PoolCapacity; i++)
            {
                Projectile? p = _pool!.Spawn(Origin, Dir, Speed, Damage, Lifetime);
                Assert.That(p, Is.Not.Null, $"pool exhausted prematurely at i={i}");
                taken.Add(p!);
            }
            Projectile? overflow = _pool!.Spawn(Origin, Dir, Speed, Damage, Lifetime);
            Assert.That(overflow, Is.Null);
            Assert.That(_pool.ActiveCount, Is.EqualTo(PoolCapacity));
            // Cleanup so TearDown sees a sane state.
            foreach (var p in taken) _pool.Return(p);
        }

        [Test]
        public void SpawnReturn_SteadyState_ReusesSameInstances()
        {
            // Loop 4× capacity Spawn/Return cycles; the total distinct instances seen must
            // stay ≤ capacity (i.e., no growth, no leak).
            var seen = new HashSet<int>();
            const int Cycles = PoolCapacity * 4;
            for (int i = 0; i < Cycles; i++)
            {
                Projectile? p = _pool!.Spawn(Origin, Dir, Speed, Damage, Lifetime);
                Assert.That(p, Is.Not.Null);
                seen.Add(p!.GetInstanceID());
                _pool.Return(p);
            }
            Assert.That(seen.Count, Is.LessThanOrEqualTo(PoolCapacity),
                $"pool grew beyond capacity ({seen.Count} distinct instances across {Cycles} cycles)");
        }

        [Test]
        public void Spawn_AppliesPosition_AndDamage()
        {
            // Sanity: LaunchLinear plumbs position into the projectile transform and damage
            // into the public Damage surface (used by HitDetector / DamageApplier).
            Vector3 spawnPos = new(3f, 0f, -2f);
            Projectile? p = _pool!.Spawn(spawnPos, Dir, Speed, Damage, Lifetime);
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.transform.position, Is.EqualTo(spawnPos));
            Assert.That(p.Damage, Is.EqualTo(Damage).Within(0.0001f));
        }
    }
}
