# Balance tools

Python helpers that bridge JSON balance sheets and Unity ScriptableObjects.

## Direction of truth

`games/<active>/data/balance/*.json` is the **source of truth**. Tools here generate / validate ScriptableObject `.asset` files in `games/<active>/unity/Assets/Data/Balance/` from those JSONs.

## Scripts

| File | Purpose |
|---|---|
| `validate_balance.py` | Lint JSONs against their sibling `.schema.md` |
| `dump_dps.py` | Print DPS tables from `weapons.json` (for cross-checking with `docs/10-balance/03-weapon-tuning.md`) |
| `make_so_stubs.py` | Generate `.asset` stubs in Unity from JSON — gameplay-engineer runs this after balance-engineer edits |

These are stubs ready to be filled when Phase 3 (tech architecture) confirms the ScriptableObject hierarchy.
