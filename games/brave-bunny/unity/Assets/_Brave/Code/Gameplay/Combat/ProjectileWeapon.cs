// Concrete weapon for Projectile archetype — e.g. Carrot Boomerang, Pebble Sling.
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Combat;

public sealed class ProjectileWeapon : Weapon
{
    [SerializeField] private ProjectilePool _projectilePool;
    [SerializeField] private float _projectileSpeed = 8f;
    [SerializeField] private float _projectileLifetime = 3f;
    [SerializeField] private int _basePierce = 0;

    public void BindPool(ProjectilePool pool) => _projectilePool = pool;

    protected override void OnFire(float runSeconds)
    {
        if (_projectilePool == null || _ownerTransform == null) return;

        var data = CurrentLevelData;
        int count = Mathf.Max(1, data.projectiles);
        Vector2 origin = _ownerTransform.position;

        // TODO(Phase 5): targeting (Nearest/Furthest) — for now fire straight up as placeholder.
        Vector2 baseDir = Vector2.up;

        for (int i = 0; i < count; i++)
        {
            float spread = (count > 1) ? (i - (count - 1) * 0.5f) * 8f : 0f;
            Vector2 dir = Rotate(baseDir, spread);
            var p = _projectilePool.Acquire(origin, dir);
            int sourceId = Definition != null ? Definition.GetInstanceID() : 0;
            p.Configure(_projectilePool, data.damage, sourceId, _basePierce, _projectileSpeed, _projectileLifetime);
        }
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
