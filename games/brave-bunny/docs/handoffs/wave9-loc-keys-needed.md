# Wave 9 — Loc Keys Needed (6 new weapons)

**From:** balance-engineer (Wave 9)
**To:** loc-agent
**Date:** 2026-05-16
**Files to update:** `unity/Assets/_Brave/Localization/en.json`, `unity/Assets/_Brave/Localization/tr.json`

## Context

Wave 9 added 6 base weapons to `data/balance/weapons.json` (12 → 18). The new IDs need EN + TR localization entries following the existing `weapons.<id>.name` / `weapons.<id>.description` pattern.

See `docs/10-balance/wave9-weapons.md` for design notes / cartoon flavor cues to draw on for description copy.

## Keys to add — 12 total (6 weapons × 2 keys each)

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

### Tone reminders

- Cartoon-friendly. Banned: skulls / blood / gore / "weapon" framing.
- "Rascals" / "trouble" is the established euphemism for enemies.
- One short sentence + one playful follow. Reference `weapons.thunder-cloud.description` for cadence.
- TR copy should match Turkish tone of existing entries (cf. `weapons.thunder-cloud` TR entry).

## Verification after loc-agent applies

1. `unity/Assets/_Brave/Localization/en.json` parses cleanly.
2. `unity/Assets/_Brave/Localization/tr.json` parses cleanly.
3. New IDs resolve in-game weapon-pick UI (no `???` fallbacks).
