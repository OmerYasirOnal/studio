#nullable enable
// Brave Bunny — Gameplay / MVP / MvpSwarmer
//
// Wave 13 vertical-slice enemy. Minimal swarmer that homes toward a target
// transform at a fixed move-speed, registers in EnemyRegistry so the canonical
// Projectile / AutoAttack target-acquire path can find it, and dies when its
// EnemyHealth hits 0. Used by MvpWaveSpawner to populate the Run scene with
// something to fight in advance of the full EnemyDefinition + pooling stack
// getting prefab-wired.
//
// Why subclass EnemyBase rather than roll a standalone MonoBehaviour: the
// existing Projectile.Update queries EnemyRegistry → only EnemyBase-typed
// objects are found, and DamageApplier.TryApply expects EnemyHealth. Reusing
// the canonical chain keeps Wave-13 spawners compatible with the real weapon
// loop without forking it.
//
// Allocation discipline: TickBehavior is invoked from EnemyTicker at 30 Hz
// (Tech-spec 05) but in MVP we drive movement from Update for simplicity
// since the EnemyTicker is not wired in the Run scene. Movement is sqrt-once.
//
// Cross-refs:
//   * Editor/SceneSetup.cs — EnsurePlayableMvpRun() spawns these via MvpWaveSpawner
//   * Brave.Gameplay.Enemies.EnemyBase, EnemyRegistry — base class + reg
//   * docs/02-gdd/01-core-loop.md — auto-attack vs swarmer baseline TTK

using UnityEngine;

using Brave.Gameplay.Damage;
using Brave.Gameplay.Enemies;

namespace Brave.Gameplay.MVP
{
    /// <summary>
    /// Vertical-slice swarmer for Wave 13. Concrete <see cref="EnemyBase"/>
    /// subclass that homes toward the assigned hero transform every Update
    /// (EnemyTicker is not wired in the MVP Run scene; Update is the
    /// fallback drive).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class MvpSwarmer : EnemyBase, IDeathListener
    {
        [SerializeField] private float fallbackMoveSpeed = 2.5f;
        [SerializeField] private float fallbackHp = 10f;

        /// <summary>Lightweight runtime configuration when no EnemyDefinition is wired.</summary>
        public void ConfigureMvp(Transform heroTransform, float moveSpeedUnitsPerSec, float hp)
        {
            Hero = heroTransform;
            ScaledMoveSpeed = moveSpeedUnitsPerSec;
            ScaledHp = hp;
            ScaledContactDamage = 0f;       // MVP: no contact damage yet (player invuln)
            var hc = Health;
            if (hc != null)
            {
                hc.Reset(hp);
                hc.RegisterDeathListener(this);
            }
            // Re-register in EnemyRegistry: MvpWaveSpawner spawns via Instantiate, not via
            // the pool, so OnGetFromPool never fires. Direct call ensures projectiles can hit.
            EnemyRegistry.Register(this);
        }

        // IDeathListener — when projectiles drive HP to 0, EnemyHealth.Die() notifies us;
        // we deactivate the GameObject (MVP has no pool, no drops yet) and unregister.
        public void OnEnemyDied(EnemyBase enemy, in HitInfo finalHit)
        {
            EnemyRegistry.Unregister(this);
            gameObject.SetActive(false);
        }

        public override void TickBehavior(float dt)
        {
            // Optional EnemyTicker integration path — duplicates Update logic for the
            // canonical pool-driven 30 Hz tick. Idempotent when called from both paths
            // because position is set absolutely.
            StepTowardHero(dt);
        }

        private void Update()
        {
            // EnemyTicker is not wired in the MVP Run scene — drive from Update.
            // Once the canonical EnemyTicker comes online, the ticker calls TickBehavior
            // and this Update can be guarded by a flag (or this MVP class deleted).
            if (Hero == null) return;
            StepTowardHero(Time.deltaTime);
        }

        private void StepTowardHero(float dt)
        {
            if (Hero == null) return;
            Vector3 pos = transform.position;
            Vector3 dir = Hero.position - pos;
            dir.y = 0f;                                          // XZ ground plane (ADR-0018)
            float sq = dir.sqrMagnitude;
            if (sq < 0.0001f) return;

            float speed = ScaledMoveSpeed > 0f ? ScaledMoveSpeed : fallbackMoveSpeed;
            float step = speed * dt;
            float invMag = 1f / Mathf.Sqrt(sq);
            pos.x += dir.x * invMag * step;
            pos.z += dir.z * invMag * step;
            transform.position = pos;
        }

        private void OnDisable()
        {
            // Mirrors EnemyBase.OnReturnToPool — keeps the registry consistent when the
            // swarmer is killed (gameObject set inactive via the death-listener chain).
            EnemyRegistry.Unregister(this);
        }
    }
}
