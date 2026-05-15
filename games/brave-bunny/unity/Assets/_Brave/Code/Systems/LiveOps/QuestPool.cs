// Brave Bunny — Systems / LiveOps
// Wave 9: QuestPool — pure-C# helper that turns a QuestPoolConfig SO + a UTC
// date + (optional) player-id seed into 3 deterministic daily quests.
//
// Determinism contract (per scope brief):
//   "same player + date = same 3 quests"
//   Seed = playerId.GetHashCode() ^ utcDate.Date.ToOADate-as-int
// One Easy + one Medium + one Hard are picked (best-effort: if a bucket is
// empty, falls back to any other bucket so the service never short-rolls).

#nullable enable

using System;
using System.Collections.Generic;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// Daily-quest selection. Stateless; <see cref="RollDaily"/> is pure given
    /// (config, date, playerId).
    /// </summary>
    public static class QuestPool
    {
        /// <summary>Number of quests rolled per UTC day (Easy + Medium + Hard).</summary>
        public const int QuestsPerDay = 3;

        private static readonly QuestDifficulty[] DailyOrder =
        {
            QuestDifficulty.Easy,
            QuestDifficulty.Medium,
            QuestDifficulty.Hard,
        };

        /// <summary>
        /// Deterministic daily seed. Same playerId + utcDate.Date always yields
        /// the same integer regardless of clock-of-day.
        /// </summary>
        public static int ComputeSeed(string playerId, DateTime utcDate)
        {
            // Day-precision: drop time-of-day so seed is stable across the day.
            var day = utcDate.Date;
            // Treat empty player-id as a constant so first-launch + no-id still
            // produces a deterministic rotation that the tests can pin.
            var pid = string.IsNullOrEmpty(playerId) ? "anon" : playerId;
            unchecked
            {
                var h = 17;
                h = h * 31 + StringHash(pid);
                h = h * 31 + day.Year;
                h = h * 31 + day.Month;
                h = h * 31 + day.Day;
                return h;
            }
        }

        /// <summary>Deterministic FNV-1a so the seed survives platform-specific GetHashCode quirks.</summary>
        private static int StringHash(string s)
        {
            unchecked
            {
                const int prime = 16777619;
                var hash = (int)2166136261;
                for (var i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= prime;
                }
                return hash;
            }
        }

        /// <summary>
        /// Roll <see cref="QuestsPerDay"/> quests from the pool. Always returns
        /// an array of length 3 (with nulls only if the pool is entirely empty).
        /// </summary>
        public static Quest?[] RollDaily(QuestPoolConfig config, string playerId, DateTime utcDate)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            var seed = ComputeSeed(playerId, utcDate);
            return RollDailyWithSeed(config.Templates, seed);
        }

        /// <summary>Test-friendly overload: pass templates + seed directly.</summary>
        public static Quest?[] RollDailyWithSeed(IReadOnlyList<QuestTemplate> templates, int seed)
        {
            var result = new Quest?[QuestsPerDay];
            if (templates == null || templates.Count == 0) return result;

            // Bucket the templates by difficulty. Stable order (input order) so
            // the seed-driven pick is reproducible across runs.
            var easy = new List<QuestTemplate>();
            var medium = new List<QuestTemplate>();
            var hard = new List<QuestTemplate>();
            for (var i = 0; i < templates.Count; i++)
            {
                var t = templates[i];
                if (t == null) continue;
                switch (t.difficulty)
                {
                    case QuestDifficulty.Easy: easy.Add(t); break;
                    case QuestDifficulty.Medium: medium.Add(t); break;
                    case QuestDifficulty.Hard: hard.Add(t); break;
                }
            }

            // System.Random seeded once — same seed → identical Next() sequence.
            var rng = new Random(seed);

            // Track already-picked ids to avoid roll-twice within the same day.
            var picked = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < QuestsPerDay; i++)
            {
                var bucket = DailyOrder[i] switch
                {
                    QuestDifficulty.Easy => easy,
                    QuestDifficulty.Medium => medium,
                    QuestDifficulty.Hard => hard,
                    _ => easy,
                };
                var chosen = PickFromBucket(bucket, picked, rng)
                             ?? PickFromBucket(easy, picked, rng)
                             ?? PickFromBucket(medium, picked, rng)
                             ?? PickFromBucket(hard, picked, rng);
                if (chosen == null) continue;
                picked.Add(chosen.id);
                result[i] = Create(chosen);
            }

            return result;
        }

        private static QuestTemplate? PickFromBucket(List<QuestTemplate> bucket, HashSet<string> excluded, Random rng)
        {
            // Linear scan into a candidate list then random-index. Bucket sizes
            // are tiny (≤ a couple dozen) so this is O(n) and allocation-free.
            QuestTemplate? first = null;
            var count = 0;
            // Reservoir-of-size-1 selection: equivalent to uniform random pick
            // over (bucket − excluded) without allocating a filtered list.
            foreach (var t in bucket)
            {
                if (excluded.Contains(t.id)) continue;
                count++;
                if (count == 1) { first = t; continue; }
                if (rng.Next(count) == 0) first = t;
            }
            return first;
        }

        /// <summary>Public factory — instantiate the concrete <see cref="Quest"/> for a template.</summary>
        public static Quest Create(QuestTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            var reward = new QuestReward(template.rewardCurrency, template.rewardAmount);
            var loc = template.EffectiveTitleLocKey;

            return template.type switch
            {
                QuestType.KillEnemies => new KillEnemiesQuest(template.id, template.difficulty, template.requiredCount, reward, loc),
                QuestType.SurviveWaves => new SurviveWavesQuest(template.id, template.difficulty, template.requiredCount, reward, loc),
                QuestType.DefeatBoss => new DefeatBossQuest(template.id, template.difficulty, template.requiredCount, reward, loc, template.bossFilter),
                QuestType.ReachLevel => new ReachLevelQuest(template.id, template.difficulty, template.requiredCount, reward, loc),
                QuestType.CollectGold => new CollectGoldQuest(template.id, template.difficulty, template.requiredCount, reward, loc),
                QuestType.RunDuration => new RunDurationQuest(template.id, template.difficulty, template.requiredCount, reward, loc),
                _ => throw new ArgumentOutOfRangeException(nameof(template), $"Unsupported QuestType {template.type}"),
            };
        }
    }
}
