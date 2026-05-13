# Tech Spec 05 — Performance Budget

> Owner: tech-architect. The per-frame CPU/GPU ms budget, memory caps, and on-disk app size targets for Brave Bunny. **Authoritative cross-reference: `brave-bunny/CLAUDE.md` perf contract (60 fps iPhone 12, 200 enemies, 80 DC, 250k tris) + `07-art-bible/08-asset-budget.md` on-disk asset budget.** Sister docs: `00-engine-and-version.md` (Burst / Job System), `04-input-system.md` (1-frame latency), GDD `05-enemies.md` (density curve).

## Target devices

| Device | Resolution | Target fps | Notes |
|---|---|---|---|
| **iPhone 12** | 2532 × 1170 | **60 fps baseline** | Sets the per-frame budget below |
| iPhone SE 3 | 1334 × 750 | 50–60 fps (acceptable degrade) | Smaller screen, slower CPU; degrade plan below |
| Android mid-range (e.g., Pixel 5) | 2340 × 1080 | 60 fps | Tracked separately by build-engineer |

## Per-frame CPU/GPU budget

**Frame budget at 60 fps = 16.67 ms.** We aim for **70% utilization = 11.6 ms hard ceiling** on typical play; the remaining ~5 ms cushions spikes, GC, and OS-side overhead. The table below allocates the 11.6 ms across subsystems plus a small reserve so total stays well under 16.67 ms.

| Subsystem | Budget (ms) | Owner | Notes |
|---|---|---|---|
| **CPU: Spawning + AI** | 2.0 | gameplay-engineer | Burst + Job System; wave driver + simple behavior LUT (no per-enemy MonoBehaviour Update) |
| **CPU: Collision (200 enemies + 50 projectiles)** | 2.5 | gameplay-engineer | Spatial-hash broadphase; **no Unity Physics on swarmers** — they use a custom 2D radial-overlap test |
| **CPU: Combat resolution** | 1.0 | gameplay-engineer | Damage formula evaluated only on contact events; not per-frame per-enemy |
| **CPU: Animations** | 0.8 | gameplay-engineer | GPU skinning where supported; CPU-skinning fallback rare; swarmers use 2-frame sprite-flip, not bone anim |
| **CPU: UI Toolkit** | 0.8 | ui-engineer | HUD redraws via panel-relative invalidation; full re-layout only on modal open/close |
| **CPU: Audio dispatch** | 0.3 | systems-engineer | Event-driven; voice cap 12; SFX pool pre-warmed at scene load |
| **CPU: Save / IO** | 0 (run loop) | systems-engineer | Saves run on boundary events only (see `03-save-system.md`); zero IO during the run |
| **Render: Geometry submission** | 2.5 | gameplay-engineer + art-director | 80 DC budget, batched aggressively (Quaternius shared meshes + instancing) |
| **Render: Lighting + post** | 3.0 | art-director (shader) | URP Forward+, toon ramp shader (ADR-0002); no real-time shadows on swarm enemies |
| **GPU: VFX particles** | 1.5 | art-director | GPU-instanced quads; per `07-art-bible/05-vfx-style.md` 500 particles peak |
| **Reserve / scheduler** | 1.2 | tech-architect | Margin for spikes, GC, OS work |
| **Total** | **15.6 ms** | | leaves **1.07 ms** to vsync (16.67 ms) |

### Notes on allocations

- **Spawning + AI (2.0 ms)** assumes 200 active enemies with their AI ticked at 30 Hz, not 60 Hz. Half the enemies tick on even frames, half on odd — common in survivor-likes. Validated separately by the gameplay-engineer in a stress scene.
- **Collision (2.5 ms)** is the most contended bucket. 200 enemies × player overlap + 50 projectiles × ≤4 nearby enemies each = ~400 broadphase tests + ~100 narrowphase. Burst + spatial hash brings this under 2 ms on iPhone 12 in early prototypes from Habby-clone open-source projects. We carry 0.5 ms cushion.
- **UI Toolkit (0.8 ms)** is the budget *during a run*. Menus run at 30 fps on a different scheduler (UI Toolkit supports targeted-frame-rate per panel) — out-of-run cost doesn't matter to the per-frame run budget.
- **Render: Lighting + post (3.0 ms)** matches the art-bible's 5 ms combined budget (we split 3.0 ms shading + 1.5 ms VFX + 0.5 ms post in the geometry bucket). ADR-0002's outline pass is the swing factor; if it overruns, the fallback is dropping the outline on swarm enemies (see "Failure modes" below).

## Memory budgets

iPhone 12 ships with 4 GB RAM but iOS allows a single app ~1.5 GB before jetsam pressure rises; we target well under that.

| Bucket | Budget |
|---|---|
| Texture memory (compressed runtime) | **120 MB** — ASTC 4×4 at launch; ETC2 dev builds; biome atlases + character albedos |
| Audio memory (in-memory pool for SFX) | **12 MB** — BGM streamed (not in this bucket); SFX OGG decoded to PCM at 22 kHz mono |
| Mesh memory | **40 MB** — Quaternius meshes + biome chunks; instance buffers ~5 MB |
| Code segment + runtime (IL2CPP managed heap + native) | **~50 MB** |
| GC roots / per-frame allocations | **~5 MB working set** — zero allocations in run hot path |
| Unity engine overhead (Profiler, allocator headroom) | **~25 MB** |
| **Total runtime RAM target** | **~250 MB** |

Under iOS's typical ~1 GB safe cap with substantial headroom. Validated via Xcode Instruments Memory profile at the end of Phase 5.

## On-disk app size

Per `07-art-bible/08-asset-budget.md`: asset on-disk total is **~124 MB**. With Unity engine baseline (~50 MB), shipping IL2CPP code, and IPA wrapper:

| Bucket | Size |
|---|---|
| Assets (meshes, textures, audio, UI per `08-asset-budget.md`) | ~124 MB |
| IL2CPP binary + engine | ~50 MB |
| IPA overhead | ~5 MB |
| **Total app on-disk** | **~180 MB** |
| **App Store hot-zone budget** | **≤ 200 MB** |

200 MB is the iOS App Store conversion-rate "hot zone" (above this Apple shows a "Download over Wi-Fi" prompt that crushes install rate). 20 MB margin covers localized fonts (PH/ID), post-launch event content (~10 MB hotfix DLC). If we exceed 200 MB, the cut-list per GAME.md kicks in (drop boss cosmetic variants first, then the 5th biome).

## Profiling cadence

- **Daily, Phase 5+:** gameplay-engineer captures a 60-second Profiler trace from a stress scene (200 enemies, peak VFX). Frame-time histogram committed to `games/brave-bunny/logs/perf-<date>.json`.
- **Weekly, Phase 5+:** Xcode Instruments capture on iPhone 12 hardware, GPU + Memory profile.
- **Pre-merge:** any PR touching the run hot path runs `tools/ci/perf-smoke.sh` (planned, build-engineer owns). Fails CI if 95th percentile frame time exceeds 20 ms on the reference scene.

## iPhone SE 3 degrade plan

The SE 3's A15 chip is faster than iPhone 12's A14 single-thread, **but** its smaller GPU + 4 GB RAM + smaller screen pixel count create a different envelope. Settings applied when `SystemInfo.deviceModel` matches an SE-class device (or via `settings.lowPowerMode`):

1. **Render resolution scale 0.85x** — game renders at 85% of screen pixels, upscaled by URP. Saves ~25% pixel shader cost.
2. **Smaller joystick** — already proportional to shorter edge (per `04-input-system.md`).
3. **Drop swarm shadows entirely** — swarmer prefabs use a `castShadows = false` variant.
4. **Reduce particle cap to 350** (from 500).
5. **Downgrade audio sample rate** to 16 kHz mono for SFX.
6. **Cap target frame rate at 50 fps** — `Application.targetFrameRate = 50` if Profiler shows sustained 55 ms p99 frame times.

These are toggles, not separate builds. The `RuntimeQualitySettings` SO holds the device-class profile.

## Failure modes if budget breached

Triage ladder. Each step is reversible; we never bake quality regressions into the master build.

1. **Drop swarm shadows.** Already planned for SE 3; apply on iPhone 12 too if shading hits 3.5 ms.
2. **Reduce particle cap** from 500 → 350 → 250 (in 50-step increments). Game-feel pillar 1 demands corpse-puffs; never below 250.
3. **Halve outline pass on enemies** — outline only on elites and bosses, not swarmers (matches ADR-0002's stated fallback).
4. **Reduce enemy AI tick rate** from 30 Hz → 20 Hz. Visible only on tank "charge" telegraphs; balance-engineer must re-validate TTK.
5. **Bake static lighting** in environment chunks (more pre-bake work, but recovers ~1 ms render).
6. **Cap on-screen enemies** below 200. **LAST RESORT** — the 200 cap is a design feature (per `brave-bunny/CLAUDE.md`); reducing it changes the genre register. If we hit this, escalate to game-designer for a wave-pacing redesign.
7. **Cut a biome** (GAME.md cut-list item 5 / 5 biome down to 4). Pure scope reduction, no quality compromise on remaining content.

The triage is logged: every applied fallback writes an entry to `games/brave-bunny/logs/perf-triage.jsonl` so the post-mortem is honest about which knobs we turned.

## Stress smoke test

An automated PlayMode test validates the perf contract at the scene level.

| Artefact | Path |
|---|---|
| Populator (Editor menu) | `unity/Assets/Editor/PerfStressPopulator.cs` |
| FPS sampler component | `unity/Assets/_Brave/Code/Diagnostics/FpsSampler.cs` |
| PlayMode test | `unity/Assets/_Brave/Code/Tests/PlayMode/Performance/PerfStressFpsTest.cs` |

**How to run:**

1. Open the Unity project.
2. Menu: **Brave > Populate PerfStress (200/50/30)** — spawns 200 enemy GameObjects (circle r=30u), 50 projectile spheres, 30 VFX placeholder cubes, and attaches `FpsSampler` to the MainCamera.
3. Open **Window > General > Test Runner**, select **PlayMode**, filter by `Category=Performance`, and run `PerfStressFpsTest`.

**Assertion:** `FpsSampler.AverageFps >= 30` after a 3-second warmup.
The 30 fps floor is intentionally loose so the test passes on slow CI VMs; the iPhone 12 hardware target remains 60 fps. Editor runs are treated as soft-pass (frame times in Editor are unreliable).

**CI integration:** tag `[Category("Performance")]` lets resource-constrained runners skip via:
```
dotnet test --filter "Category!=Performance"
```

## Cross-references

- `brave-bunny/CLAUDE.md` — perf contract (60 fps, 200 enemies, 80 DC, 250k tris).
- `07-art-bible/08-asset-budget.md` — on-disk + on-screen budget math we cross-check against.
- `00-engine-and-version.md` — Burst + Job System availability.
- `04-input-system.md` — 1-frame input latency target.
- ADR-0002 — toon shader cost (5 ms combined lighting+post target sub-bucketed here).
- ADR-0005 — engine choice gates these numbers; Unity 6 LTS URP is the assumption baseline.
- GDD `05-enemies.md` — density curve (10 → 178 enemies/min) that drives the spawning + collision budgets.
