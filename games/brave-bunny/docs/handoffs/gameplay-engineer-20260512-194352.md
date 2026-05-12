# Hand-off — gameplay-engineer — 2026-05-12 19:43:52

**Wave:** Wave 4 (orchestrator-dispatched)
**Task:** AutoAttack mechanics — tick weapon, spawn pooled projectile, hit enemies, apply damage.
Vertical-slice weapon: **Carrot Boomerang** (linear shoot — boomerang-return follow-up).

## Files written

| Path (relative to game root) | Δ |
|---|---|
| `unity/Assets/_Brave/Code/Gameplay/Combat/AutoAttackController.cs` | +~130L — added serialized `weapon`/`projectilePool`, Awake guard, `TickDirectCast`, static `TickCooldown`/`ComputeProjectileSpeedFromRange`/`ComputeProjectileLifetimeFromRange` helpers. Polymorphic `_equipped` path untouched. |
| `unity/Assets/_Brave/Code/Gameplay/Combat/CarrotProjectilePool.cs` | NEW — MonoBehaviour wrapper around `ObjectPool<Projectile>`. `Initialise`/`Spawn`/`Return` API. Pre-warms 64 instances by default. |
| `unity/Assets/_Brave/Code/Gameplay/Combat/ProjectileMath.cs` | NEW — pure helpers: `Step`, `DecayLifetime`, `ShouldExpire`, plus UI/perf constants (`RangeTravelFractionOfRate=0.5`, `MinTravelSeconds=0.05`, `MaxCastsPerTick=32`, `FacingEpsilonSqr=1e-6`). |
| `unity/Assets/_Brave/Code/Gameplay/Combat/DamageApplier.cs` | NEW — `TryApply(EnemyBase, damage, hitPoint, sourceId)` returns killing-blow bool; pure `NewHpAfter`/`IsKillingBlow` for tests. |
| `unity/Assets/_Brave/Code/Gameplay/Combat/Projectile.cs` | +`LaunchLinear()`; Update migrated to XZ-plane via `ProjectileMath.Step`; hit now routes through `DamageApplier.TryApply`. |
| `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Combat/AutoAttackControllerTests.cs` | NEW — cast cadence (1/rate), 5-period count, large-dt burst, zero-rate guard, projectile speed/lifetime derivations, naked-Awake `DirectCastEnabled` defaults-false. |
| `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Combat/CarrotProjectilePoolTests.cs` | NEW — pre-warm capacity, spawn/return identity round-trip, exhaust-then-null, steady-state distinct-instance ≤ capacity. |
| `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Combat/ProjectileMathTests.cs` | NEW — Step XZ-only arithmetic, Y-preservation, lifetime decay, 60-frame accumulation. |
| `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Combat/DamageApplierTests.cs` | NEW — NewHpAfter/IsKillingBlow exact / overkill / already-dead. |

## WeaponDefinition fields assumed

Direct-cast path reads `weapon.levels[0].fireRate`, `.damage`, `.range`. Matches existing `WeaponLevelData` schema. **Aligned with balance-engineer Wave 4 hand-off** (`balance-engineer-20260512-193850.md`) — their importer extension writes exactly these fields. Worked example from their hand-off confirms `Carrot.levels[0] = { damage 1.2, fireRate 1.0, range 5.0, projectiles 1 }` post-import. My Awake guard's "fireRate ≤ 0" trip-wire becomes the canary that flags "balance importer hasn't been re-run".

## Follow-ups flagged (NOT done this wave)

1. **Boomerang-return behaviour.** v0.1 linear-shoot only. Real Carrot Boomerang arcs back through the player; needs a `ProjectilePath` strategy (linear vs. boomerang vs. orbit) plus state for "outbound→peak→inbound" phases.
2. **Enemy death wiring.** `EnemyHealth.Die` fires `IDeathListener` but pool-return is "owned by the death-listener chain" (per existing comment) — no listener registers `EnemyPool.Release` yet. Add a `DeathToPoolReturner` listener so killed enemies actually despawn.
3. **EnemyRegistry XY→XZ bug.** `EnemyRegistry.FindFirstWithinRadius` / `SnapshotActiveInRange` compute `d.x*d.x + d.y*d.y` — pre-XZ-migration. Hit detection on the XZ plane is currently broken (projectile Update positions on XZ but registry queries on XY). Out of this wave's scope (touches Enemies/, adjacent agent territory). Surface to qa-engineer or systems-engineer for ADR-0018 follow-up.
4. **AutoAttackController.AcquireTarget same XY bug** — line 212 casts `(Vector2)transform.position` (drops Z). Front-arc/targeting won't work on XZ plane. Pre-existing.
5. **Pool overflow logging.** `CarrotProjectilePool.Spawn` returns null silently when exhausted; add `Debug.LogWarning` once-per-frame to surface content-tuning bugs.

## Orchestrator verification

```bash
./core/scripts/verify-game.sh                  # framework-level (still 26/0/0)
# Inside Unity Editor:
#   Test Runner → EditMode → run Brave.Tests.EditMode.Gameplay.Combat namespace
#   Expect: 4 new test classes, all passing. Existing 41 EditMode tests unaffected.
```

Manual smoke in Editor (vertical slice wiring not yet done by ui-engineer):
- Build a scene with PlayerMover + AutoAttackController + CarrotProjectilePool + Projectile prefab + a Carrot WeaponDefinition (hand-authored levels[0] until importer extends).
- PlayMode → player should fire one projectile/sec in facing direction; projectiles linger ~0.5 s, despawn on lifetime, return to pool.

## Blocked? No.
