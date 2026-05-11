# Balance data

JSON files here are the **source of truth** for all gameplay tuning. Gameplay-engineer reads these into ScriptableObjects at edit time. No magic numbers in scripts.

Each `*.json` has a sibling `*.schema.md` documenting fields, units, and acceptable ranges.

| File | Owner | Schema |
|---|---|---|
| `characters.json` | balance-engineer | `characters.schema.md` |
| `weapons.json` | balance-engineer | `weapons.schema.md` |
| `enemies.json` | balance-engineer | `enemies.schema.md` |
| `xp-curve.json` | balance-engineer | `xp-curve.schema.md` |
| `drops.json` | balance-engineer | `drops.schema.md` |
| `economy.json` | balance-engineer | `economy.schema.md` |
