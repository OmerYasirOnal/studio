# Beach — Arena Layout

> Owner: level-designer. Defines the physical arena for the Beach biome (biome 2 of 5). Sister docs: `02-gdd/06-biomes.md` (Beach theme + hazards + enemy variant set), `02-gdd/narrative/03-biome-flavor.md` (golden-hour mood), `07-art-bible/04-environment-style.md` (4 u tile grid, 16 u chunks, hero-prop rule), `01-color-palette.md` (sand `#F2D89B` etc.), `waves.json` (spawn schedule keyed against this layout), `../meadow/layout.md` (reference shape).

All distances in **Unity units (u)** matching the 4 u tile grid. 60 fps is the frame anchor for any motion timing.

## Arena dimensions

| Param | Value | Notes |
|---|---|---|
| Playable area shape | Square arena, ocean-edge framed on one side | Reads as "shore with a soft inland edge" |
| Playable area extent | **80 × 80 u** | 20 × 20 tiles, 5 × 5 chunks |
| Player camera anchor | World center; player always at origin | World scrolls under player |
| Visible region per frame | ~36 × 36 u (3 × 3 chunks around hero) | Per environment-style doc |
| Outer soft boundary | Dune ring (north/east/west) + gentle wave-lap (south) at radius ~42 u | Reads as natural coastline |
| Outer hard boundary | Invisible collider at radius **45 u** | Enemies despawn beyond 50 u |

Beach matches Meadow's 80 × 80 footprint so the player's first hazard biome doesn't compound spatial unfamiliarity on top of the new sand-trap mechanic.

## Tile palette (from art-bible 04)

| Tile type | Source | Coverage |
|---|---|---|
| Dry sand (`#F2D89B`) | Kenney Nature Kit recolor | ~55% of ground |
| Wet sand (`#D6B97A`) | Kenney Nature Kit recolor | ~20% (south wave-lap edge + scatter accents) |
| Sand-trap patch (`#B89A60`, darker, sparkle decal) | Custom mat (Blender atlas slot) | ~6% (hazard tiles, see Hazards) |
| Shell/pebble decals (mixed) | Quaternius nature recolor | ~4% (cosmetic decal only) |
| Palm-shade dirt (`#A89370`) | Kenney recolor | ~15% (rings under palm hero props) |

Modular grid spec: **4 × 4 u tiles**, **16 × 16 u authoring chunks**. Beach arena = 25 chunks total.

## Spawner positions

8 spawn rings, identical placement to Meadow's. Crab swarmers sidle laterally so the **flank** and **scatter** patterns load harder than the Meadow equivalent at the same spawner count.

| ID | Direction | Radius (u) | Notes |
|---|---|---|---|
| SP_N  | North      | 35 | Primary ring |
| SP_E  | East       | 35 | Primary ring |
| SP_S  | South      | 35 | Primary ring (wave-lap side; gulls favored here) |
| SP_W  | West       | 35 | Primary ring |
| SP_NE | Northeast  | 40 | Corner |
| SP_SE | Southeast  | 40 | Corner |
| SP_SW | Southwest  | 40 | Corner |
| SP_NW | Northwest  | 40 | Corner |

Spawn radii constraint: 30 u min, 40 u max. Crab swarmers walk laterally — spawn-direction is decoupled from initial-motion-direction (a crab spawning from north may sidle east-then-south).

## Hero props

3 hero props per arena per the art-bible hero-prop rule.

| Hero prop | World offset from anchor (u) | Chunk | Function |
|---|---|---|---|
| **Palm tree** | (+20, +10) — northeast | Chunk (1, 1) | Silhouette anchor for "Beach" reads at 1-second glance; casts long golden-hour shadow westward |
| **Thatched hut** | (-18, -12) — southwest | Chunk (-1, -1) | Visual landmark; doorway opening hints at "shelter" without being one mechanically |
| **Coconut pile** | (+14, -4) — center-east | Chunk (1, 0) | Mid-arena focal point. **Destructible-cosmetic**: player melee breaks the pile and drops 1 Carrot pickup. No collider once broken. |

**Traversal**: palm + hut have no collider (decorative); coconut pile has a soft collider until broken. Player can path between all three.

## Decals

Per art-bible decal budget: 0-2 decals per chunk, max 18 active on-screen.

- **Footprint trail** — bunny leaves shallow paw-prints in dry sand (dynamic decals, 8-decal pool, fade over 2.0 s).
- **Shell scatter** — 1-2 per visible chunk, static, baked into chunk merge.
- **Sparkle ring** — dynamic VFX decal for sand-trap activation telegraph (see Hazards).

Total active decals on visible 9-chunk region: ~16 (within the 18 cap, leaving 2 headroom for sparkle rings).

## Hazards

**Sand-trap patches** are the single new hazard introduced in Beach (per `06-biomes.md` teach-one-thing rule).

| Param | Value |
|---|---|
| Visible footprint | 1.0 u radius circle, darker sand tile + faint sparkle on activation |
| Effect | Player movespeed × 0.65 while inside (enemies unaffected — they are biome-natives) |
| Telegraph | Sparkle-ring VFX decal pulses 0.4 s before patch becomes active |
| Active count on-screen | 1-2 (never block traversal — always a path around) |
| Cooldown | Patch goes dormant for 6 s after deactivation, then re-activates at a new random position within the 36 × 36 u visible region |
| Damage | **None** — pure movement penalty, no DOT |
| First spawn | t=120 (per pacing model build-phase; sand-traps don't appear during the calm-intro) |

Boss-arena variant: during Crab Captain fight, sand-traps are **always-active** (no telegraph; the patches just sit there). The boss is immune to his own sand.

## Lighting

Per art-bible 02-lighting Beach golden-hour spec:

- **Key light**: warm golden directional sun at ~25° altitude, intensity ~1.1, tinted to gold-key `#FFD27A`.
- **Fill light**: warm bounce from sand below, intensity ~0.5, tinted to sand-warm `#F2D89B`.
- **Rim light**: long horizontal rim from the west (sun-side), intensity ~0.3, tinted to peach `#FFB18A` — gives hero silhouettes a soft glow.
- **Shadow strategy**: long, soft shadows angled east-northeast (sun is low west). Palm-shadows are baked into the chunk merge; bunny shadow is dynamic.

## Skybox

- **Base**: warm gradient — peach `#FFB18A` at horizon → soft cyan `#7BC4D6` at zenith.
- **No clouds** (standard biome rule), but a faint cirrus accent at the horizon band is permitted (decorative, single quad).
- **Ocean band** at south edge: animated wave-lap texture, 0.5 m wide visual strip behind the soft boundary. Cosmetic-only.
- **Total DC**: 2-3 (1 sky + 1 ocean + optional 1 cirrus accent).

## Camera

Same as Meadow (35° FOV, 18 u distance, -55° pitch, fixed yaw). The golden-hour rim catches the bunny's silhouette from the west; no per-biome camera tweak.

## Boundary handling

The arena is **bounded but not visibly walled.**

- **Dune ring** (north/east/west) at radius 42 u: cosmetic dune-mound props from Kenney Nature Kit, reads as "the beach gets sandier as you wander inland."
- **Wave-lap strip** (south) at radius 42 u: animated ocean texture + foam VFX. Reads as "the tide is in." Player never crosses it.
- **Hard collider** at radius 45 u: invisible cylinder.
- **Enemy despawn** at radius 50 u.

The player never sees the edge under normal play. Crabs sidle along the soft boundary, occasionally re-entering — this is intended skitter flavor.

## Boss-arena delta (Crab Captain at t=420)

When the boss spawns:
- Coconut pile is **guaranteed already broken** (a script ensures this — if it's still standing at t=418, it crumbles automatically with a small dust puff).
- 2 sand-trap patches lock in fixed positions: (+8, +5) and (-8, -6), always-active throughout the fight.
- Bonus prop spawns: a small **bottle-cap** decal at the boss position (cosmetic tone reference to Crab Captain's hat per `07-bosses.md`).

## Cross-references

- Wave schedule: `waves.json` (sibling).
- Boss spec: `02-bosses/crab-captain/mechanics.md`.
- Pacing curve: `00-pacing-model.md` (Beach modifier: +5% swarmer count, sand-puff minion class during boss).
- Tile/prop source: `assets-raw/kenney/`, `assets-raw/quaternius/`.
- Hazard tuning numbers: `data/balance/biomes.json` (balance-engineer).
