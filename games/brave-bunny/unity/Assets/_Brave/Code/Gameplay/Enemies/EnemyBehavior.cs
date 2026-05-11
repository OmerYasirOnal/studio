using UnityEngine;

namespace Brave.Gameplay.Enemies;

/// <summary>
/// Strategy contract for enemy AI. One instance per archetype (Swarmer/Tank/Ranged/Elite)
/// — stateless across enemies; shared instance per archetype keeps memory low.
/// </summary>
public abstract class EnemyBehavior
{
    public abstract void Tick(Enemy enemy, Vector2 playerPos, float dt);
}
