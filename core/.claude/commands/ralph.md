---
description: Run the RALPH (Discovery → Planning → Implementation → Polish) loop for an agent on the active game.
argument-hint: <agent-name>
---

# /ralph

Run the RALPH loop wrapper:

```bash
./core/scripts/ralph.sh "$ARGUMENTS"
```

The `ralph.sh` script reads `.active-game`, resolves the agent definition, opens a new tmux session with the RALPH loop hint pre-loaded, and posts the appropriate first-action prompt for that agent.

Use this when you want a stronger structured cycle than `/spawn` (which uses the agent's own discretion on RALPH boundaries).
