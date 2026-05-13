# Hand-off: make-playable-engineer — 2026-05-13 14:30

## Agent
make-playable-engineer (Wave 6 — Run scene playable wiring)

## What was done
1. **SceneSetup.cs** — extended `EnsureRun()` with 6 new helper methods:
   - `EnsureRunCarrotProjectilePool()` → `[CarrotProjectilePool]` GO
   - `EnsureRunAutoAttack()` → AutoAttackController on Player (weapon + pool wired)
   - `EnsureRunTimer()` → `[RunTimer]` GO with RunTimer component
   - `EnsureRunWaveSpawner()` → `[WaveSpawner]` GO (hero + runTimer wired; wave=null — no WaveDefinition SO yet)
   - `EnsureRunController()` → `[Run]` GO with RunController + Char_bunny
   - `EnsureRunHud()` → `[HUD]` GO with UIDocument (RunHud.uxml) + RunHudController
   - Added `ForceScaffoldRun()` (backs up + deletes Run.unity, rebuilds it) and `RunHeadlessPlayable()` CLI entry

2. **BalanceJsonImporter.cs** — fixed bug: `BalanceDataDir` was `"../data/balance"` (resolves to `unity/data/balance`) but JSON lives at `../../data/balance` relative to `Application.dataPath`. Fixed to `"../../data/balance"`. Generates 33 SOs.

3. **Run.unity** — rebuilt: 773 → 1074 lines (+301). Has Camera, Player, Ground, DirectionalLight, [CarrotProjectilePool], [RunTimer], [WaveSpawner], [Run], [HUD].

## Test results
- Compilation: clean (Tundra build success)
- EditMode tests: 1 pre-existing failure in `SaveServiceFileStoreTests.Save_AllBackupsCorrupt_ReturnsFreshDefault_NoThrow` (SaveService.Load() throws on all-corrupt backups instead of swallowing). NOT caused by this wave's changes.

## Deferred items
- WaveDefinition SO (`Wave_meadow.asset`) not generated — no wave importer exists. WaveSpawner.wave = null at runtime (graceful, no crash). Level-designer should add a waves.json importer.
- Projectile prefab (`Projectile.prefab`) not assigned to CarrotProjectilePool. Pool.Awake() will disable itself (graceful). VFX/prefab agent should create the prefab.
- AutoAttack direct-cast will be disabled at runtime until projectile prefab is assigned.
- RunController._deathChannel and _activeBiome remain null (graceful degradation).
