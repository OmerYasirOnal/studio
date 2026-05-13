# ADR-0021 — Single Canonical IRunRuntimeState + Live HUD Binding

**Date:** 2026-05-13  
**Status:** Accepted  
**Author:** hud-wire-reconciliation agent

## Context

Wave-5 shipped `UI/Bindings/IRunRuntimeState.cs` (8 polling properties, no events) consumed by `RunHudController.Render()`.

Wave-6 hud-wire branch created a duplicate `Gameplay/Run/IRunRuntimeState.cs` (6 properties + `event Action StateChanged`) in a different namespace. The merge conflicted; the branch was deferred.

## Decision

**Exactly one `IRunRuntimeState` interface** at `UI/Bindings/IRunRuntimeState.cs` in namespace `Brave.UI.Bindings`. It combines both contracts:

- All 8 Wave-5 properties preserved unchanged.
- Added: `CurrentHpNormalized` (computed), `XpPoints` (raw cumulative), `KillCount`, `Paused`, `event Action StateChanged`.

`RunController` now implements `IRunRuntimeState` and raises `StateChanged` in every mutator (`SetHp`, `AddXp`, `SetWave`, `RecordKill`, `Pause`, `Resume`, `LevelUp`).

`RunHudController` gains `BindState(IRunRuntimeState)` which subscribes to `StateChanged` and calls the existing `Render(state, _elements)`. Per-frame `Update()` polling is retained as a fallback when no state is bound (editor / stub mode).

## Consequences

- `Render()` signature unchanged — Wave-5 tests pass without modification.
- `RunHudStubRuntime` extended with the new fields; `StateChanged` is a no-op on the stub.
- Two new test files added: `IRunRuntimeStateTests.cs` + `RunHudBindingTests.cs`.
- No `Gameplay/Run/IRunRuntimeState.cs` file exists on main.
