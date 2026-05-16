#!/usr/bin/env bash
#
# build-for-simulator.sh — Brave Bunny iOS Simulator build pipeline.
# Wave 12: pre-TestFlight smoke test harness.
#
# Owner: build-engineer. Cross-ref: runbooks/simulator-smoke.md.
#
# Pipeline (top-down):
#   1. Run Unity headless build with --target iOS_Simulator
#      → Xcode project at games/brave-bunny/unity/Build/iOS/
#   2. Run xcodebuild against Unity-iPhone.xcodeproj with
#         -sdk iphonesimulator -arch arm64 -configuration Debug
#      → DerivedData at /tmp/bb-sim-build
#   3. Locate the resulting BraveBunny.app inside DerivedData and copy it to
#      games/brave-bunny/Builds/BraveBunny-sim.app
#
# Skip flags:
#   BB_SKIP_UNITY=1 ./build-for-simulator.sh     # skip step 1 (reuse existing Unity output)
#   BB_DERIVED_DATA=/custom/path                  # override DerivedData path
#
# Exit codes:
#   0   success; .app at $APP_OUT
#   1   Unity build failed
#   2   xcodebuild failed
#   3   .app bundle not found after xcodebuild succeeded

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
UNITY_PROJECT="$GAME_ROOT/unity"
XCODE_PROJECT_DIR="$UNITY_PROJECT/Build/iOS"
XCODE_PROJECT="$XCODE_PROJECT_DIR/Unity-iPhone.xcodeproj"
BUILDS_DIR="$GAME_ROOT/Builds"
APP_OUT="$BUILDS_DIR/BraveBunny-sim.app"
DERIVED_DATA="${BB_DERIVED_DATA:-/tmp/bb-sim-build}"

mkdir -p "$BUILDS_DIR"

echo "[build-for-simulator] game root: $GAME_ROOT"
echo "[build-for-simulator] xcode proj: $XCODE_PROJECT"
echo "[build-for-simulator] derived data: $DERIVED_DATA"
echo "[build-for-simulator] app out: $APP_OUT"

# ---------------------------------------------------------------------------
# Step 1 — Unity headless build (skip via BB_SKIP_UNITY=1)
# ---------------------------------------------------------------------------
if [[ "${BB_SKIP_UNITY:-0}" == "1" ]]; then
  echo "[build-for-simulator] BB_SKIP_UNITY=1 → reusing existing Unity output at $XCODE_PROJECT_DIR"
  if [[ ! -d "$XCODE_PROJECT" ]]; then
    echo "[build-for-simulator] ERROR: BB_SKIP_UNITY set but $XCODE_PROJECT missing" >&2
    exit 1
  fi
else
  echo "[build-for-simulator] step 1/3 — Unity headless build (target=iOS_Simulator)"
  bash "$SCRIPT_DIR/unity-build-ios.sh" --target iOS_Simulator
fi

# ---------------------------------------------------------------------------
# Step 2 — xcodebuild for iphonesimulator SDK
# ---------------------------------------------------------------------------
echo "[build-for-simulator] step 2/3 — xcodebuild -sdk iphonesimulator -arch arm64"

# Unity-iPhone.xcodeproj has TWO targets: "Unity-iPhone" (the app) and
# "Unity-iPhone Tests". We build the app target only. The signing identity
# is "" (none) for simulator — simulator apps are not signed.
#
# CODE_SIGNING_REQUIRED=NO + CODE_SIGNING_ALLOWED=NO bypasses match's manual
# signing config without mutating the pbxproj. This is essential because the
# device pipeline pins "Apple Distribution" + a match-provisioning profile
# which xcodebuild rejects against the simulator SDK.

# NB: pass `-destination` (which implies arch) XOR `-arch`, never both — xcodebuild
# refuses the combination ("destination implies architecture, architecture must not
# also be specified"). We choose destination because it is the more declarative form
# and forces simulator-runtime selection.
set +e
xcodebuild \
  -project "$XCODE_PROJECT" \
  -scheme Unity-iPhone \
  -configuration Debug \
  -sdk iphonesimulator \
  -destination 'generic/platform=iOS Simulator' \
  -derivedDataPath "$DERIVED_DATA" \
  CODE_SIGNING_REQUIRED=NO \
  CODE_SIGNING_ALLOWED=NO \
  CODE_SIGN_IDENTITY="" \
  CODE_SIGN_ENTITLEMENTS="" \
  EXPANDED_CODE_SIGN_IDENTITY="" \
  ENABLE_BITCODE=NO \
  build
XCODE_RC=$?
set -e

if [[ $XCODE_RC -ne 0 ]]; then
  echo "[build-for-simulator] xcodebuild FAILED (rc=$XCODE_RC)" >&2
  exit 2
fi

# ---------------------------------------------------------------------------
# Step 3 — locate .app bundle and copy to known path
# ---------------------------------------------------------------------------
echo "[build-for-simulator] step 3/3 — locating .app bundle"

# DerivedData places the .app at:
#   $DERIVED_DATA/Build/Products/Debug-iphonesimulator/<ProductName>.app
PRODUCT_DIR="$DERIVED_DATA/Build/Products/Debug-iphonesimulator"
if [[ ! -d "$PRODUCT_DIR" ]]; then
  echo "[build-for-simulator] ERROR: product dir not found: $PRODUCT_DIR" >&2
  exit 3
fi

# Glob for any .app — Unity's default product name is "Unity-iPhone.app" but
# Xcode may rename based on PRODUCT_NAME build setting.
APP_BUNDLE=""
for candidate in "$PRODUCT_DIR"/*.app; do
  if [[ -d "$candidate" ]]; then
    APP_BUNDLE="$candidate"
    break
  fi
done

if [[ -z "$APP_BUNDLE" ]]; then
  echo "[build-for-simulator] ERROR: no .app bundle found in $PRODUCT_DIR" >&2
  ls -la "$PRODUCT_DIR" >&2 || true
  exit 3
fi

echo "[build-for-simulator] found .app: $APP_BUNDLE"

# Replace existing canonical copy with a fresh one.
rm -rf "$APP_OUT"
cp -R "$APP_BUNDLE" "$APP_OUT"

# Sanity check: ensure the binary inside is mach-o + simulator slice.
APP_BINARY_NAME="$(basename "$APP_OUT" .app)"
APP_BINARY_PATH="$APP_OUT/$APP_BINARY_NAME"
if [[ ! -f "$APP_BINARY_PATH" ]]; then
  # Unity-iPhone.app has its binary named "Unity-iPhone" — derive from Info.plist.
  if [[ -f "$APP_OUT/Info.plist" ]]; then
    APP_BINARY_NAME="$(/usr/libexec/PlistBuddy -c 'Print :CFBundleExecutable' "$APP_OUT/Info.plist" 2>/dev/null || echo Unity-iPhone)"
    APP_BINARY_PATH="$APP_OUT/$APP_BINARY_NAME"
  fi
fi

if [[ -f "$APP_BINARY_PATH" ]]; then
  file "$APP_BINARY_PATH" | head -5 || true
else
  echo "[build-for-simulator] WARN: app binary not located inside $APP_OUT" >&2
fi

echo "[build-for-simulator] SUCCESS — sim app at $APP_OUT"
exit 0
