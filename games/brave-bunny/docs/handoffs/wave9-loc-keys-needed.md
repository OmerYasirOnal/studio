# Wave 9 — Localization keys needed (evolved weapons)

**From:** gameplay-engineer (Wave 9 — Weapon Evolution)
**To:** narrative-designer / ui-engineer
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
