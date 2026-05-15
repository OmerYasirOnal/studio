# evolutions.json — schema

> Source of truth for the 8 launch weapon evolution recipes. Read by
> `BalanceJsonImporter.ImportEvolutions()` (Editor-only) into
> `EvolutionRecipeAsset` ScriptableObjects under
> `Assets/_Brave/Data/Definitions/Evolutions/`.
>
> ADR-0007 governs charm consumption (always `true` at launch).

## Top-level fields

| Field | Type | Range | Notes |
|---|---|---|---|
| `schema_version` | string | `"1.0"` | Bump on breaking change. |
| `doc` | string | — | Human header. |
| `evolutions` | array | length=8 | Recipe entries. |

## `evolutions[]` entry

| Field | Type | Range | Notes |
|---|---|---|---|
| `id` | string (kebab-case) | — | Stable identifier; matches `evolved_weapon_id`. |
| `base_weapon_id` | string | weapons.json `id` | The L5 base weapon. |
| `required_charm_id` | string | passives.json `id` | The charm that must be held at max level. |
| `required_weapon_level` | int | =5 | Always 5 — recipes only trigger when the weapon is maxed. |
| `required_charm_level` | int | =5 | Always 5 — recipes only trigger when the charm is maxed. |
| `evolved_weapon_id` | string | weapons.json `id` (evolution slug) | The evolved form delivered to the player. |
| `consume_charm` | bool | `true` | ADR-0007: charm is consumed on evolution (reserved for future cosmetics). |

## Validation rules

- `id` unique kebab-case across the file.
- `base_weapon_id` must exist in `weapons.json` `weapons[]`.
- `evolved_weapon_id` must exist in `weapons.json` `evolutions[]`.
- `required_charm_id` must exist in `passives.json` `passives[]`.
- Each entry MUST mirror the corresponding `weapons[].evolution` block in `weapons.json` — discrepancies are import-time warnings.
- `required_weapon_level` and `required_charm_level` MUST equal 5 (single-recipe gate per ADR-0007).

## Launch roster (8 evolutions)

| Base weapon | + Charm | → Evolved weapon |
|---|---|---|
| carrot-boomerang | magnet-charm | harvest-cyclone |
| sunbeam | crit-charm | solar-halo |
| daisy-mine | damage-charm | meadow-bloom |
| pebble-sling | projectile-charm | stone-storm |
| honey-aura | hp-charm | honey-hug |
| acorn-cannon | crit-charm | oak-thunderclap |
| cob-mortar | damage-charm | cornfield-volley |
| whirligig | magnet-charm | pinwheel-storm |

## Cross-references

- `docs/02-gdd/04-weapons.md` § Evolution recipes
- `docs/10-balance/03-weapon-tuning.md` § Evolution collision check
- `docs/decisions/0007-evolution-charm-consumption.md`
- `data/balance/weapons.json` § `weapons[].evolution`, `evolutions[]`
- `data/balance/passives.json`
