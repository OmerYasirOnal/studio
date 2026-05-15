// Brave Bunny — Systems / Progression
// Tech spec: docs/06-tech-spec/03-save-system.md (characters.{slug} schema)
// ADR-0008: every persisted field carries [JsonProperty].
//
// Meta-progression extension (character unlock service):
//   * `Unlocked` mirrors `Owned` semantically — kept as separate field so the
//     `Owned` flag can carry purchase intent (Star spend) while `Unlocked`
//     carries "earned via gameplay condition" provenance. Both are checked by
//     CharacterUnlockService.IsUnlocked. New saves default both to false for
//     non-starter characters; the starter (bunny) is seeded by
//     DefaultSaveFactory.
//   * Lifetime stats (RunsCompleted, BossesDefeated, HighestWaveReached) feed
//     UnlockCondition evaluation. They're per-character so e.g. "reach wave 5
//     with Dingo/Bunny" only counts Bunny runs.

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

    // ---- Meta-progression: character unlock service ----

    /// <summary>
    /// True once the character has cleared its <see cref="UnlockCondition"/>
    /// (or was seeded by <c>DefaultSaveFactory</c> as a starter). Distinct from
    /// <see cref="Owned"/>, which carries Star-purchase intent — both flags are
    /// "considered unlocked" by <c>CharacterUnlockService.IsUnlocked</c>.
    /// </summary>
    [JsonProperty("unlocked")] public bool Unlocked;

    /// <summary>UTC ISO-8601 timestamp of the unlock event; null while locked.</summary>
    [JsonProperty("unlockedAt")] public string? UnlockedAt;

    // ---- Lifetime stats — drive UnlockCondition evaluation ----

    /// <summary>Runs completed with this character (any outcome).</summary>
    [JsonProperty("runsCompleted")] public int RunsCompleted;

    /// <summary>Bosses defeated by runs piloting this character.</summary>
    [JsonProperty("bossesDefeated")] public int BossesDefeated;

    /// <summary>Highest wave ordinal reached by this character across all runs.</summary>
    [JsonProperty("highestWaveReached")] public int HighestWaveReached;
}
