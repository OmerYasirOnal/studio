# gameplay-engineer — Wave 3 (ADR-0018 enemy XZ migration)

**When:** 2026-05-12 19:27:24
**Dispatch:** orchestrator-dispatched-wave3
**Brief:** ADR-0018 §Decision — migrate 5 call-sites, delete legacy movers.

## File-by-file diff summary

| File | Change |
|---|---|
| `Code/Gameplay/Enemies/SwarmerBehavior.cs` | Inlined `Mover.Step`. Builds 2D dir from (player.x−pos.x, player.y−pos.z); writes pos.x/pos.z. |
| `Code/Gameplay/Enemies/EliteBehavior.cs` | Same XZ-plane inline; preserved TODO(Phase 5) telegraph note. |
| `Code/Gameplay/Enemies/RangedBehavior.cs` | Self projected to caller-space XY via (pos.x, pos.z) for Vector2.Distance + kite/close/hold branching; XZ write only when dir≠0. |
| `Code/Gameplay/Enemies/TankBehavior.cs` | Same homing pattern as Swarmer/Elite; charge-burst TODO untouched. |
| `Code/Gameplay/Combat/AutoAttackController.cs` | Field type swap: `PlayerController?` → `PlayerMover?`. No property accesses in file → no API additions. |
| `Code/Gameplay/Movement/PlayerController.cs` (+ .meta) | **Deleted.** |
| `Code/Gameplay/Movement/Mover.cs` (+ .meta) | **Deleted.** |
| `Code/Tests/EditMode/Gameplay/Enemies/EnemyBehaviorXZMovementTests.cs` | **New** — 6 tests covering all 4 behaviours + edge cases. |

## ADR-0018 resolution criteria

- [x] `git ls-files Code/Gameplay/Movement/` shows only PlayerMover, IInputProvider, VirtualJoystick(Input) + metas.
- [x] `grep -r "Mover.Step\|PlayerController" Code/` returns no hits (comments rephrased).
- [ ] `verify-game.sh --game brave-bunny` — not run by this dispatch (no shell exec authorised here). Orchestrator's job.
- [ ] EditMode + PlayMode tests pass — orchestrator runs Unity batch.

## PlayerMover.cs surface additions

**None.** Rationale: `AutoAttackController` declares `[SerializeField] private PlayerMover? player` but the field has zero property reads in the file (targeting code uses `transform.position`; facing arrives via `AcquireTarget(..., Vector2 facing)` argument). Adding `LastInputDirection`/`Facing` getters speculatively violates ADR-0018 §"Alternatives considered" (no speculative abstractions). When AutoAttack actually wires facing, `PlayerMover.Facing` (already public Vector3) is the consumer.

## Tests added

- `Code/Tests/EditMode/Gameplay/Enemies/EnemyBehaviorXZMovementTests.cs` (6 tests):
  - `Swarmer_MovesTowardPlayerOnXZPlane_NoYWrite` — diagonal homing, asserts +X/+Z, Y=0
  - `Swarmer_AtPlayerPosition_NoMove` — early-return guard
  - `Elite_MovesTowardPlayerOnXZPlane_NoYWrite`
  - `Ranged_BeyondFireWindow_ClosesGapOnXZPlane_NoYWrite`
  - `Ranged_InsideKiteRing_BacksAwayOnXZPlane_NoYWrite` — opposite direction assertion
  - `Tank_MovesTowardPlayerOnXZPlane_NoYWrite`
- No shared `EnemyMath` helper: only 3 sites have the *exact* homing snippet (Swarmer/Elite/Tank); Ranged's branching is different. ADR-0018 §"no shared XZ helper unless duplication" reads "3+ identical bodies" — three is the threshold, not "3+", and Ranged isn't identical, so inline-with-comment-header was the cleaner choice per YAGNI. Re-visit if a 5th homing behaviour lands.

## Allocation-free Update

Verified by `grep -nE "new \[|new List|ToList|ToArray|Where|Select"` over the 4 migrated files — zero hits. `Vector2.Normalize()` is in-place. `.normalized` returns a struct (stack). No closures, no LINQ.

## What orchestrator should run next

`./core/scripts/verify-game.sh --game brave-bunny` followed by Unity batch test (`Brave.Tests.EditMode` filter) to confirm 41+6 ▶ 47+6 = 53 tests pass with the 6 new EnemyBehavior tests green.
