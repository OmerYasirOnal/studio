# feel.json — schema

> Source of truth for hitstop, time-dilate, screen shake, and crit PRD timings. **Canonical lock per ADR-0003** — these values are not subject to balance-engineer drift; they are a feel-pillar contract.

## Top-level fields

| Field | Type | Notes |
|---|---|---|
| `schema_version` | string | "1.0" |
| `doc` | string | Human header. |
| `adr_reference` | string | "0003-hitstop-timings" |
| `hitstop` | object | Per-trigger hitstop durations. |
| `time_dilate` | object | Slow-mo windows. |
| `crit_prd_window` | object | Pseudo-random distribution config. |
| `screen_shake` | object | Per-trigger camera shake amplitudes. |

## `hitstop` fields

| Field | Type | Unit | Range | Source |
|---|---|---|---|---|
| `basic_enemy_hit_ms` | int | ms | 0 | ADR-0003 |
| `basic_enemy_kill_ms` | int | ms | 20 | ADR-0003 |
| `elite_hit_ms` | int | ms | 30 | ADR-0003 |
| `elite_kill_ms` | int | ms | 80 | ADR-0003 |
| `boss_damage_tick_ms` | int | ms | 40 | ADR-0003 |
| `boss_phase_change_ms` | int | ms | 150 | ADR-0003 |
| `boss_kill_ms` | int | ms | 250 | ADR-0003 |

## `time_dilate` fields

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `boss_phase_change_factor` | float | — | [0.1, 1.0] | 0.5 = half speed. |
| `boss_phase_change_duration_ms` | int | ms | [50, 500] | 200 ms. |
| `boss_entrance_factor` | float | — | [0.1, 1.0] | 0.4 per waves.json. |
| `boss_entrance_duration_ms` | int | ms | [200, 2000] | 800 ms per waves.json. |
| `boss_kill_factor` | float | — | [0.1, 1.0] | 0.3 (run-end ceremony lead-in). |
| `boss_kill_duration_ms` | int | ms | [500, 3000] | 1200 ms. |

## `crit_prd_window` fields

| Field | Type | Notes |
|---|---|---|
| `expected_intervals_without_crit_before_force` | int | 4 — after 4× expected-crit-interval without a crit, force-crit on next hit. |
| `note` | string | Free-form. |

## `screen_shake` fields

| Field | Type | Unit | Notes |
|---|---|---|---|
| `basic_kill_amp` | float | screen-fraction | 0.05 (subtle). |
| `elite_kill_amp` | float | screen-fraction | 0.15. |
| `boss_phase_change_amp` | float | screen-fraction | 0.35. |
| `boss_kill_amp` | float | screen-fraction | 0.50 (the climactic moment). |
| `unit` | string | — | "screen-fraction" — amp is fraction of screen height. |

## Example entry expanded

```json
"hitstop": {
  "basic_enemy_hit_ms": 0,
  "basic_enemy_kill_ms": 20,
  "elite_hit_ms": 30,
  "elite_kill_ms": 80,
  "boss_damage_tick_ms": 40,
  "boss_phase_change_ms": 150,
  "boss_kill_ms": 250
}
```

These mirror ADR-0003's locked table. Basic enemies have **zero hitstop on hit** (would stutter the game at peak swarm), 20 ms on kill (subtle confirmation). Elite hits add 30 ms (player feels weight), elite kills 80 ms (the satisfying chunk). Boss ticks 40 ms (since tick rate is high), phase change 150 ms (plus the time-dilate window), kill 250 ms before the run-end ceremony starts.

## Validation rules

- **DO NOT EDIT** without an ADR amending 0003. These values are locked.
- If gameplay-engineer reports performance issues (frame stutter at high enemy density), reduce only the basic_enemy_kill_ms or basic_enemy_hit_ms; never reduce elite/boss values.
- Screen-shake amplitudes are calibrated for mobile portrait — adjust unit if engine layer changes.

## Cross-references

- ADR-0003 — hitstop timings reconciliation (canonical).
- `docs/02-gdd/11-feel-pillars.md` — feel pillars (references this file rather than inlining).
- `docs/07-art-bible/05-vfx-style.md` — VFX timings.
- `docs/10-balance/00-formulas.md` § 10 (feel/hitstop).
