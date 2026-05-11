#nullable enable
// Tech-spec 05 § Collision (200 enemies): spatial-hash broadphase. EnemyRegistry holds a
// flat list of active enemies; SnapshotActiveInRange copies into caller-supplied buffer
// (no allocations in hot path).

using System.Collections.Generic;

using UnityEngine;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Flat list of active enemies. Targeting + collision broadphase queries iterate
    /// this list. Real implementation upgrades to a uniform spatial hash in Phase 5;
    /// this stub is correct but O(N) and sufficient for 200 enemies (per perf budget).
    /// </summary>
    public static class EnemyRegistry
    {
        private static readonly List<EnemyBase> _active = new(capacity: 256);

        public static IReadOnlyList<EnemyBase> Active => _active;

        public static void Register(EnemyBase enemy)
        {
            _active.Add(enemy);
        }

        public static void Unregister(EnemyBase enemy)
        {
            // Swap-remove for O(1) without preserving order.
            int idx = _active.IndexOf(enemy);
            if (idx < 0) return;
            int last = _active.Count - 1;
            _active[idx] = _active[last];
            _active.RemoveAt(last);
        }

        /// <summary>Caller supplies a scratch buffer; we clear and fill it. Zero allocations.</summary>
        public static void SnapshotActiveInRange(Vector3 origin, float radius, List<EnemyBase> outBuffer)
        {
            outBuffer.Clear();
            float r2 = radius * radius;
            for (int i = 0, n = _active.Count; i < n; i++)
            {
                var e = _active[i];
                if (!e.Health.IsAlive) continue;
                Vector3 d = e.transform.position - origin;
                if (d.x * d.x + d.y * d.y <= r2) outBuffer.Add(e);
            }
        }

        public static EnemyBase? FindFirstWithinRadius(Vector3 origin, float radius)
        {
            float r2 = radius * radius;
            for (int i = 0, n = _active.Count; i < n; i++)
            {
                var e = _active[i];
                if (!e.Health.IsAlive) continue;
                Vector3 d = e.transform.position - origin;
                if (d.x * d.x + d.y * d.y <= r2) return e;
            }
            return null;
        }

        public static void ResetAll() => _active.Clear();
    }
}
