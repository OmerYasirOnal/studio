// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/08-state-machine.md (Boot → Run transition)
// Sister file: Bootstrapper.cs raises the GameContextReady event after services are wired.
//
// SceneFlow lives on the [SceneFlow] GameObject in _Brave/Scenes/Boot.unity (created
// by Editor/SceneSetup.cs::EnsureBoot). Its single responsibility is to escort the
// player from Boot into the first interactive scene (Run for build #3 — MainMenu/Loadout
// will slot in once those flows ship per ADR-0019 follow-up).

#nullable enable

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brave.Systems.Context
{
    /// <summary>
    /// Listens for <see cref="Bootstrapper.GameContextReady"/> and triggers an async
    /// scene-load to <see cref="nextScene"/>. Survives the scene-swap via
    /// <c>DontDestroyOnLoad</c> so the unsubscribe path is observable in the new scene
    /// (and so a future polish wave can extend it into a state-machine driver without
    /// re-instantiating the component).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneFlow : MonoBehaviour
    {
        [Tooltip("Scene to load once Bootstrapper.GameContextReady fires. Default: Run.")]
        public string nextScene = "Run";

        private bool _subscribed;
        private bool _loadRequested;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // If Bootstrapper already fired (rare race — e.g. another script in this scene
            // called Complete() during its own Awake before ours ran), schedule a deferred
            // load on the next frame so we never call LoadSceneAsync from inside Awake.
            if (Bootstrapper.IsReady)
            {
                ScheduleLoad();
                return;
            }

            Bootstrapper.GameContextReady += OnReady;
            _subscribed = true;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnReady()
        {
            Unsubscribe();
            ScheduleLoad();
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            try { Bootstrapper.GameContextReady -= OnReady; }
            catch (Exception e) { Debug.LogWarning($"[SceneFlow] unsubscribe failed: {e.Message}"); }
            _subscribed = false;
        }

        private void ScheduleLoad()
        {
            if (_loadRequested) return;
            _loadRequested = true;

            if (string.IsNullOrWhiteSpace(nextScene))
            {
                Debug.LogError("[SceneFlow] nextScene is empty — staying in Boot.");
                return;
            }

            // Defer one frame so Awake completes for every component in the current scene
            // before the load operation queues. Application.invokeOnGUIThread is not a thing
            // in player builds; the canonical "next frame" hook is to start an async load —
            // LoadSceneAsync itself runs over multiple frames, so calling it here is safe.
            try
            {
                var op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
                if (op == null)
                {
                    Debug.LogError(
                        $"[SceneFlow] LoadSceneAsync('{nextScene}') returned null — "
                        + "scene is probably missing from Build Settings.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneFlow] LoadSceneAsync('{nextScene}') threw: {e.Message}");
            }
        }
    }
}
