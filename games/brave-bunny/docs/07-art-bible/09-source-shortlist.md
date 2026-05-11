# Source Shortlist — Brave Bunny

> Owner: art-director. Cross-refs: `core/docs/asset-policy.md` (CC0 / OFL / MIT / CC-BY only), `03-character-style.md` (Quaternius recolor pipeline), `04-environment-style.md` (Kenney + Quaternius source mapping), `06-ui-visual-direction.md` (Google Fonts), `07-iconography.md` (Kenney Game Icons), `08-asset-budget.md` (per-class quantities). This doc is the **single-source procurement list** asset-curator works from. Every entry names a CC0 / OFL / MIT / CC-BY source — **zero paid assets** per CLAUDE.md principle 8.

## License legend

| Code | Meaning | Attribution required? |
|---|---|---|
| **CC0** | Public-domain dedication | No |
| **OFL** | SIL Open Font License | No (font name reservation rules apply) |
| **MIT** | MIT software license | Yes — license file in repo |
| **CC-BY** | Creative Commons Attribution | Yes — record in `LICENSES.md` + in-game credits screen |

## Characters (8) — Quaternius Animated Animals (CC0)

All character meshes recolored from the **Quaternius Animated Animals** CC0 pack. Recolor pipeline owned by blender-tech (`core/tools/blender-tech/_recolor.py`). Hex maps per character live in `03-character-style.md` §Per-character recolor maps.

| Character | Quaternius pack | Source file | Notes / Blender step |
|---|---|---|---|
| **Bunny** | Animated Animals | `Rabbit.glb` | Recolor primary_fur → `#FFF4DC` (Bunny Cream); add pink nose dot `#F39FB4` decal |
| **Tortoise** | Animated Animals | `Tortoise.glb` | Recolor shell → `#A8D86B` (Meadow Lime/jade), body → `#6FAE74` (Sage Mid); hex shell pattern preserved from source |
| **Fox** | Animated Animals | `Fox.glb` | Recolor primary → `#FF8A4C` (warmer than default orange); white tip tail; add red scarf `#FF6B6B` mesh fragment |
| **Hedgehog** | Animated Animals | `Hedgehog.glb` | Recolor fur → `#8B6A4A` (pale-ginger Bark Brown); spines → `#2E2A28` (Coal); keep 12-spine radial pattern |
| **Otter** | Animated Animals | `Otter.glb` (if absent, use Beaver as adjacency fallback) | Recolor → `#6B4A3A` (river-brown); add belly shell decal |
| **Panda** | Animated Animals | `Panda.glb` | Recolor → preserve `#FFFFFF` body + `#2E2A28` eye-patches (mostly native colors with brightness pop) |
| **Badger** | Animated Animals | `Badger.glb` | Recolor → high-contrast `#3C3A38` charcoal body + `#FFFFFF` head stripe |
| **Owl** | Animated Animals | `Owl.glb` | Recolor feathers → `#D7C5E8` (Lavender Mist amber-shift); eyes → `#FFC83D` (Pickup Gold — documented exception) |

**Confirmed coverage:** 7/8 directly in pack (Bunny, Tortoise, Fox, Hedgehog, Panda, Badger, Owl). **Otter** may need fallback to a similar Quaternius mesh (Beaver) if Otter is absent — flagged for asset-curator.

## Enemies — Mixed sources

| Class | Source | Notes |
|---|---|---|
| Trash puff-blobs | **Custom Blender** (≤ 200 tris each) | 4 base meshes recolored 15 ways per biome; geometry trivial; blender-tech authors |
| Elites | **Quaternius Monsters Pack (CC0)** | One per biome, recolored to biome palette |
| Bosses | **Quaternius Monsters Pack (CC0)** + custom kitbash | Boss-per-biome; may need extra props (e.g., crown, horns) blender-authored |

## Environment per biome — Kenney + Quaternius (CC0)

Cross-ref `04-environment-style.md` §CC0 source mapping. Per-biome:

### Meadow (vertical-slice biome)

| Source | Pack | What we use |
|---|---|---|
| Primary | **Kenney Nature Kit (CC0)** | Grass tufts, daisies, small rocks, wooden well, mushroom cluster, fence posts |
| Secondary | **Quaternius Ultimate Nature Pack (CC0)** | Lone tree (oak/poplar), pine, hero shrub variants |
| Custom | none — pack is complete | |

### Beach

| Source | Pack | What we use |
|---|---|---|
| Primary | **Kenney Platformer Kit (CC0)** | Sand tile, shell prop, coconut, sandstone rocks |
| Secondary | **Quaternius Ultimate Nature Pack** | Palm tree variants, beach shrub |
| Custom | **Blender** — beach hut (Kenney lacks one in art-style) | |

### Forest

| Source | Pack | What we use |
|---|---|---|
| Primary | **Kenney Nature Kit** | Mushrooms, large trees, leaf piles, autumn props |
| Secondary | **Quaternius Ultimate Nature Pack** | Ancient oak hero prop, large mushroom |
| Custom | **Blender** — log bridge (neither pack ships a curved bridge) | |

### Cavern

| Source | Pack | What we use |
|---|---|---|
| Primary | **Quaternius Cave Kit (CC0)** | Stalactites, rocks, ground tiles |
| Secondary | **Kenney Mini Dungeon (CC0)** | Crates, chest, brazier, gem outcrops |
| Custom | **Blender** — glow-mushroom emissive variant (paint own, then recolor) | |

### Snow

| Source | Pack | What we use |
|---|---|---|
| Primary | **Kenney Platformer Kit** (snow theme variant) | Snow tile, ice block, pine recolored |
| Secondary | **Quaternius Ultimate Nature Pack** (recolored white) | Hero pine, frost-rimmed shrub |
| Custom | **Blender** — igloo (neither pack ships it CC0) | |

## Weapons (12) — Mixed: Quaternius + Custom Blender

Per `04-weapons.md`. Most weapon props are tiny (≤ 200 tris) and easier to author from scratch than source.

| Weapon | Source | Notes |
|---|---|---|
| Carrot Boomerang | **Custom Blender** | No clean CC0 carrot-shape source; ~120 tris primitive |
| Sunbeam | **Procedural / VFX only** | No mesh — beam is a shader-driven quad strip |
| Daisy Mine | **Quaternius Ultimate Nature Pack** (daisy prop) | Recolor + wobble anim |
| Pebble Sling | **Kenney Generic Items** or custom (just a smooth grey sphere) | trivial |
| Honey Aura | **Procedural / VFX only** | aura is shader; no mesh |
| Acorn Cannon | **Quaternius Ultimate Nature Pack** (acorn prop) | Add tiny cannon barrel mesh |
| Thunder Cloud | **Custom Blender** | Stylized pillowy cloud mesh |
| Frost Whisper | **Procedural / VFX only** | mist aura, no mesh |
| Cob Mortar | **Custom Blender** | Corn-cob primitive |
| Beehive | **Kenney Nature Kit** (wooden hive prop) or custom | |
| Tumbleweed | **Custom Blender** | Brambly sphere with smile decal |
| Whirligig | **Custom Blender** | Paper-pinwheel primitive |

**Custom Blender weapon count: 6 (Carrot Boomerang, Thunder Cloud, Cob Mortar, Tumbleweed, Whirligig, optional Beehive).**

## UI icons — Kenney Game Icons (CC0) + Custom

Per `07-iconography.md`.

| Icon category | Source | Coverage |
|---|---|---|
| Currency (3) | **Kenney Game Icons (CC0)** | Full coverage |
| Navigation (5) | **Kenney Game Icons** | Full coverage |
| HUD (8) | **Kenney Game Icons** + 2 custom | 6/8 covered; need custom: boss-warning, revive |
| Weapons (12) | **Custom Figma/SVG authoring** | All 12 custom — cartoon-specific shapes |
| Characters (8) | **Custom Figma/SVG authoring** (cropped from Quaternius head silhouettes) | All 8 custom |
| Achievements (10) | **Kenney Game Icons** | Full coverage |
| Settings (8) | **Kenney Game Icons** + 1 custom | 7/8 covered; need custom: cloud-save icon |

**Total custom UI authoring: ~24 icons.**

## Fonts — Google Fonts (SIL OFL)

| Font | License | Use |
|---|---|---|
| **Fredoka** | OFL | H1/H2 headings + button labels |
| **Nunito** | OFL | Body text + micro-labels |
| **Baloo 2** | OFL | Numerics (HUD, tally, damage numbers) |

All three fetched via Google Fonts; subset to Latin Extended (post-launch: + Turkish, Spanish, Japanese for priority i18n markets).

## VFX particle bases

| Source | License | Use |
|---|---|---|
| **Built in-engine** via URP **VFX Graph** + **Shader Graph** | engine (CC0 for our authored content) | All 30 launch effects |
| Particle textures (sparkle, puff, dust) | **Kenney Particle Pack (CC0)** | Base sprites; tinted in shader |

## Audio — see `08-audio-bible/03-source-shortlist.md`

Defer audio source list to the audio-bible counterpart doc.

## Gap list — assets without a clean CC0 source

These items have **no clean CC0 source** and require **custom authoring** (blender-tech / art-director). Flagged for asset-curator and tech-architect awareness:

| Gap | Asset class | Reason | Owner | Effort estimate |
|---|---|---|---|---|
| 1 | Custom carrot boomerang mesh | No CC0 cartoon carrot in spinning-projectile form | blender-tech | 30 min |
| 2 | Sunbeam weapon visual | Procedural / VFX only — no mesh source needed | art-director (VFX Graph) | 2 hr |
| 3 | Thunder Cloud mesh | Stylized pillowy form not in CC0 packs | blender-tech | 1 hr |
| 4 | Cob Mortar mesh | Corn-cob primitive | blender-tech | 30 min |
| 5 | Tumbleweed mesh | Brambly sphere + smile decal | blender-tech | 45 min |
| 6 | Whirligig mesh | Paper-pinwheel | blender-tech | 1 hr |
| 7 | Beach biome — beach hut | No CC0 hut in our art style | blender-tech | 2 hr |
| 8 | Forest biome — log bridge | Neither pack ships a curved bridge | blender-tech | 1.5 hr |
| 9 | Cavern biome — glow-mushroom emissive variant | Need emissive mask not in source | blender-tech + art-director | 1 hr |
| 10 | Snow biome — igloo | No CC0 igloo in art style | blender-tech | 3 hr |
| 11 | Otter character base (uncertain) | Quaternius pack may lack Otter; fallback Beaver | asset-curator to confirm | 0-2 hr (verify first) |
| 12 | Trash puff-blob base meshes (4) | Custom geometric blob shapes; ≤ 200 tris each | blender-tech | 2 hr total |
| 13 | Boss kitbash additions per biome | Quaternius Monsters base + extra accessories | blender-tech | 2 hr per boss × 5 = 10 hr |
| 14 | 26 custom UI icons (12 weapons + 8 characters + 6 misc) | Cartoon-specific, no CC0 equivalents | art-director (Figma/SVG) | 8 hr total |
| 15 | 2 HUD icons (boss-warning, revive) | Specific glyphs not in Kenney pack | art-director | 30 min total |
| 16 | 1 settings icon (cloud-save) | Specific shape not in Kenney | art-director | 15 min |

### Total custom-authoring estimate: ~37 hr distributed across blender-tech (~25 hr) + art-director (~12 hr).

## Hand-off

- **Asset-curator** stages all CC0 downloads under `assets-raw/quaternius/`, `assets-raw/kenney/`, `assets-raw/kenney-game-icons/`, `assets-raw/google-fonts/`.
- **Blender-tech** owns the gap-list custom authoring under `assets-raw/custom/<class>/`.
- **Art-director** owns custom UI icons + VFX authoring.
- **Tech-architect** validates that CC0 packs ship in formats Unity can import (FBX, GLB, PNG, OGG).
- Every CC0 / OFL / MIT / CC-BY pack is recorded in `LICENSES.md` at repo root + linked from in-game credits screen (per CC-BY attribution requirements when those are present).
- **Open question for asset-curator:** verify Quaternius Otter mesh exists in current pack; if not, file Beaver fallback ADR.
