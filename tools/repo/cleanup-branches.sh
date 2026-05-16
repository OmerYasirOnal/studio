#!/usr/bin/env bash
# Idempotent cleanup of stale worktrees, branches, and locked refs.
# Leaves: main, pivot/engine-three-r3f, plus the active checkout.

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

KEEP_BRANCHES=("main" "pivot/engine-three-r3f")

is_kept() {
  local b="$1"
  for k in "${KEEP_BRANCHES[@]}"; do
    [[ "$b" == "$k" ]] && return 0
  done
  return 1
}

echo "==> Step 1: Prune locked worktrees"
mapfile -t WORKTREES < <(git worktree list --porcelain | awk '/^worktree / { print $2 }' | grep -F "/.claude/worktrees/" || true)
for wt in "${WORKTREES[@]}"; do
  echo "  remove worktree: $wt"
  git worktree unlock "$wt" 2>/dev/null || true
  git worktree remove --force "$wt" 2>/dev/null || true
done
git worktree prune

echo "==> Step 2: Delete stale local branches"
mapfile -t LOCAL < <(git for-each-ref --format='%(refname:short)' refs/heads/)
for b in "${LOCAL[@]}"; do
  if is_kept "$b"; then
    echo "  keep: $b"
    continue
  fi
  echo "  delete local: $b"
  git branch -D "$b" 2>/dev/null || true
done

echo "==> Step 3: Delete stale remote branches"
mapfile -t REMOTE < <(git for-each-ref --format='%(refname:short)' refs/remotes/origin/ \
                        | sed 's|^origin/||' \
                        | grep -v '^HEAD$' || true)
for b in "${REMOTE[@]}"; do
  if is_kept "$b"; then continue; fi
  echo "  delete remote: origin/$b"
  git push origin --delete "$b" 2>/dev/null || true
done

echo "==> Step 4: Final state"
git worktree list
git branch -a

echo "==> Cleanup complete."
