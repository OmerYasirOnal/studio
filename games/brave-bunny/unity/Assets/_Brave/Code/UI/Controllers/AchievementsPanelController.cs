// Brave Bunny — UI / Controllers / AchievementsPanelController (Wave 10).
// Bound to: _Brave/UI/Documents/AchievementsPanel.uxml
//
// Renders all 20 achievements as a scrollable list. Rows are built at runtime
// (no UXML template needed — keeps the asset reference graph trivial). Each
// row: icon | name + description | progress bar | claim button.
//
// The pure-C# AchievementRowLogic is exposed so EditMode tests can drive
// row bind/state-transition without spinning up a UIDocument.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using Brave.Systems.Achievements;
using Brave.Systems.Context;
using Brave.Systems.Localization;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>Pre-resolved handles for a single achievement row.</summary>
    public sealed class AchievementRowBinding
    {
        public VisualElement? Root;
        public VisualElement? Icon;
        public Label? Name;
        public Label? Description;
        public Label? Progress;
        public VisualElement? BarFill;
        public Button? Claim;
    }

    /// <summary>Pure logic for rendering an achievement row — testable.</summary>
    public static class AchievementRowLogic
    {
        public const string ClaimLocKey = "achievement.panel.claim";
        public const string ClaimedLocKey = "achievement.panel.claimed";
        public const string LockedLocKey = "achievement.panel.locked";
        public const string LockedClass = "is-locked";
        public const string UnlockedClass = "is-unlocked";
        public const string ClaimedClass = "is-claimed";

        public static string FormatProgress(int current, int required) =>
            current.ToString(CultureInfo.InvariantCulture)
            + " / "
            + required.ToString(CultureInfo.InvariantCulture);

        public static float FillPercent(Achievement a) =>
            a.RequiredCount <= 0 ? 0f : Mathf.Clamp01((float)a.CurrentCount / a.RequiredCount) * 100f;

        /// <summary>
        /// Push the achievement state into the row binding. Returns false if
        /// either side is null. Localizer <paramref name="tr"/> resolves loc keys.
        /// </summary>
        public static bool Render(AchievementRowBinding binding, Achievement? a, Func<string, string> tr)
        {
            if (binding == null) return false;
            if (tr == null) tr = k => k;
            if (a == null)
            {
                if (binding.Name != null) binding.Name.text = string.Empty;
                if (binding.Progress != null) binding.Progress.text = string.Empty;
                if (binding.BarFill != null)
                    binding.BarFill.style.width = new StyleLength(new Length(0f, LengthUnit.Percent));
                if (binding.Claim != null) binding.Claim.SetEnabled(false);
                return true;
            }
            if (binding.Name != null) binding.Name.text = tr(a.Def.EffectiveDisplayKey);
            if (binding.Description != null) binding.Description.text = tr(a.Def.EffectiveDescriptionKey);
            if (binding.Progress != null) binding.Progress.text = FormatProgress(a.CurrentCount, a.RequiredCount);
            if (binding.BarFill != null)
                binding.BarFill.style.width = new StyleLength(new Length(FillPercent(a), LengthUnit.Percent));
            if (binding.Claim != null)
            {
                if (a.Claimed) binding.Claim.text = tr(ClaimedLocKey);
                else if (a.Unlocked) binding.Claim.text = tr(ClaimLocKey);
                else binding.Claim.text = tr(LockedLocKey);
                binding.Claim.SetEnabled(a.Unlocked && !a.Claimed);
            }
            if (binding.Root != null)
            {
                ToggleClass(binding.Root, LockedClass, !a.Unlocked);
                ToggleClass(binding.Root, UnlockedClass, a.Unlocked && !a.Claimed);
                ToggleClass(binding.Root, ClaimedClass, a.Claimed);
            }
            return true;
        }

        private static void ToggleClass(VisualElement el, string cls, bool on)
        {
            if (on)
            {
                if (!el.ClassListContains(cls)) el.AddToClassList(cls);
            }
            else
            {
                if (el.ClassListContains(cls)) el.RemoveFromClassList(cls);
            }
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class AchievementsPanelController : MonoBehaviour
    {
        public const string ListName = "achievements-list";
        public const string CloseButtonName = "btn-close";

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement? _list;
        private readonly List<AchievementRowBinding> _bindings = new();
        private readonly List<Action> _claimHandlers = new();
        private IAchievementService? _service;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            _list = root.Q<VisualElement>(ListName);

            if (GameContextBootstrap.Context != null &&
                GameContextBootstrap.Context.TryGet<IAchievementService>(out var svc))
            {
                _service = svc;
                _service.AchievementChanged += OnAchievementChanged;
            }

            var close = root.Q<Button>(CloseButtonName);
            if (close != null) close.clicked += OnCloseClicked;

            _loc.ApplyToTree(root);
            BuildRows();
            RefreshAll();
        }

        private void OnDisable()
        {
            if (_service != null)
            {
                _service.AchievementChanged -= OnAchievementChanged;
                _service = null;
            }
            // Disconnect click handlers built in BuildRows.
            for (var i = 0; i < _bindings.Count && i < _claimHandlers.Count; i++)
            {
                if (_bindings[i].Claim != null && _claimHandlers[i] != null)
                    _bindings[i].Claim!.clicked -= _claimHandlers[i];
            }
            _bindings.Clear();
            _claimHandlers.Clear();
        }

        private void BuildRows()
        {
            if (_list == null || _service == null) return;
            _list.Clear();
            _bindings.Clear();
            _claimHandlers.Clear();
            foreach (var a in _service.All)
            {
                var binding = CreateRow(a);
                _bindings.Add(binding);
                var capturedId = a.Id;
                Action handler = () => OnClaimClicked(capturedId);
                _claimHandlers.Add(handler);
                if (binding.Claim != null) binding.Claim.clicked += handler;
                _list.Add(binding.Root!);
            }
        }

        private static AchievementRowBinding CreateRow(Achievement a)
        {
            // Build a row programmatically — keeps the UXML template lean.
            var row = new VisualElement { name = $"row-{a.Id}" };
            row.AddToClassList("card");
            row.AddToClassList("achievement-row");

            var icon = new VisualElement { name = $"icon-{a.Id}" };
            icon.AddToClassList("achievement-icon");

            var col = new VisualElement();
            col.AddToClassList("achievement-col");

            var name = new Label { name = $"lbl-{a.Id}-name" };
            name.AddToClassList("h2");

            var description = new Label { name = $"lbl-{a.Id}-desc" };
            description.AddToClassList("body-sm");

            var barRow = new VisualElement();
            barRow.AddToClassList("row-spread");

            var bar = new VisualElement { name = $"bar-{a.Id}" };
            bar.AddToClassList("bar");
            bar.AddToClassList("bar-pass");

            var fill = new VisualElement { name = $"bar-{a.Id}-fill" };
            fill.AddToClassList("bar-fill");
            fill.style.width = new StyleLength(new Length(0f, LengthUnit.Percent));
            bar.Add(fill);

            var progress = new Label { name = $"lbl-{a.Id}-progress" };
            progress.AddToClassList("body-sm");

            var claim = new Button { name = $"btn-{a.Id}-claim", text = "Claim" };
            claim.AddToClassList("btn");
            claim.AddToClassList("btn-primary");

            barRow.Add(progress);
            barRow.Add(claim);

            col.Add(name);
            col.Add(description);
            col.Add(bar);
            col.Add(barRow);

            row.Add(icon);
            row.Add(col);

            return new AchievementRowBinding
            {
                Root = row,
                Icon = icon,
                Name = name,
                Description = description,
                Progress = progress,
                BarFill = fill,
                Claim = claim,
            };
        }

        private void OnAchievementChanged(Achievement a) => RefreshOne(a);

        private void RefreshAll()
        {
            if (_service == null) return;
            var all = _service.All;
            for (var i = 0; i < _bindings.Count && i < all.Count; i++)
            {
                AchievementRowLogic.Render(_bindings[i], all[i], Loc.T);
            }
        }

        private void RefreshOne(Achievement a)
        {
            if (_service == null) return;
            var all = _service.All;
            for (var i = 0; i < all.Count && i < _bindings.Count; i++)
            {
                if (ReferenceEquals(all[i], a))
                {
                    AchievementRowLogic.Render(_bindings[i], a, Loc.T);
                    return;
                }
            }
        }

        private void OnClaimClicked(string id)
        {
            if (_service == null) return;
            _service.TryClaim(id);
            // AchievementChanged event → RefreshOne()
        }

        private void OnCloseClicked() => UIEvents.RaiseGoHomeRequested();

        // ----- Test seam -----
        public void SetService(IAchievementService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }
    }
}
