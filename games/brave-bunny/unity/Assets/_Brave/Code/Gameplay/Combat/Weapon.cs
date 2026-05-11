#nullable enable
// GDD 01 § Auto-attack + tech-spec 02 § WeaponArchetype.
// Concrete subclasses (Projectile, Aura, Area) implement Fire(). Cooldown is continuous.

using System;

using UnityEngine;

using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Combat
{
    /// <summary>
    /// Abstract per-weapon runtime instance. Wraps a <see cref="WeaponDefinition"/> at a
    /// specific level (1..5). Stateful only in the cooldown timer; no MonoBehaviour
    /// per weapon — the AutoAttackController owns the Update tick.
    /// </summary>
    public abstract class Weapon
    {
        protected readonly WeaponDefinition Definition;
        protected int Level;
        protected float Cooldown;     // seconds remaining until next fire

        protected Weapon(WeaponDefinition definition, int level)
        {
            Definition = definition;
            Level = Mathf.Clamp(level, 1, 5);
            Cooldown = 0f;
        }

        public WeaponDefinition Def => Definition;
        public int CurrentLevel => Level;
        public WeaponLevelData CurrentLevelData => Definition.levels[Level - 1];

        public virtual void OnEquip(AutoAttackController owner) { }
        public virtual void OnUnequip(AutoAttackController owner) { }

        /// <summary>Called every frame by the AutoAttackController.</summary>
        public void Tick(AutoAttackController owner, float dt)
        {
            Cooldown -= dt;
            if (Cooldown > 0f) return;

            Fire(owner);
            Cooldown = CurrentLevelData.fireRate;
        }

        /// <summary>Level-up upgrade. Caller is the draft system.</summary>
        public void LevelUp() => Level = Mathf.Min(5, Level + 1);

        protected abstract void Fire(AutoAttackController owner);
    }

    /// <summary>Projectile archetype — spawns N projectiles per fire, auto-targeting nearest.</summary>
    public sealed class ProjectileWeapon : Weapon
    {
        private readonly ObjectPool<Projectile> _projectilePool;

        public ProjectileWeapon(WeaponDefinition def, int level, ObjectPool<Projectile> pool)
            : base(def, level)
        {
            _projectilePool = pool;
        }

        protected override void Fire(AutoAttackController owner)
        {
            var data = CurrentLevelData;
            var target = owner.AcquireTarget(data.range, Definition.targeting,
                owner.transform.right);
            if (target == null) return;

            for (int i = 0; i < data.projectiles; i++)
            {
                var p = _projectilePool.Get();
                p.Launch(owner.transform.position, target.transform.position, data.damage,
                    owner: owner, spreadIndex: i, totalProjectiles: data.projectiles);
            }
        }
    }

    /// <summary>Aura archetype — radial tick around the hero; no projectile spawn.</summary>
    public sealed class AuraWeapon : Weapon
    {
        public AuraWeapon(WeaponDefinition def, int level) : base(def, level) { }

        protected override void Fire(AutoAttackController owner)
        {
            // Tick all enemies within radius; damage resolves via DamageCalculator at the call site.
            // Implementation deferred to systems-engineer's CombatResolver direct-method path.
        }
    }

    /// <summary>Area archetype — bombs/AOE bursts at a target location.</summary>
    public sealed class AreaWeapon : Weapon
    {
        public AreaWeapon(WeaponDefinition def, int level) : base(def, level) { }

        protected override void Fire(AutoAttackController owner)
        {
            var data = CurrentLevelData;
            var target = owner.AcquireTarget(data.range, Definition.targeting,
                owner.transform.right);
            if (target == null) return;

            // Spawn area-burst VFX + damage tick over duration. Stub.
        }
    }
}
