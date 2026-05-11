// Concrete weapon for Area archetype — e.g. Daisy Mine, Cob Mortar.
using Brave.Gameplay.Pooling;
using UnityEngine;

namespace Brave.Gameplay.Combat;

public sealed class AreaWeapon : Weapon
{
    [SerializeField] private VfxPool _impactVfx;
    [SerializeField] private float _armingSeconds = 1.0f;  // daisy-mine baseline
    [SerializeField] private float _splashRadius = 1.5f;

    protected override void OnFire(float runSeconds)
    {
        // TODO(Phase 5): spawn mine at random position within CurrentLevelData.range, arm, then resolve splash.
        throw new System.NotImplementedException("AreaWeapon.OnFire — Phase 5 implementation pending");
    }
}
