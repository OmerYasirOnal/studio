# weapons.json — schema

> Source of truth for the 18-weapon roster (12 launch + 6 Wave 9 expansion) + 8 evolved variants. Read into `WeaponDefinition` ScriptableObjects.

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | "1.0" | Bump on breaking change. |
| `doc` | string | — | Human header. |
| `weapons` | array | length=18 | Base weapon roster (12 launch + 6 Wave 9). |
| `evolutions` | array | length=8 | Evolved variants (6 evolution recipes + 2 spec'd post-launch). |

## `weapons[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `id` | string (kebab-case) | slug | — | Stable identifier. |
| `display_name` | string | — | — | Player-facing. |
| `archetype` | string (enum) | — | `"projectile"`, `"area"`, `"aura"`, `"summon"`, `"utility-beam"` | Drives Otter's +1 projectile rule. |
| `targeting` | string (enum) | — | `"nearest"`, `"furthest"`, `"random-in-range"`, `"random-screen-position"`, `"self-radial"`, `"nearest-sweep-lock"`, `"orbit-self"`, `"orbit-then-dive"`, `"random-direction-roll"` | Sub-targeting per weapon. |
| `dmg_base` | float | HP-per-hit | [0.1, 10.0] | Level-1 base damage. |
| `rate_ms` | int | ms | [100, 5000] | Seconds-between-fires × 1000. |
| `range_units` | float | game-units | [0, 12] | 0 for self-tied; ≤ 12 for projectile-bound. |
| `projectiles_base` | int | count | [0, 8] | Number of projectiles per fire; 0 for aura. |
| `level_mult` | array[5] of float | multiplier | each ∈ [1.0, 3.0] | Per-level damage multiplier (index 0 = L1 = 1.0). |
| `level_perks` | array of object | — | length=4 (L2..L5) | Per-level perks. |
| `evolution` | object \| null | — | — | Recipe (see sub-shape) or `null` for non-evolving. |
| `synergy_tags` | array of string | — | — | For draft-weight bias. |
| `unlock` | string | — | — | Free-form unlock condition. |
| `arm_time_ms`, `cloud_lifetime_ms`, `lifetime_ms`, `travel_ms`, `splash_units_base`, `zaps_per_cloud`, `slow_pct_base` | varies | varies | — | Optional, archetype-specific. |

### `level_perks[]` shape

| Field | Type | Notes |
|---|---|---|
| `level` | int | 2..5 |
| `perk` | string (enum) | `"dmg_pct"`, `"rate_ms"`, `"projectiles"`, `"range_units"`, `"range_units_delta"`, `"range_pct_and_pierce"`, `"arm_time_ms"`, `"chain"`, `"slow_pct"`, `"frostbite_debuff_pct"`, `"beam_width_mult"`, `"reflect_off_edge"`, `"splash_units"`, `"projectiles"`, `"contact_tick_mult"`, `"dot_ms"`, `"bounce"`, `"pierce"`, `"dmg_and_splash"`, `"zaps_per_cloud"`, `"cloud_lifetime_ms"`, `"lifetime_ms"` |
| `value` | scalar \| object | Numeric or compound. |

### `evolution` shape

| Field | Type | Notes |
|---|---|---|
| `evolved_id` | string | References `evolutions[].id`. |
| `required_charm_id` | string | References `passives.json` id. |
| `required_charm_level` | int | Always 5. |
| `required_weapon_level` | int | Always 5. |

### `evolutions[]` entry

| Field | Type | Notes |
|---|---|---|
| `id` | string | Stable slug. |
| `display_name` | string | Player-facing. |
| `dmg_base` / `rate_ms` / `range_units` / `archetype` | varies | Effective tuning of the evolved form. |
| `tag_headline` | string | One-line player-facing flavor. |

## Example entry expanded

```json
{
  "id": "carrot-boomerang",
  "display_name": "Carrot Boomerang",
  "archetype": "projectile",
  "targeting": "nearest",
  "dmg_base": 1.2,
  "rate_ms": 1000,
  "range_units": 5.0,
  "projectiles_base": 1,
  "level_mult": [1.00, 1.15, 1.35, 1.55, 1.85],
  "level_perks": [
    {"level": 2, "perk": "projectiles", "value": 1},
    {"level": 3, "perk": "dmg_pct", "value": 0.20},
    {"level": 4, "perk": "rate_ms", "value": 800},
    {"level": 5, "perk": "range_pct_and_pierce", "value": {"range_pct": 0.25, "pierce": 4}}
  ],
  "evolution": {
    "evolved_id": "harvest-cyclone",
    "required_charm_id": "magnet-charm",
    "required_charm_level": 5,
    "required_weapon_level": 5
  },
  "synergy_tags": ["kinetic", "nature"],
  "unlock": "starter"
}
```

This is the vertical-slice starter. Base 1.2 HP/hit at 1 fire/sec, 5-unit range. L2 adds a second boomerang (+1 projectile); L3 boosts DMG +20%; L4 fires every 0.8 s; L5 piercing-on-return + 25% range. Evolves to Harvest Cyclone when paired with a max-level Magnet Charm.

## Validation rules

- `id` must be unique and kebab-case.
- `level_mult` must be length 5, index 0 = 1.0 (the L1 base is implicit).
- `evolution.required_charm_id` must reference `passives.json`.
- `evolution.evolved_id` must reference `evolutions[].id`.
- Per ADR-0001, **NO `allowed_characters` field**. Universal pool.
- L5 DPS must fall within the ±20% band per `03-weapon-tuning.md` — run TTK ladder before merge.

## Cross-references

- `docs/10-balance/03-weapon-tuning.md` — per-weapon DPS tables.
- `docs/02-gdd/04-weapons.md` — design intent.
- ADR-0001 — universal weapon pool.
