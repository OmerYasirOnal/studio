#!/usr/bin/env bash
# verify-game.sh — per-game smoke test. Runs licenses.py, validate_balance.py,
# checks for required documents per Phase 0-3 exit criteria, validates wave JSON.
#
# Usage:
#   ./core/scripts/verify-game.sh                # uses .active-game
#   ./core/scripts/verify-game.sh --game brave-bunny

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT" || exit 1

GAME=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --game) GAME="$2"; shift 2 ;;
    *) echo "[verify-game] unknown arg: $1" >&2; exit 1 ;;
  esac
done

if [[ -z "$GAME" ]]; then
  GAME="$(cat .active-game 2>/dev/null || echo "")"
fi
if [[ -z "$GAME" ]]; then
  echo "[verify-game] no game specified and .active-game empty" >&2
  exit 1
fi
if [[ ! -d "games/$GAME" ]]; then
  echo "[verify-game] no such game: games/$GAME" >&2
  exit 1
fi

pass=0
fail=0
advisory=0

ok() { echo "  ✓ $1"; pass=$((pass + 1)); }
ko() { echo "  ✗ $1" >&2; fail=$((fail + 1)); }
adv() { echo "  ⚠ $1"; advisory=$((advisory + 1)); }

section() {
  echo ""
  echo "=== game: $GAME — $1 ==="
}

GAME_DIR="games/$GAME"

section "core files"
[[ -f "$GAME_DIR/GAME.md" ]]   && ok "GAME.md present"   || ko "GAME.md missing"
[[ -f "$GAME_DIR/CLAUDE.md" ]] && ok "CLAUDE.md present" || ko "CLAUDE.md missing"
[[ -f "$GAME_DIR/README.md" ]] && ok "README.md present" || ko "README.md missing"

section "Phase 1 (Discovery) exit criteria"
DECON_COUNT=$(find "$GAME_DIR/docs/01-research/02-competitors" -name "*.md" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$DECON_COUNT" -ge 5 ]]; then
  ok "5+ competitor deconstructions ($DECON_COUNT)"
elif [[ "$DECON_COUNT" -ge 3 ]]; then
  adv "$DECON_COUNT decons (Phase 1 gate requires 5; first pass OK)"
else
  ko "expected 5 decons, found $DECON_COUNT"
fi
[[ -f "$GAME_DIR/docs/01-research/01-market.md" ]]       && ok "01-market.md"       || ko "01-market.md missing"
[[ -f "$GAME_DIR/docs/01-research/03-positioning.md" ]]  && ok "03-positioning.md"  || ko "03-positioning.md missing"
[[ -f "$GAME_DIR/docs/01-research/04-references.md" ]]   && ok "04-references.md"   || ko "04-references.md missing"

section "Phase 2 (GDD) exit criteria"
GDD_COUNT=$(find "$GAME_DIR/docs/02-gdd" -maxdepth 1 -name "*.md" -not -name "README.md" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$GDD_COUNT" -ge 13 ]]; then
  ok "13+ GDD sections ($GDD_COUNT)"
else
  ko "expected 13 GDD sections, found $GDD_COUNT"
fi
USER_STORY_COUNT=$(grep -h '^### US-' "$GAME_DIR/docs/03-user-stories"/*.md 2>/dev/null | wc -l | tr -d ' ')
if [[ "$USER_STORY_COUNT" -ge 60 ]]; then
  ok "$USER_STORY_COUNT user stories (target ≥ 60)"
else
  ko "expected 60+ user stories, found $USER_STORY_COUNT"
fi
WIREFRAME_COUNT=$(find "$GAME_DIR/docs/05-wireframes" -name "*.html" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$WIREFRAME_COUNT" -ge 15 ]]; then
  ok "$WIREFRAME_COUNT HTML wireframes (target ≥ 15)"
else
  ko "expected 15+ wireframes, found $WIREFRAME_COUNT"
fi
FLOW_COUNT=$(find "$GAME_DIR/docs/04-ux-flows" -name "*.md" -not -name "README.md" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$FLOW_COUNT" -ge 5 ]]; then
  ok "$FLOW_COUNT UX flows (target ≥ 5)"
else
  ko "expected 5+ UX flows, found $FLOW_COUNT"
fi

section "Phase 3 (Tech Architecture) exit criteria"
TECH_SPEC_COUNT=$(find "$GAME_DIR/docs/06-tech-spec" -name "*.md" -not -name "README.md" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$TECH_SPEC_COUNT" -ge 11 ]]; then
  ok "$TECH_SPEC_COUNT tech-spec docs (target ≥ 11)"
else
  ko "expected 11+ tech-spec docs, found $TECH_SPEC_COUNT"
fi
ADR_COUNT=$(find "$GAME_DIR/docs/decisions" -name "[0-9]*.md" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$ADR_COUNT" -ge 8 ]]; then
  ok "$ADR_COUNT ADRs (target ≥ 8)"
else
  ko "expected 8+ ADRs, found $ADR_COUNT"
fi

section "balance data"
JSON_COUNT=$(find "$GAME_DIR/data/balance" -name "*.json" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$JSON_COUNT" -ge 4 ]]; then
  ok "$JSON_COUNT balance JSON sheets"
else
  ko "expected 4+ balance JSON sheets, found $JSON_COUNT"
fi
PARSE_FAILS=0
for f in "$GAME_DIR"/data/balance/*.json; do
  if [[ -f "$f" ]]; then
    python3 -c "import json; json.load(open('$f'))" 2>/dev/null || { ko "JSON parse fail: $(basename $f)"; PARSE_FAILS=$((PARSE_FAILS + 1)); }
  fi
done
[[ "$PARSE_FAILS" -eq 0 ]] && ok "all balance JSONs parse cleanly"

if [[ -f "core/tools/balance-tools/validate_balance.py" ]]; then
  if python3 core/tools/balance-tools/validate_balance.py --game "$GAME" >/dev/null 2>&1; then
    ok "balance validator passes"
  else
    adv "balance validator advisories (run python3 core/tools/balance-tools/validate_balance.py --game $GAME for details)"
  fi
fi

section "level design"
WAVES_COUNT=$(find "$GAME_DIR/docs/09-level-design/01-biomes" -name "waves.json" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$WAVES_COUNT" -ge 1 ]]; then
  ok "$WAVES_COUNT waves.json file(s)"
else
  ko "no waves.json found"
fi
while IFS= read -r -d '' f; do
  python3 -c "import json; json.load(open('$f'))" 2>/dev/null && ok "$(echo "$f" | sed "s|$GAME_DIR/||") parses" || ko "$f parse fail"
done < <(find "$GAME_DIR/docs/09-level-design/01-biomes" -name "waves.json" -print0 2>/dev/null)

section "asset licensing"
if [[ -f "core/tools/asset-pipeline/licenses.py" ]]; then
  if python3 core/tools/asset-pipeline/licenses.py --validate --game "$GAME" >/dev/null 2>&1; then
    ASSET_COUNT=$(python3 core/tools/asset-pipeline/licenses.py --validate --game "$GAME" 2>&1 | grep -oE '[0-9]+ files' | head -1 | grep -oE '[0-9]+' || echo 0)
    ok "license validator passes ($ASSET_COUNT files licensed)"
  else
    ko "license validator FAILED — run with --validate for details"
  fi
fi

section "unity scaffolding"
if [[ -d "$GAME_DIR/unity/Assets/_Brave/Code" ]]; then
  CS_COUNT=$(find "$GAME_DIR/unity/Assets/_Brave/Code" -name "*.cs" 2>/dev/null | wc -l | tr -d ' ')
  ASMDEF_COUNT=$(find "$GAME_DIR/unity/Assets/_Brave/Code" -name "*.asmdef" 2>/dev/null | wc -l | tr -d ' ')
  ok "$CS_COUNT C# files, $ASMDEF_COUNT asmdefs"
  if [[ "$ASMDEF_COUNT" -ge 6 ]]; then
    ok "6+ asmdefs (target met)"
  else
    adv "expected 6 asmdefs, found $ASMDEF_COUNT"
  fi
  # Cross-asmdef rule check: Gameplay must NOT reference UI.
  if grep -q '"Brave.UI"' "$GAME_DIR/unity/Assets/_Brave/Code/Gameplay"/*.asmdef 2>/dev/null; then
    ko "Brave.Gameplay illegally references Brave.UI"
  else
    ok "Brave.Gameplay does NOT reference Brave.UI (one-way dep rule)"
  fi
else
  adv "unity/ directory empty (Phase 5 not started)"
fi

section "summary for $GAME"
echo "  passed:     $pass"
echo "  failed:     $fail"
echo "  advisories: $advisory"
if [[ "$fail" -gt 0 ]]; then
  echo "[verify-game] FAILED for $GAME"
  exit 1
fi
echo "[verify-game] OK — $GAME verified"
exit 0
