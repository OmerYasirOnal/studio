# Agent system

Studio uses **16 specialist Claude Code agents** that run as independent tmux sessions. Each agent has a focused role, a clear input/output contract, and a hand-off note format. They never read each other's chat history — only files and the `memory` MCP.

## Spawning

```bash
./core/scripts/spawn-agent.sh <agent-name> "<task description>"
```

This:

1. Reads `.active-game`
2. Looks for `games/<active>/.claude/agents/<name>.md` (game-specific override) or falls back to `core/.claude/agents/<name>.md`
3. Replaces `<active>` placeholders with the real game directory
4. Opens a new tmux session named `studio-<game>-<agent>`
5. Starts Claude Code with `--append-system-prompt` pointing at the materialized agent prompt
6. Pipes the session output to `games/<active>/logs/<agent>/<timestamp>.log`
7. Emits a `status: spawning` and `status: working` JSONL event to `logs/agent-status.jsonl`

## Agent template

Every agent definition lives at `core/.claude/agents/<name>.md` and follows the same outline:

```markdown
---
name: <agent>
description: <one-line summary used when the orchestrator picks an agent>
model: opus | sonnet | haiku
---

# <Agent name>

## Inputs
<files / MCPs the agent reads>

## Outputs
<exact paths the agent is allowed to write>

## RALPH loop
1. Discovery
2. Planning
3. Implementation
4. Polish

## Self-review checklist
- [ ] ...

## Logging
<JSONL schema>

## Hand-off note template
<≤50 lines>

## Forbidden
<things this agent must not do>
```

## RALPH

Every agent runs Discovery → Planning → Implementation → Polish. The orchestrator does this *at phase boundaries*; specialists do this *within each session*.

| Step | Job |
|---|---|
| Discovery | Read inputs. Read last 3 handoffs. List unmet exit criteria. |
| Planning | Pick the next deliverable. Sketch it. |
| Implementation | Write outputs to your owned paths. |
| Polish | Run self-review. Append status. Write hand-off note. |

## Hand-off notes

At the end of every session, each agent writes `games/<active>/docs/handoffs/<agent>-<timestamp>.md`. The hand-off is the **only** signal the next agent has. Keep it ≤ 50 lines. Always include:

- What was produced
- What's blocked
- What the next agent should read first
- Key facts to record in the `memory` MCP

## Game-specific overrides

If a specific game needs to tweak an agent's behavior, drop an override at `games/<game>/.claude/agents/<name>.md`. `spawn-agent.sh` prefers it over the core version. Use sparingly — overrides are technical debt against framework reusability.

## Adding a new agent

1. Write `core/.claude/agents/<name>.md` following the template above
2. Add the agent to the roster table in `README.md`
3. Add the agent to the validation list in `core/.claude/commands/spawn.md`
4. Commit with `feat(agent): add <name>`
5. Document any new file-ownership rows in the root `CLAUDE.md`

## Token budget

The whole point of separate tmux sessions is **token efficiency**:

- Each agent's context window is just its system prompt + the files it reads
- Agents never re-load the GDD — they read summaries
- `memory` MCP holds short facts (character names, weapon names, biome ids) for cheap cross-agent lookup
- Hand-off notes are ≤ 50 lines — this is the bottleneck, optimize ruthlessly
