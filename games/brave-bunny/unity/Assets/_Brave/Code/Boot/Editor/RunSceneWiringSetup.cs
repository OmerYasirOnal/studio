#if UNITY_EDITOR
// Brave Bunny — Editor / RunSceneWiringSetup
//
// Wave 7A + 7B service wiring helpers, invoked by SceneSetup.cs when (re-)building
// Run.unity / MainMenu.unity / Loadout.unity. Pulled out of SceneSetup.cs so:
//   * The helpers live in an asmdef (Brave.Boot.Editor) that EditMode tests can
//     reference. SceneSetup.cs itself sits in the default Assembly-CSharp-Editor
//     because it predates the asmdef move.
//   * The helpers stay small + composable: each owns one logical attachment.
//
// What this wires (Run scene):
//   * RunSceneWiring          on a root "[Wiring]" GameObject
//   * GameplayAudioBindings   on a root "[GameplayAudio]" GameObject
//   * HitstopServiceHost      on a root "[HitstopHost]" GameObject
//   * ScreenShakeController   on a root "[ScreenShake]" GameObject
//   * DamageNumberSpawner     under a root "[FeelRoot]" GameObject
//   * "Bosses" container       — BossSpawner is plain C# (constructed at runtime),
//                                 so we leave an empty parent ready to receive boss
//                                 enemy instances; an EnemyPool-binding placeholder
//                                 stub component is added if/when the runtime host
//                                 ships (logged warning until then).
//   * PauseController         on a "[PauseUI]" GameObject (UIDocument + Pause.uxml)
//   * LevelUpDraftController  on a "[LevelUpUI]" GameObject (UIDocument + LevelUpDraft.uxml)
//   * RunEndTallyController   on a "[RunEndUI]" GameObject (UIDocument + RunEndTally.uxml)
//
// What this wires (MainMenu/Loadout scenes):
//   * HomeController          on a "[HomeUI]" GameObject (UIDocument + Home.uxml)
//   * LoadoutController       on a "[LoadoutUI]" GameObject (UIDocument + Loadout.uxml)
//
// Idempotence: every helper checks GetComponent before AddComponent, and reuses an
// existing child GameObject by name when present. Re-running the scaffold leaves
// the scene's component count unchanged.
//
// Missing assets / types: graceful degradation. Every lookup is wrapped in a
// Debug.LogWarning so the scene file still saves even when an asset or type can't
// be resolved (matches the existing SceneSetup contract).

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Brave.Boot.Editor
{
    /// <summary>
    /// Editor-time helpers that attach Wave 7A + 7B service components to a Unity scene.
    /// All methods are idempotent; SceneSetup invokes them every time it rebuilds a scene.
    /// </summary>
    public static class RunSceneWiringSetup
    {
        // ---- UXML asset paths ----------------------------------------------------------------
        public const string PauseUxmlPath        = "Assets/_Brave/UI/Documents/Pause.uxml";
        public const string LevelUpDraftUxmlPath = "Assets/_Brave/UI/Documents/LevelUpDraft.uxml";
        public const string RunEndTallyUxmlPath  = "Assets/_Brave/UI/Documents/RunEndTally.uxml";
        public const string HomeUxmlPath         = "Assets/_Brave/UI/Documents/Home.uxml";
        public const string LoadoutUxmlPath      = "Assets/_Brave/UI/Documents/Loadout.uxml";

        // ---- Balance / config asset paths ----------------------------------------------------
        public const string FeelConfigAssetPath  = "Assets/_Brave/Data/Definitions/Feel/FeelConfig.asset";

        // ---- Wave 7A + 7B fully-qualified type names ----------------------------------------
        // (Resolved via reflection so this Editor-only asmdef does not force a hard reference
        // on Brave.UI / Brave.Gameplay / Brave.Systems just to attach a component.)
        private const string TypeRunSceneWiring         = "Brave.Boot.RunSceneWiring";
        private const string TypeGameplayAudioBindings  = "Brave.Systems.Audio.GameplayAudioBindings";
        private const string TypeHitstopServiceHost     = "Brave.Gameplay.Feel.HitstopServiceHost";
        private const string TypeScreenShakeController  = "Brave.Gameplay.Feel.ScreenShakeController";
        private const string TypeDamageNumberSpawner    = "Brave.Gameplay.Feel.DamageNumberSpawner";
        private const string TypeDamageNumberPool       = "Brave.Gameplay.Feel.DamageNumberPool";
        private const string TypePauseController        = "Brave.UI.Controllers.PauseController";
        private const string TypeLevelUpDraftController = "Brave.UI.Controllers.LevelUpDraftController";
        private const string TypeRunEndTallyController  = "Brave.UI.Controllers.RunEndTallyController";
        private const string TypeHomeController         = "Brave.UI.Controllers.HomeController";
        private const string TypeLoadoutController      = "Brave.UI.Controllers.LoadoutController";

        // =====================================================================================
        //  Run scene
        // =====================================================================================

        /// <summary>
        /// Attaches every Wave 7A + 7B service component to <paramref name="sceneRoot"/>.
        /// Pass any GameObject in the open scene (typically a freshly-created marker); each
        /// helper creates its own child container so call-order is irrelevant.
        /// </summary>
        /// <param name="sceneRootHintIgnored">
        /// Unused; helpers create their own root GameObjects in the active scene. Kept on the
        /// signature so the caller's intent ("set up the Run scene") is explicit at the call
        /// site.
        /// </param>
        public static void WireRunScene(GameObject? sceneRootHintIgnored = null)
        {
            EnsureWiringRoot();
            EnsureGameplayAudioBindings();
            EnsureHitstopServiceHost();
            EnsureScreenShakeController();
            EnsureFeelRootWithDamageNumberSpawner();
            EnsureBossesContainer();
            EnsurePauseControllerWithUxml();
            EnsureLevelUpDraftControllerWithUxml();
            EnsureRunEndTallyControllerWithUxml();
        }

        /// <summary>[Wiring] root GameObject + RunSceneWiring component.</summary>
        public static GameObject EnsureWiringRoot()
        {
            var go = FindOrCreateRoot("[Wiring]");
            AttachComponentByTypeName(go, TypeRunSceneWiring);
            return go;
        }

        /// <summary>[GameplayAudio] root + GameplayAudioBindings component.</summary>
        public static GameObject EnsureGameplayAudioBindings()
        {
            var go = FindOrCreateRoot("[GameplayAudio]");
            AttachComponentByTypeName(go, TypeGameplayAudioBindings);
            return go;
        }

        /// <summary>[HitstopHost] root + HitstopServiceHost component (Wave 7A juice).</summary>
        public static GameObject EnsureHitstopServiceHost()
        {
            var go = FindOrCreateRoot("[HitstopHost]");
            var comp = AttachComponentByTypeName(go, TypeHitstopServiceHost);
            if (comp != null)
            {
                AssignSerializedObjectReference(comp, "_config", LoadAssetByPath(FeelConfigAssetPath));
            }
            return go;
        }

        /// <summary>[ScreenShake] root + ScreenShakeController component (Wave 7A juice).</summary>
        public static GameObject EnsureScreenShakeController()
        {
            var go = FindOrCreateRoot("[ScreenShake]");
            var comp = AttachComponentByTypeName(go, TypeScreenShakeController);
            if (comp != null)
            {
                AssignSerializedObjectReference(comp, "_config", LoadAssetByPath(FeelConfigAssetPath));
            }
            return go;
        }

        /// <summary>
        /// [FeelRoot] root with a child carrying DamageNumberSpawner. Wave 7A spawns
        /// floating damage numbers under this parent so the camera-frustum cull keeps
        /// them grouped with the gameplay layer.
        /// </summary>
        public static GameObject EnsureFeelRootWithDamageNumberSpawner()
        {
            var feelRoot = FindOrCreateRoot("[FeelRoot]");

            // Child GO that hosts the spawner — kept under FeelRoot so re-parenting at
            // runtime (RunSceneWiring moves the pool under the player) only touches the
            // child, not the root.
            var spawnerGo = FindOrCreateChild(feelRoot, "DamageNumberSpawner");
            var spawnerComp = AttachComponentByTypeName(spawnerGo, TypeDamageNumberSpawner);

            if (spawnerComp != null)
            {
                AssignSerializedObjectReference(spawnerComp, "_config", LoadAssetByPath(FeelConfigAssetPath));
            }

            return feelRoot;
        }

        /// <summary>
        /// Empty "Bosses" container ready to host the boss enemy on its t=420 trigger.
        /// BossSpawner itself is a plain C# class constructed by the run-runtime (Wave 7B
        /// follow-up), so no component is attached here. The marker keeps the hierarchy
        /// self-documenting and gives the runtime a stable parent to instantiate under.
        /// </summary>
        public static GameObject EnsureBossesContainer()
        {
            return FindOrCreateRoot("Bosses");
        }

        /// <summary>[PauseUI] root + UIDocument + PauseController (Pause.uxml).</summary>
        public static GameObject EnsurePauseControllerWithUxml()
        {
            return EnsureUiControllerRoot("[PauseUI]", TypePauseController, PauseUxmlPath);
        }

        /// <summary>[LevelUpUI] root + UIDocument + LevelUpDraftController (LevelUpDraft.uxml).</summary>
        public static GameObject EnsureLevelUpDraftControllerWithUxml()
        {
            return EnsureUiControllerRoot("[LevelUpUI]", TypeLevelUpDraftController, LevelUpDraftUxmlPath);
        }

        /// <summary>[RunEndUI] root + UIDocument + RunEndTallyController (RunEndTally.uxml).</summary>
        public static GameObject EnsureRunEndTallyControllerWithUxml()
        {
            return EnsureUiControllerRoot("[RunEndUI]", TypeRunEndTallyController, RunEndTallyUxmlPath);
        }

        // =====================================================================================
        //  MainMenu / Loadout
        // =====================================================================================

        /// <summary>[HomeUI] root + UIDocument + HomeController (Home.uxml).</summary>
        public static GameObject EnsureHomeControllerWithUxml()
        {
            return EnsureUiControllerRoot("[HomeUI]", TypeHomeController, HomeUxmlPath);
        }

        /// <summary>[LoadoutUI] root + UIDocument + LoadoutController (Loadout.uxml).</summary>
        public static GameObject EnsureLoadoutControllerWithUxml()
        {
            return EnsureUiControllerRoot("[LoadoutUI]", TypeLoadoutController, LoadoutUxmlPath);
        }

        // =====================================================================================
        //  Shared helpers
        // =====================================================================================

        /// <summary>
        /// Creates (or re-uses) a root GameObject in the open scene named
        /// <paramref name="rootName"/>. Idempotent — re-running the scaffold doesn't add
        /// duplicates.
        /// </summary>
        public static GameObject FindOrCreateRoot(string rootName)
        {
            // GameObject.Find can return inactive children only when searching the full
            // hierarchy, but for our scaffold pass everything is freshly active. Falling
            // back to a manual scene-root scan keeps the helper robust under EditMode tests
            // where Find may not see un-saved scenes.
            var existing = GameObject.Find(rootName);
            if (existing != null) return existing;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == rootName) return root;
            }
            return new GameObject(rootName);
        }

        private static GameObject FindOrCreateChild(GameObject parent, string childName)
        {
            var existing = parent.transform.Find(childName);
            if (existing != null) return existing.gameObject;
            var go = new GameObject(childName);
            go.transform.SetParent(parent.transform, worldPositionStays: false);
            return go;
        }

        /// <summary>
        /// Adds <paramref name="typeName"/> as a Component on <paramref name="go"/> if not
        /// already present. Idempotent. Returns the live component (existing or new).
        /// Returns <c>null</c> when the type can't be resolved (asmdef not loaded, e.g.
        /// during partial-compilation EditMode runs).
        /// </summary>
        public static Component? AttachComponentByTypeName(GameObject go, string typeName)
        {
            var type = FindType(typeName);
            if (type == null)
            {
                Debug.LogWarning($"[RunSceneWiringSetup] Type {typeName} not resolved — component not attached.");
                return null;
            }

            var existing = go.GetComponent(type);
            if (existing != null) return existing;

            return go.AddComponent(type);
        }

        /// <summary>
        /// Attaches a UIDocument + the named controller MonoBehaviour to a child
        /// GameObject, assigning <paramref name="uxmlPath"/> as the visual tree asset.
        /// Idempotent (re-uses existing children + components).
        /// </summary>
        public static GameObject EnsureUiControllerRoot(
            string rootName,
            string controllerTypeName,
            string uxmlPath)
        {
            var go = FindOrCreateRoot(rootName);

            // UIDocument must exist before the controller (controllers carry
            // [RequireComponent(typeof(UIDocument))], so AddComponent of the controller
            // would auto-add a UIDocument anyway — but we need the explicit handle to
            // assign the visual tree asset).
            var uiDoc = go.GetComponent<UIDocument>();
            if (uiDoc == null) uiDoc = go.AddComponent<UIDocument>();

            var vt = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (vt == null)
            {
                Debug.LogWarning(
                    $"[RunSceneWiringSetup] {uxmlPath} not found — UIDocument on '{rootName}' will be empty.");
            }
            else
            {
                AssignSerializedObjectReference(uiDoc, "m_VisualTreeAsset", vt);
            }

            AttachComponentByTypeName(go, controllerTypeName);
            return go;
        }

        /// <summary>Load an asset by AssetDatabase path. Returns null when the asset is missing.</summary>
        public static UnityEngine.Object? LoadAssetByPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }

        /// <summary>
        /// Assigns <paramref name="value"/> to the SerializedProperty
        /// <paramref name="propertyName"/> on <paramref name="target"/>. No-op when the
        /// property is missing or the value is null.
        /// </summary>
        public static void AssignSerializedObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object? value)
        {
            if (target == null || value == null) return;
            var so = new SerializedObject(target);
            var prop = so.FindProperty(propertyName);
            if (prop == null) return;
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Resolve a Type from any loaded assembly. Mirrors SceneSetup.FindType so the two
        /// helpers stay consistent in their cross-asmdef lookup contract.
        /// </summary>
        public static Type? FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, throwOnError: false);
                if (t != null) return t;
            }
            return null;
        }
    }
}
#endif
