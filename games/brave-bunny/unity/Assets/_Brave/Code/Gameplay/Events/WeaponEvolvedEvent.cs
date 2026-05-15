// Brave Bunny — Gameplay/Events/WeaponEvolvedEvent + WeaponEvolvedChannel (Wave 9).
//
// Tech-spec 09 § Tier 3 typed ScriptableObject event channel. Fired by
// WeaponEvolutionService at the moment a recipe matches and the base weapon is
// swapped for its evolved form. Consumers:
//   * UI (future) — pop the evolution toast ("Magnet Charm consumed → Carrot Boomerang
//                   evolved to Harvest Cyclone") per ADR-0007.
//   * Audio bindings — evolution stinger.
//   * Analytics — track which recipes players actually hit.
//
// Payload is a readonly struct (pass-by-value, zero GC) per tech-spec 09.

#nullable enable

using UnityEngine;

namespace Brave.Gameplay.Events
{
    /// <summary>
    /// Broadcast on <see cref="WeaponEvolvedChannel"/> when a weapon successfully
    /// evolves. Carries slugs (not SO refs) so subscribers in other asmdefs don't
    /// have to take a dependency on Brave.Gameplay.Definitions.
    /// </summary>
    public readonly struct WeaponEvolvedEvent
    {
        /// <summary>Slug of the weapon BEFORE evolution (e.g. "carrot-boomerang").</summary>
        public readonly string baseWeaponId;

        /// <summary>Slug of the evolved weapon (e.g. "harvest-cyclone").</summary>
        public readonly string evolvedWeaponId;

        /// <summary>Slug of the charm consumed (ADR-0007); empty when consumeCharm=false.</summary>
        public readonly string consumedCharmId;

        /// <summary>Whether the charm was actually consumed (ADR-0007: always true at launch).</summary>
        public readonly bool charmConsumed;

        /// <summary>Run-clock seconds at the moment of evolution.</summary>
        public readonly float runSeconds;

        public WeaponEvolvedEvent(string baseWeaponId, string evolvedWeaponId,
            string consumedCharmId, bool charmConsumed, float runSeconds)
        {
            this.baseWeaponId = baseWeaponId;
            this.evolvedWeaponId = evolvedWeaponId;
            this.consumedCharmId = consumedCharmId;
            this.charmConsumed = charmConsumed;
            this.runSeconds = runSeconds;
        }
    }

    /// <summary>SO channel — designers wire this into WeaponEvolutionService + listeners.</summary>
    [CreateAssetMenu(menuName = "Brave/Events/WeaponEvolved", fileName = "WeaponEvolvedChannel", order = 7)]
    public sealed class WeaponEvolvedChannel : EventChannel<WeaponEvolvedEvent> { }
}
