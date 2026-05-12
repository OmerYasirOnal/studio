# Hand-off — balance-engineer — 2026-05-12 19:38:50

**Wave:** Wave 4 (orchestrator-dispatched)
**Task:** Extend `BalanceJsonImporter.ImportWeapons` + `ImportEnemies` so SO fields are populated from `weapons.json` / `enemies.json`. Unblocks gameplay-engineer's AutoAttack + stress-test enemies.

## Note on field paths

`WeaponDefinition` has **no `baseStats` sub-struct** (unlike `CharacterDefinition`). Instead it has `levels[5]` — an array of `WeaponLevelData { damage, fireRate, range, projectiles, upgradeFlavor }`. The importer now writes all 5 rows per weapon using SerializedProperty path `levels.Array.data[i].<field>`.

`EnemyDefinition` has flat fields (no `baseStats`): `baseHP`, `contactDamage`, `rangedDamage`, `moveSpeed`, `defenseMultiplier`, `telegraphWindowSeconds`. Drops live in nested `DropTable drops` (left at default — drops.json importer is still a no-op stub).

## Weapon JSON → SO mapping (per level i = 0..4)

| SO field (`WeaponDefinition.levels[i].*`) | JSON expression | Doc ref |
|---|---|---|
| `damage` | `weapon.dmg_base × weapon.level_mult[i]` | §1 (damage formula); other mults applied at runtime |
| `fireRate` | initial = `rate_ms / 1000`; perk `rate_ms` (L2..L5) sets new absolute, carried forward | schema (seconds-between-fires) |
| `range` | initial = `range_units`; perk `range_units` sets absolute, `range_units_delta` adds | schema |
| `projectiles` | initial = `projectiles_base`; perk `projectiles` adds delta (carry-forward) | schema |
| `upgradeFlavor` | (no JSON source — designer copy; default `""`) | — |

Top-level: `slug ← id`, `displayName ← display_name`, `archetype ← enum(archetype)` via name/kebab-token match. `evolution`, `synergyTags`, `icon`, `projectilePrefab`, `targeting` left untouched (out of scope — asset wiring + ADR-0009 mechanic registry).

## Enemy JSON → SO mapping

| SO field (`EnemyDefinition.*`) | JSON expression | Doc ref |
|---|---|---|
| `baseHP` | `scaling.hp_base` (or `hp_mid_boss` for boss) — minute-1 anchor | §9 (per-min scaling applied by spawner at runtime) |
| `contactDamage` | `scaling.contact_dmg` | schema |
| `rangedDamage` | `scaling.ranged_dmg` (default 0) | schema |
| `moveSpeed` | `4.5 × scaling.speed_mult_vs_player` | §3 movement (player anchor 4.5 u/s) |
| `defenseMultiplier` | `scaling.defense_mult` (clamp [0, 0.75] in `OnValidate`) | §11 |
| `telegraphWindowSeconds` | `telegraph_min_ms ?? charge.telegraph_ms ?? ranged.telegraph_ms / 1000` | ADR-0003 |
| `role` | enum match on `role` string | schema enum |

Top-level: `slug ← id`, `displayName ← display_name`. `drops` (DropTable), `biome`, `prefab`, `telegraphSfxKey`, boss `phases` left at defaults — owned by drops.json / asset-curator / gameplay-engineer follow-ups.

## ADR-0006 verified

`enemies.json` already reflects recalibrated values: swarmers `6 + 4·(m−1)`, elite `300 + 80·(m−1)`, boss `hp_mid_boss=2000 / hp_end_boss=3000`. Importer reads them as-is; no formula override needed here.

## Blockers surfaced

1. **`EnemyRole` enum missing `Boss` member.** `old-boar-king` has `"role": "boss"` in JSON but `Brave.Gameplay.Definitions.EnemyRole` only defines `Swarmer / Tank / Ranged / Elite`. Importer logs a warning and leaves role at the existing default (Swarmer) — wrong. **Next step:** tech-architect or gameplay-engineer should add `Boss = 4` to `EnemyRole.cs`. Out of balance-engineer domain.
2. **`WeaponDefinition` has no field for boss/elite-only weapon traits** like `arm_time_ms`, `cloud_lifetime_ms`, `splash_units_base`, `zaps_per_cloud`, `lifetime_ms`, `slow_pct_base` — currently ignored. These are runtime/archetype-specific; surfacing for tech-architect: either extend `WeaponLevelData` or add archetype-side configs.
3. **`drops.json` importer is still a no-op** — enemy `DropTable` stays at defaults. Out of this wave's scope (the JSON owns the numbers; just no SO mirror yet).

## Worked example — Carrot Boomerang (levels 1..5)

| L | damage | fireRate (s) | range | projectiles |
|---|---|---|---|---|
| 1 | 1.20 | 1.000 | 5.0 | 1 |
| 2 | 1.38 | 1.000 | 5.0 | 2 (+1 perk) |
| 3 | 1.62 | 1.000 | 5.0 | 2 |
| 4 | 1.86 | 0.800 (perk) | 5.0 | 2 |
| 5 | 2.22 | 0.800 | 5.0 | 2 |

L3 `dmg_pct +0.20` and L5 `range_pct_and_pierce` are runtime-mechanic perks — applied by upgrade system, not baked into the level row. Static row stays formula-true.

## Worked example — HP (minute-1 anchor)

| Enemy | role | baseHP | moveSpeed (u/s) | telegraphSec |
|---|---|---|---|---|
| hop-slime | Swarmer | 6 | 4.5 × 1.10 = 4.95 | 0 |
| sleepy-boar | Tank | 80 | 4.5 × 0.60 = 2.70 | 0.400 |
| big-onion | Elite | 300 | 4.5 × 0.70 = 3.15 | 0.600 |
| old-boar-king | (blocker — see above) | 2000 (hp_mid_boss) | 4.5 × 0.50 = 2.25 | 0.800 |

Spawner applies §9 per-minute slope at runtime; SO is minute-1 only.

## Files touched

- `unity/Assets/_Brave/Code/Boot/Editor/BalanceJsonImporter.cs` — `ImportWeapons` + `ImportEnemies` extended; added `ApplyEnumByJsonString` + `NormalizeEnum` helpers. **No other files modified.** JSON, schemas, `WeaponDefinition.cs`, `EnemyDefinition.cs`, `WeaponLevelData.cs`, `EnemyRole.cs` all untouched per scope.

## Re-runnable

Menu **Brave > Generate Balance SOs from JSON** — same entry point.

## What gameplay-engineer should verify

1. Run **Brave > Generate Balance SOs from JSON**.
2. Inspect `Assets/_Brave/Data/Balance/Weapon_carrot-boomerang.asset` — `levels[0].damage == 1.2`, `levels[4].damage == 2.22`, `levels[3].fireRate == 0.8`.
3. Inspect `Enemy_hop-slime.asset` — `baseHP == 6`, `moveSpeed ≈ 4.95`. `Enemy_old-boar-king.asset` — role wrong (Swarmer default) until ADR/code fix lands.

## Blocked? Partially — see blockers above. Importer is done; SO/enum extensions are out of domain.
