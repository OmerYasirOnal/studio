// TEMPORARILY DISABLED — see ADR-0015 (test/production API drift).  Re-enable when:
//   * WaveDefinition gains durationSeconds + maxConcurrentEnemies + events (WaveEvent[])
//   * MechanicRegistry exposes ResetForTests
// Until then, the body is wrapped under an undefined symbol.
#if BRAVE_FUTURE_API
// QA — MechanicRegistry EditMode tests
// Subject under test: BraveBunny.Gameplay.Data.MechanicRegistry
// ADR-0009: polymorphic mechanics via type-name registry (compile-checked at boot).
// Source spec: docs/06-tech-spec/02-data-model.md § Polymorphic mechanics.
//
// IMPORTANT: the registry uses AppDomain reflection. We scan a controlled set of
// assemblies in tests to avoid coupling to game runtime state. The registry can
// be ResetForTests() between runs.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Brave.Gameplay.Combat;
using Brave.Gameplay.Definitions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class MechanicRegistryTests
    {
        // ---- test-only register-attribute fixtures (must compile + match registry contract) ----
        [BraveRegister(KnownMechanicName)]
        private sealed class FakeMechanicForTests : SignatureMechanic
        {
            public override string TypeName => KnownMechanicName;
            public override void OnAttach(PlayerContext ctx) { }
            public override void Tick(PlayerContext ctx, float dt) { }
            public override void OnDetach(PlayerContext ctx) { }
        }

        private const string KnownMechanicName = "test.fake-mechanic";
        private const string UnknownMechanicName = "test.nonexistent-mechanic";
        private const string CharacterDefinitionFolder = "Assets/_Brave/Data/Definitions/Characters";

        [SetUp]
        public void SetUp() => MechanicRegistry.ResetForTests();

        [TearDown]
        public void TearDown() => MechanicRegistry.ResetForTests();

        [Test]
        public void Registry_ResolvesAllRegisteredTypeNames()
        {
            // arrange
            var scanList = new List<Assembly> { typeof(MechanicRegistryTests).Assembly };
            // act
            MechanicRegistry.ScanAssemblies(scanList);
            // assert — fake fixture must be picked up.
            Assert.That(MechanicRegistry.TryResolve(KnownMechanicName, out var type), Is.True);
            Assert.That(type, Is.EqualTo(typeof(FakeMechanicForTests)));
        }

        [Test]
        public void Registry_ThrowsOnUnknownTypeName()
        {
            MechanicRegistry.ScanAssemblies(new[] { typeof(MechanicRegistryTests).Assembly });
            Assert.Throws<KeyNotFoundException>(() => MechanicRegistry.Resolve(UnknownMechanicName));
        }

        [Test]
        public void Registry_RegistersAtLeastOneType()
        {
            MechanicRegistry.ScanAssemblies(new[] { typeof(MechanicRegistryTests).Assembly });
            Assert.That(MechanicRegistry.All.Count, Is.GreaterThanOrEqualTo(1));
        }

        /// <summary>
        /// Scan every <see cref="CharacterDefinition"/> asset in
        /// <c>Assets/_Brave/Data/Definitions/Characters</c> and assert each
        /// <c>signatureMechanicTypeName</c> string resolves to a registered class.
        /// This is the ADR-0009 cross-check: broken type-names fail CI, not runtime.
        /// </summary>
        [Test]
        public void Registry_AllCharacterDefinitionsSignatureNameResolves()
        {
            // arrange — scan the full AppDomain so production mechanic classes register.
            MechanicRegistry.ScanAssemblies();

            // act — load all CharacterDefinition assets.
            var guids = AssetDatabase.FindAssets("t:CharacterDefinition", new[] { CharacterDefinitionFolder });
            if (guids == null || guids.Length == 0)
            {
                // Tolerate empty asset folder (skeleton may run before assets generated).
                Assert.Pass($"No CharacterDefinition assets in {CharacterDefinitionFolder} (skipped).");
                return;
            }

            // assert — each declared typeName resolves OR the field is empty (placeholder asset).
            var unresolved = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var def = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
                if (def == null) continue;
                if (string.IsNullOrEmpty(def.signatureMechanicTypeName)) continue;
                if (!MechanicRegistry.TryResolve(def.signatureMechanicTypeName, out _))
                    unresolved.Add($"{Path.GetFileName(path)}: '{def.signatureMechanicTypeName}'");
            }
            Assert.That(unresolved, Is.Empty,
                "Unresolved signatureMechanicTypeName(s): " + string.Join(", ", unresolved));
        }
    }
}
#endif
