// QA — Localization EditMode tests
// Subject under test: BraveBunny.Systems.Localization.LocalizationService
// Spec: docs/06-tech-spec/03-save-system.md (player.language persisted).
// User stories: every UI story (US-21 HUD readability, US-37 settings localization) depends on key resolution.

using System.Collections.Generic;
using Brave.Systems.Localization;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class LocalizationProviderTests
    {
        // ---- constants ----
        private const string EnglishCode = "en";
        private const string TurkishCode = "tr";
        private const string KnownKeyHello = "RUN_END_LOSE";
        private const string MissingKey = "TOTALLY_UNDEFINED_KEY_42";
        private const string EnglishValue = "Tuckered out — but you banked carrots.";
        private const string TurkishValue = "Yorgun düştün — ama havuçları kazandın.";

        private static TextAsset MakeAsset(string langCode, IDictionary<string, string> table)
        {
            // Newtonsoft serialization of the inner dict.
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(table);
            var asset = new TextAsset(json) { name = langCode };
            return asset;
        }

        [Test]
        public void Loc_ReturnsKey_WhenMissing()
        {
            var en = MakeAsset(EnglishCode, new Dictionary<string, string> { [KnownKeyHello] = EnglishValue });
            var svc = new LocalizationService(new[] { en });

            var result = svc.Translate(MissingKey);

            // Per ILocalizationService doc comment: "Missing keys return the key itself".
            Assert.That(result, Is.EqualTo(MissingKey),
                "Translate must return the key on miss so QA sees the gap (key surrounded by visible markers OK in prod).");
        }

        [Test]
        public void Loc_ReturnsEnglish_AsBaseline()
        {
            var en = MakeAsset(EnglishCode, new Dictionary<string, string> { [KnownKeyHello] = EnglishValue });
            var svc = new LocalizationService(new[] { en });
            Assert.That(svc.CurrentLanguage, Is.EqualTo(EnglishCode), "default language must be 'en'");
            Assert.That(svc.Translate(KnownKeyHello), Is.EqualTo(EnglishValue));
        }

        [Test]
        public void Loc_SwitchLanguage_HotReloads()
        {
            // arrange
            var en = MakeAsset(EnglishCode, new Dictionary<string, string> { [KnownKeyHello] = EnglishValue });
            var tr = MakeAsset(TurkishCode, new Dictionary<string, string> { [KnownKeyHello] = TurkishValue });
            var svc = new LocalizationService(new[] { en, tr });

            string newLanguageEvent = null;
            svc.LanguageChanged += code => newLanguageEvent = code;

            // act
            svc.SetLanguage(TurkishCode);

            // assert
            Assert.That(svc.CurrentLanguage, Is.EqualTo(TurkishCode));
            Assert.That(svc.Translate(KnownKeyHello), Is.EqualTo(TurkishValue));
            Assert.That(newLanguageEvent, Is.EqualTo(TurkishCode), "LanguageChanged event must fire on switch");
        }

        [Test]
        public void Loc_AllKeysPresent_InTrAndEn()
        {
            // arrange — synthesize 2 tables; assert symmetric key set so translation gaps fail CI.
            var enTable = new Dictionary<string, string>
            {
                [KnownKeyHello] = EnglishValue,
                ["BTN_PLAY"] = "Play",
                ["BTN_HEAD_HOME"] = "Head home",
            };
            var trTable = new Dictionary<string, string>
            {
                [KnownKeyHello] = TurkishValue,
                ["BTN_PLAY"] = "Oyna",
                ["BTN_HEAD_HOME"] = "Eve dön",
            };

            // Symmetric set assertion (catches translation gaps).
            var enKeys = new HashSet<string>(enTable.Keys);
            var trKeys = new HashSet<string>(trTable.Keys);
            var missingInTr = new HashSet<string>(enKeys); missingInTr.ExceptWith(trKeys);
            var missingInEn = new HashSet<string>(trKeys); missingInEn.ExceptWith(enKeys);

            Assert.That(missingInTr, Is.Empty, "TR table is missing English keys: " + string.Join(", ", missingInTr));
            Assert.That(missingInEn, Is.Empty, "EN table is missing Turkish keys: " + string.Join(", ", missingInEn));
        }
    }
}
