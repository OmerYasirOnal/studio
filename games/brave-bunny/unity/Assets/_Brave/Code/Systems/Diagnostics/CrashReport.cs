// Brave Bunny — Systems / Diagnostics / CrashReport
// Wave 11 — POCO payload written to disk under persistentDataPath/crashes/<id>.json.
// All fields use [JsonProperty] for rename-safe forward-compat (ADR-0008).
//
// Privacy contract (Wave 11):
//   * NO save data, NO user-typed strings, NO display name, NO analytics IDs.
//   * Captured surface: stack trace, Unity log message, Unity version, app version,
//     device model/OS/RAM, and the last N Debug.Log lines surrounding the crash.
//   * The id is a locally-generated ULID — non-correlatable across installs.

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Brave.Systems.Diagnostics
{
    /// <summary>
    /// Device fingerprint snapshot — strictly non-PII (model name, OS string,
    /// total RAM). No advertising id, no vendor id, no IP.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CrashDeviceInfo
    {
        [JsonProperty("model")] public string Model = string.Empty;
        [JsonProperty("os")] public string OperatingSystem = string.Empty;
        [JsonProperty("ramMb")] public int SystemMemoryMb;
        [JsonProperty("processorType")] public string ProcessorType = string.Empty;
        [JsonProperty("processorCount")] public int ProcessorCount;
        [JsonProperty("graphicsDeviceName")] public string GraphicsDeviceName = string.Empty;
    }

    /// <summary>One captured log line — mirrors <see cref="CrashLogLine"/> in a JSON-friendly shape.</summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CrashLogEntry
    {
        [JsonProperty("timestamp")] public string Timestamp = string.Empty;
        [JsonProperty("type")] public string LogType = string.Empty;
        [JsonProperty("message")] public string Message = string.Empty;
        [JsonProperty("stack")] public string StackTrace = string.Empty;
    }

    /// <summary>
    /// Crash report root POCO. Serialized as <c>crash-&lt;yyyy-MM-ddTHH-mm-ssZ&gt;.json</c>
    /// under <c>Application.persistentDataPath/crashes/</c>.
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class CrashReport
    {
        public const int CurrentSchemaVersion = 1;

        [JsonProperty("schemaVersion")] public int SchemaVersion = CurrentSchemaVersion;

        /// <summary>Locally-generated ULID — opaque, non-correlatable across installs.</summary>
        [JsonProperty("id")] public string Id = string.Empty;

        /// <summary>ISO-8601 UTC ("o" format) timestamp the crash was captured at.</summary>
        [JsonProperty("timestamp")] public string Timestamp = string.Empty;

        /// <summary>Unity-emitted log type (Exception / Error / Assert).</summary>
        [JsonProperty("logType")] public string LogType = string.Empty;

        /// <summary>The exception/error message that triggered capture (no PII per policy).</summary>
        [JsonProperty("message")] public string Message = string.Empty;

        /// <summary>Full stack trace as supplied by Application.logMessageReceived.</summary>
        [JsonProperty("stackTrace")] public string StackTrace = string.Empty;

        [JsonProperty("unityVersion")] public string UnityVersion = string.Empty;
        [JsonProperty("appVersion")] public string AppVersion = string.Empty;
        [JsonProperty("buildGuid")] public string BuildGuid = string.Empty;

        [JsonProperty("device")] public CrashDeviceInfo Device = new();

        /// <summary>
        /// Last-N Debug.Log lines preceding the crash. Provides debugging context.
        /// </summary>
        [JsonProperty("contextLog")] public List<CrashLogEntry> ContextLog = new();

        /// <summary>
        /// Marker the uploader flips once a report has been handed to the OS email
        /// composer. Prevents the same crash from re-prompting on every launch.
        /// </summary>
        [JsonProperty("sent")] public bool Sent;
    }
}
