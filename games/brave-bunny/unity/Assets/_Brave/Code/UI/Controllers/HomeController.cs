// Brave Bunny — UI / Controllers / HomeController
// Bound to: _Brave/UI/Documents/Home.uxml
// Wireframe spec: docs/05-wireframes/03-home-lobby.html
// User stories: US-09 home tour, US-31 next-thing, US-33 daily missions,
//               US-35 pass progress, US-36 always-play, US-37 0-input streak,
//               US-40 mailbox, US-60 hero-of-day.
//
// KPI (per wireframe): Home renders ≤ 200 ms after splash. Returning user →
// Run start ≤ 2 taps. Play button is always live (US-36) — no energy gate.

#nullable enable

using Brave.UI.Bindings;
using Brave.UI.Components;
using Brave.UI.Theming;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class HomeController : MonoBehaviour
    {
        [SerializeField] private string _heroesScreenName = "CharacterSelect";
        [SerializeField] private string _settingsScreenName = "Settings";
        [SerializeField] private string _shopScreenName = "Shop";

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private CurrencyPill _goldPill = null!;
        private CurrencyPill _gemPill = null!;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            // ── Currency pills ────────────────────────────────────────────
            _goldPill = CurrencyPill.Bind(root, "pill-gold", "lbl-gold-amount");
            _gemPill = CurrencyPill.Bind(root, "pill-gem", "lbl-gem-amount");
            RefreshCurrency();

            // ── Buttons ───────────────────────────────────────────────────
            root.Q<Button>("btn-play")!.clicked += OnPlayClicked;
            root.Q<Button>("btn-claim-daily")!.clicked += OnClaimDailyClicked;
            root.Q<Button>("btn-mailbox")!.clicked += OnMailboxClicked;

            // ── Tabs ──────────────────────────────────────────────────────
            root.Q<Button>("tab-home")!.clicked += () => { /* already here */ };
            root.Q<Button>("tab-heroes")!.clicked += () => PushScreen(_heroesScreenName);
            root.Q<Button>("tab-pass")!.clicked += () => PushScreen("BattlePass");
            root.Q<Button>("tab-shop")!.clicked += () => PushScreen(_shopScreenName);
            root.Q<Button>("tab-settings")!.clicked += () => PushScreen(_settingsScreenName);

            // ── Localization sweep ────────────────────────────────────────
            _loc.ApplyToTree(root);
        }

        private void RefreshCurrency()
        {
            if (GameContextBootstrap.Context == null) return;
            if (!GameContextBootstrap.Context.TryGet<IProgressionService>(out var prog)) return;
            // 08-economy.md three-currency model: Carrots = soft (carrots),
            // Stars = premium (gems), SoulShards = run-banked (rune currency).
            // Wallet.Get returns long; clamp to int for the chip display — the
            // pill won't show > ~2B carrots and the chip is decorative.
            _goldPill.SetAmount(ClampInt(prog.Wallet.Get(CurrencyType.Carrots)));
            _gemPill.SetAmount(ClampInt(prog.Wallet.Get(CurrencyType.Stars)));
        }

        private static int ClampInt(long v) => v > int.MaxValue ? int.MaxValue : (int)v;

        private void OnPlayClicked()
        {
            // RunService is owned by gameplay-engineer; UI only fires intent.
            // Wired by an event so we don't depend on a concrete type here.
            UIEvents.RaiseStartRunRequested();
        }

        private void OnClaimDailyClicked()
        {
            if (GameContextBootstrap.Context == null) return;
            if (!GameContextBootstrap.Context.TryGet<IDailyStreakService>(out var streak)) return;
            streak.Claim(System.DateTime.UtcNow);
            RefreshCurrency();
        }

        private void OnMailboxClicked() => UIEvents.RaiseOpenMailbox();

        private void PushScreen(string screenName) => UIEvents.RaisePushScreen(screenName);
    }
}
