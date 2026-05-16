#if UNITY_EDITOR
// MaterialMigrator — audit + migrate every Material asset under Assets/_Brave
// (and Assets/Resources if present) from Built-in render-pipeline shaders to
// their URP equivalents. Wave 12 mitigation for mixed-pipeline pink rendering
// on device.
//
// Entry points:
//   Menu:    Brave > Migrate Materials to URP
//   Batch:   -executeMethod BraveBunny.Editor.MaterialMigrator.Run
//
// Strategy:
//   1. AssetDatabase.FindAssets("t:Material", ...) over candidate roots.
//   2. For each material, classify shader.name and remap to URP equivalent.
//   3. Preserve _Color → _BaseColor and _MainTex → _BaseMap via SerializedObject
//      so we don't lose authored values on swap (Material.color assignment with
//      a fresh URP/Lit shader does NOT auto-copy these on every Unity version).
//   4. Optionally invoke Unity's built-in URP material upgrader via reflection
//      when available, then re-grep the project for leftovers.
//
// The migrator is idempotent: a second invocation reports 0 migrations because
// every shader name is already URP.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BraveBunny.Editor
{
    public static class MaterialMigrator
    {
        // Roots scanned for Material assets. _Brave is the only authored root; Resources
        // is included because runtime-loaded materials often live there in other projects
        // and we want the migrator to be reusable as the project grows.
        private static readonly string[] SearchRoots = { "Assets/_Brave", "Assets/Resources" };

        // Built-in shader name → URP shader name. Order matters only for logging clarity.
        // The values must be names Shader.Find() can resolve when URP is installed.
        private static readonly Dictionary<string, string> BuiltinToUrp = new()
        {
            { "Standard",                       "Universal Render Pipeline/Lit" },
            { "Standard (Specular setup)",      "Universal Render Pipeline/Lit" },
            { "Mobile/Diffuse",                 "Universal Render Pipeline/Lit" },
            { "Mobile/Bumped Diffuse",          "Universal Render Pipeline/Lit" },
            { "Mobile/Bumped Specular",         "Universal Render Pipeline/Lit" },
            { "Unlit/Color",                    "Universal Render Pipeline/Unlit" },
            { "Unlit/Texture",                  "Universal Render Pipeline/Unlit" },
            { "Unlit/Transparent",              "Universal Render Pipeline/Unlit" },
            { "Unlit/Transparent Cutout",       "Universal Render Pipeline/Unlit" },
            { "Particles/Standard Unlit",       "Universal Render Pipeline/Particles/Unlit" },
            { "Particles/Standard Surface",     "Universal Render Pipeline/Particles/Lit" },
            // Legacy Diffuse / Specular (pre-Standard).
            { "Legacy Shaders/Diffuse",         "Universal Render Pipeline/Lit" },
            { "Legacy Shaders/Specular",        "Universal Render Pipeline/Lit" },
            { "Legacy Shaders/Bumped Diffuse",  "Universal Render Pipeline/Lit" },
            { "Legacy Shaders/Bumped Specular", "Universal Render Pipeline/Lit" },
        };

        [MenuItem("Brave/Migrate Materials to URP")]
        public static void RunFromMenu()
        {
            var migrated = Migrate();
            EditorUtility.DisplayDialog(
                "MaterialMigrator",
                $"Migrated {migrated} material(s). See Console for details.",
                "OK");
        }

        /// <summary>CLI entry — runs the migration and exits with code 0.</summary>
        public static void Run()
        {
            int migrated = 0;
            int exitCode = 0;
            try
            {
                migrated = Migrate();
                Debug.Log($"[MaterialMigrator] DONE — migrated {migrated} material(s).");
            }
            catch (Exception ex)
            {
                // Surface the full stack into the batchmode logfile and fail loudly so
                // the caller's exit-code check catches the regression.
                Debug.LogError($"[MaterialMigrator] FAILED: {ex}");
                exitCode = 1;
            }
            EditorApplication.Exit(exitCode);
        }

        /// <summary>Returns the number of materials whose shader was swapped.</summary>
        private static int Migrate()
        {
            // Filter SearchRoots to those that actually exist — AssetDatabase.FindAssets
            // throws if any provided search-in-folder is missing.
            var validRoots = SearchRoots.Where(AssetDatabase.IsValidFolder).ToArray();
            if (validRoots.Length == 0)
            {
                Debug.LogWarning("[MaterialMigrator] None of the configured roots exist; nothing to scan. "
                                 + $"Configured: {string.Join(", ", SearchRoots)}");
                return 0;
            }

            var guids = AssetDatabase.FindAssets("t:Material", validRoots);
            Debug.Log($"[MaterialMigrator] scanning {guids.Length} material(s) under: {string.Join(", ", validRoots)}");

            int migrated = 0;
            int skipped = 0;
            int alreadyUrp = 0;
            var unmappedShaders = new HashSet<string>();
            var materialsToUpgrade = new List<Material>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null)
                {
                    skipped++;
                    continue;
                }

                var shaderName = mat.shader.name;
                if (shaderName.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal)
                    || shaderName.StartsWith("URP/", StringComparison.Ordinal))
                {
                    alreadyUrp++;
                    continue;
                }

                // Sprites/Default works in both Built-in and URP; leave it alone unless we
                // explicitly want URP's 2D variant. We treat it as a pass-through.
                if (shaderName == "Sprites/Default")
                {
                    alreadyUrp++;
                    continue;
                }

                if (!BuiltinToUrp.TryGetValue(shaderName, out var urpName))
                {
                    unmappedShaders.Add(shaderName);
                    Debug.LogWarning($"[MaterialMigrator] no URP mapping for shader '{shaderName}' "
                                     + $"(material: {path}); leaving as-is.");
                    skipped++;
                    continue;
                }

                if (SwapShader(mat, urpName, path))
                {
                    migrated++;
                    materialsToUpgrade.Add(mat);
                }
                else
                {
                    skipped++;
                }
            }

            // Optionally run the URP package's own upgrader on the freshly-swapped set so
            // keywords / queue / blend modes get fixed up the way Unity expects.
            TryRunUrpUpgrader(materialsToUpgrade);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MaterialMigrator] summary: migrated={migrated} alreadyUrp={alreadyUrp} "
                      + $"skipped={skipped} unmappedShaders=[{string.Join(",", unmappedShaders)}]");
            return migrated;
        }

        /// <summary>
        /// Swap the material's shader and re-apply main color + main texture from the
        /// legacy property names onto the URP property names. Returns true on success.
        /// </summary>
        private static bool SwapShader(Material mat, string urpShaderName, string assetPath)
        {
            var urpShader = Shader.Find(urpShaderName);
            if (urpShader == null)
            {
                Debug.LogError($"[MaterialMigrator] Shader.Find('{urpShaderName}') returned null. "
                               + "Is URP installed in this project?");
                return false;
            }

            // Snapshot legacy values BEFORE shader swap (afterwards the property table changes).
            Color? legacyColor = null;
            if (mat.HasProperty("_Color"))
                legacyColor = mat.GetColor("_Color");

            Texture? mainTex = null;
            Vector2 mainTexScale = Vector2.one;
            Vector2 mainTexOffset = Vector2.zero;
            if (mat.HasProperty("_MainTex"))
            {
                mainTex = mat.GetTexture("_MainTex");
                mainTexScale = mat.GetTextureScale("_MainTex");
                mainTexOffset = mat.GetTextureOffset("_MainTex");
            }

            var fromShader = mat.shader != null ? mat.shader.name : "(null)";
            mat.shader = urpShader;

            // Re-apply preserved properties onto URP equivalents.
            if (legacyColor.HasValue && mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", legacyColor.Value);

            if (mainTex != null && mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", mainTex);
                mat.SetTextureScale("_BaseMap", mainTexScale);
                mat.SetTextureOffset("_BaseMap", mainTexOffset);
            }

            EditorUtility.SetDirty(mat);
            Debug.Log($"[MaterialMigrator] '{assetPath}': {fromShader} → {urpShaderName}");
            return true;
        }

        /// <summary>
        /// Best-effort invocation of the URP package's MaterialUpgrader via reflection,
        /// so we don't take a hard dependency on internal types. If the URP package isn't
        /// loaded (or the entry point moved), we silently no-op — the shader swap above
        /// already produces a renderable material.
        /// </summary>
        private static void TryRunUrpUpgrader(IReadOnlyList<Material> materials)
        {
            if (materials == null || materials.Count == 0) return;

            // Known URP entry points across recent versions:
            //   UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader.UpgradeProjectMaterials
            //   UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader.UpgradeSelectedMaterials
            // Both are static and may or may not exist depending on URP version.
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Unity.RenderPipelines.Universal.Editor");
            if (asm == null) return;

            var type = asm.GetType("UnityEditor.Rendering.Universal.UniversalRenderPipelineMaterialUpgrader");
            if (type == null) return;

            // Prefer a project-wide upgrade so keywords get refreshed even on stragglers.
            var method = type.GetMethod("UpgradeProjectMaterials",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null) return;

            try
            {
                method.Invoke(null, null);
                Debug.Log("[MaterialMigrator] URP UpgradeProjectMaterials() invoked.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MaterialMigrator] URP upgrader invocation failed (non-fatal): {ex.Message}");
            }
        }
    }
}
#endif
