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

using Brave.Gameplay.Definitions;   // gameplay-engineer Wave 2: PlayerMover wiring needs CharacterDefinition
using Brave.Gameplay.Movement;

public static class SceneSetup
{
    private const string SceneDir = "Assets/_Brave/Scenes";

    // Run-scene wiring inputs (gameplay-engineer Wave 2). Const string lives here so the
    // AssetDatabase lookup has no per-frame allocation and is easy to grep when assets move.
    private const string CharBunnyAssetPath = "Assets/_Brave/Data/Balance/Char_bunny.asset";

    [MenuItem("Brave/Scaffold Phase-5 Scenes")]
    public static void Scaffold()
    {
        Directory.CreateDirectory(SceneDir);

        EnsureBoot();
        EnsureMainMenu();
        EnsureLoadout();
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

        // gameplay-engineer Wave 2 — wire PlayerMover + CharacterDefinition on the Player.
        // Kept inline here (not a separate Editor/PlayerWiring.cs) only to minimise the
        // diff; see ADR-0017 follow-up hand-off for the option of moving this out later.
        EnsureRunPlayerWiring(player);

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneSetup] created {path}");
    }

    /// <summary>
    /// Adds the canonical <see cref="PlayerMover"/> to the Run-scene Player GameObject
    /// and assigns its <c>character</c> field to the Bunny CharacterDefinition SO.
    /// Idempotent — re-running the scaffold does not stack duplicate components.
    /// Assumes <c>Brave > Generate Balance SOs from JSON</c> has already produced the SO;
    /// if it hasn't, the asset lookup logs a warning and the component is added
    /// un-configured (PlayerMover.Awake will then disable itself with a clear error).
    /// </summary>
    private static void EnsureRunPlayerWiring(GameObject player)
    {
        var mover = player.GetComponent<PlayerMover>() ?? player.AddComponent<PlayerMover>();

        var charDef = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(CharBunnyAssetPath);
        if (charDef == null)
        {
            Debug.LogWarning(
                $"[SceneSetup] {CharBunnyAssetPath} not found — run 'Brave > Generate Balance SOs from JSON' "
                + "before launching Play mode so PlayerMover can resolve its move-speed.");
            return;
        }

        // Use SerializedObject so the private [SerializeField] survives scene-save without
        // requiring a public setter on PlayerMover (which would loosen its API contract).
        var so = new SerializedObject(mover);
        var prop = so.FindProperty("character");
        if (prop != null)
        {
            prop.objectReferenceValue = charDef;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning("[SceneSetup] PlayerMover.character SerializedProperty not found — "
                + "field may have been renamed; update CharBunnyAssetPath wiring accordingly.");
        }
    }

    private static void EnsureMainMenu()
    {
        var path = $"{SceneDir}/MainMenu.unity";
        if (File.Exists(path)) { Debug.Log("[SceneSetup] MainMenu.unity already exists, skipping"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("MainCamera");
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(1f, 0.92f, 0.76f);   // bunny-cream
        cam.tag = "MainCamera";
        cam.AddComponent<AudioListener>();
        var eventSystem = new GameObject("EventSystem");
        var esType = FindType("UnityEngine.EventSystems.EventSystem");
        if (esType != null) eventSystem.AddComponent(esType);
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"[SceneSetup] created {path}");
    }

    private static void EnsureLoadout()
    {
        var path = $"{SceneDir}/Loadout.unity";
        if (File.Exists(path)) { Debug.Log("[SceneSetup] Loadout.unity already exists, skipping"); return; }
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cam = new GameObject("MainCamera");
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.74f, 0.88f, 0.45f); // meadow-lime
        cam.tag = "MainCamera";
        cam.AddComponent<AudioListener>();
        var eventSystem = new GameObject("EventSystem");
        var esType = FindType("UnityEngine.EventSystems.EventSystem");
        if (esType != null) eventSystem.AddComponent(esType);
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

        foreach (var p in new[] {
            $"{SceneDir}/Boot.unity",
            $"{SceneDir}/MainMenu.unity",
            $"{SceneDir}/Loadout.unity",
            $"{SceneDir}/Run.unity",
            $"{SceneDir}/PerfStress.unity"
        })
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
