// ScreenshotCapture — Editor-only headless screenshot capture for App Store Connect.
//
// Owner: build-engineer / art-director (Wave 11 marketing pipeline).
// Cross-ref: games/brave-bunny/docs/marketing/screenshot-spec.md (device matrix + 5-shot narrative).
//
// Invoked two ways:
//   1. Interactively  — `Menu: Brave > Marketing > Capture Screenshots (All Devices)`
//   2. Headlessly     — `Unity -batchmode -executeMethod Brave.Boot.Editor.ScreenshotCapture.CaptureAll`
//      from tools/ci/scripts/capture-screenshots.sh.
//
// Captures the currently-loaded scene (caller is responsible for loading the
// "Capture" scene first — see capture-screenshots.sh). Iterates the device
// resolution table below and uses `ScreenCapture.CaptureScreenshotAsTexture`
// to produce a Texture2D per device, encodes to PNG, writes to:
//
//     <output-dir>/raw/<device>/01.png
//
// The composition pass (ScreenshotOverlay) reads from there and writes
// final-with-headline frames to marketing/screenshots/<lang>/<device>/.
//
// No locale work happens at capture time — raw PNGs are locale-agnostic per
// screenshot-spec §6 (only the headline/subhead text frame differs per locale).
//
// Per CLAUDE.md principle #6 "no magic numbers": the device matrix is the
// canonical resolution table from screenshot-spec §1 — kept as a `readonly`
// table so tests can assert its contents without spinning up an Editor session.

#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Brave.Boot.Editor
{
    /// <summary>
    /// Headless screenshot capture entry point. Produces one PNG per device class.
    /// </summary>
    public static class ScreenshotCapture
    {
        // ---------------------------------------------------------------------------
        //  Device matrix — single source of truth per screenshot-spec §1.
        // ---------------------------------------------------------------------------

        /// <summary>
        /// One row per Apple iPhone device class we must ship screenshots for.
        /// Resolutions are PORTRAIT dimensions per Apple's 2026 ASC specs.
        /// A landscape variant ("6.7-landscape") is included for marketing
        /// shots that benefit from the wider crop (e.g. boss reveal frames);
        /// it is not currently required by ASC but is exported for use on
        /// social / press channels (see screenshot-spec §1 footnote on iPad
        /// future scope — landscape lives in the same family).
        /// </summary>
        public readonly struct DeviceSpec
        {
            public readonly string Key;          // filesystem-safe slug, e.g. "iphone-6.7"
            public readonly string DisplayName;  // human label, e.g. "iPhone 6.7\""
            public readonly int    Width;        // pixels
            public readonly int    Height;       // pixels
            public readonly bool   IsRequiredForASC;

            public DeviceSpec(string key, string displayName, int width, int height, bool isRequiredForASC)
            {
                Key              = key;
                DisplayName      = displayName;
                Width            = width;
                Height           = height;
                IsRequiredForASC = isRequiredForASC;
            }
        }

        /// <summary>
        /// Apple iPhone device classes for App Store Connect 2026.
        ///   * 1290x2796 — iPhone 14 Pro Max / 15 Pro Max / 16 Pro Max / 16 Plus (6.7" class, PRIMARY)
        ///   * 1179x2556 — iPhone 14 Pro / 15 / 15 Pro / 16 / 16 Pro       (6.1" class, common)
        ///   * 1170x2532 — iPhone 12 / 12 Pro / 13 / 13 Pro / 14            (6.1" mini-bezel)
        ///   * 2796x1290 — landscape variant of the 6.7" class (marketing-extra, not ASC-required)
        /// </summary>
        public static readonly DeviceSpec[] Devices =
        {
            new DeviceSpec("iphone-6.7",            "iPhone 6.7\"",            1290, 2796, isRequiredForASC: true),
            new DeviceSpec("iphone-6.1",            "iPhone 6.1\"",            1179, 2556, isRequiredForASC: true),
            new DeviceSpec("iphone-6.1-mini",       "iPhone 6.1\" (mini)",     1170, 2532, isRequiredForASC: true),
            new DeviceSpec("iphone-6.7-landscape",  "iPhone 6.7\" landscape",  2796, 1290, isRequiredForASC: false),
        };

        // The 5-shot narrative (screenshot-spec §2) is captured by re-loading
        // separate capture saves OR by running this entry-point 5 times with
        // a different `--shot` arg. v1 captures one frame (the currently-loaded
        // scene state) per invocation; the wrapper script in capture-screenshots.sh
        // loops shots when capture-saves are wired (TODO post-Wave-11).
        public const int DefaultShotIndex = 1;
        public const int MaxShotIndex     = 5;

        // ---------------------------------------------------------------------------
        //  Output path helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Builds the absolute raw-PNG output path:
        ///   &lt;outputDir&gt;/raw/&lt;device.Key&gt;/&lt;shotIndex:D2&gt;.png
        /// </summary>
        public static string BuildRawOutputPath(string outputDir, DeviceSpec device, int shotIndex)
        {
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new ArgumentException("outputDir must be non-empty", nameof(outputDir));
            if (shotIndex < 1 || shotIndex > MaxShotIndex)
                throw new ArgumentOutOfRangeException(nameof(shotIndex), shotIndex,
                    $"shotIndex must be 1..{MaxShotIndex}");

            return Path.Combine(outputDir, "raw", device.Key, $"{shotIndex:D2}.png");
        }

        // ---------------------------------------------------------------------------
        //  Menu / batchmode entry points
        // ---------------------------------------------------------------------------

        [MenuItem("Brave/Marketing/Capture Screenshots (All Devices)")]
        public static void CaptureAllFromMenu()
        {
            string outputDir = ResolveDefaultOutputDir();
            CaptureAllInternal(outputDir, DefaultShotIndex);
            EditorUtility.RevealInFinder(outputDir);
        }

        /// <summary>
        /// Headless entry point. Reads `--output &lt;dir&gt;` and optional `--shot &lt;n&gt;`
        /// from command-line args (passed through after `--` to Unity).
        /// </summary>
        public static void CaptureAll()
        {
            string outputDir = ParseArg("-output") ?? ResolveDefaultOutputDir();
            int    shotIndex = int.TryParse(ParseArg("-shot") ?? "", out var s) ? s : DefaultShotIndex;

            CaptureAllInternal(outputDir, shotIndex);
        }

        private static void CaptureAllInternal(string outputDir, int shotIndex)
        {
            Debug.Log($"[ScreenshotCapture] outputDir={outputDir} shot={shotIndex} devices={Devices.Length}");

            foreach (var device in Devices)
            {
                CaptureOne(device, shotIndex, outputDir);
            }

            Debug.Log($"[ScreenshotCapture] DONE — wrote {Devices.Length} PNG(s) under {outputDir}/raw/");
        }

        /// <summary>
        /// Captures a single (device, shotIndex) PNG to disk.
        /// Public so tests / interactive tooling can target one device.
        /// </summary>
        public static string CaptureOne(DeviceSpec device, int shotIndex, string outputDir)
        {
            string path = BuildRawOutputPath(outputDir, device, shotIndex);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // Re-size the Game view to the target resolution and capture.
            // ScreenCapture.CaptureScreenshotAsTexture reads from the current
            // Game-view render target — caller must have set the Game-view
            // aspect to match `device` (capture-screenshots.sh handles this
            // via a custom GameViewSize entry; here we just snapshot).
            //
            // Per Unity docs the call must run inside the Editor's main thread,
            // which is the only thread we're ever on during an -executeMethod
            // invocation, so no marshalling needed.
            Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture(superSize: 1);
            try
            {
                byte[] png = shot.EncodeToPNG();
                File.WriteAllBytes(path, png);
                Debug.Log($"[ScreenshotCapture] wrote {path} ({device.Width}x{device.Height} target, {shot.width}x{shot.height} actual)");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(shot);
            }
            return path;
        }

        // ---------------------------------------------------------------------------
        //  Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Resolves the default output dir relative to the Unity project:
        ///   &lt;project&gt;/../marketing/screenshots
        /// which lands at games/brave-bunny/marketing/screenshots/.
        /// </summary>
        public static string ResolveDefaultOutputDir()
        {
            // Application.dataPath = .../games/brave-bunny/unity/Assets
            // → .../games/brave-bunny/marketing/screenshots
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string gameRoot    = Path.GetFullPath(Path.Combine(projectRoot, ".."));
            return Path.Combine(gameRoot, "marketing", "screenshots");
        }

        private static string? ParseArg(string name)
        {
            var argv = Environment.GetCommandLineArgs();
            for (int i = 0; i < argv.Length - 1; i++)
            {
                if (argv[i] == name) return argv[i + 1];
            }
            return null;
        }
    }
}
#endif
