#!/usr/bin/env bash
# pre-bash-guard.sh — refuse to run commands that look dangerous, unless the agent
# overrode by writing "STUDIO_ALLOW_RISKY=1" to the environment.
# Receives the command string on stdin.

set -uo pipefail

CMD="$(cat || true)"
[[ -z "$CMD" ]] && exit 0

if [[ "${STUDIO_ALLOW_RISKY:-0}" == "1" ]]; then
  exit 0
fi

# Hard-no patterns.
deny_patterns=(
  'rm[[:space:]]+-rf[[:space:]]+/(\s|$)'
  'rm[[:space:]]+-rf[[:space:]]+\$HOME'
  'rm[[:space:]]+-rf[[:space:]]+~'
  'mkfs\.'
  'dd[[:space:]]+if=.*of=/dev/'
  ':\(\)\{.*\&\};:'      # fork bomb
  'curl[[:space:]]+[^|]*\|[[:space:]]*sh'
  'wget[[:space:]]+[^|]*\|[[:space:]]*sh'
  'git[[:space:]]+push[[:space:]]+--force[[:space:]]+.*main'
  'git[[:space:]]+config[[:space:]]+.*user\.'
)

for p in "${deny_patterns[@]}"; do
  if echo "$CMD" | grep -E -q -- "$p"; then
    echo "[pre-bash-guard] BLOCKED pattern: $p" >&2
    echo "[pre-bash-guard] command: $CMD" >&2
    exit 1
  fi
done

# API-key smell.
if echo "$CMD" | grep -E -i -q -- 'OPENAI_API_KEY|ANTHROPIC_API_KEY|ELEVENLABS_API|REPLICATE_API_TOKEN|HF_TOKEN'; then
  echo "[pre-bash-guard] BLOCKED: external paid AI API key referenced — forbidden by framework policy" >&2
  exit 1
fi

exit 0
