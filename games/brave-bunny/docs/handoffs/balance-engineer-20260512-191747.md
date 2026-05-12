# Hand-off — balance-engineer — 2026-05-12 19:17:47

**Wave:** Wave 2 (orchestrator-dispatched)
**Task:** Extend `BalanceJsonImporter.ImportCharacters` so `CharacterStats` (`baseStats.*`) sub-fields are populated from `data/balance/characters.json`. Unblocks gameplay-engineer's `PlayerMover.Awake` guard (was tripping on `baseMoveSpeed == 0`).

## What was mapped

All formulas come from `docs/10-balance/00-formulas.md` (§1, §2, §3, §4).

| SO field (`CharacterDefinition.baseStats.*`) | JSON expression | Doc ref |
|---|---|---|
| `baseHP` | `character.hp_base` | schema: absolute L1 HP, [50,250] |
| `baseMoveSpeed` | `base_move_units_per_sec × character.move_mult` | §3 (Bunny 4.5 × 1.0 = 4.5) |
| `damageMultiplier` | `character.dmg_mult` | §1 |
| `critRate` | `character.crit_rate` | §2 |
| `critDamage` | `character.crit_damage` | §2 |
| `magnetMultiplier` | `character.magnet_mult` | §4 |
| `xpGemValueBonus` | `character.xp_gem_value_bonus` | schema |

Round-trip check — Bunny: hp 100, move 4.5, dmg 1.0, crit 0.05/1.0, magnet 1.0, gemBonus 0.0. Fox: move = 4.5 × 1.15 = 5.175. Tortoise: 4.5 × 0.7 = 3.15. All match formula doc.

## Intentionally NOT mapped

- **`base_magnet_units`** — formula §4 keeps this on the magnet/pickup system, not in `CharacterStats`. Only the per-character `magnet_mult` lives in `baseStats`.
- **`per_level_perks.*`** — per-level deltas, applied by progression at runtime; not part of L1 baseline.
- **`signature` sub-object** — `CharacterDefinition.signatureMechanicTypeName` is the only signature field on the SO; pre-existing line in importer uses wrong field name (`signatureTypeName`) and JSON has no `signature_token` key. Left untouched (signature wiring is gameplay-engineer/tech-architect territory; ADR-0009 mechanic registry).
- **`default_starter_weapon`** — `WeaponDefinition?` reference resolution by slug requires AssetDatabase lookup; defer to a follow-up wave once weapon SO names are stable.

## Files touched

- `unity/Assets/_Brave/Code/Boot/Editor/BalanceJsonImporter.cs` — `ImportCharacters` extended (+~30 lines). No other files modified. JSON, schemas, and `CharacterDefinition.cs` untouched per scope.

## Re-runnable

Menu **Brave > Generate Balance SOs from JSON** (existing `[MenuItem]` on `GenerateAll`) — no new wiring needed; the existing entry point calls `ImportCharacters`.

## What gameplay-engineer's next test should verify

1. Run **Brave > Generate Balance SOs from JSON** in Editor.
2. Inspect `Assets/_Brave/Data/Balance/Char_bunny.asset` — `baseStats.baseMoveSpeed == 4.5`, `baseHP == 100`.
3. `PlayerMover.Awake` should no longer early-out; PlayMode hero moves at 4.5 u/s.
4. Spot-check Fox (5.175) and Tortoise (3.15) as ratio sanity.

## Blocked? No.
