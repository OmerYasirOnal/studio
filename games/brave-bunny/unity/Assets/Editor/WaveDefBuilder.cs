#if UNITY_EDITOR
// Wave 13 (MVP) — build a minimal WaveDefinition SO so WaveSpawner has a schedule.
//
// Produces: Assets/_Brave/Data/Definitions/Waves/Wave_Mvp_Test.asset
//
// Contents: 5 spawn waves, 3s apart, each spawning 8 swarmers on a ring r=10 around
// the hero. Total scheduled time ≈ 12s; combined with traversal time gives ~30s run.
//
// Calls EnemyPrefabBuilder.Build() on the fly if the default enemy SO is missing.

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using Brave.Gameplay.Definitions;

public static class WaveDefBuilder
{
    public const string WaveAssetPath = "Assets/_Brave/Data/Definitions/Waves/Wave_Mvp_Test.asset";

    private const int WaveCount = 5;
    private const int SpawnCountPerWave = 8;
    private const float RingRadius = 10f;
    private const float WaveGapSeconds = 3f;
    private const float FirstWaveDelaySeconds = 2f;

    [MenuItem("BraveBunny/MVP/Build Default WaveDefinition")]
    public static void Build()
    {
        EnsureDirectory(WaveAssetPath);

        // Resolve (or lazily build) the default swarmer EnemyDefinition.
        var enemyDef = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
            EnemyPrefabBuilder.DefinitionPath);
        if (enemyDef == null)
        {
            Debug.Log("[WaveDefBuilder] Default enemy definition missing — running EnemyPrefabBuilder.Build() first.");
            EnemyPrefabBuilder.Build();
            enemyDef = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                EnemyPrefabBuilder.DefinitionPath);
        }
        if (enemyDef == null)
        {
            Debug.LogError(
                $"[WaveDefBuilder] EnemyDefinition still null at {EnemyPrefabBuilder.DefinitionPath} — aborting.");
            return;
        }

        var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(WaveAssetPath);
        if (wave == null)
        {
            wave = ScriptableObject.CreateInstance<WaveDefinition>();
            AssetDatabase.CreateAsset(wave, WaveAssetPath);
        }

        wave.biomeSlug = "mvp-test";
        var events = new WaveSpawnEntry[WaveCount];
        for (int i = 0; i < WaveCount; i++)
        {
            float t = FirstWaveDelaySeconds + i * WaveGapSeconds;
            events[i] = new WaveSpawnEntry
            {
                triggerMinute = t / 60f, // WaveSpawner.Update compares against runTimer.RunSeconds/60
                type = WaveEventType.Spawn,
                enemy = enemyDef,
                spawnCount = SpawnCountPerWave,
                pattern = SpawnPattern.Ring,
                radius = RingRadius,
            };
        }
        wave.events = events;

        EditorUtility.SetDirty(wave);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[WaveDefBuilder] OK — built {WaveAssetPath} "
            + $"({WaveCount} waves × {SpawnCountPerWave} swarmers, ring r={RingRadius}, gap {WaveGapSeconds}s)");
    }

    private static void EnsureDirectory(string assetPath)
    {
        var dir = Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
#endif
