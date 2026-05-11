// Tech-spec 05: NO per-enemy MonoBehaviour Update — AI ticks at 30 Hz from a central job.
// This class holds state + responds to Tick() calls from EnemyTicker.
using Brave.Gameplay.Damage;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Enemies;

/// <summary>
/// Base enemy MonoBehaviour. Holds HP, contact-damage timer, behaviour strategy reference,
/// and pool ownership. Ticks driven externally for budget control.
/// </summary>
public sealed class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private EnemyDefinition _definition;
    private EnemyBehavior _behavior;
    private EnemyPool _owner;

    public float Hp { get; private set; }
    public float MaxHp { get; private set; }
    public EnemyDefinition Definition => _definition;
    public bool IsAlive => Hp > 0f;

    public void Configure(EnemyDefinition def, float scaledHp, EnemyBehavior behavior, EnemyPool owner)
    {
        _definition = def;
        MaxHp = scaledHp;
        Hp = scaledHp;
        _behavior = behavior;
        _owner = owner;
    }

    public void Acquire() { /* state reset in Configure */ }
    public void Release() { _definition = null; _behavior = null; _owner = null; Hp = 0f; }

    public void ApplyHit(in HitContext hit)
    {
        if (!IsAlive) return;
        Hp -= hit.amount;
        if (Hp <= 0f) Die(hit);
    }

    private void Die(in HitContext hit)
    {
        // TODO(Phase 5): raise EnemyKilledChannel, drop pickups via PickupPool, hitstop via Hitstop.
        if (_owner != null) _owner.Release(this);
        else gameObject.SetActive(false);
    }

    /// <summary>Called from <c>EnemyTicker</c> at 30 Hz with the player position.</summary>
    public void Tick(Vector2 playerPos, float dt)
    {
        if (!IsAlive || _behavior == null) return;
        _behavior.Tick(this, playerPos, dt);
    }
}
