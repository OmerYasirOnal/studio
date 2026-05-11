// characters.json panda.signature: xp-gem-pickup → +1 hp (capped at max).
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Events;

namespace Brave.Gameplay.Characters;

[BraveRegister("hearty-snack")]
public sealed class PandaHealOnPickup : SignatureMechanic
{
    public override string TypeName => "hearty-snack";

    public const int HealHp = 1;       // characters.json heal_hp

    private PickupChannel _pickupChannel;
    private PlayerContext _ctx;

    public override void OnAttach(PlayerContext ctx)
    {
        _ctx = ctx;
        // TODO(Phase 5): pull PickupChannel from ctx.services (GameContext.Get<PickupChannel>) and subscribe to OnPickup.
    }

    public override void OnDetach(PlayerContext ctx) { _ctx = null; _pickupChannel = null; }
    public override void Tick(PlayerContext ctx, float dt) { /* event-driven */ }

    private void OnPickup(PickupEvent e)
    {
        if (_ctx == null) return;
        if (e.kind != PickupKind.XpGemSmall && e.kind != PickupKind.XpGemMedium && e.kind != PickupKind.XpGemLarge) return;
        float max = _ctx.stats.baseHP;
        _ctx.currentHp = UnityEngine.Mathf.Min(max, _ctx.currentHp + HealHp);
    }
}
