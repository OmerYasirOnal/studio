// Spawns a single enemy from a pool at a position. Stateless helper.
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Spawning;

public sealed class Spawner
{
    private readonly System.Collections.Generic.Dictionary<string, EnemyPool> _poolsBySlug;

    public Spawner(System.Collections.Generic.Dictionary<string, EnemyPool> poolsBySlug)
        => _poolsBySlug = poolsBySlug;

    /// <summary>Spawn one enemy of <paramref name="def"/> at <paramref name="position"/>.</summary>
    public Enemy Spawn(EnemyDefinition def, Vector2 position, float runMinutes, EnemyBehavior behavior)
    {
        if (def == null) return null;
        if (!_poolsBySlug.TryGetValue(def.slug, out var pool))
        {
            Debug.LogError($"Spawner: no pool for enemy slug '{def.slug}'");
            return null;
        }
        float scaledHp = def.baseHP + def.hpPerMinute * Mathf.Max(0f, runMinutes - 1f);
        var enemy = pool.Acquire(position);
        enemy.Configure(def, scaledHp, behavior, pool);
        return enemy;
    }
}
