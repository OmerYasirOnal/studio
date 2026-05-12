// Brave Bunny — Systems / Save
// Wave 4 dispatch (systems-engineer): IFileStore abstraction so SaveService can
// be exercised deterministically in EditMode without touching the real disk.
//
// Two interfaces live here:
//
//   * IFileStore  — the minimal 3-method byte-blob store the dispatch asked for.
//                   Public surface; what UI / debug code should depend on if all
//                   it needs is "read these bytes / write these bytes / exists?".
//   * IFileSystem — the strict superset SaveService itself needs for the atomic
//                   write protocol + backup rotation (03-save-system.md). Adds
//                   Delete / Copy / Replace. Internal to the Save subsystem.
//
// Both have a DiskFileStore implementation (production default) and an
// in-memory implementation for tests (InMemoryFileSystem.cs).
//
// References:
//   - ADR-0008 — Newtonsoft JSON in binary wrapper
//   - docs/06-tech-spec/03-save-system.md — atomic write protocol, rotation

#nullable enable

using System.IO;

namespace Brave.Systems.Save;

/// <summary>
/// Minimal byte-blob store. The exact contract requested by the Wave-4
/// dispatch: three methods, no streams, no async. Implementations must be
/// thread-safe-ish under the same single-writer assumption the rest of the
/// save subsystem makes (one save call at a time).
/// </summary>
public interface IFileStore
{
    /// <summary>Read the entire contents of <paramref name="path"/>. Throws <see cref="IOException"/> if missing.</summary>
    byte[] Read(string path);

    /// <summary>Overwrite (or create) <paramref name="path"/> with <paramref name="bytes"/>.</summary>
    void Write(string path, byte[] bytes);

    /// <summary>True iff <paramref name="path"/> currently has stored bytes.</summary>
    bool Exists(string path);
}

/// <summary>
/// File-system contract SaveService needs end-to-end: read/write/exists plus
/// the destructive operations the atomic write + 3-backup rotation require.
/// Tests inject <c>InMemoryFileSystem</c>; production injects
/// <see cref="DiskFileStore"/>.
/// </summary>
public interface IFileSystem : IFileStore
{
    /// <summary>Delete <paramref name="path"/>. No-op if it doesn't exist (caller may have just rotated it).</summary>
    void Delete(string path);

    /// <summary>Copy <paramref name="src"/> over <paramref name="dst"/>; overwrites if <paramref name="dst"/> exists.</summary>
    void Copy(string src, string dst);

    /// <summary>
    /// Atomic replace: move <paramref name="src"/> to <paramref name="dst"/> while
    /// shunting any existing <paramref name="dst"/> content to
    /// <paramref name="backupTarget"/>. Mirrors <see cref="File.Replace(string,string,string)"/>.
    /// If <paramref name="dst"/> does not yet exist, falls back to a plain move
    /// (the very-first save case).
    /// </summary>
    void Replace(string src, string dst, string backupTarget);
}

/// <summary>
/// Production <see cref="IFileSystem"/> backed by <see cref="File"/>. Routes
/// every byte through the real OS — used at runtime, and in one explicit
/// EditMode test that confirms the disk path actually persists.
/// </summary>
public sealed class DiskFileStore : IFileSystem
{
    public byte[] Read(string path) => File.ReadAllBytes(path);

    public void Write(string path, byte[] bytes)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        // .NET's WriteAllBytes already creates+truncates; sufficient for the temp file the
        // atomic write protocol writes to. Real durability comes from Replace below.
        File.WriteAllBytes(path, bytes);
    }

    public bool Exists(string path) => File.Exists(path);

    public void Delete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    public void Copy(string src, string dst)
    {
        var dir = Path.GetDirectoryName(dst);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        File.Copy(src, dst, overwrite: true);
    }

    public void Replace(string src, string dst, string backupTarget)
    {
        if (File.Exists(dst))
        {
            // File.Replace is atomic on iOS/Android (single rename syscall) and
            // populates backupTarget with the prior dst contents in one shot.
            File.Replace(src, dst, backupTarget);
        }
        else
        {
            // First-save path: no prior primary, so nothing to back up.
            var dir = Path.GetDirectoryName(dst);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir!);
            File.Move(src, dst);
        }
    }
}
