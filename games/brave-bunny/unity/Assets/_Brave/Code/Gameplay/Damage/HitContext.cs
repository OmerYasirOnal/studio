using UnityEngine;

namespace Brave.Gameplay.Damage;

/// <summary>
/// Per-hit context bundle. Pass-by-value (readonly struct) — zero GC per tech-spec 05.
/// </summary>
public readonly struct HitContext
{
    public readonly int sourceId;       // weapon instance id
    public readonly int targetId;       // enemy id (slot in spatial-hash buffer)
    public readonly float amount;       // final damage (after formula)
    public readonly bool isCrit;
    public readonly bool isKillingBlow;
    public readonly Vector3 hitPoint;
    public readonly DamageType type;

    public HitContext(int sourceId, int targetId, float amount, bool isCrit, bool isKillingBlow, Vector3 hitPoint, DamageType type)
    {
        this.sourceId = sourceId;
        this.targetId = targetId;
        this.amount = amount;
        this.isCrit = isCrit;
        this.isKillingBlow = isKillingBlow;
        this.hitPoint = hitPoint;
        this.type = type;
    }
}

public enum DamageType { Kinetic, Nature, Solar, Frost, Aura, Summon, Explosive }
