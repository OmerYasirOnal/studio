// QA — PlayerMover EditMode tests
// Subject under test: Brave.Gameplay.Movement.PlayerMover.ComputeVelocity
// Specs: docs/06-tech-spec/04-input-system.md § Virtual joystick contract (normalised output),
//        docs/02-gdd/01-core-loop.md (top-down 8-way analog movement),
//        data/balance/characters.json § base_move_units_per_sec (Bunny baseline 4.5).
// User stories: US-13 (joystick responsiveness — same-frame input-to-velocity).
// Notes:
//  * ComputeVelocity is pure so we test it directly. The MonoBehaviour Update path is
//    covered by PlayMode smoke tests in a later wave (those need a scene + Keyboard.current).
//  * No magic numbers — the move-speed constant mirrors the Bunny baseline from balance JSON;
//    tests assert relationships, not absolute world distances.

using Brave.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Movement
{
    [TestFixture]
    public class PlayerMoverTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float BunnyBaseMoveSpeed = 4.5f;   // characters.json base_move_units_per_sec
        private const float Epsilon = 0.0001f;
        private const float Sqrt2 = 1.41421356f;

        // ---- Cardinal axis tests ----

        [Test]
        public void ComputeVelocity_ZeroInput_ReturnsZero()
        {
            Vector3 v = PlayerMover.ComputeVelocity(Vector2.zero, BunnyBaseMoveSpeed);
            Assert.That(v, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void ComputeVelocity_RightStick_VelocityMagnitudeEqualsSpeed()
        {
            // Input (1, 0) — full deflection right.
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(1f, 0f), BunnyBaseMoveSpeed);
            Assert.That(v.magnitude, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon));
            Assert.That(v.x, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon));
            Assert.That(v.z, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void ComputeVelocity_UpStick_MapsToWorldPlusZ()
        {
            // Top-down camera convention: input Y → world Z.
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(0f, 1f), BunnyBaseMoveSpeed);
            Assert.That(v.x, Is.EqualTo(0f).Within(Epsilon));
            Assert.That(v.z, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon));
        }

        [Test]
        public void ComputeVelocity_NeverWritesY()
        {
            // Output stays on the XZ ground plane regardless of input.
            foreach (var dir in new[]
                     {
                         new Vector2( 1f,  0f), new Vector2(-1f,  0f),
                         new Vector2( 0f,  1f), new Vector2( 0f, -1f),
                         new Vector2( 1f,  1f), new Vector2(-1f, -1f),
                         new Vector2( 0.3f, 0.7f),
                     })
            {
                Vector3 v = PlayerMover.ComputeVelocity(dir, BunnyBaseMoveSpeed);
                Assert.That(v.y, Is.EqualTo(0f).Within(Epsilon), $"Y leaked for input {dir}");
            }
        }

        // ---- Normalisation tests ----

        [Test]
        public void ComputeVelocity_Diagonal_Normalises()
        {
            // Input (1, 1) has magnitude sqrt(2) — must be clamped to magnitude 1
            // before scaling by speed, so the output magnitude equals `speed`,
            // not `speed * sqrt(2)`. (Diagonal WASD must not out-speed cardinals.)
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(1f, 1f), BunnyBaseMoveSpeed);
            Assert.That(v.magnitude, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon));
            float expectedComponent = BunnyBaseMoveSpeed / Sqrt2;
            Assert.That(v.x, Is.EqualTo(expectedComponent).Within(Epsilon));
            Assert.That(v.z, Is.EqualTo(expectedComponent).Within(Epsilon));
        }

        [Test]
        public void ComputeVelocity_SubUnitInput_PreservesProportion()
        {
            // Joystick at 50% deflection — output is 50% of speed in that direction.
            // This is the contract for analog input (no quadratic curve at launch).
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(0.5f, 0f), BunnyBaseMoveSpeed);
            Assert.That(v.magnitude, Is.EqualTo(0.5f * BunnyBaseMoveSpeed).Within(Epsilon));
        }

        [TestCase( 2f,  0f)]
        [TestCase( 0f,  3f)]
        [TestCase( 5f,  5f)]   // 7.07 magnitude, must clamp to 1.0
        [TestCase(-9f,  4f)]
        public void ComputeVelocity_OverUnitInput_ClampsToSpeedMagnitude(float ix, float iy)
        {
            // The joystick produces magnitudes in [0,1] but a buggy or test caller
            // might send larger values — output magnitude must still equal `speed`.
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(ix, iy), BunnyBaseMoveSpeed);
            Assert.That(v.magnitude, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon),
                $"input ({ix},{iy}) produced magnitude {v.magnitude}, expected {BunnyBaseMoveSpeed}");
        }

        // ---- Speed-source tests ----

        [TestCase(0f)]
        [TestCase(-1f)]
        public void ComputeVelocity_ZeroOrNegativeSpeed_ReturnsZero(float speed)
        {
            // Defensive: if a CharacterDefinition wasn't imported (base_move == 0)
            // the mover refuses to move rather than emit NaN or backwards motion.
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(1f, 0f), speed);
            Assert.That(v, Is.EqualTo(Vector3.zero));
        }

        [TestCase(1.0f)]
        [TestCase(4.5f)]    // Bunny
        [TestCase(3.15f)]   // Tortoise (4.5 * 0.70)
        [TestCase(5.175f)]  // Fox      (4.5 * 1.15)
        public void ComputeVelocity_ScalesLinearlyWithSpeed(float speed)
        {
            // Output magnitude tracks the character's resolved move-speed value.
            Vector3 v = PlayerMover.ComputeVelocity(new Vector2(1f, 0f), speed);
            Assert.That(v.magnitude, Is.EqualTo(speed).Within(Epsilon));
        }
    }
}
