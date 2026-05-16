#if UNITY_EDITOR
// -----------------------------------------------------------------------------
// SetupURP — one-shot, idempotent URP wiring for Brave Bunny.
//
// Root cause this fixes (Wave 12):
//   ProjectSettings/GraphicsSettings.asset had m_CustomRenderPipeline: {fileID: 0}
//   so the URP/Shader-Graph materials used by the gameplay assets rendered as
//   the magenta error fallback on device. TestFlight build 0.1.0(202605161452)
//   shipped pink because of this.
//
// What this script does (idempotent, safe to re-run):
//   1. Ensures Assets/_Brave/Settings/Rendering/ exists.
//   2. Creates URP_Renderer.asset (UniversalRendererData)         — if missing.
//   3. Creates URP_Pipeline.asset (UniversalRenderPipelineAsset)  — if missing,
//      with the renderer above wired into rendererDataList[0].
//   4. Assigns GraphicsSettings.defaultRenderPipeline                = URP_Pipeline.
//   5. Assigns each QualitySettings level renderPipeline             = URP_Pipeline.
//
// Run via Unity Hub menu "Brave > Wire URP Pipeline (idempotent)" or headlessly:
//   /Applications/Unity/Hub/Editor/6000.0.74f1/Unity.app/Contents/MacOS/Unity \
//     -batchmode -nographics -quit \
//     -projectPath <repo>/games/brave-bunny/unity \
//     -logFile /tmp/setup-urp.log \
//     -executeMethod BraveBunny.Editor.SetupURP.Run
// -----------------------------------------------------------------------------

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BraveBunny.Editor
{
    public static class SetupURP
    {
        private const string SettingsDir   = "Assets/_Brave/Settings/Rendering";
        private const string RendererPath  = SettingsDir + "/URP_Renderer.asset";
        private const string PipelinePath  = SettingsDir + "/URP_Pipeline.asset";

        [MenuItem("Brave/Wire URP Pipeline (idempotent)")]
        public static void Run()
        {
            try
            {
                Log("starting");

                EnsureFolders();
                var renderer = EnsureRenderer();
                var pipeline = EnsurePipeline(renderer);

                AssignToGraphicsSettings(pipeline);
                AssignToQualityLevels(pipeline);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Log("DONE");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SetupURP] FAILED: {e}");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Brave"))
                AssetDatabase.CreateFolder("Assets", "_Brave");
            if (!AssetDatabase.IsValidFolder("Assets/_Brave/Settings"))
                AssetDatabase.CreateFolder("Assets/_Brave", "Settings");
            if (!AssetDatabase.IsValidFolder(SettingsDir))
                AssetDatabase.CreateFolder("Assets/_Brave/Settings", "Rendering");
            Log($"folders ok ({SettingsDir})");
        }

        private static ScriptableRendererData EnsureRenderer()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(RendererPath);
            if (existing != null)
            {
                Log($"renderer exists at {RendererPath}");
                return existing;
            }

            // UniversalRendererData is the canonical 2D/3D forward renderer in URP 17.
            var data = ScriptableObject.CreateInstance<UniversalRendererData>();
            data.name = "URP_Renderer";
            AssetDatabase.CreateAsset(data, RendererPath);
            Log($"renderer created at {RendererPath}");
            return data;
        }

        private static UniversalRenderPipelineAsset EnsurePipeline(ScriptableRendererData renderer)
        {
            var existing = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (existing != null)
            {
                Log($"pipeline exists at {PipelinePath}");
                return existing;
            }

            // Use the URP factory so the renderer slot wires up correctly via SerializedObject.
            var pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "URP_Pipeline";
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
            Log($"pipeline created at {PipelinePath}");
            return pipeline;
        }

        private static void AssignToGraphicsSettings(RenderPipelineAsset pipeline)
        {
            // Unity 6 (URP 17): use the serialized GraphicsSettings asset directly so
            // batchmode persists the assignment to ProjectSettings/GraphicsSettings.asset.
            var gsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/GraphicsSettings.asset");
            if (gsAsset == null)
            {
                // Fallback path for older Unity layouts.
                GraphicsSettings.defaultRenderPipeline = pipeline;
                Log("graphics settings: defaultRenderPipeline assigned via runtime API (fallback)");
                return;
            }

            var so = new SerializedObject(gsAsset);
            var prop = so.FindProperty("m_CustomRenderPipeline");
            if (prop == null)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
                Log("graphics settings: m_CustomRenderPipeline property not found, used runtime API");
                return;
            }
            prop.objectReferenceValue = pipeline;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(gsAsset);
            Log("graphics settings: m_CustomRenderPipeline assigned");
        }

        private static void AssignToQualityLevels(RenderPipelineAsset pipeline)
        {
            var qsAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/QualitySettings.asset");
            if (qsAsset == null)
            {
                Log("quality settings: asset not found, skipping per-level assign");
                return;
            }

            var so = new SerializedObject(qsAsset);
            var levels = so.FindProperty("m_QualitySettings");
            if (levels == null || !levels.isArray)
            {
                Log("quality settings: m_QualitySettings array not found");
                return;
            }

            int n = levels.arraySize;
            for (int i = 0; i < n; i++)
            {
                var lvl = levels.GetArrayElementAtIndex(i);
                var rp  = lvl.FindPropertyRelative("customRenderPipeline");
                if (rp != null)
                {
                    rp.objectReferenceValue = pipeline;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(qsAsset);
            Log($"quality settings: customRenderPipeline assigned on {n} level(s)");
        }

        private static void Log(string msg) => Debug.Log($"[SetupURP] {msg}");
    }
}
#endif
