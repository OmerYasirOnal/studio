#!/usr/bin/env bash
# observer-start.sh — start the FastAPI observer dashboard on http://localhost:7777
# Creates a .venv in core/observer/ on first run.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT/core/observer"

if ! command -v python3 >/dev/null 2>&1; then
  echo "[observer] python3 is required but not found" >&2
  exit 1
fi

if [[ ! -d .venv ]]; then
  echo "[observer] creating .venv ..."
  python3 -m venv .venv
fi

# shellcheck disable=SC1091
source .venv/bin/activate

if ! python -c "import fastapi, uvicorn" >/dev/null 2>&1; then
  echo "[observer] installing dependencies ..."
  pip install --quiet --upgrade pip
  pip install --quiet -r requirements.txt
fi

# Already running?
if curl -sS -o /dev/null -m 1 http://localhost:7777/health 2>/dev/null; then
  echo "[observer] already running at http://localhost:7777/"
  exit 0
fi

mkdir -p "$ROOT/logs"
LOG="$ROOT/logs/observer.log"

echo "[observer] starting on http://localhost:7777/ (logs: $LOG)"
nohup python -m uvicorn server:app --host 127.0.0.1 --port 7777 --log-level info \
  > "$LOG" 2>&1 &

DASHBOARD_PID=$!
echo "$DASHBOARD_PID" > "$ROOT/logs/observer.pid"

# Wait for health.
for _ in 1 2 3 4 5 6 7 8 9 10; do
  if curl -sS -o /dev/null -m 1 http://localhost:7777/health 2>/dev/null; then
    echo "[observer] healthy. open http://localhost:7777/"
    exit 0
  fi
  sleep 0.3
done

echo "[observer] failed to come up — see $LOG" >&2
exit 1
