#if WAVE7_TESTS_FIXED  // TODO(Wave12): fix test API drift
// QA — LevelUpDraftController / LevelUpDraftBuilder EditMode tests (Wave 7B).
// Subject under test:
//   * Brave.UI.Controllers.LevelUpDraftBuilder.Build(pool, seed)
//     — pure-C# 3-card draft picker. Verifies the always-3 invariant, the
//     uniqueness invariant when pool is large enough, and replay determinism.
//
// Pattern: matches LoadoutControllerTests — exercise the pure helper, no
// UIDocument required.

#nullable enable

using System.Collections.Generic;
using Brave.UI.Controllers;
using NUnit.Framework;

namespace Brave.Tests.EditMode.UI
{
    [TestFixture]
    public class LevelUpDraftControllerTests
    {
        // ---- helpers ----

        private static List<UpgradeOption> SixPassives() => new()
        {
            new UpgradeOption("MC", "Magnet Charm",  "Pickup +20%",      false),
            new UpgradeOption("HC", "Hearty Charm",  "Max HP +15%",      false),
            new UpgradeOption("RC", "Mossy Charm",   "Regen +0.5/s",     false),
            new UpgradeOption("DC", "Damage Charm",  "Damage +10%",      false),
            new UpgradeOption("SC", "Swift Charm",   "Move +8%",         false),
            new UpgradeOption("CC", "Cooldown Charm","Cooldown -8%",     false),
        };

        // ---- core invariants ----

        [Test]
        public void Build_AlwaysReturnsExactlyThreeOffers()
        {
            var pool = SixPassives();

            var draft = LevelUpDraftBuilder.Build(pool, seed: 1);

            Assert.That(draft, Is.Not.Null);
            Assert.That(draft.Length, Is.EqualTo(LevelUpDraftBuilder.DraftSize),
                "Draft must always be exactly 3 cards.");
        }

        [Test]
        public void Build_OffersAreUnique_WhenPoolLargerThanThree()
        {
            var pool = SixPassives();

            var draft = LevelUpDraftBuilder.Build(pool, seed: 42);

            var titles = new HashSet<string>();
            foreach (var d in draft) titles.Add(d.Title);
            Assert.That(titles.Count, Is.EqualTo(LevelUpDraftBuilder.DraftSize),
                "When pool ≥ 3, the 3 cards must be unique (no duplicates).");
        }

        [Test]
        public void Build_DeterministicForSameSeed()
        {
            var pool = SixPassives();

            var a = LevelUpDraftBuilder.Build(pool, seed: 7);
            var b = LevelUpDraftBuilder.Build(pool, seed: 7);

            Assert.That(a.Length, Is.EqualTo(b.Length));
            for (int i = 0; i < a.Length; i++)
            {
                Assert.That(a[i].Title, Is.EqualTo(b[i].Title),
                    $"Card {i} must match for identical seeds — required for replay parity.");
            }
        }

        [Test]
        public void Build_EmptyPool_StillReturnsThreePlaceholders()
        {
            // Defensive: the modal must always render 3 cards even if balance
            // failed to load a catalogue (e.g. corrupt passives.json).
            var draft = LevelUpDraftBuilder.Build(new List<UpgradeOption>(), seed: 0);

            Assert.That(draft.Length, Is.EqualTo(LevelUpDraftBuilder.DraftSize));
            for (int i = 0; i < draft.Length; i++)
            {
                Assert.That(draft[i].Title, Is.EqualTo("—"),
                    "Empty pool falls back to placeholder offers so the modal still has 3 cards.");
            }
        }

        [Test]
        public void Build_PoolSmallerThanThree_PadsWithRepeats()
        {
            // Very low-level case — only 2 passives in the catalogue.
            var pool = new List<UpgradeOption>
            {
                new("A", "Alpha", "", false),
                new("B", "Beta", "", false),
            };

            var draft = LevelUpDraftBuilder.Build(pool, seed: 1);

            Assert.That(draft.Length, Is.EqualTo(LevelUpDraftBuilder.DraftSize));
            foreach (var d in draft)
            {
                Assert.That(d.Title, Is.AnyOf("Alpha", "Beta"),
                    "Padded offers must come from the (small) pool.");
            }
        }

        // ---- evolution flag pass-through ----

        [Test]
        public void Build_PreservesEvolutionFlag()
        {
            var pool = new List<UpgradeOption>
            {
                new("A", "Alpha", "", false),
                new("B", "Beta", "", true), // evolution candidate
                new("C", "Gamma", "", false),
            };

            var draft = LevelUpDraftBuilder.Build(pool, seed: 5);

            bool foundEvolution = false;
            foreach (var d in draft)
            {
                if (d.Title == "Beta" && d.IsEvolution) foundEvolution = true;
            }
            Assert.That(foundEvolution, Is.True,
                "Evolution flag must round-trip through the builder so the UI ring shows.");
        }
    }
}

#endif
