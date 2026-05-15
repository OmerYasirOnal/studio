// QA — ComboBadgeController EditMode tests (Wave 10 combo / kill-streak UI).
// Subject under test:
//   * Brave.UI.Controllers.ComboBadgeController.Render(ComboChangedEvent, BadgeElements, FeelConfig, StringBuilder)
//     — the pure render step; no MonoBehaviour, no UIDocument required.
// Specs:
//   * Brief: tier 1 (3+) silver, tier 2 (5+) gold, tier 3 (10+) rainbow.
//   * Below tier 1 the badge stays hidden so the HUD doesn't churn on a single kill.
//   * Streak break (currentStreak == 0) does not unhide the badge; the fade-out
//     schedule is owned by the live MonoBehaviour path (not the pure Render()).
// Pattern: mirrors RunHudControllerTests — build an in-memory BadgeElements bag
// and assert post-Render class state + label text.

using System.Text;

using Brave.Gameplay.Events;
using Brave.Gameplay.Feel;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class ComboBadgeControllerTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const int Tier1Threshold = 3;
        private const int Tier2Threshold = 5;
        private const int Tier3Threshold = 10;
        private const float ComboWindow = 2.0f;
        private const float FadeOut = 0.5f;
        private const float NowSeconds = 12.5f;

        private FeelConfig? _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<FeelConfig>();
            _config.comboWindowSeconds = ComboWindow;
            _config.comboTier1Threshold = Tier1Threshold;
            _config.comboTier2Threshold = Tier2Threshold;
            _config.comboTier3Threshold = Tier3Threshold;
            _config.comboFadeOutSeconds = FadeOut;
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null) Object.DestroyImmediate(_config);
        }

        // ---- helpers ----

        private static ComboBadgeController.BadgeElements MakeElements()
        {
            var root = new VisualElement();
            var badge = new VisualElement();
            var count = new Label();
            // Seed root with .is-hidden so the "stay hidden" assertions are meaningful.
            root.AddToClassList(ComboBadgeController.HiddenClass);
            badge.AddToClassList(ComboBadgeController.TierZeroClass);
            return new ComboBadgeController.BadgeElements { Root = root, Badge = badge, Count = count };
        }

        // ---- tests ----

        [Test]
        public void Render_Tier1Streak_ShowsBadgeWithTierOneClass()
        {
            var el = MakeElements();
            var evt = new ComboChangedEvent(Tier1Threshold, Tier1Threshold, 1, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            Assert.That(el.Root.ClassListContains(ComboBadgeController.HiddenClass), Is.False,
                "tier-1 streak should reveal the badge");
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierOneClass), Is.True);
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierZeroClass), Is.False);
            Assert.That(el.Count.text, Is.EqualTo("x3"));
        }

        [Test]
        public void Render_Tier2Streak_AppliesTierTwoClassAndDropsTierOne()
        {
            var el = MakeElements();
            // Pretend the badge was already at tier 1 before this transition.
            el.Badge.AddToClassList(ComboBadgeController.TierOneClass);
            el.Badge.RemoveFromClassList(ComboBadgeController.TierZeroClass);

            var evt = new ComboChangedEvent(Tier2Threshold, Tier2Threshold, 2, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierTwoClass), Is.True);
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierOneClass), Is.False);
            Assert.That(el.Count.text, Is.EqualTo("x5"));
        }

        [Test]
        public void Render_Tier3Streak_AppliesTierThreeClass()
        {
            var el = MakeElements();
            var evt = new ComboChangedEvent(Tier3Threshold, Tier3Threshold, 3, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierThreeClass), Is.True);
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierTwoClass), Is.False);
            Assert.That(el.Count.text, Is.EqualTo("x10"));
        }

        [Test]
        public void Render_SubThresholdStreak_KeepsBadgeHidden()
        {
            var el = MakeElements();
            var evt = new ComboChangedEvent(Tier1Threshold - 1, Tier1Threshold - 1, 0, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            Assert.That(el.Root.ClassListContains(ComboBadgeController.HiddenClass), Is.True,
                "streak below tier-1 threshold should not surface the badge");
        }

        [Test]
        public void Render_BreakEvent_DoesNotUnhideBadge()
        {
            var el = MakeElements();
            // Unhide first to mimic mid-streak state, then deliver break.
            el.Root.RemoveFromClassList(ComboBadgeController.HiddenClass);
            el.Badge.RemoveFromClassList(ComboBadgeController.TierZeroClass);
            el.Badge.AddToClassList(ComboBadgeController.TierOneClass);

            var evt = new ComboChangedEvent(0, Tier1Threshold, 0, 0f);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            // Break case: tier resolves to 0, tier classes reset, but Render() does
            // not flip .is-hidden (that's the MonoBehaviour's scheduled fade-out's job).
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierZeroClass), Is.True);
            Assert.That(el.Badge.ClassListContains(ComboBadgeController.TierOneClass), Is.False);
        }

        [Test]
        public void Render_TierClassesAreMutuallyExclusive()
        {
            var el = MakeElements();
            var evt = new ComboChangedEvent(Tier3Threshold, Tier3Threshold, 3, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());

            int tierCount = 0;
            if (el.Badge.ClassListContains(ComboBadgeController.TierZeroClass)) tierCount++;
            if (el.Badge.ClassListContains(ComboBadgeController.TierOneClass)) tierCount++;
            if (el.Badge.ClassListContains(ComboBadgeController.TierTwoClass)) tierCount++;
            if (el.Badge.ClassListContains(ComboBadgeController.TierThreeClass)) tierCount++;
            Assert.That(tierCount, Is.EqualTo(1), "exactly one tier class should be active");
        }

        [Test]
        public void Render_CountLabelReflectsExactStreakValue()
        {
            var el = MakeElements();
            const int Streak = 7;
            var evt = new ComboChangedEvent(Streak, Streak, 2, NowSeconds);
            ComboBadgeController.Render(evt, el, _config!, new StringBuilder());
            Assert.That(el.Count.text, Is.EqualTo("x" + Streak));
        }
    }
}
