// QA — WeaponArchetypeConfig EditMode tests
// Subject under test: Brave.Gameplay.Combat.Archetypes.* and the [BraveRegister]
// registry round-trip per ADR-0009 + ADR-0020.
// Source spec: docs/decisions/0020-weapon-archetype-config-and-boss-enum.md
//              §"Test surface that needs to exist after implementation".
//
// Test surface (per ADR-0020):
//   1. Each concrete subclass can be created as an SO and casts to its base type.
//   2. WeaponDefinition.archetypeConfig accepts each concrete subclass.
//   3. MechanicRegistry round-trips every [BraveRegister("weapon.archetype.*")] token.
//   4. Per-level arrays default to length 5 (matching WeaponDefinition.levels).
//   5. The registry exposes 7 distinct archetype type-name tokens.
//   6. No duplicate type-name strings across the 7 subclasses (ADR-0009 invariant).
//
// Reflection-reset mirrors MechanicRegistryTests so test ordering is isolated.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Brave.Gameplay.Combat;
using Brave.Gameplay.Combat.Archetypes;
using Brave.Gameplay.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Definitions
{
    [TestFixture]
    public class WeaponArchetypeConfigTests
    {
        // Expected [BraveRegister] tokens for the 7 archetype subclasses (ADR-0020).
        private static readonly string[] ExpectedTokens =
        {
            "weapon.archetype.projectile",
            "weapon.archetype.beam",
            "weapon.archetype.mine",
            "weapon.archetype.cloud",
            "weapon.archetype.splash_projectile",
            "weapon.archetype.aura",
            "weapon.archetype.summon",
        };

        // Concrete subclass types matching ExpectedTokens 1:1 (ordering preserved).
        private static readonly Type[] ExpectedSubclassTypes =
        {
            typeof(ProjectileArchetypeConfig),
            typeof(BeamArchetypeConfig),
            typeof(MineArchetypeConfig),
            typeof(CloudArchetypeConfig),
            typeof(SplashProjectileArchetypeConfig),
            typeof(AuraArchetypeConfig),
            typeof(SummonArchetypeConfig),
        };

        // ADR-0001 / tech-spec 02: weapons have EXACTLY 5 levels.
        private const int ExpectedLevelCount = 5;
        // ADR-0020 lists 7 concrete subclasses.
        private const int ExpectedArchetypeCount = 7;

        // --- private-field handles (mirror MechanicRegistryTests' reflection-reset) ----
        private static FieldInfo? _typesByNameField;
        private static FieldInfo? _initialisedField;

        private static FieldInfo TypesByNameField =>
            _typesByNameField ??= typeof(MechanicRegistry).GetField(
                "_typesByName", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MechanicRegistry._typesByName missing");

        private static FieldInfo InitialisedField =>
            _initialisedField ??= typeof(MechanicRegistry).GetField(
                "_initialised", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("MechanicRegistry._initialised missing");

        private static void ResetRegistry()
        {
            var dictObj = TypesByNameField.GetValue(null);
            if (dictObj is IDictionary dict) dict.Clear();
            InitialisedField.SetValue(null, false);
        }

        [SetUp] public void SetUp()       => ResetRegistry();
        [TearDown] public void TearDown() => ResetRegistry();

        // ----- SO creation / cast invariant (Assertion #1) -----

        [Test]
        public void EveryArchetypeSubclass_CanBeCreatedAsScriptableObject()
        {
            foreach (var t in ExpectedSubclassTypes)
            {
                var so = ScriptableObject.CreateInstance(t) as WeaponArchetypeConfig;
                Assert.That(so, Is.Not.Null,
                    $"ScriptableObject.CreateInstance({t.Name}) must produce a non-null WeaponArchetypeConfig");
                Assert.That(so, Is.InstanceOf(t),
                    $"Instance must be of expected runtime type {t.Name}");
                UnityEngine.Object.DestroyImmediate(so);
            }
        }

        // ----- WeaponDefinition.archetypeConfig wiring (Assertion #2) -----

        [Test]
        public void WeaponDefinition_AssignsEveryArchetypeSubclass_RoundTrip()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            try
            {
                foreach (var t in ExpectedSubclassTypes)
                {
                    var so = ScriptableObject.CreateInstance(t) as WeaponArchetypeConfig;
                    weapon.archetypeConfig = so;
                    Assert.That(weapon.archetypeConfig, Is.SameAs(so),
                        $"WeaponDefinition.archetypeConfig must round-trip {t.Name}");
                    UnityEngine.Object.DestroyImmediate(so);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(weapon);
            }
        }

        // ----- [BraveRegister] registry round-trip (Assertion #3) -----

        [Test]
        public void Registry_ResolvesEveryArchetypeToken()
        {
            MechanicRegistry.ScanAssemblies();
            foreach (var token in ExpectedTokens)
            {
                Assert.That(MechanicRegistry.TryResolve(token, out _), Is.True,
                    $"Expected [BraveRegister(\"{token}\")] to be discovered");
            }
        }

        [Test]
        public void Registry_ArchetypeTokens_ResolveToCorrectConcreteType()
        {
            MechanicRegistry.ScanAssemblies();
            for (int i = 0; i < ExpectedTokens.Length; i++)
            {
                var resolved = MechanicRegistry.Resolve(ExpectedTokens[i]);
                Assert.That(resolved, Is.EqualTo(ExpectedSubclassTypes[i]),
                    $"Token '{ExpectedTokens[i]}' must resolve to {ExpectedSubclassTypes[i].Name}");
            }
        }

        // ----- Per-level array length invariant (Assertion #4) -----

        [Test]
        public void MineArchetype_PerLevelArrayLength_IsFive()
        {
            var so = ScriptableObject.CreateInstance<MineArchetypeConfig>();
            try
            {
                Assert.That(so.armTimeMsPerLevel, Is.Not.Null);
                Assert.That(so.armTimeMsPerLevel.Length, Is.EqualTo(ExpectedLevelCount),
                    "ADR-0020: per-level overrides must be exactly 5 entries");
            }
            finally { UnityEngine.Object.DestroyImmediate(so); }
        }

        [Test]
        public void CloudArchetype_PerLevelArrayLengths_AreFive()
        {
            var so = ScriptableObject.CreateInstance<CloudArchetypeConfig>();
            try
            {
                Assert.That(so.cloudLifetimeMsPerLevel.Length, Is.EqualTo(ExpectedLevelCount));
                Assert.That(so.zapsPerCloudPerLevel.Length,    Is.EqualTo(ExpectedLevelCount));
            }
            finally { UnityEngine.Object.DestroyImmediate(so); }
        }

        [Test]
        public void SplashProjectileArchetype_PerLevelArrayLengths_AreFive()
        {
            var so = ScriptableObject.CreateInstance<SplashProjectileArchetypeConfig>();
            try
            {
                Assert.That(so.splashUnitsPerLevel.Length, Is.EqualTo(ExpectedLevelCount));
                Assert.That(so.travelMsPerLevel.Length,    Is.EqualTo(ExpectedLevelCount));
            }
            finally { UnityEngine.Object.DestroyImmediate(so); }
        }

        [Test]
        public void AuraArchetype_PerLevelArrayLengths_AreFive()
        {
            var so = ScriptableObject.CreateInstance<AuraArchetypeConfig>();
            try
            {
                Assert.That(so.slowPctPerLevel.Length,        Is.EqualTo(ExpectedLevelCount));
                Assert.That(so.tickLifetimeMsPerLevel.Length, Is.EqualTo(ExpectedLevelCount));
            }
            finally { UnityEngine.Object.DestroyImmediate(so); }
        }

        [Test]
        public void SummonArchetype_PerLevelArrayLength_IsFive()
        {
            var so = ScriptableObject.CreateInstance<SummonArchetypeConfig>();
            try
            {
                Assert.That(so.lifetimeMsPerLevel.Length, Is.EqualTo(ExpectedLevelCount));
            }
            finally { UnityEngine.Object.DestroyImmediate(so); }
        }

        // ----- Registry size + uniqueness invariants (Assertions #5, #6) -----

        [Test]
        public void Registry_ExposesExactlySevenArchetypeTokens()
        {
            MechanicRegistry.ScanAssemblies();
            int archetypeCount = MechanicRegistry.All.Keys
                .Count(k => k.StartsWith("weapon.archetype."));
            Assert.That(archetypeCount, Is.EqualTo(ExpectedArchetypeCount),
                $"ADR-0020 specifies exactly {ExpectedArchetypeCount} archetype subclasses");
        }

        [Test]
        public void ArchetypeTokens_AreUnique_NoDuplicateBraveRegisterStrings()
        {
            // ExpectedTokens itself must have no duplicates — the ADR-0009
            // BraveRegister invariant would also fail at ScanAssemblies() time
            // if duplicates existed, but assert the list integrity explicitly.
            Assert.That(ExpectedTokens.Distinct().Count(), Is.EqualTo(ExpectedTokens.Length),
                "ADR-0009: [BraveRegister] type-name strings must be unique");
        }

        // ----- WeaponArchetypeConfig.LevelCount constant invariant -----

        [Test]
        public void WeaponArchetypeConfig_LevelCount_MatchesWeaponDefinitionLevels()
        {
            Assert.That(WeaponArchetypeConfig.LevelCount, Is.EqualTo(ExpectedLevelCount),
                "ADR-0020: per-level array length must match WeaponDefinition.levels[5]");
        }
    }
}
