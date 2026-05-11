#!/usr/bin/env bash
#
# upload-testflight.sh — wrap `fastlane beta` with env-var preflight checks.
#
# Owner: build-engineer. Cross-ref: tech-spec 10-build-and-ci.md.
# Runs locally (developer's mac) or on CI (macos-14 runner).

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FASTLANE_DIR="$(cd "$SCRIPT_DIR/../fastlane" && pwd)"

REQUIRED_VARS=(
  MATCH_PASSWORD
  FASTLANE_USER
  FASTLANE_TEAM_ID
  FASTLANE_ITC_TEAM_ID
)

MISSING=()
for v in "${REQUIRED_VARS[@]}"; do
  if [[ -z "${!v:-}" ]]; then
    MISSING+=("$v")
  fi
done

if [[ ${#MISSING[@]} -gt 0 ]]; then
  echo "[upload-testflight] ERROR: missing required env vars: ${MISSING[*]}" >&2
  echo "[upload-testflight] See games/brave-bunny/tools/ci/runbooks/first-build.md" >&2
  exit 1
fi

# Either FASTLANE_PASSWORD or FASTLANE_APP_SPECIFIC_PASSWORD must be set.
if [[ -z "${FASTLANE_PASSWORD:-}" && -z "${FASTLANE_APP_SPECIFIC_PASSWORD:-}" ]]; then
  echo "[upload-testflight] ERROR: set FASTLANE_APP_SPECIFIC_PASSWORD (preferred) or FASTLANE_PASSWORD" >&2
  exit 1
fi

cd "$FASTLANE_DIR"
bundle exec fastlane ios beta
