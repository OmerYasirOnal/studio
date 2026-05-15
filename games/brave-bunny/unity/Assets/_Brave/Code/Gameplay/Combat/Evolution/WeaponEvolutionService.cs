#nullable enable
// Wave 9 — Weapon Evolution Service.
//
// Survivor.io-style evolution: at weapon L5, if the player also holds the matching
// charm at L5, the base weapon is swapped for its evolved form. ADR-0007 governs
// charm consumption (always true at launch — frees a passive slot).
//
// Listens for the two state-change events that can satisfy a recipe:
//   * LevelUpChannel  — a weapon (or charm) just reached L5 via a draft pick.
//   * PickupChannel   — reserved for in-world charm pickups (post-launch).
//
// Because the existing LevelUpEvent payload only carries player-XP-level (not
// weapon/charm slug), the service exposes explicit Notify* APIs the existing
// loadout/draft systems (UI agent) already call. The event-channel subscription
// is wired through CheckEvolutions() so any state change can trigger a sweep —
// the service is idempotent (evolutions only fire once per recipe per run).
//
// Recipe match rules (in order):
//   1. weaponInventory has baseWeaponId at level ≥ requiredWeaponLevel
//   2. charmInventory has requiredCharmId at level ≥ requiredCharmLevel
//   3. Recipe has not already fired this run
// On match:
//   * Replace baseWeaponId with evolvedWeaponId in weaponInventory (level reset to 1)
//   * If consumeCharm: remove requiredCharmId from charmInventory (ADR-0007)
//   * Raise WeaponEvolvedChannel
//   * Record recipe slug in _firedRecipes
//
// The service operates on string-keyed dictionaries (slug → level) supplied by the
// caller. WeaponDefinition / PassiveDefinition resolution is intentionally OUT of
// scope here; UI / RunController owns the catalog-to-slug mapping.

using System;
using System.Collections.Generic;
using Brave.Gameplay.Events;

namespace Brave.Gameplay.Combat.Evolution
{
    /// <summary>
    /// Mutable view of a player's owned weapons (slug → level). The service mutates
    /// this dictionary on evolution: removes the base entry, inserts the evolved one
    /// at level 1.
    /// </summary>
    public interface IWeaponInventory
    {
        bool TryGetLevel(string weaponId, out int level);
        void Remove(string weaponId);
        void Add(string weaponId, int level);
        IEnumerable<string> AllIds();
    }

    /// <summary>
    /// Mutable view of a player's owned charms (slug → level). ADR-0007: on
    /// evolution with consumeCharm=true, the matched charm is removed entirely
    /// (slot frees up for another pick).
    /// </summary>
    public interface ICharmInventory
    {
        bool TryGetLevel(string charmId, out int level);
        void Remove(string charmId);
        IEnumerable<string> AllIds();
    }

    /// <summary>Wave-9 contract surface — see file header for full semantics.</summary>
    public interface IWeaponEvolutionService
    {
        /// <summary>Register the catalog of recipes once at run start.</summary>
        void Initialize(IReadOnlyList<EvolutionRecipe> recipes);

        /// <summary>
        /// Sweep recipes against the supplied inventories. Returns the number of
        /// evolutions that fired this call. Idempotent: a recipe only fires once
        /// per run regardless of how many times this method is called.
        /// </summary>
        int CheckEvolutions(IWeaponInventory weapons, ICharmInventory charms, float runSeconds);

        /// <summary>True if the recipe with this evolved-weapon id has fired this run.</summary>
        bool HasFired(string evolvedWeaponId);
    }

    /// <summary>
    /// Default service impl. Stateless w.r.t. catalogs (re-Initialize swaps roster);
    /// stateful w.r.t. per-run "already fired" set (reset by Initialize).
    /// </summary>
    public sealed class WeaponEvolutionService : IWeaponEvolutionService
    {
        private readonly WeaponEvolvedChannel? _channel;
        private readonly HashSet<string> _firedRecipes = new HashSet<string>();
        private IReadOnlyList<EvolutionRecipe> _recipes = Array.Empty<EvolutionRecipe>();

        /// <summary>
        /// <paramref name="channel"/> may be null in EditMode tests; production wires
        /// the WeaponEvolvedChannel SO via the run-scene composition root.
        /// </summary>
        public WeaponEvolutionService(WeaponEvolvedChannel? channel = null)
        {
            _channel = channel;
        }

        public void Initialize(IReadOnlyList<EvolutionRecipe> recipes)
        {
            _recipes = recipes ?? Array.Empty<EvolutionRecipe>();
            _firedRecipes.Clear();
        }

        public bool HasFired(string evolvedWeaponId)
            => !string.IsNullOrEmpty(evolvedWeaponId) && _firedRecipes.Contains(evolvedWeaponId);

        public int CheckEvolutions(IWeaponInventory weapons, ICharmInventory charms, float runSeconds)
        {
            if (weapons == null || charms == null || _recipes.Count == 0) return 0;

            int fired = 0;
            foreach (var recipe in _recipes)
            {
                if (recipe == null || !recipe.IsValid()) continue;
                if (_firedRecipes.Contains(recipe.evolvedWeaponId)) continue;

                if (!weapons.TryGetLevel(recipe.baseWeaponId, out int wLvl)) continue;
                if (wLvl < recipe.requiredWeaponLevel) continue;

                if (!charms.TryGetLevel(recipe.requiredCharmId, out int cLvl)) continue;
                if (cLvl < recipe.requiredCharmLevel) continue;

                // Swap base → evolved in the weapon inventory.
                weapons.Remove(recipe.baseWeaponId);
                weapons.Add(recipe.evolvedWeaponId, 1);

                // ADR-0007: consume the charm (free the slot).
                if (recipe.consumeCharm)
                {
                    charms.Remove(recipe.requiredCharmId);
                }

                _firedRecipes.Add(recipe.evolvedWeaponId);
                fired++;

                _channel?.Raise(new WeaponEvolvedEvent(
                    recipe.baseWeaponId,
                    recipe.evolvedWeaponId,
                    recipe.consumeCharm ? recipe.requiredCharmId : string.Empty,
                    recipe.consumeCharm,
                    runSeconds));
            }
            return fired;
        }
    }
}
