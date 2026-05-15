#nullable enable
// Wave 10 — Owl passive ability "Foresight": +1 reroll on level-up.
// Magnitude sourced from characters.json: characters[id=owl].ability.bonus_rerolls.
//
// Out-of-scope: LevelUpDraftController owns the reroll UI (UI agent). This ability
// only contributes an integer "bonus reroll" count that the draft controller reads
// off the active CharacterAbility (or off RunController via the ability's getter).

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.foresight")]
    public sealed class OwlAbility : CharacterAbility
    {
        public override string AbilityId => "foresight";

        /// <summary>Extra rerolls granted on each level-up draft.</summary>
        public int BonusRerollsPerLevelUp = 1;
    }
}
