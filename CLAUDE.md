# Studio — Claude Code Operating Rules

> This file is loaded automatically by Claude Code at the repository root.
> Each game under `games/<name>/CLAUDE.md` extends and overrides these rules.

## Mission

Studio is a multi-agent game-development framework. The `core/` directory is the **engine**; the `games/<name>/` directories are **products**. Everything you write must respect this separation.

The framework's job is to let one developer ship a mobile game in 8 weeks using a swarm of Claude Code agents working in parallel, coordinated through the filesystem and a local observer dashboard. No paid APIs. No proprietary services. CC0 assets only.

## Active game

The currently active game is recorded in `.active-game` at the repository root. All agent-spawn helpers and slash commands read this file to know which `games/<name>/` directory to write to. Update it via `/active-game <name>` or `./core/scripts/new-game.sh <name>` — do not edit by hand during a session.

## Active engine

The active engine is **Three.js + React Three Fiber + Capacitor 7** (web-tech 3D wrapped for iOS). All code is TypeScript/JSON/Markdown; there is no GUI editor in the authoring loop. The historical Unity pivot is documented in [`docs/superpowers/specs/2026-05-16-engine-pivot-design.md`](docs/superpowers/specs/2026-05-16-engine-pivot-design.md).

## Nine guiding principles

1. **Spec-first, code-last.** No engine code before the GDD, art bible, and tech spec are written and committed.
2. **Single source of truth = git.** Conventional commits. Atomic changes. No squash merges that lose history.
3. **Documentation as code.** Markdown with Mermaid diagrams. If it isn't in the repo it doesn't exist.
4. **Observability default-on.** Every meaningful action emits a JSONL entry under `logs/` (framework) or `games/<active>/logs/` (game).
5. **RALPH loop.** Every agent works in Discovery → Planning → Implementation → Polish cycles.
6. **No magic numbers.** Balance lives in `data/balance/*.json` + ScriptableObjects, never inlined in scripts.
7. **Auto-format + auto-lint.** Pre-commit hooks under `core/.claude/hooks/` are not optional.
8. **CC0 / OFL / MIT / CC-BY only.** See `core/docs/asset-policy.md`.
9. **Framework vs Game separation is sacred.** If you have to ask, put it in the game folder.

## Zero external paid API

Never add code that calls a paid third-party API. This includes — non-exhaustively — ElevenLabs, Meshy, Tripo, Hunyuan3D, Replicate, Midjourney, OpenAI, Suno, Stable Audio, GPT API, Gemini API. Even free-tier signups are forbidden unless they are a permanent part of the publishing pipeline (App Store Connect, Google Play Console, Unity Hub). `AdMob` and `Unity IAP` are permitted because they are revenue side, not generation side.

If you think a paid API is needed, the answer is: find a CC0 alternative or write a local script. See `core/docs/asset-policy.md` for the full source list.

## Token efficiency discipline

- Every agent runs in a **fresh Claude Code conversation** (its own tmux session via `core/scripts/spawn-agent.sh`).
- Agents communicate **only** through the filesystem and the `memory` MCP server.
- Each agent receives a **minimal handoff brief** (≤ 200 lines) — never paste prior conversation history.
- Each agent emits a **hand-off note** (≤ 50 lines) at task completion under `games/<active>/docs/handoffs/<agent>-<timestamp>.md`.
- The foreground orchestrator stays idle between phase transitions; it does not babysit working agents.

## File ownership map

| Path | Owner agent(s) |
|---|---|
| `core/**` | Framework maintainer only — agents do not write here mid-session |
| `games/<active>/docs/00-vision/` | game-designer |
| `games/<active>/docs/01-research/` | researcher |
| `games/<active>/docs/02-gdd/` | game-designer, narrative-designer |
| `games/<active>/docs/03-user-stories/` | ux-designer |
| `games/<active>/docs/04-ux-flows/` | ux-designer |
| `games/<active>/docs/05-wireframes/` | ux-designer |
| `games/<active>/docs/06-tech-spec/` | tech-architect |
| `games/<active>/docs/07-art-bible/` | art-director |
| `games/<active>/docs/08-audio-bible/` | art-director (audio sub-role) |
| `games/<active>/docs/09-level-design/` | level-designer |
| `games/<active>/docs/10-balance/` | balance-engineer |
| `games/<active>/docs/decisions/` | any agent — ADRs are universal |
| `games/<active>/assets-raw/` | asset-curator, blender-tech |
| `games/<active>/data/balance/` | balance-engineer |
| `games/<active>/app/src/systems/`, `games/<active>/app/src/render/` | gameplay-engineer |
| `games/<active>/app/src/ecs/`, `games/<active>/app/src/state/`, `games/<active>/app/src/platform/`, `games/<active>/app/src/audio/` | systems-engineer |
| `games/<active>/app/src/ui/` | ui-engineer |
| `games/<active>/app/src/**/*.test.ts`, `games/<active>/app/e2e/`, `games/<active>/app/bench/` | qa-engineer |
| `games/<active>/tools/ci/` | build-engineer |

## Forbidden patterns

- **API keys / secret material** — committing one fails the pre-commit hook.
- **Paid third-party services** — see above.
- **Mock-and-forget** — every mock has a paired TODO with a real implementation deadline.
- **Magic numbers** — extract to JSON or ScriptableObject.
- **Cross-game contamination** — `core/` must never reference `brave-bunny` (or any game name) by string.

## Escalation triggers (when to surface to the human)

- Apple Developer interactive UI needed (cert selection, App Store Connect agreement)
- Test suite is broken across the last 5 commits and revert doesn't fix it
- A real blocker has been investigated with 3 different approaches and 2+ hours

Otherwise: write an ADR, decide, proceed.

## Slash commands

Run `/active-game`, `/phase-status`, `/spawn <agent> "<task>"`, `/ralph <agent>`, `/decide <topic>`, `/handoff-read <agent>`, `/observer`, `/new-game <name>`, `/build-ios` from the repo root.
