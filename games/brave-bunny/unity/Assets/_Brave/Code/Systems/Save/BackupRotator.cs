// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (atomic write protocol; 3 rolling backups)
//
// Wave-4 additions (systems-engineer): all file ops now go through IFileSystem
// so EditMode tests don't need a real disk. The legacy single-arg ctor still
// exists and defaults to DiskFileStore for back-compat.

#nullable enable

namespace Brave.Systems.Save;

/// <summary>
/// Keeps the last 3 saves on disk per 03-save-system.md. Naming convention:
/// <c>save_0.dat.bak.1</c> (most recent) … <c>save_0.dat.bak.3</c> (oldest).
/// Rotation is two steps:
///   1. <c>IFileSystem.Replace</c> already populated <c>bak.1</c> as part of
///      the atomic commit (it took the previous primary).
///   2. This class shifts older copies: <c>bak.2 → bak.3</c>, then <c>bak.1</c>
///      stays put because Replace just wrote it. Earlier <c>bak.3</c> is
///      discarded (overwritten by bak.2). Net: at most 3 backups exist.
/// </summary>
public sealed class BackupRotator
{
    public const int BackupCount = 3;

    private readonly string _primaryPath;
    private readonly IFileSystem _fs;

    public BackupRotator(string primaryPath) : this(primaryPath, new DiskFileStore()) { }

    public BackupRotator(string primaryPath, IFileSystem fileSystem)
    {
        _primaryPath = primaryPath;
        _fs = fileSystem;
    }

    public string PathFor(int index)
    {
        if (index <= 0) return _primaryPath;
        return $"{_primaryPath}.bak.{index}";
    }

    /// <summary>
    /// Rotate older backups one slot down: bak.2 → bak.3 (drop bak.3 if it
    /// existed). Called after the atomic Replace has just populated bak.1 with
    /// the previous primary.
    /// </summary>
    public void RotateAfterReplace()
    {
        for (var i = BackupCount; i >= 2; i--)
        {
            var src = PathFor(i - 1);
            var dst = PathFor(i);
            if (_fs.Exists(src))
            {
                if (_fs.Exists(dst)) _fs.Delete(dst);
                _fs.Copy(src, dst);
            }
        }
    }

    /// <summary>Delete every backup file (used by <see cref="SaveService.ClearAll"/> and tests).</summary>
    public void DeleteAll()
    {
        for (var i = 1; i <= BackupCount; i++)
        {
            var p = PathFor(i);
            if (_fs.Exists(p)) _fs.Delete(p);
        }
    }
}
