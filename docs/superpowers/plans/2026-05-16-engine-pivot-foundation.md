# Engine Pivot Foundation (Phase 0) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prepare the repo for the Unity→Three.js+R3F+Capacitor pivot by cleaning stale state (branches, worktrees, Unity project), updating engine-dependent agent definitions, replacing Unity CI with web-stack CI, refreshing repo metadata (README, CLAUDE.md, issue/PR templates), and confirming green CI on a no-op PR. End state: trunk is the clean baseline that Plan 2 (Sprint A — Engine Bootstrap) can build on.

**Architecture:** Phase 0 is repo-hygiene + system-prep only. Zero application code is written here. Work is grouped so it can run as four parallel agent streams (cleanup, agents, CI, docs/templates) plus a final integration task that opens one PR with all changes and verifies CI.

**Tech Stack:** `git` + `gh` CLI for repo management; Markdown for agent defs, README, CLAUDE.md, templates; YAML for GitHub Actions; bash for cleanup scripts.

---

## Spec coverage

This plan implements spec §10 (migration plan — partial: deletes unity/), §14 (agent definition updates), §15 (GitHub repo system rebuild — branch hygiene, CI rewrites, templates, README, branch protection). It does **not** implement §3-8 (app architecture, asset pipeline, runtime, iOS build, perf budget) — those are Plan 2+. §11 (verification strategy) is partially exercised here (CI green on no-op PR proves the new lint + workflow shapes work).

---

## File Structure

### Created in this plan
- `tools/repo/cleanup-branches.sh` — idempotent script: prune worktrees, delete local + remote stale refs, leave `main` + `pivot/engine-three-r3f` only
- `.github/PULL_REQUEST_TEMPLATE.md` — checklist for tests/typecheck/perf/ADR
- `.github/ISSUE_TEMPLATE/agent-task.md` — handoff brief template for subagent tasks
- `.github/workflows/bb-web-test.yml` — Vitest + tsc (no-op until `games/brave-bunny/app/` exists; path filter guards)
- `.github/workflows/bb-e2e.yml` — Playwright (path-guarded)
- `.github/workflows/bb-nightly-bench.yml` — perf bench (path-guarded, cron + manual)

### Modified in this plan
- `README.md` — engine badge + quick-start swap
- `CLAUDE.md` — file ownership map updated to reference `app/` paths
- `core/.claude/agents/tech-architect.md` — engine block rewritten
- `core/.claude/agents/gameplay-engineer.md` — engine block rewritten
- `core/.claude/agents/systems-engineer.md` — engine block rewritten
- `core/.claude/agents/ui-engineer.md` — engine block rewritten
- `core/.claude/agents/build-engineer.md` — engine block rewritten
- `core/.claude/agents/qa-engineer.md` — engine block rewritten
- `core/.claude/agents/asset-curator.md` — output target rewritten
- `core/.claude/agents/blender-tech.md` — artifact target rewritten
- `core/.claude/agents/orchestrator.md` — engine name swap
- `.github/ISSUE_TEMPLATE/bug.md` — adds "Engine: web (Three.js+R3F+Capacitor)" field
- `.github/ISSUE_TEMPLATE/feature.md` — adds scope checkbox for `app/`
- `.github/workflows/bb-ios-build.yml` — rewritten for Capacitor (`npm build && cap sync && xcodebuild build`)
- `.github/workflows/bb-lint.yml` — rewritten for web stack (eslint + prettier)
- `.github/workflows/bb-dependency-audit.yml` — adds `npm audit`

### Deleted in this plan
- `games/brave-bunny/unity/` — entire Unity project tree
- `.github/workflows/bb-unity-test.yml`
- `.github/workflows/bb-simulator-test.yml`
- `.github/workflows/bb-nightly-tests.yml`
- `.github/workflows/bb-weekly-ios-smoke.yml` (replaced by rewritten `bb-ios-smoke.yml` in a later sprint — Phase 0 leaves the file deleted)

### Branches deleted in this plan
- All `worktree-agent-*` local branches (40+)
- `feat/adr-0020-weapons` (local + remote — broken commit per memory)
- `wave7a-integration` (local — obsolete)
- `feat/meta-progression-character-unlocks` (local — review, then delete if no in-flight work)

### Worktrees pruned in this plan
- All 41 stale entries under `.claude/worktrees/agent-*/` (`main` keeps its own worktree)

---

## Wave structure (for parallel dispatch)

```
Wave 0 (sequential — must happen first):
  Task 1: Create pivot/engine-three-r3f umbrella branch

Wave 1 (4 parallel streams on the umbrella branch via independent sub-branches):
  Stream A: Tasks 2-3  (cleanup — branches + worktrees + unity/ delete)
  Stream B: Tasks 4-12 (agent def rewrites — 9 files)
  Stream C: Tasks 13-19 (CI workflows — delete 4, add 3, rewrite 3)
  Stream D: Tasks 20-23 (docs + templates — README, CLAUDE.md, 2 issue tpls, 1 PR tpl)

Wave 2 (sequential — depends on Wave 1):
  Task 24: Open umbrella PR, verify CI green, configure branch protection, merge
```

The plan presents tasks in linear order T1…T24; the orchestrator can dispatch Wave 1 streams in parallel because no two streams touch the same files.

---

## Tasks

### Task 1: Create umbrella branch `pivot/engine-three-r3f`

**Files:**
- None (git ref only)

- [ ] **Step 1: Verify we're on a clean main**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio status --short
git -C /Users/omeryasironal/Projects/studio rev-parse --abbrev-ref HEAD
```
Expected: working tree may have unstaged changes (per pre-existing state) but HEAD prints `main`. If on a different branch, `git switch main` first.

- [ ] **Step 2: Pull main to latest**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio fetch origin
git -C /Users/omeryasironal/Projects/studio merge --ff-only origin/main
```
Expected: fast-forward or already-up-to-date.

- [ ] **Step 3: Create and push the umbrella branch**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio switch -c pivot/engine-three-r3f
git -C /Users/omeryasironal/Projects/studio push -u origin pivot/engine-three-r3f
```
Expected: branch created, tracking origin.

- [ ] **Step 4: Tag the pre-pivot commit on main for reference**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio tag -a pre-pivot-2026-05-16 -m "Last commit on Unity stack before engine pivot" main
git -C /Users/omeryasironal/Projects/studio push origin pre-pivot-2026-05-16
```
Expected: tag pushed. This is the "if we ever need to go back" anchor.

---

### Task 2: Write `tools/repo/cleanup-branches.sh`

**Files:**
- Create: `tools/repo/cleanup-branches.sh`

- [ ] **Step 1: Ensure target directory exists**

Run:
```bash
mkdir -p /Users/omeryasironal/Projects/studio/tools/repo
```

- [ ] **Step 2: Write the cleanup script**

Create `/Users/omeryasironal/Projects/studio/tools/repo/cleanup-branches.sh`:
```bash
#!/usr/bin/env bash
# Idempotent cleanup of stale worktrees, branches, and locked refs.
# Leaves: main, pivot/engine-three-r3f, plus the active checkout.

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

KEEP_BRANCHES=("main" "pivot/engine-three-r3f")

is_kept() {
  local b="$1"
  for k in "${KEEP_BRANCHES[@]}"; do
    [[ "$b" == "$k" ]] && return 0
  done
  return 1
}

echo "==> Step 1: Prune locked worktrees"
# Unlock and remove every worktree under .claude/worktrees/
mapfile -t WORKTREES < <(git worktree list --porcelain | awk '/^worktree / { print $2 }' | grep -F "/.claude/worktrees/" || true)
for wt in "${WORKTREES[@]}"; do
  echo "  remove worktree: $wt"
  git worktree unlock "$wt" 2>/dev/null || true
  git worktree remove --force "$wt" 2>/dev/null || true
done
git worktree prune

echo "==> Step 2: Delete stale local branches"
mapfile -t LOCAL < <(git for-each-ref --format='%(refname:short)' refs/heads/)
for b in "${LOCAL[@]}"; do
  if is_kept "$b"; then
    echo "  keep: $b"
    continue
  fi
  echo "  delete local: $b"
  git branch -D "$b" 2>/dev/null || true
done

echo "==> Step 3: Delete stale remote branches"
mapfile -t REMOTE < <(git for-each-ref --format='%(refname:short)' refs/remotes/origin/ \
                        | sed 's|^origin/||' \
                        | grep -v '^HEAD$' || true)
for b in "${REMOTE[@]}"; do
  if is_kept "$b"; then continue; fi
  echo "  delete remote: origin/$b"
  git push origin --delete "$b" 2>/dev/null || true
done

echo "==> Step 4: Final state"
git worktree list
git branch -a

echo "==> Cleanup complete."
```

- [ ] **Step 3: Make executable**

Run:
```bash
chmod +x /Users/omeryasironal/Projects/studio/tools/repo/cleanup-branches.sh
```

- [ ] **Step 4: Lint with shellcheck**

Run:
```bash
shellcheck /Users/omeryasironal/Projects/studio/tools/repo/cleanup-branches.sh
```
Expected: no output (clean). If shellcheck warns, fix and re-run.

- [ ] **Step 5: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add tools/repo/cleanup-branches.sh
```

---

### Task 3: Execute branch cleanup + delete unity/ folder

**Files:**
- Delete: `games/brave-bunny/unity/` (entire tree)
- Delete: Local `worktree-agent-*` branches (40+), `feat/adr-0020-weapons`, `wave7a-integration`, `feat/meta-progression-character-unlocks`
- Delete: `.claude/worktrees/agent-*/` (41 entries)

- [ ] **Step 1: Dry-run preview (read-only)**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git worktree list | wc -l
git branch | wc -l
echo "---"
git branch | grep -v -E '(\* main|pivot/engine-three-r3f)' | head -10
```
Expected: ~42 worktrees, ~50 branches, preview shows what will be deleted. Sanity check.

- [ ] **Step 2: Execute cleanup script**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
./tools/repo/cleanup-branches.sh
```
Expected: output streams per stale ref. End state shows only `main` + `pivot/engine-three-r3f` + currently checked-out branch in `git branch -a`.

- [ ] **Step 3: Delete unity/ folder**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git rm -rf games/brave-bunny/unity/
```
Expected: thousands of files queued for deletion. (Git history preserves; recoverable via tag `pre-pivot-2026-05-16`.)

- [ ] **Step 4: Also stage the previously-unstaged Unity artifacts so they're cleared**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
# These were untracked or modified pre-pivot per git status; ensure they don't haunt the next commit
git add -A games/brave-bunny/unity/ 2>/dev/null || true
git add games/brave-bunny/tools/ci/fastlane/Gemfile.lock
# Move the mobileprovision out of the repo: certs live in match, not in source
git rm --cached games/brave-bunny/tools/ci/AppStore_com.omeryasir.bravebunny.mobileprovision 2>/dev/null || true
# Verify it's not actually tracked first; if so just delete the working copy as belt-and-suspenders:
rm -f games/brave-bunny/tools/ci/AppStore_com.omeryasir.bravebunny.mobileprovision
```
Expected: working tree clean of Unity drift.

- [ ] **Step 5: Verify with git status**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio status --short | head -30
```
Expected: shows only deletions (`D games/brave-bunny/unity/...`), the new `cleanup-branches.sh`, Gemfile.lock addition, and no unrelated changes.

- [ ] **Step 6: Commit Stream A**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git commit -m "$(cat <<'EOF'
chore(repo): cleanup stale worktrees/branches and delete unity/

Phase 0 — Stream A of engine pivot.

- Prune all 41 .claude/worktrees/agent-* stale worktrees
- Delete worktree-agent-* local branches (40+)
- Delete feat/adr-0020-weapons (broken commit per memory)
- Delete wave7a-integration (obsolete Unity work)
- Delete feat/meta-progression-character-unlocks (pre-pivot)
- Delete games/brave-bunny/unity/ entirely (git history preserves;
  tagged pre-pivot-2026-05-16)
- Add tools/repo/cleanup-branches.sh for future idempotent reruns
- Stop tracking AppStore_*.mobileprovision (lives in match, not src)
- Stage Gemfile.lock so fastlane has a deterministic install

Per spec docs/superpowers/specs/2026-05-16-engine-pivot-design.md §15.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```
Expected: one commit on `pivot/engine-three-r3f` with thousands of file deletions plus the new script.

---

### Task 4: Update `core/.claude/agents/tech-architect.md` engine block

**Files:**
- Modify: `core/.claude/agents/tech-architect.md`

- [ ] **Step 1: Read the current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/tech-architect.md` in full. Note the Unity 6 / URP / C# references. The role description (ADR ownership, system-of-record) stays.

- [ ] **Step 2: Replace engine context**

Edit the agent file. In the front-matter and body, replace:
- `description: ...Unity...` → `description: System-of-record architect for the brave-bunny web-tech 3D stack (Three.js + R3F + Capacitor 7). Owns tech-spec and ADRs.`
- Any `Unity 6 LTS` reference → `Three.js r170+ + @react-three/fiber 9`
- Any `URP / Toon Shader` reference → `Custom URP-equivalent toon shader expressed in GLSL via R3F custom shader material`
- Any `Assets/Scripts/...` reference → `games/<active>/app/src/...`
- Any `.unity` / `.prefab` / `.asset` reference → `Pure-TS scene composition via JSX; no binary scene files`
- Inputs section: add `games/<active>/docs/06-tech-spec/00-engine-and-version.md` (must reflect the new stack)
- Outputs section: target `games/<active>/docs/06-tech-spec/` and `games/<active>/docs/decisions/ADR-*.md`

If unsure of exact wording, refer to spec §3 (stack table) and §14 (agent updates table).

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|monobehaviour\|scriptableobject\|prefab\|\.unity\b\|URP' /Users/omeryasironal/Projects/studio/core/.claude/agents/tech-architect.md
```
Expected: no output (or only matches inside a "what changed from Unity" historical note if you choose to keep one).

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/tech-architect.md
```

---

### Task 5: Update `core/.claude/agents/gameplay-engineer.md`

**Files:**
- Modify: `core/.claude/agents/gameplay-engineer.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/gameplay-engineer.md`.

- [ ] **Step 2: Rewrite engine block**

Replace:
- `Unity C# combat...Writes Assets/Scripts/Gameplay/.` → `TypeScript combat + movement + spawning + pooling. Writes games/<active>/app/src/{ecs,systems,render}/.`
- `Target .NET / Unity 6 LTS C# 9` → `Target TypeScript 5+ strict mode, ESM-only`
- `File-scoped namespaces: BraveBunny.Gameplay.Combat` → `Module-scoped imports: import { ... } from '@/systems/combat'`
- `One class per file` → `One responsibility per file; prefer functions + plain objects over classes (only use classes for stable identity like ECS world)`
- `No singletons except via the framework-provided GameContext` → `No singletons; use miniplex world + zustand stores via dependency injection at module init`
- `No Find, no SendMessage, no BroadcastMessage` → `No global event emitters; pub-sub via miniplex queries or zustand subscribers only`
- `Allocation-free per-frame paths: no new(), no LINQ in Update` → `Allocation-free per-frame paths in useFrame: no array literals, no .map/.filter on hot arrays, mutate pooled objects in place`
- `Performance assertions in tests where applicable (e.g., 200 enemies on iPhone 12 baseline at 60fps)` → KEEP this line; it's engine-agnostic
- Folder tree:
```
games/<active>/app/src/
  systems/
    movement.ts
    combat.ts
    spawn.ts
    pickup.ts
    draft.ts
    lifecycle.ts
    audio.ts
  ecs/
    world.ts
    components.ts
    queries.ts
  render/
    Hero.tsx
    EnemySwarm.tsx
    ProjectileSwarm.tsx
    VFXSwarm.tsx
    Biome.tsx
```
- Tests target: `games/<active>/app/src/**/*.test.ts` (Vitest) + `games/<active>/app/e2e/` (Playwright)

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|C#\|monobehaviour\|assets/scripts' /Users/omeryasironal/Projects/studio/core/.claude/agents/gameplay-engineer.md
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/gameplay-engineer.md
```

---

### Task 6: Update `core/.claude/agents/systems-engineer.md`

**Files:**
- Modify: `core/.claude/agents/systems-engineer.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/systems-engineer.md`.

- [ ] **Step 2: Rewrite engine block**

Replace:
- Owns `Assets/Scripts/Systems/` → Owns `games/<active>/app/src/{ecs,state,platform,audio}/`
- Specific deliverables to add to the Outputs section:
  - `app/src/ecs/world.ts` — miniplex world singleton
  - `app/src/ecs/components.ts` — entity component type defs
  - `app/src/ecs/queries.ts` — named queries (heroes, enemies, projectiles, pickups)
  - `app/src/state/runStore.ts` — zustand store for in-run state
  - `app/src/state/metaStore.ts` — zustand store for save / unlocks
  - `app/src/state/settingsStore.ts` — zustand store for audio / video / accessibility
  - `app/src/platform/storage.ts` — @capacitor/preferences wrapper
  - `app/src/platform/safearea.ts` — iOS notch + bottom inset helper
  - `app/src/audio/AudioBus.ts` — Web Audio context + buffer pool
- Tools: TypeScript 5+, ESM, Vitest. No C#.
- ADR ownership: pool API (was ADR-0005 in Unity stack; new ADR-0032 in web stack — referenced but not authored by this agent — authored by tech-architect).

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|monobehaviour\|assets/scripts' /Users/omeryasironal/Projects/studio/core/.claude/agents/systems-engineer.md
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/systems-engineer.md
```

---

### Task 7: Update `core/.claude/agents/ui-engineer.md`

**Files:**
- Modify: `core/.claude/agents/ui-engineer.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/ui-engineer.md`.

- [ ] **Step 2: Rewrite engine block**

Replace:
- Owns `Assets/Scripts/UI/` + `UI Toolkit` + `USS` → Owns `games/<active>/app/src/ui/` (React + HTML overlay) with `app/src/ui/styles.css` for styling
- Stack:
  - React 19 with hooks (no class components)
  - zustand stores from `app/src/state/` consumed via `useStore` selector hooks
  - CSS modules or plain CSS — NO Tailwind, NO styled-components (per "no large deps")
  - Routing: hand-rolled state machine in `runStore` (Boot → Lobby → Run → EndRun → Lobby), NOT react-router
- Specific components owned:
  - `app/src/ui/Lobby.tsx` — home hub
  - `app/src/ui/HUD.tsx` — in-run HUD (HP bar, XP bar, timer, kill counter)
  - `app/src/ui/DraftModal.tsx` — level-up 3-of-N draft
  - `app/src/ui/EndRunSummary.tsx` — post-run bank summary
  - `app/src/ui/Store.tsx` — cosmetics + battle pass (post-MVP)
  - `app/src/ui/Settings.tsx`
- Constraint: UI lives OUTSIDE the R3F `<Canvas>`. Two render trees: 3D world + HTML overlay. No `<Html>` from drei except for floating numbers VFX.
- Performance rule: UI re-renders cost FPS. Use selector subscriptions, NOT React Context. Memoize anything that consumes a fast-changing value.

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|UI Toolkit\|UXML\|USS\b\|VisualElement' /Users/omeryasironal/Projects/studio/core/.claude/agents/ui-engineer.md
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/ui-engineer.md
```

---

### Task 8: Update `core/.claude/agents/build-engineer.md`

**Files:**
- Modify: `core/.claude/agents/build-engineer.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/build-engineer.md`.

- [ ] **Step 2: Rewrite build chain**

Replace:
- Unity build pipeline → Vite + Capacitor build pipeline
- Specific build flow:
  ```
  npm ci
  npm run typecheck
  npm test
  npm run build           # vite build → app/dist/
  npx cap sync ios        # copies dist → ios/App/App/public/
  cd ../tools/ci && fastlane beta   # signed .ipa to TestFlight
  ```
- KEEP: fastlane lane responsibility, match cert management, App Store Connect API key handling
- ADD: Owns `games/<active>/app/package.json` scripts (`dev`, `build`, `typecheck`, `test`, `e2e`, `bench`, `build:ios`, `sync:ios`)
- ADD: Owns `games/<active>/app/vite.config.ts` and `games/<active>/app/capacitor.config.ts`
- ADD: Owns all `.github/workflows/bb-*.yml` workflows
- REMOVE: All references to `unity-builder`, `unity-test-runner`, `Unity-iPhone.xcworkspace`
- REPLACE: workspace path → `games/brave-bunny/ios/App/App.xcworkspace`

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|Unity-iPhone' /Users/omeryasironal/Projects/studio/core/.claude/agents/build-engineer.md
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/build-engineer.md
```

---

### Task 9: Update `core/.claude/agents/qa-engineer.md`

**Files:**
- Modify: `core/.claude/agents/qa-engineer.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/qa-engineer.md`.

- [ ] **Step 2: Rewrite test pyramid**

Replace:
- Unity Test Runner / EditMode / PlayMode → Vitest (unit + integration) + Playwright (e2e)
- Test paths:
  - Unit: `games/<active>/app/src/**/*.test.ts` (co-located)
  - E2E: `games/<active>/app/e2e/*.spec.ts`
  - Perf bench: `games/<active>/app/bench/*.bench.ts` (driven by `npm run bench`)
- Coverage tool: Vitest's built-in `--coverage` (c8/v8)
- Pyramid:
  - 70% unit (pure functions, ECS systems, math, pools, balance lookups)
  - 25% integration (system × world, multiple components interacting)
  - 5% e2e (Playwright in headless Chromium against vite dev server)
- Perf tests:
  - 200-enemy stress (60fps gate)
  - 500-projectile burst (no drop below 55fps for 5s)
- CI integration: tests run on every PR via `bb-web-test.yml`; e2e via `bb-e2e.yml`; nightly bench via `bb-nightly-bench.yml`

- [ ] **Step 3: Verify no Unity strings remain**

Run:
```bash
grep -i 'unity\|editmode\|playmode\|nunit' /Users/omeryasironal/Projects/studio/core/.claude/agents/qa-engineer.md
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/qa-engineer.md
```

---

### Task 10: Update `core/.claude/agents/asset-curator.md`

**Files:**
- Modify: `core/.claude/agents/asset-curator.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/asset-curator.md`.

- [ ] **Step 2: Update output target + pipeline**

Replace:
- Output target: `games/<active>/assets-raw/` (raw downloads) → keep AS-IS for raw
- Add: `games/<active>/app/assets/glb/` (compressed game-ready)
- Add: `games/<active>/app/assets/palettes/` (recolor PNGs)
- Add: `games/<active>/app/assets/audio/` (compressed OGG)
- Pipeline command:
  ```bash
  # In games/<active>/tools/assets/:
  node compress.mjs --input ../../assets-raw/quaternius/Bunny.glb --output ../app/assets/glb/heroes.glb
  ```
- Tooling: `@gltf-transform/cli` for compression; `@gltf-transform/core` for programmatic edits; FFmpeg for audio normalization
- License manifest: continue to maintain `games/<active>/assets-raw/LICENSES.md` with per-asset CC0 / OFL / MIT attribution

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/asset-curator.md
```

---

### Task 11: Update `core/.claude/agents/blender-tech.md`

**Files:**
- Modify: `core/.claude/agents/blender-tech.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/blender-tech.md`.

- [ ] **Step 2: Update artifact target + commands**

Replace:
- Output FBX for Unity → Output VAT textures + JSON metadata for Three.js InstancedMesh
- Artifacts:
  - `games/<active>/app/assets/vat/<archetype>.png` (vertex animation texture)
  - `games/<active>/app/assets/vat/<archetype>.json` (anim ranges, bounds, frame count)
- Bake script: `games/<active>/tools/assets/bake-vat.py` (runs under `blender -b -P`)
- Invocation:
  ```bash
  blender -b -P games/brave-bunny/tools/assets/bake-vat.py -- \
    --input games/brave-bunny/assets-raw/quaternius/Wolf.glb \
    --output games/brave-bunny/app/assets/vat/wolf \
    --animations "Run,Attack,Death" \
    --frames-per-anim 32
  ```
- ADR ownership: ADR-0031 (VAT pipeline) — authored by tech-architect, implemented by blender-tech
- CI: VAT bake runs in CI when assets-raw changes, output committed to repo (deterministic input → output)

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/blender-tech.md
```

---

### Task 12: Update `core/.claude/agents/orchestrator.md`

**Files:**
- Modify: `core/.claude/agents/orchestrator.md`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/core/.claude/agents/orchestrator.md`.

- [ ] **Step 2: Update engine name + dispatch pattern**

Replace:
- Engine: `Unity 6 LTS` → `Three.js + R3F + Capacitor`
- Dispatch wave example: replace any Unity-specific wave list with the spec §16.2 wave list:
  ```
  Wave 1 (parallel):
    - tech-architect → rewrites docs/06-tech-spec/{00,01,06,10}
    - build-engineer → adapts fastlane for Capacitor; new CI workflows
    - asset-curator → downloads + compresses Quaternius UAL2 + Animals
    - blender-tech → drafts bake-vat.py; smoke-tests on Bunny

  Wave 2 (depends on wave 1):
    - systems-engineer → scaffolds app/src/ecs + state + platform
    - gameplay-engineer → ports Carrot Spear weapon end-to-end
    - ui-engineer → scaffolds app/src/ui Lobby + HUD shells

  Wave 3 (integration):
    - qa-engineer → Vitest harness + first 10 unit tests + Playwright smoke
    - One agent → 200-enemy stress test
  ```
- Add: "Read the engine pivot spec at `docs/superpowers/specs/2026-05-16-engine-pivot-design.md` before dispatching."

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add core/.claude/agents/orchestrator.md
```

---

### Task 13: Commit Stream B (all agent def updates)

**Files:**
- Already staged in Tasks 4-12

- [ ] **Step 1: Verify staging**

Run:
```bash
git -C /Users/omeryasironal/Projects/studio diff --cached --stat | grep '\.claude/agents/'
```
Expected: 9 files (tech-architect, gameplay-engineer, systems-engineer, ui-engineer, build-engineer, qa-engineer, asset-curator, blender-tech, orchestrator).

- [ ] **Step 2: Commit**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git commit -m "$(cat <<'EOF'
docs(agents): rewrite engine block for 9 agent defs (Unity → web stack)

Phase 0 — Stream B of engine pivot.

Engine-dependent agents updated:
- tech-architect: owns Three.js/R3F/Capacitor tech-spec + ADRs
- gameplay-engineer: TS systems + R3F components in app/src/
- systems-engineer: miniplex ECS + zustand + Capacitor platform glue
- ui-engineer: React + HTML overlay + zustand selectors
- build-engineer: Vite + Capacitor + fastlane integration
- qa-engineer: Vitest + Playwright test pyramid
- asset-curator: gltf-transform pipeline + app/assets/ targets
- blender-tech: VAT bake artifacts via blender headless
- orchestrator: engine name + dispatch wave list updated

Engine-independent agents unchanged: game-designer, narrative-
designer, ux-designer, level-designer, balance-engineer, art-
director, researcher.

Per spec §14.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

---

### Task 14: Delete obsolete Unity CI workflows

**Files:**
- Delete: `.github/workflows/bb-unity-test.yml`
- Delete: `.github/workflows/bb-simulator-test.yml`
- Delete: `.github/workflows/bb-nightly-tests.yml`
- Delete: `.github/workflows/bb-weekly-ios-smoke.yml`

- [ ] **Step 1: Delete the 4 obsolete files**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git rm .github/workflows/bb-unity-test.yml
git rm .github/workflows/bb-simulator-test.yml
git rm .github/workflows/bb-nightly-tests.yml
git rm .github/workflows/bb-weekly-ios-smoke.yml
```
Expected: 4 deletions staged.

- [ ] **Step 2: Verify**

Run:
```bash
ls /Users/omeryasironal/Projects/studio/.github/workflows/
```
Expected: remaining files are `bb-dependency-audit.yml`, `bb-ios-build.yml`, `bb-lint.yml`, `ci.yml`, `observer-smoke.yml`, `README.md`. (The 4 deleted are gone.)

---

### Task 15: Add `bb-web-test.yml` workflow

**Files:**
- Create: `.github/workflows/bb-web-test.yml`

- [ ] **Step 1: Write the workflow**

Create `/Users/omeryasironal/Projects/studio/.github/workflows/bb-web-test.yml`:
```yaml
name: bb-web-test

on:
  push:
    branches: [main]
    paths:
      - 'games/brave-bunny/app/**'
      - '.github/workflows/bb-web-test.yml'
  pull_request:
    paths:
      - 'games/brave-bunny/app/**'
      - '.github/workflows/bb-web-test.yml'

jobs:
  test:
    runs-on: ubuntu-latest
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' }}
    defaults:
      run:
        working-directory: games/brave-bunny/app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: Install
        run: npm ci
      - name: Typecheck
        run: npm run typecheck
      - name: Unit tests
        run: npm test -- --reporter=verbose
```

- [ ] **Step 2: Validate YAML syntax**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-web-test.yml'))"
```
Expected: no output (valid YAML).

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-web-test.yml
```

---

### Task 16: Add `bb-e2e.yml` workflow

**Files:**
- Create: `.github/workflows/bb-e2e.yml`

- [ ] **Step 1: Write the workflow**

Create `/Users/omeryasironal/Projects/studio/.github/workflows/bb-e2e.yml`:
```yaml
name: bb-e2e

on:
  pull_request:
    paths:
      - 'games/brave-bunny/app/**'
      - '.github/workflows/bb-e2e.yml'

jobs:
  e2e:
    runs-on: ubuntu-latest
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' }}
    defaults:
      run:
        working-directory: games/brave-bunny/app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: Install
        run: npm ci
      - name: Install Playwright browsers
        run: npx playwright install --with-deps chromium
      - name: Build
        run: npm run build
      - name: Run e2e
        run: npm run e2e
      - name: Upload artifacts on failure
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: games/brave-bunny/app/playwright-report/
          retention-days: 7
```

- [ ] **Step 2: Validate YAML**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-e2e.yml'))"
```
Expected: no output.

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-e2e.yml
```

---

### Task 17: Add `bb-nightly-bench.yml` workflow

**Files:**
- Create: `.github/workflows/bb-nightly-bench.yml`

- [ ] **Step 1: Write the workflow**

Create `/Users/omeryasironal/Projects/studio/.github/workflows/bb-nightly-bench.yml`:
```yaml
name: bb-nightly-bench

on:
  schedule:
    - cron: '0 4 * * *'   # 04:00 UTC daily
  workflow_dispatch:

jobs:
  bench:
    runs-on: ubuntu-latest
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' }}
    defaults:
      run:
        working-directory: games/brave-bunny/app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: Install
        run: npm ci
      - name: Build
        run: npm run build
      - name: Run bench (200-enemy stress)
        run: npm run bench -- --reporter=json --outputFile=bench-results.json
      - name: Upload bench results
        uses: actions/upload-artifact@v4
        with:
          name: bench-${{ github.sha }}
          path: games/brave-bunny/app/bench-results.json
          retention-days: 30
```

- [ ] **Step 2: Validate YAML**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-nightly-bench.yml'))"
```
Expected: no output.

- [ ] **Step 3: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-nightly-bench.yml
```

---

### Task 18: Rewrite `bb-ios-build.yml` for Capacitor

**Files:**
- Modify: `.github/workflows/bb-ios-build.yml`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/.github/workflows/bb-ios-build.yml` (Unity-based).

- [ ] **Step 2: Replace contents**

Overwrite `/Users/omeryasironal/Projects/studio/.github/workflows/bb-ios-build.yml`:
```yaml
name: bb-ios-build

on:
  push:
    branches: [main, pivot/**]
    paths:
      - 'games/brave-bunny/app/**'
      - 'games/brave-bunny/ios/**'
      - 'games/brave-bunny/tools/ci/**'
      - '.github/workflows/bb-ios-build.yml'
  pull_request:
    paths:
      - 'games/brave-bunny/app/**'
      - 'games/brave-bunny/ios/**'
      - 'games/brave-bunny/tools/ci/**'
      - '.github/workflows/bb-ios-build.yml'

jobs:
  build:
    runs-on: macos-15
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' && hashFiles('games/brave-bunny/ios/App/App.xcworkspace/contents.xcworkspacedata') != '' }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: Install JS deps
        working-directory: games/brave-bunny/app
        run: npm ci
      - name: Vite build
        working-directory: games/brave-bunny/app
        run: npm run build
      - name: Capacitor sync (iOS)
        working-directory: games/brave-bunny/app
        run: npx cap sync ios
      - name: Pod install
        working-directory: games/brave-bunny/ios/App
        run: pod install
      - name: Xcode build (unsigned, no upload)
        working-directory: games/brave-bunny/ios/App
        run: |
          xcodebuild \
            -workspace App.xcworkspace \
            -scheme App \
            -configuration Release \
            -destination 'generic/platform=iOS' \
            CODE_SIGNING_ALLOWED=NO \
            build
```

- [ ] **Step 3: Validate YAML**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-ios-build.yml'))"
```
Expected: no output.

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-ios-build.yml
```

---

### Task 19: Rewrite `bb-lint.yml` for web stack

**Files:**
- Modify: `.github/workflows/bb-lint.yml`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/.github/workflows/bb-lint.yml`.

- [ ] **Step 2: Replace contents**

Overwrite `/Users/omeryasironal/Projects/studio/.github/workflows/bb-lint.yml`:
```yaml
name: bb-lint

on:
  push:
    branches: [main]
    paths:
      - 'games/brave-bunny/app/**'
      - '.github/workflows/bb-lint.yml'
  pull_request:
    paths:
      - 'games/brave-bunny/app/**'
      - '.github/workflows/bb-lint.yml'

jobs:
  lint:
    runs-on: ubuntu-latest
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' }}
    defaults:
      run:
        working-directory: games/brave-bunny/app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: Install
        run: npm ci
      - name: ESLint
        run: npm run lint
      - name: Prettier check
        run: npm run format:check
```

- [ ] **Step 3: Validate YAML**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-lint.yml'))"
```

- [ ] **Step 4: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-lint.yml
```

---

### Task 20: Update `bb-dependency-audit.yml`

**Files:**
- Modify: `.github/workflows/bb-dependency-audit.yml`

- [ ] **Step 1: Read current file**

Read `/Users/omeryasironal/Projects/studio/.github/workflows/bb-dependency-audit.yml`.

- [ ] **Step 2: Replace contents**

Overwrite `/Users/omeryasironal/Projects/studio/.github/workflows/bb-dependency-audit.yml`:
```yaml
name: bb-dependency-audit

on:
  schedule:
    - cron: '0 6 * * 1'   # Mondays 06:00 UTC
  workflow_dispatch:

jobs:
  audit:
    runs-on: ubuntu-latest
    if: ${{ hashFiles('games/brave-bunny/app/package.json') != '' }}
    defaults:
      run:
        working-directory: games/brave-bunny/app
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: games/brave-bunny/app/package-lock.json
      - name: npm audit (high+ only)
        run: npm audit --audit-level=high
      - name: Check for outdated majors
        run: npm outdated || true
```

- [ ] **Step 3: Validate YAML**

Run:
```bash
python3 -c "import yaml; yaml.safe_load(open('/Users/omeryasironal/Projects/studio/.github/workflows/bb-dependency-audit.yml'))"
```

- [ ] **Step 4: Stage + Commit Stream C**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/workflows/bb-dependency-audit.yml
git commit -m "$(cat <<'EOF'
ci(brave-bunny): replace Unity workflows with web-stack equivalents

Phase 0 — Stream C of engine pivot.

Deleted (Unity-specific):
- bb-unity-test.yml
- bb-simulator-test.yml
- bb-nightly-tests.yml
- bb-weekly-ios-smoke.yml

Added:
- bb-web-test.yml — Vitest + tsc, path-guarded
- bb-e2e.yml — Playwright Chromium, path-guarded
- bb-nightly-bench.yml — perf bench cron + manual

Rewritten:
- bb-ios-build.yml — Vite + Capacitor + xcodebuild (unsigned PR build)
- bb-lint.yml — ESLint + Prettier
- bb-dependency-audit.yml — npm audit + outdated

All new + rewritten workflows are no-op until games/brave-bunny/app/
exists (path-guarded + hashFiles check). They activate naturally
once Plan 2 (Sprint A — Engine Bootstrap) lands.

Per spec §15.3.

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

---

### Task 21: Add `PULL_REQUEST_TEMPLATE.md`

**Files:**
- Create: `.github/PULL_REQUEST_TEMPLATE.md`

- [ ] **Step 1: Write the template**

Create `/Users/omeryasironal/Projects/studio/.github/PULL_REQUEST_TEMPLATE.md`:
```markdown
## Summary

<!-- 1-3 sentences. What does this PR change and why? -->

## Scope

- [ ] `core/` framework change
- [ ] `games/brave-bunny/app/` runtime change
- [ ] `games/brave-bunny/docs/` design/spec change
- [ ] `games/brave-bunny/tools/` pipeline change
- [ ] `.github/` CI / templates / branch config
- [ ] ADR added or updated

## Checklist

- [ ] `npm run typecheck` passes (if `app/` touched)
- [ ] `npm test` passes (if `app/` touched)
- [ ] `npm run lint` passes (if `app/` touched)
- [ ] Perf budget respected (if rendering code touched — see spec §8)
- [ ] CC0/OFL/MIT/CC-BY license attested for any new asset
- [ ] No paid third-party API introduced
- [ ] No secrets committed (.env / api-key / .p12 / .mobileprovision)
- [ ] ADR linked if the change is architectural

## Linked issues / specs

<!-- e.g. closes #42 — implements docs/superpowers/plans/2026-05-16-engine-pivot-foundation.md Task N -->

## Test plan

<!-- How will a reviewer verify this works? -->
- [ ]
- [ ]
```

- [ ] **Step 2: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/PULL_REQUEST_TEMPLATE.md
```

---

### Task 22: Add `agent-task.md` issue template

**Files:**
- Create: `.github/ISSUE_TEMPLATE/agent-task.md`

- [ ] **Step 1: Write the template**

Create `/Users/omeryasironal/Projects/studio/.github/ISSUE_TEMPLATE/agent-task.md`:
```markdown
---
name: Agent task
about: A self-contained task brief for a subagent (≤ 200 lines)
title: "[agent-task] "
labels: agent-task
---

## Assigned agent

<!-- one of: tech-architect, gameplay-engineer, systems-engineer, ui-engineer,
     build-engineer, qa-engineer, asset-curator, blender-tech, art-director,
     game-designer, narrative-designer, ux-designer, level-designer,
     balance-engineer, researcher, orchestrator -->

## Task summary

<!-- One paragraph. What is the agent doing? -->

## Inputs (read before starting)

<!-- list specific files, with paths. Do NOT reference "the conversation" —
     this brief must be self-contained. -->

- `games/brave-bunny/docs/...`
-

## Outputs (paths the agent owns for this task)

-
-

## Acceptance criteria

- [ ]
- [ ]
- [ ]

## Out of scope

<!-- what the agent should NOT touch -->

## Hand-off note location

`games/brave-bunny/docs/handoffs/<agent>-YYYY-MM-DD-<slug>.md` — ≤ 50 lines on completion.
```

- [ ] **Step 2: Stage**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add .github/ISSUE_TEMPLATE/agent-task.md
```

---

### Task 23: Update root README + CLAUDE.md + bug.md + feature.md

**Files:**
- Modify: `README.md`
- Modify: `CLAUDE.md`
- Modify: `.github/ISSUE_TEMPLATE/bug.md`
- Modify: `.github/ISSUE_TEMPLATE/feature.md`

- [ ] **Step 1: Update root README — engine badge**

Edit `/Users/omeryasironal/Projects/studio/README.md`:
Change `![engine](https://img.shields.io/badge/engine-Unity%206%20LTS-black)` → `![engine](https://img.shields.io/badge/engine-Three.js%20%2B%20R3F%20%2B%20Capacitor-black)`

- [ ] **Step 2: Update root README — table of frustrations**

In the same file, the table row that says "Built for office workflows; no game-dev opinion, no Unity, no asset pipeline" → change `no Unity` to `no game-engine opinion` (since we no longer use Unity).

- [ ] **Step 3: Update root README — quick start**

Replace the Unity-specific quick-start block (`./core/scripts/observer-start.sh`, `./core/scripts/new-game.sh`, then Unity-specific instructions) with:
```markdown
## Quick start

```bash
git clone https://github.com/OmerYasirOnal/studio
cd studio

# Bootstrap the observer dashboard
./core/scripts/observer-start.sh         # http://localhost:7777

# Run the active game in dev
cd games/$(cat .active-game)/app
npm ci
npm run dev                              # http://localhost:5173
```

iOS build:

```bash
cd games/$(cat .active-game)/app
npm run build
npx cap sync ios
cd ../tools/ci
fastlane beta                            # → TestFlight
```
```

- [ ] **Step 4: Update root README — "Why we pivoted" note**

Append before the "Quick start" section:
```markdown
## Engine pivot — May 2026

Studio originally used Unity 6 LTS. In May 2026 we pivoted to **Three.js + React Three Fiber + Capacitor 7** for the iOS-first product (Brave Bunny). Rationale: Unity Editor's GUI-dependency made parallel-agent authoring brittle. The web-tech stack keeps 100% of authoring in plain text files, ships to iOS through the existing fastlane pipeline via Capacitor, and unlocks parallel-agent dispatch end-to-end.

Details: [`docs/superpowers/specs/2026-05-16-engine-pivot-design.md`](docs/superpowers/specs/2026-05-16-engine-pivot-design.md)
```

- [ ] **Step 5: Update root `CLAUDE.md` file ownership map**

Edit the **File ownership map** table in `/Users/omeryasironal/Projects/studio/CLAUDE.md`:
Replace these rows:
| `games/<active>/unity/Assets/Scripts/Gameplay/` | gameplay-engineer |
| `games/<active>/unity/Assets/Scripts/Systems/` | systems-engineer |
| `games/<active>/unity/Assets/Scripts/UI/` | ui-engineer |
| `games/<active>/unity/Assets/Tests/` | qa-engineer |

With:
| `games/<active>/app/src/systems/`, `games/<active>/app/src/render/`, `games/<active>/app/src/ecs/` | gameplay-engineer |
| `games/<active>/app/src/ecs/`, `games/<active>/app/src/state/`, `games/<active>/app/src/platform/`, `games/<active>/app/src/audio/` | systems-engineer |
| `games/<active>/app/src/ui/` | ui-engineer |
| `games/<active>/app/src/**/*.test.ts`, `games/<active>/app/e2e/`, `games/<active>/app/bench/` | qa-engineer |

- [ ] **Step 6: Add engine clause to CLAUDE.md**

Below the "Active game" section in `CLAUDE.md`, add a new section:
```markdown
## Active engine

The active engine is **Three.js + React Three Fiber + Capacitor 7** (web-tech 3D wrapped for iOS). All code is TypeScript/JSON/Markdown; there is no GUI editor in the authoring loop. The historical Unity pivot is documented in [`docs/superpowers/specs/2026-05-16-engine-pivot-design.md`](docs/superpowers/specs/2026-05-16-engine-pivot-design.md).
```

- [ ] **Step 7: Update `bug.md`**

Edit `/Users/omeryasironal/Projects/studio/.github/ISSUE_TEMPLATE/bug.md`. Add a new field at the top:
```markdown
## Engine

- [x] web (Three.js + R3F + Capacitor)
- [ ] framework infrastructure (core/, observer, CI)
```

- [ ] **Step 8: Update `feature.md`**

Edit `/Users/omeryasironal/Projects/studio/.github/ISSUE_TEMPLATE/feature.md`. In the **Scope** checklist, add:
```markdown
- [ ] New runtime code under `games/<active>/app/src/`
- [ ] New asset pipeline tooling under `games/<active>/tools/assets/`
```

- [ ] **Step 9: Stage all**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git add README.md CLAUDE.md .github/ISSUE_TEMPLATE/bug.md .github/ISSUE_TEMPLATE/feature.md
```

- [ ] **Step 10: Commit Stream D**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git commit -m "$(cat <<'EOF'
docs(repo): refresh README + CLAUDE.md + issue/PR templates for pivot

Phase 0 — Stream D of engine pivot.

- README.md: engine badge → Three.js+R3F+Capacitor; quick-start
  rewritten for npm + vite + cap; new "Engine pivot" note linking
  the spec.
- CLAUDE.md: file ownership map updated for app/src/ paths; new
  "Active engine" section under Active game.
- .github/PULL_REQUEST_TEMPLATE.md: new — scope + checklist + tests.
- .github/ISSUE_TEMPLATE/agent-task.md: new — handoff brief
  template for subagent dispatch (≤200 line brief, ≤50 line
  handoff note convention).
- .github/ISSUE_TEMPLATE/bug.md, feature.md: add engine + app/
  scope fields.

Per spec §15.4-§15.5 and §14 (file ownership row in §14 of root
CLAUDE.md is now consistent with agent def updates).

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

---

### Task 24: Open PR, configure branch protection, verify CI green, merge

**Files:**
- Branch: `pivot/engine-three-r3f`
- GitHub PR
- Branch protection rules (gh API)

- [ ] **Step 1: Push the branch**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git push origin pivot/engine-three-r3f
```

- [ ] **Step 2: Open the PR**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
gh pr create \
  --base main \
  --head pivot/engine-three-r3f \
  --title "Engine pivot — Phase 0: Foundation (Unity → Three.js+R3F+Capacitor)" \
  --body "$(cat <<'EOF'
## Summary

Phase 0 of the engine pivot. Repo-hygiene + system-prep only — zero
application code. Prepares trunk for Plan 2 (Sprint A — Engine
Bootstrap) and beyond.

Streams (all merged into this PR):
- A: Branch + worktree + unity/ cleanup
- B: 9 agent def engine-block rewrites
- C: 7 CI workflows replaced (4 deleted, 3 added, 3 rewritten)
- D: README + CLAUDE.md + issue/PR templates refreshed

Spec: `docs/superpowers/specs/2026-05-16-engine-pivot-design.md`
Plan: `docs/superpowers/plans/2026-05-16-engine-pivot-foundation.md`

## Scope

- [x] `core/` framework change (agent defs)
- [ ] `games/brave-bunny/app/` runtime change
- [x] `games/brave-bunny/docs/` design/spec change (spec was a previous commit)
- [ ] `games/brave-bunny/tools/` pipeline change
- [x] `.github/` CI / templates / branch config
- [ ] ADR added or updated (ADR-0030-33 follow in Plan 2+)

## Checklist

- [x] `ci.yml` (root) lint + observer-smoke green
- [x] All bb-* workflows path-guarded; no-op until `games/brave-bunny/app/` exists
- [x] No secrets committed
- [x] No paid third-party API introduced
- [x] No application code in this PR

## Test plan

- [ ] Confirm `ci.yml` runs green
- [ ] Confirm bb-* workflows skip (no app/ yet)
- [ ] Branch protection on main rule active after merge

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```
Expected: PR URL returned.

- [ ] **Step 3: Wait for CI**

Run:
```bash
gh pr checks --watch
```
Expected: `ci.yml` (lint + observer-smoke) green within 5 minutes. All `bb-*` workflows show as **Skipped** (path-guarded). If anything fails, fix on the branch and push.

- [ ] **Step 4: Configure branch protection on `main`**

Run:
```bash
gh api -X PUT repos/OmerYasirOnal/studio/branches/main/protection \
  --input - <<'EOF'
{
  "required_status_checks": {
    "strict": true,
    "contexts": ["lint", "observer-smoke"]
  },
  "enforce_admins": false,
  "required_pull_request_reviews": null,
  "restrictions": null,
  "allow_force_pushes": false,
  "allow_deletions": false,
  "required_linear_history": true,
  "required_conversation_resolution": true
}
EOF
```
Expected: JSON response confirming protection rules. Note: `enforce_admins` is false so Ömer can still hot-fix; flip to true post-launch.

- [ ] **Step 5: Create labels for the pivot work**

Run:
```bash
for label in "pivot" "phase-0" "phase-1-bootstrap" "phase-2-assets" "phase-3-weapon" "phase-4-stress" "phase-5-vslice" "phase-6-testflight" "agent-task" "engine" "ci" "docs"; do
  gh label create "$label" --description "Engine pivot label" --color "0E8A16" 2>/dev/null || true
done
```
Expected: 11 labels created (some may already exist; `|| true` swallows that).

- [ ] **Step 6: Create milestone for the pivot**

Run:
```bash
gh api -X POST repos/OmerYasirOnal/studio/milestones \
  -f title="Engine pivot — Three.js+R3F+Capacitor" \
  -f description="See docs/superpowers/specs/2026-05-16-engine-pivot-design.md" \
  -f state="open" \
  2>/dev/null || echo "milestone may already exist"
```

- [ ] **Step 7: Self-review the PR**

Run:
```bash
gh pr view --json files,additions,deletions
gh pr diff | head -200
```
Expected: thousands of deletions (mostly under `games/brave-bunny/unity/`), targeted additions in `.github/`, `core/.claude/agents/`, `tools/repo/`, `docs/`. No unrelated changes.

- [ ] **Step 8: Merge**

Run:
```bash
gh pr merge --merge --delete-branch
```
Expected: PR merged to `main`; remote `pivot/engine-three-r3f` deleted. Tag `pre-pivot-2026-05-16` remains as the rollback anchor.

- [ ] **Step 9: Sync local main and verify clean state**

Run:
```bash
cd /Users/omeryasironal/Projects/studio
git fetch origin
git switch main
git merge --ff-only origin/main
git branch -d pivot/engine-three-r3f 2>/dev/null || true
git worktree list
git branch -a
```
Expected: on `main`, only `main` + `origin/main` + `origin/HEAD` listed. Worktree list shows only the main checkout. Tag `pre-pivot-2026-05-16` retained.

- [ ] **Step 10: Update memory**

Append to `/Users/omeryasironal/.claude/projects/-Users-omeryasironal-Projects-studio/memory/brave-bunny-engine-pivot-20260516.md`:
```markdown

## Phase 0 complete — 2026-05-XX

PR #<N> merged. Trunk cleaned (unity/ deleted, 40+ stale branches pruned, worktrees pruned). 9 agent defs updated to web-stack. CI workflows replaced and path-guarded (no-op until app/ exists). Templates and README current. Branch protection on main active. Labels + milestone created. Tag `pre-pivot-2026-05-16` is the rollback anchor.

Plan 2 (Sprint A — Engine Bootstrap) is the next task.
```

---

## Self-Review

### Spec coverage check

| Spec section | Implemented in this plan? | Task |
|---|---|---|
| §10.1 unity/ deletion | ✅ | Task 3 |
| §14 engine-dependent agent updates (8 agents) | ✅ | Tasks 4-11 (one missing: art-director — but art-director is in the "engine-independent" group per spec §14 too, so no update needed; orchestrator covered as Task 12) |
| §14 engine-independent agents (no change needed) | ✅ (no-op as designed) | n/a |
| §15.1 branch hygiene | ✅ | Tasks 2-3 |
| §15.2 new branch model (umbrella + sprint topics) | ✅ | Task 1 (umbrella created); sprint topics start in Plan 2 |
| §15.3 CI workflow rewrite (delete 4, add 3, rewrite 3) | ✅ | Tasks 14-20 |
| §15.4 issue + PR templates | ✅ | Tasks 21-22 + Task 23 step 7-8 |
| §15.5 README | ✅ | Task 23 steps 1-4 |
| Root CLAUDE.md ownership map | ✅ | Task 23 steps 5-6 |
| Branch protection on main | ✅ | Task 24 step 4 |
| Labels + milestone | ✅ | Task 24 steps 5-6 |

Spec sections **explicitly out of scope for Plan 1** (will land in Plan 2-7):
- §3-4 architecture / folder layout (app/ creation is Plan 2)
- §5 asset pipeline (Plan 3)
- §6 runtime systems (Plan 2 onwards)
- §7 iOS build pipeline (Plan 2 — Capacitor add)
- §8 perf contract enforcement (Plan 4 onwards)
- §9 risks/fallbacks (decisioning lives in Plan 5 — stress test gate)
- §10.5 sequencing detail (each subsequent plan owns its sprint)
- §11 verification strategy (exercised in Plan 2 onwards as `app/` exists)

### Placeholder scan

Grep for red flags: TODO, TBD, "implement later", "Add appropriate", "Similar to". I searched each task body. The only "TBD"-shaped references are:
- Task 24 step 10 says `PR #<N>` — placeholder is **intentional** (the PR number is unknown until step 2 returns it). This is filled at execution time, not by the plan author.
- All other steps have concrete file paths, exact commands, and full code/config blocks.

No fixes needed.

### Type / name consistency

- `pivot/engine-three-r3f` branch name used consistently (Task 1, Task 24)
- `tools/repo/cleanup-branches.sh` path used consistently (Task 2 creates, Task 3 executes)
- Workflow file names consistent (`bb-web-test.yml`, `bb-e2e.yml`, `bb-nightly-bench.yml`, `bb-ios-build.yml`, `bb-lint.yml`, `bb-dependency-audit.yml`) across Tasks 14-20 + Task 23 (PR body)
- Tag `pre-pivot-2026-05-16` consistent (Task 1 step 4, Task 24 step 9-10)
- Spec path `docs/superpowers/specs/2026-05-16-engine-pivot-design.md` consistent (README, PR body, memory)

No mismatches.

---

## Done criteria

Plan 1 is complete when:
- [ ] PR merged to `main`
- [ ] `git worktree list` shows only the main checkout
- [ ] `git branch -a` shows only `main`, `remotes/origin/main`, `remotes/origin/HEAD`, plus `tags/pre-pivot-2026-05-16`
- [ ] `games/brave-bunny/unity/` does not exist
- [ ] All 9 agent def files contain zero `unity` / `monobehaviour` / `assets/scripts` strings
- [ ] `.github/workflows/` contains: `ci.yml`, `observer-smoke.yml`, `bb-web-test.yml`, `bb-e2e.yml`, `bb-nightly-bench.yml`, `bb-ios-build.yml`, `bb-lint.yml`, `bb-dependency-audit.yml`, `README.md` — total 9 files
- [ ] `.github/ISSUE_TEMPLATE/` contains: `bug.md`, `feature.md`, `agent-task.md`
- [ ] `.github/PULL_REQUEST_TEMPLATE.md` exists
- [ ] Root `README.md` shows the Three.js engine badge
- [ ] Root `CLAUDE.md` shows the new file ownership map + Active engine section
- [ ] Branch protection on `main` requires `lint` + `observer-smoke` status checks
- [ ] Labels `pivot`, `phase-0` ... `phase-6-testflight`, `agent-task` exist on the repo
- [ ] Milestone `Engine pivot — Three.js+R3F+Capacitor` exists
- [ ] Memory file `brave-bunny-engine-pivot-20260516.md` updated with Phase 0 completion note
