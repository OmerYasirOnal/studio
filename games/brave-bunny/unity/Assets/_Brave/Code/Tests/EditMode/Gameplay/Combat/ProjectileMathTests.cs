// QA — Pure projectile math EditMode tests.
// Subject under test: Brave.Gameplay.Combat.ProjectileMath
// Specs: docs/02-gdd/04-weapons.md § Carrot Boomerang (linear projectile baseline),
//        docs/06-tech-spec/05-runtime-architecture.md § Camera convention (XZ ground plane),
//        ADR-0005 (pooling + zero-alloc hot path).
// Why pure: Projectile.Update is allocation-free but exercising it requires a MonoBehaviour
//           + scene; the arithmetic was extracted into ProjectileMath so EditMode tests can
//           verify the math directly (PlayerMover.ComputeVelocity precedent).
// No magic numbers: every constant below is named with a comment justifying its value.

using Brave.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class ProjectileMathTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float Epsilon = 0.0001f;
        private const float Speed = 8f;                 // typical projectile speed (u/s)
        private const float Dt = 1f / 60f;              // 60 fps frame budget
        private const float UnitDir = 1f;
        private const float StartX = 2f;
        private const float StartZ = -3f;
        private const float NominalLifetime = 1.0f;     // weapon lifetime test fixture

        // ---- Step arithmetic ----

        [Test]
        public void Step_PlusXDirection_AdvancesXOnly()
        {
            // Direction (1,0,0) at 8 u/s for 1/60 s → +0.1333.. on X.
            Vector3 result = ProjectileMath.Step(
                new Vector3(StartX, 0f, StartZ),
                new Vector3(UnitDir, 0f, 0f),
                Speed, Dt);
            Assert.That(result.x, Is.EqualTo(StartX + Speed * Dt).Within(Epsilon));
            Assert.That(result.y, Is.EqualTo(0f).Within(Epsilon));
            Assert.That(result.z, Is.EqualTo(StartZ).Within(Epsilon));
        }

        [Test]
        public void Step_PlusZDirection_AdvancesZOnly()
        {
            Vector3 result = ProjectileMath.Step(
                new Vector3(StartX, 0f, StartZ),
                new Vector3(0f, 0f, UnitDir),
                Speed, Dt);
            Assert.That(result.x, Is.EqualTo(StartX).Within(Epsilon));
            Assert.That(result.z, Is.EqualTo(StartZ + Speed * Dt).Within(Epsilon));
        }

        [Test]
        public void Step_ZeroDt_NoMovement()
        {
            Vector3 start = new(StartX, 1f, StartZ);
            Vector3 result = ProjectileMath.Step(start, new Vector3(UnitDir, 0f, 0f), Speed, 0f);
            Assert.That(result, Is.EqualTo(start));
        }

        [Test]
        public void Step_ZeroSpeed_NoMovement()
        {
            Vector3 start = new(StartX, 0f, StartZ);
            Vector3 result = ProjectileMath.Step(start, new Vector3(UnitDir, 0f, 0f), 0f, Dt);
            Assert.That(result, Is.EqualTo(start));
        }

        [Test]
        public void Step_PreservesY()
        {
            // Direction has only XZ components — Y must not drift. Important for the
            // XZ-ground-plane camera (projectiles never lift off the floor).
            const float SpawnY = 0.5f;
            Vector3 result = ProjectileMath.Step(
                new Vector3(StartX, SpawnY, StartZ),
                new Vector3(UnitDir, 0f, UnitDir),
                Speed, Dt);
            Assert.That(result.y, Is.EqualTo(SpawnY).Within(Epsilon));
        }

        [Test]
        public void Step_OneFullSecond_TravelsSpeedUnits()
        {
            // Sanity: a 1-second integration at speed=8 along +X travels exactly 8 units.
            Vector3 result = ProjectileMath.Step(Vector3.zero, new Vector3(UnitDir, 0f, 0f), Speed, 1f);
            Assert.That(result.x, Is.EqualTo(Speed).Within(Epsilon));
        }

        // ---- Lifetime decay ----

        [Test]
        public void DecayLifetime_SubtractsDt()
        {
            float result = ProjectileMath.DecayLifetime(NominalLifetime, Dt);
            Assert.That(result, Is.EqualTo(NominalLifetime - Dt).Within(Epsilon));
        }

        [TestCase(NominalLifetime, false)]    // fresh
        [TestCase(Dt, false)]                  // 1 frame remaining (>0)
        [TestCase(0f, true)]                   // exactly expired
        [TestCase(-Dt, true)]                  // overshoot
        public void ShouldExpire_TriggersAtOrBelowZero(float remaining, bool expectedExpired)
        {
            Assert.That(ProjectileMath.ShouldExpire(remaining), Is.EqualTo(expectedExpired));
        }

        [Test]
        public void DecayLifetime_AccumulatesAcrossManyFrames()
        {
            // Sanity: 60 frames of 1/60 s should consume exactly 1 second of lifetime.
            float remaining = NominalLifetime;
            const int FramesInOneSecond = 60;
            for (int i = 0; i < FramesInOneSecond; i++)
                remaining = ProjectileMath.DecayLifetime(remaining, Dt);
            Assert.That(remaining, Is.EqualTo(0f).Within(Epsilon));
            Assert.That(ProjectileMath.ShouldExpire(remaining), Is.True);
        }
    }
}
