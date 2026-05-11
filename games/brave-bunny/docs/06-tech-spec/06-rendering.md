# Tech Spec 06 — Rendering

> Owner: tech-architect. URP feature set, custom toon shader contract, shadow strategy, batching, and texture compression for Brave Bunny. Authoritative cross-refs: `00-engine-and-version.md` (Unity 6 LTS + URP + IL2CPP), `05-performance-budget.md` (≤ 3 ms lighting+post, ≤ 80 DC, 250k tris), `07-art-bible/02-lighting.md` (per-biome lights + post values), `07-art-bible/05-vfx-style.md` (VFX Graph + GPU instancing), **ADR-0002** (custom toon shader, in-house). Sister doc: `07-audio.md` for the parallel audio pipeline.

## URP project settings

Settings live in `unity/Assets/Settings/UniversalRP-Brave.asset` (referenced from `unity/Assets/Settings/QualitySettings.asset`).

| Setting | Value | Reason |
|---|---|---|
| Renderer | **Forward+** | Best mobile perf when many small lights are possible (Cavern biome ≤ 4 point lights); avoids deferred g-buffer memory hit |
| Color space | **Linear** | sRGB textures auto-converted; required for ACES tonemap from `02-lighting.md` |
| HDR | **Enabled (bloom only)** | Needed for bloom threshold 1.1 from `02-lighting.md`; no other HDR effects |
| MSAA | **2× (Multi-Sample 2x)** | 4× too costly on iPhone SE 3 A14 GPU per `05-performance-budget.md`; 2× preserves outline-pass edges |
| Render scale | 1.0 (iPhone 12) / **0.85** (iPhone SE 3 / low-power) | Per `05-performance-budget.md` degrade plan |
| Anti-aliasing (per-camera) | FXAA off, MSAA on | FXAA chews outline shader; MSAA preserves it |
| Opaque texture | Disabled | Not used by any shader |
| Depth texture | Enabled (boss arena outline rim only) | Needed by toon outline depth-discontinuity pass |
| Target frame rate | 60 fps (default) / 50 fps (SE 3 fallback) | Per `Application.targetFrameRate` policy |

### Lighting setup

- **1 real-time directional light** — the biome sun. Position + color from `07-art-bible/02-lighting.md` per-biome table.
- **0–4 real-time spot/point lights** — Cavern biome only (torches, lava cracks); other biomes 0 per the perf ceiling in lighting bible.
- **No real-time spot lights in run hot path** — boss arenas may use 1 cinematic spot for boss intro framing, culled the instant boss spawn animation ends.
- **No swarmer-area lighting** — swarmers receive ambient + key only; no per-enemy point lights.
- **Light layers** (URP Forward+ light culling):

| Layer | Bit | Lights affecting | Receivers |
|---|---|---|---|
| Default | 0 | Sun key, ambient probe | All non-tagged objects |
| Hero | 1 | Sun + hero rim light | Hero mesh only — gets dedicated rim |
| Boss | 2 | Sun + boss spot (intro) + biome point lights | Boss mesh only |
| Environment | 3 | Sun + baked lightmap | Environment chunks |
| VFX | 4 | None (unlit shaders) | All particles + decals |

### Post-process stack

Single global URP Volume profile at `unity/Assets/Settings/PostProcess/Brave-Global.asset`. Per-biome volumes override LUT only.

| Effect | Value | Source |
|---|---|---|
| Bloom Threshold | 1.1 | `02-lighting.md` |
| Bloom Intensity | 0.4 (iPhone 12) / 0.25 (SE 3) | `02-lighting.md` |
| Bloom Scatter | 0.7 | `02-lighting.md` |
| Vignette Intensity | 0.15 | `02-lighting.md` |
| Vignette Smoothness | 0.6 | `02-lighting.md` |
| Tonemap | **ACES** | `02-lighting.md` |
| Color LUT (per biome) | `Meadow_LUT.png`, `Beach_LUT.png`, `Forest_LUT.png`, `Cavern_LUT.png`, `Snow_LUT.png` | `02-lighting.md` |
| Depth of Field | **DISABLED** | `02-lighting.md` (readability) |
| Motion Blur | **DISABLED** | `02-lighting.md` (perf + brand) |
| Chromatic Aberration | **DISABLED** | `02-lighting.md` (brand) |
| Film Grain | **DISABLED** | `02-lighting.md` (brand) |

Volume blending is driven by a `BiomeVolumeSwitcher` MonoBehaviour on the run camera; switch crossfades over **400 ms** on biome change inside `Run.unity`.

## Custom toon shader

Per **ADR-0002**, the toon shader is authored in-house, not adopted from a community library.

### File layout

```
unity/Assets/_Brave/Shaders/Toon/
├── BraveToon.shadergraph              # main lit pass + outline sub-shader
├── BraveToon_Particle.shadergraph     # particle variant (unlit, accepts vertex color)
├── BraveToon.shader                   # compiled stub (committed; CI verifies graph→.shader regen)
├── RampTextures/
│   ├── Ramp_2Tone.png                 # default 1-tone shadow ramp (128 × 4 px)
│   └── Ramp_3Tone.png                 # boss-only 2-tone variant
└── README.md                          # author notes (links back to ADR-0002)
```

### Shader properties

| Property | Type | Range | Default | Use |
|---|---|---|---|---|
| `_BaseMap` | Texture2D | — | white | Albedo, ASTC 4×4 |
| `_BaseColor` | Color | — | `#FFFFFF` | Material tint |
| `_RampTexture` | Texture2D | — | `Ramp_2Tone.png` | NdotL ramp lookup; X = lit→shadow gradient |
| `_OutlineColor` | Color | — | `#1A1A1A` | Per-character outline color (hero gets darker outline) |
| `_OutlineWidth` | float | 0.0 → 0.05 (world units) | 0.012 | Inflated-shell outline pass thickness |
| `_HeroSaturationBoost` | float | 1.0 → 1.3 (multiplier) | 1.0 | Per-renderer override: heroes set to 1.20 per art bible |
| `_BiomeLUT` | Texture3D | — | identity | 32-slice LUT injected by `BiomeVolumeSwitcher` (LUT also lives in post-volume; this is for material-local grading on bosses) |
| `_RimColor` | Color | — | `#FFFFFF` | Hero rim per biome (from `02-lighting.md` per-biome table) |
| `_RimPower` | float | 1.0 → 6.0 | 3.0 | Rim falloff sharpness |
| `_EmissiveTint` | Color (HDR) | — | `#000000` | Hit-flash and elite-glow target (driven from C# `_FlashTime` per `05-vfx-style.md`) |

### Outline pass

Implemented as a **separate sub-shader pass** within the same `.shadergraph`:

- Renders **before** the lit pass with `Cull Front` and an **inflated vertex shell** along the vertex normal by `_OutlineWidth` world units.
- Stencil/depth: outline pass writes to depth but not to a stencil mask; it relies on Cull Front + depth-test-less-equal to draw only a thin rim where back faces don't get overwritten by front-face shading.
- Outline pass is **disabled on swarm enemies** (set via `_OutlineWidth = 0` on shared swarmer materials) — per ADR-0002 fallback and `05-performance-budget.md` triage step 3.
- Outline pass **enabled on**: hero, elites, bosses, all pickups (so they read against busy backgrounds).

### Hero saturation boost

Implemented as a **per-renderer material property override** (Unity 6 LTS `MaterialPropertyBlock`) — does not create new material instances (preserves SRP Batcher compatibility). The hero spawn code in `Brave.Gameplay.HeroSpawner.Spawn()` sets `_HeroSaturationBoost = 1.20f` once at instantiation. Source value from `07-art-bible/03-character-style.md` (+20%).

### VFX Graph + toon path

Open question from `07-art-bible/05-vfx-style.md`: confirm VFX Graph GPU instance path does not fall back to CPU sim when the toon shader is active on a particle quad. **Decision:** particle systems use `BraveToon_Particle.shadergraph` (unlit, no outline pass, no ramp lookup, no NdotL) — explicitly authored to stay on the GPU sim path. The lit toon shader is **not** to be assigned to particle materials. Documented for art-director sign-off; verification owned by qa-engineer in Phase 5.

## Shadow strategy

Echoes `02-lighting.md`; restated here as a tech contract.

| Caster | Shadow type | Cost |
|---|---|---|
| Hero | Real-time soft directional, 1 cascade, distance 12 u | ≤ 0.4 ms |
| Boss | Real-time soft directional, 1 cascade, distance 12 u | ≤ 0.3 ms (boss is 1 instance) |
| Swarmer / standard enemy | **NONE** (`castShadows = false` on prefab) | 0 ms |
| Pickups | None (emissive halo only) | 0 ms |
| Environment chunks | **Baked lightmap** at 2048² per biome, 4 px/unit | ≤ 8 MB per biome (streamed on biome change) |
| Dynamic destructibles | Blob-shadow decal quad | < 0.05 ms each |

Shadow distance cap **12 units** from camera focus (per `02-lighting.md`); beyond fades to ambient. Cascade count **1** — survivor-like camera is top-down with shallow depth range.

## GPU instancing + batching

| Knob | Setting | Reason |
|---|---|---|
| **SRP Batcher** | Enabled (URP default) | Largest single perf win on mobile; requires all materials use `BraveToon.shadergraph` constant-buffer layout — enforced via `[Test]` in `Brave.Tests.EditMode` |
| **GPU instancing** | Enabled on all enemy materials, prop materials, pickup materials | 200 enemies × shared mesh → 1 DC |
| **Static batching** | Enabled on environment chunks (marked `Static` in inspector) | Per-biome chunk batches collapse into 1–2 DC |
| **Dynamic batching** | Enabled for meshes < 300 verts | Catches small props heroes/enemies use; rarely fires when SRP Batcher already handles them |

### Per-frame draw call budget

Target: well under the 80-DC cap from `brave-bunny/CLAUDE.md`.

| Bucket | DC count | Notes |
|---|---|---|
| Environment chunks | 1–2 per visible chunk × ~3 visible | ~6 DC |
| Hero (lit pass + outline pass) | 2 | Outline + lit |
| Enemies (instanced, 200 active, ~3 unique meshes) | ~3 × 2 (lit + outline only on elites) | ~5 DC |
| Projectiles (instanced, ~50 active, 3 meshes) | 3 | ~3 DC |
| VFX particles (GPU-instanced quads, ~10 systems) | ~10 | ~10 DC |
| Pickups (instanced, 3 types) | 3 | ~3 DC |
| Boss (when present) | 2 (lit + outline) | ~2 DC |
| Decals (blob shadows + telegraph) | ~5 | ~5 DC |
| UI (UI Toolkit single root) | 1 | 1 DC |
| Post-process (bloom + tonemap + LUT) | ~4 | 4 DC |
| **Total typical** | **~30 DC** | ~50 DC peak with boss + dense VFX |

Headroom of ~30 DC absorbs spikes; if peak exceeds 80, triage falls to `05-performance-budget.md` step 3 (drop outline on swarmers — already default).

## Texture compression

| Platform | Format | Reason |
|---|---|---|
| **iOS (iPhone 12 + SE 3)** | **ASTC 4×4** | Universal A14+ baseline per `00-engine-and-version.md`; ASTC 6×6 **not supported** on iPhone SE 3 A14 GPU at acceptable quality — `05-performance-budget.md` confirms |
| **Android (dev builds)** | **ETC2 RGBA** | Per `00-engine-and-version.md`; OPL builds switch to ASTC 4×4 on supported devices (covered in `10-build-and-ci.md`) |
| **Editor (asset import)** | RGBA32 uncompressed | Authoring source; conversion happens at platform-bake time |

Per-texture mip chain enabled on environment albedos; disabled on UI atlases (no mip needed at 1:1 UI pixel mapping). LUT 3D textures (`Meadow_LUT.png` etc.) ship as **R8G8B8A8 uncompressed** — 5 × ~128 KB = ~640 KB total per `02-lighting.md` budget.

## Cross-references

- **ADR-0002** — custom toon shader source decision.
- `00-engine-and-version.md` — URP + Unity 6 LTS pin; ASTC 4×4 baseline.
- `05-performance-budget.md` — 3 ms lighting+post + 1.5 ms VFX budget; SE 3 degrade plan.
- `07-art-bible/02-lighting.md` — per-biome lighting setup, post stack values.
- `07-art-bible/05-vfx-style.md` — particle authoring rules, VFX Graph requirement.
- `brave-bunny/CLAUDE.md` — 80 DC cap, 250k tris, 60 fps iPhone 12 contract.
