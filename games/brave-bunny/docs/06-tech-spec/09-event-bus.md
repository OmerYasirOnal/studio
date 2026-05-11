# Tech Spec 09 — Event Bus / Service Locator

> Owner: tech-architect. The communication pattern between subsystems in Brave Bunny: **service locator + typed event channels**, deliberately **not** a global string-keyed event bus and **not** singletons. Cross-refs: `08-state-machine.md` (`GameContext` owns state manager), `05-performance-budget.md` (zero allocations in run hot loop), `01-project-layout.md` (asmdef layering — Gameplay can't reference UI).

## Decision

**Direct references via `GameContext` service locator** for app-lifetime services. **Typed `EventChannel<T>` ScriptableObjects** for loose-coupling pub/sub. **Direct method calls** for high-frequency runtime events.

Explicitly rejected:

- **Singletons** (`Instance` pattern) — make testing hard and obscure dependency direction.
- **Global string-keyed event bus** (e.g. `EventBus.Send("EnemyDied", payload)`) — magic strings break compile-time checking and CTRL+F search.
- **`SendMessage` / `BroadcastMessage`** — reflection-based, IL2CPP-hostile, slow, untyped.
- **C# static events on shared types** — order-of-init bugs across asmdef boundaries.

## The three communication tiers

### Tier 1 — App-lifetime services: `GameContext` locator

Services that live for the app's lifetime (audio, save, catalog, input) are registered at `Boot` and accessed by name-of-interface.

```csharp
public sealed class GameContext {
    private readonly Dictionary<Type, object> _services = new();

    public void Register<T>(T impl) where T : class => _services[typeof(T)] = impl;
    public T Get<T>() where T : class => (T)_services[typeof(T)];
    public bool TryGet<T>(out T impl) where T : class {
        if (_services.TryGetValue(typeof(T), out var raw)) { impl = (T)raw; return true; }
        impl = null;
        return false;
    }
}
```

Registered services at `Boot` (per `08-state-machine.md`):

| Interface | Owner asmdef | Lifecycle |
|---|---|---|
| `ISaveService` | `Brave.Systems` | App lifetime |
| `ICatalogService` | `Brave.Systems` | App lifetime |
| `IAudioService` | `Brave.Systems` | App lifetime |
| `IInputService` | `Brave.Systems` | App lifetime |
| `IAnalyticsService` | `Brave.Systems` | App lifetime (no-op stub at launch) |
| `IPoolService` | `Brave.Gameplay` | App lifetime |
| `GameStateManager` | `Brave.Boot` | App lifetime |

Consumers receive `GameContext` via constructor injection (or `[SerializeField]` for MonoBehaviours that need it at scene boundaries). No service references another service directly — they all go through the locator, so wiring order at `Boot` is the only place that knows the concrete graph.

### Tier 2 — High-frequency runtime events: direct method calls

Events that fire many times per frame (enemy hit, projectile hit, damage tick, pickup grab) use **direct method calls on subscribed listeners** — no pub/sub overhead, no allocations.

```csharp
public interface IDamageListener {
    void OnDamage(DamageEvent evt);
}

public readonly struct DamageEvent {
    public readonly int sourceId;
    public readonly int targetId;
    public readonly float amount;
    public readonly DamageType type;
}

public sealed class CombatResolver {
    private readonly List<IDamageListener> _listeners = new(capacity: 16);

    public void RegisterListener(IDamageListener l) => _listeners.Add(l);
    public void UnregisterListener(IDamageListener l) => _listeners.Remove(l);

    public void ResolveHit(int srcId, int tgtId, float amount, DamageType type) {
        var evt = new DamageEvent { sourceId = srcId, targetId = tgtId, amount = amount, type = type };
        for (int i = 0, n = _listeners.Count; i < n; i++) _listeners[i].OnDamage(evt);
    }
}
```

Rules:

- **Listener lists are pre-allocated** (`capacity: N` in constructor) so the list never grows during a run.
- **`DamageEvent` is a `readonly struct`** — passed by value, no GC, fits the zero-alloc hot-loop contract from `05-performance-budget.md`.
- **Foreach is banned in hot path** — explicit `for (int i = 0; i < n; i++)` to avoid struct enumerator allocations on older Mono profiles (defensive; IL2CPP usually fine).

### Tier 3 — Loose-coupling cross-system events: `EventChannel<T>` ScriptableObjects

Events that cross asmdef boundaries (UI listening to Gameplay), fire rarely (~1–10 per run), and benefit from inspector visibility for designers use **typed `EventChannel<T>` ScriptableObjects**.

```csharp
[CreateAssetMenu(menuName = "Brave/Events/Channel")]
public class EventChannel<T> : ScriptableObject {
    private event Action<T> _listeners;

    public void Raise(T arg) => _listeners?.Invoke(arg);
    public void Subscribe(Action<T> listener) => _listeners += listener;
    public void Unsubscribe(Action<T> listener) => _listeners -= listener;

    private void OnDisable() {
        // domain reload + scene unload safety: clear stale subscriptions
        _listeners = null;
    }
}
```

Concrete subclasses are one-line wrappers — Unity 6 supports generic `ScriptableObject` types since 2020 but inspector display still prefers a named subclass:

```csharp
[CreateAssetMenu(menuName = "Brave/Events/DeathChannel")]
public sealed class DeathChannel : EventChannel<DeathEvent> { }

public readonly struct DeathEvent {
    public readonly int characterSlugHash;
    public readonly float runSeconds;
    public readonly int enemiesKilled;
    public readonly Cause cause;
    public enum Cause { Killed, Quit, TimedOut }
}
```

#### Approved channels (vertical slice + launch)

| Channel | Payload | Raised by | Listeners |
|---|---|---|---|
| `DeathChannel` | `DeathEvent` | gameplay `Run` → on hero HP = 0 | UI run-end screen, analytics, achievement tracker |
| `LevelUpChannel` | `LevelUpEvent` (level int + xp remainder) | gameplay XP system | UI draft screen, audio (level-up stinger) |
| `AchievementClaimChannel` | `AchievementClaimEvent` (slug + reward bundle) | UI achievement modal | save service (trigger save), currency system |
| `CurrencyChangedChannel` | `CurrencyChangedEvent` (currency type + new total + delta) | save service | UI home-screen currency widgets, animations |
| `BossPhaseChannel` | `BossPhaseEvent` (phase 1→2→3) | boss controller | audio (snapshot/stinger), VFX (phase-change shockwave), camera shake |

#### Where they live

Channel asset files under `unity/Assets/_Brave/Data/Definitions/EventChannels/`:

```
DeathChannel.asset
LevelUpChannel.asset
AchievementClaimChannel.asset
CurrencyChangedChannel.asset
BossPhaseChannel.asset
```

Subscribers reference the channel asset via `[SerializeField]` (UI components) or via `GameContext.Get<ICatalogService>().GetEventChannel("death")` (gameplay code that can't `[SerializeField]` because it's not a MonoBehaviour).

### Why ScriptableObject channels for loose-coupling

1. **Designer visibility** — channel assets show up in the inspector with the list of subscribers (via a debug `EventChannelInspector` editor script). Helps debugging "why didn't the level-up sound play?" without `grep`-ing.
2. **Asmdef boundary respect** — `Brave.UI` and `Brave.Gameplay` don't need a direct reference to each other; they both reference the channel asset (which lives in `Brave.Systems` data folder).
3. **Domain reload safety** — `OnDisable` clears stale listeners on Editor reload + scene unload. C# static events do not have this safety.
4. **Discoverable** — `Find References` in the Editor shows every `[SerializeField] DeathChannel` on every MonoBehaviour.

### Why ScriptableObject vs plain C# events

Open question bumped to ADR: **`EventChannel<T>` SO vs. C# event with explicit lifetime owner.** Plain C# events on a single owner (`GameContext.Death += handler`) work too, are slightly cheaper (no asset reference indirection), and don't require asset authoring. The ScriptableObject path wins on designer visibility + asmdef boundaries; the C# event path wins on simplicity. Tech-architect locks this as **ScriptableObject channels for the 5 approved loose-coupling events** but flags **ADR-0013 — Event channel mechanism: SO vs C# event** for the gameplay-engineer to validate during Phase 5 — if Profiler shows the SO indirection adds > 0.05 ms in aggregate, we revisit.

## Banned patterns

| Pattern | Why banned |
|---|---|
| `GameObject.SendMessage("OnEnemyDied", payload)` | Reflection-based; IL2CPP slow; untyped |
| `GameObject.BroadcastMessage(...)` | Same; also walks the hierarchy unpredictably |
| `static Action OnEnemyDied` on shared types | Domain reload leaks subscriptions; cross-asmdef init-order bugs |
| `EventBus.Send("string_key", payload)` | Magic strings break refactor + grep; no payload typing |
| Singleton (`public static Foo Instance`) | Hidden dependencies; can't unit-test in isolation; init-order fragile |
| `FindObjectOfType<T>()` in run hot path | Linear scene scan; allocates; banned in `Update()` paths |

`FindObjectOfType` is *allowed* at scene boundary code (e.g., `RunIntro.OnEnterAsync`) but **not** inside the run's per-frame logic. Enforced via an analyzer rule in Phase 5 `tools/ci/` (build-engineer to wire).

## Example end-to-end: enemy dies → UI updates achievement counter

1. `EnemyController.OnHpZero()` calls `CombatResolver.ResolveDeath(enemyId)` — direct method call (Tier 2).
2. `CombatResolver` walks pre-registered `IDeathTickListener`s; one of them is `AchievementTracker` from `Brave.Systems`, which increments its in-memory counter.
3. If the counter crosses an achievement threshold, `AchievementTracker` calls `_achievementChannel.Raise(...)` — Tier 3 SO channel.
4. UI's `AchievementToastWidget` is subscribed; pops a 1.5 s toast.
5. `SaveService` is also subscribed; flushes a save write (`AchievementClaimChannel` is in `03-save-system.md` save-trigger list).

Cross-asmdef: step 2 lives in `Brave.Gameplay`; step 3 lives in `Brave.Systems`; step 4 lives in `Brave.UI`. None of them reference each other directly — only the channel asset and `GameContext`.

## Cross-references

- `01-project-layout.md` — asmdef boundaries (`Brave.Gameplay` cannot reference `Brave.UI`).
- `05-performance-budget.md` — zero allocations in run hot path; listener lists pre-allocated.
- `08-state-machine.md` — `GameContext` owns the state manager.
- `02-data-model.md` — Catalogs (also SO-based) follow the same locator pattern.
- **ADR-0013 (pending)** — Event channel mechanism: SO vs C# event.
