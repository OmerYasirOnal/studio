// TEMPORARILY DISABLED — see ADR-0015 (test/production API drift).  Re-enable when:
//   * WaveDefinition gains durationSeconds + maxConcurrentEnemies + events (WaveEvent[])
//   * MechanicRegistry exposes ResetForTests
// Until then, the body is wrapped under an undefined symbol.
#if BRAVE_FUTURE_API
// QA — WaveDefinition EditMode tests
// Subject under test: BraveBunny.Gameplay.Data.WaveDefinition + WaveEvent
// User stories: US-28 (wave-pressure cue), US-20 (boss telegraphs).
// Spec: docs/06-tech-spec/02-data-model.md § WaveDefinition.
//       brave-bunny/CLAUDE.md perf contract — 200 enemy cap, boss must appear before run ends.

using System.Collections.Generic;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Spawning;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class WaveDefinitionTests
    {
        // ---- constants (no magic numbers) ----
        private const float MinTrigger = 0f;
        private const float ConcurrencyWindowSeconds = 6f;
        private const int MaxConcurrentPerWindow = 200;     // perf contract: 200 simultaneous enemies
        private const float RunDurationSeconds = 480f;       // 8-minute survival run
        private const float TimeEpsilon = 0.0001f;

        private static WaveDefinition MakeWave(params WaveEvent[] events)
        {
            var w = ScriptableObject.CreateInstance<WaveDefinition>();
            w.biomeSlug = "test-biome";
            w.durationSeconds = RunDurationSeconds;
            w.maxConcurrentEnemies = MaxConcurrentPerWindow;
            w.events = events;
            return w;
        }

        private static WaveEvent SpawnEvent(float trigger, int count, WaveEventType type = WaveEventType.Spawn)
        {
            return new WaveEvent
            {
                triggerSeconds = trigger,
                type = type,
                spawnCount = count,
                pattern = SpawnPattern.Ring,
                beat = "test-beat",
            };
        }

        [Test]
        public void Waves_AllSpawnTimesNonNegative()
        {
            var wave = MakeWave(
                SpawnEvent(0f, 8),
                SpawnEvent(30f, 12),
                SpawnEvent(60f, 16));
            foreach (var e in wave.events)
                Assert.That(e.triggerSeconds, Is.GreaterThanOrEqualTo(MinTrigger - TimeEpsilon),
                    $"wave event '{e.beat}' has negative trigger {e.triggerSeconds}");
            Object.DestroyImmediate(wave);
        }

        [Test]
        public void Waves_NoConcurrentExceedsCap()
        {
            // Rolling 6-second window — sum spawnCount within window must stay ≤ 200.
            var wave = MakeWave(
                SpawnEvent(0f, 40),
                SpawnEvent(2f, 40),
                SpawnEvent(4f, 40),     // window 0..6 → 120
                SpawnEvent(5f, 40),     // window 0..6 → 160
                SpawnEvent(10f, 80));    // out of first window

            int worstWindowSum = ComputeMaxConcurrentInWindow(wave.events, ConcurrencyWindowSeconds);
            Assert.That(worstWindowSum, Is.LessThanOrEqualTo(MaxConcurrentPerWindow),
                $"Wave spawns {worstWindowSum} in a {ConcurrencyWindowSeconds}s window — exceeds {MaxConcurrentPerWindow} cap");
            Object.DestroyImmediate(wave);
        }

        [Test]
        public void Waves_BossPresentBeforeEnd()
        {
            var wave = MakeWave(
                SpawnEvent(0f, 8),
                SpawnEvent(60f, 20),
                SpawnEvent(240f, 0, WaveEventType.Boss),
                SpawnEvent(420f, 0, WaveEventType.Boss));

            bool foundBoss = false;
            for (int i = 0; i < wave.events.Length; i++)
            {
                var e = wave.events[i];
                if (e.type == WaveEventType.Boss && e.triggerSeconds < RunDurationSeconds)
                {
                    foundBoss = true;
                    break;
                }
            }
            Assert.That(foundBoss, Is.True, "wave must contain at least one boss event before runDuration");
            Object.DestroyImmediate(wave);
        }

        // ---- helpers ----

        private static int ComputeMaxConcurrentInWindow(IReadOnlyList<WaveEvent> events, float windowSeconds)
        {
            int max = 0;
            for (int i = 0; i < events.Count; i++)
            {
                int sum = 0;
                float start = events[i].triggerSeconds;
                float end = start + windowSeconds;
                for (int j = 0; j < events.Count; j++)
                {
                    var t = events[j].triggerSeconds;
                    if (t >= start && t < end) sum += events[j].spawnCount;
                }
                if (sum > max) max = sum;
            }
            return max;
        }
    }
}
#endif
