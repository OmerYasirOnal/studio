// Brave Bunny — Systems / Progression
// Design source: docs/02-gdd/02-meta-loop.md (character/biome ladders)
//                docs/02-gdd/08-economy.md (currency wallet & character meta levels)
// Tech spec: 03-save-system.md save triggers — every change here is followed by SaveService.Save().

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression;

public interface IProgressionService : IService
{
    CurrencyWallet Wallet { get; }
    bool IsCharacterOwned(string slug);
    void UnlockCharacter(string slug);
    int GetCharacterLevel(string slug);
    void AddCharacterXp(string slug, int xp);
    void EquipWeapon(string characterSlug, string weaponSlug);
}

/// <summary>
/// Character unlocks, levels, currencies. Achievements + daily streak live in
/// sibling services (<see cref="AchievementService"/>, <see cref="DailyStreakService"/>)
/// so this file stays under the 200-line guidance.
/// </summary>
public sealed class ProgressionService : IProgressionService
{
    private readonly ISaveService _save;

    public CurrencyWallet Wallet { get; }

    public ProgressionService(ISaveService save)
    {
        _save = save;
        Wallet = new CurrencyWallet(_save.Data.Currencies);
    }

    public bool IsCharacterOwned(string slug) =>
        _save.Data.Characters.TryGetValue(slug, out var p) && p.Owned;

    public void UnlockCharacter(string slug)
    {
        var profile = GetOrCreate(slug);
        if (profile.Owned) return;
        profile.Owned = true;
        _save.Save(); // 03-save-system.md trigger: "Character unlocked"
    }

    public int GetCharacterLevel(string slug) =>
        _save.Data.Characters.TryGetValue(slug, out var p) ? p.Level : 1;

    public void AddCharacterXp(string slug, int xp)
    {
        if (xp <= 0) return;
        var profile = GetOrCreate(slug);
        profile.Xp += xp;
        // Level-up curve lives in data/balance/economy.json (per CLAUDE.md principle 6 — no magic numbers).
        // This stub flags TODO for balance-engineer to wire the real XP curve.
        // TODO: consult ICatalogService for level-up thresholds.
    }

    public void EquipWeapon(string characterSlug, string weaponSlug)
    {
        var profile = GetOrCreate(characterSlug);
        if (profile.EquippedWeaponSlug == weaponSlug) return;
        profile.EquippedWeaponSlug = weaponSlug;
        _save.Save(); // 03-save-system.md trigger: "Cosmetic equipped" (weapon-equip uses same pattern)
    }

    private CharacterProfile GetOrCreate(string slug)
    {
        if (!_save.Data.Characters.TryGetValue(slug, out var profile))
        {
            profile = new CharacterProfile { Owned = false, Level = 1, Xp = 0 };
            _save.Data.Characters[slug] = profile;
        }
        return profile;
    }
}
