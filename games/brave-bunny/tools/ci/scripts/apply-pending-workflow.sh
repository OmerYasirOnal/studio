#!/usr/bin/env bash
# apply-pending-workflow.sh — apply the .pending bb-ios-build.yml update.
#
# GitHub OAuth tokens require 'workflow' scope to modify .github/workflows/*.yml.
# The gh CLI's default token (`repo` scope only) cannot push workflow changes,
# so the autonomous setup left the update at:
#   games/brave-bunny/tools/ci/github-actions/ios-build.yml.pending
#
# Two paths to apply:
#
# Path A — Add workflow scope and git push (recommended, ~30 sec):
#   gh auth refresh --hostname github.com --scopes workflow
#   ./games/brave-bunny/tools/ci/scripts/apply-pending-workflow.sh
#
# Path B — Edit on web UI:
#   Open the .pending file content, paste into GitHub web editor at
#   github.com/OmerYasirOnal/studio/blob/main/.github/workflows/bb-ios-build.yml

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../../../../.." && pwd)"
cd "$ROOT"

PENDING="games/brave-bunny/tools/ci/github-actions/ios-build.yml.pending"
TARGET=".github/workflows/bb-ios-build.yml"

if [[ ! -f "$PENDING" ]]; then
  echo "[apply-workflow] no pending file at $PENDING — nothing to do" >&2
  exit 1
fi

if ! gh auth status 2>&1 | grep -q "workflow"; then
  echo "[apply-workflow] gh token lacks 'workflow' scope. Run:" >&2
  echo "  gh auth refresh --hostname github.com --scopes workflow" >&2
  echo "" >&2
  echo "Then re-run this script." >&2
  exit 2
fi

cp "$PENDING" "$TARGET"
git add "$TARGET" "$PENDING"
git rm -f "$PENDING" 2>/dev/null || rm -f "$PENDING"
git add -A "$TARGET" "$PENDING"

git diff --staged --stat
echo ""
read -p "Commit and push these changes? [y/N] " yn
case "$yn" in
  [Yy]*)
    git commit -m "ci(brave-bunny): apply pending workflow update — ASC API key + Unity 6000.0.74f1 pin"
    git push origin main
    echo "[apply-workflow] OK — workflow updated, .pending removed"
    ;;
  *)
    echo "[apply-workflow] cancelled; changes staged but not committed"
    ;;
esac
