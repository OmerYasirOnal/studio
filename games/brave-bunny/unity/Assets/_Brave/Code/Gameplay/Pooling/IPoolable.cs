#nullable enable
// ADR-0005 + tech-spec 05 § Pool pre-warm. Every spawnable obeys this contract.

namespace Brave.Gameplay.Pooling
{
    /// <summary>
    /// Contract for pooled objects. Implementers reset per-acquire state in
    /// <see cref="OnGetFromPool"/> and clear references in <see cref="OnReturnToPool"/>.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object leaves the pool. Reset visible state.</summary>
        void OnGetFromPool();

        /// <summary>Called when the object returns to the pool. Clear references.</summary>
        void OnReturnToPool();
    }
}
