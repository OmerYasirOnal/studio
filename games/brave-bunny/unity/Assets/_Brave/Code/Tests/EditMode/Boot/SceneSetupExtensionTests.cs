// QA — SceneSetup extension tests (Wave 7A + 7B wiring).
//
// Subject under test:
//   * Brave.Boot.Editor.RunSceneWiringSetup — Editor-only helpers that attach Wave 7A
//     + 7B service components (RunSceneWiring, GameplayAudioBindings, HitstopServiceHost,
//     ScreenShakeController, DamageNumberSpawner, BossSpawner container, PauseController,
//     LevelUpDraftController, RunEndTallyController, HomeController, LoadoutController)
//     to the active scene's GameObject hierarchy.
//
// Pattern: each test opens a fresh empty scene via EditorSceneManager.NewScene, invokes
// the helper under test, and asserts the expected GameObjects + Components were attached
// by fully-qualified type-name (so this test asmdef does not have to compile against
// Brave.UI / Brave.Gameplay / Brave.Systems just to look them up).
//
// Idempotence: each helper is invoked twice and the resulting component-count is
// asserted equal. This is the "running SceneSetup twice doesn't duplicate components"
// contract from the task brief.

#if UNITY_EDITOR
#nullable enable

using System;
using Brave.Boot.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Brave.Tests.EditMode.Boot
{
    [TestFixture]
    public class SceneSetupExtensionTests
    {
        // Fully-qualified component type names that each helper is expected to attach.
        private const string TypeRunSceneWiring         = "Brave.Boot.RunSceneWiring";
        private const string TypeGameplayAudioBindings  = "Brave.Systems.Audio.GameplayAudioBindings";
        private const string TypeHitstopServiceHost     = "Brave.Gameplay.Feel.HitstopServiceHost";
        private const string TypeScreenShakeController  = "Brave.Gameplay.Feel.ScreenShakeController";
        private const string TypeDamageNumberSpawner    = "Brave.Gameplay.Feel.DamageNumberSpawner";
        private const string TypePauseController        = "Brave.UI.Controllers.PauseController";
        private const string TypeLevelUpDraftController = "Brave.UI.Controllers.LevelUpDraftController";
        private const string TypeRunEndTallyController  = "Brave.UI.Controllers.RunEndTallyController";
        private const string TypeHomeController         = "Brave.UI.Controllers.HomeController";
        private const string TypeLoadoutController      = "Brave.UI.Controllers.LoadoutController";

        private UnityEngine.SceneManagement.Scene _scene;

        [SetUp]
        public void SetUp()
        {
            // Fresh, in-memory empty scene per test so GameObject.Find + scene-root scans
            // don't see leftovers from the previous test.
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            // Empty scene; closing without saving leaves no asset behind.
            // Note: NewScene during the next SetUp implicitly discards this one.
        }

        // ---------------------------------------------------------------------------
        //  Run-scene helpers
        // ---------------------------------------------------------------------------

        [Test]
        public void WireRunScene_AttachesAllWave7AAnd7BRootGameObjects()
        {
            RunSceneWiringSetup.WireRunScene();

            // Wave 7A juice
            AssertRootExists("[Wiring]");
            AssertRootExists("[GameplayAudio]");
            AssertRootExists("[HitstopHost]");
            AssertRootExists("[ScreenShake]");
            AssertRootExists("[FeelRoot]");
            AssertRootExists("Bosses");

            // Wave 7B UI modals
            AssertRootExists("[PauseUI]");
            AssertRootExists("[LevelUpUI]");
            AssertRootExists("[RunEndUI]");
        }

        [Test]
        public void EnsureWiringRoot_AttachesRunSceneWiringComponent()
        {
            var go = RunSceneWiringSetup.EnsureWiringRoot();
            AssertComponentByTypeName(go, TypeRunSceneWiring);
        }

        [Test]
        public void EnsureGameplayAudioBindings_AttachesComponent()
        {
            var go = RunSceneWiringSetup.EnsureGameplayAudioBindings();
            AssertComponentByTypeName(go, TypeGameplayAudioBindings);
        }

        [Test]
        public void EnsureHitstopServiceHost_AttachesComponent()
        {
            var go = RunSceneWiringSetup.EnsureHitstopServiceHost();
            AssertComponentByTypeName(go, TypeHitstopServiceHost);
        }

        [Test]
        public void EnsureScreenShakeController_AttachesComponent()
        {
            var go = RunSceneWiringSetup.EnsureScreenShakeController();
            AssertComponentByTypeName(go, TypeScreenShakeController);
        }

        [Test]
        public void EnsureFeelRootWithDamageNumberSpawner_AttachesChildAndComponent()
        {
            var feelRoot = RunSceneWiringSetup.EnsureFeelRootWithDamageNumberSpawner();
            Assert.IsNotNull(feelRoot, "[FeelRoot] missing");

            var child = feelRoot.transform.Find("DamageNumberSpawner");
            Assert.IsNotNull(child, "DamageNumberSpawner child missing under [FeelRoot]");
            AssertComponentByTypeName(child!.gameObject, TypeDamageNumberSpawner);
        }

        [Test]
        public void EnsureBossesContainer_CreatesEmptyRoot()
        {
            var go = RunSceneWiringSetup.EnsureBossesContainer();
            Assert.IsNotNull(go);
            Assert.AreEqual("Bosses", go.name);
            // No component is attached: BossSpawner is a plain C# class constructed by
            // the run-runtime; the container is a marker for the boss instance parent.
        }

        [Test]
        public void EnsurePauseControllerWithUxml_AttachesUIDocumentAndController()
        {
            var go = RunSceneWiringSetup.EnsurePauseControllerWithUxml();
            Assert.IsNotNull(go.GetComponent<UIDocument>(), "UIDocument missing on [PauseUI]");
            AssertComponentByTypeName(go, TypePauseController);
        }

        [Test]
        public void EnsureLevelUpDraftControllerWithUxml_AttachesUIDocumentAndController()
        {
            var go = RunSceneWiringSetup.EnsureLevelUpDraftControllerWithUxml();
            Assert.IsNotNull(go.GetComponent<UIDocument>(), "UIDocument missing on [LevelUpUI]");
            AssertComponentByTypeName(go, TypeLevelUpDraftController);
        }

        [Test]
        public void EnsureRunEndTallyControllerWithUxml_AttachesUIDocumentAndController()
        {
            var go = RunSceneWiringSetup.EnsureRunEndTallyControllerWithUxml();
            Assert.IsNotNull(go.GetComponent<UIDocument>(), "UIDocument missing on [RunEndUI]");
            AssertComponentByTypeName(go, TypeRunEndTallyController);
        }

        // ---------------------------------------------------------------------------
        //  MainMenu / Loadout helpers
        // ---------------------------------------------------------------------------

        [Test]
        public void EnsureHomeControllerWithUxml_AttachesUIDocumentAndController()
        {
            var go = RunSceneWiringSetup.EnsureHomeControllerWithUxml();
            Assert.IsNotNull(go.GetComponent<UIDocument>(), "UIDocument missing on [HomeUI]");
            AssertComponentByTypeName(go, TypeHomeController);
        }

        [Test]
        public void EnsureLoadoutControllerWithUxml_AttachesUIDocumentAndController()
        {
            var go = RunSceneWiringSetup.EnsureLoadoutControllerWithUxml();
            Assert.IsNotNull(go.GetComponent<UIDocument>(), "UIDocument missing on [LoadoutUI]");
            AssertComponentByTypeName(go, TypeLoadoutController);
        }

        // ---------------------------------------------------------------------------
        //  Idempotence — calling each helper twice must not duplicate components
        // ---------------------------------------------------------------------------

        [Test]
        public void WireRunScene_IsIdempotent_NoDuplicateComponentsAfterSecondCall()
        {
            RunSceneWiringSetup.WireRunScene();
            int firstPassRootCount = CountRootGameObjects();

            RunSceneWiringSetup.WireRunScene();
            int secondPassRootCount = CountRootGameObjects();

            Assert.AreEqual(firstPassRootCount, secondPassRootCount,
                "WireRunScene added duplicate root GameObjects on the second call.");

            // Each of the named roots must still carry exactly one of its expected
            // component type. We sample the easy-to-resolve ones; the rest follow the
            // same code path (AttachComponentByTypeName already guards via GetComponent).
            AssertExactlyOneComponentByTypeName("[Wiring]", TypeRunSceneWiring);
            AssertExactlyOneComponentByTypeName("[GameplayAudio]", TypeGameplayAudioBindings);
            AssertExactlyOneComponentByTypeName("[PauseUI]", TypePauseController);
        }

        [Test]
        public void EnsureHitstopServiceHost_IsIdempotent()
        {
            var firstCall = RunSceneWiringSetup.EnsureHitstopServiceHost();
            var secondCall = RunSceneWiringSetup.EnsureHitstopServiceHost();
            Assert.AreSame(firstCall, secondCall,
                "Second call should re-use the same GameObject, not create a new one.");
            AssertExactlyOneComponentByTypeName("[HitstopHost]", TypeHitstopServiceHost);
        }

        [Test]
        public void EnsureUiControllerRoot_IsIdempotent_ForUIDocumentToo()
        {
            var first  = RunSceneWiringSetup.EnsurePauseControllerWithUxml();
            var second = RunSceneWiringSetup.EnsurePauseControllerWithUxml();
            Assert.AreSame(first, second);

            // UIDocument must appear exactly once after two passes.
            var uiDocs = first.GetComponents<UIDocument>();
            Assert.AreEqual(1, uiDocs.Length, "UIDocument duplicated on second wiring pass.");
        }

        // ---------------------------------------------------------------------------
        //  Shared assertion helpers
        // ---------------------------------------------------------------------------

        private static void AssertRootExists(string name)
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return;
            }
            Assert.Fail($"Expected root GameObject '{name}' in active scene; not found.");
        }

        private static void AssertComponentByTypeName(GameObject go, string typeName)
        {
            var type = ResolveType(typeName);
            Assert.IsNotNull(type, $"Type {typeName} could not be resolved.");
            var comp = go.GetComponent(type!);
            Assert.IsNotNull(comp, $"Expected component {typeName} on GameObject '{go.name}'.");
        }

        private static void AssertExactlyOneComponentByTypeName(string rootName, string typeName)
        {
            var type = ResolveType(typeName);
            Assert.IsNotNull(type, $"Type {typeName} could not be resolved.");
            GameObject? root = null;
            var scene = SceneManager.GetActiveScene();
            foreach (var r in scene.GetRootGameObjects())
            {
                if (r.name == rootName) { root = r; break; }
            }
            Assert.IsNotNull(root, $"Root GameObject '{rootName}' missing.");
            var comps = root!.GetComponents(type!);
            Assert.AreEqual(1, comps.Length,
                $"Expected exactly 1 of {typeName} on '{rootName}' but found {comps.Length}.");
        }

        private static int CountRootGameObjects()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects().Length;
        }

        private static Type? ResolveType(string fullName)
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
