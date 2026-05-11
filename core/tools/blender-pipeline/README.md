# Blender pipeline

Shared Python utilities for headless Blender (`bpy`) work.

## Convention

Every per-asset transform script lives at `games/<active>/assets-raw/3d/custom-blender/<entity>/build.py` and imports helpers from this directory.

Run a build:

```bash
blender --background base.blend --python build.py -- --output bunny.glb
```

## Helpers

| File | Purpose |
|---|---|
| `_palette.py` | Color palette helpers — load a palette JSON, swap material colors via principled-BSDF base-color tint |
| `_glb_export.py` | Standardized glTF export options for Unity (Y-up forward, +Z up, no lights) |
| `_atlas.py` | Atlas builder — merge multiple meshes' textures into one for draw-call reduction |
| `_recolor.py` | Apply a palette to all materials; bake to a new texture if needed |

These are stubs ready to be filled by blender-tech as needs arise. Keep them small and composable.
