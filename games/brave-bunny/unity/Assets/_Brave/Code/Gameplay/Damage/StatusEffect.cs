namespace Brave.Gameplay.Damage;

/// <summary>Base for time-bounded debuffs (slow, dot, shield, frostbite).</summary>
public abstract class StatusEffect
{
    public float DurationRemaining { get; protected set; }
    public bool IsExpired => DurationRemaining <= 0f;

    protected StatusEffect(float durationSeconds) => DurationRemaining = durationSeconds;

    /// <summary>Advance the effect by <paramref name="dt"/> seconds. Implementer applies side-effects.</summary>
    public abstract void Tick(float dt);
}

public sealed class SlowEffect : StatusEffect
{
    public float SlowPercent { get; }
    public SlowEffect(float slowPercent, float durationSeconds) : base(durationSeconds)
        => SlowPercent = slowPercent;
    public override void Tick(float dt) => DurationRemaining -= dt;
}

public sealed class DotEffect : StatusEffect
{
    public float DamagePerSecond { get; }
    public DotEffect(float dps, float durationSeconds) : base(durationSeconds)
        => DamagePerSecond = dps;
    public override void Tick(float dt) => DurationRemaining -= dt;
    public float TickDamage(float dt) => DamagePerSecond * dt;
}

public sealed class ShieldEffect : StatusEffect
{
    public float HpRemaining { get; private set; }
    public ShieldEffect(float hp, float durationSeconds) : base(durationSeconds) => HpRemaining = hp;
    public override void Tick(float dt) => DurationRemaining -= dt;

    /// <summary>Returns the leftover damage that bleeds through after the shield absorbs.</summary>
    public float Absorb(float incoming)
    {
        if (incoming <= HpRemaining) { HpRemaining -= incoming; return 0f; }
        float leftover = incoming - HpRemaining;
        HpRemaining = 0f;
        DurationRemaining = 0f;
        return leftover;
    }
}
