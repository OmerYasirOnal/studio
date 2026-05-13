#nullable enable
// ADR-0019 item 3: pool-return component wired via IDeathListener.
// Attach to an enemy prefab alongside an Enemy component. On death, returns the enemy
// to its owning EnemyPool so it can be reused without allocation.

using UnityEngine;

using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// MonoBehaviour that listens for the entity's death (via <see cref="IDeathListener"/>)
    /// and releases the <see cref="Enemy"/> component back to its <see cref="EnemyPool"/>.
    ///
    /// Attach to every enemy prefab. The pool reference is injected by the spawner via
    /// <see cref="Initialise"/> at dequeue time. If the pool reference is null the component
    /// falls back to deactivating the GameObject directly (safe degradation, no crash).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyPoolReturnOnDeath : MonoBehaviour, IDeathListener
    {
        private EnemyPool? _pool;
        private Enemy? _enemy;

        /// <summary>Called by the spawner/configure step immediately after dequeue.</summary>
        public void Initialise(EnemyPool pool)
        {
            _pool = pool;
            _enemy = GetComponent<Enemy>();
        }

        /// <inheritdoc/>
        public void OnDeath(GameObject entity)
        {
            if (_pool != null && _enemy != null)
                _pool.Release(_enemy);
            else
                gameObject.SetActive(false);   // safe fallback — no crash if pool is missing

            // Clear refs so the GC can collect the pool if all enemies are returned.
            _pool = null;
            _enemy = null;
        }
    }
}
