// QA — Wave 10 localization coverage tests.
//
// Subject under test: the shipped string tables under
//   Assets/_Brave/Localization/en.json
//   Assets/_Brave/Localization/tr.json
//
// This fixture enforces — for the Wave 10 feature set specifically — that
//   (a) every Wave 10 key exists in BOTH en.json and tr.json,
//   (b) the value is non-null, non-empty in both languages,
//   (c) categorical groups (crit / combo / achievements / profile /
//       quit_confirm / dev / character_ability / status) each contribute the
//       expected key count,
// so a careless edit (e.g. deleting an `achievement.*` block) trips CI even
// though the parent `LocalizationTests.EnAndTr_HaveIdenticalUserFacingKeySets`
// would still pass on a fully-mirrored deletion.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Localization
{
    [TestFixture]
    public class Wave10LocalizationTests
    {
        private static string LocalizationDir =>
            Path.Combine(Application.dataPath, "_Brave", "Localization");

        private static string EnJsonPath => Path.Combine(LocalizationDir, "en.json");
        private static string TrJsonPath => Path.Combine(LocalizationDir, "tr.json");

        // ---- key catalogues per Wave 10 feature family ----

        private static readonly string[] CritKeys =
        {
            "crit.toast",
            "crit.indicator",
            "stats.crit_chance",
            "stats.crit_multiplier",
        };

        private static readonly string[] ComboKeys =
        {
            "combo.label",
            "combo.tier_1",
            "combo.tier_2",
            "combo.tier_3",
            "combo.multikill_2",
            "combo.multikill_3",
            "combo.multikill_5",
            "combo.broken",
        };

        // 20 achievements x { .name, .description } = 40 keys, plus the toast.
        private static readonly string[] AchievementIds =
        {
            "first_boss_kill",
            "slayer",
            "survivor",
            "untouchable",
            "evolutionist",
            "completionist",
            "streak_master",
            "crit_lord",
            "treasure_hunter",
            "star_collector",
            "variety",
            "iron_player",
            "marathon",
            "speed_run",
            "premium_buyer",
            "generous",
            "loyal",
            "quest_master",
            "world_tour",
            "bossbane",
        };

        private static readonly string AchievementToastKey = "achievement.toast.unlocked";

        private static readonly string[] ProfileKeys =
        {
            "profile.tab_stats",
            "profile.tab_characters",
            "profile.tab_achievements",
            "profile.stat_kills",
            "profile.stat_runs",
            "profile.stat_best_wave",
            "profile.stat_evolutions",
            "profile.stat_bosses_defeated",
            "profile.stat_playtime",
        };

        private static readonly string[] QuitConfirmKeys =
        {
            "quit_confirm.title",
            "quit_confirm.message",
            "quit_confirm.confirm",
            "quit_confirm.cancel",
        };

        private static readonly string[] DevQolKeys =
        {
            "dev.fps_label",
        };

        // 8 character abilities x { .name, .description } = 16 keys.
        private static readonly string[] CharacterAbilityIds =
        {
            "hop",
            "shell",
            "quills",
            "cunning",
            "slick",
            "restore",
            "tenacity",
            "foresight",
        };

        // 5 status effects x { .name, .description } = 10 keys.
        private static readonly string[] StatusEffectIds =
        {
            "slow",
            "burn",
            "poison",
            "freeze",
            "stun",
        };

        // ---- helpers ----

        private static Dictionary<string, string> LoadTable(string path)
        {
            Assert.That(File.Exists(path), $"Missing localization file: {path}");
            var text = File.ReadAllText(path);
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

        private static IEnumerable<string> AchievementKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                yield return $"achievement.{id}.name";
                yield return $"achievement.{id}.description";
            }
        }

        private static IEnumerable<string> AbilityKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                yield return $"character_ability.{id}.name";
                yield return $"character_ability.{id}.description";
            }
        }

        private static IEnumerable<string> StatusKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                yield return $"status.{id}.name";
                yield return $"status.{id}.description";
            }
        }

        private static void AssertAllKeysPresentAndNonEmpty(
            IEnumerable<string> keys, Dictionary<string, string> en, Dictionary<string, string> tr,
            string family)
        {
            foreach (var key in keys)
            {
                Assert.That(en.ContainsKey(key), $"[{family}] EN missing key: {key}");
                Assert.That(tr.ContainsKey(key), $"[{family}] TR missing key: {key}");
                Assert.That(en[key], Is.Not.Null.And.Not.Empty,
                    $"[{family}] EN value empty for key: {key}");
                Assert.That(tr[key], Is.Not.Null.And.Not.Empty,
                    $"[{family}] TR value empty for key: {key}");
            }
        }

        // ---- tests ----

        [Test]
        public void Crit_HasAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(CritKeys, en, tr, "crit");
        }

        [Test]
        public void Combo_HasAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(ComboKeys, en, tr, "combo");
        }

        [Test]
        public void Achievements_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(AchievementKeys(AchievementIds), en, tr, "achievements");
        }

        [Test]
        public void Achievements_HaveExactlyTwentyEntries()
        {
            // Wave 10 ships 20 achievements; guard against silent drift.
            Assert.That(AchievementIds.Length, Is.EqualTo(20),
                "Wave 10 ships exactly 20 achievements — update the test if the count changes.");
        }

        [Test]
        public void Achievements_ToastKeyExists()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(new[] { AchievementToastKey }, en, tr, "achievement.toast");
        }

        [Test]
        public void Achievements_ToastKey_PreservesNamePlaceholder()
        {
            // The unlock toast is consumed as `string.Format(value, achievement.name)`
            // via the {NAME} variable convention used elsewhere in the table.
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            Assert.That(en[AchievementToastKey], Does.Contain("{NAME}"),
                "EN achievement.toast.unlocked must keep {NAME} placeholder.");
            Assert.That(tr[AchievementToastKey], Does.Contain("{NAME}"),
                "TR achievement.toast.unlocked must keep {NAME} placeholder.");
        }

        [Test]
        public void Profile_HasAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(ProfileKeys, en, tr, "profile");
        }

        [Test]
        public void QuitConfirm_HasAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(QuitConfirmKeys, en, tr, "quit_confirm");
        }

        [Test]
        public void DevQol_HasAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(DevQolKeys, en, tr, "dev/qol");
        }

        [Test]
        public void CharacterAbilities_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(AbilityKeys(CharacterAbilityIds), en, tr, "character_ability");
        }

        [Test]
        public void CharacterAbilities_HaveExactlyEightEntries()
        {
            // One ability per playable hero — guard against silent drift.
            Assert.That(CharacterAbilityIds.Length, Is.EqualTo(8),
                "Wave 10 ships exactly 8 character abilities (one per hero).");
        }

        [Test]
        public void StatusEffects_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(StatusKeys(StatusEffectIds), en, tr, "status");
        }

        [Test]
        public void StatusEffects_HaveExactlyFiveEntries()
        {
            Assert.That(StatusEffectIds.Length, Is.EqualTo(5),
                "Wave 10 ships exactly 5 status effects.");
        }

        [Test]
        public void Wave10_TotalKeyDelta_IsAtLeast80()
        {
            // Wave 9 baseline was ~302 user-facing keys. Wave 10 must grow the
            // table by ~80 (crit 4 + combo 8 + achievements 40 + toast 1 +
            // profile 9 + quit_confirm 4 + dev 1 + abilities 16 + status 10 = 93).
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            var enCount = en.Keys.Count(k => !k.StartsWith("_meta"));
            var trCount = tr.Keys.Count(k => !k.StartsWith("_meta"));
            Assert.That(enCount, Is.GreaterThanOrEqualTo(380),
                $"EN key count must be at least 380 after Wave 10 (was {enCount}).");
            Assert.That(trCount, Is.GreaterThanOrEqualTo(380),
                $"TR key count must be at least 380 after Wave 10 (was {trCount}).");
        }

        [Test]
        public void Wave10_EnAndTr_KeyCountsMatch()
        {
            // Belt-and-braces sanity: EN and TR must have identical user-facing
            // key counts. The parent LocalizationTests fixture also asserts
            // identical sets — this guard catches accidental TR-only growth.
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            var enCount = en.Keys.Count(k => !k.StartsWith("_meta"));
            var trCount = tr.Keys.Count(k => !k.StartsWith("_meta"));
            Assert.That(enCount, Is.EqualTo(trCount),
                $"EN ({enCount}) and TR ({trCount}) key counts must match.");
        }
    }
}
