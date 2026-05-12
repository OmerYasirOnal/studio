// QA — VirtualJoystickInput EditMode tests
// Subject under test: Brave.Gameplay.Movement.VirtualJoystickInput.ScreenDeltaToNormalized
// Specs: docs/06-tech-spec/04-input-system.md § Virtual joystick contract,
//        US-13 (joystick produces normalised [-1,+1] vector).
// Notes:
//  * The MonoBehaviour Update path requires Touchscreen.current and lives behind PlayMode.
//  * Tested values use the same MaxDragRadiusPx default (100f) as the component.
//  * Constants are defined here (no magic numbers per CLAUDE.md principle 6).

using Brave.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Movement
{
    [TestFixture]
    public class VirtualJoystickInputTests
    {
        // ---- constants ----
        private const float MaxRadius = 100f;       // matches VirtualJoystickInput.maxDragRadiusPx default
        private const float Epsilon = 0.0001f;
        private const float InvSqrt2 = 0.70710678f; // 1/√2 — diagonal unit-vector component

        // ---- Centre / zero-input cases ----

        [Test]
        public void ScreenDeltaToNormalized_ZeroDelta_ReturnsZero()
        {
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(Vector2.zero, MaxRadius);
            Assert.That(v, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ScreenDeltaToNormalized_ZeroMaxRadius_ReturnsZero()
        {
            // Defensive — caller passed a bogus radius. Don't divide by zero.
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(50f, 50f), 0f);
            Assert.That(v, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ScreenDeltaToNormalized_NegativeMaxRadius_ReturnsZero()
        {
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(50f, 50f), -1f);
            Assert.That(v, Is.EqualTo(Vector2.zero));
        }

        // ---- Edge (full deflection) cases ----

        [Test]
        public void ScreenDeltaToNormalized_FullRightAtRadius_ReturnsUnitX()
        {
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(MaxRadius, 0f), MaxRadius);
            Assert.That(v.x, Is.EqualTo(1f).Within(Epsilon));
            Assert.That(v.y, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void ScreenDeltaToNormalized_FullLeftAtRadius_ReturnsNegativeUnitX()
        {
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(-MaxRadius, 0f), MaxRadius);
            Assert.That(v.x, Is.EqualTo(-1f).Within(Epsilon));
            Assert.That(v.y, Is.EqualTo(0f).Within(Epsilon));
        }

        // ---- Beyond-edge clamp ----

        [TestCase( 500f,    0f)]
        [TestCase(   0f,  300f)]
        [TestCase(-200f, -200f)]
        [TestCase( 999f,  999f)]
        public void ScreenDeltaToNormalized_BeyondMaxRadius_ClampsToUnitMagnitude(float dx, float dy)
        {
            // Drag past the ring → magnitude clamped to 1.0, direction preserved.
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(dx, dy), MaxRadius);
            Assert.That(v.magnitude, Is.EqualTo(1f).Within(Epsilon),
                $"({dx},{dy}) produced magnitude {v.magnitude}, expected 1.0");
        }

        // ---- Sub-edge proportion ----

        [Test]
        public void ScreenDeltaToNormalized_HalfRadius_ReturnsHalfMagnitude()
        {
            // 50% drag → 50% of unit output (analog proportion, no quadratic curve).
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(MaxRadius * 0.5f, 0f), MaxRadius);
            Assert.That(v.x, Is.EqualTo(0.5f).Within(Epsilon));
            Assert.That(v.y, Is.EqualTo(0f).Within(Epsilon));
        }

        // ---- Diagonal normalisation ----

        [Test]
        public void ScreenDeltaToNormalized_DiagonalBeyondRadius_NormalisesToUnit()
        {
            // (MaxRadius, MaxRadius) → magnitude √2·MaxRadius (beyond ring) → unit diagonal.
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(
                new Vector2(MaxRadius, MaxRadius), MaxRadius);
            Assert.That(v.x, Is.EqualTo(InvSqrt2).Within(Epsilon));
            Assert.That(v.y, Is.EqualTo(InvSqrt2).Within(Epsilon));
            Assert.That(v.magnitude, Is.EqualTo(1f).Within(Epsilon));
        }

        [Test]
        public void ScreenDeltaToNormalized_DiagonalWithinRadius_KeepsLinearProportion()
        {
            // (MaxRadius/2, MaxRadius/2) is within the ring → linear scaling, NOT clamped to unit.
            float half = MaxRadius * 0.5f;
            Vector2 v = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(half, half), MaxRadius);
            Assert.That(v.x, Is.EqualTo(0.5f).Within(Epsilon));
            Assert.That(v.y, Is.EqualTo(0.5f).Within(Epsilon));
            // Magnitude is √2 / 2 ≈ 0.7071 — NOT 1.0. Joystick UX: within-ring drags
            // preserve the analog proportion so micro-aim works.
            Assert.That(v.magnitude, Is.EqualTo(InvSqrt2).Within(Epsilon));
        }
    }
}
