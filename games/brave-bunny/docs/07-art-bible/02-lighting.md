# Lighting — Brave Bunny

> Owner: art-director. Cross-refs: `00-style-overview.md` (toon shader is the rendering substrate), `01-color-palette.md` (per-biome shadow ramps), `docs/06-tech-spec/` (URP feature set), `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 draw calls, 250k tris, 60 fps iPhone 12). All values target the iPhone 12 baseline; iPhone SE 3 inherits the same setup with bloom intensity scaled to 0.25.

## Lighting register per biome

Every biome ships a key + fill + rim setup baked into a Light Group preset. Hero rim is always tuned to make the hero pop against the biome — never to look "realistic."

### Meadow — bright noon

| Light | Angle | Color (hex) | Intensity | Notes |
|---|---|---|---|---|
| Key (sun) | 50° elevation, 30° azimuth | `#FFF4DC` | 1.4 | Warm noon sun |
| Fill (bounced grass) | -10° elevation (below horizon) | `#A8D86B` | 0.25 | Greenish under-bounce |
| Rim (hero only) | Behind hero, 120° from key | `#BEE3F0` | 0.8 | Cool sky-rim separates hero from grass |
| Ambient probe | n/a | `#CDEBD0` | tint multiplier 0.6 | Mint wash |

### Beach — golden hour

| Light | Angle | Color (hex) | Intensity | Notes |
|---|---|---|---|---|
| Key (sun, low) | 15° elevation, 270° azimuth (camera-right setting sun) | `#FFD9A0` | 1.6 | Strong golden direct |
| Fill | 30° elevation, opposite side | `#F39FB4` | 0.35 | Pink-tinged sky fill |
| Rim (hero) | Camera-left, strong | `#FFC83D` | 1.2 | Pickup-gold rim — warmest in the game |
| Ambient | n/a | `#F6D6B5` | tint multiplier 0.7 | Sand bounce |

### Forest — dappled

| Light | Angle | Color (hex) | Intensity | Notes |
|---|---|---|---|---|
| Key (sun with cookie/gobo) | 70° elevation | `#FFFFEC` | 1.1 | Light cookie texture authored as `LeafCookie.png` |
| Fill | Top-down | `#2F5A3A` | 0.15 | Low ambient — forest is dim |
| Rim (hero) | Behind, **desaturated** | `#EAEAEA` | 1.0 | Hero gets a near-white rim so silhouette survives the dappled chaos |
| Ambient | n/a | `#5E6B3E` | tint multiplier 0.4 | Olive deep |

### Cavern — torchlit

| Light | Angle | Color (hex) | Intensity | Notes |
|---|---|---|---|---|
| Key | 80° elevation (overhead) | `#BEE3F0` | 0.7 | Cool overhead — like a crystal glow above |
| Fill | n/a (ambient only) | `#5B4A6E` | 0.3 | Plum ambient |
| Hot points (per hazard) | Point lights on torches, lava cracks | `#FF7E6B` | 1.5, range 4u | Max 4 simultaneous per screen (perf) |
| Rim (hero) | Top-rear | `#FF7E6B` | 0.9 | Coral rim |
| Ambient | n/a | `#1F1830` | tint multiplier 0.5 | Dim, never black |

### Snow — overcast diffuse

| Light | Angle | Color (hex) | Intensity | Notes |
|---|---|---|---|---|
| Key (sun behind clouds) | 60° elevation | `#EAF2F5` | 1.2 | Bright but soft — diffuse shadows |
| Fill | Hemisphere | `#D8E6EE` | 0.5 | Snow up-bounce |
| Rim (hero) | Behind | `#FFFFFF` | 1.0 | Pure white separation rim |
| Ambient | n/a | `#7E96A6` | tint multiplier 0.6 | Cool gray-blue |

## Post-process stack (URP Volume)

Shared base volume, biome volumes override the LUT only.

| Effect | Setting | Value | Notes |
|---|---|---|---|
| Bloom | Threshold | 1.1 | Just above white — only emissive picks up bloom |
| Bloom | Intensity | 0.4 (iPhone 12) / 0.25 (iPhone SE 3) | Auto-scale on quality tier |
| Bloom | Scatter | 0.7 | Soft bloom, not hot |
| Vignette | Intensity | 0.15 | Gentle — frames the action, never crushes corners |
| Vignette | Smoothness | 0.6 | Soft edge |
| Color grading | Tonemap | ACES | Industry-standard filmic curve |
| Color grading | LUT | Per-biome 32-slice LUT | `Meadow_LUT.png`, `Beach_LUT.png`, ... |
| Depth of field | n/a | **DISABLED** on gameplay camera | DOF destroys readability for swarms |
| Motion blur | n/a | **DISABLED** | Mobile perf + readability |
| Chromatic aberration | n/a | **DISABLED** | Off-brand for cartoon |
| Film grain | n/a | **DISABLED** | Off-brand for cartoon |

## Shadow strategy

Aggressive: real-time shadows only where they earn their place in the 80-DC, 250k-tri budget.

| Caster class | Shadow type | Notes |
|---|---|---|
| Hero | Real-time soft directional (1 cascade) | Earns its cost — hero is the focus |
| Boss | Real-time soft directional (1 cascade) | Only 1 boss on screen ever |
| Standard / swarm enemies | **NONE** (no real-time shadow) | Up to 200 active — would blow the perf contract |
| Pickups | None | Read via emissive halo instead |
| Environment chunks | Baked lightmap | Per-biome bake at 2048×2048, 4 px/unit |
| Dynamic props (destructibles) | Blob shadow decal (single quad) | Cheap, reads enough |

Shadow distance cap: **12 units from camera focus**. Beyond that, shadows fade to ambient.

## Hit-flash and impact lighting

Tight, expressive, perf-cheap. All values are art-side; gameplay-engineer wires the triggers.

| Event | Visual | Duration | Cost |
|---|---|---|---|
| Enemy takes hit | White emissive ramp on enemy material → `#FFFFFF` at intensity 2.0 | 50 ms | Shader-only, free |
| Enemy dies (standard) | Puff VFX (5-particle one-shot) + biome-accent screen-tint flash 5% alpha | 80 ms | <0.1 ms |
| Elite kill | Biome-accent screen-tint 15% alpha + 200ms bloom intensity spike to 0.7 | 200 ms | <0.3 ms |
| Boss kill | Full screen flash to biome key color 40% alpha, fade over 600ms, slow-mo 0.6× for 400ms | 600 ms | One-time event |
| Hero takes hit | Damage Red (`#E83C3C`) hero material flash for 100ms + 0.2 unit screen-shake | 100 ms | Shader-only, free |
| Level-up | Hero Highlight (`#FF6B6B`) radial burst behind hero, 0.5s outward; bloom spike to 0.6 | 500 ms | Particle, ~0.4 ms |
| Pickup grab (gold gem) | Pickup Gold (`#FFC83D`) sparkle + 50ms emissive ping on hero | 100 ms | Particle, <0.1 ms |

## Performance ceiling

| Metric | Budget on iPhone 12 baseline | Notes |
|---|---|---|
| Lighting cost per frame | < 3 ms | Combined GPU shadow + light eval |
| Real-time directional lights | 1 | Sun key only |
| Real-time point lights | ≤ 4 simultaneous (Cavern biome only) | Other biomes use 0 |
| Shadow casters | ≤ 2 (hero + boss) | Hard cap |
| Post-process budget | < 1.5 ms | Bloom + ACES + LUT only |
| LUT memory | 5 × 32-slice LUTs × ~128 KB each = ~640 KB total | One per biome |
| Lightmap memory per biome | ≤ 8 MB (2048² compressed) | Streamed in/out on biome change |

Total combined art-rendering budget (lighting + post + shadows): **≤ 5 ms per frame**, leaving ~11 ms for gameplay + UI + physics inside the 16.67 ms / 60 fps frame.
