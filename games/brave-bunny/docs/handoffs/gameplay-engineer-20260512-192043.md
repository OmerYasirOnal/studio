# Hand-off — gameplay-engineer · 2026-05-12 19:20:43 (Wave 2)

## Task

ADR-0017 cleanup + finish VirtualJoystickInput + Run-scene PlayerMover wiring.

## Part A — Deletions per ADR-0017 — **NOT EXECUTED; STOPPED PER INSTRUCTIONS**

Grep found **external callers** of both classes. The dispatch said: "If found, STOP and put them in the hand-off as orchestrator-decision items — do not aggressively rewrite call-sites." So I did **not** delete the files. They are still on disk.

External references found:

- **`Mover`** — called by 4 enemy behaviour files:
  - `unity/Assets/_Brave/Code/Gameplay/Enemies/SwarmerBehavior.cs:16` — `Mover.Step(...)`
  - `unity/Assets/_Brave/Code/Gameplay/Enemies/EliteBehavior.cs:16` — `Mover.Step(...)`
  - `unity/Assets/_Brave/Code/Gameplay/Enemies/RangedBehavior.cs:34` — `Mover.Step(...)`
  - `unity/Assets/_Brave/Code/Gameplay/Enemies/TankBehavior.cs:29` — `Mover.Step(...)`
- **`PlayerController`** — called by:
  - `unity/Assets/_Brave/Code/Gameplay/Combat/AutoAttackController.cs:23` — `[SerializeField] private PlayerController? player;`

**Decision needed from orchestrator:**
1. `Mover.Step` is a stateless XY-plane helper. The enemy behaviours use it correctly on the XY plane — BUT the rest of the game (PlayerMover, camera) is XZ-plane. The enemies are already inconsistent with the world plane. This is a wider issue than ADR-0017; suggest a dedicated dispatch to either (a) port `Mover.Step` to XZ semantics, or (b) replace it with a tiny inline `pos += dir * speed * dt` in each enemy. Option (b) lets `Mover.cs` be deleted cleanly.
2. `AutoAttackController.player` is typed as `PlayerController` only for the `LastInput`/`Facing` properties. Refactor to take `PlayerMover` instead — a small, contained edit. Out of my Wave-2 scope.

## Part B — VirtualJoystickInput.cs — **COMPLETE**

- Removed all `TODO(Phase 5)` markers; implemented Input-System wiring via `Touchscreen.current.primaryTouch`.
- Public API unchanged (still implements `IInputProvider`: `StickDirection`, `PausePressed`, `AbilityPressed`).
- Editor / desktop fallback: returns `Vector2.zero` when `Touchscreen.current` is null — PlayerMover's WASD path then takes over.
- Allocation-free `Update`: only struct math, no `new()`, no LINQ, no string ops. `Touchscreen.current` and `primaryTouch` are control references, not allocations.
- New `[SerializeField] float maxDragRadiusPx = 100f` — UI-geometry constant lives in code per dispatch direction (not a balance value).
- **Extracted static helper** `Vector2 ScreenDeltaToNormalized(Vector2 delta, float maxRadius)` for testability.
- File: `unity/Assets/_Brave/Code/Gameplay/Movement/VirtualJoystickInput.cs` (~108 lines; ~+82 net vs prior stub).
- Tests added: `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Movement/VirtualJoystickInputTests.cs` — 9 tests covering centre=zero, edge=unit, beyond-edge clamp (4 cases), half-radius proportion, diagonal-beyond clamp, diagonal-within preserves analog magnitude, zero/negative `maxRadius` no-op.

## Part C — SceneSetup.cs — **COMPLETE**

- Added `using Brave.Gameplay.Definitions; using Brave.Gameplay.Movement;` + `const string CharBunnyAssetPath = "Assets/_Brave/Data/Balance/Char_bunny.asset";`.
- Added private `EnsureRunPlayerWiring(GameObject player)` invoked from `EnsureRun()` after primitives.
- Idempotent: `GetComponent<PlayerMover>() ?? AddComponent<PlayerMover>()`.
- Uses `AssetDatabase.LoadAssetAtPath<CharacterDefinition>(CharBunnyAssetPath)` + `SerializedObject` + `FindProperty("character")` to set the private SerializeField without exposing a public setter on `PlayerMover` (preserves its API).
- If SO doesn't exist (balance generator hasn't run), logs a warning and skips — `PlayerMover.Awake` then errors with its own clear "run Brave > Generate Balance SOs from JSON" message.
- Diff: ~+40 lines, single file. **Awkwardness:** `SceneSetup.cs` is "general utility" territory, not gameplay's domain. Recommend moving this method to a new `unity/Assets/_Brave/Code/Gameplay/Movement/Editor/PlayerWiring.cs` (with its own `Brave.Gameplay.Movement.Editor` asmdef) in a future cleanup dispatch; for now the inline placement is minimal and tagged with a `gameplay-engineer Wave 2` comment for easy grep.

## What orchestrator should run next to verify

1. `./core/scripts/verify-game.sh --game brave-bunny` (compile + tests; expect 47+9 ≈ 56 EditMode tests, all green)
2. Open Unity → `Brave > Scaffold Phase-5 Scenes` → confirm Run.unity's Player has `PlayerMover` with `character` set (only after running `Brave > Generate Balance SOs from JSON` first)
3. Decide on the Part A follow-ups (see above) — issue a fresh dispatch for the `Mover`/`PlayerController` external-caller cleanup; do **not** delete those two files manually until callers are migrated.

## Touched files

- WROTE: `unity/Assets/_Brave/Code/Gameplay/Movement/VirtualJoystickInput.cs` (replaced TODO stub)
- WROTE: `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Movement/VirtualJoystickInputTests.cs` (new, 9 tests)
- EDITED: `unity/Assets/Editor/SceneSetup.cs` (+~40 lines, single new private method)

Did NOT touch: `tools/ci/`, `_Brave/Code/UI/`, `_Brave/Code/Systems/`, `data/balance/*.json`, `BalanceJsonImporter.cs`, `PlayerMover.cs`, `IInputProvider.cs`, art bibles, the `.unity` scene file.
