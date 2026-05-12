#if UNITY_EDITOR
// One-shot scene generator — creates the minimum-viable scenes the PlayMode
// tests expect (Boot, Run, PerfStress) and registers them in EditorBuildSettings.
// Run via menu: Brave > Scaffold Phase-5 Scenes.
//
// Each scene is intentionally minimal — the goal is to make the smoke tests
// satisfy their scene-load preconditions; runtime wiring (mixer reference,
// localization assets, etc.) ships in a follow-up wave.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneSetup
{
    private const string SceneDir = "Assets/_Brave/Scenes";

    [MenuItem("Brave/Scaffold Phase-5 Scenes")]
    public static void Scaffold()
    {
        Directory.CreateDirectory(SceneDir);

        EnsureBoot();
        EnsureRun();
        EnsurePerfStress();
        RegisterInBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneSetup] OK — Boot/Run/PerfStress scenes scaffolded and registered.");
    }

    /// <summary>CLI entry for headless invocation.</summary>
    public static void RunHeadless()
    {
        Scaffold();
        EditorApplication.Exit(0);
    }

    private static void EnsureBoot()
    {
        var path = $"{SceneDir}/Boot.unity";
        if (File.Exists(path)) { Debug.Log("[SceneSetup] Boot.unity already exists, skipping"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // [Bootstrap] root GameObject — hosts GameContextBootstrap.
        var bootstrap = new GameObject("[Bootstrap]");

        var bootstrapType = FindType("Brave.Systems.Context.GameContextBootstrap");
        if (bootstrapType != null)
        {
            bootstrap.AddComponent(bootstrapType);
        }
        else
        {
            Debug.LogWarning("[SceneSetup] GameContextBootstrap type not resolved — Boot scene shipped without it.");
        }

        // EventSystem so UI input works on first run.
        var eventSystem = new GameObject("EventSystem");
        var esType = FindType("UnityEngine.EventSystems.EventSystem");
        if (esType != null) eventSystem.AddComponent(esType);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneSetup] created {path}");
    }

    private static void EnsureRun()
    {
        var path = $"{SceneDir}/Run.unity";
        if (File.Exists(path)) { Debug.Log("[SceneSetup] Run.unity already exists, skipping"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Top-down 3/4 camera per art-bible 00-style-overview (FOV 35°, dist 18u).
        var cam = new GameObject("MainCamera");
        var camera = cam.AddComponent<Camera>();
        camera.fieldOfView = 35;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.74f, 0.88f, 0.45f, 1f); // meadow-lime
        cam.transform.position = new Vector3(0, 14.7f, -10.4f);     // top-down 3/4
        cam.transform.rotation = Quaternion.Euler(55, 0, 0);
        cam.tag = "MainCamera";
        cam.AddComponent<AudioListener>();

        // Directional light approximating Meadow noon.
        var light = new GameObject("DirectionalLight");
        var l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.2f;
        l.color = new Color(1f, 0.96f, 0.85f);
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        // Placeholder "Player" — a primitive cube tinted bunny-cream so the
        // tests have something to assert on; replaced by the real character
        // prefab in a later wave.
        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = Vector3.zero;
        var renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.92f, 0.76f); // bunny cream
            renderer.sharedMaterial = mat;
        }

        // Ground plane — Meadow.
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "MeadowGround";
        ground.transform.localScale = new Vector3(8, 1, 8); // 80x80 game units
        ground.transform.position = new Vector3(0, -0.5f, 0);
        var gr = ground.GetComponent<Renderer>();
        if (gr != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(0.65f, 0.84f, 0.37f); // meadow lime, slightly desaturated
            gr.sharedMaterial = mat;
        }

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneSetup] created {path}");
    }

    private static void EnsurePerfStress()
    {
        var path = $"{SceneDir}/PerfStress.unity";
        if (File.Exists(path)) { Debug.Log("[SceneSetup] PerfStress.unity already exists, skipping"); return; }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("MainCamera");
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cam.transform.position = new Vector3(0, 30, -20);
        cam.transform.rotation = Quaternion.Euler(55, 0, 0);
        cam.tag = "MainCamera";
        cam.AddComponent<AudioListener>();

        // Light so things render in screenshots.
        var light = new GameObject("DirectionalLight");
        light.AddComponent<Light>().type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(60, 0, 0);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneSetup] created {path}");
    }

    private static void RegisterInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var have = new HashSet<string>();
        foreach (var s in scenes) have.Add(s.path);

        foreach (var p in new[] { $"{SceneDir}/Boot.unity", $"{SceneDir}/Run.unity", $"{SceneDir}/PerfStress.unity" })
        {
            if (have.Contains(p) || !File.Exists(p)) continue;
            scenes.Add(new EditorBuildSettingsScene(p, enabled: true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[SceneSetup] EditorBuildSettings.scenes now: {EditorBuildSettings.scenes.Length}");
    }

    private static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName, throwOnError: false);
            if (t != null) return t;
        }
        return null!;
    }
}
#endif
