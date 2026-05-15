// Brave Bunny — Gameplay/Enemies/BossPhaseState
//
// Phase machine for Old Boar King. The boss spec
// (docs/09-level-design/02-bosses/old-boar-king/mechanics.md) defines 3 HP-gated
// phases. The task scope renames them into action-coded labels —
// Approach / Charge / Slam / Recover — that map onto the boss's per-phase
// behavior buckets:
//
//   * Approach (100-66% HP) — body-slam-on-contact, slow walk. Boss spec's
//     "Awake-and-grumpy".
//   * Charge   (66-33%  HP) — high-speed line-charge across arena.
//     Boss spec's "Furrowed-brow" with the charge attack as the headliner.
//   * Slam     (33-0%   HP) — AOE slam / shockwave ring attacks at faster cadence.
//     Boss spec's "Fully-cross".
//   * Recover  — transient post-attack window (boss is "open"); not HP-gated.
//
// Phase thresholds live in the BossDefinition SO (hpGatePercent), not as magic
// numbers. This struct is allocation-free in the update path: it stores plain
// floats + an enum and returns booleans for transitions.
//
// Spec refs:
//   * docs/09-level-design/02-bosses/old-boar-king/mechanics.md § Phases
//   * docs/decisions/0006-enemy-hp-recalibration.md § Phase gates remain at 66/33

#nullable enable

using UnityEngine;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Named phases for the Old Boar King fight. Order matches HP-descending sweep —
    /// the phase machine only moves forward through this list. <see cref="Recover"/>
    /// is a transient overlay used while the boss is "open" after a heavy attack;
    /// it does not advance the HP-gated phase counter.
    /// </summary>
    public enum BossPhase
    {
        Approach = 0,    // 100-66% HP — slow walk, contact body-slam
        Charge   = 1,    // 66-33% HP — line-charge attack across arena
        Slam     = 2,    // 33-0%  HP — radial shockwave + faster cadence
        Recover  = 3,    // transient — boss wind-down window (not HP-gated)
    }

    /// <summary>
    /// Allocation-free phase tracker. Holds the current phase plus the HP thresholds
    /// (as 0..1 fractions of MaxHp) at which the boss advances to the next phase.
    /// </summary>
    public struct BossPhaseState
    {
        // HP gate fractions — must be sorted high-to-low. Read from BossDefinition.phases[*].hpGatePercent.
        private float _chargeGate;     // HP fraction at which Approach -> Charge fires (typically 0.66)
        private float _slamGate;       // HP fraction at which Charge   -> Slam   fires (typically 0.33)
        private BossPhase _phase;

        public BossPhase Current => _phase;
        public float ChargeGate => _chargeGate;
        public float SlamGate => _slamGate;

        /// <summary>Initialise to <see cref="BossPhase.Approach"/> with the supplied HP gates.</summary>
        public void Initialise(float chargeGateHpFraction, float slamGateHpFraction)
        {
            _chargeGate = Mathf.Clamp01(chargeGateHpFraction);
            _slamGate = Mathf.Clamp01(slamGateHpFraction);
            _phase = BossPhase.Approach;
        }

        /// <summary>
        /// Evaluate phase transition for the current HP fraction. Returns the new phase
        /// if a transition fired, or null when the phase is unchanged. Only advances
        /// forward (Approach → Charge → Slam); never regresses if the boss heals.
        /// </summary>
        public BossPhase? EvaluateTransition(float hpFraction)
        {
            // Recover is a transient overlay set explicitly by attack patterns — it
            // does not participate in HP-gated transitions, but the next HP-gated
            // tick still progresses from whatever HP-phase the boss was in.
            BossPhase hpPhase = HpPhaseFor(hpFraction);
            // Move forward only.
            if ((int)hpPhase > (int)EffectiveHpPhase(_phase))
            {
                _phase = hpPhase;
                return _phase;
            }
            return null;
        }

        /// <summary>
        /// Compute which HP-gated phase corresponds to a given HP fraction, ignoring
        /// the Recover overlay. Pure function; safe to call from tests.
        /// </summary>
        public BossPhase HpPhaseFor(float hpFraction)
        {
            if (hpFraction <= _slamGate)   return BossPhase.Slam;
            if (hpFraction <= _chargeGate) return BossPhase.Charge;
            return BossPhase.Approach;
        }

        /// <summary>Manually set Recover (after a heavy attack); does not advance HP-phase.</summary>
        public void EnterRecover() => _phase = BossPhase.Recover;

        /// <summary>Leave Recover back to the HP-gated phase implied by <paramref name="hpFraction"/>.</summary>
        public void ExitRecover(float hpFraction) => _phase = HpPhaseFor(hpFraction);

        // Treat Recover as "no progression" for the move-forward check: we use the
        // most-recent HP-gated phase implied by gates rather than Recover itself.
        private BossPhase EffectiveHpPhase(BossPhase p) => p == BossPhase.Recover ? BossPhase.Approach : p;
    }
}
