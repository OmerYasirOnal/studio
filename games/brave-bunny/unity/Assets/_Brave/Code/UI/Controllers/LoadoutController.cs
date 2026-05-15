// Brave Bunny — UI / Controllers / LoadoutController
// Bound to: _Brave/UI/Documents/Loadout.uxml
// Wireframe spec: docs/05-wireframes/04-loadout.html
// User stories: US-04 pick hero, US-05 starter visible, US-08 locked w/ hint.
//
// Wiring:
//   * Queries Brave.Systems.Progression.ICharacterUnlockService for unlocked roster.
//   * The roster catalogue (every slug + unlock_hint loc key) is supplied at OnEnable
//     from a SerializeField list — keeps this controller free of CharacterDefinition
//     so the UI assembly doesn't depend on art-bible-driven SO assets.
//   * Renders one card per slug: unlocked → tappable + selectable; locked → greyed
//     out with the "characters.<slug>.unlock_hint" loc-key.
//   * Play button persists choice to LoadoutSelection then raises StartRunRequested.
//
// Render is exposed as a pure static method (LoadoutRenderer.Render) so EditMode
// tests can exercise the binding without instantiating UIDocument.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;
using Brave.Systems.Progression;
using Brave.UI.Bindings;
using Brave.UI.Theming;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Brave.UI.Controllers
{
    /// <summary>One entry in the displayed character roster — character-definition snapshot.</summary>
    public readonly struct LoadoutRosterEntry
    {
        public readonly string Slug;
        public readonly string NameLocKey;
        public readonly string BioLocKey;
        public readonly string UnlockHintLocKey;

        public LoadoutRosterEntry(string slug, string nameLocKey, string bioLocKey, string unlockHintLocKey)
        {
            Slug = slug;
            NameLocKey = nameLocKey;
            BioLocKey = bioLocKey;
            UnlockHintLocKey = unlockHintLocKey;
        }

        /// <summary>Canonical builder: derives all loc-keys from the slug convention.</summary>
        public static LoadoutRosterEntry FromSlug(string slug) => new(
            slug,
            $"characters.{slug}.name",
            $"characters.{slug}.bio",
            $"characters.{slug}.unlock_hint");
    }

    /// <summary>Pure render layer — testable without UIDocument.</summary>
    public static class LoadoutRenderer
    {
        public const string CardClass = "loadout-card";
        public const string CardLockedClass = "loadout-card-locked";
        public const string CardSelectedClass = "loadout-card-selected";
        public const string CardLockedSuffix = "-locked";

        /// <summary>
        /// Rebuild the roster list under <paramref name="cardList"/>.
        /// Each entry is rendered as a tappable button; locked entries are greyed
        /// and not clickable. <paramref name="onTap"/> is called with the slug on a
        /// successful (unlocked) tap.
        /// </summary>
        public static void RenderRoster(
            VisualElement cardList,
            IReadOnlyList<LoadoutRosterEntry> roster,
            IReadOnlyCollection<string> unlocked,
            string? selectedSlug,
            LocalizationProvider loc,
            Action<string> onTap)
        {
            if (cardList == null) throw new ArgumentNullException(nameof(cardList));
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (unlocked == null) throw new ArgumentNullException(nameof(unlocked));
            if (loc == null) throw new ArgumentNullException(nameof(loc));
            if (onTap == null) throw new ArgumentNullException(nameof(onTap));

            cardList.Clear();
            foreach (var entry in roster)
            {
                bool isUnlocked = unlocked.Contains(entry.Slug);
                bool isSelected = isUnlocked && string.Equals(entry.Slug, selectedSlug, StringComparison.Ordinal);

                var card = new Button { name = $"card-{entry.Slug}" };
                card.AddToClassList(CardClass);
                if (!isUnlocked) card.AddToClassList(CardLockedClass);
                if (isSelected) card.AddToClassList(CardSelectedClass);

                var nameLabel = new Label(loc.Loc(entry.NameLocKey)) { name = $"lbl-{entry.Slug}-name" };
                nameLabel.AddToClassList("num");
                card.Add(nameLabel);

                if (!isUnlocked)
                {
                    var hint = new Label(loc.Loc(entry.UnlockHintLocKey))
                    {
                        name = $"lbl-{entry.Slug}-hint",
                    };
                    hint.AddToClassList("micro");
                    card.Add(hint);
                }

                if (isUnlocked)
                {
                    var slug = entry.Slug;
                    card.clicked += () => onTap(slug);
                }

                cardList.Add(card);
            }
        }

        /// <summary>Apply the selection chrome to the currently-picked card.</summary>
        public static void ApplySelection(VisualElement cardList, string? selectedSlug)
        {
            if (cardList == null) return;
            foreach (var child in cardList.Children())
            {
                child.RemoveFromClassList(CardSelectedClass);
            }
            if (string.IsNullOrEmpty(selectedSlug)) return;
            var sel = cardList.Q<Button>($"card-{selectedSlug}");
            sel?.AddToClassList(CardSelectedClass);
        }

        /// <summary>
        /// Choose the slug to auto-select when the screen opens. Honours the
        /// player's previous pick if still unlocked; otherwise the first
        /// unlocked entry in the roster (deterministic registry order).
        /// </summary>
        public static string? AutoSelect(
            IReadOnlyList<LoadoutRosterEntry> roster,
            IReadOnlyCollection<string> unlocked,
            string? previousSelection)
        {
            if (!string.IsNullOrEmpty(previousSelection) && unlocked.Contains(previousSelection!))
            {
                return previousSelection;
            }
            foreach (var entry in roster)
            {
                if (unlocked.Contains(entry.Slug)) return entry.Slug;
            }
            return null;
        }

        /// <summary>Update the selected-character summary panel.</summary>
        public static void RenderSummary(
            Label nameLabel,
            Label bioLabel,
            Label hintLabel,
            LoadoutRosterEntry? entry,
            bool isUnlocked,
            LocalizationProvider loc)
        {
            if (entry == null)
            {
                if (nameLabel != null) nameLabel.text = string.Empty;
                if (bioLabel != null) bioLabel.text = string.Empty;
                if (hintLabel != null) hintLabel.text = string.Empty;
                return;
            }
            var e = entry.Value;
            if (nameLabel != null) nameLabel.text = loc.Loc(e.NameLocKey);
            if (bioLabel != null) bioLabel.text = loc.Loc(e.BioLocKey);
            if (hintLabel != null) hintLabel.text = isUnlocked ? string.Empty : loc.Loc(e.UnlockHintLocKey);
        }
    }

    /// <summary>MonoBehaviour shell — wires UI Toolkit to the renderer + services.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class LoadoutController : MonoBehaviour
    {
        public const string RunSceneName = "Run";
        public const string HomeSceneName = "MainMenu";

        // ---- Element names (must match Loadout.uxml) ----
        public const string CardListName = "card-list";
        public const string PlayButtonName = "btn-play";
        public const string BackButtonName = "btn-back";
        public const string SelectedNameLabel = "lbl-selected-name";
        public const string SelectedBioLabel = "lbl-selected-bio";
        public const string SelectedHintLabel = "lbl-selected-unlock-hint";

        [Tooltip("Roster slugs (kebab-case). Order is preserved by the UI grid.")]
        [SerializeField] private string[] _rosterSlugs = Array.Empty<string>();

        private UIDocument _doc = null!;
        private LocalizationProvider _loc = null!;
        private VisualElement _cardList = null!;
        private Label _selectedName = null!;
        private Label _selectedBio = null!;
        private Label _selectedHint = null!;
        private Button _btnPlay = null!;
        private string? _selectedSlug;
        private List<LoadoutRosterEntry> _roster = new();

        private void Awake()
        {
            _doc = GetComponent<UIDocument>();
            _loc = new LocalizationProvider();
            SafeAreaUtility.Attach(gameObject, _doc.rootVisualElement);
            _roster = BuildRoster(_rosterSlugs);
        }

        private void OnEnable()
        {
            var root = _doc.rootVisualElement;
            _cardList = root.Q<VisualElement>(CardListName)!;
            _selectedName = root.Q<Label>(SelectedNameLabel)!;
            _selectedBio = root.Q<Label>(SelectedBioLabel)!;
            _selectedHint = root.Q<Label>(SelectedHintLabel)!;
            _btnPlay = root.Q<Button>(PlayButtonName)!;

            _btnPlay.clicked += OnPlayClicked;
            root.Q<Button>(BackButtonName)?.RegisterCallback<ClickEvent>(_ => UIEvents.RaisePushScreen(HomeSceneName));

            RefreshRoster();
            _loc.ApplyToTree(root);
        }

        private void OnDisable()
        {
            if (_btnPlay != null) _btnPlay.clicked -= OnPlayClicked;
        }

        /// <summary>Pull unlocked roster from the service and rebuild the grid.</summary>
        public void RefreshRoster()
        {
            var unlocked = ReadUnlocked();
            _selectedSlug = LoadoutRenderer.AutoSelect(_roster, unlocked, LoadoutSelection.SelectedCharacterId);
            LoadoutRenderer.RenderRoster(_cardList, _roster, unlocked, _selectedSlug, _loc, OnCardTapped);
            RenderSummary(unlocked);
        }

        private void OnCardTapped(string slug)
        {
            _selectedSlug = slug;
            LoadoutRenderer.ApplySelection(_cardList, _selectedSlug);
            RenderSummary(ReadUnlocked());
        }

        private void OnPlayClicked()
        {
            // Defensive: if for any reason nothing is selected, fall back to the
            // first unlocked roster slug so the Play button is always live (US-36).
            var unlocked = ReadUnlocked();
            var slug = _selectedSlug ?? LoadoutRenderer.AutoSelect(_roster, unlocked, null);
            if (!string.IsNullOrEmpty(slug))
            {
                LoadoutSelection.Select(slug!);
            }
            UIEvents.RaiseStartRunRequested();
            SceneManager.LoadScene(RunSceneName, LoadSceneMode.Single);
        }

        private IReadOnlyCollection<string> ReadUnlocked()
        {
            if (GameContextBootstrap.Context == null) return Array.Empty<string>();
            if (!GameContextBootstrap.Context.TryGet<ICharacterUnlockService>(out var svc))
            {
                return Array.Empty<string>();
            }
            return new HashSet<string>(svc.GetUnlockedCharacterIds(), StringComparer.Ordinal);
        }

        private void RenderSummary(IReadOnlyCollection<string> unlocked)
        {
            LoadoutRosterEntry? entry = null;
            if (!string.IsNullOrEmpty(_selectedSlug))
            {
                foreach (var e in _roster)
                {
                    if (e.Slug == _selectedSlug) { entry = e; break; }
                }
            }
            bool isUnlocked = entry != null && unlocked.Contains(entry.Value.Slug);
            LoadoutRenderer.RenderSummary(_selectedName, _selectedBio, _selectedHint, entry, isUnlocked, _loc);
        }

        private static List<LoadoutRosterEntry> BuildRoster(string[] slugs)
        {
            var list = new List<LoadoutRosterEntry>(capacity: slugs.Length);
            foreach (var slug in slugs)
            {
                if (!string.IsNullOrEmpty(slug)) list.Add(LoadoutRosterEntry.FromSlug(slug));
            }
            return list;
        }
    }
}
