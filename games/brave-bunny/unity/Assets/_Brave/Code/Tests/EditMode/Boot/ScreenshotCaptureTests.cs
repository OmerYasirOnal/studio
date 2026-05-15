// QA — ScreenshotCapture + ScreenshotOverlay table / path tests (Wave 11).
//
// Subjects under test:
//   * Brave.Boot.Editor.ScreenshotCapture   — device matrix + raw path builder
//   * Brave.Boot.Editor.ScreenshotOverlay   — locale set + overlay path builder + key reader
//
// These tests deliberately AVOID calling the actual capture / compose paths —
// those require a live Game-view render target and a real PNG on disk, which
// is out of scope for EditMode tests. The contract under test here is:
//
//   1. The device matrix matches the screenshot-spec §1 resolutions exactly.
//   2. BuildRawOutputPath produces the documented layout: raw/<device>/<NN>.png
//   3. BuildOverlayOutputPath produces the documented layout:
//      <lang>/<device>/<NN>-<slug>.png
//   4. ScreenshotOverlay.Locales iterates EN + TR per Wave 11 brief.
//   5. The shot-key table covers all 5 carousel positions and maps to keys
//      that actually exist in screenshot-keys.json.

#if UNITY_EDITOR
#nullable enable

using System.IO;
using System.Linq;
using Brave.Boot.Editor;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Brave.Tests.EditMode.Boot
{
    [TestFixture]
    public class ScreenshotCaptureTests
    {
        // ---------------------------------------------------------------------------
        //  Device matrix
        // ---------------------------------------------------------------------------

        [Test]
        public void Devices_IncludesAllAscRequiredIPhoneClasses()
        {
            var keys = ScreenshotCapture.Devices.Select(d => d.Key).ToArray();
            Assert.Contains("iphone-6.7",       keys, "6.7-inch class missing (1290x2796)");
            Assert.Contains("iphone-6.1",       keys, "6.1-inch class missing (1179x2556)");
            Assert.Contains("iphone-6.1-mini",  keys, "6.1-inch mini class missing (1170x2532)");
        }

        [Test]
        public void Devices_IncludesLandscapeMarketingExtra()
        {
            var landscape = ScreenshotCapture.Devices.FirstOrDefault(d => d.Key == "iphone-6.7-landscape");
            Assert.AreNotEqual(default(ScreenshotCapture.DeviceSpec), landscape, "landscape variant missing");
            Assert.AreEqual(2796, landscape.Width,  "landscape width wrong");
            Assert.AreEqual(1290, landscape.Height, "landscape height wrong");
            Assert.IsFalse(landscape.IsRequiredForASC, "landscape should not be ASC-required");
        }

        [Test]
        public void Devices_ResolutionsMatchAppStoreConnect2026Specs()
        {
            AssertDeviceResolution("iphone-6.7",      1290, 2796);
            AssertDeviceResolution("iphone-6.1",      1179, 2556);
            AssertDeviceResolution("iphone-6.1-mini", 1170, 2532);
        }

        [Test]
        public void Devices_AscRequiredFlagSetForPortraitClasses()
        {
            var required = ScreenshotCapture.Devices.Where(d => d.IsRequiredForASC).Select(d => d.Key).ToArray();
            Assert.AreEqual(3, required.Length, "expected 3 ASC-required device classes");
            CollectionAssert.AreEquivalent(
                new[] { "iphone-6.7", "iphone-6.1", "iphone-6.1-mini" },
                required);
        }

        private static void AssertDeviceResolution(string key, int width, int height)
        {
            var device = ScreenshotCapture.Devices.FirstOrDefault(d => d.Key == key);
            Assert.AreNotEqual(default(ScreenshotCapture.DeviceSpec), device, $"device {key} missing");
            Assert.AreEqual(width,  device.Width,  $"{key} width");
            Assert.AreEqual(height, device.Height, $"{key} height");
        }

        // ---------------------------------------------------------------------------
        //  Raw output path
        // ---------------------------------------------------------------------------

        [Test]
        public void BuildRawOutputPath_FollowsDocumentedLayout()
        {
            var device = ScreenshotCapture.Devices.First(d => d.Key == "iphone-6.7");
            string path = ScreenshotCapture.BuildRawOutputPath("/tmp/screens", device, 1);
            // Use Path.Combine for cross-platform comparison.
            string expected = Path.Combine("/tmp/screens", "raw", "iphone-6.7", "01.png");
            Assert.AreEqual(expected, path);
        }

        [Test]
        public void BuildRawOutputPath_RejectsEmptyOutputDir()
        {
            var device = ScreenshotCapture.Devices.First();
            Assert.Throws<System.ArgumentException>(() => ScreenshotCapture.BuildRawOutputPath("", device, 1));
        }

        [Test]
        public void BuildRawOutputPath_RejectsOutOfRangeShotIndex()
        {
            var device = ScreenshotCapture.Devices.First();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ScreenshotCapture.BuildRawOutputPath("/tmp", device, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => ScreenshotCapture.BuildRawOutputPath("/tmp", device, 6));
        }

        // ---------------------------------------------------------------------------
        //  Overlay locales
        // ---------------------------------------------------------------------------

        [Test]
        public void Locales_IncludesEnAndTrAtWave11()
        {
            CollectionAssert.AreEquivalent(new[] { "en", "tr" }, ScreenshotOverlay.Locales,
                "Wave 11 ships EN + TR only; PH and ID are deferred per cut-list #3.");
        }

        [Test]
        public void ShotKeys_CoverAllFiveCarouselPositions()
        {
            var shots = ScreenshotOverlay.GetShotKeys().Select(t => t.shot).OrderBy(s => s).ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, shots,
                "screenshot-spec §2 defines 5 carousel frames; the table must cover all of them.");
        }

        [Test]
        public void ShotKeys_HeadlineAndSubheadKeysFollowSnakeCaseConvention()
        {
            foreach (var sk in ScreenshotOverlay.GetShotKeys())
            {
                StringAssert.IsMatch($@"^screenshot_{sk.shot}_headline$", sk.headline);
                StringAssert.IsMatch($@"^screenshot_{sk.shot}_subhead$",  sk.subhead);
                Assert.IsFalse(string.IsNullOrWhiteSpace(sk.slug), $"shot {sk.shot} has empty slug");
            }
        }

        // ---------------------------------------------------------------------------
        //  Overlay output path
        // ---------------------------------------------------------------------------

        [Test]
        public void BuildOverlayOutputPath_FollowsDocumentedLayout()
        {
            var device = ScreenshotCapture.Devices.First(d => d.Key == "iphone-6.1");
            string path = ScreenshotOverlay.BuildOverlayOutputPath(
                "/tmp/screens", "tr", device, 3, "eight-heroes");
            string expected = Path.Combine("/tmp/screens", "tr", "iphone-6.1", "03-eight-heroes.png");
            Assert.AreEqual(expected, path);
        }

        [Test]
        public void BuildOverlayOutputPath_RejectsEmptyLocale()
        {
            var device = ScreenshotCapture.Devices.First();
            Assert.Throws<System.ArgumentException>(() =>
                ScreenshotOverlay.BuildOverlayOutputPath("/tmp", "", device, 1, "slug"));
        }

        [Test]
        public void BuildOverlayOutputPath_RejectsEmptySlug()
        {
            var device = ScreenshotCapture.Devices.First();
            Assert.Throws<System.ArgumentException>(() =>
                ScreenshotOverlay.BuildOverlayOutputPath("/tmp", "en", device, 1, ""));
        }

        // ---------------------------------------------------------------------------
        //  ScreenshotOverlay key reader — uses real screenshot-keys.json from
        //  unity/Assets/_Brave/Localization/. This is checked into the repo so
        //  the test is deterministic; failure means the keys file regressed.
        // ---------------------------------------------------------------------------

        [Test]
        public void LoadScreenshotKeys_ReadsTheCheckedInLocalizationFile()
        {
            JObject keys = ScreenshotOverlay.LoadScreenshotKeys();
            // Sample 2 keys per carousel position — proves all 10 keys live in the file.
            foreach (var sk in ScreenshotOverlay.GetShotKeys())
            {
                Assert.IsNotNull(keys[sk.headline], $"key {sk.headline} missing from screenshot-keys.json");
                Assert.IsNotNull(keys[sk.subhead],  $"key {sk.subhead} missing from screenshot-keys.json");
            }
        }

        [Test]
        public void ReadLocalized_ReturnsNullForUntranslatedLocales()
        {
            JObject keys = ScreenshotOverlay.LoadScreenshotKeys();
            // PH + ID are explicitly null in screenshot-keys.json — readers must tolerate that.
            string? phHeadline = ScreenshotOverlay.ReadLocalized(keys, "screenshot_1_headline", "tl-PH");
            Assert.IsNull(phHeadline, "PH headline should be null in the source file at Wave 11");
        }

        [Test]
        public void ReadLocalized_ReturnsEnAndTrCopyForShot1()
        {
            JObject keys = ScreenshotOverlay.LoadScreenshotKeys();
            string? en = ScreenshotOverlay.ReadLocalized(keys, "screenshot_1_headline", "en");
            string? tr = ScreenshotOverlay.ReadLocalized(keys, "screenshot_1_headline", "tr");
            Assert.IsFalse(string.IsNullOrEmpty(en), "EN copy missing");
            Assert.IsFalse(string.IsNullOrEmpty(tr), "TR copy missing");
            Assert.AreNotEqual(en, tr, "EN and TR should be different strings");
        }
    }
}
#endif
