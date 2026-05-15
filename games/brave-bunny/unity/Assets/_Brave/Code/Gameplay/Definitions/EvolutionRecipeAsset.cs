#nullable enable
// Wave 9 — Weapon Evolution.
// ScriptableObject wrapper around a single EvolutionRecipe for inspector authoring and
// for BalanceJsonImporter's editor-time generation. Lives in Definitions/ alongside the
// other authored data SOs (CharacterDefinition, PassiveDefinition, …).
//
// One asset per recipe — 8 SOs total at launch — under Assets/_Brave/Data/Definitions/Evolutions/.
// The WeaponEvolutionService loads these via Resources or designer-wired inspector array.

using UnityEngine;
using Brave.Gameplay.Combat.Evolution;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Authored evolution recipe. The contained <see cref="EvolutionRecipe"/> is the
    /// runtime-shaped record (slug-based) consumed by <c>WeaponEvolutionService</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/EvolutionRecipe", fileName = "Evolution", order = 4)]
    public sealed class EvolutionRecipeAsset : ScriptableObject
    {
        [Header("Recipe")]
        public EvolutionRecipe recipe = new EvolutionRecipe();
    }
}
