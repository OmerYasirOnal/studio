# Meadow — Arena Layout

> Owner: level-designer. Defines the physical arena for the Meadow biome (vertical-slice ship biome). Sister docs: `02-gdd/06-biomes.md` (Meadow theme + mood + enemy variant set), `07-art-bible/04-environment-style.md` (4 u tile grid, 16 u chunks, hero-prop rule), `07-art-bible/02-lighting.md` (Meadow noon lighting spec), `01-color-palette.md` (grass `#7CC95F` etc.), `waves.json` (spawn schedule keyed against this layout).

All distances in **Unity units (u)** matching the 4 u tile grid from the environment-style doc. 60 fps is the frame anchor for any motion timing.

## Arena dimensions

| Param | Value | Notes |
|---|---|---|
| Playable area shape | Circular-ish (square arena with rounded corner dressing) | Reads as "field with a soft edge" |
| Playable area extent | **80 × 80 u** | 20 × 20 tiles, 5 × 5 chunks (chunks are 16 × 16 u) |
| Player camera anchor | World center; player always at origin | World scrolls under player, not vice versa |
| Visible region per frame | ~36 × 36 u (3 × 3 chunks around hero) | Per environment-style doc |
| Outer soft boundary | Dense-bush ring at radius ~42 u | Prevents off-grid wandering; cosmetic-only |
| Outer hard boundary | Invisible collider at radius **45 u** | Player can't physically pass; enemies despawn beyond 50 u |

## Tile palette (from art-bible 04)

| Tile type | Source | Coverage |
|---|---|---|
| Grass-base | Kenney Nature Kit (recolored to `#7CC95F`) | ~65% of ground |
| Grass-variant (slightly darker `#6FB854`) | Kenney Nature Kit recolor | ~25% (breaks tiling repeat) |
| Dirt-path (warm `#A37344`) | Kenney Nature Kit | ~8% (cosmetic paths around hero props) |
| Flower-cluster (mixed reds/yellows) | Quaternius Nature recolor | ~2% (accent decals, not standalone tiles) |

Modular grid spec: **4 × 4 u tiles**, **16 × 16 u authoring chunks** (4 × 4 tiles per chunk). Meadow arena = 25 chunks total (5 × 5). Per environment-style doc: chunks are pre-merged in Blender into a single mesh per material atlas, then re-instanced in Unity.

## Spawner positions

8 spawn rings positioned around the player's central anchor. Spawners are **invisible** game objects parented to the world-scroll root, which keeps them at fixed radii from the player as the world scrolls.

| ID | Direction | Radius from player (u) | Notes |
|---|---|---|---|
| SP_N  | North      | 35 | Primary ring spawner |
| SP_E  | East       | 35 | Primary ring spawner |
| SP_S  | South      | 35 | Primary ring spawner |
| SP_W  | West       | 35 | Primary ring spawner |
| SP_NE | Northeast  | 40 | Corner spawner (slightly further to hide spawn pop) |
| SP_SE | Southeast  | 40 | Corner spawner |
| SP_SW | Southwest  | 40 | Corner spawner |
| SP_NW | Northwest  | 40 | Corner spawner |

**Spawn radii constraint**: 30 u min (just outside camera reveal radius of 8 u + safety buffer), 40 u max (still inside the 45 u hard boundary). Enemies spawning at 35-40 u traverse 22-32 u to reach the player at standard movespeed.

Spawner activation is **time-keyed** via `waves.json`. Each spawn entry references the spawn pattern (`ring` = all 8 spawners; `stream` = one cardinal spawner; `flank` = two adjacent spawners; `scatter` = random angular positions on the ring).

## Hero props

Per art-bible hero-prop rule: **1-3 named hero props per chunk**, recognizable silhouettes that anchor "I'm in the Meadow."

For Meadow arena, 3 hero props are placed at fixed offsets from the player anchor. They are static and parented to the world-scroll root, so they appear to drift past as the player moves. **None block traversal** — they are visual landmarks only (no collider).

| Hero prop | World offset from anchor (u) | Chunk | Function |
|---|---|---|---|
| **Lone tree** | (+18, +12) — northeast | Chunk (1, 1) | Silhouette anchor for "Meadow" reads at 1-second glance |
| **Wooden well** | (-16, -14) — southwest | Chunk (-1, -1) | Visual landmark; bunny references it in idle animation |
| **Mushroom cluster** | (+14, -2) — center-east | Chunk (1, 0) | Mid-arena focal point; useful for player positional orientation |

**Note on traversal**: hero props are **decorative-only** in Meadow per the calibration-biome rule (`06-biomes.md`: "Meadow has no hazards; balance-engineer treats Meadow TTK as the 1.0 anchor"). No collision, no buff/debuff zones, no destructibles in Meadow. Beach onward layers in interactive props (per `06-biomes.md` per-biome interactive props).

## Decals

Per art-bible decal budget: **0-2 decals per chunk**, max 18 active on-screen.

Meadow uses:
- **Scuff marks** — 3-5 per visible chunk, randomly placed within the chunk's interior 12 × 12 u (margin of 2 u from chunk edges). Persistent (baked into the chunk merge).
- **Flower scatter** — 2-3 small flower-cluster decals per chunk, randomly placed. Cosmetic only.
- **Hero footprint** — NOT used in Meadow (per art-bible: "Hero footprint dynamic decals — Snow biome only").

Total active decals on visible 9-chunk region: ~15 (within the 18 cap).

## Hazards

**NONE.** Meadow is the **calibration biome** per `06-biomes.md`. New players never die to a hazard here — only to enemies. Balance-engineer treats Meadow TTK as the 1.0 anchor for every other biome.

No sand traps, no root snares, no stalactite drops, no ice slides, no cold-tick patches. Meadow is **the cleanest possible arena** so the player can learn the auto-attack contract, the draft cadence, and the spawn-pattern vocabulary without distraction.

## Lighting

Per art-bible 02-lighting Meadow noon spec (refer to that doc for canonical numbers; below is the level-design-side summary):

- **Key light**: warm-noon sun, directional, intensity ~1.2, tinted to grass-key `#FFE9A0`.
- **Fill light**: soft sky bounce, intensity ~0.4, tinted to sky-cool `#C8DEF0`.
- **Rim light**: minimal in Meadow (no dramatic silhouette needed).
- **Shadow strategy**: short, soft shadows (sun is high). Hero prop shadows are baked into the chunk merge where possible.

## Skybox

Per art-bible 04-environment-style spec:
- **Base**: solid sky-soft `#9BD6F2`.
- **Gradient band**: sky → ground horizon blend, sky-soft `#9BD6F2` at top → meadow-lime `#A8D87E` at horizon line.
- **No clouds** (standard biome rule).
- **No stars** (Cavern only).
- **Total DC**: 1-2.

## Camera

Already established in art-bible. Restated here for cross-doc clarity:

| Param | Value |
|---|---|
| Mode | Top-down 3/4 (slight tilt) |
| FOV | 35° |
| Distance from player | 18 u |
| Pitch | -55° (looking down from above) |
| Yaw | Fixed at world-north (no rotation per player input) |

The 36 × 36 u visible region at this camera setup means the player sees ~9 chunks at once, matching the art-bible streaming spec.

## Boundary handling

The arena is **bounded but not visibly walled**. The player anchor stays at world center; the world scrolls under them.

- **Outer dense-bush ring** at radius 42 u: cosmetic "fence" using bush props from the Kenney Nature Kit. Reads as a natural meadow edge, not a level boundary.
- **Hard collider** at radius 45 u: invisible cylinder, prevents player from physically wandering further (mostly impossible anyway since the world scrolls, but a safety net).
- **Enemy despawn** at radius 50 u: any enemy that drifts beyond 50 u (e.g., a kited tank that overshoots) returns to pool. Prevents off-screen ghosts from accumulating against the perf cap.

The player **never sees the edge** under normal play. The bush ring is camouflaged into the arena dressing; the hard collider is invisible. If a player somehow pushes outward (e.g., persistent forward joystick), the dense-bush ring appears as "the meadow gets bushier near the edge" — never as a wall.

## Cross-references

- Wave schedule for this arena: `waves.json` (sibling file).
- Wave field schema: `waves.schema.md` (sibling file).
- Boss arena (Old Boar King at 7:00): `02-bosses/old-boar-king/arena.md`.
- Pacing curve this arena instantiates: `00-pacing-model.md`.
- Tile/prop source pipeline: `assets-raw/kenney/`, `assets-raw/quaternius/`, blender-tech owns the merge.
