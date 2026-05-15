# enemies.json — schema

> Source of truth for enemy stats. Read into `EnemyDefinition` ScriptableObjects. **Per-minute HP scaling** is computed at runtime via the formula in `00-formulas.md` § 9.

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | "1.1" | Bump on breaking change. `1.1` (Wave 9) adds Beach + Cavern rosters; field set unchanged. |
| `doc` | string | — | Human header. |
| `scaling_method` | string (enum) | `"linear-per-minute"` | Only mode supported at launch. |
| `minute_zero_reference` | int | 1 | The minute baseline that `hp_base` corresponds to (minute 1, not 0). |
| `enemies` | array | — | Enemy entries. |

## `enemies[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `id` | string (kebab-case) | slug | — | Stable identifier. |
| `display_name` | string | — | — | Player-facing. |
| `role` | string (enum) | — | `"swarmer"`, `"tank"`, `"ranged"`, `"elite"`, `"boss"` | Drives AI + drop table + density slot. |
| `biome` | string (kebab-case) | — | — | Owning biome. |
| `scaling` | object | varies | varies | HP/DMG/SPD baselines + per-minute slope. |
| `drops_ref` | string | — | — | Reference into `drops.json` table id. |
| `behavior_note` | string (opt) | — | — | Free-form designer note explaining variant behavior choice. Not consumed at runtime. Added Wave 9 to document biome 2/4 variant intent (e.g. "sidesteps laterally", "hovering motion"). |
| (role-specific) | (varies) | (varies) | — | `charge` for tanks, `ranged` for ranged, `phases` + telegraph_min_ms for elites/bosses. |

### `scaling` sub-fields

| Field | Type | Unit | Notes |
|---|---|---|---|
| `hp_base` | int | HP | Minute-1 HP. (Boss uses `hp_mid_boss` / `hp_end_boss` instead.) |
| `hp_per_min` | int | HP | Linear-per-minute slope. |
| `contact_dmg` | int | HP | Per-touch damage. |
| `ranged_dmg` | int (opt) | HP | Per-projectile damage. |
| `aoe_dmg` | int (opt) | HP | Per AOE-marker damage. |
| `speed_mult_vs_player` | float | — | 1.0 = player baseline (4.5 u/s). |
| `defense_mult` | float | fraction | [0, 0.75] — clamped at runtime. |

### Boss-specific

| Field | Type | Notes |
|---|---|---|
| `phases` | array | HP gate transitions; phase change triggers feel.json timings. |
| `telegraph_min_ms` | int | Boss attack telegraph floor (800 ms). |
| `hitstop_phase_change_ms` | int | Hitstop on phase change (per ADR-0003: 150 ms). |
| `time_dilate_after_phase_ms` | int | Slow-mo window length (200 ms). |
| `time_dilate_factor` | float | 0.5 = half-speed. |
| `hitstop_on_kill_ms` | int | Per ADR-0003 (boss kill: 250 ms). |

## Example entry expanded

```json
{
  "id": "sleepy-boar",
  "display_name": "Sleepy Boar",
  "role": "tank",
  "biome": "meadow",
  "scaling": {
    "hp_base": 80,
    "hp_per_min": 40,
    "contact_dmg": 18,
    "speed_mult_vs_player": 0.60,
    "defense_mult": 0
  },
  "charge": {
    "interval_ms": 4000,
    "burst_speed_mult": 1.50,
    "burst_duration_ms": 1000,
    "telegraph_ms": 400
  },
  "drops_ref": "tank-default"
}
```

Sleepy Boar is the Meadow tank. HP at minute 1 = 80; minute 5 = 80+4×40=240; minute 10 = 80+9×40=440. Slow base movement (0.6× player), but charges every 4 s with a 1 s burst at 1.5×. Charge is telegraphed with a 0.4 s wind-up animation. Drops mid-XP gems, 60% chance of gold, 8% chance of heart (from `drops.json` → `tank-default`).

## Validation rules

- `id` unique kebab-case.
- `role` must be one of the 5 enum values.
- Boss entries use `hp_mid_boss` / `hp_end_boss` instead of `hp_base` / `hp_per_min`.
- `defense_mult` ≤ 0.75 (per formulas clamp).
- HP scaling must be cross-checked against `04-enemy-tuning.md` TTK ladders before merge.
- The current Meadow set diverges from `05-enemies.md` baselines for swarmer/ranged/elite/boss — see pending ADR-0006.
- **Wave 9 (Beach + Cavern rosters):** biome `"beach"` and `"cavern"` slugs are now valid. Per `docs/02-gdd/06-biomes.md` Beach is +10% harder than Meadow and Cavern is +40% harder; HP/`defense_mult` deltas in the new entries are calibrated to this target. Boss IDs `crab-captain` / `sneaky-cave-mole` are referenced from `waves.json` but intentionally NOT yet present here (single-launch-boss policy — Old Boar King only).

## Cross-references

- `docs/02-gdd/05-enemies.md` — design intent + visual variants.
- `docs/10-balance/04-enemy-tuning.md` — TTK ladders + recalibration math.
- `docs/10-balance/00-formulas.md` § 9 — per-minute scaling formula.
- `data/balance/drops.json` — referenced by `drops_ref`.
- ADR-0003 — boss hitstop / time-dilate timings.
