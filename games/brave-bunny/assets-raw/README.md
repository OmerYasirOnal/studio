# Raw assets (download cache)

This directory holds source assets pulled from CC0 sources.
Generated/compressed outputs live in `app/assets/glb/` and
`app/assets/audio/`.

To regenerate compressed assets: `cd games/brave-bunny && node tools/assets/compress.mjs`.

License manifest: `LICENSES.md`.

## Subdirectories

- `quaternius/` — Quaternius CC0 animated characters (.glb). Sources: poly.pizza / quaternius.com / Quaternius itch.io.
- `kenney/` — Kenney CC0 audio + biome props. Sources: kenney.nl.
- `3d/`, `audio/`, `textures/`, `hdri/`, `ui/`, `vfx/`, `fonts/`, `icons/`, `custom/` — legacy layout from the Unity era; preserved for reference and asset reuse.

## Gitignore policy

Per root `.gitignore`, raw `.glb` and `.zip` downloads under `quaternius/` and `kenney/` are NOT tracked. Re-fetch via the download scripts; the compressed outputs in `app/assets/glb/` are the source of truth committed to git.
