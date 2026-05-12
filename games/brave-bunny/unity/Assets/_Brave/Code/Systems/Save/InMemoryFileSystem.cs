// Brave Bunny — Systems / Save
// Wave 4 dispatch (systems-engineer): test-only IFileSystem implementation.
// Lives in the production assembly (no test framework dep) so debug tooling
// can re-use it if it ever wants to run SaveService against a virtual disk.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace Brave.Systems.Save;

/// <summary>
/// In-memory <see cref="IFileSystem"/>. Backing store is a dictionary of
/// path → byte[]. Writes deep-copy the bytes so external mutation of the
/// supplied buffer doesn't bleed into the store.
/// </summary>
public sealed class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new();

    public IReadOnlyCollection<string> Paths => _files.Keys;

    public byte[] Read(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        if (!_files.TryGetValue(path, out var bytes))
            throw new FileNotFoundException($"InMemoryFileSystem: no entry for '{path}'", path);
        var copy = new byte[bytes.Length];
        Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
        return copy;
    }

    public void Write(string path, byte[] bytes)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        var copy = new byte[bytes.Length];
        Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
        _files[path] = copy;
    }

    public bool Exists(string path) => path != null && _files.ContainsKey(path);

    public void Delete(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        _files.Remove(path);
    }

    public void Copy(string src, string dst)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (dst is null) throw new ArgumentNullException(nameof(dst));
        if (!_files.TryGetValue(src, out var bytes))
            throw new FileNotFoundException($"InMemoryFileSystem: no entry for '{src}'", src);
        var copy = new byte[bytes.Length];
        Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);
        _files[dst] = copy;
    }

    public void Replace(string src, string dst, string backupTarget)
    {
        if (src is null) throw new ArgumentNullException(nameof(src));
        if (dst is null) throw new ArgumentNullException(nameof(dst));
        if (backupTarget is null) throw new ArgumentNullException(nameof(backupTarget));
        if (!_files.TryGetValue(src, out var srcBytes))
            throw new FileNotFoundException($"InMemoryFileSystem: no entry for '{src}'", src);

        if (_files.TryGetValue(dst, out var prior))
        {
            // Shunt the prior primary into the backup slot.
            _files[backupTarget] = prior;
        }

        _files[dst] = srcBytes;
        _files.Remove(src);
    }

    /// <summary>Test helper — flip a byte inside a stored payload to simulate corruption.</summary>
    public void Corrupt(string path, int offset)
    {
        if (!_files.TryGetValue(path, out var bytes))
            throw new FileNotFoundException($"InMemoryFileSystem: no entry for '{path}'", path);
        if (offset < 0 || offset >= bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        bytes[offset] = (byte)~bytes[offset];
    }

    /// <summary>Test helper — wipe everything.</summary>
    public void Clear() => _files.Clear();
}
