// characters.json badger.signature: interval 30s → summon baby-badger companion (0.6× dmg, 60s lifetime, max 3).
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("baby-patrol")]
public sealed class BadgerSummonBaby : SignatureMechanic
{
    public override string TypeName => "baby-patrol";

    private const float IntervalSeconds = 30f;        // characters.json interval_ms
    public const float CompanionDmgMult = 0.6f;       // characters.json companion_dmg_mult
    public const float CompanionLifetimeSeconds = 60f;
    public const int MaxSimultaneous = 3;             // characters.json max_simultaneous

    private float _nextSummonAt;

    public override void OnAttach(PlayerContext ctx) { _nextSummonAt = IntervalSeconds; }
    public override void OnDetach(PlayerContext ctx) { }

    public override void Tick(PlayerContext ctx, float dt)
    {
        if (ctx.runSeconds < _nextSummonAt) return;
        _nextSummonAt = ctx.runSeconds + IntervalSeconds;
        // TODO(Phase 5): summon a baby-badger companion via summon-pool, capped at MaxSimultaneous.
    }
}
