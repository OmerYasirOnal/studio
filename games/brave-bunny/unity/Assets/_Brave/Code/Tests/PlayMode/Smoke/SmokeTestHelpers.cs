// QA — PlayMode smoke-test helpers
//
// Shared utility for VerticalSliceSmokeTest. Keeps the test method readable by extracting
// the verbose "load scene + wait for ready + find component" plumbing into named helpers.
//
// Design choices:
//   * Helpers return IEnumerator yields so callers stay inside the [UnityTest] coroutine
//     pattern (no synchronous waits or extra threads).
//   * No magic numbers — frame/second budgets live as named consts here so the test can
//     reference them in failure messages.
//   * All helpers are static. No state between tests.
//   * Diagnostic-friendly: every helper logs a [SmokeTestHelpers] tag on failure paths so
//     CI grep can route back to this file.
//
// Cross-refs:
//   * docs/06-tech-spec/08-state-machine.md — Boot → Run transition contract.
//   * docs/06-tech-spec/09-event-bus.md     — service registry table.
//   * games/brave-bunny/CLAUDE.md           — perf + wave-timing contracts.

#nullable enable

using System.Collections;
using Brave.Gameplay.Run;
using Brave.Systems.Context;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brave.Tests.PlayMode.Smoke
{
    /// <summary>
    /// Coroutine helpers shared by <see cref="VerticalSliceSmokeTest"/>. Pure utility class —
    /// no Unity Lifecycle, no fields, no state retained between calls.
    /// </summary>
    internal static class SmokeTestHelpers
    {
        // ---- shared constants ----

        /// <summary>Frame cap for "wait until Bootstrap services register" — ~10 s @ 60 fps.</summary>
        public const int MaxFramesWaitingForBootstrap = 600;

        /// <summary>Frame cap for "wait until target scene becomes active" — ~10 s @ 60 fps.</summary>
        public const int MaxFramesWaitingForSceneActive = 600;

        /// <summary>Frame cap for the in-Run simulation window — ~5 s @ 60 fps. Caller may
        /// short-circuit earlier if its predicate is satisfied.</summary>
        public const int MaxFramesForRunSimulation = 300;

        // ---- scene loading ----

        /// <summary>
        /// Load <paramref name="sceneNameOrPath"/> as a Single scene. Tolerates the scene
        /// being absent from Build Settings (LoadSceneAsync may throw or return null on a
        /// CI agent without the scene baked in) — sets <paramref name="loaded"/> false in
        /// that case so the caller can <c>Assert.Pass</c>-skip the test.
        /// </summary>
        public static IEnumerator LoadSceneOrSkip(string sceneNameOrPath, System.Action<bool> loaded)
        {
            AsyncOperation? op = null;
            bool threw = false;
            try { op = SceneManager.LoadSceneAsync(sceneNameOrPath, LoadSceneMode.Single); }
            catch { threw = true; }

            if (threw || op == null)
            {
                Debug.LogWarning($"[SmokeTestHelpers] LoadSceneAsync('{sceneNameOrPath}') unavailable — scene not in Build Settings");
                loaded(false);
                yield break;
            }

            yield return op;
            loaded(true);
        }

        /// <summary>
        /// Wait until <see cref="GameContextBootstrap.Context"/> is non-null, bounded by
        /// <see cref="MaxFramesWaitingForBootstrap"/>. Useful immediately after a Boot scene
        /// load — the bootstrap Awake registers services in the same frame, but the
        /// asynchronous scene-activation can defer Awake by 1-2 frames in batchmode.
        /// </summary>
        public static IEnumerator WaitForBootstrapReady(System.Action<bool> ready)
        {
            int waited = 0;
            while (GameContextBootstrap.Context == null && waited < MaxFramesWaitingForBootstrap)
            {
                yield return null;
                waited++;
            }
            ready(GameContextBootstrap.Context != null);
        }

        /// <summary>
        /// Wait for <see cref="SceneManager.GetActiveScene"/>.name to equal
        /// <paramref name="expectedName"/>. SceneFlow loads Run asynchronously after
        /// Bootstrapper.GameContextReady fires, so the active-scene change is observed
        /// 1-N frames after Boot finishes its Awake.
        /// </summary>
        public static IEnumerator WaitForActiveScene(string expectedName, System.Action<bool> active)
        {
            int waited = 0;
            while (waited < MaxFramesWaitingForSceneActive)
            {
                if (SceneManager.GetActiveScene().name == expectedName)
                {
                    active(true);
                    yield break;
                }
                yield return null;
                waited++;
            }
            active(false);
        }

        // ---- finders ----

        /// <summary>
        /// Find the single <see cref="RunController"/> in the active scene. Returns null if
        /// the Run scene loaded without one wired (e.g. Run.unity stripped in CI).
        /// </summary>
        public static RunController? FindRunController()
        {
            return Object.FindAnyObjectByType<RunController>(FindObjectsInactive.Include);
        }

        /// <summary>
        /// Find any active MonoBehaviour of type <typeparamref name="T"/> in the loaded
        /// scenes. Returns null on miss.
        /// </summary>
        public static T? FindComponent<T>() where T : Component
        {
            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }
    }
}
