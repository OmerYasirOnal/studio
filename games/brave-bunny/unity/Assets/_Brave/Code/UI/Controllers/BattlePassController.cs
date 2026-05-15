// Brave Bunny — UI / Controllers / BattlePassController
// Bound to: _Brave/UI/Documents/BattlePass.uxml
// Wireframe spec: docs/05-wireframes/09-battle-pass.html
// Wave 9 LiveOps scaffold — pairs with Systems/LiveOps/BattlePassService.
//
// Wiring:
//   * Pulls IBattlePassService + BattlePassSeasonConfig from GameContext.
//   * Builds two horizontal tier-cell rails (free + premium) of length
//     BattlePassSeasonConfig.TierCount; highlights the current tier.
//   * Claim buttons call IBattlePassService.Claim(tier, isPremium); on success
//     the cell visually switches to "claimed" and the renderer refreshes the
//     summary band.
//   * Premium activation button toggles IBattlePassService.ActivatePremium —
//     no real-money flow yet (IAP deferred to post-soft-launch per task brief).
//
// Pure rendering lives in BattlePassRenderer below so EditMode tests can
// exercise the binding without instantiating UIDocument.

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.LiveOps;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>Pure render layer — testable without UIDocument.</summary>
    public static class BattlePassRenderer
    {
        // Loc keys — per task brief (handoffs/wave9-loc-keys-needed.md).
        public const string TitleLocKey = "battlepass.title";
        public const string ClaimLocKey = "battlepass.claim";
        public const string LockedLocKey = "battlepass.locked";
        public const string ClaimedLocKey = "battlepass.claimed";
        public const string TierLocKeyFormat = "battlepass.tier_{0}";

        // USS classes (defined in theme.uss / screens.uss — UI artist task).
        public const string TierCellClass = "bp-tier-cell";
        public const string TierCellCurrentClass = "bp-tier-current";
        public const string TierCellClaimedClass = "bp-tier-claimed";
        public const string TierCellLockedClass = "bp-tier-locked";
        public const string TierCellPremiumGatedClass = "bp-tier-premium-gated";
        public const string TierNumberClass = "bp-tier-number";

        /// <summary>Build (or rebuild) a single row of tier cells under <paramref name="row"/>.</summary>
        public static void RenderRow(
            VisualElement row,
            BattlePassSeasonConfig config,
            IBattlePassService svc,
            bool isPremiumRow,
            LocalizationProvider loc,
            Action<int, bool> onClaim)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (svc == null) throw new ArgumentNullException(nameof(svc));

            row.Clear();
            int current = svc.CurrentTier;

            for (int i = 1; i <= BattlePassSeasonConfig.TierCount; i++)
            {
                int tier = i; // closure capture
                var cell = new Button { name = $"bp-cell-{(isPremiumRow ? "p" : "f")}-{tier}" };
                cell.AddToClassList(TierCellClass);

                bool claimed = svc.IsTierClaimed(tier, isPremiumRow);
                bool reachable = tier <= current;
                bool premiumGated = isPremiumRow && !svc.IsPremiumActive;

                if (tier == current) cell.AddToClassList(TierCellCurrentClass);
                if (claimed) cell.AddToClassList(TierCellClaimedClass);
                if (!reachable) cell.AddToClassList(TierCellLockedClass);
                if (premiumGated) cell.AddToClassList(TierCellPremiumGatedClass);

                var reward = isPremiumRow
                    ? config.PremiumRewardAtTier(tier)
                    : config.FreeRewardAtTier(tier);

                cell.text = BuildCellLabel(loc, tier, reward, claimed, reachable, premiumGated);

                if (reachable && !claimed && !premiumGated)
                {
                    cell.clicked += () => onClaim(tier, isPremiumRow);
                }
                else
                {
                    cell.SetEnabled(false);
                }

                row.Add(cell);
            }
        }

        /// <summary>Render the tier-number rail between the two rows (1..30).</summary>
        public static void RenderTierRail(VisualElement rail, int currentTier, LocalizationProvider loc)
        {
            if (rail == null) return;
            rail.Clear();
            for (int i = 1; i <= BattlePassSeasonConfig.TierCount; i++)
            {
                var lbl = new Label { name = $"bp-tier-num-{i}", text = i.ToString() };
                lbl.AddToClassList(TierNumberClass);
                if (i == currentTier) lbl.AddToClassList(TierCellCurrentClass);
                rail.Add(lbl);
            }
        }

        /// <summary>Update the top summary band — tier ordinal, raw XP, progress fraction.</summary>
        public static void RenderSummary(
            Label tierLabel, Label xpLabel, VisualElement progressFill,
            IBattlePassService svc, BattlePassSeasonConfig config)
        {
            if (svc == null || config == null) return;
            int tier = svc.CurrentTier;
            int xp = svc.CurrentXp;
            if (tierLabel != null) tierLabel.text = $"Tier {tier} / {BattlePassSeasonConfig.TierCount}";
            if (xpLabel != null) xpLabel.text = $"{xp} XP";

            if (progressFill == null) return;
            float frac = ComputeProgressFraction(xp, tier, config);
            progressFill.style.width = new StyleLength(
                new Length(Mathf.Clamp01(frac) * 100f, LengthUnit.Percent));
        }

        /// <summary>
        /// Fraction of progress between the current tier's start XP and the next tier's
        /// threshold. 1.0 at max tier. Pure for unit testing.
        /// </summary>
        public static float ComputeProgressFraction(int xp, int tier, BattlePassSeasonConfig config)
        {
            if (config?.tierXpThresholds == null || config.tierXpThresholds.Length == 0) return 0f;
            int max = config.tierXpThresholds.Length;
            if (tier >= max) return 1f;
            int floor = tier <= 0 ? 0 : config.tierXpThresholds[tier - 1];
            int ceiling = config.tierXpThresholds[tier]; // tier index uses [tier] = next threshold
            int span = ceiling - floor;
            if (span <= 0) return 0f;
            return (float)(xp - floor) / span;
        }

        private static string BuildCellLabel(LocalizationProvider loc, int tier,
            BattlePassReward? reward, bool claimed, bool reachable, bool premiumGated)
        {
            if (loc == null)
            {
                // No loc binding — fall back to a developer-readable label.
                if (claimed) return $"T{tier}: claimed";
                if (premiumGated) return $"T{tier}: premium";
                if (!reachable) return $"T{tier}: locked";
                return reward != null ? $"T{tier}: {reward.amount}x {reward.currencyType}" : $"T{tier}";
            }
            string prefix = loc.Loc(string.Format(TierLocKeyFormat, tier));
            if (claimed) return $"{prefix} — {loc.Loc(ClaimedLocKey)}";
            if (premiumGated) return $"{prefix} — {loc.Loc(LockedLocKey)}";
            if (!reachable) return $"{prefix} — {loc.Loc(LockedLocKey)}";
            return $"{prefix} — {loc.Loc(ClaimLocKey)}";
        }
    }

    /// <summary>MonoBehaviour shell — wires UI Toolkit to the renderer + service.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BattlePassController : MonoBehaviour
    {
        // ---- Element names (must match BattlePass.uxml) ----
        public const string TierListFreeName = "tier-list-free";
        public const string TierListPremiumName = "tier-list-premium";
        public const string TierRailName = "bp-tier-rail";
        public const string CurrentTierLabel = "lbl-current-tier";
        public const string CurrentXpLabel = "lbl-current-xp";
        public const string ProgressFillName = "bp-progress-fill";
        public const string PremiumStatusLabel = "lbl-premium-status";
        public const string ActivatePremiumButton = "btn-activate-premium";
        public const string BackButtonName = "btn-back";

        [Tooltip("Season SO. Optional — falls back to the SO registered against GameContext if null.")]
        [SerializeField] private BattlePassSeasonConfig? _seasonConfig;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement _rowFree = null!;
        private VisualElement _rowPremium = null!;
        private VisualElement _tierRail = null!;
        private Label _tierLabel = null!;
        private Label _xpLabel = null!;
        private VisualElement _progressFill = null!;
        private Label _premiumStatus = null!;
        private Button _btnActivatePremium = null!;
        private IBattlePassService? _svc;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            if (root == null) return;

            _rowFree = root.Q<VisualElement>(TierListFreeName)!;
            _rowPremium = root.Q<VisualElement>(TierListPremiumName)!;
            _tierRail = root.Q<VisualElement>(TierRailName)!;
            _tierLabel = root.Q<Label>(CurrentTierLabel)!;
            _xpLabel = root.Q<Label>(CurrentXpLabel)!;
            _progressFill = root.Q<VisualElement>(ProgressFillName)!;
            _premiumStatus = root.Q<Label>(PremiumStatusLabel)!;
            _btnActivatePremium = root.Q<Button>(ActivatePremiumButton)!;

            _svc = ResolveService();
            if (_btnActivatePremium != null) _btnActivatePremium.clicked += OnActivatePremiumClicked;

            Refresh();
            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            if (_btnActivatePremium != null) _btnActivatePremium.clicked -= OnActivatePremiumClicked;
        }

        /// <summary>Rebuild the whole panel — called after every claim / activation.</summary>
        public void Refresh()
        {
            var config = _seasonConfig;
            if (config == null || _svc == null) return;
            BattlePassRenderer.RenderRow(_rowFree, config, _svc, isPremiumRow: false, _loc, OnClaim);
            BattlePassRenderer.RenderRow(_rowPremium, config, _svc, isPremiumRow: true, _loc, OnClaim);
            BattlePassRenderer.RenderTierRail(_tierRail, _svc.CurrentTier, _loc);
            BattlePassRenderer.RenderSummary(_tierLabel, _xpLabel, _progressFill, _svc, config);
            if (_premiumStatus != null)
            {
                _premiumStatus.text = _svc.IsPremiumActive
                    ? _loc.Loc("battlepass.premium_active")
                    : _loc.Loc("battlepass.premium_locked");
            }
            if (_btnActivatePremium != null) _btnActivatePremium.SetEnabled(!_svc.IsPremiumActive);
        }

        private void OnClaim(int tier, bool isPremium)
        {
            if (_svc == null) return;
            var reward = _svc.Claim(tier, isPremium);
            if (reward == null) return; // gating / already-claimed; UI no-op
            // Currency dispense is the BattlePassService caller's responsibility — left as a
            // TODO until balance-engineer wires the CurrencyService.Add hook (Wave 9 polish).
            // TODO(brave-bunny#wave9): dispatch reward.currencyType + reward.amount via CurrencyService.
            Refresh();
        }

        private void OnActivatePremiumClicked()
        {
            _svc?.ActivatePremium();
            Refresh();
        }

        /// <summary>
        /// Resolve <see cref="IBattlePassService"/> from GameContext. Returns null until the
        /// service is registered — controllers must Refresh() once it is.
        /// </summary>
        private IBattlePassService? ResolveService()
        {
            if (GameContextBootstrap.Context == null) return null;
            return GameContextBootstrap.Context.TryGet<IBattlePassService>(out var svc) ? svc : null;
        }
    }
}
