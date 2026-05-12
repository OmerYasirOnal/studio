// QA — EnemyRegistry EditMode tests (ADR-0019 Wave 5A: XY→XZ distance migration).
// Subject under test:
//   * Brave.Gameplay.Enemies.EnemyRegistry.SnapshotActiveInRange
//   * Brave.Gameplay.Enemies.EnemyRegistry.FindFirstWithinRadius
//   * Brave.Gameplay.Enemies.EnemyRegistry.FindNearestWithinRadius
// Specs: ADR-0018 (XZ-plane migration), ADR-0019 (cross-plane bug closure).
// Why: pre-Wave-5A helpers computed `d.x²+d.y²` while the world (camera, player, projectile,
//      enemies) all operate on the XZ ground plane. The bug caused AutoAttack targeting to
//      query the wrong plane and miss every shot. These tests pin the XZ semantics.
// Pattern: register real EnemyBase MonoBehaviours (Swarmer concrete) against a clean registry
//          per test. Y-offsets are mixed in to prove the Y-axis is *ignored* (not folded in).

using System.Collections.Generic;
using Brave.Gameplay.Damage;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Enemies
{
    [TestFixture]
    public class EnemyRegistryTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float Epsilon = 0.0001f;
        private const float Radius = 5.0f;
        private const float ScaledHpForAlive = 10f;
        private const float ScaledContactDamage = 1f;
        private const float ScaledMoveSpeed = 1f;
        // A trickery-Y deliberately chosen to be larger than the test radius — if the registry
        // ever folded Y into the distance check again, this point would be "out of range" even
        // though its XZ distance is well inside.
        private const float TrickeryY = 10.0f;

        // Track scratch objects for guaranteed teardown.
        private readonly List<GameObject> _spawned = new();
        private EnemyDefinition? _def;

        [SetUp]
        public void SetUp()
        {
            EnemyRegistry.ResetAll();
            _def = ScriptableObject.CreateInstance<EnemyDefinition>();
            _def.slug = "test-enemy";
            _def.moveSpeed = ScaledMoveSpeed;
        }

        [TearDown]
        public void TearDown()
        {
            // Unregister + destroy. Order matters: OnReturnToPool unregisters; we then destroy.
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                if (_spawned[i] != null) Object.DestroyImmediate(_spawned[i]);
            }
            _spawned.Clear();
            EnemyRegistry.ResetAll();
            if (_def != null) Object.DestroyImmediate(_def);
            _def = null;
        }

        // ---- Helpers ----

        /// <summary>Spawn a Swarmer at <paramref name="worldPos"/>, register it, return the base.</summary>
        private EnemyBase Spawn(Vector3 worldPos)
        {
            var go = new GameObject($"E_{_spawned.Count}");
            go.transform.position = worldPos;
            go.AddComponent<EnemyHealth>();
            var swarmer = go.AddComponent<Swarmer>();
            // Awake on EnemyBase populates the health-field, but we still defensively wire it.
            swarmer.Configure(_def!, hero: go.transform, scaledHp: ScaledHpForAlive,
                scaledContactDamage: ScaledContactDamage, scaledMoveSpeed: ScaledMoveSpeed);
            EnemyRegistry.Register(swarmer);
            _spawned.Add(go);
            return swarmer;
        }

        // ---- FindFirstWithinRadius — XZ semantics ----

        [Test]
        public void FindFirstWithinRadius_EmptyRegistry_ReturnsNull()
        {
            var hit = EnemyRegistry.FindFirstWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.Null);
        }

        [Test]
        public void FindFirstWithinRadius_OutOfRange_ReturnsNull()
        {
            // Place at XZ distance 6 — outside the radius-5 ring.
            Spawn(new Vector3(6f, 0f, 0f));
            var hit = EnemyRegistry.FindFirstWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.Null);
        }

        [Test]
        public void FindFirstWithinRadius_WithinRange_ReturnsEnemy()
        {
            var inRange = Spawn(new Vector3(3f, 0f, 4f));      // XZ distance 5 — exactly on the ring
            var hit = EnemyRegistry.FindFirstWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.SameAs(inRange));
        }

        [Test]
        public void FindFirstWithinRadius_IgnoresYOffset()
        {
            // ADR-0019 regression guard. If the registry ever folds Y back into the distance
            // check, this enemy (XZ dist = 3, Y = 10) would be flagged as out-of-range.
            var inRange = Spawn(new Vector3(3f, TrickeryY, 0f));
            var hit = EnemyRegistry.FindFirstWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.SameAs(inRange),
                "Y-offset must not affect XZ-plane distance (ADR-0019).");
        }

        [Test]
        public void FindFirstWithinRadius_SkipsDeadEnemies()
        {
            var dead = Spawn(new Vector3(1f, 0f, 0f));
            // Kill via TakeHit (HP ≤ 0 → IsAlive flips false).
            dead.Health.TakeHit(new HitInfo(amount: ScaledHpForAlive + 1f,
                impactPosition: Vector3.zero, isCrit: false, sourceId: 0, targetId: 0));
            Assert.That(dead.Health.IsAlive, Is.False);

            var hit = EnemyRegistry.FindFirstWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.Null);
        }

        // ---- FindNearestWithinRadius — true-nearest XZ semantics ----

        [Test]
        public void FindNearestWithinRadius_PrefersClosestOnXZIgnoringY()
        {
            // far has a small Y but is at XZ distance 4
            var far = Spawn(new Vector3(4f, 0f, 0f));
            // near has a HUGE Y but is at XZ distance 2 — must be picked because Y is ignored
            var near = Spawn(new Vector3(2f, TrickeryY, 0f));

            var hit = EnemyRegistry.FindNearestWithinRadius(Vector3.zero, Radius);
            Assert.That(hit, Is.SameAs(near),
                "Nearest must be by XZ distance only (ADR-0019); Y-offset is irrelevant.");
            Assert.That(hit, Is.Not.SameAs(far));
        }

        [Test]
        public void FindNearestWithinRadius_EmptyRegistry_ReturnsNull()
        {
            Assert.That(EnemyRegistry.FindNearestWithinRadius(Vector3.zero, Radius), Is.Null);
        }

        [Test]
        public void FindNearestWithinRadius_AllOutOfRange_ReturnsNull()
        {
            Spawn(new Vector3(Radius + 1f, 0f, 0f));
            Spawn(new Vector3(0f, 0f, Radius + 2f));
            Assert.That(EnemyRegistry.FindNearestWithinRadius(Vector3.zero, Radius), Is.Null);
        }

        // ---- SnapshotActiveInRange — XZ semantics, scratch buffer contract ----

        [Test]
        public void SnapshotActiveInRange_ReturnsOnlyXZInRangeEnemies_IgnoringY()
        {
            var inA = Spawn(new Vector3(1f, 0f, 1f));               // XZ dist √2 < 5
            var inB = Spawn(new Vector3(0f, TrickeryY, 4f));        // XZ dist 4 < 5; huge Y irrelevant
            var outOfRange = Spawn(new Vector3(6f, 0f, 0f));        // XZ dist 6 > 5

            var buffer = new List<EnemyBase>(8);
            EnemyRegistry.SnapshotActiveInRange(Vector3.zero, Radius, buffer);

            Assert.That(buffer, Has.Count.EqualTo(2));
            Assert.That(buffer, Contains.Item(inA));
            Assert.That(buffer, Contains.Item(inB));
            Assert.That(buffer, Does.Not.Contain(outOfRange));
        }

        [Test]
        public void SnapshotActiveInRange_ClearsCallerBufferBeforeFilling()
        {
            // Caller-supplied buffer must be cleared (zero allocations contract).
            var buffer = new List<EnemyBase>(8) { /* pre-existing junk */ };
            // Add a stale GameObject reference (cast through Spawn so the entry is non-null).
            buffer.Add(Spawn(new Vector3(100f, 0f, 0f)));
            EnemyRegistry.ResetAll();   // wipe registry — query result must be empty

            EnemyRegistry.SnapshotActiveInRange(Vector3.zero, Radius, buffer);
            Assert.That(buffer, Is.Empty,
                "SnapshotActiveInRange must clear the caller's buffer before populating it.");
        }
    }
}
