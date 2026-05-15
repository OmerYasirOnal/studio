#!/usr/bin/env bash
#
# build-ios-headless.sh — Wave 11 wrapper around Unity batchmode iOS build.
#
# Owner: build-engineer.
# Cross-ref: tech-spec 10-build-and-ci.md, Wave 11 CI hardening.
#
# This wrapper calls the new BraveBunny.Editor.BuildScripts.BuildIOS entry
# point (Wave 11). The older Brave.Boot.IOSBuilder.Build entry remains
# available — see unity-build-ios.sh — but new CI lanes should prefer this
# script because it supports:
#   - reading version + commit SHA from env (GIT_COMMIT_SHA)
#   - writing to a deterministic Builds/ output root
#   - failing fast with a parseable log tail on error
#
# Usage:
#   ./build-ios-headless.sh
#   GIT_COMMIT_SHA=$(git rev-parse HEAD) ./build-ios-headless.sh
#   OUTPUT_PATH=/tmp/iOSBuild ./build-ios-headless.sh
#
# Exit codes:
#   0   build succeeded; Xcode project at $OUTPUT_PATH/Unity-iPhone.xcodeproj
#   1   Unity build failed
#   2   Unity exited 0 but Xcode project missing (silent failure)
#   127 Unity binary not found

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_PROJECT="$(cd "$SCRIPT_DIR/../../../unity" && pwd)"

# ---------------------------------------------------------------------------
# Unity binary resolution
# ---------------------------------------------------------------------------
PROJECT_VERSION_FILE="$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt"
if [[ -f "$PROJECT_VERSION_FILE" ]]; then
  UNITY_VERSION="$(grep '^m_EditorVersion:' "$PROJECT_VERSION_FILE" | awk '{print $2}')"
else
  UNITY_VERSION="${UNITY_VERSION:-6000.0.74f1}"
  echo "[ios-headless] WARN: $PROJECT_VERSION_FILE missing, defaulting to $UNITY_VERSION"
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_DIR="$UNITY_PROJECT/Logs"
LOG_FILE="$LOG_DIR/build-ios-headless-${TIMESTAMP}.log"
OUTPUT_PATH="${OUTPUT_PATH:-$UNITY_PROJECT/Build/iOS}"

# Commit SHA passthrough — BuildScripts reads GIT_COMMIT_SHA via Environment.GetEnvironmentVariable.
if [[ -z "${GIT_COMMIT_SHA:-}" ]]; then
  if git -C "$UNITY_PROJECT" rev-parse HEAD >/dev/null 2>&1; then
    GIT_COMMIT_SHA="$(git -C "$UNITY_PROJECT" rev-parse HEAD)"
  else
    GIT_COMMIT_SHA="unknown"
  fi
fi
export GIT_COMMIT_SHA

mkdir -p "$LOG_DIR" "$OUTPUT_PATH"

echo "[ios-headless] starting batchmode iOS build (BuildScripts.BuildIOS)"
echo "[ios-headless] unity:    $UNITY_BIN"
echo "[ios-headless] version:  $UNITY_VERSION"
echo "[ios-headless] project:  $UNITY_PROJECT"
echo "[ios-headless] output:   $OUTPUT_PATH"
echo "[ios-headless] commit:   $GIT_COMMIT_SHA"
echo "[ios-headless] log:      $LOG_FILE"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[ios-headless] ERROR: Unity binary not found or not executable: $UNITY_BIN" >&2
  echo "[ios-headless] hint: install Unity $UNITY_VERSION via Unity Hub, or set UNITY_BIN env var." >&2
  exit 127
fi

set +e
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$UNITY_PROJECT" \
  -buildTarget iOS \
  -executeMethod BraveBunny.Editor.BuildScripts.BuildIOS \
  -logFile "$LOG_FILE" \
  -- \
  -output "$OUTPUT_PATH" \
  -commit "$GIT_COMMIT_SHA"
RC=$?
set -e

echo "[ios-headless] unity exit: $RC"

if [[ $RC -ne 0 ]]; then
  echo "[ios-headless] FAILED — tail of log follows:" >&2
  tail -n 80 "$LOG_FILE" >&2 || true
  exit "$RC"
fi

if [[ ! -d "$OUTPUT_PATH/Unity-iPhone.xcodeproj" ]]; then
  echo "[ios-headless] ERROR: Unity exited 0 but Xcode project not found at $OUTPUT_PATH/Unity-iPhone.xcodeproj" >&2
  echo "[ios-headless] tail of log:" >&2
  tail -n 60 "$LOG_FILE" >&2 || true
  exit 2
fi

echo "[ios-headless] SUCCESS — Xcode project at $OUTPUT_PATH/Unity-iPhone.xcodeproj"
exit 0
