// ADR-0007: evolution charm consumption. weapons.json + passives.json drive recipes.
using Brave.Gameplay.Definitions;
using System.Collections.Generic;
using UnityEngine;

namespace Brave.Gameplay.Combat;

/// <summary>
/// Checks the player's owned weapon + passive levels against each weapon's
/// <see cref="WeaponDefinition.evolution"/> recipe; on match, swaps the base weapon for
/// its evolved form and consumes the ingredient charm (ADR-0007).
/// </summary>
public sealed class WeaponEvolution
{
    private readonly Dictionary<string, int> _weaponLevels;   // slug → level
    private readonly Dictionary<string, int> _passiveLevels;  // slug → level

    public WeaponEvolution(Dictionary<string, int> weaponLevels, Dictionary<string, int> passiveLevels)
    {
        _weaponLevels = weaponLevels;
        _passiveLevels = passiveLevels;
    }

    /// <summary>
    /// Try to evolve <paramref name="weapon"/>. Returns the evolved <see cref="WeaponDefinition"/>
    /// on success and removes the consumed passive from the owned-passive set.
    /// </summary>
    public bool TryEvolve(WeaponDefinition weapon, out WeaponDefinition evolved)
    {
        evolved = null;
        if (weapon == null || weapon.evolution == null) return false;
        if (weapon.evolution.ingredient == null || weapon.evolution.resultWeapon == null) return false;
        if (!_weaponLevels.TryGetValue(weapon.slug, out var wLvl) || wLvl < 5) return false;
        if (!_passiveLevels.TryGetValue(weapon.evolution.ingredient.slug, out var pLvl) || pLvl < 5) return false;

        // ADR-0007: consume the charm.
        _passiveLevels[weapon.evolution.ingredient.slug] = 0;
        evolved = weapon.evolution.resultWeapon;
        return true;
    }
}
