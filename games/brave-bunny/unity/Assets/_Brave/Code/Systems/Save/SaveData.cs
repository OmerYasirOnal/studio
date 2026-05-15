// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (payload schema v1)
// ADR-0008: every field gets [JsonProperty] for rename-safe forward-compat.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Brave.Systems.Progression;

namespace Brave.Systems.Save;

/// <summary>
/// Root POCO that mirrors the v1 schema in 03-save-system.md.
/// OptIn member serialization — unknown fields are stripped, every persisted
/// field must declare <see cref="JsonPropertyAttribute"/>.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class SaveData
{
    [JsonProperty("version")] public int Version = SaveHeader.CurrentVersion;
    [JsonProperty("player")] public PlayerSection Player = new();
    [JsonProperty("currencies")] public CurrenciesSection Currencies = new();
    [JsonProperty("characters")] public Dictionary<string, CharacterProfile> Characters = new();
    [JsonProperty("weapons")] public Dictionary<string, WeaponEntry> Weapons = new();
    [JsonProperty("passives")] public Dictionary<string, PassiveEntry> Passives = new();
    [JsonProperty("cosmetics")] public Dictionary<string, CosmeticEntry> Cosmetics = new();
    [JsonProperty("battlePass")] public BattlePassSection BattlePass = new();
    [JsonProperty("achievements")] public Dictionary<string, AchievementEntry> Achievements = new();
    [JsonProperty("dailyMissions")] public DailyMissionsSection DailyMissions = new();
    [JsonProperty("dailyStreak")] public DailyStreakSection DailyStreak = new();
    // Wave 9: 7-day rotating login-reward calendar. Owned by DailyRewardService.
    // Forward-compat per ADR-0008 — missing key in v1/v2 saves defaults to day 1,
    // no lastClaimUtc, zero lifetime claims (i.e. claimable on first launch).
    [JsonProperty("dailyRewardState")] public DailyRewardState DailyRewardState = new();
    [JsonProperty("settings")] public SettingsSection Settings = new();
    [JsonProperty("stats")] public StatsSection Stats = new();
    // Wave 7C: first-run tutorial completion flag. Defaults to false; the
    // TutorialController consults TutorialState.ShouldShow() to decide whether
    // to mount the overlay on Run-scene start. Forward-compat per ADR-0008
    // (missing key in v1 saves deserializes as default false).
    [JsonProperty("tutorialSeen")] public bool TutorialSeen;
    [JsonProperty("createdAt")] public string CreatedAt = DateTime.UtcNow.ToString("o");
    [JsonProperty("lastSavedAt")] public string LastSavedAt = DateTime.UtcNow.ToString("o");

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class PlayerSection
    {
        [JsonProperty("id")] public string Id = string.Empty;             // ULID, locally generated at first launch
        [JsonProperty("displayName")] public string DisplayName = "Player";
        [JsonProperty("language")] public string Language = "en";
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CurrenciesSection
    {
        [JsonProperty("carrots")] public long Carrots;
        [JsonProperty("stars")] public long Stars;
        [JsonProperty("soulShards")] public long SoulShards;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class WeaponEntry
    {
        [JsonProperty("permaUnlocked")] public bool PermaUnlocked;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class PassiveEntry
    {
        [JsonProperty("permaUnlocked")] public bool PermaUnlocked;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CosmeticEntry
    {
        [JsonProperty("owned")] public bool Owned;
        [JsonProperty("shards")] public int Shards;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class BattlePassSection
    {
        [JsonProperty("season")] public int Season = 1;
        [JsonProperty("tier")] public int Tier;
        [JsonProperty("xp")] public int Xp;
        [JsonProperty("premiumOwned")] public bool PremiumOwned;
        [JsonProperty("claimedFreeTiers")] public List<int> ClaimedFreeTiers = new();
        [JsonProperty("claimedPremiumTiers")] public List<int> ClaimedPremiumTiers = new();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class AchievementEntry
    {
        [JsonProperty("progress")] public int Progress;
        [JsonProperty("claimed")] public bool Claimed;
        [JsonProperty("completedAt")] public string? CompletedAt;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DailyMissionEntry
    {
        [JsonProperty("slug")] public string Slug = string.Empty;
        [JsonProperty("progress")] public int Progress;
        [JsonProperty("completed")] public bool Completed;
        [JsonProperty("claimed")] public bool Claimed;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DailyMissionsSection
    {
        [JsonProperty("rolledForDate")] public string? RolledForDate;
        [JsonProperty("missions")] public List<DailyMissionEntry> Missions = new();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class DailyStreakSection
    {
        [JsonProperty("currentDay")] public int CurrentDay = 1;
        [JsonProperty("lastClaimUtcDate")] public string? LastClaimUtcDate;
        [JsonProperty("skipTokensUsed")] public int SkipTokensUsed;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SettingsSection
    {
        [JsonProperty("audioMaster")] public float AudioMaster = 0.8f;
        [JsonProperty("audioMusic")] public float AudioMusic = 0.7f;
        [JsonProperty("audioSfx")] public float AudioSfx = 0.9f;
        [JsonProperty("hapticsEnabled")] public bool HapticsEnabled = true;
        [JsonProperty("lowPowerMode")] public bool LowPowerMode;
        [JsonProperty("tapToMove")] public bool TapToMove;
        [JsonProperty("language")] public string Language = "en";
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class StatsSection
    {
        [JsonProperty("totalRuns")] public long TotalRuns;
        [JsonProperty("totalKills")] public long TotalKills;
        [JsonProperty("bestRunTimeSeconds")] public float BestRunTimeSeconds;
        [JsonProperty("bossesDefeated")] public long BossesDefeated;
        [JsonProperty("evolutionsTriggered")] public long EvolutionsTriggered;
    }
}

/// <summary>
/// Wave 9 — daily login reward calendar state. Owned by
/// <see cref="Brave.Systems.LiveOps.DailyRewardService"/>. Top-level (not nested
/// in SaveData) so the LiveOps service references it without coupling to the
/// SaveData class graph; the SaveData root holds an instance for serialization.
/// ADR-0008: every field declares [JsonProperty] for rename-safe forward-compat.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class DailyRewardState
{
    [JsonProperty("currentDay")] public int CurrentDay = 1;
    [JsonProperty("lastClaimUtc")] public string? LastClaimUtc;
    [JsonProperty("lifetimeClaims")] public int LifetimeClaims;
}
