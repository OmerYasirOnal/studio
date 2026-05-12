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
        protected WeaponDefinition Definition = default!;
        protected int Level;
        protected float Cooldown;     // seconds remaining until next fire
        protected Transform? _ownerTransform;
        protected float _runSeconds;

        protected Weapon() { }

        protected Weapon(WeaponDefinition definition, int level)
        {
            Definition = definition;
            Level = Mathf.Clamp(level, 1, 5);
            Cooldown = 0f;
        }

        public WeaponDefinition Def => Definition;
        public int CurrentLevel => Level;
        public WeaponLevelData CurrentLevelData => Definition.levels[Level - 1];

        /// <summary>Late initialisation for weapons created via Unity serialisation
        /// (where the parameterless constructor is required).</summary>
        public virtual void Initialise(WeaponDefinition def, Transform owner, int level = 1)
        {
            Definition = def;
            _ownerTransform = owner;
            Level = Mathf.Clamp(level, 1, 5);
            Cooldown = 0f;
        }

        public virtual void OnEquip(AutoAttackController owner)
        {
            _ownerTransform = owner != null ? owner.transform : null;
        }

        public virtual void OnUnequip(AutoAttackController owner) { }

        /// <summary>Called every frame by the AutoAttackController.</summary>
        public void Tick(AutoAttackController owner, float dt)
        {
            _runSeconds += dt;
            Cooldown -= dt;
            if (Cooldown > 0f) return;

            OnFire(_runSeconds);
            Cooldown = CurrentLevelData.fireRate;
        }

        /// <summary>Level-up upgrade. Caller is the draft system.</summary>
        public void LevelUp() => Level = Mathf.Min(5, Level + 1);

        /// <summary>Concrete subclasses implement firing logic. <paramref name="runSeconds"/>
        /// is the elapsed run time for scheduling-aware behaviours.</summary>
        protected abstract void OnFire(float runSeconds);
    }

    // Concrete weapon classes live in their own files:
    //   ProjectileWeapon.cs, AuraWeapon.cs, AreaWeapon.cs
}
