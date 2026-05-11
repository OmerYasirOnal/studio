# Cavern — Arena Layout

> Owner: level-designer. Defines the physical arena for the Cavern biome (biome 4 of 5). Sister docs: `02-gdd/06-biomes.md` (Cavern theme + stalactite + reduced reveal), `02-gdd/narrative/03-biome-flavor.md` (basement-flashlight mood), `07-art-bible/04-environment-style.md` (4 u tile grid, 16 u chunks, hero-prop rule), `01-color-palette.md` (cavern floor `#4A4550` etc.), `waves.json` (spawn schedule), `../meadow/layout.md` (reference shape).

All distances in **Unity units (u)**. 60 fps frame anchor.

## Arena dimensions

| Param | Value | Notes |
|---|---|---|
| Playable area shape | Square arena, tighter feel | Reads as "stone chamber" |
| Playable area extent | **60 × 60 u** | 15 × 15 tiles, ~16 chunks. **Smallest arena in the game** — claustrophobic by design. |
| Player camera anchor | World center; player always at origin | World scrolls under player |
| Visible region per frame | ~36 × 36 u | Per environment-style doc |
| Player reveal radius | **6.5 u** (vs 8.0 u standard) | Per `06-biomes.md` — Cavern's defining reveal modifier |
| Outer soft boundary | Stone-wall ring at radius ~28 u | Cosmetic stalactite cluster fringe |
| Outer hard boundary | Invisible collider at radius **30 u** | Enemies despawn beyond 35 u |

The smaller arena + reduced reveal radius compound: in Meadow the player sees the arena edge during retreat; in Cavern they don't, but they hit the hard collider sooner. Net effect — the cavern *feels* much smaller than its 60 × 60 dimension, which is the intended claustrophobia.

## Tile palette (from art-bible 04)

| Tile type | Source | Coverage |
|---|---|---|
| Cavern floor stone (`#4A4550`) | Kenney recolor | ~55% — base ground |
| Wet stone variant (`#3A3540`) | Kenney recolor | ~20% — slightly darker, breaks tiling |
| Glow-mushroom cyan accent (`#5FE0D6`) | Quaternius recolor | ~5% — small emissive tiles around glow-mushroom prop |
| Gem-outcrop purple (`#8E5BC9`) | Custom Blender | ~3% — emissive accent at the gem outcrop prop |
| Torchlight orange (`#F5A04A`) | Custom emissive | ~4% — local torchlight ground glow under torch props |
| Stalactite-drop zone (`#5A4F60`, dust-marked) | Custom mat | ~6% — hazard tiles |
| Ambient dark (`#2A2632`) | Base material | ~7% — beyond-reveal-radius tiles render at this base color |

Cavern is the **darkest palette** in the game (per `06-biomes.md`) — but no black, no skulls, no spider-web visuals per tone bible.

## Spawner positions

8 spawners, **closer to the player** than other biomes because the arena is smaller and the reveal radius is reduced.

| ID | Direction | Radius (u) | Notes |
|---|---|---|---|
| SP_N  | North      | 22 | Primary ring (tighter than 35 u standard) |
| SP_E  | East       | 22 | Primary ring |
| SP_S  | South      | 22 | Primary ring |
| SP_W  | West       | 22 | Primary ring |
| SP_NE | Northeast  | 25 | Corner |
| SP_SE | Southeast  | 25 | Corner |
| SP_SW | Southwest  | 25 | Corner |
| SP_NW | Northwest  | 25 | Corner |

Spawn radii constraint: 18 u min, 28 u max. With reveal radius at 6.5 u, enemies spawn ~15 u beyond visibility — still gives the player 4-5 seconds to react before contact at standard movespeeds.

## Hero props

3 hero props per art-bible. Cavern props are **closer together** (smaller arena) and several extend the reveal radius locally.

| Hero prop | World offset from anchor (u) | Chunk | Function |
|---|---|---|---|
| **Stalactite cluster** | (0, +14) — north | Chunk (0, 1) | Vertical silhouette anchor; **does not drop** (purely decorative cluster, not a stalactite-drop hazard zone) |
| **Glow-mushroom patch** | (-12, -6) — southwest | Chunk (-1, 0) | **Interactive cosmetic**: pulse-glow when bunny within 2 u (no mechanical effect — pure mood) |
| **Gem outcrop** | (+10, -8) — southeast | Chunk (1, 0) | **Destructible**: player melee breaks the outcrop, drops 1 Carrot pickup + emits 2 s of +3 u reveal-radius extension VFX (the gems briefly light the chamber) |

Additionally, **4 torch props** at fixed corner positions (NE/SE/SW/NW at ~14 u offset). Torches are not "hero" props — they're functional: each extends local reveal radius from 6.5 u to 8.0 u within 4.0 u of the torch. The player learns to path between torches to maintain visibility.

**Traversal**: stalactite cluster has a soft collider (player path around it). Glow-mushroom and torches have no collider. Gem outcrop has a soft collider until broken.

## Decals

- **Dust motes** — VFX particle decals, 5-8 active in visible region, drift slowly. Pure mood.
- **Crack lines on stone** — 2-3 per visible chunk, static, baked into chunk merge.
- **Stalactite-drop dust telegraph** — dynamic decal that appears 0.8 s before a stalactite drop (see Hazards).

Total active decals on visible 9-chunk region: ~15-17 (within 18 cap).

## Hazards

**Stalactite drop zones** — Cavern's defining hazard, **first hazard with a damage value**.

| Param | Value |
|---|---|
| Visible footprint | 1.2 u radius circle on the ground, telegraphed by falling-dust VFX |
| Effect | 25 hp damage + 0.3 s stagger on hit |
| Telegraph | Dust-falling VFX **0.8 s** before impact (largest telegraph window of any non-boss hazard) |
| Active count | Never more than 2 simultaneous |
| Cooldown | After drop, zone goes dormant 4 s, then a new zone spawns at a new random position |
| First spawn | t=120 (build-phase) |

**Reduced reveal radius** is documented in arena-dimensions above; not a discrete hazard in waves.json but a constant biome modifier.

Boss-arena variant: during Sneaky Cave Mole fight, ambient stalactite zones **stay active** — the mole boss riffs on the mechanic in his phase 2 stalactite-shake attack (which fires its own additional 3-zone burst). Total stalactite count can briefly spike to 5 active during phase 2.

## Lighting

Per art-bible 02-lighting Cavern torchlight spec:

- **Key light**: dim overhead ambient, intensity ~0.35, tinted to cavern-ambient `#3A3540` (very low — the whole biome reads dim).
- **Fill light**: minimal, intensity ~0.1.
- **Point lights**: 4 torch-props are point-light sources, intensity ~1.2 each, range 6 u, tinted torchlight-orange `#F5A04A`. These are the primary illumination source.
- **Glow-mushroom**: emissive material, no light component (cosmetic glow only — perf).
- **Rim light**: subtle cool rim from above, tinted glow-mushroom-cyan `#5FE0D6`, gives enemies a faint edge that helps reads at the edge of reveal radius.
- **Shadow strategy**: torches cast soft circular pool-shadows on the floor. Stalactite-cluster casts a long downward shadow from its overhead light contribution. Dynamic shadows for hero only; enemies use blob shadows.

## Skybox

- **Base**: solid cavern-ambient dark `#2A2632`. **No gradient** (you're underground).
- **Ceiling band**: top of the camera frustum is masked with a faint stalactite-silhouette texture. Reads as "rocky ceiling above," not as sky.
- **No stars** (the ceiling is rock; per `06-biomes.md` Cavern is the only no-sky biome).
- **Total DC**: 1 (single dark ambient material; ceiling mask is part of the chunk merge).

## Camera

Same setup as Meadow (35° FOV, 18 u distance, -55° pitch, fixed yaw). However: **camera vignette** is enabled in Cavern at 0.6 intensity, tinted ambient-dark. This combined with the 6.5 u reveal radius is what produces the "flashlight" feel.

## Boundary handling

- **Stone-wall ring** at radius 28 u: cosmetic stalactite/stone-wall mesh.
- **Hard collider** at radius 30 u.
- **Enemy despawn** at radius 35 u (tighter buffer because the arena is smaller).

Player feels the wall closer than other biomes — intentional. The cavern is supposed to feel cramped.

## Boss-arena delta (Sneaky Cave Mole at t=420)

When the boss spawns:
- Arena identical (no size change, props persist).
- **Torch props become brighter** — point-light intensity jumps from 1.2 → 1.6 for the duration of the boss fight (the mole carries a faint glow that catches on the chamber).
- Ambient stalactite hazards **remain active** (the mole's phase 2 stalactite-shake adds its own).
- Mole boss spawns at (0, 0) for the entrance animation, then immediately burrows to begin his pop-up rotation (see `02-bosses/sneaky-cave-mole/mechanics.md`).
- 4 telegraphed dig-mound markers spawn at fixed positions: (±8, ±8). Boss surfaces from one of these per dig-strike pattern.

## Cross-references

- Wave schedule: `waves.json` (sibling).
- Boss spec: `02-bosses/sneaky-cave-mole/mechanics.md`.
- Pacing curve: `00-pacing-model.md` (Cavern modifier: -10% density throughout because reveal-radius pressure is already doing work).
- Hazard tuning numbers: `data/balance/biomes.json`.
- Tile/prop source: `assets-raw/kenney/`, `assets-raw/quaternius/`, plus custom gem-outcrop Blender.
