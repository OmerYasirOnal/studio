// Brave Bunny — UI / Bindings / LoadoutSelection
//
// Tiny PlayerPrefs-backed memory of "which character did the player pick on the
// Loadout screen?" The Run scene reads SelectedCharacterId on enter; the
// LoadoutController writes it before pushing the Run scene.
//
// Design choice:
//   * Lives in Brave.UI.Bindings so we don't pull a new service into
//     GameContext or block on integration-agent registry plumbing — this is
//     a UI-screen-local breadcrumb, not a meta-progression record.
//   * Storage is testable via ILoadoutSelectionStore (PlayerPrefs in production,
//     InMemoryLoadoutSelectionStore in EditMode tests).
//   * Empty string ⇒ "no choice yet" (LoadoutController falls back to the
//     first unlocked character).
//
// Spec refs:
//   * docs/05-wireframes/04-loadout.html — Off-we-go button persists choice.
//   * docs/02-gdd/02-meta-loop.md § Character ladder — chosen slug is the
//     argument the Run scene passes to RunController.Begin().

#nullable enable

using UnityEngine;

namespace Brave.UI.Bindings
{
    /// <summary>Storage abstraction so tests can swap PlayerPrefs for an in-memory dict.</summary>
    public interface ILoadoutSelectionStore
    {
        string? Read();
        void Write(string slug);
    }

    /// <summary>Production store — round-trips through PlayerPrefs.</summary>
    public sealed class PlayerPrefsLoadoutSelectionStore : ILoadoutSelectionStore
    {
        public const string PrefKey = "brave.loadout.character";

        public string? Read()
        {
            var v = PlayerPrefs.GetString(PrefKey, string.Empty);
            return string.IsNullOrEmpty(v) ? null : v;
        }

        public void Write(string slug)
        {
            PlayerPrefs.SetString(PrefKey, slug ?? string.Empty);
            PlayerPrefs.Save();
        }
    }

    /// <summary>In-memory store — used by EditMode tests.</summary>
    public sealed class InMemoryLoadoutSelectionStore : ILoadoutSelectionStore
    {
        private string? _slug;
        public string? Read() => _slug;
        public void Write(string slug) => _slug = slug;
    }

    /// <summary>
    /// "Which character did the player choose on the Loadout screen?" surface.
    /// Defaults to <see cref="PlayerPrefsLoadoutSelectionStore"/> for production
    /// boot, but EditMode tests can <see cref="UseStore"/> to inject a fake.
    /// </summary>
    public static class LoadoutSelection
    {
        private static ILoadoutSelectionStore _store = new PlayerPrefsLoadoutSelectionStore();

        /// <summary>The slug last picked by the player, or null if none picked yet.</summary>
        public static string? SelectedCharacterId => _store.Read();

        /// <summary>Persist <paramref name="slug"/> as the active selection.</summary>
        public static void Select(string slug) => _store.Write(slug);

        /// <summary>Test hook: swap out the store. Production code never calls this.</summary>
        public static void UseStore(ILoadoutSelectionStore store) => _store = store
            ?? throw new System.ArgumentNullException(nameof(store));

        /// <summary>EditMode-test reset.</summary>
        public static void ResetForTests()
        {
            _store = new InMemoryLoadoutSelectionStore();
        }
    }
}
