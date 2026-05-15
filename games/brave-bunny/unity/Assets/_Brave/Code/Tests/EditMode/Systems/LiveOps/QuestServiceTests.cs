// QA — QuestService EditMode tests (Wave 9 LiveOps).
// Subject under test: Brave.Systems.LiveOps.QuestService against
// InMemoryFileSystem-backed SaveService. Verifies:
//   (a) deterministic 3-quest roll for a given (playerId, date) seed;
//   (b) OnEvent fans out to active quests (kills, level, gold, boss, duration, waves);
//   (c) Claim grants currency + marks Claimed, idempotent on second call;
//   (d) Save round-trip preserves progress across reload;
//   (e) UTC midnight rollover replaces the day's quest set.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.LiveOps;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.LiveOps
{
    [TestFixture]
    public class QuestServiceTests
    {
        private const string RootDir = "/virt/brave-quest";

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;
        private QuestPoolConfig _config = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load();
            _save.Data.Player.Id = "test-player";
            _config = MakeTestConfig();
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null) ScriptableObject.DestroyImmediate(_config);
        }

        private static QuestPoolConfig MakeTestConfig()
        {
            var cfg = ScriptableObject.CreateInstance<QuestPoolConfig>();
            cfg.templates = new List<QuestTemplate>
            {
                new()
                {
                    id = "kill_easy",
                    type = QuestType.KillEnemies,
                    difficulty = QuestDifficulty.Easy,
                    requiredCount = 5,
                    rewardCurrency = CurrencyType.Carrots,
                    rewardAmount = 100,
                },
                new()
                {
                    id = "level_med",
                    type = QuestType.ReachLevel,
                    difficulty = QuestDifficulty.Medium,
                    requiredCount = 10,
                    rewardCurrency = CurrencyType.Carrots,
                    rewardAmount = 250,
                },
                new()
                {
                    id = "boss_hard",
                    type = QuestType.DefeatBoss,
                    difficulty = QuestDifficulty.Hard,
                    requiredCount = 1,
                    rewardCurrency = CurrencyType.Stars,
                    rewardAmount = 5,
                },
                new()
                {
                    id = "gold_easy",
                    type = QuestType.CollectGold,
                    difficulty = QuestDifficulty.Easy,
                    requiredCount = 100,
                    rewardCurrency = CurrencyType.Carrots,
                    rewardAmount = 50,
                },
                new()
                {
                    id = "wave_med",
                    type = QuestType.SurviveWaves,
                    difficulty = QuestDifficulty.Medium,
                    requiredCount = 5,
                    rewardCurrency = CurrencyType.Carrots,
                    rewardAmount = 200,
                },
                new()
                {
                    id = "duration_hard",
                    type = QuestType.RunDuration,
                    difficulty = QuestDifficulty.Hard,
                    requiredCount = 300,
                    rewardCurrency = CurrencyType.Carrots,
                    rewardAmount = 500,
                },
            };
            return cfg;
        }

        private static DateTime Day(int year, int month, int day) =>
            new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Utc);

        [Test]
        public void GetTodaysQuests_ReturnsThreeDeterministicallyForSameSeed()
        {
            DateTime now = Day(2026, 5, 16);
            var a = new QuestService(_save, _config, currency: null, utcNow: () => now);
            var b = new QuestService(_save, _config, currency: null, utcNow: () => now);

            var setA = a.GetTodaysQuests();
            var setB = b.GetTodaysQuests();

            Assert.That(setA, Has.Length.EqualTo(3));
            for (var i = 0; i < 3; i++)
            {
                Assert.That(setA[i], Is.Not.Null, $"Slot {i} must roll a quest.");
                Assert.That(setB[i]!.Id, Is.EqualTo(setA[i]!.Id),
                    "Same player + UTC date must produce identical rotation.");
            }
        }

        [Test]
        public void DailyRotation_PicksOneOfEachDifficulty()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var set = svc.GetTodaysQuests();

            Assert.That(set[0]!.Difficulty, Is.EqualTo(QuestDifficulty.Easy));
            Assert.That(set[1]!.Difficulty, Is.EqualTo(QuestDifficulty.Medium));
            Assert.That(set[2]!.Difficulty, Is.EqualTo(QuestDifficulty.Hard));
        }

        [Test]
        public void OnEvent_KillEnemy_IncrementsKillQuest()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var killQuest = FindByType(svc, QuestType.KillEnemies)!;
            var required = killQuest.RequiredCount;

            for (var i = 0; i < required; i++)
                svc.OnEvent(new EnemyKilledProgress(wasElite: false));

            Assert.That(killQuest.CurrentCount, Is.EqualTo(required));
            Assert.That(killQuest.IsComplete, Is.True);
            Assert.That(killQuest.IsClaimable, Is.True);
        }

        [Test]
        public void OnEvent_LevelReached_SetsAbsoluteProgress()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var lvl = FindByType(svc, QuestType.ReachLevel)!;

            svc.OnEvent(new LevelReachedProgress(3));
            Assert.That(lvl.CurrentCount, Is.EqualTo(3));

            // Lower level event must NOT regress progress.
            svc.OnEvent(new LevelReachedProgress(2));
            Assert.That(lvl.CurrentCount, Is.EqualTo(3));

            svc.OnEvent(new LevelReachedProgress(lvl.RequiredCount + 10));
            Assert.That(lvl.CurrentCount, Is.EqualTo(lvl.RequiredCount));
            Assert.That(lvl.IsComplete, Is.True);
        }

        [Test]
        public void Claim_GrantsRewardAndMarksClaimed_AndIsIdempotent()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var kill = FindByType(svc, QuestType.KillEnemies)!;
            for (var i = 0; i < kill.RequiredCount; i++)
                svc.OnEvent(new EnemyKilledProgress(false));

            var carrotsBefore = _save.Data.Currencies.Carrots;
            var reward = svc.Claim(kill.Id);

            Assert.That(reward.Amount, Is.EqualTo(kill.Reward.Amount),
                "Reward struct must mirror the template amount.");
            Assert.That(_save.Data.Currencies.Carrots, Is.EqualTo(carrotsBefore + kill.Reward.Amount),
                "Carrots must increase by the reward amount (no currency service path).");
            Assert.That(kill.Claimed, Is.True);

            // Second claim → empty reward; no double-credit.
            var second = svc.Claim(kill.Id);
            Assert.That(second.Amount, Is.EqualTo(0));
            Assert.That(_save.Data.Currencies.Carrots, Is.EqualTo(carrotsBefore + kill.Reward.Amount));
        }

        [Test]
        public void Claim_BeforeCompletion_Fails()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var kill = FindByType(svc, QuestType.KillEnemies)!;

            var reward = svc.Claim(kill.Id);
            Assert.That(reward.Amount, Is.EqualTo(0));
            Assert.That(kill.Claimed, Is.False);
        }

        [Test]
        public void Persistence_SaveAndReload_PreservesProgressAndClaim()
        {
            DateTime now = Day(2026, 5, 16);
            var svc1 = new QuestService(_save, _config, null, () => now);
            var kill = FindByType(svc1, QuestType.KillEnemies)!;
            for (var i = 0; i < kill.RequiredCount; i++)
                svc1.OnEvent(new EnemyKilledProgress(false));
            svc1.Claim(kill.Id);
            _save.Save(); // belt-and-braces, Claim already saves

            // Fresh service against the same SaveService instance — should
            // reconcile from QuestState entries.
            var svc2 = new QuestService(_save, _config, null, () => now);
            var killAfter = FindById(svc2, kill.Id);
            Assert.That(killAfter, Is.Not.Null);
            Assert.That(killAfter!.Claimed, Is.True);
            Assert.That(killAfter.CurrentCount, Is.EqualTo(kill.RequiredCount));
        }

        [Test]
        public void Persistence_RoundTripsThroughDisk()
        {
            DateTime now = Day(2026, 5, 16);
            var svc1 = new QuestService(_save, _config, null, () => now);
            var lvl = FindByType(svc1, QuestType.ReachLevel)!;
            svc1.OnEvent(new LevelReachedProgress(3));
            _save.Save();

            // Force load from the InMemoryFileSystem on a brand-new SaveService.
            var save2 = new SaveService(RootDir, _fs);
            save2.Load();
            Assert.That(save2.Data.QuestState.Entries.Count, Is.GreaterThan(0));
            var matching = save2.Data.QuestState.Entries.Find(e => e.Id == lvl.Id);
            Assert.That(matching, Is.Not.Null);
            Assert.That(matching!.Progress, Is.EqualTo(3));
        }

        [Test]
        public void DayRollover_ReplacesQuestSetAndClearsEntries()
        {
            DateTime day1 = Day(2026, 5, 16);
            DateTime day2 = Day(2026, 5, 17);

            var clock = day1;
            var svc = new QuestService(_save, _config, null, () => clock);
            var day1Quests = svc.GetTodaysQuests();
            var anyKill = FindByType(svc, QuestType.KillEnemies);
            if (anyKill != null) svc.OnEvent(new EnemyKilledProgress(false));

            // Advance the clock past midnight UTC.
            clock = day2;
            var day2Quests = svc.GetTodaysQuests();

            Assert.That(day2Quests, Has.Length.EqualTo(3));
            Assert.That(_save.Data.QuestState.RolledForDate, Is.EqualTo("2026-05-17"));

            // Day-1 progress should be wiped from the persisted entries.
            foreach (var entry in _save.Data.QuestState.Entries)
            {
                Assert.That(entry.Progress, Is.EqualTo(0),
                    $"Entry {entry.Id} must reset across UTC rollover.");
                Assert.That(entry.Claimed, Is.False);
            }
        }

        [Test]
        public void QuestUpdated_FiresOnProgressAndClaim()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var fired = new List<string>();
            svc.QuestUpdated += q => fired.Add(q.Id);

            var kill = FindByType(svc, QuestType.KillEnemies)!;
            svc.OnEvent(new EnemyKilledProgress(false));
            Assert.That(fired, Contains.Item(kill.Id));

            for (var i = 1; i < kill.RequiredCount; i++) svc.OnEvent(new EnemyKilledProgress(false));
            fired.Clear();
            svc.Claim(kill.Id);
            Assert.That(fired, Contains.Item(kill.Id),
                "QuestUpdated must fire once on claim.");
        }

        [Test]
        public void GoldCollectedProgress_AccumulatesAmount()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var gold = FindByType(svc, QuestType.CollectGold)!;

            svc.OnEvent(new GoldCollectedProgress(40));
            svc.OnEvent(new GoldCollectedProgress(50));
            Assert.That(gold.CurrentCount, Is.EqualTo(90));
        }

        [Test]
        public void RunDurationProgress_TakesMaxSeconds()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var dur = FindByType(svc, QuestType.RunDuration)!;

            svc.OnEvent(new RunDurationProgress(120f));
            svc.OnEvent(new RunDurationProgress(90f));
            Assert.That(dur.CurrentCount, Is.EqualTo(120));
        }

        [Test]
        public void DefeatBossProgress_IncrementsAnyBoss()
        {
            DateTime now = Day(2026, 5, 16);
            var svc = new QuestService(_save, _config, null, () => now);
            var boss = FindByType(svc, QuestType.DefeatBoss)!;

            svc.OnEvent(new BossDefeatedProgress("any-boss"));
            Assert.That(boss.CurrentCount, Is.EqualTo(1));
        }

        [Test]
        public void Seed_DiffersAcrossDates_SameDayMatches()
        {
            var s1 = QuestPool.ComputeSeed("player-a", Day(2026, 5, 16));
            var s2 = QuestPool.ComputeSeed("player-a", Day(2026, 5, 17));
            var s3 = QuestPool.ComputeSeed("player-a", Day(2026, 5, 16));
            Assert.That(s1, Is.Not.EqualTo(s2));
            Assert.That(s1, Is.EqualTo(s3));
        }

        // ---- helpers ----

        private static Quest? FindByType(QuestService svc, QuestType type)
        {
            foreach (var q in svc.GetTodaysQuests())
            {
                if (q != null && q.Type == type) return q;
            }
            return null;
        }

        private static Quest? FindById(QuestService svc, string id)
        {
            foreach (var q in svc.GetTodaysQuests())
            {
                if (q != null && q.Id == id) return q;
            }
            return null;
        }
    }
}
