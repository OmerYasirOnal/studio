// QA — DailyRewardService EditMode tests (Wave 9).
// Subject under test: Brave.Systems.LiveOps.DailyRewardService
// Spec: docs/02-gdd/02-meta-loop.md (7-day rotating calendar)
// Cross-ref: docs/06-tech-spec/03-save-system.md (save fires on claim)
//           ADR-0008 (forward-compat — DailyRewardState defaults safely).

using System;
using System.Collections.Generic;
using System.IO;
using Brave.Systems.LiveOps;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.LiveOps
{
    [TestFixture]
    public class DailyRewardServiceTests
    {
        // ---- constants (CLAUDE.md principle 6 — no magic numbers) ----
        private const int Day1Carrots = 50;
        private const int Day2Carrots = 100;
        private const int Day3Stars = 1;
        private const int Day4Carrots = 200;
        private const int Day5Stars = 2;
        private const int Day6Carrots = 300;
        private const int Day7SummonShards = 1;
        private const int CycleLength = 7;

        private static readonly DateTime Day0Utc = new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc);

        private string _rootDir = string.Empty;
        private SaveService _save = null!;
        private ProgressionService _progression = null!;
        private DailyRewardConfig _config = null!;
        private DailyRewardService _svc = null!;

        [SetUp]
        public void SetUp()
        {
            _rootDir = Path.Combine(Path.GetTempPath(), "brave-daily-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_rootDir);
            _save = new SaveService(_rootDir);
            _save.Load();
            _progression = new ProgressionService(_save);

            _config = UnityEngine.ScriptableObject.CreateInstance<DailyRewardConfig>();
            _config.SetEntriesForTest(BuildDefaultEntries());

            _svc = new DailyRewardService(_save, _progression, _config);
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null) UnityEngine.Object.DestroyImmediate(_config);
            try { if (Directory.Exists(_rootDir)) Directory.Delete(_rootDir, true); } catch { /* best-effort */ }
        }

        private static List<DailyRewardConfig.Entry> BuildDefaultEntries() => new()
        {
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Carrots,    amount = Day1Carrots,       isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Carrots,    amount = Day2Carrots,       isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Stars,      amount = Day3Stars,         isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Carrots,    amount = Day4Carrots,       isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Stars,      amount = Day5Stars,         isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.Carrots,    amount = Day6Carrots,       isMilestone = false },
            new DailyRewardConfig.Entry { currencyType = CurrencyType.SoulShards, amount = Day7SummonShards,  isMilestone = true },
        };

        // ---- core claim flow ----

        [Test]
        public void CanClaim_OnFirstLaunch_IsTrue()
        {
            Assert.That(_svc.CanClaim(Day0Utc), Is.True,
                "Player who has never claimed must be eligible on day 0.");
        }

        [Test]
        public void Claim_GrantsConfiguredCurrencyForDay1()
        {
            var reward = _svc.Claim(Day0Utc);

            Assert.That(reward, Is.Not.Null);
            Assert.That(reward!.Day, Is.EqualTo(1));
            Assert.That(reward.CurrencyType, Is.EqualTo(CurrencyType.Carrots));
            Assert.That(reward.Amount, Is.EqualTo(Day1Carrots));
            Assert.That(_progression.Wallet.Get(CurrencyType.Carrots), Is.EqualTo(Day1Carrots),
                "Wallet must reflect the granted reward.");
        }

        [Test]
        public void Claim_TwiceSameUtcDay_SecondReturnsNull()
        {
            var first = _svc.Claim(Day0Utc);
            var second = _svc.Claim(Day0Utc.AddHours(6)); // same UTC day, later in the day

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Null, "Second claim within the same UTC day must be refused.");
            Assert.That(_progression.Wallet.Get(CurrencyType.Carrots), Is.EqualTo(Day1Carrots),
                "Wallet must not double-grant.");
        }

        [Test]
        public void Claim_AdvancesCurrentDayInCycle()
        {
            Assert.That(_svc.CurrentDay, Is.EqualTo(1));
            _svc.Claim(Day0Utc);
            Assert.That(_svc.CurrentDay, Is.EqualTo(2), "After claiming day 1 the cycle must advance to day 2.");
        }

        // ---- UTC roll-over ----

        [Test]
        public void Claim_NewUtcDay_IsEligibleAgain()
        {
            _svc.Claim(Day0Utc);
            var nextDay = Day0Utc.AddDays(1);
            Assert.That(_svc.CanClaim(nextDay), Is.True);

            var reward2 = _svc.Claim(nextDay);
            Assert.That(reward2, Is.Not.Null);
            Assert.That(reward2!.Day, Is.EqualTo(2), "Second day on the cycle is day 2.");
            Assert.That(reward2.Amount, Is.EqualTo(Day2Carrots));
        }

        [Test]
        public void Claim_Day8_WrapsBackToDay1()
        {
            // March through all 7 days.
            var current = Day0Utc;
            for (var i = 0; i < CycleLength; i++)
            {
                _svc.Claim(current);
                current = current.AddDays(1);
            }
            Assert.That(_svc.CurrentDay, Is.EqualTo(1),
                "After claiming day 7 the cycle must wrap back to day 1.");

            var reward = _svc.Claim(current);
            Assert.That(reward, Is.Not.Null);
            Assert.That(reward!.Day, Is.EqualTo(1));
            Assert.That(reward.Amount, Is.EqualTo(Day1Carrots),
                "Wrapped-around day-1 claim should hand out the day-1 reward again.");
        }

        // ---- lifetime + missed-day semantics ----

        [Test]
        public void Claim_MissedDay_StillIncrementsLifetime()
        {
            _svc.Claim(Day0Utc);
            var threeDaysLater = Day0Utc.AddDays(3); // skipped 2 calendar days

            Assert.That(_svc.CanClaim(threeDaysLater), Is.True);
            var second = _svc.Claim(threeDaysLater);

            Assert.That(second, Is.Not.Null);
            Assert.That(_svc.LifetimeClaims, Is.EqualTo(2),
                "Missing days does not penalise lifetime (calendar advances, count grows).");
        }

        [Test]
        public void LifetimeClaims_StartsAtZero_AndIncrementsPerClaim()
        {
            Assert.That(_svc.LifetimeClaims, Is.EqualTo(0));
            _svc.Claim(Day0Utc);
            Assert.That(_svc.LifetimeClaims, Is.EqualTo(1));
            _svc.Claim(Day0Utc.AddDays(1));
            Assert.That(_svc.LifetimeClaims, Is.EqualTo(2));
        }

        // ---- save round-trip ----

        [Test]
        public void Claim_PersistsAcrossSaveLoad()
        {
            _svc.Claim(Day0Utc);
            _save.Save();

            var save2 = new SaveService(_rootDir);
            save2.Load();
            var prog2 = new ProgressionService(save2);
            var svc2 = new DailyRewardService(save2, prog2, _config);

            Assert.That(svc2.CurrentDay, Is.EqualTo(2),
                "Cycle position must survive a save round-trip.");
            Assert.That(svc2.LifetimeClaims, Is.EqualTo(1));
            Assert.That(svc2.CanClaim(Day0Utc.AddHours(1)), Is.False,
                "Re-loaded service must still recognise today's claim.");
        }

        // ---- peek + milestone ----

        [Test]
        public void PeekToday_ReturnsDay1RewardForFreshPlayer()
        {
            var peek = _svc.PeekToday();
            Assert.That(peek.Day, Is.EqualTo(1));
            Assert.That(peek.Amount, Is.EqualTo(Day1Carrots));
            Assert.That(peek.IsMilestone, Is.False);
            // Peek must not mutate state.
            Assert.That(_svc.CurrentDay, Is.EqualTo(1));
            Assert.That(_svc.LifetimeClaims, Is.EqualTo(0));
        }

        [Test]
        public void Claim_OnDay7_ReturnsMilestoneReward()
        {
            var current = Day0Utc;
            for (var i = 0; i < CycleLength - 1; i++)
            {
                _svc.Claim(current);
                current = current.AddDays(1);
            }
            Assert.That(_svc.CurrentDay, Is.EqualTo(CycleLength));

            var day7 = _svc.Claim(current);
            Assert.That(day7, Is.Not.Null);
            Assert.That(day7!.IsMilestone, Is.True, "Day 7 must be flagged as milestone.");
            Assert.That(day7.CurrencyType, Is.EqualTo(CurrencyType.SoulShards),
                "Day-7 milestone grants the summon-ticket currency.");
        }
    }
}
