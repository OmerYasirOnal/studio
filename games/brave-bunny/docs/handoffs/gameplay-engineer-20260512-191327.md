# Hand-off — gameplay-engineer · 2026-05-12 19:13:27

## Task

Implement player movement (joystick + WASD fallback) on the Run scene's Player GameObject. Auto-attack is a SEPARATE later dispatch.

## What I delivered

- **Rewrote** `unity/Assets/_Brave/Code/Gameplay/Movement/PlayerMover.cs`
  - Namespace `Brave.Gameplay.Movement`, sealed `MonoBehaviour`, `[DisallowMultipleComponent]`.
  - Speed source: `CharacterDefinition.baseStats.baseMoveSpeed` (SO field). **No magic-number default.** If the SO isn't assigned or `baseMoveSpeed <= 0`, logs an error and disables itself.
  - Input: primary `IInputProvider` (the existing `VirtualJoystick`/`VirtualJoystickInput`); WASD + Arrow-key fallback via `Keyboard.current` from Unity Input System.
  - Top-down XZ-plane movement (matches `SceneSetup.cs` camera at (0, 14.7, -10.4) rot (55, 0, 0)).
  - Allocation-free `Update`: only struct constructions (`Vector2`/`Vector3`), no `GameObject.Find`, no LINQ, no string ops, no coroutines.
  - Pure `static Vector3 ComputeVelocity(Vector2, float)` helper — handles diagonal normalisation, sub-unit analog, over-unit clamp, and speed=0 defence; testable without a scene.
  - `Configure(IInputProvider, CharacterDefinition)` runtime injection for the boot composition root.
  - `SetFrozen(bool)` for pause / level-up draft.

- **Created** `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Movement/PlayerMoverTests.cs`
  - 9 tests including (1,0)→speed-magnitude, (0,0)→zero, diagonal-normalised, sub-unit-proportion, over-unit-clamped, y-never-written, scales-linearly-with-speed, zero-speed-defence.
  - Style mirrors `DamageCalculatorTests.cs` (constants table at top, no magic numbers, block-scoped namespace).

## Scene-wiring note (do NOT hand-edit the .unity file)

The Run scene's `Player` GameObject (created by `Editor/SceneSetup.cs` as a cube primitive) needs:

1. Add component **`PlayerMover`** (`Brave.Gameplay.Movement.PlayerMover`).
2. Assign **`character`** field → `Assets/_Brave/Data/Balance/Char_bunny.asset` (the imported `CharacterDefinition` SO).
3. Optionally assign **`inputProviderBehaviour`** → the scene's `VirtualJoystick` instance once UI lands; otherwise WASD fallback works in editor immediately.

I recommend extending `SceneSetup.EnsureRun()` to programmatically `player.AddComponent<PlayerMover>()` and assign the SO via `AssetDatabase.LoadAssetAtPath`. Track that as a follow-up dispatch (Editor-side, not gameplay-side).

## Gaps spotted — surface to orchestrator

- **BLOCKER for runtime wiring:** `BalanceJsonImporter.ImportCharacters` only writes `slug`, `displayName`, `signatureTypeName`, `unlockStarCost` to the SO. It does **NOT** populate `baseStats.baseMoveSpeed`, `baseHP`, `damageMultiplier`, etc. So even after running `Brave > Generate Balance SOs from JSON`, `Char_bunny.asset.baseStats.baseMoveSpeed` will be 0 and `PlayerMover.Awake` will refuse to enable.
  - **Dispatch needed:** balance-engineer (or whoever owns `BalanceJsonImporter`) to map `characters.json` → `baseStats` fields, applying `base_move_units_per_sec × characters[i].move_mult` for `baseMoveSpeed` and the per-character HP/crit/etc.
- **Pre-existing conflict:** `Movement/PlayerController.cs` and the old `Movement/Mover.cs` partially overlap with `PlayerMover.cs`. I did not delete them (out of scope, may have other call-sites). Orchestrator should consider an ADR or a cleanup dispatch to pick one canonical mover. `PlayerController.cs` writes to `pos.x`/`pos.y` (XY-plane), which contradicts the top-down 3/4 camera — likely needs deprecation.
- `VirtualJoystickInput.cs` still has `TODO(Phase 5)`-marked input-system wiring — also not in scope here.

## Touched files

- WROTE: `unity/Assets/_Brave/Code/Gameplay/Movement/PlayerMover.cs` (existing .meta GUID preserved)
- WROTE: `unity/Assets/_Brave/Code/Tests/EditMode/Gameplay/Movement/PlayerMoverTests.cs` (new)

Did NOT touch: `tools/ci/`, `_Brave/Code/UI/`, `_Brave/Code/Systems/`, `data/balance/*.json`, art bibles, ADRs, the `.unity` scene file.
