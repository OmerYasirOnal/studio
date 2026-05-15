#nullable enable
// Implements data/balance/00-formulas.md § 1 (damage) and § 2 (crit).
// Pure function — no Unity dependency aside from Mathf.Clamp. Safe for unit tests.

using System;

using UnityEngine;

namespace Brave.Gameplay.Damage
{
    /// <summary>
    /// Per-hit damage calculator. All inputs flow from <c>characters.json</c> + <c>weapons.json</c>
    /// + <c>enemies.json</c> via ScriptableObjects; no magic numbers in this class.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>Hard floor — final damage is at least 1 (per formulas.md § 1).</summary>
        public const float MinDamage = 1f;

        /// <summary>Defense multiplier is clamped to [0, 0.75] (per formulas.md § 1).</summary>
        public const float MaxDefenseMultiplier = 0.75f;

        /// <summary>Crit rate is clamped to [0, 0.95] (per formulas.md § 2).</summary>
        public const float MaxCritRate = 0.95f;

        /// <summary>
        /// damage = base_damage × character_dmg_mult × weapon_level_mult × crit_mult × (1 - defense_mult)
        /// </summary>
        public static float Compute(
            float baseDamage,
            float characterDamageMultiplier,
            float weaponLevelMultiplier,
            float critMultiplier,
            float enemyDefenseMultiplier)
        {
            float defense = Mathf.Clamp(enemyDefenseMultiplier, 0f, MaxDefenseMultiplier);
            float raw = baseDamage
                * characterDamageMultiplier
                * weaponLevelMultiplier
                * critMultiplier
                * (1f - defense);
            return Mathf.Max(raw, MinDamage);
        }

        /// <summary>
        /// Crit roll per formulas.md § 2. PRD layer (force-crit after 4× expected interval)
        /// lives in the calling system, not here — this is a pure stateless roll.
        /// </summary>
        public static bool RollCrit(float critRate, float random01)
        {
            float effective = Mathf.Clamp(critRate, 0f, MaxCritRate);
            return random01 < effective;
        }

        /// <summary>Crit multiplier given crit_damage (default 1.0 = "2x").</summary>
        public static float CritMultiplier(bool isCrit, float critDamage)
            => isCrit ? 1f + critDamage : 1f;

        /// <summary>
        /// Wave 10 — convenience: roll crit + apply multiplier in one stateless call.
        /// Returns the post-crit damage and the crit flag (for <see cref="HitInfo.isCrit"/>
        /// propagation to <c>DamageNumberSpawner</c> + achievement listeners). Allocation-free.
        /// Caller supplies <paramref name="random01"/> so the RNG source stays injectable
        /// (deterministic replay / unit tests) — see formulas.md §2.
        /// </summary>
        public static (float damage, bool isCrit) RollAndApplyCrit(
            float baseDamage,
            float critRate,
            float critDamage,
            float random01)
        {
            bool isCrit = RollCrit(critRate, random01);
            float mult = CritMultiplier(isCrit, critDamage);
            return (baseDamage * mult, isCrit);
        }
    }
}
