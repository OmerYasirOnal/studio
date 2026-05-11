# VFX Style — Brave Bunny

> Owner: art-director. Cross-refs: `00-style-overview.md` (saturation budget, what we don't do), `01-color-palette.md` (accent palette = VFX color source), `02-lighting.md` (hit-flash and impact lighting register), `docs/02-gdd/11-feel-pillars.md` (pillars 1, 2, 4 set the timing), `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 DC, 250k tris; combined lighting+VFX budget 5 ms/frame). All VFX authored in **URP Shader Graph** + **VFX Graph**; **no Shuriken CPU sim** except boss arenas.

## VFX taxonomy

Every effect in the game falls into one of these 7 buckets. If a request doesn't fit, file an ADR before authoring.

### 1. Combat hits

| Effect | Trigger | Spec |
|---|---|---|
| Projectile spawn | Auto-attack fires | 1-shot 4-particle muzzle puff, 80 ms |
| Projectile trail | Per-frame on projectile | GPU trail, 6 segments, biome-accent color, 200 ms tail |
| Impact | Projectile hits enemy | 6-particle radial puff, biome-tinted, 120 ms (pillar 4) |
| Knockback puff | On hit | 4-particle directional puff along projectile vector, 80 ms |

### 2. Pickup

| Effect | Trigger | Spec |
|---|---|---|
| XP gem idle pulse | Gem exists in world | Emissive sine pulse 1 Hz, ±20% intensity, **shader-only** (free) |
| Pickup magnetize | Within radius (1.5 u XP / 2.5 u heart) | Smooth ease-in trail 4 particles, 200 ms |
| Pickup absorb burst | Contact with hero | 4-particle micro-burst, 180 ms (pillar 3) |

### 3. Level-up

| Effect | Trigger | Spec |
|---|---|---|
| Slow-mo | Level threshold | Time-dilate 0.4× for 200 ms (pillar 2) |
| Gold burst | Same frame | 30-particle gold radial, 1.0 u radius, 400 ms (pillar 2) |
| Card slam | UI cards arrive | UI animation — owned by ui-engineer; art delivers card frame |
| Fanfare halo | Behind hero | Hero Highlight `#FF6B6B` radial, 0.5 s outward, bloom spike 0.6 |

### 4. Hero state

| Effect | Trigger | Spec |
|---|---|---|
| Heal | HP regen tick > 0 | 6-particle green sparkle upward, 400 ms |
| Shield | Shield buff active | Persistent shader-only rim, 0.2 cyan tint |
| Dash | Dash skill | Smear trail, 4 ghost-quad afterimages, 150 ms total |
| Hit reaction | Hero damage taken | Damage Red flash 100 ms (pillar 4) + 0.2 u screen-shake |

### 5. Enemy state

| Effect | Trigger | Spec |
|---|---|---|
| Spawn poof | Enemy spawn | 5-particle ground puff, 150 ms |
| Hit flash | Damage tick | White tint to 0.7 alpha, 50 ms (pillar 4) |
| Death dissolve | HP = 0 | 8-particle biome-tinted puff, 300 ms (pillar 1) |
| Elite aura | Elite alive | Persistent accent ring decal, slow rotate 0.5 rps |

### 6. Boss state

| Effect | Trigger | Spec |
|---|---|---|
| Phase-change shockwave | Boss phase boundary | Fullscreen radial wave, 600 ms outward |
| Telegraph radial | Boss windup | Danger Red `#E83C3C` ground decal, fills 800 ms |
| Intro framing | Boss spawn | Slow-zoom 1.0× → 1.05× over 500 ms + name plate |
| Outro framing | Boss death | Full-screen biome-key flash 40% alpha, 600 ms fade (pillar 6 reuse) |

### 7. Environment

| Effect | Trigger | Spec |
|---|---|---|
| Wind / grass sway | Persistent | Vertex shader on instanced grass, 2 s cycle, ±3° (pillar 7) |
| Meadow — pollen | Per chunk | 3-particle drift, 4 s lifetime, 1 per chunk active |
| Beach — sand drift | Per chunk | 4-particle horizontal drift, 3 s |
| Forest — leaf fall | Per chunk | 5-particle downward drift, 5 s |
| Cavern — embers | Per chunk near hot points | 6-particle upward ember, 2 s, coral-tinted |
| Snow — snowflakes | Per chunk | 8-particle downward drift, 6 s |

## Particle authoring rules

| Rule | Value | Reason |
|---|---|---|
| **GPU-instanced quads only** | All particles except boss arenas | CPU sim breaks the 5 ms combined budget at 200-enemy density |
| Max particle systems concurrent in a chunk | **4** | Includes ambient + active combat |
| Total emission budget on screen | **500 particles concurrent** | Hard cap before culling oldest |
| Per-particle cost | ≤ 0.001 ms (GPU quad) | Aggregate stays under 0.5 ms at 500 particles |
| Per-effect runtime cost | ≤ **0.2 ms each** | Combined VFX budget 3.5 ms (5 ms total − 1.5 ms post per `02-lighting.md`) |

## Color rules

VFX inherits from the **accent palette** in `01-color-palette.md`, not the biome environment palette.

| VFX role | Color slot | Hex |
|---|---|---|
| XP gem, gold pickup | Pickup Gold | `#FFC83D` |
| Hero highlight, level-up | Hero Highlight | `#FF6B6B` |
| Biome ambient VFX (pollen, leaves) | Biome prop accent | per biome |
| Rare drop, epic glow | Rare Drop Cyan | `#3DE0E0` |
| Damage to hero, boss telegraph | Danger Red | `#E83C3C` |

## Timing register

> All values are authoritative for VFX Graph timeline keys. Cross-ref `02-lighting.md` impact lighting table and feel-pillars.

| Event | Value | Source |
|---|---|---|
| Hit-flash tint | 50 ms to white, 100 ms restore | Pillar 4 |
| Hitstop on basic | **none** | Pillar 4 |
| Hitstop on elite | 60 ms | Pillar 4 |
| Hitstop on boss damage tick > 5% HP | 120 ms (spec'd here; pillar 4 said 40 — see ADR note) | Brief override |
| Screenshake on trash kill | 2 px, 80 ms ease-out | Pillar 1 |
| Screenshake on elite kill | 4–8 px, 160 ms | Pillar 1 (range to taste-test) |
| Screenshake on boss phase change | 12–16 px, 240 ms | Pillar 1 extended |
| Hero damage screen-shake | 0.2 u (≈ 12 px), 100 ms | Pillar 4 |
| Level-up gold burst lifetime | 400 ms | Pillar 2 |
| Death tally dilate | 0.3×, 300 ms | Pillar 6 |

> NOTE: pillar 4 specs 40 ms boss-tick hitstop; this doc specs 120 ms because the boss is on-screen so rarely we can afford the larger pause for impact. **File ADR-0011 — hitstop reconciliation** for tech-architect + game-designer to decide.

## File formats

| Asset type | Format | Notes |
|---|---|---|
| Particle systems | **URP VFX Graph** (`.vfx`) | GPU sim |
| Particle shaders | **Shader Graph** | All particle materials |
| Trails | VFX Graph particle strip | No legacy TrailRenderer |
| Screen-shake | C# camera controller (data-driven from `data/balance/feel.json`) | Not a VFX asset |
| Hit-flash | Material `_FlashTime` float — shader-only | Triggered by Animator |

## "What we don't do"

Per `00-style-overview.md`, restated for VFX-specific clarity:

- **No gore** — no red mist, no blood spray, no viscera, no chunks.
- **No blood splatter** — even cartoon-stylized.
- **No skull motifs** — no skull pickups, no skull death VFX.
- **No photoreal smoke** — only stylized 6-particle puffs.
- **No motion blur** — disabled per `02-lighting.md`.
- **No chromatic aberration** — disabled, off-brand.
- **No screen-tear / glitch** — no horror-game post.
- **No realistic fire** — flat-shaded orange-yellow 2-frame flicker only.

## VFX checklist for vertical slice

> Scope per `docs/02-gdd/00-overview.md`: **1 hero (Bunny)**, **1 biome (Meadow)**, **3 weapons** (Carrot Lob, Burrow Spike, Acorn Mortar — placeholder names), **1 boss** (Meadow Warden, placeholder).

| Bucket | Effect | Count needed | Cumulative concurrent particles |
|---|---|---|---|
| Combat hits | Projectile spawn (× 3 weapons) | 3 effects | ~12 |
| Combat hits | Projectile trail (× 3 weapons) | 3 effects | ~36 (50 trails × 6 segments / scaled) |
| Combat hits | Impact puff (× 3 weapons, biome-tinted) | 3 effects | ~120 (20 impacts × 6 particles) |
| Combat hits | Knockback puff (shared) | 1 effect | ~24 |
| Pickup | XP gem idle pulse (shared) | 1 shader, no particles | 0 |
| Pickup | Magnetize trail (shared) | 1 effect | ~16 |
| Pickup | Absorb burst (shared) | 1 effect | ~16 |
| Level-up | Slow-mo (time only) | 0 particles | 0 |
| Level-up | Gold burst | 1 effect | ~30 |
| Level-up | Fanfare halo | 1 effect | ~20 |
| Hero state | Heal (later — not vertical slice) | 0 | 0 |
| Hero state | Hit reaction (shader + shake) | 0 particles | 0 |
| Enemy state | Spawn poof (Meadow tint) | 1 effect | ~40 (8 spawns × 5) |
| Enemy state | Hit flash (shader) | 0 particles | 0 |
| Enemy state | Death dissolve (Meadow tint) | 1 effect | ~80 (10 deaths × 8) |
| Enemy state | Elite aura (decal) | 1 effect | ~0 (decal, not particle) |
| Boss state | Phase-change shockwave | 1 effect | ~40 (peaks once) |
| Boss state | Telegraph radial (decal) | 1 effect | 0 (decal) |
| Boss state | Outro framing | 1 effect | ~30 |
| Environment | Grass sway (shader) | 0 particles | 0 |
| Environment | Meadow pollen | 1 effect | ~27 (9 chunks × 3) |
| **Total authoring count** | | **20 unique VFX assets** | |
| **Worst-case concurrent particles** | | | **≈ 491 — under 500 cap** |

### Authoring order for vertical slice

1. Hit-flash shader (gates pillar 4 acceptance)
2. Death dissolve (Meadow tint) (gates pillar 1)
3. XP gem idle pulse + absorb burst (gates pillar 3)
4. Level-up gold burst + fanfare halo (gates pillar 2)
5. 3 × projectile spawn/trail/impact sets (gates 3-weapon demo)
6. Meadow pollen ambient (gates pillar 7)
7. Boss telegraph + phase-change + outro (gates boss fight)
8. Spawn poof, knockback puff, magnetize trail, elite aura (polish pass)

## Hand-off

- VFX prefabs live under `unity/Assets/Art/VFX/<bucket>/`; pool API per ADR-0005.
- All effect timing values originate in `data/balance/feel.json`; gameplay-engineer reads, never inlines.
- Open ADR-0011: hitstop reconciliation (pillar 4 says 40 ms boss tick, this doc says 120 ms).
- Open question for tech-architect: confirm VFX Graph GPU instance path doesn't fall back to CPU sim when the toon shader is active on a particle quad.
