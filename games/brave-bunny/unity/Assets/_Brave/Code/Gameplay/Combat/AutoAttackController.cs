#nullable enable
// GDD 01 § Auto-attack details. No fire button; ticks cooldown continuously.
// Targeting priority: nearest in range, front-arc preference, low-HP tie-break.

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
        // ADR-0018: references the canonical XZ PlayerMover. The field is currently
        // unread by AutoAttack logic — targeting uses this controller's own transform.position.
        // When AutoAttack needs the player's facing direction for front-arc preference
        // (today seeded from a hard-coded Vector2.right at the call-site), it can read
        // PlayerMover.Facing through this reference.
        [SerializeField] private PlayerMover? player;

        // Pre-allocated lists — never resized in hot loop.
        private readonly List<Weapon> _equipped = new(capacity: 8);
        private readonly List<EnemyBase> _targetScratch = new(capacity: 32);
        private float _runSeconds;

        public IReadOnlyList<Weapon> Equipped => _equipped;

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

        private void Update()
        {
            float dt = Time.deltaTime;
            _runSeconds += dt;

            for (int i = 0, n = _equipped.Count; i < n; i++)
                _equipped[i].Tick(this, dt);
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
