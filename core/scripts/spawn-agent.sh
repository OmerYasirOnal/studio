#!/usr/bin/env bash
# spawn-agent.sh <agent-name> "<task description>"
#
# Opens a clean tmux session running Claude Code with the agent's definition
# as its system prompt. Per-agent logs land in games/<active>/logs/<agent>/.
# Status events go to logs/agent-status.jsonl which the observer dashboard tails.

set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <agent-name> [task description...]" >&2
  exit 1
fi

AGENT="$1"; shift
TASK="${*:-Start the RALPH loop using the role definition.}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT"

ACTIVE_GAME="$(cat .active-game 2>/dev/null || true)"
if [[ -z "$ACTIVE_GAME" ]]; then
  echo "[spawn] no .active-game — run ./core/scripts/new-game.sh <name> first" >&2
  exit 1
fi

if [[ ! -d "games/$ACTIVE_GAME" ]]; then
  echo "[spawn] .active-game points to games/$ACTIVE_GAME which does not exist" >&2
  exit 1
fi

# Find the agent definition: game-specific override first, then core.
AGENT_FILE=""
if [[ -f "games/$ACTIVE_GAME/.claude/agents/$AGENT.md" ]]; then
  AGENT_FILE="games/$ACTIVE_GAME/.claude/agents/$AGENT.md"
elif [[ -f "core/.claude/agents/$AGENT.md" ]]; then
  AGENT_FILE="core/.claude/agents/$AGENT.md"
else
  echo "[spawn] no agent definition for '$AGENT' in core/.claude/agents/ or games/$ACTIVE_GAME/.claude/agents/" >&2
  exit 1
fi

SESSION="studio-${ACTIVE_GAME}-${AGENT}"

if ! command -v tmux >/dev/null 2>&1; then
  echo "[spawn] tmux is not installed — install with: brew install tmux" >&2
  exit 1
fi

if tmux has-session -t "$SESSION" 2>/dev/null; then
  echo "[spawn] session $SESSION is already running"
  echo "[spawn] attach with: tmux attach -t $SESSION"
  exit 0
fi

LOG_DIR="games/$ACTIVE_GAME/logs/$AGENT"
mkdir -p "$LOG_DIR"
TS="$(date +%Y%m%d-%H%M%S)"
LOG_FILE="$LOG_DIR/$TS.log"

# Materialize an agent prompt with <active> placeholders replaced.
TMP_AGENT_FILE="$LOG_DIR/agent-system-prompt-$TS.md"
sed "s|<active>|games/$ACTIVE_GAME|g" "$AGENT_FILE" > "$TMP_AGENT_FILE"

# Status: spawning.
TS_EPOCH="$(date +%s)"
mkdir -p logs
echo "{\"ts\":$TS_EPOCH,\"game\":\"$ACTIVE_GAME\",\"agent\":\"$AGENT\",\"status\":\"spawning\",\"task\":$(printf '%s' "$TASK" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()))'),\"session\":\"$SESSION\"}" \
  >> logs/agent-status.jsonl

# Kick off tmux session. We pipe panes to a file so the observer can tail.
tmux new-session -d -s "$SESSION" -n main
tmux pipe-pane -t "$SESSION" -o "cat >> $LOG_FILE"

# In the session: cd repo root and start Claude Code with the agent prompt.
tmux send-keys -t "$SESSION" "cd $ROOT" C-m
# --append-system-prompt is the Claude Code flag for adding a system prompt from a file.
tmux send-keys -t "$SESSION" "claude --dangerously-skip-permissions --append-system-prompt \"\$(cat $TMP_AGENT_FILE)\"" C-m

# Brief settle delay so Claude is ready to receive the first user turn.
sleep 3
tmux send-keys -t "$SESSION" "$TASK" C-m

# Status: working.
TS_EPOCH2="$(date +%s)"
echo "{\"ts\":$TS_EPOCH2,\"game\":\"$ACTIVE_GAME\",\"agent\":\"$AGENT\",\"status\":\"working\",\"task\":$(printf '%s' "$TASK" | python3 -c 'import json,sys; print(json.dumps(sys.stdin.read()))'),\"session\":\"$SESSION\",\"log\":\"$LOG_FILE\"}" \
  >> logs/agent-status.jsonl

cat <<EOF
[spawn] $AGENT spawned for game=$ACTIVE_GAME
  session: $SESSION    (tmux attach -t $SESSION)
  log:     $LOG_FILE
  dashboard: http://localhost:7777/?game=$ACTIVE_GAME
EOF
