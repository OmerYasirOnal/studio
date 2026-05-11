#nullable enable
// Tech-spec 05 § Spawning + AI: AI ticked at 30 Hz (half on even frames, half on odd).
// EnemyBase is a thin MonoBehaviour; the heavy lifting lives in Burst-job ticks driven
// by the wave driver. Stats come from EnemyDefinition + biome ScalingCurve.

using System;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Enemies
{
    /// <summary>
    /// Base MonoBehaviour for every enemy variant. Holds the typed reference to
    /// <see cref="EnemyDefinition"/>, the per-instance scaled stats, and a hook into the
    /// shared <see cref="EnemyHealth"/>. Concrete subclasses (Swarmer, Tank, Ranged) override
    /// <see cref="TickBehavior(float)"/> with their movement / attack logic.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class EnemyBase : MonoBehaviour, IPoolable
    {
        [SerializeField] private EnemyDefinition? definition;
        [SerializeField] private EnemyHealth? health;

        protected Transform? Hero;
        protected float ScaledHp;
        protected float ScaledContactDamage;
        protected float ScaledMoveSpeed;

        public EnemyDefinition? Definition => definition;
        public EnemyHealth Health => health!;        // populated in Awake

        private void Awake()
        {
            if (health == null) health = GetComponent<EnemyHealth>();
        }

        /// <summary>Configure on dequeue from the pool. Stats are scaled per minute via the biome curve.</summary>
        public void Configure(EnemyDefinition def, Transform hero, float scaledHp,
            float scaledContactDamage, float scaledMoveSpeed)
        {
            definition = def;
            Hero = hero;
            ScaledHp = scaledHp;
            ScaledContactDamage = scaledContactDamage;
            ScaledMoveSpeed = scaledMoveSpeed;
            if (health != null) health.Reset(scaledHp);
        }

        /// <summary>Called by the AI driver at 30 Hz (not Update — see tech-spec 05).</summary>
        public abstract void TickBehavior(float dt);

        /// <summary>Apply contact damage to the hero on overlap. Called by the collision broadphase.</summary>
        public float ContactDamageThisTick(float dt) => ScaledContactDamage * dt;

        public virtual void OnGetFromPool()
        {
            gameObject.SetActive(true);
            EnemyRegistry.Register(this);
        }

        public virtual void OnReturnToPool()
        {
            EnemyRegistry.Unregister(this);
            gameObject.SetActive(false);
            Hero = null;
        }
    }
}
