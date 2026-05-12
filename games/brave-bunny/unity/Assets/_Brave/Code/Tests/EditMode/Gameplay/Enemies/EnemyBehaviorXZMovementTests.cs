// QA — Enemy behaviour XZ-plane movement tests (ADR-0018).
// Subjects under test:
//   * Brave.Gameplay.Enemies.SwarmerBehavior.Tick
//   * Brave.Gameplay.Enemies.EliteBehavior.Tick
//   * Brave.Gameplay.Enemies.RangedBehavior.Tick
//   * Brave.Gameplay.Enemies.TankBehavior.Tick
// Specs: ADR-0018 (XZ-plane migration), ADR-0017 (PlayerMover canonical),
//        docs/06-tech-spec/05-runtime-architecture.md § Camera convention.
// Why: pre-migration helper wrote to (pos.x, pos.y) — incompatible with the XZ ground-plane
//      camera. Migration inlines the math with input.x → world.x, input.y → world.z.
// Notes:
//  * EnemyBehavior.Tick takes a Vector2 playerPos in caller-space; the world is XZ. Tests
//    confirm the migrated bodies map that 2D vector onto the XZ plane (no Y writes).
//  * Construction uses ScriptableObject.CreateInstance + new GameObject — valid in EditMode.
//    Each test cleans up its scratch objects to avoid bleed between tests.
//  * No magic numbers: move-speeds and dt are explicit named constants per CLAUDE.md
//    principle 6.

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Enemies
{
    [TestFixture]
    public class EnemyBehaviorXZMovementTests
    {
        // ---- constants (no magic numbers) ----
        private const float MoveSpeed = 2.5f;        // EnemyDefinition.moveSpeed default
        private const float Dt = 1f / 30f;           // EnemyTicker runs at 30 Hz per tech-spec 05
        private const float Epsilon = 0.0001f;
        // RangedBehavior tuning — picked so a player two units away is OUTSIDE the kite ring
        // AND ABOVE the fire-window minimum, forcing a "close gap" move (non-zero delta).
        private const float RangedKiteDistance = 1f;
        private const float RangedFireWindowMin = 1.2f;
        private const float RangedFireWindowMax = 1.5f;
        private const float RangedTelegraphMs = 250f;
        private const float RangedProjectileSpeed = 6f;
        // Tank tuning — values are inert for the homing-step assertion below; any non-zero suffice.
        private const float TankChargeIntervalMs = 4000f;
        private const float TankBurstSpeedMult = 2f;
        private const float TankBurstDurationMs = 600f;
        private const float TankTelegraphMs = 250f;
        // Elite tuning.
        private const float EliteTelegraphMs = 500f;

        private EnemyDefinition? _def;
        private GameObject? _enemyGo;
        private Enemy? _enemy;

        [SetUp]
        public void SetUp()
        {
            _def = ScriptableObject.CreateInstance<EnemyDefinition>();
            _def.slug = "test-enemy";
            _def.moveSpeed = MoveSpeed;

            _enemyGo = new GameObject("TestEnemy");
            _enemy = _enemyGo.AddComponent<Enemy>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_enemyGo != null) Object.DestroyImmediate(_enemyGo);
            if (_def != null) Object.DestroyImmediate(_def);
        }

        // ---- Helpers ----

        /// <summary>Configure the enemy with a behaviour and place at origin.</summary>
        private void Place(EnemyBehavior behavior, Vector3 worldPos)
        {
            Assert.That(_enemy, Is.Not.Null);
            Assert.That(_enemyGo, Is.Not.Null);
            _enemyGo!.transform.position = worldPos;
            _enemy!.Configure(_def!, scaledHp: 10f, behavior, owner: null!);
        }

        /// <summary>Assert that a single Tick moved the enemy on the XZ plane only.</summary>
        private void AssertSingleTickIsXZ(Vector2 playerPos, Vector3 startPos)
        {
            Vector3 after = _enemyGo!.transform.position;
            Vector3 delta = after - startPos;

            Assert.That(delta.y, Is.EqualTo(0f).Within(Epsilon),
                $"Y-axis write detected — XZ migration regressed (delta={delta})");

            // Direction sanity: enemy should have moved TOWARD the player on XZ (homing).
            // playerPos.x → world.x; playerPos.y → world.z.
            Vector2 want = new(playerPos.x - startPos.x, playerPos.y - startPos.z);
            float dot = delta.x * want.x + delta.z * want.y;
            Assert.That(dot, Is.GreaterThan(0f),
                $"Step did not advance toward player on XZ (delta={delta}, want={want})");

            // Step magnitude bounded by speed*dt (single normalized step).
            Assert.That(delta.magnitude, Is.LessThanOrEqualTo(MoveSpeed * Dt + Epsilon),
                $"Step exceeded speed*dt cap (mag={delta.magnitude})");
        }

        // ---- SwarmerBehavior ----

        [Test]
        public void Swarmer_MovesTowardPlayerOnXZPlane_NoYWrite()
        {
            // Player at world (3, 0, 4) → caller-space (3, 4); enemy starts at origin.
            // Diagonal homing — both x and z deltas must be non-zero and positive.
            Vector3 start = Vector3.zero;
            Place(new SwarmerBehavior(), start);
            Vector2 playerPos = new(3f, 4f);

            _enemy!.Tick(playerPos, Dt);

            AssertSingleTickIsXZ(playerPos, start);
            Vector3 delta = _enemyGo!.transform.position - start;
            Assert.That(delta.x, Is.GreaterThan(0f), "Expected +X step toward player");
            Assert.That(delta.z, Is.GreaterThan(0f), "Expected +Z step toward player (input Y maps to world Z)");
        }

        [Test]
        public void Swarmer_AtPlayerPosition_NoMove()
        {
            // Sanity: when self ≈ player, the early-return short-circuits — no NaN, no move.
            Vector3 start = new(2f, 0f, 5f);
            Place(new SwarmerBehavior(), start);
            Vector2 playerPos = new(2f, 5f);   // matches start.x/start.z

            _enemy!.Tick(playerPos, Dt);

            Vector3 after = _enemyGo!.transform.position;
            Assert.That(after, Is.EqualTo(start));
        }

        // ---- EliteBehavior ----

        [Test]
        public void Elite_MovesTowardPlayerOnXZPlane_NoYWrite()
        {
            Vector3 start = new(-1f, 0f, 2f);
            Place(new EliteBehavior(EliteTelegraphMs), start);
            Vector2 playerPos = new(5f, 6f);

            _enemy!.Tick(playerPos, Dt);
            AssertSingleTickIsXZ(playerPos, start);
        }

        // ---- RangedBehavior ----

        [Test]
        public void Ranged_BeyondFireWindow_ClosesGapOnXZPlane_NoYWrite()
        {
            // Distance from enemy(0,0,0) to player(2,0,2)-ish via Vector2 = √(4+4)=2.83 ≈ 2.83 units,
            // > _fireWindowMax (1.5) → "close gap" mode → enemy steps toward player.
            Vector3 start = Vector3.zero;
            Place(
                new RangedBehavior(
                    RangedKiteDistance,
                    new Vector2(RangedFireWindowMin, RangedFireWindowMax),
                    RangedTelegraphMs,
                    RangedProjectileSpeed),
                start);
            Vector2 playerPos = new(2f, 2f);

            _enemy!.Tick(playerPos, Dt);
            AssertSingleTickIsXZ(playerPos, start);
        }

        [Test]
        public void Ranged_InsideKiteRing_BacksAwayOnXZPlane_NoYWrite()
        {
            // Enemy at (0,0,0), player at (0.3, 0, 0.4) → caller-space (0.3, 0.4), distance = 0.5
            // < _kiteDistance (1.0) → enemy moves AWAY from player; delta dot (player-self) < 0.
            Vector3 start = Vector3.zero;
            Place(
                new RangedBehavior(
                    RangedKiteDistance,
                    new Vector2(RangedFireWindowMin, RangedFireWindowMax),
                    RangedTelegraphMs,
                    RangedProjectileSpeed),
                start);
            Vector2 playerPos = new(0.3f, 0.4f);

            _enemy!.Tick(playerPos, Dt);

            Vector3 after = _enemyGo!.transform.position;
            Vector3 delta = after - start;
            Assert.That(delta.y, Is.EqualTo(0f).Within(Epsilon), "Y-axis write detected (kite mode)");
            // Backing away: delta projected onto (player - self) is negative.
            float dot = delta.x * playerPos.x + delta.z * playerPos.y;
            Assert.That(dot, Is.LessThan(0f),
                $"Kite mode should retreat from player on XZ (delta={delta})");
        }

        // ---- TankBehavior ----

        [Test]
        public void Tank_MovesTowardPlayerOnXZPlane_NoYWrite()
        {
            Vector3 start = new(4f, 0f, -3f);
            Place(
                new TankBehavior(
                    TankChargeIntervalMs,
                    TankBurstSpeedMult,
                    TankBurstDurationMs,
                    TankTelegraphMs),
                start);
            Vector2 playerPos = new(-2f, 5f);

            _enemy!.Tick(playerPos, Dt);
            AssertSingleTickIsXZ(playerPos, start);
        }
    }
}
