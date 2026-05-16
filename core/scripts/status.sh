#!/usr/bin/env bash
# status.sh — quick terminal-friendly report of framework state.

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT" || exit 1

ACTIVE="$(cat .active-game 2>/dev/null || echo "")"

echo "================================================================"
echo " Studio status — $(date '+%Y-%m-%d %H:%M:%S')"
echo "================================================================"
echo " framework version : $(cat core/VERSION 2>/dev/null || echo unknown)"
echo " active game       : ${ACTIVE:-<none — run new-game.sh>}"
echo " games on disk     : $(ls games 2>/dev/null | wc -l | tr -d ' ')"
echo

if command -v tmux >/dev/null 2>&1; then
  echo "tmux sessions (studio-*):"
  tmux ls 2>/dev/null | grep -E "^studio-" || echo "  (none)"
else
  echo "tmux: not installed"
fi
echo

echo "observer:"
if curl -sS -o /dev/null -m 1 http://localhost:7777/health 2>/dev/null; then
  echo "  up at http://localhost:7777/"
else
  echo "  not running (start with ./core/scripts/observer-start.sh)"
fi
echo

if [[ -n "$ACTIVE" && -d "games/$ACTIVE/docs/handoffs" ]]; then
  echo "recent handoffs for $ACTIVE:"
  # 5 most recent .md handoffs.
  ls -t "games/$ACTIVE/docs/handoffs"/*.md 2>/dev/null | head -5 | sed 's|^|  |' || echo "  (none yet)"
else
  echo "handoffs: <no active game>"
fi
echo

if [[ -f logs/agent-status.jsonl ]]; then
  echo "last 5 agent-status events:"
  tail -5 logs/agent-status.jsonl | sed 's|^|  |'
fi
