// QA — CharacterUnlockService EditMode tests
// Subject: Brave.Systems.Progression.CharacterUnlockService against
// InMemoryFileSystem-backed SaveService. Verifies:
//   (a) all non-starter characters are locked by default;
//   (b) starter is unlocked from the first call (no save needed);
//   (c) reach_wave condition fires when stat threshold met;
//   (d) defeat_boss condition fires on RecordBossDefeated;
//   (e) save round-trip preserves unlocks via Persisted CharacterProfile.Unlocked;
//   (f) IsUnlocked(slug) returns true once unlocked;
//   (g) GetUnlockedCharacterIds enumerates only unlocked slugs;
//   (h) TryPurchase deducts Stars and unlocks.

#nullable enable

using System.Collections.Generic;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.Progression
{
    [TestFixture]
    public class CharacterUnlockTests
    {
        private const string RootDir = "/virt/brave-unlock";

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load();
        }

        private static IReadOnlyDictionary<string, UnlockCondition?> StandardRoster() =>
            new Dictionary<string, UnlockCondition?>
            {
                ["bunny"] = null, // starter
                ["tortoise"] = new UnlockCondition { Type = UnlockConditionType.PayStars, Stars = 200 },
                ["fox"] = new UnlockCondition
                {
                    Type = UnlockConditionType.ReachWave,
                    Wave = 30,
                    WithCharacter = "bunny",
                },
                ["badger"] = new UnlockCondition
                {
                    Type = UnlockConditionType.DefeatBoss,
                    Boss = "old-boar-king",
                },
                ["owl"] = new UnlockCondition
                {
                    Type = UnlockConditionType.ReachWave,
                    Wave = 50,
                    WithCharacter = "bunny",
                },
            };

        // (a) + (b) — default-locked except starter
        [Test]
        public void Defaults_OnlyStarterIsUnlocked()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());

            Assert.That(svc.IsUnlocked("bunny"), Is.True, "starter (no condition) must be unlocked");
            Assert.That(svc.IsUnlocked("tortoise"), Is.False);
            Assert.That(svc.IsUnlocked("fox"), Is.False);
            Assert.That(svc.IsUnlocked("badger"), Is.False);
            Assert.That(svc.IsUnlocked("owl"), Is.False);

            var unlocked = svc.GetUnlockedCharacterIds();
            Assert.That(unlocked, Has.Count.EqualTo(1));
            Assert.That(unlocked[0], Is.EqualTo("bunny"));
        }

        // (c) — reach_wave condition fires
        [Test]
        public void RecordRunCompletion_TriggersReachWaveUnlock()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());

            // 1 wave shy of fox's threshold → no unlock yet
            svc.RecordRunCompletion("bunny", waveReached: 29, bossesDefeatedThisRun: 0);
            Assert.That(svc.IsUnlocked("fox"), Is.False);

            string? observed = null;
            svc.CharacterUnlocked += slug => observed = slug;

            svc.RecordRunCompletion("bunny", waveReached: 30, bossesDefeatedThisRun: 0);
            Assert.That(svc.IsUnlocked("fox"), Is.True);
            Assert.That(observed, Is.EqualTo("fox"), "CharacterUnlocked event must fire");
            // Owl needs wave 50 — still locked
            Assert.That(svc.IsUnlocked("owl"), Is.False);
        }

        // (c-bis) — withCharacter gating: a run on a different character must NOT count
        [Test]
        public void RecordRunCompletion_OnWrongCharacter_DoesNotSatisfyWaveCondition()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());

            // Reaching wave 30 with tortoise must not unlock fox (which requires bunny runs).
            svc.RecordRunCompletion("tortoise", waveReached: 30, bossesDefeatedThisRun: 0);

            Assert.That(svc.IsUnlocked("fox"), Is.False);
        }

        // (d) — defeat_boss condition fires
        [Test]
        public void RecordBossDefeated_TriggersDefeatBossUnlock()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());

            Assert.That(svc.IsUnlocked("badger"), Is.False);
            svc.RecordBossDefeated("old-boar-king", "bunny");
            Assert.That(svc.IsUnlocked("badger"), Is.True);
        }

        // (e) — save round-trip preserves unlocks
        [Test]
        public void SaveRoundTrip_PreservesUnlocks()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());
            svc.RecordBossDefeated("old-boar-king", "bunny"); // unlocks badger + Save()
            Assert.That(svc.IsUnlocked("badger"), Is.True);

            // Fresh service over the same filesystem fully re-exercises the load path.
            var reloadedSave = new SaveService(RootDir, _fs);
            reloadedSave.Load();
            var reloadedSvc = new CharacterUnlockService(reloadedSave, StandardRoster());

            Assert.That(reloadedSvc.IsUnlocked("badger"), Is.True,
                "Unlocked flag must survive save → load cycle");
            Assert.That(reloadedSvc.IsUnlocked("fox"), Is.False, "unrelated slugs stay locked");
        }

        // (f) + (g) — IsUnlocked + GetUnlockedCharacterIds enumerate correctly
        [Test]
        public void GetUnlockedCharacterIds_ReflectsLiveUnlocks()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());
            svc.RecordRunCompletion("bunny", waveReached: 30, bossesDefeatedThisRun: 0); // fox
            svc.RecordBossDefeated("old-boar-king", "bunny"); // badger

            var ids = svc.GetUnlockedCharacterIds();
            Assert.That(ids, Does.Contain("bunny"));
            Assert.That(ids, Does.Contain("fox"));
            Assert.That(ids, Does.Contain("badger"));
            Assert.That(ids, Does.Not.Contain("tortoise"));
            Assert.That(ids, Does.Not.Contain("owl"));
        }

        // (h) — TryPurchase Star branch
        [Test]
        public void TryPurchase_DeductsStars_AndUnlocks()
        {
            _save.Data.Currencies.Stars = 500;
            var wallet = new CurrencyWallet(_save.Data.Currencies);
            var svc = new CharacterUnlockService(_save, StandardRoster());

            var ok = svc.TryPurchase("tortoise", wallet);

            Assert.That(ok, Is.True);
            Assert.That(svc.IsUnlocked("tortoise"), Is.True);
            Assert.That(wallet.Get(CurrencyType.Stars), Is.EqualTo(300L),
                "200 Stars must be deducted on purchase");
        }

        [Test]
        public void TryPurchase_InsufficientStars_DoesNotUnlock()
        {
            _save.Data.Currencies.Stars = 50;
            var wallet = new CurrencyWallet(_save.Data.Currencies);
            var svc = new CharacterUnlockService(_save, StandardRoster());

            var ok = svc.TryPurchase("tortoise", wallet);

            Assert.That(ok, Is.False);
            Assert.That(svc.IsUnlocked("tortoise"), Is.False);
            Assert.That(wallet.Get(CurrencyType.Stars), Is.EqualTo(50L), "balance unchanged on failed purchase");
        }

        [Test]
        public void TryPurchase_NonStarsCondition_Rejected()
        {
            // fox has a reach_wave condition — TryPurchase should refuse.
            _save.Data.Currencies.Stars = 10_000;
            var wallet = new CurrencyWallet(_save.Data.Currencies);
            var svc = new CharacterUnlockService(_save, StandardRoster());

            var ok = svc.TryPurchase("fox", wallet);

            Assert.That(ok, Is.False);
            Assert.That(svc.IsUnlocked("fox"), Is.False);
            Assert.That(wallet.Get(CurrencyType.Stars), Is.EqualTo(10_000L));
        }

        [Test]
        public void EvaluateAll_IsIdempotent()
        {
            var svc = new CharacterUnlockService(_save, StandardRoster());
            int fireCount = 0;
            svc.CharacterUnlocked += _ => fireCount++;

            svc.RecordBossDefeated("old-boar-king", "bunny"); // 1 unlock
            Assert.That(fireCount, Is.EqualTo(1));

            // Re-evaluating with no new state changes must not re-fire.
            var newly = svc.EvaluateAll();
            Assert.That(newly, Is.Empty);
            Assert.That(fireCount, Is.EqualTo(1));
        }
    }
}
