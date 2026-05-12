# ADR 0019 — Wave 4 cleanup debt: cross-plane bug + 4 follow-ups

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing Wave 4 dispatch findings)

## Context

Phase 5 Wave 4 landed AutoAttack mechanics, SaveService, balance importer
extensions, and re-enabled the BRAVE_FUTURE_API tests — all in parallel,
~140 new C# files/tests at the time of writing. During implementation,
the dispatched agents surfaced **5 cleanup items** that cannot be
addressed inside their respective domains without scope-creep, but which
break the vertical-slice runtime if left:

| # | Severity | Surface | Component |
|---|---|---|---|
| 1 | **HIGH** — runtime broken | `EnemyRegistry.FindFirstWithinRadius` uses XY (`d.x²+d.y²`) | Code/Gameplay/Enemies/ |
| 2 | **HIGH** — runtime broken | `AutoAttackController.AcquireTarget` same XY plane bug | Code/Gameplay/Combat/ |
| 3 | MED — content gap | Enemy death → projectile pool return is not wired | Code/Gameplay/ |
| 4 | MED — content gap | `EnemyRole` enum has no `Boss` value (boss spawning blocked) | Code/Gameplay/Enemies/ |
| 5 | MED — content gap | `WeaponDefinition` lacks archetype-specific fields (`arm_time_ms`, `splash_units`, `cloud_lifetime_ms`, `zaps_per_cloud`, `slow_pct_base`, `lifetime_ms`) | Code/Gameplay/Definitions/ |

Items 1–2 are the "cross-plane bug": the world's rendering, player
movement, projectile travel, and enemy movement are all on the XZ plane
(per ADR-0018), but the registry and target-acquisition helpers still
query distances in XY. **Visible runtime symptom:** the carrot projectile
spawns, travels in the correct XZ direction, but the registry's
"find-nearest-enemy" returns wrong neighbours (or none), so AutoAttack
either fires at wrong targets or fails to hit anything.

Items 3-5 are content gaps that block 6 of the 12 weapons from being
fully data-driven and prevent the boss from spawning at all. Important
for the eventual Vertical-Slice Phase-5 exit, but the carrot-boomerang
+ swarm-mole flow can vertical-slice **without** them.

## Decision

**One coordinated Wave 5 dispatch** to address the HIGH items, and a
separate tech-architect dispatch (later) for the content gaps.

**Wave 5A (gameplay-engineer — immediate, unblocks runtime):**
1. Port `EnemyRegistry.FindFirstWithinRadius` (and any sibling helpers)
   to XZ semantics: `dx² + dz²` instead of `dx² + dy²`. Add tests in
   `Tests/EditMode/Gameplay/Enemies/EnemyRegistryTests.cs`.
2. Port `AutoAttackController.AcquireTarget` to XZ semantics. Add a test
   in `Tests/EditMode/Gameplay/Combat/AutoAttackControllerTests.cs`.
3. Wire enemy death → projectile-pool return (or the equivalent
   "enemy returns to pool on HP <= 0" path if such a sink exists for
   enemies as well). Defensive: do not crash if a projectile reaches a
   target that died one frame earlier.

**Wave 5B (tech-architect — later, after 5A lands):**
4. Add `EnemyRole.Boss = 4` to the enum + propagate through any switch
   tables.
5. Extend `WeaponLevelData` (or introduce a sibling per-archetype config
   block) for the 6 archetype-specific fields. This needs a small data-
   model ADR of its own before any change — the design choice between
   "fat WeaponLevelData with optional fields" vs. "WeaponLevelData +
   ArchetypeConfig sidecar" is non-trivial and should be deliberate.

**Wave 5C (asset-curator / framework maintainer — out of band):**
6. Populate `drops.json` importer (the current `ImportDrops` is a stub).
   Lower urgency: drops can mock at runtime until the data is hooked.

## Consequences

- 5A is the critical-path: without it, the AutoAttack pipeline runs but
  hits the wrong enemies or none. The Vertical-Slice perf test cannot
  meaningfully measure 200 enemies if the player's projectiles miss.
- 5B unlocks the remaining 6 weapons + the boss. Without it, the
  vertical slice ships with 3 weapons + 4 enemy roles, which is what
  the design always anchored on (Carrot Boomerang + Sunbeam + Daisy
  Mine + Swarm/Tank/Ranged/Elite). So 5B is **not** a vertical-slice
  blocker — it's a Phase-6 ramp-up unblocker.
- 5C is bookkeeping. Defer freely.

## Resolution criteria (when to close this ADR)

- `EnemyRegistry`, `AutoAttackController`, and all distance-based
  gameplay helpers use XZ semantics; tests prove it.
- `grep -E '\.y[ ]*\*[ ]*' Code/Gameplay/` returns zero hits in
  distance-computation contexts (excluding intentionally-XY UI math).
- `EnemyRole.Boss` exists.
- `WeaponDefinition` (or its sibling config) carries the 6 archetype
  fields; the importer populates them; the 3 vertical-slice weapons
  still pass their unit tests.

## Alternatives considered

1. **Fix the cross-plane bug inside Wave 4 by extending the gameplay-
   engineer dispatch in-flight** — rejected. The Wave 4 dispatch had
   a clean scope; adding "also fix this discovered bug" mid-flight
   would have either bloated the brief or violated the "no scope creep"
   discipline. Surfacing as ADR + Wave-5 is the right discipline.
2. **Patch only `EnemyRegistry` and ignore `AutoAttackController` for
   now (since it has its own private target-search)** — rejected. Two
   diverged code paths for the same bug type guarantees the fix gets
   half-applied. Co-located fix is cheaper.
3. **Treat the 6 missing weapon fields as a `[SerializeReference]`
   polymorphic-block** — rejected at the ADR-0009 level (registry-based
   polymorphism, not SerializeReference). Will revisit in 5B's design
   ADR.

## References

- `docs/handoffs/gameplay-engineer-20260512-194352.md` (Wave 4
  AutoAttack hand-off — original bug surface)
- `docs/handoffs/balance-engineer-20260512-193850.md` (Wave 4 importer
  hand-off — content-gap surface)
- ADR-0018 (XZ migration that closed the player/enemy side but missed
  the registry helpers)
- `docs/06-tech-spec/05-performance-budget.md` (200-enemy stress target
  that 5A unblocks)
