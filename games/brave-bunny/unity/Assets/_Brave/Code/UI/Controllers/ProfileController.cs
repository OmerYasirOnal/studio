// Brave Bunny — UI / Controllers / ProfileController (Wave 10).
// Bound to: _Brave/UI/Documents/Profile.uxml
//
// Three-tab player-profile screen:
//   * Stats        — lifetime totals from SaveData.Stats (kills, runs, best wave,
//                    best run time, bosses defeated, evolutions, playtime).
//   * Characters   — full roster sourced from a serializable string[] of slugs,
//                    with bios + per-character unlock progress from
//                    ICharacterUnlockService + SaveData.Characters. Locked
//                    characters render in the `profile-char-row-locked` style.
//   * Achievements — out of scope; this tab carries a single button that pushes
//                    the AchievementsPanel screen (owned by a separate agent).
//
// Pattern mirrors ShopController + LoadoutController:
//   * Pure-C# `ProfileScreenLogic` static class with the render path, exercised
//     by EditMode tests without spinning up a UIDocument.
//   * MonoBehaviour shell resolves services from GameContext, hands data to the
//     logic class, and wires localization at OnEnable.
//   * Allocation-free in the steady-state — re-renders only on tab change /
//     OnEnable / explicit Refresh(); not on Update.
//
// Spec refs:
//   * docs/02-gdd/03-characters.md § Character bio strings.
//   * docs/06-tech-spec/03-save-system.md § stats payload schema.
//   * docs/handoffs/wave10-loc-keys-needed.md § new loc keys.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using Brave.Systems.Context;
using Brave.Systems.Localization;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>
    /// Plain DTO for a single stats-tab row. Keeps the formatter free of
    /// UIElements types so EditMode tests do not need a panel.
    /// </summary>
    public readonly struct LifetimeStatRow
    {
        public readonly string Label;
        public readonly string Value;

        public LifetimeStatRow(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    /// <summary>One character row in the roster tab.</summary>
    public readonly struct CharacterRosterRow
    {
        public readonly string Slug;
        public readonly string NameLocKey;
        public readonly string BioLocKey;
        public readonly string UnlockHintLocKey;
        public readonly bool IsUnlocked;
        public readonly int RunsCompleted;
        public readonly int BossesDefeated;
        public readonly int HighestWaveReached;

        public CharacterRosterRow(string slug, bool isUnlocked,
            int runsCompleted, int bossesDefeated, int highestWaveReached)
        {
            Slug = slug;
            NameLocKey = $"characters.{slug}.name";
            BioLocKey = $"characters.{slug}.bio";
            UnlockHintLocKey = $"characters.{slug}.unlock_hint";
            IsUnlocked = isUnlocked;
            RunsCompleted = runsCompleted;
            BossesDefeated = bossesDefeated;
            HighestWaveReached = highestWaveReached;
        }
    }

    /// <summary>
    /// Pure-C# render facade for the profile screen — no UIDocument required.
    /// Tab state is intentionally a simple enum so test assertions stay readable.
    /// </summary>
    public static class ProfileScreenLogic
    {
        public const string ActiveTabClass = "is-active";
        public const string LockedRowClass = "profile-char-row-locked";

        // Loc keys consumed by the static formatter — kept here so handoff to
        // loc-agent has one canonical list and tests can assert key stability.
        public const string LocTitle           = "profile.title";
        public const string LocTabStats        = "profile.tab_stats";
        public const string LocTabCharacters   = "profile.tab_characters";
        public const string LocTabAchievements = "profile.tab_achievements";
        public const string LocStatsHeading    = "profile.stats_heading";
        public const string LocStatKills       = "profile.stat_kills";
        public const string LocStatRuns        = "profile.stat_runs";
        public const string LocStatBestWave    = "profile.stat_best_wave";
        public const string LocStatBestTime    = "profile.stat_best_time";
        public const string LocStatBosses      = "profile.stat_bosses";
        public const string LocStatEvolutions  = "profile.stat_evolutions";
        public const string LocStatPlaytime    = "profile.stat_playtime";
        public const string LocCharsEmpty      = "profile.characters_empty";
        public const string LocAchHeading      = "profile.achievements_heading";
        public const string LocAchHint         = "profile.achievements_hint";
        public const string LocOpenAch         = "profile.open_achievements";
        public const string LocCharLocked      = "profile.character_locked";
        public const string LocCharRunsFmt     = "profile.character_runs";
        public const string LocCharBossesFmt   = "profile.character_bosses";
        public const string LocCharBestWaveFmt = "profile.character_best_wave";

        /// <summary>Tab identifiers — string-typed so tests can assert without UXML.</summary>
        public enum Tab
        {
            Stats,
            Characters,
            Achievements,
        }

        /// <summary>Convert a saved-stats playtime double (seconds) to "Xh YYm".</summary>
        public static string FormatPlaytime(double seconds)
        {
            if (seconds < 0) seconds = 0;
            var totalMinutes = (long)(seconds / SecondsPerMinute);
            var hours = totalMinutes / MinutesPerHour;
            var minutes = (int)(totalMinutes % MinutesPerHour);
            return hours.ToString(CultureInfo.InvariantCulture)
                + "h "
                + minutes.ToString("D2", CultureInfo.InvariantCulture)
                + "m";
        }

        /// <summary>Format a best-run-time float (seconds) as "M:SS".</summary>
        public static string FormatRunTime(float seconds)
        {
            if (seconds < 0) seconds = 0;
            var total = (int)seconds;
            var minutes = total / (int)SecondsPerMinute;
            var secs = total % (int)SecondsPerMinute;
            return minutes.ToString(CultureInfo.InvariantCulture)
                + ":"
                + secs.ToString("D2", CultureInfo.InvariantCulture);
        }

        /// <summary>Format an integer scalar with invariant culture (no thousands separator).</summary>
        public static string FormatLong(long value) =>
            value.ToString(CultureInfo.InvariantCulture);

        public static string FormatInt(int value) =>
            value.ToString(CultureInfo.InvariantCulture);

        // ---- Stats tab render ----

        /// <summary>
        /// Build the list of (label, value) rows from a SaveData.StatsSection.
        /// Pure — no UIElements. Tests assert on the resulting strings.
        /// </summary>
        public static IReadOnlyList<LifetimeStatRow> BuildStatRows(
            SaveData.StatsSection stats, Func<string, string> translate)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (translate == null) translate = k => k;

            return new[]
            {
                new LifetimeStatRow(translate(LocStatKills),      FormatLong(stats.TotalKills)),
                new LifetimeStatRow(translate(LocStatRuns),       FormatLong(stats.TotalRuns)),
                new LifetimeStatRow(translate(LocStatBestWave),   FormatInt(stats.BestWaveReached)),
                new LifetimeStatRow(translate(LocStatBestTime),   FormatRunTime(stats.BestRunTimeSeconds)),
                new LifetimeStatRow(translate(LocStatBosses),     FormatLong(stats.BossesDefeated)),
                new LifetimeStatRow(translate(LocStatEvolutions), FormatLong(stats.EvolutionsTriggered)),
                new LifetimeStatRow(translate(LocStatPlaytime),   FormatPlaytime(stats.TotalPlaytimeSeconds)),
            };
        }

        /// <summary>
        /// Push the formatted stats into the named value labels under <paramref name="statsRoot"/>.
        /// Returns false if the root is null. Missing labels are tolerated (defensive against
        /// UXML drift).
        /// </summary>
        public static bool RenderStatValues(
            VisualElement? statsRoot,
            SaveData.StatsSection stats,
            Func<string, string> translate)
        {
            if (statsRoot == null) return false;

            var rows = BuildStatRows(stats, translate);
            // Mirror UXML element naming: lbl-stat-<key>-value.
            SetText(statsRoot, "lbl-stat-kills-value",      rows[0].Value);
            SetText(statsRoot, "lbl-stat-runs-value",       rows[1].Value);
            SetText(statsRoot, "lbl-stat-best-wave-value",  rows[2].Value);
            SetText(statsRoot, "lbl-stat-best-time-value",  rows[3].Value);
            SetText(statsRoot, "lbl-stat-bosses-value",     rows[4].Value);
            SetText(statsRoot, "lbl-stat-evolutions-value", rows[5].Value);
            SetText(statsRoot, "lbl-stat-playtime-value",   rows[6].Value);
            return true;
        }

        // ---- Characters tab render ----

        /// <summary>
        /// Read per-slug unlock + lifetime stats from the save data and build
        /// the renderable roster snapshot. Order is preserved from <paramref name="slugs"/>.
        /// </summary>
        public static IReadOnlyList<CharacterRosterRow> BuildRoster(
            IReadOnlyList<string> slugs,
            ICharacterUnlockService unlockService,
            IReadOnlyDictionary<string, CharacterProfile> profiles)
        {
            if (slugs == null) throw new ArgumentNullException(nameof(slugs));
            if (unlockService == null) throw new ArgumentNullException(nameof(unlockService));
            if (profiles == null) throw new ArgumentNullException(nameof(profiles));

            var result = new List<CharacterRosterRow>(capacity: slugs.Count);
            foreach (var slug in slugs)
            {
                if (string.IsNullOrEmpty(slug)) continue;
                var unlocked = unlockService.IsUnlocked(slug);
                var hasProfile = profiles.TryGetValue(slug, out var profile);
                result.Add(new CharacterRosterRow(
                    slug,
                    unlocked,
                    hasProfile ? profile!.RunsCompleted : 0,
                    hasProfile ? profile!.BossesDefeated : 0,
                    hasProfile ? profile!.HighestWaveReached : 0));
            }
            return result;
        }

        /// <summary>
        /// Clone the row template under <paramref name="listRoot"/> once per character.
        /// Locked rows get the <see cref="LockedRowClass"/> + their progress label shows
        /// the unlock-hint translation; unlocked rows show per-character lifetime stats.
        /// </summary>
        public static void RenderRoster(
            VisualElement listRoot,
            VisualElement? template,
            IReadOnlyList<CharacterRosterRow> roster,
            Func<string, string> translate)
        {
            if (listRoot == null) throw new ArgumentNullException(nameof(listRoot));
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (translate == null) translate = k => k;

            listRoot.Clear();
            foreach (var row in roster)
            {
                var card = BuildCharacterCard(template, row, translate);
                listRoot.Add(card);
            }
        }

        /// <summary>
        /// Construct a single character row. Public so EditMode tests can drive
        /// it without a template (template parameter may be null — we fall back
        /// to building a fresh row from scratch).
        /// </summary>
        public static VisualElement BuildCharacterCard(
            VisualElement? template, CharacterRosterRow row, Func<string, string> translate)
        {
            if (translate == null) translate = k => k;

            var card = new VisualElement { name = $"row-{row.Slug}" };
            card.AddToClassList("card");
            card.AddToClassList("card-row");
            card.AddToClassList("profile-char-row");
            if (!row.IsUnlocked) card.AddToClassList(LockedRowClass);

            var portrait = new VisualElement { name = $"portrait-{row.Slug}" };
            portrait.AddToClassList("profile-char-portrait");
            card.Add(portrait);

            var nameLabel = new Label(translate(row.NameLocKey)) { name = $"lbl-{row.Slug}-name" };
            nameLabel.AddToClassList("num");
            card.Add(nameLabel);

            var bioLabel = new Label(translate(row.BioLocKey)) { name = $"lbl-{row.Slug}-bio" };
            bioLabel.AddToClassList("body-sm");
            card.Add(bioLabel);

            var progressLabel = new Label(BuildProgressText(row, translate))
            {
                name = $"lbl-{row.Slug}-progress",
            };
            progressLabel.AddToClassList("micro");
            card.Add(progressLabel);

            return card;
        }

        /// <summary>Per-row progress copy. Unlocked rows show stats; locked rows show the hint.</summary>
        public static string BuildProgressText(CharacterRosterRow row, Func<string, string> translate)
        {
            if (translate == null) translate = k => k;

            if (!row.IsUnlocked)
            {
                // Prefer the unlock_hint key when present; fall back to a generic
                // "locked" label so we never render a blank cell.
                var hint = translate(row.UnlockHintLocKey);
                if (!string.IsNullOrEmpty(hint) && !hint.Equals(row.UnlockHintLocKey, StringComparison.Ordinal))
                {
                    return hint;
                }
                return translate(LocCharLocked);
            }

            // Unlocked: "Runs N · Bosses M · Best wave W".
            var runsFmt = translate(LocCharRunsFmt);
            var bossesFmt = translate(LocCharBossesFmt);
            var waveFmt = translate(LocCharBestWaveFmt);
            return $"{Substitute(runsFmt, row.RunsCompleted)}"
                + $" {Separator} {Substitute(bossesFmt, row.BossesDefeated)}"
                + $" {Separator} {Substitute(waveFmt, row.HighestWaveReached)}";
        }

        /// <summary>Toggle the `is-active` class on the tab buttons + display style on tab roots.</summary>
        public static void ApplyTabSelection(
            VisualElement root,
            Tab active,
            (string TabBtn, string PanelRoot, Tab Tab)[] map)
        {
            if (root == null) return;
            foreach (var (btnName, panelName, tab) in map)
            {
                var btn = root.Q<Button>(btnName);
                if (btn != null)
                {
                    if (tab == active) btn.AddToClassList(ActiveTabClass);
                    else btn.RemoveFromClassList(ActiveTabClass);
                }
                var panel = root.Q<VisualElement>(panelName);
                if (panel != null)
                {
                    panel.style.display = tab == active ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }

        // ---- helpers ----

        // Magic-number-free time conversion (CLAUDE.md principle 6).
        private const double SecondsPerMinute = 60.0;
        private const long MinutesPerHour = 60;
        private const string Separator = "·";

        private static void SetText(VisualElement root, string name, string text)
        {
            var lbl = root.Q<Label>(name);
            if (lbl != null) lbl.text = text;
        }

        /// <summary>Substitute `{count}` (the convention used by Wave-9 quest keys) with the integer.</summary>
        public static string Substitute(string template, int count)
        {
            if (string.IsNullOrEmpty(template)) return count.ToString(CultureInfo.InvariantCulture);
            return template.Replace("{count}", count.ToString(CultureInfo.InvariantCulture));
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class ProfileController : MonoBehaviour
    {
        // ---- Element names (must match Profile.uxml) ----
        public const string RootName = "profile-root";
        public const string StatsTabRoot = "profile-stats-tab";
        public const string CharactersTabRoot = "profile-characters-tab";
        public const string AchievementsTabRoot = "profile-achievements-tab";
        public const string CharacterListName = "profile-character-list";
        public const string TemplateName = "tpl-character-row";
        public const string CharsEmptyHint = "lbl-characters-empty";

        public const string TabStatsBtn = "tab-stats";
        public const string TabCharactersBtn = "tab-characters";
        public const string TabAchievementsBtn = "tab-achievements";

        public const string BackButton = "btn-back";
        public const string OpenAchievementsButton = "btn-open-achievements";

        public const string AchievementsScreenName = "AchievementsPanel";

        // The Profile screen is reachable from Home + Pause; we route back via UIEvents.GoHome.
        [Tooltip("Roster slugs (kebab-case). Order is preserved by the Characters tab grid.")]
        [SerializeField] private string[] _rosterSlugs = Array.Empty<string>();

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;

        private VisualElement _root = null!;
        private VisualElement _statsTab = null!;
        private VisualElement _characterListRoot = null!;
        private VisualElement? _rowTemplate;
        private Label? _emptyHint;

        private ISaveService? _save;
        private ICharacterUnlockService? _unlock;
        private ProfileScreenLogic.Tab _activeTab = ProfileScreenLogic.Tab.Stats;

        private (string TabBtn, string PanelRoot, ProfileScreenLogic.Tab Tab)[] _tabMap = null!;

        /// <summary>Test seam — inject services before OnEnable for EditMode tests.</summary>
        public void ConfigureForTests(
            ISaveService save,
            ICharacterUnlockService unlock,
            IReadOnlyList<string> roster)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            _unlock = unlock ?? throw new ArgumentNullException(nameof(unlock));
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            _rosterSlugs = new string[roster.Count];
            for (var i = 0; i < roster.Count; i++) _rosterSlugs[i] = roster[i];
        }

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
            _tabMap = new[]
            {
                (TabStatsBtn,        StatsTabRoot,        ProfileScreenLogic.Tab.Stats),
                (TabCharactersBtn,   CharactersTabRoot,   ProfileScreenLogic.Tab.Characters),
                (TabAchievementsBtn, AchievementsTabRoot, ProfileScreenLogic.Tab.Achievements),
            };
        }

        private void OnEnable()
        {
            _root = _doc.rootVisualElement;

            _statsTab = _root.Q<VisualElement>(StatsTabRoot)!;
            _characterListRoot = _root.Q<VisualElement>(CharacterListName)!;
            _rowTemplate = _root.Q<VisualElement>(TemplateName);
            _emptyHint = _root.Q<Label>(CharsEmptyHint);

            // Hide the embedded row template so we don't render an extra empty row.
            if (_rowTemplate != null) _rowTemplate.style.display = DisplayStyle.None;

            // Tab buttons.
            BindTabButton(TabStatsBtn,        ProfileScreenLogic.Tab.Stats);
            BindTabButton(TabCharactersBtn,   ProfileScreenLogic.Tab.Characters);
            BindTabButton(TabAchievementsBtn, ProfileScreenLogic.Tab.Achievements);

            // Back + Achievements-deeplink buttons.
            var back = _root.Q<Button>(BackButton);
            if (back != null) back.clicked += OnBackClicked;

            var openAch = _root.Q<Button>(OpenAchievementsButton);
            if (openAch != null) openAch.clicked += OnOpenAchievementsClicked;

            // Resolve services from the live context if the test hook didn't.
            EnsureServicesFromContext();

            _loc.ApplyToTree(_root);
            Refresh();
            ProfileScreenLogic.ApplyTabSelection(_root, _activeTab, _tabMap);
        }

        private void OnDisable()
        {
            // No persistent subscriptions — services live for the lifetime of the
            // controller; we re-pull on each OnEnable so stale captures never linger.
        }

        /// <summary>Re-read the save + roster and re-render both tabs.</summary>
        public void Refresh()
        {
            RenderStats();
            RenderRoster();
        }

        private void RenderStats()
        {
            if (_save == null) return;
            ProfileScreenLogic.RenderStatValues(_statsTab, _save.Data.Stats, Loc.T);
        }

        private void RenderRoster()
        {
            if (_save == null || _unlock == null) return;

            var roster = ProfileScreenLogic.BuildRoster(_rosterSlugs, _unlock, _save.Data.Characters);
            ProfileScreenLogic.RenderRoster(_characterListRoot, _rowTemplate, roster, Loc.T);

            if (_emptyHint != null)
            {
                _emptyHint.style.display = roster.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void BindTabButton(string buttonName, ProfileScreenLogic.Tab tab)
        {
            var btn = _root.Q<Button>(buttonName);
            if (btn == null) return;
            var captured = tab;
            btn.clicked += () => SelectTab(captured);
        }

        private void SelectTab(ProfileScreenLogic.Tab tab)
        {
            _activeTab = tab;
            ProfileScreenLogic.ApplyTabSelection(_root, _activeTab, _tabMap);
        }

        private void OnBackClicked() => UIEvents.RaiseGoHomeRequested();

        private void OnOpenAchievementsClicked() =>
            UIEvents.RaisePushScreen(AchievementsScreenName);

        private void EnsureServicesFromContext()
        {
            if (GameContextBootstrap.Context == null) return;
            if (_save == null
                && GameContextBootstrap.Context.TryGet<ISaveService>(out var saveSvc))
            {
                _save = saveSvc;
            }
            if (_unlock == null
                && GameContextBootstrap.Context.TryGet<ICharacterUnlockService>(out var unlockSvc))
            {
                _unlock = unlockSvc;
            }
        }
    }
}
