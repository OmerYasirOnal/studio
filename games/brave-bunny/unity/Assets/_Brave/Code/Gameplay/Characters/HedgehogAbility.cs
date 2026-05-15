#nullable enable
// Wave 10 — Hedgehog passive ability "Quills": 5% reflected damage on hit (thorns).
// Magnitude sourced from characters.json: characters[id=hedgehog].ability.reflect_pct.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.quills")]
    public sealed class HedgehogAbility : CharacterAbility
    {
        public override string AbilityId => "quills";

        /// <summary>Fraction (0..1) of incoming damage reflected to the attacker.</summary>
        public float ReflectFraction = 0.05f;

        /// <summary>Reflected damage for a given incoming hit. Crit / armour ignored — straight passthrough.</summary>
        public float ComputeReflectedDamage(float incomingDamage)
            => incomingDamage > 0f ? incomingDamage * ReflectFraction : 0f;
    }
}
