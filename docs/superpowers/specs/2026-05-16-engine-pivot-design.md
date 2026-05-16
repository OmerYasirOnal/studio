# Engine pivot — Unity → Three.js + R3F + Capacitor (iOS)

> **Status:** Approved (high-level) — awaits spec review and implementation plan.
> **Date:** 2026-05-16
> **Author:** Claude (with Ömer Yasir Önal)
> **Game:** brave-bunny (Survivor.io-style action-roguelite, iOS first)
> **Supersedes:** `games/brave-bunny/docs/06-tech-spec/00-engine-and-version.md` (will be rewritten by tech-architect during implementation)

---

## 1. Context — why pivot

Brave Bunny is currently on Unity 6 LTS + URP, with 5 TestFlight builds shipped (latest: `85705c5`, Build #5 0.1.0(202605161658)). The Unity build pipeline works end-to-end — fastlane + match certs + `com.omeryasir.bravebunny` provisioning are stable.

The **problem is the development pipeline**, not the publishing pipeline. Specifically:

- Unity scenes (`.unity`), prefabs (`.prefab`), ScriptableObjects (`.asset`), and import settings are GUI-authored YAML files with stable GUIDs and binary-shaped sub-resource refs. An AI coding agent can edit them but the failure modes are silent: a missing component reference produces a null at runtime, not a compile error.
- MCP-Unity (Unity Editor MCP server) exists, but the round-trip is brittle: domain reloads, compile-error guards, and the Editor's "must-recompile-first" gating means many actions silently no-op or stall.
- Wave 13 surfaced the cost concretely: the [Bootstrap] GameObject's script reference de-wired multiple times across builds; ADR-0020 was committed with only `.meta` files (the `.cs` sources were missing); editor compile errors in `Assets/Editor/SceneSetup.cs` blocked all other domain reloads.
- The team's working style is autonomous multi-agent parallel execution. Unity's editor-centricity creates a permanent serialization point: every change must round-trip through one Editor process. Parallel agents can't author Unity scenes simultaneously.

The result is high token cost per shipped feature. We've spent significant context on Unity-Editor-state debugging that doesn't contribute to the game.

**Pivot thesis:** A pure-code, web-tech 3D stack lets a single AI agent (or a swarm) author the entire game — scenes, entities, UI, build config — as plain TypeScript/JSON/Markdown files. Verification happens via Vite's HMR + Playwright in seconds. iOS shipping reuses the existing fastlane pipeline via Capacitor's `npx cap copy ios`.

The non-pivot risk: continuing to burn tokens on Unity Editor state recovery instead of making the game.

---

## 2. Goals & non-goals

### Goals

1. **100% code-authored game**: every scene, entity, weapon, wave, UI screen, and build config is a plain text file (`.ts`, `.tsx`, `.json`, `.md`) editable by an AI agent without GUI intervention.
2. **Same iOS app identity**: ships to the existing `com.omeryasir.bravebunny` App Store Connect record; users see "Build #6" continuing from Build #5.
3. **Same perf contract**: 60 fps on iPhone 12 with 200 active enemies + 50 projectiles + 30 VFX puffs. Draw-call cap: 80. Tris cap on-screen: 250k. (Inherited from `games/brave-bunny/CLAUDE.md`.)
4. **Same GDD**: all `games/brave-bunny/docs/` content (vision, GDD, art bible, balance, level design, monetization) remains the source of truth. No re-design.
5. **Same CC0 asset policy**: only Quaternius / Kenney / OFL / MIT / CC-BY sources. Zero paid third-party APIs (`core/docs/asset-policy.md` rules apply).
6. **Existing fastlane preserved**: `games/brave-bunny/tools/ci/fastlane/` and the `com.omeryasir.bravebunny` match certs continue to drive TestFlight uploads.

### Non-goals

1. **No re-design**: GDD, art bible, balance JSON, and waves data are not re-litigated by this spec.
2. **No Android in scope** for this milestone: iOS-first per existing roadmap. Web-tech base means Android is a Capacitor target swap later, not a rewrite.
3. **No native Swift code** beyond what Capacitor generates and what the existing fastlane signs. We don't add custom UIViewControllers.
4. **No paid 3rd-party services**: no Replicate/ElevenLabs/Meshy/etc. AdMob + IAP (Capacitor plugins, free SDKs) are the only commercial integrations and are post-vertical-slice.
5. **No premature optimization**: VAT bake pipeline is for the 15 enemy archetypes + 1 boss only. Heroes use stock R3F `SkinnedMesh`.

---

## 3. Architecture overview

```
┌──────────────────────────────────────────────────────────────┐
│                       iOS App (WKWebView)                    │
│  ┌────────────────────────────────────────────────────────┐  │
│  │         Vite-built bundle (loaded from disk)           │  │
│  │  ┌──────────────────────────────────────────────────┐  │  │
│  │  │  React (UI: HUD, menus, draft) ── zustand store  │  │  │
│  │  │           ↕ (events / state read)                │  │  │
│  │  │  R3F <Canvas> ── miniplex world ── systems       │  │  │
│  │  │     │                                             │  │  │
│  │  │     ├── render system (R3F components)            │  │  │
│  │  │     ├── movement system (rapier or AABB-manual)   │  │  │
│  │  │     ├── combat / projectile system                │  │  │
│  │  │     ├── spawn system (reads waves.json)           │  │  │
│  │  │     ├── pickup / xp system                        │  │  │
│  │  │     └── audio system (Web Audio API)              │  │  │
│  │  └──────────────────────────────────────────────────┘  │  │
│  │  Assets: heroes.glb, enemies-vat/*.png, palettes/*.png │  │
│  └────────────────────────────────────────────────────────┘  │
│  Capacitor bridge: Storage, IAP, AdMob (post-MVP)            │
└──────────────────────────────────────────────────────────────┘
                            ↑
                            │ npx cap copy ios
                            │
┌──────────────────────────────────────────────────────────────┐
│            Dev / build pipeline (host: macOS)                 │
│  vite dev ──→ http://localhost:5173 (Playwright-testable)     │
│  vite build ──→ app/dist/                                     │
│  npx cap sync ios ──→ ios/App/App/public/                     │
│  fastlane beta ──→ TestFlight (existing match certs)          │
└──────────────────────────────────────────────────────────────┘
```

### Runtime stack (version-pinned at implementation time)

| Layer | Library | Why this one |
|---|---|---|
| Build / dev server | **Vite 6+** | Native HMR for R3F, plain TS, no bundler config |
| UI framework | **React 19** | Required by R3F; used only for HUD/menus (NOT for in-game render) |
| 3D render | **three.js (r170+) + @react-three/fiber 9** | JSX scenes = git-diffable, agent-authorable; no editor file format |
| R3F helpers | **@react-three/drei** | Camera helpers, asset loaders, performance helpers |
| Physics | **@react-three/rapier** | AABB collision for swarm — only on hero ↔ enemy ↔ projectile, NOT all-pairs |
| ECS | **miniplex** | R3F-native, hooks-friendly; pairs with object pools |
| UI state | **zustand** | Outside R3F render loop; no React re-render thrashing |
| Audio | **Web Audio API (raw)** | WKWebView audio latency demands pre-decoded buffers; `<audio>` element is too slow |
| Native shell | **@capacitor/core + @capacitor/ios 7** | iOS WKWebView wrap; preserves fastlane pipeline |
| Animation pipeline | **Custom VAT shader + Blender VAT addon** | For instanced enemy swarms — see §5 |
| Asset pipeline | **@gltf-transform/cli + @gltf-transform/core** | Node CLI; recolor + palette + meshopt + prune |

### Critical perf decisions

| Decision | Choice | Reason |
|---|---|---|
| Enemy animation | **VAT** baked from Blender, rendered via `InstancedMesh` + custom vertex shader | Stock `SkinnedMesh` × 200 = ~200 draw calls, fails iPhone 12 budget |
| Hero animation | Stock `useGLTF` + `SkinnedMesh` + `AnimationMixer` | Only 1 hero on screen; full fidelity fine |
| Hero variants (8 species) | `gltf-transform` palette texture swap | 8 PNG palettes < 16 KB total; one shared base mesh |
| Boss animation | Stock `SkinnedMesh` | 1 on screen; same as hero |
| Projectile render | `InstancedMesh` (cube or quad) + per-instance transform | Flat-shaded, no skin; up to 500 cheaply |
| VFX | GPU particles via custom shader on `InstancedMesh` | No skinning, no physics |
| Collision | Manual AABB grid (NOT rapier solver) | Survivor.io scale demands O(n) broadphase; rapier only for hero ↔ wall edge cases |
| Audio | Web Audio + pre-decoded `AudioBuffer` pool | WKWebView `<audio>` latency too high; pool reuse for SFX spam |
| Device pixel ratio | Capped at **2** | iPhone 12 native 3x at full res destroys fill rate |
| WebGPU vs WebGL2 | **WebGL2 (default)**; opt-in WebGPU detection later | WebGPU on iOS 26 Safari is stable but R3F WebGPU support is partial in 2026 |

---

## 4. Folder layout

```
studio/
├── core/                         # framework (untouched by this pivot)
├── games/
│   └── brave-bunny/
│       ├── CLAUDE.md             # updated: engine block rewritten
│       ├── docs/                 # UNCHANGED (vision, GDD, art bible, balance, ...)
│       │   └── 06-tech-spec/     # 00-engine-and-version.md rewritten by tech-architect
│       ├── app/                  # NEW — Vite + R3F + TS project root
│       │   ├── package.json
│       │   ├── vite.config.ts
│       │   ├── tsconfig.json
│       │   ├── capacitor.config.ts
│       │   ├── public/
│       │   ├── src/
│       │   │   ├── main.tsx              # React entry
│       │   │   ├── App.tsx               # Routes: Boot → Lobby → Run → End
│       │   │   ├── ecs/
│       │   │   │   ├── world.ts          # miniplex world singleton
│       │   │   │   ├── components.ts     # entity component types
│       │   │   │   └── queries.ts        # named queries
│       │   │   ├── systems/
│       │   │   │   ├── movement.ts
│       │   │   │   ├── combat.ts
│       │   │   │   ├── spawn.ts          # consumes waves.json (existing data)
│       │   │   │   ├── pickup.ts
│       │   │   │   ├── draft.ts          # level-up upgrade draft
│       │   │   │   ├── lifecycle.ts      # death / damage / hit-flash
│       │   │   │   └── audio.ts
│       │   │   ├── render/
│       │   │   │   ├── Game.tsx          # R3F <Canvas> + scene root
│       │   │   │   ├── Hero.tsx          # SkinnedMesh + palette swap
│       │   │   │   ├── EnemySwarm.tsx    # InstancedMesh + VAT shader
│       │   │   │   ├── ProjectileSwarm.tsx
│       │   │   │   ├── VFXSwarm.tsx
│       │   │   │   ├── Biome.tsx         # ground, props (Kenney)
│       │   │   │   └── shaders/
│       │   │   │       ├── vat.vert.glsl
│       │   │   │       └── vat.frag.glsl
│       │   │   ├── ui/                   # React HTML overlay (HUD, modals)
│       │   │   │   ├── HUD.tsx
│       │   │   │   ├── DraftModal.tsx
│       │   │   │   ├── EndRunSummary.tsx
│       │   │   │   ├── Lobby.tsx
│       │   │   │   └── styles.css
│       │   │   ├── state/                # zustand stores
│       │   │   │   ├── runStore.ts
│       │   │   │   ├── metaStore.ts      # saves, gold, unlocks
│       │   │   │   └── settingsStore.ts
│       │   │   ├── data/                 # JSON re-imported from docs/10-balance
│       │   │   │   ├── weapons.json
│       │   │   │   ├── enemies.json
│       │   │   │   ├── waves.json
│       │   │   │   └── characters.json
│       │   │   ├── audio/
│       │   │   │   ├── AudioBus.ts
│       │   │   │   └── pool.ts
│       │   │   └── platform/             # Capacitor-specific glue
│       │   │       ├── storage.ts        # Preferences plugin wrapper
│       │   │       └── safearea.ts
│       │   ├── assets/                   # source assets (git-tracked, LFS optional)
│       │   │   ├── glb/                  # heroes.glb, boss.glb, props.glb
│       │   │   ├── vat/                  # baked per-enemy: <archetype>.{png,json}
│       │   │   ├── palettes/             # 8 hero recolor PNGs
│       │   │   └── audio/                # OGG/MP3 (CC0 / OFL only)
│       │   └── dist/                     # vite build output (gitignored)
│       ├── ios/                          # NEW — Capacitor-generated Xcode project
│       │   ├── App/
│       │   │   ├── App.xcodeproj/
│       │   │   ├── App/                  # Info.plist, AppDelegate.swift (untouched)
│       │   │   └── public/               # Vite dist copied here by `cap sync`
│       │   └── Podfile
│       ├── tools/
│       │   ├── ci/                       # existing fastlane (UNCHANGED)
│       │   └── assets/                   # NEW — asset pipeline scripts
│       │       ├── compress.mjs          # gltf-transform: meshopt + palette + prune
│       │       ├── recolor.mjs           # batch hero palette generator
│       │       ├── bake-vat.py           # Blender headless (-b -P) VAT bake
│       │       ├── balance-sync.mjs      # copies docs/10-balance/*.json → app/src/data/
│       │       └── verify.mjs            # asset budget guard (tris, file size)
│       └── unity/                        # DELETED — see §10 migration
└── ...
```

---

## 5. Asset pipeline

### 5.1 Source assets

- **Heroes (8)**: Quaternius Universal Animation Library 2 humanoid rig (shared skeleton) + 8 hero meshes pinned to that rig. URL: https://quaternius.com/packs/universalanimationlibrary2.html
- **Enemies (15 archetypes + 1 boss)**: Quaternius Ultimate Animated Animal Pack (per-species rigs are fine; VAT bakes them flat). URL: https://quaternius.com/packs/ultimateanimatedanimals.html
- **Environment**: Kenney Nature Kit. URL: https://kenney.nl/assets/nature-kit
- **Audio**: CC0 / OFL only — Freesound CC0 filter + Kenney audio packs

### 5.2 Hero pipeline

```
quaternius_ual2.glb
    │
    ├── tools/assets/compress.mjs (gltf-transform meshopt + prune)
    │      ↓
    │   app/assets/glb/heroes.glb (~< 2 MB)
    │
    └── tools/assets/recolor.mjs (palette texture generator from balance/characters.json)
           ↓
       app/assets/palettes/{bunny,fox,bear,...}.png (8 × ~2 KB)
```

Runtime: load `heroes.glb` once via `useGLTF`. Per-hero variant = `SkeletonUtils.clone()` + `material.map = palette<HeroId>.png`.

### 5.3 Enemy + boss pipeline

```
quaternius_animal_<species>.glb
    │
    ├── tools/assets/compress.mjs
    │      ↓
    │   intermediate.glb
    │
    └── tools/assets/bake-vat.py (Blender headless, runs in CI)
           ↓
       app/assets/vat/<archetype>.png   (position texture)
       app/assets/vat/<archetype>.json  (metadata: bounds, anim ranges, frame count)
```

Blender VAT addon: https://extensions.blender.org/add-ons/vat/  
Invocation: `blender -b -P tools/assets/bake-vat.py -- --input <species>.glb --output app/assets/vat/<archetype>`

Runtime: `EnemySwarm.tsx` creates one `InstancedMesh` per archetype, custom shader samples the VAT texture per-frame per-instance. Total cost: ~16 draw calls for full enemy fleet.

### 5.4 Balance data sync

Balance JSON authored in `games/brave-bunny/docs/10-balance/` (existing). A `tools/assets/balance-sync.mjs` script copies the JSON files into `app/src/data/` at build time and runs schema validation. **Single source of truth stays in docs.**

---

## 6. Runtime systems

### 6.1 Game loop

R3F's `useFrame(delta)` drives every system in order each frame. Frame budget at 60fps = 16.6ms; per-system budget tracked via `console.time` in dev.

Per-frame order (must be stable to avoid one-frame visual lag):
1. **Input** — read touch/keyboard, update virtual joystick state
2. **Movement** — apply velocity × delta, clamp to map bounds
3. **Spawning** — read `waves.json`, instantiate due enemies
4. **AI** — enemy `seek(hero)` (simple steering, no path-finding)
5. **Combat** — weapon tick → spawn projectiles
6. **Collision** — AABB grid broadphase, then pairwise overlap
7. **Pickup** — magnet radius check, increment XP/gold
8. **Lifecycle** — apply damage, trigger death anims, banking
9. **Render** — R3F reconciles; instanced buffers updated in-place
10. **Audio** — flush queued SFX (deduplicated within frame)

### 6.2 Object pooling

Every spawnable type has a pre-allocated pool (size from `tools/assets/verify.mjs` budgets). Pools are **arrays of plain objects with `active: boolean`** — no class allocation in hot path.

| Pool | Size | Rationale |
|---|---|---|
| Enemies | 250 | 200 active + 50 spawning headroom |
| Projectiles | 500 | 50 active per perf contract × 10x burst headroom |
| Pickups | 100 | Death-burst worst case |
| VFX puffs | 200 | 30 active × headroom + boss-burst |

### 6.3 Save / persistence

Capacitor `@capacitor/preferences` plugin (key-value on iOS UserDefaults). JSON-serialized save schema in `app/src/state/metaStore.ts`. Cloud save deferred (post-MVP).

### 6.4 UI architecture

**Two separate render trees:**
- 3D world: R3F `<Canvas>` — fullscreen, in-game only
- HTML overlay: React DOM — HUD, modals, menus

This separation matters because:
- HTML text is 10x cheaper than 3D text on iOS
- HUD updates don't trigger 3D scene re-render
- Native CSS animations for menu transitions

UI components consume `zustand` stores (not React Context — Context re-renders the tree).

---

## 7. iOS build pipeline

### 7.1 Local dev → device

```bash
# dev: iterate in browser, Playwright-verifiable
cd games/brave-bunny/app
npm run dev                # vite dev server, HMR

# device test:
npm run build              # vite production build → app/dist/
npx cap sync ios           # copy dist → ios/App/App/public/
open ios/App/App.xcworkspace  # Xcode → run on device or simulator
```

### 7.2 TestFlight ship (re-uses existing fastlane)

```bash
cd games/brave-bunny/app
npm run build && npx cap sync ios
cd ../tools/ci
fastlane beta              # SAME lane as Unity — just points at ios/App now
```

The fastlane `beta` lane already:
- Uses `match` for cert/profile fetch (read-only)
- Increments build number
- Runs `gym` against `App.xcworkspace`
- Uploads to App Store Connect via API key

What changes: `gym` builds the Capacitor-generated `ios/App/App.xcworkspace` instead of the Unity-generated `unity-ios/Unity-iPhone.xcworkspace`. Lane file edits are mechanical (paths only).

Bundle ID stays `com.omeryasir.bravebunny`. Version stays `0.1.0`. Build number continues from #6.

### 7.3 Capacitor config

```ts
// app/capacitor.config.ts
import type { CapacitorConfig } from '@capacitor/cli';
const config: CapacitorConfig = {
  appId: 'com.omeryasir.bravebunny',
  appName: 'Brave Bunny',
  webDir: 'dist',
  ios: {
    contentInset: 'never',          // fullscreen behind notch
    scrollEnabled: false,           // game canvas, no scroll
    backgroundColor: '#1a0d2e',     // matches loading screen
  },
};
export default config;
```

---

## 8. Performance contract (carried over)

Inherited from `games/brave-bunny/CLAUDE.md`:
- 60 fps on iPhone 12 with 200 active enemies + 50 projectiles + 30 VFX puffs
- Draw-call cap: **80** (revised target for WebGL2; reassess if WebGPU enabled)
- Tris on-screen cap: **250k**

New budgets specific to the web stack:
- JS bundle (`app/dist/assets/`): **≤ 2 MB gzipped**
- GLB assets total: **≤ 8 MB** uncompressed (with meshopt, ~2.5 MB on disk)
- VAT textures total: **≤ 4 MB** (16 archetypes × ~256 KB)
- First contentful paint inside WKWebView: **≤ 2.0 s** on iPhone 12

**Stress test gate at W14:** A 200-enemy benchmark scene runs on a physical iPhone 12 connected via Safari Web Inspector. If sustained fps < 55, escalate to fallback (§9).

---

## 9. Risks & fallbacks

| Risk | Likelihood | Mitigation / fallback |
|---|---|---|
| VAT shader doesn't hit 60fps with 200 enemies | Medium | Fallback to `agargaro/instanced-mesh` GPU skinning (no Blender step, drop-in lib) |
| Even instanced-mesh GPU skinning fails | Low | Fallback to hybrid billboard sprites (8-direction pre-rendered, Diablo-style); art bible "puff-blob silhouette" supports this |
| WKWebView audio latency unusable | Low | Web Audio with pre-decoded `AudioBuffer` pool — already in design |
| Capacitor 7 has a regression on iOS 18+ | Low | Cap to last-stable; alternative is `@ionic/portals` or custom WKWebView shim |
| WebGPU on iOS Safari 26 has a bug we hit | Low | Default to WebGL2; WebGPU is opt-in flag |
| Quaternius UAL2 + Animated Animals don't share humanoid rig | Medium | Use UAL2 ONLY for humanoid-ish heroes; animal-shaped heroes use per-species rig + per-hero anims (more work, fewer shared anims) |
| 8 hero recolor via palette texture looks flat | Low | Move to per-material clone + tinted PBR (slightly more memory, same visual budget) |

---

## 10. Migration plan

### 10.1 What gets deleted

- `games/brave-bunny/unity/` — entire Unity project tree (per user decision 2026-05-16)
- Any Unity-specific entries in `.gitignore`, `core/` scripts that reference `unity/`

Git history preserves it; we can `git show <commit>:games/brave-bunny/unity/...` if ever needed.

### 10.2 What gets preserved (verbatim)

- `games/brave-bunny/docs/` — entire docs tree (vision, GDD, art bible, balance, level design, etc.)
- `games/brave-bunny/tools/ci/` — fastlane, match, AppStore mobileprovision
- `games/brave-bunny/CLAUDE.md` — updated engine block only; rules carry over
- All ADRs in `games/brave-bunny/docs/decisions/`

### 10.3 What gets rewritten

- `games/brave-bunny/docs/06-tech-spec/00-engine-and-version.md` → references Three.js + R3F + Capacitor instead of Unity + URP
- `games/brave-bunny/docs/06-tech-spec/01-project-layout.md` → new folder map (§4)
- `games/brave-bunny/docs/06-tech-spec/06-rendering.md` → R3F + VAT shader instead of URP toon shader
- `games/brave-bunny/docs/06-tech-spec/10-build-and-ci.md` → Capacitor + fastlane steps
- Existing ADRs that explicitly cite Unity APIs (ADR-0005 pooling, ADR-0009 input, ADR-0020 weapons) → re-implementation ADRs added; original ADRs marked **SUPERSEDED** with link.

### 10.4 What gets newly authored

- `games/brave-bunny/app/` — the Vite + R3F TS project (§4)
- `games/brave-bunny/tools/assets/` — asset pipeline scripts (§5)
- New ADRs:
  - ADR-0030 — Engine pivot from Unity to Three.js + R3F + Capacitor (this spec → ADR)
  - ADR-0031 — VAT pipeline for enemy swarms
  - ADR-0032 — miniplex ECS + pooling pattern
  - ADR-0033 — Capacitor build integration with existing fastlane

### 10.5 Sequencing (high level — full plan via `writing-plans`)

1. **Sprint A (W14)**: Bootstrap `app/` with empty R3F scene, get it running in browser + on physical iPhone via Capacitor; verify fastlane still produces signed `.ipa`.
2. **Sprint B (W14-15)**: Asset pipeline — gltf-transform + Blender VAT bake; load Bunny + Wolf + one biome.
3. **Sprint C (W15-16)**: Port one weapon (Carrot Spear) end-to-end: input → movement → spawn → projectile → enemy hit → death → XP gem → level-up draft.
4. **Sprint D (W16)**: 200-enemy stress test on iPhone 12; gate decision (continue VAT vs fallback).
5. **Sprint E (W17)**: Remaining vertical-slice content (2 weapons, full Carrot Fields biome, end-run boss, meta loop).
6. **Sprint F (W18)**: TestFlight Build #6, soft-launch readiness review.

---

## 11. Verification strategy (how Claude verifies own work)

This is what made Unity painful — verifying that a scene actually wired up. The new stack verifies via:

| Layer | Verification | Tool |
|---|---|---|
| TypeScript correctness | `tsc --noEmit` | npm script `typecheck` |
| Unit tests (ECS, math, pools) | Vitest | npm script `test` |
| Asset budget | `tools/assets/verify.mjs` (tris, file size, palette count) | npm script `verify` |
| Visual smoke test | Playwright loads `http://localhost:5173`, takes screenshot, asserts hero/enemy visible | npm script `e2e` |
| Runtime perf | Headless Chromium devtools `Performance.metrics()`, fps counter | npm script `bench` |
| iOS device perf | Safari Web Inspector → Timelines (manual; user-initiated) | n/a |
| iOS build green | `npx cap sync ios && xcodebuild -workspace ... -scheme App -destination 'generic/platform=iOS' build` | npm script `build:ios` |
| TestFlight upload | `fastlane beta` exit code 0 | existing |

Crucially: **every layer above is a CLI command Claude can run and read the output of**. No GUI required.

---

## 12. Open questions (decided in implementation plan, not here)

- Exact version pins for `three`, `@react-three/fiber`, `@react-three/rapier`, `miniplex` — picked at sprint A bootstrap, recorded in `package.json` and ADR-0030
- Whether to use a monorepo (npm workspaces / pnpm) inside `games/brave-bunny/app/` — likely no for MVP; revisit if `core/` grows JS tools
- Cloud save backend — deferred post-MVP; skeleton interface only
- Game-Center / Apple ID auth — deferred post-MVP

---

## 13. References

- Three.js + R3F vs alternatives (research summary 2026-05-16): see `docs/superpowers/specs/2026-05-16-engine-pivot-design.md` (this doc); raw agent transcripts stored in tasks dir
- React Three Fiber: https://r3f.docs.pmnd.rs/
- @react-three/drei: https://github.com/pmndrs/drei
- @react-three/rapier: https://github.com/pmndrs/react-three-rapier
- miniplex (ECS): https://github.com/hmans/miniplex
- Capacitor iOS docs: https://capacitorjs.com/docs/ios
- Capacitor Games guide: https://capacitorjs.com/docs/guides/games
- glTF-Transform: https://gltf-transform.dev/
- Blender VAT addon: https://extensions.blender.org/add-ons/vat/
- agargaro/instanced-mesh (fallback): https://github.com/agargaro/instanced-mesh
- R3F VAT example: https://github.com/mikelyndon/r3f-webgl-vertex-animation-textures
- Quaternius UAL2: https://quaternius.com/packs/universalanimationlibrary2.html
- Quaternius Ultimate Animated Animals: https://quaternius.com/packs/ultimateanimatedanimals.html
- WebKit WebGPU news (Safari 26): https://webkit.org/blog/16993/news-from-wwdc25-web-technology-coming-this-fall-in-safari-26-beta/
- WebKit InstancedMesh perf bug #218949: https://bugs.webkit.org/show_bug.cgi?id=218949

---

## 14. Agent definitions — what changes

Sixteen agent definitions live in `core/.claude/agents/`. The pivot rewrites the **engine assumption block** in each, but the role, ownership map, and process discipline survive intact. Below: per-agent what changes / what stays.

### Engine-dependent (must rewrite engine block)

| Agent | What stays | What changes |
|---|---|---|
| **tech-architect** | ADR discipline, system-of-record role | Authors Three.js + R3F + Capacitor spec; rewrites `docs/06-tech-spec/00,01,06,10`; new ADRs 0030-0033 |
| **gameplay-engineer** | Owns `Scripts/Gameplay/` analog | New path: `app/src/systems/` + `app/src/render/`. Writes TS with R3F + miniplex, not C# with Unity MonoBehaviour |
| **systems-engineer** | Owns infrastructure code | New path: `app/src/ecs/`, `app/src/state/`, `app/src/platform/`. Pool, save, audio system in TS |
| **ui-engineer** | Owns `Scripts/UI/` analog | New path: `app/src/ui/`. React + zustand + CSS instead of UI Toolkit + USS |
| **build-engineer** | Owns `tools/ci/` (UNCHANGED contents) | Adapts fastlane to point at Capacitor's Xcode workspace; new npm/Vite/Capacitor scripts; updates CI workflows |
| **qa-engineer** | Owns `Tests/` analog | New path: `app/src/**/*.test.ts` (Vitest) + `app/e2e/` (Playwright). Test pyramid stays |
| **asset-curator** | CC0 sourcing, license tracking | Outputs to `app/assets/glb/`, runs gltf-transform compress; new responsibility for VAT bake recipes |
| **blender-tech** | Headless Blender pipelines | New artifact: VAT textures (not Unity FBX import). Bake script lives in `tools/assets/bake-vat.py` |

### Engine-independent (no change)

| Agent | Why unchanged |
|---|---|
| **game-designer** | Outputs to `docs/02-gdd/`. Markdown is engine-agnostic. |
| **narrative-designer** | Outputs to `docs/02-gdd/narrative/`. Markdown. |
| **ux-designer** | Outputs to `docs/03-05`. Wireframes (Mermaid) are engine-agnostic. |
| **level-designer** | Owns `docs/09-level-design/` + `waves.json`. JSON consumed by either engine. |
| **balance-engineer** | Owns `docs/10-balance/` + `data/balance/*.json`. JSON consumed by either engine. |
| **art-director** | Owns `docs/07-art-bible/`. Visual spec engine-agnostic; ramp-shader recipe gets a Three.js section but stays the same intent. |
| **researcher** | Outputs to `docs/01-research/`. |
| **orchestrator** | Process file; updates the engine name only. |

### New agent definition

| Agent | Role |
|---|---|
| **web-engineer** (NEW — _optional addition_) | If we want a dedicated owner for Capacitor / Vite / Web Audio / WKWebView quirks. Otherwise systems-engineer absorbs this. Decision deferred to implementation plan; lean toward "no new agent, expand systems-engineer." |

Each engine-dependent agent gets a `## Engine context` block updated to:
- Stack: Three.js r170+ / @react-three/fiber 9 / Capacitor 7
- Build dir: `games/<active>/app/`
- iOS shell: `games/<active>/ios/` (Capacitor-generated, signed via existing fastlane)
- File ownership map (root `CLAUDE.md`) updated accordingly

---

## 15. GitHub repo system

The repo is `OmerYasirOnal/studio` (public). What exists:

- **Issue templates** (`.github/ISSUE_TEMPLATE/`): `bug.md`, `feature.md` — generic; will add `agent-task.md`
- **Workflows** (`.github/workflows/`):
  - `ci.yml` — root sanity (lint, no-secrets, asset-license check)
  - `observer-smoke.yml` — framework dashboard smoke test
  - `bb-*` × 7 — brave-bunny Unity-specific (lint, unity-test, ios-build, simulator-test, nightly-tests, weekly-ios-smoke, dependency-audit)
- **Branches** (remote): `main` + `feat/adr-0020-weapons` (stale, broken commit per memory)
- **Worktrees**: 42 active local worktrees (stale agent leftovers; locked; need cleanup)

### 15.1 Branch hygiene plan

| Branch | Action | Reason |
|---|---|---|
| `main` | Keep | trunk |
| `feat/adr-0020-weapons` | **Delete** | Broken commit, doesn't compile, will be re-implemented on web stack |
| `feat/meta-progression-character-unlocks` | **Review then likely delete** | Pre-pivot work; meta progression re-spec'd in new stack |
| `wave7a-integration` | **Delete** | Unity-specific integration; obsolete |
| `worktree-agent-*` (40+ branches) | **Bulk delete** + `git worktree remove --force` | Stale agent leftovers; no value to preserve |

Execution: an `engine-pivot/cleanup-branches.sh` script (idempotent) runs as the first implementation task. Local worktrees pruned first (so branch delete succeeds), then `git push origin --delete` for any tracked stale refs.

### 15.2 New branch model post-pivot

```
main                              # always green, always shippable
└── pivot/engine-three-r3f        # umbrella feature branch (the pivot itself)
    ├── pivot/sprint-a-bootstrap  # short-lived per-sprint topic branches
    ├── pivot/sprint-b-assets
    └── ...
```

Linear history. Squash-merge into `main` ONLY for completed sprint sets where every commit on the topic branch was an atomic conventional commit (consistent with the `core/CLAUDE.md` "no squash merges that lose history" rule means: never use squash to merge a 50-commit topic, but a 3-commit clean topic is fine).

### 15.3 CI workflow rewrite

Replace the 7 Unity workflows with 6 web-stack equivalents. Naming: keep `bb-` prefix for game-specific so framework-wide workflows are easy to spot.

| Old (delete) | New (add) | Triggers | What it does |
|---|---|---|---|
| `bb-unity-test.yml` | `bb-web-test.yml` | PR + push | `npm ci && npm run typecheck && npm test` (Vitest) |
| `bb-simulator-test.yml` | `bb-e2e.yml` | PR | Playwright e2e on Chromium headless |
| `bb-ios-build.yml` | `bb-ios-build.yml` (rewritten) | PR + nightly | `npm run build && npx cap sync ios && xcodebuild -workspace ... -scheme App build` (no signing — fast) |
| `bb-weekly-ios-smoke.yml` | `bb-ios-smoke.yml` (weekly + manual) | cron weekly | Full `fastlane beta` to TestFlight; signed; uploads |
| `bb-lint.yml` | `bb-lint.yml` (rewritten) | PR + push | `eslint . && prettier --check .` |
| `bb-nightly-tests.yml` | `bb-nightly-bench.yml` | cron nightly | Headless Chromium perf bench on 200-enemy stress scene |
| `bb-dependency-audit.yml` | keep, retarget | weekly | `npm audit` + `npx better-npm-audit` |

`ci.yml` (root) keeps its current scope (license + secret scan + observer Python tests).

### 15.4 Issue + PR templates

| File | Purpose |
|---|---|
| `.github/ISSUE_TEMPLATE/bug.md` | exists — minor edits, add "Engine: web (Three.js+R3F+Capacitor)" field |
| `.github/ISSUE_TEMPLATE/feature.md` | exists — add scope checkbox for `games/<active>/app/` |
| `.github/ISSUE_TEMPLATE/agent-task.md` | NEW — for tasks dispatched to subagents (handoff brief template) |
| `.github/PULL_REQUEST_TEMPLATE.md` | NEW — checklist: (a) tests pass, (b) typecheck pass, (c) perf budget if rendering, (d) ADR if architectural |

### 15.5 README

Root `README.md` rewritten:
- Engine badge: `engine-Three.js+R3F+Capacitor` (replacing `Unity 6 LTS`)
- Status badge updated to current sprint
- Replace Unity quick-start with `npm` + `npx cap` commands
- Keep multi-game framework framing
- Add "Why we pivoted" note linking to this spec

Game-level `games/brave-bunny/README.md` (if exists; if not, created): build status, vertical-slice progress, link to GDD, link to ADR-0030.

---

## 16. Orchestration model — Claude as the brain

The user wants Claude (this conversation) to be the system orchestrator: dispatch parallel agents, manage state, decide what to do next, surface only blockers.

### 16.1 Roles

| Role | Who | What |
|---|---|---|
| **Orchestrator** (system brain) | This Claude session (main thread) | Reads project state, plans phases, dispatches subagents, reviews their handoffs, gates merges, surfaces blockers |
| **Specialist agents** | Subagents (Task tool with role-specific brief) | One focused task each; one handoff note per task; no chat-back |
| **Human** (Ömer) | Final approver | Spec/plan approvals, escalations from §17, App Store interactive UI |

### 16.2 Dispatch pattern

Per `core/CLAUDE.md` § "Token efficiency discipline":
- Each subagent receives a **minimal brief** (≤ 200 lines), self-contained, never the parent conversation
- Each subagent emits a **handoff note** to `games/brave-bunny/docs/handoffs/<agent>-<timestamp>.md`
- Orchestrator reads only the handoff note + the diff, not the subagent's full transcript

For this pivot, the orchestrator dispatches subagents in waves:

```
Wave 1 (parallel, no shared state):
  - tech-architect → rewrites docs/06-tech-spec/{00,01,06,10}
  - build-engineer → adapts fastlane for Capacitor; drafts new CI workflows
  - asset-curator → downloads + compresses Quaternius UAL2 + Animated Animals
  - blender-tech → drafts bake-vat.py; smoke-tests on Bunny model

Wave 2 (depends on wave 1):
  - systems-engineer → scaffolds app/src/ecs + state + platform glue
  - gameplay-engineer → ports Carrot Spear weapon end-to-end (the spike)
  - ui-engineer → scaffolds app/src/ui Lobby + HUD shells

Wave 3 (integration):
  - qa-engineer → Vitest harness, first 10 unit tests, Playwright smoke
  - Single agent (gameplay or systems) → 200-enemy stress test
```

The orchestrator does NOT do implementation work itself when a subagent can. The orchestrator's job is: plan → dispatch → review → integrate.

### 16.3 Parallel execution discipline

- Use the `Agent` tool with `run_in_background: true` for waves where tasks are independent
- Per-wave gate: all agents in a wave must hand off before the next wave starts
- Conflict avoidance: file ownership map (root `CLAUDE.md`) enforced — no two agents touch the same path in the same wave
- Worktree isolation (existing `superpowers:using-git-worktrees`) for any agent doing substantive code changes; agents merge their topic branch into `pivot/engine-three-r3f` via PR, not direct push to main

### 16.4 State surface for Claude orchestrator

Claude consults, in this order, when picking the next action:
1. The implementation plan (output of `superpowers:writing-plans`)
2. Sprint status in `games/brave-bunny/docs/11-roadmap/`
3. Handoff notes in `games/brave-bunny/docs/handoffs/`
4. CI status via `gh run list --workflow=...`
5. Memory (`MEMORY.md`) for prior-session context
6. Latest commits + branch state

### 16.5 Escalation to human (unchanged)

Per `core/CLAUDE.md` Escalation triggers:
- Apple Developer interactive UI
- Cross-commit test breakage with no revert fix
- 3 approaches × 2 hours stuck on a real blocker

Otherwise: ADR + decide + proceed.

---

## 17. Approval log

- 2026-05-16 — High-level architecture and VAT crowd-rendering decision approved by Ömer Y.
- 2026-05-16 — Decision to delete `games/brave-bunny/unity/` entirely (legacy preserved in git history only) approved by Ömer Y.
- 2026-05-16 — Scope expansion: agent-definition updates, GitHub repo system rebuild, Claude-as-orchestrator model — included in this spec at user's request.
- _Pending_ — Final spec doc review by Ömer Y. before invoking `writing-plans` for implementation plan.
