# ADR 0003 — Hitstop timings reconciliation

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator

## Context

Two adjacent specs disagree on hitstop duration:

- `docs/02-gdd/11-feel-pillars.md` — "60ms hitstop on elite kills" (under the "Auto-attack has impact" pillar)
- `docs/07-art-bible/05-vfx-style.md` — "Hitstop timing: 60ms on elite, 120ms on boss, none on basic enemies"

The feel-pillars doc only specified elites. The VFX style spec extended to bosses. This needs explicit lock so gameplay-engineer knows the canonical numbers when implementing.

## Decision

**Canonical hitstop timings (game-wide):**

| Trigger | Hitstop duration |
|---|---|
| Basic enemy hit | **0 ms** (none) |
| Basic enemy kill | **20 ms** (subtle confirmation) |
| Elite enemy hit | **30 ms** |
| Elite enemy kill | **80 ms** |
| Boss damage tick | **40 ms** (was 60 in feel pillars — reduced because tick rate is high) |
| Boss phase change | **150 ms** + 0.5x time-dilate for 200 ms after |
| Boss kill | **250 ms** + run-end ceremony |

These numbers will be stored in `data/balance/feel.json` (not as magic constants in scripts).

## Consequences

- balance-engineer extends `data/balance/` with a new file `feel.json` to hold these timings.
- gameplay-engineer reads `feel.json` into a `FeelDefinition` ScriptableObject and consults it at every hit/kill event.
- QA test scenarios in `Assets/Tests/PlayMode/Smoke/HitStopTests.cs` (added by qa-engineer in Phase 5) assert these durations.
- `docs/02-gdd/11-feel-pillars.md` will be updated to reference this ADR; do NOT inline the numbers in the pillars doc.

## Alternatives considered

- **Use feel-pillars values verbatim (60ms elite only)** — rejected. Leaves bosses ambiguous and the VFX team's nuance is correct.
- **Use VFX style values verbatim (60/120/0)** — rejected. 60ms on every elite hit is too disruptive at high attack speed; per-kill only is correct.
- **Tune in playtest** — rejected as the initial lock. We'll absolutely tune later, but we need numbers now for the systems to wire up.

## References

- `docs/02-gdd/11-feel-pillars.md`
- `docs/07-art-bible/05-vfx-style.md`
- `data/balance/feel.json` (to be created by balance-engineer)
