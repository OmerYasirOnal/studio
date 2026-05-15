#!/usr/bin/env bash
#
# run-edit-mode-tests.sh — invoke Unity in batchmode to run EditMode tests
# and parse NUnit XML for pass/fail.
#
# Owner: build-engineer. Wave 11 hardening (Brave Bunny CI).
# Cross-ref: tech-spec 10-build-and-ci.md.
#
# Usage:
#   ./run-edit-mode-tests.sh                  # default project + results path
#   UNITY_BIN=/path/to/Unity ./run-edit-mode-tests.sh
#   TEST_FILTER='Brave.Tests.EditMode.Boot.*' ./run-edit-mode-tests.sh
#
# Exit codes:
#   0  all tests passed
#   1  one or more tests failed
#   2  Unity failed to run or NUnit XML missing
#   127 Unity binary not found
#
# Output:
#   - NUnit XML at $RESULTS_PATH (default: unity/TestResults/editmode-<ts>.xml)
#   - Unity logfile at unity/Logs/test-editmode-<ts>.log
#   - Summary printed to stdout, parsed pass/fail counts

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNITY_PROJECT="$(cd "$SCRIPT_DIR/../../../unity" && pwd)"

# ---------------------------------------------------------------------------
# Resolve Unity binary from ProjectVersion.txt (single source of truth)
# ---------------------------------------------------------------------------
PROJECT_VERSION_FILE="$UNITY_PROJECT/ProjectSettings/ProjectVersion.txt"
if [[ -f "$PROJECT_VERSION_FILE" ]]; then
  UNITY_VERSION="$(grep '^m_EditorVersion:' "$PROJECT_VERSION_FILE" | awk '{print $2}')"
else
  UNITY_VERSION="${UNITY_VERSION:-6000.0.74f1}"
  echo "[edit-mode] WARN: $PROJECT_VERSION_FILE missing, defaulting to $UNITY_VERSION"
fi

UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
LOG_DIR="$UNITY_PROJECT/Logs"
RESULTS_DIR="$UNITY_PROJECT/TestResults"
RESULTS_PATH="${RESULTS_PATH:-$RESULTS_DIR/editmode-${TIMESTAMP}.xml}"
LOG_FILE="$LOG_DIR/test-editmode-${TIMESTAMP}.log"
TEST_FILTER="${TEST_FILTER:-}"

mkdir -p "$LOG_DIR" "$RESULTS_DIR"

echo "[edit-mode] starting EditMode test run"
echo "[edit-mode] unity:   $UNITY_BIN"
echo "[edit-mode] version: $UNITY_VERSION"
echo "[edit-mode] project: $UNITY_PROJECT"
echo "[edit-mode] results: $RESULTS_PATH"
echo "[edit-mode] log:     $LOG_FILE"
[[ -n "$TEST_FILTER" ]] && echo "[edit-mode] filter:  $TEST_FILTER"

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "[edit-mode] ERROR: Unity binary not found or not executable: $UNITY_BIN" >&2
  echo "[edit-mode] hint: install Unity $UNITY_VERSION via Unity Hub, or set UNITY_BIN env var." >&2
  exit 127
fi

# ---------------------------------------------------------------------------
# Invoke Unity in batchmode.
# `-runTests` exits Unity automatically after tests; `-quit` is implicit.
# `testFilter` is optional; passes only when set.
# ---------------------------------------------------------------------------
unity_args=(
  -batchmode
  -nographics
  -projectPath "$UNITY_PROJECT"
  -runTests
  -testPlatform editmode
  -testResults "$RESULTS_PATH"
  -logFile "$LOG_FILE"
)
if [[ -n "$TEST_FILTER" ]]; then
  unity_args+=(-testFilter "$TEST_FILTER")
fi

set +e
"$UNITY_BIN" "${unity_args[@]}"
UNITY_RC=$?
set -e

echo "[edit-mode] unity exit: $UNITY_RC"

# ---------------------------------------------------------------------------
# Parse NUnit XML for pass/fail counts.
# NUnit3 schema: <test-run total="N" passed="N" failed="N" skipped="N" inconclusive="N">
# ---------------------------------------------------------------------------
if [[ ! -f "$RESULTS_PATH" ]]; then
  echo "[edit-mode] ERROR: NUnit results file missing at $RESULTS_PATH" >&2
  echo "[edit-mode] tail of Unity log:" >&2
  tail -n 80 "$LOG_FILE" >&2 || true
  exit 2
fi

# Extract counts via pure-grep (no xmllint dependency on macOS runners).
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
echo "[edit-mode] === NUnit summary ==="
echo "[edit-mode]   total:        $TOTAL"
echo "[edit-mode]   passed:       $PASSED"
echo "[edit-mode]   failed:       $FAILED"
echo "[edit-mode]   skipped:      $SKIPPED"
echo "[edit-mode]   inconclusive: $INCONCLUSIVE"

# Unity returns non-zero when tests fail (RC=2 typically). Surface both signals.
if [[ "$FAILED" != "0" ]]; then
  echo "[edit-mode] FAIL — $FAILED test(s) failed; see $RESULTS_PATH" >&2
  echo "[edit-mode] tail of log:" >&2
  tail -n 40 "$LOG_FILE" >&2 || true
  exit 1
fi

if [[ "$TOTAL" == "0" ]]; then
  echo "[edit-mode] WARN: zero tests reported — verify test filter / asmdef wiring" >&2
fi

if [[ $UNITY_RC -ne 0 && "$FAILED" == "0" ]]; then
  echo "[edit-mode] WARN: Unity exited $UNITY_RC but no failed tests; treating as run-time error" >&2
  exit 2
fi

echo "[edit-mode] OK — all EditMode tests passed"
exit 0
