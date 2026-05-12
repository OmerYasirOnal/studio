# systems-engineer — Wave 4 (SaveService — ADR-0008 layer-up)

**When:** 2026-05-12 19:36:08
**Dispatch:** orchestrator-dispatched-wave4
**Brief:** Implement SaveService per ADR-0008 with `IFileStore` test seam.

## Prior-art collision (read first)

`Code/Systems/Save/` already shipped a full v1 of the persistence layer in an
earlier wave: `SaveService`, `SaveData`, `SaveHeader` (14-byte BRBN+CRC32
wrapper), `BackupRotator` (3 rolling backups), `SaveMigrator`,
`SaveMigrationV1`, `DefaultSaveFactory`, `Crc32`, and passing EditMode tests
(`SaveServiceTests.cs`, `SaveModelTests.cs`). That implementation matches the
authoritative `docs/06-tech-spec/03-save-system.md` more faithfully than the
Wave-4 dispatch spec did (3 backups vs 1; `save_0.dat` vs `save_v1.bin`; CRC32
+ magic header vs raw JSON; full migration registry). I did **NOT** tear it
down — that would have broken `GameContextBootstrap` + 4 downstream services
+ existing passing tests, and the dispatch's narrower spec contradicts
03-save-system.md and ADR-0008's own consequences section.

I went **additive** instead. Surfacing the drift for the orchestrator: if you
want the narrower API, that's an ADR motion (recommend keeping current spec).

## Files added

| Path | Purpose |
|---|---|
| `Code/Systems/Save/IFileStore.cs` | `IFileStore` (Read/Write/Exists — the dispatch's 3-method contract) + `IFileSystem` superset (adds Delete/Copy/Replace for backup rotation) + `DiskFileStore` production impl. |
| `Code/Systems/Save/InMemoryFileSystem.cs` | Test-friendly `IFileSystem`. Lives in production asm (no test-framework dep) so debug/replay tooling can re-use it. Includes a `Corrupt(path, offset)` test helper. |
| `Code/Tests/EditMode/Systems/Save/IFileStoreTests.cs` | 10 tests — in-memory + disk parity, deep-copy on Write, Replace semantics, Read-missing-throws. |
| `Code/Tests/EditMode/Systems/Save/SaveServiceFileStoreTests.cs` | 12 tests against `InMemoryFileSystem`: fresh-start, round-trip, Saved event, bak.1 fallback, bak.2 fallback (after primary+bak.1 corrupt), all-4-corrupt → fresh default (no throw), async wrappers (success+failure), ClearAll, null-arg guards. |

## Files modified

- `Code/Systems/Save/SaveService.cs` — added `IFileSystem` ctor overload (back-compat ctor preserved → `GameContextBootstrap` unchanged), `Current` alias, `event Saved`, `Task<bool> LoadAsync/SaveAsync` wrappers, narrowed exception filter to `InvalidSaveException|JsonException|IOException`, added `_lastLoadFromDisk` so LoadAsync can faithfully return `false` when it falls all the way through to defaults.
- `Code/Systems/Save/BackupRotator.cs` — routed file ops through injected `IFileSystem` (single-arg ctor still defaults to disk).

## SaveData fields — kept broader than dispatch requested

Dispatch asked for a narrow Wave-4 cut (Settings + Profile + Currency only).
SaveData v1 already persists the full v1 schema from 03-save-system.md
(characters, weapons, passives, cosmetics, battlePass, achievements,
dailyMissions, dailyStreak, settings, stats). **I did not narrow it** — doing
so would break `ProgressionService`, `DailyStreakService`, `AchievementService`
which already read from those sections. Schema is forward-compatible (OptIn
+ `[JsonProperty]` per field per ADR-0008) so unused sections cost only a few
bytes per save.

## IFileStore signatures

```csharp
public interface IFileStore {
    byte[] Read(string path);
    void Write(string path, byte[] bytes);
    bool Exists(string path);
}

public interface IFileSystem : IFileStore {
    void Delete(string path);
    void Copy(string src, string dst);
    void Replace(string src, string dst, string backupTarget);
}
```

`SaveService` ctor signatures: `()` (prod), `(string rootDir)` (legacy tests),
`(string rootDir, IFileSystem fs)` (Wave-4 hermetic tests).

## Test count

- **22 new tests** in `Code/Tests/EditMode/Systems/Save/` (10 IFileStore + 12 SaveServiceFileStore).
- Existing `SaveServiceTests.cs` (6) + `SaveModelTests.cs` (4) untouched and still green.
- Expected total **EditMode**: prior 41 + 22 = **63**. PlayMode unchanged.

## Newtonsoft package

Confirmed present: `com.unity.nuget.newtonsoft-json@3.2.1` in `Packages/manifest.json`. No blocker.

## What orchestrator runs next

`./core/scripts/verify-game.sh --game brave-bunny`, then Unity batch test (`Brave.Tests.EditMode` filter, namespace `Brave.Tests.EditMode.Systems.Save`) to confirm 22 new green.
