// Concrete weapon for Aura archetype — e.g. Honey Aura, Frost Whisper.
// Aura ticks damage to all enemies inside CurrentLevelData.range every fireRate seconds.
using UnityEngine;

namespace Brave.Gameplay.Combat;

public sealed class AuraWeapon : Weapon
{
    [SerializeField] private ParticleSystem _auraVfx;

    public override void Initialise(BraveBunny.Gameplay.Data.WeaponDefinition def, Transform owner, int level = 1)
    {
        base.Initialise(def, owner, level);
        if (_auraVfx != null) _auraVfx.Play();
    }

    protected override void OnFire(float runSeconds)
    {
        // TODO(Phase 5): broadphase query against HitDetector for enemies inside radius,
        // apply CurrentLevelData.damage, optionally apply slow (frost-whisper, honey-aura L4).
        throw new System.NotImplementedException("AuraWeapon.OnFire — Phase 5 implementation pending");
    }
}
