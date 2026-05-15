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
// Wiring (Wave 7B):
//   * Subscribes to LevelUpChannel — Gameplay's LevelUpController raises it
//     when XP threshold crossed. On event: pause run, build 3-card draft from
//     the passive/weapon catalogue, Show modal.
//   * Pick → applies (raises UIEvents.UpgradePicked), hides, restores timescale.
//
// Draft selection is delegated to LevelUpDraftBuilder (pure-C# helper) so the
// 3-card invariant can be exercised in EditMode tests without a MonoBehaviour.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>One upgrade option — either a passive or a weapon-level bump.</summary>
    public readonly struct UpgradeOption
    {
        public readonly string IconText;
        public readonly string Title;
        public readonly string DeltaBody;
        public readonly bool IsEvolution;

        public UpgradeOption(string iconText, string title, string deltaBody, bool isEvolution)
        {
            IconText = iconText;
            Title = title;
            DeltaBody = deltaBody;
            IsEvolution = isEvolution;
        }
    }

    /// <summary>
    /// Pure-C# draft builder. Picks N unique offers from a flat option pool with
    /// a deterministic seed (for tests + replay-buffer parity). When the pool is
    /// smaller than N the builder pads with the last option to honour the
    /// "always 3 cards" UI invariant.
    /// </summary>
    public static class LevelUpDraftBuilder
    {
        public const int DraftSize = 3;

        /// <summary>Build a draft of exactly 3 offers from <paramref name="pool"/>.</summary>
        public static UpgradeOption[] Build(IReadOnlyList<UpgradeOption> pool, int seed)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            var draft = new UpgradeOption[DraftSize];

            if (pool.Count == 0)
            {
                // Degenerate case — emit placeholder offers so the modal still has 3 cards.
                var placeholder = new UpgradeOption("?", "—", "(no offers)", false);
                for (int i = 0; i < DraftSize; i++) draft[i] = placeholder;
                return draft;
            }

            var rng = new System.Random(seed);
            // Reservoir-style: pick DraftSize distinct indices when pool is large enough,
            // otherwise sample with replacement (rare — pool size < 3 only at very low levels).
            var taken = new HashSet<int>();
            if (pool.Count >= DraftSize)
            {
                while (taken.Count < DraftSize)
                {
                    taken.Add(rng.Next(pool.Count));
                }
                int idx = 0;
                foreach (var i in taken) draft[idx++] = pool[i];
            }
            else
            {
                for (int i = 0; i < DraftSize; i++) draft[i] = pool[rng.Next(pool.Count)];
            }
            return draft;
        }
    }

    /// <summary>
    /// Optional pool source — the Run scene wires a concrete provider that pulls
    /// from the live passives catalogue + character weapon upgrades. EditMode tests
    /// inject a fake provider.
    /// </summary>
    public interface IUpgradePoolProvider
    {
        IReadOnlyList<UpgradeOption> BuildPool();
    }

    [RequireComponent(typeof(UIDocument))]
    public sealed class LevelUpDraftController : MonoBehaviour
    {
        /// <summary>One offer card — prepared by UpgradePicker (legacy alias).</summary>
        public readonly struct DraftOffer
        {
            public readonly string IconText;
            public readonly string Title;
            public readonly string DeltaBody;
            public readonly bool   IsEvolution;
            public DraftOffer(string iconText, string title, string deltaBody, bool isEvolution)
            {
                IconText = iconText; Title = title; DeltaBody = deltaBody; IsEvolution = isEvolution;
            }
            public UpgradeOption ToUpgrade() => new(IconText, Title, DeltaBody, IsEvolution);
        }

        [Header("Gameplay event channels (optional — wired by Run scene)")]
        [SerializeField] private LevelUpChannel? _levelUpChannel;

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement _root = null!;
        private Label _flavorLabel = null!;
        private Label _levelBadge = null!;
        private readonly List<CardSlot> _cardSlots = new(capacity: 3);
        private bool _banishUsedThisRun;
        private int _rerollsUsedThisRun;
        private float _priorTimeScale = 1f;
        private IUpgradePoolProvider? _poolProvider;

        // Default pool: 6 hard-coded passives mirroring data/balance/passives.json.
        // The Run scene replaces this via SetPoolProvider when the live catalogue is wired.
        private static readonly UpgradeOption[] DefaultPool =
        {
            new("MC", "Magnet Charm Lv +1", "Pickup radius +20%", false),
            new("HC", "Hearty Charm Lv +1", "Max HP +15%", false),
            new("RC", "Mossy Charm Lv +1", "HP regen +0.5/s", false),
            new("DC", "Damage Charm Lv +1", "Damage +10%", false),
            new("SC", "Swift Charm Lv +1", "Move speed +8%", false),
            new("CC", "Cooldown Charm Lv +1", "Cooldown -8%", false),
        };

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

            if (_levelUpChannel != null) _levelUpChannel.Subscribe(OnLevelUp);

            _loc.ApplyToTree(_root);
            Hide();
        }

        private void OnDisable()
        {
            if (_levelUpChannel != null) _levelUpChannel.Unsubscribe(OnLevelUp);
            // Restore timescale defensively if torn down while paused.
            if (Time.timeScale == 0f) Time.timeScale = _priorTimeScale;
        }

        /// <summary>Test/integration hook — Run scene wires the live passive catalogue here.</summary>
        public void SetPoolProvider(IUpgradePoolProvider provider)
        {
            _poolProvider = provider;
        }

        /// <summary>Subscriber for <see cref="LevelUpChannel"/>. Builds 3 offers and shows.</summary>
        public void OnLevelUp(LevelUpEvent evt)
        {
            var pool = _poolProvider?.BuildPool() ?? DefaultPool;
            // Seed with new level so the draft is deterministic per level (replay parity).
            var draft = LevelUpDraftBuilder.Build(pool, seed: evt.newLevel);
            Show(evt.newLevel, draft);
        }

        /// <summary>Show the modal with three concrete offers + level number.</summary>
        public void Show(int newLevel, UpgradeOption[] offers)
        {
            if (offers == null || offers.Length != LevelUpDraftBuilder.DraftSize)
                throw new ArgumentException(
                    $"LevelUpDraftController requires exactly {LevelUpDraftBuilder.DraftSize} offers.",
                    nameof(offers));

            _levelBadge.text = $"Lv {newLevel}";
            _flavorLabel.text = _loc.Loc("LEVEL_UP_FLAVOR_PLUCKY");

            for (int i = 0; i < _cardSlots.Count; i++)
            {
                _cardSlots[i].Render(offers[i]);
            }
            _root.style.display = DisplayStyle.Flex;

            _priorTimeScale = Time.timeScale;
            Time.timeScale = 0f; // pause-on-draft per 02-gdd/03-run-loop.md
        }

        /// <summary>Legacy overload for callers using <see cref="DraftOffer"/>.</summary>
        public void Show(int newLevel, DraftOffer[] offers)
        {
            if (offers == null || offers.Length != LevelUpDraftBuilder.DraftSize)
                throw new ArgumentException(
                    $"LevelUpDraftController requires exactly {LevelUpDraftBuilder.DraftSize} offers.",
                    nameof(offers));
            var upgrades = new UpgradeOption[offers.Length];
            for (int i = 0; i < offers.Length; i++) upgrades[i] = offers[i].ToUpgrade();
            Show(newLevel, upgrades);
        }

        public void Hide()
        {
            _root.style.display = DisplayStyle.None;
            // Restore the timescale we captured on Show() — never blindly snap to 1.
            Time.timeScale = _priorTimeScale;
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

            public void Render(UpgradeOption offer)
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
