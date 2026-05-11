using Brave.Gameplay.Definitions;

namespace Brave.Gameplay.Combat;

/// <summary>
/// Polymorphic surface for every wielded weapon. Concrete implementations live in
/// <see cref="ProjectileWeapon"/>, <see cref="AreaWeapon"/>, <see cref="AuraWeapon"/>, etc.
/// </summary>
public interface IWeapon
{
    WeaponDefinition Definition { get; }
    int Level { get; }                    // 1..5 (balance/00-formulas.md § 11 clamp)
    void Fire(float runSeconds);          // called by FireScheduler when rate-window elapses
    void LevelUp();                       // bumps level, recomputes cached stats
}
