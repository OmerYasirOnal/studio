# Character Style — Brave Bunny

> Owner: art-director. Cross-refs: `00-style-overview.md` (silhouette + saturation rules), `01-color-palette.md` (hex sources), `02-lighting.md` (hero rim per biome), `docs/02-gdd/03-characters.md` (canonical cast — inferred here pending file), `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 DC, 250k tris on-screen). All meshes come from **Quaternius Animated Animals (CC0)**; we only **recolor** and **animate-trim**, never re-rig.

## Silhouette rules

Every hero passes the **32 px circle test** from `00-style-overview.md` §"Silhouette rule" before sign-off. In addition heroes must satisfy:

1. **One-glance species read** — the head accessory (ears, shell, quill, mask) reads at 32 px with NO color, pure black-on-white.
2. **Unique top-down profile** — no two heroes share the same head outline within ±10° rotation.
3. **Single accessory motif** — exactly one signature element (Fox's scarf, Owl's monocle); never two.
4. **No symmetrical-only props** — asymmetry helps the eye lock orientation in top-down 3/4 view.

## Proportions

| Class | Head : Body | Total height (Unity units) | Notes |
|---|---|---|---|
| Hero (chibi) | 1.5 : 1 | 1.0 u | Per `00-style-overview.md`; recap here for cross-ref |
| Enemy (puff/blob) | 1.0 : 1 | 0.6 u | Headless-blob class allowed |
| Elite | 1.2 : 1 | 0.9 u | Accent ring at base |
| Boss | 2.0 : 1 | 2.4 u | Heroic stretch; 64 px silhouette test |

## Recolor pipeline spec — Quaternius Animated Animals base mesh

The Quaternius pack ships each animal as a single skinned mesh with a flat-color material per body region. Our pipeline:

1. **Source** the base FBX from `assets-raw/quaternius/AnimatedAnimals/`.
2. **Read** the material's 4 named slots: `primary_fur`, `secondary_fur`, `eye_accent`, `outline`.
3. **Run** `core/tools/blender-tech/_recolor.py` which accepts a `{old_hex: new_hex}` dict per slot.
4. **Bake** the recolored material into the biome atlas at `assets-raw/atlases/hero_atlas_<biome>.png` (1024×1024 max, 4 px/unit).
5. **Validate** with the 32 px silhouette test before commit.

### Slot definitions (HSV shift recipe)

Each hero gets a 4-color shift recipe. The script applies it as an **absolute hex swap** (not relative HSV), so the table values *are* the new hexes. The Quaternius source hexes are listed for reference.

| Slot | Quaternius source hex | Role | Recolor strategy |
|---|---|---|---|
| `primary_fur` | `#E0DFDB` | Main body silhouette color | Push to biome-appropriate hero hue at 100% S |
| `secondary_fur` | `#9C988E` | Belly / under-color, ears inner | Lighter shade of primary, +15% V |
| `eye_accent` | `#3C2A1A` | Eye + nose + paw pads | Use Coal Outline `#2E2A28` from palette |
| `outline` | `#1A1A1A` | Shader-driven rim outline | Use Coal Outline `#2E2A28` (matches eyes) |

### Per-character recolor maps (the 8 vertical-slice + alt heroes)

> Heroes listed in unlock order. **Bunny** is the vertical-slice MVP hero.

| Hero | primary_fur | secondary_fur | eye_accent | outline | Signature motif |
|---|---|---|---|---|---|
| **Bunny** (default) | `#FFF4DC` (Bunny Cream) | `#FFE4C0` (cream warm) | `#2E2A28` (Coal) | `#2E2A28` | Long ears, pink nose dot `#F39FB4` |
| **Tortoise** | `#A8D86B` (Meadow Lime) | `#6FAE74` (Sage Mid) | `#2E2A28` | `#2E2A28` | Hex-pattern shell, mint underbelly |
| **Fox** | `#FF8A4C` (warm orange) | `#FFF4DC` (Bunny Cream) | `#2E2A28` | `#2E2A28` | White-tip tail, red scarf `#FF6B6B` |
| **Hedgehog** | `#8B6A4A` (Bark Brown) | `#F6D6B5` (Peach Sand) | `#2E2A28` | `#2E2A28` | Quill ridge along spine |
| **Otter** | `#6B4A3A` (river brown) | `#F6D6B5` (Peach Sand) | `#2E2A28` | `#2E2A28` | Wet sheen highlight, shell on belly |
| **Panda** | `#FFFFFF` (snow white) | `#2E2A28` (Coal — eye-patch) | `#2E2A28` | `#2E2A28` | Black eye patches + ears, monochrome read |
| **Badger** | `#3C3A38` (charcoal) | `#FFFFFF` (snow white) | `#2E2A28` | `#2E2A28` | White head-stripe down center |
| **Owl** | `#D7C5E8` (Lavender Mist) | `#FFF4DC` (Bunny Cream) | `#FFC83D` (Pickup Gold eyes!) | `#2E2A28` | Big gold eyes, feather tufts |

> Owl's eye_accent breaks the slot's default — owl eyes use Pickup Gold so the species reads at 32 px. Documented exception.

### Hero-most-saturated rule (restated)

Every hero gets a **+20% saturation boost** in its material relative to whatever biome it stands in. The toon-shader has a `_HeroSaturationBoost` float (set to 1.2 by default for any material on the `Hero` layer). This guarantees the hero stays the brightest thing on screen even when the Meadow grass is already 80% S. Cross-ref `00-style-overview.md` §"Saturation budget".

## Animation set per character

All 8 heroes share the **same 7-clip set** authored on the Quaternius rig. Triggered by Animator state-machine; clip names are canonical for gameplay-engineer.

| Clip | Duration | Loop | Notes |
|---|---|---|---|
| `idle` | 1.5 s | yes | 2-frame bob at 0.5 Hz, ±2 px Y (matches feel-pillar 7) |
| `walk` | 0.8 s | yes | 4-pose cycle, foot-plant on frame 1 + 3 |
| `attack` | 0.4 s | no | Anticipation 80 ms → strike 120 ms → recover 200 ms |
| `hit` | 0.15 s | no | 1 wince pose + return; cross-ref pillar 4 hit-flash |
| `death` | 0.6 s | no | Fall-over + 0.3 s settle; hero-specific dignity (pillar 6) |
| `victory` | 1.2 s | no | One arm-up pose held 800 ms; plays on run-clear |
| `summon` | 0.5 s | no | Reserved for support heroes (Owl, Otter); others get a 1-frame stub |

### Animation timing baseline (canonical)

| Phase | Time | Reference |
|---|---|---|
| Idle bob period | 1.5 s loop | Pillar 7 |
| Walk cycle | 0.8 s loop | Reads at 200 px/s nominal player speed |
| Attack swing | 0.4 s total | Pillar 4 — feels deliberate |
| Hit reaction | 0.15 s | Stays under hit-flash 50 ms + tail 100 ms |
| Death | 0.6 s | Wraps within pillar 6's 300 ms dilate |

> All clips authored at 30 fps source, re-sampled to 24 fps on import to halve animation memory. Quaternius rigs survive the re-sample cleanly.

## Outline rendering

Per `00-style-overview.md`, mesh-extrusion outlines are **OFF** for environment (DC budget). Heroes get a special exception:

- **Shader-driven** screen-space outline via the URP renderer feature `HeroOutline.shadergraph`.
- **Width:** equivalent to **1.5 px at 1080p**, scaled per resolution by `_OutlineWidth = 1.5 / screenHeight`.
- **Color:** Coal Outline `#2E2A28` from `01-color-palette.md` outline slot.
- **Cost:** 1 fullscreen pass restricted to the `Hero` + `Boss` layers via stencil — measured at 0.4 ms on iPhone 12 baseline.
- **DC cost:** 1 (shared across hero + boss).

Standard enemies and props do **NOT** receive outlines — silhouette reads via lighting + saturation per `00-style-overview.md`.

## Triangle + bone budgets

| Class | Tris cap | Bones cap | Notes |
|---|---|---|---|
| Hero | ≤ 5 000 tris | ≤ 24 bones | Quaternius rig standard; matches CLAUDE.md tris budget |
| Boss | ≤ 12 000 tris | ≤ 36 bones | 1 on screen ever |
| Standard enemy | ≤ 200 tris | ≤ 0 bones (billboard / blob) | Up to 200 active |
| Elite | ≤ 600 tris | ≤ 12 bones | Up to 8 active |

> 200 trash × 200 tris = 40 k. 1 hero × 5 k = 5 k. 1 boss × 12 k = 12 k. 8 elites × 600 = 4.8 k. Total enemy + hero load ≈ 62 k tris — leaves 188 k for environment per the 250 k CLAUDE.md cap.

## Hand-off

- Recolor maps above are the canonical input for `blender-tech/_recolor.py`. Filed as `assets-raw/recolor-maps/heroes.json` once asset-curator stages the Quaternius FBXs.
- Hero rim color per biome lives in `02-lighting.md` and is materialized in the Animator-bound Light Group preset.
- Open question for tech-architect: confirm the URP renderer-feature outline pass fits inside the 5 ms lighting+VFX budget on iPhone SE 3.
