# Tech Spec 03 — Save System

> Owner: tech-architect. Local-only persistence for Brave Bunny: format, location, migrations, corruption recovery, write triggers, and privacy posture. **No cloud save at launch** (v1.1 adds optional iCloud / Play Games sign-in). Sister docs: `02-data-model.md` (SOs are static config, not save data), `00-engine-and-version.md` (.NET Standard 2.1 surface).

## Goals

- **Resilience** — a corrupted save never bricks the game. Players see "Save reset" before they see "App crashed".
- **Forward-compatibility** — bumping the schema version never requires data loss. Migration scripts are part of the build.
- **Inspectability** — a developer can `cat` the save file (after a one-time header strip) and read JSON.
- **Atomicity** — saves either commit fully or not at all. No half-written files.
- **No PII** — the save is local; the only player identifier is a locally generated UUID.

## File format

Each save file is a binary blob with this layout:

| Offset | Size | Field | Notes |
|---|---|---|---|
| 0 | 4 bytes | Magic header | ASCII `BRBN` |
| 4 | 2 bytes | Version | uint16, little-endian; current = `0x0001` |
| 6 | 4 bytes | Payload length | uint32, little-endian, byte count of payload |
| 10 | 4 bytes | Payload CRC32 | uint32, little-endian; computed over JSON bytes |
| 14 | N bytes | Payload | UTF-8 JSON, NOT gzipped at launch (small enough; gzip in v1.1 if file exceeds 100 KB) |

Header total = 14 bytes. Payload is JSON via **Newtonsoft.Json** (`com.unity.nuget.newtonsoft-json` package — free, MIT-licensed, ships with Unity 6).

Why JSON over `BinaryFormatter` or MessagePack:
- Forward compat — adding a field never breaks readers.
- Inspectability — `tail -c +15 save_0.dat | jq` works.
- No `BinaryFormatter` security surface (deprecated in .NET 5+).
- Size impact negligible at our scale (<50 KB even at full unlock).

## Save location

```
$Application.persistentDataPath/
├── save_0.dat              # primary slot (only slot at launch)
├── save_0.dat.bak.1        # most recent backup
├── save_0.dat.bak.2        # second-most recent
└── save_0.dat.bak.3        # third-most recent (oldest)
```

- iOS resolves `persistentDataPath` to `~/Library/Application Support/<bundleId>/`.
- Android resolves to `/data/data/<package>/files/` (internal storage).
- Both are sandboxed and survive app updates.

### Slot count

**1 slot at launch.** The save filename includes `_0` so multi-slot is a non-breaking change (multi-profile post-launch would add `save_1.dat`, `save_2.dat`).

## Atomic write protocol

Every save uses **write-temp + rename**:

1. Serialize payload to UTF-8 JSON bytes.
2. Build header + payload in memory.
3. Write to `save_0.dat.tmp` (atomic in the filesystem call).
4. `File.Replace("save_0.dat.tmp", "save_0.dat", "save_0.dat.bak.1")` — atomic rename + backup rotation in one syscall on iOS/Android.
5. Rotate older backups: `bak.2 → bak.3`, `bak.1 → bak.2` (Step 4 already populated `bak.1`).
6. If any step throws, leave the previous save intact and surface a diagnostic toast.

Backups: **3 rolling files**. Means a player must corrupt 4 saves in a row to fully lose state.

## Save triggers

Saves are event-driven, **never every frame.** Triggering events:

| Trigger | When |
|---|---|
| Run-end (death or win) | When the run-end tally finishes |
| IAP purchase confirmed | After the App Store / Play Store callback |
| Character unlocked | After spending Stars or claiming achievement reward |
| Achievement claimed | After tap on "Claim" in achievement modal |
| Battle pass tier-up | After tier reward accept |
| Settings changed | On settings modal close |
| App goes to background | Single safety write |
| Cosmetic equipped | After loadout commit |

Explicit **non-triggers**: gem pickup, kill, draft pick, level-up, joystick movement, every frame. These happen too often and would write thousands of times per run.

## Save payload schema (version 1)

```json
{
  "version": 1,
  "player": {
    "id": "01HXAB23CDE4FG5H6JK7MN8PQR",
    "displayName": "Player",
    "language": "en"
  },
  "currencies": {
    "carrots": 0,
    "stars": 0,
    "soulShards": 0
  },
  "characters": {
    "bunny": {
      "owned": true,
      "level": 1,
      "xp": 0,
      "equippedWeaponSlug": "carrot-boomerang",
      "equippedSkinSlug": null
    },
    "tortoise": { "owned": false, "level": 1, "xp": 0, "equippedWeaponSlug": null, "equippedSkinSlug": null }
  },
  "weapons": {
    "carrot-boomerang": { "permaUnlocked": true },
    "pebble-sling": { "permaUnlocked": false }
  },
  "passives": {
    "magnet-charm": { "permaUnlocked": true }
  },
  "cosmetics": {
    "bunny-carnival": { "owned": false, "shards": 0 }
  },
  "battlePass": {
    "season": 1,
    "tier": 0,
    "xp": 0,
    "premiumOwned": false,
    "claimedFreeTiers": [],
    "claimedPremiumTiers": []
  },
  "achievements": {
    "survive-five-min": { "progress": 0, "claimed": false, "completedAt": null }
  },
  "dailyMissions": {
    "rolledForDate": "2026-09-01",
    "missions": [
      { "slug": "kill-200-swarmers", "progress": 0, "completed": false, "claimed": false }
    ]
  },
  "dailyStreak": {
    "currentDay": 1,
    "lastClaimUtcDate": null,
    "skipTokensUsed": 0
  },
  "settings": {
    "audioMaster": 0.8,
    "audioMusic": 0.7,
    "audioSfx": 0.9,
    "hapticsEnabled": true,
    "lowPowerMode": false,
    "tapToMove": false
  },
  "stats": {
    "totalRuns": 0,
    "totalKills": 0,
    "bestRunTimeSeconds": 0,
    "bossesDefeated": 0,
    "evolutionsTriggered": 0
  },
  "createdAt": "2026-09-01T12:00:00Z",
  "lastSavedAt": "2026-09-01T12:00:00Z"
}
```

Notes:
- The `id` is a locally generated UUID (Crockford-base32 ULID is the chosen format — sortable, no hyphens, ~26 chars).
- Unowned characters / weapons / cosmetics still get an entry so the UI can render lock states without a separate catalog lookup. Generation is idempotent on first load (missing slugs are filled in from `Catalog<T>`).
- All timestamps are UTC ISO-8601.

## Migration system

Each schema bump ships a migration class. The loader walks them in order from the file's version up to current.

```csharp
public interface ISaveMigration {
    int FromVersion { get; }                // e.g., 1
    int ToVersion { get; }                  // e.g., 2
    void Apply(JObject root);               // mutates the JSON in place
}

public sealed class SaveMigrationV1ToV2 : ISaveMigration {
    public int FromVersion => 1;
    public int ToVersion => 2;
    public void Apply(JObject root) {
        // example: add new "runes" section in v2
        if (root["runes"] == null) root["runes"] = new JObject();
        root["version"] = 2;
    }
}
```

The migration registry is a hard-coded array — adding a migration is a code change reviewed in PR. The loader fails closed if a migration is missing (player sees "Save needs update; restore from backup?" rather than silent data loss).

## Corruption recovery

On boot, `SaveLoader.Load()` attempts files in order:

1. `save_0.dat` — primary.
2. `save_0.dat.bak.1` — most recent backup.
3. `save_0.dat.bak.2`.
4. `save_0.dat.bak.3`.
5. Default save — fresh new-player state from `DefaultSaveFactory`.

For each candidate:

```csharp
1. Read header bytes; check magic == "BRBN".
2. Read version; reject if version > CURRENT_VERSION (newer save than this build).
3. Read payload length; bounds-check against file size.
4. Read payload; compute CRC32; compare against header CRC.
5. JSON-parse payload; catch JsonReaderException.
6. Apply migrations from file's version up to CURRENT_VERSION.
7. Hydrate into SaveState struct; validate invariants.
8. Return.
```

Any failure short-circuits to the next candidate. If all four backups fail, the loader builds the default state and **records a `save_reset.log` entry** in `persistentDataPath` (timestamped, lists which files were tried + why each failed). On next launch a one-time non-blocking toast appears: "Save was reset due to a corrupt file. Sorry about that!".

## Privacy posture

- **No PII written.** No email, no phone, no advertising ID, no Apple ID — none of these appear in the save.
- The `player.id` is a locally generated ULID. It's used only for client-side correlation (e.g., crash logs) and is **not** transmitted at launch.
- `player.displayName` is user-editable cosmetic only; default is "Player".
- IAP receipts are stored separately by Unity IAP / the platform SDK; they are not duplicated into our save.

## Future: cloud save (v1.1)

Optional **iCloud (iOS)** / **Google Play Games (Android)** sign-in adds:
- One-tap profile sync between devices.
- Conflict resolution: last-write-wins on `lastSavedAt`, with a "Restore from cloud" modal if local vs cloud diverge by > 1 hour.
- Implementation: Unity's `iCloud` plugin + Google Play Games Services package; both are free and platform-native.

Out of scope for launch — the local save is sufficient for the soft-launch markets (TR/PH/ID), where multi-device per player is uncommon.

## Test plan (qa-engineer hand-off)

- **Round-trip:** save → quit → load equals original state, all currencies and unlocks intact.
- **Migration:** craft a v1 save manually; bump build to v2; verify `runes` field appears.
- **Corruption:** truncate `save_0.dat` mid-payload; verify load falls back to `bak.1`.
- **Atomic:** kill the app between step 3 and step 4 of the write protocol; verify previous save is intact.
- **CRC drift:** flip a byte in the payload; verify CRC mismatch falls through.
- **Backup rotation:** save 5 times; verify only `bak.1`, `bak.2`, `bak.3` exist (no `bak.4`).
- **Defaults:** delete all save files; verify fresh-player default save loads with Bunny owned, Carrot Boomerang equipped, currencies at 0.

## Cross-references

- `02-data-model.md` — SOs are static config; this doc owns dynamic per-player state.
- GDD `02-meta-loop.md` — currencies + ladders that shape the save schema.
- GDD `08-economy.md` — Soul Shard exchange caps that the schema must support.
- `00-engine-and-version.md` — Newtonsoft.Json + .NET Standard 2.1 surface.
- Repo-root `CLAUDE.md` — observability (every save emits a JSONL entry in `games/<active>/logs/`).
