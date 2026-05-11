#!/usr/bin/env bash
# post-commit-log.sh — append a JSONL line after each git commit.
# Run from a git hook (.git/hooks/post-commit) which calls this script.

set -uo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)" || exit 0
cd "$REPO_ROOT"

LOG_DIR="$REPO_ROOT/logs"
mkdir -p "$LOG_DIR"

SHA="$(git rev-parse HEAD)"
SHORT="$(git rev-parse --short HEAD)"
MSG="$(git log -1 --pretty=%s | tr -d '"' | head -c 200)"
AUTHOR="$(git log -1 --pretty=%an | tr -d '"')"
ACTIVE_GAME="$(cat "$REPO_ROOT/.active-game" 2>/dev/null || echo "")"
FILES_CHANGED="$(git diff-tree --no-commit-id --name-only -r HEAD | wc -l | tr -d ' ')"
TS=$(date +%s)

ENTRY="{\"ts\":$TS,\"sha\":\"$SHA\",\"short\":\"$SHORT\",\"author\":\"$AUTHOR\",\"msg\":\"$MSG\",\"files\":$FILES_CHANGED,\"game\":\"$ACTIVE_GAME\"}"
echo "$ENTRY" >> "$LOG_DIR/commits.jsonl"

exit 0
