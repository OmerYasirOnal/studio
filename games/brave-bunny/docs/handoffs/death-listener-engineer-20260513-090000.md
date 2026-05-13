# Hand-off — death-listener-engineer — 2026-05-13T09:00:00Z

## Task
ADR-0019 item 3: Wire enemy death → pool return via `IDeathListener`.

## What was delivered

| File | Change |
|---|---|
| `Code/Gameplay/Combat/IDeathListener.cs` | New interface — `OnDeath(GameObject entity)` |
| `Code/Gameplay/Combat/EnemyPoolReturnOnDeath.cs` | New MonoBehaviour — implements `IDeathListener`; calls `EnemyPool.Release` on death, falls back to `SetActive(false)` if pool ref is missing |
| `Code/Gameplay/Combat/DamageApplier.cs` | Modified — on killing blow, `GetComponents<IDeathListener>` and invoke `OnDeath` on each; idempotency via existing `!health.IsAlive` guard |
| `Code/Tests/EditMode/Gameplay/Combat/DeathListenerTests.cs` | New tests — `EnemyHealth` listener fires once on kill, never on corpse; `DamageApplier.IsKillingBlow` idempotency anchors |
| `Code/Tests/EditMode/Gameplay/Combat/EnemyPoolReturnTests.cs` | New tests — spawn → kill → assert pool inactive + count=0; double-call safety; null-pool fallback |

## Key design notes

- `Brave.Gameplay.Combat.IDeathListener` (`OnDeath(GameObject)`) is distinct from `Brave.Gameplay.Enemies.IDeathListener` (`OnEnemyDied`) — the Enemies one is the analytics/XP chain; the Combat one is the pool-return chain. Both coexist without conflict.
- `EnemyPoolReturnOnDeath.Initialise(pool)` must be called by the spawner after `Acquire` (mirrors the Pickup pattern in `PickupPool`).
- `DamageApplier` diff is 9 net code lines (well under 25-line cap).
- `Enemy.cs` already had inline pool-return in `Die()`; that path remains intact. `EnemyPoolReturnOnDeath` targets the `EnemyBase`/`EnemyHealth` hierarchy.

## Remaining gaps (not in scope)

- XP gem drops still not wired (ADR-0019 item 3 notes this; separate agent scope).
- `EnemyBase`-based enemies need the spawner to call `EnemyPoolReturnOnDeath.Initialise` — `Spawner.cs` only configures `Enemy`, not `EnemyBase` subclasses; that wiring lands when the spawner is extended.
