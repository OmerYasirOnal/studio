# GitHub Actions workflows — Studio

This directory hosts all CI/CD workflows for the Studio framework and its active
game (`brave-bunny`). Workflows are split into two families:

- **Framework workflows** (`ci.yml`, `observer-smoke.yml`) — gate the engine
  under `core/` and the observer dashboard. Triggered on every PR.
- **Game workflows** (`bb-*.yml`) — gate Brave Bunny under
  `games/brave-bunny/`. A mix of PR-triggered and scheduled jobs.

Game workflows are prefixed `bb-` so a future second game can coexist without
filename collisions. All game-specific workflow logic lives here at the repo
root because GitHub Actions only discovers workflows under `.github/workflows/`
— but ownership is still build-engineer's, with mirror copies kept under
`games/brave-bunny/tools/ci/github-actions/` for code-review and ADR reference.

## Active workflows

| File | Trigger | Runner | Owner | Purpose |
|---|---|---|---|---|
| `ci.yml` | push / PR | `ubuntu-latest` | framework | Lint Python / shell / markdown across the repo; asset-license validation per game |
| `observer-smoke.yml` | push / PR (when observer changes) | `ubuntu-latest` | framework | Boot the FastAPI observer dashboard and curl `/health` |
| `bb-lint.yml` | push to `main` / PR (paths) | `ubuntu-latest` | brave-bunny | Brave-Bunny-specific lint: balance JSON schema, asset manifest, markdown, shell |
| `bb-unity-test.yml` | PR (paths) | `macos-14` | brave-bunny | Unity EditMode + PlayMode test runner via `game-ci/unity-test-runner` |
| `bb-ios-build.yml` | manual / PR label `build:ios` | `macos-14` | brave-bunny | Full Unity → fastlane iOS build. Lanes: `preview`, `beta` |
| `bb-nightly-tests.yml` | **schedule** 03:00 UTC daily | `macos-14` | brave-bunny | EditMode regression on `main`; opens `ci-failure` issue on failure |
| `bb-weekly-ios-smoke.yml` | **schedule** 04:00 UTC Mondays | `macos-14` | brave-bunny | Full fastlane `preview` lane to keep the iOS toolchain warm (no TestFlight upload) |
| `bb-dependency-audit.yml` | **schedule** 05:00 UTC Sundays | `ubuntu-latest` | brave-bunny | Scan `Packages/manifest.json` for outdated UPM packages; open a draft PR with bumps |

## Cost awareness — macOS mac-minutes

`macos-14` runners cost **10x** the Linux multiplier against the GitHub Free tier
budget (2,000 min/month for private repos → ~200 mac-min/month). Current
scheduled mac-minute consumption:

| Workflow | Cadence | Avg run (min) | Mac-min / month |
|---|---|---:|---:|
| `bb-nightly-tests.yml` | daily | 25 | ~750 |
| `bb-weekly-ios-smoke.yml` | weekly | 40 | ~160 |
| `bb-unity-test.yml` (PRs) | per-PR | 25 | variable |
| `bb-ios-build.yml` | manual | 40 | variable |

> **WARNING** — The nightly EditMode run alone exceeds the 200-mac-min budget.
> If billing becomes an issue, either (a) move EditMode to a self-hosted Linux
> runner with a Unity license, or (b) drop the cadence to 3x/week (Mon/Wed/Fri).
> Tracked in `games/brave-bunny/docs/handoffs/wave11-automation.md`.

## Failure notifications

Scheduled workflows that fail open (or refresh) a GitHub issue with:

- Label: `ci-failure`
- Assignee: `OmerYasirOnal`
- Title prefix: `[ci-failure]`

No Slack / Discord webhook is configured yet — that is a deliberate Wave 11
TODO documented in the handoff. If/when a webhook is added, drop a step into
each scheduled workflow's `if: failure()` branch.

## Cron schedule (UTC)

```
0 3 * * *   bb-nightly-tests       (daily, 03:00 UTC = 06:00 Istanbul)
0 4 * * 1   bb-weekly-ios-smoke    (Mondays, 04:00 UTC)
0 5 * * 0   bb-dependency-audit    (Sundays, 05:00 UTC)
```

Times are staggered so they don't fight for the macOS runner queue or for the
brave-bunny `Library/` cache key.

## Adding a new workflow

1. File name: `bb-<short-name>.yml` for game-specific, otherwise the framework
   name without a prefix.
2. Always set `concurrency.group` to avoid overlapping runs.
3. Always set `timeout-minutes` — protects the mac-minutes budget on stuck
   runners.
4. Add the row to the table above in the same PR. Workflows that aren't
   documented here are considered orphaned and may be removed.
5. If scheduled, update the cron table above too.

## Manual trigger

All scheduled workflows expose `workflow_dispatch`:

```bash
gh workflow run bb-nightly-tests.yml
gh workflow run bb-weekly-ios-smoke.yml
gh workflow run bb-dependency-audit.yml
```

Useful for verifying the pipeline itself without waiting for the cron tick.
