# Meadow Flower Cluster — recipe

> Owner: blender-tech. Cross-refs: `docs/07-art-bible/04-environment-style.md` (Meadow filler props), `docs/07-art-bible/08-asset-budget.md` (filler-prop cap = 300 tris / 64 KB).

## Summary

A filler-prop cluster of 5 stylized flowers (yellow + pink variants, ~50/50 split) within a 1x1 u footprint, joined into a single mesh. Level-designer drops 2–4 instances per Meadow chunk to give vegetation read at low cost — the URP GPU instancer batches across chunks (one DC per `04-environment-style.md` §"Vegetation strategy").

Layout is deterministic: a SHA-1 hash of the entity name seeds the RNG, so every CI run produces identical jitter.

## Base asset attribution

- **Base mesh:** None — fully procedural.
- **License:** CC0.

## Output specs

| Field | Value |
|---|---|
| File | `meadow-flower-cluster.glb` |
| Tri count | ≤ 250 (asserted in `build.py`) |
| Materials | 1 (joined after recolor) |
| Texture | shares Meadow atlas (~ negligible KB; all colors are atlas swatches) |
| Animations | none |
| Footprint | 1x1 u |

## How to invoke

```bash
cd games/brave-bunny/assets-raw/3d/custom-blender/meadow-flower-cluster
blender --background --factory-startup --python build.py -- --output meadow-flower-cluster.glb
```

## Hand-off

- `level-designer` places 2–4 instances per Meadow chunk via the Unity prefab pipeline.
- Wind sway is handled by the chunk-wide vegetation vertex shader, not by this mesh (no built-in anim).
- For Beach / Forest / etc., author a sibling recipe (`beach-shell-cluster/`, etc.) rather than recoloring this one — palette stays per-biome.
