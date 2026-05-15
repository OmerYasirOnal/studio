#!/usr/bin/env bash
#
# capture-screenshots.sh — headless Unity screenshot capture + overlay pipeline.
#
# Owner: build-engineer / art-director (Wave 11 marketing pipeline).
# Cross-ref: games/brave-bunny/docs/marketing/screenshot-spec.md
#            unity/Assets/_Brave/Code/Boot/Editor/ScreenshotCapture.cs
#            unity/Assets/_Brave/Code/Boot/Editor/ScreenshotOverlay.cs
#
# Two-pass workflow:
#   1. Unity -executeMethod Brave.Boot.Editor.ScreenshotCapture.CaptureAll
#      → raw PNGs to marketing/screenshots/raw/<device>/01.png ... 05.png
#   2. Unity -executeMethod Brave.Boot.Editor.ScreenshotOverlay.OverlayAll
#      → composited PNGs (headline + subhead) to marketing/screenshots/<lang>/<device>/
#
# Output dir is configurable via OUTPUT_DIR env var. Default is
# games/brave-bunny/marketing/screenshots/ which is .gitignored for PNG content.
#
# Usage:
#   ./capture-screenshots.sh                  # uses default output dir
#   OUTPUT_DIR=/tmp/bb-shots ./capture-screenshots.sh
#
# CI hook: deferred — Wave 11 ships interactive capture only; CI capture would
# need a hand-authored "Capture" scene wired into EditorBuildSettings. TODO when
# the capture scene is checked in (asset-curator follow-up).

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
UNITY_PROJECT="$GAME_ROOT/unity"

# Discover Unity version from ProjectVersion.txt (same pattern as unity-build-ios.sh).
PROJECT_VERSION_FILE="$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt"
if [[ -f "$PROJECT_VERSION_FILE" ]]; then
  UNITY_VERSION="$(grep '^m_EditorVersion:' "$PROJECT_VERSION_FILE" | awk '{print $2}')"
else
  UNITY_VERSION="6000.0.31f1"
  echo "[capture-screenshots] WARN: $PROJECT_VERSION_FILE missing, defaulting to $UNITY_VERSION"
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"
OUTPUT_DIR="${OUTPUT_DIR:-$GAME_ROOT/marketing/screenshots}"

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_DIR="$UNITY_PROJECT/Logs"
LOG_FILE_CAPTURE="$LOG_DIR/capture-screenshots-${TIMESTAMP}-capture.log"
LOG_FILE_OVERLAY="$LOG_DIR/capture-screenshots-${TIMESTAMP}-overlay.log"

mkdir -p "$LOG_DIR" "$OUTPUT_DIR/raw"

echo "[capture-screenshots] unity:      $UNITY_BIN"
echo "[capture-screenshots] version:    $UNITY_VERSION"
echo "[capture-screenshots] project:    $UNITY_PROJECT"
echo "[capture-screenshots] output:     $OUTPUT_DIR"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[capture-screenshots] ERROR: Unity binary not found or not executable: $UNITY_BIN" >&2
  echo "[capture-screenshots] hint: install Unity $UNITY_VERSION via Unity Hub, or set UNITY_BIN env var." >&2
  exit 127
fi

# ---------------------------------------------------------------------------
# Pass 1 — capture raw PNGs from current Capture scene at each device aspect.
# ---------------------------------------------------------------------------

echo "[capture-screenshots] pass 1/2 — raw capture → $LOG_FILE_CAPTURE"

set +e
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$UNITY_PROJECT" \
  -executeMethod Brave.Boot.Editor.ScreenshotCapture.CaptureAll \
  -logFile "$LOG_FILE_CAPTURE" \
  -- \
  -output "$OUTPUT_DIR"
RC_CAPTURE=$?
set -e

if [[ $RC_CAPTURE -ne 0 ]]; then
  echo "[capture-screenshots] pass 1 FAILED — tail of log follows:" >&2
  tail -n 80 "$LOG_FILE_CAPTURE" >&2 || true
  exit "$RC_CAPTURE"
fi

echo "[capture-screenshots] pass 1 OK"

# ---------------------------------------------------------------------------
# Pass 2 — apply EN + TR headline overlays to the raw PNGs.
# ---------------------------------------------------------------------------

echo "[capture-screenshots] pass 2/2 — headline overlays → $LOG_FILE_OVERLAY"

set +e
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$UNITY_PROJECT" \
  -executeMethod Brave.Boot.Editor.ScreenshotOverlay.OverlayAll \
  -logFile "$LOG_FILE_OVERLAY" \
  -- \
  -output "$OUTPUT_DIR"
RC_OVERLAY=$?
set -e

if [[ $RC_OVERLAY -ne 0 ]]; then
  echo "[capture-screenshots] pass 2 FAILED — tail of log follows:" >&2
  tail -n 80 "$LOG_FILE_OVERLAY" >&2 || true
  exit "$RC_OVERLAY"
fi

echo "[capture-screenshots] pass 2 OK"

# ---------------------------------------------------------------------------
# Summary
# ---------------------------------------------------------------------------

RAW_COUNT="$(find "$OUTPUT_DIR/raw" -name '*.png' 2>/dev/null | wc -l | tr -d ' ')"
FINAL_COUNT="$(find "$OUTPUT_DIR" -path "$OUTPUT_DIR/raw" -prune -o -name '*.png' -print 2>/dev/null | wc -l | tr -d ' ')"
echo "[capture-screenshots] SUCCESS — raw PNGs: $RAW_COUNT, final PNGs (EN+TR): $FINAL_COUNT"
echo "[capture-screenshots] output tree: $OUTPUT_DIR"
exit 0
