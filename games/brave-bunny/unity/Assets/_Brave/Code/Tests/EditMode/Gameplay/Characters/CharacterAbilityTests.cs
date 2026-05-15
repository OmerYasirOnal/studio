// Wave 10 — QA: CharacterAbility EditMode tests (one per launch character).
//
// Subject under test:
//   * Brave.Gameplay.Characters.CharacterAbility (abstract base lifecycle)
//   * 8 concrete CharacterAbility subclasses
//   * Brave.Gameplay.Characters.CharacterAbilityRegistry (id → Type round-trip)
//
// Each character ability has exactly one behaviour-focused test. The registry
// round-trip is covered by a single fixture that asserts all 8 ids resolve and
// every constructed instance carries the matching AbilityId / [BraveRegister] token.
//
// Reset semantics: CharacterAbilityRegistry.ResetForTests() is internal — we use
// reflection to clear it between tests, mirroring MechanicRegistryTests.

#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Brave.Gameplay.Characters;
using Brave.Gameplay.Combat;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Characters
{
    [TestFixture]
    public class CharacterAbilityTests
    {
        // ---- expected ability ids (one per launch character) ----
        private static readonly string[] ExpectedAbilityIds =
        {
            "hop",        // BunnyAbility
            "shell",      // TortoiseAbility
            "quills",     // HedgehogAbility
            "cunning",    // FoxAbility
            "slick",      // OtterAbility
            "restore",    // PandaAbility
            "tenacity",   // BadgerAbility
            "foresight",  // OwlAbility
        };

        // ---- registry private-field reset (matches MechanicRegistryTests pattern) ----

        private static void ResetRegistry()
        {
            var typesField = typeof(CharacterAbilityRegistry).GetField(
                "_typesById", BindingFlags.NonPublic | BindingFlags.Static);
            var initField = typeof(CharacterAbilityRegistry).GetField(
                "_initialised", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(typesField, Is.Not.Null, "CharacterAbilityRegistry._typesById missing");
            Assert.That(initField, Is.Not.Null, "CharacterAbilityRegistry._initialised missing");
            if (typesField!.GetValue(null) is IDictionary dict) dict.Clear();
            initField!.SetValue(null, false);
        }

        [SetUp] public void SetUp() => ResetRegistry();
        [TearDown] public void TearDown() => ResetRegistry();

        // ---- registry contract ----

        [Test]
        public void Registry_DiscoversAllEightAbilityIds()
        {
            CharacterAbilityRegistry.ScanAssemblies();
            foreach (var id in ExpectedAbilityIds)
            {
                Assert.That(CharacterAbilityRegistry.TryResolve(id, out _), Is.True,
                    $"Expected ability id '{id}' to be registered");
            }
            Assert.That(CharacterAbilityRegistry.All.Count, Is.GreaterThanOrEqualTo(ExpectedAbilityIds.Length));
        }

        [Test]
        public void Registry_ConstructedInstance_RoundTripsAbilityId()
        {
            CharacterAbilityRegistry.ScanAssemblies();
            foreach (var id in ExpectedAbilityIds)
            {
                var instance = CharacterAbilityRegistry.Construct(id);
                Assert.That(instance, Is.InstanceOf<CharacterAbility>());
                Assert.That(instance.AbilityId, Is.EqualTo(id),
                    $"Ability constructed from id '{id}' must report matching AbilityId");
            }
        }

        [Test]
        public void Registry_UnknownId_Throws()
        {
            CharacterAbilityRegistry.ScanAssemblies();
            Assert.Throws<KeyNotFoundException>(
                () => CharacterAbilityRegistry.Resolve("nonexistent-ability"));
        }

        // ---- abstract base lifecycle ----

        [Test]
        public void Base_Lifecycle_ActivateThenDeactivate_FlipsIsActive()
        {
            var ability = new BunnyAbility();
            Assert.That(ability.IsActive, Is.False, "Fresh ability must start inactive");
            ability.OnActivate(new FakeRunContext());
            Assert.That(ability.IsActive, Is.True);
            ability.OnDeactivate();
            Assert.That(ability.IsActive, Is.False);
        }

        // ---- per-ability behavioural tests (one per character, 8 total) ----

        [Test]
        public void Bunny_Hop_GrantsTenPercentMoveSpeed()
        {
            var ability = new BunnyAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("hop"));
            Assert.That(ability.MoveSpeedMultiplierBonus, Is.EqualTo(0.10f).Within(1e-6f));
            Assert.That(ability.EffectiveMoveSpeedMultiplier, Is.EqualTo(1.10f).Within(1e-6f));
        }

        [Test]
        public void Tortoise_Shell_GrantsFiftyPercentHpAndCostsTwentyPercentSpeed()
        {
            var ability = new TortoiseAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("shell"));
            Assert.That(ability.EffectiveHpMultiplier, Is.EqualTo(1.50f).Within(1e-6f));
            Assert.That(ability.EffectiveMoveSpeedMultiplier, Is.EqualTo(0.80f).Within(1e-6f));
        }

        [Test]
        public void Hedgehog_Quills_ReflectsFivePercentOfIncomingDamage()
        {
            var ability = new HedgehogAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("quills"));
            // Reflect 5% of 100 = 5.
            Assert.That(ability.ComputeReflectedDamage(100f), Is.EqualTo(5f).Within(1e-4f));
            // Negative / zero incoming damage reflects nothing.
            Assert.That(ability.ComputeReflectedDamage(0f), Is.EqualTo(0f));
            Assert.That(ability.ComputeReflectedDamage(-10f), Is.EqualTo(0f));
        }

        [Test]
        public void Fox_Cunning_AddsOneHundredPercentCritDamage()
        {
            var ability = new FoxAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("cunning"));
            Assert.That(ability.CritDamageBonus, Is.EqualTo(1.00f).Within(1e-6f));
            // Base 1.0 + bonus 1.0 = 2.0× multiplier.
            Assert.That(ability.CritMultiplier, Is.EqualTo(2.00f).Within(1e-6f));
        }

        [Test]
        public void Otter_Slick_GrantsFifteenPercentProjectileSpeed()
        {
            var ability = new OtterAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("slick"));
            Assert.That(ability.EffectiveProjectileSpeedMultiplier, Is.EqualTo(1.15f).Within(1e-6f));
        }

        [Test]
        public void Panda_Restore_RegensOneHpPerSecondAfterThreeSecondCombatLull()
        {
            var ability = new PandaAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("restore"));
            ability.OnActivate(new FakeRunContext());

            // First 3 seconds: still in combat (timer < 3s) → no regen accumulation.
            ability.OnTick(2.0f);
            Assert.That(ability.IsOutOfCombat, Is.False);
            Assert.That(ability.ConsumeWholeRegenTicks(), Is.EqualTo(0));

            // Cross the 3s threshold; two more 1.5s ticks both regen one full hp each
            // (TimeSinceLastKill is >= 3.0 at the start of each subsequent tick).
            ability.OnTick(1.5f); // total 3.5s — IsOutOfCombat true ⇒ accumulated += 1.5
            ability.OnTick(1.5f); // total 5.0s — accumulated += 1.5 ⇒ 3.0 total
            Assert.That(ability.IsOutOfCombat, Is.True);
            Assert.That(ability.ConsumeWholeRegenTicks(), Is.EqualTo(3));

            // A kill resets the timer — regen pauses again.
            ability.NotifyKill();
            ability.OnTick(1.0f);
            Assert.That(ability.IsOutOfCombat, Is.False);
            Assert.That(ability.ConsumeWholeRegenTicks(), Is.EqualTo(0));
        }

        [Test]
        public void Badger_Tenacity_GrantsTwentyFivePercentDamageBelowThirtyPercentHp()
        {
            var ability = new BadgerAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("tenacity"));

            // Above threshold: 1.0× damage.
            Assert.That(ability.GetDamageMultiplier(currentHp: 100f, maxHp: 100f),
                Is.EqualTo(1.0f).Within(1e-6f));
            Assert.That(ability.GetDamageMultiplier(currentHp: 35f, maxHp: 100f),
                Is.EqualTo(1.0f).Within(1e-6f), "Just above 30% threshold should not trigger");

            // Below threshold: 1.25× damage.
            Assert.That(ability.GetDamageMultiplier(currentHp: 20f, maxHp: 100f),
                Is.EqualTo(1.25f).Within(1e-6f));

            // Degenerate maxHp avoids divide-by-zero.
            Assert.That(ability.GetDamageMultiplier(currentHp: 0f, maxHp: 0f),
                Is.EqualTo(1.0f).Within(1e-6f));
        }

        [Test]
        public void Owl_Foresight_GrantsOneBonusRerollPerLevelUp()
        {
            var ability = new OwlAbility();
            Assert.That(ability.AbilityId, Is.EqualTo("foresight"));
            Assert.That(ability.BonusRerollsPerLevelUp, Is.EqualTo(1));
        }

        // ---- minimal IRunContext stub for OnActivate ----

        private sealed class FakeRunContext : IRunContext
        {
            public object Services => new object();
            public Transform HeroTransform => null!;
            public float RunSeconds => 0f;
            public int PlayerLevel => 1;
        }
    }
}
