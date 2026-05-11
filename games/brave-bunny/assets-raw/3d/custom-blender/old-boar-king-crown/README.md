# Old Boar King Crown — recipe

> Owner: blender-tech. Cross-refs: `docs/02-gdd/05-bosses.md` (Old Boar King), `docs/07-art-bible/08-asset-budget.md` (boss tris cap 8 000; crown is a separate-mesh accessory).

## Summary

A small 5-tooth crown ring with a single front gem. Exported **standalone** — gameplay-engineer parents it to the Quaternius boar boss mesh's `Head` bone via a Unity prefab nested transform. This pattern (additive prop authored separately) avoids touching the Quaternius source FBX and keeps it cleanly recolorable for future boss variants.

## Base asset attribution

- **Base mesh:** None — fully procedural.
- **Boar host:** Quaternius Animated Animals "Boar" (CC0) — host file is in `assets-raw/quaternius/AnimatedAnimals/Boar.fbx`, untouched here.
- **License:** CC0.

## Output specs

| Field | Value |
|---|---|
| File | `old-boar-king-crown.glb` |
| Tri count | ≤ 150 (asserted in `build.py`) |
| Materials | 1 (joined after recolor) |
| Texture | shares boss atlas (~ negligible KB) |
| Animations | none |
| Scale | total height ~0.16 u (Z); Unity prefab scales 1.0 onto a 0.3 u boar head crest |

## How to invoke

```bash
cd games/brave-bunny/assets-raw/3d/custom-blender/old-boar-king-crown
blender --background --factory-startup --python build.py -- --output old-boar-king-crown.glb
```

## Hand-off

- `gameplay-engineer` adds the `.glb` as a child of the boar boss prefab; parents the root to the `Head` bone with offset (0, 0.12, 0.02) and rotation (-10, 0, 0).
- No animation needed — the crown rides the head bone's transform.
