// QA — AchievementService EditMode tests (Wave 10).
// Subject under test: Brave.Systems.Achievements.AchievementService against
// InMemoryFileSystem-backed SaveService + a synthetic AchievementCatalogConfig
// containing all 20 launch kinds. Verifies:
//   (a) 20 distinct achievements are instantiated from the catalog;
//   (b) gameplay-event progress (Slayer = kill enemies, Bossbane = boss kills,
//       Evolutionist = first evolution, Survivor = wave count from run end);
//   (c) threshold crossing fires AchievementUnlockedChannel exactly once;
//   (d) TryClaim grants reward + marks claimed + is idempotent;
//   (e) save round-trip preserves progress + claim flag.

#nullable enable

using System.Collections.Generic;
using Brave.Gameplay.Events;
using Brave.Gameplay.Run;
using Brave.Systems.Achievements;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Achievements
{
    [TestFixture]
    public class AchievementServiceTests
    {
        private const string RootDir = "/virt/brave-ach-w10";

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;
        private AchievementCatalogConfig _catalog = null!;
        private AchievementUnlockedChannel _channel = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load();
            _save.Data.Player.Id = "test-player";
            _catalog = MakeCatalog();
            _channel = ScriptableObject.CreateInstance<AchievementUnlockedChannel>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_catalog != null) ScriptableObject.DestroyImmediate(_catalog);
            if (_channel != null) ScriptableObject.DestroyImmediate(_channel);
        }

        // ---- Catalog factory: every kind, small thresholds so tests cross quickly. ----

        private static AchievementCatalogConfig MakeCatalog()
        {
            var cfg = ScriptableObject.CreateInstance<AchievementCatalogConfig>();
            cfg.entries = new List<AchievementDef>
            {
                D(AchievementKind.FirstBossKill,   "first-boss-kill",  1,  CurrencyType.Stars, 5),
                D(AchievementKind.Slayer,          "slayer",           5,  CurrencyType.Stars, 10),
                D(AchievementKind.Survivor,        "survivor",         10, CurrencyType.Stars, 15),
                D(AchievementKind.Untouchable,     "untouchable",      1,  CurrencyType.Stars, 20),
                D(AchievementKind.Evolutionist,    "evolutionist",     1,  CurrencyType.Stars, 10),
                D(AchievementKind.Completionist,   "completionist",    7,  CurrencyType.Stars, 10),
                D(AchievementKind.StreakMaster,    "streak-master",    20, CurrencyType.Carrots, 500),
                D(AchievementKind.CritLord,        "crit-lord",        50, CurrencyType.Carrots, 500),
                D(AchievementKind.TreasureHunter,  "treasure-hunter",  500, CurrencyType.Carrots, 1000),
                D(AchievementKind.StarCollector,   "star-collector",   25, CurrencyType.Stars, 10),
                D(AchievementKind.Variety,         "variety",          6,  CurrencyType.Stars, 8),
                D(AchievementKind.IronPlayer,      "iron-player",      120, CurrencyType.Stars, 15),
                D(AchievementKind.Marathon,        "marathon",         60, CurrencyType.Stars, 10),
                D(AchievementKind.SpeedRun,        "speed-run",        5,  CurrencyType.Stars, 15, secondary: 30),
                D(AchievementKind.PremiumBuyer,    "premium-buyer",    1,  CurrencyType.Stars, 5),
                D(AchievementKind.Generous,        "generous",         100, CurrencyType.Carrots, 300),
                D(AchievementKind.Loyal,           "loyal",            7,  CurrencyType.Stars, 10),
                D(AchievementKind.QuestMaster,     "quest-master",     10, CurrencyType.Stars, 12),
                D(AchievementKind.WorldTour,       "world-tour",       3,  CurrencyType.Stars, 25),
                D(AchievementKind.Bossbane,        "bossbane",         3,  CurrencyType.Stars, 20),
            };
            return cfg;
        }

        private static AchievementDef D(AchievementKind kind, string id, int required, CurrencyType cur, int amt, int secondary = 0)
            => new() { id = id, kind = kind, requiredCount = required, rewardCurrency = cur, rewardAmount = amt, secondaryThreshold = secondary };

        // ---- core assertions ----

        [Test]
        public void Hydrate_Produces20DistinctAchievements()
        {
            var svc = new AchievementService(_save, _catalog);
            Assert.That(svc.All, Has.Count.EqualTo(20));

            var seen = new HashSet<string>();
            foreach (var a in svc.All)
            {
                Assert.That(seen.Add(a.Id), Is.True, $"Duplicate id {a.Id} in catalog.");
            }
            // Every kind is represented exactly once.
            var kinds = new HashSet<AchievementKind>();
            foreach (var a in svc.All)
            {
                kinds.Add(a.Def.kind);
            }
            Assert.That(kinds, Has.Count.EqualTo(20));
        }

        [Test]
        public void EnemyKilled_IncrementsSlayer_UpToThreshold()
        {
            var svc = new AchievementService(_save, _catalog);
            var slayer = svc.Get("slayer")!;
            var evt = new EnemyKilledEvent(0, Vector3.zero, false, 0f);
            for (var i = 0; i < slayer.RequiredCount; i++) svc.OnEnemyKilled(in evt);

            Assert.That(slayer.CurrentCount, Is.EqualTo(slayer.RequiredCount));
            Assert.That(slayer.Unlocked, Is.True);
        }

        [Test]
        public void BossDefeated_TriggersFirstBossKill_AndIncrementsBossbane()
        {
            var svc = new AchievementService(_save, _catalog);
            var first = svc.Get("first-boss-kill")!;
            var bossbane = svc.Get("bossbane")!;

            var evt = new BossDefeatedEvent("old-boar-king", 0, 100f, Vector3.zero);
            svc.OnBossDefeated(in evt);

            Assert.That(first.Unlocked, Is.True, "FirstBossKill must trigger on a single boss kill.");
            Assert.That(bossbane.CurrentCount, Is.EqualTo(1), "Bossbane must increment on each boss kill.");
            Assert.That(bossbane.Unlocked, Is.False);
        }

        [Test]
        public void WeaponEvolved_TriggersEvolutionist()
        {
            var svc = new AchievementService(_save, _catalog);
            var evo = svc.Get("evolutionist")!;
            var evt = new WeaponEvolvedEvent("carrot-boomerang", "harvest-cyclone", "magnet-charm", true, 60f);
            svc.OnWeaponEvolved(in evt);
            Assert.That(evo.Unlocked, Is.True);
        }

        [Test]
        public void RunEnded_SetsSurvivorToHighestWaveReached()
        {
            var svc = new AchievementService(_save, _catalog);
            var survivor = svc.Get("survivor")!;
            var report = new RunEndReport { wavesCleared = 6, runDurationSeconds = 200f, outcome = RunOutcome.Lose };
            svc.OnRunEnded(new RunEndedEvent(report));
            Assert.That(survivor.CurrentCount, Is.EqualTo(6));

            // Lower wave count must NOT regress.
            var report2 = new RunEndReport { wavesCleared = 4, runDurationSeconds = 100f, outcome = RunOutcome.Lose };
            svc.OnRunEnded(new RunEndedEvent(report2));
            Assert.That(survivor.CurrentCount, Is.EqualTo(6));

            // Threshold cross.
            var report3 = new RunEndReport { wavesCleared = 20, runDurationSeconds = 600f, outcome = RunOutcome.Win };
            svc.OnRunEnded(new RunEndedEvent(report3));
            Assert.That(survivor.Unlocked, Is.True);
        }

        [Test]
        public void UnlockedChannel_FiresOnceWhenThresholdCrossed()
        {
            var svc = new AchievementService(_save, _catalog, unlockedChannel: _channel);
            var fired = new List<string>();
            _channel.Subscribe(e => fired.Add(e.achievementId));

            var evt = new EnemyKilledEvent(0, Vector3.zero, false, 0f);
            var slayer = svc.Get("slayer")!;
            for (var i = 0; i < slayer.RequiredCount; i++) svc.OnEnemyKilled(in evt);

            // Single threshold-cross — single channel raise.
            Assert.That(fired, Has.Count.EqualTo(1));
            Assert.That(fired[0], Is.EqualTo("slayer"));

            // Further kills must not re-fire.
            for (var i = 0; i < 5; i++) svc.OnEnemyKilled(in evt);
            Assert.That(fired, Has.Count.EqualTo(1));
        }

        [Test]
        public void TryClaim_GrantsRewardAndMarksClaimed_AndIsIdempotent()
        {
            var svc = new AchievementService(_save, _catalog);
            var evt = new EnemyKilledEvent(0, Vector3.zero, false, 0f);
            var slayer = svc.Get("slayer")!;
            for (var i = 0; i < slayer.RequiredCount; i++) svc.OnEnemyKilled(in evt);

            var starsBefore = _save.Data.Currencies.Stars;
            var granted = svc.TryClaim("slayer");
            Assert.That(granted.amount, Is.EqualTo(slayer.Def.rewardAmount));
            Assert.That(granted.currency, Is.EqualTo(slayer.Def.rewardCurrency));
            Assert.That(_save.Data.Currencies.Stars, Is.EqualTo(starsBefore + granted.amount));
            Assert.That(slayer.Claimed, Is.True);

            // Idempotent — second claim grants nothing.
            var second = svc.TryClaim("slayer");
            Assert.That(second.amount, Is.EqualTo(0));
            Assert.That(_save.Data.Currencies.Stars, Is.EqualTo(starsBefore + granted.amount));
        }

        [Test]
        public void TryClaim_BeforeUnlock_Fails()
        {
            var svc = new AchievementService(_save, _catalog);
            var result = svc.TryClaim("slayer");
            Assert.That(result.amount, Is.EqualTo(0));
            Assert.That(svc.IsClaimed("slayer"), Is.False);
        }

        [Test]
        public void Persistence_RoundTrip_RestoresProgressAndClaim()
        {
            var svc1 = new AchievementService(_save, _catalog);
            var evt = new EnemyKilledEvent(0, Vector3.zero, false, 0f);
            var slayer = svc1.Get("slayer")!;
            for (var i = 0; i < slayer.RequiredCount; i++) svc1.OnEnemyKilled(in evt);
            svc1.TryClaim("slayer"); // forces _save.Save()

            // Force fresh SaveService against the same in-memory disk.
            var save2 = new SaveService(RootDir, _fs);
            save2.Load();
            Assert.That(save2.Data.Achievements.ContainsKey("slayer"), Is.True);
            Assert.That(save2.Data.Achievements["slayer"].Claimed, Is.True);
            Assert.That(save2.Data.Achievements["slayer"].Progress,
                Is.EqualTo(slayer.RequiredCount));

            // Fresh service must hydrate the existing state.
            var svc2 = new AchievementService(save2, _catalog);
            var hydrated = svc2.Get("slayer")!;
            Assert.That(hydrated.Unlocked, Is.True);
            Assert.That(hydrated.Claimed, Is.True);
            Assert.That(hydrated.CurrentCount, Is.EqualTo(slayer.RequiredCount));
        }

        [Test]
        public void AddProgress_ManualIncrement_TracksAchievement()
        {
            var svc = new AchievementService(_save, _catalog);
            // Variety has no auto-event channel — manual ticks.
            for (var i = 0; i < 6; i++) svc.AddProgress("variety", 1);
            Assert.That(svc.IsUnlocked("variety"), Is.True);
        }

        [Test]
        public void Untouchable_TriggersOnZeroDamageWin()
        {
            var svc = new AchievementService(_save, _catalog);
            // No damage notified — winning run should unlock.
            var report = new RunEndReport { wavesCleared = 5, runDurationSeconds = 200f, outcome = RunOutcome.Win };
            svc.OnRunEnded(new RunEndedEvent(report));
            Assert.That(svc.IsUnlocked("untouchable"), Is.True);
        }

        [Test]
        public void Untouchable_DoesNotTrigger_WhenDamageTaken()
        {
            var svc = new AchievementService(_save, _catalog);
            svc.NotifyDamageTaken(5);
            var report = new RunEndReport { wavesCleared = 5, runDurationSeconds = 200f, outcome = RunOutcome.Win };
            svc.OnRunEnded(new RunEndedEvent(report));
            Assert.That(svc.IsUnlocked("untouchable"), Is.False);
        }

        [Test]
        public void Marathon_TriggersAtLongRun()
        {
            var svc = new AchievementService(_save, _catalog);
            // Marathon threshold = 60s here. Run 90s → unlock.
            var report = new RunEndReport { runDurationSeconds = 90f, outcome = RunOutcome.Lose, wavesCleared = 3 };
            svc.OnRunEnded(new RunEndedEvent(report));
            Assert.That(svc.IsUnlocked("marathon"), Is.True);
        }

        [Test]
        public void SpeedRun_NeedsWaveTargetUnderTimeCap()
        {
            var svc = new AchievementService(_save, _catalog);
            // SpeedRun: 5 waves in <= 30s (secondaryThreshold)
            var miss = new RunEndReport { wavesCleared = 5, runDurationSeconds = 40f, outcome = RunOutcome.Win };
            svc.OnRunEnded(new RunEndedEvent(miss));
            Assert.That(svc.IsUnlocked("speed-run"), Is.False);

            var hit = new RunEndReport { wavesCleared = 6, runDurationSeconds = 25f, outcome = RunOutcome.Win };
            svc.OnRunEnded(new RunEndedEvent(hit));
            Assert.That(svc.IsUnlocked("speed-run"), Is.True);
        }
    }
}
