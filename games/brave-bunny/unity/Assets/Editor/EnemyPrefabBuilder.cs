#if UNITY_EDITOR
// Wave 13 (MVP) — build a default enemy prefab so WaveSpawner has something to spawn.
//
// Produces:
//   Assets/_Brave/Art/Prefabs/Enemy_Swarmer_Default.prefab
//   Assets/_Brave/Data/Definitions/Enemy_Swarmer_Default.asset   (EnemyDefinition SO)
//
// Composition (matches tech-spec 05 + ADR-0018):
//   - Cube primitive, scale 0.5u
//   - Rigidbody, kinematic=true (no physics step; Swarmer moves the Transform itself)
//   - CapsuleCollider, isTrigger=true (contact-damage detection only — broadphase elsewhere)
//   - MeshRenderer with URP/Lit RED (#cc4444) material
//   - Swarmer (concrete EnemyBase subclass) — the codebase's IEnemyBehavior equivalent
//   - EnemyHealth — paired health component (EnemyBase.Awake auto-binds via GetComponent)
//
// Why Swarmer + EnemyBase (not the standalone Enemy.cs + SwarmerBehavior pair)?
// WaveSpawner.PrewarmPools and SpawnerRing.Spawn both consume <EnemyBase> — wiring the
// prefab against EnemyBase is the only path that actually spawns through the pool.

using System.IO;
using UnityEditor;
using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;

public static class EnemyPrefabBuilder
{
    public const string PrefabPath = "Assets/_Brave/Art/Prefabs/Enemy_Swarmer_Default.prefab";
    public const string DefinitionPath = "Assets/_Brave/Data/Definitions/Enemy_Swarmer_Default.asset";

    private const float DefaultHp = 3f;
    private const float DefaultSpeed = 2f;
    private const float DefaultContactDamage = 5f;
    private const string EnemyColorHex = "#cc4444"; // bright red for MVP visibility

    [MenuItem("BraveBunny/MVP/Build Default Enemy Prefab")]
    public static void Build()
    {
        EnsureDirectory(PrefabPath);
        EnsureDirectory(DefinitionPath);

        // Build the prefab source GameObject in-memory.
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Enemy_Swarmer_Default";
        go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Drop the default BoxCollider that comes with the cube primitive; we
        // replace it with a trigger CapsuleCollider per tech-spec 05.
        var existingBox = go.GetComponent<BoxCollider>();
        if (existingBox != null) Object.DestroyImmediate(existingBox);

        // CapsuleCollider — trigger-only (contact-damage broadphase reads OverlapSphere etc.).
        var capsule = go.AddComponent<CapsuleCollider>();
        capsule.isTrigger = true;
        capsule.radius = 0.5f;
        capsule.height = 1.0f;
        capsule.direction = 1; // Y-axis

        // Rigidbody — kinematic so physics doesn't fight the Transform writes in Swarmer.TickBehavior.
        // ADR-0018: enemies move via transform on the XZ plane.
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Red URP/Lit material.
        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = "Enemy_Swarmer_Default_Mat" };
            if (ColorUtility.TryParseHtmlString(EnemyColorHex, out var c))
                mat.color = c;
            renderer.sharedMaterial = mat;
        }

        // EnemyHealth first so EnemyBase.Awake's GetComponent call resolves it at runtime;
        // we still wire it via SerializedObject below for deterministic asset state.
        var health = go.AddComponent<EnemyHealth>();
        var swarmer = go.AddComponent<Swarmer>();

        // Create or reuse the paired EnemyDefinition SO.
        var def = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(DefinitionPath);
        if (def == null)
        {
            def = ScriptableObject.CreateInstance<EnemyDefinition>();
            AssetDatabase.CreateAsset(def, DefinitionPath);
        }
        def.slug = "swarmer-default";
        def.role = EnemyRole.Swarmer;
        def.baseHP = DefaultHp;
        def.moveSpeed = DefaultSpeed;
        def.contactDamage = DefaultContactDamage;
        def.rangedDamage = 0f;
        def.defenseMultiplier = 0f;
        def.telegraphSfxKey = string.Empty;
        def.telegraphWindowSeconds = 0f;
        EditorUtility.SetDirty(def);

        // Wire EnemyBase serialized fields (definition + health) so the prefab carries them.
        var so = new SerializedObject(swarmer);
        var defProp = so.FindProperty("definition");
        if (defProp != null) defProp.objectReferenceValue = def;
        var healthProp = so.FindProperty("health");
        if (healthProp != null) healthProp.objectReferenceValue = health;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save the prefab; destroy the temp scene instance.
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath, out var ok);
        Object.DestroyImmediate(go);

        if (!ok || prefab == null)
        {
            Debug.LogError($"[EnemyPrefabBuilder] Failed to save prefab at {PrefabPath}.");
            return;
        }

        // Back-reference: EnemyDefinition.prefab → the saved prefab GameObject.
        def.prefab = prefab;
        EditorUtility.SetDirty(def);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EnemyPrefabBuilder] OK — built {PrefabPath} (HP={DefaultHp}, Speed={DefaultSpeed})");
    }

    private static void EnsureDirectory(string assetPath)
    {
        var dir = Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }
}
#endif
