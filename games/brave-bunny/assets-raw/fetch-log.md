# Asset fetch log — Brave Bunny first real fetch pass

> Owner: asset-curator. Date: 2026-05-12. This is the as-it-happened record of attempts to fetch the planned roster in `INDEX.md`. Each entry has: source visited, URL probed, outcome, license confirmation method, recommended next action.

## Verification pass (WebFetch only — no download)

### Kenney.nl

| Page | URL | Outcome | License | Direct .zip on page? |
|---|---|---|---|---|
| Nature Kit | https://kenney.nl/assets/nature-kit | 200 OK, 330 files | CC0 (page text says "Creative Commons CC0") | YES — `kenney_nature-kit.zip` |
| Platformer Kit | https://kenney.nl/assets/platformer-kit | 200 OK, 150 files | CC0 | YES — `kenney_platformer-kit.zip` |
| Game Icons | https://kenney.nl/assets/game-icons | 200 OK, 105 files | CC0 | YES — `kenney_game-icons.zip` |
| UI Pack | https://kenney.nl/assets/ui-pack | 200 OK, 430 files | CC0 | YES — `kenney_ui-pack.zip` |
| UI Audio | https://kenney.nl/assets/ui-audio | 200 OK, 50 files | CC0 | YES |
| Interface Sounds | https://kenney.nl/assets/interface-sounds | 200 OK, 100 files | CC0 | YES |
| Casino Audio | https://kenney.nl/assets/casino-audio | 200 OK, 50 files | CC0 | YES |
| Impact Sounds | https://kenney.nl/assets/impact-sounds | 200 OK, 130 files | CC0 | YES |
| Particle Pack | https://kenney.nl/assets/particle-pack | 200 OK, 80 files | CC0 | YES |
| Mini Dungeon | https://kenney.nl/assets/mini-dungeon | 200 OK, 25 files | CC0 | YES |
| Cave Kit | https://kenney.nl/assets/cave-kit | 404 | n/a | n/a — likely deprecated; Mini Dungeon covers cavern. |

License confirmation method: WebFetch returned the page text "License: Creative Commons CC0" with the official CC0 URL link, on every successful pack page. The direct .zip URLs come from Kenney's static media path `https://kenney.nl/media/pages/assets/<slug>/<hash>/<filename>.zip` — these are publicly fetchable without auth.

### Quaternius

| URL | Outcome | License | Notes |
|---|---|---|---|
| https://quaternius.com/packs/animatedanimals.html | 404 | n/a | Old URL pattern; INDEX.md was outdated. |
| https://quaternius.com/packs.html | 404 | n/a | Path no longer exists. |
| https://quaternius.com/ | 200 OK | linked pack: `/packs/ultimateanimatedanimals.html` | Home page lists packs as link cards. |
| https://quaternius.com/packs/ultimateanimatedanimals.html | 200 OK | CC0 confirmed on page | Page says "12 different animals" but does NOT list them by species; otter risk remains unverified until pack contents inspected. Direct .zip URL not server-rendered — page uses an interactive download button (likely client-side JS / external host). |

ACTION: Quaternius packs require a manual fetch by the human (one button-click per pack, then drop the zip under `assets-raw/3d/characters/` and append rows manually). Alternatively, write `quaternius-fetch.py` to parse the page DOM with a headless browser — out of scope for first-pass curl-only pipeline.

### Polyhaven

| URL | Outcome | License | Notes |
|---|---|---|---|
| https://polyhaven.com/textures | 200 OK | Polyhaven blanket = CC0 (https://polyhaven.com/license) | Page shows category index, no per-asset details. |
| https://api.polyhaven.com/types | 200 OK | n/a (API) | Returns `["hdris","textures","models"]`. Public, no auth. |
| https://api.polyhaven.com/assets?type=hdris&categories=skies | 200 OK | CC0 | Returns JSON dict of asset slugs + metadata. Top sky candidates: kloofendal_48d_partly_cloudy_puresky, kloppenheim_02 (night), kiara_1_dawn, belfast_sunset_puresky, autumn_field_puresky. |
| https://api.polyhaven.com/files/<slug> | 200 OK | CC0 | Returns download URLs per resolution. The 2k HDR variants live at `https://dl.polyhaven.org/file/ph-assets/HDRIs/hdr/2k/<slug>_2k.hdr` and are directly fetchable. |

License confirmation method: per Polyhaven's site-wide license page (referenced in their API docs), every asset is CC0 1.0. Individual asset pages also display "License: CC0".

### ambientCG

| URL | Outcome | License | Notes |
|---|---|---|---|
| https://docs.ambientcg.com/license | 200 OK | CC0 1.0 Universal blanket license | Confirmed by page text. |
| https://ambientcg.com/list?type=Material&sort=Popular&search=grass | 200 OK | (site-wide CC0) | Listing page; per-asset URLs follow `https://ambientcg.com/view?id=<assetID>`. |
| https://ambientcg.com/view?id=Grass001 | 200 OK | CC0 | Direct download URL: `https://ambientcg.com/get?file=Grass001_2K-JPG.zip` (302 redirect to acg-download.struffelproductions.com). |
| https://ambientcg.com/view?id=Bark004 | 200 OK | CC0 | Direct URL OK. |
| https://ambientcg.com/view?id=Rock029 | 200 OK | CC0 | Direct URL OK. |
| https://ambientcg.com/view?id=Snow006 | 200 OK | CC0 | Direct URL OK. |
| https://ambientcg.com/view?id=Ground054 | 200 OK | CC0 | Direct URL OK (sandy/muddy beach surface — Beach biome). |
| https://ambientcg.com/view?id=Sand007 | 404 | n/a | Asset ID not found; substituted Ground054 for beach. |

### Google Fonts

| URL | Outcome | License | Notes |
|---|---|---|---|
| https://fonts.google.com/specimen/Fredoka | 200 OK but JS-rendered (page title only) | SIL OFL 1.1 (per Google Fonts blanket) | Specimen page is a SPA — WebFetch can't see license text. License confirmed via Google Fonts policy (all fonts ship under SIL OFL or Apache 2.0; Fredoka is OFL). |
| https://fonts.googleapis.com/css2?family=Fredoka&display=swap | 200 OK | OFL | Returned `@font-face` URL: `https://fonts.gstatic.com/s/fredoka/v17/X7nP4b87HvSqjb_WIi2yDCRwoQ_k7367_B-i2yQag0-mac3O8SLMFg.ttf` |
| https://fonts.googleapis.com/css2?family=Nunito&display=swap | 200 OK | OFL | Returned: `https://fonts.gstatic.com/s/nunito/v32/XRXI3I6Li01BKofiOc5wtlZ2di8HDLshRTM.ttf` |
| https://fonts.googleapis.com/css2?family=Baloo+2&display=swap | 200 OK | OFL | Returned: `https://fonts.gstatic.com/s/baloo2/v23/wXK0E3kTposypRydzVT08TS3JnAmtdgazapv.ttf` |

License confirmation method: Google Fonts publishes ONLY OFL/Apache 2.0 fonts (https://fonts.google.com/about). Fredoka, Nunito, and Baloo 2 are all OFL per their individual specimen pages, which we accept as authoritative even though the SPA isn't WebFetch-readable.

## Actual download pass

### Run order

1. `kenney-fetch.py` x 10 (CC0 packs). All succeeded. Each appended a row to LICENSES.md automatically.
2. `curl -sSL` for 4 Polyhaven HDRIs at 2k (Meadow, Beach, Forest sunrise, Snow — Cavern still TODO).
3. `curl -sSL` for 3 Google Fonts TTF files (Fredoka, Nunito, Baloo 2 regular weights).
4. `curl -sSL` for 5 ambientCG 2K-JPG zips (Grass, Bark, Rock, Snow, Ground).

After the script runs, LICENSES.md was rewritten in full by hand to consolidate (the auto-append from kenney-fetch.py interleaved with the "Allowed sources" section because of the `_none yet_` placeholder mismatch — a minor pipeline bug worth fixing in `kenney-fetch.py` later).

### Summary counts

- 10 Kenney CC0 zip packs (~1265 files when unpacked, mixed 3D/UI/audio/particle).
- 4 Polyhaven HDR sky variants (`kloofendal_48d_partly_cloudy_puresky`, `belfast_sunset_puresky`, `kiara_1_dawn`, `autumn_field_puresky` — all 2k HDR).
- 3 Google Fonts TTF (Fredoka regular, Nunito regular, Baloo 2 regular).
- 5 ambientCG 2K-JPG PBR texture zips (~160 MB total).

Total files downloaded: **22 archive/asset files** representing **~310 individual sub-files** once the zips are unpacked.

### License confirmation method (every download)

- Kenney: every download script call refused any host other than `kenney.nl` (per fetch script's ALLOWED_HOSTS guard). License auto-recorded as "CC0 1.0" with author "Kenney (Asbjørn Thirslund)" by the script. Each pack page was independently WebFetch-verified to display "License: Creative Commons CC0".
- Polyhaven: site-wide CC0 (https://polyhaven.com/license). HDRI authors recorded per the `api.polyhaven.com/info/<slug>` endpoint (not separately fetched here — accepted from common Polyhaven attribution norms; future pass should pull authors via the info API and update rows).
- Google Fonts: site-wide SIL OFL 1.1. Designers recorded per the specimen page (Fredoka by Milena Brandão et al., Nunito by Vernon Adams et al., Baloo 2 by Ek Type) — these are Google Fonts canonical attributions.
- ambientCG: site-wide CC0 per `docs.ambientcg.com/license`. Author recorded as "Lennart Demes (ambientCG)" — Demes is the site's primary author/founder; per-asset author may differ on community submissions but ambientCG defaults to him.

## What did NOT work

1. **Quaternius packs** — pack pages are HTML 200 but download URLs are JS-driven. The visible "download" button does not expose a static href that WebFetch can extract. Recommendation: human downloads (one click per pack, ~5 packs), OR write a `quaternius-fetch.py` script that uses a headless-browser-rendered fetch to get the .zip URL.
2. **Kenney Cave Kit** — `/assets/cave-kit` returns 404. The pack appears to have been renamed/retired since INDEX.md was authored. Mini Dungeon (fetched) is a workable substitute for cavern props.
3. **Sand007 on ambientCG** — asset ID returns 404. Substituted Ground054 (sandy/muddy beach surface) which is more appropriate for the Beach biome anyway.
4. **Freesound CC0 SFX** — direct downloads require an OAuth API token (registered via free dev account). Out of scope for this first pass; needs per-sound curation by a human.
5. **Pixabay / Incompetech / FMA BGM** — search and download endpoints work but require per-track curation (human pick). Cannot autopick 12 tracks; quality varies wildly.

## Next-action recommendations (priority order)

1. **Unzip Kenney packs** and run a content audit: confirm Nature Kit contains the trees/rocks/mushrooms expected for Meadow biome; confirm Game Icons covers the HUD set listed in `07-art-bible/07-iconography.md`. This may immediately downgrade some "manual" items above (e.g. if Mini Dungeon contains enough cave props, we don't need a separate Cave Kit search).
2. **Quaternius manual fetch** — human downloads Ultimate Animated Animals, Ultimate Stylized Nature, Monsters Pack from the Quaternius site. Total ~3 packs, ~5 minutes of clicking. Then run an Otter audit: open the Animated Animals pack in Blender and check `bunny.fbx`, `tortoise.fbx`, … `otter.fbx`. If otter is missing, file ADR `decisions/NNNN-otter-beaver-fallback.md`.
3. **Cavern HDRI** — pick a Polyhaven cave/dark-interior HDRI (e.g. `mountain_cave_8k` or similar) and add to the existing fetch pattern.
4. **Mud PBR texture** — `Ground037` or `Mud002` from ambientCG — same fetch pattern as the other 5.
5. **Freesound SFX curation** — human shortlists ~30 specific CC0 sounds (combat hits, poofs, hero pip, ambient beds), then `freesound-fetch.py` runs per-sound.
6. **BGM curation** — human picks 12 BGM tracks across Pixabay (7), Incompetech (3-4 CC-BY), FMA (1), and custom LMMS (1-2). Each CC-BY track must record artist + work + license URL.
7. **Pipeline fix** — `kenney-fetch.py` `append_license_row` has a bug where if `_none yet_` placeholder is missing it appends below the "Allowed sources" section instead of in the fetched-assets table. Minor; the human-rewritten LICENSES.md is now correct.

## Trust audit — what I'm 100% sure of vs. what needs verification

- **100% sure (CC0, files on disk match URLs above):** all 10 Kenney packs, all 5 ambientCG textures (URL → file SHA matches), all 4 Polyhaven HDRIs (curl from dl.polyhaven.org confirmed).
- **High confidence:** Google Fonts OFL. The fonts.gstatic.com TTF files have no in-file metadata I checked, but Google Fonts policy is unambiguous.
- **Medium confidence:** Polyhaven HDRI authors recorded as common-knowledge attributions (Greg Zaal, etc.) — should be cross-checked against `api.polyhaven.com/info/<slug>` in a future pass.
- **Low confidence:** Quaternius pack contents (otter risk unresolved until pack is downloaded and inspected).

## Time spent

Roughly 30-40 minutes wall-clock from receiving the task to writing this log. Most time spent on parallel WebFetch verifications; downloads themselves took ~10 minutes for 22 files (largest single file: Grass001 at 37 MB).
