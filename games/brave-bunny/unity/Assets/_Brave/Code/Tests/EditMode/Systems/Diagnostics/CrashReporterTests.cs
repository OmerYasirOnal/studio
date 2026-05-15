// QA — CrashReporter EditMode tests
// Subject under test: Brave.Systems.Diagnostics.CrashReporter (+ CrashLogBuffer,
// CrashReport(Uploader)). Wave 11.
//
// Coverage:
//   * Buffer rotation (push beyond capacity → FIFO eviction, chronological order).
//   * Exception/Error capture writes a JSON file under the crash dir.
//   * Non-fatal Log/Warning lines DO NOT spawn a file (only buffered).
//   * 20-report rotation (oldest evicted by lexicographic filename order).
//   * No double-report on restart: a second reporter pointed at the same dir
//     does not re-emit prior reports (files persist as written; uploader
//     decides what to do with them).
//   * Uploader respects opt-in flag — opt-out skips all reports; opt-in opens
//     ONE mailto URL and flips the `sent` flag to prevent re-prompts.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brave.Systems.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Diagnostics
{
    [TestFixture]
    public class CrashReporterTests
    {
        private string _tempDir = null!;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Application.temporaryCachePath, "brave-crash-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
            catch { /* temporaryCachePath is OS-managed — best-effort cleanup */ }
        }

        // ---- Helpers ----

        /// <summary>Fake env so we don't depend on Application static state.</summary>
        private sealed class FakeEnv : ICrashEnvironment
        {
            public string UnityVersion { get; set; } = "6000.0.0f1";
            public string AppVersion { get; set; } = "0.1.0";
            public string BuildGuid { get; set; } = "test-guid";
            public string DeviceModel { get; set; } = "TestDevice,1";
            public string OperatingSystem { get; set; } = "TestOS 1.0";
            public int SystemMemoryMb { get; set; } = 4096;
            public string ProcessorType { get; set; } = "TestCPU";
            public int ProcessorCount { get; set; } = 8;
            public string GraphicsDeviceName { get; set; } = "TestGPU";
            public DateTime UtcNow { get; set; } = new DateTime(2026, 5, 16, 12, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Captures the binder callback so tests can fire log events directly
        /// without depending on Application.logMessageReceived (which is gated
        /// on a running player loop and would couple tests to global state).
        /// </summary>
        private static (CrashReporter reporter, Action<string, string, LogType> fire) NewReporter(
            string dir, FakeEnv? env = null, int maxStored = 20)
        {
            Action<string, string, LogType>? sink = null;
            var reporter = new CrashReporter(
                dir,
                environment: env ?? new FakeEnv(),
                maxStoredReports: maxStored,
                eventBinder: s => sink = s);
            return (reporter, sink!);
        }

        // ---- CrashLogBuffer ----

        [Test]
        public void Buffer_PushBeyondCapacity_DropsOldestFifo()
        {
            var buf = new CrashLogBuffer(capacity: 3);
            for (int i = 0; i < 5; i++)
                buf.Push(new CrashLogLine("msg" + i, "", LogType.Log, DateTime.UtcNow));

            var snap = buf.Snapshot();
            Assert.That(snap.Count, Is.EqualTo(3));
            Assert.That(snap[0].Message, Is.EqualTo("msg2"));
            Assert.That(snap[1].Message, Is.EqualTo("msg3"));
            Assert.That(snap[2].Message, Is.EqualTo("msg4"));
        }

        [Test]
        public void Buffer_BelowCapacity_PreservesChronologicalOrder()
        {
            var buf = new CrashLogBuffer(capacity: 50);
            buf.Push(new CrashLogLine("a", "", LogType.Log, DateTime.UtcNow));
            buf.Push(new CrashLogLine("b", "", LogType.Log, DateTime.UtcNow));
            buf.Push(new CrashLogLine("c", "", LogType.Log, DateTime.UtcNow));

            var snap = buf.Snapshot();
            Assert.That(snap.Select(l => l.Message).ToArray(), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void Buffer_CtorRejectsNonPositiveCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrashLogBuffer(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrashLogBuffer(-1));
        }

        // ---- CrashReporter — capture + persistence ----

        [Test]
        public void Ctor_RejectsEmptyDirectory()
        {
            Assert.Throws<ArgumentException>(() => new CrashReporter(string.Empty));
        }

        [Test]
        public void Ctor_RejectsNonPositiveMaxStored()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CrashReporter(_tempDir, maxStoredReports: 0));
        }

        [Test]
        public void Ctor_CreatesCrashDirectory()
        {
            var nested = Path.Combine(_tempDir, "deep", "nested");
            Assert.That(Directory.Exists(nested), Is.False);
            var (r, _) = NewReporter(nested);
            using (r)
            {
                Assert.That(Directory.Exists(nested), Is.True);
            }
        }

        [Test]
        public void HandleLog_ExceptionType_WritesJsonFile()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("NullReferenceException: boom", "at Foo.Bar()\nat Main()", LogType.Exception);
                var files = Directory.GetFiles(_tempDir, "crash-*.json");
                Assert.That(files.Length, Is.EqualTo(1), "exactly one crash file expected");

                var json = File.ReadAllText(files[0]);
                var report = JsonConvert.DeserializeObject<CrashReport>(json);
                Assert.That(report, Is.Not.Null);
                Assert.That(report!.Message, Does.Contain("boom"));
                Assert.That(report.StackTrace, Does.Contain("Foo.Bar"));
                Assert.That(report.LogType, Is.EqualTo("Exception"));
                Assert.That(report.Id, Is.Not.Empty);
                Assert.That(report.SchemaVersion, Is.EqualTo(CrashReport.CurrentSchemaVersion));
                Assert.That(report.Sent, Is.False);
            }
        }

        [Test]
        public void HandleLog_ErrorType_WritesJsonFile()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("error happened", "stack", LogType.Error);
                Assert.That(Directory.GetFiles(_tempDir, "crash-*.json").Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void HandleLog_NonFatalTypes_DoNotWriteFile()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("info", "", LogType.Log);
                fire("careful", "", LogType.Warning);
                Assert.That(Directory.GetFiles(_tempDir, "crash-*.json").Length, Is.EqualTo(0));
                // But they should still be buffered for context.
                Assert.That(reporter.Buffer.Count, Is.EqualTo(2));
            }
        }

        [Test]
        public void HandleLog_IncludesContextLogFromBuffer()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("breadcrumb-1", "", LogType.Log);
                fire("breadcrumb-2", "", LogType.Warning);
                fire("kaboom", "stack", LogType.Exception);

                var path = Directory.GetFiles(_tempDir, "crash-*.json").Single();
                var report = JsonConvert.DeserializeObject<CrashReport>(File.ReadAllText(path))!;

                // The Exception itself is also buffered before the report is built,
                // so we expect all 3 lines, oldest-first.
                Assert.That(report.ContextLog.Count, Is.EqualTo(3));
                Assert.That(report.ContextLog[0].Message, Is.EqualTo("breadcrumb-1"));
                Assert.That(report.ContextLog[1].Message, Is.EqualTo("breadcrumb-2"));
                Assert.That(report.ContextLog[2].Message, Is.EqualTo("kaboom"));
            }
        }

        [Test]
        public void HandleLog_PopulatesDeviceAndAppFields()
        {
            var env = new FakeEnv
            {
                AppVersion = "9.9.9",
                UnityVersion = "6000.99",
                DeviceModel = "iPhone66,6",
                SystemMemoryMb = 8192,
            };
            var (reporter, fire) = NewReporter(_tempDir, env);
            using (reporter)
            {
                fire("boom", "stk", LogType.Exception);
                var path = Directory.GetFiles(_tempDir, "crash-*.json").Single();
                var report = JsonConvert.DeserializeObject<CrashReport>(File.ReadAllText(path))!;
                Assert.That(report.AppVersion, Is.EqualTo("9.9.9"));
                Assert.That(report.UnityVersion, Is.EqualTo("6000.99"));
                Assert.That(report.Device.Model, Is.EqualTo("iPhone66,6"));
                Assert.That(report.Device.SystemMemoryMb, Is.EqualTo(8192));
            }
        }

        // ---- Report rotation (20 cap) ----

        [Test]
        public void RotateOldReports_KeepsAtMostMaxStored()
        {
            const int max = 5;
            var env = new FakeEnv();
            var (reporter, fire) = NewReporter(_tempDir, env, maxStored: max);
            using (reporter)
            {
                // Advance the clock per crash so file-names sort naturally.
                for (int i = 0; i < max + 4; i++)
                {
                    env.UtcNow = env.UtcNow.AddSeconds(1);
                    fire("kaboom-" + i, "stk", LogType.Exception);
                }
                var files = Directory.GetFiles(_tempDir, "crash-*.json");
                Assert.That(files.Length, Is.EqualTo(max), "rotation should leave exactly maxStored files");
            }
        }

        [Test]
        public void RotateOldReports_EvictsOldestFirst()
        {
            const int max = 3;
            var env = new FakeEnv();
            var (reporter, fire) = NewReporter(_tempDir, env, maxStored: max);
            using (reporter)
            {
                var fileNames = new List<string>();
                for (int i = 0; i < max + 2; i++)
                {
                    env.UtcNow = env.UtcNow.AddSeconds(1);
                    fire("k", "s", LogType.Exception);
                    fileNames.Add(Directory.GetFiles(_tempDir, "crash-*.json")
                        .OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal)
                        .Last());
                }
                var remaining = Directory.GetFiles(_tempDir, "crash-*.json")
                    .Select(Path.GetFileName)
                    .OrderBy(n => n, StringComparer.Ordinal)
                    .ToList();
                Assert.That(remaining.Count, Is.EqualTo(max));
                // The first two writes must be the ones evicted.
                var evictedNames = fileNames.Take(2).Select(Path.GetFileName).ToList();
                foreach (var name in evictedNames)
                    Assert.That(remaining, Does.Not.Contain(name));
            }
        }

        // ---- Second-launch / "no double-report on restart" ----

        [Test]
        public void SecondReporter_OnSameDir_DoesNotRecreateFiles()
        {
            // First "session": fire one crash, keep the file on disk.
            var env = new FakeEnv();
            int initialFileCount;
            {
                var (reporter, fire) = NewReporter(_tempDir, env);
                using (reporter)
                {
                    fire("kaboom", "stk", LogType.Exception);
                }
                initialFileCount = Directory.GetFiles(_tempDir, "crash-*.json").Length;
                Assert.That(initialFileCount, Is.EqualTo(1));
            }

            // Second "session": construct a new reporter without firing anything.
            // It must NOT touch existing files (no double-emit).
            {
                var (reporter2, _) = NewReporter(_tempDir, env);
                using (reporter2)
                {
                    Assert.That(reporter2.HasUnsentReports, Is.True);
                    Assert.That(Directory.GetFiles(_tempDir, "crash-*.json").Length, Is.EqualTo(initialFileCount));
                }
            }
        }

        // ---- Uploader ----

        private sealed class FakeOpener : IUrlOpener
        {
            public List<string> OpenedUrls { get; } = new();
            public void Open(string url) => OpenedUrls.Add(url);
        }

        [Test]
        public void Uploader_OptOut_SkipsAllReports()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("boom", "stk", LogType.Exception);
            }
            var opener = new FakeOpener();
            var up = new CrashReportUploader(_tempDir, opener);

            var sent = up.ProcessUnsentReports(optInEnabled: false);
            Assert.That(sent, Is.EqualTo(0));
            Assert.That(opener.OpenedUrls, Is.Empty);
        }

        [Test]
        public void Uploader_OptIn_OpensMailtoAndMarksSent()
        {
            var (reporter, fire) = NewReporter(_tempDir);
            using (reporter)
            {
                fire("boom", "stk", LogType.Exception);
            }
            var opener = new FakeOpener();
            var up = new CrashReportUploader(_tempDir, opener, "support@example.test");

            var sent = up.ProcessUnsentReports(optInEnabled: true);
            Assert.That(sent, Is.EqualTo(1));
            Assert.That(opener.OpenedUrls.Count, Is.EqualTo(1));
            Assert.That(opener.OpenedUrls[0], Does.StartWith("mailto:support@example.test"));

            // Re-processing must NOT re-prompt — file has been flipped to sent=true.
            var sentAgain = up.ProcessUnsentReports(optInEnabled: true);
            Assert.That(sentAgain, Is.EqualTo(0));
            Assert.That(opener.OpenedUrls.Count, Is.EqualTo(1));
        }

        [Test]
        public void Uploader_MailtoUrl_ContainsEncodedSubjectAndBody()
        {
            var report = new CrashReport
            {
                Id = "01ABC",
                Timestamp = "2026-05-16T12:00:00Z",
                Message = "boom & smoke",
                StackTrace = "at Foo.Bar()",
                AppVersion = "0.1.0",
                UnityVersion = "6000.0.0f1",
                LogType = "Exception",
                Device = new CrashDeviceInfo { Model = "TestDevice,1", OperatingSystem = "TestOS", SystemMemoryMb = 4096 },
            };
            var up = new CrashReportUploader(_tempDir, new FakeOpener(), "ops@example.test");
            var url = up.BuildMailtoUrl(report);

            Assert.That(url, Does.StartWith("mailto:ops@example.test?"));
            Assert.That(url, Does.Contain("subject="));
            Assert.That(url, Does.Contain("body="));
            // Spaces and ampersand must be percent-encoded.
            Assert.That(url, Does.Not.Contain("boom & smoke"));
            Assert.That(url, Does.Contain(Uri.EscapeDataString("boom & smoke")));
        }
    }
}
