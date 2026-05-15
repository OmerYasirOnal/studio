#nullable enable
// Wave 10 — Panda passive ability "Restore": regen 1HP/s when out of combat (no kill for 3s).
// Magnitudes sourced from characters.json: characters[id=panda].ability.{regen_hp_per_sec,out_of_combat_seconds}.

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    [BraveRegister("ability.restore")]
    public sealed class PandaAbility : CharacterAbility
    {
        public override string AbilityId => "restore";

        /// <summary>HP regenerated per second while out of combat.</summary>
        public float RegenHpPerSecond = 1.0f;

        /// <summary>Seconds since last kill required to enter "out of combat".</summary>
        public float OutOfCombatSeconds = 3.0f;

        /// <summary>Seconds since last kill. Reset to 0 via <see cref="NotifyKill"/>.</summary>
        public float TimeSinceLastKill { get; private set; }

        /// <summary>Accumulated HP debt for the integer pickup pipeline. Allows fractional regen.</summary>
        public float AccumulatedRegen { get; private set; }

        public bool IsOutOfCombat => TimeSinceLastKill >= OutOfCombatSeconds;

        public override void OnActivate(IRunContext ctx)
        {
            base.OnActivate(ctx);
            TimeSinceLastKill = 0f;
            AccumulatedRegen = 0f;
        }

        public override void OnTick(float dt)
        {
            TimeSinceLastKill += dt;
            if (IsOutOfCombat)
            {
                AccumulatedRegen += RegenHpPerSecond * dt;
            }
        }

        /// <summary>Call from the kill-event pipeline to reset the out-of-combat timer.</summary>
        public void NotifyKill()
        {
            TimeSinceLastKill = 0f;
        }

        /// <summary>Drain accumulated regen for application to current HP. Returns whole HP integer drained.</summary>
        public int ConsumeWholeRegenTicks()
        {
            int whole = (int)AccumulatedRegen;
            AccumulatedRegen -= whole;
            return whole;
        }
    }
}
