// QA — Localization key-coverage EditMode tests.
// Subject under test: the shipped string tables under
//   Assets/_Brave/Localization/en.json
//   Assets/_Brave/Localization/tr.json
//   Assets/_Brave/Localization/screenshot-keys.json
//
// These tests load the JSON files directly off disk (Application.dataPath relative)
// so a translation drift fails CI even if no one wired the TextAssets into Boot.unity.
//
// Coverage contract (per CLAUDE.md tone-bible §6 + brief):
//   (a) Every key in en.json also exists in tr.json.
//   (b) No key resolves to null/empty string in either table.
//   (c) Fallback to EN works when the active language doesn't have the key.
//   (d) Every key referenced in screenshot-keys.runtime_keys_used_for_screenshots
//       is defined in BOTH en.json and tr.json.
//   (e) JSON files load cleanly with or without a UTF-8 BOM.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brave.Systems.Localization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Localization
{
    [TestFixture]
    public class LocalizationTests
    {
        private const string EnglishCode = "en";
        private const string TurkishCode = "tr";

        private static string LocalizationDir =>
            Path.Combine(Application.dataPath, "_Brave", "Localization");

        private static string EnJsonPath => Path.Combine(LocalizationDir, "en.json");
        private static string TrJsonPath => Path.Combine(LocalizationDir, "tr.json");
        private static string ScreenshotJsonPath => Path.Combine(LocalizationDir, "screenshot-keys.json");

        /// <summary>Parse a flat JSON object file into a key→value dictionary.</summary>
        private static Dictionary<string, string> LoadTable(string path)
        {
            Assert.That(File.Exists(path), $"Missing localization file: {path}");
            var text = File.ReadAllText(path);
            // Strip UTF-8 BOM if present (parser should be resilient).
            if (text.Length > 0 && text[0] == '﻿') text = text.Substring(1);
            var root = JObject.Parse(text);
            var table = new Dictionary<string, string>();
            foreach (var kv in root)
            {
                if (kv.Value?.Type == JTokenType.String)
                {
                    table[kv.Key] = (string)kv.Value!;
                }
            }
            return table;
        }

        /// <summary>Keys with this prefix are file-level metadata, not user-facing strings.</summary>
        private static bool IsMetaKey(string key) => key.StartsWith("_meta");

        [Test]
        public void EnAndTr_HaveIdenticalUserFacingKeySets()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);

            var enKeys = new HashSet<string>(en.Keys.Where(k => !IsMetaKey(k)));
            var trKeys = new HashSet<string>(tr.Keys.Where(k => !IsMetaKey(k)));

            var missingInTr = new HashSet<string>(enKeys); missingInTr.ExceptWith(trKeys);
            var missingInEn = new HashSet<string>(trKeys); missingInEn.ExceptWith(enKeys);

            Assert.That(missingInTr, Is.Empty,
                "TR table is missing English keys: " + string.Join(", ", missingInTr));
            Assert.That(missingInEn, Is.Empty,
                "EN table is missing Turkish keys: " + string.Join(", ", missingInEn));
        }

        [Test]
        public void EnAndTr_HaveAtLeast150UserFacingKeys()
        {
            // Minimum coverage target per brief — guards against regressions that
            // would re-introduce inline English in UXML.
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);

            var enCount = en.Keys.Count(k => !IsMetaKey(k));
            var trCount = tr.Keys.Count(k => !IsMetaKey(k));

            Assert.That(enCount, Is.GreaterThanOrEqualTo(150),
                $"EN key count fell below floor: {enCount}");
            Assert.That(trCount, Is.GreaterThanOrEqualTo(150),
                $"TR key count fell below floor: {trCount}");
        }

        [Test]
        public void NoKey_ResolvesToNullOrEmpty()
        {
            foreach (var path in new[] { EnJsonPath, TrJsonPath })
            {
                var table = LoadTable(path);
                foreach (var kv in table)
                {
                    if (IsMetaKey(kv.Key)) continue;
                    Assert.That(kv.Value, Is.Not.Null.And.Not.Empty,
                        $"{Path.GetFileName(path)}: key '{kv.Key}' is empty");
                }
            }
        }

        [Test]
        public void LocalizationService_FallsBackToEnglish_WhenKeyMissingInActiveLanguage()
        {
            // arrange
            var en = MakeTextAsset(EnglishCode, new Dictionary<string, string>
            {
                ["GREETING"] = "Hi.",
                ["ONLY_IN_EN"] = "EN baseline string."
            });
            var tr = MakeTextAsset(TurkishCode, new Dictionary<string, string>
            {
                ["GREETING"] = "Selam.",
                // ONLY_IN_EN intentionally omitted.
            });
            var svc = new LocalizationService(new[] { en, tr });
            svc.SetLanguage(TurkishCode);

            // act + assert: TR-defined key resolves in TR.
            Assert.That(svc.Translate("GREETING"), Is.EqualTo("Selam."));
            // EN-only key falls back to EN value, NOT the key.
            Assert.That(svc.Translate("ONLY_IN_EN"), Is.EqualTo("EN baseline string."));
        }

        [Test]
        public void LocalizationService_HandlesUtf8Bom()
        {
            // arrange: prepend a UTF-8 BOM to the JSON payload.
            var raw = "{\"WAVE_INCOMING\":\"Heads up.\"}";
            var bom = "﻿" + raw;
            var asset = new TextAsset(bom) { name = EnglishCode };

            // act
            var svc = new LocalizationService(new[] { asset });

            // assert
            Assert.That(svc.Translate("WAVE_INCOMING"), Is.EqualTo("Heads up."),
                "Service must parse files written with a UTF-8 BOM.");
        }

        [Test]
        public void ScreenshotKeys_AreAllDefined_InEnAndTr()
        {
            Assert.That(File.Exists(ScreenshotJsonPath),
                $"Missing screenshot-keys file: {ScreenshotJsonPath}");

            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);

            var screenshotJson = JObject.Parse(File.ReadAllText(ScreenshotJsonPath));
            var runtimeRefs = screenshotJson["runtime_keys_used_for_screenshots"] as JArray;
            Assert.That(runtimeRefs, Is.Not.Null,
                "screenshot-keys.json must list runtime_keys_used_for_screenshots[] " +
                "so the screenshot pipeline knows which keys it consumes.");

            foreach (var token in runtimeRefs!)
            {
                var key = (string)token!;
                Assert.That(en.ContainsKey(key), $"EN missing screenshot key: {key}");
                Assert.That(tr.ContainsKey(key), $"TR missing screenshot key: {key}");
            }
        }

        [Test]
        public void Translate_ReturnsKey_WhenNoTableLoadedAndKeyMissing()
        {
            // Guard the "screamingly visible miss" contract — QA must see the raw key
            // rather than an empty label when a string isn't translated yet.
            var svc = new LocalizationService(System.Array.Empty<TextAsset>());
            Assert.That(svc.Translate("TOTALLY_UNDEFINED"), Is.EqualTo("TOTALLY_UNDEFINED"));
        }

        // ---- helpers ----

        private static TextAsset MakeTextAsset(string langCode, IDictionary<string, string> table)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(table);
            return new TextAsset(json) { name = langCode };
        }
    }
}
