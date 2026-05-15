// Brave Bunny — Systems / Audio
// Drives BGM snapshot transitions off scene/state transitions so the game stops being silent.
//
// Cross-refs:
//   * docs/08-audio-bible/01-bgm-spec.md — 12 launch tracks and per-state mood mapping.
//   * docs/06-tech-spec/07-audio.md      — snapshot crossfade table (Boot→Home 400ms,
//                                          Home→Run 800ms, Run→Boss 600ms, Boss→RunEnd 400ms).
//   * docs/06-tech-spec/08-state-machine.md — Boot / MainMenu / Run / RunEnd nodes.
//   * MusicStateMachine — the wrapper this driver invokes.
//
// Design:
//   * Pure C# service (no MonoBehaviour). One method per state-entry; the caller wires up
//     transitions from whichever side observes them (Run scene Awake() calls EnterRun, the
//     boss-spawn flow calls EnterBoss, the run-end report calls EnterRunEnd).
//   * Defers to <see cref="IMusicStateMachine"/> so the snapshot-vs-clip choice is owned
//     centrally — this driver only knows about *states*, not about clips/snapshots.
//   * Scene-listener convenience: <see cref="AttachSceneAutoTransitions"/> subscribes to
//     <see cref="UnityEngine.SceneManagement.SceneManager.activeSceneChanged"/> and routes
//     scene-name → state. Optional (and skipped in unit tests) so the driver remains
//     testable without invoking Unity scene plumbing.

#nullable enable

using System;
using Brave.Systems.Context;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brave.Systems.Audio
{
    /// <summary>
    /// High-level BGM orchestrator. Wraps <see cref="IMusicStateMachine"/> and routes
    /// scene/state transitions to the right music state-entry call. Registered with
    /// <see cref="GameContext"/> at Boot so any system can resolve and call it.
    /// </summary>
    public sealed class BgmGameplayDriver : IService, IDisposable
    {
        // ---- Scene name constants — sourced from Assets/_Brave/Scenes/*.unity ----
        // These mirror the file basenames in /unity/Assets/_Brave/Scenes/, which are the
        // strings Unity passes to activeSceneChanged. Kept here as constants per CLAUDE.md
        // "no magic numbers / strings" rule (string slugs are extracted, not inlined).
        public const string SceneBoot = "Boot";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneLoadout = "Loadout";
        public const string SceneRun = "Run";

        // ---- Default biome slug — overrideable per-run by the loadout flow. ----
        // "Meadow" matches Snapshot_Run_Meadow in the mixer asset (07-audio.md table).
        public const string DefaultBiomeSlug = "Meadow";

        private readonly IMusicStateMachine _music;
        private bool _autoSceneAttached;
        private string _currentBiomeSlug = DefaultBiomeSlug;
        private bool _bossActive;

        /// <summary>True between <see cref="EnterBoss"/> and the next <see cref="EnterRun"/>
        /// or <see cref="EnterRunEnd"/>. Exposed for tests + UI banner gating.</summary>
        public bool BossActive => _bossActive;

        /// <summary>Currently selected biome slug. Set via <see cref="EnterRun"/>.</summary>
        public string CurrentBiomeSlug => _currentBiomeSlug;

        public BgmGameplayDriver(IMusicStateMachine music)
        {
            _music = music ?? throw new ArgumentNullException(nameof(music));
        }

        // ---- State-entry methods ----

        /// <summary>
        /// Boot scene entry → bgm_loading / Home snapshot. The cold-start splash stinger
        /// is a one-shot SFX (handled elsewhere); this routes the underlying ambient bed
        /// to the Home snapshot per 01-bgm-spec.md.
        /// </summary>
        public void EnterBoot()
        {
            _bossActive = false;
            // No dedicated "Loading" snapshot — Home pad doubles as the Boot bed per 01-bgm-spec.md.
            _music.EnterHome();
        }

        /// <summary>MainMenu / Home → Home snapshot.</summary>
        public void EnterMainMenu()
        {
            _bossActive = false;
            _music.EnterHome();
        }

        /// <summary>Pre-run loadout → Lobby snapshot.</summary>
        public void EnterLoadout()
        {
            _bossActive = false;
            _music.EnterLobby();
        }

        /// <summary>
        /// Run scene normal-combat → Run snapshot for the given biome. When called without
        /// a biome string the previously-set biome (default <see cref="DefaultBiomeSlug"/>) is reused.
        /// </summary>
        public void EnterRun(string? biomeSlug = null)
        {
            _bossActive = false;
            if (!string.IsNullOrEmpty(biomeSlug)) _currentBiomeSlug = biomeSlug!;
            _music.EnterRun(_currentBiomeSlug);
        }

        /// <summary>
        /// Boss is active → Boss snapshot. Placeholder dispatch per task brief: the snapshot
        /// itself is authored in BraveBunny.mixer (Snapshot_Run_Boss); per-biome boss tracks
        /// are a follow-up (see 01-bgm-spec.md track #8 "alt bosses re-key per biome").
        /// </summary>
        public void EnterBoss()
        {
            _bossActive = true;
            _music.EnterBoss();
        }

        /// <summary>
        /// Run-end → Win or Lose snapshot. Caller passes the run outcome.
        /// </summary>
        public void EnterRunEnd(bool win)
        {
            _bossActive = false;
            _music.EnterRunEnd(win);
        }

        // ---- Optional scene-name routing ----

        /// <summary>
        /// Subscribe to <see cref="SceneManager.activeSceneChanged"/> and auto-route by
        /// scene name. Idempotent. Optional: not invoked from tests; production code calls
        /// this once at Boot.
        /// </summary>
        public void AttachSceneAutoTransitions()
        {
            if (_autoSceneAttached) return;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            _autoSceneAttached = true;
        }

        /// <summary>Idempotent counterpart to <see cref="AttachSceneAutoTransitions"/>.</summary>
        public void DetachSceneAutoTransitions()
        {
            if (!_autoSceneAttached) return;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _autoSceneAttached = false;
        }

        public void Dispose() => DetachSceneAutoTransitions();

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            RouteForSceneName(next.name);
        }

        /// <summary>Pure scene-name → state-entry router. Exposed for unit tests so the
        /// scene-name → music-state mapping is verifiable without Unity scene plumbing.</summary>
        internal void RouteForSceneName(string sceneName)
        {
            switch (sceneName)
            {
                case SceneBoot: EnterBoot(); break;
                case SceneMainMenu: EnterMainMenu(); break;
                case SceneLoadout: EnterLoadout(); break;
                case SceneRun: EnterRun(); break;
                default:
                    Debug.LogWarning(
                        $"[BgmGameplayDriver] unknown scene '{sceneName}' — leaving BGM unchanged. "
                        + "Add a SceneXxx constant + RouteForSceneName case to drive BGM here.");
                    break;
            }
        }
    }
}
