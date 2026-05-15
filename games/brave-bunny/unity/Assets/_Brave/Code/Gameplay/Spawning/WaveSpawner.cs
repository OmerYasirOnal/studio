#nullable enable
// Tech-spec 05 § Spawning: drives the WaveDefinition schedule. Per game CLAUDE.md, the
// wave timing is non-negotiable; this code consumes WaveDefinition but never modifies it.

using System;
using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;
using Brave.Gameplay.Run;

namespace Brave.Gameplay.Spawning
{
    /// <summary>
    /// Walks the wave schedule and triggers spawns at the correct run-time minute marks.
    /// Subscribes to <see cref="RunTimer"/> for the run clock so pause/resume is honored.
    /// </summary>
    public sealed class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveDefinition? wave;
        [SerializeField] private RunTimer? runTimer;
        [SerializeField] private Transform? hero;

        // ADR-0020: GDD specifies one boss per biome at a time. Capacity 1
        // suffices and avoids over-allocating pools for a singleton encounter.
        [SerializeField] private int bossCapacity = 1;

        private int _nextEventIndex;
        private readonly Dictionary<EnemyDefinition, ObjectPool<EnemyBase>> _enemyPools = new(16);

        /// <summary>Pre-warms one pool per enemy variant in the wave. Called at RunIntro entry.</summary>
        public void PrewarmPools(int swarmerCapacity, int tankCapacity, int rangedCapacity)
        {
            if (wave == null) return;

            for (int i = 0; i < wave.events.Length; i++)
            {
                var entry = wave.events[i];
                if (entry.enemy == null || _enemyPools.ContainsKey(entry.enemy)) continue;

                var prefab = entry.enemy.prefab;
                if (prefab == null) continue;
                var component = prefab.GetComponent<EnemyBase>();
                if (component == null) continue;

                int cap = entry.enemy.role switch
                {
                    EnemyRole.Swarmer => swarmerCapacity,
                    EnemyRole.Tank => tankCapacity,
                    EnemyRole.Ranged => rangedCapacity,
                    EnemyRole.Elite => 4,
                    EnemyRole.Boss => bossCapacity,
                    _ => 8,
                };

                var pool = new ObjectPool<EnemyBase>(component, cap, transform);
                pool.Warm();
                _enemyPools[entry.enemy] = pool;
            }
        }

        private void Update()
        {
            if (wave == null || runTimer == null || hero == null) return;

            float runMinutes = runTimer.RunSeconds / 60f;
            while (_nextEventIndex < wave.events.Length
                && wave.events[_nextEventIndex].triggerMinute <= runMinutes)
            {
                DispatchEvent(wave.events[_nextEventIndex]);
                _nextEventIndex++;
            }
        }

        private void DispatchEvent(in WaveSpawnEntry entry)
        {
            if (entry.type != WaveEventType.Spawn) return;
            if (entry.enemy == null) return;
            if (!_enemyPools.TryGetValue(entry.enemy, out var pool)) return;

            SpawnerRing.Spawn(entry.pattern, entry.spawnCount, hero!.position, entry.radius,
                pool, entry.enemy, hero!);
        }

        public void Teardown()
        {
            foreach (var pool in _enemyPools.Values) pool.Teardown();
            _enemyPools.Clear();
            _nextEventIndex = 0;
        }
    }
}
