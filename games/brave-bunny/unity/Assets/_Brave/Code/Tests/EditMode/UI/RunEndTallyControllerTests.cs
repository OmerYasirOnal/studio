// QA — RunEndTallyController / RunEndTallyRenderer EditMode tests (Wave 7B).
// Subject under test:
//   * Brave.UI.Controllers.RunEndTallyRenderer.Render(RunEndReport, TallyElements, loc).
// Verifies the channel-payload → label-text contract for Win/Lose outcomes,
// duration formatting, character/weapon loc-key resolution, and the
// carrots-banked numeric.

#nullable enable

using System.Collections.Generic;
using Brave.Gameplay.Run;
using Brave.UI.Controllers;
using Brave.UI.Theming;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class RunEndTallyControllerTests
    {
        // ---- constants (no magic numbers — CLAUDE.md principle 6) ----
        private const int FinalLevel14 = 14;
        private const float Duration462s = 462f; // 07:42
        private const int Kills312 = 312;
        private const int Waves7 = 7;
        private const int Gold240 = 240;
        private const int PassXp180 = 180;
        private const int HeroXp96 = 96;
        private const string CharBunny = "bunny";
        private const string WeaponBoomerang = "carrot-boomerang";

        // ---- helpers ----

        private static TallyElements MakeElements() => new()
        {
            Title = new Label(),
            RunSummary = new Label(),
            RunDetail = new Label(),
            Carrots = new Label(),
            PassXp = new Label(),
            HeroXp = new Label(),
            Missions = new Label(),
            AdPreview = new Label(),
            DeclineAd = new Button(),
            WatchAd = new Button(),
        };

        private static LocalizationProvider MakeLoc()
        {
            var raw = new Dictionary<Brave.Systems.Settings.LanguageCode, string>
            {
                [Brave.Systems.Settings.LanguageCode.En] = "{}",
                [Brave.Systems.Settings.LanguageCode.Tr] = "{}",
            };
            return new LocalizationProvider(raw);
        }

        private static RunEndReport StandardReport(RunOutcome outcome) => new()
        {
            outcome = outcome,
            result = RunEndReport.ResultFromOutcome(outcome),
            deathCause = RunEndReport.DefaultCauseFor(outcome),
            finalLevel = FinalLevel14,
            runDurationSeconds = Duration462s,
            totalKills = Kills312,
            wavesCleared = Waves7,
            goldGained = Gold240,
            passXpEarned = PassXp180,
            xpGained = HeroXp96,
            characterId = CharBunny,
            weaponIdsUsed = new[] { WeaponBoomerang },
        };

        // ---- Outcome → loc-key mapping ----

        [Test]
        public void OutcomeLocKey_Win_MapsToYouWonKey()
        {
            Assert.That(RunEndTallyRenderer.OutcomeLocKey(RunOutcome.Win),
                Is.EqualTo("runend.you_won"));
        }

        [Test]
        public void OutcomeLocKey_Lose_MapsToYouDiedKey()
        {
            Assert.That(RunEndTallyRenderer.OutcomeLocKey(RunOutcome.Lose),
                Is.EqualTo("runend.you_died"));
        }

        [Test]
        public void OutcomeLocKey_Timeout_MapsToYouDiedKey()
        {
            // Timeout collapses to "lose" presentation.
            Assert.That(RunEndTallyRenderer.OutcomeLocKey(RunOutcome.Timeout),
                Is.EqualTo("runend.you_died"));
        }

        // ---- Title rendering ----

        [Test]
        public void Render_WinOutcome_TitleResolvesYouWon()
        {
            var el = MakeElements();
            var loc = MakeLoc();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, loc);

            // With empty loc tables, Loc() returns the key as identity.
            Assert.That(el.Title.text, Is.EqualTo("runend.you_won"),
                "Win outcome must render the you_won loc-key.");
        }

        [Test]
        public void Render_LoseOutcome_TitleResolvesYouDied()
        {
            var el = MakeElements();
            var loc = MakeLoc();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Lose), el, loc);

            Assert.That(el.Title.text, Is.EqualTo("runend.you_died"));
        }

        // ---- Numeric label rendering ----

        [Test]
        public void Render_FinalLevelAndDuration_FormatAsLvAndMmSs()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.RunSummary.text, Is.EqualTo("Lv 14 · 07:42"),
                "Run summary must be 'Lv {level} · mm:ss' with zero-padded minutes.");
        }

        [Test]
        public void Render_CarrotsLabel_PrefixedWithPlus()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.Carrots.text, Is.EqualTo("+ 240"));
            Assert.That(el.PassXp.text, Is.EqualTo("+ 180"));
            Assert.That(el.HeroXp.text, Is.EqualTo("+ 96"));
        }

        [Test]
        public void Render_AdPreview_ShowsDoubledTotal()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.AdPreview.text, Does.Contain("240").And.Contain("480"),
                "Ad preview must show both base gold and the doubled total.");
        }

        // ---- Character / weapon loc-key resolution ----

        [Test]
        public void Render_CharacterId_ResolvesCharacterNameKey()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.RunDetail.text, Does.Contain("characters.bunny.name"),
                "Run detail must reference the characters.<id>.name loc-key.");
        }

        [Test]
        public void Render_WeaponIds_ResolveWeaponNameKeys()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.RunDetail.text, Does.Contain("weapons.carrot-boomerang.name"),
                "Run detail must reference the weapons.<id>.name loc-key for each weapon used.");
        }

        // ---- Channel subscription contract ----

        [Test]
        public void Render_NullReport_DoesNotThrow()
        {
            // Defensive: the channel must never raise with null, but we should
            // not crash if it does (e.g. test harness mistake).
            var el = MakeElements();
            Assert.DoesNotThrow(() => RunEndTallyRenderer.Render(null!, el, MakeLoc()));
        }

        [Test]
        public void Render_KillCountAndWaves_AppearInDetail()
        {
            var el = MakeElements();
            RunEndTallyRenderer.Render(StandardReport(RunOutcome.Win), el, MakeLoc());

            Assert.That(el.RunDetail.text, Does.Contain("312"),
                "Run detail must include the total-kills count.");
            Assert.That(el.RunDetail.text, Does.Contain("7"),
                "Run detail must include the waves-cleared count.");
        }
    }
}
