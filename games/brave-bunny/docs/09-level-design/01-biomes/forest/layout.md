# Forest — Arena Layout

> Owner: level-designer. Defines the physical arena for the Forest biome (biome 3 of 5). Sister docs: `02-gdd/06-biomes.md` (Forest theme + root-snare + low-vision underbrush), `02-gdd/narrative/03-biome-flavor.md` (dappled-shade mood), `07-art-bible/04-environment-style.md` (4 u tile grid, 16 u chunks, hero-prop rule), `01-color-palette.md` (canopy-shade `#3F6B3A` etc.), `waves.json` (spawn schedule keyed against this layout), `../meadow/layout.md` (reference shape).

All distances in **Unity units (u)**. 60 fps frame anchor.

## Arena dimensions

| Param | Value | Notes |
|---|---|---|
| Playable area shape | Slightly **oval** (N/S long, E/W short) | Reads as "glade pinched between two old oaks" |
| Playable area extent | **70 × 90 u** (E/W × N/S) | 17 × 22 tiles, ~24 chunks. Oval mask in shader fades corners. |
| Player camera anchor | World center; player always at origin | World scrolls under player |
| Visible region per frame | ~36 × 36 u | Per environment-style doc |
| Outer soft boundary | Dense oak-trunk ring at radius ~38 u (E/W) / ~48 u (N/S) | Cosmetic; the oval shape reads as natural |
| Outer hard boundary | Invisible ovaloid: 40 u E/W, 50 u N/S | Enemies despawn 5 u beyond |

The oval shape means cardinal-N/S spawners are 5 u further out than E/W — this asymmetric framing biases the player's "long lines of sight" axis and pairs with the dappled-light tiles to reinforce the "deep grove" read.

## Tile palette (from art-bible 04)

| Tile type | Source | Coverage |
|---|---|---|
| Canopy-shade green (`#3F6B3A`) | Kenney Nature Kit recolor | ~40% — dappled-shade tiles |
| Sun-dapple green (`#8FBE5A`) | Kenney recolor | ~30% — bright patches between shadow |
| Forest-floor brown (`#4A3A24`) | Kenney recolor | ~15% — exposed root/dirt zones |
| Mossy stone (`#5C7050`) | Quaternius recolor | ~5% — accent boulders |
| Mushroom-cap orange (`#D87B36`) | Quaternius accent decals | ~3% — accent decals, not standalone tiles |
| Root-snare patch (`#3A2818`, vine knot) | Custom mat | ~5% — hazard tiles, see Hazards |
| Underbrush fern decals | Quaternius | ~2% — decorative + reveal-radius hazard markers |

Higher contrast than Meadow per `06-biomes.md` (dappled shadow tiles are core).

## Spawner positions

8 spawn rings; oval-shape adjustments mean N/S spawners sit further out.

| ID | Direction | Radius (u) | Notes |
|---|---|---|---|
| SP_N  | North      | 42 | Long-axis spawner |
| SP_E  | East       | 33 | Short-axis spawner |
| SP_S  | South      | 42 | Long-axis spawner |
| SP_W  | West       | 33 | Short-axis spawner |
| SP_NE | Northeast  | 38 | Corner (oval-interp) |
| SP_SE | Southeast  | 38 | Corner |
| SP_SW | Southwest  | 38 | Corner |
| SP_NW | Northwest  | 38 | Corner |

Spawn radii constraint: 30 u min, 42 u max. The oval bias means players who hug the long axis (N/S) get marginally more spawn time-to-impact than those who hug E/W — a subtle positional teach.

## Hero props

3 hero props per arena per art-bible.

| Hero prop | World offset from anchor (u) | Chunk | Function |
|---|---|---|---|
| **Ancient oak** | (0, +24) — north long-axis | Chunk (0, 2) | Dominant silhouette; canopy partially shadows surrounding tiles |
| **Log bridge** | (-16, -4) — west center | Chunk (-1, 0) | Cosmetic walk-over; reads as "passage." No traversal effect (Forest has no elevation). |
| **Mushroom ring** | (+14, -12) — southeast | Chunk (1, -1) | **Interactive cosmetic**: +5% pickup-magnet zone inside the ring (1.5 u radius). Visual pulse when bunny is inside. Lightest possible biome-interactive prop per `06-biomes.md`. |

**Traversal**: all three are decorative-only (no colliders). Mushroom ring has an invisible trigger volume for the magnet buff.

## Decals

- **Dappled-shadow decals** — 4-6 per visible chunk, baked into chunk merge. Static. These are *the look* of the Forest.
- **Fern underbrush patches** — 2-3 per visible chunk, dynamic-trigger (see Hazards). They're both decoration and hazard markers.
- **Acorn scatter** — 1-2 per visible chunk, cosmetic only, static.

Total active decals on visible 9-chunk region: ~16-17 (within 18 cap).

## Hazards

Forest introduces **two new hazards** (the only biome that introduces two; the second is cosmetic-mechanical).

### Root-snare patches

| Param | Value |
|---|---|
| Visible footprint | 0.8 u radius, knot-of-vines art |
| Effect | Roots player for **0.4 s** on first contact (cannot move; auto-attack still fires) |
| Telegraph | Vines ripple 0.5 s before activation |
| Active count | 1-3 simultaneous patches on visible region |
| Cooldown | 5 s per patch after firing; patch goes dormant (visible-but-inert) then re-arms |
| Damage | None directly; root sets up enemy follow-up |
| First spawn | t=60 (build-phase) — Forest is the first biome where the player can be hit by a hazard during normal play |

### Low-vision underbrush

| Param | Value |
|---|---|
| Visible footprint | 2.0 u radius fern patch |
| Effect | Player reveal radius drops from 8.0 u → 5.5 u while standing inside |
| Telegraph | None (patches are visible as tall fern) |
| Active count | 2-4 patches on visible region; stationary, persistent |
| Damage | None — purely a visibility penalty |

The two hazards stack: a player standing inside a fern patch with reduced reveal *and* a snare-pop nearby has a real positional puzzle to solve. The intended teach is "step out of cover before drafting."

Boss-arena variant: ambient root-snares **suppressed during Mama Oak fight** (boss owns snares this fight); fern patches stay.

## Lighting

Per art-bible 02-lighting Forest dappled spec:

- **Key light**: filtered sun shafts through canopy, directional at ~60° altitude, intensity ~0.9 (lower than Meadow — canopy filter), tinted to dapple-warm `#FFE9A0`.
- **Fill light**: green bounce from canopy, intensity ~0.45, tinted to leaf-green `#8FBE5A`.
- **Rim light**: subtle, comes from upward bounce on the bunny's underside.
- **Shadow strategy**: complex dappled-shadow patterns baked into chunk merge. **No real-time shadow casters from canopy** (perf — baked-in only). Bunny + enemy shadows are dynamic blob-shadows.

## Skybox

- **Base**: muted blue with a green tint where the canopy meets sky, `#A8C9F0` → `#6B9A4A` at the canopy-mask edge.
- **Canopy-mask plane**: a top-down quad just above camera frustum, textured with leafy-edge alpha. Reads as "you're under the canopy" without modeling individual leaves.
- **No clouds** (canopy masks them).
- **Total DC**: 2 (sky + canopy mask).

## Camera

Same as Meadow (35° FOV, 18 u distance, -55° pitch, fixed yaw). The dappled-shadow tiles read clearly at this pitch.

## Boundary handling

- **Dense oak-trunk ring** at radius 38 u (E/W) / 48 u (N/S): cosmetic; reads as "the trees thicken at the glade's edge."
- **Hard collider** at ovaloid (40 / 50 u): invisible.
- **Enemy despawn** at +5 u beyond the hard collider.

Player never sees the edge; oak trunks at the boundary look like the same density of trunk as the interior, just thicker spacing.

## Boss-arena delta (Mama Oak at t=420)

When the boss spawns:
- **Mama Oak takes center position** (offset 0, 0). Her trunk has a 3.0 u radius collider — the bunny cannot stand on top of her, must circle.
- Ambient root-snare patches **despawn** at t=418 (1.5 s before boss entrance).
- The two mushroom-ring props **persist** and give the player +5% pickup-magnet refuge zones during the fight (per `06-biomes.md` Forest arena suggestion in `07-bosses.md`).
- 4 fixed root-snare patches spawn at boss-call (Mama Oak's phase 2/3 mechanic, see `02-bosses/mama-oak/mechanics.md`).

## Cross-references

- Wave schedule: `waves.json` (sibling).
- Boss spec: `02-bosses/mama-oak/mechanics.md`.
- Pacing curve: `00-pacing-model.md` (Forest modifier: softer pre-boss taper, low-vision adds cognitive load).
- Hazard tuning numbers: `data/balance/biomes.json`.
- Tile/prop source: `assets-raw/kenney/`, `assets-raw/quaternius/`.
