// Concrete weapon for Aura archetype — e.g. Honey Aura, Frost Whisper.
// Aura ticks damage to all enemies inside CurrentLevelData.range every fireRate seconds.
//
// Wave 10: on each fire tick, the aura now applies a SlowEffect to enemies inside its
// radius. The slow magnitude (and tick lifetime, when AuraArchetypeConfig.tickLifetimeMs
// is non-zero) is pulled from the archetype-config sidecar — never inlined here. Damage
// path is unchanged (DamageApplier ownership stays with the crit agent).
using Brave.Gameplay.Combat.Archetypes;
using Brave.Gameplay.Combat.StatusEffects;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Enemies;
using UnityEngine;

namespace Brave.Gameplay.Combat;

public sealed class AuraWeapon : Weapon
{
    [SerializeField] private ParticleSystem _auraVfx;

    // Default slow lifetime when AuraArchetypeConfig.tickLifetimeMs is 0
    // (Frost Whisper L1 baseline — extracted to ADR-0020 in a future wave).
    private const int DefaultSlowDurationMs = 1000;

    // Service handle; injected by RunController. Lazily resolved (null-safe) so
    // unit tests that don't wire the applier don't crash.
    public static StatusEffectApplier? Applier { get; set; }

    public override void Initialise(WeaponDefinition def, Transform owner, int level = 1)
    {
        base.Initialise(def, owner, level);
        if (_auraVfx != null) _auraVfx.Play();
    }

    protected override void OnFire(float runSeconds)
    {
        // TODO(Phase 5): broadphase query against HitDetector for enemies inside radius,
        // apply CurrentLevelData.damage to each. The damage path is OWNED by the crit
        // agent (DamageApplier / HitResult schema) — do not modify it here. This stub
        // is the integration point where the broadphase result fans out into
        // ApplySlowTo(...) per hit enemy.
    }

    /// <summary>Apply this aura's slow to <paramref name="target"/>. Called per hit by the
    /// broadphase result (TODO above). Public so the future broadphase tick can fan out.</summary>
    public void ApplySlowTo(Enemy target)
    {
        if (target == null || Applier == null) return;

        // Pull magnitude + lifetime from archetype config (ADR-0020 §Decision).
        // Never inline magic numbers — the JSON-importer wires slow_pct_base /
        // tick_lifetime_ms onto AuraArchetypeConfig.
        var cfg = Definition.archetypeConfig as AuraArchetypeConfig;
        if (cfg == null) return;

        int levelIdx = Mathf.Clamp(Level - 1, 0, AuraArchetypeConfig.LevelCount - 1);
        float slowPct = cfg.slowPctPerLevel != null && cfg.slowPctPerLevel.Length > levelIdx
            ? cfg.slowPctPerLevel[levelIdx]
            : cfg.slowPctBase;

        int durMs = cfg.tickLifetimeMsPerLevel != null && cfg.tickLifetimeMsPerLevel.Length > levelIdx
            ? cfg.tickLifetimeMsPerLevel[levelIdx]
            : cfg.tickLifetimeMs;
        if (durMs <= 0) durMs = cfg.tickLifetimeMs > 0 ? cfg.tickLifetimeMs : DefaultSlowDurationMs;

        Applier.Apply(target, new SlowEffect(durationMs: durMs, magnitude: slowPct));
    }
}
