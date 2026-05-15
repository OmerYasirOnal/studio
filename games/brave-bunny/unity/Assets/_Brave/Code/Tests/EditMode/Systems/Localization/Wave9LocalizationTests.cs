// QA — Wave 9 localization coverage tests.
//
// Subject under test: the shipped string tables under
//   Assets/_Brave/Localization/en.json
//   Assets/_Brave/Localization/tr.json
//
// This fixture enforces — for the Wave 9 feature set specifically — that
//   (a) every Wave 9 key exists in BOTH en.json and tr.json,
//   (b) the value is non-null, non-empty in both languages,
//   (c) categorical groups (weapons / enemies / daily / quest / battlepass /
//       shop / biome) each contribute the expected key count,
// so a careless edit (e.g. deleting a `shop.*` block) trips CI even though
// the parent `LocalizationTests.EnAndTr_HaveIdenticalUserFacingKeySets` would
// still pass on a fully-mirrored deletion.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Localization
{
    [TestFixture]
    public class Wave9LocalizationTests
    {
        private static string LocalizationDir =>
            Path.Combine(Application.dataPath, "_Brave", "Localization");

        private static string EnJsonPath => Path.Combine(LocalizationDir, "en.json");
        private static string TrJsonPath => Path.Combine(LocalizationDir, "tr.json");

        // ---- key catalogues per Wave 9 feature family ----

        private static readonly string[] NewWeaponBaseIds =
        {
            "storm-cloud",
            "sapling-summon",
            "maple-boomerang",
            "sunflower-beam",
            "cherry-bomb",
            "wasp-swarm",
        };

        // Evolutions already lived in en/tr before Wave 9 but are exercised here
        // so a follow-up rename in `data/balance/weapons.json` would surface as a
        // missing localization key at test time, not at runtime.
        private static readonly string[] EvolutionWeaponIds =
        {
            "harvest-cyclone",
            "solar-halo",
            "meadow-bloom",
            "stone-storm",
            "oak-thunderclap",
            "cornfield-volley",
            "honey-hug",
            "pinwheel-storm",
        };

        private static readonly string[] NewEnemyIds =
        {
            "crab",
            "gull",
            "sand-puff",
            "mosquito",
            "bog-boar",
            "throw-frog",
            "big-hermit-crab",
            "bat-mini",
            "glow-bug",
            "rock-tumble",
            "cave-slime",
            "stone-ox",
            "crystal-slinger",
            "stalagmite-walker",
        };

        private static readonly string[] BiomeIds =
        {
            "meadow",
            "beach",
            "forest",
            "cave",
            "snow",
        };

        private static readonly string[] DailyRewardKeys =
        {
            "daily.title",
            "daily.day_1",
            "daily.day_2",
            "daily.day_3",
            "daily.day_4",
            "daily.day_5",
            "daily.day_6",
            "daily.day_7",
            "daily.claim",
            "daily.claimed",
            "daily.come_back_tomorrow",
        };

        private static readonly string[] QuestKeys =
        {
            "quest.title",
            "quest.progress",
            "quest.claim",
            "quest.claimed",
            "quest.kill_enemies.title",
            "quest.survive_waves.title",
            "quest.defeat_boss.title",
            "quest.reach_level.title",
            "quest.collect_gold.title",
            "quest.run_duration.title",
        };

        private static readonly string[] BattlePassKeys =
        {
            "battlepass.title",
            "battlepass.tier_progress",
            "battlepass.claim",
            "battlepass.locked",
            "battlepass.premium_required",
            "battlepass.season_ends_in",
        };

        private static readonly string[] ShopKeys =
        {
            "shop.tab_currency",
            "shop.tab_characters",
            "shop.tab_specials",
            "shop.tab_battlepass",
            "shop.buy",
            "shop.owned",
            "shop.purchase_success",
            "shop.purchase_failed",
            "shop.restore_purchases",
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

        private static IEnumerable<string> WeaponKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                yield return $"weapons.{id}.name";
                yield return $"weapons.{id}.description";
            }
        }

        private static IEnumerable<string> EnemyKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids) yield return $"enemies.{id}.name";
        }

        private static IEnumerable<string> BiomeKeys(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                yield return $"biomes.{id}.name";
                yield return $"biomes.{id}.description";
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
        public void NewWeapons_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(WeaponKeys(NewWeaponBaseIds), en, tr, "weapons (new)");
        }

        [Test]
        public void EvolutionWeapons_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(WeaponKeys(EvolutionWeaponIds), en, tr, "weapons (evolutions)");
        }

        [Test]
        public void NewEnemies_HaveLocalizedName()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(EnemyKeys(NewEnemyIds), en, tr, "enemies (new)");
        }

        [Test]
        public void Biomes_HaveLocalizedNameAndDescription()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(BiomeKeys(BiomeIds), en, tr, "biomes");
        }

        [Test]
        public void DailyRewards_HaveAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(DailyRewardKeys, en, tr, "daily");
        }

        [Test]
        public void Quests_HaveAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(QuestKeys, en, tr, "quest");
        }

        [Test]
        public void Quests_ProgressKey_PreservesPositionalPlaceholders()
        {
            // `quest.progress` is consumed as `string.Format(value, current, total)` so
            // the {0}/{1} positional placeholders MUST appear verbatim in TR.
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            Assert.That(en["quest.progress"], Does.Contain("{0}").And.Contain("{1}"),
                "EN quest.progress must keep {0} and {1} positional placeholders.");
            Assert.That(tr["quest.progress"], Does.Contain("{0}").And.Contain("{1}"),
                "TR quest.progress must keep {0} and {1} positional placeholders.");
        }

        [Test]
        public void BattlePass_HaveAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(BattlePassKeys, en, tr, "battlepass");
        }

        [Test]
        public void Shop_HaveAllExpectedKeys()
        {
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            AssertAllKeysPresentAndNonEmpty(ShopKeys, en, tr, "shop");
        }

        [Test]
        public void Wave9_TotalKeyDelta_IsAtLeast70()
        {
            // Wave 7A baseline was 230 user-facing keys (cf. brief). Wave 9 must
            // grow the table by at least 70 keys (6 weapons × 2 + 14 enemies
            // + 5 biomes × 2 + 11 daily + 10 quest + 6 pass + 9 shop = 72).
            var en = LoadTable(EnJsonPath);
            var tr = LoadTable(TrJsonPath);
            var enCount = en.Keys.Count(k => !k.StartsWith("_meta"));
            var trCount = tr.Keys.Count(k => !k.StartsWith("_meta"));
            Assert.That(enCount, Is.GreaterThanOrEqualTo(300),
                $"EN key count must be at least 300 after Wave 9 (was {enCount}).");
            Assert.That(trCount, Is.GreaterThanOrEqualTo(300),
                $"TR key count must be at least 300 after Wave 9 (was {trCount}).");
        }
    }
}
