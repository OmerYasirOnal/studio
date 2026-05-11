# ADR 0008 — Save format: Newtonsoft JSON

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing tech-architect wave-4 flag)

## Context

`docs/06-tech-spec/03-save-system.md` chose binary header + Newtonsoft JSON payload. Tech-architect flagged this as ADR-worthy because reversing the choice post-launch requires player-data migration. Three options:

- **A. Newtonsoft JSON in binary wrapper** (4-byte magic + uint16 version + JSON payload) — human-inspectable, generous to schema evolution, ~3x size vs binary, ~5ms parse on load
- **B. MessagePack** — binary, ~1.2x JSON size, faster parse (~1ms), schema-evolution-tolerant if used correctly, requires a 3rd-party package (free MIT — neuecc/MessagePack-CSharp)
- **C. BinaryFormatter** — Microsoft-native, very fast, NO forward-compat (type renames break loads), deprecated in .NET 5+, NOT viable

## Decision

**Option A — Newtonsoft JSON in binary wrapper.**

Rationale:

1. **Schema evolution** is the dominant requirement. We will rename fields, add fields, deprecate fields between v1 and v1.1 and beyond. Newtonsoft's `[JsonIgnore]`, `[JsonProperty("oldName")]`, optional/default values, and migration classes make this trivial.
2. **Human inspection** during dev is invaluable. We can `cat save_0.dat` (skip the 6-byte header) and read the JSON.
3. **Parse cost** of 5ms happens only at app cold start and after each meaningful save event — never per-frame.
4. **Size penalty** (3x vs binary) is bounded: typical save is < 50 KB at launch, < 200 KB at v1.x mature. Comfortable on iOS.
5. **MessagePack** would save ~30 KB per save and 4ms parse. Not enough win to take on the dependency.

## Consequences

- `unity/Packages/manifest.json` includes `com.unity.nuget.newtonsoft-json` (free, MIT, Unity-hosted)
- systems-engineer authors `SaveService.cs` using Newtonsoft conventions:
  - `[JsonObject(MemberSerialization.OptIn)]` on the root save object so unknown fields don't get serialized
  - `[JsonProperty("...")]` on every field so renames don't break loads
- Save migration classes (`SaveMigrationV1ToV2.cs`) operate on `JObject`, not the typed model — robust against schema drift
- Backup files store full JSON (no incremental diffs at launch)
- qa-engineer writes EditMode tests:
  - Round-trip serialization
  - Backward-compat: load a v1 save into v2 code
  - Corruption: tamper with the JSON, verify graceful recovery

## Alternatives considered

- **B. MessagePack** — rejected for dev ergonomics. Reconsider only if save file size grows > 1 MB at v2.x.
- **C. BinaryFormatter** — rejected categorically (deprecated, no schema evolution).
- **Custom binary format** — rejected. Writing our own serializer is undifferentiated work.
- **PlayerPrefs** — rejected for anything except trivial settings (`docs/06-tech-spec/03-save-system.md` already mandates this).

## References

- `docs/06-tech-spec/03-save-system.md`
- `docs/06-tech-spec/11-third-party.md` (Newtonsoft is on the approved list)
