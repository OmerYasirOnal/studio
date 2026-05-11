# Environment Style — Brave Bunny

> Owner: art-director. Cross-refs: `00-style-overview.md` (pastels + 80% S ceiling), `01-color-palette.md` (per-biome palettes), `02-lighting.md` (per-biome key/fill/rim + shadow strategy), `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 DC, 250k tris on-screen). Production sources: **Kenney Nature Kit + Platformer Kit (CC0)** and **Quaternius Ultimate Nature Pack (CC0)**, recolored to the per-biome palette.

## Modular tile system

The world is built from a single square tile grid:

| Param | Value | Notes |
|---|---|---|
| Tile edge | **4 × 4 Unity units** | Snap step in editor; matches Quaternius modular grid |
| Tile height | 0.5 u (ground) / variable (props) | Ground tiles are thin slabs |
| Authoring chunk | **16 × 16 u** (4 × 4 tiles) | Chunk is the streaming and merge unit |
| Chunks per visible frame | 9 max (3 × 3 around hero) | Camera fov + 18 u dist sees ~36 × 36 u |
| Snap | Grid-locked to 4 u | No off-grid rotation/position for tiles |

## Chunk authoring spec

Each `16×16 u` chunk = **tile arrangement + 1–3 hero props**. Chunks are pre-merged in Blender into a single mesh per material atlas, then re-instanced in Unity. This is the linchpin of the 80-DC budget.

### Hero-prop rule (anchor reads)

Every chunk has **1–3 named hero props** — these are the recognizable silhouettes that tell the player "I'm in the Meadow." Background filler (grass, small rocks) is procedural via the GPU instancer.

| Biome | Hero props (named anchors) |
|---|---|
| **Meadow** | Lone tree, wooden well, mushroom cluster |
| **Beach** | Palm tree, beach hut, coconut pile |
| **Forest** | Ancient oak, log bridge, mushroom ring |
| **Cavern** | Stalactite cluster, glow-mushroom, gem outcrop |
| **Snow** | Pine tree, ice formation, igloo |

Each hero prop ships in 3 rotation variants (0°, 120°, 240°) so re-use doesn't read as a copy-paste grid.

## Material atlas spec

Per-biome budget: **1 material atlas per biome**, max 4 unique materials inside.

| Map | Channels | Resolution | Notes |
|---|---|---|---|
| Albedo + AO | RGB = albedo, A = AO mask | 1024 × 1024 | ASTC 4×4 on iPhone, ~1.0 MB |
| Normal + Emissive | RG = normal (octahedral packed), B = emissive mask, A = unused | 512 × 512 | ASTC 6×6, ~0.5 MB |

> Two maps per biome × 5 biomes = ~7.5 MB texture memory total. Within the lightmap budget of 8 MB/biome from `02-lighting.md` (textures and lightmaps live in separate pools).

### Material count cap

| Per biome | Cap | Notes |
|---|---|---|
| Unique materials | ≤ 4 | Ground / props / vegetation / hero-prop variant |
| Unique shaders | 1 (toon-ramp) + 1 (instanced-grass) | No per-prop custom shaders |

## Vegetation strategy

Wind-sway grass, leaves, and small foliage **never** go through the standard mesh path.

| Param | Value | Notes |
|---|---|---|
| Renderer | URP GPU instancer + custom vertex shader for sway | Single DC for up to 1023 instances per batch |
| Max visible instances | **200** | Per-frame cap; LOD culls beyond 14 u from camera |
| Sway period | 2 s, ±3° per pillar 7 | Randomized phase per instance via instance ID |
| Tris per instance | ≤ 16 | Cross-quad billboard |

## Decals

Per-chunk decal budget: **0–2** for variety. Decals are projected quads in URP's decal projector, single-material.

| Decal type | Use case | Lifetime |
|---|---|---|
| Paw prints | Snow / Beach trail accent | Persistent (baked into chunk) |
| Leaf scatter | Forest ground accent | Persistent |
| Scuff marks | Cavern combat ring | Persistent |
| Hero footprint (dynamic) | Snow biome only | 4 s fade |

Total active decals on-screen: **≤ 18** (9 chunks × 2 max).

## Skybox

No HDR cubemaps — they cost too much VRAM for the perf budget.

| Component | Spec | Cost |
|---|---|---|
| Base | Solid color = biome sky hex from `01-color-palette.md` | 0 DC (clear color) |
| Gradient band | Single horizon gradient (sky → ground) blended via fullscreen quad | 1 DC |
| Clouds | NONE (no overlay clouds on standard biomes) | 0 DC |
| Stars (Cavern only) | 8-star sparkle decal on ceiling | 1 DC |

Skybox total: **1–2 DC** per biome.

## CC0 source mapping

We mix and match Kenney and Quaternius packs by biome strengths. **Custom Blender additions** are only authored when there is a clear CC0-source gap.

| Biome | Primary source | Secondary source | Custom Blender additions (gap fill) |
|---|---|---|---|
| Meadow | Kenney Nature Kit | Quaternius Nature (trees) | — none — pack is complete |
| Beach | Kenney Platformer Kit | Quaternius Nature (palms) | Beach hut variant (Kenney lacks one in art-style) |
| Forest | Kenney Nature Kit | Quaternius Nature (mushrooms) | Log bridge (custom — neither pack ships a curved bridge) |
| Cavern | Quaternius Cave Kit | Kenney Mini Dungeon (props) | Glow-mushroom emissive variant (paint own, then recolor) |
| Snow | Kenney Platformer Kit | Quaternius Nature (recolored) | Igloo (custom — neither pack ships it CC0) |

> All custom Blender additions land in `assets-raw/custom/<biome>/` and follow the same recolor + atlas pipeline as Quaternius source. Asset-curator owns sourcing; blender-tech owns the merge.

## Triangle + draw-call budget per chunk

| Metric | Cap per chunk | Notes |
|---|---|---|
| Total tris | ≤ **8 000** tris | Ground + 1–3 hero props + decals |
| Static draw calls | 1–2 DC | Atlas-merged ground = 1; props if separate material = 1 |
| With vegetation instancer | +1 DC (shared across all chunks per biome) | Single GPU instance batch |
| With decals | +1 DC (URP decal pass per chunk) | Shared if all chunks have decals |

> 9 visible chunks × 1.5 avg DC = 14 DC for environment. Plus 1 DC outline (heroes), 1 DC skybox, 1 DC decals, 1 DC grass instancer = ~18 DC. Leaves **62 DC** for enemies + projectiles + VFX + UI within the 80-DC CLAUDE.md cap.

> 9 visible chunks × 6 k avg tris = 54 k tris environment baseline. Combined with hero + enemies (~62 k from `03-character-style.md`), total ~116 k — comfortably under the 250 k cap.

## Hand-off

- Per-biome atlas layout (UV maps for each material slot) belongs in `assets-raw/atlases/<biome>_layout.png`; asset-curator authors.
- Quaternius and Kenney source FBXs land in `assets-raw/kenney/` and `assets-raw/quaternius/`; blender-tech runs the merge + recolor pipeline before they ship to Unity.
- Open question for tech-architect: confirm URP decal projector batches across chunks at the 18-decal-on-screen cap.
- Open question for tech-architect: GPU instancer compatibility with the toon-ramp shader's vertex paint tint feature.
