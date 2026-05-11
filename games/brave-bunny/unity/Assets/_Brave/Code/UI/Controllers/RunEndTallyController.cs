// Brave Bunny — UI / Controllers / RunEndTallyController
// Bound to: _Brave/UI/Documents/RunEndTally.uxml
// Wireframe spec: docs/05-wireframes/08-run-end-tally.html
// User stories: US-18 banked first, US-30 2-tap replay, US-44 rewarded opt-in,
//               US-47 larger decline + decline pre-focused, US-55 share,
//               US-62 milestone share prompt.
//
// Tally summary is presented by the caller (run-service) — this controller
// just renders the payload. Bank-first ordering is hard-coded by UXML layout.

#nullable enable

using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class RunEndTallyController : MonoBehaviour
    {
        /// <summary>Payload assembled by run-service on run-end.</summary>
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

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;

        private Label _title = null!;
        private Label _runSummary = null!;
        private Label _runDetail = null!;
        private Label _carrots = null!;
        private Label _passXp = null!;
        private Label _heroXp = null!;
        private Label _missions = null!;
        private Label _adPreview = null!;
        private Button _declineAd = null!;
        private Button _watchAd = null!;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            _title = root.Q<Label>("lbl-title")!;
            _runSummary = root.Q<Label>("lbl-run-summary")!;
            _runDetail = root.Q<Label>("lbl-run-detail")!;
            _carrots = root.Q<Label>("lbl-banked-carrots")!;
            _passXp = root.Q<Label>("lbl-banked-pass-xp")!;
            _heroXp = root.Q<Label>("lbl-banked-hero-xp")!;
            _missions = root.Q<Label>("lbl-mission-progress")!;
            _adPreview = root.Q<Label>("lbl-ad-preview")!;
            _declineAd = root.Q<Button>("btn-decline-ad")!;
            _watchAd = root.Q<Button>("btn-watch-ad")!;

            _declineAd.clicked += () => OnAdChoice(false);
            _watchAd.clicked += () => OnAdChoice(true);

            root.Q<Button>("btn-share")!.clicked += () => UIEvents.RaiseShareRunRequested();
            root.Q<Button>("btn-home")!.clicked += () => UIEvents.RaiseGoHomeRequested();
            root.Q<Button>("btn-retry")!.clicked += () => UIEvents.RaiseRetryRunRequested();

            _loc.ApplyToTree(root);

            // US-47: pre-focus the decline button so the friendly default wins
            // if the player taps through quickly.
            _declineAd.Focus();
        }

        /// <summary>Called by run-service after the run finishes.</summary>
        public void Render(TallyPayload p)
        {
            _title.text = string.Format(
                _loc.Loc(p.Victory ? "RUN_END_WIN_GENERIC" : "RUN_END_LOSE_GENERIC")
                    .Replace("{GOLD}", p.CarrotsEarned.ToString()),
                p.CarrotsEarned);

            var m = Mathf.FloorToInt(p.RunSeconds / 60f);
            var s = Mathf.FloorToInt(p.RunSeconds % 60f);
            _runSummary.text = $"Lv {p.LevelReached} · {m:D2}:{s:D2}";
            _runDetail.text = $"{p.BiomeDisplayName} · {p.EnemiesKilled} rascals sent packing";

            _carrots.text = $"+ {p.CarrotsEarned}";
            _passXp.text = $"+ {p.PassXp}";
            _heroXp.text = $"+ {p.HeroXp}";
            _missions.text = $"{p.MissionsCompleted} of {p.MissionsTotal} ✓";

            _adPreview.text = $"Preview: +{p.CarrotsEarned} carrots (total {p.CarrotsEarned * 2})";
        }

        private void OnAdChoice(bool accept)
        {
            UIEvents.RaiseAdDoubleRewardsRequested(accept);
        }
    }
}
