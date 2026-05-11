// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (atomic write, corruption recovery, save triggers)
// ADR-0008: Newtonsoft JSON in binary wrapper.

#nullable enable

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Brave.Systems.Context;

namespace Brave.Systems.Save;

public interface ISaveService : IService
{
    SaveData Data { get; }
    void Load();
    void Save();
    void Backup();
    SaveData? GetBackup(int index);
    void ClearAll();
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
    private readonly JsonSerializerSettings _jsonSettings;

    public SaveData Data { get; private set; } = new();

    public SaveService() : this(Application.persistentDataPath) { }

    public SaveService(string rootDir)
    {
        _primaryPath = Path.Combine(rootDir, PrimaryFileName);
        _tempPath = _primaryPath + ".tmp";
        _rotator = new BackupRotator(_primaryPath);
        _migrator = SaveMigrator.Default();
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
            if (!File.Exists(path)) continue;
            try
            {
                Data = ReadFile(path);
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: rejected {path}: {e.Message}");
            }
        }
        // 03-save-system.md: all candidates failed → fall back to defaults.
        Data = DefaultSaveFactory.Create();
    }

    public void Save()
    {
        Data.LastSavedAt = DateTime.UtcNow.ToString("o");
        Data.Version = SaveHeader.CurrentVersion;

        var json = JsonConvert.SerializeObject(Data, _jsonSettings);
        var payload = Encoding.UTF8.GetBytes(json);
        var crc = Crc32.Compute(payload);
        var header = new SaveHeader(SaveHeader.CurrentVersion, (uint)payload.Length, crc).ToBytes();

        // Atomic write: temp → File.Replace into primary (Replace populates bak.1).
        using (var fs = File.Open(_tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fs.Write(header, 0, header.Length);
            fs.Write(payload, 0, payload.Length);
            fs.Flush(flushToDisk: true);
        }
        if (File.Exists(_primaryPath)) File.Replace(_tempPath, _primaryPath, _rotator.PathFor(1));
        else File.Move(_tempPath, _primaryPath);
        _rotator.RotateAfterReplace();
    }

    public void Backup() => Save();

    public SaveData? GetBackup(int index)
    {
        var p = _rotator.PathFor(index);
        if (!File.Exists(p)) return null;
        try { return ReadFile(p); } catch { return null; }
    }

    public void ClearAll()
    {
        if (File.Exists(_primaryPath)) File.Delete(_primaryPath);
        if (File.Exists(_tempPath)) File.Delete(_tempPath);
        _rotator.DeleteAll();
        Data = DefaultSaveFactory.Create();
    }

    // ---------- helpers ----------

    private SaveData ReadFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
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
