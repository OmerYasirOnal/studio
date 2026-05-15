// Brave Bunny — Systems / Diagnostics / CrashReportUploader
// Wave 11 — opt-in email "uploader". Purely client-side; no backend, no paid SDK.
//
// On launch:
//   1. Inspect the crash directory for unsent reports.
//   2. If the user has opted in (SettingsSection.CrashOptInEnabled), hand the
//      first/newest report to the OS's mailto: handler. The user reviews the
//      pre-filled body and decides whether to send.
//   3. Mark the report Sent=true regardless of whether the user clicks send —
//      we surrender control to the OS composer and cannot observe the outcome,
//      and we never want to nag the user twice for the same crash.
//
// TODO(soft-launch): replace the placeholder mailto target with the real
// support inbox once provisioned. The `support@bravebunny.example` literal
// below must be swapped before the soft-launch build.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Brave.Systems.Diagnostics
{
    /// <summary>
    /// Indirection over the platform `Application.OpenURL` so EditMode tests can
    /// assert the composed mailto URL without launching the OS handler.
    /// </summary>
    public interface IUrlOpener
    {
        void Open(string url);
    }

    /// <summary>Default opener that delegates to <see cref="Application.OpenURL(string)"/>.</summary>
    public sealed class UnityUrlOpener : IUrlOpener
    {
        public void Open(string url) => Application.OpenURL(url);
    }

    /// <summary>
    /// Inspects the crash directory for unsent reports and (if opted in) hands
    /// the next one to the OS email composer via <c>mailto:</c>.
    /// </summary>
    public sealed class CrashReportUploader
    {
        // TODO(soft-launch): swap to the real support inbox once provisioned.
        public const string PlaceholderSupportEmail = "support@bravebunny.example";
        public const string MailSubject = "Brave Bunny Crash Report";

        // mailto bodies are capped by every OS — keep the URL under ~2000 chars to
        // be safe across iOS / Android / desktop. We embed a summary; the full
        // JSON stays on disk for advanced support.
        public const int MaxBodyChars = 1800;

        private readonly string _crashDir;
        private readonly IUrlOpener _opener;
        private readonly string _supportEmail;

        public string CrashDirectory => _crashDir;
        public string SupportEmail => _supportEmail;

        public CrashReportUploader(string crashDirectory, IUrlOpener? opener = null, string? supportEmail = null)
        {
            if (string.IsNullOrWhiteSpace(crashDirectory))
                throw new ArgumentException("crashDirectory must be non-empty", nameof(crashDirectory));
            _crashDir = crashDirectory;
            _opener = opener ?? new UnityUrlOpener();
            _supportEmail = string.IsNullOrWhiteSpace(supportEmail) ? PlaceholderSupportEmail : supportEmail!;
        }

        /// <summary>
        /// On-launch entry point. Returns the number of reports prompted for upload
        /// (always 0 or 1 — we surface one composer at a time to avoid spamming).
        /// </summary>
        public int ProcessUnsentReports(bool optInEnabled)
        {
            if (!optInEnabled) return 0;
            if (!Directory.Exists(_crashDir)) return 0;

            var unsent = EnumerateUnsentReports().ToList();
            if (unsent.Count == 0) return 0;

            var path = unsent[0];
            var report = TryLoadReport(path);
            if (report == null) return 0;

            var url = BuildMailtoUrl(report);
            _opener.Open(url);

            // Mark sent regardless of whether the OS composer is honored — we
            // cannot observe the user's send decision and must not re-prompt.
            MarkSent(path, report);
            return 1;
        }

        /// <summary>Enumerate report files where <c>sent == false</c>.</summary>
        public IEnumerable<string> EnumerateUnsentReports()
        {
            if (!Directory.Exists(_crashDir)) yield break;
            foreach (var path in Directory.EnumerateFiles(_crashDir, CrashReporter.CrashFilePrefix + "*" + CrashReporter.CrashFileExtension)
                .OrderBy(p => Path.GetFileName(p), StringComparer.Ordinal))
            {
                var report = TryLoadReport(path);
                if (report != null && !report.Sent)
                    yield return path;
            }
        }

        /// <summary>Build the <c>mailto:</c> URL with a redacted summary body.</summary>
        public string BuildMailtoUrl(CrashReport report)
        {
            var body = BuildBody(report);
            var encodedSubject = Uri.EscapeDataString(MailSubject);
            var encodedBody = Uri.EscapeDataString(body);
            return $"mailto:{_supportEmail}?subject={encodedSubject}&body={encodedBody}";
        }

        public static string BuildBody(CrashReport report)
        {
            var sb = new StringBuilder();
            sb.Append("Brave Bunny — Crash Report\n");
            sb.Append("Id: ").Append(report.Id).Append('\n');
            sb.Append("Time: ").Append(report.Timestamp).Append('\n');
            sb.Append("App: ").Append(report.AppVersion).Append(" (build ").Append(report.BuildGuid).Append(")\n");
            sb.Append("Unity: ").Append(report.UnityVersion).Append('\n');
            sb.Append("Device: ").Append(report.Device.Model).Append(" / ").Append(report.Device.OperatingSystem)
              .Append(" / ").Append(report.Device.SystemMemoryMb).Append(" MB\n");
            sb.Append("Type: ").Append(report.LogType).Append('\n');
            sb.Append("\n--- Message ---\n").Append(report.Message).Append('\n');
            sb.Append("\n--- Stack ---\n").Append(report.StackTrace).Append('\n');

            // Truncate to mailto-safe size — the full JSON remains on disk.
            if (sb.Length > MaxBodyChars)
            {
                sb.Length = MaxBodyChars;
                sb.Append("\n...[truncated; full report retained locally]");
            }
            return sb.ToString();
        }

        public static CrashReport? TryLoadReport(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<CrashReport>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void MarkSent(string path, CrashReport report)
        {
            report.Sent = true;
            try
            {
                var json = JsonConvert.SerializeObject(report, Formatting.Indented);
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch
            {
                // Best-effort: if we can't persist the sent flag, the worst case
                // is re-prompting the user on the next launch. Not fatal.
            }
        }
    }
}
