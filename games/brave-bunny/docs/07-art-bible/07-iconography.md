# Iconography — Brave Bunny

> Owner: art-director. Cross-refs: `06-ui-visual-direction.md` (icon usage in buttons and cards), `01-color-palette.md` (palette source — mono + accent), `core/docs/asset-policy.md` (CC0 only). Primary CC0 source: **Kenney Game Icons (CC0)** for raster bases; **custom Blender / Figma authoring** only where Kenney lacks coverage (e.g., character-portrait icons).

## Production rules

| Rule | Value | Notes |
|---|---|---|
| Base canvas | **24 × 24 px** | All icons author at this size; export 1x/2x/3x for Unity UI Toolkit |
| Internal padding | 2 px on all sides | Visual content lives in 20 × 20 px |
| Line weight | **2 px** solid stroke | No tapered strokes, no double-stroke |
| Style | Monoline + single fill accent | Stays consistent with `06-ui-visual-direction.md` |
| Default color | Coal Outline `#2E2A28` on light bg; `#FFFFFF` on dark bg | Sample from `01-color-palette.md` |
| Accent fill | One palette accent per icon (Pickup Gold, Hero Highlight, Rare Drop Cyan, Danger Red) | Used sparingly to flag importance |
| Format | **SVG primary** + PNG @ 1x/2x/3x fallback | SVG for vector scaling; PNG for Unity UI fallback |
| Filename slug | `icon_<category>_<slug>.svg` | e.g. `icon_currency_gold.svg` |

## Icon catalog by category

### Currency (3 icons)

| Slug | Use | Accent fill |
|---|---|---|
| `icon_currency_gold` | Soft currency (gold coin) | Pickup Gold `#FFC83D` |
| `icon_currency_stars` | Premium currency (stars) | Hero Highlight `#FF6B6B` |
| `icon_currency_pass` | Battle Pass XP | Rare Drop Cyan `#3DE0E0` |

### Navigation (5 icons)

| Slug | Use | Accent fill |
|---|---|---|
| `icon_nav_home` | Home / lobby | mono Coal Outline |
| `icon_nav_play` | Start run | Hero Highlight `#FF6B6B` |
| `icon_nav_pass` | Battle Pass tab | Rare Drop Cyan `#3DE0E0` |
| `icon_nav_store` | Store / IAP tab | Pickup Gold `#FFC83D` |
| `icon_nav_settings` | Settings tab | mono Coal Outline |

### HUD (8 icons)

| Slug | Use | Accent fill | Source file |
|---|---|---|---|
| `icon_hud_hp` | HP indicator (health/plus) | Berry Pink `#F39FB4` | `PNG/Black/1x/plus.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_xp` | XP gem indicator | Rare Drop Cyan `#3DE0E0` | `PNG/Black/1x/star.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_timer` | Run timer (circular return arrow) | mono Coal Outline | `PNG/Black/1x/return.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_wave` | Wave counter (signal bars) | mono Coal Outline | `PNG/Black/1x/signal3.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_kills` | Kill counter (target/crosshair) | mono Coal Outline | `PNG/Black/1x/target.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_pause` | Pause button | mono Coal Outline | `PNG/Black/1x/pause.png` from kenney_game-icons.zip (CC0); resized 64×64 |
| `icon_hud_boss_warning` | Boss incoming (danger triangle + skull) | Danger Red `#E83C3C` | Custom hand-authored SVG `boss-warning.svg` (MIT in-house); rsvg-convert → PNG 64×64 |
| `icon_hud_revive` | Revive offer (heart + upward arrow) | Hero Highlight `#FF6B6B` | Custom hand-authored SVG `revive.svg` (MIT in-house); rsvg-convert → PNG 64×64 |

### Weapons (12 icons — 1 per weapon from `docs/02-gdd/04-weapons.md`)

| Slug | Weapon |
|---|---|
| `icon_weapon_carrot-boomerang` | Carrot Boomerang |
| `icon_weapon_sunbeam` | Sunbeam |
| `icon_weapon_daisy-mine` | Daisy Mine |
| `icon_weapon_pebble-sling` | Pebble Sling |
| `icon_weapon_honey-aura` | Honey Aura |
| `icon_weapon_acorn-cannon` | Acorn Cannon |
| `icon_weapon_thunder-cloud` | Thunder Cloud |
| `icon_weapon_frost-whisper` | Frost Whisper |
| `icon_weapon_cob-mortar` | Cob Mortar |
| `icon_weapon_beehive` | Beehive |
| `icon_weapon_tumbleweed` | Tumbleweed |
| `icon_weapon_whirligig` | Whirligig |

All weapon icons mono Coal Outline with biome-tinted accent on the "active" slot in HUD.

### Characters (8 icons — 1 per character from `docs/02-gdd/03-characters.md`)

Character icons are **portrait-style** rather than monoline — each shows the head silhouette of the animal with one accent color matching the character's primary fur per `03-character-style.md`. Size 24 × 24 base; also rendered at 64 × 64 for character-select tiles.

| Slug | Character |
|---|---|
| `icon_char_bunny` | Bunny |
| `icon_char_tortoise` | Tortoise |
| `icon_char_hedgehog` | Hedgehog |
| `icon_char_fox` | Fox |
| `icon_char_otter` | Otter |
| `icon_char_panda` | Panda |
| `icon_char_badger` | Badger |
| `icon_char_owl` | Owl |

### Achievements (10 category icons)

| Slug | Use |
|---|---|
| `icon_ach_combat` | Kill-based achievements |
| `icon_ach_survival` | Time/wave survival |
| `icon_ach_pickup` | Pickup collection |
| `icon_ach_levelup` | Level milestones |
| `icon_ach_evolution` | Weapon evolutions |
| `icon_ach_boss` | Boss defeats |
| `icon_ach_streak` | Daily streak |
| `icon_ach_explorer` | Biome discovery |
| `icon_ach_collector` | Character/weapon unlocks |
| `icon_ach_mastery` | Per-character mastery |

### Settings (8 icons)

| Slug | Use |
|---|---|
| `icon_set_audio` | Audio settings |
| `icon_set_haptics` | Haptics toggle |
| `icon_set_graphics` | Graphics quality |
| `icon_set_language` | Language picker |
| `icon_set_account` | Account / sign-in |
| `icon_set_cloud` | Cloud save status |
| `icon_set_support` | Help / contact |
| `icon_set_credits` | Credits / licenses |

## "New unlock" badge spec

A persistent indicator that something inside the destination needs the player's attention.

| Property | Value |
|---|---|
| Shape | Solid circle |
| Diameter | 8 px |
| Color | Pickup Gold `#FFC83D` (always — even on weapon/char icons) |
| Position | Top-right of icon, with 1 px overlap onto icon edge |
| Animation | 1 Hz subtle pulse: scale 1.0 ↔ 1.15 over 1 s, ease-in-out |

## Locked icons

| Property | Value |
|---|---|
| Base | Original icon, **50% grayscale** (HSV S → 0.50) |
| Overlay | Padlock glyph 12 × 12 px, Coal Outline `#2E2A28`, centered |
| Background | If on card: tint card fill 8% darker |
| Tap behavior | Plays "locked shake" (Pillar 5: 3 px horizontal, 2 oscillations, 180 ms) + opens unlock requirements tooltip |

## Tier indicators (rarity rings)

Used on draft cards, store SKUs, weapon-level indicators.

| Tier | Ring color | Ring stroke | Notes |
|---|---|---|---|
| Common | `#9FA5A8` (Stone Gray) | 2 px | Most pickups + L1 weapons |
| Rare | `#3DE0E0` (Rare Drop Cyan) | 2 px | L3+ weapons, rare drops |
| Epic | `#A855F7` (purple — new addition, derived from Lavender Mist family) | 2 px | L5 weapons, evolutions |
| Legendary (reserved) | `#FFC83D` (Pickup Gold) | 3 px | Battle Pass capstone rewards |

Single-stroke ring only — no double rings, no animated rings (animated rings break GPU instancing on UI atlas).

## Source notes

- **Kenney Game Icons (CC0)** covers: navigation (5/5), HUD (6/8 — boss warning + revive need custom), settings (7/8 — cloud icon needs custom).
- **Custom authoring needed** (asset-curator owns sourcing, blender-tech / Figma authoring):
  - All 12 weapon icons (Kenney has generic weapons but not our cartoon-specific ones)
  - All 8 character portrait icons (animal-specific, recolored from Quaternius head crops)
  - 2 HUD icons: `icon_hud_boss_warning`, `icon_hud_revive`
  - 1 settings icon: `icon_set_cloud`
- **Total icon count for launch: 54 unique icons** (3 + 5 + 8 + 12 + 8 + 10 + 8).
- **Kenney coverage: ~28 icons** (~52%). Custom: ~26 icons (~48%).

## Hand-off

- Icon source FBX/PNG raw assets land in `assets-raw/kenney/game-icons/` and `assets-raw/custom/icons/`.
- SVG masters live in `unity/Assets/Art/UI/Icons/svg/`; baked PNG @ 1x/2x/3x in adjacent folders.
- Asset-curator stages Kenney downloads; blender-tech / art-director authors the custom 26.
- Open question for ui-engineer: confirm Unity UI Toolkit's SVG package handles our 24 × 24 atlasing without blowing the UI DC budget (≤8 DC).
