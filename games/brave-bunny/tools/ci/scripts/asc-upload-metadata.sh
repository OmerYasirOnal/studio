#!/usr/bin/env bash
#
# asc-upload-metadata.sh — push metadata-only (no binary) to App Store Connect.
#
# Owner: build-engineer (Wave 11 marketing pipeline).
# Cross-ref: games/brave-bunny/marketing/asc-metadata/SCHEMA.md
#            games/brave-bunny/tools/ci/fastlane/Fastfile
#
# Wraps `fastlane deliver` with the metadata-only flag set so we can iterate
# on App Store copy (name, subtitle, description, keywords, what's new) without
# touching the binary at all. Binary delivery still flows through the
# `beta` / `release` lanes in Fastfile.
#
# Inputs (read from games/brave-bunny/marketing/asc-metadata/):
#   en-US.json      — primary language metadata
#   tr-TR.json      — Turkish soft-launch metadata
#
# The JSON files are converted into the directory structure fastlane deliver
# expects on the fly: ./fastlane/metadata/<locale>/<field>.txt — one file per
# field. We use a temp dir so we never check the materialised .txt files into
# the repo (JSON is the SoT; the .txt files are just a fastlane-required
# transport format).
#
# Required env vars (same as Fastfile):
#   ASC_KEY_ID, ASC_ISSUER_ID — App Store Connect API key identifiers
#   ASC_KEY_PATH              — path to .p8 file (default: ~/.appstoreconnect/private_keys/AuthKey_<KEY_ID>.p8)
#
# Usage:
#   ./asc-upload-metadata.sh                    # uploads en-US + tr-TR
#   ./asc-upload-metadata.sh --dry-run          # validates JSON, doesn't upload

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GAME_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
ASC_METADATA_DIR="$GAME_ROOT/marketing/asc-metadata"
FASTLANE_DIR="$GAME_ROOT/tools/ci/fastlane"

APP_IDENTIFIER="com.omeryasir.bravebunny"
DRY_RUN=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run) DRY_RUN=1; shift ;;
    -h|--help)
      echo "Usage: $0 [--dry-run]"
      echo "Uploads en-US + tr-TR ASC metadata from $ASC_METADATA_DIR"
      exit 0
      ;;
    *) echo "[asc-upload-metadata] unknown arg: $1" >&2; exit 2 ;;
  esac
done

echo "[asc-upload-metadata] game root:     $GAME_ROOT"
echo "[asc-upload-metadata] metadata dir:  $ASC_METADATA_DIR"
echo "[asc-upload-metadata] fastlane dir:  $FASTLANE_DIR"
echo "[asc-upload-metadata] dry-run:       $DRY_RUN"

# ---------------------------------------------------------------------------
# Dependency check
# ---------------------------------------------------------------------------

if ! command -v jq >/dev/null 2>&1; then
  echo "[asc-upload-metadata] ERROR: jq is required (brew install jq)" >&2
  exit 1
fi

if [[ $DRY_RUN -eq 0 ]]; then
  if ! command -v fastlane >/dev/null 2>&1 && ! command -v bundle >/dev/null 2>&1; then
    echo "[asc-upload-metadata] ERROR: neither fastlane nor bundler found in PATH" >&2
    echo "[asc-upload-metadata] hint: brew install fastlane  OR  bundle install in $FASTLANE_DIR" >&2
    exit 1
  fi
fi

# ---------------------------------------------------------------------------
# Validate JSON inputs
# ---------------------------------------------------------------------------

LOCALES=("en-US" "tr-TR")
REQUIRED_FIELDS=("name" "subtitle" "description" "keywords" "release_notes" "support_url" "marketing_url" "privacy_url")

for locale in "${LOCALES[@]}"; do
  json="$ASC_METADATA_DIR/$locale.json"
  if [[ ! -f "$json" ]]; then
    echo "[asc-upload-metadata] ERROR: missing $json" >&2
    exit 1
  fi
  if ! jq empty "$json" 2>/dev/null; then
    echo "[asc-upload-metadata] ERROR: $json is not valid JSON" >&2
    exit 1
  fi
  for field in "${REQUIRED_FIELDS[@]}"; do
    if [[ "$(jq -r --arg f "$field" 'has($f)' "$json")" != "true" ]]; then
      echo "[asc-upload-metadata] ERROR: $json missing required field '$field'" >&2
      exit 1
    fi
  done

  # ASC limits: keywords ≤ 100 chars, description ≤ 4000 chars, subtitle ≤ 30 chars, name ≤ 30 chars.
  for pair in "name:30" "subtitle:30" "keywords:100" "description:4000"; do
    field="${pair%%:*}"; limit="${pair##*:}"
    value="$(jq -r ".$field" "$json")"
    length=${#value}
    if [[ $length -gt $limit ]]; then
      echo "[asc-upload-metadata] ERROR: $json field '$field' is $length chars (limit $limit)" >&2
      exit 1
    fi
  done
  echo "[asc-upload-metadata] validated $json (${#REQUIRED_FIELDS[@]} fields, limits OK)"
done

if [[ $DRY_RUN -eq 1 ]]; then
  echo "[asc-upload-metadata] dry-run OK — JSON valid, limits respected. Skipping upload."
  exit 0
fi

# ---------------------------------------------------------------------------
# Materialise fastlane deliver's expected metadata/ tree in a temp dir.
# Deliverfile (see fastlane/Deliverfile if present) is generated on the fly.
# ---------------------------------------------------------------------------

TMP_DIR="$(mktemp -d -t bb-asc-meta.XXXXXX)"
trap 'rm -rf "$TMP_DIR"' EXIT
META_DIR="$TMP_DIR/fastlane/metadata"
mkdir -p "$META_DIR"

for locale in "${LOCALES[@]}"; do
  json="$ASC_METADATA_DIR/$locale.json"
  loc_dir="$META_DIR/$locale"
  mkdir -p "$loc_dir"

  jq -r '.name'          "$json" > "$loc_dir/name.txt"
  jq -r '.subtitle'      "$json" > "$loc_dir/subtitle.txt"
  jq -r '.description'   "$json" > "$loc_dir/description.txt"
  jq -r '.keywords'      "$json" > "$loc_dir/keywords.txt"
  jq -r '.release_notes' "$json" > "$loc_dir/release_notes.txt"
  jq -r '.support_url'   "$json" > "$loc_dir/support_url.txt"
  jq -r '.marketing_url' "$json" > "$loc_dir/marketing_url.txt"
  jq -r '.privacy_url'   "$json" > "$loc_dir/privacy_url.txt"

  # Optional fields that ASC tolerates being absent.
  if [[ "$(jq -r 'has("promotional_text")' "$json")" == "true" ]]; then
    jq -r '.promotional_text' "$json" > "$loc_dir/promotional_text.txt"
  fi

  echo "[asc-upload-metadata] materialised $locale → $loc_dir"
done

# ---------------------------------------------------------------------------
# Invoke fastlane deliver in metadata-only mode.
# ---------------------------------------------------------------------------

# Pick fastlane invocation — `bundle exec fastlane` if a Gemfile lives next to
# the rest of the brave-bunny fastlane config, plain `fastlane` otherwise.
if [[ -f "$FASTLANE_DIR/Gemfile" ]] && command -v bundle >/dev/null 2>&1; then
  FASTLANE_CMD=(bundle exec --gemfile="$FASTLANE_DIR/Gemfile" fastlane)
else
  FASTLANE_CMD=(fastlane)
fi

ASC_KEY_ID="${ASC_KEY_ID:-93HFBMV3MA}"
ASC_ISSUER_ID="${ASC_ISSUER_ID:-3894e346-c886-4ca5-91b7-773aaa6e85bd}"
ASC_KEY_PATH="${ASC_KEY_PATH:-$HOME/.appstoreconnect/private_keys/AuthKey_${ASC_KEY_ID}.p8}"

if [[ ! -f "$ASC_KEY_PATH" ]]; then
  echo "[asc-upload-metadata] ERROR: ASC API key not found at $ASC_KEY_PATH" >&2
  echo "[asc-upload-metadata] hint: see brave-bunny-secrets-paths memory note" >&2
  exit 1
fi

echo "[asc-upload-metadata] invoking fastlane deliver (metadata only)"

cd "$TMP_DIR"
"${FASTLANE_CMD[@]}" deliver \
  --app_identifier        "$APP_IDENTIFIER" \
  --api_key_path          "$ASC_KEY_PATH" \
  --metadata_path         "$META_DIR" \
  --skip_binary_upload    true \
  --skip_screenshots      true \
  --force                 true \
  --precheck_include_in_app_purchases false

echo "[asc-upload-metadata] SUCCESS — metadata uploaded for: ${LOCALES[*]}"
exit 0
