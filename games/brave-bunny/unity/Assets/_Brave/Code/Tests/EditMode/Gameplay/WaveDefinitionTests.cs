// QA — WaveDefinition EditMode tests
// Subject under test: Brave.Gameplay.Definitions.WaveDefinition + WaveSpawnEntry
// User stories: US-28 (wave-pressure cue), US-20 (boss telegraphs).
// Spec: docs/06-tech-spec/02-data-model.md § WaveDefinition.
//       brave-bunny/CLAUDE.md perf contract — 200 enemy cap.
// ADR: 0015 — rewritten against current minimal production shape (Path B).
//      Production exposes biomeSlug + WaveSpawnEntry[] events (triggerMinute-keyed).
//      Once production grows durationSeconds / maxConcurrentEnemies these tests
//      gain assertions; until then we lock the invariants the data has today.

using System;
using Brave.Gameplay.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class WaveDefinitionTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string TestBiomeSlug = "test-biome";
        private const float MinuteZero = 0f;
        private const float BossApproachMinute = 7f;     // tech-spec 02 — boss approach at 7:00
        private const float BossMinute = 8f;             // boss event at 8:00
        private const int SwarmerCount = 12;
        private const int RangerCount = 8;
        private const int MiniBossCount = 1;
        private const int BossCount = 1;
        private const float SpawnRadius = 6f;
        private const float TimeEpsilon = 0.0001f;

        private static WaveDefinition MakeWave(params WaveSpawnEntry[] entries)
        {
            var w = ScriptableObject.CreateInstance<WaveDefinition>();
            w.biomeSlug = TestBiomeSlug;
            w.events = entries ?? Array.Empty<WaveSpawnEntry>();
            return w;
        }

        private static WaveSpawnEntry SpawnAt(float minute, int count,
            WaveEventType type = WaveEventType.Spawn,
            SpawnPattern pattern = SpawnPattern.Ring)
        {
            return new WaveSpawnEntry
            {
                triggerMinute = minute,
                type = type,
                enemy = null,
                spawnCount = count,
                pattern = pattern,
                radius = SpawnRadius,
            };
        }

        // ---- public-API surface ----

        [Test]
        public void WaveDefinition_NEntries_ExposesNEntries()
        {
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(1f, RangerCount),
                SpawnAt(2f, SwarmerCount));
            Assert.That(wave.events, Is.Not.Null);
            Assert.That(wave.events.Length, Is.EqualTo(3),
                "WaveDefinition.events must round-trip the assigned array length");
            ScriptableObject.DestroyImmediate(wave);
        }

        [Test]
        public void WaveDefinition_TotalSpawnCount_EqualsSumOfEntries()
        {
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(1f, RangerCount),
                SpawnAt(2f, SwarmerCount));

            int total = 0;
            for (int i = 0; i < wave.events.Length; i++)
                total += wave.events[i].spawnCount;

            int expected = SwarmerCount + RangerCount + SwarmerCount;
            Assert.That(total, Is.EqualTo(expected),
                $"Sum of spawnCount across entries must equal {expected}");
            ScriptableObject.DestroyImmediate(wave);
        }

        // ---- time-bounds ----

        [Test]
        public void WaveDefinition_FirstSpawnTime_IsMinOfTriggerMinutes()
        {
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(BossApproachMinute, MiniBossCount, WaveEventType.MiniBoss),
                SpawnAt(BossMinute, BossCount, WaveEventType.Boss));

            float first = float.PositiveInfinity;
            for (int i = 0; i < wave.events.Length; i++)
                if (wave.events[i].triggerMinute < first)
                    first = wave.events[i].triggerMinute;

            Assert.That(first, Is.EqualTo(MinuteZero).Within(TimeEpsilon),
                "First spawn time must equal the smallest triggerMinute in events");
            Assert.That(first, Is.GreaterThanOrEqualTo(MinuteZero - TimeEpsilon),
                "First spawn time must not be negative");
            ScriptableObject.DestroyImmediate(wave);
        }

        [Test]
        public void WaveDefinition_LastSpawnTime_IsMaxOfTriggerMinutes()
        {
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(BossApproachMinute, MiniBossCount, WaveEventType.MiniBoss),
                SpawnAt(BossMinute, BossCount, WaveEventType.Boss));

            float last = float.NegativeInfinity;
            for (int i = 0; i < wave.events.Length; i++)
                if (wave.events[i].triggerMinute > last)
                    last = wave.events[i].triggerMinute;

            Assert.That(last, Is.EqualTo(BossMinute).Within(TimeEpsilon),
                "Last spawn time must equal the largest triggerMinute in events");
            ScriptableObject.DestroyImmediate(wave);
        }

        [Test]
        public void WaveDefinition_AllTriggerMinutes_NonNegative()
        {
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(1f, RangerCount),
                SpawnAt(BossApproachMinute, MiniBossCount, WaveEventType.MiniBoss),
                SpawnAt(BossMinute, BossCount, WaveEventType.Boss));

            for (int i = 0; i < wave.events.Length; i++)
            {
                Assert.That(wave.events[i].triggerMinute,
                    Is.GreaterThanOrEqualTo(MinuteZero - TimeEpsilon),
                    $"Entry {i} has negative triggerMinute {wave.events[i].triggerMinute}");
            }
            ScriptableObject.DestroyImmediate(wave);
        }

        // ---- ordering invariant (mirrors production OnValidate contract) ----

        [Test]
        public void WaveDefinition_Events_AreSortedByTriggerMinute()
        {
            // Production OnValidate logs an error when out-of-order; data authored via the
            // balance tool is expected to be sorted. This test pins the contract.
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(1f, RangerCount),
                SpawnAt(BossApproachMinute, MiniBossCount, WaveEventType.MiniBoss),
                SpawnAt(BossMinute, BossCount, WaveEventType.Boss));

            for (int i = 1; i < wave.events.Length; i++)
            {
                Assert.That(wave.events[i].triggerMinute,
                    Is.GreaterThanOrEqualTo(wave.events[i - 1].triggerMinute - TimeEpsilon),
                    $"Entries must be sorted by triggerMinute (violation at index {i})");
            }
            ScriptableObject.DestroyImmediate(wave);
        }

        // ---- field round-trips ----

        [Test]
        public void WaveDefinition_BiomeSlug_RoundTrips()
        {
            var wave = MakeWave();
            Assert.That(wave.biomeSlug, Is.EqualTo(TestBiomeSlug));
            ScriptableObject.DestroyImmediate(wave);
        }

        [Test]
        public void WaveDefinition_SpawnEntry_FieldsRoundTrip()
        {
            // WaveSpawnEntry is a struct — assert every public field survives assignment.
            var entry = SpawnAt(BossApproachMinute, MiniBossCount,
                WaveEventType.MiniBoss, SpawnPattern.Arc);
            var wave = MakeWave(entry);

            Assert.That(wave.events.Length, Is.EqualTo(1));
            var e = wave.events[0];
            Assert.That(e.triggerMinute, Is.EqualTo(BossApproachMinute).Within(TimeEpsilon));
            Assert.That(e.type, Is.EqualTo(WaveEventType.MiniBoss));
            Assert.That(e.spawnCount, Is.EqualTo(MiniBossCount));
            Assert.That(e.pattern, Is.EqualTo(SpawnPattern.Arc));
            Assert.That(e.radius, Is.EqualTo(SpawnRadius).Within(TimeEpsilon));
            Assert.That(e.enemy, Is.Null, "enemy ref defaults to null when not assigned");
            ScriptableObject.DestroyImmediate(wave);
        }

        // ---- empty edge case ----

        [Test]
        public void WaveDefinition_Empty_IsValid_ZeroEntries()
        {
            var wave = MakeWave(); // no entries
            Assert.That(wave.events, Is.Not.Null,
                "events array defaults to Array.Empty<>, never null");
            Assert.That(wave.events.Length, Is.EqualTo(0));

            // Sum invariants hold trivially.
            int total = 0;
            for (int i = 0; i < wave.events.Length; i++)
                total += wave.events[i].spawnCount;
            Assert.That(total, Is.EqualTo(0));

            ScriptableObject.DestroyImmediate(wave);
        }

        [Test]
        public void WaveDefinition_DefaultInstance_EventsNeverNull()
        {
            // A freshly created SO (no events ever assigned) must still have an array,
            // not null — gameplay-engineer's spawner iterates without null-checks.
            var wave = ScriptableObject.CreateInstance<WaveDefinition>();
            Assert.That(wave.events, Is.Not.Null,
                "Default events array must be Array.Empty<>, not null");
            Assert.That(wave.events.Length, Is.EqualTo(0));
            ScriptableObject.DestroyImmediate(wave);
        }

        // ---- boss-presence (game-CLAUDE.md: boss must appear; minute-keyed) ----

        [Test]
        public void WaveDefinition_BossEventBeforeRunEnd_DetectablePerType()
        {
            // Production currently has no durationSeconds field — we assert the
            // boss-presence invariant in minute space, which is what the data carries.
            var wave = MakeWave(
                SpawnAt(MinuteZero, SwarmerCount),
                SpawnAt(BossApproachMinute, MiniBossCount, WaveEventType.MiniBoss),
                SpawnAt(BossMinute, BossCount, WaveEventType.Boss));

            bool foundBoss = false;
            for (int i = 0; i < wave.events.Length; i++)
            {
                if (wave.events[i].type == WaveEventType.Boss)
                {
                    foundBoss = true;
                    break;
                }
            }
            Assert.That(foundBoss, Is.True,
                "A campaign wave must contain at least one Boss event");
            ScriptableObject.DestroyImmediate(wave);
        }
    }
}
