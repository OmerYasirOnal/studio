// characters.json fox.signature: kills under 25% hp trigger 3x dmg strike (with chain).
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters;

[BraveRegister("cunning-strike")]
public sealed class FoxExec : SignatureMechanic
{
    public override string TypeName => "cunning-strike";

    public const float TriggerHpFraction = 0.25f;     // characters.json enemy-below-25pct-hp
    public const float ExecDmgMult = 3.0f;            // characters.json exec_dmg_mult
    public const int ChainMax = 5;                    // characters.json chain_max
    public const float ChainWindowSeconds = 1.5f;     // characters.json chain_window_ms

    public override void OnAttach(PlayerContext ctx) { }
    public override void OnDetach(PlayerContext ctx) { }
    public override void Tick(PlayerContext ctx, float dt)
    {
        // TODO(Phase 5): hook into HitDetector pre-damage callback — multiply damage by ExecDmgMult
        // when target.Hp < target.MaxHp * TriggerHpFraction; on kill, start chain window allowing
        // up to ChainMax additional triggers within ChainWindowSeconds.
    }
}
