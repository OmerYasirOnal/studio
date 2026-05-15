# characters.json — schema

> Source of truth for character baselines. Read by gameplay-engineer into `CharacterDefinition` ScriptableObjects at edit time.

## Top-level fields

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `schema_version` | string | — | "1.0" | Bump on breaking change. |
| `doc` | string | — | — | Human-readable header note. |
| `calibration_anchor` | string | id | — | Always `"bunny"`; the 1.0 baseline. |
| `base_move_units_per_sec` | float | units/sec | (0, 10] | Bunny's raw move speed (4.5). |
| `base_magnet_units` | float | units | (0, 5] | Default pickup pull radius (1.5). |
| `per_level_perks` | object | — | — | The level 1→30 grant per stat line. |
| `characters` | array | — | length=8 | The 8 character entries. |

### `per_level_perks` sub-fields

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `hp_per_level_pct` | float | fraction | [0, 0.05] | +1% HP per level (= 0.01). |
| `dmg_per_level_pct` | float | fraction | [0, 0.02] | +0.7% damage per level. |
| `move_per_level_pct` | float | fraction | [0, 0.01] | +0.2% move per level. |
| `crit_rate_per_level` | float | fraction | [0, 0.005] | +0.05% crit per level. |
| `max_level` | int | level | [1, 50] | Hard cap (30). |

### `characters[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `id` | string (kebab-case) | slug | — | Stable identifier; do not rename. |
| `display_name` | string | — | — | Player-facing. |
| `hp_base` | int | HP | [50, 250] | Level-1 absolute HP. |
| `dmg_mult` | float | multiplier | [0.5, 2.0] | Bunny = 1.00. |
| `move_mult` | float | multiplier | [0.5, 2.0] | Bunny = 1.00; ≤ 2 per move-cap clamp. |
| `crit_rate` | float | fraction | [0, 0.5] | Base; clamped to [0, 0.95] after stacking. |
| `crit_damage` | float | multiplier | [0.5, 3.0] | 1.0 = 2x crit damage. |
| `magnet_mult` | float | multiplier | [0.5, 4.0] | Owl = 3.0 post-fix; others = 1.0. |
| `xp_gem_value_bonus` | float | fraction | [0, 0.5] | Owl = 0.10; others = 0.0. |
| `default_starter_weapon` | string | weapon-id | — | Default-equipped weapon; per ADR-0001, NO `allowed_weapons` array. |
| `signature` | object | — | — | Signature mechanic spec; varies per character. |
| `unlock_condition` | object | — | — | Meta-progression gate. Absent / `{type:"none"}` = starter (unlocked from day 0). See `Unlock condition shape` below. |

### `unlock_condition` shape

Drives `Brave.Systems.Progression.CharacterUnlockService`. The importer mirrors these fields onto `CharacterDefinition.unlockCondition` (a `UnlockConditionData` raw struct) at edit-time; runtime translation into `UnlockCondition` happens via `UnlockConditionDataExtensions.ToRuntime()`.

| Field | Type | Required when `type` is | Notes |
|---|---|---|---|
| `type` | string enum | always | One of: `"none"`, `"reach_wave"`, `"defeat_boss"`, `"complete_runs"`, `"pay_stars"`. |
| `wave` | int | `reach_wave` | Wave ordinal threshold (≥ this counts as met). |
| `boss` | string slug | `defeat_boss` | Boss slug from `enemies.json` (e.g. `"wolf-pack-leader"`). |
| `runs` | int | `complete_runs` | Number of completed runs required. |
| `with_character` | string slug | optional (`reach_wave`, `complete_runs`) | Only runs piloted by this character count toward the threshold. Omit for "any character". |
| `stars` | int | `pay_stars` | Star price spent at purchase. Bypasses passive evaluation — triggered by `CharacterUnlockService.TryPurchase`. |

### `signature` shape

The shape varies per character. Common fields:

| Field | Type | Unit | Notes |
|---|---|---|---|
| `id` | string (kebab-case) | slug | Unique signature identifier. |
| `trigger` | string | enum | `"passive"`, `"interval"`, `"every-Nth-weapon-hit"`, `"hp-below-Npct"`, `"enemy-below-Npct-hp"`, `"xp-gem-pickup"`. |
| (varies) | (varies) | (varies) | Per-signature numerics — see example below. |

## Example entry expanded

```json
{
  "id": "bunny",
  "display_name": "Bunny",
  "hp_base": 100,
  "dmg_mult": 1.00,
  "move_mult": 1.00,
  "crit_rate": 0.05,
  "crit_damage": 1.00,
  "magnet_mult": 1.00,
  "xp_gem_value_bonus": 0.00,
  "default_starter_weapon": "carrot-boomerang",
  "signature": {
    "id": "hop-dodge",
    "trigger": "every-5th-weapon-hit",
    "iframe_ms": 400,
    "cooldown_ms": 5000
  }
}
```

Bunny is the calibration anchor (every mult = 1.0). The signature triggers an i-frame burst every 5th weapon hit, lasting 400 ms, then 5000 ms cooldown. All durations are milliseconds; all multipliers dimensionless.

## Validation rules

- All `id` fields must be kebab-case and unique within the array.
- `default_starter_weapon` must reference a weapon `id` in `weapons.json`.
- Sum of any `*_mult` field across `characters[]` should not skew the character balance band (`01-tuning-philosophy.md`) — re-run TTK ladder if changed.
- Per ADR-0001, **NEVER add `allowed_weapons` array** to entries — the weapon catalog is universal.

## Cross-references

- `docs/10-balance/00-formulas.md` § 1, § 3, § 4 (damage, movement, magnet).
- `docs/10-balance/02-character-tuning.md` — per-character L1/L10/L20/L30 tables.
- ADR-0001 — universal weapon pool.
