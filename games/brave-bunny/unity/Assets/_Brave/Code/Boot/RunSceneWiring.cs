// Brave Bunny — Boot / RunSceneWiring
//
// Run scene Awake-time helper: completes the last-mile wiring between
// Boot-registered services (resolved from GameContext) and Run-scene-only
// references (Camera.main, the Run-scene's GameplayAudioBindings MonoBehaviour,
// the player root for damage-number/screenshake anchoring).
//
// Why a separate MonoBehaviour and not Boot:
//   * Camera.main, the GameplayAudioBindings GO (which holds the channel SOs),
//     and the player root all live in the Run scene. Boot is loaded earlier and
//     does not see those references.
//   * Drag this onto an empty GameObject in the Run scene; on Awake it pulls
//     services out of GameContext and finishes the wiring. No SerializeField
//     wiring required — every reference is resolved by Camera.main / GameObject.Find
//     style lookups or by GameContext.TryGet so the script tolerates partial scenes.
//
// Cross-refs:
//   * GameContextBootstrap.cs — registers HitstopService / ScreenShakeController /
//                                DamageNumberSpawner / DamageNumberPool / CharacterUnlockService.
//   * GameplayAudioBindings.cs — exposes SetDispatcher() so this script can inject
//                                 the Boot-time SfxDispatcher into the Run-scene bindings.
//   * RunEndIntegrationBridge.cs — Boot already subscribes; no Run-scene work needed.

#nullable enable

using Brave.Gameplay.Feel;
using Brave.Systems.Audio;
using Brave.Systems.Context;
using UnityEngine;

namespace Brave.Boot
{
    /// <summary>
    /// Awake-time wiring step for the Run scene. Drag onto an empty GameObject
    /// (e.g. <c>[RunWiring]</c>) — no SerializeFields required; every reference
    /// is resolved at Awake. Safe to omit during development — every step is
    /// guarded with a TryGet and logs a warning when something can't be wired.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunSceneWiring : MonoBehaviour
    {
        [Tooltip("Optional: explicit player root. When null, falls back to a tag = 'Player' lookup.")]
        [SerializeField] private Transform? _playerRoot;

        [Tooltip("Optional: explicit camera reference. When null, uses Camera.main.")]
        [SerializeField] private Camera? _camera;

        [Tooltip("Optional: explicit audio bindings reference. When null, uses Object.FindObjectOfType.")]
        [SerializeField] private GameplayAudioBindings? _audioBindings;

        private void Awake()
        {
            var ctx = GameContextBootstrap.Context;
            if (ctx == null)
            {
                Debug.LogWarning(
                    "[RunSceneWiring] GameContextBootstrap.Context is null — "
                    + "the Run scene was loaded without going through Boot. "
                    + "Wiring step skipped; gameplay will run with un-bound audio + shake.");
                return;
            }

            WireAudioBindings(ctx);
            WireScreenShake(ctx);
            WireDamageNumberSpawnerAnchor(ctx);
        }

        // ---- Audio: inject the Boot-side SfxDispatcher into the Run-scene's GameplayAudioBindings ----

        private void WireAudioBindings(GameContext ctx)
        {
            var bindings = _audioBindings != null
                ? _audioBindings
                : FindFirstInScene<GameplayAudioBindings>();

            if (bindings == null)
            {
                Debug.LogWarning(
                    "[RunSceneWiring] No GameplayAudioBindings MonoBehaviour found in Run scene — "
                    + "in-game SFX (weapon-fire / enemy-killed / pickup) will be silent. "
                    + "Add a GameplayAudioBindings component to the Run scene.");
                return;
            }

            if (!ctx.TryGet<ISfxDispatcher>(out var dispatcher))
            {
                Debug.LogWarning("[RunSceneWiring] ISfxDispatcher not registered in GameContext.");
                return;
            }

            bindings.SetDispatcher(dispatcher);

            // Register the Run-scene bindings instance with GameContext so the static
            // WeaponFireBridge subscriber (registered below) can forward weapon-fire
            // pulses to this instance.
            ctx.Register<GameplayAudioBindings>(bindings);

            // Subscribe the Run-scene bindings to the Gameplay-side WeaponFireBridge so
            // AutoAttackController's Notify() lands on the right audio handler. Bridge is
            // a static event, so we capture the bindings reference at scene-load time and
            // release it in OnDestroy.
            _weaponFireHandler = (archetype, position) =>
            {
                if (bindings != null) bindings.NotifyWeaponFired(archetype, position);
            };
            Brave.Gameplay.Combat.WeaponFireBridge.Fired += _weaponFireHandler;
        }

        // ---- ScreenShakeController: bind Camera.main ----

        private void WireScreenShake(GameContext ctx)
        {
            if (!ctx.TryGet<ScreenShakeController>(out var shake))
            {
                // No-op when FeelConfig wasn't wired in Boot (Wave 7A juice services skipped).
                return;
            }

            var cam = _camera != null ? _camera : Camera.main;
            if (cam == null)
            {
                Debug.LogWarning(
                    "[RunSceneWiring] Camera.main is null — ScreenShakeController not bound. "
                    + "Tag the Run-scene camera as MainCamera or assign the _camera SerializeField.");
                return;
            }
            shake.BindCamera(cam);
        }

        // ---- DamageNumberSpawner: optionally re-parent the pool under the player root ----

        private void WireDamageNumberSpawnerAnchor(GameContext ctx)
        {
            if (!ctx.TryGet<DamageNumberPool>(out var pool)) return;

            var anchor = _playerRoot != null
                ? _playerRoot
                : FindPlayerRoot();
            if (anchor == null) return;       // pool stays under the Bootstrap container — still functional

            // Move the pool container under the player root so per-player spatial cull
            // (camera frustum / Run-scene unload) keeps it cleanly bundled. Pre-spawned
            // widgets stay parented to the pool's transform, so no individual re-parent loop.
            pool.transform.SetParent(anchor, worldPositionStays: true);
        }

        // ---- Cleanup ----

        private System.Action<string, Vector3>? _weaponFireHandler;

        private void OnDestroy()
        {
            if (_weaponFireHandler != null)
            {
                Brave.Gameplay.Combat.WeaponFireBridge.Fired -= _weaponFireHandler;
                _weaponFireHandler = null;
            }
        }

        // ---- helpers ----

        private static T? FindFirstInScene<T>() where T : MonoBehaviour
        {
            // Unity 6 LTS — FindFirstObjectByType is the canonical replacement for the
            // deprecated FindObjectOfType. Include inactive so wiring still works when a
            // GameplayAudioBindings GO is disabled at scene-start.
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        }

        private static Transform? FindPlayerRoot()
        {
            // Tag lookup is cheap and tolerant of scene re-shuffles; the Run scene's hero
            // prefab is expected to carry the standard Unity "Player" tag.
            var tagged = GameObject.FindGameObjectWithTag("Player");
            return tagged != null ? tagged.transform : null;
        }
    }
}
