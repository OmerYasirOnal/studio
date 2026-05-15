// Brave Bunny — UI / Controllers / ShopController
// Bound to: _Brave/UI/Documents/Shop.uxml
// Wireframe spec: docs/05-wireframes/13-shop.html
// User stories: US-50..US-54.
//
// Responsibilities:
//   * Render IapCatalogConfig product rows under four tabs.
//   * Route Buy-button clicks through IapPurchaseFlow (mocked locally in
//     Editor; real Apple/Google IAP plugin lands post-soft-launch).
//   * Show a transient toast on success / failure using the localized
//     `shop.purchase_success` and `shop.purchase_failed` keys.
//   * Re-render rows when a one-time SKU is bought (button → "Owned").
//
// Currency strip + Restore-purchases follow the same patterns as HomeController
// / SettingsController so the rest of the navigation layer keeps working.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;
using Brave.Systems.Iap;
using Brave.Systems.Localization;
using Brave.Systems.Progression;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>Pure-render facade for the Shop tab routing + product binding.</summary>
    public static class ShopMenuLogic
    {
        // USS class names — kept here so tests / restyles don't need to grep UXML.
        public const string ActiveTabClass = "is-active";
        public const string OwnedButtonClass = "btn-owned";

        // Tab IDs (also doubles as the suffix on `tab-<id>` button names in UXML).
        public static readonly IReadOnlyList<(string Id, IapCategory Category, string LocKey)> Tabs = new[]
        {
            ("currency",   IapCategory.Currency,   "shop.tab_currency"),
            ("characters", IapCategory.Characters, "shop.tab_characters"),
            ("specials",   IapCategory.Specials,   "shop.tab_specials"),
            ("battlepass", IapCategory.BattlePass, "shop.tab_battle_pass"),
        };
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class ShopController : MonoBehaviour
    {
        [Tooltip("Designer-authored product catalog. Required at runtime — the controller is a no-op without it.")]
        [SerializeField] private IapCatalogConfig? _catalog;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement _productList = null!;
        private VisualElement _rowTemplate = null!;
        private Label _toast = null!;
        private Label _emptyHint = null!;

        private IapPurchaseFlow? _purchaseFlow;
        private IapCategory _activeCategory = IapCategory.Currency;

        /// <summary>Test hook — inject the catalog at runtime.</summary>
        public void SetCatalog(IapCatalogConfig catalog) =>
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

        /// <summary>Test hook — inject the purchase flow.</summary>
        public void SetPurchaseFlow(IapPurchaseFlow flow) =>
            _purchaseFlow = flow ?? throw new ArgumentNullException(nameof(flow));

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;

            _productList = root.Q<VisualElement>("shop-product-list")!;
            _rowTemplate = root.Q<VisualElement>("tpl-product-row")!;
            _toast = root.Q<Label>("lbl-toast")!;
            _emptyHint = root.Q<Label>("lbl-empty")!;

            // Hide the embedded template — we clone it per row.
            _rowTemplate.style.display = DisplayStyle.None;

            // Tab buttons.
            foreach (var (id, category, _) in ShopMenuLogic.Tabs)
            {
                var btn = root.Q<Button>($"tab-{id}");
                if (btn == null) continue;
                var capturedCategory = category;
                btn.clicked += () => SelectTab(capturedCategory);
            }

            root.Q<Button>("btn-back")!.clicked += () => UIEvents.RaisePushScreen("Home");
            root.Q<Button>("btn-restore-purchases")!.clicked += OnRestoreClicked;

            // Resolve services from the live context if the test hook didn't.
            EnsureFlowFromContext();
            RefreshCurrencyStrip();

            // Localization sweep (does nothing if Loc.T returns the key — keys
            // still need entries in _Brave/Localization/{lang}.json per Wave 9 handoff).
            _loc.ApplyToTree(root);

            // Initial render.
            SelectTab(_activeCategory);
        }

        private void OnDisable()
        {
            if (_purchaseFlow != null) _purchaseFlow.PurchaseCompleted -= OnPurchaseCompleted;
        }

        // ---------- tab + row rendering ----------

        private void SelectTab(IapCategory category)
        {
            _activeCategory = category;
            var root = _doc.rootVisualElement;

            // Toggle active-state class on tab buttons.
            foreach (var (id, cat, _) in ShopMenuLogic.Tabs)
            {
                var btn = root.Q<Button>($"tab-{id}");
                if (btn == null) continue;
                if (cat == category) btn.AddToClassList(ShopMenuLogic.ActiveTabClass);
                else btn.RemoveFromClassList(ShopMenuLogic.ActiveTabClass);
            }

            RenderProductList();
        }

        private void RenderProductList()
        {
            _productList.Clear();

            if (_catalog == null)
            {
                _emptyHint.style.display = DisplayStyle.Flex;
                return;
            }

            var any = false;
            foreach (var product in _catalog.ForCategory(_activeCategory))
            {
                _productList.Add(BuildRow(product));
                any = true;
            }
            _emptyHint.style.display = any ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private VisualElement BuildRow(IapProduct product)
        {
            // Clone the template by hand — UXML <Template> requires asset
            // round-trips; cloning keeps the controller editor-friendly.
            var row = new VisualElement();
            row.AddToClassList("card");
            row.AddToClassList("card-row");
            row.AddToClassList("shop-row");

            var icon = new VisualElement();
            icon.AddToClassList("shop-row-icon");
            row.Add(icon);

            var col = new VisualElement();
            col.AddToClassList("col");
            col.style.flexGrow = 1;
            col.style.paddingLeft = 12;
            var name = new Label(product.DisplayName) { name = "lbl-name" };
            name.AddToClassList("num");
            var desc = new Label(BuildDescription(product)) { name = "lbl-desc" };
            desc.AddToClassList("body-sm");
            col.Add(name);
            col.Add(desc);
            row.Add(col);

            var buy = new Button { name = "btn-buy" };
            buy.AddToClassList("btn");
            buy.AddToClassList("btn-primary");
            buy.AddToClassList("shop-buy");

            var owned = product.IsOneTime && _purchaseFlow != null && _purchaseFlow.HasPurchased(product.Sku);
            if (owned)
            {
                buy.text = Loc.T("shop.owned");
                buy.AddToClassList(ShopMenuLogic.OwnedButtonClass);
                buy.SetEnabled(false);
            }
            else
            {
                buy.text = product.PriceDisplay();
                buy.clicked += () => OnBuyClicked(product);
            }
            row.Add(buy);
            return row;
        }

        private static string BuildDescription(IapProduct product)
        {
            // Simple summary — designers can override per-SKU later via SO field.
            if (product.StarsGranted > 0) return $"+{product.StarsGranted}★";
            if (product.Grants != null && product.Grants.Length > 0) return string.Join(" · ", product.Grants);
            return product.Sku;
        }

        // ---------- purchase plumbing ----------

        private void OnBuyClicked(IapProduct product)
        {
            EnsureFlowFromContext();
            if (_purchaseFlow == null)
            {
                ShowToast(Loc.T("shop.purchase_failed"));
                return;
            }
            _purchaseFlow.TryPurchase(product.Sku);
        }

        private void OnRestoreClicked()
        {
            if (GameContextBootstrap.Context == null) return;
            if (!GameContextBootstrap.Context.TryGet<IIapService>(out var iap)) return;
            iap.RestorePurchases(_ => ShowToast(Loc.T("shop.purchase_success")));
        }

        private void OnPurchaseCompleted(IapPurchaseOutcome outcome)
        {
            ShowToast(outcome.Success ? Loc.T("shop.purchase_success") : Loc.T("shop.purchase_failed"));
            RefreshCurrencyStrip();
            RenderProductList(); // re-render so one-time SKUs flip to "Owned"
        }

        private void EnsureFlowFromContext()
        {
            if (_purchaseFlow != null)
            {
                _purchaseFlow.PurchaseCompleted -= OnPurchaseCompleted;
                _purchaseFlow.PurchaseCompleted += OnPurchaseCompleted;
                return;
            }

            var ctx = GameContextBootstrap.Context;
            if (ctx == null) return;
            if (!ctx.TryGet<IIapService>(out var iap)) return;
            if (!ctx.TryGet<Brave.Systems.Save.ISaveService>(out var save)) return;
            if (!ctx.TryGet<ICurrencyService>(out var currency)) return;

            _purchaseFlow = new IapPurchaseFlow(iap, save, new ProductionPurchaseGrants(currency, save), _catalog);
            _purchaseFlow.PurchaseCompleted += OnPurchaseCompleted;
        }

        private void RefreshCurrencyStrip()
        {
            var root = _doc.rootVisualElement;
            var gold = root.Q<Label>("lbl-gold-amount");
            var gem = root.Q<Label>("lbl-gem-amount");
            if (gold == null || gem == null) return;

            if (GameContextBootstrap.Context == null) return;
            if (!GameContextBootstrap.Context.TryGet<IProgressionService>(out var prog)) return;

            gold.text = HomeMenuLogic.ClampInt(prog.Wallet.Get(CurrencyType.Carrots)).ToString();
            gem.text = HomeMenuLogic.ClampInt(prog.Wallet.Get(CurrencyType.Stars)).ToString();
        }

        private void ShowToast(string message)
        {
            if (_toast == null) return;
            _toast.text = message ?? string.Empty;
            _toast.style.opacity = 1f;
        }
    }
}
