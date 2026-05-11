# Per-game CI templates

These YAML files are this game's reference CI workflows. **GitHub Actions only fires from `.github/workflows/` at the repo root** — they are activated by copying to that location with a game-slug prefix:

```bash
for f in tools/ci/github-actions/*.yml; do
  cp "$f" "../../.github/workflows/bb-$(basename $f)"
done
```

(Replace `bb-` with your game's slug.)

The active versions for brave-bunny are at:
- `.github/workflows/bb-ios-build.yml`
- `.github/workflows/bb-unity-test.yml`
- `.github/workflows/bb-lint.yml`

Keep these source templates and the active workflows in sync. When you modify a template here, run the copy command above and commit both. (CI: future improvement — pre-commit hook that diffs the two locations.)
