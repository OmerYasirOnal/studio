# Snow — Arena Layout

> Owner: level-designer. Defines the physical arena for the Snow biome (biome 5 of 5 — launch endgame). Sister docs: `02-gdd/06-biomes.md` (Snow theme + ice-slides + cold-tick + snowdrift), `02-gdd/narrative/03-biome-flavor.md` (overcast morning mood), `07-art-bible/04-environment-style.md` (4 u tile grid, 16 u chunks, hero-prop rule), `01-color-palette.md` (snow white `#F4F8FA` etc.), `waves.json` (spawn schedule), `../meadow/layout.md` (reference shape).

All distances in **Unity units (u)**. 60 fps frame anchor.

## Arena dimensions

| Param | Value | Notes |
|---|---|---|
| Playable area shape | Square arena, open plain | Reads as "wide winter field" |
| Playable area extent | **90 × 90 u** | 22 × 22 tiles, ~36 chunks. **Largest arena in the game** — open by design. |
| Player camera anchor | World center; player always at origin | World scrolls under player |
| Visible region per frame | ~36 × 36 u | Per environment-style doc |
| Player reveal radius | **9.0 u** (vs 8.0 u standard) | Slight bonus — overcast bright biome gives extended sight lines |
| Outer soft boundary | Pine-tree ring at radius ~46 u | Reads as "the forest re-thickens at the field edge" |
| Outer hard boundary | Invisible collider at radius **50 u** | Enemies despawn beyond 55 u |

Snow is the **open inverse of Cavern**: longer sight lines, larger arena, more visual breathing room. The challenge isn't claustrophobia — it's compounding hazards (slowdown + drift + DOT) across a wider field where the player can't take cover by retreating to a corner. Per `06-biomes.md`: density rebalanced +10% to compensate for the "feels lower" effect of long sight lines.

## Tile palette (from art-bible 04)

| Tile type | Source | Coverage |
|---|---|---|
| Snow white (`#F4F8FA`) | Kenney Nature Kit recolor | ~50% — base ground |
| Shadow-blue snow (`#B5C6D8`) | Kenney recolor | ~20% — accent shadows under props |
| Ice-slide patch (`#9FE0E8`, glossy) | Custom mat (gloss + slight emissive) | ~10% — hazard tiles |
| Snowdrift patch (`#E0E8EE`, packed snow) | Custom mat | ~5% — hazard tiles (thicker variant of Beach sand-trap) |
| Pine-shadow green (`#3F6B5C`) | Kenney recolor | ~6% — local shadow tiles under pines |
| Frozen-pond cyan accent (`#A8E0E8`) | Custom mat | ~4% — fixed cosmetic pond near center |
| Igloo-pack-snow (`#D6DCE0`) | Custom mat | ~5% — local tiles around igloo prop |

Brightest overall lightness in the game per `06-biomes.md` — overcast-bright, no harsh sun, no nighttime. The hero footprint dynamic decal (per art-bible "Snow biome only" rule) is enabled here.

## Spawner positions

8 spawners, **further out** than other biomes because the arena is bigger.

| ID | Direction | Radius (u) | Notes |
|---|---|---|---|
| SP_N  | North      | 42 | Primary ring |
| SP_E  | East       | 42 | Primary ring |
| SP_S  | South      | 42 | Primary ring |
| SP_W  | West       | 42 | Primary ring |
| SP_NE | Northeast  | 46 | Corner |
| SP_SE | Southeast  | 46 | Corner |
| SP_SW | Southwest  | 46 | Corner |
| SP_NW | Northwest  | 46 | Corner |

Spawn radii constraint: 38 u min, 48 u max. With reveal radius at 9.0 u, enemies are still off-screen at spawn — player gets ~5 s before contact, slightly longer than Meadow because spawns are further out, but ice-slide drift eats reaction time.

## Hero props

3 hero props per art-bible.

| Hero prop | World offset from anchor (u) | Chunk | Function |
|---|---|---|---|
| **Pine tree** | (+18, +16) — northeast | Chunk (1, 1) | Silhouette anchor; **2 s cold-tick suppression aura** on exit (per `06-biomes.md` pine shelter rule); 3 u suppression radius |
| **Ice formation** | (-18, +12) — northwest | Chunk (-1, 1) | Decorative-only — visual reward, no shelter. Cosmetic crystal-shimmer. |
| **Igloo** | (-14, -16) — southwest | Chunk (-1, -1) | **First sheltering prop in the game**: 4 s cold-tick suppression on exit (per `06-biomes.md`); 3 u suppression radius; critical during Big Snow-yeti's blizzard-howl |

Additionally, a fixed **frozen pond patch** at (+12, -10) — cosmetic ice with permanent 0.4 s drift, always-active (regardless of ice-slide hazard spawns). Pond is decorative landmark + drift teaching surface.

**Traversal**: pine + igloo have no colliders (player can path through). Ice formation has a soft collider (decoration that blocks). Frozen pond has no collider but the ice-drift effect.

## Decals

- **Hero footprint trail** — bunny leaves paw-prints (Snow-biome-only per art-bible 04). Dynamic decal pool, 12 footprints, fade over 4.0 s. Adds to mood + tracks player path.
- **Snowflake-fall ambient** — VFX particles, 8-12 active. Pure mood.
- **Ice-shimmer decals** — 2-3 per visible chunk over ice-slide patches, static.

Total active decals on visible 9-chunk region: ~17-18 (at the cap — Snow is the heaviest decal biome).

## Hazards

Snow combines all prior hazard *classes* (slowdown, drift, DOT, prop-suppression) — it's the **graduation biome.**

### Ice slides

| Param | Value |
|---|---|
| Visible footprint | Large patches of glossy ice tile (variable shape: 2-3 u diameter) |
| Effect | Player movement has **0.4 s drift** after joystick release (vs 0.0 s on grass/sand/stone) |
| Telegraph | None — patches are visible as different ground texture |
| Active count | 3-5 patches visible at once, persistent (don't despawn) |
| Damage | None — pure positional uncertainty |

### Cold-tick (open-patch DOT)

| Param | Value |
|---|---|
| Effect | 2 hp/sec DOT in any tile not within 3.0 u of pine/igloo/campfire prop |
| Suppression | Within 3.0 u of pine = 2 s suppression on exit; within 3.0 u of igloo = 4 s suppression |
| Telegraph | None — biome-ambient; UI shows a small frost-tint border when player is in the DOT |
| Damage | 2 hp/sec (continuous), 4 hp/sec during Big Snow-yeti's phase-3 blizzard-howl |
| First active | t=60 (turns on after calm-intro; the first 60 s is hazard-free to let the player orient) |

### Snowdrift

| Param | Value |
|---|---|
| Visible footprint | 1.2 u radius patch, packed-snow texture (thicker variant of Beach sand-trap) |
| Effect | Movespeed × 0.55 inside |
| Telegraph | Slow-falling-snow VFX 0.5 s before patch settles |
| Active count | 1-2 patches at a time |
| Damage | None — movement penalty |

Boss-arena variant: **all hazards remain active** during Big Snow-yeti fight (only fight where ambient + boss hazards stack fully). Yeti adds his own ice patches via Ice-stomp + amplifies cold-tick via blizzard-howl.

## Lighting

Per art-bible 02-lighting Snow overcast spec:

- **Key light**: diffuse overcast directional, intensity ~0.85, tinted to overcast-cool `#E0E8EE`.
- **Fill light**: bounce from snow below, intensity ~0.6 (highest fill in any biome — snow is reflective), tinted snow-warm `#FFFCF0`.
- **Rim light**: subtle cyan rim, intensity ~0.2, tinted ice-cyan `#9FE0E8`.
- **Shadow strategy**: soft, low-contrast shadows (overcast diffuses everything). Pine + igloo shadows are baked. Bunny shadow is dynamic but softer than Meadow.

Snow is the **highest-lightness** biome (overall scene brightness) and the **lowest-contrast** (overcast diffuses harshness). It's the visual opposite of Cavern.

## Skybox

- **Base**: solid overcast-sky `#C5CDD2`.
- **Gradient band**: sky → ground transition is very subtle (overcast doesn't have a clean horizon line); `#C5CDD2` at top → `#D8DEE4` at horizon.
- **Falling snow particle layer** at the camera frustum top — drifts down across the visible region. ~30 particles active.
- **No stars** (overcast).
- **No sun disc** (cloud cover).
- **Total DC**: 2 (sky + snow-particle layer).

## Camera

Same setup as Meadow (35° FOV, 18 u distance, -55° pitch, fixed yaw). The longer sight lines work in-engine because reveal radius is 9.0 u and the snowflake particles add depth cue.

## Boundary handling

- **Pine-tree ring** at radius 46 u: cosmetic; reads as "the field meets the forest beyond."
- **Hard collider** at radius 50 u.
- **Enemy despawn** at radius 55 u.

The wider boundary means kited enemies have more distance to despawn — important for tank/elite kiting strategies. Boundary uses the same camouflage rule as other biomes: player never sees a wall.

## Boss-arena delta (Big Snow-yeti at t=420)

When the boss spawns:
- Arena identical (no size change, props persist).
- **Igloo prop is critical** — player must use it for cold-tick suppression during phase 3 blizzard-howl. UI hints at this with a small "shelter" pip near the igloo silhouette.
- Snowdrift hazards **freeze in fixed positions** (no respawn) at: (+10, +6) and (-8, +10). Always-active.
- Ice-slide patches **persist** and the yeti adds more via Ice-stomp attacks (4 new fixed slides per stomp-quake in phase 3).
- Cold-tick rate amplifies to 4.0 hp/sec during phase-3 blizzard-howl (boss attack, 4 s duration).

## Cross-references

- Wave schedule: `waves.json` (sibling).
- Boss spec: `02-bosses/big-snow-yeti/mechanics.md`.
- Pacing curve: `00-pacing-model.md` (Snow modifier: +10% spawn count because long sight lines make density feel lower at same count).
- Hazard tuning numbers: `data/balance/biomes.json`.
- Tile/prop source: `assets-raw/kenney/`, `assets-raw/quaternius/`, plus custom igloo + ice-formation Blender.
