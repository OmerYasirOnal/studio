// QA — AutoAttackController EditMode tests (Wave 4 vertical slice).
// Subjects under test:
//   * Brave.Gameplay.Combat.AutoAttackController.TickCooldown (pure cadence math)
//   * Brave.Gameplay.Combat.AutoAttackController.ComputeProjectileSpeedFromRange
//   * Brave.Gameplay.Combat.AutoAttackController.ComputeProjectileLifetimeFromRange
//   * Defensive Awake guard (DirectCastEnabled flag)
// Specs: docs/02-gdd/04-weapons.md § Carrot Boomerang (RATE = seconds-between-fires),
//        docs/10-balance/00-formulas.md § 1 (damage formula consumes WeaponLevelData),
//        ADR-0005 (pooled spawnables only).
// Pattern: PlayerMover.ComputeVelocity precedent — cast-cadence math extracted into a pure
//          static helper so we don't need to construct a Unity scene to verify the contract.

using Brave.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class AutoAttackControllerTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float Epsilon = 0.0001f;
        // Carrot Boomerang baseline from data/balance/weapons.json (L1).
        private const float CarrotFireRate = 1.0f;          // RATE — seconds between fires
        private const float CarrotRange = 5.0f;             // RANGE — world units
        private const float FrameDt = 1f / 60f;             // 60 fps
        private const float HalfRate = CarrotFireRate * 0.5f;
        private const int TicksToFirePerSecond = 60;        // 60 × 1/60 s = 1 s

        // ---- Cast cadence (pure TickCooldown) ----

        [Test]
        public void TickCooldown_BelowRate_NoFire()
        {
            // After one frame at 1/60 s the controller has not yet reached the 1 s rate.
            float cd = CarrotFireRate;
            cd = AutoAttackController.TickCooldown(cd, FrameDt, CarrotFireRate, out int fired);
            Assert.That(fired, Is.EqualTo(0));
            Assert.That(cd, Is.EqualTo(CarrotFireRate - FrameDt).Within(Epsilon));
        }

        [Test]
        public void TickCooldown_AtRate_FiresOnceAndResets()
        {
            // 61 frames at 1/60 s ≈ 1.0167 s > 1 s rate → guaranteed exactly one cast.
            // (Using 60 frames is float-precision-flaky: 60 × 1/60 rounds to ~0.99999 vs 1.0.)
            float cd = CarrotFireRate;
            int totalCasts = 0;
            for (int i = 0; i < TicksToFirePerSecond + 1; i++)
            {
                cd = AutoAttackController.TickCooldown(cd, FrameDt, CarrotFireRate, out int fired);
                totalCasts += fired;
            }
            Assert.That(totalCasts, Is.EqualTo(1),
                $"expected exactly 1 cast in {TicksToFirePerSecond + 1} frames at rate {CarrotFireRate}, got {totalCasts}");
            Assert.That(cd, Is.GreaterThan(0f));
            Assert.That(cd, Is.LessThanOrEqualTo(CarrotFireRate + Epsilon));
        }

        [Test]
        public void TickCooldown_FivePeriods_FiresFiveTimes()
        {
            // 5 × (TicksToFirePerSecond + 1) frames ≈ 5.083 s → exactly 5 casts.
            float cd = CarrotFireRate;
            int totalCasts = 0;
            const int Frames = (TicksToFirePerSecond + 1) * 5;
            for (int i = 0; i < Frames; i++)
            {
                cd = AutoAttackController.TickCooldown(cd, FrameDt, CarrotFireRate, out int fired);
                totalCasts += fired;
            }
            Assert.That(totalCasts, Is.EqualTo(5));
        }

        [Test]
        public void TickCooldown_LargeDt_FiresMultipleCastsInOneTick()
        {
            // 2.5 × rate worth of dt in one tick → 2 casts fire and remaining cooldown is
            // 0.5 × rate (because the controller never "skips" a rate window).
            float cd = CarrotFireRate;
            float dt = CarrotFireRate * 2.5f;
            cd = AutoAttackController.TickCooldown(cd, dt, CarrotFireRate, out int fired);
            Assert.That(fired, Is.EqualTo(2));
            Assert.That(cd, Is.EqualTo(HalfRate).Within(Epsilon));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void TickCooldown_ZeroOrNegativeRate_NoFire(float rate)
        {
            // Defensive: the Awake-time gate already refuses non-positive rates, but the
            // helper itself must be safe to call with bad data (e.g. SO not imported).
            float cd = CarrotFireRate;
            cd = AutoAttackController.TickCooldown(cd, FrameDt, rate, out int fired);
            Assert.That(fired, Is.EqualTo(0));
        }

        // ---- Projectile speed / lifetime derivations ----

        [Test]
        public void ComputeProjectileSpeedFromRange_TravelsRangeInHalfRateWindow()
        {
            // At 1 s rate and 5 units range, projectile crosses 5 units in 0.5 s → speed = 10 u/s.
            float speed = AutoAttackController.ComputeProjectileSpeedFromRange(CarrotRange, CarrotFireRate);
            float expected = CarrotRange / (CarrotFireRate * ProjectileMath.RangeTravelFractionOfRate);
            Assert.That(speed, Is.EqualTo(expected).Within(Epsilon));
        }

        [Test]
        public void ComputeProjectileSpeedFromRange_ClampsMinTravelOnSubFrameRate()
        {
            // A pathological 1 ms rate would otherwise yield a huge speed; the MinTravelSeconds
            // floor keeps the projectile readable on screen for at least one frame.
            float speed = AutoAttackController.ComputeProjectileSpeedFromRange(CarrotRange, 0.001f);
            float expected = CarrotRange / ProjectileMath.MinTravelSeconds;
            Assert.That(speed, Is.EqualTo(expected).Within(Epsilon));
        }

        [Test]
        public void ComputeProjectileLifetimeFromRange_MatchesTravelTime()
        {
            // lifetime = range / speed — equals the travel window used to derive speed.
            float speed = AutoAttackController.ComputeProjectileSpeedFromRange(CarrotRange, CarrotFireRate);
            float lifetime = AutoAttackController.ComputeProjectileLifetimeFromRange(CarrotRange, speed);
            float expected = CarrotFireRate * ProjectileMath.RangeTravelFractionOfRate;
            Assert.That(lifetime, Is.EqualTo(expected).Within(Epsilon));
        }

        [Test]
        public void ComputeProjectileLifetimeFromRange_ZeroSpeed_FallsBackToMin()
        {
            float lifetime = AutoAttackController.ComputeProjectileLifetimeFromRange(CarrotRange, 0f);
            Assert.That(lifetime, Is.EqualTo(ProjectileMath.MinTravelSeconds).Within(Epsilon));
        }

        // ---- Defensive Awake guard ----

        [Test]
        public void DirectCastEnabled_DefaultsFalse_WhenNothingWired()
        {
            // Naked controller: no weapon, no pool, no player. Direct cast must stay disabled
            // (and the polymorphic _equipped path remains the active code path).
            var go = new GameObject("Test_AutoAttackController");
            try
            {
                var controller = go.AddComponent<AutoAttackController>();
                // Force Awake to run on a manually-constructed component: Unity invokes Awake
                // synchronously when AddComponent returns on enabled MonoBehaviours.
                Assert.That(controller.DirectCastEnabled, Is.False);
                Assert.That(controller.Equipped, Is.Empty);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void TickCooldown_ZeroFireRate_NeverFires_AcrossManyFrames()
        {
            // Mirrors the Awake guard's contract at the pure-arithmetic layer:
            // a WeaponDefinition with zero fireRate (e.g. balance JSON not yet imported)
            // must not produce any casts even when ticked for a long time. The Awake guard
            // disables the controller in this case, but TickCooldown is itself defensive.
            float cd = 0f;
            int totalCasts = 0;
            const int LongRun = TicksToFirePerSecond * 10;
            for (int i = 0; i < LongRun; i++)
            {
                cd = AutoAttackController.TickCooldown(cd, FrameDt, 0f, out int fired);
                totalCasts += fired;
            }
            Assert.That(totalCasts, Is.EqualTo(0));
        }
    }
}
