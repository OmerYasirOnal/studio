#!/usr/bin/env bash
#
# run-play-mode-tests.sh — invoke Unity in batchmode to run PlayMode tests
# and parse NUnit XML for pass/fail.
#
# Owner: build-engineer. Wave 11 hardening (Brave Bunny CI).
# Cross-ref: tech-spec 10-build-and-ci.md.
#
# Usage:
#   ./run-play-mode-tests.sh                                # all PlayMode tests
#   TEST_CATEGORY=Smoke ./run-play-mode-tests.sh            # only [Category("Smoke")]
#   TEST_FILTER='Brave.Tests.PlayMode.Smoke.*' \
#     ./run-play-mode-tests.sh                              # filter by name
#
# Exit codes:
#   0  all tests passed
#   1  one or more tests failed
#   2  Unity failed to run or NUnit XML missing
#   127 Unity binary not found

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_PROJECT="$(cd "$SCRIPT_DIR/../../../unity" && pwd)"

PROJECT_VERSION_FILE="$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt"
if [[ -f "$PROJECT_VERSION_FILE" ]]; then
  UNITY_VERSION="$(grep '^m_EditorVersion:' "$PROJECT_VERSION_FILE" | awk '{print $2}')"
else
  UNITY_VERSION="${UNITY_VERSION:-6000.0.74f1}"
  echo "[play-mode] WARN: $PROJECT_VERSION_FILE missing, defaulting to $UNITY_VERSION"
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_DIR="$UNITY_PROJECT/Logs"
RESULTS_DIR="$UNITY_PROJECT/TestResults"
RESULTS_PATH="${RESULTS_PATH:-$RESULTS_DIR/playmode-${TIMESTAMP}.xml}"
LOG_FILE="$LOG_DIR/test-playmode-${TIMESTAMP}.log"
TEST_FILTER="${TEST_FILTER:-}"
TEST_CATEGORY="${TEST_CATEGORY:-}"

mkdir -p "$LOG_DIR" "$RESULTS_DIR"

echo "[play-mode] starting PlayMode test run"
echo "[play-mode] unity:    $UNITY_BIN"
echo "[play-mode] version:  $UNITY_VERSION"
echo "[play-mode] project:  $UNITY_PROJECT"
echo "[play-mode] results:  $RESULTS_PATH"
echo "[play-mode] log:      $LOG_FILE"
[[ -n "$TEST_FILTER"   ]] && echo "[play-mode] filter:   $TEST_FILTER"
[[ -n "$TEST_CATEGORY" ]] && echo "[play-mode] category: $TEST_CATEGORY"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[play-mode] ERROR: Unity binary not found or not executable: $UNITY_BIN" >&2
  echo "[play-mode] hint: install Unity $UNITY_VERSION via Unity Hub, or set UNITY_BIN env var." >&2
  exit 127
fi

unity_args=(
  -batchmode
  -nographics
  -projectPath "$UNITY_PROJECT"
  -runTests
  -testPlatform playmode
  -testResults "$RESULTS_PATH"
  -logFile "$LOG_FILE"
)
if [[ -n "$TEST_FILTER" ]]; then
  unity_args+=(-testFilter "$TEST_FILTER")
fi
if [[ -n "$TEST_CATEGORY" ]]; then
  unity_args+=(-testCategory "$TEST_CATEGORY")
fi

set +e
"$UNITY_BIN" "${unity_args[@]}"
UNITY_RC=$?
set -e

echo "[play-mode] unity exit: $UNITY_RC"

if [[ ! -f "$RESULTS_PATH" ]]; then
  echo "[play-mode] ERROR: NUnit results file missing at $RESULTS_PATH" >&2
  echo "[play-mode] tail of Unity log:" >&2
  tail -n 80 "$LOG_FILE" >&2 || true
  exit 2
fi

extract_attr() {
  local attr="$1"
  grep -oE "${attr}=\"[0-9]+\"" "$RESULTS_PATH" | head -1 | grep -oE '[0-9]+' || echo "0"
}
TOTAL=$(extract_attr "total")
PASSED=$(extract_attr "passed")
FAILED=$(extract_attr "failed")
SKIPPED=$(extract_attr "skipped")
INCONCLUSIVE=$(extract_attr "inconclusive")

echo ""
echo "[play-mode] === NUnit summary ==="
echo "[play-mode]   total:        $TOTAL"
echo "[play-mode]   passed:       $PASSED"
echo "[play-mode]   failed:       $FAILED"
echo "[play-mode]   skipped:      $SKIPPED"
echo "[play-mode]   inconclusive: $INCONCLUSIVE"

if [[ "$FAILED" != "0" ]]; then
  echo "[play-mode] FAIL — $FAILED test(s) failed; see $RESULTS_PATH" >&2
  echo "[play-mode] tail of log:" >&2
  tail -n 60 "$LOG_FILE" >&2 || true
  exit 1
fi

if [[ "$TOTAL" == "0" ]]; then
  echo "[play-mode] WARN: zero tests reported — verify test filter / asmdef wiring" >&2
fi

if [[ $UNITY_RC -ne 0 && "$FAILED" == "0" ]]; then
  echo "[play-mode] WARN: Unity exited $UNITY_RC but no failed tests; treating as run-time error" >&2
  exit 2
fi

echo "[play-mode] OK — all PlayMode tests passed"
exit 0
