// PerfStressFpsTest — PlayMode regression guard for the Phase 5 perf contract.
// Loads PerfStress.unity, warms up 3 s, then asserts FpsSampler.AverageFps >= 30.
//
// The floor is intentionally loose (30 fps) so the test passes on slow CI VMs.
// The real target (60 fps, iPhone 12) is validated in hardware runs.
// Tag [Category("Performance")] lets CI jobs skip this on resource-constrained runners:
//   dotnet test --filter "Category!=Performance"
//
// Companion populator: Assets/Editor/PerfStressPopulator.cs
// Perf contract:       brave-bunny/CLAUDE.md + docs/06-tech-spec/05-perf-budget.md

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using Brave.Diagnostics;

namespace Brave.Tests.PlayMode.Performance
{
    [TestFixture]
    [Category("Performance")]
    public class PerfStressFpsTest
    {
        private const string SceneName    = "PerfStress";
        private const float  WarmupSeconds = 3f;
        private const float  FpsFloor      = 30f;   // loose floor — CI machines vary; iPhone 12 target = 60

        [UnityTest]
        public IEnumerator PerfStress_AverageFps_MeetsFloor()
        {
            // ---- Load scene ----
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);

            // Verify the scene loaded.
            Assert.AreEqual(SceneName, SceneManager.GetActiveScene().name,
                $"Expected active scene '{SceneName}' after load.");

            // ---- Warmup — let the scene settle for WarmupSeconds ----
            float elapsed = 0f;
            while (elapsed < WarmupSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // ---- Locate FpsSampler ----
            var camObj = GameObject.FindWithTag("MainCamera");
            Assert.IsNotNull(camObj, "MainCamera not found in PerfStress scene. "
                + "Run 'Brave > Populate PerfStress (200/50/30)' to set up the scene.");

            var sampler = camObj.GetComponent<FpsSampler>();
            Assert.IsNotNull(sampler, "FpsSampler component not found on MainCamera. "
                + "Re-run the PerfStressPopulator to attach it.");

            // ---- Sample one more frame so AverageFps is fresh ----
            yield return null;

            float fps = sampler.AverageFps;
            Debug.Log($"[PerfStressFpsTest] AverageFps after {WarmupSeconds}s warmup: {fps:F1} fps (floor {FpsFloor} fps)");

            // Soft pass in Editor — frame times are unreliable in the Editor host process.
            if (Application.isEditor)
            {
                Assert.Pass($"Editor run: {fps:F1} fps recorded (soft pass — device hardware required for hard assertion).");
                yield break;
            }

            Assert.GreaterOrEqual(fps, FpsFloor,
                $"AverageFps {fps:F1} is below the {FpsFloor} fps regression floor. "
                + "Check perf budget docs/06-tech-spec/05-perf-budget.md for triage steps.");
        }
    }
}
