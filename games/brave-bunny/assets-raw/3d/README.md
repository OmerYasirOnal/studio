# `assets-raw/3d/` — 3D source layout

> Owner: asset-curator (CC0 fetches) + blender-tech (custom authoring). This README documents the expected directory layout under `3d/`. Every file landed here must have a corresponding row in `../LICENSES.md`.

## Directory layout

```
3d/
├── characters/                     # Quaternius Animated Animals — recolored per art bible
│   ├── bunny/<files>
│   ├── tortoise/<files>
│   ├── hedgehog/<files>
│   ├── fox/<files>
│   ├── otter/<files>               # may fall back to beaver kitbash — see ADR
│   ├── panda/<files>
│   ├── badger/<files>
│   └── owl/<files>
├── enemies/                        # Quaternius Animated Animals + Monsters Pack recolors
│   ├── hop-slime/<files>
│   ├── bee-buzz/<files>
│   ├── daisy-bite/<files>
│   ├── crab/<files>
│   ├── bat-mini/<files>
│   ├── snow-rat/<files>
│   ├── sleepy-boar/<files>         # also Old Boar King base
│   ├── sleepy-ox/<files>
│   ├── yak/<files>
│   ├── walrus/<files>
│   ├── archer-mole/<files>
│   ├── throw-frog/<files>
│   ├── big-onion/<files>           # elite
│   ├── treant-sprout/<files>       # elite
│   └── ... (per docs/02-gdd/05-enemies.md taxonomy)
├── weapons/                        # Quaternius/Kenney sourced weapon props
│   ├── daisy-mine/<files>
│   ├── acorn-cannon/<files>
│   ├── pebble-sling/<files>
│   ├── beehive/<files>
│   └── ... (per docs/02-gdd/04-weapons.md)
├── environment/                    # Kenney + Quaternius biome chunks
│   ├── meadow/<files>              # grass, fence, well, mushroom, lone tree
│   ├── beach/<files>               # sand, palm, coconut, shells, rocks
│   ├── forest/<files>              # oak, ferns, mushroom-ring props
│   ├── cavern/<files>              # stalactite, gem outcrop, dungeon kit
│   └── snow/<files>                # pine, ice, snow tile, drifts
└── custom-blender/                 # blender-tech outputs — see /core/tools/blender-pipeline/
    └── <entity>/
        ├── <entity>.blend          # source file
        ├── <entity>.glb            # exported Unity-ready
        ├── build.py                # reproducible build script
        └── README.md               # design notes + tri budget
```

## Custom-Blender entities (initial gap list)

Per `docs/07-art-bible/09-source-shortlist.md` §Gap list — these land under `custom-blender/`:

- Weapons: `carrot-boomerang`, `thunder-cloud`, `cob-mortar`, `tumbleweed`, `whirligig`
- Environment: `beach-hut`, `forest-log-bridge`, `cavern-glow-mushroom`, `snow-igloo`
- Enemies: 4 trash puff-blob base meshes, boss kitbash accessories (crown, horns, etc.)

## Rules

- Every glb/fbx/png file under `3d/` requires a row in `../LICENSES.md`. CI enforces via `core/tools/asset-pipeline/licenses.py --validate`.
- Pack-original archives (zip/7z) live under `<source>-raw/` siblings inside each subfolder; only the extracted Unity-ready files (glb/fbx/png/mat) ship into Unity.
- Custom-Blender outputs **must** include a `build.py` so they are reproducible by another agent; the `.blend` is the source-of-truth, the `.glb` is the deliverable.
- Tri-budget per entity follows the performance contract in `games/brave-bunny/CLAUDE.md` (on-screen 250k cap) — see per-entity README inside `custom-blender/` for individual caps.
