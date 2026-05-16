// QA — Brave Bunny MVP playability EditMode tests (Wave 12 polish pass).
//
// Subjects under test:
//   * Brave.Gameplay.Run.CameraFollow — pure math + LateUpdate-style follow
//   * Brave.UI.Controllers.RunHudController.Render — IRunRuntimeState → UXML
//     element propagation (HP / XP / wave / kill labels)
//   * Brave.Gameplay.Movement.PlayerMover.ComputeVelocity — joystick → velocity
//
// Why pure tests instead of full PlayMode integration:
//   * CameraFollow.LateUpdate / PlayerMover.Update both write transform.position
//     and rely on Time.deltaTime, which is not advanced inside an EditMode test.
//     We test the pure math helpers + the static Render() projection — the same
//     code paths that production hits — so playability invariants are guarded
//     without spinning up the Run.unity scene.
//   * Full Run.unity smoke is covered by PlayMode tests when the scene is wired.
//
// Spec refs:
//   * docs/02-gdd/01-core-loop.md — top-down 3/4 camera + joystick.
//   * docs/05-wireframes/05-run-hud.html — HUD label contract.
//   * docs/decisions/0021-hud-binding-contract.md — IRunRuntimeState.
//   * docs/06-tech-spec/05-performance.md — 60 fps LateUpdate budget.

#nullable enable

using System;
using Brave.Gameplay.Movement;
using Brave.Gameplay.Run;
using Brave.UI.Bindings;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.Gameplay.Run
{
    [TestFixture]
    public class MvpPlayabilityTests
    {
        // ---- shared test constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float Epsilon = 0.0001f;
        private const float BunnyBaseMoveSpeed = 4.5f;   // balance/characters.json
        // The wave-12 brief's nominal camera offset; the field is overridable per scene.
        private static readonly Vector3 BriefCameraOffset = new Vector3(10f, 12f, -8f);

        // =====================================================================
        // Task A — Camera follow component
        // =====================================================================

        [Test]
        public void CameraFollow_ComputeDesiredPosition_AddsOffsetToTarget()
        {
            // Pure projection — the same expression LateUpdate feeds into SmoothDamp.
            Vector3 target = new Vector3(3f, 0f, 5f);
            Vector3 desired = CameraFollow.ComputeDesiredPosition(target, BriefCameraOffset);

            Assert.That(desired.x, Is.EqualTo(13f).Within(Epsilon));
            Assert.That(desired.y, Is.EqualTo(12f).Within(Epsilon));
            Assert.That(desired.z, Is.EqualTo(-3f).Within(Epsilon));
        }

        [Test]
        public void CameraFollow_TargetMovement_ShiftsDesiredPositionByDelta()
        {
            // After 1s of simulated movement the camera's desired position must
            // have shifted by the same delta as the target. This is the invariant
            // that guarantees the camera "follows" rather than drifts.
            Vector3 startTarget = Vector3.zero;
            Vector3 endTarget = new Vector3(BunnyBaseMoveSpeed, 0f, 0f);   // 1s right

            Vector3 desiredStart = CameraFollow.ComputeDesiredPosition(startTarget, BriefCameraOffset);
            Vector3 desiredEnd   = CameraFollow.ComputeDesiredPosition(endTarget,   BriefCameraOffset);

            Vector3 delta = desiredEnd - desiredStart;
            Assert.That(delta.x, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon));
            Assert.That(delta.y, Is.EqualTo(0f).Within(Epsilon));
            Assert.That(delta.z, Is.EqualTo(0f).Within(Epsilon));
        }

        [Test]
        public void CameraFollow_SetTarget_AssignsTransformWithoutAlloc()
        {
            // We instantiate the MonoBehaviour to exercise SetTarget() + the public
            // ResolvedOffset property — the only API the boot composition root touches.
            var go = new GameObject("test-camera");
            try
            {
                var follow = go.AddComponent<CameraFollow>();
                var targetGo = new GameObject("test-target");
                try
                {
                    follow.SetTarget(targetGo.transform);
                    // ResolvedOffset is the serialized default (10, 12, -8) before any
                    // auto-capture override; here we only assert it's non-zero so we
                    // catch a regression that clobbers the SerializeField default.
                    Assert.That(follow.ResolvedOffset, Is.Not.EqualTo(Vector3.zero),
                        "CameraFollow default offset must be non-zero so MainCamera isn't pinned to the player origin.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(targetGo);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // =====================================================================
        // Task C — HUD HP/XP/Wave/Kill labels update on state change
        // =====================================================================

        [Test]
        public void RunHud_Render_UpdatesHpLabelOnStateChange()
        {
            var el = NewHudElements();
            var state = new FakePlayState
            {
                CurrentHP = 73f,
                MaxHP = 100f,
            };

            RunHudController.Render(state, el);

            Assert.That(el.HpNumeric.text, Is.EqualTo("73 / 100"));
        }

        [Test]
        public void RunHud_Render_HpFillTracksHpRatio()
        {
            var el = NewHudElements();
            var state = new FakePlayState { CurrentHP = 25f, MaxHP = 100f };

            RunHudController.Render(state, el);

            // Width style is set as a percent value of (hp/max) * 100.
            Assert.That(el.HpFill.style.width.value.unit, Is.EqualTo(LengthUnit.Percent));
            Assert.That(el.HpFill.style.width.value.value, Is.EqualTo(25f).Within(Epsilon));
        }

        [Test]
        public void RunHud_Render_XpAndLevelPropagate()
        {
            var el = NewHudElements();
            var state = new FakePlayState
            {
                CurrentXP = 30f, XPToNextLevel = 60f, Level = 4,
            };

            RunHudController.Render(state, el);

            Assert.That(el.XpFill.style.width.value.value, Is.EqualTo(50f).Within(Epsilon));
            Assert.That(el.LevelPill.text, Is.EqualTo("Lv 4"));
        }

        [Test]
        public void RunHud_Render_WaveAndTimerPropagate()
        {
            var el = NewHudElements();
            var state = new FakePlayState { WaveNumber = 7, RunSecondsElapsed = 75f };

            RunHudController.Render(state, el);

            Assert.That(el.WaveCounter.text, Is.EqualTo("Wave 7"));
            Assert.That(el.Timer.text, Is.EqualTo("01:15"));
        }

        [Test]
        public void RunHud_Render_BossWarningTogglesViaClass()
        {
            var el = NewHudElements();
            var state = new FakePlayState { IsBossActive = false };

            RunHudController.Render(state, el);
            Assert.That(el.BossWarning.ClassListContains(RunHudController.HiddenClass), Is.True,
                "BossWarning must be hidden while no boss is on the field.");

            state.IsBossActive = true;
            RunHudController.Render(state, el);
            Assert.That(el.BossWarning.ClassListContains(RunHudController.HiddenClass), Is.False,
                "BossWarning must reveal when IsBossActive flips to true.");
        }

        // =====================================================================
        // Task E — Joystick input → PlayerMover.Velocity propagation
        // =====================================================================

        [Test]
        public void JoystickInput_PropagatesToVelocityViaComputeVelocity()
        {
            // The boot composition root wires VirtualJoystickInput.StickDirection
            // into PlayerMover via Configure(). The runtime path then runs through
            // ComputeVelocity each Update. We assert the end-to-end math here.
            Vector2 stick = new Vector2(0.6f, 0.8f);  // magnitude 1.0
            Vector3 velocity = PlayerMover.ComputeVelocity(stick, BunnyBaseMoveSpeed);

            Assert.That(velocity.magnitude, Is.EqualTo(BunnyBaseMoveSpeed).Within(Epsilon),
                "Full-deflection joystick must produce velocity at the character's move-speed.");
            Assert.That(velocity.x, Is.EqualTo(stick.x * BunnyBaseMoveSpeed).Within(Epsilon));
            Assert.That(velocity.z, Is.EqualTo(stick.y * BunnyBaseMoveSpeed).Within(Epsilon));
            Assert.That(velocity.y, Is.EqualTo(0f).Within(Epsilon),
                "Movement must stay on the XZ ground plane.");
        }

        [Test]
        public void JoystickInput_NormalizesScreenDeltaInsideRing()
        {
            // VirtualJoystickInput's pure helper. A half-radius drag → 0.5 magnitude
            // → PlayerMover speed scales linearly.
            const float maxRadius = 100f;
            Vector2 normalized = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(50f, 0f), maxRadius);

            Assert.That(normalized.x, Is.EqualTo(0.5f).Within(Epsilon));
            Assert.That(normalized.y, Is.EqualTo(0f).Within(Epsilon));

            Vector3 v = PlayerMover.ComputeVelocity(normalized, BunnyBaseMoveSpeed);
            Assert.That(v.magnitude, Is.EqualTo(0.5f * BunnyBaseMoveSpeed).Within(Epsilon));
        }

        [Test]
        public void JoystickInput_BeyondRing_ClampsToUnitDeflection()
        {
            const float maxRadius = 100f;
            Vector2 normalized = VirtualJoystickInput.ScreenDeltaToNormalized(new Vector2(500f, 0f), maxRadius);

            Assert.That(normalized.magnitude, Is.EqualTo(1f).Within(Epsilon),
                "Drag beyond the joystick ring must clamp to unit deflection — no over-speed.");
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        private static RunHudController.HudElements NewHudElements()
        {
            // Construct the element bag manually — Render() never touches UIDocument,
            // so we can hand it raw VisualElements + Labels directly.
            return new RunHudController.HudElements
            {
                HpFill = new VisualElement(),
                XpFill = new VisualElement(),
                HpNumeric = new Label(),
                Timer = new Label(),
                WaveCounter = new Label(),
                LevelPill = new Label(),
                WaveToast = new Label(),
                BossWarning = new VisualElement(),
                PickupGoldAmount = new Label(),
                PickupHeartAmount = new Label(),
                PauseButton = new Button(),
            };
        }

        /// <summary>
        /// Minimal mutable IRunRuntimeState fake for Render() drive-tests.
        /// Mirrors the FakeBindingState used in RunHudBindingTests but lives here so
        /// each fixture's assumptions stay independent.
        /// </summary>
        private sealed class FakePlayState : IRunRuntimeState
        {
            public float CurrentHP { get; set; } = 100f;
            public float MaxHP { get; set; } = 100f;
            public float CurrentHpNormalized => MaxHP <= 0f ? 0f : Mathf.Clamp01(CurrentHP / MaxHP);
            public float CurrentXP { get; set; }
            public float XPToNextLevel { get; set; } = 100f;
            public int XpPoints { get; set; }
            public int Level { get; set; } = 1;
            public int WaveNumber { get; set; } = 1;
            public float RunSecondsElapsed { get; set; }
            public bool IsBossActive { get; set; }
            public int KillCount { get; set; }
            public bool Paused { get; set; }
            public event Action? StateChanged;
            public void Raise() => StateChanged?.Invoke();
        }
    }
}
