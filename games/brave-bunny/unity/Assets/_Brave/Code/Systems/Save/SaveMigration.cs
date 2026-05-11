// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (migration system)
// ADR-0008: migrations operate on JObject, not the typed model.

#nullable enable

using Newtonsoft.Json.Linq;

namespace Brave.Systems.Save;

/// <summary>
/// Abstract base for a single-step schema migration. Concrete migrations live
/// in this folder as <c>SaveMigrationV{N}.cs</c>. Loader walks them in order
/// from the file's version up to <see cref="SaveHeader.CurrentVersion"/>.
/// </summary>
public abstract class SaveMigration
{
    /// <summary>Version the migration consumes.</summary>
    public abstract int FromVersion { get; }

    /// <summary>Version the migration produces.</summary>
    public abstract int ToVersion { get; }

    /// <summary>Mutate <paramref name="root"/> in place. Must also bump the <c>version</c> field.</summary>
    public abstract void Apply(JObject root);
}
