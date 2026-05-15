#nullable enable
// Brave Bunny — Hit Feedback Juice
// ADR-0003: hitstop timings are canonical and live in data/balance/feel.json. The
// per-trigger lookup table is the existing <see cref="Brave.Gameplay.Damage.FeelDefinition"/>.
//
// This <see cref="FeelConfig"/> is a *companion* SO holding the scalar knobs used by
// the four runtime juice services (Hitstop / HitFlash / DamageNumber / ScreenShake)
// that don't fit the per-trigger table — flash duration, damage-number lifetime,
// and the low/med/high screenshake amplitude buckets.
//
// Per CLAUDE.md principle 6 (no magic numbers): every numeric value in the four
// feedback services comes from this asset; defaults are seeded from feel.json so
// the SO can be re-imported by Editor.BalanceJsonImporter without drift.

using UnityEngine;

namespace Brave.Gameplay.Feel
{
    /// <summary>
    /// Companion ScriptableObject for hit-feedback juice. Sourced from
    /// <c>data/balance/feel.json</c>. See file header for relationship to
    /// <c>FeelDefinition</c> (per-trigger lookup) — both are read at boot.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Feel/FeelConfig", fileName = "FeelConfig", order = 10)]
    public sealed class FeelConfig : ScriptableObject
    {
        // ---- Hitstop (scalar shortcut for the non-per-trigger callers) ----

        [Header("Hitstop (ms) — per-trigger values live in FeelDefinition")]
        [Tooltip("Default hitstop window in ms used when caller has no specific trigger context. " +
                 "Sourced from feel.json hitstop.basic_enemy_kill_ms.")]
        public float hitstopMs = 20f;

        [Tooltip("Time-scale value held during hitstop. 0 = full freeze; >0 = slow-mo. " +
                 "ADR-0003 specifies full freeze (Time.timeScale = 0) for hitstop windows.")]
        [Range(0f, 1f)] public float hitstopTimeScale;

        // ---- Hit flash ----

        [Header("Hit flash (white-tint on hurt) — MaterialPropertyBlock")]
        [Tooltip("Flash duration in ms — how long the enemy material flashes white on damage.")]
        public float flashMs = 60f;

        [Tooltip("Flash color applied during the flash window. White by default.")]
        public Color flashColor = Color.white;

        [Tooltip("Material property name to tint. _BaseColor for URP Lit / Toon; legacy uses _Color.")]
        public string flashColorPropertyName = "_BaseColor";

        // ---- Damage numbers ----

        [Header("Floating damage numbers")]
        [Tooltip("Lifetime in seconds before a damage number returns to the pool.")]
        public float dmgNumberLifetime = 0.6f;

        [Tooltip("Vertical float distance in world units over the lifetime.")]
        public float dmgNumberFloatHeight = 0.75f;

        [Tooltip("Color for normal (non-crit) damage on enemies.")]
        public Color dmgNumberColorNormal = Color.white;

        [Tooltip("Color for critical-hit damage.")]
        public Color dmgNumberColorCrit = new Color(1f, 0.85f, 0.2f, 1f);

        [Tooltip("Color for damage taken by the player (red feedback).")]
        public Color dmgNumberColorPlayerHit = new Color(1f, 0.25f, 0.25f, 1f);

        [Tooltip("Random horizontal jitter in world units applied to spawn position (so stacked hits don't overlap).")]
        public float dmgNumberJitter = 0.25f;

        // ---- Screen shake ----

        [Header("Screen shake (screen-fraction amplitude) — sourced from feel.json screen_shake")]
        [Tooltip("Low-intensity shake (basic enemy kill). Sourced from feel.json basic_kill_amp.")]
        public float screenshakeAmpLow = 0.05f;

        [Tooltip("Medium-intensity shake (elite kill / player hit). Sourced from feel.json elite_kill_amp.")]
        public float screenshakeAmpMed = 0.15f;

        [Tooltip("High-intensity shake (boss phase change / boss kill). Sourced from feel.json boss_phase_change_amp.")]
        public float screenshakeAmpHigh = 0.35f;

        [Tooltip("Default shake duration in seconds for the Low/Med/High triggers.")]
        public float screenshakeDurationSeconds = 0.18f;

        [Tooltip("Shake frequency (oscillations per second).")]
        public float screenshakeFrequencyHz = 35f;

        // ---- Combo / kill-streak (Wave 10) ----

        [Header("Combo / kill-streak — Wave 10")]
        [Tooltip("Rolling window in seconds: kills landing inside this window since the previous " +
                 "kill extend the streak. If no kill arrives before the window expires, the " +
                 "streak breaks and resets to zero.")]
        public float comboWindowSeconds = 2.0f;

        [Tooltip("Streak count at which tier-1 styling (silver) activates on the combo badge.")]
        public int comboTier1Threshold = 3;

        [Tooltip("Streak count at which tier-2 styling (gold) activates on the combo badge.")]
        public int comboTier2Threshold = 5;

        [Tooltip("Streak count at which tier-3 styling (rainbow) activates on the combo badge.")]
        public int comboTier3Threshold = 10;

        [Tooltip("Fade-out delay (seconds) before the combo badge hides after a streak break.")]
        public float comboFadeOutSeconds = 0.5f;

        /// <summary>Hitstop duration in seconds (convenience for callers that work in seconds).</summary>
        public float HitstopSeconds => hitstopMs * 0.001f;

        /// <summary>Hit-flash duration in seconds.</summary>
        public float FlashSeconds => flashMs * 0.001f;
    }
}
