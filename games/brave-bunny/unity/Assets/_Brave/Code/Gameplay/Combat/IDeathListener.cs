#nullable enable
// ADR-0019 item 3: enemy death → pool return wiring.
// This interface is the Combat-namespace bridge so Pooling components can respond to deaths
// without a direct coupling to Brave.Gameplay.Enemies internals.

using UnityEngine;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Listener notified when an entity's HP reaches zero. Implement on a MonoBehaviour
    /// attached to an enemy GameObject to react to death without coupling to the specific
    /// HP system. Pool-return components are the primary consumers.
    /// </summary>
    public interface IDeathListener
    {
        /// <summary>Called exactly once when the entity dies (HP ≤ 0). Never called for
        /// already-dead entities (idempotency is enforced by <see cref="DamageApplier"/>).
        /// </summary>
        void OnDeath(GameObject entity);
    }
}
