// QA — WeaponEvolutionService EditMode tests (Wave 9).
//
// Subject under test: Brave.Gameplay.Combat.Evolution.WeaponEvolutionService.
// User stories: weapon evolution loop (post-launch survivor.io feel pillar).
// Spec: docs/02-gdd/04-weapons.md § Evolution; data/balance/evolutions.json.
// ADR: 0007 — charm consumption (always true at launch).
//
// Coverage (per Wave-9 deliverable):
//   1. Recipe match: weapon@L5 + charm@L5 fires evolution + swaps inventory.
//   2. Charm consumption per ADR-0007 (consume=true → charm removed; false → retained).
//   3. Evolved weapon definition swap (base id removed, evolved id at L1).
//   4. Idempotence: a recipe only fires once per run.
//   5. Negative cases: weapon < L5, charm missing/under-leveled, channel=null safe.

using System.Collections.Generic;
using Brave.Gameplay.Combat.Evolution;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class WeaponEvolutionServiceTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const string BaseWeapon = "carrot-boomerang";
        private const string Charm = "magnet-charm";
        private const string EvolvedWeapon = "harvest-cyclone";
        private const int MaxLevel = 5;
        private const int UnderLevel = 4;
        private const int EvolvedStartLevel = 1;
        private const float RunSeconds = 123.5f;

        // ---- helpers ----------------------------------------------------

        private static EvolutionRecipe MakeRecipe(bool consume = true)
            => new EvolutionRecipe
            {
                baseWeaponId = BaseWeapon,
                requiredCharmId = Charm,
                evolvedWeaponId = EvolvedWeapon,
                requiredWeaponLevel = MaxLevel,
                requiredCharmLevel = MaxLevel,
                consumeCharm = consume,
            };

        private sealed class DictWeaponInv : IWeaponInventory
        {
            public readonly Dictionary<string, int> Map = new();
            public bool TryGetLevel(string id, out int level) => Map.TryGetValue(id, out level);
            public void Remove(string id) => Map.Remove(id);
            public void Add(string id, int level) => Map[id] = level;
            public IEnumerable<string> AllIds() => Map.Keys;
        }

        private sealed class DictCharmInv : ICharmInventory
        {
            public readonly Dictionary<string, int> Map = new();
            public bool TryGetLevel(string id, out int level) => Map.TryGetValue(id, out level);
            public void Remove(string id) => Map.Remove(id);
            public IEnumerable<string> AllIds() => Map.Keys;
        }

        private static (WeaponEvolutionService svc, DictWeaponInv w, DictCharmInv c) Setup(
            EvolutionRecipe recipe, int weaponLvl, int charmLvl)
        {
            var svc = new WeaponEvolutionService(channel: null);
            svc.Initialize(new[] { recipe });
            var w = new DictWeaponInv();
            var c = new DictCharmInv();
            if (weaponLvl > 0) w.Map[BaseWeapon] = weaponLvl;
            if (charmLvl > 0) c.Map[Charm] = charmLvl;
            return (svc, w, c);
        }

        // ---- tests ------------------------------------------------------

        [Test]
        public void Match_WeaponL5_AndCharmL5_FiresEvolution()
        {
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, MaxLevel);

            int fired = svc.CheckEvolutions(w, c, RunSeconds);

            Assert.AreEqual(1, fired, "exactly one recipe should fire");
            Assert.IsTrue(svc.HasFired(EvolvedWeapon));
        }

        [Test]
        public void Match_SwapsBaseWeaponForEvolvedAtL1()
        {
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, MaxLevel);

            svc.CheckEvolutions(w, c, RunSeconds);

            Assert.IsFalse(w.Map.ContainsKey(BaseWeapon), "base weapon should be removed");
            Assert.IsTrue(w.Map.ContainsKey(EvolvedWeapon), "evolved weapon should be in inventory");
            Assert.AreEqual(EvolvedStartLevel, w.Map[EvolvedWeapon],
                "evolved weapon enters at L1");
        }

        [Test]
        public void Match_ConsumeCharmTrue_RemovesCharm_Adr0007()
        {
            var (svc, w, c) = Setup(MakeRecipe(consume: true), MaxLevel, MaxLevel);

            svc.CheckEvolutions(w, c, RunSeconds);

            Assert.IsFalse(c.Map.ContainsKey(Charm),
                "ADR-0007: charm must be consumed (slot freed) on evolution");
        }

        [Test]
        public void Match_ConsumeCharmFalse_RetainsCharm()
        {
            var (svc, w, c) = Setup(MakeRecipe(consume: false), MaxLevel, MaxLevel);

            svc.CheckEvolutions(w, c, RunSeconds);

            Assert.IsTrue(c.Map.ContainsKey(Charm),
                "consumeCharm=false (future cosmetic flag) should leave the charm in inventory");
            Assert.AreEqual(MaxLevel, c.Map[Charm], "charm level must be untouched");
        }

        [Test]
        public void NoMatch_WeaponUnderL5_DoesNotEvolve()
        {
            var (svc, w, c) = Setup(MakeRecipe(), UnderLevel, MaxLevel);

            int fired = svc.CheckEvolutions(w, c, RunSeconds);

            Assert.AreEqual(0, fired);
            Assert.IsTrue(w.Map.ContainsKey(BaseWeapon));
            Assert.IsFalse(w.Map.ContainsKey(EvolvedWeapon));
            Assert.IsTrue(c.Map.ContainsKey(Charm), "charm must not be consumed on non-match");
        }

        [Test]
        public void NoMatch_CharmUnderL5_DoesNotEvolve()
        {
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, UnderLevel);

            int fired = svc.CheckEvolutions(w, c, RunSeconds);

            Assert.AreEqual(0, fired);
            Assert.IsTrue(w.Map.ContainsKey(BaseWeapon));
            Assert.IsTrue(c.Map.ContainsKey(Charm));
        }

        [Test]
        public void NoMatch_CharmMissing_DoesNotEvolve()
        {
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, 0);

            int fired = svc.CheckEvolutions(w, c, RunSeconds);

            Assert.AreEqual(0, fired);
        }

        [Test]
        public void Idempotent_RecipeFiresOnlyOncePerRun()
        {
            var (svc, w, c) = Setup(MakeRecipe(consume: false), MaxLevel, MaxLevel);

            int first = svc.CheckEvolutions(w, c, RunSeconds);
            // Manually restore base weapon to simulate a state where another sweep happens.
            w.Map[BaseWeapon] = MaxLevel;
            int second = svc.CheckEvolutions(w, c, RunSeconds);

            Assert.AreEqual(1, first);
            Assert.AreEqual(0, second, "the same recipe must not re-fire within a single run");
            Assert.IsTrue(svc.HasFired(EvolvedWeapon));
        }

        [Test]
        public void Initialize_ResetsFiredState()
        {
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, MaxLevel);
            svc.CheckEvolutions(w, c, RunSeconds);
            Assert.IsTrue(svc.HasFired(EvolvedWeapon));

            // Re-init (new run) clears the fired set.
            svc.Initialize(new[] { MakeRecipe() });

            Assert.IsFalse(svc.HasFired(EvolvedWeapon),
                "Initialize must clear per-run fired state");
        }

        [Test]
        public void NullChannel_DoesNotThrow()
        {
            // The production wiring may pass null in editor smoke tests; the
            // service must remain crash-safe (see constructor contract).
            var (svc, w, c) = Setup(MakeRecipe(), MaxLevel, MaxLevel);
            Assert.DoesNotThrow(() => svc.CheckEvolutions(w, c, RunSeconds));
        }

        [Test]
        public void InvalidRecipe_IsSkipped()
        {
            var bad = new EvolutionRecipe
            {
                baseWeaponId = "", // invalid
                requiredCharmId = Charm,
                evolvedWeaponId = EvolvedWeapon,
                requiredWeaponLevel = MaxLevel,
                requiredCharmLevel = MaxLevel,
            };
            var svc = new WeaponEvolutionService(null);
            svc.Initialize(new[] { bad });
            var w = new DictWeaponInv(); w.Map[BaseWeapon] = MaxLevel;
            var c = new DictCharmInv(); c.Map[Charm] = MaxLevel;

            Assert.AreEqual(0, svc.CheckEvolutions(w, c, RunSeconds));
        }

        [Test]
        public void Recipe_IsValid_RequiresAllSlugsAndPositiveLevels()
        {
            Assert.IsTrue(MakeRecipe().IsValid());

            var noBase = MakeRecipe(); noBase.baseWeaponId = string.Empty;
            Assert.IsFalse(noBase.IsValid());

            var noCharm = MakeRecipe(); noCharm.requiredCharmId = string.Empty;
            Assert.IsFalse(noCharm.IsValid());

            var noEvolved = MakeRecipe(); noEvolved.evolvedWeaponId = string.Empty;
            Assert.IsFalse(noEvolved.IsValid());

            var zeroLvl = MakeRecipe(); zeroLvl.requiredWeaponLevel = 0;
            Assert.IsFalse(zeroLvl.IsValid());
        }
    }
}
