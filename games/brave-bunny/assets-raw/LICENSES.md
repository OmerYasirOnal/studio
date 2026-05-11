# Asset license ledger — `Brave Bunny`

Every asset under `assets-raw/` must appear in this table. asset-curator maintains it. `licenses.py --validate` runs in CI to enforce.

| File (relative to assets-raw/) | Source | License | URL | Author | Fetched |
|---|---|---|---|---|---|
| _none yet_ | | | | | |

## Planned acquisitions (not yet fetched)

These rows will be populated as assets are downloaded. The current state is "no assets fetched". Use the fetch scripts in `core/tools/asset-pipeline/` to download CC0 packs; each fetch appends a row here automatically.

Per `INDEX.md` in this directory, the planned roster covers:
- 8 characters from Quaternius Animated Animals
- 16 chunks × 5 biomes = 80 environment chunks (Kenney Nature/Platformer/Cave/Mini Dungeon)
- 12 weapons (some from Quaternius/Kenney, some custom Blender)
- 50 SFX (Freesound CC0 + Kenney UI Audio)
- 12 BGM tracks (Pixabay + Incompetech CC-BY)
- 3 fonts (Google Fonts SIL OFL)
- ~52% UI icons from Kenney Game Icons (CC0)

| File | Source | License | URL | Author | Fetched |
|---|---|---|---|---|---|
| _none yet — see INDEX.md for the plan_ | | | | | |

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
