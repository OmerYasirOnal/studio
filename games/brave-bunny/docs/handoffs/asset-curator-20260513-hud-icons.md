# Hand-off — asset-curator — HUD Icons — 2026-05-13

## Task completed
8 HUD icons integrated into `unity/Assets/_Brave/Art/UI/Icons/`.

## Icons delivered
| Icon file | Source | Method |
|---|---|---|
| `icon_hud_hp.png` | Kenney `plus.png` (CC0) | sips 64×64 |
| `icon_hud_xp.png` | Kenney `star.png` (CC0) | sips 64×64 |
| `icon_hud_timer.png` | Kenney `return.png` (CC0) | sips 64×64 |
| `icon_hud_wave.png` | Kenney `signal3.png` (CC0) | sips 64×64 |
| `icon_hud_kills.png` | Kenney `target.png` (CC0) | sips 64×64 |
| `icon_hud_pause.png` | Kenney `pause.png` (CC0) | sips 64×64 |
| `icon_hud_boss_warning.png` | Custom SVG (MIT in-house) | rsvg-convert 64×64 |
| `icon_hud_revive.png` | Custom SVG (MIT in-house) | rsvg-convert 64×64 |

## Files changed
- `assets-raw/icons/hud/` — 8 PNGs + 2 SVG masters
- `unity/Assets/_Brave/Art/UI/Icons/` — 8 PNGs + 2 SVGs + 10 `.meta` files + folder `.meta`
- `assets-raw/LICENSES.md` — 10 new rows appended
- `docs/07-art-bible/07-iconography.md` — HUD table updated with source filenames

## For HUD agent
Load all 8 by slug: `Resources.Load<Sprite>("_Brave/Art/UI/Icons/icon_hud_<slug>")` (remove
`Assets/` prefix for Resources API). Unity will auto-import the PNGs on next editor refresh;
TextureImporter .meta files pre-configure them as Sprite (UI) with alpha transparency.

## Open items
- Accent tinting (HP→Berry Pink, XP→Cyan, etc.) should be applied at runtime via `Image.color`
  rather than baked into the PNG, keeping the grayscale source reusable.
- `icon_hud_hp` uses a plus/cross shape rather than a heart; if design requires a heart, swap
  with a custom SVG authored like boss-warning/revive.
