# passives.json — schema

> Source of truth for the 6 launch passive items. Read into `PassiveDefinition` ScriptableObjects.

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | "1.0" | Bump on breaking change. |
| `doc` | string | — | Human header. |
| `passives` | array | length=6 | The 6 passive entries. |

## `passives[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `id` | string (kebab-case) | slug | — | Stable identifier. |
| `display_name` | string | — | — | Player-facing. |
| `effect_key` | string (enum) | — | `"magnet_pct"`, `"max_hp_pct"`, `"hp_regen_per_sec"`, `"global_dmg_pct"`, `"crit_rate"`, `"projectile_count_at_l2_l4"` | Maps to a runtime stat modifier. |
| `value_per_level` | float | varies | [0, 1.0] | Additive per level. For percent stats, fraction (e.g. 0.20 = +20%). |
| `max_level` | int | level | 5 | Always 5 per design rule. |
| `evolution_ingredient_for` | array of string | — | — | List of `evolutions[].id` that consume this passive at L5. |
| `note` | string (optional) | — | — | Free-form caveat. |

## Example entry expanded

```json
{
  "id": "magnet-charm",
  "display_name": "Magnet Charm",
  "effect_key": "magnet_pct",
  "value_per_level": 0.20,
  "max_level": 5,
  "evolution_ingredient_for": ["harvest-cyclone", "pinwheel-storm"]
}
```

At L1 this grants +20% pickup magnet radius; at L5, +100%. Ingredient for both Harvest Cyclone (Carrot Boomerang evolution) and Pinwheel Storm (Whirligig evolution). Per `03-weapon-tuning.md` collision check, picking this charm locks the player into a single evolution path.

## Validation rules

- `id` unique kebab-case.
- `value_per_level × max_level` must not violate `00-formulas.md` clamps (e.g., crit-charm caps at 0.25 added crit; combined with character crit_rate ≤ 0.95 stays in band).
- `evolution_ingredient_for` entries must reference valid `evolutions[].id` in `weapons.json`.

## Cross-references

- `docs/02-gdd/04-weapons.md` § Passive items.
- `docs/10-balance/03-weapon-tuning.md` § Evolution recipes collision check.
