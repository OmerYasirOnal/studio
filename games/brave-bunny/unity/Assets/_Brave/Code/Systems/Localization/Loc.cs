// Brave Bunny — Systems / Localization
// Static façade so UI code can call Loc.T("key") without injecting ILocalizationService.
// Bound once by GameContextBootstrap.Awake().

#nullable enable

namespace Brave.Systems.Localization;

/// <summary>
/// Convenience façade. <see cref="Bind"/> is called once by
/// <c>GameContextBootstrap.Awake</c>; UI code uses <c>Loc.T("home.play_button")</c>.
/// </summary>
public static class Loc
{
    private static ILocalizationService? _impl;

    public static void Bind(ILocalizationService impl) { _impl = impl; }

    /// <summary>Translate; returns the key untouched if no service is bound (test mode / pre-boot).</summary>
    public static string T(string key) => _impl?.Translate(key) ?? key;
}
