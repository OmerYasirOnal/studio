# Current phase

**Phase:** 5 — Prototype (FIRST iOS .ipa archive shipped; TestFlight upload gated on 2 manual user steps)

> Phases 0-4 are complete. Phase 5 reached its first major milestone on 2026-05-12: a signed, app-store-ready `.ipa` (104 MB) was produced end-to-end via `fastlane preview`. The next gate is TestFlight upload (`fastlane beta`), which requires the ASC app entry to exist — see "Active blockers" below.

## Phase 1 — Discovery (COMPLETE)

- [x] 5 competitor deconstructions (Survivor.io, Vampire Survivors, Archero, Brotato, Capybara Go!)
- [x] Market overview (`docs/01-research/01-market.md`)
- [x] Positioning + feature matrix + risk matrix (`docs/01-research/03-positioning.md`)
- [x] References + source quality grades (`docs/01-research/04-references.md`)
- [x] Soft-launch market scan (TR/PH/ID)

## Phase 2 — GDD (COMPLETE)

- [x] 14 GDD sections (00 overview through 13 risks-and-cuts)
- [x] Narrative: tone bible, world premise, 8 character bios with TR seeds, biome flavor, 5 boss intros, 89 localization keys
- [x] UX: 62 user stories across 5 epics
- [x] UX: 5 flow diagrams (Mermaid)
- [x] UX: 15 HTML wireframes + shared `_style.css` (iPhone SE 3 fit verified)
- [x] Art bible: 10 sections (overview, palette, lighting, character-style, environment-style, vfx-style, ui-direction, iconography, asset-budget, source-shortlist)
- [x] Audio bible: 5 sections (overview, BGM 12-track spec, SFX 50-slug spec, source-shortlist, mixer-routing)
- [x] Initial balance JSON sheets (8 files + 8 schemas)
- [x] Balance docs: formulas, tuning philosophy, character/weapon/enemy/economy tuning, Monte Carlo notes
- [x] Level design: 5 biomes laid out + 5 bosses speced + waves.json per biome

## Phase 3 — Tech Architecture (COMPLETE)

- [x] 11 tech-spec docs (engine, project layout, data model, save system, input, perf budget, rendering, audio, state machine, event bus, build-and-ci, third-party)
- [x] 16 ADRs total (0001-0015; 0011 and 0012 proposed/deferred; 0013-0015 added during Phase 5 implementation)

## Phase 4 — Asset Pipeline (COMPLETE — 22 real CC0 fetches on disk)

- [x] Asset INDEX with planned roster mapped to CC0 sources (`assets-raw/INDEX.md`)
- [x] LICENSES.md with all 24 files audited + permissive
- [x] Blender pipeline templates: 4 example `build.py` recipes
- [x] 22 real CC0 downloads (216 MB): 10 Kenney zips, 4 Polyhaven HDRIs, 5 ambientCG PBR, 3 Google Fonts
- [x] ADR-0014: Otter→Beaver fallback (Quaternius Otter unavailable)

## Phase 5 — Prototype (MILESTONE HIT — first .ipa archive shipped)

## Wave 6 (2026-05-13) — agent-routed, 6 parallel agents

| Subsystem | Status | Commit |
|---|---|---|
| **Crash fix** — empty scenes → populated; Boot→Run SceneFlow; null-safe Bootstrap | ✅ shipped | `7cce787` (closes #1) |
| **PerfStress populator** + FPS PlayMode test (200 enemies, 50 projectiles, 30 VFX) | ✅ shipped | `5fafb43` |
| **ADR-0020** WeaponArchetypeConfig + 3 archetypes + EnemyRole.Boss | ✅ shipped | `f0a1b46` |
| **IDeathListener** + enemy pool return (ADR-0019 item 3) | ✅ shipped (same commit) | `f0a1b46` |
| **8 HUD icons** — 6 Kenney CC0 + 2 custom SVG | ✅ shipped | `bac168b` |
| **IRunRuntimeState live binding** — RunController impl + BindState | ⏸️ deferred — duplicate interface conflict with Wave 5; needs reconciliation | branch `worktree-agent-ad5bee576529346e8` |

Wave 6 totals: 5 commits on main + 1 deferred branch. EditMode tests: 209 total (198 pass, 11 fail — pre-existing ADR-0019 backlog, not regressions). PlayMode: 7/7 pass.

What was done autonomously this session series (commits `ac688ed` → `3716734`):

- [x] Unity 6 LTS 6000.0.74f1 + iOS Build Support installed via `Unity Hub --headless`
- [x] Unity project compiles with 0 errors (149 C# files across 6 asmdefs)
- [x] **41/41 EditMode tests pass** (commit `9cebdbd`)
- [x] **6/6 PlayMode tests pass** after fixing InputSystem activeInputHandler (-1→1)
- [x] **47/47 total tests pass** (commit `cb36929`)
- [x] BalanceJsonImporter Editor script → SOs generated from `data/balance/*.json`
- [x] SceneSetup.cs: Boot/MainMenu/Loadout/Run/PerfStress scenes generated; Run scene has top-down camera + Player cube + Meadow plane
- [x] Apple Developer integration — bundle id `com.omeryasir.bravebunny`, cert `K83U6UWWN4`, IAP-enabled provisioning profile, all encrypted to `OmerYasirOnal/studio-certs`
- [x] Fastlane lanes complete: `asc_api_key`, `register_app`, `list_apps`, `enable_iap`, `refresh_profile`, `preview`, `beta`, `release`
- [x] `fastlane preview` produces signed .ipa locally: `Builds/BraveBunny-preview-<ts>.ipa` (104 MB) + dSYM (191 MB)
- [x] ADR-0015: test/production API drift documented
- [x] GitHub Actions: 5 ASC secrets set on `OmerYasirOnal/studio`; `bb-ios-build.yml` workflow live

## Active blockers (2026-05-13 status)

### 🔴 P0 — TestFlight 0.1.0(2) crashes on launch (Issue #1)

Tester report: iPhone 14 Pro / iOS 26.3.1 / build 0.1.0(2) crashes immediately.

**Root cause:** Boot/MainMenu/Loadout/PerfStress scenes shipped EMPTY (12 YAML docs each, 0 GameObjects). `SceneSetup.cs::Ensure*` methods skip if file exists, so the empty .unity placeholders no-op the scaffolder. No SceneManager.LoadScene call exists anywhere in non-test code → no Boot → Run transition.

**Fix:** Wave 6 / `crash-fix-engineer` agent dispatched 2026-05-13. See Issue #1 for full diagnosis.

### ✅ ASC app entry — RESOLVED (verified via `fastlane list_apps` 2026-05-13)

```
com.omeryasir.bravebunny   Brave Bunny: Survivors
```

Entry exists in ASC. The earlier "ASC API key role" gate was already cleared.

### ✅ Repo visibility — PUBLIC (flipped 2026-05-13)

`gh repo edit OmerYasirOnal/studio --visibility public`. Side benefits: unlimited free CI minutes, no `workflow`-scope gate for community PRs.

### 🟡 P2 — Apply pending bb-ios-build.yml workflow update

`gh` token still lacks `workflow` scope; `apply-pending-workflow.sh` correctly refuses. NOT a blocker for shipping (local `fastlane beta` works), only blocks auto-CI builds.

**Path to fix (one-time, user-side):**

```bash
gh auth refresh --hostname github.com --scopes workflow
./games/brave-bunny/tools/ci/scripts/apply-pending-workflow.sh
```

## Next steps (once the 2 blockers above are cleared)

```bash
# 1. Push first build to TestFlight
cd games/brave-bunny/tools/ci/fastlane
fastlane beta

# 2. From this point onward, every iOS-affecting push to main
#    runs bb-ios-build.yml in GitHub Actions → uploads to TestFlight
```

## Recommended cadence (8-week soft-launch path)

| Week | Focus | Owner agents | Status |
|---|---|---|---|
| 2026-05-12 (this week) | Phase 5 milestone: .ipa shipped, ASC entry + workflow scope | build + human | 🟡 90% — 2 manual steps left |
| Week of 2026-05-19 | First TestFlight upload; joystick + auto-attack gameplay | gameplay + build | ⏳ |
| Week of 2026-05-26 | 200-enemy stress scene at 60fps; pooling + draw-call audit | gameplay + qa | ⏳ |
| Week of 2026-06-02 | Save round-trip + boot-to-meadow gameplay loop | systems + gameplay | ⏳ |
| Week of 2026-06-09 | UI Toolkit screens wired to runtime state | ui + systems | ⏳ |
| Week of 2026-06-16 | Old Boar King boss fully fightable | gameplay + level-designer + balance | ⏳ |
| Week of 2026-06-23 | Vertical-slice QA gate (Phase 6 exit) | qa | ⏳ |
| Week of 2026-06-30 | TestFlight beta polish + localization pass | qa + build + narrative | ⏳ |
| Week of 2026-07-07+ | Soft-launch TR/PH/ID | (entire team) | ⏳ |

## Progress log

| Date | Event |
|---|---|
| 2026-05-11 | Phase 0 (Framework) complete — studio v0.1.0 bootstrapped |
| 2026-05-11 | Phase 0 (Game) complete — brave-bunny scaffolded |
| 2026-05-12 | Phases 1-3 complete on documents (3 continuous sessions) |
| 2026-05-12 | Phase 4 — 22 real CC0 fetches (216 MB) on disk |
| 2026-05-12 | Phase 5 wave 1 — Unity skeleton + 149 C# files + 12 test files |
| 2026-05-12 | Phase 5 wave 2 — Apple Developer integration (cert + profile + ASC secrets) |
| 2026-05-12 | Phase 5 wave 3 — Unity 6 LTS 6000.0.74f1 installed; 41/41 EditMode tests PASS |
| 2026-05-12 | Phase 5 wave 4 — 47/47 tests PASS (41 EditMode + 6 PlayMode) |
| 2026-05-12 | **Phase 5 milestone — FIRST iOS .ipa archive built (104 MB, signed)** |
| 2026-05-12 | Fastfile fix: register_app now uses Spaceship::ConnectAPI::App.create (api_key-aware) |

## Next agents to spawn (after the 2 manual steps are done)

```bash
# Phase 5 finish — joystick + auto-attack + 200-enemy stress
./core/scripts/spawn-agent.sh gameplay-engineer "Open the Unity project. Implement joystick + auto-attack per tech-spec 04-input.md. Wire to the existing Run scene. Stand up the PerfStress scene with 200 enemy spawners + 50 projectiles + 30 VFX puffs. Target 60fps on iPhone 12 per the perf contract."

# Phase 6 prep — save round-trip
./core/scripts/spawn-agent.sh systems-engineer "Implement SaveService per tech-spec 03 + ADR-0008 (Newtonsoft JSON in binary wrapper). Round-trip test, corruption test, backup rotation. Confirm save_schema_v1."

# Phase 6 prep — UI wiring
./core/scripts/spawn-agent.sh ui-engineer "Wire the 5 UXML screens (Boot, MainMenu, Loadout, Run, Pause) to live runtime state. Theme USS is already in place. Start with Home + Run HUD as critical path."

# Continuous — QA gates
./core/scripts/spawn-agent.sh qa-engineer "Extend EditMode tests for ADR-0009 (mechanic registry). Add PlayMode test for 200-enemy stress scene FPS floor. Wire to Unity Test Framework + CI."
```

## Open blockers (require human input)

- [ ] **ASC app entry** — one Apple web-UI click (path A or path B above)
- [ ] **gh workflow scope** — one `gh auth refresh` command (interactive OAuth)
- [ ] **Quaternius pack** — manual click-through OR accept ADR-0014 Otter→Beaver fallback (already accepted, but raw assets still missing if Otter ever revisited)

## Reading order for the next agent / collaborator

1. `games/brave-bunny/GAME.md` (concept + scope + cut list)
2. This file
3. `docs/handoffs/orchestrator-20260512-apple-integration.md` (Apple side detail)
4. `docs/decisions/INDEX.md` (16 ADRs)
5. `games/brave-bunny/tools/ci/runbooks/first-build.md` (manual steps in detail)
6. Any specific subsystem you're picking up — go to its docs + handoffs
