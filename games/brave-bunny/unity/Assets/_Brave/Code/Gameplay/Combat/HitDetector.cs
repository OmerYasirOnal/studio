// Tech-spec 05: spatial hash broadphase + per-projectile narrowphase. NO Unity Physics on swarmers.
using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;
using System.Collections.Generic;
using UnityEngine;

namespace Brave.Gameplay.Combat;

/// <summary>
/// Spatial-hash broadphase + radial-overlap narrowphase. 200 enemies + 50 projectiles target
/// budget 2.5 ms on iPhone 12 (tech-spec 05). Burst-friendly: per-frame queries fill a
/// pre-allocated <see cref="_hitBuffer"/> rather than allocating new lists.
/// </summary>
public sealed class HitDetector : MonoBehaviour
{
    [SerializeField] private float _cellSize = 2.0f;
    [SerializeField] private int _expectedEnemies = 200;

    private readonly Dictionary<(int, int), List<Enemy>> _cells = new(capacity: 256);
    private readonly List<Enemy> _hitBuffer = new(capacity: 32);

    public void RegisterEnemy(Enemy e)
    {
        var key = CellOf(e.transform.position);
        if (!_cells.TryGetValue(key, out var list)) { list = new List<Enemy>(8); _cells[key] = list; }
        list.Add(e);
    }

    public void UnregisterEnemy(Enemy e)
    {
        var key = CellOf(e.transform.position);
        if (_cells.TryGetValue(key, out var list)) list.Remove(e);
    }

    /// <summary>
    /// Fills the shared <see cref="_hitBuffer"/> with enemies within <paramref name="radius"/> of <paramref name="center"/>.
    /// Caller must not retain the returned list across frames.
    /// </summary>
    public IReadOnlyList<Enemy> QueryRadius(Vector2 center, float radius)
    {
        _hitBuffer.Clear();
        int cellsRadius = Mathf.CeilToInt(radius / _cellSize);
        var cc = CellOf(center);
        float sqr = radius * radius;
        for (int dx = -cellsRadius; dx <= cellsRadius; dx++)
        for (int dy = -cellsRadius; dy <= cellsRadius; dy++)
        {
            if (!_cells.TryGetValue((cc.Item1 + dx, cc.Item2 + dy), out var list)) continue;
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var e = list[i];
                if (e == null) continue;
                Vector2 p = e.transform.position;
                if ((p - center).sqrMagnitude <= sqr) _hitBuffer.Add(e);
            }
        }
        return _hitBuffer;
    }

    /// <summary>Per-projectile narrowphase check + damage application.</summary>
    public void CheckProjectile(Projectile p)
    {
        var hits = QueryRadius(p.transform.position, 0.3f);  // narrowphase radius — projectile half-extent
        for (int i = 0, n = hits.Count; i < n; i++)
        {
            var enemy = hits[i];
            enemy.ApplyHit(new HitContext(
                sourceId: p.SourceWeaponId,
                targetId: enemy.GetInstanceID(),
                amount: p.Damage,
                isCrit: false,
                isKillingBlow: false,
                hitPoint: p.transform.position,
                type: DamageType.Kinetic));
            p.NotifyHit();
            if (p.PierceRemaining <= 0) break;
        }
    }

    private (int, int) CellOf(Vector2 pos) =>
        (Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.y / _cellSize));
}
