// Brave Bunny — UI / Controllers / LevelUpDraftController
// Bound to: _Brave/UI/Documents/LevelUpDraft.uxml
// Wireframe spec: docs/05-wireframes/06-levelup-draft.html
// User stories: US-14 < 2-second pick, US-15 banish (one per run), US-23
//               delta clarity (current → next bold), US-25 evolution callout,
//               US-29 reroll (1 free, 2nd ad-gated).
//
// Critical fit: iPhone SE 3 (375 × 667). Modal max-height 640. 3 cards stacked
// vertically, each ≥ 88 pt — verified at design time in the wireframe.
//
// Wiring: gameplay-engineer's UpgradePicker passes a `DraftOffer[3]` and a
// callback. The controller does NOT know about WeaponDefinition etc.; it
// only renders presentation data the picker prepares for it.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class LevelUpDraftController : MonoBehaviour
    {
        /// <summary>One offer card — prepared by UpgradePicker.</summary>
        public readonly struct DraftOffer
        {
            public readonly string IconText;       // "CC" / "★" / "+HP"
            public readonly string Title;          // "Carrot Cannon Lv 2"
            public readonly string DeltaBody;      // "Dmg 8 → 12 · Rate 1.2 → 1.5/s"
            public readonly bool   IsEvolution;    // adds rare ring
            public DraftOffer(string iconText, string title, string deltaBody, bool isEvolution)
            {
                IconText = iconText; Title = title; DeltaBody = deltaBody; IsEvolution = isEvolution;
            }
        }

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement _root = null!;
        private Label _flavorLabel = null!;
        private Label _levelBadge = null!;
        private readonly List<CardSlot> _cardSlots = new(capacity: 3);
        private bool _banishUsedThisRun;
        private int _rerollsUsedThisRun;

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
        }

        private void OnEnable()
        {
            _root = _doc.rootVisualElement;
            _flavorLabel = _root.Q<Label>("lbl-flavor")!;
            _levelBadge = _root.Q<Label>("lbl-level-badge")!;

            _cardSlots.Clear();
            _cardSlots.Add(new CardSlot(_root, "card-a", "lbl-card-a-icon", "lbl-card-a-title", "lbl-card-a-delta", "btn-take-a"));
            _cardSlots.Add(new CardSlot(_root, "card-b", "lbl-card-b-icon", "lbl-card-b-title", "lbl-card-b-delta", "btn-take-b"));
            _cardSlots.Add(new CardSlot(_root, "card-c", "lbl-card-c-icon", "lbl-card-c-title", "lbl-card-c-delta", "btn-take-c"));

            for (int i = 0; i < _cardSlots.Count; i++)
            {
                int index = i;
                _cardSlots[i].TakeButton.clicked += () => OnTake(index);
                _cardSlots[i].Card.RegisterCallback<ClickEvent>(_ => OnTake(index));
            }

            _root.Q<Button>("btn-banish")!.clicked += OnBanishClicked;
            _root.Q<Button>("btn-reroll")!.clicked += OnRerollClicked;

            _loc.ApplyToTree(_root);
            Hide();
        }

        /// <summary>Show the modal with three concrete offers + level number.</summary>
        public void Show(int newLevel, DraftOffer[] offers)
        {
            if (offers == null || offers.Length != 3)
                throw new ArgumentException("LevelUpDraftController requires exactly 3 offers.", nameof(offers));

            _levelBadge.text = $"Lv {newLevel}";
            _flavorLabel.text = _loc.Loc("LEVEL_UP_FLAVOR_PLUCKY");

            for (int i = 0; i < 3; i++)
            {
                _cardSlots[i].Render(offers[i]);
            }
            _root.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f; // pause-on-draft per 02-gdd/03-run-loop.md
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }

        private void OnTake(int cardIndex)
        {
            UIEvents.RaiseUpgradePicked(cardIndex);
            Hide();
        }

        private void OnBanishClicked()
        {
            if (_banishUsedThisRun) return; // US-15: one banish per run
            _banishUsedThisRun = true;
            UIEvents.RaiseBanishRequested();
            // Picker re-emits a new triple → Show() called again by caller.
            Hide();
        }

        private void OnRerollClicked()
        {
            // US-29: first reroll free, second ad-gated. The Ads service
            // arbitrates the ad surface; UI only fires intent + tracks count.
            _rerollsUsedThisRun++;
            UIEvents.RaiseRerollRequested();
            Hide();
        }

        /// <summary>Per-card UI cache.</summary>
        private sealed class CardSlot
        {
            public readonly VisualElement Card;
            public readonly Label Icon;
            public readonly Label Title;
            public readonly Label Delta;
            public readonly Button TakeButton;

            public CardSlot(VisualElement root, string cardName, string iconName, string titleName, string deltaName, string btnName)
            {
                Card = root.Q<VisualElement>(cardName)!;
                Icon = root.Q<Label>(iconName)!;
                Title = root.Q<Label>(titleName)!;
                Delta = root.Q<Label>(deltaName)!;
                TakeButton = root.Q<Button>(btnName)!;
            }

            public void Render(DraftOffer offer)
            {
                Icon.text = offer.IconText;
                Title.text = offer.Title;
                Delta.text = offer.DeltaBody;
                if (offer.IsEvolution)
                {
                    Card.AddToClassList("card-rare");
                }
                else
                {
                    Card.RemoveFromClassList("card-rare");
                }
            }
        }
    }
}
