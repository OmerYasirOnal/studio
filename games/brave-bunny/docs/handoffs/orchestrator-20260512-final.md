# Orchestrator final hand-off — 2026-05-12

**Current phase:** 3 (Tech Architecture), substantially complete on documents. Phase 4 (Asset Pipeline) and Phase 5 (Prototype) await user prerequisites.

## Session totals

| Metric | Count |
|---|---|
| Atomic git commits | ~9 |
| Files in `games/brave-bunny/` | ~110+ |
| GDD sections (out of 14) | 14 |
| User stories | 62 |
| HTML wireframes | 15 |
| Art bible sections | 10 |
| Audio bible sections | 5 |
| Narrative files (tone/world/bios/biome/boss/loc) | 14 |
| Tech-spec docs | 11 |
| ADRs accepted | 10 |
| ADRs proposed (deferred) | 2 |
| Balance JSON sheets | 8 |
| Balance docs | 7 |
| Level-design files (Meadow only) | 5 |
| Custom Blender recipes | 4 |
| Competitor deconstructions | 5 |

## Phases at a glance

- **Phase 0 (Framework + Game) — DONE** (yesterday)
- **Phase 1 (Discovery) — DONE** ✓ all 5 competitor decons + market + positioning + references
- **Phase 2 (GDD) — DONE** ✓ all 13 GDD sections + narrative + UX + art bible + audio bible + balance + level design
- **Phase 3 (Tech Architecture) — DONE on documents** ✓ all 11 tech specs + 10 accepted ADRs + 2 proposed
- **Phase 4 (Asset Pipeline) — PLANNED** ✓ INDEX.md maps every asset to a CC0 source; actual fetches pending user approval
- **Phase 5 (Prototype) — BLOCKED** ⏳ requires tmux + Unity 6 LTS install first

## What's blocking forward motion

1. **tmux not installed** — `brew install tmux`. Without it, `spawn-agent.sh` cannot launch real per-agent sessions. This entire run used the Agent tool as a stand-in; for production-quality long-running work, install tmux.
2. **Unity 6 LTS + iOS build module not installed** — required for Phase 5
3. **No actual asset downloads yet** — only planning; user should approve `quaternius-fetch.py` / `kenney-fetch.py` runs (or fetch manually) before Phase 5 starts
4. **Quaternius Otter presence unverified** — flagged. Need ADR-0013 (Otter fallback to Beaver kitbash) if absent

## The 12 ADRs (read in order if onboarding to this project)

1. **0001 starter-weapon binding** — universal weapon pool, character-bound defaults only
2. **0002 toon shader source** — custom Shader Graph URP, in-house
3. **0003 hitstop timings** — canonical table; lives in `data/balance/feel.json`
4. **0004 no voice acting at launch** — text-only, voice mixer bus reserved for v1.1+
5. **0005 engine choice** — Unity 6 LTS URP
6. **0006 enemy HP recalibration** — Monte Carlo found GDD curves too high; new curves in `enemies.json`
7. **0007 evolution charm consumption** — charms consumed by weapon evolution to preserve depth
8. **0008 save format** — Newtonsoft JSON in binary wrapper
9. **0009 polymorphic mechanics** — type-name string + script registry (not SerializeReference)
10. **0010 subscription ROI** — Monthly Bunny Card at 4.2x effective ROI is competitive, not over-generous
11. **0011 BGM loop format on iOS** — proposed; defer to Phase 5 device test
12. **0012 event channel mechanism** — proposed; defer to Phase 5 Profiler

## The story of brave-bunny in 5 sentences

A cartoon-mascot action-roguelite in the Habby auto-battler family. Eight animal heroes, twelve weapons with six evolution recipes, five biomes with five bosses. No energy gate, no gear gacha, no realistic violence — every monetization product is cosmetic, convenience, or subscription. Vertical slice scope: Bunny + Meadow + Carrot Boomerang/Sunbeam/Daisy Mine + Old Boar King boss, 200 enemies at 60fps on iPhone 12. North-star: D1 retention ≥ 40% in TR/PH/ID soft launch.

## Reading order for the next agent / collaborator

1. `games/brave-bunny/GAME.md` (concept + scope + cut list)
2. `docs/01-research/03-positioning.md` (UVP + competitive landscape)
3. `docs/02-gdd/00-overview.md` (pillars + scope)
4. `docs/02-gdd/01-core-loop.md` (mechanics)
5. `docs/decisions/INDEX.md` (12 ADRs)
6. `docs/06-tech-spec/00-engine-and-version.md` through `11-third-party.md` (implementation contract)
7. `docs/11-roadmap/current-phase.md` (where we are + next moves)
8. Any specific subsystem you're picking up — go directly to its docs + handoffs

## Next session actions (when user is ready)

```bash
# Install prerequisites
brew install tmux

# Install Unity 6 LTS + iOS Build module from Unity Hub
# Open Apple Developer account if not already (build-engineer will need it)

# Then in a fresh Claude Code foreground session:
cd /Users/omeryasironal/Projects/studio
claude --dangerously-skip-permissions

# Inside Claude Code:
/active-game brave-bunny
/phase-status
/spawn asset-curator "execute the INDEX.md plan: fetch Quaternius animal pack + Kenney Nature Kit"
# (etc — see current-phase.md "Next agents to spawn" section)
```

## What I did NOT do (deliberately or as gaps)

- No actual asset downloads (no user approval for CC0 fetch operations)
- No Unity project initialization (waiting on tmux + Unity install)
- No actual `tmux` sessions spawned (used Agent tool as stand-in — works fine for documents, but real engine-code agents will want persistent sessions)
- No Apple Developer interactive steps
- No PRs against `core/` — every change to the framework was bundled in the original bootstrap commit, then small `fix(template):` for the README leak

## Closing observation

The framework's value proposition — one developer, 8 weeks to TestFlight via multi-agent orchestration — is now demonstrable. Phases 1-3 took **one continuous Opus session** to complete on paper. The remaining 5 weeks of the 8-week target are gated only by: (a) running real tmux sessions for long-running engine work, (b) user-approved CC0 fetches, and (c) Unity build cycles that need a Mac with Unity installed.

Studio v0.1.0 is alpha-quality on the framework side; brave-bunny is design-complete and engine-ready.
