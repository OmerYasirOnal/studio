#nullable enable
// Wave 10 — Character active/passive abilities.
//
// Abstract base for the 8 launch characters' unique passive abilities. Each
// concrete subclass owns one ability id (e.g. "hop", "shell") and is tagged with
// [BraveRegister(<id>)] so the registry round-trips it at boot — see ADR-0009.
//
// Distinct from Brave.Gameplay.Combat.SignatureMechanic: the SignatureMechanic
// path covers the heavy designer-facing "signature" mechanics (HopDodge, ShellShield,
// FoxExec, etc., each with multiple JSON-balanced sub-fields). CharacterAbility is
// the lightweight per-character permanent perk layer — a single magnitude per
// ability, sourced from <c>characters.json:ability.*</c>. Both can coexist on a
// character (Bunny has the HopDodge i-frame mechanic AND the Hop +10% move-speed
// passive ability).
//
// Lifecycle:
//   * OnActivate(ctx)    — once at run start (or when ability is granted mid-run).
//   * OnTick(dt)         — per-frame; cheap defaults for stateless passives.
//   * OnDeactivate()     — once at run end / character swap.
//
// Cross-refs:
//   * docs/06-tech-spec/02-data-model.md § Polymorphic mechanics
//   * docs/02-gdd/03-characters.md
//   * ADR-0009 (BraveRegister token-registry)

using Brave.Gameplay.Combat;

namespace Brave.Gameplay.Characters
{
    /// <summary>
    /// Per-character passive ability. Concrete subclasses carry one
    /// <see cref="BraveRegisterAttribute"/> matching the ability id in
    /// <c>data/balance/characters.json</c> (e.g. "hop", "shell").
    /// </summary>
    public abstract class CharacterAbility
    {
        /// <summary>Stable id token (e.g. "hop"). Matches characters.json ability_id.</summary>
        public abstract string AbilityId { get; }

        /// <summary>Cached run context — set by <see cref="OnActivate"/>, cleared in <see cref="OnDeactivate"/>.</summary>
        protected IRunContext? Context;

        /// <summary>True between Activate and Deactivate.</summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Called once when the ability becomes active for a run. Concrete subclasses
        /// override to stamp stat modifiers, subscribe to events, or seed internal state.
        /// </summary>
        public virtual void OnActivate(IRunContext ctx)
        {
            Context = ctx;
            IsActive = true;
        }

        /// <summary>Per-frame tick. Default is no-op — most abilities are stat passives.</summary>
        public virtual void OnTick(float dt) { }

        /// <summary>Called once when the ability is removed (run end or character swap).</summary>
        public virtual void OnDeactivate()
        {
            IsActive = false;
            Context = null;
        }
    }
}
