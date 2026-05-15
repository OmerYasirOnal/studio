// QA — LocalTelemetryService EditMode tests
// Subject under test: Brave.Systems.Telemetry.LocalTelemetryService.
//
// Coverage:
//   * File write + JSONL format (one event per line, valid JSON shape).
//   * Auto-flush at threshold (5 events default) + manual Flush() + Dispose flush.
//   * Round-trip read: written lines parse back into the canonical {t, ts, f} shape.
//   * Empty buffer Flush() is a no-op (no zero-byte file written gratuitously).
//   * Field-typed values (int/float/bool/string) serialize to JSON-correct tokens.
//
// Convention: never touch Application.persistentDataPath (per IFileStoreTests
// header). Tests use Application.temporaryCachePath under a per-test GUID dir.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Brave.Systems.Telemetry;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Telemetry
{
    [TestFixture]
    public class LocalTelemetryServiceTests
    {
        private string _tempDir = null!;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Application.temporaryCachePath, "brave-telemetry-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
            catch { /* temporaryCachePath is OS-managed — best-effort cleanup */ }
        }

        private string DayFile() => LocalTelemetryService.ComputeFilePath(_tempDir, DateTime.UtcNow);

        // ---- Construction guards ----

        [Test]
        public void Ctor_RejectsEmptyDirectory()
        {
            Assert.Throws<ArgumentException>(() => new LocalTelemetryService(string.Empty));
        }

        [Test]
        public void Ctor_RejectsZeroOrNegativeFlushThreshold()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalTelemetryService(_tempDir, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LocalTelemetryService(_tempDir, -1));
        }

        [Test]
        public void Ctor_CreatesDirectoryWhenMissing()
        {
            var sub = Path.Combine(_tempDir, "nested", "deep");
            Assert.That(Directory.Exists(sub), Is.False);
            using var svc = new LocalTelemetryService(sub, flushThreshold: 5);
            Assert.That(Directory.Exists(sub), Is.True);
        }

        // ---- Auto-flush threshold ----

        [Test]
        public void Log_BelowThreshold_DoesNotWriteFile()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            for (int i = 0; i < 4; i++)
                svc.Log(new TelemetryEvent("t" + i));

            Assert.That(svc.QueuedCount, Is.EqualTo(4));
            Assert.That(File.Exists(DayFile()), Is.False, "no flush expected before threshold");
        }

        [Test]
        public void Log_AtThreshold_AutoFlushesToDisk()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            for (int i = 0; i < 5; i++)
                svc.Log(new TelemetryEvent("t" + i));

            Assert.That(svc.QueuedCount, Is.EqualTo(0));
            Assert.That(File.Exists(DayFile()), Is.True);
            var lines = File.ReadAllLines(DayFile());
            Assert.That(lines.Length, Is.EqualTo(5));
        }

        [Test]
        public void Flush_EmptyBuffer_IsNoOp_AndDoesNotCreateFile()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            svc.Flush();
            Assert.That(File.Exists(DayFile()), Is.False);
        }

        [Test]
        public void Flush_PartialBuffer_WritesAllEvents()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            svc.Log(new TelemetryEvent("run_start"));
            svc.Log(new TelemetryEvent("level_up"));

            svc.Flush();

            Assert.That(svc.QueuedCount, Is.EqualTo(0));
            var lines = File.ReadAllLines(DayFile());
            Assert.That(lines.Length, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_FlushesPendingBuffer()
        {
            string path;
            {
                var svc = new LocalTelemetryService(_tempDir, flushThreshold: 100);
                svc.Log(new TelemetryEvent("only_event"));
                path = DayFile();
                svc.Dispose();
            }
            Assert.That(File.Exists(path), Is.True);
            Assert.That(File.ReadAllLines(path).Length, Is.EqualTo(1));
        }

        [Test]
        public void PostDispose_LogIsNoOp()
        {
            var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            svc.Dispose();
            Assert.DoesNotThrow(() => svc.Log(new TelemetryEvent("ignored")));
            Assert.That(svc.QueuedCount, Is.EqualTo(0));
        }

        [Test]
        public void Log_EmptyType_IsIgnored()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 5);
            svc.Log(new TelemetryEvent(string.Empty));
            svc.Log(new TelemetryEvent((string)null!));
            Assert.That(svc.QueuedCount, Is.EqualTo(0));
        }

        // ---- JSONL format ----

        [Test]
        public void Log_WritesValidJsonlOneEventPerLine_EndingInNewline()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 2);
            svc.Log(new TelemetryEvent("a"));
            svc.Log(new TelemetryEvent("b"));

            var raw = File.ReadAllText(DayFile());
            Assert.That(raw, Does.EndWith("\n"));
            var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines.Length, Is.EqualTo(2));
            foreach (var line in lines)
            {
                Assert.That(line, Does.StartWith("{"));
                Assert.That(line, Does.EndWith("}"));
                Assert.That(line, Does.Contain("\"t\":"));
                Assert.That(line, Does.Contain("\"ts\":"));
            }
        }

        [Test]
        public void Log_TimestampIsIso8601Utc()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 1);
            var pinned = new DateTime(2026, 5, 16, 12, 34, 56, DateTimeKind.Utc);
            svc.Log(new TelemetryEvent("run_start", null, pinned));

            var line = File.ReadAllLines(DayFile())[0];
            // ISO 8601 "o" round-trip format — fragment match is enough.
            Assert.That(line, Does.Contain("2026-05-16T12:34:56"));
        }

        [Test]
        public void Log_FieldsSerializeTypedJsonTokens()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 1);
            svc.Log(new TelemetryEvent("run_end", new Dictionary<string, object>
            {
                ["kills"]    = 42,
                ["seconds"]  = 13.5f,
                ["won"]      = true,
                ["lost"]     = false,
                ["cause"]    = "hp_zero",
            }));

            var line = File.ReadAllLines(DayFile())[0];
            Assert.That(line, Does.Contain("\"kills\":42"));
            Assert.That(line, Does.Contain("\"won\":true"));
            Assert.That(line, Does.Contain("\"lost\":false"));
            Assert.That(line, Does.Contain("\"cause\":\"hp_zero\""));
            // Float should appear as a number (no quotes around it).
            Assert.That(line, Does.Contain("\"seconds\":13.5"));
        }

        [Test]
        public void Log_EscapesQuotesAndBackslashesInStrings()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 1);
            svc.Log(new TelemetryEvent("note", new Dictionary<string, object>
            {
                ["msg"] = "hello \"world\" \\ ok",
            }));

            var line = File.ReadAllLines(DayFile())[0];
            Assert.That(line, Does.Contain("\\\"world\\\""));
            Assert.That(line, Does.Contain("\\\\"));
        }

        // ---- Round-trip: re-read flushed file and confirm structural integrity ----

        [Test]
        public void Log_RoundTrip_ReadsBackAllEventsInOrder()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 3);
            svc.Log(new TelemetryEvent("run_start"));
            svc.Log(new TelemetryEvent("level_up", new Dictionary<string, object> { ["level"] = 2 }));
            svc.Log(new TelemetryEvent("boss_kill", new Dictionary<string, object> { ["boss_id"] = "old-boar-king" }));

            var lines = File.ReadAllLines(DayFile());
            Assert.That(lines.Length, Is.EqualTo(3));
            Assert.That(lines[0], Does.Contain("\"t\":\"run_start\""));
            Assert.That(lines[1], Does.Contain("\"t\":\"level_up\""));
            Assert.That(lines[1], Does.Contain("\"level\":2"));
            Assert.That(lines[2], Does.Contain("\"t\":\"boss_kill\""));
            Assert.That(lines[2], Does.Contain("\"boss_id\":\"old-boar-king\""));
        }

        [Test]
        public void Log_MultipleFlushes_AppendsRatherThanOverwrites()
        {
            using var svc = new LocalTelemetryService(_tempDir, flushThreshold: 2);
            svc.Log(new TelemetryEvent("a"));
            svc.Log(new TelemetryEvent("b")); // flush #1
            svc.Log(new TelemetryEvent("c"));
            svc.Log(new TelemetryEvent("d")); // flush #2

            var lines = File.ReadAllLines(DayFile());
            Assert.That(lines.Length, Is.EqualTo(4), "second flush must append, not overwrite");
            Assert.That(lines[0], Does.Contain("\"t\":\"a\""));
            Assert.That(lines[3], Does.Contain("\"t\":\"d\""));
        }

        [Test]
        public void ComputeFilePath_FormatsYearMonthDayUtc()
        {
            var pinned = new DateTime(2026, 1, 7, 0, 0, 0, DateTimeKind.Utc);
            var p = LocalTelemetryService.ComputeFilePath(_tempDir, pinned);
            var expected = Path.Combine(_tempDir, "telemetry-2026-01-07.jsonl");
            Assert.That(p, Is.EqualTo(expected));
        }
    }
}
