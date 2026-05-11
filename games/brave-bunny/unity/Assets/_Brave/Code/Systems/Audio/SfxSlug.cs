// Brave Bunny — Systems / Audio
// Slug constants matching docs/08-audio-bible/02-sfx-spec.md (~53 launch slugs).
// String constants (not enums) so callers can reference slugs without circular
// asmdef dependencies into Catalog. Slugs map 1:1 to .ogg file basenames under
// unity/Assets/_Brave/Audio/SFX/<bucket>/.

#nullable enable

namespace Brave.Systems.Audio;

/// <summary>SFX slug constants. Source of truth: 08-audio-bible/02-sfx-spec.md.</summary>
public static class SfxSlug
{
    // UI bucket
    public const string UiButtonPress = "ui_button_press";
    public const string UiButtonBack = "ui_button_back";
    public const string UiModalOpen = "ui_modal_open";
    public const string UiModalClose = "ui_modal_close";
    public const string UiTabSwitch = "ui_tab_switch";
    public const string UiTapTick = "ui_tap_tick";
    public const string UiLockedShake = "ui_locked_shake";
    public const string UiToastIn = "ui_toast_in";
    public const string UiPurchaseConfirm = "ui_purchase_confirm";
    public const string UiStreakClaim = "ui_streak_claim";
    public const string UiAchievementPop = "ui_achievement_pop";
    public const string UiStoreBrowse = "ui_store_browse";
    public const string UiIapConfirm = "ui_iap_confirm";

    // Gameplay (non-combat)
    public const string RunStart = "run_start";
    public const string RunLevelup = "run_levelup";
    public const string RunPickupXpSmall = "run_pickup_xp_small";
    public const string RunPickupXpLarge = "run_pickup_xp_large";
    public const string RunPickupGold = "run_pickup_gold";
    public const string RunPickupHeart = "run_pickup_heart";

    // Combat (weapons + enemies + hero)
    public const string WeaponCarrotFire = "weapon_carrot_fire";
    public const string WeaponCarrotReturn = "weapon_carrot_return";
    public const string WeaponSunbeamLoop = "weapon_sunbeam_loop";
    public const string WeaponSunbeamStart = "weapon_sunbeam_start";
    public const string WeaponDaisyDrop = "weapon_daisy_drop";
    public const string WeaponDaisyExplode = "weapon_daisy_explode";
    public const string EnemySwarmerHit = "enemy_swarmer_hit";
    public const string EnemySwarmerDie = "enemy_swarmer_die";
    public const string EnemyEliteHit = "enemy_elite_hit";
    public const string EnemyEliteDie = "enemy_elite_die";
    public const string EnemyBossHit = "enemy_boss_hit";
    public const string EnemyBossDie = "enemy_boss_die";
    public const string BossIntroSting = "boss_intro_sting";
    public const string BossPhaseChange = "boss_phase_change";
    public const string BossTelegraphWarn = "boss_telegraph_warn";
    public const string HeroHit = "hero_hit";
    public const string HeroDeath = "hero_death";
    public const string HeroLevelupFanfare = "hero_levelup_fanfare";
    public const string HeroDash = "hero_dash";
    public const string HeroHeal = "hero_heal";

    // Endgame
    public const string RunEndWin = "run_end_win";
    public const string RunEndLose = "run_end_lose";
    public const string TallyCountTick = "tally_count_tick";
    public const string TallySlam = "tally_slam";
    public const string ReviveOfferIn = "revive_offer_in";

    // Meta
    public const string UnlockCharacter = "unlock_character";
    public const string UnlockWeapon = "unlock_weapon";
    public const string PassTierUp = "pass_tier_up";
    public const string DailyStreakChime = "daily_streak_chime";

    // Environment ambient beds (looping)
    public const string AmbientMeadowBed = "ambient_meadow_bed";
    public const string AmbientBeachBed = "ambient_beach_bed";
    public const string AmbientForestBed = "ambient_forest_bed";
    public const string AmbientCavernBed = "ambient_cavern_bed";
    public const string AmbientSnowBed = "ambient_snow_bed";
}
