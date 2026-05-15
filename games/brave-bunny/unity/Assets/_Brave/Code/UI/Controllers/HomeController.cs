// Brave Bunny — UI / Controllers / HomeController
// Bound to: _Brave/UI/Documents/Home.uxml
// Wireframe spec: docs/05-wireframes/03-home-lobby.html
// User stories: US-09 home tour, US-31 next-thing, US-33 daily missions,
//               US-35 pass progress, US-36 always-play, US-37 0-input streak,
//               US-40 mailbox, US-60 hero-of-day.
//
// Aliases: this is the "MainMenu" screen of the production scene graph —
// Boot loads `MainMenu.unity` whose UIDocument carries `Home.uxml`. The
// controller exposes scene names as constants so callers/tests reference them
// without magic strings (CLAUDE.md principle 6).
//
// KPI (per wireframe): Home renders ≤ 200 ms after splash. Returning user →
// Run start ≤ 2 taps. Play button is always live (US-36) — no energy gate.

#nullable enable

using System;
using Brave.UI.Bindings;
using Brave.UI.Components;
using Brave.UI.Theming;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>Tiny abstraction over the Wallet for tests that don't want a real SaveService.</summary>
    public interface ICurrencyReader
    {
        long Get(CurrencyType type);
    }

    /// <summary>Production wallet reader — wraps the Progression service's wallet.</summary>
    public sealed class ProgressionWalletReader : ICurrencyReader
    {
        private readonly IProgressionService _prog;
        public ProgressionWalletReader(IProgressionService prog) => _prog = prog;
        public long Get(CurrencyType type) => _prog.Wallet.Get(type);
    }

    /// <summary>Pure-render facade for the Home/MainMenu currency strip + button routing.</summary>
    public static class HomeMenuLogic
    {
        public const string LoadoutSceneName = "Loadout";
        public const string SettingsScreenName = "Settings";
        public const string CharactersScreenName = "CharacterSelect";
        public const string ShopScreenName = "Shop";

        public static int ClampInt(long v) => v > int.MaxValue ? int.MaxValue : (int)v;

        /// <summary>
        /// Push the currency numerics into the two pills. Returns false if either
        /// pill is null (defensive — Home.uxml drift would otherwise NPE silently).
        /// </summary>
        public static bool RenderCurrency(CurrencyPill? gold, CurrencyPill? gems, ICurrencyReader wallet)
        {
            if (gold == null || gems == null || wallet == null) return false;
            gold.SetAmount(ClampInt(wallet.Get(CurrencyType.Carrots)));
            gems.SetAmount(ClampInt(wallet.Get(CurrencyType.Stars)));
            return true;
        }

        /// <summary>
        /// Play-button click → navigate to the Loadout scene + fire intent.
        /// Returns the scene the loader was asked to load (for assertion).
        /// </summary>
        public static string OnPlayClicked(ISceneLoader loader)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));
            UIEvents.RaiseStartRunRequested();
            loader.Load(LoadoutSceneName);
            return LoadoutSceneName;
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class HomeController : MonoBehaviour
    {
        [SerializeField] private string _heroesScreenName = HomeMenuLogic.CharactersScreenName;
        [SerializeField] private string _settingsScreenName = HomeMenuLogic.SettingsScreenName;
        [SerializeField] private string _shopScreenName = HomeMenuLogic.ShopScreenName;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private CurrencyPill _goldPill = null!;
        private CurrencyPill _gemPill = null!;
        private ISceneLoader _sceneLoader = new SceneManagerLoader();

        /// <summary>Test hook — inject a fake scene loader.</summary>
        public void SetSceneLoader(ISceneLoader loader) => _sceneLoader = loader
            ?? throw new ArgumentNullException(nameof(loader));

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
            HomeMenuLogic.RenderCurrency(_goldPill, _gemPill, new ProgressionWalletReader(prog));
        }

        private void OnPlayClicked()
        {
            // Loadout-first per Wave 7B routing — let the player pick a hero
            // before kicking off the run. The integration agent wires the
            // Loadout scene's Play button to actually start the run.
            HomeMenuLogic.OnPlayClicked(_sceneLoader);
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
