// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (atomic write, corruption recovery, save triggers)
// ADR-0008: Newtonsoft JSON in binary wrapper.
//
// Wave-4 additions (systems-engineer):
//   * IFileSystem indirection so EditMode tests can run without touching disk.
//   * Current alias + Saved event + LoadAsync/SaveAsync wrappers for callers
//     that prefer the Task<bool> contract (the new UI screens land on these).
//   * All file I/O routed through the injected IFileSystem; behaviour against
//     the real disk is unchanged.

#nullable enable

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Brave.Systems.Context;

namespace Brave.Systems.Save;

public interface ISaveService : IService
{
    SaveData Data { get; }
    /// <summary>Dispatch-requested alias for <see cref="Data"/>.</summary>
    SaveData Current { get; }
    /// <summary>Raised after every successful <see cref="Save"/> / <see cref="SaveAsync"/>.</summary>
    event Action<SaveData>? Saved;

    void Load();
    void Save();
    void Backup();
    SaveData? GetBackup(int index);
    void ClearAll();

    /// <summary>
    /// Async wrapper. Returns true on success, false on any failure (missing
    /// file or corrupt → <see cref="Current"/> is set to fresh defaults so the
    /// game can keep running).
    /// </summary>
    Task<bool> LoadAsync();

    /// <summary>Async wrapper. Returns true if the save committed, false on I/O failure.</summary>
    Task<bool> SaveAsync();
}

/// <summary>
/// Local-only save service. Atomic write-temp + rename, CRC32 validation,
/// 3 rolling backups, JObject-based migrations. See ADR-0008.
/// </summary>
public sealed class SaveService : ISaveService
{
    private const string PrimaryFileName = "save_0.dat";

    private readonly string _primaryPath;
    private readonly string _tempPath;
    private readonly BackupRotator _rotator;
    private readonly SaveMigrator _migrator;
    private readonly IFileSystem _fs;
    private readonly JsonSerializerSettings _jsonSettings;

    /// <summary>
    /// True iff the most recent <see cref="Load"/> returned data from disk
    /// (any backup tier counts) rather than falling all the way through to
    /// <see cref="DefaultSaveFactory"/>. Used by <see cref="LoadAsync"/> to
    /// produce its <c>bool</c> contract.
    /// </summary>
    private bool _lastLoadFromDisk;

    public SaveData Data { get; private set; } = new();

    /// <summary>Dispatch-requested alias for <see cref="Data"/>. Same reference.</summary>
    public SaveData Current => Data;

    public event Action<SaveData>? Saved;

    public SaveService() : this(Application.persistentDataPath, new DiskFileStore()) { }

    public SaveService(string rootDir) : this(rootDir, new DiskFileStore()) { }

    public SaveService(string rootDir, IFileSystem fileSystem)
    {
        if (rootDir is null) throw new ArgumentNullException(nameof(rootDir));
        if (fileSystem is null) throw new ArgumentNullException(nameof(fileSystem));

        _primaryPath = Path.Combine(rootDir, PrimaryFileName);
        _tempPath = _primaryPath + ".tmp";
        _rotator = new BackupRotator(_primaryPath, fileSystem);
        _migrator = SaveMigrator.Default();
        _fs = fileSystem;
        _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.None,
        };
    }

    public void Load()
    {
        for (var i = 0; i <= BackupRotator.BackupCount; i++)
        {
            var path = _rotator.PathFor(i);
            if (!_fs.Exists(path)) continue;
            try
            {
                Data = ReadFile(path);
                _lastLoadFromDisk = true;
                return;
            }
            catch (Exception e) when (e is InvalidSaveException || e is JsonException || e is IOException)
            {
                // 03-save-system.md: every failure cascades to the next backup.
                // Catch the JsonException base so JsonReaderException AND
                // JsonSerializationException both flow into the fallback path —
                // a payload that parses but fails to bind is just as corrupt.
                Debug.LogWarning($"SaveService: rejected {path}: {e.Message}");
            }
        }
        // All candidates failed → fresh defaults so the game can keep running.
        Data = DefaultSaveFactory.Create();
        _lastLoadFromDisk = false;
    }

    public void Save()
    {
        Data.LastSavedAt = DateTime.UtcNow.ToString("o");
        Data.Version = SaveHeader.CurrentVersion;

        var json = JsonConvert.SerializeObject(Data, _jsonSettings);
        var payload = Encoding.UTF8.GetBytes(json);
        var crc = Crc32.Compute(payload);
        var header = new SaveHeader(SaveHeader.CurrentVersion, (uint)payload.Length, crc).ToBytes();

        var blob = new byte[header.Length + payload.Length];
        Buffer.BlockCopy(header, 0, blob, 0, header.Length);
        Buffer.BlockCopy(payload, 0, blob, header.Length, payload.Length);

        // Atomic write: write temp → Replace (which shunts prior primary into bak.1) → rotate older bakups.
        _fs.Write(_tempPath, blob);
        _fs.Replace(_tempPath, _primaryPath, _rotator.PathFor(1));
        _rotator.RotateAfterReplace();

        Saved?.Invoke(Data);
    }

    public void Backup() => Save();

    public SaveData? GetBackup(int index)
    {
        var p = _rotator.PathFor(index);
        if (!_fs.Exists(p)) return null;
        try { return ReadFile(p); } catch { return null; }
    }

    public void ClearAll()
    {
        _fs.Delete(_primaryPath);
        _fs.Delete(_tempPath);
        _rotator.DeleteAll();
        Data = DefaultSaveFactory.Create();
    }

    public Task<bool> LoadAsync()
    {
        try
        {
            Load();
            // Load() never throws — corruption fallback is internal. Report
            // success iff we actually pulled data off disk (any backup tier);
            // a fall-through to DefaultSaveFactory is reported as false so UI
            // can show "save was reset" prompts per 03-save-system.md.
            return Task.FromResult(_lastLoadFromDisk);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveService.LoadAsync swallowed exception: {e.Message}");
            Data = DefaultSaveFactory.Create();
            _lastLoadFromDisk = false;
            return Task.FromResult(false);
        }
    }

    public Task<bool> SaveAsync()
    {
        try
        {
            Save();
            return Task.FromResult(true);
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            Debug.LogWarning($"SaveService.SaveAsync I/O failure: {e.Message}");
            return Task.FromResult(false);
        }
    }

    // ---------- helpers ----------

    private SaveData ReadFile(string path)
    {
        var bytes = _fs.Read(path);
        if (bytes.Length < SaveHeader.Size) throw new InvalidSaveException("File shorter than header.");
        var header = SaveHeader.FromBytes(bytes);
        if (header.Version > SaveHeader.CurrentVersion)
            throw new InvalidSaveException($"Save version {header.Version} > supported {SaveHeader.CurrentVersion}.");
        if (SaveHeader.Size + header.PayloadLength > bytes.Length)
            throw new InvalidSaveException("Payload length exceeds file size.");

        var payload = new byte[header.PayloadLength];
        Buffer.BlockCopy(bytes, SaveHeader.Size, payload, 0, (int)header.PayloadLength);
        if (Crc32.Compute(payload) != header.PayloadCrc32)
            throw new InvalidSaveException("CRC32 mismatch.");

        var json = Encoding.UTF8.GetString(payload);
        var root = JObject.Parse(json);
        _migrator.Apply(root);
        return root.ToObject<SaveData>(JsonSerializer.Create(_jsonSettings))
               ?? throw new InvalidSaveException("Payload deserialized to null.");
    }
}
