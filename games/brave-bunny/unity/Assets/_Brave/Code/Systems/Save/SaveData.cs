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
    // Wave 9: LiveOps battle-pass scaffold. Lives alongside the legacy
    // BattlePassSection (v1 schema) for forward-compat per ADR-0008 — the new
    // BattlePassService writes only to this field. Legacy section is preserved
    // so older saves don't lose data during migration.
    [JsonProperty("battlePassState")] public BattlePassState BattlePassState = new();
    [JsonProperty("achievements")] public Dictionary<string, AchievementEntry> Achievements = new();
    [JsonProperty("dailyMissions")] public DailyMissionsSection DailyMissions = new();
    [JsonProperty("dailyStreak")] public DailyStreakSection DailyStreak = new();
    // Wave 9: 7-day rotating login-reward calendar (DailyRewardService).
    [JsonProperty("dailyRewardState")] public DailyRewardState DailyRewardState = new();
    // Wave 9 LiveOps — daily quest/mission system (rotates at UTC midnight).
    [JsonProperty("questState")] public QuestState QuestState = new();
    [JsonProperty("settings")] public SettingsSection Settings = new();
    [JsonProperty("stats")] public StatsSection Stats = new();
    // Wave 7C: first-run tutorial completion flag. Defaults to false; the
    // TutorialController consults TutorialState.ShouldShow() to decide whether
    // to mount the overlay on Run-scene start. Forward-compat per ADR-0008
    // (missing key in v1 saves deserializes as default false).
    [JsonProperty("tutorialSeen")] public bool TutorialSeen;
    // Wave 9: opaque IAP receipt tokens (<sku>_<utc_timestamp>). One-time
    // non-consumable SKUs (ad removal, character unlocks, BP premium) check
    // this list to gate duplicate purchases. The platform-issued receipts
    // remain inside Unity IAP's own store and are not persisted here.
    [JsonProperty("purchaseReceipts")] public List<string> PurchaseReceipts = new();
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

    // ---- Wave 9 LiveOps: daily quest system ----
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class QuestEntry
    {
        [JsonProperty("id")] public string Id = string.Empty;
        [JsonProperty("progress")] public int Progress;
        [JsonProperty("claimed")] public bool Claimed;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class QuestState
    {
        [JsonProperty("rolledForDate")] public string? RolledForDate;
        [JsonProperty("entries")] public List<QuestEntry> Entries = new();
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

/// Wave 9 — daily login reward calendar state. Owned by
/// <see cref="Brave.Systems.LiveOps.DailyRewardService"/>.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class DailyRewardState
{
    [JsonProperty("currentDay")] public int CurrentDay = 1;
    [JsonProperty("lastClaimUtc")] public string? LastClaimUtc;
    [JsonProperty("lifetimeClaims")] public int LifetimeClaims;
}

/// <summary>
/// Wave 9 LiveOps battle-pass persistent state. Top-level POCO referenced by
/// LiveOps assembly without circular dep. Coexists with the legacy
/// <see cref="SaveData.BattlePassSection"/> for ADR-0008 forward-compat.
/// </summary>
[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public sealed class BattlePassState
{
    [JsonProperty("seasonId")] public string SeasonId = string.Empty;
    [JsonProperty("currentXp")] public int CurrentXp;
    [JsonProperty("currentTier")] public int CurrentTier;
    [JsonProperty("premiumActive")] public bool PremiumActive;
    [JsonProperty("claimedFreeTiers")] public List<int> ClaimedFreeTiers = new();
    [JsonProperty("claimedPremiumTiers")] public List<int> ClaimedPremiumTiers = new();
    [JsonProperty("lastSeasonResetUtc")] public string LastSeasonResetUtc = string.Empty;

    public void BeginNewSeason(string seasonId)
    {
        SeasonId = seasonId;
        CurrentXp = 0;
        CurrentTier = 0;
        PremiumActive = false;
        ClaimedFreeTiers.Clear();
        ClaimedPremiumTiers.Clear();
        LastSeasonResetUtc = DateTime.UtcNow.ToString("o");
    }
}
