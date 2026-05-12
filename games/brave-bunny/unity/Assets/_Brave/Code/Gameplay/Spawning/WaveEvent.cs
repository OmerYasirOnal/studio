#nullable enable
// Tech-spec 02 § WaveEvent — runtime view of a wave entry.
// Sister struct to the serialized `WaveSpawnEntry` (Definitions/) authored by level-designer.
// Bridges between the SO data model and the WaveRunner timeline cursor.
// Keep all fields here; do NOT add fields to WaveSpawnEntry without an ADR (ScriptableObject
// serialization stability).

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;

// Disambiguate: this file straddles both SpawnPattern enums and intentionally
// surfaces the runtime-helper one in its public field.
using DataPattern = Brave.Gameplay.Definitions.SpawnPattern;

namespace Brave.Gameplay.Spawning
{
    /// <summary>
    /// Runtime, expanded form of a single wave event. WaveRunner constructs these from
    /// <see cref="WaveSpawnEntry"/> rows in the authored <c>WaveDefinition.events</c> array.
    /// </summary>
    public struct WaveEvent
    {
        public float triggerSeconds;          // seconds since run start
        public WaveEventType type;
        public EnemyDefinition? enemy;        // for Spawn / MiniBoss
        public EnemyDefinition? boss;         // for Boss
        public int spawnCount;
        public SpawnPattern pattern;          // runtime helper enum (Spawning.SpawnPattern)
        public float radius;                  // world units for ring/arc/stream patterns
        public string fromDirection;          // "north" | "east" | "south" | "west" — pattern hint
        public string beat;                   // optional level-designer tag for telemetry / tests

        /// <summary>Build a runtime WaveEvent from the serialized entry.</summary>
        public static WaveEvent FromEntry(in WaveSpawnEntry e)
        {
            return new WaveEvent
            {
                triggerSeconds = e.triggerMinute * 60f,
                type = e.type,
                enemy = e.type == WaveEventType.Boss ? null : e.enemy,
                boss = e.type == WaveEventType.Boss ? e.enemy : null,
                spawnCount = e.spawnCount,
                pattern = ToRuntime(e.pattern),
                radius = e.radius,
                fromDirection = "north",
                beat = string.Empty,
            };
        }

        /// <summary>Maps the data-model pattern enum to its runtime-helper counterpart.</summary>
        private static SpawnPattern ToRuntime(DataPattern p) => p switch
        {
            DataPattern.Ring => SpawnPattern.Ring,
            DataPattern.Stream => SpawnPattern.Stream,
            DataPattern.Scatter => SpawnPattern.Scatter,
            DataPattern.Arc => SpawnPattern.Flank,                    // closest semantic
            DataPattern.RandomEdge => SpawnPattern.Scatter,           // edge-spawn ≈ scatter at radius
            DataPattern.ScriptedPoints => SpawnPattern.ScriptedSpawn,
            _ => SpawnPattern.Scatter,
        };
    }
}
