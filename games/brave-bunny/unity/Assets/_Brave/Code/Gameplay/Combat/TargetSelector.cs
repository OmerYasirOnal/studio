#nullable enable
// ADR-0019 follow-up: target-acquisition strategy dispatch.
//
// Closes the Phase-5 TODO in ProjectileWeapon.OnFire where the targeting mode
// (Nearest / Furthest / Random / LowestHP) was stubbed to "fire straight up".
// AutoAttackController already has a XZ-aware Nearest scorer (with a front-arc
// preference) — that helper stays as the production hot-path for the direct-cast
// vertical slice. TargetSelector is the generalized, allocation-free strategy
// table used by polymorphic weapons (ProjectileWeapon et al.) which read
// WeaponDefinition.targeting at fire-time.
//
// Performance contract (CLAUDE.md game perf budget, 200 enemies):
//   * No allocations per call — caller supplies the scratch List<EnemyBase>.
//   * Single pass through the snapshot for Nearest / Furthest / LowestHP.
//   * Random uses UnityEngine.Random.Range (no managed allocations).
//   * XZ-plane semantics per ADR-0018/ADR-0019.

using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.Combat
{
    /// <summary>Strategy enum kept local to the combat module to avoid bloating
    /// <see cref="TargetingMode"/> (owned by Definitions / ADR-0020). Map from the
    /// data-driven <see cref="TargetingMode"/> via <see cref="FromTargetingMode"/>.</summary>
    public enum TargetStrategy
    {
        Nearest,
        Furthest,
        Random,
        LowestHP,
    }

    /// <summary>Pure (almost — Random.Range is the one impurity) allocation-free
    /// target selector. Caller is responsible for snapshotting the in-range enemy
    /// list (typically via <see cref="EnemyRegistry.SnapshotActiveInRange"/>) into
    /// the supplied scratch buffer.</summary>
    public static class TargetSelector
    {
        /// <summary>Maps a data-driven <see cref="TargetingMode"/> onto a
        /// <see cref="TargetStrategy"/>. Non-targeting modes (SelfCentered /
        /// OrbitPlayer / RandomScreenPos) fall through to <see cref="TargetStrategy.Nearest"/>
        /// — they are positional placement modes, not target acquisition.</summary>
        public static TargetStrategy FromTargetingMode(TargetingMode mode)
        {
            return mode switch
            {
                TargetingMode.Nearest        => TargetStrategy.Nearest,
                TargetingMode.Furthest       => TargetStrategy.Furthest,
                TargetingMode.Random         => TargetStrategy.Random,
                TargetingMode.SelfCentered   => TargetStrategy.Nearest,
                TargetingMode.OrbitPlayer    => TargetStrategy.Nearest,
                TargetingMode.RandomScreenPos => TargetStrategy.Random,
                _ => TargetStrategy.Nearest,
            };
        }

        /// <summary>Selects a target from the pre-filtered (in-range, alive)
        /// <paramref name="candidates"/> list according to <paramref name="strategy"/>.
        /// Returns <c>null</c> when the list is empty. Allocation-free.</summary>
        /// <param name="origin">XZ-plane origin (the firing actor's position) — used by
        /// Nearest / Furthest distance scoring. Y component ignored (ADR-0018).</param>
        public static EnemyBase? Select(
            Vector3 origin,
            IReadOnlyList<EnemyBase> candidates,
            TargetStrategy strategy)
        {
            int n = candidates.Count;
            if (n == 0) return null;
            if (n == 1) return candidates[0];

            return strategy switch
            {
                TargetStrategy.Nearest   => SelectNearest(origin, candidates),
                TargetStrategy.Furthest  => SelectFurthest(origin, candidates),
                TargetStrategy.Random    => SelectRandom(candidates),
                TargetStrategy.LowestHP  => SelectLowestHP(candidates),
                _ => SelectNearest(origin, candidates),
            };
        }

        private static EnemyBase SelectNearest(Vector3 origin, IReadOnlyList<EnemyBase> candidates)
        {
            EnemyBase best = candidates[0];
            float bestSq = SqrXZ(best.transform.position - origin);
            for (int i = 1, n = candidates.Count; i < n; i++)
            {
                var e = candidates[i];
                float sq = SqrXZ(e.transform.position - origin);
                if (sq < bestSq) { bestSq = sq; best = e; }
            }
            return best;
        }

        private static EnemyBase SelectFurthest(Vector3 origin, IReadOnlyList<EnemyBase> candidates)
        {
            EnemyBase best = candidates[0];
            float bestSq = SqrXZ(best.transform.position - origin);
            for (int i = 1, n = candidates.Count; i < n; i++)
            {
                var e = candidates[i];
                float sq = SqrXZ(e.transform.position - origin);
                if (sq > bestSq) { bestSq = sq; best = e; }
            }
            return best;
        }

        private static EnemyBase SelectRandom(IReadOnlyList<EnemyBase> candidates)
        {
            int idx = Random.Range(0, candidates.Count);
            return candidates[idx];
        }

        private static EnemyBase SelectLowestHP(IReadOnlyList<EnemyBase> candidates)
        {
            EnemyBase best = candidates[0];
            float bestHp = HpOf(best);
            for (int i = 1, n = candidates.Count; i < n; i++)
            {
                var e = candidates[i];
                float hp = HpOf(e);
                if (hp < bestHp) { bestHp = hp; best = e; }
            }
            return best;
        }

        private static float SqrXZ(Vector3 d) => d.x * d.x + d.z * d.z;

        private static float HpOf(EnemyBase e)
        {
            // Defensive: EnemyBase.Health is populated in Awake. In production it is never
            // null on a registered enemy. Treat a missing EnemyHealth as +inf so it never
            // wins a "lowest HP" race.
            var h = e.Health;
            return h != null ? h.Hp : float.PositiveInfinity;
        }
    }
}
