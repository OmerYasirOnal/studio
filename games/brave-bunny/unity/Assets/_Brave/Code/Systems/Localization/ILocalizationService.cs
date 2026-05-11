// Brave Bunny — Systems / Localization
// Tech spec: docs/06-tech-spec/03-save-system.md (player.language persisted in save)
// CC0 / OFL fonts only (CLAUDE.md principle 8). Tables under _Brave/Localization/Tables/<lang>.json.

#nullable enable

using Brave.Systems.Context;

namespace Brave.Systems.Localization;

/// <summary>
/// Loads language tables (key → localized string) at boot and serves lookups
/// to UI and code via <see cref="Loc"/>. Missing keys return the key itself so
/// untranslated UI is visibly broken in QA rather than silently empty.
/// </summary>
public interface ILocalizationService : IService
{
    /// <summary>BCP-47 language code currently in use (e.g. <c>"en"</c>, <c>"tr"</c>).</summary>
    string CurrentLanguage { get; }

    /// <summary>Switch language. Triggers UI rebinds via <see cref="LanguageChanged"/>.</summary>
    void SetLanguage(string code);

    /// <summary>Resolve <paramref name="key"/> in the current language; returns the key on miss.</summary>
    string Translate(string key);

    /// <summary>Raised after <see cref="SetLanguage"/> succeeds; payload is the new language code.</summary>
    event System.Action<string>? LanguageChanged;
}
