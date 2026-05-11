# ADR 0009 — Polymorphic mechanics via type-name registry

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing tech-architect wave-4 flag)

## Context

`docs/06-tech-spec/02-data-model.md` defined polymorphic data shapes:

- `SignatureMechanic` — one concrete subclass per character (BunnyHopDodge, TortoiseShellShield, FoxExec, ...)
- `BossAttackPattern` — one per boss attack (BoarCharge, BoarSweep, BoarHopStomp, ...)
- `AchievementCondition` — one per achievement type (KillCountCondition, RunTimeCondition, ...)
- `WeaponSignatureMod` — additive modifications a passive applies to a weapon

Two implementation approaches:

- **A. Type-name string + script registry** — each subclass registers itself at boot; ScriptableObjects hold a `string typeName` field; runtime resolves to the class via a static dictionary
- **B. `[SerializeReference]` on ScriptableObjects** — Unity's official polymorphic-serialization mechanism

## Decision

**Option A — type-name string + script registry.**

Rationale:

1. **Compile-checked**: a missing `[Brave.Register]` attribute on a new class fails the EditMode test, not just at runtime when the SO is loaded
2. **Code-reviewable**: every mechanic is a named class in source control; rename detection by IDE is trivial
3. **Save-compat**: if we ever serialize a mechanic state to save (we will for boss progress), strings are forward-compatible — `[SerializeReference]`'s typename changes break saves
4. **Editor stability**: `[SerializeReference]` in Unity 6 is improved but still occasionally drops references on type renames; manual registries don't have this risk

## Implementation

```csharp
// In each implementing class:
[Brave.Register("bunny.hop_dodge")]
public class BunnyHopDodge : SignatureMechanic {
    public override void Activate(...) { ... }
}

// In CharacterDefinition (ScriptableObject):
public string signatureTypeName = "bunny.hop_dodge";

// At boot, MechanicRegistry scans assemblies for [Brave.Register]:
public static class MechanicRegistry {
    private static readonly Dictionary<string, Func<SignatureMechanic>> _factories = new();
    static MechanicRegistry() { /* reflection scan */ }
    public static SignatureMechanic Resolve(string typeName) =>
        _factories.TryGetValue(typeName, out var f) ? f() : throw new KeyNotFoundException(typeName);
}
```

EditMode test asserts every `CharacterDefinition.signatureTypeName` resolves before runtime.

## Consequences

- gameplay-engineer authors mechanics as named classes with `[Brave.Register]` attribute
- systems-engineer's `MechanicRegistry` scans `[Brave.Register]` at boot (~10ms one-time)
- qa-engineer writes `MechanicRegistryTests.cs` asserting all `*.asset` SO `typeName` strings resolve
- Renames are explicit: change the attribute string, run the rename migration on existing SOs (small Editor script)
- ADR-0008's save-system mechanic-state serialization uses these same typename strings

## Alternatives considered

- **B. `[SerializeReference]`** — rejected for save-compat reasons. Revisit if Unity 7 stabilizes typename change handling.
- **Pure ScriptableObject polymorphism** — rejected. Would require one SO subclass per mechanic, file-explosion (60+ SOs), and clunky Inspector workflow.
- **Lua / scripting language for mechanics** — rejected. Adds runtime cost + no static checking.

## References

- `docs/06-tech-spec/02-data-model.md`
- `docs/06-tech-spec/03-save-system.md`
- `docs/06-tech-spec/09-event-bus.md` (event channels use a similar typed pattern)
