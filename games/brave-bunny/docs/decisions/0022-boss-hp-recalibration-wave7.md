# ADR 0022 — Boss HP recalibration (Wave 7 vertical-slice TTK)

**Date:** 2026-05-16
**Status:** accepted
**Owner:** balance-engineer (Wave 7C follow-up)
**Supersedes (partial):** ADR-0006 — boss HP only. ADR-0006's swarmer and
elite curves remain canonical for their respective roles.

## Context

ADR-0006 (`docs/decisions/0006-enemy-hp-recalibration.md`, 2026-05-12)
locked **Old Boar King** total HP at **3000** with phase gates at 66% / 33%.
That number was sized against the Wave-4 Monte-Carlo player DPS curve
(median minute-7 player DPS ~80), giving a target TTK of ~38 s — inside
the 90 s boss-fight window of the GDD's first draft.

Wave 7A then delivered the shippable vertical slice. Wave 7B (TTK polish
pass, `docs/10-balance/wave7-ttk-pass.md`) re-targeted the entire TTK
ladder to a punchier feel called out in the orchestrator brief:

| Encounter | Pre-7B TTK target | Wave 7B TTK target |
|---|---|---|
| Wave-1 swarmer w/ starter weapon | 0.3 s | **0.4 – 0.8 s** |
| Wave-5 tank | 18 s | **2 – 4 s** |
| Old Boar King at wave 15 | 90 s | **30 – 45 s** |

Holding the Wave-7B weapon damage curve constant (carrot-boomerang
`dmg_base 1.2 → 3.0`, acorn-cannon `dmg_base 3.0 → 5.5`, etc.), the
resulting late-build DPS rises from ~30 to ~32. At ADR-0006's 3000 HP,
that produces a boss TTK of ~94 s — **more than twice** the new
30–45 s target.

Two paths out:

1. Roll back the Wave-7B weapon damage bumps (returns starter weapons to
   5 s TTK on a swarmer — directly contradicts the new brief).
2. Recalibrate boss HP downward and supersede ADR-0006's boss row.

Path 2 is chosen.

## Decision

**Old Boar King total HP: 3000 → 1200.** Mid-boss equivalent: 2000 → 800.

Phase-gate fractions remain at **66% / 33%** (per ADR-0006). At the new
1200 HP that gives:

| Phase | HP slice | Width | DPS (late-build, ~32) | TTK |
|---|---|---|---|---|
| Phase 1 (100% → 66%) | 408 HP | 33% | 32 | **12.75 s** |
| Phase 2 (66% → 33%) | 396 HP | 33% | 32 | **12.4 s** |
| Phase 3 (33% → 0%) | 396 HP | 33% | 32 | **12.4 s** |
| **Total** | **1200 HP** | 100% | 32 | **37.5 s** ✓ |

37.5 s lands at the median of the 30–45 s target window with ~8–10 s of
phase-change ceremony / telegraph / iframe space slotted between phases.

The numeric values live in `data/balance/enemies.json` under
`old-boar-king.hp_end_boss` (1200) and `hp_mid_boss` (800). Wave 7B
already wrote these — this ADR ratifies them.

## Scope of supersede

This ADR supersedes ADR-0006 **only for the boss HP row**. ADR-0006's
recalibrations for swarmers, archer-mole, and elites remain canonical
for their respective roles — they were independently re-touched in
Wave 7B (swarmer hp_base 6 → 3, big-onion 300 → 150, etc.) but those
moves are scaled adjustments inside the same framework ADR-0006
established, not a re-derivation.

The phrasing on ADR-0006 has been updated to read:
> Status: superseded by ADR-0022 for boss HP (other roles still apply).

## Consequences

- Boss-rush event (post-launch, Capybara Go!-style) — still needs its own
  HP curve per ADR-0006's note. Unchanged by this ADR.
- Hi-DPS late-build outliers (12.6 DPS Carrot Boomerang L5 pre-Wave-7C
  trim) would beat the boss in ~25 s — outside the lower band. Wave 7C
  follow-up (companion commit to this ADR) trims those L5 multipliers to
  bring DPS into the 5.2 – 7.8 band, restoring boss TTK to band.
- If playtest shows the boss too easy at 1200, the lever is `hp_end_boss`
  in `enemies.json`. Each +200 HP adds ~6 s of TTK at late-build DPS.
- gameplay-engineer reads `enemies.json` via the generated
  ScriptableObjects — no code change needed.

## Alternatives considered

- **Keep 3000 HP, accept TTK ~94 s** — rejected. Violates the orchestrator's
  Wave 7B brief and contradicts `wave7-ttk-pass.md`.
- **Keep 3000 HP, bump player late-build DPS to 80+** — rejected.
  Same reason as ADR-0006's parallel alternative ("inflating player DPS
  makes everything feel weightless"). The Wave 7B brief is a feel pass
  on the enemy side, not a power-creep pass on the player side.
- **Add an extra phase (4 phases at 25% each)** — rejected for soft
  launch. Adds choreography load with no playtest signal yet that more
  phases is the right complication.

## References

- ADR-0006 — Enemy HP recalibration (superseded for boss HP)
- `docs/10-balance/wave7-ttk-pass.md` — Wave 7B TTK ladder polish pass
- `data/balance/enemies.json` — boss HP values (canonical)
- `docs/02-gdd/05-enemies.md` — boss role intent
- `docs/10-balance/00-formulas.md` — TTK formula
- `docs/10-balance/01-tuning-philosophy.md` — TTK ladder framework
