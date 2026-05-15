// Brave Bunny — Gameplay/Enemies/BossSpawner
//
// Spawns the boss enemy at the t=420 wave event (per
// docs/09-level-design/01-biomes/meadow/waves.json § boss). Uses the existing
// EnemyPool API per CLAUDE.md "Pooling is mandatory" and ADR-0005.
//
// The boss is an Enemy instance (role=EnemyRole.Boss) with a per-spawn
// BossBehavior strategy attached. Only one boss is alive at a time
// (ADR-0020 bossCapacity=1). On death the pool returns the instance via
// IDeathListener (EnemyPoolReturnOnDeath).
//
// Caller — typically WaveRunner.FireEvent when WaveEvent.boss != null — provides
// the BossDefinition's enemy entry + spawn position. This class wraps the
// EnemyPool.Acquire + Configure step and seeds the BossBehavior with the
// scaled HP coming from the boss-row in enemies.json.
//
// Allocation-free per-spawn aside from the BossBehavior instance (one per
// boss spawn — single-active per ADR-0020).

#nullable enable

using System;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies.Behaviors;
using Brave.Gameplay.Events;
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Stateless helper that instantiates the boss enemy from its <see cref="EnemyPool"/>
    /// and attaches a <see cref="BossBehavior"/> strategy. Caller drives this on the
    /// boss-trigger wave event (see <c>WaveRunner.FireEvent</c>).
    /// </summary>
    public sealed class BossSpawner
    {
        private readonly EnemyPool _pool;
        private readonly BossConfig _config;
        private readonly BossPhaseChannel? _phaseChannel;
        private readonly BossDefeatedChannel? _defeatedChannel;
        private readonly EnemyKilledChannel? _enemyKilledChannel;
        private readonly Func<float>? _runSecondsGetter;
        private readonly Action<string>? _onDefeated;

        public BossConfig Config => _config;

        public BossSpawner(
            EnemyPool pool,
            BossConfig config,
            BossPhaseChannel? phaseChannel = null,
            BossDefeatedChannel? defeatedChannel = null,
            EnemyKilledChannel? enemyKilledChannel = null,
            Func<float>? runSecondsGetter = null,
            Action<string>? onDefeated = null)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _phaseChannel = phaseChannel;
            _defeatedChannel = defeatedChannel;
            _enemyKilledChannel = enemyKilledChannel;
            _runSecondsGetter = runSecondsGetter;
            _onDefeated = onDefeated;
        }

        /// <summary>
        /// Spawn the boss at <paramref name="position"/>. Returns the live Enemy or null
        /// if the pool was exhausted (should never happen — bossCapacity=1).
        /// </summary>
        public Enemy? Spawn(EnemyDefinition bossDef, Vector2 position, float scaledHp)
        {
            if (bossDef == null) throw new ArgumentNullException(nameof(bossDef));
            if (bossDef.role != EnemyRole.Boss)
            {
                Debug.LogError($"BossSpawner: enemy '{bossDef.slug}' role is {bossDef.role}, expected Boss");
                return null;
            }

            var enemy = _pool.Acquire(new Vector3(position.x, 0f, position.y));
            if (enemy == null)
            {
                Debug.LogError($"BossSpawner: pool exhausted for '{bossDef.slug}'");
                return null;
            }

            var behavior = new BossBehavior(
                config: _config,
                phaseChannel: _phaseChannel,
                defeatedChannel: _defeatedChannel,
                enemyKilledChannel: _enemyKilledChannel,
                runSecondsGetter: _runSecondsGetter,
                onDefeated: _onDefeated);

            enemy.Configure(bossDef, scaledHp, behavior, _pool);
            return enemy;
        }
    }
}
