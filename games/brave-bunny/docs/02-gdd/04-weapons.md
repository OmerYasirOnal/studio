# GDD 04 — Weapons

> The full 12-weapon launch arsenal + 6 passives for Brave Bunny. Vertical slice ships **3 weapons** (Carrot Boomerang, Sunbeam, Daisy Mine). Sister docs: `01-core-loop.md` (auto-attack contract), `03-characters.md` (signature mechanics that modify weapons), `10-balance/` (TTK ladder, DMG numbers).

## Design rules

1. **Every weapon respects the auto-attack contract** in `01-core-loop.md` — no aim, no fire button.
2. **Cartoon flavor only** — banned: skulls, blood, gore, demonic imagery, realistic weapons (guns, knives). Encouraged: food, nature, weather, sparkle, light.
3. **Five levels per weapon** — each level improves at least one stat or adds a meaningful effect. No "level 4: +5% damage" filler.
4. **Half evolve, half don't** — 6 weapons have evolution recipes (the "build-crafting" pillar); 6 are utility/synergy weapons that complete builds.
5. **Synergy tags drive draft offers** — if a player picks 2 weapons sharing a tag, the draft slightly weights toward the third tag-mate (balance-engineer owns weights).

## Stat-unit legend

| Stat | Unit |
|---|---|
| DMG | hit points per hit (Bunny baseline 1.0x) |
| RATE | seconds between fires |
| RANGE | world units |
| PROJECTILES | integer count per fire |

## Weapon archetypes — quick map

| Archetype | Targeting model | Examples |
|---|---|---|
| **Projectile** | Nearest-in-range, auto-fire | Carrot Boomerang, Pebble Sling, Acorn Cannon |
| **Area** | Random target area inside range | Daisy Mine, Thunder Cloud, Cob Mortar |
| **Aura** | Self-centered radius, auto-tick | Honey Aura, Frost Whisper |
| **Summon** | Spawns autonomous unit | Beehive, Tumbleweed |
| **Utility** | Beam/wave from self | Sunbeam |

## Weapon roster — 12 launch weapons

### 1. Carrot Boomerang — `carrot-boomerang` *(vertical slice)*

| Field | Value |
|---|---|
| Slug | `carrot-boomerang` |
| Display name | Carrot Boomerang |
| Archetype | Projectile |
| Targeting | Nearest enemy; arcs back to player |
| Base stats | DMG 1.2 / RATE 1.0 s / RANGE 5.0 units / PROJECTILES 1 |
| Level effects | L2: +1 PROJECTILE / L3: +20% DMG / L4: RATE 0.8 s / L5: +25% RANGE, pierce up to 4 enemies on return arc |
| Evolution recipe | L5 Carrot Boomerang **+** L5 Magnet Charm → **Harvest Cyclone** (massive area boomerang, pulls + damages) |
| Synergy class | Kinetic, Nature |
| Cartoon flavor | A bright-orange carrot spinning end-over-end with a soft "wahoo" SFX on each throw |
| Unlock | Starter weapon — available from run 1, all characters |

### 2. Sunbeam — `sunbeam` *(vertical slice)*

| Field | Value |
|---|---|
| Slug | `sunbeam` |
| Display name | Sunbeam |
| Archetype | Utility (beam) |
| Targeting | Continuous beam from player toward nearest enemy; sweep-lock |
| Base stats | DMG 0.6 / RATE 0.2 s (tick rate) / RANGE 6.0 units / PROJECTILES 1 (beam) |
| Level effects | L2: +25% DMG / L3: beam width 1.5x (multi-hits) / L4: tick RATE 0.15 s / L5: beam reflects off screen edges once |
| Evolution recipe | L5 Sunbeam **+** L5 Crit Charm → **Solar Halo** (orbiting twin beams, 360° coverage) |
| Synergy class | Solar, Beam |
| Cartoon flavor | A warm golden ray with sparkle particles — looks like a Pixar sunbeam, not a laser |
| Unlock | Starter weapon — available from run 1, all characters |

### 3. Daisy Mine — `daisy-mine` *(vertical slice)*

| Field | Value |
|---|---|
| Slug | `daisy-mine` |
| Display name | Daisy Mine |
| Archetype | Area |
| Targeting | Drops at random enemy position within RANGE; arms over 1 s; detonates on enemy contact |
| Base stats | DMG 2.5 / RATE 2.0 s / RANGE 4.0 units / PROJECTILES 1 |
| Level effects | L2: +1 PROJECTILE (drops 2) / L3: arm time 0.5 s / L4: +30% DMG / L5: detonation chains to nearest 3 enemies (chain dmg 0.5x) |
| Evolution recipe | L5 Daisy Mine **+** L5 Damage Charm → **Meadow Bloom** (every detonation grows a 2-unit flower-field DOT for 4 s) |
| Synergy class | Nature, Explosive |
| Cartoon flavor | A puffy white-and-yellow daisy that wobbles, then pops with a flower-petal explosion (no fire, no smoke) |
| Unlock | Starter weapon — available from run 1, all characters |

### 4. Pebble Sling — `pebble-sling`

| Field | Value |
|---|---|
| Slug | `pebble-sling` |
| Display name | Pebble Sling |
| Archetype | Projectile |
| Targeting | Nearest in range |
| Base stats | DMG 0.8 / RATE 0.8 s / RANGE 6.0 units / PROJECTILES 1 |
| Level effects | L2: +1 PROJECTILE / L3: +25% RATE / L4: +1 PROJECTILE / L5: pebbles bounce once between enemies |
| Evolution recipe | L5 Pebble Sling **+** L5 Projectile Charm → **Stone Storm** (6 pebbles per fire, all bounce) |
| Synergy class | Kinetic, Bounce |
| Cartoon flavor | Smooth grey pebbles with a cute slingshot animation on player's arm |
| Unlock | Achievement: "Complete a run with only one weapon equipped" |

### 5. Honey Aura — `honey-aura`

| Field | Value |
|---|---|
| Slug | `honey-aura` |
| Display name | Honey Aura |
| Archetype | Aura |
| Targeting | Self-centered radial DOT |
| Base stats | DMG 0.3/tick / RATE 0.4 s / RANGE 2.5 units / PROJECTILES n/a |
| Level effects | L2: +0.5 unit RANGE / L3: +50% DMG / L4: enemies slowed −15% movespeed inside aura / L5: +1.0 unit RANGE |
| Evolution recipe | L5 Honey Aura **+** L5 HP Charm → **Honey Hug** (aura also heals player for 1 HP per 3 enemies inside per second) |
| Synergy class | Nature, Aura |
| Cartoon flavor | Translucent gold haze with floating honeycomb-hex motes; enemies move like they're wading through syrup |
| Unlock | Character-bound: Panda starts with it equipped |

### 6. Acorn Cannon — `acorn-cannon`

| Field | Value |
|---|---|
| Slug | `acorn-cannon` |
| Display name | Acorn Cannon |
| Archetype | Projectile |
| Targeting | Furthest in range (unusual — anti-tank by design) |
| Base stats | DMG 3.0 / RATE 1.8 s / RANGE 7.0 units / PROJECTILES 1 |
| Level effects | L2: +30% DMG / L3: RATE 1.4 s / L4: pierces 1 enemy / L5: +50% DMG, splash radius 1.0 unit on impact |
| Evolution recipe | L5 Acorn Cannon **+** L5 Crit Charm → **Oak Thunderclap** (huge AOE on impact, 4x DMG on crit hit) |
| Synergy class | Kinetic, Heavy |
| Cartoon flavor | Brown acorn with a tiny cap, thunks loudly on impact, leaves a "thwack" cloud puff |
| Unlock | Character-bound: Badger starts with it equipped |

### 7. Thunder Cloud — `thunder-cloud`

| Field | Value |
|---|---|
| Slug | `thunder-cloud` |
| Display name | Thunder Cloud |
| Archetype | Area |
| Targeting | Spawns at random enemy position, hovers 4 s, zaps 3 enemies inside its 1.5-unit radius |
| Base stats | DMG 1.5/zap / RATE 3.0 s / RANGE 5.0 units / PROJECTILES 1 (cloud) |
| Level effects | L2: +1 zap target / L3: +25% DMG / L4: cloud lifetime 6 s / L5: +1 PROJECTILE (2 clouds) |
| Synergy class | Solar (electrical sub-tag) |
| Cartoon flavor | Pillowy blue cloud with a tiny scowl; lightning is sparkle-yellow, no jagged scary forks |
| Unlock | Battle Pass Season 1 Tier 12 |
| Evolution recipe | None (utility weapon — does not evolve) |

### 8. Frost Whisper — `frost-whisper`

| Field | Value |
|---|---|
| Slug | `frost-whisper` |
| Display name | Frost Whisper |
| Archetype | Aura |
| Targeting | Self-centered slow + light DOT |
| Base stats | DMG 0.1/tick / RATE 0.5 s / RANGE 3.0 units / PROJECTILES n/a |
| Level effects | L2: slow −10% → −25% / L3: +0.5 unit RANGE / L4: enemies inside take +15% DMG from all sources (frostbite debuff) / L5: +0.5 unit RANGE |
| Synergy class | Frost, Aura |
| Cartoon flavor | Pale blue mist with snowflake particles; enemies wear a cute icy-blush tint when slowed |
| Unlock | Biome-bound: unlocks after first run in Frost Burrow |
| Evolution recipe | None (utility weapon) |

### 9. Cob Mortar — `cob-mortar`

| Field | Value |
|---|---|
| Slug | `cob-mortar` |
| Display name | Cob Mortar |
| Archetype | Area |
| Targeting | Lobs an arcing cob at random screen position, 1.2 s travel time |
| Base stats | DMG 4.0 / RATE 2.5 s / RANGE 8.0 units / PROJECTILES 1 |
| Level effects | L2: splash radius 1.5 → 2.0 units / L3: +30% DMG / L4: RATE 1.8 s / L5: +1 PROJECTILE (2 cobs per fire) |
| Evolution recipe | L5 Cob Mortar **+** L5 Damage Charm → **Cornfield Volley** (3 cobs per fire, each spawns a 2-second mini-DOT field) |
| Synergy class | Nature, Explosive |
| Cartoon flavor | A yellow corn-cob spinning end-over-end, lands with a popcorn-shower visual (no fire) |
| Unlock | Character-bound: Bunny achievement "Reach Lv 10 with Bunny" |

### 10. Beehive — `beehive`

| Field | Value |
|---|---|
| Slug | `beehive` |
| Display name | Beehive |
| Archetype | Summon |
| Targeting | Spawns 3 autonomous bees that orbit the player, dive at nearest enemy on cooldown |
| Base stats | DMG 0.5/bee / RATE 0.6 s (bee dive cooldown) / RANGE 4.0 units / PROJECTILES 3 (bees) |
| Level effects | L2: +1 bee / L3: +25% DMG per bee / L4: bees apply 0.5-second DOT on hit / L5: +1 bee (5 total) |
| Synergy class | Nature, Summon |
| Cartoon flavor | Bumblebees with stripey friendly faces; the hive is a wooden cube hovering above the player |
| Unlock | Character-bound: Badger Lv 5 OR achievement "Maintain 3 minions for 60 s" |
| Evolution recipe | None (utility/summon) |

### 11. Tumbleweed — `tumbleweed`

| Field | Value |
|---|---|
| Slug | `tumbleweed` |
| Display name | Tumbleweed |
| Archetype | Summon |
| Targeting | Spawns a tumbleweed that rolls in a random direction for 4 s, damaging on contact |
| Base stats | DMG 1.0/contact-tick / RATE 2.0 s / RANGE n/a (lifetime-based) / PROJECTILES 1 |
| Level effects | L2: lifetime 6 s / L3: +1 PROJECTILE / L4: contact-tick RATE doubled / L5: +1 PROJECTILE (3 tumbleweeds simultaneously) |
| Synergy class | Nature, Kinetic |
| Cartoon flavor | Spherical brown bramble with a tiny smile; rolls with a charming wobble; never harms allies |
| Unlock | Limited event "Tumbleweed Rodeo" (free) OR Battle Pass Season 2 Tier 20 |
| Evolution recipe | None (utility/summon) |

### 12. Whirligig — `whirligig`

| Field | Value |
|---|---|
| Slug | `whirligig` |
| Display name | Whirligig |
| Archetype | Projectile |
| Targeting | Orbits the player at fixed radius, damages on contact |
| Base stats | DMG 0.7/contact-tick / RATE 0.3 s (orbit-tick) / RANGE 2.0 units (orbit radius) / PROJECTILES 2 |
| Level effects | L2: +1 PROJECTILE / L3: +25% DMG / L4: orbit RANGE 3.0 units / L5: +1 PROJECTILE (4 whirligigs) |
| Evolution recipe | L5 Whirligig **+** L5 Magnet Charm → **Pinwheel Storm** (8 whirligigs at varying radii) |
| Synergy class | Mech, Kinetic |
| Cartoon flavor | Bright paper-pinwheel-style toys; child-friendly mechanical aesthetic |
| Unlock | Character-bound: Owl starts with it equipped |

## Evolution recipes — summary

The 6 evolving weapons:

| Base weapon (L5) | + Passive (L5) | = Evolved weapon | Effect headline |
|---|---|---|---|
| Carrot Boomerang | Magnet Charm | **Harvest Cyclone** | Giant boomerang pulls + damages |
| Sunbeam | Crit Charm | **Solar Halo** | Orbiting twin beams, 360° coverage |
| Daisy Mine | Damage Charm | **Meadow Bloom** | Detonations grow DOT flower-fields |
| Pebble Sling | Projectile Charm | **Stone Storm** | 6 bouncing pebbles per fire |
| Acorn Cannon | Crit Charm | **Oak Thunderclap** | Huge AOE, 4x DMG on crit |
| Cob Mortar | Damage Charm | **Cornfield Volley** | 3 cobs each spawning DOT fields |

The 6 non-evolving weapons (utility, summon, character-bound) fill out builds without competing for evolution slots — keeps the build-crafting puzzle from becoming "always go for the evolutions."

## Passive items — 6 launch passives

Passives offer in the draft pool alongside weapons. Each has 5 levels.

| Slug | Display name | Effect per level | Evolution role |
|---|---|---|---|
| `magnet-charm` | Magnet Charm | +20% pickup magnet radius/level | Ingredient: Harvest Cyclone, Pinwheel Storm |
| `hp-charm` | Hearty Charm | +15% max HP/level | Ingredient: Honey Hug |
| `regen-charm` | Mossy Charm | +0.5 HP/sec regen per level | No evolution |
| `damage-charm` | Damage Charm | +10% global DMG/level | Ingredient: Meadow Bloom, Cornfield Volley |
| `crit-charm` | Lucky Charm | +5% crit chance/level | Ingredient: Solar Halo, Oak Thunderclap |
| `projectile-charm` | Splash Charm | +1 projectile (every 2 levels: L2, L4) per level on all projectile weapons | Ingredient: Stone Storm |

## Synergy class adjacency table

The synergy class tags are how balance-engineer weights draft offers. Two weapons sharing a tag bias the draft toward the third tag-mate.

| Tag | Members |
|---|---|
| Kinetic | Carrot Boomerang, Pebble Sling, Acorn Cannon, Tumbleweed, Whirligig |
| Nature | Carrot Boomerang, Honey Aura, Cob Mortar, Beehive, Tumbleweed, Daisy Mine |
| Solar | Sunbeam, Thunder Cloud |
| Frost | Frost Whisper |
| Aura | Honey Aura, Frost Whisper |
| Summon | Beehive, Tumbleweed |
| Mech | Whirligig |
| Explosive | Cob Mortar, Daisy Mine |
| Bounce | Pebble Sling |
| Beam | Sunbeam |
| Heavy | Acorn Cannon |

The dense Kinetic and Nature tags reward common cross-recipes; the sparse Mech and Frost tags reward niche/late-game commitment.

## Cross-references

- Auto-attack contract (RANGE, RATE, target priority) sourced from `01-core-loop.md`.
- Per-weapon raw DMG numbers tuned in `data/balance/weapons.json` (balance-engineer owns).
- Character-bound weapons cross-link to `03-characters.md` signature fields.
- Cartoon flavor enforces `00-overview.md` family-safe rule + `narrative/` banned-words list.
- VFX specs (particle counts, biome tinting) deferred to `07-art-bible/`.
