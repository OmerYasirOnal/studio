// QA — EnemyPoolReturnOnDeath EditMode tests (ADR-0019 item 3).
// Subject under test: Brave.Gameplay.Combat.EnemyPoolReturnOnDeath
// Specs: ADR-0005 (pool contract), ADR-0019 item 3 (enemy death → pool return).
// What we verify:
//   * OnDeath returns the enemy to the pool (InUse count drops).
//   * After OnDeath, the enemy's GameObject is inactive.
//   * Calling OnDeath with a null pool gracefully deactivates instead of crashing.
//   * A second OnDeath call after the first (post-release) does not double-release.
// Pattern: construct real Enemy + EnemyPoolReturnOnDeath MonoBehaviours;
//          build a minimal EnemyPool with capacity 1 so counts are easy to assert.

using Brave.Gameplay.Combat;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class EnemyPoolReturnTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const int PoolCapacity = 1;
        private const float BaseHp = 20f;
        private static readonly Vector3 SpawnPos = new(0f, 0f, 0f);

        private GameObject _prefabGo = null!;
        private Enemy _prefab = null!;
        private GameObject _poolRootGo = null!;
        private EnemyPool _pool = null!;
        private EnemyDefinition _def = null!;

        [SetUp]
        public void SetUp()
        {
            // Create a minimal enemy prefab (inactive, as Unity prefabs are).
            _prefabGo = new GameObject("Test_EnemyPrefab");
            _prefab = _prefabGo.AddComponent<Enemy>();
            // Add the pool-return listener component to the prefab so every acquired
            // instance carries it.
            _prefabGo.AddComponent<EnemyPoolReturnOnDeath>();
            _prefabGo.SetActive(false);

            _poolRootGo = new GameObject("Test_EnemyPoolRoot");

            // EnemyDefinition — only slug/baseHP needed for configure.
            _def = ScriptableObject.CreateInstance<EnemyDefinition>();
            _def.slug = "test-swarmer";
            _def.baseHP = BaseHp;

            _pool = new EnemyPool(_def.slug, _prefab, PoolCapacity, _poolRootGo.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_poolRootGo != null) Object.DestroyImmediate(_poolRootGo);
            if (_prefabGo != null) Object.DestroyImmediate(_prefabGo);
            if (_def != null) Object.DestroyImmediate(_def);
        }

        // ---- helpers ----

        /// <summary>Acquire + configure one enemy from the pool, returning the component.</summary>
        private Enemy AcquireEnemy()
        {
            var e = _pool.Acquire(SpawnPos);
            // Injecting null behavior is fine — we only test pool return, not AI ticks.
            e.Configure(_def, BaseHp, behavior: null!, _pool);
            // Wire the pool-return component (simulating the spawner's Initialise call).
            var ret = e.GetComponent<EnemyPoolReturnOnDeath>();
            ret?.Initialise(_pool);
            return e;
        }

        // ---- Tests ----

        [Test]
        public void OnDeath_ReturnsEnemyToPool_InUseDropsToZero()
        {
            var e = AcquireEnemy();
            Assert.That(_pool.InUse, Is.EqualTo(1), "pre-condition: enemy is in use");

            var ret = e.GetComponent<EnemyPoolReturnOnDeath>();
            Assert.That(ret, Is.Not.Null, "prefab must carry EnemyPoolReturnOnDeath");
            ret!.OnDeath(e.gameObject);

            Assert.That(_pool.InUse, Is.EqualTo(0),
                "pool InUse must drop to 0 after pool return");
        }

        [Test]
        public void OnDeath_DeactivatesEnemyGameObject()
        {
            var e = AcquireEnemy();
            Assert.That(e.gameObject.activeSelf, Is.True, "pre-condition: enemy is active");

            var ret = e.GetComponent<EnemyPoolReturnOnDeath>();
            ret!.OnDeath(e.gameObject);

            Assert.That(e.gameObject.activeSelf, Is.False,
                "enemy GameObject must be inactive after returning to pool");
        }

        [Test]
        public void OnDeath_WithoutPoolInitialise_DeactivatesWithoutCrash()
        {
            // A component with no Initialise call (e.g. pool reference not injected)
            // must fall back to deactivating the GameObject rather than throwing.
            var go = new GameObject("Test_Orphan");
            go.AddComponent<Enemy>();
            var ret = go.AddComponent<EnemyPoolReturnOnDeath>();
            // Deliberately skip Initialise — _pool is null.
            go.SetActive(true);

            Assert.DoesNotThrow(() => ret.OnDeath(go),
                "OnDeath must not throw when pool reference is missing");
            Assert.That(go.activeSelf, Is.False,
                "fallback deactivation must fire when pool reference is null");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void OnDeath_CalledTwice_DoesNotDoubleRelease()
        {
            // After the first OnDeath the component nulls its pool ref. A second call
            // must silently no-op (deactivate path), not throw or corrupt the pool.
            var e = AcquireEnemy();
            var ret = e.GetComponent<EnemyPoolReturnOnDeath>();
            ret!.OnDeath(e.gameObject);

            Assert.That(_pool.InUse, Is.EqualTo(0), "pre-condition: already released");

            // Second call — must not crash or produce negative InUse.
            Assert.DoesNotThrow(() => ret.OnDeath(e.gameObject),
                "second OnDeath call must not throw");
            Assert.That(_pool.InUse, Is.GreaterThanOrEqualTo(0),
                "pool InUse must remain non-negative after redundant OnDeath call");
        }
    }
}
