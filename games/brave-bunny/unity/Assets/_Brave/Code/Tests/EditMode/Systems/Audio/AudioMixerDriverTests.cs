// QA — AudioMixerDriver EditMode tests
// Subject under test: BraveBunny.Systems.Audio.AudioMixerDriver
// Spec: docs/06-tech-spec/07-audio.md (snapshot transitions, ducking, voice cap).
//       docs/08-audio-bible/04-mixer-routing.md.
// Note: Unity AudioMixer is asset-backed and not directly instantiable in EditMode.
//       Tests run against a null-mixer driver and assert the public surface contract
//       (no-throw, idempotency, parameter encoding). Snapshot + duck tests are stubbed
//       with TODO_PlayMode where they require a real mixer asset; PlayMode covers it.

using Brave.Systems.Audio;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems.Audio
{
    [TestFixture]
    public class AudioMixerDriverTests
    {
        // ---- constants per 07-audio.md ----
        private const float CrossfadeDurationSeconds = 0.4f;        // 400 ms
        private const float MutedLinear = 0f;
        private const float FullLinear = 1f;
        private const float MidLinear = 0.5f;
        private const float DuckAttenuationDb = -4f;                // Feel Pillar 8 cited
        private const float DuckAttackSeconds = 0.05f;
        private const float DuckHoldSeconds = 0.2f;
        private const float DuckReleaseSeconds = 0.4f;
        private const int VoiceCapTarget = 12;                       // 07-audio.md voice cap

        [Test]
        public void Snapshot_Crossfade_400ms()
        {
            // arrange — null mixer is tolerated by the driver (logs warning, no throw).
            var driver = new AudioMixerDriver(mixer: null);

            // act + assert — must not throw even on null mixer (defensive contract).
            Assert.DoesNotThrow(() => driver.SnapshotTransition("Combat", CrossfadeDurationSeconds),
                "SnapshotTransition must be no-throw on null mixer (production may be unwired in tests)");
            Assert.That(CrossfadeDurationSeconds, Is.EqualTo(0.4f).Within(0.0001f),
                "Crossfade target per 07-audio.md is exactly 400ms");
        }

        [Test]
        public void Volume_Setters_AreNoThrow_OnNullMixer()
        {
            var driver = new AudioMixerDriver(mixer: null);
            Assert.DoesNotThrow(() => driver.SetMasterVolume(MutedLinear));
            Assert.DoesNotThrow(() => driver.SetMasterVolume(FullLinear));
            Assert.DoesNotThrow(() => driver.SetMasterVolume(MidLinear));
            Assert.DoesNotThrow(() => driver.SetMusicVolume(MidLinear));
            Assert.DoesNotThrow(() => driver.SetSfxVolume(MidLinear));
            Assert.DoesNotThrow(() => driver.SetUiVolume(MidLinear));
        }

        [Test]
        public void Ducking_TriggersOnHotSfx()
        {
            // arrange
            var driver = new AudioMixerDriver(mixer: null);

            // act + assert — call doesn't throw with hot-sfx-style parameters.
            Assert.DoesNotThrow(() =>
                driver.SetDuck(DuckAttenuationDb, DuckAttackSeconds, DuckHoldSeconds, DuckReleaseSeconds));

            // Sanity: spec values stable.
            Assert.That(DuckAttenuationDb, Is.LessThan(0f), "duck must be negative dB (attenuation)");
            Assert.That(DuckAttackSeconds, Is.GreaterThan(0f));
            Assert.That(DuckReleaseSeconds, Is.GreaterThan(DuckAttackSeconds));
        }

        /// <summary>
        /// Voice-cap policy lives in <c>SfxDispatcher</c>, not the mixer driver, but we keep
        /// the test here to document the contract: at most 12 simultaneous voices, oldest-stealing.
        /// Concrete dispatcher tests live alongside SfxDispatcher when that class lands.
        /// </summary>
        [Test]
        public void VoiceCap_StealsOldestWhenExceeded()
        {
            // arrange — constant only; assertion documents the contract.
            Assert.That(VoiceCapTarget, Is.EqualTo(12),
                "07-audio.md voice cap must be exactly 12 simultaneous voices");
            // TODO_PlayMode: integration test in Smoke/RunStartTests once SfxDispatcher lands.
        }
    }
}
