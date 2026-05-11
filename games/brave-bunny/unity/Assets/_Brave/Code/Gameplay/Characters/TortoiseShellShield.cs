// characters.json tortoise.signature: hp-below-50pct → shield absorbs 100hp, 8s cooldown.
using Brave.Gameplay.Damage;
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("shell-brace")]
public sealed class TortoiseShellShield : SignatureMechanic
{
    public override string TypeName => "shell-brace";

    private const float ShieldHp = 100f;        // characters.json shield_absorb_hp
    private const float CooldownSeconds = 8f;   // characters.json cooldown_ms
    private const float TriggerHpFraction = 0.5f;

    private float _cooldownRemaining;
    private ShieldEffect _activeShield;

    public override void OnAttach(PlayerContext ctx) { _cooldownRemaining = 0f; _activeShield = null; }
    public override void OnDetach(PlayerContext ctx) { _activeShield = null; }

    public override void Tick(PlayerContext ctx, float dt)
    {
        if (_cooldownRemaining > 0f) _cooldownRemaining -= dt;
        _activeShield?.Tick(dt);

        if (_cooldownRemaining <= 0f && _activeShield == null && ctx.currentHp < ctx.stats.baseHP * TriggerHpFraction)
        {
            _activeShield = new ShieldEffect(ShieldHp, durationSeconds: 99f);
            _cooldownRemaining = CooldownSeconds;
            // TODO(Phase 5): register shield with player damage pipeline so incoming hits absorb first.
        }
    }
}
