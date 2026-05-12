#!/usr/bin/env bash
# verify-secrets.sh — verify all required GitHub Actions secrets are set on the
# studio repo. Run before triggering the first iOS build to fail-fast on missing secrets.

set -uo pipefail

REPO="${REPO:-OmerYasirOnal/studio}"

required=(
  "MATCH_PASSWORD"
  "ASC_KEY_ID"
  "ASC_ISSUER_ID"
  "ASC_KEY_P8"
  "MATCH_GIT_BASIC_AUTHORIZATION"
)

optional=(
  "UNITY_LICENSE"
  "UNITY_EMAIL"
  "UNITY_PASSWORD"
)

if ! command -v gh >/dev/null 2>&1; then
  echo "[verify-secrets] gh CLI not installed — run: brew install gh" >&2
  exit 2
fi

if ! gh auth status >/dev/null 2>&1; then
  echo "[verify-secrets] not authenticated — run: gh auth login" >&2
  exit 2
fi

echo "=== Verifying secrets on $REPO ==="
existing="$(gh secret list --repo "$REPO" 2>&1 | awk '{print $1}')"

missing=()
present=()
for s in "${required[@]}"; do
  if echo "$existing" | grep -qx "$s"; then
    present+=("$s")
  else
    missing+=("$s")
  fi
done

echo ""
echo "Required (build will fail without these):"
for s in "${required[@]}"; do
  if echo "$existing" | grep -qx "$s"; then
    echo "  ✓ $s"
  else
    echo "  ✗ $s    -- MISSING" >&2
  fi
done

echo ""
echo "Optional (Unity license — only needed for CI builds):"
for s in "${optional[@]}"; do
  if echo "$existing" | grep -qx "$s"; then
    echo "  ✓ $s"
  else
    echo "  · $s    -- not set (Unity build CI step will fall back to a different auth path)"
  fi
done

echo ""
if [[ ${#missing[@]} -eq 0 ]]; then
  echo "[verify-secrets] OK — all required secrets present"
  exit 0
fi

echo "[verify-secrets] MISSING ${#missing[@]} required secret(s)"
echo ""
echo "To set them (commands assume you have the values locally):"
for s in "${missing[@]}"; do
  case "$s" in
    MATCH_PASSWORD)
      echo "  gh secret set $s --repo $REPO --body \"\$(cat /tmp/match_password.txt 2>/dev/null || pbpaste)\""
      ;;
    ASC_KEY_ID)
      echo "  gh secret set $s --repo $REPO --body \"93HFBMV3MA\""
      ;;
    ASC_ISSUER_ID)
      echo "  gh secret set $s --repo $REPO --body \"3894e346-c886-4ca5-91b7-773aaa6e85bd\""
      ;;
    ASC_KEY_P8)
      echo "  gh secret set $s --repo $REPO < ~/.appstoreconnect/private_keys/AuthKey_93HFBMV3MA.p8"
      ;;
    MATCH_GIT_BASIC_AUTHORIZATION)
      echo "  gh secret set $s --repo $REPO --body \"\$(printf 'OmerYasirOnal:%s' \"\$(gh auth token)\" | base64)\""
      ;;
  esac
done
exit 1
