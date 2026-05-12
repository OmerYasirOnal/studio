// Reads WaveDefinition (from waves.json) and schedules spawns over the run-clock timeline.
// Level-designer's waves.json is the source of truth; gameplay-engineer never modifies it.
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Spawning;

public sealed class WaveRunner
{
    private readonly WaveDefinition _wave;
    private readonly Spawner _spawner;
    private readonly Vector2[] _positionBuffer = new Vector2[64];  // pre-allocated per perf-budget

    private int _cursor;       // index of next event to fire
    private float _lastSeconds;

    public WaveRunner(WaveDefinition wave, Spawner spawner)
    {
        _wave = wave;
        _spawner = spawner;
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
        if (evt.spawnCount > _positionBuffer.Length)
        {
            Debug.LogWarning($"WaveRunner: spawnCount {evt.spawnCount} exceeds buffer; clamping");
        }
        int count = Mathf.Min(evt.spawnCount, _positionBuffer.Length);
        Vector2 dirHint = DirectionFromString(evt.fromDirection);
        SpawnPatternHelper.Resolve(evt.pattern, playerPos, count, Mathf.Max(1f, evt.radius), dirHint, _positionBuffer);

        float runMinutes = nowSeconds / 60f;
        for (int i = 0; i < count; i++)
        {
            // TODO(Phase 5): pick behavior strategy per enemy.role; share singletons across enemies.
            _spawner.Spawn(evt.enemy, _positionBuffer[i], runMinutes, behavior: null);
        }

        // Boss spawn is handled separately by BossSpawner; WaveRunner only emits a request.
        // TODO(Phase 5): wire to BossSpawner when type == Boss.
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
