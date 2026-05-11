#!/usr/bin/env bash
#
# unity-build-ios.sh — headless Unity build invocation for iOS.
#
# Owner: build-engineer. Cross-ref: tech-spec 10-build-and-ci.md ("Headless Unity build").
# Called by fastlane (Fastfile :preview / :beta / :release lanes) and runnable
# standalone from a local shell.
#
# Output: games/brave-bunny/unity/Build/iOS/Unity-iPhone.xcodeproj
# Logs:   games/brave-bunny/unity/Logs/build-ios-<timestamp>.log
#
# Unity Editor invokes the static method `Brave.Boot.IOSBuilder.Build`
# (see unity/Assets/_Brave/Code/Boot/Editor/IOSBuilder.cs).
#
# Override UNITY_BIN env var to point at a non-default Unity install:
#   UNITY_BIN=/Applications/Unity/Hub/Editor/6000.0.31f1/Unity.app/Contents/MacOS/Unity \
#     ./unity-build-ios.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_PROJECT="$(cd "$SCRIPT_DIR/../../../unity" && pwd)"

# Discover Unity version from ProjectVersion.txt (tech-spec 00: this file is SoT).
PROJECT_VERSION_FILE="$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt"
if [[ -f "$PROJECT_VERSION_FILE" ]]; then
  UNITY_VERSION="$(grep '^m_EditorVersion:' "$PROJECT_VERSION_FILE" | awk '{print $2}')"
else
  UNITY_VERSION="6000.0.31f1"   # fallback; tech-spec 00 pins to Unity 6 LTS latest patch
  echo "[unity-build-ios] WARN: $PROJECT_VERSION_FILE missing, defaulting to $UNITY_VERSION"
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_DIR="$UNITY_PROJECT/Logs"
LOG_FILE="$LOG_DIR/build-ios-${TIMESTAMP}.log"
OUTPUT="$UNITY_PROJECT/Build/iOS"

mkdir -p "$LOG_DIR" "$OUTPUT"

echo "[unity-build-ios] starting headless Unity build"
echo "[unity-build-ios] unity:   $UNITY_BIN"
echo "[unity-build-ios] version: $UNITY_VERSION"
echo "[unity-build-ios] project: $UNITY_PROJECT"
echo "[unity-build-ios] output:  $OUTPUT"
echo "[unity-build-ios] log:     $LOG_FILE"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[unity-build-ios] ERROR: Unity binary not found or not executable: $UNITY_BIN" >&2
  echo "[unity-build-ios] hint: install Unity $UNITY_VERSION via Unity Hub, or set UNITY_BIN env var." >&2
  exit 127
fi

# -batchmode + -nographics + -quit = headless build that exits when done.
# Args after `--` are surfaced to the C# entry point via Environment.GetCommandLineArgs().
set +e
"$UNITY_BIN" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$UNITY_PROJECT" \
  -buildTarget iOS \
  -executeMethod Brave.Boot.IOSBuilder.Build \
  -logFile "$LOG_FILE" \
  -- \
  -output "$OUTPUT"
RC=$?
set -e

echo "[unity-build-ios] exit: $RC"

if [[ $RC -ne 0 ]]; then
  echo "[unity-build-ios] FAILED — tail of log follows:" >&2
  tail -n 80 "$LOG_FILE" >&2 || true
  exit "$RC"
fi

if [[ ! -d "$OUTPUT/Unity-iPhone.xcodeproj" ]]; then
  echo "[unity-build-ios] ERROR: Unity exited 0 but Xcode project not found at $OUTPUT/Unity-iPhone.xcodeproj" >&2
  exit 2
fi

echo "[unity-build-ios] SUCCESS — Xcode project at $OUTPUT/Unity-iPhone.xcodeproj"
exit 0
