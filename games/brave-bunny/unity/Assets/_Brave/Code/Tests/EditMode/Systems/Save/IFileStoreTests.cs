// QA — IFileStore / IFileSystem EditMode tests
// Subject under test: Brave.Systems.Save.{IFileStore, IFileSystem,
//                                          DiskFileStore, InMemoryFileSystem}.
// Wave-4 dispatch: confirms the disk + in-memory implementations agree on
// behaviour so SaveService tests can rely on the in-memory store as a faithful
// stand-in for the production disk path.

#nullable enable

using System.IO;
using Brave.Systems.Save;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Systems.Save
{
    [TestFixture]
    public class IFileStoreTests
    {
        private string _tempDir = null!;

        [SetUp]
        public void SetUp()
        {
            // 03-save-system.md: tests must NEVER touch persistentDataPath
            // (it persists across runs and pollutes player saves). Use Unity's
            // temporaryCachePath, which the OS allowed to reclaim at will.
            _tempDir = Path.Combine(Application.temporaryCachePath, "brave-iofs-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); }
            catch { /* ignore — temporaryCachePath is OS-managed */ }
        }

        // ---- InMemoryFileSystem ----

        [Test]
        public void InMemory_WriteThenRead_RoundTrips()
        {
            var fs = new InMemoryFileSystem();
            var payload = new byte[] { 1, 2, 3, 4, 5 };

            fs.Write("a.bin", payload);

            Assert.That(fs.Exists("a.bin"), Is.True);
            Assert.That(fs.Read("a.bin"), Is.EqualTo(payload));
        }

        [Test]
        public void InMemory_WriteDeepCopies_SoCallerMutationDoesNotBleed()
        {
            var fs = new InMemoryFileSystem();
            var payload = new byte[] { 1, 2, 3 };
            fs.Write("a.bin", payload);

            payload[0] = 99;
            Assert.That(fs.Read("a.bin")[0], Is.EqualTo(1), "store must hold a private copy");
        }

        [Test]
        public void InMemory_Delete_RemovesEntry()
        {
            var fs = new InMemoryFileSystem();
            fs.Write("a.bin", new byte[] { 7 });
            fs.Delete("a.bin");
            Assert.That(fs.Exists("a.bin"), Is.False);
        }

        [Test]
        public void InMemory_Delete_MissingPathIsNoOp()
        {
            var fs = new InMemoryFileSystem();
            Assert.DoesNotThrow(() => fs.Delete("never-existed.bin"));
        }

        [Test]
        public void InMemory_Replace_ShuntsPriorPrimaryIntoBackup()
        {
            var fs = new InMemoryFileSystem();
            fs.Write("primary", new byte[] { 1, 1, 1 });
            fs.Write("tmp", new byte[] { 2, 2, 2 });

            fs.Replace("tmp", "primary", "primary.bak.1");

            Assert.That(fs.Read("primary"), Is.EqualTo(new byte[] { 2, 2, 2 }));
            Assert.That(fs.Read("primary.bak.1"), Is.EqualTo(new byte[] { 1, 1, 1 }));
            Assert.That(fs.Exists("tmp"), Is.False, "src must be consumed by Replace");
        }

        [Test]
        public void InMemory_Replace_NoPriorPrimaryFallsBackToMove()
        {
            var fs = new InMemoryFileSystem();
            fs.Write("tmp", new byte[] { 9 });

            fs.Replace("tmp", "primary", "primary.bak.1");

            Assert.That(fs.Read("primary"), Is.EqualTo(new byte[] { 9 }));
            Assert.That(fs.Exists("primary.bak.1"), Is.False, "no prior primary → nothing to back up");
        }

        [Test]
        public void InMemory_ReadMissing_Throws()
        {
            var fs = new InMemoryFileSystem();
            Assert.Throws<FileNotFoundException>(() => fs.Read("nope"));
        }

        // ---- DiskFileStore (end-to-end disk smoke) ----
        // This is the ONE test that touches the real disk per the dispatch.

        [Test]
        public void Disk_WriteRoundTrip_UnderTemporaryCachePath()
        {
            var fs = new DiskFileStore();
            var path = Path.Combine(_tempDir, "brave-disk-smoke.bin");
            var payload = new byte[] { 0xBA, 0xAD, 0xF0, 0x0D };

            fs.Write(path, payload);

            Assert.That(fs.Exists(path), Is.True, "disk file should exist after Write");
            Assert.That(File.Exists(path), Is.True, "the underlying OS file must really be there");
            Assert.That(fs.Read(path), Is.EqualTo(payload));
        }

        [Test]
        public void Disk_Replace_AtomicallySwapsPrimaryAndBackup()
        {
            var fs = new DiskFileStore();
            var primary = Path.Combine(_tempDir, "primary.dat");
            var tmp = Path.Combine(_tempDir, "primary.dat.tmp");
            var bak = Path.Combine(_tempDir, "primary.dat.bak.1");

            fs.Write(primary, new byte[] { 1, 1, 1 });
            fs.Write(tmp, new byte[] { 2, 2, 2 });
            fs.Replace(tmp, primary, bak);

            Assert.That(fs.Read(primary), Is.EqualTo(new byte[] { 2, 2, 2 }));
            Assert.That(fs.Read(bak), Is.EqualTo(new byte[] { 1, 1, 1 }));
            Assert.That(fs.Exists(tmp), Is.False);
        }
    }
}
