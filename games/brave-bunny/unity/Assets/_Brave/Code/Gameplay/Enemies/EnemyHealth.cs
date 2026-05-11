#nullable enable
// Tech-spec 09 Tier-2: TakeHit is a direct-method call (no event bus). Death raises an
// EnemyKilledEvent through the channel that UI + analytics + drops listen to.

using System;
using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Damage;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Per-enemy HP component. Maintains hp/maxHp, applies the hit reaction, and notifies
    /// pre-registered listeners on death. Listener list is pre-allocated; zero allocations
    /// in the hot loop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHealth : MonoBehaviour
    {
        private readonly List<IDeathListener> _deathListeners = new(capacity: 4);

        private float _hp;
        private float _maxHp;
        private bool _alive;
        private EnemyBase? _enemy;

        public float Hp => _hp;
        public float MaxHp => _maxHp;
        public bool IsAlive => _alive;

        private void Awake() => _enemy = GetComponent<EnemyBase>();

        /// <summary>Resets HP on dequeue from pool. Called by EnemyBase.Configure.</summary>
        public void Reset(float maxHp)
        {
            _maxHp = maxHp;
            _hp = maxHp;
            _alive = true;
        }

        public void RegisterDeathListener(IDeathListener listener) => _deathListeners.Add(listener);
        public void UnregisterDeathListener(IDeathListener listener) => _deathListeners.Remove(listener);

        /// <summary>Apply a hit. Cleared by the projectile/weapon code (no Unity collider events).</summary>
        public void TakeHit(in HitInfo info)
        {
            if (!_alive) return;

            _hp -= info.amount;
            if (_hp <= 0f) Die(info);
            else PlayHitReaction(info);
        }

        private void PlayHitReaction(in HitInfo info)
        {
            // Stub — qa-engineer adds VFX hit-flash here. No allocations.
        }

        private void Die(in HitInfo info)
        {
            _alive = false;
            for (int i = 0, n = _deathListeners.Count; i < n; i++)
                _deathListeners[i].OnEnemyDied(_enemy!, info);
            // Pool return is owned by the death-listener chain (so drops can spawn first).
        }
    }

    /// <summary>Direct-method listener (Tech-spec 09 Tier-2 pattern).</summary>
    public interface IDeathListener
    {
        void OnEnemyDied(EnemyBase enemy, in HitInfo finalHit);
    }
}
