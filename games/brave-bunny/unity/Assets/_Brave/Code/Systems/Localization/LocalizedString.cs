// Brave Bunny — Systems / Localization
// Wrapper struct so UI code can hold a key without an immediate lookup.
// Resolution happens at draw time so language switches hot-swap without rebinding.

#nullable enable

using System;

namespace Brave.Systems.Localization;

/// <summary>
/// Carries a localization key plus optional args until resolved. Equality is
/// key-only so collections of <see cref="LocalizedString"/> can be deduped
/// regardless of arg payload.
/// </summary>
public readonly struct LocalizedString : IEquatable<LocalizedString>
{
    public readonly string Key;
    public readonly object[]? Args;

    public LocalizedString(string key, params object[] args) { Key = key; Args = args; }

    public string Resolve()
    {
        var raw = Loc.T(Key);
        return Args is { Length: > 0 } ? string.Format(System.Globalization.CultureInfo.InvariantCulture, raw, Args) : raw;
    }

    public bool Equals(LocalizedString other) => Key == other.Key;
    public override bool Equals(object? obj) => obj is LocalizedString o && Equals(o);
    public override int GetHashCode() => Key?.GetHashCode() ?? 0;
    public override string ToString() => Resolve();
}
