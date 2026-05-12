#nullable enable
// Tech-spec 02 § CharacterDefinition. Implements ADR-0001 (defaultStarterWeapon is a default,
// not a lock) and ADR-0009 (signatureMechanicTypeName resolves via MechanicRegistry).

using System;

using UnityEngine;

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Designer-facing static config for one playable character. Mirrors GDD 03-characters.md
    /// and is generated from <c>data/balance/characters.json</c> by <c>make_so_stubs.py</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Character", fileName = "Character", order = 0)]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string slug = string.Empty;                 // kebab-case e.g. "bunny"
        public string displayName = string.Empty;          // LocalizedString in production
        public Sprite? portrait;                           // 256x256 UI portrait
        public GameObject? prefab;                         // hero prefab incl. anim + collider

        [Header("Loadout (ADR-0001)")]
        public WeaponDefinition? defaultStarterWeapon;     // default only — universal pool

        [Header("Stats — sourced from characters.json")]
        public CharacterStats baseStats = new CharacterStats();

        [Header("Signature mechanic (ADR-0009)")]
        public string signatureMechanicTypeName = string.Empty;  // resolves via MechanicRegistry

        [Header("Progression")]
        public AnimationCurve levelCurveXp = AnimationCurve.Linear(1f, 100f, 30f, 3000f);
        public int maxLevel = 30;
        public int unlockStarCost;                         // 0 for starter (Bunny)

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(slug))
                Debug.LogError($"{name}: slug required", this);
            if (maxLevel != 30)
                Debug.LogError($"{slug}: maxLevel must be 30 (character progression cap)", this);
            if (baseStats.baseHP <= 0f)
                Debug.LogError($"{slug}: baseHP must be > 0", this);
        }
    }

}
// CharacterStats lives in its own file: Definitions/CharacterStats.cs
