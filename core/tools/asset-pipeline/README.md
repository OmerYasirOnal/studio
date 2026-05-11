# Asset pipeline

Scripts here run **as the asset-curator agent's hands**. Every script:

1. Accepts a CLI URL — never hard-coded
2. Validates the host is on a per-script allow-list (see ALLOWED_HOSTS in each file)
3. Downloads the file
4. Appends a row to `games/<active>/assets-raw/LICENSES.md`
5. Refuses to overwrite existing files

## Scripts

| Script | Source | License produced |
|---|---|---|
| `quaternius-fetch.py` | quaternius.com | CC0 1.0 |
| `kenney-fetch.py` | kenney.nl | CC0 1.0 |
| `freesound-fetch.py` | freesound.org | CC0 1.0 |
| `licenses.py` | (validation only) | — |

Adding a new source = a new fetch script in this directory, plus a row in the "Approved sources" section of every game's `assets-raw/LICENSES.md`, plus an entry in `core/docs/asset-policy.md`.

## Validation

Run before every commit that touches `assets-raw/`:

```bash
python core/tools/asset-pipeline/licenses.py --validate
```

Returns 0 if every file is licensed and every license is permissive (CC0, CC-BY, MIT, SIL OFL). Non-zero on any violation.

## Reporting

```bash
python core/tools/asset-pipeline/licenses.py --report
```

Prints a license/source breakdown for the active game.
