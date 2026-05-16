#nullable enable
// QA — TargetSelector EditMode tests (ADR-0019 follow-up).
//
// Subject under test: Brave.Gameplay.Combat.TargetSelector
// Specs: ADR-0019 (Wave-4 cleanup debt — ProjectileWeapon targeting dispatch),
//        ADR-0018 (XZ plane semantics — distance scoring ignores world.Y).
// What we verify:
//   * Nearest  — picks the smallest XZ-distance candidate.
//   * Furthest — picks the largest XZ-distance candidate.
//   * Random   — returns SOME candidate in the supplied list (forced index via
//                deterministic state — see helper).
//   * LowestHP — picks the candidate with the smallest EnemyHealth.Hp.
// Pattern: build a synthetic enemy list backed by Swarmer + EnemyHealth (matches
//          AutoAttackControllerTests.AcquireTarget setup so we don't diverge in
//          test-infra conventions).

using System.Collections.Generic;

using Brave.Gameplay.Combat;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;

using NUnit.Framework;

using UnityEngine;

namespace Brave.Tests.EditMode.Gameplay.Combat
{
    [TestFixture]
    public class TargetSelectorTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float ScaledHp = 10f;
        private const float ScaledContactDamage = 1f;
        private const float ScaledMoveSpeed = 1f;
        private const float TrickeryY = 50f;            // huge Y offset — must be ignored

        // Scratch state for teardown — mirrors AutoAttackControllerTests' XZ section.
        private readonly List<GameObject> _scratch = new();
        private EnemyDefinition? _def;

        [TearDown]
        public void TearDown()
        {
            for (int i = _scratch.Count - 1; i >= 0; i--)
                if (_scratch[i] != null) Object.DestroyImmediate(_scratch[i]);
            _scratch.Clear();
            EnemyRegistry.ResetAll();
            if (_def != null) { Object.DestroyImmediate(_def); _def = null; }
        }

        private EnemyBase SpawnEnemy(Vector3 worldPos, float hp = ScaledHp)
        {
            if (_def == null)
            {
                _def = ScriptableObject.CreateInstance<EnemyDefinition>();
                _def.slug = "test-enemy-tselect";
                _def.moveSpeed = ScaledMoveSpeed;
            }
            var go = new GameObject($"TS_E_{_scratch.Count}");
            go.transform.position = worldPos;
            go.AddComponent<EnemyHealth>();
            var swarmer = go.AddComponent<Swarmer>();
            swarmer.Configure(_def!, go.transform, hp, ScaledContactDamage, ScaledMoveSpeed);
            _scratch.Add(go);
            return swarmer;
        }

        // ---- Strategy: Nearest ----

        [Test]
        public void Select_Nearest_ReturnsXZNearest_IgnoringY()
        {
            // near = XZ distance 2 with a huge Y offset; far = XZ distance 6 on the X axis.
            // Pre-cleanup XY-bug would have mis-scored near via its Y component.
            var far  = SpawnEnemy(new Vector3(6f, 0f, 0f));
            var near = SpawnEnemy(new Vector3(2f, TrickeryY, 0f));

            var list = new List<EnemyBase> { far, near };
            var pick = TargetSelector.Select(Vector3.zero, list, TargetStrategy.Nearest);

            Assert.That(pick, Is.SameAs(near),
                "Nearest must pick the XZ-closest enemy regardless of Y offset (ADR-0018).");
        }

        // ---- Strategy: Furthest ----

        [Test]
        public void Select_Furthest_ReturnsXZFurthest()
        {
            var near    = SpawnEnemy(new Vector3(1f, 0f, 0f));
            var middle  = SpawnEnemy(new Vector3(3f, 0f, 0f));
            var furthest = SpawnEnemy(new Vector3(0f, 0f, 7f));   // XZ distance 7

            var list = new List<EnemyBase> { near, middle, furthest };
            var pick = TargetSelector.Select(Vector3.zero, list, TargetStrategy.Furthest);

            Assert.That(pick, Is.SameAs(furthest),
                "Furthest must pick the XZ-most-distant enemy.");
        }

        // ---- Strategy: Random ----

        [Test]
        public void Select_Random_ReturnsCandidateFromList()
        {
            // Build 3 enemies. Force the RNG state so Random.Range(0, count) is deterministic
            // for this call — we don't care WHICH it picks, only that it's one of them.
            var a = SpawnEnemy(new Vector3(1f, 0f, 0f));
            var b = SpawnEnemy(new Vector3(3f, 0f, 0f));
            var c = SpawnEnemy(new Vector3(5f, 0f, 0f));
            var list = new List<EnemyBase> { a, b, c };

            // Seed RNG for repeatability. The exact pick is an implementation detail of
            // UnityEngine.Random; the contract is "uniform pick from in-range list".
            Random.State prev = Random.state;
            try
            {
                Random.InitState(12345);
                var pick = TargetSelector.Select(Vector3.zero, list, TargetStrategy.Random);
                Assert.That(pick, Is.EqualTo(a).Or.EqualTo(b).Or.EqualTo(c),
                    "Random must return one of the supplied in-range candidates.");
            }
            finally { Random.state = prev; }
        }

        // ---- Strategy: LowestHP ----

        [Test]
        public void Select_LowestHP_ReturnsCandidateWithMinHp()
        {
            // Three enemies with distinct HP. Position is irrelevant for LowestHP.
            var a = SpawnEnemy(new Vector3(1f, 0f, 0f), hp: 100f);
            var b = SpawnEnemy(new Vector3(2f, 0f, 0f), hp: 5f);    // <-- the winner
            var c = SpawnEnemy(new Vector3(3f, 0f, 0f), hp: 50f);

            var list = new List<EnemyBase> { a, b, c };
            var pick = TargetSelector.Select(Vector3.zero, list, TargetStrategy.LowestHP);

            Assert.That(pick, Is.SameAs(b),
                "LowestHP must pick the alive enemy with the smallest EnemyHealth.Hp.");
        }
    }
}
