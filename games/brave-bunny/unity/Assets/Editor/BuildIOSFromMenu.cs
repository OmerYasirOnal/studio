#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BraveBunny.Editor;

namespace BraveBunny.Editor.MenuBuild
{
    public static class BuildIOSFromMenu
    {
        [MenuItem("BraveBunny/MVP/Build iOS Xcode Project (in-editor)")]
        public static void BuildIOS()
        {
            // 1. Make sure URP + scene wiring are applied first.
            try { SetupURP.Run(); } catch (System.Exception e) { Debug.LogWarning($"[MvpBuild] SetupURP skipped: {e.Message}"); }
            try { SceneSetup.EnsurePlayableMvpRun(); } catch (System.Exception e) { Debug.LogWarning($"[MvpBuild] EnsurePlayableMvpRun skipped: {e.Message}"); }

            EditorSceneManager.SaveOpenScenes();

            // 2. Build iOS player to ../unity/Build/iOS
            string outDir = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(Application.dataPath, "..", "Build", "iOS"));
            System.IO.Directory.CreateDirectory(outDir);

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            var report = BuildPipeline.BuildPlayer(
                EditorBuildSettings.scenes,
                outDir,
                BuildTarget.iOS,
                BuildOptions.None
            );
            Debug.Log($"[MvpBuild] iOS build result: {report.summary.result} at {outDir}");
        }
    }
}
#endif
