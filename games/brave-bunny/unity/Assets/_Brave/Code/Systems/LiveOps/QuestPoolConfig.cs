// Brave Bunny — Systems / LiveOps
// Wave 9: QuestPoolConfig — ScriptableObject catalogue of quest templates the
// QuestService rolls 3 quests from each UTC day (1 easy, 1 medium, 1 hard).
//
// Design source: docs/02-gdd/02-meta-loop.md (daily missions: 3 / day)
// CLAUDE.md principle 6: every threshold + reward sits in this SO (or the
// .asset under Data/Definitions/LiveOps), never inline in code.

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Progression;
using UnityEngine;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// Inspector-friendly quest template. Mirrors the QuestType enum but adds
    /// the SO-only knobs (required count, reward, loc key, optional boss filter).
    /// Cloned into a runtime Quest by <see cref="QuestPool.Create"/>.
    /// </summary>
    [Serializable]
    public sealed class QuestTemplate
    {
        [Tooltip("Stable slug; persisted in save. Must be unique within the pool.")]
        public string id = string.Empty;

        public QuestType type;

        public QuestDifficulty difficulty;

        [Tooltip("Target count (kills / seconds / waves / level / gold amount).")]
        [Min(1)] public int requiredCount = 1;

        [Tooltip("Currency type granted on claim.")]
        public CurrencyType rewardCurrency = CurrencyType.Carrots;

        [Tooltip("Amount of the reward currency granted on claim.")]
        [Min(0)] public int rewardAmount;

        [Tooltip("Optional: localization key for the title. Defaults to quest.<type>.title when empty.")]
        public string titleLocKey = string.Empty;

        [Tooltip("DefeatBoss only — empty matches any boss.")]
        public string bossFilter = string.Empty;

        /// <summary>Effective loc key — falls back to the conventional pattern.</summary>
        public string EffectiveTitleLocKey =>
            string.IsNullOrEmpty(titleLocKey) ? $"quest.{TypeToken(type)}.title" : titleLocKey;

        private static string TypeToken(QuestType t) => t switch
        {
            QuestType.KillEnemies => "kill_enemies",
            QuestType.SurviveWaves => "survive_waves",
            QuestType.DefeatBoss => "defeat_boss",
            QuestType.ReachLevel => "reach_level",
            QuestType.CollectGold => "collect_gold",
            QuestType.RunDuration => "run_duration",
            _ => "unknown",
        };
    }

    /// <summary>
    /// SO catalogue: ~15 templates spanning Easy/Medium/Hard. Authored in the
    /// Unity inspector at <c>Data/Definitions/LiveOps/QuestPoolConfig.asset</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "Brave/LiveOps/Quest Pool Config", fileName = "QuestPoolConfig", order = 10)]
    public sealed class QuestPoolConfig : ScriptableObject
    {
        [Tooltip("Master template list. QuestPool partitions by Difficulty when rolling.")]
        public List<QuestTemplate> templates = new();

        /// <summary>Read-only enumerable for service consumption / tests.</summary>
        public IReadOnlyList<QuestTemplate> Templates => templates;
    }
}
