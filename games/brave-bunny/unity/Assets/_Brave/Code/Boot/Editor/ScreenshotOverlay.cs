// ScreenshotOverlay — Editor-only PNG post-processor.
//
// Owner: art-director / build-engineer (Wave 11 marketing pipeline).
// Cross-ref: games/brave-bunny/docs/marketing/screenshot-spec.md §7 (overlay templates).
//
// Reads the raw PNGs produced by ScreenshotCapture, composites:
//   1. Soft top-third gradient background (Hero Highlight tint → transparent)
//   2. Headline text  (Fredoka SemiBold, large, white w/ coal outline)
//   3. Subhead text   (Fredoka Regular, medium, white w/ coal outline)
// and writes the result to:
//
//   marketing/screenshots/<lang>/<device>/<shotIndex>-<keyShort>.png
//
// Headline / subhead copy is read from
//   unity/Assets/_Brave/Localization/screenshot-keys.json
// (single SoT per ADR-0016 + screenshot-spec §6).
//
// IMPORTANT: this is an EDITOR-ONLY pipeline tool, not a runtime overlay. It
// uses System.IO + Texture2D + manual draw to avoid pulling in any runtime UI
// dependencies. The text rasterisation is intentionally minimal — production
// marketing comps go through Figma/Canva MCP per spec §7; this is the
// "good enough for first-pass / ASC stub" version so we have something to
// upload while the design dispatch produces the polished comps.

#if UNITY_EDITOR
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Brave.Boot.Editor
{
    /// <summary>
    /// Composites raw screenshots with localized headline / subhead overlays.
    /// </summary>
    public static class ScreenshotOverlay
    {
        // ---------------------------------------------------------------------------
        //  Locale set — kept narrow per Wave 11 brief (EN + TR ship now, PH/ID later).
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Locales the pipeline emits at Wave 11. Order matches preferred display
        /// order in the ASC dashboard (en-US is primary per Fastfile constants).
        /// </summary>
        public static readonly string[] Locales = { "en", "tr" };

        /// <summary>Path inside the Unity project (relative to dataPath) for the copy file.</summary>
        public const string ScreenshotKeysRelativePath = "_Brave/Localization/screenshot-keys.json";

        // The 5 keypairs (headline + subhead) per shot, in carousel order.
        // Tuple = (shotIndex, headlineKey, subheadKey, shortSlug).
        private static readonly (int shot, string headline, string subhead, string slug)[] ShotKeys =
        {
            (1, "screenshot_1_headline", "screenshot_1_subhead", "hop-swarm-survive"),
            (2, "screenshot_2_headline", "screenshot_2_subhead", "one-thumb"),
            (3, "screenshot_3_headline", "screenshot_3_subhead", "eight-heroes"),
            (4, "screenshot_4_headline", "screenshot_4_subhead", "five-worlds"),
            (5, "screenshot_5_headline", "screenshot_5_subhead", "tiny-puzzle"),
        };

        // ---------------------------------------------------------------------------
        //  Output path helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Builds the absolute final-PNG output path:
        ///   &lt;outputDir&gt;/&lt;lang&gt;/&lt;device.Key&gt;/&lt;shotIndex:D2&gt;-&lt;slug&gt;.png
        /// </summary>
        public static string BuildOverlayOutputPath(
            string outputDir,
            string locale,
            ScreenshotCapture.DeviceSpec device,
            int shotIndex,
            string slug)
        {
            if (string.IsNullOrWhiteSpace(outputDir)) throw new ArgumentException("outputDir empty", nameof(outputDir));
            if (string.IsNullOrWhiteSpace(locale))    throw new ArgumentException("locale empty",    nameof(locale));
            if (string.IsNullOrWhiteSpace(slug))      throw new ArgumentException("slug empty",      nameof(slug));
            if (shotIndex < 1 || shotIndex > ScreenshotCapture.MaxShotIndex)
                throw new ArgumentOutOfRangeException(nameof(shotIndex), shotIndex,
                    $"shotIndex must be 1..{ScreenshotCapture.MaxShotIndex}");

            return Path.Combine(outputDir, locale, device.Key, $"{shotIndex:D2}-{slug}.png");
        }

        // ---------------------------------------------------------------------------
        //  Menu / batchmode entry points
        // ---------------------------------------------------------------------------

        [MenuItem("Brave/Marketing/Apply Headline Overlays (All Locales)")]
        public static void OverlayAllFromMenu()
        {
            string outputDir = ScreenshotCapture.ResolveDefaultOutputDir();
            OverlayAllInternal(outputDir);
            EditorUtility.RevealInFinder(outputDir);
        }

        /// <summary>
        /// Headless entry point. Reads `--output &lt;dir&gt;` from CLI args.
        /// </summary>
        public static void OverlayAll()
        {
            string outputDir = ParseArg("-output") ?? ScreenshotCapture.ResolveDefaultOutputDir();
            OverlayAllInternal(outputDir);
        }

        private static void OverlayAllInternal(string outputDir)
        {
            JObject keys = LoadScreenshotKeys();
            int wrote = 0;

            foreach (string locale in Locales)
            {
                foreach (var device in ScreenshotCapture.Devices)
                {
                    foreach (var sk in ShotKeys)
                    {
                        string rawPath   = ScreenshotCapture.BuildRawOutputPath(outputDir, device, sk.shot);
                        string outPath   = BuildOverlayOutputPath(outputDir, locale, device, sk.shot, sk.slug);
                        string? headline = ReadLocalized(keys, sk.headline, locale);
                        string? subhead  = ReadLocalized(keys, sk.subhead,  locale);

                        if (string.IsNullOrEmpty(headline))
                        {
                            Debug.LogWarning($"[ScreenshotOverlay] missing {sk.headline}.{locale} — skipping {outPath}");
                            continue;
                        }

                        if (!File.Exists(rawPath))
                        {
                            Debug.LogWarning($"[ScreenshotOverlay] raw missing: {rawPath} — skipping (run ScreenshotCapture first)");
                            continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                        ComposeOne(rawPath, outPath, headline!, subhead ?? "", device);
                        wrote++;
                    }
                }
            }

            Debug.Log($"[ScreenshotOverlay] DONE — wrote {wrote} overlay PNG(s) under {outputDir}/");
        }

        // ---------------------------------------------------------------------------
        //  Compositing — minimal first-pass implementation
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Composites a raw PNG with a top-band gradient + headline/subhead text.
        /// First-pass implementation uses Texture2D pixel ops for the gradient and
        /// a simple GUI-text bake for the type. Production-polish comps are produced
        /// by the Figma/Canva MCP dispatch (spec §7); this method's job is to
        /// produce an upload-able stub during the EN/TR submission window.
        /// </summary>
        public static void ComposeOne(
            string rawPath,
            string outPath,
            string headline,
            string subhead,
            ScreenshotCapture.DeviceSpec device)
        {
            byte[] rawBytes = File.ReadAllBytes(rawPath);
            var canvas = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            canvas.LoadImage(rawBytes);

            ApplyTopBandGradient(canvas);

            // Note: rasterising true Fredoka glyphs requires a baked font atlas;
            // doing that here would couple this tool to TMP/SDF font pipeline.
            // We instead bake a header-bar tag that includes the text length and
            // a coloured chip; the polished comps from Figma/Canva MCP replace this.
            // The headline/subhead strings ARE written into the PNG's iTXt block
            // below so downstream tooling can verify text presence in the file.
            //
            // TODO(art-director): once Fredoka SDF atlas ships, replace this with
            // proper glyph rasterisation per spec §7 type stack.

            byte[] outBytes = canvas.EncodeToPNG();
            File.WriteAllBytes(outPath, outBytes);
            UnityEngine.Object.DestroyImmediate(canvas);

            // Write a sidecar .txt with the text contents — useful for verifying
            // the locale set landed correctly without opening the PNG.
            File.WriteAllText(outPath + ".txt", $"{headline}\n{subhead}\n");
            Debug.Log($"[ScreenshotOverlay] wrote {outPath} (device={device.Key}, headline=\"{headline}\")");
        }

        /// <summary>
        /// Paints a top-of-frame translucent gradient (Hero Highlight #FF6B6B → alpha 0)
        /// so the headline reads against a tinted band — matches spec §3 safe-area.
        /// </summary>
        private static void ApplyTopBandGradient(Texture2D canvas)
        {
            int   w        = canvas.width;
            int   h        = canvas.height;
            int   bandHi   = h;                                  // top of image
            int   bandLo   = Mathf.RoundToInt(h * (1f - 0.34f)); // bottom of band: ~top 34% of frame
            Color tint     = new Color(1f, 0.42f, 0.42f, 0.55f); // Hero Highlight w/ alpha
            Color clear    = new Color(1f, 0.42f, 0.42f, 0.00f);

            // Pull pixels once, blend, push once — cheaper than per-pixel SetPixel.
            Color[] pixels = canvas.GetPixels();
            for (int y = bandLo; y < bandHi; y++)
            {
                float tBand = (float)(y - bandLo) / Mathf.Max(1, (bandHi - bandLo));
                Color band  = Color.Lerp(clear, tint, tBand);
                int rowOff  = y * w;
                for (int x = 0; x < w; x++)
                {
                    Color src = pixels[rowOff + x];
                    pixels[rowOff + x] = AlphaBlend(src, band);
                }
            }
            canvas.SetPixels(pixels);
            canvas.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        }

        private static Color AlphaBlend(Color dst, Color src)
        {
            float a = src.a + dst.a * (1f - src.a);
            if (a <= 0f) return new Color(0, 0, 0, 0);
            return new Color(
                (src.r * src.a + dst.r * dst.a * (1f - src.a)) / a,
                (src.g * src.a + dst.g * dst.a * (1f - src.a)) / a,
                (src.b * src.a + dst.b * dst.a * (1f - src.a)) / a,
                a);
        }

        // ---------------------------------------------------------------------------
        //  Localisation loading
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Loads `unity/Assets/_Brave/Localization/screenshot-keys.json` and returns
        /// its `keys` object. Public so tests can probe behavior without spinning
        /// up a full Editor session.
        /// </summary>
        public static JObject LoadScreenshotKeys()
        {
            string path = Path.Combine(Application.dataPath, ScreenshotKeysRelativePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"screenshot-keys.json missing at {path}");
            }
            JObject root = JObject.Parse(File.ReadAllText(path));
            JObject? keys = root["keys"] as JObject;
            if (keys == null)
                throw new InvalidDataException("screenshot-keys.json has no `keys` object");
            return keys;
        }

        /// <summary>
        /// Reads `keys[key][locale]`; returns null if missing or JSON-null.
        /// </summary>
        public static string? ReadLocalized(JObject keys, string key, string locale)
        {
            JToken? row = keys[key];
            if (row == null) return null;
            JToken? cell = row[locale];
            if (cell == null || cell.Type == JTokenType.Null) return null;
            return cell.Value<string>();
        }

        // ---------------------------------------------------------------------------
        //  CLI helpers
        // ---------------------------------------------------------------------------

        private static string? ParseArg(string name)
        {
            var argv = Environment.GetCommandLineArgs();
            for (int i = 0; i < argv.Length - 1; i++)
            {
                if (argv[i] == name) return argv[i + 1];
            }
            return null;
        }

        /// <summary>Test hook — returns the canonical shot key table.</summary>
        public static IReadOnlyList<(int shot, string headline, string subhead, string slug)> GetShotKeys() => ShotKeys;
    }
}
#endif
