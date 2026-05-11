// characters.json hedgehog.signature: passive ring of damage every 3s, radius 1.5, dmg_mult_per_tick 0.5.
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("thorn-ring")]
public sealed class HedgehogThornRing : SignatureMechanic
{
    public override string TypeName => "thorn-ring";

    private const float TickIntervalSeconds = 3f;     // characters.json tick_interval_ms
    private const float RadiusUnits = 1.5f;           // characters.json radius_units
    private const float DmgMultPerTick = 0.5f;        // characters.json dmg_mult_per_tick

    private float _nextTickAt;

    public override void OnAttach(PlayerContext ctx) { _nextTickAt = TickIntervalSeconds; }
    public override void OnDetach(PlayerContext ctx) { }

    public override void Tick(PlayerContext ctx, float dt)
    {
        if (ctx.runSeconds < _nextTickAt) return;
        _nextTickAt = ctx.runSeconds + TickIntervalSeconds;
        // TODO(Phase 5): HitDetector.QueryRadius around player; apply damage scaled by DmgMultPerTick × characterDmgMult.
    }
}
