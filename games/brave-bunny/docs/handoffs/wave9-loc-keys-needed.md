# Wave 9 — Loc Keys Needed (handoff to loc-agent)

**From:** Wave 9 parallel agents (weapons, enemies, daily-rewards, quests, battle-pass, shop, evolutions)
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`

## A — 6 new base weapons (12 keys)

Wave 9 added 6 base weapons to `data/balance/weapons.json` (12 → 18). See `docs/10-balance/wave9-weapons.md` for design notes / cartoon flavor cues.

### English suggested copy (draft — loc-agent owns final wording)

```json
"weapons.storm-cloud.name": "Storm Cloud",
"weapons.storm-cloud.description": "An angrier cousin of the thunder cloud — four zaps and a longer fuse.",

"weapons.sapling-summon.name": "Sapling Summon",
"weapons.sapling-summon.description": "A friendly sprout that plants itself and pokes at rascals nearby.",

"weapons.maple-boomerang.name": "Maple Boomerang",
"weapons.maple-boomerang.description": "A spinning maple leaf — slices two rascals in a row on the way back.",

"weapons.sunflower-beam.name": "Sunflower Beam",
"weapons.sunflower-beam.description": "A focused golden ray from a tall sunflower that follows the trouble.",

"weapons.cherry-bomb.name": "Cherry Bomb",
"weapons.cherry-bomb.description": "Tossed cherry that pops with a sweet little splash.",

"weapons.wasp-swarm.name": "Wasp Swarm",
"weapons.wasp-swarm.description": "Three buzzy friends that circle Bunny and dive whenever trouble gets close."
```

## B — 14 new enemies (Beach + Cavern biomes, 14 keys)

Wave 9 added 14 enemy entries to `data/balance/enemies.json`. Each needs an `enemies.<id>.name` key.

| Key | English |
|---|---|
| `enemies.crab.name`              | Crab |
| `enemies.gull.name`              | Gull |
| `enemies.sand-puff.name`         | Sand-Puff |
| `enemies.mosquito.name`          | Mosquito |
| `enemies.bog-boar.name`          | Bog Boar |
| `enemies.throw-frog.name`        | Throw Frog |
| `enemies.big-hermit-crab.name`   | Big Hermit Crab |
| `enemies.bat-mini.name`          | Bat-Mini |
| `enemies.glow-bug.name`          | Glow-Bug |
| `enemies.rock-tumble.name`       | Rock-Tumble |
| `enemies.cave-slime.name`        | Cave-Slime |
| `enemies.stone-ox.name`          | Stone Ox |
| `enemies.crystal-slinger.name`   | Crystal Slinger |
| `enemies.stalagmite-walker.name` | Stalagmite Walker |

### Notes for translator (`tr.json`)

- Cartoon-friendly. Banned: skulls / blood / gore / "weapon" framing. "Rascals" / "trouble" is the established euphemism for enemies.
- One short sentence + one playful follow. Reference `weapons.thunder-cloud.description` for cadence.
- Beach is "wee beasties" register; Cavern is "troublemakers" register (`docs/02-gdd/narrative/00-tone-bible.md`).
- Diminutives (`sand-puff`, `bat-mini`, `glow-bug`, `cave-slime`) — preserve affectionate feel (suffixes `-cik`/`-ciğim` appropriate).
- `Big Hermit Crab` → `Koca Keşiş Yengeç` (per `big-onion` → `Koca Soğan` pattern).

## C — Other Wave 9 features (loc agent fills additional keys)

The localization agent (W9.8) ships additional keys for:
- Daily rewards (`daily.*`)
- Quests (`quest.*`, `quest.<type>.title`)
- Battle pass (`battlepass.*`)
- Shop (`shop.*`)
- Biomes (`biomes.*`)
- Evolution toast (`evolution.consume_toast`, evolved weapon names)

## Verification after loc-agent applies

1. `unity/Assets/_Brave/Localization/en.json` parses cleanly.
2. `unity/Assets/_Brave/Localization/tr.json` parses cleanly.
3. EN/TR parity preserved.
4. New IDs resolve in-game UI (no `???` fallbacks).

## Cross-refs

- `data/balance/weapons.json` (18 base weapons)
- `data/balance/enemies.json` (Beach + Cavern roster)
- `Code/UI/Theming/LocalizationProvider.cs` — loads via `Resources.Load<TextAsset>` / file path
- `docs/02-gdd/narrative/00-tone-bible.md` — tone register
