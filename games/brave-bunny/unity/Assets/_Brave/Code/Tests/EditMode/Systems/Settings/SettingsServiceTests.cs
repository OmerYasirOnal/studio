// QA — SettingsService EditMode tests.
// Subject under test: Brave.Systems.Settings.SettingsService.
// Tech spec: docs/06-tech-spec/03-save-system.md (Settings save trigger).
//            docs/06-tech-spec/07-audio.md (3 sliders — linear 0..1 inputs).
//
// Coverage:
//   * Setters clamp linear audio inputs to [0, 1].
//   * Setters raise OnChanged exactly once per call.
//   * Hydrate() reads existing SaveData.Settings into Current.
//   * Commit() flushes Current → SaveData.Settings + triggers SaveService.Save().
//   * Round-trip: persist via Commit() → reload SaveService → SettingsService
//     observes the round-tripped values.
//   * Language ISO mapping round-trips Tr / Id / Ph / En through the save POCO.
//
// Hermetic: every test uses InMemoryFileSystem so disk is never touched.

#nullable enable

using Brave.Systems.Save;
using Brave.Systems.Settings;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.SettingsTests
{
    [TestFixture]
    public class SettingsServiceTests
    {
        private const string RootDir = "/virt/brave-settings";

        // Linear audio inputs (per 07-audio.md).
        private const float LowVolume = 0.25f;
        private const float MidVolume = 0.55f;
        private const float HighVolume = 0.9f;

        // Out-of-range inputs to exercise Clamp01.
        private const float NegativeVolume = -0.5f;
        private const float OverOneVolume = 1.5f;

        // Default values from SettingsData defaults — guarded so a future tweak
        // breaks this test loudly.
        private const float DefaultAudioMaster = 0.8f;
        private const float DefaultAudioMusic = 0.7f;
        private const float DefaultAudioSfx = 0.9f;
        private const bool DefaultHaptics = true;

        private InMemoryFileSystem _fs = null!;
        private SaveService _save = null!;
        private SettingsService _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _fs = new InMemoryFileSystem();
            _save = new SaveService(RootDir, _fs);
            _save.Load(); // populates fresh defaults
            _settings = new SettingsService(_save);
        }

        // ---- defaults hydrated from SaveData ----

        [Test]
        public void Current_AfterHydrate_MatchesSaveDataDefaults()
        {
            Assert.That(_settings.Current.AudioMaster, Is.EqualTo(DefaultAudioMaster).Within(0.001f));
            Assert.That(_settings.Current.AudioMusic, Is.EqualTo(DefaultAudioMusic).Within(0.001f));
            Assert.That(_settings.Current.AudioSfx, Is.EqualTo(DefaultAudioSfx).Within(0.001f));
            Assert.That(_settings.Current.HapticsEnabled, Is.EqualTo(DefaultHaptics));
            Assert.That(_settings.Current.Language, Is.EqualTo(LanguageCode.En));
        }

        [Test]
        public void Hydrate_ReadsExistingSaveDataValues()
        {
            // Stamp a non-default block into the save POCO before constructing
            // the SettingsService so we can assert Hydrate() pulled it in.
            _save.Data.Settings.AudioMaster = LowVolume;
            _save.Data.Settings.AudioMusic = MidVolume;
            _save.Data.Settings.AudioSfx = HighVolume;
            _save.Data.Settings.HapticsEnabled = false;
            _save.Data.Settings.Language = "tr";

            var svc = new SettingsService(_save);

            Assert.That(svc.Current.AudioMaster, Is.EqualTo(LowVolume).Within(0.001f));
            Assert.That(svc.Current.AudioMusic, Is.EqualTo(MidVolume).Within(0.001f));
            Assert.That(svc.Current.AudioSfx, Is.EqualTo(HighVolume).Within(0.001f));
            Assert.That(svc.Current.HapticsEnabled, Is.False);
            Assert.That(svc.Current.Language, Is.EqualTo(LanguageCode.Tr));
        }

        // ---- clamping ----

        [Test]
        public void SetAudioMaster_NegativeValue_ClampsToZero()
        {
            _settings.SetAudioMaster(NegativeVolume);
            Assert.That(_settings.Current.AudioMaster, Is.EqualTo(0f));
        }

        [Test]
        public void SetAudioMaster_OverOne_ClampsToOne()
        {
            _settings.SetAudioMaster(OverOneVolume);
            Assert.That(_settings.Current.AudioMaster, Is.EqualTo(1f));
        }

        [Test]
        public void SetAudioMusic_ClampsRange()
        {
            _settings.SetAudioMusic(NegativeVolume);
            Assert.That(_settings.Current.AudioMusic, Is.EqualTo(0f));
            _settings.SetAudioMusic(OverOneVolume);
            Assert.That(_settings.Current.AudioMusic, Is.EqualTo(1f));
        }

        [Test]
        public void SetAudioSfx_ClampsRange()
        {
            _settings.SetAudioSfx(NegativeVolume);
            Assert.That(_settings.Current.AudioSfx, Is.EqualTo(0f));
            _settings.SetAudioSfx(OverOneVolume);
            Assert.That(_settings.Current.AudioSfx, Is.EqualTo(1f));
        }

        // ---- OnChanged event ----

        [Test]
        public void Setters_RaiseOnChangedExactlyOnce()
        {
            int callCount = 0;
            _settings.OnChanged += _ => callCount++;

            _settings.SetAudioMaster(MidVolume);
            _settings.SetAudioMusic(MidVolume);
            _settings.SetAudioSfx(MidVolume);
            _settings.SetHaptics(false);
            _settings.SetLanguage(LanguageCode.Tr);

            Assert.That(callCount, Is.EqualTo(5),
                "Each setter must raise OnChanged exactly once.");
        }

        // ---- round-trip persist + load ----

        [Test]
        public void Commit_PersistsAllSettings_ReloadsIdentically()
        {
            // mutate every field
            _settings.SetAudioMaster(LowVolume);
            _settings.SetAudioMusic(MidVolume);
            _settings.SetAudioSfx(HighVolume);
            _settings.SetHaptics(false);
            _settings.SetLanguage(LanguageCode.Tr);

            _settings.Commit(); // writes through SaveService.Save()

            // Fresh SaveService against the same in-memory fs — should observe persisted bytes.
            var save2 = new SaveService(RootDir, _fs);
            save2.Load();
            var settings2 = new SettingsService(save2);

            Assert.That(settings2.Current.AudioMaster, Is.EqualTo(LowVolume).Within(0.001f));
            Assert.That(settings2.Current.AudioMusic, Is.EqualTo(MidVolume).Within(0.001f));
            Assert.That(settings2.Current.AudioSfx, Is.EqualTo(HighVolume).Within(0.001f));
            Assert.That(settings2.Current.HapticsEnabled, Is.False);
            Assert.That(settings2.Current.Language, Is.EqualTo(LanguageCode.Tr));
        }

        [Test]
        public void Commit_PersistsLanguageEn_RoundTrips()
        {
            _settings.SetLanguage(LanguageCode.En);
            _settings.Commit();
            var fresh = ReloadSettings();
            Assert.That(fresh.Current.Language, Is.EqualTo(LanguageCode.En));
        }

        [Test]
        public void Commit_PersistsLanguageTr_RoundTrips()
        {
            _settings.SetLanguage(LanguageCode.Tr);
            _settings.Commit();
            var fresh = ReloadSettings();
            Assert.That(fresh.Current.Language, Is.EqualTo(LanguageCode.Tr));
        }

        [Test]
        public void Commit_PersistsLanguageId_RoundTrips()
        {
            _settings.SetLanguage(LanguageCode.Id);
            _settings.Commit();
            var fresh = ReloadSettings();
            Assert.That(fresh.Current.Language, Is.EqualTo(LanguageCode.Id));
        }

        [Test]
        public void Commit_PersistsLanguagePh_RoundTrips()
        {
            _settings.SetLanguage(LanguageCode.Ph);
            _settings.Commit();
            var fresh = ReloadSettings();
            Assert.That(fresh.Current.Language, Is.EqualTo(LanguageCode.Ph));
        }

        [Test]
        public void Commit_WritesToSaveDataSettingsBlock()
        {
            // Direct inspection: Commit() must write into _save.Data.Settings
            // before triggering Save() so future readers see the latest values.
            _settings.SetAudioMaster(LowVolume);
            _settings.SetHaptics(false);
            _settings.Commit();

            Assert.That(_save.Data.Settings.AudioMaster, Is.EqualTo(LowVolume).Within(0.001f));
            Assert.That(_save.Data.Settings.HapticsEnabled, Is.False);
        }

        [Test]
        public void Commit_DoesNotRaiseOnChanged()
        {
            // Commit is the modal-close flush — it must not fire OnChanged
            // (which would feed back into UI bindings and cause redraw loops).
            int callCount = 0;
            _settings.OnChanged += _ => callCount++;

            _settings.Commit();

            Assert.That(callCount, Is.EqualTo(0),
                "Commit() is a write-through, not a mutation; OnChanged must stay silent.");
        }

        [Test]
        public void SetHaptics_PersistsBothValues()
        {
            _settings.SetHaptics(false);
            _settings.Commit();
            var off = ReloadSettings();
            Assert.That(off.Current.HapticsEnabled, Is.False);

            off.SetHaptics(true);
            off.Commit();
            var on = ReloadSettings();
            Assert.That(on.Current.HapticsEnabled, Is.True);
        }

        // ---- helpers ----

        private SettingsService ReloadSettings()
        {
            var save = new SaveService(RootDir, _fs);
            save.Load();
            return new SettingsService(save);
        }
    }
}
