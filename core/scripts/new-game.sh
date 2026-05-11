#!/usr/bin/env bash
# new-game.sh <name> [--template <action-roguelite|endless-runner|puzzle>]
#
# Scaffolds a new game project under games/<name>/ using one of the templates
# under core/templates/. Writes .active-game at the repo root so subsequent
# spawn-agent.sh calls know which game to operate on.
#
# Usage:
#   ./core/scripts/new-game.sh brave-bunny --template action-roguelite

set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: $0 <game-name> [--template <action-roguelite|endless-runner|puzzle>]" >&2
  exit 1
fi

NAME="$1"
shift

TEMPLATE="action-roguelite"
while [[ $# -gt 0 ]]; do
  case "$1" in
    --template) TEMPLATE="$2"; shift 2 ;;
    *) echo "[new-game] unknown arg: $1" >&2; exit 1 ;;
  esac
done

# Repo root = the directory containing this script's parent (core/scripts -> ..).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT"

if [[ ! -d "core/templates/$TEMPLATE" ]]; then
  echo "[new-game] no such template: core/templates/$TEMPLATE" >&2
  echo "[new-game] available: $(ls core/templates | grep -v '^_common$' | tr '\n' ' ')" >&2
  exit 1
fi

GAME_DIR="games/$NAME"
if [[ -d "$GAME_DIR" ]]; then
  echo "[new-game] $GAME_DIR already exists — aborting" >&2
  exit 1
fi

mkdir -p "$GAME_DIR"
# Copy template files (and shared _common files) preserving directory tree.
# shellcheck disable=SC2046
cp -R "core/templates/$TEMPLATE/." "$GAME_DIR/"
cp -R "core/templates/_common/." "$GAME_DIR/"

# Replace placeholders inside the game directory.
TS="$(date +%Y-%m-%d)"
DISPLAY_NAME="$(echo "$NAME" | awk -F'-' '{for(i=1;i<=NF;i++){$i=toupper(substr($i,1,1)) substr($i,2)}; print}' OFS=' ')"

while IFS= read -r -d '' f; do
  if [[ -f "$f" ]]; then
    sed -i.bak \
      -e "s|__GAME_NAME__|$NAME|g" \
      -e "s|__DISPLAY_NAME__|$DISPLAY_NAME|g" \
      -e "s|__TEMPLATE__|$TEMPLATE|g" \
      -e "s|__SCAFFOLD_DATE__|$TS|g" \
      "$f"
    rm -f "${f}.bak"
  fi
done < <(find "$GAME_DIR" -type f \( -name '*.md' -o -name '*.yml' -o -name '*.yaml' -o -name '*.json' -o -name 'GAME.md' \) -print0)

# Mark the active game.
echo "$NAME" > .active-game

# Log it.
mkdir -p logs
TS_EPOCH=$(date +%s)
echo "{\"ts\":$TS_EPOCH,\"event\":\"new-game\",\"game\":\"$NAME\",\"template\":\"$TEMPLATE\"}" >> logs/framework-events.jsonl

cat <<EOF

[new-game] OK
  game:       $NAME
  template:   $TEMPLATE
  directory:  $GAME_DIR
  active:     yes (.active-game updated)

Next steps:
  1. Edit games/$NAME/GAME.md and fill in the YAML (genre, inspirations, platforms, etc.)
  2. ./core/scripts/observer-start.sh   # http://localhost:7777
  3. In Claude Code at the repo root: /phase-status
EOF
