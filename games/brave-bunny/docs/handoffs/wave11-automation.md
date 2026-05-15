# Wave 11 — Scheduled CI automation handoff

**Agent:** build-engineer
**Date:** 2026-05-16
**Branch:** `worktree-agent-a1d28a936eb131fa2`

## Summary

Adds three scheduled GitHub Actions workflows for Brave Bunny:

1. `bb-nightly-tests.yml` — nightly EditMode regression on `main`
2. `bb-weekly-ios-smoke.yml` — weekly fastlane preview build (no upload)
3. `bb-dependency-audit.yml` — weekly Unity Package Manager bump PR

Plus supporting infrastructure: `nightly-test-report.sh` (NUnit → markdown
formatter) and `.github/workflows/README.md` (workflow inventory).

## Schedule (UTC)

| Workflow | Cron | Local (Istanbul UTC+3) |
|---|---|---|
| `bb-nightly-tests` | `0 3 * * *` (daily) | 06:00 every morning |
| `bb-weekly-ios-smoke` | `0 4 * * 1` (Mondays) | 07:00 Monday |
| `bb-dependency-audit` | `0 5 * * 0` (Sundays) | 08:00 Sunday |

Staggered intentionally so consecutive scheduled jobs don't fight for the
single `macos-14` runner concurrency slot.

## Where notifications go

- **GitHub Issues** — Failures open / refresh an issue labelled `ci-failure`
  and assigned to `OmerYasirOnal`. Each workflow reuses an existing open
  issue (commenting on it) rather than filing a fresh one, to avoid noise.
- **GitHub Step Summary** — Every run writes a markdown summary visible in
  the Actions UI. Useful for quickly skimming nightly test counts.
- **Artifacts** — NUnit XML + Unity logs kept for 14 days.
- **Slack / Discord** — _Not yet configured._ TODO below.

## Where each piece lives

```
.github/workflows/
  bb-nightly-tests.yml          ← cron 03:00 UTC daily
  bb-weekly-ios-smoke.yml       ← cron 04:00 UTC Mondays
  bb-dependency-audit.yml       ← cron 05:00 UTC Sundays
  README.md                     ← workflow inventory

games/brave-bunny/tools/ci/scripts/
  nightly-test-report.sh        ← NUnit XML → markdown (called from nightly)
```

## Required GitHub Actions secrets

The nightly and weekly-iOS workflows reuse the same secrets the existing
`bb-ios-build.yml` and `bb-unity-test.yml` workflows already depend on:

- `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` — game-ci runner activation
- `MATCH_PASSWORD`, `MATCH_GIT_AUTHOR` — fastlane match cert decryption
- `FASTLANE_TEAM_ID` — Apple developer team (only used by smoke preview)

The dependency audit needs no secrets — it only reads the public UPM registry
and uses the auto-provided `GITHUB_TOKEN` to open the PR.

Verify with `games/brave-bunny/tools/ci/scripts/verify-secrets.sh`.

## Failure playbook

### Nightly EditMode fails

1. Check the open `[ci-failure] Nightly EditMode tests failed` issue — it has
   the formatted failure list + run link.
2. Download the `nightly-editmode-results-<run>` artifact for full NUnit XML
   (useful when there are more than 20 failures).
3. Reproduce locally:
   `Unity -batchmode -projectPath games/brave-bunny/unity -runTests -testPlatform editmode`
4. Fix forward via a PR. The nightly issue stays open and accrues new
   comments until a passing run closes it — _close it manually_ once the
   next run is green; we don't auto-close to keep an audit trail.

### Weekly iOS smoke fails

Most common causes, in historical order:

1. **Xcode bump on `macos-14`** — GitHub rolls Xcode versions on the runner.
   Check the runner image release notes; pin the Xcode version if needed.
2. **Unity package drift** — diff `Packages/manifest.json` against the last
   green commit.
3. **fastlane / cocoapods gem yank** — refresh `Gemfile.lock`.
4. **match cert expiry** — re-run `bundle exec fastlane match appstore` locally
   to refresh, then commit the cert repo bump.

### Dependency audit opens a noisy PR

Edit the PR description manually, drop the bumps you don't want, and merge a
subset. The workflow uses `peter-evans/create-pull-request` with
`delete-branch: true` so closing the PR cleans up the branch automatically.

### Cron didn't fire

GitHub Actions disables scheduled workflows on repos with no activity for 60
days. If we see nothing in the run history, either (a) push any commit to
`main`, or (b) trigger the workflow manually once:

```bash
gh workflow run bb-nightly-tests.yml
gh workflow run bb-weekly-ios-smoke.yml
gh workflow run bb-dependency-audit.yml
```

## Cost / budget watch

`bb-nightly-tests` alone (~25 min × 30 days × 10x mac multiplier = ~750
mac-min/month) is **over** the GitHub Free 200-mac-min budget. Options when
we hit the wall:

1. Drop nightly cadence to 3x/week (Mon/Wed/Fri).
2. Move EditMode runs to a self-hosted Linux runner with Unity headless
   license.
3. Pay for additional minutes (last resort — Wave 12 conversation).

Tracked here, not yet acted on.

## Out of scope this wave (TODOs)

- **Slack / Discord notifications** — no webhook configured. When added,
  insert into each workflow's `if: failure()` branch as a final step.
- **PlayMode in nightly** — kept EditMode-only to fit the timebox; PlayMode
  stays on the PR-triggered `bb-unity-test.yml`.
- **Crash report aggregation** — separate concern; not part of this wave.
- **Production deployment automation** — `bb-ios-build.yml` already covers
  manual + label-gated TestFlight uploads.

## Verification

- All three YAML files validated with `pyyaml.safe_load`.
- `nightly-test-report.sh` smoke-tested against a synthetic NUnit XML fixture
  (passing case, failing case, empty-artifacts case) — output matches the
  documented markdown shape.
- `bash -n` clean. Compatible with macOS bash 3.2 (no `mapfile`).

## Files touched

```
.github/workflows/bb-nightly-tests.yml          (new)
.github/workflows/bb-weekly-ios-smoke.yml       (new)
.github/workflows/bb-dependency-audit.yml       (new)
.github/workflows/README.md                     (new)
games/brave-bunny/tools/ci/scripts/nightly-test-report.sh  (new, +x)
games/brave-bunny/docs/handoffs/wave11-automation.md       (this file)
```
