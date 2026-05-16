#!/usr/bin/env bash
#
# test-in-simulator.sh — boot iOS Simulator, install Brave Bunny, launch,
# screenshot, and assert that the screen is NOT the dreaded pink-shader-error
# (>30% pink pixels triggers a hard failure).
# Wave 12: pre-TestFlight smoke verification (catches pink-screen regressions
# in <2 min instead of waiting 15+ min for TestFlight processing).
#
# Owner: build-engineer. Cross-ref: runbooks/simulator-smoke.md.
#
# Environment overrides:
#   BB_APP_PATH        absolute path to the .app bundle (default: Builds/BraveBunny-sim.app)
#   BB_DEVICE          simulator device name or UDID  (default: iPhone 17)
#   BB_BUNDLE_ID       launch identifier              (default: com.omeryasir.bravebunny)
#   BB_SCREENSHOT_OUT  output png path                (default: /tmp/bb-sim-screen.png)
#   BB_BOOT_WAIT       seconds to wait after launch   (default: 8)
#   BB_PINK_THRESHOLD  failure threshold ratio        (default: 0.3)
#
# Exit codes:
#   0   playable — screenshot captured, pink ratio under threshold
#   1   pink-screen regression detected
#   2   simulator boot / install / launch failure
#   3   screenshot capture failed
#   4   PIL not available / pink check could not run

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

APP_PATH="${BB_APP_PATH:-$GAME_ROOT/Builds/BraveBunny-sim.app}"
DEVICE="${BB_DEVICE:-iPhone 17}"
BUNDLE_ID="${BB_BUNDLE_ID:-com.omeryasir.bravebunny}"
SCREENSHOT_OUT="${BB_SCREENSHOT_OUT:-/tmp/bb-sim-screen.png}"
BOOT_WAIT="${BB_BOOT_WAIT:-8}"
PINK_THRESHOLD="${BB_PINK_THRESHOLD:-0.3}"

echo "[test-in-simulator] app:      $APP_PATH"
echo "[test-in-simulator] device:   $DEVICE"
echo "[test-in-simulator] bundle:   $BUNDLE_ID"
echo "[test-in-simulator] shot:     $SCREENSHOT_OUT"
echo "[test-in-simulator] wait:     ${BOOT_WAIT}s"
echo "[test-in-simulator] pink-thr: $PINK_THRESHOLD"

if [[ ! -d "$APP_PATH" ]]; then
  echo "[test-in-simulator] ERROR: app bundle not found: $APP_PATH" >&2
  echo "[test-in-simulator] hint: run build-for-simulator.sh first" >&2
  exit 2
fi

# ---------------------------------------------------------------------------
# Boot — `simctl boot` exits non-zero if already booted; we tolerate that.
# ---------------------------------------------------------------------------
echo "[test-in-simulator] booting '$DEVICE'..."
xcrun simctl boot "$DEVICE" 2>/dev/null || true

# Wait for the boot to actually complete (state=Booted) — simctl boot returns
# immediately but the runtime services take a beat. Cap at 60s.
boot_deadline=$(( $(date +%s) + 60 ))
while true; do
  state="$(xcrun simctl list devices | grep "$DEVICE" | grep -v unavailable | head -1 | sed -n 's/.*(\(Booted\|Shutdown\|Booting\|Shutting Down\)).*/\1/p')"
  if [[ "$state" == "Booted" ]]; then
    echo "[test-in-simulator] device booted (state=Booted)"
    break
  fi
  if [[ $(date +%s) -ge $boot_deadline ]]; then
    echo "[test-in-simulator] ERROR: device did not reach Booted state within 60s (last=$state)" >&2
    exit 2
  fi
  sleep 1
done

# ---------------------------------------------------------------------------
# Install — `simctl install booted` accepts the .app bundle directly.
# ---------------------------------------------------------------------------
echo "[test-in-simulator] installing $APP_PATH..."
if ! xcrun simctl install booted "$APP_PATH"; then
  echo "[test-in-simulator] ERROR: install failed" >&2
  exit 2
fi

# ---------------------------------------------------------------------------
# Launch — capture stdout/stderr so a launch failure shows up in CI logs.
# ---------------------------------------------------------------------------
echo "[test-in-simulator] launching $BUNDLE_ID..."
if ! xcrun simctl launch booted "$BUNDLE_ID"; then
  echo "[test-in-simulator] ERROR: launch failed" >&2
  exit 2
fi

# ---------------------------------------------------------------------------
# Wait for first frame, then screenshot.
# ---------------------------------------------------------------------------
echo "[test-in-simulator] sleeping ${BOOT_WAIT}s for first frame..."
sleep "$BOOT_WAIT"

echo "[test-in-simulator] capturing screenshot..."
if ! xcrun simctl io booted screenshot "$SCREENSHOT_OUT"; then
  echo "[test-in-simulator] ERROR: screenshot capture failed" >&2
  exit 3
fi
ls -la "$SCREENSHOT_OUT"

# ---------------------------------------------------------------------------
# Pink-pixel detector — fails if >30% of pixels look like the shader-error pink.
# Pink-error pixel heuristic: R>200 and G<100 and B>200 (RGB 255,0,255-ish).
# Uses system python3 + Pillow.
# ---------------------------------------------------------------------------
echo "[test-in-simulator] running pink-pixel inspector..."
set +e
python3 - <<PY
import sys
try:
    from PIL import Image
except Exception as e:
    print(f"pink-inspector: PIL not available: {e}", file=sys.stderr)
    sys.exit(4)

shot = "$SCREENSHOT_OUT"
threshold = float("$PINK_THRESHOLD")
im = Image.open(shot).convert("RGB")
pixels = list(im.getdata())
total = len(pixels)
pink = sum(1 for r, g, b in pixels if r > 200 and g < 100 and b > 200)
ratio = pink / total if total else 0
print(f"pink_ratio={ratio:.3f} pink_pixels={pink} total_pixels={total} threshold={threshold}")
if ratio > threshold:
    print(f"FAIL: pink ratio {ratio:.3f} > threshold {threshold}", file=sys.stderr)
    sys.exit(1)
print("OK: screen does not look like a shader-error pink screen")
sys.exit(0)
PY
PY_RC=$?
set -e

case $PY_RC in
  0) echo "[test-in-simulator] PASS — playable build (screenshot: $SCREENSHOT_OUT)" ;;
  1) echo "[test-in-simulator] FAIL — pink-screen regression detected (screenshot: $SCREENSHOT_OUT)" >&2 ;;
  4) echo "[test-in-simulator] PIL missing — pip3 install Pillow" >&2 ;;
  *) echo "[test-in-simulator] pink inspector failed with rc=$PY_RC" >&2 ;;
esac

exit $PY_RC
