// QA — SaveService EditMode tests (Wave-4 dispatch additions)
// Subject under test: Brave.Systems.Save.SaveService against InMemoryFileSystem.
// Complements SaveServiceTests.cs (which uses a real Path.GetTempPath() dir);
// these tests exercise the same code paths through the IFileSystem indirection
// so they're hermetic, deterministic, and don't depend on real-disk timing.
//
// Coverage:
//   * Fresh-start when no file exists
//   * Save → Load round-trip preserves data
//   * Backup rotation: save twice, simulate primary corruption, load → bak content
//   * Both primary and bak corrupt → returns fresh default, doesn't throw
//   * Saved event fires on successful save
//   * Async wrappers report success/failure correctly
//   * ClearAll wipes every backup

#nullable enable

using System.Threading.Tasks;
using Brave.Systems.Save;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.Save
{
    [TestFixture]
    public class SaveServiceFileStoreTests
    {
        private const string RootDir = "/virt/brave";
        private const long ExpectedCarrots = 4242L;
        private const long ExpectedStars = 17L;

        private InMemoryFileSystem _fs = null!;
        private SaveService _svc = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _svc = new SaveService(RootDir, _fs);
        }

        // ---- fresh start ----

        [Test]
        public void Load_NoFileExists_PopulatesFreshDefaults()
        {
            _svc.Load();

            Assert.That(_svc.Current, Is.SameAs(_svc.Data), "Current must alias Data");
            Assert.That(_svc.Data.Player.DisplayName, Is.EqualTo("Player"));
            Assert.That(_svc.Data.Currencies.Carrots, Is.EqualTo(0L));
            Assert.That(_svc.Data.Characters.ContainsKey("bunny"), Is.True,
                "fresh default must include the starter character");
            Assert.That(_svc.Data.Characters["bunny"].EquippedWeaponSlug, Is.EqualTo("carrot-boomerang"));
            Assert.That(_svc.Data.Version, Is.EqualTo(SaveHeader.CurrentVersion));
        }

        // ---- round-trip ----

        [Test]
        public void Save_ThenLoad_PreservesData()
        {
            _svc.Load();
            _svc.Data.Currencies.Carrots = ExpectedCarrots;
            _svc.Data.Currencies.Stars = ExpectedStars;
            _svc.Data.Player.DisplayName = "Onal";
            _svc.Data.Settings.AudioMaster = 0.42f;
            _svc.Data.Settings.HapticsEnabled = false;
            _svc.Save();

            // Fresh service over the same filesystem — fully exercises the load path.
            var reloaded = new SaveService(RootDir, _fs);
            reloaded.Load();

            Assert.That(reloaded.Data.Currencies.Carrots, Is.EqualTo(ExpectedCarrots));
            Assert.That(reloaded.Data.Currencies.Stars, Is.EqualTo(ExpectedStars));
            Assert.That(reloaded.Data.Player.DisplayName, Is.EqualTo("Onal"));
            Assert.That(reloaded.Data.Settings.AudioMaster, Is.EqualTo(0.42f).Within(1e-4f));
            Assert.That(reloaded.Data.Settings.HapticsEnabled, Is.False);
        }

        // ---- Saved event ----

        [Test]
        public void Save_FiresSavedEvent_WithCurrentData()
        {
            _svc.Load();
            SaveData? observed = null;
            _svc.Saved += d => observed = d;

            _svc.Save();

            Assert.That(observed, Is.Not.Null, "Saved event must fire after successful save");
            Assert.That(observed, Is.SameAs(_svc.Current), "Saved event must hand back the live model");
        }

        // ---- backup rotation + corruption fallback ----

        [Test]
        public void Save_TwiceThenCorruptPrimary_LoadFallsBackToBak1()
        {
            // First save establishes the primary.
            _svc.Load();
            _svc.Data.Currencies.Carrots = ExpectedCarrots;
            _svc.Save();

            // Second save shunts the first save into bak.1 and writes a new primary.
            _svc.Data.Currencies.Carrots = ExpectedCarrots + 999;
            _svc.Save();

            // Both files exist.
            Assert.That(_fs.Exists("/virt/brave/save_0.dat"), Is.True);
            Assert.That(_fs.Exists("/virt/brave/save_0.dat.bak.1"), Is.True);

            // Corrupt the primary payload (skip the 14-byte header).
            _fs.Corrupt("/virt/brave/save_0.dat", SaveHeader.Size + 4);

            var reloaded = new SaveService(RootDir, _fs);
            reloaded.Load();

            // bak.1 holds the FIRST save (carrots=ExpectedCarrots) because the
            // second save shunted it into bak.1.
            Assert.That(reloaded.Data.Currencies.Carrots, Is.EqualTo(ExpectedCarrots),
                "loader must fall back to bak.1 when primary CRC fails");
        }

        [Test]
        public void Save_PrimaryAndBak1Corrupt_FallsThroughToBak2()
        {
            // Establish primary + bak.1 + bak.2 (third save fills the rotation).
            _svc.Load();
            _svc.Data.Currencies.Carrots = 1; _svc.Save();   // primary=1
            _svc.Data.Currencies.Carrots = 2; _svc.Save();   // primary=2, bak.1=1
            _svc.Data.Currencies.Carrots = 3; _svc.Save();   // primary=3, bak.1=2, bak.2=1

            // Corrupt primary AND bak.1 → loader must find bak.2 (carrots=1).
            _fs.Corrupt("/virt/brave/save_0.dat", SaveHeader.Size + 4);
            _fs.Corrupt("/virt/brave/save_0.dat.bak.1", SaveHeader.Size + 4);

            var reloaded = new SaveService(RootDir, _fs);
            reloaded.Load();

            Assert.That(reloaded.Data.Currencies.Carrots, Is.EqualTo(1L),
                "after two corruptions the loader must keep walking down the backup chain");
        }

        [Test]
        public void Save_AllBackupsCorrupt_ReturnsFreshDefault_NoThrow()
        {
            // Establish a full rotation, then corrupt everything.
            _svc.Load();
            _svc.Data.Currencies.Carrots = 1; _svc.Save();
            _svc.Data.Currencies.Carrots = 2; _svc.Save();
            _svc.Data.Currencies.Carrots = 3; _svc.Save();
            _svc.Data.Currencies.Carrots = 4; _svc.Save();   // primary=4, bak.1=3, bak.2=2, bak.3=1

            _fs.Corrupt("/virt/brave/save_0.dat", SaveHeader.Size + 4);
            _fs.Corrupt("/virt/brave/save_0.dat.bak.1", SaveHeader.Size + 4);
            _fs.Corrupt("/virt/brave/save_0.dat.bak.2", SaveHeader.Size + 4);
            _fs.Corrupt("/virt/brave/save_0.dat.bak.3", SaveHeader.Size + 4);

            var reloaded = new SaveService(RootDir, _fs);
            Assert.DoesNotThrow(() => reloaded.Load(), "loader must NEVER throw — game keeps running");
            Assert.That(reloaded.Data.Currencies.Carrots, Is.EqualTo(0L),
                "all candidates corrupt → fresh DefaultSaveFactory state");
            Assert.That(reloaded.Data.Characters.ContainsKey("bunny"), Is.True);
        }

        // ---- async wrappers ----

        [Test]
        public async Task LoadAsync_NoFile_ReturnsFalse_AndPopulatesDefaults()
        {
            var ok = await _svc.LoadAsync();
            Assert.That(ok, Is.False, "no save file on disk → LoadAsync reports false");
            Assert.That(_svc.Current.Player.DisplayName, Is.EqualTo("Player"));
        }

        [Test]
        public async Task LoadAsync_AfterPriorSave_ReturnsTrue()
        {
            _svc.Load();
            _svc.Data.Currencies.Carrots = ExpectedCarrots;
            _svc.Save();

            var reloaded = new SaveService(RootDir, _fs);
            var ok = await reloaded.LoadAsync();

            Assert.That(ok, Is.True);
            Assert.That(reloaded.Current.Currencies.Carrots, Is.EqualTo(ExpectedCarrots));
        }

        [Test]
        public async Task SaveAsync_HappyPath_ReturnsTrue()
        {
            _svc.Load();
            _svc.Data.Currencies.Stars = 99;
            var ok = await _svc.SaveAsync();

            Assert.That(ok, Is.True);
            Assert.That(_fs.Exists("/virt/brave/save_0.dat"), Is.True);
        }

        // ---- ClearAll ----

        [Test]
        public void ClearAll_RemovesPrimaryAndAllBackups_AndResetsModel()
        {
            _svc.Load();
            _svc.Data.Currencies.Carrots = 1; _svc.Save();
            _svc.Data.Currencies.Carrots = 2; _svc.Save();
            _svc.Data.Currencies.Carrots = 3; _svc.Save();
            _svc.Data.Currencies.Carrots = 4; _svc.Save();

            _svc.ClearAll();

            Assert.That(_fs.Exists("/virt/brave/save_0.dat"), Is.False);
            Assert.That(_fs.Exists("/virt/brave/save_0.dat.bak.1"), Is.False);
            Assert.That(_fs.Exists("/virt/brave/save_0.dat.bak.2"), Is.False);
            Assert.That(_fs.Exists("/virt/brave/save_0.dat.bak.3"), Is.False);
            Assert.That(_svc.Current.Currencies.Carrots, Is.EqualTo(0L),
                "ClearAll must reseat model from DefaultSaveFactory");
        }

        // ---- null-arg guards at API boundary ----

        [Test]
        public void Ctor_NullFileSystem_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new SaveService(RootDir, null!));
        }

        [Test]
        public void Ctor_NullRootDir_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new SaveService(null!, new InMemoryFileSystem()));
        }
    }
}
