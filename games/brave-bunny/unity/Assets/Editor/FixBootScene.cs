#if UNITY_EDITOR
// Wave 13.5 — Boot scene re-wiring utility.
//
// Symptom this fixes: Build #4 (and TestFlight builds before it) ship with
// "Script attached to '[Bootstrap]' in scene 'Assets/_Brave/Scenes/Boot.unity'
// is missing or no valid script is attached." The on-device result is an
// orange (peach/bunny-cream) screen forever — Boot.unity's MainCamera shows
// its solid-color background and no scene transition is ever triggered.
//
// Root cause: the MonoBehaviour record on the [Bootstrap] GameObject has the
// correct script GUID (76e12912107b546a9ae448d6e9120e8d -> GameContextBootstrap.cs.meta)
// but Unity's MonoScript -> Type binding has gone stale (likely because the class
// accumulated many SerializeFields since the scene was first saved, and the cached
// MonoScript identity at &581996090 inside the scene YAML diverged from the live
// assembly). The instance never gets created at scene-load, so Awake() never runs.
//
// Fix: scrub missing-script entries off [Bootstrap], then add a fresh
// GameContextBootstrap component via the live type reference. Re-save the scene.
// Run via menu: Brave > MVP > Fix Boot Scene (or CLI -executeMethod BraveBunny.Editor.FixBootScene.Fix).

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BraveBunny.Editor
{
    public static class FixBootScene
    {
        private const string BootScenePath = "Assets/_Brave/Scenes/Boot.unity";
        private const string BootstrapTypeName = "Brave.Systems.Context.GameContextBootstrap, Brave.Systems";

        [MenuItem("Brave/MVP/Fix Boot Scene")]
        public static void Fix()
        {
            var scene = EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);

            var bootstrapGo = GameObject.Find("[Bootstrap]");
            if (bootstrapGo == null)
            {
                bootstrapGo = new GameObject("[Bootstrap]");
                Debug.Log("[FixBootScene] Created [Bootstrap] GameObject (was missing).");
            }

            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(bootstrapGo);
            if (removed > 0)
            {
                Debug.Log($"[FixBootScene] Removed {removed} missing-script entries from [Bootstrap].");
            }

            var bootstrapType = ResolveType(BootstrapTypeName);
            if (bootstrapType == null)
            {
                Debug.LogError(
                    $"[FixBootScene] Could not resolve type '{BootstrapTypeName}'. "
                    + "Check that Brave.Systems.asmdef compiled and GameContextBootstrap.cs is present.");
                return;
            }

            if (bootstrapGo.GetComponent(bootstrapType) == null)
            {
                bootstrapGo.AddComponent(bootstrapType);
                Debug.Log($"[FixBootScene] Added {bootstrapType.FullName} to [Bootstrap].");
            }
            else
            {
                Debug.Log($"[FixBootScene] [Bootstrap] already has {bootstrapType.FullName} — no-op.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FixBootScene] Boot.unity saved.");
        }

        private static Type ResolveType(string typeName)
        {
            var t = Type.GetType(typeName);
            if (t != null) return t;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType("Brave.Systems.Context.GameContextBootstrap");
                if (t != null) return t;
            }
            return null;
        }
    }
}
#endif
