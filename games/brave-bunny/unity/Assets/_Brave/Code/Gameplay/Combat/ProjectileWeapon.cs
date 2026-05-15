#nullable enable
// Concrete weapon for Projectile archetype — e.g. Carrot Boomerang, Pebble Sling.
// ADR-0019 follow-up: targeting mode (Nearest / Furthest / Random / LowestHP) is
// resolved via TargetSelector instead of the prior straight-up placeholder.
using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    public sealed class ProjectileWeapon : Weapon
    {
        [SerializeField] private ProjectilePool? _projectilePool;
        [SerializeField] private float _projectileSpeed = 8f;
        [SerializeField] private float _projectileLifetime = 3f;
        [SerializeField] private int _basePierce = 0;

        // Pre-allocated scratch buffer — reused every fire; never resized in hot loop.
        private readonly List<EnemyBase> _targetScratch = new(capacity: 32);

        public void BindPool(ProjectilePool pool) => _projectilePool = pool;

        protected override void OnFire(float runSeconds)
        {
            if (_projectilePool == null || _ownerTransform == null) return;

            var data = CurrentLevelData;
            int count = Mathf.Max(1, data.projectiles);
            Vector3 origin3 = _ownerTransform.position;
            Vector2 origin = _ownerTransform.position;      // legacy spawn-coord (pre-cleanup)

            // Resolve targeting via TargetSelector strategy table. WeaponDefinition.targeting
            // is the data-driven mode; non-targeting placement modes fall through to Nearest.
            TargetingMode mode = Definition != null ? Definition.targeting : TargetingMode.Nearest;
            Vector2 baseDir = AcquireFireDirection(origin3, data.range, mode);

            for (int i = 0; i < count; i++)
            {
                float spread = (count > 1) ? (i - (count - 1) * 0.5f) * 8f : 0f;
                Vector2 dir = Rotate(baseDir, spread);
                var p = _projectilePool!.Acquire(origin, dir);
                int sourceId = Definition != null ? Definition.GetInstanceID() : 0;
                p.Configure(_projectilePool, data.damage, sourceId, _basePierce, _projectileSpeed, _projectileLifetime);
            }
        }

        /// <summary>Resolves the fire direction from the targeting mode. When no enemy is
        /// in range, falls back to <see cref="Vector2.up"/> so the weapon still emits a
        /// readable projectile (matches the pre-cleanup behaviour).</summary>
        private Vector2 AcquireFireDirection(Vector3 origin3, float range, TargetingMode mode)
        {
            EnemyRegistry.SnapshotActiveInRange(origin3, range, _targetScratch);
            if (_targetScratch.Count == 0) return Vector2.up;

            var strategy = TargetSelector.FromTargetingMode(mode);
            EnemyBase? target = TargetSelector.Select(origin3, _targetScratch, strategy);
            if (target == null) return Vector2.up;

            // XZ direction (ADR-0018): map world.x → dir.x and world.z → dir.y so the
            // resulting Vector2 matches PlayerMover.Facing's convention.
            Vector3 d = target.transform.position - origin3;
            Vector2 dir = new(d.x, d.z);
            float sqr = dir.sqrMagnitude;
            if (sqr < 1e-6f) return Vector2.up;
            return dir / Mathf.Sqrt(sqr);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad), s = Mathf.Sin(rad);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
    }
}
