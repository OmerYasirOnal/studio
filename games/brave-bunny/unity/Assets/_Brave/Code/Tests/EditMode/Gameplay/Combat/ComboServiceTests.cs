// QA — ComboService EditMode tests (Wave 10 combo / kill-streak).
// Subject under test: Brave.Gameplay.Combat.ComboService
// Specs:
//   * Brief: kills within FeelConfig.comboWindowSeconds (2s default) extend the
//     streak; no kill within the window breaks it. Tier thresholds at 3/5/10.
//   * CLAUDE.md principle 6: no magic numbers in code — durations + thresholds
//     come from FeelConfig.
// What we verify:
//   * First kill sets streak to 1 + raises ComboChangedEvent
//   * Successive kills inside the window scale the streak monotonically
//   * A kill *outside* the window (without an intervening Tick) restarts the streak at 1
//   * Tick() past the window with a non-zero streak fires a break event (streak=0)
//   * Tier resolution: 0/1/2/3 boundaries are correct against the thresholds
//   * Peak streak is preserved across breaks within a single run
//   * Reset() zeros state (no event)

using System.Collections.Generic;

using Brave.Gameplay.Combat;
using Brave.Gameplay.Events;
using Brave.Gameplay.Feel;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class ComboServiceTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float ComboWindow = 2.0f;
        private const int Tier1 = 3;
        private const int Tier2 = 5;
        private const int Tier3 = 10;
        private const float FadeOut = 0.5f;
        private const float HalfWindow = ComboWindow * 0.5f;
        private const float JustInsideWindow = ComboWindow - 0.01f;
        private const float JustOutsideWindow = ComboWindow + 0.01f;
        private const float StartTime = 100f;

        private FeelConfig? _config;
        private ComboChangedChannel? _channel;
        private List<ComboChangedEvent> _captured = null!;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<FeelConfig>();
            _config.comboWindowSeconds = ComboWindow;
            _config.comboTier1Threshold = Tier1;
            _config.comboTier2Threshold = Tier2;
            _config.comboTier3Threshold = Tier3;
            _config.comboFadeOutSeconds = FadeOut;

            _channel = ScriptableObject.CreateInstance<ComboChangedChannel>();
            _captured = new List<ComboChangedEvent>();
            _channel.Subscribe(e => _captured.Add(e));
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null) Object.DestroyImmediate(_config);
            if (_channel != null) Object.DestroyImmediate(_channel);
        }

        // ---- core increment ----

        [Test]
        public void RegisterKill_FirstKill_SetsStreakToOneAndRaises()
        {
            var svc = new ComboService(_config!, _channel);
            int after = svc.RegisterKill(StartTime);
            Assert.That(after, Is.EqualTo(1));
            Assert.That(svc.CurrentStreak, Is.EqualTo(1));
            Assert.That(_captured.Count, Is.EqualTo(1));
            Assert.That(_captured[0].currentStreak, Is.EqualTo(1));
            Assert.That(_captured[0].tier, Is.EqualTo(0), "streak of 1 is below tier-1 threshold");
        }

        [Test]
        public void RegisterKill_WithinWindow_IncrementsStreak()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            svc.RegisterKill(StartTime + HalfWindow);
            svc.RegisterKill(StartTime + HalfWindow + HalfWindow * 0.5f);
            Assert.That(svc.CurrentStreak, Is.EqualTo(3));
            Assert.That(_captured[^1].tier, Is.EqualTo(1),
                "streak == tier-1 threshold should resolve to tier 1");
        }

        [Test]
        public void RegisterKill_JustInsideWindow_StillExtends()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            svc.RegisterKill(StartTime + JustInsideWindow);
            Assert.That(svc.CurrentStreak, Is.EqualTo(2));
        }

        [Test]
        public void RegisterKill_OutsideWindow_RestartsAtOne()
        {
            // No Tick() in between — RegisterKill itself must handle the stale-kill case.
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            svc.RegisterKill(StartTime + JustOutsideWindow);
            Assert.That(svc.CurrentStreak, Is.EqualTo(1),
                "kill arriving past the window must start a fresh streak");
        }

        // ---- break via Tick ----

        [Test]
        public void Tick_PastWindow_BreaksStreakAndRaisesZero()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            _captured.Clear();
            svc.Tick(StartTime + JustOutsideWindow);
            Assert.That(svc.CurrentStreak, Is.EqualTo(0));
            Assert.That(_captured.Count, Is.EqualTo(1));
            Assert.That(_captured[0].currentStreak, Is.EqualTo(0));
            Assert.That(_captured[0].tier, Is.EqualTo(0));
        }

        [Test]
        public void Tick_StillInsideWindow_DoesNotBreak()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            _captured.Clear();
            svc.Tick(StartTime + JustInsideWindow);
            Assert.That(svc.CurrentStreak, Is.EqualTo(1));
            Assert.That(_captured, Is.Empty, "no event while still inside the window");
        }

        [Test]
        public void Tick_WithoutStreak_NoOp()
        {
            var svc = new ComboService(_config!, _channel);
            svc.Tick(StartTime);
            Assert.That(_captured, Is.Empty);
        }

        // ---- multi-kill scaling + tiers ----

        [Test]
        public void MultiKillScaling_TenInsideWindow_ReachesTierThree()
        {
            var svc = new ComboService(_config!, _channel);
            for (int i = 0; i < Tier3; i++)
            {
                svc.RegisterKill(StartTime + i * (ComboWindow * 0.25f));
            }
            Assert.That(svc.CurrentStreak, Is.EqualTo(Tier3));
            Assert.That(svc.CurrentTier, Is.EqualTo(3));
            Assert.That(_captured[^1].tier, Is.EqualTo(3));
        }

        [Test]
        public void TierFor_BoundariesAreInclusive()
        {
            var c = _config!;
            Assert.That(ComboService.TierFor(0, c), Is.EqualTo(0));
            Assert.That(ComboService.TierFor(c.comboTier1Threshold - 1, c), Is.EqualTo(0));
            Assert.That(ComboService.TierFor(c.comboTier1Threshold, c), Is.EqualTo(1));
            Assert.That(ComboService.TierFor(c.comboTier2Threshold - 1, c), Is.EqualTo(1));
            Assert.That(ComboService.TierFor(c.comboTier2Threshold, c), Is.EqualTo(2));
            Assert.That(ComboService.TierFor(c.comboTier3Threshold - 1, c), Is.EqualTo(2));
            Assert.That(ComboService.TierFor(c.comboTier3Threshold, c), Is.EqualTo(3));
            Assert.That(ComboService.TierFor(c.comboTier3Threshold + 100, c), Is.EqualTo(3));
        }

        // ---- peak + reset ----

        [Test]
        public void PeakStreak_PreservedAcrossBreak()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            svc.RegisterKill(StartTime + HalfWindow);
            svc.RegisterKill(StartTime + HalfWindow * 1.25f);
            int peakBefore = svc.PeakStreak;
            Assert.That(peakBefore, Is.EqualTo(3));

            svc.Tick(StartTime + JustOutsideWindow * 2f);
            Assert.That(svc.CurrentStreak, Is.EqualTo(0));
            Assert.That(svc.PeakStreak, Is.EqualTo(peakBefore),
                "peak should not regress when the streak breaks");
        }

        [Test]
        public void Reset_ZeroesEverything_NoEvent()
        {
            var svc = new ComboService(_config!, _channel);
            svc.RegisterKill(StartTime);
            svc.RegisterKill(StartTime + HalfWindow);
            _captured.Clear();
            svc.Reset();
            Assert.That(svc.CurrentStreak, Is.EqualTo(0));
            Assert.That(svc.PeakStreak, Is.EqualTo(0));
            Assert.That(_captured, Is.Empty);
        }

        // ---- channel binding (EnemyKilledChannel pathway) ----

        [Test]
        public void EnemyKilledChannel_IncrementsStreakOnPublish()
        {
            var killChannel = ScriptableObject.CreateInstance<EnemyKilledChannel>();
            try
            {
                var svc = new ComboService(_config!, _channel);
                svc.BindEnemyKilledChannel(killChannel);
                killChannel.Raise(new EnemyKilledEvent(0, Vector3.zero, false, StartTime));
                killChannel.Raise(new EnemyKilledEvent(0, Vector3.zero, false, StartTime + HalfWindow));
                Assert.That(svc.CurrentStreak, Is.EqualTo(2));
                svc.UnbindEnemyKilledChannel(killChannel);
            }
            finally
            {
                Object.DestroyImmediate(killChannel);
            }
        }
    }
}
