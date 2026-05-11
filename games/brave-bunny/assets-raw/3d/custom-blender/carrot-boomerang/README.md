# Carrot Boomerang — recipe

> Owner: blender-tech. Cross-refs: `docs/02-gdd/04-weapons.md` (gameplay role), `docs/07-art-bible/08-asset-budget.md` (weapon-prop cap = 200 tris / 64 KB).

## Summary

Vertical-slice weapon #1. A curved (~15 deg) carrot with a small leaf tuft on top. Recolored to brave-bunny's pickup-gold + accent-orange palette so it reads against any biome ground. Built additively over Kenney's veggie pack carrot mesh; falls back to a procedural taper-cone if the Kenney FBX hasn't been staged yet (asset-curator owns the stage).

## Base asset attribution

- **Base mesh:** Kenney Food Kit (CC0). https://www.kenney.nl/assets/food-kit
- **License:** CC0 — output inherits CC0.

## Output specs

| Field | Value |
|---|---|
| File | `carrot-boomerang.glb` |
| Tri count | ≤ 200 (asserted in `build.py`) |
| Materials | 1 (joined mesh) |
| Texture | shares weapons atlas (~64 KB after atlas pack) |
| Animations | none — gameplay-engineer drives spin via Transform |
| Y-up forward | yes (`_glb_export.export_glb` default) |

## How to invoke

```bash
cd games/brave-bunny/assets-raw/3d/custom-blender/carrot-boomerang
blender --background --factory-startup --python build.py -- --output carrot-boomerang.glb
```

Or if a `carrot-boomerang.blend` source exists (post asset-curator stage):

```bash
blender --background carrot-boomerang.blend --python build.py -- --output carrot-boomerang.glb
```

## Hand-off

- `gameplay-engineer` imports the `.glb` to `unity/Assets/Art/Weapons/CarrotBoomerang.prefab`.
- Projectile spin = 720 deg/s yaw (per `weapons.md`); no built-in anim.
- VFX trail authored separately in `05-vfx-style.md` §"weapon trio: carrot".
