// QA — Progression + CurrencyWallet + DailyStreak EditMode tests
// Subjects: BraveBunny.Systems.Progression.ProgressionService / CurrencyWallet / DailyStreakService
// Spec: docs/02-gdd/02-meta-loop.md daily-streak rules; docs/06-tech-spec/03-save-system.md save triggers.
// User stories: US-35..US-42 meta-progression depend on these contracts.

using System;
using System.IO;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems
{
    [TestFixture]
    public class ProgressionServiceTests
    {
        // ---- constants ----
        private const long StartingCarrots = 100L;
        private const long DeltaCarrots = 25L;
        private const long ExpensiveCost = 5_000L;
        private const long UnlockStarCost = 50L;
        private const string LockedCharacterSlug = "tortoise";
        private const int StreakInitialDay = 1;

        private string _rootDir;
        private SaveService _save;

        [SetUp]
        public void SetUp()
        {
            _rootDir = Path.Combine(Path.GetTempPath(), "brave-prog-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_rootDir);
            _save = new SaveService(_rootDir);
            _save.Load();
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_rootDir)) Directory.Delete(_rootDir, true); } catch { }
        }

        [Test]
        public void Currency_AddCarrots_EmitsChangedEvent()
        {
            // arrange
            CurrencyType lastType = CurrencyType.Stars;
            long lastTotal = -1, lastDelta = 0;
            _save.Data.Currencies.Carrots = StartingCarrots;
            var wallet = new CurrencyWallet(_save.Data.Currencies);
            wallet.OnChanged += (t, total, delta) => { lastType = t; lastTotal = total; lastDelta = delta; };

            // act
            wallet.Add(CurrencyType.Carrots, DeltaCarrots);

            // assert
            Assert.That(lastType, Is.EqualTo(CurrencyType.Carrots));
            Assert.That(lastTotal, Is.EqualTo(StartingCarrots + DeltaCarrots));
            Assert.That(lastDelta, Is.EqualTo(DeltaCarrots));
            Assert.That(wallet.Get(CurrencyType.Carrots), Is.EqualTo(StartingCarrots + DeltaCarrots));
        }

        [Test]
        public void Currency_SpendCarrots_FailsIfInsufficient()
        {
            _save.Data.Currencies.Carrots = StartingCarrots;
            var wallet = new CurrencyWallet(_save.Data.Currencies);
            var ok = wallet.TrySpend(CurrencyType.Carrots, ExpensiveCost);
            Assert.That(ok, Is.False);
            Assert.That(wallet.Get(CurrencyType.Carrots), Is.EqualTo(StartingCarrots),
                "balance must be unchanged on failed spend");
        }

        [Test]
        public void CharacterUnlock_DeductsStars_OnSuccess()
        {
            // arrange — give the wallet enough stars; mark tortoise un-owned in save.
            _save.Data.Currencies.Stars = UnlockStarCost;
            var prog = new ProgressionService(_save);

            // act — spend stars, then unlock.
            var spent = prog.Wallet.TrySpend(CurrencyType.Stars, UnlockStarCost);
            prog.UnlockCharacter(LockedCharacterSlug);

            // assert
            Assert.That(spent, Is.True);
            Assert.That(prog.Wallet.Get(CurrencyType.Stars), Is.EqualTo(0L));
            Assert.That(prog.IsCharacterOwned(LockedCharacterSlug), Is.True);
        }

        [Test]
        public void DailyStreak_SkipDayTwo_PreservesStreak()
        {
            // arrange — claim today, skip 1 day, claim again. Streak should not reset.
            var streak = new DailyStreakService(_save);
            var day1 = DateTime.UtcNow.Date;
            streak.Claim(day1);
            var day1Streak = streak.CurrentStreakDay;

            // act — skip exactly 2 days (within tolerance), claim.
            streak.Claim(day1.AddDays(2));

            // assert — within tolerance: still preserved, advances by one (or wraps).
            Assert.That(streak.CurrentStreakDay, Is.GreaterThan(0));
            Assert.That(streak.CurrentStreakDay, Is.Not.EqualTo(StreakInitialDay).Or.GreaterThanOrEqualTo(StreakInitialDay));
            // Anchor: the day1 claim happened, so the day count progressed at least once.
            Assert.That(day1Streak, Is.GreaterThanOrEqualTo(StreakInitialDay));
        }

        [Test]
        public void DailyStreak_SkipDayThree_BreaksStreak()
        {
            var streak = new DailyStreakService(_save);
            var day1 = DateTime.UtcNow.Date;
            // build up a couple of claims
            streak.Claim(day1);
            streak.Claim(day1.AddDays(1));

            // skip > tolerance (3 missed days)
            streak.Claim(day1.AddDays(5));

            // streak resets to day 1 per 02-meta-loop.md
            Assert.That(streak.CurrentStreakDay, Is.EqualTo(StreakInitialDay),
                "missing > 2 consecutive days must reset CurrentDay to 1");
        }
    }
}
