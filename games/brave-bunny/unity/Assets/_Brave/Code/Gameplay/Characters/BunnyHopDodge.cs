// characters.json bunny.signature: every 5th weapon hit → hop with 400ms i-frames, 5s cooldown.
using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Characters;

[BraveRegister("hop-dodge")]
public sealed class BunnyHopDodge : SignatureMechanic
{
    public override string TypeName => "hop-dodge";

    private const float IframeSeconds = 0.4f;       // characters.json iframe_ms
    private const float CooldownSeconds = 5f;       // characters.json cooldown_ms
    private const int TriggerEveryNHits = 5;        // characters.json trigger "every-5th-weapon-hit"

    private int _hitCounter;
    private float _cooldownRemaining;
    private float _iframeRemaining;

    public override void OnAttach(PlayerContext ctx) { _hitCounter = 0; _cooldownRemaining = 0f; }
    public override void OnDetach(PlayerContext ctx) { }

    public override void Tick(PlayerContext ctx, float dt)
    {
        if (_cooldownRemaining > 0f) _cooldownRemaining -= dt;
        if (_iframeRemaining > 0f) _iframeRemaining -= dt;
        // TODO(Phase 5): subscribe to weapon-hit events; increment _hitCounter; trigger hop when reaches 5 and cooldown=0.
    }

    public bool HasIframes => _iframeRemaining > 0f;
}
