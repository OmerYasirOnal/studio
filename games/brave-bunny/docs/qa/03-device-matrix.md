# QA 03 — Device Matrix

> Owner: qa-engineer. Sister docs: `00-test-plan.md`, `01-manual-checklist.md`. Cross-references: `docs/06-tech-spec/05-performance-budget.md` § Target devices, `brave-bunny/CLAUDE.md` perf contract.

## Launch-target devices (v1.0, iOS only)

| Device | Chip | Display | Role | Perf target |
|---|---|---|---|---|
| **iPhone 12** | A14 Bionic, 4 GB RAM | 2532×1170, 6.1" OLED | **Performance baseline.** Every test ladder is anchored here. | **60 fps** sustained with 200 enemies + 50 projectiles + 30 VFX puffs |
| **iPhone SE 3 (2022)** | A15 Bionic, 4 GB RAM | 1334×750, 4.7" LCD | Small-screen + safe-area smoke. Triggers the degrade plan per `05-performance-budget.md`. | 50-60 fps acceptable (degrade plan applied) |
| **iPhone 15 / 16** | A16 / A17 Pro | Modern, large | Latest-flagship smoke pass; should always exceed iPhone 12 perf. | Smoke pass only (boot → one run → run-end) |
| **iPad Air (M1)** | M1 | 2360×1640, 10.9" | Tablet + landscape smoke. Validates safe-area + UI scaling on a non-phone. | Smoke pass only |

## Android — DEFERRED to v0.2

Per `brave-bunny/CLAUDE.md` and `05-performance-budget.md`, Android support is **deferred to v0.2** to keep the soft-launch scope tight. Build-engineer will scaffold Android-CI in Phase 9; QA matrix above adds a Pixel-5-class baseline at that time.

## Soft-launch market device skew (TR / PH / ID)

- **TR**: iPhone-share is high (~25% of mobile gamers). iPhone 12 + SE 3 cover the bulk.
- **PH**: iPhone-share is lower; SE 3 over-represents in the upgrade cycle. SE 3 sweep is critical.
- **ID**: iPhone-share is low; iOS soft-launch covers premium players (Habby-fan Hakim persona). Family Fadia persona is iPad-relevant.

## Pre-release sweep coverage matrix

| Device | Boot | FTUE | Run loop | Run end | Meta | Monetization | Settings |
|---|---|---|---|---|---|---|---|
| iPhone 12 | full | full | full | full | full | full | full |
| iPhone SE 3 | full | full | full | full | full | full | full |
| iPhone 15/16 | smoke | smoke | smoke | smoke | — | — | — |
| iPad Air (M1) | smoke | smoke | smoke | smoke | — | — | — |

"full" = all checklist items in that section; "smoke" = first item only.

## Profiler capture cadence

- **iPhone 12**: weekly Xcode Instruments capture (GPU + Memory) starting Phase 5.
- **iPhone SE 3**: weekly capture; specifically verify the degrade plan is engaging.
- **Other devices**: ad-hoc.

## Cross-references

- `00-test-plan.md` — strategy + live-monitor SLOs.
- `01-manual-checklist.md` — what runs on each device.
- `05-performance-budget.md` — per-device fps + memory targets and the SE 3 degrade plan.
