// characters.json otter.signature: passive +1 projectile to projectile-archetype weapons, 20° spread.
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("splash-volley")]
public sealed class OtterMultiShot : SignatureMechanic
{
    public override string TypeName => "splash-volley";

    public const int ExtraProjectiles = 1;            // characters.json extra_projectiles
    public const float SpreadDegrees = 20f;           // characters.json spread_degrees

    public override void OnAttach(PlayerContext ctx) { /* projectile weapons read ExtraProjectiles via PlayerContext */ }
    public override void OnDetach(PlayerContext ctx) { }
    public override void Tick(PlayerContext ctx, float dt) { /* pure passive */ }
}
