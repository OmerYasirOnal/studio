# Version-bump runbook — Brave Bunny SemVer

> Owner: build-engineer. Cross-ref: tech-spec `10-build-and-ci.md` ("Versioning").
>
> Marketing version (`CFBundleShortVersionString`) is driven by `GAME.md`.
> Build number (`CFBundleVersion`) is driven by `GITHUB_RUN_NUMBER` automatically
> on CI — no manual bump needed for build numbers.

## When to bump

| Change type | Bump | Example |
|---|---|---|
| Bug fix, balance tweak | patch | 0.1.0 → 0.1.1 |
| New feature, content drop | minor | 0.1.1 → 0.2.0 |
| Breaking save format, store re-cert | major | 0.9.0 → 1.0.0 |

Vertical-slice ships as `0.1.0`. v1 launch is `1.0.0` (per tech-spec 10).

## Procedure

1. **Edit `GAME.md`** — add or update the `semver:` line in the frontmatter
   (if missing, add `semver: "0.1.0"` to the YAML block). This file is the
   single source of truth — `IOSBuilder` reads it at build time.

   ```yaml
   semver: "0.1.1"
   ```

2. **Update `CHANGELOG.md`** at the game root (`games/brave-bunny/CHANGELOG.md`).
   TestFlight changelog is fetched from this file by the `beta` lane.

3. **Commit the bump** as its own commit, conventional-commit prefixed:

   ```bash
   git add games/brave-bunny/GAME.md games/brave-bunny/CHANGELOG.md
   git commit -m "chore(brave-bunny): bump to v0.1.1"
   ```

4. **Tag the commit** with the matching SemVer tag:

   ```bash
   git tag -a brave-bunny-v0.1.1 -m "Brave Bunny 0.1.1"
   git push origin main brave-bunny-v0.1.1
   ```

   The `brave-bunny-` prefix matters — the studio monorepo will eventually host
   multiple games, so tags must be game-scoped.

5. **Trigger the build** via GitHub Actions:

   - Actions tab → `ios-build` workflow → Run workflow.
   - Choose `lane: beta` for TestFlight, or `lane: preview` for a smoke build.
   - The CI run number flows into `CFBundleVersion` automatically.

6. **Downgrade guard** — CI rejects any merge to `main` that decreases the
   SemVer (guard owned by build-engineer; lives in `lint.yml` once implemented).
   If you need to downgrade for any reason, surface to the human first.
