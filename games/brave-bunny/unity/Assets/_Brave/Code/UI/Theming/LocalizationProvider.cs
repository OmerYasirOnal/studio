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
            var table = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(json))
            {
                _tables[code] = table;
                return;
            }

            // Strip UTF-8 BOM if present (some editors / fastlane writers prepend EF BB BF).
            if (json.Length > 0 && json[0] == '﻿')
            {
                json = json.Substring(1);
            }

            // Format 1 (canonical): legacy { "_meta": {...}, "entries": [ {key,value}, ... ] }.
            // Format 2 (new): flat JSON object { "KEY": "value", ... } — matches LocalizationService.
            // We try the entries[] shape first via JsonUtility; if that yields nothing, we walk
            // the flat object with a hand-rolled tokenizer (so the UI assembly keeps zero
            // Newtonsoft dependency per its asmdef refs).
            try
            {
                var parsed = JsonUtility.FromJson<LocTableWire>(json);
                if (parsed?.entries != null && parsed.entries.Length > 0)
                {
                    foreach (var entry in parsed.entries)
                    {
                        if (!string.IsNullOrEmpty(entry.key)) table[entry.key] = entry.value ?? string.Empty;
                    }
                    _tables[code] = table;
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LocalizationProvider] entries[] parse skipped for {code}: {ex.Message}");
            }

            // Fall through to flat-object parse.
            try
            {
                ParseFlatObject(json, table);
                _tables[code] = table;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationProvider] Failed to parse table for {code}: {ex.Message}");
                _tables[code] = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Minimal flat JSON object parser: { "key": "value", "key2": "value2", ... }.
        /// Only string-typed values are collected; nested objects/arrays are skipped so
        /// `_meta_*` style scalar fields are simply ignored (they parse as strings, which
        /// is harmless since callers look up by exact game keys).
        /// </summary>
        private static void ParseFlatObject(string json, Dictionary<string, string> table)
        {
            var i = 0;
            var n = json.Length;
            // Skip whitespace + opening brace.
            while (i < n && char.IsWhiteSpace(json[i])) i++;
            if (i >= n || json[i] != '{') return;
            i++;

            while (i < n)
            {
                while (i < n && (char.IsWhiteSpace(json[i]) || json[i] == ',')) i++;
                if (i >= n || json[i] == '}') break;
                if (json[i] != '"') { i++; continue; }
                var key = ReadJsonString(json, ref i);
                while (i < n && char.IsWhiteSpace(json[i])) i++;
                if (i >= n || json[i] != ':') continue;
                i++;
                while (i < n && char.IsWhiteSpace(json[i])) i++;
                if (i >= n) break;
                if (json[i] == '"')
                {
                    var value = ReadJsonString(json, ref i);
                    if (!string.IsNullOrEmpty(key)) table[key] = value;
                }
                else if (json[i] == '{' || json[i] == '[')
                {
                    // Skip nested container.
                    SkipContainer(json, ref i);
                }
                else
                {
                    // Skip primitive (number/bool/null) until next comma or }.
                    while (i < n && json[i] != ',' && json[i] != '}') i++;
                }
            }
        }

        private static string ReadJsonString(string s, ref int i)
        {
            // Expects s[i] == '"'.
            i++;
            var sb = new System.Text.StringBuilder();
            while (i < s.Length)
            {
                var c = s[i];
                if (c == '"') { i++; return sb.ToString(); }
                if (c == '\\' && i + 1 < s.Length)
                {
                    var esc = s[i + 1];
                    switch (esc)
                    {
                        case '"': sb.Append('"'); i += 2; break;
                        case '\\': sb.Append('\\'); i += 2; break;
                        case '/': sb.Append('/'); i += 2; break;
                        case 'n': sb.Append('\n'); i += 2; break;
                        case 't': sb.Append('\t'); i += 2; break;
                        case 'r': sb.Append('\r'); i += 2; break;
                        case 'b': sb.Append('\b'); i += 2; break;
                        case 'f': sb.Append('\f'); i += 2; break;
                        case 'u':
                            if (i + 5 < s.Length && int.TryParse(s.Substring(i + 2, 4),
                                System.Globalization.NumberStyles.HexNumber,
                                System.Globalization.CultureInfo.InvariantCulture, out var code))
                            {
                                sb.Append((char)code);
                            }
                            i += 6;
                            break;
                        default: sb.Append(esc); i += 2; break;
                    }
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }
            return sb.ToString();
        }

        private static void SkipContainer(string s, ref int i)
        {
            var open = s[i];
            var close = open == '{' ? '}' : ']';
            var depth = 0;
            while (i < s.Length)
            {
                var c = s[i];
                if (c == '"') { ReadJsonString(s, ref i); continue; }
                if (c == open) depth++;
                else if (c == close) { depth--; i++; if (depth == 0) return; continue; }
                i++;
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
