// BuildScriptsTests — Wave 11 unit tests for BraveBunny.Editor.BuildScripts helpers.
//
// We do NOT invoke the Unity BuildPipeline here. The goal is to lock down the
// pure resolution logic — CLI arg parsing, default output paths, commit SHA
// shortening, and SemVer-ish "+meta" suffixing — so a refactor of the build
// hook can't silently drift the iOS / Android CFBundle / versionCode behaviour.
//
// Subject under test: BraveBunny.Editor.BuildScripts.ResolveBuildOptions,
//                    BraveBunny.Editor.ArgParser,
//                    BraveBunny.Editor.VersionResolver.

#if UNITY_EDITOR
#nullable enable

using System.Collections;
using System.Collections.Generic;
using BraveBunny.Editor;
using NUnit.Framework;
using UnityEditor;

namespace Brave.Tests.EditMode.Boot
{
    [TestFixture]
    public class BuildScriptsTests
    {
        // -----------------------------------------------------------------------
        //  ArgParser
        // -----------------------------------------------------------------------

        [Test]
        public void ArgParser_Returns_Value_When_Flag_Present()
        {
            var args = new[] { "Unity", "-output", "/tmp/out", "-commit", "abc123" };
            Assert.AreEqual("/tmp/out", ArgParser.ParseArg(args, "-output"));
            Assert.AreEqual("abc123", ArgParser.ParseArg(args, "-commit"));
        }

        [Test]
        public void ArgParser_Returns_Null_When_Flag_Missing()
        {
            var args = new[] { "Unity", "-output", "/tmp/out" };
            Assert.IsNull(ArgParser.ParseArg(args, "-commit"));
        }

        [Test]
        public void ArgParser_Returns_Null_When_Flag_Has_No_Value()
        {
            // Trailing flag with no following token must NOT crash and must return null.
            var args = new[] { "Unity", "-output" };
            Assert.IsNull(ArgParser.ParseArg(args, "-output"));
        }

        [Test]
        public void ArgParser_Handles_Null_And_Empty_Args()
        {
            Assert.IsNull(ArgParser.ParseArg(null!, "-output"));
            Assert.IsNull(ArgParser.ParseArg(System.Array.Empty<string>(), "-output"));
        }

        // -----------------------------------------------------------------------
        //  VersionResolver.ShortenSha
        // -----------------------------------------------------------------------

        [Test]
        public void ShortenSha_Truncates_To_7_Chars()
        {
            Assert.AreEqual("abc1234", VersionResolver.ShortenSha("abc1234567890deadbeef"));
        }

        [Test]
        public void ShortenSha_Returns_Empty_When_Null_Or_Empty()
        {
            Assert.AreEqual(string.Empty, VersionResolver.ShortenSha(null));
            Assert.AreEqual(string.Empty, VersionResolver.ShortenSha(string.Empty));
        }

        [Test]
        public void ShortenSha_Preserves_Sha_Shorter_Than_7()
        {
            Assert.AreEqual("abc", VersionResolver.ShortenSha("abc"));
        }

        // -----------------------------------------------------------------------
        //  VersionResolver.AppendCommitSuffix
        // -----------------------------------------------------------------------

        [Test]
        public void AppendCommitSuffix_Appends_When_Sha_Present()
        {
            Assert.AreEqual("0.1.0+abc1234",
                VersionResolver.AppendCommitSuffix("0.1.0", "abc1234"));
        }

        [Test]
        public void AppendCommitSuffix_Returns_Original_When_Sha_Empty()
        {
            Assert.AreEqual("0.1.0", VersionResolver.AppendCommitSuffix("0.1.0", ""));
            Assert.AreEqual("0.1.0", VersionResolver.AppendCommitSuffix("0.1.0", null!));
        }

        [Test]
        public void AppendCommitSuffix_Is_Idempotent_For_Same_Sha()
        {
            var once = VersionResolver.AppendCommitSuffix("0.1.0", "abc1234");
            var twice = VersionResolver.AppendCommitSuffix(once, "abc1234");
            Assert.AreEqual(once, twice, "appending the same SHA twice must not accumulate");
        }

        [Test]
        public void AppendCommitSuffix_Replaces_Existing_Suffix()
        {
            // SemVer +meta: rebuilding on a new commit replaces the old SHA.
            var withFirst = VersionResolver.AppendCommitSuffix("0.1.0", "abc1234");
            var withSecond = VersionResolver.AppendCommitSuffix(withFirst, "def5678");
            Assert.AreEqual("0.1.0+def5678", withSecond);
        }

        [Test]
        public void AppendCommitSuffix_Returns_Empty_When_BundleVersion_Empty()
        {
            Assert.AreEqual(string.Empty, VersionResolver.AppendCommitSuffix(string.Empty, "abc1234"));
        }

        // -----------------------------------------------------------------------
        //  BuildScripts.ResolveBuildOptions — full integration of the pure parts.
        // -----------------------------------------------------------------------

        [Test]
        public void ResolveBuildOptions_iOS_Defaults_To_BuildIOS_Path_When_No_Output_Arg()
        {
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity" },
                env: new Hashtable());

            Assert.AreEqual(BuildTarget.iOS, opts.Target);
            Assert.AreEqual("Build/iOS", opts.LocationPathName);
        }

        [Test]
        public void ResolveBuildOptions_iOS_Respects_Output_Arg()
        {
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity", "-output", "/custom/path" },
                env: new Hashtable());

            Assert.AreEqual("/custom/path", opts.LocationPathName);
        }

        [Test]
        public void ResolveBuildOptions_Android_Default_Path_Has_Apk_Extension()
        {
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.Android,
                cliArgs: new[] { "Unity" },
                env: new Hashtable());

            Assert.AreEqual(BuildTarget.Android, opts.Target);
            StringAssert.EndsWith(".apk", opts.LocationPathName);
        }

        [Test]
        public void ResolveBuildOptions_Commit_Arg_Takes_Precedence_Over_Env()
        {
            var env = new Hashtable { { "GIT_COMMIT_SHA", "envcommitsha000000" } };
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity", "-commit", "clicommitsha111111" },
                env: env);

            Assert.AreEqual("clicommitsha111111", opts.CommitShaFull);
            Assert.AreEqual("clicomm", opts.CommitShaShort);
        }

        [Test]
        public void ResolveBuildOptions_Falls_Back_To_Env_When_Commit_Arg_Missing()
        {
            var env = new Hashtable { { "GIT_COMMIT_SHA", "envcommitsha000000" } };
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity" },
                env: env);

            Assert.AreEqual("envcommitsha000000", opts.CommitShaFull);
            Assert.AreEqual("envcomm", opts.CommitShaShort);
        }

        [Test]
        public void ResolveBuildOptions_Commit_Empty_When_Neither_Arg_Nor_Env_Set()
        {
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity" },
                env: new Hashtable());

            Assert.AreEqual(string.Empty, opts.CommitShaFull);
            Assert.AreEqual(string.Empty, opts.CommitShaShort);
        }

        [Test]
        public void ResolveBuildOptions_Scenes_Sourced_From_EditorBuildSettings()
        {
            // We don't enforce a count — that drifts as the project grows — but
            // every returned path must be non-empty and the array must be non-null.
            var opts = BuildScripts.ResolveBuildOptions(
                BuildTarget.iOS,
                cliArgs: new[] { "Unity" },
                env: new Hashtable());

            Assert.IsNotNull(opts.Scenes);
            foreach (var s in opts.Scenes)
            {
                Assert.IsFalse(string.IsNullOrEmpty(s), "Scene path must not be empty");
            }
        }

        [Test]
        public void ResolveBuildOptions_Handles_Null_Env_Without_Throwing()
        {
            // Defensive: a caller passing null env (e.g. test harness) shouldn't NRE.
            Assert.DoesNotThrow(() =>
                BuildScripts.ResolveBuildOptions(
                    BuildTarget.iOS,
                    cliArgs: new[] { "Unity", "-output", "/tmp" },
                    env: null!));
        }
    }
}
#endif
