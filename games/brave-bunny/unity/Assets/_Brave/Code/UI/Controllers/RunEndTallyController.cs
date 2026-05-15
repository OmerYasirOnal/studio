// Brave Bunny — UI / Controllers / RunEndTallyController
// Bound to: _Brave/UI/Documents/RunEndTally.uxml
// Wireframe spec: docs/05-wireframes/08-run-end-tally.html
// User stories: US-18 banked first, US-30 2-tap replay, US-44 rewarded opt-in,
//               US-47 larger decline + decline pre-focused, US-55 share,
//               US-62 milestone share prompt.
//
// Wiring contract (Wave 7B):
//   * Subscribes to RunEndedChannel — RunController raises it via End()
//     after CurrentRunEndReport is populated. We read the payload's report
//     directly (no IRunRuntimeState polling needed at this point — the run is
//     done and CurrentRunEndReport is stable).
//   * Outcome mapping: Win → loc-key "runend.you_won" (banner title),
//                      everything else → "runend.you_died".
//   * Character used → loc-key "characters.<slug>.name".
//   * Weapons used → loc-keys "weapons.<slug>.name" (or the slug fallback if
//     the key is missing — LocalizationProvider returns the key as identity).
//
// Render is exposed as a pure static method (RunEndTallyRenderer.Render) so
// EditMode tests can build a fake RunEndReport and assert label text without
// instantiating UIDocument.

#nullable enable

using System.Text;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>Bag of resolved tally <see cref="VisualElement"/> refs.</summary>
    public sealed class TallyElements
    {
        public Label Title = null!;
        public Label RunSummary = null!;
        public Label RunDetail = null!;
        public Label Carrots = null!;
        public Label PassXp = null!;
        public Label HeroXp = null!;
        public Label Missions = null!;
        public Label AdPreview = null!;
        public Button DeclineAd = null!;
        public Button WatchAd = null!;
    }

    /// <summary>Pure render layer — testable without UIDocument.</summary>
    public static class RunEndTallyRenderer
    {
        public const string OutcomeWinKey = "runend.you_won";
        public const string OutcomeLoseKey = "runend.you_died";

        // Legacy keys preserved for backwards-compat with prior tests.
        public const string LegacyWinTitleKey = "RUN_END_WIN_GENERIC";
        public const string LegacyLoseTitleKey = "RUN_END_LOSE_GENERIC";

        /// <summary>Map outcome to the canonical title loc-key.</summary>
        public static string OutcomeLocKey(RunOutcome outcome) => outcome switch
        {
            RunOutcome.Win => OutcomeWinKey,
            _              => OutcomeLoseKey,
        };

        /// <summary>
        /// Render a populated <see cref="RunEndReport"/> into <paramref name="el"/>.
        /// Loc keys are resolved through <paramref name="loc"/>; if a key is absent
        /// the provider returns the key itself, which is safe for QA visibility.
        /// </summary>
        public static void Render(RunEndReport report, TallyElements el, LocalizationProvider loc)
        {
            if (report == null) return;
            if (el == null) return;
            if (loc == null) return;

            el.Title.text = loc.Loc(OutcomeLocKey(report.outcome));

            var m = Mathf.FloorToInt(report.runDurationSeconds / 60f);
            var s = Mathf.FloorToInt(report.runDurationSeconds % 60f);
            // Lv X · mm:ss
            el.RunSummary.text = $"Lv {report.finalLevel} · {m:D2}:{s:D2}";

            // {characterName} · {kills} rascals · waves {wavesCleared}
            var characterName = string.IsNullOrEmpty(report.characterId)
                ? string.Empty
                : loc.Loc($"characters.{report.characterId}.name");
            el.RunDetail.text = BuildRunDetail(characterName, report.totalKills, report.wavesCleared, report.weaponIdsUsed, loc);

            el.Carrots.text = $"+ {report.goldGained}";
            el.PassXp.text = $"+ {report.passXpEarned}";
            el.HeroXp.text = $"+ {report.xpGained}";
            el.Missions.text = "0 of 0";

            int doubled = report.goldGained * 2;
            el.AdPreview.text = $"Preview: +{report.goldGained} carrots (total {doubled})";
        }

        private static string BuildRunDetail(string characterName, int kills, int waves,
            string[] weaponIds, LocalizationProvider loc)
        {
            var sb = new StringBuilder(64);
            if (!string.IsNullOrEmpty(characterName))
            {
                sb.Append(characterName);
                sb.Append(" · ");
            }
            sb.Append(kills);
            sb.Append(" rascals · waves ");
            sb.Append(waves);
            if (weaponIds != null && weaponIds.Length > 0)
            {
                sb.Append(" · ");
                for (int i = 0; i < weaponIds.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(loc.Loc($"weapons.{weaponIds[i]}.name"));
                }
            }
            return sb.ToString();
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class RunEndTallyController : MonoBehaviour
    {
        /// <summary>Payload assembled by run-service on run-end (legacy compatibility wrapper).</summary>
        public readonly struct TallyPayload
        {
            public readonly bool   Victory;
            public readonly int    LevelReached;
            public readonly float  RunSeconds;
            public readonly string BiomeDisplayName;
            public readonly int    EnemiesKilled;
            public readonly int    CarrotsEarned;
            public readonly int    PassXp;
            public readonly int    HeroXp;
            public readonly int    MissionsCompleted;
            public readonly int    MissionsTotal;
            public TallyPayload(bool victory, int levelReached, float runSeconds, string biomeDisplayName,
                int enemiesKilled, int carrotsEarned, int passXp, int heroXp,
                int missionsCompleted, int missionsTotal)
            {
                Victory = victory; LevelReached = levelReached; RunSeconds = runSeconds;
                BiomeDisplayName = biomeDisplayName; EnemiesKilled = enemiesKilled;
                CarrotsEarned = carrotsEarned; PassXp = passXp; HeroXp = heroXp;
                MissionsCompleted = missionsCompleted; MissionsTotal = missionsTotal;
            }
        }

        [Header("Gameplay event channels (wired by Boot or the Tally scene)")]
        [SerializeField] private RunEndedChannel? _runEndedChannel;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private readonly TallyElements _el = new();

        /// <summary>Last report observed via the channel — exposed for tests + analytics.</summary>
        public RunEndReport? LastReport { get; private set; }

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            _el.Title = root.Q<Label>("lbl-title")!;
            _el.RunSummary = root.Q<Label>("lbl-run-summary")!;
            _el.RunDetail = root.Q<Label>("lbl-run-detail")!;
            _el.Carrots = root.Q<Label>("lbl-banked-carrots")!;
            _el.PassXp = root.Q<Label>("lbl-banked-pass-xp")!;
            _el.HeroXp = root.Q<Label>("lbl-banked-hero-xp")!;
            _el.Missions = root.Q<Label>("lbl-mission-progress")!;
            _el.AdPreview = root.Q<Label>("lbl-ad-preview")!;
            _el.DeclineAd = root.Q<Button>("btn-decline-ad")!;
            _el.WatchAd = root.Q<Button>("btn-watch-ad")!;

            _el.DeclineAd.clicked += OnDeclineAdClicked;
            _el.WatchAd.clicked += OnWatchAdClicked;

            root.Q<Button>("btn-share")!.clicked += OnShareClicked;
            root.Q<Button>("btn-home")!.clicked += OnHomeClicked;
            root.Q<Button>("btn-retry")!.clicked += OnRetryClicked;

            if (_runEndedChannel != null) _runEndedChannel.Subscribe(OnRunEnded);

            _loc.ApplyToTree(root);

            // US-47: pre-focus the decline button so the friendly default wins
            // if the player taps through quickly.
            _el.DeclineAd.Focus();
        }

        private void OnDisable()
        {
            if (_runEndedChannel != null) _runEndedChannel.Unsubscribe(OnRunEnded);

            if (_el.DeclineAd != null) _el.DeclineAd.clicked -= OnDeclineAdClicked;
            if (_el.WatchAd != null) _el.WatchAd.clicked -= OnWatchAdClicked;
        }

        /// <summary>
        /// Subscriber for <see cref="RunEndedChannel"/>. Caches the report and renders.
        /// Exposed via the <c>internal</c> path so EditMode tests can call it without a
        /// real ScriptableObject channel asset.
        /// </summary>
        public void OnRunEnded(RunEndedEvent evt)
        {
            LastReport = evt.report;
            if (evt.report != null)
            {
                RunEndTallyRenderer.Render(evt.report, _el, _loc);
            }
        }

        /// <summary>Called by run-service after the run finishes (legacy compatibility).</summary>
        public void Render(TallyPayload p)
        {
            var report = new RunEndReport
            {
                outcome = p.Victory ? RunOutcome.Win : RunOutcome.Lose,
                finalLevel = p.LevelReached,
                runDurationSeconds = p.RunSeconds,
                totalKills = p.EnemiesKilled,
                goldGained = p.CarrotsEarned,
                passXpEarned = p.PassXp,
                xpGained = p.HeroXp,
                wavesCleared = 0,
            };
            RunEndTallyRenderer.Render(report, _el, _loc);

            // Legacy biome + mission text — wireframes still show these. Keep the
            // legacy keys live for the immediate-render-from-TallyPayload path so
            // older smoke tests aren't broken.
            _el.RunDetail.text = $"{p.BiomeDisplayName} · {p.EnemiesKilled} rascals sent packing";
            _el.Missions.text = $"{p.MissionsCompleted} of {p.MissionsTotal}";
        }

        private void OnDeclineAdClicked() => UIEvents.RaiseAdDoubleRewardsRequested(false);
        private void OnWatchAdClicked() => UIEvents.RaiseAdDoubleRewardsRequested(true);
        private void OnShareClicked() => UIEvents.RaiseShareRunRequested();
        private void OnHomeClicked() => UIEvents.RaiseGoHomeRequested();
        private void OnRetryClicked() => UIEvents.RaiseRetryRunRequested();
    }
}
