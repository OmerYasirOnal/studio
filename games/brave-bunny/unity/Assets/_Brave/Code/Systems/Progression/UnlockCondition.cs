// Brave Bunny — Systems / Progression
// Meta-progression: data class describing the gameplay condition a character
// must satisfy to be unlocked. Authored from data/balance/characters.json
// → mirrored into CharacterDefinition by the BalanceJsonImporter and consulted
// at runtime by CharacterUnlockService.
//
// Design choices:
//   * One flat class rather than a polymorphic hierarchy — keeps JSON shape
//     trivial ({ "type": "reach_wave", "wave": 5, "with_character": "bunny" })
//     and ScriptableObject serialization friendly (Unity does not serialize
//     polymorphic class hierarchies without [SerializeReference] hassle).
//   * `Type` is a string-typed enum-like (lowercase-snake) so the save file is
//     forward-compatible with new condition types per ADR-0008.
//   * `IsSatisfied` consults a CharacterUnlockContext snapshot — no service
//     locator coupling here, so this class stays pure data + a small static
//     evaluator function.
//
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md § Character unlock ladder.
//   * docs/02-gdd/03-characters.md § Unlock condition (per-character).
//   * data/balance/characters.json — `unlock_condition` JSON shape.

#nullable enable

using System;
using Brave.Gameplay.Definitions;
using Newtonsoft.Json;
using UnityEngine;

namespace Brave.Systems.Progression
{
    /// <summary>
    /// String-typed enumeration of supported unlock condition types. New types
    /// extend this set; the evaluator below switches on these strings.
    /// </summary>
    public static class UnlockConditionType
    {
        /// <summary>Always unlocked — used by starters (e.g. bunny).</summary>
        public const string None = "none";

        /// <summary>Reach a specific wave ordinal with a specific character (or any).</summary>
        public const string ReachWave = "reach_wave";

        /// <summary>Defeat a specific boss (by slug) at least once.</summary>
        public const string DefeatBoss = "defeat_boss";

        /// <summary>Complete N runs (any outcome) with a specific character (or any).</summary>
        public const string CompleteRuns = "complete_runs";

        /// <summary>Pay a Star price — used by the meta-loop ladder.</summary>
        public const string PayStars = "pay_stars";
    }

    /// <summary>
    /// Designer-facing unlock condition. Serialized in <c>characters.json</c>
    /// under <c>unlock_condition</c>, mirrored to a <c>CharacterDefinition</c>
    /// at edit time, and re-evaluated by <c>CharacterUnlockService</c> at
    /// runtime against a <see cref="CharacterUnlockContext"/>.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class UnlockCondition
    {
        /// <summary>One of <see cref="UnlockConditionType"/> values.</summary>
        [JsonProperty("type")] public string Type = UnlockConditionType.None;

        /// <summary>Wave ordinal for <see cref="UnlockConditionType.ReachWave"/>.</summary>
        [JsonProperty("wave")] public int Wave;

        /// <summary>Boss slug for <see cref="UnlockConditionType.DefeatBoss"/>.</summary>
        [JsonProperty("boss")] public string? Boss;

        /// <summary>Run count for <see cref="UnlockConditionType.CompleteRuns"/>.</summary>
        [JsonProperty("runs")] public int Runs;

        /// <summary>Optional gating: only count progress made with this character slug.</summary>
        [JsonProperty("withCharacter")] public string? WithCharacter;

        /// <summary>Star price for <see cref="UnlockConditionType.PayStars"/>.</summary>
        [JsonProperty("stars")] public int Stars;

        /// <summary>True if this condition has no gating — equivalent to "starter".</summary>
        public bool IsUnconditional() =>
            string.IsNullOrEmpty(Type) || Type == UnlockConditionType.None;

        /// <summary>
        /// Evaluate the condition against a snapshot of current progression
        /// state. Pay-stars conditions return false — they are handled by an
        /// explicit purchase flow in <c>CharacterUnlockService.TryPurchase</c>,
        /// not by passive evaluation.
        /// </summary>
        public bool IsSatisfied(CharacterUnlockContext ctx)
        {
            if (IsUnconditional()) return true;
            switch (Type)
            {
                case UnlockConditionType.ReachWave:
                    return ctx.GetHighestWaveReached(WithCharacter) >= Wave;
                case UnlockConditionType.DefeatBoss:
                    return !string.IsNullOrEmpty(Boss) && ctx.WasBossDefeated(Boss!);
                case UnlockConditionType.CompleteRuns:
                    return ctx.GetRunsCompleted(WithCharacter) >= Runs;
                case UnlockConditionType.PayStars:
                    // Star-purchase is an explicit user action — not satisfied by
                    // passive evaluation. CharacterUnlockService.TryPurchase
                    // bypasses IsSatisfied for this branch.
                    return false;
                default:
                    Debug.LogWarning($"[UnlockCondition] unknown type '{Type}' — treating as locked");
                    return false;
            }
        }
    }

    /// <summary>
    /// Bridges <see cref="UnlockConditionData"/> (inspector-friendly raw struct
    /// inside the Gameplay asmdef) into the runtime <see cref="UnlockCondition"/>
    /// POCO. Lives on the Systems side because Systems references Gameplay
    /// (the reverse is forbidden by the asmdef layering).
    /// </summary>
    public static class UnlockConditionDataExtensions
    {
        /// <summary>
        /// Translate the SO-serialized raw fields into a runtime
        /// <see cref="UnlockCondition"/>. Returns null when the data declares
        /// no condition (treated as "starter / always unlocked" by callers).
        /// </summary>
        public static UnlockCondition? ToRuntime(this UnlockConditionData? raw)
        {
            if (raw == null) return null;
            if (string.IsNullOrEmpty(raw.type) || raw.type == UnlockConditionType.None) return null;
            return new UnlockCondition
            {
                Type = raw.type,
                Wave = raw.wave,
                Boss = string.IsNullOrEmpty(raw.boss) ? null : raw.boss,
                Runs = raw.runs,
                WithCharacter = string.IsNullOrEmpty(raw.withCharacter) ? null : raw.withCharacter,
                Stars = raw.stars,
            };
        }
    }

    /// <summary>
    /// Read-only snapshot of progression state consulted by
    /// <see cref="UnlockCondition.IsSatisfied"/>. Built on demand by
    /// <c>CharacterUnlockService</c> from the live <c>SaveData</c>.
    /// </summary>
    public readonly struct CharacterUnlockContext
    {
        private readonly Func<string?, int> _highestWave;
        private readonly Func<string?, int> _runsCompleted;
        private readonly Func<string, bool> _bossDefeated;

        public CharacterUnlockContext(
            Func<string?, int> highestWave,
            Func<string?, int> runsCompleted,
            Func<string, bool> bossDefeated)
        {
            _highestWave = highestWave;
            _runsCompleted = runsCompleted;
            _bossDefeated = bossDefeated;
        }

        public int GetHighestWaveReached(string? slug) => _highestWave(slug);
        public int GetRunsCompleted(string? slug) => _runsCompleted(slug);
        public bool WasBossDefeated(string bossSlug) => _bossDefeated(bossSlug);
    }
}
