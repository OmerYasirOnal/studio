// Reads WaveDefinition (from waves.json) and schedules spawns over the run-clock timeline.
// Level-designer's waves.json is the source of truth; gameplay-engineer never modifies it.
using Brave.Gameplay.AI;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Spawning;

public sealed class WaveRunner
{
    private readonly WaveDefinition _wave;
    private readonly Spawner _spawner;
    private readonly BossSpawner _bossSpawner;
    private readonly Vector2[] _positionBuffer = new Vector2[64];  // pre-allocated per perf-budget

    private int _cursor;       // index of next event to fire
    private float _lastSeconds;

    public WaveRunner(WaveDefinition wave, Spawner spawner, BossSpawner bossSpawner = null)
    {
        _wave = wave;
        _spawner = spawner;
        _bossSpawner = bossSpawner;
    }

    public void Reset() { _cursor = 0; _lastSeconds = 0f; }

    /// <summary>Fire any events whose <c>triggerSeconds</c> is in (lastSeconds, nowSeconds].</summary>
    public void Tick(float nowSeconds, Vector2 playerPos)
    {
        if (_wave == null || _wave.events == null) return;
        while (_cursor < _wave.events.Length)
        {
            var evt = WaveEvent.FromEntry(_wave.events[_cursor]);
            if (evt.triggerSeconds > nowSeconds) break;
            FireEvent(evt, playerPos, nowSeconds);
            _cursor++;
        }
        _lastSeconds = nowSeconds;
    }

    private void FireEvent(in WaveEvent evt, Vector2 playerPos, float nowSeconds)
    {
        if (evt.enemy == null && evt.boss == null) return;
        if (evt.spawnCount <= 0) return;

        // Boss spawn — singleton encounter; route through BossSpawner which constructs
        // the per-instance BossBehavior with channel refs (BehaviorChooser returns null
        // for EnemyRole.Boss by design).
        if (evt.type == WaveEventType.Boss && evt.boss != null && _bossSpawner != null)
        {
            float scaledBossHp = evt.boss.baseHP;   // pool already scaled per minute upstream.
            _bossSpawner.Spawn(evt.boss, playerPos, scaledBossHp);
            return;
        }

        if (evt.spawnCount > _positionBuffer.Length)
        {
            Debug.LogWarning($"WaveRunner: spawnCount {evt.spawnCount} exceeds buffer; clamping");
        }
        int count = Mathf.Min(evt.spawnCount, _positionBuffer.Length);
        Vector2 dirHint = DirectionFromString(evt.fromDirection);
        SpawnPatternHelper.Resolve(evt.pattern, playerPos, count, Mathf.Max(1f, evt.radius), dirHint, _positionBuffer);

        float runMinutes = nowSeconds / 60f;
        // Role → behavior dispatch via singleton chooser (allocation-free; one shared
        // instance per archetype). Boss is excluded above.
        var behavior = evt.enemy != null ? BehaviorChooser.For(evt.enemy.role) : null;
        for (int i = 0; i < count; i++)
        {
            _spawner.Spawn(evt.enemy, _positionBuffer[i], runMinutes, behavior);
        }
    }

    private static Vector2 DirectionFromString(string s) => s?.ToLowerInvariant() switch
    {
        "north" => Vector2.up,
        "east"  => Vector2.right,
        "south" => Vector2.down,
        "west"  => Vector2.left,
        _       => Vector2.up,
    };
}
