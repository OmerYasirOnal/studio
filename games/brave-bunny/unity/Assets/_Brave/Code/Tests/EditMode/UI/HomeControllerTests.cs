// QA — HomeController / HomeMenuLogic EditMode tests (Wave 7B).
// Subject under test:
//   * Brave.UI.Controllers.HomeMenuLogic — pure-C# routing + currency render.
//     Verifies the Play button loads the Loadout scene, push-screen intents
//     match expected screen names, and currency pills read from the injected
//     wallet reader.
//
// Pattern: matches PauseControllerTests — exercise the logic class against
// fake ISceneLoader / ICurrencyReader, no UIDocument required.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Progression;
using Brave.UI.Bindings;
using Brave.UI.Components;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class HomeControllerTests
    {
        // ---- constants ----
        private const long Carrots1240 = 1240;
        private const long Stars28 = 28;
        private const long LongOverflow = (long)int.MaxValue + 100;

        // ---- test doubles ----

        private sealed class FakeCurrencyReader : ICurrencyReader
        {
            public readonly Dictionary<CurrencyType, long> Balances = new();
            public long Get(CurrencyType type) => Balances.TryGetValue(type, out var v) ? v : 0;
        }

        private sealed class FakeSceneLoader : ISceneLoader
        {
            public readonly List<string> LoadedScenes = new();
            public void Load(string sceneName) => LoadedScenes.Add(sceneName);
        }

        // ---- Currency pill rendering ----

        [Test]
        public void RenderCurrency_CarrotsAndStars_ArePushedToPills()
        {
            var goldPillContainer = new VisualElement { name = "pill-gold" };
            var goldAmount = new Label { name = "lbl-gold-amount" };
            goldPillContainer.Add(goldAmount);
            var gold = new CurrencyPill(goldPillContainer, goldAmount);

            var gemPillContainer = new VisualElement { name = "pill-gem" };
            var gemAmount = new Label { name = "lbl-gem-amount" };
            gemPillContainer.Add(gemAmount);
            var gems = new CurrencyPill(gemPillContainer, gemAmount);

            var wallet = new FakeCurrencyReader();
            wallet.Balances[CurrencyType.Carrots] = Carrots1240;
            wallet.Balances[CurrencyType.Stars] = Stars28;

            var ok = HomeMenuLogic.RenderCurrency(gold, gems, wallet);

            Assert.That(ok, Is.True);
            // Format honours the current culture (thousands separator varies); we
            // assert digit content rather than the exact separator.
            Assert.That(goldAmount.text, Does.Contain("240"),
                "Gold pill must display the wallet carrots count.");
            Assert.That(gemAmount.text, Does.Contain("28"),
                "Gem pill must display the wallet stars count.");
        }

        [Test]
        public void RenderCurrency_NullPills_ReturnsFalseInsteadOfThrowing()
        {
            var wallet = new FakeCurrencyReader();
            Assert.That(HomeMenuLogic.RenderCurrency(null, null, wallet), Is.False);
        }

        [Test]
        public void ClampInt_TruncatesLongOverflowToIntMax()
        {
            Assert.That(HomeMenuLogic.ClampInt(LongOverflow), Is.EqualTo(int.MaxValue),
                "ClampInt guards against pill overflow when wallet > 2B.");
        }

        // ---- Scene routing ----

        [Test]
        public void OnPlayClicked_LoadsLoadoutScene()
        {
            var scene = new FakeSceneLoader();
            UIEvents.ResetAllSubscribers();

            var loaded = HomeMenuLogic.OnPlayClicked(scene);

            Assert.That(loaded, Is.EqualTo(HomeMenuLogic.LoadoutSceneName));
            Assert.That(scene.LoadedScenes, Has.Exactly(1).EqualTo("Loadout"),
                "Play button must route to the Loadout scene per Wave 7B routing.");

            UIEvents.ResetAllSubscribers();
        }

        [Test]
        public void OnPlayClicked_RaisesStartRunRequestedIntent()
        {
            // Boot wires StartRunRequested to analytics + the Run service — the
            // UI button must still raise the intent even though we load Loadout
            // first (so the integration agent's pre-run telemetry fires).
            UIEvents.ResetAllSubscribers();
            int raised = 0;
            UIEvents.StartRunRequested += () => raised++;

            var scene = new FakeSceneLoader();
            HomeMenuLogic.OnPlayClicked(scene);

            Assert.That(raised, Is.EqualTo(1),
                "Play button must raise UIEvents.StartRunRequested.");

            UIEvents.ResetAllSubscribers();
        }

        // ---- Screen-name constants ----

        [Test]
        public void ScreenNameConstants_AreStable()
        {
            // Guards against accidental renames; downstream integration agent
            // depends on these names matching Build Settings + UIEvents handlers.
            Assert.That(HomeMenuLogic.LoadoutSceneName, Is.EqualTo("Loadout"));
            Assert.That(HomeMenuLogic.SettingsScreenName, Is.EqualTo("Settings"));
            Assert.That(HomeMenuLogic.CharactersScreenName, Is.EqualTo("CharacterSelect"));
            Assert.That(HomeMenuLogic.ShopScreenName, Is.EqualTo("Shop"));
        }
    }
}
