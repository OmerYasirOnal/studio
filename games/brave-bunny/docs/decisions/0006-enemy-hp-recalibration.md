# ADR 0006 — Enemy HP recalibration

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing balance-engineer wave-4 flag)

## Context

`docs/02-gdd/05-enemies.md` gave a first-pass HP curve for the 5 enemy roles. Balance-engineer's wave-4 Monte Carlo (`docs/10-balance/06-monte-carlo-notes.md`) found those curves over-tuned by 30-50% — median run never reached the boss, and when it did the TTK against Old Boar King (12000 HP) was longer than the 90s target.

The balance-engineer recalibrated in `data/balance/enemies.json` and surfaced the gap.

## Decision

**Adopt the balance-engineer's recalibrated curves as canonical.** Three specific changes:

### 1. Swarmer baseline
| Minute | Old (GDD) | New (balance) |
|---|---|---|
| m1 | 8 HP | **6 HP** |
| m3 | 20 HP | **14 HP** |
| m5 | 32 HP | **22 HP** |
| m7 | 44 HP | **30 HP** |
| m10 | 62 HP | **42 HP** |

Formula: `swarmer_hp(m) = 6 + 4 × (m - 1)` (was `8 + 6 × (m - 1)`).

### 2. Elite baseline
- m1 HP: 600 → **300**
- Slope: `+250` → `+80` per minute
- Reason: at the old curve elites became damage sponges by minute 5, blocking pacing flow.

### 3. Old Boar King total HP
- 12000 → **3000**
- Phase gates remain at 66% / 33% (so phase 1 = 1020 HP, phase 2 = 1020 HP, phase 3 = 960 HP)
- With median minute-7 player DPS of ~80, target TTK becomes 38 seconds — well within the 90s boss-fight window, leaving margin for telegraphs and intermission

`docs/02-gdd/05-enemies.md` will be updated to reference this ADR as the source of truth for HP values. Going forward: GDD describes intent; balance JSON has the numbers.

## Consequences

- balance-engineer continues to own the JSON; game-designer reviews changes via PR
- gameplay-engineer reads from `data/balance/enemies.json` via the generated ScriptableObjects — no code changes needed
- The Capybara Go!-style boss-rush event (post-launch) will need a separate HP curve; cross-reference content-roadmap
- If playtest shows the boss too easy, slope the phase 3 HP up to 1200 (40% of total) — flagged as a tuning lever

## Alternatives considered

- **Keep GDD curve, scale up player DPS** — rejected. Pillar 2 (build depth) thrives on consistent enemy density; inflating player DPS to match makes everything feel weightless.
- **Halve enemy density instead** — rejected. The 200-enemy perf budget exists; using it for visual swarm is the genre's pillar.
- **Tune-by-playtest only** — rejected. The Monte Carlo gives us defensible starting numbers; we'll absolutely tune from playtest later.

## References

- `docs/02-gdd/05-enemies.md`
- `docs/10-balance/00-formulas.md`
- `docs/10-balance/04-enemy-tuning.md`
- `docs/10-balance/06-monte-carlo-notes.md`
- `data/balance/enemies.json`
