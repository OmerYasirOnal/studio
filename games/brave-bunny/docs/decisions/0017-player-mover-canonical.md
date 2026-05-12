# ADR 0017 — PlayerMover is canonical; deprecate legacy XY-plane movers

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing Wave 1 dispatch findings)

## Context

Phase 5 Wave 1's gameplay-engineer dispatch surfaced a pre-existing
duplicate-implementation in `unity/Assets/_Brave/Code/Gameplay/Movement/`:

- `PlayerMover.cs` — newly rewritten, XZ-plane movement, matches the Run
  scene's top-down 3/4 camera (`SceneSetup.cs` positions camera at
  `(0, 14.7, -10.4)` rotated `(55, 0, 0)`). Reads speed from
  `CharacterDefinition.baseStats.baseMoveSpeed`. Allocation-free Update.
  9 EditMode tests pass.
- `PlayerController.cs` — pre-existing scaffolding from the Phase-5 wave-1
  agent burst. Writes to `pos.x`/`pos.y` (XY-plane), inconsistent with the
  camera orientation. Likely an early sketch before the camera direction
  was finalised.
- `Mover.cs` — partial helper class with overlapping concerns.

Three movers in one directory is not "extension"; it's confusion. Future
agents will not know which to wire to the Player GameObject, and
`grep`-driven changes will hit the wrong file half the time.

## Decision

**`PlayerMover` is the canonical player-movement component** for the
vertical slice and forward. The other two are deleted.

Specifically:
- **Keep:** `Movement/PlayerMover.cs` + `Movement/PlayerMoverTests.cs`
- **Keep:** `Movement/IInputProvider.cs` (interface; unchanged)
- **Keep:** `Movement/VirtualJoystickInput.cs` after its `TODO(Phase 5)`
  Input-System wiring lands (next dispatch)
- **Delete:** `Movement/PlayerController.cs` (XY-plane, contradicts camera)
- **Delete:** `Movement/Mover.cs` (helper now redundant; PlayerMover.ComputeVelocity is the canonical math)

## Consequences

- One canonical mover. Future dispatches (UI binding, scene composition,
  perf tests) wire to a single component.
- Camera-coordinate consistency: XZ-plane movement now matches the only
  rendered camera in the project; no risk of "movement looks wrong"
  surprises during the first PlayMode stress test.
- Loss: any unfinished work that was sketched in `PlayerController.cs` is
  gone. The wave-1 burst comment in `gameplay-engineer-20260512-191327.md`
  notes it had no real callers — safe to remove.

## Resolution criteria (when to close this ADR)

- `git ls-files unity/Assets/_Brave/Code/Gameplay/Movement/` does not
  list `PlayerController.cs` or `Mover.cs`.
- `verify-game.sh --game brave-bunny` is still green after deletion.
- All EditMode tests pass.

## Alternatives considered

1. **Keep `PlayerController` as a "future XY-plane camera mode"** —
   rejected. We have no XY camera mode planned in any GDD / tech spec.
   YAGNI; speculative code rots into ignored code.
2. **Merge `PlayerController` features into `PlayerMover`** — rejected.
   `PlayerController` has nothing `PlayerMover` lacks (no joystick
   integration, no SO-driven speed, no per-frame allocation discipline).
   Nothing to merge.
3. **Rename `PlayerMover` to `PlayerController` for "Unity-conventional"
   naming** — rejected. Unity's recommended verb-noun naming for
   "thing that moves the player" is `PlayerMover` /
   `CharacterController2D` / etc., not `Controller` (which collides with
   `PlayerInput`, `CharacterController`, `AnimController`).

## References

- `games/brave-bunny/docs/handoffs/gameplay-engineer-20260512-191327.md`
  (gaps section)
- `games/brave-bunny/unity/Assets/_Brave/Code/Gameplay/Movement/PlayerMover.cs`
- `games/brave-bunny/unity/Assets/Editor/SceneSetup.cs` (camera setup)
- `docs/decisions/0009-polymorphic-mechanics-registry.md` (constrains how
  movement variants would be added in the future — via registry, not via
  parallel classes)
