# Tech Spec 00 — Engine and Version

> Owner: tech-architect. Locks the engine + language stack so balance-engineer, gameplay-engineer, systems-engineer, and ui-engineer can stop hedging. Authoritative source: **ADR-0005**. Sister docs: `01-project-layout.md` (folder structure), `05-performance-budget.md` (per-frame budget).

## Decision (one-line)

**Unity 6 LTS + URP + C# 9 + UI Toolkit + IL2CPP (iOS / Android), .NET Standard 2.1 runtime.** See ADR-0005.

## Engine

- **Editor:** Unity 6 LTS — pin **the latest patch release at Phase 5 (Prototype) kickoff**. Once locked, `unity/ProjectSettings/ProjectVersion.txt` becomes the single source of truth and is committed to git. Out-of-band Editor upgrades require an ADR.
- **Render pipeline:** Universal Render Pipeline (URP). Forward+ rendering. Compatible with the toon-ramp shader graph per ADR-0002.
- **License:** Unity Personal (free) covers studio at our pre-launch revenue tier; no paid Unity subscription is in the dependency graph. Revenue gate per Unity terms tracked by build-engineer.

## Language and runtime

- **Language version:** C# 9 (Unity 6 LTS default; `<LangVersion>9.0</LangVersion>` in any generated `csproj` overrides only if Unity bumps to C# 10+ mid-development).
- **API compatibility level:** **.NET Standard 2.1** (set in Player Settings). Chosen for portability + smaller IL2CPP output vs .NET Framework profile.
- **Scripting backend:** **IL2CPP only.** Mono is disabled for iOS (Apple requires AOT) and disabled for Android (smaller binary + better startup latency).
- **Managed stripping level:** **Low** (Phase 5), **Medium** (Phase 6 polish). Aggressive stripping deferred until reflection inventory is known.
- **Garbage collector:** Incremental GC enabled. Per `05-performance-budget.md` we target zero allocations in the hot run loop (pooling per ADR-0005 reference, see `brave-bunny/CLAUDE.md`).

## UI framework

- **UI Toolkit** (formerly UIElements). UXML + USS authoring; runtime panels rendered via UIDocument components. UGUI is **not** used at launch.
- One UXML root per screen lives in `unity/Assets/_Brave/UI/` (see `01-project-layout.md`).
- Rationale: UI Toolkit's retained-mode tree dovetails with the perf budget (one redraw on panel invalidation, not per-frame OnGUI).

## Asset pipeline

- **3D import:** glTF 2.0 (preferred for CC0 sources like Quaternius) via the free `com.unity.cloud.gltfast` package, and FBX via Unity's built-in importer. No paid plugins.
- **Texture import:** PNG source, compressed to **ASTC 4×4** on iOS (universal A14+ baseline) and **ETC2 RGBA** on Android development builds. ASTC 6×6 evaluated post-Phase-5 if iPhone SE 3 supports it (open question flagged in `07-art-bible/08-asset-budget.md`).
- **Audio import:** OGG Vorbis source (per audio-bible). BGM streamed; SFX in-memory pool.

## Performance and tooling

- **Burst Compiler:** Available; used selectively in **spawning** (wave-template driver) and **collision** (spatial-hash broadphase). Not blanket-enabled — see `05-performance-budget.md`.
- **Job System:** Used for the same two subsystems, paired with Burst. Main-thread fallback path retained for Editor debugging.
- **Profiler:** Unity Profiler + Frame Debugger; iOS device profiling via Xcode Instruments. CI captures captured in `tools/ci/`.

## Build targets

- **iOS:** primary platform per `GAME.md`. Bundle id pattern `com.yasironal.brave-bunny`. Minimum iOS version 14 (iPhone SE 3 is iOS 15+; safe).
- **Android:** secondary. Min SDK 26 (Android 8.0).

## Cross-references

- **ADR-0005** — engine choice rationale.
- **ADR-0002** — toon shader source; depends on URP Shader Graph in Unity 6.
- **ADR-0001** — starter-weapon binding; informs `02-data-model.md` SO schema.
- **`brave-bunny/CLAUDE.md`** — perf contract (60 fps iPhone 12, 80 DC, 250k tris, pooling mandatory).
- **`07-art-bible/08-asset-budget.md`** — on-disk and on-screen budgets that bound this engine choice.
