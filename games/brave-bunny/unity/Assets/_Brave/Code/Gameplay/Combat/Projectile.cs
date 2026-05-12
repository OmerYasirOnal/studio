#nullable enable
// Tech-spec 05 § Collision. Projectile uses pooled lifecycle (IPoolable). No per-frame allocs.

using System;

using UnityEngine;

using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Pooled projectile. Moves in a straight line, hits the first enemy within hit-radius,
    /// applies damage via the direct-method <see cref="EnemyHealth.TakeHit"/> path, and returns
    /// itself to the pool. Zero allocations per fire.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speedUnitsPerSecond = 12f;
        [SerializeField] private float hitRadius = 0.4f;
        [SerializeField] private float lifetimeSeconds = 2.0f;

        private Vector2 _direction;
        private float _damage;
        private float _ageSeconds;
        private bool _alive;
        private AutoAttackController? _owner;
        private ObjectPool<Projectile>? _pool;
        private object? _genericPoolOwner;
        private int _sourceWeaponId;
        private int _pierceRemaining;

        // Surface expected by ProjectileWeapon + HitDetector (parallel-agent API drift).
        public int SourceWeaponId => _sourceWeaponId;
        public float Damage => _damage;
        public int PierceRemaining => _pierceRemaining;

        public void BindPool(ObjectPool<Projectile> pool) => _pool = pool;

        /// <summary>Configures a freshly-acquired projectile (per ProjectileWeapon fire path).</summary>
        public void Configure(object pool, float damage, int sourceWeaponId, int pierce, float speed, float lifetime)
        {
            _genericPoolOwner = pool;
            _damage = damage;
            _sourceWeaponId = sourceWeaponId;
            _pierceRemaining = pierce;
            speedUnitsPerSecond = speed;
            lifetimeSeconds = lifetime;
            _ageSeconds = 0f;
            _alive = true;
        }

        /// <summary>Sets the travel direction (called by the pool after spawn-place positioning).</summary>
        public void SetDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
            _direction = dir.normalized;
        }

        /// <summary>Decrements pierce and despawns when exhausted. Called per-hit by HitDetector.</summary>
        public void NotifyHit()
        {
            _pierceRemaining--;
            if (_pierceRemaining <= 0) Despawn();
        }

        /// <summary>Spawn-time setup. <paramref name="spreadIndex"/> is used to fan a salvo.</summary>
        public void Launch(Vector3 from, Vector3 toward, float damage,
            AutoAttackController owner, int spreadIndex, int totalProjectiles)
        {
            transform.position = from;
            Vector2 dir = (Vector2)(toward - from);
            if (dir.sqrMagnitude < 1e-6f) dir = Vector2.right;
            _direction = dir.normalized;

            // Apply a small angular spread if salvo > 1
            if (totalProjectiles > 1)
            {
                float spreadDeg = 12f * (spreadIndex - (totalProjectiles - 1) * 0.5f);
                float rad = spreadDeg * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
                _direction = new Vector2(
                    _direction.x * cos - _direction.y * sin,
                    _direction.x * sin + _direction.y * cos);
            }

            _damage = damage;
            _ageSeconds = 0f;
            _alive = true;
            _owner = owner;
        }

        private void Update()
        {
            if (!_alive) return;

            float dt = Time.deltaTime;
            _ageSeconds += dt;
            if (_ageSeconds >= lifetimeSeconds) { Despawn(); return; }

            Vector3 pos = transform.position;
            pos.x += _direction.x * speedUnitsPerSecond * dt;
            pos.y += _direction.y * speedUnitsPerSecond * dt;
            transform.position = pos;

            var hit = EnemyRegistry.FindFirstWithinRadius(pos, hitRadius);
            if (hit != null)
            {
                var info = new HitInfo(_damage, pos, isCrit: false, sourceId: 0,
                    targetId: hit.GetInstanceID());
                hit.Health.TakeHit(info);
                Despawn();
            }
        }

        private void Despawn()
        {
            _alive = false;
            _pool?.Return(this);
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            _alive = false;
            _owner = null;
        }
    }
}
