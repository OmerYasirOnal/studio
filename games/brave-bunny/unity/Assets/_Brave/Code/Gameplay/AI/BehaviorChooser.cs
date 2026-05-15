// Brave Bunny — Gameplay/AI/BehaviorChooser
//
// Role → EnemyBehavior dispatch. WaveRunner.FireEvent picks a behavior for each
// spawn via this chooser; SwarmerBehavior / EliteBehavior / RangedBehavior /
// TankBehavior are stateless singletons (one shared instance per archetype) so
// 200 enemies share 4 behavior objects. BossBehavior is the exception — it is
// per-instance because the boss is single-active (ADR-0020 bossCapacity=1) and
// carries phase / attack state.
//
// Singletons live here so call-sites do not allocate per spawn. The Boss case
// returns null because BossSpawner constructs the per-spawn BossBehavior at
// dequeue time (it needs channel refs + config that this static chooser does
// not hold). Callers spawning a Boss should route through BossSpawner, not the
// generic Spawner path.
//
// Spec refs:
//   * docs/06-tech-spec/05-runtime-architecture.md § AI dispatch table
//   * docs/decisions/0020-weapon-archetype-config-and-boss-enum.md § EnemyRole.Boss

#nullable enable

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.AI
{
    /// <summary>
    /// Static role → behavior dispatch. The four non-boss archetypes share a
    /// single instance per role; <see cref="EnemyRole.Boss"/> returns null because
    /// the boss behavior is per-spawn and is constructed by <c>BossSpawner</c>.
    /// </summary>
    public static class BehaviorChooser
    {
        // ---- Default tunings (mirror the existing constructors in qa tests; real
        //      values come from EnemyDefinition + balance JSON when the spawner
        //      starts feeding per-enemy config through). ----
        private const float EliteTelegraphMs        = 600f;
        private const float RangedKiteDistance      = 3.0f;
        private const float RangedFireWindowMin     = 3.0f;
        private const float RangedFireWindowMax     = 6.0f;
        private const float RangedTelegraphMs       = 500f;
        private const float RangedProjectileSpeed   = 4.0f;
        private const float TankChargeIntervalMs    = 4000f;
        private const float TankBurstSpeedMult      = 1.5f;
        private const float TankBurstDurationMs     = 1000f;
        private const float TankTelegraphMs         = 400f;

        // Singleton instances — one per archetype, shared across all enemies of that role.
        private static readonly EnemyBehavior _swarmer = new SwarmerBehavior();
        private static readonly EnemyBehavior _elite   = new EliteBehavior(EliteTelegraphMs);
        private static readonly EnemyBehavior _ranged  = new RangedBehavior(
            kiteDistance: RangedKiteDistance,
            fireWindow:   new UnityEngine.Vector2(RangedFireWindowMin, RangedFireWindowMax),
            telegraphMs:  RangedTelegraphMs,
            projectileSpeed: RangedProjectileSpeed);
        private static readonly EnemyBehavior _tank    = new TankBehavior(
            chargeIntervalMs:   TankChargeIntervalMs,
            burstSpeedMult:     TankBurstSpeedMult,
            burstDurationMs:    TankBurstDurationMs,
            telegraphMs:        TankTelegraphMs);

        /// <summary>
        /// Pick an EnemyBehavior for the given <paramref name="role"/>. Returns null for
        /// <see cref="EnemyRole.Boss"/> — boss spawning routes through <c>BossSpawner</c>
        /// which constructs the per-instance <c>BossBehavior</c> with channel refs.
        /// </summary>
        public static EnemyBehavior? For(EnemyRole role) => role switch
        {
            EnemyRole.Swarmer => _swarmer,
            EnemyRole.Tank    => _tank,
            EnemyRole.Ranged  => _ranged,
            EnemyRole.Elite   => _elite,
            EnemyRole.Boss    => null,   // see BossSpawner
            _                 => _swarmer,
        };
    }
}
