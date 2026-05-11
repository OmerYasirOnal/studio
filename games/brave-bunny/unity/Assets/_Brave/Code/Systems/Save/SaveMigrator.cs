// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (migration system — "fails closed")
// ADR-0008: migration registry is hard-coded; adding a step is a code review.

#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Brave.Systems.Save;

/// <summary>
/// Walks registered <see cref="SaveMigration"/> steps in version order until
/// the root payload reaches <see cref="SaveHeader.CurrentVersion"/>. Fails
/// closed: missing step in the chain throws and the loader treats the file as
/// corrupt (player sees "Save needs update" rather than silent data loss).
/// </summary>
public sealed class SaveMigrator
{
    private readonly Dictionary<int, SaveMigration> _byFromVersion;

    public SaveMigrator(IEnumerable<SaveMigration> migrations)
    {
        _byFromVersion = new Dictionary<int, SaveMigration>();
        foreach (var m in migrations) _byFromVersion[m.FromVersion] = m;
    }

    /// <summary>Hard-coded default registry. Adding a new migration is a code change reviewed in PR.</summary>
    public static SaveMigrator Default() => new(new SaveMigration[]
    {
        new SaveMigrationV1(), // v1 -> v2 (placeholder for v1.1 rune section)
    });

    /// <summary>
    /// Apply migrations in order. Mutates <paramref name="root"/> in place.
    /// Throws <see cref="InvalidSaveException"/> if a required step is missing
    /// or would loop without progress.
    /// </summary>
    public void Apply(JObject root, int target = SaveHeader.CurrentVersion)
    {
        var current = (int?)root["version"] ?? 1;
        var safety = 64;
        while (current < target)
        {
            if (!_byFromVersion.TryGetValue(current, out var step))
                throw new InvalidSaveException($"No migration registered for version {current} → ?.");
            step.Apply(root);
            var next = (int?)root["version"] ?? current;
            if (next <= current)
                throw new InvalidSaveException($"Migration {current} did not advance version field.");
            current = next;
            if (--safety <= 0)
                throw new InvalidSaveException("Migration loop guard tripped.");
        }
    }
}
