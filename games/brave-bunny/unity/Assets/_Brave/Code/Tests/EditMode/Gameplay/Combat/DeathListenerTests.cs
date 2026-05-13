// QA — IDeathListener / EnemyHealth death-notification tests (ADR-0019 item 3).
// Subject under test: Brave.Gameplay.Enemies.EnemyHealth (which holds the authoritative
//   IDeathListener chain) and Brave.Gameplay.Combat.DamageApplier.IsKillingBlow (the
//   pure-arithmetic idempotency guard — no MonoBehaviour needed for this half).
//
// Strategy:
//   * EnemyHealth.TakeHit on a live enemy: verify listener fired once.
//   * EnemyHealth.TakeHit on a dead enemy (HP ≤ 0): verify listener NOT fired again.
//   * DamageApplier.IsKillingBlow covers the arithmetic guard (already in DamageApplierTests
//     but duplicated here as the canonical ADR-0019 regression anchor).

using Brave.Gameplay.Combat;
using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class DeathListenerTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float MaxHp = 100f;
        private const float KillingDamage = 100f;
        private const float OverkillDamage = 150f;
        private const float AlreadyDeadExtraDamage = 50f;

        /// <summary>Minimal spy that counts how many times OnEnemyDied was called.</summary>
        private sealed class ListenerSpy : MonoBehaviour, Brave.Gameplay.Enemies.IDeathListener
        {
            public int CallCount { get; private set; }
            public void OnEnemyDied(EnemyBase enemy, in HitInfo finalHit) => CallCount++;
        }

        private GameObject _go = null!;
        private EnemyHealth _health = null!;
        private ListenerSpy _spy = null!;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Test_EnemyHealth");
            _health = _go.AddComponent<EnemyHealth>();
            _spy = _go.AddComponent<ListenerSpy>();
            _health.Reset(MaxHp);
            _health.RegisterDeathListener(_spy);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void TakeHit_ReducesHpToZero_FiresListenerExactlyOnce()
        {
            // A single killing blow must invoke each listener exactly once.
            var hit = new HitInfo(KillingDamage, Vector3.zero, isCrit: false,
                sourceId: 0, targetId: 0);
            _health.TakeHit(hit);

            Assert.That(_spy.CallCount, Is.EqualTo(1),
                "listener should fire exactly once on the killing blow");
            Assert.That(_health.IsAlive, Is.False);
            Assert.That(_health.Hp, Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void TakeHit_OnAlreadyDeadEnemy_DoesNotFireListenerAgain()
        {
            // Death event must be idempotent: a second hit on a corpse fires nothing.
            var killHit = new HitInfo(KillingDamage, Vector3.zero, isCrit: false,
                sourceId: 0, targetId: 0);
            _health.TakeHit(killHit);
            Assert.That(_spy.CallCount, Is.EqualTo(1), "pre-condition: first kill fired once");

            var extraHit = new HitInfo(AlreadyDeadExtraDamage, Vector3.zero, isCrit: false,
                sourceId: 0, targetId: 0);
            _health.TakeHit(extraHit);

            Assert.That(_spy.CallCount, Is.EqualTo(1),
                "listener must NOT fire a second time on an already-dead enemy");
        }

        [Test]
        public void TakeHit_ChipDamage_DoesNotFireListener()
        {
            // Non-lethal hit must not invoke the death listener.
            const float ChipDamage = MaxHp * 0.5f;
            var hit = new HitInfo(ChipDamage, Vector3.zero, isCrit: false,
                sourceId: 0, targetId: 0);
            _health.TakeHit(hit);

            Assert.That(_spy.CallCount, Is.EqualTo(0),
                "listener must NOT fire for a non-lethal hit");
            Assert.That(_health.IsAlive, Is.True);
        }

        // ---- DamageApplier arithmetic idempotency anchor (ADR-0019) ----

        [Test]
        public void DamageApplier_IsKillingBlow_AlreadyDead_ReturnsFalse()
        {
            // hp ≤ 0 before the hit → idempotency guard → no second death credit.
            Assert.That(DamageApplier.IsKillingBlow(0f, AlreadyDeadExtraDamage), Is.False,
                "hp=0 before hit is not a killing blow");
            Assert.That(DamageApplier.IsKillingBlow(-10f, AlreadyDeadExtraDamage), Is.False,
                "negative hp before hit is not a killing blow");
        }

        [Test]
        public void DamageApplier_IsKillingBlow_LiveEnemy_ReturnsTrue()
        {
            Assert.That(DamageApplier.IsKillingBlow(MaxHp, KillingDamage), Is.True,
                "full-hp enemy killed by exact damage → killing blow");
            Assert.That(DamageApplier.IsKillingBlow(MaxHp, OverkillDamage), Is.True,
                "overkill damage is still a killing blow");
        }
    }
}
