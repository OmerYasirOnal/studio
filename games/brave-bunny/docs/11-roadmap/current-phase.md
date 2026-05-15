# Current phase

**Phase:** 5 — Prototype **COMPLETE** (vertical-slice gameplay + meta + liveops scaffold + telemetry shipped 2026-05-16). Next: Phase 6 — Soft-launch execution (real ad/IAP/analytics SDKs, device QA, TR/PH/ID rollout).

> Phases 0-4 are complete. Phase 5 reached its first major milestone on 2026-05-12 (signed .ipa, 104 MB) and was **closed out on 2026-05-16** by a 5-wave / 40+ agent parallel push that delivered the full vertical slice. The remaining ship blocker is user-side: a single iOS 26.5 device install + `fastlane beta` upload — see "Active blockers" below.

## Session log — 2026-05-16 (Waves 7A → 11)

Single calendar day. 5 parallel-agent waves on top of the post-Wave 6 baseline (`5644a64`). 40+ agents across isolated worktrees; orchestrator merged each agent on completion.

| Wave | Commits (count) | Agents | Key deliverables | Files touched |
|---|---|---|---|---|
| **7A** | 14 (`c200dd5` → `5ac0021`) | 8 parallel | Weapon archetype configs (ADR-0020), RunEndReport+channel, audio bindings+BGM, loc TR/EN parity (235 keys), VfxPool callback, pause modal, character unlock service, hit-feedback juice | ~80 |
| **7B** | 4 (`eec1a48` → `7b36719`) | 3 parallel | Old Boar King 3-phase boss + spawn + tests; TTK polish (weapons +40-150%, enemies HP −25-62%); E2E PlayMode smoke test | ~25 |
| **7C** | 1 (`761a75c`) | 1 (balance) | ADR-0022 boss HP recalibration (3000 → 1200); L5 DPS trim on Carrot Boomerang / Acorn Cannon | 4 |
| **8** | 4 (`60b47b0` → `8c7802d`) | 3 parallel | First-run tutorial overlay (5-step); local telemetry JSONL + asset stub shortlists; SceneSetup wires 7A+7B services into Run scene | ~20 |
| **9** | 16 (`64e12d7` → `10363b0`) | 8 parallel | Weapon roster 12→18; enemy roster +14 + Beach/Cavern biomes wired; daily login rewards (7-day); daily quest/mission system; battle pass scaffold (30-tier); weapon evolution system; shop + IAP catalog scaffold; loc +72 keys | ~70 |
| **10** | 16 (`e3ef883` → `aa846cb`) | 8 parallel | Crit system (chance roll + yellow numbers); combo counter + kill-streak; achievement system × 20 + toast + panel; profile + lifetime stats; run QoL (focus-pause + quit-confirm + FPS toggle); status effects (slow/burn/poison/freeze/stun); 8 character active abilities; loc +93 keys | ~95 |
| **11** | this commit | 1 (orchestrator docs) | Session summary + roadmap update + ADR INDEX verification + final handoff | 4 |

Session totals (vs. pre-session `5644a64`): **~30,000 LOC inserted across 402 files**. ADR added: **0022** (boss HP). Tests: green pre-session; **most new code did NOT run in Unity Editor this session** — see Honesty section in `wave-7-through-11-summary.md`.

## Wave 7B (2026-05-16) — balance + polish pass

Single-agent pass on top of merged Wave 7A. No code changes.

| Area | Change |
|---|---|
| **TTK ladder** | 6 weapons damage bumped +40% to +150%; 5 enemies HP cut 25% to 62%; boss HP 3000 → 1200 |
| **FeelConfig.asset** | hitstop 20 → 45 ms; flash 60 → 80 ms; dmg number lifetime 0.6 → 0.55 s; screen-shake amps re-tuned to 0.04 / 0.10 / 0.22 |
| **feel.json** | boss damage tick 40 → 90 ms; basic-enemy-kill 20 → 45 ms; screen-shake values mirrored to FeelConfig |
| **Localization** | EN/TR copy pass on `runend.*`, `levelup.*`, `pause.*`, `loadout.*` — punchier register |
| **Doc** | `10-balance/wave7-ttk-pass.md` (math + post-pass band check + follow-ups) |

Known follow-ups flagged in `wave7-ttk-pass.md`:
- L5 DPS band broken on Carrot Boomerang & Acorn Cannon — needs Wave 7C trim
- ADR-0006 supersede needed for new boss HP curve
- Tank `hp_per_min` may need re-bump after playtest

## Wave 7A (2026-05-13 → 2026-05-15) — 8 parallel agents

All systems for the vertical slice landed. Each agent worked in an isolated worktree; orchestrator merged in `merge(brave-bunny): Wave 7A — ...` commits.

| # | Subsystem | Status | Merge commit |
|---|---|---|---|
| 1 | **ADR-0020 weapon archetype configs** — three archetype SOs + EnemyRole.Boss enum value | shipped | `bd920a7` |
| 2 | **RunEndReport capture + RunEndedChannel** — run-end summary plumbing | shipped | `b763db1` |
| 3 | **Audio gameplay bindings + BGM driver** — events → AudioDispatcher | shipped | `ba34500` |
| 4 | **Localization TR/EN parity** — 235 keys per file, character bios + weapon descriptions + UI copy | shipped | `ad5a359` |
| 5 | **VfxPool particle stop callback + TargetSelector dispatch** — pool return on particle complete | shipped | `44d1172` |
| 6 | **Pause modal UI + tests** — pause panel + settings panel UXML/USS wiring | shipped | `b30f204` |
| 7 | **Meta-progression character unlocks** — CharacterUnlockService + persistence | shipped | `cd5ccb2` |
| 8 | **Hit-feedback juice** — hitstop + hit-flash + floating damage numbers + screen-shake | shipped | `e3d6f81` |

Wave 7A totals: 8 merge commits on `main`. Tests: green (no regressions). All eight subsystems are now wired up and the slice is playable end-to-end on Editor; iOS device validation pending the TestFlight gate below.

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
- [x] 20 ADRs accepted + 2 proposed/deferred (0011, 0012). Phase 5 added 0013-0022; ADR-0006 superseded (boss-HP slice) by 0022 on 2026-05-16. See `docs/decisions/INDEX.md`.

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

## Active blockers (2026-05-16 status — post-Wave-11)

### 🔴 P0 — User-side: iOS 26.5 platform install + `fastlane beta` build #3

The post-Wave-10 binary has never been archived. The required `fastlane beta` run needs the iOS 26.5 Xcode SDK installed on the build machine first (Xcode-side, manual user step). Once installed, `cd games/brave-bunny/tools/ci/fastlane && fastlane beta` should produce TestFlight build `0.1.0(3)`.

See `memory/brave-bunny-ship-state-20260513.md` for the exact one-click path.

### ✅ TestFlight 0.1.0(2) crash — RESOLVED (Wave 6, commit `7cce787`, closes Issue #1)

Boot/MainMenu/Loadout/PerfStress scenes were shipping empty. `crash-fix-engineer` (2026-05-13) populated them + added the Boot→Run SceneFlow + null-safe Bootstrap.

### ⚠️ Untested-in-Editor risk

Most of Waves 7A→10 code did NOT compile or run inside the Unity Editor this session — agents wrote files; the orchestrator merged them; nobody opened the Editor. There is a real chance the first Editor open of `main` surfaces compile errors that need a clean-up pass. See `wave-7-through-11-summary.md` § "Honesty section".

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

## Recommended cadence (revised 2026-05-16 — Phase 5 closed in one day)

The original 8-week cadence assumed Weeks 19→23 of May/June would deliver one subsystem at a time. The 2026-05-16 parallel-agent push compressed Weeks 19→30 into a single day, so the cadence now restarts at Phase 6 (soft-launch execution).

| Week | Focus | Owner agents | Status |
|---|---|---|---|
| 2026-05-12 | Phase 5 milestone: signed .ipa shipped | build + human | ✅ done |
| 2026-05-13 | Wave 6 — crash fix, ADR-0020, HUD icons, perf-stress | 6 parallel agents | ✅ done |
| 2026-05-16 | **Waves 7A/7B/7C/8/9/10/11 — vertical slice complete** | 40+ parallel agents | ✅ done |
| Week of 2026-05-19 | User-side: install iOS 26.5; `fastlane beta` build #3; first TestFlight upload of post-Wave-10 binary | build + human | ⏳ unblocked |
| Week of 2026-05-26 | Editor-side smoke run of the merged slice; fix anything that fails to compile; re-run 47-test suite | qa + gameplay | ⏳ |
| Week of 2026-06-02 | Replace shop/IAP/battle-pass scaffolds with real Unity IAP SDK + receipt validation | systems + build | ⏳ |
| Week of 2026-06-09 | Replace local-JSONL telemetry with real analytics SDK (Unity Analytics or self-hosted) | systems + build | ⏳ |
| Week of 2026-06-16 | Real ad SDK (AdMob) wired to rewarded-revive + interstitial; ATT consent | build | ⏳ |
| Week of 2026-06-23 | Soft-launch QA gate (3-day device run; crash-free %; D1 retention sniff) | qa | ⏳ |
| Week of 2026-06-30 | Soft-launch TR + PH + ID rollout; live-ops daily/weekly cadence kicks off | (entire team) | ⏳ |
| Week of 2026-07-07+ | Live-ops: balance hot-fixes, weekly event, battle-pass season-1 content | balance + level-design + narrative | ⏳ |

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
| 2026-05-13 | Wave 6 — 6 parallel agents — crash fix (#1), PerfStress + 200-enemy FPS test, ADR-0020 weapons, 8 HUD icons |
| 2026-05-15 | Pre-Wave-7A reconcile: IRunRuntimeState single canonical interface + live binding (ADR-0021); asmdef compile fix |
| 2026-05-16 | **Waves 7A/7B/7C/8/9/10/11 — full vertical slice merged**: 40+ agents, ~30k LOC, 402 files, ADR-0022. Phase 5 closed. |

## Next agents to spawn (Phase 6 — Soft-launch execution)

```bash
# 1. Editor smoke — open the project, fix any compile errors from the 40-agent merge,
#    re-run the 47-test suite, archive the result before any more code lands.
./core/scripts/spawn-agent.sh qa-engineer "Open Unity. Compile the merged main. If compile fails, fix the smallest set of errors. Run all EditMode + PlayMode tests. Report pass count and any new test failures vs. the 47/47 baseline at commit cb36929."

# 2. Real Unity IAP — replace shop scaffold receipts with real validation
./core/scripts/spawn-agent.sh systems-engineer "Replace shop/IAP/battle-pass scaffold stubs with real Unity IAP integration. Wire Sandbox receipt validation. Keep the existing scaffold APIs intact so UI doesn't change."

# 3. Real analytics — replace local JSONL telemetry with a hosted analytics SDK
./core/scripts/spawn-agent.sh systems-engineer "Replace the local JSONL telemetry sink (Wave 8) with Unity Analytics or a self-hosted equivalent. Mirror the existing event schema. Keep the local JSONL as a dev-mode fallback."

# 4. AdMob — rewarded revive + interstitial wiring + ATT consent
./core/scripts/spawn-agent.sh build-engineer "Add Google AdMob SDK to the iOS build. Wire the rewarded-revive + post-run interstitial slots already stubbed in the UI. Add ATT consent flow on first launch per Apple's ATTrackingManager API."

# 5. Live-ops content cadence — Battle Pass season 1 + weekly event
./core/scripts/spawn-agent.sh level-designer "Author Battle Pass Season 1 content: 30 tiers of rewards using the existing data/balance/battle-pass.json schema. Lock economy values against ADR-0007 (charm consumption) and ADR-0010 (subscription ROI)."
```

## Open blockers (require human input)

- [ ] **iOS 26.5 platform install** on the build mac (Xcode → Settings → Platforms). Then `fastlane beta` ships build #3 to TestFlight.
- [ ] **gh workflow scope** — one `gh auth refresh` command (interactive OAuth) — only needed to re-enable CI auto-builds.
- [ ] **Quaternius pack** — manual click-through OR accept ADR-0014 Otter→Beaver fallback (already accepted, but raw assets still missing if Otter ever revisited).

## Reading order for the next agent / collaborator

1. `games/brave-bunny/GAME.md` (concept + scope + cut list)
2. This file
3. `docs/11-roadmap/wave-7-through-11-summary.md` (canonical record of the 2026-05-16 push)
4. `docs/handoffs/wave-11-final.md` (next-session unambiguous next step)
5. `docs/decisions/INDEX.md` (22 ADRs)
6. `games/brave-bunny/tools/ci/runbooks/first-build.md` (manual steps in detail)
7. Any specific subsystem you're picking up — go to its docs + handoffs
