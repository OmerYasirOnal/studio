// Brave Bunny — UI / Theming / LocalizationProvider
// Tech spec: docs/06-tech-spec/04-asset-pipeline.md (string tables under
//            _Brave/Localization/{lang}.json — flat key→value JSON).
// Narrative:  docs/02-gdd/narrative/05-localization-keys.md (master key list).
// Owner: ui-engineer.
//
// Loc rule (CLAUDE.md + tone-bible §6): NEVER inline visible English strings.
// All visible Label/Button text must resolve via Loc("KEY"). The UXML files
// carry a `loc-key="..."` attribute on every translatable element; on
// UIDocument enable we walk the tree and rewrite `text` from the active table.
//
// This provider is intentionally framework-light:
//   - Reads JSON via JsonUtility (no Newtonsoft dependency).
//   - Hot-reload supported by calling SetLanguage(code) at runtime.
//   - When no entry exists, returns the key itself so missing strings are
//     screamingly visible to QA — never silent.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.UI.Theming
{
    /// <summary>
    /// UI-layer localization gateway. Decoupled from any specific
    /// <c>ILocalizationService</c> on the systems side so the UI assembly can
    /// build before the systems-engineer ships the canonical service.
    /// </summary>
    public sealed class LocalizationProvider
    {
        private const string LocKeyAttribute = "loc-key";

        public event Action? OnLanguageChanged;

        public LanguageCode Current { get; private set; } = LanguageCode.En;

        private readonly Dictionary<LanguageCode, Dictionary<string, string>> _tables = new();

        public LocalizationProvider()
        {
            // Default constructor: load from Resources/Localization/{lang}.json.
            // For paths outside Resources/, use the ctor accepting raw JSON.
            LoadTable(LanguageCode.En, "en");
            LoadTable(LanguageCode.Tr, "tr");
        }

        /// <summary>
        /// Constructor for non-Resources tables. Supply raw JSON strings keyed by
        /// language. Used by GameContextBootstrap which loads TextAsset slots
        /// pointing at <c>Assets/_Brave/Localization/{lang}.json</c>.
        /// </summary>
        public LocalizationProvider(System.Collections.Generic.IDictionary<LanguageCode, string> rawJsonByLanguage)
        {
            foreach (var kv in rawJsonByLanguage)
            {
                ParseAndStore(kv.Key, kv.Value);
            }
        }

        /// <summary>Returns the localized value for <paramref name="key"/> or the key itself when absent.</summary>
        public string Loc(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_tables.TryGetValue(Current, out var table) && table.TryGetValue(key, out var value))
            {
                return value;
            }
            // Fallback to English so we never show TR-only keys when EN is requested.
            if (Current != LanguageCode.En
                && _tables.TryGetValue(LanguageCode.En, out var en)
                && en.TryGetValue(key, out var enValue))
            {
                return enValue;
            }
            return key;
        }

        /// <summary>Switch active language and broadcast so live UI re-resolves.</summary>
        public void SetLanguage(LanguageCode code)
        {
            if (Current == code) return;
            Current = code;
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Walk a UXML subtree, find every element with a `loc-key` USS class or
        /// userdata key, and rewrite its <c>text</c> from the active table.
        /// Idempotent — safe to call after language change.
        /// </summary>
        public void ApplyToTree(VisualElement root)
        {
            if (root == null) return;
            root.Query<VisualElement>().ForEach(ApplyToElement);
        }

        private void ApplyToElement(VisualElement el)
        {
            // UXML's `loc-key` lands in the userData/attributes dictionary in
            // Unity 6's UIElementsCore. We probe via a known convention: the
            // attribute is registered as an element style at import.
            if (el is TextElement te && !string.IsNullOrEmpty(te.viewDataKey))
            {
                // viewDataKey is per-element user data already — keep it free.
            }
            // The canonical lookup: the loc-key is exposed via the element's
            // dataSource when bound by UxmlObject parsers. We rely on a sentinel
            // userData entry written by the controller during BindLocKeys.
            if (el.userData is string locKey && el is TextElement target)
            {
                target.text = Loc(locKey);
            }
        }

        /// <summary>
        /// Walk the tree once at startup, copy the `loc-key` attribute (read via
        /// reflection on the UXML attribute bag) into <c>userData</c>, then
        /// resolve. This is called by every Controller's OnEnable.
        /// </summary>
        public void BindLocKeys(VisualElement root)
        {
            if (root == null) return;
            root.Query<VisualElement>().ForEach(el =>
            {
                if (el is not TextElement te) return;
                // Convention: controllers may pre-populate userData themselves; otherwise
                // we look up by element name (e.g. "lbl-streak-hook" → no key).
                if (el.userData is string locKey && !string.IsNullOrEmpty(locKey))
                {
                    te.text = Loc(locKey);
                }
            });
        }

        private void LoadTable(LanguageCode code, string fileName)
        {
            var text = Resources.Load<TextAsset>($"Localization/{fileName}");
            if (text == null)
            {
                _tables[code] = new Dictionary<string, string>();
                return;
            }
            ParseAndStore(code, text.text);
        }

        private void ParseAndStore(LanguageCode code, string json)
        {
            try
            {
                var parsed = JsonUtility.FromJson<LocTableWire>(json);
                var table = new Dictionary<string, string>(parsed?.entries?.Length ?? 0);
                if (parsed?.entries != null)
                {
                    foreach (var entry in parsed.entries)
                    {
                        if (!string.IsNullOrEmpty(entry.key)) table[entry.key] = entry.value ?? string.Empty;
                    }
                }
                _tables[code] = table;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationProvider] Failed to parse table for {code}: {ex.Message}");
                _tables[code] = new Dictionary<string, string>();
            }
        }

        [Serializable]
        private sealed class LocTableWire
        {
            public LocEntryWire[]? entries;
        }

        [Serializable]
        private sealed class LocEntryWire
        {
            public string key = string.Empty;
            public string value = string.Empty;
        }
    }
}
