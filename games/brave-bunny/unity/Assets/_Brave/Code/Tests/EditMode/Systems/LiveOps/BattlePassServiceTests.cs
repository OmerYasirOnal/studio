// QA — BattlePassService EditMode tests (Wave 9 LiveOps scaffold).
// Subject: Brave.Systems.LiveOps.BattlePassService against InMemoryFileSystem-
// backed SaveService. Verifies:
//   (a) GrantXp advances CurrentTier per the linear threshold table.
//   (b) GrantXp at XP cap saturates without overflow.
//   (c) Claim free row succeeds on reached tier, persists, refuses re-claim.
//   (d) Premium claim REJECTED while PremiumActive == false.
//   (e) ActivatePremium → previously earned premium tiers become claimable.
//   (f) Tier 0 / out-of-range claims rejected.
//   (g) Season swap (different seasonId on the SO) resets state.
//   (h) Post-endDate GrantXp is a no-op but Claim still works.
//   (i) Save round-trip restores xp / tier / premium / claimed lists.

#nullable enable

using System;
using Brave.Systems.LiveOps;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.LiveOps
{
    [TestFixture]
    public class BattlePassServiceTests
    {
        private const string RootDir = "/virt/brave-battlepass";

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load();
        }

        [TearDown]
        public void TearDown()
        {
            // ScriptableObject.CreateInstance leaks unless explicitly destroyed; clean up
            // any SOs we made so the EditMode test runner doesn't accumulate them.
            foreach (var so in _ownedSos)
            {
                if (so != null) UnityEngine.Object.DestroyImmediate(so);
            }
            _ownedSos.Clear();
        }

        // ---- Helpers ----

        private readonly System.Collections.Generic.List<ScriptableObject> _ownedSos = new();

        private BattlePassSeasonConfig BuildSeason(string seasonId,
            string endDate = "2099-01-01", int step = 1000)
        {
            var so = ScriptableObject.CreateInstance<BattlePassSeasonConfig>();
            so.seasonId = seasonId;
            so.startDate = "2020-01-01";
            so.endDate = endDate;
            so.tierXpThresholds = new int[BattlePassSeasonConfig.TierCount];
            so.freeRewards = new BattlePassReward[BattlePassSeasonConfig.TierCount];
            so.premiumRewards = new BattlePassReward[BattlePassSeasonConfig.TierCount];
            for (int i = 0; i < BattlePassSeasonConfig.TierCount; i++)
            {
                so.tierXpThresholds[i] = (i + 1) * step;
                so.freeRewards[i] = new BattlePassReward(CurrencyType.Carrots, 100 + i * 10, isPremium: false);
                so.premiumRewards[i] = new BattlePassReward(CurrencyType.Stars, 5 + i, isPremium: true);
            }
            _ownedSos.Add(so);
            return so;
        }

        // ---- (a) GrantXp advances CurrentTier ----

        [Test]
        public void GrantXp_AdvancesTier_PerLinearThresholds()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            Assert.That(svc.CurrentTier, Is.EqualTo(0));
            svc.GrantXp(999);
            Assert.That(svc.CurrentTier, Is.EqualTo(0), "999 XP still under tier-1 (1000) threshold");

            svc.GrantXp(1);
            Assert.That(svc.CurrentTier, Is.EqualTo(1));

            // After 999 + 1 = 1000 XP we are at tier 1. Adding 2000 → 3000 XP → tier 3.
            svc.GrantXp(2000);
            Assert.That(svc.CurrentXp, Is.EqualTo(3000));
            Assert.That(svc.CurrentTier, Is.EqualTo(3));
        }

        [Test]
        public void GrantXp_FiresTierAdvancedEvent()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            int observed = -1;
            svc.TierAdvanced += t => observed = t;

            svc.GrantXp(2500);
            Assert.That(observed, Is.EqualTo(2));
        }

        // ---- (b) GrantXp caps at top threshold ----

        [Test]
        public void GrantXp_SaturatesAtCap()
        {
            var cfg = BuildSeason("season-test", step: 1000);
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(int.MaxValue / 2);
            Assert.That(svc.CurrentXp, Is.EqualTo(cfg.tierXpThresholds[BattlePassSeasonConfig.TierCount - 1]));
            Assert.That(svc.CurrentTier, Is.EqualTo(BattlePassSeasonConfig.TierCount));

            svc.GrantXp(1_000_000);
            Assert.That(svc.CurrentXp, Is.EqualTo(cfg.tierXpThresholds[BattlePassSeasonConfig.TierCount - 1]),
                "XP must NOT overflow past the top-tier threshold");
        }

        [Test]
        public void GrantXp_NegativeOrZero_NoOp()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(0);
            svc.GrantXp(-500);
            Assert.That(svc.CurrentXp, Is.EqualTo(0));
            Assert.That(svc.CurrentTier, Is.EqualTo(0));
        }

        // ---- (c) Free claim ----

        [Test]
        public void Claim_FreeRow_OnReachedTier_GrantsReward()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(3500); // tier 3

            var reward = svc.Claim(tier: 2, isPremium: false);
            Assert.That(reward, Is.Not.Null);
            Assert.That(reward!.amount, Is.EqualTo(cfg.freeRewards[1].amount));
            Assert.That(svc.IsTierClaimed(2, isPremium: false), Is.True);
        }

        [Test]
        public void Claim_FreeRow_BeforeReached_Rejected()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(500); // tier 0
            var reward = svc.Claim(tier: 1, isPremium: false);
            Assert.That(reward, Is.Null);
        }

        [Test]
        public void Claim_FreeRow_DoubleClaim_Rejected()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(2500);
            Assert.That(svc.Claim(1, isPremium: false), Is.Not.Null);
            Assert.That(svc.Claim(1, isPremium: false), Is.Null, "second claim must reject");
        }

        // ---- (d) Premium gating ----

        [Test]
        public void Claim_PremiumRow_WhenPremiumInactive_Rejected()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(5000); // tier 5
            Assert.That(svc.IsPremiumActive, Is.False);

            var reward = svc.Claim(tier: 3, isPremium: true);
            Assert.That(reward, Is.Null, "premium row must be gated by PremiumActive");
            Assert.That(svc.IsTierClaimed(3, isPremium: true), Is.False);
        }

        // ---- (e) Retroactive premium claims ----

        [Test]
        public void ActivatePremium_AllowsRetroactiveClaimOfPreviouslyEarnedTiers()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            // Earn 5 tiers BEFORE activating premium.
            svc.GrantXp(5500); // tier 5
            Assert.That(svc.Claim(2, isPremium: true), Is.Null);

            // Activate premium → tiers 1..5 must become claimable retroactively.
            svc.ActivatePremium();
            Assert.That(svc.IsPremiumActive, Is.True);

            var reward = svc.Claim(2, isPremium: true);
            Assert.That(reward, Is.Not.Null);
            Assert.That(reward!.amount, Is.EqualTo(cfg.premiumRewards[1].amount));
        }

        // ---- (f) Out-of-range claims ----

        [Test]
        public void Claim_OutOfRangeTier_Rejected()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(30_000);

            Assert.That(svc.Claim(0, isPremium: false), Is.Null);
            Assert.That(svc.Claim(-1, isPremium: false), Is.Null);
            Assert.That(svc.Claim(BattlePassSeasonConfig.TierCount + 1, isPremium: false), Is.Null);
        }

        // ---- (g) Season swap ----

        [Test]
        public void SeasonSwap_ResetsStateAndPremium()
        {
            var s1 = BuildSeason("season-1");
            var svc = new BattlePassService(_save, s1);
            svc.GrantXp(3500);
            svc.ActivatePremium();
            Assert.That(svc.CurrentTier, Is.EqualTo(3));

            var s2 = BuildSeason("season-2");
            var svc2 = new BattlePassService(_save, s2);

            Assert.That(svc2.CurrentSeasonId, Is.EqualTo("season-2"));
            Assert.That(svc2.CurrentXp, Is.EqualTo(0));
            Assert.That(svc2.CurrentTier, Is.EqualTo(0));
            Assert.That(svc2.IsPremiumActive, Is.False, "premium does NOT carry into the next season");
        }

        // ---- (h) Season-end behavior ----

        [Test]
        public void GrantXp_AfterEndDate_NoOp_ButClaimStillWorks()
        {
            // endDate already passed at construction time.
            var cfg = BuildSeason("season-test", endDate: "2020-01-01");

            // First, advance XP BEFORE flipping the clock — we need a reached tier to claim.
            // Simulate by directly seeding state through a service constructed with an
            // override that reports "now" as pre-endDate.
            var preEnd = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var preSvc = new BattlePassService(_save, cfg, () => preEnd);
            preSvc.GrantXp(3500);
            Assert.That(preSvc.CurrentTier, Is.EqualTo(3));

            // Now construct a service with default "now" (real UtcNow > endDate).
            var postSvc = new BattlePassService(_save, cfg);
            int beforeXp = postSvc.CurrentXp;
            postSvc.GrantXp(500);
            Assert.That(postSvc.CurrentXp, Is.EqualTo(beforeXp), "GrantXp must be no-op after endDate");

            // Claim still works for tiers already reached.
            var reward = postSvc.Claim(2, isPremium: false);
            Assert.That(reward, Is.Not.Null);
        }

        // ---- (i) Save round-trip ----

        [Test]
        public void SaveRoundTrip_PreservesXpTierClaimsAndPremium()
        {
            var cfg = BuildSeason("season-test");
            var svc = new BattlePassService(_save, cfg);

            svc.GrantXp(4500);
            svc.ActivatePremium();
            svc.Claim(2, isPremium: false);
            svc.Claim(3, isPremium: true);

            // Reload from disk-equivalent fixture.
            var reloaded = new SaveService(RootDir, _fs);
            reloaded.Load();
            var svc2 = new BattlePassService(reloaded, cfg);

            Assert.That(svc2.CurrentXp, Is.EqualTo(4500));
            Assert.That(svc2.CurrentTier, Is.EqualTo(4));
            Assert.That(svc2.IsPremiumActive, Is.True);
            Assert.That(svc2.IsTierClaimed(2, isPremium: false), Is.True);
            Assert.That(svc2.IsTierClaimed(3, isPremium: true), Is.True);
            Assert.That(svc2.IsTierClaimed(2, isPremium: true), Is.False);
        }
    }
}
