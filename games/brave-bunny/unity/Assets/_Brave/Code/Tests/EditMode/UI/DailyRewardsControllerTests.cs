// QA — DailyRewardsController EditMode tests (Wave 9).
// Subject under test:
//   * Brave.UI.Controllers.DailyRewardsRenderLogic — pure-C# render + claim
//     facade. Tests build a bare VisualElement tree mirroring DailyRewards.uxml
//     and verify class-list state + button enablement after render/claim.
//
// Pattern: matches HomeControllerTests / PauseControllerTests — exercise the
// logic class against fake services, no UIDocument required.

#nullable enable

using System;
using Brave.Systems.LiveOps;
using Brave.Systems.Progression;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class DailyRewardsControllerTests
    {
        private const int CycleLength = 7;
        private static readonly DateTime FixedUtc = new DateTime(2026, 5, 16, 9, 0, 0, DateTimeKind.Utc);

        // ---- test doubles ----

        private sealed class FakeDailyRewardService : IDailyRewardService
        {
            public int CurrentDay { get; set; } = 1;
            public int LifetimeClaims { get; set; }
            public bool CanClaimResult { get; set; } = true;
            public DailyReward? NextReward { get; set; }
            public int ClaimCalls;

            public bool CanClaim(DateTime utcNow) => CanClaimResult;

            public DailyReward? Claim(DateTime utcNow)
            {
                ClaimCalls++;
                if (!CanClaimResult) return null;
                var reward = NextReward ?? new DailyReward(CurrentDay, CurrencyType.Carrots, 50, false);
                CurrentDay = (CurrentDay % CycleLength) + 1;
                LifetimeClaims++;
                CanClaimResult = false;
                return reward;
            }

            public DailyReward PeekToday() =>
                NextReward ?? new DailyReward(CurrentDay, CurrencyType.Carrots, 50, false);
        }

        // ---- helpers ----

        private static VisualElement BuildRoot()
        {
            // Mirror the structural cells in DailyRewards.uxml (cell-day-1..7,
            // btn-claim, lbl-claimed-state, lbl-streak-day). No styles required —
            // tests assert class lists, not visual output.
            var root = new VisualElement();
            for (var d = 1; d <= CycleLength; d++)
            {
                var cell = new VisualElement { name = DailyRewardsRenderLogic.CellPrefix + d };
                cell.AddToClassList("daily-cell");
                if (d == CycleLength) cell.AddToClassList(DailyRewardsRenderLogic.MilestoneClass);
                root.Add(cell);
            }
            root.Add(new Button { name = DailyRewardsRenderLogic.ClaimButtonName, text = "Claim" });
            root.Add(new Label { name = DailyRewardsRenderLogic.ClaimedStateLabelName, text = "Come back tomorrow" });
            root.Add(new Label { name = DailyRewardsRenderLogic.StreakDayLabelName, text = "Day ?" });
            return root;
        }

        // ---- render ----

        [Test]
        public void RenderCalendar_HighlightsTodayCell()
        {
            var root = BuildRoot();
            DailyRewardsRenderLogic.RenderCalendar(root, currentDay: 3, claimedToday: false);

            var todayCell = root.Q<VisualElement>(DailyRewardsRenderLogic.CellPrefix + 3);
            Assert.That(todayCell, Is.Not.Null);
            Assert.That(todayCell!.ClassListContains(DailyRewardsRenderLogic.TodayClass), Is.True,
                "Day-3 cell must carry .is-today when current day is 3 and unclaimed.");
            Assert.That(todayCell.ClassListContains(DailyRewardsRenderLogic.ClaimedClass), Is.False);
        }

        [Test]
        public void RenderCalendar_GreysOutPastDays()
        {
            var root = BuildRoot();
            DailyRewardsRenderLogic.RenderCalendar(root, currentDay: 4, claimedToday: false);

            for (var d = 1; d < 4; d++)
            {
                var cell = root.Q<VisualElement>(DailyRewardsRenderLogic.CellPrefix + d);
                Assert.That(cell!.ClassListContains(DailyRewardsRenderLogic.ClaimedClass), Is.True,
                    $"Past day {d} must be greyed out (.is-claimed).");
            }
        }

        [Test]
        public void RenderCalendar_ClaimButtonDisabledAfterClaim()
        {
            var root = BuildRoot();
            DailyRewardsRenderLogic.RenderCalendar(root, currentDay: 2, claimedToday: true);

            var btn = root.Q<Button>(DailyRewardsRenderLogic.ClaimButtonName);
            Assert.That(btn!.enabledSelf, Is.False,
                "Claim button must be disabled after the player has claimed today.");
        }

        [Test]
        public void RenderCalendar_ClaimButtonEnabledWhenUnclaimed()
        {
            var root = BuildRoot();
            DailyRewardsRenderLogic.RenderCalendar(root, currentDay: 2, claimedToday: false);

            var btn = root.Q<Button>(DailyRewardsRenderLogic.ClaimButtonName);
            Assert.That(btn!.enabledSelf, Is.True,
                "Claim button must be live when claim is available.");
        }

        [Test]
        public void RenderCalendar_MilestoneClassPreservedAcrossRenders()
        {
            var root = BuildRoot();
            DailyRewardsRenderLogic.RenderCalendar(root, currentDay: 7, claimedToday: false);

            var milestoneCell = root.Q<VisualElement>(DailyRewardsRenderLogic.CellPrefix + 7);
            Assert.That(milestoneCell!.ClassListContains(DailyRewardsRenderLogic.MilestoneClass), Is.True,
                "RenderCalendar must NOT strip .is-milestone authored in UXML.");
            Assert.That(milestoneCell.ClassListContains(DailyRewardsRenderLogic.TodayClass), Is.True);
        }

        // ---- claim flow ----

        [Test]
        public void RunClaim_GrantsRewardAndMarksCellClaimed()
        {
            var root = BuildRoot();
            var svc = new FakeDailyRewardService { CurrentDay = 2, CanClaimResult = true };
            DailyRewardsRenderLogic.RenderCalendar(root, svc.CurrentDay, claimedToday: false);

            var reward = DailyRewardsRenderLogic.RunClaim(root, svc, FixedUtc);

            Assert.That(reward, Is.Not.Null);
            Assert.That(svc.ClaimCalls, Is.EqualTo(1));

            var claimedCell = root.Q<VisualElement>(DailyRewardsRenderLogic.CellPrefix + 2);
            Assert.That(claimedCell!.ClassListContains(DailyRewardsRenderLogic.ClaimedClass), Is.True,
                "After claiming day 2, that cell must show the claimed state.");
        }

        [Test]
        public void RunClaim_WhenServiceRefuses_LeavesStateClaimed()
        {
            var root = BuildRoot();
            var svc = new FakeDailyRewardService { CurrentDay = 2, CanClaimResult = false };

            var reward = DailyRewardsRenderLogic.RunClaim(root, svc, FixedUtc);
            Assert.That(reward, Is.Null, "Service refused — RunClaim returns null.");
            Assert.That(svc.ClaimCalls, Is.EqualTo(1));

            var btn = root.Q<Button>(DailyRewardsRenderLogic.ClaimButtonName);
            Assert.That(btn!.enabledSelf, Is.False,
                "Button must remain disabled when the service refuses (already claimed today).");
        }
    }
}
