#!/usr/bin/env bash
# verify-framework.sh — comprehensive smoke test for the studio framework.
# Runs lint/validation tools and reports pass/fail counts. Non-zero exit if
# any HARD check fails (parseable JSON, license enforcement, valid Python).
# Advisories are printed but don't fail the script.
#
# Usage:
#   ./core/scripts/verify-framework.sh
#   ./core/scripts/verify-framework.sh --game brave-bunny    # also run per-game checks
#   ./core/scripts/verify-framework.sh --strict              # advisory drift becomes hard fail

set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
cd "$ROOT" || exit 1

GAME=""
STRICT=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --game) GAME="$2"; shift 2 ;;
    --strict) STRICT=1; shift ;;
    --help|-h)
      sed -n '2,11p' "$0" | sed 's/^# //;s/^#//'
      exit 0
      ;;
    *) echo "[verify] unknown arg: $1" >&2; exit 1 ;;
  esac
done

pass=0
fail=0
advisory=0

ok() { echo "  ✓ $1"; pass=$((pass + 1)); }
ko() { echo "  ✗ $1" >&2; fail=$((fail + 1)); }
adv() { echo "  ⚠ $1"; advisory=$((advisory + 1)); }

section() {
  echo ""
  echo "=== $1 ==="
}

section "framework directory layout"
[[ -d core ]]        && ok "core/ exists"      || ko "core/ missing"
[[ -d games ]]       && ok "games/ exists"     || ko "games/ missing"
[[ -d shared ]]      && ok "shared/ exists"    || ko "shared/ missing"
[[ -d logs ]]        && ok "logs/ exists"      || ko "logs/ missing"
[[ -f core/VERSION ]] && ok "core/VERSION present ($(cat core/VERSION))" || ko "core/VERSION missing"
[[ -f CLAUDE.md ]]   && ok "root CLAUDE.md present" || ko "root CLAUDE.md missing"
[[ -f LICENSE ]]     && ok "LICENSE present"   || ko "LICENSE missing"

section "agent definitions (16 expected)"
AGENT_COUNT=$(find core/.claude/agents -name "*.md" | wc -l | tr -d ' ')
if [[ "$AGENT_COUNT" -eq 16 ]]; then
  ok "16 game-agnostic agent definitions present"
else
  ko "expected 16 agent definitions, found $AGENT_COUNT"
fi

section "slash commands (9 expected)"
CMD_COUNT=$(find core/.claude/commands -name "*.md" | wc -l | tr -d ' ')
if [[ "$CMD_COUNT" -eq 9 ]]; then
  ok "9 slash commands present"
else
  ko "expected 9 slash commands, found $CMD_COUNT"
fi

section "hooks (4 expected, all executable)"
for hook in pre-edit-format.sh post-edit-lint.sh pre-bash-guard.sh post-commit-log.sh; do
  if [[ -x "core/.claude/hooks/$hook" ]]; then
    ok "$hook executable"
  else
    ko "$hook missing or not executable"
  fi
done

section "MCP config"
if [[ -f core/.claude/mcp.json ]]; then
  if python3 -c "import json; d = json.load(open('core/.claude/mcp.json')); assert 'mcpServers' in d, 'missing mcpServers key'; print(f'  servers configured: {len(d[\"mcpServers\"])}')" 2>/dev/null; then
    ok "mcp.json valid"
  else
    ko "mcp.json invalid"
  fi
else
  ko "mcp.json missing"
fi

section "CLI scripts (5 expected, all executable)"
for script in new-game.sh spawn-agent.sh observer-start.sh ralph.sh status.sh; do
  if [[ -x "core/scripts/$script" ]]; then
    ok "$script executable"
  else
    ko "$script missing or not executable"
  fi
done

section "observer"
if [[ -d core/observer ]]; then
  [[ -f core/observer/server.py ]] && ok "server.py present" || ko "server.py missing"
  [[ -f core/observer/requirements.txt ]] && ok "requirements.txt present" || ko "requirements.txt missing"
  STATIC_COUNT=$(find core/observer/static -name "*.html" 2>/dev/null | wc -l | tr -d ' ')
  if [[ "$STATIC_COUNT" -ge 5 ]]; then
    ok "5+ HTML pages ($STATIC_COUNT)"
  else
    ko "expected 5+ HTML pages, found $STATIC_COUNT"
  fi
  # Liveness check (don't fail if not running).
  if curl -sS -o /dev/null -m 1 http://localhost:7777/health 2>/dev/null; then
    ok "observer running on localhost:7777"
  else
    adv "observer not running (start with: ./core/scripts/observer-start.sh)"
  fi
else
  ko "core/observer/ missing"
fi

section "game templates"
for tpl in action-roguelite endless-runner puzzle _common; do
  if [[ -d "core/templates/$tpl" ]]; then
    ok "template $tpl present"
  else
    ko "template $tpl missing"
  fi
done

section "asset pipeline tools"
for tool in licenses.py quaternius-fetch.py kenney-fetch.py freesound-fetch.py; do
  if [[ -f "core/tools/asset-pipeline/$tool" ]]; then
    if python3 -c "import ast; ast.parse(open('core/tools/asset-pipeline/$tool').read())" 2>/dev/null; then
      ok "$tool valid Python"
    else
      ko "$tool has syntax error"
    fi
  else
    ko "$tool missing"
  fi
done

section "documentation"
for doc in getting-started creating-a-game agent-system observability asset-policy architecture; do
  if [[ -f "core/docs/$doc.md" ]]; then
    ok "$doc.md present"
  else
    ko "$doc.md missing"
  fi
done

section "github actions"
WORKFLOW_COUNT=$(find .github/workflows -name "*.yml" 2>/dev/null | wc -l | tr -d ' ')
if [[ "$WORKFLOW_COUNT" -ge 2 ]]; then
  ok "$WORKFLOW_COUNT workflows present"
else
  ko "expected at least 2 workflows, found $WORKFLOW_COUNT"
fi

# Per-game checks
if [[ -n "$GAME" ]]; then
  "$SCRIPT_DIR/verify-game.sh" --game "$GAME" || fail=$((fail + 1))
fi

section "summary"
echo "  passed:     $pass"
echo "  failed:     $fail"
echo "  advisories: $advisory"
if [[ "$fail" -gt 0 ]]; then
  echo ""
  echo "[verify] FRAMEWORK VERIFICATION FAILED"
  exit 1
fi
if [[ "$STRICT" -eq 1 && "$advisory" -gt 0 ]]; then
  echo ""
  echo "[verify] strict mode: $advisory advisor(y/ies) treated as failure"
  exit 1
fi
echo ""
echo "[verify] OK — framework v$(cat core/VERSION) verified"
exit 0
