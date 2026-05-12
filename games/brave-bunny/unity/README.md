# Brave Bunny — Unity 6 LTS URP Project

Action-roguelite (Survivor.io-like) shipping iOS-first, Android-second.
Bundle id: `com.omeryasir.bravebunny`.

## Minimum Unity version

**Unity 6 LTS — `6000.0.31f1` or newer patch in the 6000.0.x line.**
Pinned in `ProjectSettings/ProjectVersion.txt`. Out-of-band Editor upgrades
require an ADR (see `../docs/06-tech-spec/00-engine-and-version.md`).

## Open in Unity

1. Install Unity 6 LTS via Unity Hub (include iOS Build Support + Android
   Build Support modules).
2. In Unity Hub click **Open** and select `games/brave-bunny/unity/`.
3. First import takes ~5 min while Package Manager resolves dependencies
   from `Packages/manifest.json` and Unity recompiles all asmdefs.
4. If a dialog complains about a missing scripting backend, install IL2CPP
   support for iOS + Android via Unity Hub > Installs > Add Modules.

## Dependencies (Unity Package Manager)

Pinned in `Packages/manifest.json` (see `../docs/06-tech-spec/11-third-party.md`):

- URP, Shader Graph, VFX Graph (rendering)
- Input System, Animation Rigging, TextMeshPro
- Newtonsoft Json (save serialization)
- Burst, Mathematics, Collections (jobified perf path)
- Addressables, Unity IAP, Localization
- glTFast (CC0 mesh import)
- Test Framework (NUnit + PlayMode harness)

**UniTask** ships separately via the OpenUPM scoped registry already
declared in `manifest.json` — add `"com.cysharp.unitask": "2.5.10"` to
`dependencies` when ready to wire it in.

**Google Mobile Ads (AdMob)** is added at first beta build by
build-engineer via Google's UPM registry; intentionally not in the
initial manifest.

## Folder convention

- `Assets/_Brave/Code/` — all C# under `Brave.*` namespaces
- `Assets/_Brave/Data/` — ScriptableObjects + balance assets
- `Assets/_Brave/Art/` — meshes, textures, materials
- `Assets/_Brave/UI/` — UXML, USS, theme assets (UI Toolkit)
- `Assets/_Brave/Scenes/` — Boot, MainMenu, Loadout, Run, Test
- `Assets/_Brave/Shaders/Toon/` — toon ramp shader per ADR-0002

## Assembly definition graph

- `Brave.Gameplay` — no UI deps (combat, movement, spawning, pooling)
- `Brave.Systems` — depends on Gameplay (save, audio, settings, analytics, IAP, ads, GameContext)
- `Brave.UI` — depends on Systems (screens, widgets, UI Toolkit panels)
- `Brave.Boot` — depends on all 3 (app entry point + composition root)
- `Brave.Tests.EditMode` — depends on all 4 + NUnit + UnityEditor.TestRunner
- `Brave.Tests.PlayMode` — depends on all 4 + NUnit (TestAssemblies)

**Hard rule:** `Brave.Gameplay` must NOT reference `Brave.UI`. Enforced at
asmdef level — violations break the build.

## First-run housekeeping

```bash
# Generate ScriptableObject .asset stubs from balance JSON
python ../../../core/tools/balance-tools/make_so_stubs.py brave-bunny
```

The generated `.asset` files are committed; hand-edits to them are blocked
by a header check in the same tool.

## Tech contract

See `../docs/06-tech-spec/` for the full architecture.
**Performance budget:** 60 fps on iPhone 12 with 200 enemies + 50 projectiles
+ 30 VFX puffs. Draw-call cap 80; on-screen tris cap 250 k.

## ADR cross-reference

- ADR-0001 — starter-weapon character binding (informs `CharacterDefinition` SO)
- ADR-0002 — toon shader source (custom Shader Graph in `Assets/_Brave/Shaders/Toon/`)
- ADR-0005 — Unity 6 LTS URP engine choice
- ADR-0008 — save format via Newtonsoft JSON
- ADR-0009 — polymorphic mechanics via `[Brave.Register]` type-name registry
