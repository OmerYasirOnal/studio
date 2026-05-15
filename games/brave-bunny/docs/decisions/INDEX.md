# Architecture Decision Records — Brave Bunny

ADRs are numbered sequentially, four-digit zero-padded. Use `/decide "<topic>"` from Claude Code to create one.

| # | Title | Date | Status |
|---|---|---|---|
| 0001 | Starter-weapon character binding | 2026-05-12 | accepted |
| 0002 | Toon shader source | 2026-05-12 | accepted |
| 0003 | Hitstop timings reconciliation | 2026-05-12 | accepted |
| 0004 | No voice acting at launch | 2026-05-12 | accepted |
| 0005 | Engine choice: Unity 6 LTS URP | 2026-05-12 | accepted |
| 0006 | Enemy HP recalibration | 2026-05-12 | superseded by 0022 (boss HP only) |
| 0007 | Evolution charm consumption | 2026-05-12 | accepted |
| 0008 | Save format: Newtonsoft JSON | 2026-05-12 | accepted |
| 0009 | Polymorphic mechanics via type-name registry | 2026-05-12 | accepted |
| 0010 | Subscription ROI policy | 2026-05-12 | accepted |
| 0011 | BGM loop format on iOS | — | proposed (deferred to Phase 5 device validation) |
| 0012 | Event channel mechanism (SO vs C# events) | — | proposed (deferred to Phase 5 Profiler) |
| 0013 | Arena spawn-radius invariant | 2026-05-12 | accepted |
| 0014 | Otter-Beaver fallback for the 8-character launch roster | 2026-05-12 | accepted |
| 0015 | Test/production API drift (temporarily disabled tests) | 2026-05-12 | accepted |
| 0016 | App Store display name: "Brave Bunny: Survivors" | 2026-05-12 | accepted |
| 0017 | PlayerMover canonical; deprecate legacy XY-plane movers | 2026-05-12 | accepted (partial — deletion gated on 0018) |
| 0018 | Enemy + AutoAttack XZ-plane migration (closes ADR-0017 deletion gap) | 2026-05-12 | accepted |
| 0019 | Wave 4 cleanup debt (cross-plane bug + 4 follow-ups) | 2026-05-12 | accepted |
| 0020 | Weapon archetype-config sidecar + `EnemyRole.Boss` enum value | 2026-05-12 | accepted |
| 0021 | Single canonical IRunRuntimeState + live HUD binding | 2026-05-13 | accepted |
| 0022 | Boss HP recalibration (Wave 7 vertical-slice TTK) | 2026-05-16 | accepted |

## Conventions

- File name: `NNNN-<short-slug>.md`
- Status values: `proposed` / `accepted` / `superseded by NNNN` / `rejected`
- Template sections: Context / Decision / Consequences / Alternatives considered / References
- Updates that revise a prior ADR get a new ADR with `Status: superseded by NNNN` on the old one
