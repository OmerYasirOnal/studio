#!/usr/bin/env bash
# ralph.sh <agent> — wrapper around spawn-agent.sh that injects a RALPH-loop prompt.
# RALPH = Discovery → Planning → Implementation → Polish.

set -euo pipefail

AGENT="${1:-}"
if [[ -z "$AGENT" ]]; then
  echo "Usage: $0 <agent-name>" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

read -r -d '' RALPH_TASK <<'EOF' || true
Run a full RALPH loop for this role on the active game.

1. DISCOVERY: read the inputs listed in your role definition. Read the 3 most recent handoffs in <active>/docs/handoffs/. Read the current phase status under <active>/docs/11-roadmap/. List unmet exit criteria for the current phase. DO NOT load full GDDs or specs — read summaries and handoffs only.

2. PLANNING: identify the single highest-value next deliverable in your domain. Sketch it (a section list, a class skeleton, a test list, etc.) in your hand-off note draft.

3. IMPLEMENTATION: produce the deliverable. Write only to paths in your output list. Commit atomically.

4. POLISH: run your self-review checklist. Append to logs/agent-status.jsonl. Write a hand-off note ≤50 lines summarizing what's done, what's blocked, what the next agent needs.

When you're done, stop. Do NOT pick up another deliverable in this session — that's the orchestrator's call.
EOF

exec "$ROOT/core/scripts/spawn-agent.sh" "$AGENT" "$RALPH_TASK"
