# Asset Acquisition Index — Brave Bunny

> Owner: asset-curator. This is a **planning artifact** — nothing has been fetched yet. Every row maps a GDD/art-bible requirement to a specific CC0 / CC-BY / SIL OFL / MIT source per `core/docs/asset-policy.md`. When fetched, each item will append a row to `LICENSES.md` via `core/tools/asset-pipeline/`.
>
> Cross-refs: `docs/07-art-bible/09-source-shortlist.md` (art-director roster), `docs/08-audio-bible/03-source-shortlist.md` (audio roster), `docs/02-gdd/03-characters.md`, `04-weapons.md`, `06-biomes.md`, `core/docs/asset-policy.md` (approved-source list).

## 3D — Characters (8) — Quaternius Animated Animals (CC0)

| Slug | Source | Pack | License | Notes / Blender step |
|---|---|---|---|---|
| bunny | Quaternius | Ultimate Animated Animals | CC0 | Recolor primary fur `#FFF4DC` (Bunny Cream); pink nose dot `#F39FB4` |
| tortoise | Quaternius | Ultimate Animated Animals | CC0 | Jade shell `#A8D86B`, sage body `#6FAE74` |
| fox | Quaternius | Ultimate Animated Animals | CC0 | Deep orange `#FF8A4C` + cream belly, red scarf `#FF6B6B` mesh fragment |
| hedgehog | Quaternius | Ultimate Animated Animals | CC0 | Pale ginger fur `#8B6A4A` + coal spines `#2E2A28` (12-spine pattern) |
| otter | Quaternius (**verify**) / fallback: Beaver | Ultimate Animated Animals | CC0 | River-brown `#6B4A3A` + belly shell decal — **RISK: Otter may not be in pack; fall back to Beaver mesh kitbashed with paddle tail** |
| panda | Quaternius | Ultimate Animated Animals | CC0 | Cream-white `#FFFFFF` body + coal eye-patches `#2E2A28` (mostly native colors) |
| badger | Quaternius | Ultimate Animated Animals | CC0 | High-contrast `#3C3A38` charcoal + `#FFFFFF` head stripe |
| owl | Quaternius | Ultimate Animated Animals (or birds pack) | CC0 | Tawny `#D7C5E8` (Lavender Mist amber-shift) + gold eyes `#FFC83D` |

**Confirmed coverage:** 7/8 directly. **Otter is the one risk** — flagged for fetch-time verification (see Hand-off below).

## 3D — Environment (5 biomes × ~16 chunks = ~80 chunks)

| Biome | Primary source | Pack | Hero props | License |
|---|---|---|---|---|
| Meadow | Kenney + Quaternius | Nature Kit + Platformer Kit + Ultimate Nature Pack | lone tree, wooden well, mushroom cluster | CC0 |
| Beach | Kenney + Quaternius | Platformer Kit + Ultimate Nature Pack (palm) | palm, beach hut (custom), coconut pile | CC0 |
| Forest | Kenney + Quaternius | Nature Kit + Ultimate Nature Pack | ancient oak, log bridge (custom), mushroom ring | CC0 |
| Cavern | Quaternius + Kenney | Cave Kit + Mini Dungeon | stalactite, glow-mushroom (custom emissive), gem outcrop | CC0 |
| Snow | Kenney + Quaternius | Platformer Kit (snow theme) + Ultimate Nature (recolored white) | pine, ice formation, igloo (custom) | CC0 |

## 3D — Enemies (rascals)

Per `05-enemies.md` taxonomy: 5 roles × 5 biomes. Most are recolors of the same base meshes.

| Role | Examples | Source | Notes |
|---|---|---|---|
| Swarmer (trash) | hop-slime, bee-buzz, daisy-bite, crab, bat-mini, snow-rat | Quaternius Ultimate Stylized Nature + Quaternius dungeon-pack slimes + custom Blender daisies/puffs | 4 base meshes recolored 15 ways per biome |
| Tank | sleepy-boar (and Old Boar King base), sleepy-ox, yak, walrus, stone-ox | Quaternius Animated Animals (boar, ox, yak) | Recolor per biome for elite vs boss distinction |
| Ranged | archer-mole, throw-frog, acorn-slinger, crystal-slinger, snowball-mole | Quaternius Monsters Pack + Animated Animals (mole, frog) | Per-biome recolor + prop swap (slingshot → beanbag → snowball) |
| Elite | Big Onion, Big Hermit Crab, Treant-Sprout, Stalagmite-Walker, Snow Yeti-Cub | Quaternius Monsters Pack | Bigger scale + biome accessory (crown, shell, gem, spines, frost-rim) |
| Boss | Old Boar King, Crab Captain, Mama Oak, Sneaky Cave Mole, Big Snow-yeti | Quaternius Animated Animals + Monsters Pack | Kitbash: crown prop (Boar), giant pincer (Crab), root-arms (Oak), burrow VFX (Mole), ice-mane (Yeti) |

## 3D — Weapons (12) + Pickups

| Weapon | Source / Blender | License |
|---|---|---|
| Carrot Boomerang | **Custom Blender** (no clean CC0 carrot in spinning-projectile form; ≤120 tris) | CC0 (we author) |
| Sunbeam | **Pure VFX shader** (URP Shader Graph, no mesh) | CC0 (we author) |
| Daisy Mine | Quaternius Ultimate Nature (daisy prop) + Blender wobble anim | CC0 |
| Pebble Sling | Kenney Generic Items (or trivial Blender sphere) | CC0 |
| Honey Aura | **Pure VFX shader** (no mesh) | CC0 (we author) |
| Acorn Cannon | Quaternius Ultimate Nature (acorn) + Blender cannon-barrel mesh | CC0 |
| Thunder Cloud | **Custom Blender** (stylized pillowy cloud) | CC0 (we author) |
| Frost Whisper | **Pure VFX shader** (mist aura, no mesh) | CC0 (we author) |
| Cob Mortar | **Custom Blender** (corn-cob primitive) | CC0 (we author) |
| Beehive | Kenney Nature Kit (wooden hive prop) + Blender bee meshes | CC0 |
| Tumbleweed | **Custom Blender** (brambly sphere + smile decal) | CC0 (we author) |
| Whirligig | **Custom Blender** (paper-pinwheel primitive) | CC0 (we author) |

**Custom-Blender weapon count: 6** (Carrot Boomerang, Thunder Cloud, Cob Mortar, Tumbleweed, Whirligig, and the Sunbeam/Honey/Frost VFX assets — no mesh but custom-authored shaders).

**Pickups:** XP gem (Quaternius gem), gold coin (Kenney coin), heart (Kenney heart). All CC0.

## Textures / PBR

| Asset | Source | License | Use |
|---|---|---|---|
| Grass | ambientCG.com | CC0 | Meadow ground |
| Sand | ambientCG.com | CC0 | Beach ground |
| Bark | ambientCG.com | CC0 | Forest hero-oak |
| Stone | ambientCG.com | CC0 | Cavern floor |
| Snow | ambientCG.com | CC0 | Snow biome ground |
| Mud | ambientCG.com | CC0 | Forest wet patches |
| HDRI (5 sky variants) | Polyhaven.com | CC0 | 1 per biome — Meadow noon, Beach gold, Forest dapple, Cavern dark, Snow overcast |

## Audio — BGM (12 tracks)

Per `08-audio-bible/01-bgm-spec.md` + `03-source-shortlist.md`.

| # | State | Source / Track candidate | License |
|---|---|---|---|
| 1 | Home / Lobby | Pixabay royalty-free ("cozy acoustic loop", "happy ukulele") | RF |
| 2 | Splash / Title cold-start | Custom LMMS (we author 5s stinger) | CC0 |
| 3 | Run — Meadow | Incompetech (Kevin MacLeod "Wallpaper" / "Mister Exposition" candidates) | CC-BY 4.0 |
| 4 | Run — Beach | Pixabay + Incompetech fallback (ukulele/marimba) | mixed (RF or CC-BY) |
| 5 | Run — Forest | Incompetech ("Dreamer" / "Sneaky Snitch" candidates) | CC-BY 4.0 |
| 6 | Run — Cavern | Free Music Archive (CC0 ambient/ethereal filter) | CC0 |
| 7 | Run — Snow | Pixabay royalty-free (bell-layer biased) | RF |
| 8 | Boss — Meadow | Incompetech ("Volatile Reaction" candidate, retoned major) | CC-BY 4.0 |
| 9 | Run-end win | Pixabay stinger pack (8-sec fanfare) | RF |
| 10 | Run-end lose | Pixabay or custom LMMS (gentle descending pluck) | RF or CC0 |
| 11 | Battle pass screen | Pixabay royalty-free (soft pad reward loop) | RF |
| 12 | Lobby tempo-up variant | Pixabay or derived from Track 1 | RF |

**CC-BY count: 3-4 tracks** (Tracks 3, 5, 8 confirmed CC-BY; Track 4 conditional). Each **must** be recorded in `LICENSES.md` with author + URL + license URL, and ui-engineer must surface them on the in-game credits screen (per `04-monetization-and-iap.md` settings flow → credits, and `core/docs/asset-policy.md` §Attribution).

## Audio — SFX (~50 SFX, ~92 files with round-robin)

Per `08-audio-bible/02-sfx-spec.md` + `03-source-shortlist.md`.

| Bucket | Primary source | License |
|---|---|---|
| UI clicks / pops / chimes | Kenney UI Audio + Kenney Interface Sounds | CC0 |
| Coin / currency pickups | Kenney Game Casino Audio | CC0 |
| Combat hits / thuds / poofs | Freesound (CC0 filter: "cartoon hit", "poof", "thump") | CC0 |
| Pickup chimes / sparkles | Freesound CC0 ("ukulele", "pluck", "bell") + Kenney Interface | CC0 |
| Enemy death poofs | Freesound CC0 ("puff", "cartoon pop") + pitch-shift variants | CC0 |
| Boss stingers (intro / phase / death) | Freesound CC0 ("orchestral hit", "cute fanfare") | CC0 |
| Hero pip / "ouch" | Freesound CC0 ("cute creature", "cartoon animal") — NOT human voice | CC0 |
| Weapon SFX (carrot fire, sunbeam hum, daisy explode) | Freesound CC0 + custom layered Audacity edits | CC0 |
| Ambient biome beds | Freesound CC0 ("meadow ambient loop", "cavern drip", "beach wave loop") | CC0 |
| Endgame fanfares / wind-downs | Freesound CC0 + Pixabay | mixed |
| Meta unlocks (character, weapon, pass) | Kenney Interface Sounds + Freesound CC0 | CC0 |

**Freesound user shortlist for asset-curator:** Kenney.nl audio packs (UI Audio, Interface, Impact, Sci-Fi cartoon subset), user `LittleRobotSoundFactory`, user `Sonic-Sentinel`. All filtered to **CC0 only**.

## Fonts (Google Fonts, SIL OFL)

| Font | License | Use |
|---|---|---|
| Fredoka | SIL OFL 1.1 | H1/H2 headings + button labels |
| Nunito | SIL OFL 1.1 | Body text + micro-labels |
| Baloo 2 | SIL OFL 1.1 | Numerics (HUD, tally, damage numbers) |

All three fetched via Google Fonts; subset to Latin Extended initially.

## Icons

| Source | License | Coverage |
|---|---|---|
| Kenney Game Icons pack | CC0 | ~52% of catalog (currency 3/3, nav 5/5, HUD 6/8, achievements 10/10, settings 7/8) |
| Custom Figma/SVG (MIT under brave-bunny repo) | MIT | ~48% — all 12 weapon icons, all 8 character icons, 2 HUD (boss-warning, revive), 1 settings (cloud-save) |

**Total custom UI icons: ~24** (per `07-art-bible/07-iconography.md`).

## VFX particle bases

| Source | License | Use |
|---|---|---|
| URP VFX Graph + Shader Graph (in-engine authored) | CC0 (we author) | All 30 launch effects |
| Kenney Particle Pack | CC0 | Base sparkle / puff / dust sprites; tinted in shader |

## Planned acquisition totals

| Category | Planned count | Source breakdown |
|---|---|---|
| 3D characters | 8 | Quaternius (7 confirmed, 1 risk: Otter) |
| 3D environment chunks | ~80 | Kenney + Quaternius + custom (~10) |
| 3D enemies | ~25 | Quaternius (recolors of ~6 base meshes) |
| 3D weapons | 12 | Quaternius + Kenney (6) + Custom Blender (6) |
| Pickups | 3 | Quaternius + Kenney |
| PBR textures | 6 | ambientCG |
| HDRI skies | 5 | Polyhaven |
| BGM tracks | 12 | Pixabay (7) + Incompetech CC-BY (3-4) + Custom (1-2) + FMA (1) |
| SFX files | ~92 (50 SFX × RR) | Kenney UI/Interface (~70%) + Freesound CC0 (~30%) |
| Fonts | 3 | Google Fonts |
| UI icons | ~50 | Kenney (~26) + Custom MIT (~24) |
| VFX particle sprites | ~10 | Kenney Particle Pack |
| **Total planned files** | **~310** | **~95% CC0, ~3% CC-BY, ~2% SIL OFL/MIT** |

## Hand-off

- All CC0 packs fetched via `core/tools/asset-pipeline/quaternius-fetch.py`, `kenney-fetch.py`, etc. (each script appends a row to `LICENSES.md` automatically).
- **Open verification at fetch time:** Quaternius Otter mesh — if absent, file ADR `decisions/NNNN-otter-beaver-fallback.md` and proceed with Beaver kitbash.
- CC-BY tracks: log artist + work + license URL in `LICENSES.md` and notify ui-engineer to include in credits screen.
- Custom-Blender items (6 weapons + ~10 environment props + 4 trash-puff bases) land under `assets-raw/3d/custom-blender/`, owned by blender-tech, gap-list in `07-art-bible/09-source-shortlist.md` §Gap list.
- This INDEX is the single source of truth for procurement; if the GDD or art bible changes, update this file *before* fetching.
