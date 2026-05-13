#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Brave.Editor
{
    // -------------------------------------------------------------------------
    // iOSAppIconAssigner
    // -------------------------------------------------------------------------
    // Wires Assets/_Brave/Art/UI/AppIcon/AppIcon-1024.png into PlayerSettings'
    // iOS icon list at build start, so the next `fastlane beta` rebuild keeps
    // the icon automatically — no manual patching of the gitignored
    // Build/iOS/Unity-iPhone/Images.xcassets/AppIcon.appiconset/ tree.
    //
    // Why an IPreprocessBuildWithReport (not PostProcessBuild):
    //   PlayerSettings.SetPlatformIcons must be called BEFORE Unity's iOS
    //   build pass generates the Xcode project's Images.xcassets. PostProcess
    //   runs AFTER that pass, so any PlayerSettings change there is too late
    //   to flow into the asset catalog Unity writes. We pair the preprocess
    //   with a PostProcessBuild safety-net (priority 50, BEFORE
    //   iOSExportComplianceProcessor at 100) that directly seeds the App
    //   Store 1024 entry into the asset catalog if for any reason Unity 6
    //   LTS's icon-list export drops it.
    //
    // Reference: Unity Manual — PlayerSettings.SetPlatformIcons (2022.1+).
    // -------------------------------------------------------------------------
    public class iOSAppIconAssigner : IPreprocessBuildWithReport
    {
        // Texture lives in Assets at this path. Keep in sync with the actual
        // file location; PNG was placed by build-engineer on 2026-05-12.
        private const string IconAssetPath =
            "Assets/_Brave/Art/UI/AppIcon/AppIcon-1024.png";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) return;
            AssignIcons();
        }

        private static void AssignIcons()
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconAssetPath);
            if (tex == null)
            {
                Debug.LogWarning(
                    $"[iOSAppIconAssigner] No icon at {IconAssetPath}; " +
                    "skipping. iOS build will use Unity default icon.");
                return;
            }

            // PlayerSettings.GetSupportedIconKindsForPlatform takes BuildTargetGroup,
            // not the newer NamedBuildTarget overload (Unity 6 LTS preserves the legacy signature).
            var platform = BuildTargetGroup.iOS;

            // Apply the same 1024 PNG to every icon kind Unity exposes for
            // iOS (Application, Spotlight, Settings, Notification, Marketing).
            // Unity downsamples each slot at xcassets generation time, so the
            // 1024 master is the right input for every kind.
            var kinds = PlayerSettings.GetSupportedIconKindsForPlatform(platform);
            foreach (var kind in kinds)
            {
                var icons = PlayerSettings.GetPlatformIcons(platform, kind);
                if (icons == null || icons.Length == 0) continue;
                foreach (var icon in icons)
                {
                    icon.SetTexture(tex);
                }
                PlayerSettings.SetPlatformIcons(platform, kind, icons);
            }

            // Persist so the change is visible to Unity's iOS build pass that
            // follows this preprocess callback in the same process.
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"[iOSAppIconAssigner] Wired {IconAssetPath} into iOS icons " +
                "(Application + Spotlight + Settings + Notification + Marketing).");
        }
    }

    // -------------------------------------------------------------------------
    // Safety-net: ensure the 1024 marketing PNG is in the generated
    // AppIcon.appiconset even if PlayerSettings.SetPlatformIcons doesn't
    // emit it (Unity LTS quirks around the Marketing slot have been
    // reported in past releases).
    //
    // Priority 50 — runs BEFORE iOSExportComplianceProcessor (priority 100)
    // for deterministic, predictable post-build ordering.
    // -------------------------------------------------------------------------
    public static class iOSAppIconCatalogSafetyNet
    {
        private const string IconAssetPath =
            "Assets/_Brave/Art/UI/AppIcon/AppIcon-1024.png";

        [PostProcessBuild(50)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            var sourcePng = Path.GetFullPath(IconAssetPath);
            if (!File.Exists(sourcePng))
            {
                Debug.LogWarning(
                    $"[iOSAppIconCatalogSafetyNet] Source PNG missing: {sourcePng}");
                return;
            }

            var iconset = Path.Combine(
                pathToBuiltProject,
                "Unity-iPhone/Images.xcassets/AppIcon.appiconset");
            if (!Directory.Exists(iconset))
            {
                Debug.LogWarning(
                    $"[iOSAppIconCatalogSafetyNet] AppIcon.appiconset not found at {iconset}; " +
                    "skipping safety-net step.");
                return;
            }

            var destPng = Path.Combine(iconset, "Icon-AppStore-1024.png");
            File.Copy(sourcePng, destPng, overwrite: true);

            // Patch Contents.json to register the marketing icon entry if
            // it isn't already present. Naive string check — Unity emits a
            // stable JSON layout, and the entry's signature ("ios-marketing"
            // + size "1024x1024") is unique enough to detect.
            var contentsJsonPath = Path.Combine(iconset, "Contents.json");
            if (File.Exists(contentsJsonPath))
            {
                var json = File.ReadAllText(contentsJsonPath);
                if (!json.Contains("\"ios-marketing\"") || !json.Contains("Icon-AppStore-1024.png"))
                {
                    var entry =
                        "    {\n" +
                        "      \"size\" : \"1024x1024\",\n" +
                        "      \"idiom\" : \"ios-marketing\",\n" +
                        "      \"filename\" : \"Icon-AppStore-1024.png\",\n" +
                        "      \"scale\" : \"1x\"\n" +
                        "    }";
                    // Insert before the closing ']' of the "images" array.
                    var idx = json.LastIndexOf("  ],");
                    if (idx > 0)
                    {
                        // Add comma to previous entry's closing brace.
                        var head = json.Substring(0, idx).TrimEnd();
                        if (head.EndsWith("}"))
                        {
                            head += ",\n" + entry + "\n";
                        }
                        else
                        {
                            head += "\n" + entry + "\n";
                        }
                        var tail = json.Substring(idx);
                        File.WriteAllText(contentsJsonPath, head + tail);
                    }
                }
            }

            Debug.Log(
                "[iOSAppIconCatalogSafetyNet] Ensured Icon-AppStore-1024.png " +
                "is in AppIcon.appiconset.");
        }
    }
}
#endif
