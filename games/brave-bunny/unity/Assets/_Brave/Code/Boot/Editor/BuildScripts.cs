// BuildScripts.cs — Wave 11 Editor-only build hooks.
//
// Owner: build-engineer. Cross-ref: tech-spec 10-build-and-ci.md, Wave 11 CI hardening.
//
// This file lives next to IOSBuilder.cs but exposes a NEW entry-point surface
// (BraveBunny.Editor.BuildScripts.BuildIOS / .BuildAndroid) that the Wave 11
// shell wrappers + GitHub Actions workflow target. Behaviour vs IOSBuilder:
//   - Reads BUNDLE VERSION from PlayerSettings (single source of truth)
//   - Reads commit SHA from env (GIT_COMMIT_SHA → CFBundleShortVersionString suffix
//     via PlayerSettings.iOS.buildNumber + bundle settings).
//   - Outputs to <repo>/games/brave-bunny/Builds by default — same root used
//     by fastlane (`BUILD_OUTPUT_ROOT`) — but overridable with `-output <path>`.
//   - All resolution logic factored into pure helpers (BuildOptionsResolver,
//     PathResolver, VersionResolver) so EditMode tests can exercise it without
//     invoking the Unity BuildPipeline.
//
// This file is UnityEditor-only via the asmdef `includePlatforms: Editor`
// (Brave.Boot.Editor.asmdef) — it does NOT need a UNITY_EDITOR guard.

#nullable enable

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BraveBunny.Editor
{
    /// <summary>
    /// CLI build entry points invoked by the Wave 11 shell wrappers.
    /// See build-ios-headless.sh and the bb-ios-build.yml workflow.
    /// </summary>
    public static class BuildScripts
    {
        // -----------------------------------------------------------------------
        //  Public entry points (invoked via Unity -executeMethod)
        // -----------------------------------------------------------------------

        /// <summary>
        /// iOS build entry. Reads -output / -commit CLI args (with env fallbacks),
        /// configures iOS player settings, and runs the BuildPipeline. Exits the
        /// editor on completion so the shell wrapper sees a clean return code.
        /// </summary>
        public static void BuildIOS()
        {
            ExecuteBuild(BuildTarget.iOS);
        }

        /// <summary>
        /// Android build entry. Wired but DEFERRED to post-soft-launch per the
        /// Wave 11 scope. Still callable for local smoke tests.
        /// </summary>
        public static void BuildAndroid()
        {
            ExecuteBuild(BuildTarget.Android);
        }

        // -----------------------------------------------------------------------
        //  Core build runner — shared between targets.
        // -----------------------------------------------------------------------

        private static void ExecuteBuild(BuildTarget target)
        {
            try
            {
                var options = ResolveBuildOptions(target, Environment.GetCommandLineArgs(), Environment.GetEnvironmentVariables());

                LogBanner($"[BuildScripts] target:        {target}");
                LogBanner($"[BuildScripts] output:        {options.LocationPathName}");
                LogBanner($"[BuildScripts] scenes:        {options.Scenes.Length}");
                LogBanner($"[BuildScripts] bundle version: {PlayerSettings.bundleVersion}");
                LogBanner($"[BuildScripts] build number:  {ResolveDisplayBuildNumber(target)}");

                if (options.Scenes.Length == 0)
                {
                    Debug.LogError("[BuildScripts] no enabled scenes in EditorBuildSettings — aborting");
                    EditorApplication.Exit(1);
                    return;
                }

                ApplyPlatformSettings(target, options);

                Directory.CreateDirectory(Path.GetDirectoryName(options.LocationPathName) ?? options.LocationPathName);

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = options.Scenes,
                    locationPathName = options.LocationPathName,
                    target = target,
                    targetGroup = BuildPipeline.GetBuildTargetGroup(target),
                    options = BuildOptions.None
                });

                var summary = report.summary;
                Debug.Log($"[BuildScripts] result: {summary.result}, totalTime: {summary.totalTime}, " +
                          $"size: {summary.totalSize} bytes, warnings: {summary.totalWarnings}, errors: {summary.totalErrors}");

                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"[BuildScripts] build failed: {summary.result}");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log("[BuildScripts] build succeeded");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BuildScripts] unhandled exception: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static void ApplyPlatformSettings(BuildTarget target, BuildOptionsResolved options)
        {
            // Wire commit SHA into PlayerSettings.bundleVersion as a suffix when env
            // var present. This makes the in-app About screen show "0.1.0+abc1234"
            // even on local builds without GitHub Actions context.
            var versionedBundle = VersionResolver.AppendCommitSuffix(PlayerSettings.bundleVersion, options.CommitShaShort);
            if (!string.IsNullOrEmpty(versionedBundle) && versionedBundle != PlayerSettings.bundleVersion)
            {
                Debug.Log($"[BuildScripts] bundleVersion += commit suffix: {PlayerSettings.bundleVersion} -> {versionedBundle}");
                PlayerSettings.bundleVersion = versionedBundle;
            }

            if (target == BuildTarget.iOS)
            {
                PlayerSettings.applicationIdentifier = "com.omeryasir.bravebunny";
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard);
                PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // 1 = ARM64
                PlayerSettings.iOS.targetOSVersionString = "14.0";

                // iOS CFBundleVersion: prefer GITHUB_RUN_NUMBER, then env build number, else keep existing.
                var buildNumber = ResolveDisplayBuildNumber(target);
                if (!string.IsNullOrEmpty(buildNumber))
                {
                    PlayerSettings.iOS.buildNumber = buildNumber;
                    Debug.Log($"[BuildScripts] iOS buildNumber: {buildNumber}");
                }
            }
            else if (target == BuildTarget.Android)
            {
                PlayerSettings.applicationIdentifier = "com.omeryasir.bravebunny";
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard);

                if (int.TryParse(ResolveDisplayBuildNumber(target), out var versionCode) && versionCode > 0)
                {
                    PlayerSettings.Android.bundleVersionCode = versionCode;
                    Debug.Log($"[BuildScripts] Android bundleVersionCode: {versionCode}");
                }
            }
        }

        private static string ResolveDisplayBuildNumber(BuildTarget target)
        {
            var run = Environment.GetEnvironmentVariable("GITHUB_RUN_NUMBER");
            if (!string.IsNullOrEmpty(run)) return run;

            // Local fallback — keep whatever's already set in PlayerSettings.
            return target == BuildTarget.iOS
                ? PlayerSettings.iOS.buildNumber
                : PlayerSettings.Android.bundleVersionCode.ToString();
        }

        private static void LogBanner(string msg) => Debug.Log(msg);

        // -----------------------------------------------------------------------
        //  Pure resolution helpers — exercised by BuildScriptsTests in EditMode.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Resolve a fully-formed <see cref="BuildOptionsResolved"/> from CLI args,
        /// environment, and EditorBuildSettings — without touching the build pipeline.
        /// Public for testing.
        /// </summary>
        public static BuildOptionsResolved ResolveBuildOptions(
            BuildTarget target,
            string[] cliArgs,
            System.Collections.IDictionary env)
        {
            var output = ArgParser.ParseArg(cliArgs, "-output");
            var commit = ArgParser.ParseArg(cliArgs, "-commit") ?? GetEnv(env, "GIT_COMMIT_SHA");

            // Default output: under the unity project, e.g. unity/Build/iOS.
            var defaultOutput = target switch
            {
                BuildTarget.iOS => "Build/iOS",
                BuildTarget.Android => "Build/Android/BraveBunny.apk",
                _ => "Build/Unknown"
            };
            var locationPathName = output ?? defaultOutput;

            // Pull enabled scenes — same source as IOSBuilder.
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                .Select(s => s.path)
                .ToArray();

            return new BuildOptionsResolved(
                target: target,
                locationPathName: locationPathName,
                commitShaFull: commit ?? string.Empty,
                commitShaShort: VersionResolver.ShortenSha(commit),
                scenes: scenes
            );
        }

        private static string? GetEnv(System.Collections.IDictionary env, string key)
        {
            if (env == null) return null;
            if (!env.Contains(key)) return null;
            var v = env[key]?.ToString();
            return string.IsNullOrEmpty(v) ? null : v;
        }

        /// <summary>
        /// Resolved + sanitized build options (immutable). Returned by
        /// <see cref="ResolveBuildOptions"/> for both runtime + test use.
        /// </summary>
        public readonly struct BuildOptionsResolved
        {
            public readonly BuildTarget Target;
            public readonly string LocationPathName;
            public readonly string CommitShaFull;
            public readonly string CommitShaShort;
            public readonly string[] Scenes;

            public BuildOptionsResolved(BuildTarget target, string locationPathName,
                string commitShaFull, string commitShaShort, string[] scenes)
            {
                Target = target;
                LocationPathName = locationPathName;
                CommitShaFull = commitShaFull ?? string.Empty;
                CommitShaShort = commitShaShort ?? string.Empty;
                Scenes = scenes ?? Array.Empty<string>();
            }
        }
    }

    /// <summary>CLI flag → value parser, mirrors the IOSBuilder helper but stand-alone.</summary>
    public static class ArgParser
    {
        public static string? ParseArg(string[] args, string name)
        {
            if (args == null) return null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            return null;
        }
    }

    /// <summary>
    /// Pure version-string helpers. Public for tests so we don't have to
    /// invoke Unity to verify the suffix / shortening logic.
    /// </summary>
    public static class VersionResolver
    {
        /// <summary>Shorten a git SHA to 7 chars (industry default). Empty → empty.</summary>
        public static string ShortenSha(string? fullSha)
        {
            if (string.IsNullOrEmpty(fullSha)) return string.Empty;
            return fullSha!.Length >= 7 ? fullSha.Substring(0, 7) : fullSha;
        }

        /// <summary>
        /// Append "+&lt;short-sha&gt;" to a SemVer-ish bundle version string.
        /// Idempotent — calling twice with the same SHA returns the same string.
        /// Returns the original string unchanged when sha is empty.
        /// </summary>
        public static string AppendCommitSuffix(string bundleVersion, string shortSha)
        {
            if (string.IsNullOrEmpty(bundleVersion)) return bundleVersion;
            if (string.IsNullOrEmpty(shortSha)) return bundleVersion;

            var suffix = "+" + shortSha;
            // Strip any existing build-metadata suffix first (SemVer "+meta") so we
            // don't accumulate +abc1234+def5678 across rebuilds.
            var plusIdx = bundleVersion.IndexOf('+');
            var baseVer = plusIdx >= 0 ? bundleVersion.Substring(0, plusIdx) : bundleVersion;
            return baseVer + suffix;
        }
    }
}
