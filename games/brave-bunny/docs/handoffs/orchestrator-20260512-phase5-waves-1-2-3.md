# Orchestrator hand-off — 2026-05-12 (Phase 5 Waves 1-2-3, agent-routed)

**Current phase:** 5 — Prototype
**Active agents:** none (all dispatched agents wrote their hand-offs and exited)
**Session token usage (rough):** orchestrator + 5 dispatched agents ≈ 320k tokens

## Mode

Framework-disciplined. Five agent dispatches via Claude Code's Agent tool (in-session, mirrors `spawn-agent.sh` conventions): each agent got a tight brief, stayed in its domain, wrote a hand-off note in `docs/handoffs/`, appended a status entry to `logs/agent-status.jsonl`. Orchestrator only wrote ADRs and commit messages.

## Three Wave commits this session

| Commit | Wave | Highlights |
|---|---|---|
| `4dca8ef` | Wave 1 | build-engineer wired 1024 app icon into PlayerSettings (Editor script approach); gameplay-engineer rewrote PlayerMover.cs (XZ-plane, allocation-free, SO-sourced speed, 9 tests) |
| `80c35d4` | Wave 2 | balance-engineer extended BalanceJsonImporter to populate all 7 CharacterStats fields per `docs/10-balance/00-formulas.md`; gameplay-engineer completed VirtualJoystickInput (Touchscreen.current path, +9 tests) and added EnsureRunPlayerWiring to SceneSetup |
| `d1b5be2` | Wave 3 | gameplay-engineer migrated 4 enemy behaviours + AutoAttackController to XZ-plane (+ 6 tests); deleted PlayerController.cs + Mover.cs per ADR-0018; asset-curator catalogued branding icons in LICENSES.md (validator now green) |

## ADRs written

| # | Title | What it locks in |
|---|---|---|
| 0017 | PlayerMover canonical | One mover for the player, XZ-plane, matches camera |
| 0018 | Enemy XZ migration | Enemy + AutoAttack XY→XZ migration plan; allowed final deletion of Mover.cs/PlayerController.cs |

## Phase 5 state of progress

| Subsystem | State |
|---|---|
| App Store icon → Unity PlayerSettings | ✅ wired via Editor script, source-of-truth in `Assets/_Brave/Art/UI/AppIcon/` |
| Player movement (joystick + WASD) | ✅ PlayerMover + IInputProvider + VirtualJoystickInput, all XZ |
| Balance JSON → SO importer | ✅ all 7 CharacterStats fields populated |
| Run scene Player wiring | ✅ SceneSetup.EnsureRunPlayerWiring (run via Brave > Scaffold Phase-5 Scenes) |
| Enemy XZ-plane consistency | ✅ 4 behaviours migrated, allocation-free |
| Movement/ directory cleanup | ✅ exactly canonical surface (3 source + tests) |
| LICENSES.md validator | ✅ 26 files / 26 rows |
| `verify-game.sh --game brave-bunny` | ✅ 26 / 0 / 0 |
| TestFlight build with real icon (full Unity rebuild) | ⏳ not yet run — pending `fastlane beta` validation |

## What's still open (Wave 4 candidates, in priority order)

| Candidate | Owner agent | Size | Unblocks |
|---|---|---|---|
| `fastlane beta` end-to-end validation pass | build-engineer (or orchestrator-as-validator) | ~20 min | TestFlight build #3, confirms full Unity compile + icon flow |
| AutoAttack mechanics (cast cadence, projectile spawn, hit detection, damage application) | gameplay-engineer | 60-90 min | Visible combat in Run scene |
| SaveService implementation per ADR-0008 (Newtonsoft JSON in binary wrapper) | systems-engineer | 60-90 min | Persistence; UI screens that depend on save state |
| 200-enemy stress scene + 60fps perf test | gameplay-engineer + qa-engineer | 30 min build + iPhone 12 test | Phase 5 exit criterion |
| HUD screens wired to runtime state | ui-engineer | 60-90 min | Vertical-slice critical path (HP/XP/timer visible) |
| Store screenshots (5.5" + 6.5") | art-director | 30 min Canva + 30 min Unity-shot composition | Public submission (NOT TestFlight; TestFlight has #2 already) |
| BRAVE_FUTURE_API tests re-enable | qa-engineer | 30-60 min | Tech-debt cleanup per ADR-0015 |
| Re-enabling CI workflow scope | human action | gh OAuth refresh | Automated CI TestFlight uploads |
| In-house license tag formalisation in asset-policy | (orchestrator or framework maintainer) | 15 min docs | Future asset-curator clarity (asset-curator surfaced this gap) |

## Recommended Wave 4 (parallel-safe)

**Option A — Validation-first** (safer):
- build-engineer: run `fastlane beta` to validate the full pipeline. Produces TestFlight build #3.
- (No second parallel agent; reserve token budget for the long-running build.)

**Option B — Push-the-slice** (faster overall):
- gameplay-engineer: AutoAttack mechanics
- systems-engineer: SaveService

Both are file-disjoint and parallel-safe. Skips the `fastlane beta` validation until later.

**Recommendation:** **Option B** (push-the-slice). The Wave 1-3 work is all unit-tested and structurally verified; the `fastlane beta` cost (20 min, full Unity rebuild) is amortised better when it bundles a larger payload of new code. Run `fastlane beta` after AutoAttack and SaveService land, then we get TestFlight build #3 with: real icon + movement + auto-attack + persistence in one validation pass.

## Orchestrator discipline note

`tmux 3.6a` is installed (no longer blocked). The dispatches this session used Claude Code's in-session Agent tool with briefs that strictly mirror `spawn-agent.sh` conventions (hand-off file path, status JSONL entry, domain boundaries). Future sessions could move to `./core/scripts/spawn-agent.sh <agent> "<brief>"` to populate the observer dashboard with real tmux session entries — that requires the human to be willing to leave tmux sessions running async. The current in-session-Agent approach gives orchestrator immediate integration feedback at the cost of not lighting up the observer's "agent status" panel for spawned agents.

## Reading order for the next orchestrator session

1. This file
2. `docs/decisions/INDEX.md` — confirm 18 ADRs accepted
3. `docs/handoffs/{build,gameplay,balance,asset-curator}-engineer-20260512-19*.md` — the 5 agent hand-offs from this session
4. `git log --oneline -8` — three Wave commits
5. `Movement/` and `Enemies/` directories — see the new canonical layout
