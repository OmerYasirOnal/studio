// Brave Bunny — Systems / Settings
// Localization scope: TR / PH (Tagalog) / ID (Bahasa) launch markets + English baseline.
// See CLAUDE.md "soft-launch markets" reference + 02-meta-loop.md daily-streak markets.

#nullable enable

namespace Brave.Systems.Settings;

/// <summary>
/// Languages shipped at launch. Codes line up with ISO-639-1 / 639-3 where
/// applicable and with the JSON table file names under
/// <c>_Brave/Data/Localization/{code}.json</c>.
/// </summary>
public enum LanguageCode
{
    En = 0,
    Tr = 1,
    Id = 2,
    Ph = 3, // Filipino (Tagalog)
}

internal static class LanguageCodeExtensions
{
    public static string ToIso(this LanguageCode code) => code switch
    {
        LanguageCode.Tr => "tr",
        LanguageCode.Id => "id",
        LanguageCode.Ph => "fil",
        _ => "en",
    };

    public static LanguageCode FromIso(string? iso) => iso?.ToLowerInvariant() switch
    {
        "tr" => LanguageCode.Tr,
        "id" => LanguageCode.Id,
        "fil" or "tl" or "ph" => LanguageCode.Ph,
        _ => LanguageCode.En,
    };
}
