// Brave Bunny — Systems / Progression
// Meta-progression: evaluates UnlockConditions and persists unlock state.
//
// Design / scope:
//   * Sits beside ProgressionService (which owns currency + level XP). This
//     service owns ONLY the "is this character unlocked yet?" question and the
//     lifetime stats that feed UnlockCondition.IsSatisfied.
//   * Persists via the shared ISaveService — no new save format. New fields
//     on CharacterProfile (Unlocked, UnlockedAt, RunsCompleted,
//     BossesDefeated, HighestWaveReached) are JsonProperty-tagged per
//     ADR-0008 forward-compat.
//   * Registry of (slug → UnlockCondition) is injected — UI / boot wires this
//     from the CharacterDefinition catalogue. Tests pass an in-memory dict.
//   * Run-end event subscription happens in the UI/boot layer; this service
//     exposes RecordRunCompletion + RecordBossDefeated for the subscriber to
//     call. Keeps this service free of UnityEvent / channel coupling so
//     EditMode tests can drive it without Unity SO assets.
//
// Spec refs:
//   * docs/02-gdd/02-meta-loop.md § Character unlock ladder.
//   * docs/02-gdd/03-characters.md § Unlock condition per character.
//   * docs/06-tech-spec/03-save-system.md § Save triggers (CharacterUnlock).

#nullable enable

using System;
using System.Collections.Generic;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Progression
{
    /// <summary>Service contract — see <see cref="CharacterUnlockService"/>.</summary>
    public interface ICharacterUnlockService : IService
    {
        /// <summary>True if the character is playable (starter, condition met, or purchased).</summary>
        bool IsUnlocked(string slug);

        /// <summary>Enumerate every slug currently unlocked (deterministic order).</summary>
        IReadOnlyList<string> GetUnlockedCharacterIds();

        /// <summary>
        /// Re-evaluate all known conditions against current save state and
        /// fire <see cref="CharacterUnlocked"/> for any slug that newly clears.
        /// Idempotent — already-unlocked slugs are not re-fired.
        /// </summary>
        /// <returns>Slugs newly unlocked by this evaluation pass.</returns>
        IReadOnlyList<string> EvaluateAll();

        /// <summary>
        /// Record a finished run for <paramref name="characterSlug"/>. Updates
        /// lifetime stats then re-evaluates conditions.
        /// </summary>
        void RecordRunCompletion(string characterSlug, int waveReached, int bossesDefeatedThisRun);

        /// <summary>
        /// Record a boss defeat (called by the boss-defeat event subscriber).
        /// Updates the boss-defeated registry then re-evaluates conditions.
        /// </summary>
        void RecordBossDefeated(string bossSlug, string characterSlug);

        /// <summary>Star-purchase branch — spends Stars then marks the slug unlocked.</summary>
        bool TryPurchase(string slug, CurrencyWallet wallet);

        /// <summary>Raised when a slug transitions from locked → unlocked.</summary>
        event Action<string>? CharacterUnlocked;
    }

    /// <inheritdoc/>
    public sealed class CharacterUnlockService : ICharacterUnlockService
    {
        private readonly ISaveService _save;
        private readonly Dictionary<string, UnlockCondition?> _conditions;
        private readonly HashSet<string> _bossesDefeated = new(StringComparer.Ordinal);

        public event Action<string>? CharacterUnlocked;

        /// <param name="save">Save service for persistence.</param>
        /// <param name="conditions">
        /// Slug → condition map. A null condition (or one with type=None)
        /// marks the slug as a starter — unlocked from the first call.
        /// </param>
        public CharacterUnlockService(
            ISaveService save,
            IReadOnlyDictionary<string, UnlockCondition?> conditions)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            _conditions = new Dictionary<string, UnlockCondition?>(conditions, StringComparer.Ordinal);
            // Seed starters immediately — defensive against fresh saves that
            // didn't run through DefaultSaveFactory (e.g. EditMode tests).
            EvaluateAll();
        }

        public bool IsUnlocked(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            // Starter slugs are implicitly unlocked even before persistence.
            if (_conditions.TryGetValue(slug, out var cond) && (cond == null || cond.IsUnconditional()))
                return true;
            return _save.Data.Characters.TryGetValue(slug, out var p) && (p.Unlocked || p.Owned);
        }

        public IReadOnlyList<string> GetUnlockedCharacterIds()
        {
            var result = new List<string>();
            // Enumerate in the registry's canonical order (insertion order of
            // the dict — preserved by Dictionary<TKey,TValue> since .NET Core 2.0+).
            foreach (var slug in _conditions.Keys)
            {
                if (IsUnlocked(slug)) result.Add(slug);
            }
            return result;
        }

        public IReadOnlyList<string> EvaluateAll()
        {
            var newlyUnlocked = new List<string>();
            var ctx = BuildContext();
            foreach (var (slug, condition) in _conditions)
            {
                var profile = GetOrCreate(slug);
                if (profile.Unlocked || profile.Owned) continue;

                bool clears = condition == null || condition.IsUnconditional() || condition.IsSatisfied(ctx);
                if (!clears) continue;

                profile.Unlocked = true;
                profile.UnlockedAt = DateTime.UtcNow.ToString("o");
                newlyUnlocked.Add(slug);
            }
            if (newlyUnlocked.Count > 0)
            {
                _save.Save(); // 03-save-system.md trigger: "Character unlocked"
                foreach (var slug in newlyUnlocked) CharacterUnlocked?.Invoke(slug);
            }
            return newlyUnlocked;
        }

        public void RecordRunCompletion(string characterSlug, int waveReached, int bossesDefeatedThisRun)
        {
            if (string.IsNullOrEmpty(characterSlug)) return;
            var profile = GetOrCreate(characterSlug);
            profile.RunsCompleted++;
            if (waveReached > profile.HighestWaveReached) profile.HighestWaveReached = waveReached;
            if (bossesDefeatedThisRun > 0) profile.BossesDefeated += bossesDefeatedThisRun;
            // Persist stat bump and check conditions — Save() is called inside
            // EvaluateAll() iff anything newly unlocked. Otherwise we still need
            // to persist the lifetime stat tick.
            var unlocked = EvaluateAll();
            if (unlocked.Count == 0) _save.Save();
        }

        public void RecordBossDefeated(string bossSlug, string characterSlug)
        {
            if (string.IsNullOrEmpty(bossSlug)) return;
            _bossesDefeated.Add(bossSlug);
            if (!string.IsNullOrEmpty(characterSlug))
            {
                var profile = GetOrCreate(characterSlug);
                profile.BossesDefeated++;
            }
            var unlocked = EvaluateAll();
            if (unlocked.Count == 0) _save.Save();
        }

        public bool TryPurchase(string slug, CurrencyWallet wallet)
        {
            if (string.IsNullOrEmpty(slug)) return false;
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            if (!_conditions.TryGetValue(slug, out var cond) || cond == null) return false;
            if (cond.Type != UnlockConditionType.PayStars) return false;
            if (IsUnlocked(slug)) return false;
            if (!wallet.TrySpend(CurrencyType.Stars, cond.Stars)) return false;

            var profile = GetOrCreate(slug);
            profile.Unlocked = true;
            profile.Owned = true;
            profile.UnlockedAt = DateTime.UtcNow.ToString("o");
            _save.Save();
            CharacterUnlocked?.Invoke(slug);
            return true;
        }

        // ---- helpers ----

        private CharacterProfile GetOrCreate(string slug)
        {
            if (!_save.Data.Characters.TryGetValue(slug, out var profile))
            {
                profile = new CharacterProfile { Owned = false, Level = 1, Xp = 0 };
                _save.Data.Characters[slug] = profile;
            }
            return profile;
        }

        private CharacterUnlockContext BuildContext()
        {
            int HighestWave(string? withSlug)
            {
                if (string.IsNullOrEmpty(withSlug))
                {
                    var max = 0;
                    foreach (var p in _save.Data.Characters.Values)
                        if (p.HighestWaveReached > max) max = p.HighestWaveReached;
                    return max;
                }
                return _save.Data.Characters.TryGetValue(withSlug!, out var profile)
                    ? profile.HighestWaveReached : 0;
            }

            int RunsCompleted(string? withSlug)
            {
                if (string.IsNullOrEmpty(withSlug))
                {
                    var total = 0;
                    foreach (var p in _save.Data.Characters.Values) total += p.RunsCompleted;
                    return total;
                }
                return _save.Data.Characters.TryGetValue(withSlug!, out var profile)
                    ? profile.RunsCompleted : 0;
            }

            bool BossDefeated(string bossSlug) => _bossesDefeated.Contains(bossSlug);

            return new CharacterUnlockContext(HighestWave, RunsCompleted, BossDefeated);
        }
    }
}
