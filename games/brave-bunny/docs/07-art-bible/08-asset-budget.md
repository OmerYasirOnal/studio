# Asset Budget — Brave Bunny

> Owner: art-director. Cross-refs: `games/brave-bunny/CLAUDE.md` (perf contract: ≤80 DC, 250k tris on-screen, 60 fps iPhone 12, 5 ms combined lighting+post budget), `03-character-style.md` (hero/enemy tris caps), `04-environment-style.md` (chunk tris cap), `05-vfx-style.md` (particle budget). This doc is the **single source of truth for per-asset-class size and quantity** at launch and at vertical slice. Asset-curator and tech-architect cross-check against this before sourcing/baking.

## Perf budget recap (from CLAUDE.md)

- **Draw calls on-screen: ≤ 80**
- **Triangles on-screen: ≤ 250 000**
- **Combined lighting + post-FX: 5 ms / frame** on iPhone 12 (URP toon shader path)
- **Target device: iPhone 12 (60 fps), iPhone SE 3 (acceptable degrade to 50 fps)**
- **App size hot zone: < 200 MB on-disk** (iOS App Store install size matters for conversion)

## Per-asset-class budget table

All triangle counts are caps. All texture KB counts are post-compression (ASTC 4×4 / 6×6 on iOS, ETC2 on Android dev builds).

| Asset class | Per-item tris cap | Per-item texture KB | Quantity (launch) | On-disk total (KB) | Notes |
|---|---|---|---|---|---|
| Hero character | 5 000 | 512 KB (albedo + normal stub) | **8** | ~4 100 | Quaternius Animated Animals base + per-character recolor; 7-clip shared anim set |
| Enemy basic | 600 | 128 KB | **15 variants × 5 biomes = 75** | ~9 600 | Quaternius + Kenney puff-blobs; biome recolors share base mesh |
| Enemy elite | 1 500 | 256 KB | **5** | ~1 300 | One elite per biome; small bone rig (≤ 12 bones) |
| Boss | 8 000 | 1 024 KB | **5** | ~5 100 | One per biome; 1 on-screen ever; budget bumped from char-style 12k cap — boss-only |
| Weapon prop | 200 | 64 KB | **12** | ~770 | Per-weapon visible mesh (carrot, daisy, sunbeam disk, etc.) |
| Pickup (XP/gold/heart/etc.) | 100 | 32 KB | **6** | ~190 | XP gem (3 tiers), gold coin, heart, rare crystal — share material |
| Environment chunk | 8 000 | 1 024 KB atlas | **16 chunks × 5 biomes = 80** | ~81 920 | Atlas shared per biome (5 atlases × 1024 KB = 5.0 MB texture); 16 chunk meshes per biome |
| Prop (hero/anchor) | 1 000 | 256 KB | **15** | ~3 800 | Named anchor props per `04-environment-style.md` |
| Prop (filler) | 300 | 64 KB | **60** | ~3 800 | Background filler; instanced |
| VFX particle | 4 verts | 32 KB | **30** | ~960 | GPU-instanced quads; per `05-vfx-style.md` |
| UI icon | n/a (2D) | 8 KB (SVG + PNG fallback) | **54** | ~430 | Per `07-iconography.md` |
| UI font | n/a | ~200 KB per family | **3** (Fredoka, Nunito, Baloo 2) | ~600 | SIL OFL Google Fonts; subset to Latin Extended |
| Audio BGM (compressed OGG) | n/a | ~600 KB / 2 min loop | **12 tracks** | ~7 200 | See `08-audio-bible/01-bgm-spec.md` |
| Audio SFX (compressed OGG) | n/a | ~30 KB avg | **~50 SFX × ~3 round-robin = ~150 files** | ~4 500 | See `08-audio-bible/02-sfx-spec.md` |

### Totals

| Bucket | On-disk MB |
|---|---|
| Meshes (characters + enemies + bosses + weapons + pickups + chunks + props + VFX) | ~111 MB |
| Textures (atlases counted inside chunk row above; standalone hero/enemy/weapon textures) | included above |
| UI (icons + fonts) | ~1 MB |
| Audio (BGM + SFX) | ~12 MB |
| Code + Unity engine baseline | ~50 MB (engine-side, not in asset budget) |
| **Asset on-disk total** | **~124 MB** |
| **Launch on-disk asset budget cap** | **< 200 MB** |

Headroom: ~76 MB for unforeseen growth (localized fonts, additional VFX, post-launch hot-fix DLC).

## Cross-check vs on-screen perf budget

Recap from `03-character-style.md` + `04-environment-style.md`:

| Layer | Worst-case tris on-screen | Worst-case DC |
|---|---|---|
| Hero (1 active) | 5 000 | 1 (with shared outline pass) |
| Enemies (200 trash + 8 elite) | 200 × 200 + 8 × 600 = 44 800 | 2 (instanced batches) |
| Boss (1) | 8 000 | 1 |
| Weapon projectiles (50 active) | 50 × 200 = 10 000 | 1 (instanced) |
| Pickups (~30 active) | 30 × 100 = 3 000 | 1 (shared material) |
| Environment (9 chunks visible) | 9 × 8 000 = 72 000 | ~14 (chunk + 1 grass instancer + 1 skybox + decals) |
| Anchor props (visible ~6) | 6 × 1 000 = 6 000 | 1 (atlas) |
| Filler props (~20 instanced) | 20 × 300 = 6 000 | 1 (instancer) |
| VFX (worst-case 500 particles) | 500 × 4 = 2 000 verts (~700 tris) | 1-2 (VFX Graph atlas) |
| UI (HUD + modal + 1 toast) | ~500 verts | ≤ 8 |
| **Total** | **~155 500 tris** | **~32-35 DC** |

Headroom: **~94 500 tris** + **~45 DC** for VFX peaks, boss-arena particles, post-launch growth. Comfortably under the 250 k tris / 80 DC cap.

## Per-category notes

### Hero characters (8)
- All share **Quaternius Animated Animals** base mesh + 7-clip animation set.
- Per-character recolor adds zero polys, ~30 KB of recolored albedo texture each.
- Owl `eye_accent` exception (Pickup Gold eyes) documented in `03-character-style.md`.

### Enemies (75 + 5 elite + 5 boss)
- **15 visual variants per biome × 5 biomes** — but the *behavior* roster is much smaller; visuals are recolors.
- Puff-blob class shares 4 base meshes recolored 15 ways per biome.
- Elites get small bone rigs for telegraph anims.

### Bosses (5)
- One per biome. **Only one boss on screen at a time, ever.**
- Boss budget bumped to 8 000 tris (vs char-style 12 000 cap → conservatively budgeted at 8 000 here for safety; bosses may use up to 12 000 in extremis with art-director sign-off).
- Boss texture 1 MB allows ramp + emissive for telegraphs.

### Environment chunks (80 = 16 × 5)
- **Pre-merged in Blender per chunk** to hit the 1-2 DC per chunk target.
- 1 atlas shared across all 16 chunks per biome (1024 × 1024).
- 5 biome atlases total = 5 MB texture memory.

### Props (hero anchors 15 + filler 60)
- Hero anchors are the named per-biome props (`04-environment-style.md`): 3 anchors × 5 biomes = 15.
- Filler is generic flora/rocks; instanced.

### VFX (30 effects)
- 20 needed for vertical slice (per `05-vfx-style.md` checklist).
- 10 more for biome ambient + boss-specific extras at launch.
- All GPU-instanced quads; per-particle cost ≤ 0.001 ms.

### UI (54 icons + 3 font families)
- See `07-iconography.md` for icon spec; `06-ui-visual-direction.md` for fonts.
- Subset fonts to Latin Extended + Turkish + Spanish + Japanese (priority i18n markets) to keep size down.

### Audio (12 BGM + ~50 SFX)
- Full spec in `08-audio-bible/`.
- Audio compression target: OGG Vorbis at 96 kbps for BGM, 128 kbps for SFX (better transient response).

## Vertical-slice asset bill of materials

Per `docs/02-gdd/00-overview.md` scope: **1 character + 1 biome + 3 weapons + 1 boss + UI**. The exact ship list for the vertical slice:

### Characters (1)
- Bunny (recolored Quaternius `Rabbit.glb` + 7 anim clips)

### Biome (1) — Meadow
- 1 atlas (`atlas_meadow_albedo.png` 1024 × 1024 + `atlas_meadow_norm.png` 512 × 512)
- 16 chunk meshes (pre-merged in Blender)
- 3 hero-prop anchors: lone tree, wooden well, mushroom cluster — each in 3 rotation variants
- ~12 filler props (grass tufts, small rocks, daisies)
- 1 skybox (sky color `#BEE3F0` + gradient band)
- 1 decal type: leaf scatter

### Enemies (Meadow only)
- 3 trash variants (puff-blob recolors)
- 1 elite (Meadow Stomper — placeholder name)
- 1 boss (Meadow Warden — placeholder name)

### Weapons (3 — per `docs/02-gdd/04-weapons.md` vertical-slice list)
- Carrot Boomerang (1 mesh + projectile + VFX trio)
- Sunbeam (1 beam mesh + VFX trio)
- Daisy Mine (1 mesh + 2-frame "wobble" anim + VFX trio)

### Pickups (4)
- XP gem (small), XP gem (large), gold coin, heart

### VFX (20 — per `05-vfx-style.md` vertical-slice checklist)
- 4 combat-hit effects, 3 pickup, 2 level-up, 1 hero-state, 3 enemy-state, 3 boss-state, 1 environment ambient (Meadow pollen), 3 weapon-specific projectile sets

### UI (subset)
- Fonts: Fredoka + Nunito + Baloo 2 (3 families)
- Icons: ~24 critical (3 currency + 5 nav + 8 HUD + 3 vertical-slice weapons + 1 char + 4 settings = ~24)
- Screens needed: home/lobby, character select (1 char), loadout, in-run HUD, level-up modal, run-end tally, settings, IAP confirm (mock)

### Audio (subset)
- BGM: Home, Lobby, Run-Meadow, Boss-Meadow, Run-end-win, Run-end-lose, Cold-start splash = **7 tracks**
- SFX: ~25 (UI subset + combat for 3 weapons + 1 boss + pickups + endgame stingers)

### Vertical-slice on-disk asset estimate
~40 MB (well within budget; gives runway to validate the pipeline before scaling to launch quantities).

## Hand-off

- **Asset-curator** uses this doc as the procurement checklist; cross-refs `09-source-shortlist.md` for where to fetch each asset.
- **Blender-tech** uses per-item tris caps as the hard ceiling for merge/decimation passes.
- **Tech-architect** validates the on-screen DC + tris math against this doc when reviewing pool/instancer ADRs.
- **Open question for tech-architect:** confirm ASTC compression block sizes on iPhone SE 3 (which lacks A14 hardware decoder for some ASTC variants). If 6×6 unsupported, fall back to 4×4 and re-budget texture memory upward by ~30%.
