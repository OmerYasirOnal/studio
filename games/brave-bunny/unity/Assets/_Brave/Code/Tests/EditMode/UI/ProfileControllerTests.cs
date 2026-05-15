// QA — ProfileController EditMode tests (Wave 10).
//
// Subject under test:
//   * Brave.UI.Controllers.ProfileScreenLogic — the pure-C# render path for the
//     Profile screen. Tests cover stat-row formatting, roster snapshot building
//     against ICharacterUnlockService, locked-vs-unlocked styling, and tab
//     selection toggling.
//
// Pattern: matches LoadoutControllerTests + QuestPanelControllerTests — exercise
// the static logic with raw VisualElement / Label instances; no UIDocument
// required, so EditMode is fast.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using Brave.UI.Controllers;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class ProfileControllerTests
    {
        // ---- constants ----
        private const string SlugBunny = "bunny";
        private const string SlugFox = "fox";
        private const string SlugTortoise = "tortoise";
        private const long Kills = 1234;
        private const long Runs = 17;
        private const long Bosses = 5;
        private const long Evolutions = 3;
        private const int BestWave = 22;
        private const float BestRunTimeSeconds = 305f; // 5:05
        private const double PlaytimeSeconds = 7384.0; // 2h 03m

        // ---- test doubles ----

        /// <summary>Identity translator: returns the key untouched.</summary>
        private static string IdentityTr(string key) => key;

        /// <summary>Fake unlock service backed by an in-memory set.</summary>
        private sealed class FakeUnlockService : ICharacterUnlockService
        {
            public readonly HashSet<string> Unlocked = new(StringComparer.Ordinal);
#pragma warning disable CS0067 // Event never used — interface contract only.
            public event Action<string>? CharacterUnlocked;
#pragma warning restore CS0067
            public bool IsUnlocked(string slug) => Unlocked.Contains(slug);
            public IReadOnlyList<string> GetUnlockedCharacterIds() => new List<string>(Unlocked);
            public IReadOnlyList<string> EvaluateAll() => Array.Empty<string>();
            public void RecordRunCompletion(string slug, int wave, int bosses) { }
            public void RecordBossDefeated(string bossSlug, string charSlug) { }
            public bool TryPurchase(string slug, CurrencyWallet wallet) => false;
        }

        private static SaveData.StatsSection MakeStats() => new SaveData.StatsSection
        {
            TotalKills            = Kills,
            TotalRuns             = Runs,
            BossesDefeated        = Bosses,
            EvolutionsTriggered   = Evolutions,
            BestWaveReached       = BestWave,
            BestRunTimeSeconds    = BestRunTimeSeconds,
            TotalPlaytimeSeconds  = PlaytimeSeconds,
        };

        // ============================================================
        // Stats tab rendering
        // ============================================================

        [Test]
        public void BuildStatRows_FormatsAllSevenStats()
        {
            var rows = ProfileScreenLogic.BuildStatRows(MakeStats(), IdentityTr);
            Assert.That(rows.Count, Is.EqualTo(7), "Seven stat rows per UXML spec.");
            // Identity translator → labels are the raw loc keys.
            Assert.That(rows[0].Label, Is.EqualTo(ProfileScreenLogic.LocStatKills));
            Assert.That(rows[0].Value, Is.EqualTo("1234"),
                "Total kills must render as invariant-culture integer.");
            Assert.That(rows[1].Value, Is.EqualTo("17"));
            Assert.That(rows[2].Value, Is.EqualTo("22"));
            Assert.That(rows[3].Value, Is.EqualTo("5:05"),
                "Best run time must render as M:SS.");
            Assert.That(rows[4].Value, Is.EqualTo("5"));
            Assert.That(rows[5].Value, Is.EqualTo("3"));
            Assert.That(rows[6].Value, Is.EqualTo("2h 03m"),
                "Total playtime must render as Xh YYm.");
        }

        [Test]
        public void FormatPlaytime_ZeroSeconds_RendersZeroHoursZeroMinutes()
        {
            Assert.That(ProfileScreenLogic.FormatPlaytime(0), Is.EqualTo("0h 00m"));
        }

        [Test]
        public void FormatRunTime_NegativeClampsToZero()
        {
            Assert.That(ProfileScreenLogic.FormatRunTime(-5f), Is.EqualTo("0:00"));
        }

        [Test]
        public void RenderStatValues_PushesIntoNamedLabels()
        {
            var root = new VisualElement();
            foreach (var name in new[]
            {
                "lbl-stat-kills-value", "lbl-stat-runs-value", "lbl-stat-best-wave-value",
                "lbl-stat-best-time-value", "lbl-stat-bosses-value", "lbl-stat-evolutions-value",
                "lbl-stat-playtime-value",
            })
            {
                root.Add(new Label("?") { name = name });
            }

            var ok = ProfileScreenLogic.RenderStatValues(root, MakeStats(), IdentityTr);
            Assert.That(ok, Is.True);
            Assert.That(root.Q<Label>("lbl-stat-kills-value")!.text, Is.EqualTo("1234"));
            Assert.That(root.Q<Label>("lbl-stat-runs-value")!.text, Is.EqualTo("17"));
            Assert.That(root.Q<Label>("lbl-stat-best-wave-value")!.text, Is.EqualTo("22"));
            Assert.That(root.Q<Label>("lbl-stat-best-time-value")!.text, Is.EqualTo("5:05"));
            Assert.That(root.Q<Label>("lbl-stat-bosses-value")!.text, Is.EqualTo("5"));
            Assert.That(root.Q<Label>("lbl-stat-evolutions-value")!.text, Is.EqualTo("3"));
            Assert.That(root.Q<Label>("lbl-stat-playtime-value")!.text, Is.EqualTo("2h 03m"));
        }

        [Test]
        public void RenderStatValues_NullRoot_ReturnsFalse()
        {
            Assert.That(ProfileScreenLogic.RenderStatValues(null, MakeStats(), IdentityTr), Is.False);
        }

        // ============================================================
        // Characters tab rendering
        // ============================================================

        [Test]
        public void BuildRoster_PreservesSlugOrderAndUnlockState()
        {
            var unlock = new FakeUnlockService();
            unlock.Unlocked.Add(SlugBunny);
            var profiles = new Dictionary<string, CharacterProfile>(StringComparer.Ordinal)
            {
                [SlugBunny] = new CharacterProfile
                {
                    Unlocked = true,
                    RunsCompleted = 4,
                    BossesDefeated = 1,
                    HighestWaveReached = 9,
                },
            };
            var roster = ProfileScreenLogic.BuildRoster(
                new[] { SlugBunny, SlugFox, SlugTortoise }, unlock, profiles);

            Assert.That(roster.Count, Is.EqualTo(3));
            Assert.That(roster[0].Slug, Is.EqualTo(SlugBunny));
            Assert.That(roster[0].IsUnlocked, Is.True);
            Assert.That(roster[0].RunsCompleted, Is.EqualTo(4));
            Assert.That(roster[0].BossesDefeated, Is.EqualTo(1));
            Assert.That(roster[0].HighestWaveReached, Is.EqualTo(9));

            Assert.That(roster[1].Slug, Is.EqualTo(SlugFox));
            Assert.That(roster[1].IsUnlocked, Is.False);
            Assert.That(roster[1].RunsCompleted, Is.EqualTo(0),
                "No profile entry → zero stats.");
        }

        [Test]
        public void RenderRoster_BuildsOneCardPerSlug()
        {
            var unlock = new FakeUnlockService();
            unlock.Unlocked.Add(SlugBunny);

            var roster = ProfileScreenLogic.BuildRoster(
                new[] { SlugBunny, SlugFox },
                unlock,
                new Dictionary<string, CharacterProfile>(StringComparer.Ordinal));

            var list = new VisualElement();
            ProfileScreenLogic.RenderRoster(list, template: null, roster, IdentityTr);

            Assert.That(list.childCount, Is.EqualTo(2));
            Assert.That(list.Q<VisualElement>($"row-{SlugBunny}"), Is.Not.Null);
            Assert.That(list.Q<VisualElement>($"row-{SlugFox}"), Is.Not.Null);
        }

        [Test]
        public void BuildCharacterCard_LockedCharacter_HasLockedClass()
        {
            var row = new CharacterRosterRow(SlugFox, isUnlocked: false,
                runsCompleted: 0, bossesDefeated: 0, highestWaveReached: 0);
            var card = ProfileScreenLogic.BuildCharacterCard(template: null, row, IdentityTr);

            Assert.That(card.ClassListContains(ProfileScreenLogic.LockedRowClass), Is.True,
                "Locked roster row must carry the greyed-out USS class.");
        }

        [Test]
        public void BuildCharacterCard_UnlockedCharacter_DoesNotHaveLockedClass()
        {
            var row = new CharacterRosterRow(SlugBunny, isUnlocked: true,
                runsCompleted: 2, bossesDefeated: 0, highestWaveReached: 5);
            var card = ProfileScreenLogic.BuildCharacterCard(template: null, row, IdentityTr);

            Assert.That(card.ClassListContains(ProfileScreenLogic.LockedRowClass), Is.False);
        }

        [Test]
        public void BuildCharacterCard_NameLabel_UsesCharactersSlugNameKey()
        {
            var row = new CharacterRosterRow(SlugBunny, isUnlocked: true, 0, 0, 0);
            var card = ProfileScreenLogic.BuildCharacterCard(null, row, IdentityTr);
            var nameLabel = card.Q<Label>($"lbl-{SlugBunny}-name");
            Assert.That(nameLabel, Is.Not.Null);
            Assert.That(nameLabel!.text, Is.EqualTo($"characters.{SlugBunny}.name"),
                "Name label routes through the characters.<slug>.name loc key.");
        }

        [Test]
        public void BuildCharacterCard_BioLabel_UsesCharactersSlugBioKey()
        {
            var row = new CharacterRosterRow(SlugBunny, isUnlocked: true, 0, 0, 0);
            var card = ProfileScreenLogic.BuildCharacterCard(null, row, IdentityTr);
            var bioLabel = card.Q<Label>($"lbl-{SlugBunny}-bio");
            Assert.That(bioLabel, Is.Not.Null);
            Assert.That(bioLabel!.text, Is.EqualTo($"characters.{SlugBunny}.bio"));
        }

        [Test]
        public void BuildProgressText_LockedRow_UsesUnlockHintKey()
        {
            var row = new CharacterRosterRow(SlugFox, isUnlocked: false, 0, 0, 0);
            // Translator returns a non-key string only when the key is in our table.
            string Tr(string k) => k == $"characters.{SlugFox}.unlock_hint"
                ? "Beat the boss with Bunny." : k;
            var text = ProfileScreenLogic.BuildProgressText(row, Tr);
            Assert.That(text, Is.EqualTo("Beat the boss with Bunny."),
                "Locked rows render the unlock_hint loc value when present.");
        }

        [Test]
        public void BuildProgressText_LockedRow_FallsBackToLockedKey()
        {
            var row = new CharacterRosterRow(SlugFox, isUnlocked: false, 0, 0, 0);
            // Identity translator → unlock_hint key resolves to itself, so the
            // fallback path kicks in and returns the LocCharLocked key.
            var text = ProfileScreenLogic.BuildProgressText(row, IdentityTr);
            Assert.That(text, Is.EqualTo(ProfileScreenLogic.LocCharLocked));
        }

        [Test]
        public void BuildProgressText_UnlockedRow_FormatsStatsLine()
        {
            var row = new CharacterRosterRow(SlugBunny, isUnlocked: true,
                runsCompleted: 4, bossesDefeated: 1, highestWaveReached: 9);
            string Tr(string k) => k switch
            {
                ProfileScreenLogic.LocCharRunsFmt => "Runs {count}",
                ProfileScreenLogic.LocCharBossesFmt => "Bosses {count}",
                ProfileScreenLogic.LocCharBestWaveFmt => "Wave {count}",
                _ => k,
            };
            var text = ProfileScreenLogic.BuildProgressText(row, Tr);
            Assert.That(text, Does.Contain("Runs 4"));
            Assert.That(text, Does.Contain("Bosses 1"));
            Assert.That(text, Does.Contain("Wave 9"));
        }

        // ============================================================
        // Tab toggling
        // ============================================================

        [Test]
        public void ApplyTabSelection_ActiveTabHasIsActiveClass()
        {
            var root = new VisualElement();
            var btnStats = new Button { name = "tab-stats" };
            var btnChars = new Button { name = "tab-characters" };
            var btnAch = new Button { name = "tab-achievements" };
            root.Add(btnStats); root.Add(btnChars); root.Add(btnAch);

            var panelStats = new VisualElement { name = "profile-stats-tab" };
            var panelChars = new VisualElement { name = "profile-characters-tab" };
            var panelAch = new VisualElement { name = "profile-achievements-tab" };
            root.Add(panelStats); root.Add(panelChars); root.Add(panelAch);

            var map = new[]
            {
                ("tab-stats",        "profile-stats-tab",        ProfileScreenLogic.Tab.Stats),
                ("tab-characters",   "profile-characters-tab",   ProfileScreenLogic.Tab.Characters),
                ("tab-achievements", "profile-achievements-tab", ProfileScreenLogic.Tab.Achievements),
            };

            ProfileScreenLogic.ApplyTabSelection(root, ProfileScreenLogic.Tab.Characters, map);

            Assert.That(btnStats.ClassListContains(ProfileScreenLogic.ActiveTabClass), Is.False);
            Assert.That(btnChars.ClassListContains(ProfileScreenLogic.ActiveTabClass), Is.True);
            Assert.That(btnAch.ClassListContains(ProfileScreenLogic.ActiveTabClass), Is.False);

            Assert.That(panelStats.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(panelChars.style.display.value, Is.EqualTo(DisplayStyle.Flex));
            Assert.That(panelAch.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        // ============================================================
        // Loc-key stability — guards against renames
        // ============================================================

        [Test]
        public void LocKeys_AreStable()
        {
            // The Wave-10 loc handoff lists these exact keys. A rename here
            // requires a coordinated handoff update.
            Assert.That(ProfileScreenLogic.LocTitle, Is.EqualTo("profile.title"));
            Assert.That(ProfileScreenLogic.LocTabStats, Is.EqualTo("profile.tab_stats"));
            Assert.That(ProfileScreenLogic.LocTabCharacters, Is.EqualTo("profile.tab_characters"));
            Assert.That(ProfileScreenLogic.LocTabAchievements, Is.EqualTo("profile.tab_achievements"));
            Assert.That(ProfileScreenLogic.LocStatKills, Is.EqualTo("profile.stat_kills"));
            Assert.That(ProfileScreenLogic.LocStatRuns, Is.EqualTo("profile.stat_runs"));
            Assert.That(ProfileScreenLogic.LocStatBestWave, Is.EqualTo("profile.stat_best_wave"));
            Assert.That(ProfileScreenLogic.LocStatBestTime, Is.EqualTo("profile.stat_best_time"));
            Assert.That(ProfileScreenLogic.LocStatBosses, Is.EqualTo("profile.stat_bosses"));
            Assert.That(ProfileScreenLogic.LocStatEvolutions, Is.EqualTo("profile.stat_evolutions"));
            Assert.That(ProfileScreenLogic.LocStatPlaytime, Is.EqualTo("profile.stat_playtime"));
        }
    }
}
