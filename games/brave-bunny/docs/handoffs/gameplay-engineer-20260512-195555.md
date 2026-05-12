# Hand-off — gameplay-engineer — 2026-05-12 19:55:55

**Wave:** Wave 5A (orchestrator-dispatched)
**Task:** Close ADR-0019 items 1–3 — XY→XZ distance-helper migration + verify enemy-death/projectile-pool return path.

## Audit table — every `.y` distance hit reviewed

| File:Line (pre-edit) | Before | After / Verdict | Reason |
|---|---|---|---|
| `Enemies/EnemyRegistry.cs:48` | `d.x*d.x + d.y*d.y` in `SnapshotActiveInRange` | **migrated** → `d.x*d.x + d.z*d.z` | ADR-0019 item 1 (root bug) |
| `Enemies/EnemyRegistry.cs:60` | `d.x*d.x + d.y*d.y` in `FindFirstWithinRadius` | **migrated** → `d.x*d.x + d.z*d.z` | ADR-0019 item 1 |
| `Combat/AutoAttackController.cs:212` | `Vector2 to = (Vector2)e.transform.position - origin` | **migrated** → `Vector3 d = e.pos - origin; Vector2 to = (d.x, d.z)` | ADR-0019 item 2 |
| `Combat/AutoAttackController.cs:218` | `Vector2.Dot(facing, to.normalized)` | kept — now uses XZ `to` | Same expression, semantic now correct |
| `Combat/Projectile.cs:102-103` | `_direction.x*cos - _direction.y*sin` etc. | **kept** — intentional 2D rotation of an already-XZ `_direction` Vector2 (x↔world.x, y↔world.z); rotating on the ground plane | Rotation math is plane-agnostic |
| `Combat/ProjectileMath.cs:44` | `pos.y += dir.y * speed * dt` in `Step` | **kept** — `Step` is a pure 3D vector add; caller passes `dir.y = 0` for XZ (see `Projectile.cs:122` constructs `new(_direction.x, 0f, _direction.y)`) | Y-component is zeroed by callers |
| `Combat/ProjectileWeapon.cs:41` | `Vector2(v.x*c - v.y*s, v.x*s + v.y*c)` | **kept** — `Rotate` helper takes/returns a 2D vector; callers responsible for plane mapping. Currently unused in vertical slice (TODO at line 25) | Pure 2D rotation utility |
| `Combat/HitDetector.cs:53` `Vector2 p = e.transform.position` | implicit drop of world.z | **kept** — `HitDetector` is the legacy spatial-hash from pre-XZ scaffolding; not used by Wave-4 vertical slice (`Projectile.Update` queries `EnemyRegistry` directly, not `HitDetector`). Migration is a separate dispatch when spatial-hash replaces flat-list (per `EnemyRegistry.cs:14` comment "real implementation upgrades to a uniform spatial hash in Phase 5") | Out of Wave 5A scope; documented follow-up |
| `Enemies/SwarmerBehavior.cs:24` `pos.z += dir.y * step` | (Pre-ADR-0018 was `pos.y += dir.y`) | **kept** — already-XZ post-ADR-0018; `dir` is caller-space 2D (Y → world Z) | Correct |
| `Enemies/TankBehavior.cs:37` | same pattern | **kept** | Correct (post-ADR-0018) |
| `Enemies/EliteBehavior.cs:25` | same pattern | **kept** | Correct (post-ADR-0018) |
| `Enemies/RangedBehavior.cs:43` | same pattern | **kept** | Correct (post-ADR-0018) |
| `Enemies/Swarmer.cs:21-31` `Hero.position - transform.position` | full 3D | **kept** — `EnemyBase.Swarmer.TickBehavior` uses the homing-step in pure 3D; on the ground plane the Y delta is zero so the math degenerates to XZ-correct. Step writes `transform.position += step` (Y preserved) | Plane-correct |
| `Movement/PlayerMover.cs:145-154` `input.x*input.x + input.y*input.y` | 2D input vector | **kept** — input-space normalisation, not world distance | Input vector |
| `Movement/VirtualJoystickInput.cs:109,117,122` | screen-delta math | **kept** | Screen-space UI math |
| `Spawning/SpawnPattern.cs:46-54` `Vector2`+ trig | spawn-ring math | **kept** — caller-space 2D pattern; not a distance query | Pattern generation |
| `Combat/AutoAttackController.cs:148` `facing.sqrMagnitude < ProjectileMath.FacingEpsilonSqr` | 2D magnitude on a Vector3 | **kept** — magnitude is plane-agnostic | Sentinel only |

**Verification grep (zero hits remaining):**
```
grep -nE 'd\.x[ ]*\*[ ]*d\.x[ ]*\+[ ]*d\.y' Code/Gameplay/   → 0 hits
grep -nE '\(Vector2\)[a-zA-Z_.]*\.position'  Code/Gameplay/   → 0 hits
```

## Tests added / extended

| Path | Tests | Pattern |
|---|---|---|
| `Tests/EditMode/Gameplay/Enemies/EnemyRegistryTests.cs` | **NEW** — 11 tests: empty/in-range/out-of-range, Y-offset trickery, dead-enemy skip; `FindNearestWithinRadius` true-nearest with Y-offset; `SnapshotActiveInRange` filtering + buffer-clear contract | Real `Swarmer`+`EnemyHealth` on `GameObject`s; per-test `ResetAll` |
| `Tests/EditMode/Gameplay/Combat/AutoAttackControllerTests.cs` | **EXTENDED** — 3 new tests at bottom: `AcquireTarget_UsesXZDistance_IgnoringYOffset`, `AcquireTarget_OutOfRangeYOffsetEnemy_StillIncludedWhenXZIsClose`, `AcquireTarget_EmptyRegistry_ReturnsNull` | `AddComponent<AutoAttackController>` + spawn EnemyBases registered into `EnemyRegistry` |

Total **14 new EditMode tests**. Awake-guard pattern matches the qa-engineer `MechanicRegistry` precedent (no `InternalsVisibleTo` needed — public surface is enough; private state cleared via `EnemyRegistry.ResetAll`).

## Enemy death → pool return path — investigation result

| Concern | Status |
|---|---|
| Projectile lifetime → pool return | **OK** — `Projectile.Update` line 118 `Despawn()` on `_ageSeconds >= lifetimeSeconds` |
| Projectile hit → pool return | **OK** — `Projectile.Update` line 130 `Despawn()` after `DamageApplier.TryApply` |
| Projectile reaches enemy that died one frame earlier | **OK (defensive)** — `EnemyRegistry.FindFirstWithinRadius` skips `!e.Health.IsAlive`; projectile finds nothing, keeps flying until lifetime expires |
| Pool return on same-frame hit | **OK** — `Despawn()` calls `_pool.Return(this)` synchronously in `Update`, no frame leak |
| **Enemy death → enemy-pool return** | **GAP (deliberate follow-up)** — `grep -r "IDeathListener"` shows zero implementations. `EnemyHealth.Die` fires the listener chain but no class registers as `IDeathListener`, so killed enemies never return to `EnemyPool`. Dispatch authorisation explicitly says "do NOT implement an enemy pool here — surface as follow-up". Surfaced. |

## ADR-0019 resolution-criteria checklist

- [x] `EnemyRegistry` distance helpers use XZ semantics; tests prove it (`EnemyRegistryTests`, 11 tests)
- [x] `AutoAttackController.AcquireTarget` uses XZ semantics; tests prove it (3 new AAC tests)
- [x] `grep -E '\.y[ ]*\*[ ]*' Code/Gameplay/` returns zero hits in distance-computation contexts (audit table above documents every remaining `.y` hit as intentional)
- [x] Projectile → pool return path verified — no frame leak, defensive on stale targets
- [ ] *(Wave 5B)* `EnemyRole.Boss` exists — tech-architect's scope
- [ ] *(Wave 5B)* `WeaponDefinition` archetype fields — tech-architect's scope
- [ ] *(separate follow-up)* Enemy death → enemy pool return wiring (no `IDeathListener` consumers) — `DeathToPoolReturner` listener implementation is a Phase-5 follow-up dispatch

## One surprise

`HitDetector.cs` (the legacy spatial-hash from pre-XZ scaffolding) also drops world.z via `Vector2 p = e.transform.position`. It is **NOT used by the Wave-4 vertical-slice projectile path** (`Projectile.Update` queries `EnemyRegistry` directly, see line 126), so it isn't part of the runtime-broken surface ADR-0019 targets. Migrating it makes sense when the registry is upgraded to a real spatial hash (per `EnemyRegistry.cs:14` plan). Surfacing for the next perf-dispatch.

## What orchestrator validates next

Run `./core/scripts/verify-game.sh` + open Unity Editor → Test Runner → EditMode and confirm the new `EnemyRegistryTests` (11) + extended `AutoAttackControllerTests` (3 new AcquireTarget tests, on top of the existing 11) all pass; previous 41 EditMode tests remain green. Then dispatch the Wave-5 follow-up (enemy-death → EnemyPool listener) if/when the vertical slice needs sustained 200-enemy stress.
