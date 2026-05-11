// Patterns drawn from docs/09-level-design/01-biomes/meadow/waves.json schema.
using UnityEngine;

namespace Brave.Gameplay.Spawning;

public enum SpawnPattern
{
    Ring,            // N evenly-spaced around player at "radius"
    Stream,          // spawned at a directional edge ("from": north/east/south/west)
    Flank,           // two side bursts perpendicular to player facing
    Scatter,         // random scattered around the arena
    ScriptedSpawn,   // single named spawn (elite telegraph)
    CenterSpawn,     // arena center (boss)
}

public static class SpawnPatternHelper
{
    public static SpawnPattern Parse(string s) => s?.ToLowerInvariant() switch
    {
        "ring" => SpawnPattern.Ring,
        "stream" => SpawnPattern.Stream,
        "flank" => SpawnPattern.Flank,
        "scatter" => SpawnPattern.Scatter,
        "scripted-spawn" => SpawnPattern.ScriptedSpawn,
        "center-spawn" => SpawnPattern.CenterSpawn,
        _ => SpawnPattern.Scatter,
    };

    /// <summary>Fills <paramref name="positions"/> with concrete world positions for the pattern.</summary>
    public static void Resolve(
        SpawnPattern pattern,
        Vector2 playerPos,
        int count,
        float radius,
        Vector2 directionHint,
        Vector2[] positions)
    {
        if (positions == null || positions.Length < count) return;
        switch (pattern)
        {
            case SpawnPattern.Ring:
                float step = 360f / Mathf.Max(1, count);
                for (int i = 0; i < count; i++)
                {
                    float a = i * step * Mathf.Deg2Rad;
                    positions[i] = playerPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
                }
                break;
            case SpawnPattern.Stream:
                for (int i = 0; i < count; i++)
                    positions[i] = playerPos + directionHint * radius + new Vector2(i * 0.5f, 0f);
                break;
            case SpawnPattern.Flank:
                Vector2 perp = new(-directionHint.y, directionHint.x);
                for (int i = 0; i < count; i++)
                    positions[i] = playerPos + perp * (radius * ((i % 2 == 0) ? 1f : -1f));
                break;
            case SpawnPattern.Scatter:
                for (int i = 0; i < count; i++)
                    positions[i] = playerPos + Random.insideUnitCircle * radius;
                break;
            case SpawnPattern.ScriptedSpawn:
            case SpawnPattern.CenterSpawn:
                for (int i = 0; i < count; i++)
                    positions[i] = playerPos + directionHint * radius;
                break;
        }
    }
}
