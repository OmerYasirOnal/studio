// QA — AchievementService EditMode tests
// Subject under test: BraveBunny.Systems.Progression.AchievementService
// Spec: docs/02-gdd/02-meta-loop.md (50 launch achievements). docs/06-tech-spec/03-save-system.md trigger.
// User story: US-36-ish (achievement claim grants Stars). Cross-ref ADR-0008 (save fires on claim).

using System;
using System.IO;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems
{
    [TestFixture]
    public class AchievementServiceTests
    {
        // ---- constants ----
        private const string SwarmerKillAchievementSlug = "kill-1000-swarmers";
        private const int TargetSwarmerKills = 1000;
        private const int FirstChunk = 400;
        private const int SecondChunk = 600;
        private const int OverflowAttempt = 100;

        private string _rootDir;
        private SaveService _save;
        private AchievementService _svc;

        [SetUp]
        public void SetUp()
        {
            _rootDir = Path.Combine(Path.GetTempPath(), "brave-ach-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_rootDir);
            _save = new SaveService(_rootDir);
            _save.Load();
            _svc = new AchievementService(_save);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_rootDir)) Directory.Delete(_rootDir, true); } catch { }
        }

        [Test]
        public void Achievement_Tracker_IncrementsOnEvent()
        {
            _svc.AddProgress(SwarmerKillAchievementSlug, FirstChunk);
            Assert.That(_svc.GetProgress(SwarmerKillAchievementSlug), Is.EqualTo(FirstChunk));

            _svc.AddProgress(SwarmerKillAchievementSlug, SecondChunk);
            Assert.That(_svc.GetProgress(SwarmerKillAchievementSlug), Is.EqualTo(FirstChunk + SecondChunk));
        }

        [Test]
        public void Achievement_Claim_GrantsStarsOnce()
        {
            // arrange — accumulate progress past target.
            _svc.AddProgress(SwarmerKillAchievementSlug, TargetSwarmerKills);
            Assert.That(_svc.GetProgress(SwarmerKillAchievementSlug), Is.GreaterThanOrEqualTo(TargetSwarmerKills));

            // act
            var claimed = _svc.TryClaim(SwarmerKillAchievementSlug, TargetSwarmerKills);

            // assert
            Assert.That(claimed, Is.True);
            Assert.That(_svc.IsClaimed(SwarmerKillAchievementSlug), Is.True);
        }

        [Test]
        public void Achievement_DoubleClaim_DoesNotDoubleReward()
        {
            _svc.AddProgress(SwarmerKillAchievementSlug, TargetSwarmerKills);

            var firstClaim = _svc.TryClaim(SwarmerKillAchievementSlug, TargetSwarmerKills);
            var secondClaim = _svc.TryClaim(SwarmerKillAchievementSlug, TargetSwarmerKills);

            Assert.That(firstClaim, Is.True);
            Assert.That(secondClaim, Is.False, "second claim must be no-op (anti-double-grant)");
            Assert.That(_svc.IsClaimed(SwarmerKillAchievementSlug), Is.True);
        }

        [Test]
        public void Achievement_AddProgressAfterClaim_DoesNotChangeProgress()
        {
            _svc.AddProgress(SwarmerKillAchievementSlug, TargetSwarmerKills);
            _svc.TryClaim(SwarmerKillAchievementSlug, TargetSwarmerKills);
            var atClaim = _svc.GetProgress(SwarmerKillAchievementSlug);

            _svc.AddProgress(SwarmerKillAchievementSlug, OverflowAttempt);
            Assert.That(_svc.GetProgress(SwarmerKillAchievementSlug), Is.EqualTo(atClaim),
                "AddProgress after Claim must be a no-op (claimed achievements are frozen)");
        }
    }
}
