// Brave Bunny — Systems / Progression
// Tech spec: docs/06-tech-spec/03-save-system.md (characters.{slug} schema)
// ADR-0008: every persisted field carries [JsonProperty].

#nullable enable

using System;
using Newtonsoft.Json;

namespace Brave.Systems.Progression;

/// <summary>
/// Per-character persistent profile. Mirrors the
/// <c>characters.{slug}</c> shape in the v1 save schema.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class CharacterProfile
{
    [JsonProperty("owned")] public bool Owned;
    [JsonProperty("level")] public int Level = 1;
    [JsonProperty("xp")] public int Xp;
    [JsonProperty("equippedWeaponSlug")] public string? EquippedWeaponSlug;
    [JsonProperty("equippedSkinSlug")] public string? EquippedSkinSlug;
}
