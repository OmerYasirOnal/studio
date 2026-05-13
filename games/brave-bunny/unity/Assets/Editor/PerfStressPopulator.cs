#if UNITY_EDITOR
// PerfStressPopulator — populates PerfStress.unity with the Phase 5 stress load:
//   200 enemy GameObjects (Enemy prefab or cube primitives) arranged in a circle (r=30u)
//   50 projectile spheres at random offsets within r=20u
//   30 VFX placeholder cubes (scale 0.3) at random offsets within r=25u
//   FpsSampler component added to the MainCamera
//
// Invoke: Brave > Populate PerfStress (200/50/30)
// Does NOT modify the .unity file directly — all work is done at runtime via the
// EditorSceneManager API so the change only hits the scene asset, never raw YAML.
//
// Perf contract: brave-bunny/CLAUDE.md (200 enemies + 50 proj + 30 VFX @ 60 fps iPhone 12)
// Companion test: Assets/_Brave/Code/Tests/PlayMode/Performance/PerfStressFpsTest.cs

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using Brave.Diagnostics;

public static class PerfStressPopulator
{
    private const string ScenePath   = "Assets/_Brave/Scenes/PerfStress.unity";
    private const int    EnemyCount  = 200;
    private const int    ProjCount   = 50;
    private const int    VfxCount    = 30;
    private const float  EnemyRadius = 30f;
    private const float  ProjRadius  = 20f;
    private const float  VfxRadius   = 25f;

    // Enemy prefab path — use if present; fall back to primitive cube tagged "Enemy".
    private const string EnemyPrefabPath = "Assets/_Brave/Data/Definitions/Enemy_swarmer.prefab";

    [MenuItem("Brave/Populate PerfStress (200/50/30)")]
    public static void Populate()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogError($"[PerfStressPopulator] Scene not found: {ScenePath}. "
                + "Run 'Brave > Scaffold Phase-5 Scenes' first.");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        // ---- find or create camera ----
        var camObj = GameObject.FindWithTag("MainCamera");
        if (camObj == null)
        {
            camObj = new GameObject("MainCamera");
            var cam = camObj.AddComponent<Camera>();
            cam.clearFlags  = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            camObj.transform.position = new Vector3(0, 30, -20);
            camObj.transform.rotation = Quaternion.Euler(55, 0, 0);
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        // Attach FpsSampler (idempotent).
        if (camObj.GetComponent<FpsSampler>() == null)
            camObj.AddComponent<FpsSampler>();

        // ---- parent containers ----
        var enemyRoot = GetOrCreate("[Enemies]");
        var projRoot  = GetOrCreate("[Projectiles]");
        var vfxRoot   = GetOrCreate("[VFX]");

        // Optionally clear previous stress objects before re-populating.
        ClearChildren(enemyRoot);
        ClearChildren(projRoot);
        ClearChildren(vfxRoot);

        // ---- try to load enemy prefab ----
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);

        var rng = new System.Random(42);

        // ---- 200 enemies — circle r=30 ----
        for (int i = 0; i < EnemyCount; i++)
        {
            float angle = i * Mathf.PI * 2f / EnemyCount;
            float r     = EnemyRadius * (0.8f + (float)rng.NextDouble() * 0.4f);
            var   pos   = new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);

            GameObject go;
            if (enemyPrefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
                go.transform.position = pos;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = pos;
                go.transform.localScale = Vector3.one * 0.8f;
                go.tag = "Enemy";
            }
            go.name = $"Enemy_{i:000}";
            go.transform.SetParent(enemyRoot.transform, worldPositionStays: true);
        }

        // ---- 50 projectiles — spheres at random offsets ----
        for (int i = 0; i < ProjCount; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r     = (float)rng.NextDouble() * ProjRadius;
            var   pos   = new Vector3(Mathf.Cos(angle) * r, 0.3f, Mathf.Sin(angle) * r);

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Projectile_{i:00}";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.25f;
            go.transform.SetParent(projRoot.transform, worldPositionStays: true);
        }

        // ---- 30 VFX placeholders — small cubes at random offsets ----
        for (int i = 0; i < VfxCount; i++)
        {
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            float r     = (float)rng.NextDouble() * VfxRadius;
            var   pos   = new Vector3(Mathf.Cos(angle) * r, 0.15f, Mathf.Sin(angle) * r);

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"VFX_{i:00}";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.3f;
            go.transform.SetParent(vfxRoot.transform, worldPositionStays: true);
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PerfStressPopulator] Done — {EnemyCount} enemies / {ProjCount} projectiles / {VfxCount} VFX — scene saved.");
    }

    private static GameObject GetOrCreate(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        return go;
    }

    private static void ClearChildren(GameObject parent)
    {
        var children = new System.Collections.Generic.List<GameObject>();
        foreach (Transform c in parent.transform) children.Add(c.gameObject);
        foreach (var c in children) Object.DestroyImmediate(c);
    }
}
#endif
