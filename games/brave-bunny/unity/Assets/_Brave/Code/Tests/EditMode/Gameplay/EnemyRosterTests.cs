// QA — Enemy Roster schema/invariant tests (Wave 9)
// Subject under test: data/balance/enemies.json (source of truth) + BiomeRegistry.
//
// What we guard:
//   * Every enemy has the required fields (id, display_name, role, biome, scaling).
//   * `role` is one of the 5 valid EnemyRole enum names (kebab-case → enum match).
//   * `biome` matches a registered slug in BiomeRegistry.
//   * Role → BehaviorChooser dispatch resolves (non-null for non-boss roles).
//   * `scaling.hp_base` is present for non-boss roles; bosses use hp_mid_boss / hp_end_boss.
//   * `defense_mult`, when present, is in [0, 0.75] per docs/10-balance/00-formulas.md §11.
//   * `id` is unique kebab-case across the file.
//
// Cross-refs:
//   * data/balance/enemies.schema.md       — field schema
//   * docs/10-balance/00-formulas.md §9    — per-minute scaling formula
//   * ADR-0006                              — HP scaling
//   * ADR-0020                              — EnemyRole.Boss exists; non-boss must resolve a behavior

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brave.Gameplay.AI;
using Brave.Gameplay.Definitions;
using Brave.Systems.LiveOps;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class EnemyRosterTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        // Path matches BalanceJsonImporter.BalanceDataDir: Application.dataPath/../../data/balance.
        private const string BalanceRelativeDir = "../../data/balance";
        private const string EnemiesFileName    = "enemies.json";

        private const float MinDefenseMult = 0f;
        private const float MaxDefenseMult = 0.75f;
        private const int MinSchemaMajor   = 1;

        // Required scaling sub-fields (non-boss). Bosses use the hp_mid/hp_end variant set.
        private static readonly string[] NonBossRequiredScaling =
        {
            "hp_base", "hp_per_min", "contact_dmg", "speed_mult_vs_player"
        };
        // Bosses are exempt from hp_base / hp_per_min; they ship hp_mid_boss + hp_end_boss instead.
        private static readonly string[] BossRequiredScaling =
        {
            "contact_dmg", "speed_mult_vs_player"
        };
        private const string BossHpMid = "hp_mid_boss";
        private const string BossHpEnd = "hp_end_boss";

        // Wave 9 lower-bound: 5 Meadow + 5+ new = >= 10 entries. Pin a soft floor.
        private const int MinEntryCount = 10;

        // Cached roster — loaded once, reused across tests.
        private static JArray? _enemies;
        private static JObject? _root;

        [OneTimeSetUp]
        public void LoadRoster()
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, BalanceRelativeDir, EnemiesFileName));
            Assert.That(File.Exists(path), Is.True,
                $"enemies.json must exist at {path} (BalanceJsonImporter convention)");
            var text = File.ReadAllText(path);
            _root = JObject.Parse(text);
            Assert.That(_root, Is.Not.Null, "enemies.json must parse as a JSON object");
            var arr = _root!["enemies"] as JArray;
            Assert.That(arr, Is.Not.Null, "enemies.json must have a top-level 'enemies' array");
            _enemies = arr;
        }

        // ---- file-level shape ----

        [Test]
        public void Roster_HasSchemaVersion_AtLeastMajor1()
        {
            var v = _root!.Value<string>("schema_version");
            Assert.That(v, Is.Not.Null.And.Not.Empty,
                "enemies.json must have 'schema_version' top-level field");
            // Accept "1", "1.0", "1.1", … — anything that parses to major >= 1.
            int major = ParseMajor(v);
            Assert.That(major, Is.GreaterThanOrEqualTo(MinSchemaMajor),
                $"schema_version major must be >= {MinSchemaMajor}");
        }

        private static int ParseMajor(string? version)
        {
            if (string.IsNullOrEmpty(version)) return -1;
            var head = version!.Split('.')[0];
            return int.TryParse(head, out var n) ? n : -1;
        }

        [Test]
        public void Roster_HasAtLeastWave9EntryCount()
        {
            Assert.That(_enemies!.Count, Is.GreaterThanOrEqualTo(MinEntryCount),
                $"enemies.json must ship >= {MinEntryCount} entries after Wave 9 expansion (Beach + Cavern)");
        }

        // ---- per-entry invariants ----

        [Test]
        public void Roster_AllIdsUnique_KebabCase()
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in EnemyEntries())
            {
                var id = entry.Value<string>("id");
                Assert.That(id, Is.Not.Null.And.Not.Empty,
                    $"entry must have non-empty 'id' (entry={entry})");
                Assert.That(IsKebabCase(id!), Is.True,
                    $"id '{id}' must be kebab-case [a-z0-9-]+");
                Assert.That(seen.Add(id!), Is.True, $"duplicate id: '{id}'");
            }
        }

        [Test]
        public void Roster_AllEntries_HaveDisplayName()
        {
            foreach (var entry in EnemyEntries())
            {
                var dn = entry.Value<string>("display_name");
                Assert.That(dn, Is.Not.Null.And.Not.Empty,
                    $"entry '{entry.Value<string>("id")}' must have non-empty display_name");
            }
        }

        [Test]
        public void Roster_AllRoles_ResolveToValidEnemyRoleEnum()
        {
            foreach (var entry in EnemyEntries())
            {
                var id   = entry.Value<string>("id");
                var role = entry.Value<string>("role");
                Assert.That(role, Is.Not.Null.And.Not.Empty,
                    $"entry '{id}' must declare a 'role'");
                var parsed = TryParseRole(role!);
                Assert.That(parsed.HasValue, Is.True,
                    $"entry '{id}': role '{role}' is not a known EnemyRole");
            }
        }

        [Test]
        public void Roster_NonBossRoles_HaveResolvableBehavior()
        {
            // BehaviorChooser returns null only for Boss (per ADR-0020 / chooser comment).
            // Every non-boss role must dispatch to a non-null behavior singleton.
            foreach (var entry in EnemyEntries())
            {
                var id   = entry.Value<string>("id");
                var role = entry.Value<string>("role");
                var roleEnum = TryParseRole(role!);
                Assume.That(roleEnum.HasValue, Is.True);
                if (roleEnum!.Value == EnemyRole.Boss) continue;

                var behavior = BehaviorChooser.For(roleEnum.Value);
                Assert.That(behavior, Is.Not.Null,
                    $"entry '{id}' (role={role}): BehaviorChooser.For must return a non-null behavior for non-boss roles");
            }
        }

        [Test]
        public void Roster_BossEntries_ReturnNullFromBehaviorChooser_RoutesViaBossSpawner()
        {
            // Inverse of the above — bosses are handled by BossSpawner, NOT BehaviorChooser.
            int bossCount = 0;
            foreach (var entry in EnemyEntries())
            {
                var role = entry.Value<string>("role");
                var roleEnum = TryParseRole(role!);
                if (roleEnum != EnemyRole.Boss) continue;
                bossCount++;
                Assert.That(BehaviorChooser.For(EnemyRole.Boss), Is.Null,
                    "BehaviorChooser.For(Boss) must return null — boss behavior is per-instance via BossSpawner");
            }
            // Launch policy: single boss (Old Boar King) — guard against accidental boss-count drift.
            Assert.That(bossCount, Is.EqualTo(1),
                "Launch policy: exactly one 'boss' role enemy (Old Boar King). Add new bosses through a separate ADR.");
        }

        [Test]
        public void Roster_AllBiomes_ResolveToBiomeRegistry()
        {
            foreach (var entry in EnemyEntries())
            {
                var id    = entry.Value<string>("id");
                var biome = entry.Value<string>("biome");
                Assert.That(biome, Is.Not.Null.And.Not.Empty,
                    $"entry '{id}' must declare a 'biome' slug");
                Assert.That(BiomeRegistry.TryResolveBySlug(biome, out _), Is.True,
                    $"entry '{id}': biome '{biome}' is not registered in BiomeRegistry");
            }
        }

        [Test]
        public void Roster_NonBoss_HaveRequiredScalingFields()
        {
            foreach (var entry in EnemyEntries())
            {
                var id   = entry.Value<string>("id");
                var role = TryParseRole(entry.Value<string>("role")!);
                Assume.That(role.HasValue, Is.True);
                if (role!.Value == EnemyRole.Boss) continue;

                var scaling = entry["scaling"] as JObject;
                Assert.That(scaling, Is.Not.Null, $"entry '{id}' must have 'scaling' object");
                foreach (var field in NonBossRequiredScaling)
                {
                    Assert.That(scaling![field], Is.Not.Null,
                        $"entry '{id}' (role={role}) must have 'scaling.{field}'");
                }
            }
        }

        [Test]
        public void Roster_Boss_UsesMidEndHpFields()
        {
            foreach (var entry in EnemyEntries())
            {
                var id   = entry.Value<string>("id");
                var role = TryParseRole(entry.Value<string>("role")!);
                Assume.That(role.HasValue, Is.True);
                if (role!.Value != EnemyRole.Boss) continue;

                var scaling = entry["scaling"] as JObject;
                Assert.That(scaling, Is.Not.Null, $"boss '{id}' must have 'scaling' object");
                Assert.That(scaling![BossHpMid], Is.Not.Null,
                    $"boss '{id}' must have 'scaling.{BossHpMid}'");
                Assert.That(scaling![BossHpEnd], Is.Not.Null,
                    $"boss '{id}' must have 'scaling.{BossHpEnd}'");
                foreach (var field in BossRequiredScaling)
                {
                    Assert.That(scaling![field], Is.Not.Null,
                        $"boss '{id}' must have 'scaling.{field}'");
                }
            }
        }

        [Test]
        public void Roster_DefenseMult_InClampRange()
        {
            foreach (var entry in EnemyEntries())
            {
                var id      = entry.Value<string>("id");
                var scaling = entry["scaling"] as JObject;
                if (scaling == null) continue;
                var dm = scaling.Value<float?>("defense_mult");
                if (!dm.HasValue) continue;
                Assert.That(dm.Value, Is.InRange(MinDefenseMult, MaxDefenseMult),
                    $"entry '{id}': defense_mult {dm.Value} out of [{MinDefenseMult}, {MaxDefenseMult}] per formulas §11");
            }
        }

        // ---- helpers ----

        private static IEnumerable<JObject> EnemyEntries() =>
            _enemies!.OfType<JObject>();

        private static bool IsKebabCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool ok = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-';
                if (!ok) return false;
            }
            // No leading/trailing dash, no consecutive dashes.
            if (s[0] == '-' || s[s.Length - 1] == '-') return false;
            return !s.Contains("--");
        }

        private static EnemyRole? TryParseRole(string raw)
        {
            // JSON ships "swarmer"/"tank"/"ranged"/"elite"/"boss" — case-insensitive enum match.
            foreach (EnemyRole r in Enum.GetValues(typeof(EnemyRole)))
            {
                if (string.Equals(r.ToString(), raw, StringComparison.OrdinalIgnoreCase))
                    return r;
            }
            return null;
        }
    }
}
