#!/usr/bin/env bash
# post-edit-lint.sh — run after Edit/Write. Lints the modified file.
# Non-zero exit signals a problem worth fixing.

set -uo pipefail

TARGET="${1:-}"
[[ -z "$TARGET" || ! -f "$TARGET" ]] && exit 0

RC=0
case "$TARGET" in
  *.py)
    command -v ruff >/dev/null 2>&1 && { ruff check "$TARGET" || RC=$? ; }
    ;;
  *.sh)
    command -v shellcheck >/dev/null 2>&1 && { shellcheck -x -S warning "$TARGET" || RC=$? ; }
    ;;
  *.md)
    command -v markdownlint >/dev/null 2>&1 && { markdownlint "$TARGET" || RC=$? ; }
    ;;
  *.json)
    command -v jq >/dev/null 2>&1 && { jq -e . "$TARGET" >/dev/null || RC=$? ; }
    ;;
  *.yml|*.yaml)
    command -v yamllint >/dev/null 2>&1 && { yamllint -d relaxed "$TARGET" || RC=$? ; }
    ;;
esac

exit $RC
