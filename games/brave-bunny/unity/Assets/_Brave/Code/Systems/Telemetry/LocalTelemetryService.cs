// Brave Bunny — Systems / Telemetry
//
// Local-only, file-based JSONL event logger used pre-soft-launch to track D1
// retention + funnel signals without any paid analytics SDK. Writes one JSON
// object per line to:
//     <persistentDataPath>/telemetry-<yyyy-MM-dd>.jsonl
//
// Cross-refs:
//   * CLAUDE.md — observability-default-on; zero external paid API.
//   * docs/06-tech-spec/03-save-system.md — persistentDataPath conventions.
//   * Brave.Systems.Analytics.AnalyticsService — *real-sdk* path. This service
//     is the stop-gap layer that runs alongside until a free-tier SDK is wired
//     post-soft-launch (Firebase Analytics or Unity Analytics, both free).
//
// Design choices:
//   * No allocation per event during gameplay: events live in a List<T> buffer
//     and only render to JSON during Flush.
//   * Auto-flush every 5 events (cheap counter check); explicit Flush() on app
//     pause + app quit + Dispose so a force-kill on iOS still leaves the day's
//     events on disk (modulo the 4 most-recent — acceptable for D1 retention).
//   * No external JSON serializer dependency — we hand-format the JSONL bodies
//     so the Newtonsoft assembly is NOT a hard prerequisite for telemetry. The
//     payload shape stays trivial (`{"t":"<type>","ts":"<iso>","f":{...}}`).
//   * File path is computed from <see cref="DateTime.UtcNow"/> on each flush so
//     a long-running session that crosses midnight UTC writes to the new day's
//     file. Acceptable single-process semantic; no rotation race.
//
// NOT in scope:
//   * Upload / network — purely local for now.
//   * PII — events carry no player-identifying data. Run-id (a random GUID per
//     run) is the only correlation key.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Brave.Systems.Context;
using UnityEngine;

namespace Brave.Systems.Telemetry
{
    /// <summary>Public interface so the bridge + tests can swap in a fake.</summary>
    public interface ILocalTelemetryService : IService
    {
        /// <summary>Append an event to the in-memory buffer; auto-flush at threshold.</summary>
        void Log(TelemetryEvent evt);

        /// <summary>Force the buffer to disk (no-op when empty).</summary>
        void Flush();

        /// <summary>Buffered events not yet on disk. Exposed for test assertions.</summary>
        int QueuedCount { get; }
    }

    /// <summary>
    /// Single telemetry event. <see cref="Fields"/> is a free-form bag of
    /// snake_case-keyed primitive values (int / float / bool / string). The
    /// canonical event type names live in <see cref="TelemetryEventTypes"/>.
    /// </summary>
    public readonly struct TelemetryEvent
    {
        /// <summary>Event type slug, e.g. "run_start", "run_end".</summary>
        public readonly string Type;

        /// <summary>UTC timestamp captured at construction.</summary>
        public readonly DateTime TimestampUtc;

        /// <summary>Optional fields. May be null when the event is fact-only.</summary>
        public readonly IReadOnlyDictionary<string, object>? Fields;

        public TelemetryEvent(string type, IReadOnlyDictionary<string, object>? fields = null, DateTime? timestamp = null)
        {
            Type = type;
            Fields = fields;
            TimestampUtc = (timestamp ?? DateTime.UtcNow).ToUniversalTime();
        }
    }

    /// <summary>Canonical event-type slugs. Keep aligned with the analytics schema.</summary>
    public static class TelemetryEventTypes
    {
        public const string RunStart   = "run_start";
        public const string RunEnd     = "run_end";
        public const string LevelUp    = "level_up";
        public const string BossKill   = "boss_kill";
        public const string Death      = "death";
        public const string Purchase   = "purchase";
        public const string AppPause   = "app_pause";
        public const string AppResume  = "app_resume";
        public const string AppQuit    = "app_quit";
    }

    /// <summary>
    /// Pre-analytics-SDK local JSONL logger. Appends events to a per-day file
    /// under <see cref="Application.persistentDataPath"/>. Auto-flushes every
    /// <see cref="DefaultFlushThreshold"/> events; callers should also call
    /// <see cref="Flush"/> on app pause / quit (see TelemetryEventBridge).
    /// </summary>
    public sealed class LocalTelemetryService : ILocalTelemetryService, IDisposable
    {
        /// <summary>Auto-flush every N events. 5 matches the spec.</summary>
        public const int DefaultFlushThreshold = 5;

        /// <summary>Filename prefix; full pattern is "telemetry-yyyy-MM-dd.jsonl".</summary>
        public const string FileNamePrefix = "telemetry-";
        public const string FileNameExtension = ".jsonl";

        private readonly string _directory;
        private readonly int _flushThreshold;
        private readonly List<TelemetryEvent> _buffer;
        private readonly StringBuilder _scratch = new(256);
        private bool _disposed;

        public int QueuedCount => _buffer.Count;

        /// <summary>Public path resolution helper — tests pin the date for determinism.</summary>
        public static string ComputeFilePath(string directory, DateTime utcDay)
        {
            var name = FileNamePrefix + utcDay.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + FileNameExtension;
            return Path.Combine(directory, name);
        }

        /// <summary>Production constructor — writes under Application.persistentDataPath.</summary>
        public LocalTelemetryService() : this(Application.persistentDataPath, DefaultFlushThreshold) { }

        /// <summary>Test constructor — caller pins the directory (temporaryCachePath) and threshold.</summary>
        public LocalTelemetryService(string directory, int flushThreshold = DefaultFlushThreshold)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("directory must be non-empty", nameof(directory));
            if (flushThreshold < 1)
                throw new ArgumentOutOfRangeException(nameof(flushThreshold), "flushThreshold must be >= 1");

            _directory = directory;
            _flushThreshold = flushThreshold;
            _buffer = new List<TelemetryEvent>(capacity: flushThreshold * 2);

            // Idempotent — Directory.CreateDirectory no-ops when the dir exists.
            Directory.CreateDirectory(_directory);
        }

        public void Log(TelemetryEvent evt)
        {
            if (_disposed) return;
            if (string.IsNullOrEmpty(evt.Type)) return; // ignore malformed; never throw on hot path

            _buffer.Add(evt);
            if (_buffer.Count >= _flushThreshold)
                Flush();
        }

        public void Flush()
        {
            if (_disposed) return;
            if (_buffer.Count == 0) return;

            // All events in the buffer share the same flush instant for routing.
            var path = ComputeFilePath(_directory, DateTime.UtcNow);
            _scratch.Length = 0;
            for (int i = 0; i < _buffer.Count; i++)
            {
                AppendJsonLine(_scratch, _buffer[i]);
            }

            try
            {
                File.AppendAllText(path, _scratch.ToString(), Encoding.UTF8);
                _buffer.Clear();
            }
            catch (Exception ex)
            {
                // Never let a disk error kill the game loop. Drop the buffer to
                // avoid unbounded growth; we already lost the events if AppendAllText
                // failed mid-batch, so retrying would only spam.
                Debug.LogWarning($"[LocalTelemetryService] flush failed: {ex.Message}");
                _buffer.Clear();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            try { Flush(); } catch { /* swallow — best-effort */ }
            _disposed = true;
        }

        // ---- JSONL serialization (intentionally minimal — no external deps) ----

        private static void AppendJsonLine(StringBuilder sb, in TelemetryEvent evt)
        {
            sb.Append('{');
            sb.Append("\"t\":");
            AppendJsonString(sb, evt.Type);
            sb.Append(",\"ts\":");
            AppendJsonString(sb, evt.TimestampUtc.ToString("o", CultureInfo.InvariantCulture));
            if (evt.Fields != null && evt.Fields.Count > 0)
            {
                sb.Append(",\"f\":{");
                bool first = true;
                foreach (var kv in evt.Fields)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    AppendJsonString(sb, kv.Key);
                    sb.Append(':');
                    AppendJsonValue(sb, kv.Value);
                }
                sb.Append('}');
            }
            sb.Append('}');
            sb.Append('\n');
        }

        private static void AppendJsonString(StringBuilder sb, string? s)
        {
            sb.Append('"');
            if (!string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            else sb.Append(c);
                            break;
                    }
                }
            }
            sb.Append('"');
        }

        private static void AppendJsonValue(StringBuilder sb, object? v)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case bool b: sb.Append(b ? "true" : "false"); break;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); break;
                case long l: sb.Append(l.ToString(CultureInfo.InvariantCulture)); break;
                case float f: sb.Append(f.ToString("R", CultureInfo.InvariantCulture)); break;
                case double d: sb.Append(d.ToString("R", CultureInfo.InvariantCulture)); break;
                case string s: AppendJsonString(sb, s); break;
                default: AppendJsonString(sb, v.ToString()); break;
            }
        }
    }
}
