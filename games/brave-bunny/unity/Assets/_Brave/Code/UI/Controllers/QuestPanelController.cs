// Brave Bunny — UI / Controllers / QuestPanelController (Wave 9 LiveOps).
// Bound to: _Brave/UI/Documents/QuestPanel.uxml
//
// 3 daily quest cards with progress bar + claim button. Pulls today's roster
// from IQuestService.GetTodaysQuests() and binds them in document order. The
// pure-C# QuestPanelLogic is exposed so EditMode tests can drive bind/claim
// without spinning up a UIDocument.

#nullable enable

using System;
using System.Globalization;
using Brave.Systems.Context;
using Brave.Systems.Localization;
using Brave.Systems.LiveOps;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>One row of pre-resolved UI handles for a single quest card.</summary>
    public sealed class QuestCardBinding
    {
        public Label? Title;
        public Label? Reward;
        public Label? Progress;
        public VisualElement? BarFill;
        public Button? Claim;
    }

    /// <summary>Pure logic for rendering quests into bindings — testable without UIDocument.</summary>
    public static class QuestPanelLogic
    {
        public const string ClaimLocKey = "quest.claim";
        public const string ProgressLocKey = "quest.progress";

        public static string FormatProgress(int current, int required) =>
            current.ToString(CultureInfo.InvariantCulture)
            + " / "
            + required.ToString(CultureInfo.InvariantCulture);

        public static string FormatReward(QuestReward reward) =>
            "+" + reward.Amount.ToString(CultureInfo.InvariantCulture);

        public static float FillPercent(Quest q) => q.Progress01 * 100f;

        /// <summary>
        /// Push quest state into the given binding. Returns false if the binding
        /// has no usable handles. Null quest renders an empty card.
        /// </summary>
        public static bool Render(QuestCardBinding binding, Quest? quest, Func<string, string> tr)
        {
            if (binding == null) return false;
            if (tr == null) tr = k => k;
            if (quest == null)
            {
                if (binding.Title != null) binding.Title.text = string.Empty;
                if (binding.Reward != null) binding.Reward.text = string.Empty;
                if (binding.Progress != null) binding.Progress.text = string.Empty;
                if (binding.BarFill != null) binding.BarFill.style.width = new StyleLength(new Length(0f, LengthUnit.Percent));
                if (binding.Claim != null) binding.Claim.SetEnabled(false);
                return true;
            }
            if (binding.Title != null) binding.Title.text = tr(quest.TitleLocKey);
            if (binding.Reward != null) binding.Reward.text = FormatReward(quest.Reward);
            if (binding.Progress != null) binding.Progress.text = FormatProgress(quest.CurrentCount, quest.RequiredCount);
            if (binding.BarFill != null)
                binding.BarFill.style.width = new StyleLength(new Length(FillPercent(quest), LengthUnit.Percent));
            if (binding.Claim != null)
            {
                binding.Claim.text = tr(ClaimLocKey);
                binding.Claim.SetEnabled(quest.IsClaimable);
            }
            return true;
        }
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class QuestPanelController : MonoBehaviour
    {
        public const int CardCount = QuestPool.QuestsPerDay;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private QuestCardBinding[] _bindings = new QuestCardBinding[CardCount];
        private Action[] _claimHandlers = new Action[CardCount];
        private IQuestService? _service;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            for (var i = 0; i < CardCount; i++)
            {
                _bindings[i] = BindCard(root, i);
                var idx = i; // capture
                _claimHandlers[i] = () => OnClaimClicked(idx);
                if (_bindings[i].Claim != null) _bindings[i].Claim!.clicked += _claimHandlers[i];
            }

            if (GameContextBootstrap.Context != null &&
                GameContextBootstrap.Context.TryGet<IQuestService>(out var svc))
            {
                _service = svc;
                _service.QuestUpdated += OnQuestUpdated;
            }

            var close = root.Q<Button>("btn-close");
            if (close != null) close.clicked += OnCloseClicked;

            _loc.ApplyToTree(root);
            Refresh();
        }

        private void OnDisable()
        {
            if (_service != null)
            {
                _service.QuestUpdated -= OnQuestUpdated;
                _service = null;
            }
            for (var i = 0; i < CardCount; i++)
            {
                if (_bindings[i]?.Claim != null && _claimHandlers[i] != null)
                {
                    _bindings[i].Claim!.clicked -= _claimHandlers[i];
                }
            }
        }

        private static QuestCardBinding BindCard(VisualElement root, int index)
        {
            var binding = new QuestCardBinding
            {
                Title = root.Q<Label>($"lbl-quest-{index}-title"),
                Reward = root.Q<Label>($"lbl-quest-{index}-reward"),
                Progress = root.Q<Label>($"lbl-quest-{index}-progress"),
                BarFill = root.Q<VisualElement>($"quest-{index}-bar-fill"),
                Claim = root.Q<Button>($"btn-quest-{index}-claim"),
            };
            return binding;
        }

        private void OnQuestUpdated(Quest _) => Refresh();

        private void Refresh()
        {
            if (_service == null) return;
            var today = _service.GetTodaysQuests();
            for (var i = 0; i < CardCount; i++)
            {
                var quest = i < today.Length ? today[i] : null;
                QuestPanelLogic.Render(_bindings[i], quest, Loc.T);
            }
        }

        private void OnClaimClicked(int index)
        {
            if (_service == null) return;
            var today = _service.GetTodaysQuests();
            if (index < 0 || index >= today.Length) return;
            var q = today[index];
            if (q == null) return;
            _service.Claim(q.Id);
            // QuestUpdated event → Refresh().
        }

        // Quest panel sits on the Home screen — closing routes back home.
        private void OnCloseClicked() => UIEvents.RaiseGoHomeRequested();

        // ----- Test seam -----
        /// <summary>EditMode tests inject the service directly + manually drive Refresh().</summary>
        public void SetService(IQuestService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }
    }
}
