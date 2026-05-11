# Asset policy

> **Rule:** every asset shipped in a Studio-built game must be CC0, CC-BY, MIT, or SIL OFL. No exceptions, no signoff, no "we'll fix it later." Period.

## Why

- **Legal**: per-game license auditing for an indie studio with no lawyer is too risky. CC0 / CC-BY-with-attribution eliminates surprises at submission.
- **Reproducible**: every team member or future you can re-fetch the source. Closed-source asset packs vanish when the maintainer leaves.
- **Framework purity**: the framework guarantees zero paid AI services. Closing the loop on assets means a fresh clone can ship a game with no account creation.

## Allowed sources

### 3D models
- **Quaternius.com** — CC0. Animated animals, characters, environment kits. Primary source for low-poly cartoon games.
- **Kenney.nl** — CC0. Modular kits (nature, dungeon, platformer, city, scifi).
- **Poly.pizza** — CC0 aggregator. Search across many CC0 creators.
- **OpenGameArt.org** — Multiple licenses; use `license:cc0` filter only.
- **Sketchfab** — Set the license filter to **Creative Commons 0**. Other CC variants are NOT allowed without an ADR.

### Textures / PBR
- **ambientCG.com** — CC0 PBR textures.
- **Polyhaven.com** — CC0 textures, HDRI, models.

### HDRI / lighting
- **Polyhaven.com** — CC0.

### Audio (SFX)
- **Freesound.org** — Filter to **Creative Commons 0**. Other CC variants are NOT allowed without an ADR.

### Audio (BGM)
- **Pixabay Music** — Royalty-free, attribution not required.
- **Incompetech (Kevin MacLeod)** — CC-BY 4.0. Attribution is mandatory and recorded in LICENSES.md.
- **Free Music Archive** — Filter to CC0.

### Fonts
- **Google Fonts** — SIL OFL 1.1.

## Forbidden

| Source | Reason |
|---|---|
| Unity Asset Store paid | Per-seat license incompatible with open framework |
| ArtStation | Mixed-license platform, manual auditing burden |
| Gumroad / itch.io paid | Mixed, often EULA-restricted |
| Adobe Stock / Shutterstock | Subscription EULA |
| AI image / audio / 3D generators (Midjourney, Stable Diffusion, Suno, ElevenLabs, Meshy, Hunyuan3D, Tripo, etc.) | License uncertainty + framework's "zero external paid API" rule |

## CI enforcement

```bash
python core/tools/asset-pipeline/licenses.py --validate
```

Runs in `.github/workflows/ci.yml`. Fails the build if:

- A file exists in `assets-raw/` without a row in `LICENSES.md`
- A row references a non-existent file
- Any row has a license outside the allowed set

## Attribution (for CC-BY assets)

CC-BY assets must record:

- Author name (as required by the license — usually the upload page)
- Asset URL
- License URL (e.g., `https://creativecommons.org/licenses/by/4.0/`)
- Game's published credits screen (ui-engineer must include a credits screen for any game using CC-BY assets)

The ui-engineer reads `games/<active>/assets-raw/LICENSES.md`, filters CC-BY rows, and renders them in the credits screen.

## Adding a new approved source

1. Write an ADR proposing the source: `games/<active>/docs/decisions/NNNN-add-source-<name>.md`
2. Justify why existing sources don't cover the need
3. Verify the new source's primary license is CC0 / CC-BY / MIT / SIL OFL
4. Add to the "Allowed sources" section here
5. Add a fetch script under `core/tools/asset-pipeline/<source>-fetch.py`
6. Update every game template's `assets-raw/LICENSES.md` allowed-sources block

## Asset moderation

If you discover an asset in `assets-raw/` whose license is wrong, delete the file, remove the row from LICENSES.md, write an ADR documenting the mistake, and re-source from an approved location.
