#!/usr/bin/env bash
#
# archive.sh — wrap `fastlane preview` and stash the resulting IPA under
# games/brave-bunny/Builds/ with a SemVer tag derived from GAME.md.
#
# Owner: build-engineer. Cross-ref: tech-spec 10-build-and-ci.md.
# Local convenience wrapper; CI uses fastlane directly.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
FASTLANE_DIR="$GAME_ROOT/tools/ci/fastlane"
BUILDS_DIR="$GAME_ROOT/Builds"

# Parse semver from GAME.md frontmatter — fallback to 0.0.0 if absent.
SEMVER="$(grep -E '^semver:' "$GAME_ROOT/GAME.md" 2>/dev/null | awk '{print $2}' | tr -d '"' || true)"
SEMVER="${SEMVER:-0.0.0}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
TAG="v${SEMVER}-${TIMESTAMP}"

echo "[archive] semver: $SEMVER"
echo "[archive] tag:    $TAG"
echo "[archive] builds: $BUILDS_DIR"

mkdir -p "$BUILDS_DIR"

# Drop into fastlane dir so Fastfile relative paths resolve.
cd "$FASTLANE_DIR"

if ! command -v bundle >/dev/null 2>&1; then
  echo "[archive] ERROR: bundler not installed. Run \`gem install bundler\` first." >&2
  exit 127
fi

bundle exec fastlane ios preview

# Rename the most recent IPA with the SemVer tag for archival traceability.
LATEST_IPA="$(ls -t "$BUILDS_DIR"/*.ipa 2>/dev/null | head -n 1 || true)"
if [[ -n "$LATEST_IPA" ]]; then
  ARCHIVED="$BUILDS_DIR/BraveBunny-${TAG}.ipa"
  cp "$LATEST_IPA" "$ARCHIVED"
  echo "[archive] archived: $ARCHIVED"
else
  echo "[archive] WARN: no IPA found in $BUILDS_DIR after fastlane preview" >&2
  exit 1
fi
