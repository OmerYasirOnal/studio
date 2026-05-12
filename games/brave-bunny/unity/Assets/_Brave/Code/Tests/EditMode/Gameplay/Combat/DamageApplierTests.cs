// QA — DamageApplier EditMode tests (Wave 4 vertical slice).
// Subject under test: Brave.Gameplay.Combat.DamageApplier
// Specs: docs/10-balance/00-formulas.md § 1 (damage formula clamp: floor 1, applied by
//        DamageCalculator BEFORE DamageApplier — applier is just the application surface).
// Pattern: pure-arithmetic helpers tested directly (NewHpAfter, IsKillingBlow); EnemyBase
//          integration deferred to the PlayMode smoke test in a later wave (needs a real
//          EnemyHealth-on-GameObject which is awkward in EditMode without a prefab).

using Brave.Gameplay.Combat;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class DamageApplierTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float Epsilon = 0.0001f;
        private const float Hp = 10f;             // swarmer-ish HP for the test fixtures
        private const float HalfHp = Hp * 0.5f;
        private const float OverkillDamage = Hp * 2f;
        private const float ChipDamage = 1f;

        [Test]
        public void NewHpAfter_PartialDamage_SubtractsAmount()
        {
            float result = DamageApplier.NewHpAfter(Hp, ChipDamage);
            Assert.That(result, Is.EqualTo(Hp - ChipDamage).Within(Epsilon));
        }

        [Test]
        public void NewHpAfter_ExactKill_ReturnsZero()
        {
            float result = DamageApplier.NewHpAfter(Hp, Hp);
            Assert.That(result, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void NewHpAfter_Overkill_ClampsToZero()
        {
            // Damage application must never produce negative HP — the floor lives here, not
            // at every call-site.
            float result = DamageApplier.NewHpAfter(Hp, OverkillDamage);
            Assert.That(result, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void IsKillingBlow_ChipDamage_False()
        {
            Assert.That(DamageApplier.IsKillingBlow(Hp, ChipDamage), Is.False);
        }

        [Test]
        public void IsKillingBlow_ExactDamage_True()
        {
            Assert.That(DamageApplier.IsKillingBlow(Hp, Hp), Is.True);
        }

        [Test]
        public void IsKillingBlow_Overkill_True()
        {
            Assert.That(DamageApplier.IsKillingBlow(Hp, OverkillDamage), Is.True);
        }

        [Test]
        public void IsKillingBlow_AlreadyDead_False()
        {
            // Predicate must short-circuit on already-dead enemies (no double-kill credit).
            Assert.That(DamageApplier.IsKillingBlow(0f, ChipDamage), Is.False);
            Assert.That(DamageApplier.IsKillingBlow(-1f, ChipDamage), Is.False);
        }

        [Test]
        public void NewHpAfter_HalfDamage_LeavesHalfHp()
        {
            float result = DamageApplier.NewHpAfter(Hp, HalfHp);
            Assert.That(result, Is.EqualTo(HalfHp).Within(Epsilon));
            Assert.That(DamageApplier.IsKillingBlow(Hp, HalfHp), Is.False);
        }
    }
}
