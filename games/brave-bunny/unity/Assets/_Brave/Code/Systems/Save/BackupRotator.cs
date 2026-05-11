// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (atomic write protocol; 3 rolling backups)

#nullable enable

using System.IO;

namespace Brave.Systems.Save;

/// <summary>
/// Keeps the last 3 saves on disk per 03-save-system.md. Naming convention:
/// <c>save_0.dat.bak.1</c> (most recent) … <c>save_0.dat.bak.3</c> (oldest).
/// Rotation is two steps:
///   1. <c>File.Replace</c> already populated <c>bak.1</c> as part of the atomic
///      commit (it took the previous primary).
///   2. This class shifts older copies: <c>bak.2 → bak.3</c>, then <c>bak.1
///      stays put</c> because Replace just wrote it. Earlier <c>bak.3</c> is
///      discarded (overwritten by bak.2). Net: at most 3 backups exist.
/// </summary>
public sealed class BackupRotator
{
    public const int BackupCount = 3;

    private readonly string _primaryPath;

    public BackupRotator(string primaryPath) { _primaryPath = primaryPath; }

    public string PathFor(int index)
    {
        if (index <= 0) return _primaryPath;
        return $"{_primaryPath}.bak.{index}";
    }

    /// <summary>
    /// Rotate older backups one slot down: bak.2 → bak.3 (drop bak.3 if it
    /// existed). Called after <see cref="File.Replace(string,string,string)"/>
    /// has just populated bak.1 with the previous primary.
    /// </summary>
    public void RotateAfterReplace()
    {
        for (var i = BackupCount; i >= 2; i--)
        {
            var src = PathFor(i - 1);
            var dst = PathFor(i);
            if (File.Exists(src))
            {
                if (File.Exists(dst)) File.Delete(dst);
                File.Copy(src, dst, overwrite: true);
            }
        }
    }

    /// <summary>Delete every backup file (used by <see cref="SaveService.ClearAll"/> and tests).</summary>
    public void DeleteAll()
    {
        for (var i = 1; i <= BackupCount; i++)
        {
            var p = PathFor(i);
            if (File.Exists(p)) File.Delete(p);
        }
    }
}
