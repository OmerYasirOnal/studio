// QA — MechanicRegistry EditMode tests
// Subject under test: Brave.Gameplay.Combat.MechanicRegistry
// ADR-0009: polymorphic mechanics via type-name registry (compile-checked at boot).
// Source spec: docs/06-tech-spec/02-data-model.md § Polymorphic mechanics.
// ADR-0015: re-enabled per Path B — production exposes only ScanAssemblies / Resolve /
//           TryResolve / All. Test isolation uses BindingFlags.NonPublic reflection to
//           clear the registry's private backing fields between tests.
//
// Why reflection: MechanicRegistry.ResetForTests() is declared `internal`. The
// Brave.Tests.EditMode asmdef does not declare InternalsVisibleTo, so the public
// surface alone is consumed; isolation comes from private-reflection reset.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Brave.Gameplay.Combat;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class MechanicRegistryTests
    {
        // ---- expected production registry contents (8 character signature mechanics per ADR-0009) ----
        private const int ExpectedProductionMechanicCount = 8;

        // The full set of [BraveRegister] tokens defined under Gameplay/Characters/.
        // Asserting against the list — not just the count — catches both
        // "scanner missed a class" AND "rename without test update".
        private static readonly string[] ExpectedTokens =
        {
            "hop-dodge",        // BunnyHopDodge
            "shell-brace",      // TortoiseShellShield
            "cunning-strike",   // FoxExec
            "thorn-ring",       // HedgehogThornRing
            "splash-volley",    // OtterMultiShot
            "hearty-snack",     // PandaHealOnPickup
            "baby-patrol",      // BadgerSummonBaby
            "far-sight",        // OwlMagnet
        };

        private const string UnknownMechanicName = "test.nonexistent-mechanic";
        private const string MixedCaseToken      = "HOP-DODGE";   // upper-cased "hop-dodge"

        // ---- private-field handles (cached on first use) ----
        private static FieldInfo? _typesByNameField;
        private static FieldInfo? _initialisedField;

        private static FieldInfo TypesByNameField =>
            _typesByNameField ??= typeof(MechanicRegistry).GetField(
                "_typesByName",
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "MechanicRegistry._typesByName private field missing — registry shape changed");

        private static FieldInfo InitialisedField =>
            _initialisedField ??= typeof(MechanicRegistry).GetField(
                "_initialised",
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                "MechanicRegistry._initialised private field missing — registry shape changed");

        /// <summary>
        /// Reset the registry to "never scanned" state via reflection.
        /// Mirrors the internal ResetForTests() implementation byte-for-byte
        /// without needing InternalsVisibleTo on the test asmdef.
        /// </summary>
        private static void ResetRegistry()
        {
            var dictObj = TypesByNameField.GetValue(null);
            if (dictObj is IDictionary dict) dict.Clear();
            InitialisedField.SetValue(null, false);
        }

        [SetUp]
        public void SetUp()       => ResetRegistry();

        [TearDown]
        public void TearDown()    => ResetRegistry();

        // ---- core scan behaviour ----

        [Test]
        public void Registry_ScanAssemblies_RegistersAllExpectedTokens()
        {
            MechanicRegistry.ScanAssemblies();

            foreach (var token in ExpectedTokens)
            {
                Assert.That(
                    MechanicRegistry.TryResolve(token, out _), Is.True,
                    $"Expected [BraveRegister(\"{token}\")] to be discovered by ScanAssemblies()");
            }
        }

        [Test]
        public void Registry_All_ContainsAtLeastExpectedProductionCount()
        {
            MechanicRegistry.ScanAssemblies();
            // >= rather than == — test assembly may add fixtures; we lock the floor.
            Assert.That(MechanicRegistry.All.Count,
                Is.GreaterThanOrEqualTo(ExpectedProductionMechanicCount),
                $"Registry must contain at least {ExpectedProductionMechanicCount} production mechanics");
        }

        [Test]
        public void Registry_All_ContainsEveryExpectedToken()
        {
            MechanicRegistry.ScanAssemblies();
            var keys = MechanicRegistry.All.Keys.ToArray();
            foreach (var token in ExpectedTokens)
            {
                Assert.That(keys, Contains.Item(token),
                    $"Registry.All.Keys must include '{token}'");
            }
        }

        // ---- resolution semantics ----

        [Test]
        public void Registry_Resolve_KnownToken_ReturnsSignatureMechanicType()
        {
            MechanicRegistry.ScanAssemblies();
            var type = MechanicRegistry.Resolve(ExpectedTokens[0]);
            Assert.That(type, Is.Not.Null);
            Assert.That(typeof(SignatureMechanic).IsAssignableFrom(type), Is.True,
                $"Resolved type {type.FullName} must derive from SignatureMechanic");
        }

        [Test]
        public void Registry_Resolve_UnknownToken_Throws()
        {
            MechanicRegistry.ScanAssemblies();
            Assert.Throws<KeyNotFoundException>(
                () => MechanicRegistry.Resolve(UnknownMechanicName));
        }

        [Test]
        public void Registry_TryResolve_UnknownToken_ReturnsFalseAndNull()
        {
            MechanicRegistry.ScanAssemblies();
            bool ok = MechanicRegistry.TryResolve(UnknownMechanicName, out var type);
            Assert.That(ok, Is.False);
            Assert.That(type, Is.Null);
        }

        [Test]
        public void Registry_TryResolve_KnownToken_ReturnsTrueAndType()
        {
            MechanicRegistry.ScanAssemblies();
            bool ok = MechanicRegistry.TryResolve(ExpectedTokens[1], out var type);
            Assert.That(ok, Is.True);
            Assert.That(type, Is.Not.Null);
            Assert.That(typeof(SignatureMechanic).IsAssignableFrom(type), Is.True);
        }

        // ---- case-sensitivity (ADR-0009: tokens are exact-match strings) ----

        [Test]
        public void Registry_LookupIsCaseSensitive()
        {
            MechanicRegistry.ScanAssemblies();
            // Lower-case version is registered, upper-case must miss.
            Assert.That(MechanicRegistry.TryResolve(ExpectedTokens[0], out _), Is.True,
                "Lower-case canonical token must resolve");
            Assert.That(MechanicRegistry.TryResolve(MixedCaseToken, out _), Is.False,
                $"Mixed-case '{MixedCaseToken}' must NOT resolve — registry is case-sensitive");
        }

        // ---- construct (instance creation) ----

        [Test]
        public void Registry_Construct_KnownToken_ReturnsInstance()
        {
            MechanicRegistry.ScanAssemblies();
            var instance = MechanicRegistry.Construct(ExpectedTokens[0]);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<SignatureMechanic>());
            Assert.That(instance.TypeName, Is.EqualTo(ExpectedTokens[0]),
                "Constructed mechanic's TypeName must round-trip its registration token");
        }

        [Test]
        public void Registry_Construct_UnknownToken_Throws()
        {
            MechanicRegistry.ScanAssemblies();
            Assert.Throws<KeyNotFoundException>(
                () => MechanicRegistry.Construct(UnknownMechanicName));
        }

        // ---- isolation contract (the reset itself) ----

        [Test]
        public void Registry_ResetThenScan_RebuildsTable()
        {
            // First scan populates.
            MechanicRegistry.ScanAssemblies();
            int firstCount = MechanicRegistry.All.Count;
            Assert.That(firstCount, Is.GreaterThan(0));

            // Manual reset between scans (mirrors SetUp behaviour mid-test).
            ResetRegistry();

            // After reset the dictionary is empty and _initialised is false.
            var dictObj = TypesByNameField.GetValue(null);
            Assert.That(((IDictionary)dictObj!).Count, Is.EqualTo(0),
                "Reset must clear the backing dictionary");
            Assert.That((bool)InitialisedField.GetValue(null)!, Is.False,
                "Reset must flip _initialised back to false");

            // Re-scan repopulates to the same count.
            MechanicRegistry.ScanAssemblies();
            Assert.That(MechanicRegistry.All.Count, Is.EqualTo(firstCount),
                "Re-scan after reset must yield the same registry size");
        }

        [Test]
        public void Registry_ScanAssemblies_IsIdempotent()
        {
            MechanicRegistry.ScanAssemblies();
            int firstCount = MechanicRegistry.All.Count;
            // Second call with no reset must not throw on duplicates — the
            // _initialised flag short-circuits the scan.
            Assert.DoesNotThrow(() => MechanicRegistry.ScanAssemblies());
            Assert.That(MechanicRegistry.All.Count, Is.EqualTo(firstCount));
        }
    }
}
