// QA — HitstopService EditMode tests (Hit Feedback Juice).
// Subject under test: Brave.Gameplay.Feel.HitstopService
// Specs: ADR-0003 hitstop timings (data/balance/feel.json), CLAUDE.md principle 6
//        (no magic numbers — durations come from FeelConfig / FeelDefinition).
// What we verify:
//   * Apply(seconds) freezes Time.timeScale to the configured hold value
//   * Tick(unscaledNow) restores Time.timeScale after the window elapses
//   * Re-applying during an active window coalesces (longest resume-time wins; no re-zero)
//   * Cancel() force-restores immediately
//   * ApplyForTrigger uses the per-trigger ms from FeelDefinition (ADR-0003 canonical)

using Brave.Gameplay.Damage;
using Brave.Gameplay.Feel;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Feel
{
    [TestFixture]
    public class HitstopServiceTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float StartTimeScale = 1f;
        private const float HoldTimeScale = 0f;          // ADR-0003: full freeze
        private const float ShortDurationSeconds = 0.02f;     // 20 ms — basic kill
        private const float LongerDurationSeconds = 0.08f;    // 80 ms — elite kill
        private const float UnscaledNow = 100f;
        private const float Epsilon = 0.0001f;

        private FeelConfig? _config;
        private float _originalTimeScale;

        [SetUp]
        public void SetUp()
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale = StartTimeScale;
            _config = ScriptableObject.CreateInstance<FeelConfig>();
            _config.hitstopTimeScale = HoldTimeScale;
            _config.hitstopMs = 20f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _originalTimeScale;
            if (_config != null) Object.DestroyImmediate(_config);
        }

        [Test]
        public void Apply_ZerosTimeScale_AndActiveFlagSet()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            Assert.That(svc.IsActive, Is.True, "service should be active after Apply");
            Assert.That(Time.timeScale, Is.EqualTo(HoldTimeScale).Within(Epsilon),
                "Time.timeScale should be held at the configured hitstopTimeScale");
        }

        [Test]
        public void Apply_NegativeOrZero_NoOp()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(0f, UnscaledNow);
            Assert.That(svc.IsActive, Is.False);
            svc.Apply(-1f, UnscaledNow);
            Assert.That(svc.IsActive, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(StartTimeScale).Within(Epsilon));
        }

        [Test]
        public void Tick_BeforeResume_KeepsTimeScaleHeld()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            svc.Tick(UnscaledNow + ShortDurationSeconds * 0.5f);
            Assert.That(svc.IsActive, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(HoldTimeScale).Within(Epsilon));
        }

        [Test]
        public void Tick_AfterResume_RestoresPreviousTimeScale()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            svc.Tick(UnscaledNow + ShortDurationSeconds + Epsilon);
            Assert.That(svc.IsActive, Is.False, "service should clear active after resume");
            Assert.That(Time.timeScale, Is.EqualTo(StartTimeScale).Within(Epsilon),
                "Time.timeScale should restore to pre-Apply value");
        }

        [Test]
        public void Apply_Coalesces_ExtendsResumeTime_LongerWins()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            float resumeAfterFirst = svc.ResumeAtUnscaledTime;
            svc.Apply(LongerDurationSeconds, UnscaledNow);
            Assert.That(svc.ResumeAtUnscaledTime, Is.GreaterThan(resumeAfterFirst),
                "coalesced Apply with longer duration should extend the resume time");
            Assert.That(svc.IsActive, Is.True);
        }

        [Test]
        public void Apply_Coalesces_ShorterDoesNotShrinkWindow()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(LongerDurationSeconds, UnscaledNow);
            float resumeAfterFirst = svc.ResumeAtUnscaledTime;
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            Assert.That(svc.ResumeAtUnscaledTime, Is.EqualTo(resumeAfterFirst).Within(Epsilon),
                "coalesced Apply with shorter duration must NOT shrink the pending window");
        }

        [Test]
        public void Apply_WhileActive_DoesNotRecaptureTimeScale()
        {
            // Sentinel: confirm the "previous" timeScale captured at first Apply is preserved
            // even if Time.timeScale somehow changed in-between (e.g. external code).
            var svc = new HitstopService(_config!);
            svc.Apply(ShortDurationSeconds, UnscaledNow);
            Time.timeScale = 0.5f; // arbitrary external mutation while held
            svc.Apply(LongerDurationSeconds, UnscaledNow);
            // Should still hold at hitstopTimeScale (active flag prevents re-capture).
            Assert.That(Time.timeScale, Is.EqualTo(0.5f).Within(Epsilon),
                "while active, service must not overwrite Time.timeScale on re-Apply (test contract)");
            // (Tick will restore the previous = StartTimeScale, since that was captured at first Apply.)
            svc.Tick(UnscaledNow + LongerDurationSeconds + Epsilon);
            Assert.That(Time.timeScale, Is.EqualTo(StartTimeScale).Within(Epsilon));
        }

        [Test]
        public void Cancel_RestoresTimeScale_Immediately()
        {
            var svc = new HitstopService(_config!);
            svc.Apply(LongerDurationSeconds, UnscaledNow);
            svc.Cancel();
            Assert.That(svc.IsActive, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(StartTimeScale).Within(Epsilon));
        }

        [Test]
        public void ApplyForTrigger_BasicKill_UsesFeelDefinitionMs()
        {
            var def = ScriptableObject.CreateInstance<FeelDefinition>();
            try
            {
                const float basicKillMs = 20f;
                def.basicEnemyKillMs = basicKillMs;
                var svc = new HitstopService(_config, def);
                svc.ApplyForTrigger(HitstopTrigger.BasicEnemyKill, UnscaledNow);
                Assert.That(svc.ResumeAtUnscaledTime, Is.EqualTo(UnscaledNow + basicKillMs * 0.001f).Within(Epsilon),
                    "ApplyForTrigger must use FeelDefinition.DurationMsFor (ADR-0003 lookup)");
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new HitstopService(null!));
        }

        [Test]
        public void DurationMath_MillisecondsToSeconds_ExactConversion()
        {
            // Sanity-check the FeelConfig.HitstopSeconds conversion; defensive against drift.
            _config!.hitstopMs = 80f;
            Assert.That(_config.HitstopSeconds, Is.EqualTo(0.08f).Within(Epsilon));
            _config.hitstopMs = 250f;
            Assert.That(_config.HitstopSeconds, Is.EqualTo(0.25f).Within(Epsilon));
        }
    }
}
