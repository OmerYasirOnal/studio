# Asset license ledger — `Brave Bunny`

Every asset under `assets-raw/` must appear in this table. asset-curator maintains it. `licenses.py --validate` runs in CI to enforce.

## Fetched assets

| File (relative to assets-raw/) | Source | License | URL | Author | Fetched |
|---|---|---|---|---|---|
| 3d/environment/meadow/kenney_nature-kit.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/nature-kit/8334871c74-1677698939/kenney_nature-kit.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| 3d/environment/platformer/kenney_platformer-kit.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/platformer-kit/9fd25e14aa-1775122253/kenney_platformer-kit.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| 3d/environment/cavern/kenney_mini-dungeon.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/mini-dungeon/2de2de674e-1771249391/kenney_mini-dungeon.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| icons/kenney-game-icons/kenney_game-icons.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/game-icons/94af1f5c0b-1677661579/kenney_game-icons.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| ui/kenney-ui-pack/kenney_ui-pack.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/ui-pack/af874291da-1718203990/kenney_ui-pack.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| audio/sfx/ui-audio/kenney_ui-audio.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/ui-audio/e19c9b1814-1677590494/kenney_ui-audio.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| audio/sfx/interface-sounds/kenney_interface-sounds.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/interface-sounds/d23a84242e-1677589452/kenney_interface-sounds.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| audio/sfx/casino-audio/kenney_casino-audio.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/casino-audio/f578a13f51-1721639069/kenney_casino-audio.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| audio/sfx/impact-sounds/kenney_impact-sounds.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/impact-sounds/8aa7b545c9-1677589768/kenney_impact-sounds.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| vfx/kenney-particle-pack/kenney_particle-pack.zip | Kenney | CC0 1.0 | https://kenney.nl/media/pages/assets/particle-pack/1dd3d4cbe2-1677578741/kenney_particle-pack.zip | Kenney (Asbjørn Thirslund) | 2026-05-12 |
| fonts/Fredoka-Regular.ttf | Google Fonts | SIL OFL 1.1 | https://fonts.google.com/specimen/Fredoka | Milena Brandão et al. | 2026-05-12 |
| fonts/Nunito-Regular.ttf | Google Fonts | SIL OFL 1.1 | https://fonts.google.com/specimen/Nunito | Vernon Adams et al. | 2026-05-12 |
| fonts/Baloo2-Regular.ttf | Google Fonts | SIL OFL 1.1 | https://fonts.google.com/specimen/Baloo+2 | Ek Type | 2026-05-12 |
| hdri/kloofendal_48d_partly_cloudy_puresky_2k.hdr | Polyhaven | CC0 1.0 | https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky | Greg Zaal | 2026-05-12 |
| hdri/belfast_sunset_puresky_2k.hdr | Polyhaven | CC0 1.0 | https://polyhaven.com/a/belfast_sunset_puresky | Andreas Mischok | 2026-05-12 |
| hdri/kiara_1_dawn_2k.hdr | Polyhaven | CC0 1.0 | https://polyhaven.com/a/kiara_1_dawn | Greg Zaal | 2026-05-12 |
| hdri/autumn_field_puresky_2k.hdr | Polyhaven | CC0 1.0 | https://polyhaven.com/a/autumn_field_puresky | Jarod Guest, Sergej Majboroda | 2026-05-12 |
| textures/ambientcg/Grass001_2K-JPG.zip | ambientCG | CC0 1.0 | https://ambientcg.com/view?id=Grass001 | Lennart Demes (ambientCG) | 2026-05-12 |
| textures/ambientcg/Bark004_2K-JPG.zip | ambientCG | CC0 1.0 | https://ambientcg.com/view?id=Bark004 | Lennart Demes (ambientCG) | 2026-05-12 |
| textures/ambientcg/Rock029_2K-JPG.zip | ambientCG | CC0 1.0 | https://ambientcg.com/view?id=Rock029 | Lennart Demes (ambientCG) | 2026-05-12 |
| textures/ambientcg/Snow006_2K-JPG.zip | ambientCG | CC0 1.0 | https://ambientcg.com/view?id=Snow006 | Lennart Demes (ambientCG) | 2026-05-12 |
| textures/ambientcg/Ground054_2K-JPG.zip | ambientCG | CC0 1.0 | https://ambientcg.com/view?id=Ground054 | Lennart Demes (ambientCG) | 2026-05-12 |

## Best-effort fetches attempted (2026-05-12)

This section documents the asset-curator's first real pass at fetching the planned roster from `INDEX.md`. Status legend:

- OK = file is on disk + LICENSES.md row exists above
- MANUAL = URL identified and CC0-confirmed, but the source requires interactive download (click-through, captcha, or login). To be fetched by a human and re-recorded here.
- UNAVAILABLE = source 404'd or moved at the time of the fetch pass; alternative noted.

### Fetched OK (22 files)

- 10 Kenney CC0 packs (Nature Kit, Platformer Kit, Mini Dungeon, Game Icons, UI Pack, UI Audio, Interface Sounds, Casino Audio, Impact Sounds, Particle Pack)
- 3 Google Fonts (Fredoka, Nunito, Baloo 2 regular weights)
- 4 Polyhaven HDRIs (Meadow / Beach / Forest / Snow sky candidates — Cavern still TODO)
- 5 ambientCG PBR textures (Grass, Bark, Rock, Snow, Ground)

### Manual download required (URL + license confirmed, fetch script not applicable)

| Planned asset | Source page | License | Reason |
|---|---|---|---|
| Quaternius Ultimate Animated Animals (8 characters) | https://quaternius.com/packs/ultimateanimatedanimals.html | CC0 (page-confirmed) | Page exposes a download button but no static .zip URL discoverable from server-rendered HTML — likely client-side JS or external host (itch.io / gumroad). asset-curator could not extract a direct URL via WebFetch. ACTION: human or future Quaternius API fetch. |
| Quaternius Ultimate Stylized Nature | https://quaternius.com/packs/ — index page only | CC0 (likely, per Quaternius blanket license) | Index page 404'd at /packs.html — need to crawl /packs/ root or use Quaternius homepage cards. |
| Quaternius Monsters Pack (enemies — swarmer/elite/boss base meshes) | https://quaternius.com/packs/ | CC0 (likely) | Same as above. |
| Kenney Cave Kit | https://kenney.nl/assets/cave-kit | unknown | Page 404'd. ALTERNATIVE: Mini Dungeon (already fetched) covers cavern biome. Cave Kit may be deprecated or renamed; recheck Kenney library for current cave-themed pack. |
| Incompetech BGM (3-4 CC-BY tracks: Wallpaper, Mister Exposition, Dreamer, Sneaky Snitch, Volatile Reaction) | https://incompetech.com/music/royalty-free/ | CC-BY 4.0 | Each track has a download button but URLs are user-search-driven; needs per-track WebFetch of https://incompetech.com/music/royalty-free/music.html plus per-track license capture. Not blocked, but a real curation pass (artist + title + license URL per row). |
| Pixabay BGM (~7 tracks: cozy acoustic loop, ukulele, etc.) | https://pixabay.com/music/ | Pixabay Content License (RF) | Pixabay requires JS-driven search and login-free downloads work but URLs are not server-rendered for the bot. Per-track human pick recommended. |
| Free Music Archive ambient (Cavern run track) | https://freemusicarchive.org/ | varies — must filter CC0 | Per-track human pick. |
| Freesound CC0 SFX (~30 files: combat hits, poofs, bell chimes, hero pip, ambient beds) | https://freesound.org/search/?f=license:%22Creative+Commons+0%22 | CC0 | Direct downloads on Freesound require a free OAuth API token. Per-sound page URL must be human-selected (no autopick — relevance/quality varies). Use `freesound-fetch.py` once specific sounds are chosen. |

### Unavailable / moved (note alternative)

| Planned asset | Source attempted | Outcome | Alternative |
|---|---|---|---|
| Kenney Cave Kit | https://kenney.nl/assets/cave-kit | 404 | Mini Dungeon (already fetched) or search Kenney library for `cavern` / `dungeon-remastered`. |
| Quaternius packs.html index | https://quaternius.com/packs.html | 404 | Use https://quaternius.com/ home cards instead. |
| Quaternius Otter (specific animal) | https://quaternius.com/packs/ultimateanimatedanimals.html | Page confirmed CC0 but does NOT list per-animal roster on the page itself; INDEX.md flagged otter as a risk. | If Otter mesh absent after pack inspection, file ADR `decisions/NNNN-otter-beaver-fallback.md` and proceed with Beaver kitbash per INDEX hand-off. |

## Planned acquisitions (not yet fetched)

The "Best-effort fetches attempted" section above supersedes the original planning list. Open items per `INDEX.md`:

- 8 Quaternius character meshes (MANUAL: pack URL confirmed CC0; per-animal verification needed)
- ~25 Quaternius enemy meshes (MANUAL: same)
- ~10 Quaternius environment hero props (MANUAL: same)
- 1 Cavern HDRI (TODO: pick a Polyhaven cave or dark-interior HDRI)
- 1 Mud PBR texture (TODO: pick from ambientCG, e.g. `Ground037`, `Mud002`)
- 12 BGM tracks (MANUAL: per-track curation across Pixabay / Incompetech / FMA)
- ~30 Freesound CC0 SFX (MANUAL: per-sound curation)
- 6 custom-Blender weapons (owner: blender-tech, not asset-curator)
- ~24 custom MIT UI icons (owner: ui-engineer, not asset-curator)

## Allowed licenses

- CC0 1.0 Universal
- CC-BY 4.0 (attribution required — record in this table)
- MIT
- SIL OFL 1.1 (fonts)

## Allowed sources

- Quaternius (quaternius.com) — CC0
- Kenney (kenney.nl) — CC0
- Poly.pizza — CC0 aggregator
- OpenGameArt.org — CC0 filter
- Sketchfab — CC0 filter only
- ambientCG.com — CC0 PBR
- Polyhaven.com — CC0 textures/HDRI
- Freesound.org — CC0 filter
- Pixabay Music — royalty-free
- Incompetech / Kevin MacLeod — CC-BY (attribution recorded here)
- FMA (Free Music Archive) — CC0 filter
- Google Fonts — SIL OFL

Adding a source not on this list requires an ADR proposing its addition.
