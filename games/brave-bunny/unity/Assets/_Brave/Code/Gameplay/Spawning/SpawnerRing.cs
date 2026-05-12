#nullable enable
// Tech-spec 02 § SpawnPattern. Pure utility — computes spawn positions for ring / arc /
// stream / scatter patterns around the hero. Zero allocations.

using System;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Spawning
{
    // Definitions.SpawnPattern is the data-model enum; Spawning.SpawnPattern (sibling) is
    // the runtime-helper enum. C# resolves bare `SpawnPattern` to the same-namespace one,
    // so we fully-qualify the data-model values below.
    using DefPattern = Brave.Gameplay.Definitions.SpawnPattern;

    /// <summary>
    /// Spawn-pattern utility. Given a pattern + count, places N enemies at computed
    /// positions and configures each from the EnemyDefinition.
    /// </summary>
    public static class SpawnerRing
    {
        public static void Spawn(DefPattern pattern, int count, Vector3 anchor, float radius,
            ObjectPool<EnemyBase> pool, EnemyDefinition def, Transform hero)
        {
            if (count <= 0) return;
            if (radius <= 0f) radius = 10f;     // default off-screen ring radius

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = ComputePosition(pattern, i, count, anchor, radius);
                var enemy = pool.Get();
                if (enemy == null) return;       // pool exhausted
                enemy.transform.position = pos;
                enemy.Configure(def, hero, def.baseHP, def.contactDamage, def.moveSpeed);
            }
        }

        private static Vector3 ComputePosition(DefPattern pattern, int index, int total,
            Vector3 anchor, float radius)
        {
            switch (pattern)
            {
                case DefPattern.Ring:
                {
                    float t = (float)index / total;
                    float angle = t * Mathf.PI * 2f;
                    return anchor + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                }
                case DefPattern.Arc:
                {
                    float t = total <= 1 ? 0.5f : (float)index / (total - 1);
                    // 90-degree arc centered on +X
                    float angle = (t - 0.5f) * (Mathf.PI * 0.5f);
                    return anchor + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                }
                case DefPattern.RandomEdge:
                {
                    float a = UnityEngine.Random.value * Mathf.PI * 2f;
                    return anchor + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                }
                case DefPattern.Stream:
                {
                    // Linear stream along +X with small Y jitter
                    float jitter = (index - total * 0.5f) * 0.6f;
                    return anchor + new Vector3(radius, jitter, 0f);
                }
                case DefPattern.Scatter:
                {
                    Vector2 jitter = UnityEngine.Random.insideUnitCircle * radius;
                    return anchor + new Vector3(jitter.x, jitter.y, 0f);
                }
                case DefPattern.ScriptedPoints:
                default:
                    return anchor;
            }
        }
    }
}
