#if UNITY_EDITOR
// IOSBuilder — headless iOS build entry point.
// Owner: build-engineer. Cross-ref: tech-spec 10-build-and-ci.md ("Headless Unity build").
//
// Invoked by tools/ci/scripts/unity-build-ios.sh via:
//   Unity -batchmode -executeMethod Brave.Boot.IOSBuilder.Build -- -output <path>
//
// This file is UnityEditor-only (UNITY_EDITOR guard); it does NOT ship in player builds.
// The Brave.Boot asmdef does not restrict to Editor — so the preprocessor guard is
// what keeps this code out of the player. Moving to Boot/Editor/ with its own asmdef
// is a follow-up; for v0.1 the guard is sufficient.

#nullable enable
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Brave.Boot
{
    /// <summary>
    /// Headless iOS build entry point. Called by tools/ci/scripts/unity-build-ios.sh.
    /// Reads --output &lt;path&gt; from command-line args. Writes Xcode project to that path.
    /// Exits with code 1 on build failure so the shell wrapper propagates the error.
    /// </summary>
    public static class IOSBuilder
    {
        /// <summary>
        /// Entry point. Reads CLI args, configures player settings, runs BuildPipeline.
        /// </summary>
        public static void Build()
        {
            try
            {
                string outputPath = ParseArg("-output") ?? "Build/iOS";
                Debug.Log($"[IOSBuilder] output path: {outputPath}");

                // Force iOS player settings per tech-spec 00 (IL2CPP, .NET Std 2.1, ARM64).
                ConfigureIOSPlayerSettings();

                // Pull enabled scenes from EditorBuildSettings.
                string[] scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                if (scenes.Length == 0)
                {
                    Debug.LogError("[IOSBuilder] no enabled scenes in EditorBuildSettings — aborting");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"[IOSBuilder] building {scenes.Length} scene(s):");
                foreach (string s in scenes) Debug.Log($"  - {s}");

                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.iOS,
                    targetGroup = BuildTargetGroup.iOS,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                Debug.Log($"[IOSBuilder] result: {summary.result}, " +
                          $"total time: {summary.totalTime}, " +
                          $"size: {summary.totalSize} bytes, " +
                          $"warnings: {summary.totalWarnings}, " +
                          $"errors: {summary.totalErrors}");

                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"[IOSBuilder] build failed: {summary.result}");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log("[IOSBuilder] build succeeded");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IOSBuilder] unhandled exception: {ex}");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Apply tech-spec 00 player settings: IL2CPP, .NET Standard 2.1, ARM64,
        /// bundle id, build number from GITHUB_RUN_NUMBER if present.
        /// </summary>
        private static void ConfigureIOSPlayerSettings()
        {
            PlayerSettings.applicationIdentifier = "com.yasironal.brave-bunny";
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // 1 = ARM64

            // iOS deployment target — tech-spec 00 declares iOS 14 minimum.
            PlayerSettings.iOS.targetOSVersionString = "14.0";

            // Build number: prefer CI run number for monotonic CFBundleVersion.
            string? buildNumber = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
            if (!string.IsNullOrEmpty(buildNumber))
            {
                PlayerSettings.iOS.buildNumber = buildNumber;
                Debug.Log($"[IOSBuilder] buildNumber from GITHUB_RUN_NUMBER: {buildNumber}");
            }
            else
            {
                Debug.Log($"[IOSBuilder] buildNumber (unchanged): {PlayerSettings.iOS.buildNumber}");
            }
        }

        /// <summary>
        /// Find a command-line arg by name (e.g. "-output") and return the next token,
        /// or null if absent. Used for fastlane → shell → Unity arg passthrough.
        /// </summary>
        private static string? ParseArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            return null;
        }
    }
}
#endif
