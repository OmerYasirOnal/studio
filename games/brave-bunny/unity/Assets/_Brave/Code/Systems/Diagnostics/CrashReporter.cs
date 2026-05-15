// Brave Bunny — Systems / Diagnostics / CrashReporter
// Wave 11 — local-only client-side crash capture. No paid SDKs.
//
// Lifecycle:
//   * Construct at boot with a target directory (Application.persistentDataPath/crashes
//     by default). The reporter subscribes to Application.logMessageReceived and
//     captures Exception / Error / Assert log types into JSON files.
//   * Every Debug.Log* call is funneled through the same hook so a rolling
//     50-line CrashLogBuffer can decorate the next crash with context.
//   * On every captured crash, a CrashReport JSON is atomically written
//     (tmp → rename) and the oldest reports are rotated out (cap = 20).
//
// Privacy contract (Wave 11):
//   * NO save data, NO user-typed strings, NO display name.
//   * Stack trace, log message, device fingerprint, last-N log lines only.
//
// TODO(soft-launch): replace the placeholder mailto target inside
// CrashReportUploader once the support inbox is provisioned. See top-of-file
// comment in CrashReportUploader.cs.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Brave.Systems.Context;
using Newtonsoft.Json;
using UnityEngine;

namespace Brave.Systems.Diagnostics
{
    /// <summary>
    /// Abstraction over the few static UnityEngine surfaces the reporter touches.
    /// Lets EditMode tests drive the reporter without spinning up a player loop.
    /// </summary>
    public interface ICrashEnvironment
    {
        string UnityVersion { get; }
        string AppVersion { get; }
        string BuildGuid { get; }
        string DeviceModel { get; }
        string OperatingSystem { get; }
        int SystemMemoryMb { get; }
        string ProcessorType { get; }
        int ProcessorCount { get; }
        string GraphicsDeviceName { get; }
        DateTime UtcNow { get; }
    }

    /// <summary>Default <see cref="ICrashEnvironment"/> that reads from UnityEngine static surfaces.</summary>
    public sealed class UnityCrashEnvironment : ICrashEnvironment
    {
        public string UnityVersion => Application.unityVersion;
        public string AppVersion => Application.version;
        public string BuildGuid => Application.buildGUID;
        public string DeviceModel => SystemInfo.deviceModel;
        public string OperatingSystem => SystemInfo.operatingSystem;
        public int SystemMemoryMb => SystemInfo.systemMemorySize;
        public string ProcessorType => SystemInfo.processorType;
        public int ProcessorCount => SystemInfo.processorCount;
        public string GraphicsDeviceName => SystemInfo.graphicsDeviceName;
        public DateTime UtcNow => DateTime.UtcNow;
    }

    /// <summary>
    /// Boot-time service that captures unhandled exceptions / errors via
    /// <see cref="Application.logMessageReceived"/> and serializes them to disk.
    /// Implements <see cref="IDisposable"/> so the boot scope can deterministically
    /// unhook the static event (mandatory in EditMode tests).
    /// </summary>
    public sealed class CrashReporter : IService, IDisposable
    {
        public const string CrashFilePrefix = "crash-";
        public const string CrashFileExtension = ".json";
        public const string CrashTimestampFormat = "yyyy-MM-ddTHH-mm-ssZ";
        public const int DefaultMaxStoredReports = 20;

        private readonly string _crashDir;
        private readonly ICrashEnvironment _env;
        private readonly CrashLogBuffer _buffer;
        private readonly int _maxStoredReports;

        // Optional sink that swallows the raw event for in-process EditMode tests
        // (bypasses Application.logMessageReceived which only fires inside a player).
        private readonly Action<Action<string, string, LogType>>? _eventBinder;

        private bool _subscribed;
        private bool _disposed;

        public string CrashDirectory => _crashDir;
        public CrashLogBuffer Buffer => _buffer;
        public int MaxStoredReports => _maxStoredReports;
        public bool HasUnsentReports => Directory.Exists(_crashDir) && EnumerateReportFiles().Any();

        /// <summary>
        /// Construct + immediately subscribe to <see cref="Application.logMessageReceived"/>.
        /// Pass a custom binder in tests to avoid the static event coupling.
        /// </summary>
        public CrashReporter(
            string crashDirectory,
            ICrashEnvironment? environment = null,
            int maxStoredReports = DefaultMaxStoredReports,
            int logBufferCapacity = CrashLogBuffer.DefaultCapacity,
            Action<Action<string, string, LogType>>? eventBinder = null)
        {
            if (string.IsNullOrWhiteSpace(crashDirectory))
                throw new ArgumentException("crashDirectory must be non-empty", nameof(crashDirectory));
            if (maxStoredReports <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStoredReports), "must be > 0");

            _crashDir = crashDirectory;
            _env = environment ?? new UnityCrashEnvironment();
            _maxStoredReports = maxStoredReports;
            _buffer = new CrashLogBuffer(logBufferCapacity);
            _eventBinder = eventBinder;

            Directory.CreateDirectory(_crashDir);

            Subscribe();
        }

        /// <summary>Default crash directory under <see cref="Application.persistentDataPath"/>.</summary>
        public static string DefaultCrashDirectory()
            => Path.Combine(Application.persistentDataPath, "crashes");

        // ---- Subscription ----

        private void Subscribe()
        {
            if (_subscribed) return;
            if (_eventBinder != null)
            {
                _eventBinder(HandleLog);
            }
            else
            {
                Application.logMessageReceived += HandleLog;
            }
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            if (_eventBinder == null)
            {
                Application.logMessageReceived -= HandleLog;
            }
            _subscribed = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            Unsubscribe();
            _disposed = true;
        }

        // ---- Event hook ----

        /// <summary>
        /// Direct test entry point — feeds a log event without requiring the
        /// Unity static event. The production path arrives here via the
        /// <see cref="Application.logMessageReceived"/> subscription.
        /// </summary>
        public void HandleLog(string condition, string stackTrace, LogType type)
        {
            // Always buffer the line so non-fatal Debug.Log noise still provides context.
            _buffer.Push(new CrashLogLine(
                condition ?? string.Empty,
                stackTrace ?? string.Empty,
                type,
                _env.UtcNow));

            if (!IsFatal(type)) return;

            try
            {
                var report = BuildReport(condition ?? string.Empty, stackTrace ?? string.Empty, type);
                WriteReport(report);
                RotateOldReports();
            }
            catch
            {
                // We are running inside the global log handler — swallowing here is
                // intentional. Letting an exception escape would recursively re-enter
                // logMessageReceived and crash the player. There is no upstream caller
                // to surface the error to in this context.
            }
        }

        private static bool IsFatal(LogType type)
            => type == LogType.Exception || type == LogType.Error || type == LogType.Assert;

        // ---- Report building / persistence ----

        public CrashReport BuildReport(string message, string stackTrace, LogType type)
        {
            var now = _env.UtcNow;
            var report = new CrashReport
            {
                Id = Ulid.NewUlid(now),
                Timestamp = now.ToString("o"),
                LogType = type.ToString(),
                Message = message,
                StackTrace = stackTrace,
                UnityVersion = _env.UnityVersion ?? string.Empty,
                AppVersion = _env.AppVersion ?? string.Empty,
                BuildGuid = _env.BuildGuid ?? string.Empty,
                Device = new CrashDeviceInfo
                {
                    Model = _env.DeviceModel ?? string.Empty,
                    OperatingSystem = _env.OperatingSystem ?? string.Empty,
                    SystemMemoryMb = _env.SystemMemoryMb,
                    ProcessorType = _env.ProcessorType ?? string.Empty,
                    ProcessorCount = _env.ProcessorCount,
                    GraphicsDeviceName = _env.GraphicsDeviceName ?? string.Empty,
                },
                ContextLog = BuildContextLog(),
                Sent = false,
            };
            return report;
        }

        private List<CrashLogEntry> BuildContextLog()
        {
            var snapshot = _buffer.Snapshot();
            var list = new List<CrashLogEntry>(snapshot.Count);
            foreach (var line in snapshot)
            {
                list.Add(new CrashLogEntry
                {
                    Timestamp = line.UtcTimestamp.ToString("o"),
                    LogType = line.Type.ToString(),
                    Message = line.Message,
                    StackTrace = line.StackTrace,
                });
            }
            return list;
        }

        /// <summary>Atomically write the report (tmp → rename) under the crash directory.</summary>
        public string WriteReport(CrashReport report)
        {
            Directory.CreateDirectory(_crashDir);

            var fileName = $"{CrashFilePrefix}{_env.UtcNow.ToString(CrashTimestampFormat)}-{report.Id}{CrashFileExtension}";
            var finalPath = Path.Combine(_crashDir, fileName);
            var tmpPath = finalPath + ".tmp";

            var json = JsonConvert.SerializeObject(report, Formatting.Indented);
            File.WriteAllText(tmpPath, json);
            if (File.Exists(finalPath)) File.Delete(finalPath);
            File.Move(tmpPath, finalPath);
            return finalPath;
        }

        /// <summary>
        /// Keep at most <see cref="MaxStoredReports"/> files in the crash dir.
        /// Oldest (lowest file-name lexicographic order — timestamps embed natural sort) wins.
        /// </summary>
        public int RotateOldReports()
        {
            if (!Directory.Exists(_crashDir)) return 0;
            var files = EnumerateReportFiles()
                .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
                .ToList();
            int deleted = 0;
            while (files.Count > _maxStoredReports)
            {
                try
                {
                    File.Delete(files[0]);
                    deleted++;
                }
                catch
                {
                    // Ignore individual delete failures — we'll retry on the next crash.
                }
                files.RemoveAt(0);
            }
            return deleted;
        }

        /// <summary>Enumerate crash report files (ignoring tmp/in-flight writes).</summary>
        public IEnumerable<string> EnumerateReportFiles()
        {
            if (!Directory.Exists(_crashDir)) return Array.Empty<string>();
            return Directory.EnumerateFiles(_crashDir, CrashFilePrefix + "*" + CrashFileExtension);
        }

        /// <summary>
        /// Locally-generated ULID (Crockford base32, 26 chars). Time + cryptographic
        /// random bytes — opaque, non-correlatable. We avoid pulling in a third-party
        /// dependency for one method.
        /// </summary>
        internal static class Ulid
        {
            // Crockford base32 alphabet (no I, L, O, U to dodge ambiguous chars).
            private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

            public static string NewUlid(DateTime utcNow)
            {
                long ms = (long)(utcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var rand = new byte[10];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(rand);
                }

                // 26 chars: 10 chars of time (48 bits) + 16 chars of randomness (80 bits).
                var chars = new char[26];
                for (int i = 9; i >= 0; i--)
                {
                    chars[i] = Alphabet[(int)(ms & 31)];
                    ms >>= 5;
                }
                ulong hi = ((ulong)rand[0] << 32) | ((ulong)rand[1] << 24) | ((ulong)rand[2] << 16) | ((ulong)rand[3] << 8) | rand[4];
                ulong lo = ((ulong)rand[5] << 32) | ((ulong)rand[6] << 24) | ((ulong)rand[7] << 16) | ((ulong)rand[8] << 8) | rand[9];
                for (int i = 17; i >= 10; i--)
                {
                    chars[i] = Alphabet[(int)(lo & 31)];
                    lo >>= 5;
                }
                for (int i = 25; i >= 18; i--)
                {
                    chars[i] = Alphabet[(int)(hi & 31)];
                    hi >>= 5;
                }
                return new string(chars);
            }
        }
    }
}
