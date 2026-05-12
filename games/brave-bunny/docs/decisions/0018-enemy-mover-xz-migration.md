# ADR 0018 — Enemy & AutoAttack migration to XZ-plane math (closes ADR-0017 deletion gap)

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing Wave 2 gameplay-engineer findings)

## Context

ADR-0017 declared `PlayerMover` canonical and authorised deletion of
`Movement/PlayerController.cs` and `Movement/Mover.cs`. Wave 2's
gameplay-engineer dispatch correctly **stopped before deleting** when it
discovered external callers:

| File | Calls | Plane today |
|---|---|---|
| `Code/Gameplay/Enemies/SwarmerBehavior.cs:16` | `Mover.Step(...)` | XY |
| `Code/Gameplay/Enemies/EliteBehavior.cs:16`   | `Mover.Step(...)` | XY |
| `Code/Gameplay/Enemies/RangedBehavior.cs:34`  | `Mover.Step(...)` | XY |
| `Code/Gameplay/Enemies/TankBehavior.cs:29`    | `Mover.Step(...)` | XY |
| `Code/Gameplay/Combat/AutoAttackController.cs:23` | `[SerializeField] PlayerController? player;` | XY |

The world's camera is **XZ-plane** (`SceneSetup.cs` positions camera at
`(0, 14.7, -10.4)` rotated `(55, 0, 0)`). `PlayerMover` is XZ. The
enemies and AutoAttack — pre-existing Wave-1-burst scaffolding — are XY.
Visual outcome on the Run scene: the player moves on the ground plane,
enemies move on the (invisible) vertical plane → enemies appear stuck on
a wall to the camera. This is the pre-existing inconsistency that the
ADR-0017 deletion would have exposed as a compile error.

## Decision

Migrate the 5 call-sites to XZ-plane semantics in a single coordinated
gameplay-engineer dispatch:

1. **Inline the `Mover.Step` math** at each enemy behaviour (per the
   gameplay-engineer's recommendation in `gameplay-engineer-20260512-192043.md`):
   the body is essentially `transform.position += dir.normalized * speed * dt`
   — 1-2 lines per call-site, with XZ semantics (`Vector3` over the
   ground plane, no Y-axis writes).
2. **Re-target `AutoAttackController`** from `PlayerController` to
   `PlayerMover` (the canonical mover). Read whatever properties the
   controller actually needs (`LastInput`/`Facing` per the existing
   field). If those properties don't exist on `PlayerMover`, the
   gameplay-engineer adds the minimal additions there (Wave-3 dispatch
   has explicit permission to extend `PlayerMover` for this).
3. **After all call-sites compile against the new path**, delete:
   - `Movement/PlayerController.cs` (+ `.meta`)
   - `Movement/Mover.cs` (+ `.meta`)
4. **No new "shared XZ helper" class** unless the gameplay-engineer
   surfaces a clear duplication (3+ identical bodies). YAGNI; inline is
   fine for 4 enemy behaviours.

## Consequences

- Camera-coordinate consistency across the entire Gameplay surface. The
  next time someone opens the Run scene, the player AND enemies move on
  the ground plane.
- `Movement/` directory ends with exactly four files: `PlayerMover.cs`,
  `IInputProvider.cs`, `VirtualJoystickInput.cs`, `PlayerMoverTests.cs`
  (counting tests for completeness). Plus the new
  `VirtualJoystickInputTests.cs`. Clean ownership.
- Mid-term: the enemy `Update` paths get the same allocation-free,
  no-LINQ discipline `PlayerMover` already enforces. If the migration
  reveals any LINQ / `new` in `Update`, gameplay-engineer's brief
  includes fixing them.
- ADR-0017's resolution criteria become reachable.

## Alternatives considered

1. **Port `Mover.Step` to XZ semantics in place** — rejected. Keeps a
   helper class around for ~12 net lines of math. The dispatch math is
   `pos += dir * speed * dt`; inline is clearer at every call-site.
2. **Build a generic `IMover` interface and dispatch behaviour-by-behaviour**
   — rejected. We don't have evidence of varied movement strategies
   warranting an interface; ADR-0009 already covers polymorphic
   mechanics for *abilities*, not for the trivial straight-line move.
3. **Defer the migration until Phase 6** — rejected. The 200-enemy
   stress scene is a Phase-5 exit criterion; it cannot stress-test
   correctly if enemies are on a different plane than the camera frame.

## Resolution criteria (when to close this ADR)

- `git ls-files Code/Gameplay/Movement/` returns: `PlayerMover.cs`,
  `IInputProvider.cs`, `VirtualJoystickInput.cs` (and tests in the
  Tests/ tree).
- `grep -r "Mover.Step\|PlayerController" Code/` returns no hits.
- `verify-game.sh --game brave-bunny` still green.
- EditMode + PlayMode tests still pass; any new tests added by the
  migration are documented in the dispatch hand-off.

## References

- ADR-0017 (parent decision)
- `docs/handoffs/gameplay-engineer-20260512-192043.md` (Part A
  surface — external callers)
- `unity/Assets/_Brave/Code/Gameplay/Movement/PlayerMover.cs`
  (the canonical mover the migration targets)
- `unity/Assets/Editor/SceneSetup.cs` (camera math reference for the
  XZ-plane convention)
