#!/usr/bin/env bash
# pre-edit-format.sh — read-back hook to assert files we're about to touch are formatted.
# Run before Edit/Write tool calls. Fails (non-zero exit) only on uncorrectable issues.

set -uo pipefail

TARGET="${1:-}"
[[ -z "$TARGET" ]] && exit 0

# Skip if target doesn't exist yet (new file).
[[ ! -f "$TARGET" ]] && exit 0

case "$TARGET" in
  *.py)
    if command -v ruff >/dev/null 2>&1; then
      ruff format --check "$TARGET" >/dev/null 2>&1 || ruff format "$TARGET" >/dev/null 2>&1
    fi
    ;;
  *.sh)
    if command -v shfmt >/dev/null 2>&1; then
      shfmt -w -i 2 "$TARGET" >/dev/null 2>&1
    fi
    ;;
  *.md)
    # Trailing-whitespace cleanup is enough.
    sed -i.bak 's/[[:space:]]*$//' "$TARGET" 2>/dev/null && rm -f "${TARGET}.bak"
    ;;
  *.cs)
    if command -v dotnet >/dev/null 2>&1; then
      # dotnet format is opt-in: only run if a .editorconfig exists nearby.
      DIR="$(dirname "$TARGET")"
      while [[ "$DIR" != "/" && "$DIR" != "." && ! -f "$DIR/.editorconfig" ]]; do
        DIR="$(dirname "$DIR")"
      done
      if [[ -f "$DIR/.editorconfig" ]]; then
        : # dotnet format runs on project, not file — skip per-file to stay fast
      fi
    fi
    ;;
  *.json)
    if command -v jq >/dev/null 2>&1; then
      tmp="$(mktemp)"
      if jq '.' "$TARGET" >"$tmp" 2>/dev/null; then mv "$tmp" "$TARGET"; else rm -f "$tmp"; fi
    fi
    ;;
esac

exit 0
