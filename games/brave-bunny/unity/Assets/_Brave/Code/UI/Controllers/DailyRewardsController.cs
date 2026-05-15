// Brave Bunny — UI / Controllers / DailyRewardsController
// Bound to: _Brave/UI/Documents/DailyRewards.uxml
// Wireframe spec: docs/05-wireframes/03-home-lobby.html (daily-strap → modal)
// User story: US-37 (0-input daily streak claim).
//
// Pattern matches PauseController / HomeController:
//   * DailyRewardsRenderLogic — pure-C# class that mutates VisualElement state.
//     Tests instantiate against bare VisualElement trees with no UIDocument.
//   * DailyRewardsController (MonoBehaviour) — wires the logic to the live
//     UIDocument, service locator, and claim button.

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.LiveOps;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>
    /// Pure-C# render + claim flow for the daily-rewards calendar.
    /// Operates on a VisualElement root so EditMode tests can drive it.
    /// </summary>
    public static class DailyRewardsRenderLogic
    {
        public const string CellPrefix = "cell-day-";
        public const string ClaimButtonName = "btn-claim";
        public const string ClaimedStateLabelName = "lbl-claimed-state";
        public const string StreakDayLabelName = "lbl-streak-day";

        public const string TodayClass = "is-today";
        public const string ClaimedClass = "is-claimed";
        public const string MilestoneClass = "is-milestone";

        /// <summary>
        /// Apply highlight + claimed classes across all 7 cells.
        /// </summary>
        /// <param name="root">UXML root carrying <c>cell-day-1..7</c>.</param>
        /// <param name="currentDay">1..7 — today's cycle day.</param>
        /// <param name="claimedToday">True if the player has already claimed within the current UTC day.</param>
        public static void RenderCalendar(VisualElement root, int currentDay, bool claimedToday)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            for (var d = 1; d <= DailyRewardConfig.CycleLength; d++)
            {
                var cell = root.Q<VisualElement>(CellPrefix + d);
                if (cell == null) continue;

                // Reset the volatile classes (preserves .is-milestone authored in UXML).
                cell.RemoveFromClassList(TodayClass);
                cell.RemoveFromClassList(ClaimedClass);

                if (d < currentDay)
                {
                    // Past days within the cycle — show as claimed.
                    cell.AddToClassList(ClaimedClass);
                }
                else if (d == currentDay)
                {
                    if (claimedToday) cell.AddToClassList(ClaimedClass);
                    else cell.AddToClassList(TodayClass);
                }
            }

            var streakLabel = root.Q<Label>(StreakDayLabelName);
            if (streakLabel != null) streakLabel.text = $"Day {currentDay}";

            var claimBtn = root.Q<Button>(ClaimButtonName);
            if (claimBtn != null) claimBtn.SetEnabled(!claimedToday);

            var claimedHint = root.Q<Label>(ClaimedStateLabelName);
            if (claimedHint != null)
                claimedHint.style.display = claimedToday
                    ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex)
                    : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        /// <summary>
        /// Run the claim flow against the service + re-render. Returns the
        /// reward granted (or null if the service refused, e.g. same-day reclaim).
        /// </summary>
        public static DailyReward? RunClaim(VisualElement root, IDailyRewardService service, DateTime utcNow)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (service == null) throw new ArgumentNullException(nameof(service));

            var reward = service.Claim(utcNow);
            // After claim, the service has advanced to *tomorrow's* slot; render
            // the just-claimed day as claimed by passing claimedToday=true relative
            // to the previous day in the cycle.
            var displayedDay = reward != null ? reward.Day : service.CurrentDay;
            // The displayed day is the cell the player just lit up; service.CurrentDay
            // has moved on, so use displayedDay for highlight.
            RenderCalendar(root, displayedDay, claimedToday: reward != null);
            return reward;
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class DailyRewardsController : MonoBehaviour
    {
        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            RefreshFromService(root);

            var claimBtn = root.Q<Button>(DailyRewardsRenderLogic.ClaimButtonName);
            if (claimBtn != null) claimBtn.clicked += OnClaimClicked;

            _loc.ApplyToTree(root);
        }

        private void RefreshFromService(VisualElement root)
        {
            if (!TryGetService(out var service)) return;
            var claimedToday = !service.CanClaim(DateTime.UtcNow);
            DailyRewardsRenderLogic.RenderCalendar(root, service.CurrentDay, claimedToday);
        }

        private void OnClaimClicked()
        {
            if (!TryGetService(out var service)) return;
            DailyRewardsRenderLogic.RunClaim(_doc.rootVisualElement, service, DateTime.UtcNow);
        }

        private static bool TryGetService(out IDailyRewardService service)
        {
            service = null!;
            if (GameContextBootstrap.Context == null) return false;
            return GameContextBootstrap.Context.TryGet<IDailyRewardService>(out service);
        }
    }
}
