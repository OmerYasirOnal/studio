# Handoff — hud-reconcile-engineer — 2026-05-13T12:00:00Z

## Task completed
Reconciled dual `IRunRuntimeState` definitions into one canonical interface.

## Changes made

| File | Action |
|---|---|
| `UI/Bindings/IRunRuntimeState.cs` | Extended with `event Action StateChanged`, `CurrentHpNormalized`, `XpPoints`, `KillCount`, `Paused` |
| `UI/Bindings/RunHudStubRuntime.cs` | Implements new fields; `StateChanged` is no-op on stub |
| `UI/Controllers/RunHudController.cs` | Added `BindState()` + `OnStateChanged()`; fallback polling preserved |
| `Gameplay/Run/RunController.cs` | Implements `IRunRuntimeState`; mutators raise `StateChanged` |
| `Tests/EditMode/UI/IRunRuntimeStateTests.cs` | New — contract tests (canonical field names) |
| `Tests/EditMode/UI/RunHudBindingTests.cs` | New — BindState subscription contract tests |
| `docs/decisions/0021-hud-binding-contract.md` | ADR written |

## Key decisions (see ADR-0021)
- One interface, one namespace (`Brave.UI.Bindings`). No Gameplay-layer duplicate.
- `Render()` signature unchanged — all Wave-5 tests unmodified.
- `Update()` polling kept as fallback for stub/editor mode.

## Outstanding items
- `RunHudController` `_runStartTime` is set but not read in fallback path (timer comes from stub). Low priority — tracked in code comment.
- Wave-6 deferred branch `worktree-agent-ad5bee576529346e8` can be deleted after this merges.
