// characters.json owl.signature: magnet_mult 3.0, xp_gem_value_bonus 0.10 (pure passive carry).
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("far-sight")]
public sealed class OwlMagnet : SignatureMechanic
{
    public override string TypeName => "far-sight";

    // Stats are pre-folded into CharacterStats.magnetMultiplier and CharacterStats.xpGemValueBonus
    // at hero-spawn from characters.json — this class is effectively a marker for the registry.
    public override void OnAttach(PlayerContext ctx) { }
    public override void OnDetach(PlayerContext ctx) { }
    public override void Tick(PlayerContext ctx, float dt) { /* pure passive */ }
}
