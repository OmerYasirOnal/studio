---
description: Show the active game's current phase, exit criteria progress, and active agent sessions.
---

# /phase-status

Read `.active-game` to identify the active game folder. Then read:

1. `games/<active>/docs/11-roadmap/current-phase.md` (or `games/<active>/docs/11-roadmap/phase-<N>-status.md` it points at)
2. The most recent 5 files in `games/<active>/docs/handoffs/`
3. The tail of `logs/agent-status.jsonl` filtered to the active game

Report:

- **Active game:** name
- **Current phase:** number + name
- **Exit criteria:** checked / unchecked count and the next 3 unmet items
- **Active agent sessions:** list from `tmux ls 2>/dev/null | grep "^studio-<active>-"`
- **Recent handoffs (last 5):** agent + filename + first line of summary
- **Blockers:** any `status:blocked` entries in last 50 lines of `agent-status.jsonl`
- **Next recommended action:** one sentence

Format the output as compact terminal-friendly text (no Markdown headers in the response).

If `.active-game` does not exist, print: `No active game. Run ./core/scripts/new-game.sh <name>` and stop.
