// QA — Crit-roll system EditMode tests (Wave 10).
// Subject under test: Brave.Gameplay.Damage.DamageCalculator.RollAndApplyCrit,
//                     Brave.Gameplay.Damage.DamageCalculator.RollCrit,
//                     Brave.Gameplay.Damage.DamageCalculator.CritMultiplier
// Specs:  docs/10-balance/00-formulas.md § 2 (crit roll), §11 (CritRateMax = 0.95 clamp).
//         data/balance/characters.json (per-character critRate / critDamage values).
// What we verify:
//   * RollCrit is deterministic for a given (rate, roll) pair — same seed → same outcome.
//   * CritMultiplier == (1 + critDamage) when isCrit; == 1.0 otherwise.
//   * RollAndApplyCrit returns post-crit damage AND the isCrit flag in one stateless call.
//   * isCrit propagates correctly into a HitInfo (the "HitResult" struct per Wave 7A juice).
//   * Crit rate is clamped to MaxCritRate per formulas §11.

using Brave.Gameplay.Damage;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Damage
{
    [TestFixture]
    public class CritRollTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float BaseDamage = 10f;
        private const float NoCrit = 0f;
        private const float HalfCrit = 0.5f;
        private const float AlwaysCrit = 1f;             // clamped to MaxCritRate internally
        private const float DefaultCritDamage = 1f;      // +1.0 → 2x multiplier (Bunny baseline)
        private const float HighCritDamage = 0.5f;       // +0.5 → 1.5x multiplier (task brief default)
        private const float RollAlwaysHits = 0f;         // roll < rate → guaranteed crit
        private const float RollAlwaysMisses = 0.999f;
        private const float Epsilon = 0.0001f;
        private const int SourceId = 7;
        private const int TargetId = 13;
        private static readonly Vector3 HitPoint = new(2f, 0f, 3f);

        // ---- RollCrit: deterministic ----

        [TestCase(0.10f, 0.05f, true,  TestName = "Roll_BelowRate_Crits")]
        [TestCase(0.10f, 0.15f, false, TestName = "Roll_AboveRate_NoCrit")]
        [TestCase(0.50f, 0.50f, false, TestName = "Roll_EqualToRate_NoCrit_HalfOpenInterval")]
        [TestCase(NoCrit, RollAlwaysHits, false, TestName = "Roll_ZeroRate_NeverCrits")]
        public void RollCrit_RespectsRollAndRate(float critRate, float roll, bool expected)
        {
            bool result = DamageCalculator.RollCrit(critRate, roll);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RollCrit_IsDeterministic_SameSeedYieldsSameOutcome()
        {
            // RollCrit is pure → same inputs must always produce the same output. We don't
            // own UnityEngine.Random here; the caller injects random01. This test guards
            // the contract that the function itself is stateless.
            const float critRate = 0.25f;
            const float seededRoll = 0.10f;       // < 0.25 → must crit
            for (int i = 0; i < 1000; i++)
            {
                Assert.That(DamageCalculator.RollCrit(critRate, seededRoll), Is.True,
                    $"iteration {i}: deterministic crit roll diverged");
            }
        }

        [Test]
        public void RollCrit_ClampsToMaxCritRate()
        {
            // Even with critRate = 1.0 (i.e. above the cap), roll just above MaxCritRate must miss.
            float justBelowCap = DamageCalculator.MaxCritRate - Epsilon;
            float justAboveCap = DamageCalculator.MaxCritRate + Epsilon;

            Assert.That(DamageCalculator.RollCrit(AlwaysCrit, justBelowCap), Is.True,
                "roll < MaxCritRate must crit when nominal rate >= MaxCritRate");
            Assert.That(DamageCalculator.RollCrit(AlwaysCrit, justAboveCap), Is.False,
                "roll > MaxCritRate must NOT crit (cap enforced per formulas §11)");
        }

        // ---- CritMultiplier ----

        [TestCase(true,  DefaultCritDamage, 2.0f, TestName = "Mult_Crit_DefaultCritDamage_2x")]
        [TestCase(true,  HighCritDamage,    1.5f, TestName = "Mult_Crit_HighCritDamage_1_5x")]
        [TestCase(false, DefaultCritDamage, 1.0f, TestName = "Mult_NoCrit_AlwaysOne")]
        [TestCase(false, HighCritDamage,    1.0f, TestName = "Mult_NoCrit_IgnoresCritDamage")]
        public void CritMultiplier_AppliesAdditiveCritDamage(bool isCrit, float critDamage, float expected)
        {
            float result = DamageCalculator.CritMultiplier(isCrit, critDamage);
            Assert.That(result, Is.EqualTo(expected).Within(Epsilon));
        }

        // ---- RollAndApplyCrit: combined surface ----

        [Test]
        public void RollAndApplyCrit_OnCrit_ReturnsMultipliedDamageAndTrueFlag()
        {
            var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                baseDamage: BaseDamage,
                critRate: AlwaysCrit,
                critDamage: DefaultCritDamage,
                random01: RollAlwaysHits);

            Assert.That(isCrit, Is.True);
            Assert.That(dmg, Is.EqualTo(BaseDamage * 2f).Within(Epsilon),
                "default critDamage 1.0 → 2x baseline");
        }

        [Test]
        public void RollAndApplyCrit_OnMiss_ReturnsBaseDamageAndFalseFlag()
        {
            var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                baseDamage: BaseDamage,
                critRate: HalfCrit,
                critDamage: DefaultCritDamage,
                random01: RollAlwaysMisses);

            Assert.That(isCrit, Is.False);
            Assert.That(dmg, Is.EqualTo(BaseDamage).Within(Epsilon),
                "miss must leave damage at baseline");
        }

        [Test]
        public void RollAndApplyCrit_HighCritDamage_AppliesOneAndAHalfMultiplier()
        {
            // Task brief default per feel.json/CritMultiplier convention: 1.5x on crit.
            var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                baseDamage: BaseDamage,
                critRate: AlwaysCrit,
                critDamage: HighCritDamage,    // +0.5 → 1.5x
                random01: RollAlwaysHits);

            Assert.That(isCrit, Is.True);
            Assert.That(dmg, Is.EqualTo(BaseDamage * 1.5f).Within(Epsilon));
        }

        [Test]
        public void RollAndApplyCrit_ZeroCritRate_NeverCrits_ForAnyRoll()
        {
            // Even with roll = 0 (the lowest possible random01), a 0% critRate must never crit.
            for (int i = 0; i < 100; i++)
            {
                float roll = i / 100f;
                var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                    BaseDamage, NoCrit, DefaultCritDamage, roll);
                Assert.That(isCrit, Is.False, $"roll={roll} should not crit at 0% critRate");
                Assert.That(dmg, Is.EqualTo(BaseDamage).Within(Epsilon));
            }
        }

        // ---- isCrit propagation into HitInfo (the "HitResult" per Wave 7A juice) ----

        [Test]
        public void IsCrit_PropagatesIntoHitInfo()
        {
            var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                BaseDamage, AlwaysCrit, DefaultCritDamage, RollAlwaysHits);

            // Build the HitInfo the way HitDetector / DamageApplier do at the call site.
            var info = new HitInfo(
                amount: dmg,
                impactPosition: HitPoint,
                isCrit: isCrit,
                sourceId: SourceId,
                targetId: TargetId);

            Assert.That(info.isCrit, Is.True, "HitInfo.isCrit must carry the crit flag downstream");
            Assert.That(info.amount, Is.EqualTo(dmg).Within(Epsilon));
            Assert.That(info.impactPosition, Is.EqualTo(HitPoint));
            Assert.That(info.sourceId, Is.EqualTo(SourceId));
            Assert.That(info.targetId, Is.EqualTo(TargetId));
        }

        [Test]
        public void IsCrit_PropagatesIntoHitContext()
        {
            // The other downstream consumer (DamageNumberSpawner.Spawn(in HitContext)).
            var (dmg, isCrit) = DamageCalculator.RollAndApplyCrit(
                BaseDamage, AlwaysCrit, DefaultCritDamage, RollAlwaysHits);

            var ctx = new HitContext(
                sourceId: SourceId,
                targetId: TargetId,
                amount: dmg,
                isCrit: isCrit,
                isKillingBlow: false,
                hitPoint: HitPoint,
                type: DamageType.Kinetic);

            Assert.That(ctx.isCrit, Is.True);
            Assert.That(ctx.amount, Is.EqualTo(BaseDamage * 2f).Within(Epsilon));
        }
    }
}
