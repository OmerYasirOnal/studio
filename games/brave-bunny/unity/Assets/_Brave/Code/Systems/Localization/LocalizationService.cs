// Brave Bunny — Systems / Localization
// Tech spec: docs/06-tech-spec/00-engine-and-version.md (Unity.Localization package present).
// Lightweight in-house key→string lookup. Future wave can swap to Unity.Localization tables
// without changing the ILocalizationService surface.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brave.Systems.Localization;

/// <summary>
/// File-backed localization service. Each <c>TextAsset</c> in the
/// constructor is a JSON object of <c>"key": "value"</c> pairs; the file's
/// asset name (without extension) is the BCP-47 language code (e.g. <c>en.json</c>).
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _tables;
    private string _current = "en";

    public string CurrentLanguage => _current;
    public event Action<string>? LanguageChanged;

    public LocalizationService(IReadOnlyList<TextAsset>? tables)
    {
        _tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        if (tables == null) return;

        for (var i = 0; i < tables.Count; i++)
        {
            var asset = tables[i];
            if (asset == null) continue;
            try
            {
                var root = JObject.Parse(asset.text);
                var map = new Dictionary<string, string>(capacity: 64);
                foreach (var kv in root)
                {
                    if (kv.Value?.Type == JTokenType.String) map[kv.Key] = (string)kv.Value!;
                }
                _tables[asset.name] = map;
            }
            catch (JsonException e)
            {
                Debug.LogError($"[LocalizationService] Parse failure on '{asset.name}': {e.Message}");
            }
        }
    }

    public void SetLanguage(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        if (string.Equals(_current, code, StringComparison.OrdinalIgnoreCase)) return;
        if (!_tables.ContainsKey(code))
        {
            Debug.LogWarning($"[LocalizationService] Unknown language '{code}', staying on '{_current}'.");
            return;
        }
        _current = code;
        LanguageChanged?.Invoke(_current);
    }

    public string Translate(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        if (_tables.TryGetValue(_current, out var table) && table.TryGetValue(key, out var v)) return v;
        // Missing-key fallback: try "en" then echo the key so QA sees the gap.
        if (!string.Equals(_current, "en", StringComparison.OrdinalIgnoreCase) &&
            _tables.TryGetValue("en", out var enTable) &&
            enTable.TryGetValue(key, out var enVal)) return enVal;
        return key;
    }
}
