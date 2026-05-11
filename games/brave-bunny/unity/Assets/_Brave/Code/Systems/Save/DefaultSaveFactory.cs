// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (defaults: Bunny owned, Carrot Boomerang equipped)
// 02-meta-loop.md: Bunny is the starter character; daily-streak starts day 1.

#nullable enable

using System;
using Brave.Systems.Progression;

namespace Brave.Systems.Save;

/// <summary>
/// Builds a fresh-player <see cref="SaveData"/>. Invoked by
/// <see cref="SaveService.ClearAll"/> and when every backup candidate fails
/// the corruption-recovery cascade.
/// </summary>
internal static class DefaultSaveFactory
{
    public static SaveData Create()
    {
        var now = DateTime.UtcNow.ToString("o");
        var data = new SaveData
        {
            Version = SaveHeader.CurrentVersion,
            CreatedAt = now,
            LastSavedAt = now,
        };
        data.Player.Id = NewUlid();
        data.Player.DisplayName = "Player";
        data.Player.Language = "en";

        data.Characters["bunny"] = new CharacterProfile
        {
            Owned = true,
            Level = 1,
            Xp = 0,
            EquippedWeaponSlug = "carrot-boomerang",
            EquippedSkinSlug = null,
        };
        data.Weapons["carrot-boomerang"] = new SaveData.WeaponEntry { PermaUnlocked = true };
        data.Passives["magnet-charm"] = new SaveData.PassiveEntry { PermaUnlocked = true };
        return data;
    }

    // Lightweight ULID-shaped identifier (Crockford-base32, 26 chars). Not
    // strictly monotonic — sufficient for client-side correlation per
    // 03-save-system.md privacy posture.
    private static string NewUlid()
    {
        const string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        var rng = new Random();
        var buf = new char[26];
        for (var i = 0; i < buf.Length; i++) buf[i] = alphabet[rng.Next(alphabet.Length)];
        return new string(buf);
    }
}
