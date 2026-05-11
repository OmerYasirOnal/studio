// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (migration example v1→v2)
// Placeholder: at launch we are at version 1, so this migration only runs once
// the rune system ships in v1.1. Kept here so the migration-registry pattern
// exists from day 1 and has at least one example to template from.

#nullable enable

using Newtonsoft.Json.Linq;

namespace Brave.Systems.Save;

/// <summary>
/// V1 → V2 migration. v2 adds the <c>runes</c> section (per 02-meta-loop.md
/// Soul Shards launch caveat — runes are the v1.1 sink). Adding a section is
/// the safe shape of a migration: never delete, never rename, only extend.
/// </summary>
public sealed class SaveMigrationV1 : SaveMigration
{
    public override int FromVersion => 1;
    public override int ToVersion => 2;

    public override void Apply(JObject root)
    {
        if (root["runes"] == null) root["runes"] = new JObject();
        root["version"] = ToVersion;
    }
}
