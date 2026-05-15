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

## D — 8 evolution recipes (16 keys + 1 toast template)

**Date:** 2026-05-16

The Wave-9 evolution system swaps in 8 evolved weapons at run-time. UI surfaces
(level-up draft toast, post-evolution flash, run-end recap) need name + description
strings for each evolved id. ADR-0007 also defines a consume-toast string that
needs a generic template.

## Loc keys to add to `narrative/05-localization-keys.md`

For each evolved weapon id, two keys are needed:

| Loc key | Source (English placeholder) | Notes |
|---|---|---|
| `weapons.harvest-cyclone.name` | "Harvest Cyclone" | from `weapons.json` evolutions[] |
| `weapons.harvest-cyclone.description` | "Massive area boomerang pulls + damages." | from `tag_headline` |
| `weapons.solar-halo.name` | "Solar Halo" | |
| `weapons.solar-halo.description` | "Orbiting twin beams give 360 degree coverage." | |
| `weapons.meadow-bloom.name` | "Meadow Bloom" | |
| `weapons.meadow-bloom.description` | "Detonations grow DOT flower fields for 4s." | |
| `weapons.stone-storm.name` | "Stone Storm" | |
| `weapons.stone-storm.description` | "Six bouncing pebbles per fire." | |
| `weapons.honey-hug.name` | "Honey Hug" | |
| `weapons.honey-hug.description` | "Aura also heals 1HP per 3 enemies per second." | |
| `weapons.oak-thunderclap.name` | "Oak Thunderclap" | |
| `weapons.oak-thunderclap.description` | "Huge AOE; 4x DMG on crit." | |
| `weapons.cornfield-volley.name` | "Cornfield Volley" | |
| `weapons.cornfield-volley.description` | "Three cobs each spawning DOT fields." | |
| `weapons.pinwheel-storm.name` | "Pinwheel Storm" | |
| `weapons.pinwheel-storm.description` | "Eight whirligigs at varying radii." | |

## ADR-0007 consume toast

Also need the post-evolution toast string referenced in ADR-0007 §Consequences:

| Loc key | Template (English placeholder) | Notes |
|---|---|---|
| `evolution.consume_toast` | "{charm} consumed — {base} evolved to {evolved}" | Three tokens: `{charm}` (display_name), `{base}` (display_name), `{evolved}` (display_name) |

## Cross-references

- `data/balance/evolutions.json`
- `data/balance/weapons.json` § `evolutions[]`
- `docs/decisions/0007-evolution-charm-consumption.md`
- `unity/Assets/_Brave/Code/Gameplay/Events/WeaponEvolvedEvent.cs` (event payload UI subscribes to)

## E — Daily rewards (10 keys)

The localization engineer (or ui-engineer's loc sub-role) consumes this file
to extend `_Brave/Localization/{lang}.json` tables.

## Daily Rewards (Wave 9 — daily-login calendar)

Source: `_Brave/UI/Documents/DailyRewards.uxml` + `Code/UI/Controllers/DailyRewardsController.cs`.

| Key            | English (fallback)        | Notes                                  |
|----------------|---------------------------|----------------------------------------|
| `daily.title`  | Daily rewards             | Modal title                            |
| `daily.day_1`  | Day 1                     | Reward cell label                      |
| `daily.day_2`  | Day 2                     | Reward cell label                      |
| `daily.day_3`  | Day 3                     | Reward cell label                      |
| `daily.day_4`  | Day 4                     | Reward cell label                      |
| `daily.day_5`  | Day 5                     | Reward cell label                      |
| `daily.day_6`  | Day 6                     | Reward cell label                      |
| `daily.day_7`  | Day 7                     | Milestone cell label                   |
| `daily.claim`  | Claim today's gift        | CTA button                             |
| `daily.claimed`| Come back tomorrow.       | Post-claim hint label                  |

## F — Quest/mission keys


| Key                            | English string (placeholder)                         |
|--------------------------------|------------------------------------------------------|
| `quest.panel.title`            | Today's quests                                       |
| `quest.claim`                  | Claim                                                |
| `quest.progress`               | Progress                                             |
| `quest.kill_enemies.title`     | Defeat {count} rascals                               |
| `quest.survive_waves.title`    | Survive {count} waves                                |
| `quest.defeat_boss.title`      | Defeat {count} boss                                  |
| `quest.reach_level.title`      | Reach level {count} in a run                         |
| `quest.collect_gold.title`     | Collect {count} carrots in a run                     |
| `quest.run_duration.title`     | Survive {count} seconds                              |

Append additional LiveOps loc keys below as DailyRewards / BattlePass / Shop
agents land their UI.

## G — Battle pass keys

Consumed by: `Code/UI/Controllers/BattlePassController.cs` + `UI/Documents/BattlePass.uxml`.

Append to `_Brave/Localization/{lang}.json` for every supported language. The
controller falls back to the raw key when no entry exists (LocalizationProvider
echo behaviour), so missing translations stay screamingly visible to QA.

## Required keys

| Key                          | English source        | Notes |
|------------------------------|-----------------------|-------|
| `battlepass.title`           | Battle Pass           | Header title. |
| `battlepass.tier_1` … `battlepass.tier_30` | Tier 1 … Tier 30 | Per-tier label. 30 entries. |
| `battlepass.claim`           | Claim                 | Active CTA on cells. |
| `battlepass.claimed`         | Claimed               | Cell state when already taken. |
| `battlepass.locked`          | Locked                | Cell state when tier not reached or premium-gated. |
| `battlepass.row_free`        | Free                  | Free row label. |
| `battlepass.row_premium`     | Premium               | Premium row label. |
| `battlepass.activate_premium`| Activate              | Premium-pass activation CTA. |
| `battlepass.premium_active`  | Premium pass: active  | Premium-strip status text. |
| `battlepass.premium_locked`  | Premium pass: locked  | Premium-strip status text. |

## Generation hint

Tier keys can be bulk-emitted with a one-liner against the master string table
once the translator opens the file:

```jq
range(1;31) | "battlepass.tier_\(.)": "Tier \(.)"
```

## Status

- [ ] EN entries committed
- [ ] TR / ES / PT-BR pass-1 translations
- [ ] QA review of fallback (missing key) behaviour on the tier rail
