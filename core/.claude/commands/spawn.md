---
description: Spawn a specialist agent for the active game in a clean tmux session.
argument-hint: <agent-name> "<task description>"
---

# /spawn

Parse `$ARGUMENTS` as `<agent-name> <quoted-task>`. Validate that:

1. `.active-game` exists at repo root
2. An agent definition exists at `games/<active>/.claude/agents/<name>.md` OR `core/.claude/agents/<name>.md`
3. The agent name is one of: orchestrator, researcher, game-designer, narrative-designer, ux-designer, tech-architect, art-director, asset-curator, blender-tech, level-designer, balance-engineer, gameplay-engineer, systems-engineer, ui-engineer, qa-engineer, build-engineer

Then run:

```bash
./core/scripts/spawn-agent.sh <agent-name> "<task description>"
```

Stream the output. Report the tmux session name and a one-line confirmation. Do not wait for the agent to finish — that's the observer's job.

If validation fails, print the specific error and stop.
