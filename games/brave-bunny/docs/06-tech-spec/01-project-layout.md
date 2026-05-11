# Tech Spec 01 — Project Layout

> Owner: tech-architect. The on-disk Unity project structure, assembly-definition boundaries, and file-naming conventions for `games/brave-bunny/unity/`. Sister docs: `00-engine-and-version.md` (engine pins), `02-data-model.md` (where SOs live), `03-save-system.md` (`Application.persistentDataPath` slot layout).

## Top-level convention

Everything authored by the team lives under `Assets/_Brave/`. The underscore prefix sorts it to the top of the Project window and makes the framework-vs-game separation visible at a glance. Third-party packages and Unity built-ins live in `Assets/Plugins/` and `Packages/`.

```
unity/
├── Assets/
│   ├── _Brave/                           # ALL game code, data, and content (single root for clarity)
│   │   ├── Code/
│   │   │   ├── Gameplay/                 # gameplay-engineer owns
│   │   │   │   └── Brave.Gameplay.asmdef
│   │   │   ├── Systems/                  # systems-engineer owns
│   │   │   │   └── Brave.Systems.asmdef
│   │   │   ├── UI/                       # ui-engineer owns
│   │   │   │   └── Brave.UI.asmdef
│   │   │   ├── Boot/                     # app entry point + bootstrap composition root
│   │   │   │   └── Brave.Boot.asmdef
│   │   │   └── Tests/                    # qa-engineer; mirrors source layout
│   │   │       ├── EditMode/
│   │   │       │   └── Brave.Tests.EditMode.asmdef
│   │   │       └── PlayMode/
│   │   │           └── Brave.Tests.PlayMode.asmdef
│   │   ├── Data/
│   │   │   ├── Balance/                  # ScriptableObject .asset files (gen'd from data/balance JSONs)
│   │   │   │   ├── Characters/
│   │   │   │   ├── Weapons/
│   │   │   │   ├── Enemies/
│   │   │   │   ├── Bosses/
│   │   │   │   ├── Biomes/
│   │   │   │   ├── Waves/
│   │   │   │   ├── Passives/
│   │   │   │   ├── UpgradeNodes/
│   │   │   │   └── BattlePass/
│   │   │   └── Definitions/              # other SO types (achievements, cosmetics, currency caps)
│   │   ├── Art/
│   │   │   ├── Characters/<character>/   # one folder per character: mesh + materials + animations
│   │   │   ├── Environment/<biome>/      # per-biome environment chunks + atlas
│   │   │   ├── VFX/                      # particle prefabs + textures
│   │   │   └── Icons/                    # UI icon sprites
│   │   ├── Audio/                        # imported OGG assets (BGM streamed, SFX in-memory)
│   │   │   ├── BGM/
│   │   │   └── SFX/
│   │   ├── UI/                           # UXML, USS, theme files (UI Toolkit)
│   │   │   ├── Screens/                  # one UXML per screen
│   │   │   ├── Components/               # reusable controls
│   │   │   └── Themes/                   # USS theme files
│   │   ├── Scenes/                       # exactly 4 scenes (see Scene list below)
│   │   ├── Shaders/
│   │   │   └── Toon/                     # toon ramp shader per ADR-0002
│   │   └── Prefabs/                      # composed prefabs (player, pools, FX rigs)
│   ├── Plugins/                          # third-party (MIT / Apache / CC0); no paid plugins
│   └── Settings/                         # URP global settings, build settings, render assets
├── Packages/                             # Unity Package Manager manifest + lockfile
├── ProjectSettings/                      # committed; ProjectVersion.txt is the engine pin
└── Library/                              # GITIGNORED — Unity's local cache
```

`Temp/`, `Logs/`, `MemoryCaptures/`, `obj/`, and `*.csproj` artifacts are gitignored alongside `Library/`.

## Scene list

Exactly four scenes ship at launch:

| Scene | Purpose | Owner |
|---|---|---|
| `Boot.unity` | Cold-start; loads save, decides next scene (MainMenu or Run resume) | systems-engineer |
| `MainMenu.unity` | Lobby / store / battle-pass / meta-progression hub | ui-engineer |
| `Run.unity` | The 7–10 min gameplay scene; one scene drives all 5 biomes via biome SO | gameplay-engineer |
| `Test.unity` | Sandbox for gameplay-engineer + qa-engineer; never built into shipping IPA | gameplay-engineer + qa-engineer |

Additive loading is **not** used at launch (keeps memory model predictable). Biome content swaps inside `Run.unity` via prefab pools, not by switching scenes.

## Assembly definitions (asmdef boundaries)

Six asmdefs enforce the dependency graph below. Cross-references are one-way to prevent UI code from leaking into gameplay code.

| Assembly | Depends on | Allowed to reference |
|---|---|---|
| `Brave.Gameplay` | (none in this project) | Unity engine, third-party plugins |
| `Brave.Systems` | `Brave.Gameplay` | Unity engine, third-party plugins, Gameplay |
| `Brave.UI` | `Brave.Systems` | Systems (and transitively Gameplay), UI Toolkit |
| `Brave.Boot` | `Brave.Gameplay`, `Brave.Systems`, `Brave.UI` | all of the above |
| `Brave.Tests.EditMode` | all of the above | all of the above + NUnit |
| `Brave.Tests.PlayMode` | all of the above | all of the above + NUnit |

**Hard rule:** `Brave.Gameplay` may not reference `Brave.UI`. Compile-time enforcement via asmdef. Violations break the build, which is intentional.

Why the layering matters:
- Gameplay code (combat, AI, pooling) can be unit-tested with no UI dependency.
- UI can be re-skinned or replaced without touching gameplay logic.
- Build size for headless server-style runs (future: replay/balancing-bot scenarios) is achievable by stripping `Brave.UI` + `Brave.Boot`.

## File naming conventions

- **PascalCase** for all C# types, files, and folders inside `Code/`.
- **One public class per file.** File name == class name (`PlayerController.cs` contains `class PlayerController`).
- **Nested types** stay in the same file as their declaring type; if a nested type grows past ~50 lines, promote it to its own file.
- **Partial classes** allowed only for Editor extensions (`Foo.cs` + `Foo.Editor.cs`).
- **Interfaces** prefixed with `I` (`IDamageable`, `IPoolable`).
- **Abstract base classes** suffixed with `Base` only when ambiguity matters (`SignatureMechanicBase` vs `SignatureMechanic` derived).
- **ScriptableObjects:** SO type names end with `Definition` (`CharacterDefinition`, `WeaponDefinition`). Asset file names mirror the slug from the GDD: `bunny.asset`, `carrot-boomerang.asset`.
- **kebab-case** for asset slugs, JSON files, and UXML/USS files (`hud-joystick.uxml`, `theme-default.uss`).
- **snake_case** in JSON balance files (matches `data/balance/*.json` convention from `brave-bunny/CLAUDE.md`).

## Folder ownership cross-check

Cross-referenced with the file ownership map in repo-root `CLAUDE.md`. The Unity project layout intentionally mirrors that map:

| Folder | Owner |
|---|---|
| `Code/Gameplay/` | gameplay-engineer |
| `Code/Systems/` | systems-engineer |
| `Code/UI/` | ui-engineer |
| `Code/Boot/` | systems-engineer (composition root) |
| `Code/Tests/` | qa-engineer |
| `Data/Balance/` | balance-engineer (asset gen tool below) |
| `Art/` | art-director + asset-curator |
| `Audio/` | art-director (audio sub-role) |
| `UI/` (UXML/USS) | ui-engineer |
| `Shaders/Toon/` | gameplay-engineer (per ADR-0002) |

## Build-time balance → ScriptableObject generation

Balance authors edit JSON in `games/brave-bunny/data/balance/`. A Python tool (`core/tools/balance-tools/make_so_stubs.py`, framework-side) converts JSON → `.asset` files in `Assets/_Brave/Data/Balance/`. The tool runs:

1. Pre-commit hook (warn on drift).
2. CI step before builds (fail on mismatch).
3. Manually via `python core/tools/balance-tools/make_so_stubs.py brave-bunny` when a designer edits JSON.

The generated `.asset` files **are committed** to git (otherwise the Unity project does not open cleanly on a fresh clone). Hand-edits to generated assets are blocked by a `// generated, do not edit` header check in the same tool.

## Cross-references

- ADR-0005 — engine choice.
- `02-data-model.md` — what lives in `Data/Balance/` and `Data/Definitions/`.
- `03-save-system.md` — paths under `Application.persistentDataPath`.
- `core/docs/asset-policy.md` — CC0 / MIT only for everything under `Plugins/` and `Art/`.
- Repo-root `CLAUDE.md` — folder ownership map.
