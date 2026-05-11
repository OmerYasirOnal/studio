# drops.json — schema

> Drop tables per enemy role. Referenced from `enemies.json` via `drops_ref`. Read by gameplay-engineer at edit time.

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | "1.0" | Bump on breaking change. |
| `doc` | string | — | Human header. |
| `tables` | array | — | Drop table entries. |
| `drop_buff_global` | float | [0, 1.0] | Global drop-rate buff; 0 by default. |
| `drop_buff_note` | string | — | Free-form caveat. |

## `tables[]` entry

| Field | Type | Notes |
|---|---|---|
| `id` | string (kebab-case) | Stable slug; referenced from `enemies.json` `drops_ref`. |
| `drops` | array | Drop items. |

### `drops[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `item` | string (enum) | — | `"xp-gem-small"`, `"xp-gem-medium"`, `"xp-gem-large"`, `"gold-coin"`, `"heart"`, `"chest-small"`, `"chest-medium"`, `"chest-large"`, `"character-shard"`, `"soul-shard"` | Item type. |
| `chance` | float | fraction | [0, 1.0] | Drop probability per kill. |
| `xp_value` | int (opt) | XP | — | For XP gems. |
| `carrot_value` | int (opt) | carrots | — | For gold coins (per-coin). |
| `heal_hp` | int (opt) | HP | — | For hearts. |
| `stack` | int (opt) | count | — | How many of this item drop in one stack. |
| `contains` | string (opt) | — | — | For chests (free-form). |
| `weighted_count` | array (opt) | — | — | For weighted drops (soul shards). |
| `expected_value` | float (opt) | — | — | Computed EV of the weighted draw. |

### `weighted_count[]` entry

| Field | Type | Notes |
|---|---|---|
| `n` | int | Count value. |
| `w` | float | Weight; weights should sum to 1.0. |

## Example entry expanded

```json
{
  "id": "elite-default",
  "drops": [
    {"item": "xp-gem-large",  "chance": 1.00, "xp_value": 25},
    {"item": "gold-coin",     "chance": 1.00, "carrot_value": 20, "stack": 5},
    {"item": "chest-small",   "chance": 1.00, "contains": "1-weapon-or-charm-draft-token"},
    {"item": "soul-shard",    "chance": 1.00,
     "weighted_count": [{"n": 1, "w": 0.50}, {"n": 2, "w": 0.35}, {"n": 3, "w": 0.15}],
     "expected_value": 1.65}
  ]
}
```

Every elite kill drops: 1 large XP gem (25 XP); 5 gold coins (20 carrots each = 100 carrots total); a chest containing one draft token; and 1-3 soul shards weighted 50%/35%/15% (expected ~1.65).

## Validation rules

- `id` unique kebab-case.
- All `chance` values ∈ [0, 1.0]; product with `(1 + drop_buff_global)` clamped to ≤ 1.0 at runtime.
- `weighted_count[]` weights should sum to 1.0 (±0.01 tolerance).
- `expected_value` should match `Σ(n × w)` of the `weighted_count`.

## Cross-references

- `docs/10-balance/00-formulas.md` § 6 (drop chance) + § 7 (soul shards).
- `data/balance/enemies.json` — `drops_ref` field.
- `docs/02-gdd/08-economy.md` — currency model + earn baselines.
