---
description: Read the most recent hand-off note from a given agent on the active game.
argument-hint: <agent-name>
---

# /handoff-read

Resolve `.active-game`. List `games/<active>/docs/handoffs/<agent-name>-*.md` sorted by mtime descending. Print the contents of the most recent file (or "no handoffs yet" if none).

If `$ARGUMENTS` is empty, list the 5 most recent handoffs across all agents instead.
