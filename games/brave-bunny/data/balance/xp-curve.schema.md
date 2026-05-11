# xp-curve.json — schema

> Precomputed XP-to-next-level table for L1..L30. Computed via formula `floor(20 * level^1.55 + 5)`.

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | "1.0" | Bump on breaking change. |
| `doc` | string | — | Human header. |
| `formula` | string | — | Source-of-truth formula text. |
| `max_level` | int | 30 | Meta-progression cap. |
| `total_xp_to_max` | int | 47930 | Sum of `xp_to_next[].xp`. |
| `xp_to_next` | array | length=30 | Per-level XP requirement to advance. |

## `xp_to_next[]` entry

| Field | Type | Unit | Range | Notes |
|---|---|---|---|---|
| `from_level` | int | level | [1, 30] | Source level. |
| `to_level` | int | level | from_level+1 | Always +1. |
| `xp` | int | XP | computed | Amount of XP to advance. |

## Example entry expanded

```json
{"from_level": 9, "to_level": 10, "xp": 607}
```

At level 9 the player needs 607 XP to reach level 10. Computed as `floor(20 * 9^1.55 + 5) = floor(20 * 30.06 + 5) = floor(606.2) = 607`.

## Validation rules

- `xp_to_next` length must equal `max_level` (30 entries).
- `from_level` must form a consecutive 1..30 sequence.
- Sum of `xp_to_next[].xp` must equal `total_xp_to_max`.
- Do **not** edit individual values; regenerate the entire table from the formula if the exponent changes.

## Cross-references

- `docs/10-balance/00-formulas.md` § 5 — XP curve rationale + sensitivity.
- `docs/02-gdd/01-core-loop.md` — 15-25 level-ups per run target.
- `docs/09-level-design/00-pacing-model.md` — beat-by-beat level-up cadence.
