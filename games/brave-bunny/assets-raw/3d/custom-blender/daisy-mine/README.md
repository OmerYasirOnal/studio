# Daisy Mine — recipe

> Owner: blender-tech. Cross-refs: `docs/02-gdd/04-weapons.md` (mine weapon role), `docs/07-art-bible/08-asset-budget.md`.

## Summary

Vertical-slice weapon #3 (per the vertical-slice BOM in `08-asset-budget.md`). A low-poly daisy: 5 flat-plane petals radial around a small cylinder center, on a thin stem. The mesh ships with a `Pulse` vertex-group on each petal so gameplay-engineer's "armed" pulse-scale animation can drive radial expansion without bones.

## Base asset attribution

- **Base mesh:** None — fully procedural from `bpy.ops.mesh.primitive_*`.
- **License:** CC0.

## Output specs

| Field | Value |
|---|---|
| File | `daisy-mine.glb` |
| Tri count | ≤ 300 (asserted in `build.py`) |
| Materials | 1 (joined after recolor) |
| Texture | ~32 KB after atlas pack (pickup-class shares material with carrot trail) |
| Animations | none — `Pulse` vertex group drives Unity-side scale |
| Vertex groups | `Pulse` (one per petal) |

> **Budget note:** weapon-prop cap is 200 tris (`08-asset-budget.md`). Daisy mine is permitted up to 300 because the 5-petal radial silhouette is the signature read — documented exception, planned ADR-0007.

## How to invoke

```bash
cd games/brave-bunny/assets-raw/3d/custom-blender/daisy-mine
blender --background --factory-startup --python build.py -- --output daisy-mine.glb
```

## Hand-off

- `gameplay-engineer` drives "armed" pulse via Unity Animator on the `Pulse` vertex group (uniform-scale petals 1.0 -> 1.15 -> 1.0 at 1.5 Hz).
- "Trigger" explosion VFX (per `05-vfx-style.md`) replaces the mesh on detonate; no built-in destruction anim.
