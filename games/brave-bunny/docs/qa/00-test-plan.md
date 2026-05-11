# QA 00 — Test Plan

> Owner: qa-engineer. Sister docs: `01-manual-checklist.md`, `02-bug-triage.md`, `03-device-matrix.md`, `04-known-issues.md`. Cross-references: `docs/06-tech-spec/03-save-system.md`, `docs/06-tech-spec/05-performance-budget.md`, `decisions/0008-save-format-newtonsoft-json.md`, `decisions/0009-polymorphic-mechanics-registry.md`.

## Strategy

Brave Bunny ships with a three-tier QA strategy that maps cleanly to the soft-launch retention bet (TR / PH / ID, D1 ≥ 40%):

1. **Automated tests** — fast, deterministic, run on every PR. Catch regressions in damage math, save round-tripping, wave-table integrity, and mechanic-registry resolution before code review. ADR-0008 + ADR-0009 are enforced here.
2. **Manual QA sweep** — pre-release pass over `01-manual-checklist.md` on the device matrix (`03-device-matrix.md`) per build. Catches what automation cannot: feel, tone, copy, accessibility, monetization decline-friendliness.
3. **Live monitoring** — crash-free rate, FPS distribution, and key telemetry on soft-launch builds. Catches what dev-device QA cannot: regional-network corner cases, low-end thermal throttling, real player crash signatures.

## Test pyramid (target distribution by test count, not run-time)

| Tier | Share | Where | What |
|---|---|---|---|
| **EditMode** | 70% | `Assets/_Brave/Code/Tests/EditMode/` | Pure C# logic: damage formula, save serialization, mechanic registry, wave-table parsing, currency wallet, achievement progression, localization key resolution. |
| **PlayMode smoke** | 20% | `Assets/_Brave/Code/Tests/PlayMode/Smoke/` | Scene boot, service registration, run-start kill within 30s, level-up event firing. |
| **PlayMode performance** | 10% | `Assets/_Brave/Code/Tests/PlayMode/Performance/` | 200-enemy stress scene frame time, 100-projectile-burst GC alloc, p99 ≤ 16.67 ms. |

EditMode is the dominant tier because the gameplay layer is intentionally pure-C# (no MonoBehaviour `Update` per enemy — see `tech-spec/05-performance-budget.md` CPU bucket commentary).

## Coverage targets

- **DamageFormula**: 100% branch coverage. All clamp paths (defense, crit-rate, floor) tested.
- **SaveService**: round-trip + corruption + 4 backup-fallback paths + migration. Per ADR-0008 § Consequences.
- **MechanicRegistry**: every shipping `CharacterDefinition` asset's `signatureMechanicTypeName` resolves. Per ADR-0009 § Implementation.
- **WaveDefinition**: every shipping wave JSON respects 200-concurrent cap and triggers at non-negative times.
- **Other systems**: at least one test per public service method.

## Manual QA tier — device matrix + before-release sweep

Pre-release sweep runs on every TestFlight candidate. Manual checklist (`01-manual-checklist.md`) is 50+ items grouped into 8 sections; the QA-lead signs off in `04-known-issues.md`. The device matrix in `03-device-matrix.md` is iPhone-focused at launch; Android is deferred to v0.2.

## Live monitoring — soft-launch dashboards

Soft-launch (Phase 9) introduces Unity Cloud Diagnostics + custom telemetry. Live SLOs:

| Metric | Target | Alert threshold |
|---|---|---|
| Crash-free user rate | ≥ 99.5% | < 99.0% triggers P0 triage |
| FPS p99 (iPhone 12) | ≥ 60 fps | p99 < 50 fps triggers P1 perf-triage |
| Save-reset events / 1000 sessions | < 1 | > 5 triggers ADR-0008 audit |
| Rewarded-ad attach rate | track only (no target) | — |

Crash log triage: every reported `save_reset.log` file (per `03-save-system.md` corruption recovery) is investigated as a P2 minimum.

## How to run the tests locally

```bash
# Unity test runner — EditMode
unity-test-runner --platform EditMode --testFilter "Brave.Tests.EditMode" --logFile -

# PlayMode (requires graphics device; CI uses xvfb on Linux agents)
unity-test-runner --platform PlayMode --testFilter "Brave.Tests.PlayMode" --logFile -

# All
unity-test-runner --testFilter "Brave.Tests" --logFile artifacts/test-results.xml
```

CI hook: `tools/ci/test.sh` (build-engineer owns). Tests are blocking for PR merge.

## Test ownership

- **qa-engineer** owns this folder, the test files, and the manual checklists.
- **gameplay-engineer + systems-engineer** are CODEOWNERS for tests under their respective subfolders (`Tests/EditMode/Gameplay/`, `Tests/EditMode/Systems/`) — they are expected to add tests with their feature PRs.
- **build-engineer** owns the CI wiring (`tools/ci/`).

## Cross-references

- `01-manual-checklist.md` — pre-release sweep.
- `02-bug-triage.md` — severity SLA.
- `03-device-matrix.md` — supported devices.
- `04-known-issues.md` — living defect log.
- ADR-0008 — Newtonsoft save format invariants under test.
- ADR-0009 — mechanic registry invariants under test.
- `06-tech-spec/05-performance-budget.md` — iPhone 12 60-fps target enforced by Stress tests.
