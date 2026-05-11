// Boss base — owns the phase machine. Phase transitions raise BossPhaseChannel.
// Hitstop on phase change driven by FeelDefinition.bossPhaseChangeMs (ADR-0003).
using Brave.Gameplay.Damage;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Events;
using UnityEngine;

namespace Brave.Gameplay.Enemies;

public abstract class BossBase : MonoBehaviour
{
    [SerializeField] protected BossDefinition _definition;
    [SerializeField] protected BossPhaseChannel _phaseChannel;
    [SerializeField] protected FeelDefinition _feel;

    public float Hp { get; protected set; }
    public float MaxHp { get; protected set; }
    public int CurrentPhase { get; protected set; } = 1;
    public BossDefinition Definition => _definition;

    public virtual void Spawn(BossDefinition def, float scaledHp)
    {
        _definition = def;
        MaxHp = scaledHp;
        Hp = scaledHp;
        CurrentPhase = 1;
        OnPhaseChanged(1);
    }

    public void ApplyHit(in HitContext hit)
    {
        if (Hp <= 0f) return;
        Hp -= hit.amount;
        CheckPhaseGate();
        if (Hp <= 0f) Die();
    }

    private void CheckPhaseGate()
    {
        float hpPct = Hp / MaxHp;
        for (int i = CurrentPhase; i < _definition.phases.Length; i++)
        {
            float gate = _definition.phases[i].hpGatePercent;
            if (hpPct <= gate)
            {
                CurrentPhase = i + 1;
                _phaseChannel?.Raise(new BossPhaseEvent(CurrentPhase, _definition.slug.GetHashCode()));
                OnPhaseChanged(CurrentPhase);
            }
        }
    }

    protected abstract void OnPhaseChanged(int newPhase);

    private void Die()
    {
        // TODO(Phase 5): boss-kill hitstop (250 ms) + time-dilate ceremony from FeelDefinition.
        gameObject.SetActive(false);
    }
}
