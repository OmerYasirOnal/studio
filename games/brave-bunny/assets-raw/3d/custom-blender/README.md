# Custom Blender builds — Brave Bunny

> Owner: blender-tech. Cross-refs: `docs/07-art-bible/03-character-style.md` (recolor pipeline + per-char hex tables), `docs/07-art-bible/04-environment-style.md` (chunk/atlas rules + CC0 source mapping), `docs/07-art-bible/08-asset-budget.md` (per-asset-class tris/texture caps), `core/tools/blender-pipeline/_recolor.py` + `_glb_export.py` (shared utilities).

This folder holds **only** the assets the CC0 packs (Kenney + Quaternius) don't ship in our style. Every other 3D asset is sourced + recolored — see `04-environment-style.md` §"CC0 source mapping" for the gap analysis.

## When to use a custom Blender recipe

Only when **all four** of these are true:

1. Kenney Nature/Platformer/Mini-Dungeon doesn't ship the prop.
2. Quaternius Animated Animals / Ultimate Nature / Cave Kit doesn't ship the prop.
3. The asset is small enough that a procedural / additive recipe is faster than hand-sculpting.
4. The art-director has signed off on the gap (recorded in `04-environment-style.md` "Custom Blender additions" column).

If a CC0 source exists, **recolor it** — don't rebuild from primitives.

## Directory convention

Every custom entity gets its own subdirectory:

```
custom-blender/<entity>/
  <entity>.blend     # source — committed via git-lfs (binary)
  build.py           # deterministic headless build script
  <entity>.glb       # built output — committed for Unity ingestion
  README.md          # recipe summary, attribution, output specs
```

`<entity>.blend` is optional for fully-procedural recipes (the procedural ones use a fresh empty scene). For additive-prop recipes that modify a CC0 base mesh, the `.blend` references the base FBX via relative path (`../../quaternius/...`).

## Headless build command

Standard invocation (works without GUI; suitable for CI):

```bash
blender --background <entity>.blend --python build.py -- --output <entity>.glb
```

For fully-procedural recipes (no source `.blend`):

```bash
blender --background --factory-startup --python build.py -- --output <entity>.glb
```

CI build target (build-engineer): a single `make custom-blender` target that walks every `build.py` under `assets-raw/3d/custom-blender/*/` and re-emits the `.glb` next to it. Deterministic — no random seeds; if a script needs jitter it uses a hashed seed from the entity name.

## Triangle budgets (cross-refs `08-asset-budget.md`)

| Class | Tris cap | Texture KB | Source |
|---|---|---|---|
| Weapon prop | 200 | 64 KB | 08-asset-budget §"Weapon prop" |
| Pickup | 100 | 32 KB | 08-asset-budget §"Pickup" |
| Hero-anchor prop | 1 000 | 256 KB | 08-asset-budget §"Prop (hero/anchor)" |
| Filler prop | 300 | 64 KB | 08-asset-budget §"Prop (filler)" |
| Enemy basic | 600 | 128 KB | 03-character-style §"Triangle + bone budgets" |
| Boss accessory | n/a (counted in boss 8 000 cap) | shared atlas | additive prop only |

Every `build.py` MUST `assert` its output is at-or-under its class cap before calling `export_glb`.

## License inheritance

| Base source license | Recipe output license |
|---|---|
| CC0 (Kenney / Quaternius) | **CC0** — the most permissive — declared in entity `README.md` |
| CC-BY (rare; only with art-director sign-off) | **CC-BY** — attribution string copied into entity `README.md` and `assets-raw/LICENSES.md` |
| Procedural-only (no base mesh) | **CC0** — authored in-repo by blender-tech |

Whenever a recipe touches a base mesh, the entity `README.md` MUST cite the base file path and original attribution. The asset-curator audits this monthly against `assets-raw/LICENSES.md`.

## What's currently here

| Entity | Type | Class | Tris target | Status |
|---|---|---|---|---|
| `carrot-boomerang/` | Additive over Kenney veggie | Weapon prop | ≤ 200 | example recipe |
| `daisy-mine/` | Fully procedural | Weapon prop | ≤ 300 | example recipe |
| `old-boar-king-crown/` | Additive prop (separate mesh) | Boss accessory | ≤ 150 | example recipe |
| `meadow-flower-cluster/` | Fully procedural | Filler prop | ≤ 250 | example recipe |

For the **vertical slice** (Meadow biome only), the planned custom-Blender ship list is the four above. For full launch, add: beach hut, log bridge, glow-mushroom emissive, igloo (per `04-environment-style.md` §"CC0 source mapping").
