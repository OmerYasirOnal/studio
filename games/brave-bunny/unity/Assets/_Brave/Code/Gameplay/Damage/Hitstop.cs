#nullable enable
// ADR-0003: hitstop timings are canonical. All durations read from data/balance/feel.json
// via FeelDefinition ScriptableObject. NEVER inline ms values here.

using System;

using UnityEngine;

namespace Brave.Gameplay.Damage
{
    public enum HitstopTrigger
    {
        BasicEnemyHit       = 0,
        BasicEnemyKill      = 1,
        EliteHit            = 2,
        EliteKill           = 3,
        BossDamageTick      = 4,
        BossPhaseChange     = 5,
        BossKill            = 6,
    }

    /// <summary>
    /// Designer-facing wrapper around <c>data/balance/feel.json</c>. Loaded into a
    /// ScriptableObject so it can be referenced by <c>[SerializeField]</c> in the inspector.
    /// Per ADR-0003 — these are the canonical lock; do not inline elsewhere.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/Feel", fileName = "Feel", order = 9)]
    public sealed class FeelDefinition : ScriptableObject
    {
        [Header("Hitstop (ms) — ADR-0003 canonical lock")]
        public float basicEnemyHitMs;
        public float basicEnemyKillMs = 20f;
        public float eliteHitMs       = 30f;
        public float eliteKillMs      = 80f;
        public float bossDamageTickMs = 40f;
        public float bossPhaseChangeMs = 150f;
        public float bossKillMs       = 250f;

        [Header("Time dilate (boss phase + entrance + kill)")]
        public float bossPhaseChangeFactor = 0.5f;
        public float bossPhaseChangeDurationMs = 200f;
        public float bossEntranceFactor = 0.4f;
        public float bossEntranceDurationMs = 800f;
        public float bossKillFactor = 0.3f;
        public float bossKillDurationMs = 1200f;

        [Header("Crit PRD window")]
        public int expectedIntervalsWithoutCritBeforeForce = 4;

        [Header("Screen shake (screen fraction)")]
        public float basicKillAmp = 0.05f;
        public float eliteKillAmp = 0.15f;
        public float bossPhaseChangeAmp = 0.35f;
        public float bossKillAmp = 0.50f;

        public float DurationMsFor(HitstopTrigger trigger) => trigger switch
        {
            HitstopTrigger.BasicEnemyHit    => basicEnemyHitMs,
            HitstopTrigger.BasicEnemyKill   => basicEnemyKillMs,
            HitstopTrigger.EliteHit         => eliteHitMs,
            HitstopTrigger.EliteKill        => eliteKillMs,
            HitstopTrigger.BossDamageTick   => bossDamageTickMs,
            HitstopTrigger.BossPhaseChange  => bossPhaseChangeMs,
            HitstopTrigger.BossKill         => bossKillMs,
            _ => 0f,
        };
    }

    /// <summary>
    /// Runtime hitstop applier. Sets <c>Time.timeScale</c> to 0 for the window then restores
    /// it. Single instance per RunController; never re-entered concurrently.
    /// </summary>
    public sealed class Hitstop : MonoBehaviour
    {
        [SerializeField] private FeelDefinition? feel;

        private float _resumeAtUnscaledTime;
        private float _previousTimeScale = 1f;
        private bool _active;

        public void BindFeel(FeelDefinition feelDefinition) => feel = feelDefinition;

        /// <summary>Apply the window for a trigger. Re-applying extends the resume time.</summary>
        public void Apply(HitstopTrigger trigger)
        {
            if (feel == null) return;
            float ms = feel.DurationMsFor(trigger);
            if (ms <= 0f) return;

            float seconds = ms * 0.001f;
            float resumeAt = Time.unscaledTime + seconds;

            if (!_active)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _active = true;
            }

            if (resumeAt > _resumeAtUnscaledTime)
                _resumeAtUnscaledTime = resumeAt;
        }

        private void Update()
        {
            if (!_active) return;
            if (Time.unscaledTime >= _resumeAtUnscaledTime)
            {
                Time.timeScale = _previousTimeScale;
                _active = false;
            }
        }
    }
}
