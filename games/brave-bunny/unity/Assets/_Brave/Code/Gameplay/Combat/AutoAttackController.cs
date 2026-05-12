#nullable enable
// GDD 01 § Auto-attack details. No fire button; ticks cooldown continuously.
// Targeting priority: nearest in range, front-arc preference, low-HP tie-break.
//
// Wave 4 (vertical slice — Carrot Boomerang) adds a direct cast loop driven by a single
// serialized WeaponDefinition + CarrotProjectilePool. This sits alongside the polymorphic
// `_equipped` Weapon list (which remains the long-term API once Mechanics-Registry is wired
// per ADR-0009 / tech-spec 09). The two paths are complementary: vertical slice uses the
// direct path; full-roster runs will use the polymorphic path.

using System;
using System.Collections.Generic;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Movement;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Drives auto-firing of every equipped weapon. Maintains per-weapon cooldown state and
    /// resolves targeting once per fire (NOT per frame). One controller per hero.
    /// </summary>
    public sealed class AutoAttackController : MonoBehaviour
    {
        // ADR-0018: references the canonical XZ PlayerMover. AutoAttack reads
        // PlayerMover.Facing for the direct-cast firing direction; targeting (full system)
        // uses this controller's own transform.position via EnemyRegistry queries.
        [SerializeField] private PlayerMover? player;

        // Wave 4 direct-cast slice. Optional — when both fields are non-null AND fireRate > 0,
        // the controller's Update fires the weapon every `fireRate` seconds (GDD 04 RATE
        // contract: seconds-between-fires). The polymorphic `_equipped` system below still
        // ticks independently and is the long-term API.
        [SerializeField] private WeaponDefinition? weapon;
        [SerializeField] private CarrotProjectilePool? projectilePool;

        // Pre-allocated lists — never resized in hot loop.
        private readonly List<Weapon> _equipped = new(capacity: 8);
        private readonly List<EnemyBase> _targetScratch = new(capacity: 32);
        private float _runSeconds;

        // Direct-cast state. _directCastEnabled latches the Awake-time precondition check
        // so the hot Update loop has zero null-branching.
        private bool _directCastEnabled;
        private float _directCooldown;
        private float _cachedFireRateSeconds;
        private float _cachedDamage;
        private float _cachedProjectileSpeed;
        private float _cachedProjectileLifetime;

        public IReadOnlyList<Weapon> Equipped => _equipped;

        /// <summary>True after Awake when the serialized weapon + pool + player are wired.</summary>
        public bool DirectCastEnabled => _directCastEnabled;

        /// <summary>Seconds remaining until the next direct cast. 0 when ready to fire.</summary>
        public float DirectCooldown => _directCooldown;

        /// <summary>Equips a weapon. Called from Loadout (initial) and draft-pick (on level-up).</summary>
        public void Equip(Weapon weapon)
        {
            if (_equipped.Count >= 8) return;       // hard cap matches loadout slot count
            weapon.OnEquip(this);
            _equipped.Add(weapon);
        }

        /// <summary>Unequips and releases. Used when a weapon evolves and the base is consumed.</summary>
        public void Unequip(Weapon weapon)
        {
            if (_equipped.Remove(weapon))
                weapon.OnUnequip(this);
        }

        private void Awake()
        {
            // Defensive precondition gate matching PlayerMover's pattern. The direct-cast
            // path is OPTIONAL — when not wired we silently skip; the polymorphic _equipped
            // path still runs. This means presence of the polymorphic path alone is valid.
            _directCastEnabled = false;
            if (weapon == null) return;
            if (projectilePool == null) return;
            if (player == null)
            {
                Debug.LogError(
                    $"{nameof(AutoAttackController)}: '{weapon.slug}' has weapon+pool but no PlayerMover — disabling direct cast.",
                    this);
                return;
            }
            if (weapon.levels == null || weapon.levels.Length == 0)
            {
                Debug.LogError(
                    $"{nameof(AutoAttackController)}: '{weapon.slug}' has no level data — disabling direct cast.",
                    this);
                return;
            }

            WeaponLevelData l1 = weapon.levels[0];
            if (l1.fireRate <= 0f)
            {
                Debug.LogError(
                    $"{nameof(AutoAttackController)}: '{weapon.slug}'.levels[0].fireRate is {l1.fireRate} — " +
                    "balance JSON not imported into WeaponDefinition. Run 'Brave > Generate Balance SOs from JSON'.",
                    this);
                return;
            }

            _cachedFireRateSeconds = l1.fireRate;            // GDD 04: RATE = seconds-between-fires
            _cachedDamage = l1.damage;
            _cachedProjectileSpeed = ComputeProjectileSpeedFromRange(l1.range, l1.fireRate);
            _cachedProjectileLifetime = ComputeProjectileLifetimeFromRange(l1.range, _cachedProjectileSpeed);
            _directCooldown = 0f;
            _directCastEnabled = true;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _runSeconds += dt;

            // Polymorphic Weapon list (long-term API).
            for (int i = 0, n = _equipped.Count; i < n; i++)
                _equipped[i].Tick(this, dt);

            // Direct-cast vertical-slice path.
            if (_directCastEnabled)
                TickDirectCast(dt);
        }

        private void TickDirectCast(float dt)
        {
            _directCooldown -= dt;
            if (_directCooldown > 0f) return;

            CastDirect();
            _directCooldown = _cachedFireRateSeconds;
        }

        private void CastDirect()
        {
            // Carrot Boomerang v0.1: linear shoot in player's facing direction.
            // Boomerang-return behaviour is a follow-up (see hand-off).
            Vector3 origin = transform.position;
            Vector3 facing = player!.Facing;
            if (facing.sqrMagnitude < ProjectileMath.FacingEpsilonSqr)
                facing = Vector3.right;

            projectilePool!.Spawn(
                origin,
                facing.normalized,
                _cachedProjectileSpeed,
                _cachedDamage,
                _cachedProjectileLifetime);
        }

        // The weapon JSON exposes RANGE (units) and RATE (s/fire) but not raw projectile
        // speed/lifetime — the projectile's travel-time is derived from range so the visual
        // arrives at the targeted enemy boundary within a sensible fraction of the rate.
        // Pure helper, exposed for tests.
        public static float ComputeProjectileSpeedFromRange(float rangeUnits, float fireRateSeconds)
        {
            // Travel range in a fraction of the fire-rate window — keeps projectiles on screen
            // long enough to be readable but well before the next cast lands. fraction = 1/2.
            float fraction = ProjectileMath.RangeTravelFractionOfRate;
            float travelSeconds = Mathf.Max(fireRateSeconds * fraction, ProjectileMath.MinTravelSeconds);
            return rangeUnits / travelSeconds;
        }

        public static float ComputeProjectileLifetimeFromRange(float rangeUnits, float projectileSpeed)
        {
            if (projectileSpeed <= 0f) return ProjectileMath.MinTravelSeconds;
            return rangeUnits / projectileSpeed;
        }

        /// <summary>Pure helper: integrate one frame of the cooldown timer. Returns the new
        /// cooldown value and emits the number of casts that fired during <paramref name="dt"/>.
        /// <para>Contract: when the cooldown crosses zero, exactly one cast fires and the
        /// cooldown resets to <paramref name="fireRateSeconds"/>. Multiple casts can fire in
        /// a single tick if <paramref name="dt"/> ≥ <paramref name="fireRateSeconds"/> (e.g.
        /// after a long pause); the helper handles that case correctly.</para></summary>
        public static float TickCooldown(float cooldown, float dt, float fireRateSeconds, out int castsFired)
        {
            castsFired = 0;
            if (fireRateSeconds <= 0f) return cooldown;     // defensive: caller's gate
            cooldown -= dt;
            while (cooldown <= 0f)
            {
                castsFired++;
                cooldown += fireRateSeconds;
                // Bail if fireRate is bizarre (e.g. infinite casts on a zero-rate config).
                if (castsFired > ProjectileMath.MaxCastsPerTick) break;
            }
            return cooldown;
        }

        /// <summary>Pure targeting helper. Returns null when no in-range target exists.</summary>
        public EnemyBase? AcquireTarget(float rangeUnits, TargetingMode mode, Vector2 facing)
        {
            EnemyRegistry.SnapshotActiveInRange(transform.position, rangeUnits, _targetScratch);
            if (_targetScratch.Count == 0) return null;

            EnemyBase? best = null;
            float bestScore = float.PositiveInfinity;
            Vector2 origin = transform.position;

            for (int i = 0, n = _targetScratch.Count; i < n; i++)
            {
                var e = _targetScratch[i];
                Vector2 to = (Vector2)e.transform.position - origin;
                float dist = to.magnitude;
                if (dist > rangeUnits) continue;

                // Score: nearest first; front-arc preference within ±60° subtracts a small bonus.
                float score = dist;
                if (Vector2.Dot(facing, to.normalized) > 0.5f) score -= 0.5f;

                if (score < bestScore) { bestScore = score; best = e; }
            }

            return best;
        }

        public float RunSeconds => _runSeconds;
    }
}
