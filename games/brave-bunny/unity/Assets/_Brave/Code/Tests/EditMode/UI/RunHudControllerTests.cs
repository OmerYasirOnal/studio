// QA — RunHudController EditMode tests (Wave 5).
// Subject under test:
//   * Brave.UI.Controllers.RunHudController.Render(IRunRuntimeState, HudElements)
//     — the pure HUD render step; no MonoBehaviour, no UIDocument required.
// Specs:
//   * docs/05-wireframes/05-run-hud.html — HP / XP / timer / wave / boss banner.
//   * docs/07-art-bible/06-ui-visual-direction.md §HUD readability.
//   * Wave-5 ui-engineer dispatch — `IRunRuntimeState` contract.
// Pattern:
//   * Same approach as AutoAttackController tests — exercise the pure static
//     method against in-memory inputs; no scene needed.
//   * We build a HudElements bag with fresh VisualElement / Label / Button
//     instances and assert post-Render state.

using Brave.UI.Bindings;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class RunHudControllerTests
    {
        // ---- constants (no magic numbers — CLAUDE.md principle 6) ----
        private const float HalfHp = 50f;
        private const float FullHp = 100f;
        private const float QuarterXp = 25f;
        private const float FullXp = 100f;
        private const int Level1 = 1;
        private const int Wave1 = 1;
        private const int Wave5 = 5;
        private const float ZeroSeconds = 0f;
        private const float OneMinFiveSec = 65f;
        private const float TwoMinFortyFiveSec = 165f;
        private const string ExpectedHalfHpText = "50 / 100";
        private const string ExpectedTimerZero = "00:00";
        private const string ExpectedTimerOneOhFive = "01:05";
        private const string ExpectedTimerTwoForty5 = "02:45";
        private const string ExpectedWaveOne = "Wave 1";
        private const string ExpectedWaveFive = "Wave 5";
        private const string ExpectedLevelOne = "Lv 1";

        // ---- helpers ----

        private static RunHudController.HudElements MakeElements()
        {
            // Fabricate the same elements RunHud.uxml exposes; tests don't need
            // the real UXML — Render() only touches the typed refs.
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

        private static RunHudStubRuntime MakeStubState()
        {
            return new RunHudStubRuntime
            {
                CurrentHP = HalfHp,
                MaxHP = FullHp,
                CurrentXP = QuarterXp,
                XPToNextLevel = FullXp,
                Level = Level1,
                WaveNumber = Wave1,
                RunSecondsElapsed = ZeroSeconds,
                IsBossActive = false,
            };
        }

        private static float WidthPercent(VisualElement el)
        {
            // resolvedStyle is not populated without a panel; read style.width
            // (which Render() writes as percent).
            var len = el.style.width.value;
            return len.value;
        }

        // ---- tests ----

        [Test]
        public void Render_HpHalf_FillsBarTo50Percent()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.CurrentHP = HalfHp;
            state.MaxHP = FullHp;

            RunHudController.Render(state, el);

            Assert.That(WidthPercent(el.HpFill), Is.EqualTo(50f).Within(0.001f),
                "HP 50/100 must drive the HP fill width to 50%.");
            Assert.That(el.HpNumeric.text, Is.EqualTo(ExpectedHalfHpText),
                "HP numeric overlay must read '50 / 100'.");
        }

        [Test]
        public void Render_HpZero_DoesNotDivideByZero_WhenMaxHpZero()
        {
            // Defensive: gameplay-engineer may briefly publish MaxHP=0 during
            // run-init; the HUD must not throw or NaN.
            var el = MakeElements();
            var state = MakeStubState();
            state.CurrentHP = 0f;
            state.MaxHP = 0f;

            Assert.DoesNotThrow(() => RunHudController.Render(state, el));
            Assert.That(WidthPercent(el.HpFill), Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void Render_XpQuarter_FillsXpBarTo25Percent()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.CurrentXP = QuarterXp;
            state.XPToNextLevel = FullXp;

            RunHudController.Render(state, el);

            Assert.That(WidthPercent(el.XpFill), Is.EqualTo(25f).Within(0.001f));
        }

        [Test]
        public void Render_Wave1_LabelTextIsWave1()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.WaveNumber = Wave1;

            RunHudController.Render(state, el);

            Assert.That(el.WaveCounter.text, Is.EqualTo(ExpectedWaveOne));
        }

        [Test]
        public void Render_Wave5_LabelTextIsWave5()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.WaveNumber = Wave5;

            RunHudController.Render(state, el);

            Assert.That(el.WaveCounter.text, Is.EqualTo(ExpectedWaveFive));
        }

        [Test]
        public void Render_Level1_LevelPillIsLv1()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.Level = Level1;

            RunHudController.Render(state, el);

            Assert.That(el.LevelPill.text, Is.EqualTo(ExpectedLevelOne));
        }

        [Test]
        public void Render_TimerZero_LabelTextIs0000()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.RunSecondsElapsed = ZeroSeconds;

            RunHudController.Render(state, el);

            Assert.That(el.Timer.text, Is.EqualTo(ExpectedTimerZero));
        }

        [Test]
        public void Render_Timer65Seconds_LabelTextIs0105()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.RunSecondsElapsed = OneMinFiveSec;

            RunHudController.Render(state, el);

            Assert.That(el.Timer.text, Is.EqualTo(ExpectedTimerOneOhFive),
                "65 seconds must format as 01:05 (mm:ss zero-padded).");
        }

        [Test]
        public void Render_Timer165Seconds_LabelTextIs0245()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.RunSecondsElapsed = TwoMinFortyFiveSec;

            RunHudController.Render(state, el);

            Assert.That(el.Timer.text, Is.EqualTo(ExpectedTimerTwoForty5));
        }

        [Test]
        public void Render_BossActiveTrue_RemovesIsHiddenFromBossWarning()
        {
            var el = MakeElements();
            // Seed the element with the hidden class — the default RunHud.uxml
            // ships boss-warning with `is-hidden` applied.
            el.BossWarning.AddToClassList(RunHudController.HiddenClass);
            var state = MakeStubState();
            state.IsBossActive = true;

            RunHudController.Render(state, el);

            Assert.That(el.BossWarning.ClassListContains(RunHudController.HiddenClass), Is.False,
                "IsBossActive=true must remove the is-hidden class so the banner renders.");
        }

        [Test]
        public void Render_BossActiveFalse_KeepsBossWarningHidden()
        {
            var el = MakeElements();
            var state = MakeStubState();
            state.IsBossActive = false;

            RunHudController.Render(state, el);

            Assert.That(el.BossWarning.ClassListContains(RunHudController.HiddenClass), Is.True,
                "IsBossActive=false must apply the is-hidden class.");
        }

        [Test]
        public void Render_BossActiveTogglesIdempotent()
        {
            // Defensive: calling Render() each frame with no IsBossActive
            // change must not pile duplicate is-hidden classes onto the boss
            // warning. (UI Toolkit's class list IS a List<string> under the
            // hood; defensive guards inside SetHidden prevent dupes.)
            var el = MakeElements();
            var state = MakeStubState();
            state.IsBossActive = false;

            RunHudController.Render(state, el);
            RunHudController.Render(state, el);
            RunHudController.Render(state, el);

            int hiddenCount = 0;
            foreach (var c in el.BossWarning.GetClasses())
            {
                if (c == RunHudController.HiddenClass) hiddenCount++;
            }
            Assert.That(hiddenCount, Is.EqualTo(1),
                "is-hidden class must appear exactly once after 3 consecutive Render() calls.");
        }

        [Test]
        public void Render_StubRuntimeDefaults_ProducePlaceholderText()
        {
            // The handoff contract says: with no real RunService wired,
            // the HUD should render placeholder values from RunHudStubRuntime.
            var el = MakeElements();
            var stub = new RunHudStubRuntime();

            RunHudController.Render(stub, el);

            Assert.That(el.HpNumeric.text, Is.EqualTo("50 / 100"),
                "Stub default: 50/100 HP placeholder.");
            Assert.That(el.WaveCounter.text, Is.EqualTo(ExpectedWaveOne),
                "Stub default: Wave 1 placeholder.");
            Assert.That(el.Timer.text, Is.EqualTo(ExpectedTimerZero),
                "Stub default: 00:00 timer placeholder.");
            Assert.That(el.LevelPill.text, Is.EqualTo(ExpectedLevelOne),
                "Stub default: Lv 1 placeholder.");
        }
    }
}
