// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/08-state-machine.md (Boot entry actions)
//            docs/06-tech-spec/09-event-bus.md (service registry table)
// Lives on the [Bootstrap] GameObject in _Brave/Scenes/Boot.unity.

#nullable enable

using System.Collections.Generic;
using Brave.Gameplay.Definitions;
using Brave.Gameplay.Feel;
using Brave.Gameplay.Run;
using Brave.Systems.Ads;
using Brave.Systems.Analytics;
using Brave.Systems.Audio;
using Brave.Systems.Iap;
using Brave.Systems.Localization;
using Brave.Systems.Progression;
using Brave.Systems.Save;
using Brave.Systems.Settings;
using UnityEngine;
using UnityEngine.Audio;

namespace Brave.Systems.Context;

/// <summary>
/// Single MonoBehaviour responsible for constructing the service graph and
/// registering everything against <see cref="GameContext"/>. No service
/// references another service directly — wiring order lives here so the
/// dependency direction is reviewable in one file (see 09-event-bus.md).
/// </summary>
[DisallowMultipleComponent]
public sealed class GameContextBootstrap : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer _mixer;

    [Header("Localization")]
    [SerializeField] private TextAsset[] _languageTables;

    [Header("Feel (Wave 7A — hit-feedback juice)")]
    [Tooltip("Companion SO for scalar juice tuning (hitstop / flash / damage-number / shake).")]
    [SerializeField] private FeelConfig? _feelConfig;
    [Tooltip("Per-trigger hitstop ms lookup (ADR-0003). Optional — when null, HitstopService falls back to FeelConfig.HitstopSeconds.")]
    [SerializeField] private Brave.Gameplay.Damage.FeelDefinition? _feelDefinition;
    [Tooltip("TMP widget prefab for floating damage numbers. Required for DamageNumberPool warm-up.")]
    [SerializeField] private DamageNumberWidget? _damageNumberWidgetPrefab;
    [Tooltip("Pre-warm size of the damage-number pool. Matches default in DamageNumberPool.")]
    [SerializeField] private int _damageNumberPoolCapacity = 32;

    [Header("Meta-progression (Wave 7A — character unlocks)")]
    [Tooltip("Character catalogue used to seed the unlock-condition registry. Slug → CharacterDefinition.")]
    [SerializeField] private CharacterDefinition[]? _characterCatalogue;

    /// <summary>Singleton-style accessor for code that cannot receive ctx via injection (e.g. static <c>Loc</c>).</summary>
    public static GameContext Context { get; private set; } = null!;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var ctx = new GameContext();

        // Order matters: Save loads first so Settings/Progression hydrate from it.
        var save = new SaveService();
        save.Load();
        ctx.Register<ISaveService>(save);

        var settings = new SettingsService(save);
        ctx.Register<ISettingsService>(settings);

        // Null-safe: shipping build #3 with an empty Boot.unity in CI — the SerializeField
        // may not be wired yet. LocalizationService already handles a null/empty table set
        // (keys round-trip as identity strings). Substitute an empty array so we never throw.
        var tables = _languageTables ?? System.Array.Empty<TextAsset>();
        if (_languageTables == null)
        {
            Debug.LogWarning(
                "[GameContextBootstrap] _languageTables not wired in Boot.unity — "
                + "Loc.Tr() will echo keys until the SerializeField is populated.");
        }
        var localization = new LocalizationService(tables);
        localization.SetLanguage(settings.Current.Language.ToIso());
        ctx.Register<ILocalizationService>(localization);

        // Null-safe: AudioMixer asset may be missing in CI / first boot. AudioMixerDriver
        // is internally null-tolerant (every setter no-ops when mixer is null), but we log
        // a clear warning so the missing reference is obvious in player.log.
        if (_mixer == null)
        {
            Debug.LogWarning(
                "[GameContextBootstrap] _mixer SerializeField is null — "
                + "AudioMixerDriver registered in mute-mode (set the BraveBunny.mixer asset on Boot's [Bootstrap] to enable audio).");
        }
        var mixerDriver = new AudioMixerDriver(_mixer);
        ctx.Register<IAudioMixerDriver>(mixerDriver);

        var music = new MusicStateMachine(mixerDriver);
        ctx.Register<IMusicStateMachine>(music);

        var sfx = new SfxDispatcher(mixerDriver, transform);
        ctx.Register<ISfxDispatcher>(sfx);

        // BgmGameplayDriver: scene/state → BGM snapshot routing. Pure-C# service; the
        // [SceneFlow] component in Boot.unity does the actual SceneManager.LoadSceneAsync,
        // so the driver's AttachSceneAutoTransitions() picks up activeSceneChanged after
        // that load completes. GameplayAudioBindings itself is a MonoBehaviour that lives
        // in the Run scene (channel SO refs are wired there) — it is NOT registered here.
        var bgmDriver = new BgmGameplayDriver(music);
        bgmDriver.AttachSceneAutoTransitions();
        ctx.Register<BgmGameplayDriver>(bgmDriver);

        var progression = new ProgressionService(save);
        ctx.Register<IProgressionService>(progression);

        var dailyStreak = new DailyStreakService(save);
        ctx.Register<IDailyStreakService>(dailyStreak);

        var achievements = new AchievementService(save);
        ctx.Register<IAchievementService>(achievements);

        var analytics = new AnalyticsService(new AnalyticsBackend());
        ctx.Register<IAnalyticsService>(analytics);

        var iap = new IapService();
        ctx.Register<IIapService>(iap);

        var ads = new AdsService();
        ctx.Register<IAdsService>(ads);

        // ---- Wave 7A: meta-progression — character unlocks ----
        // Builds the slug → UnlockCondition registry from the character catalogue
        // (each CharacterDefinition carries an UnlockConditionData inspector struct).
        // Empty catalogue is acceptable — CharacterUnlockService treats every slug as a
        // starter when its condition is null. The starter ("bunny") is still unlocked.
        var unlockRegistry = BuildCharacterUnlockRegistry(_characterCatalogue);
        var characterUnlock = new CharacterUnlockService(save, unlockRegistry);
        ctx.Register<ICharacterUnlockService>(characterUnlock);
        ctx.Register<CharacterUnlockService>(characterUnlock);

        // ---- Wave 7A: hit-feedback juice services ----
        // FeelConfig is the scalar tuning SO; HitstopService is a pure-C# service
        // (its MonoBehaviour host ticks the timer in the Run scene). The other three
        // (DamageNumberSpawner, DamageNumberPool, ScreenShakeController) are MonoBehaviours
        // — instantiated as child components so they can be inspected at runtime and
        // resolved via GameContext.TryGet<T> by the Run scene wiring step.
        if (_feelConfig == null)
        {
            Debug.LogWarning(
                "[GameContextBootstrap] _feelConfig SerializeField is null — "
                + "Wave 7A hit-feedback services skipped (HitstopService/ScreenShakeController/DamageNumberSpawner). "
                + "Wire FeelConfig.asset on the Boot [Bootstrap] to enable juice.");
        }
        else
        {
            var hitstop = new HitstopService(_feelConfig, _feelDefinition);
            ctx.Register<HitstopService>(hitstop);

            // Child container for the Wave 7A Feel MonoBehaviours so they live alongside
            // Bootstrap (DontDestroyOnLoad already applied to the root above).
            var feelRoot = new GameObject("[FeelServices]");
            feelRoot.transform.SetParent(transform, worldPositionStays: false);

            var screenShake = feelRoot.AddComponent<ScreenShakeController>();
            screenShake.BindConfig(_feelConfig);
            // Camera ref is wired by RunSceneWiring at run-time (Camera.main lives in Run scene).
            ctx.Register<ScreenShakeController>(screenShake);

            var pool = feelRoot.AddComponent<DamageNumberPool>();
            if (_damageNumberWidgetPrefab != null)
            {
                pool.Initialise(_damageNumberWidgetPrefab, _damageNumberPoolCapacity, feelRoot.transform);
            }
            else
            {
                Debug.LogWarning(
                    "[GameContextBootstrap] _damageNumberWidgetPrefab is null — "
                    + "DamageNumberPool registered but uninitialised; Spawn() will no-op until a prefab is wired.");
            }
            ctx.Register<DamageNumberPool>(pool);

            var spawner = feelRoot.AddComponent<DamageNumberSpawner>();
            spawner.Config = _feelConfig;
            spawner.Pool = pool;
            ctx.Register<DamageNumberSpawner>(spawner);
        }

        Context = ctx;
        Loc.Bind(localization);

        // Wave 7A: subscribe Systems-side meta services to the static run-end bridge.
        // The bridge fires after RunController.End raises RunEndedChannel; this is where
        // BGM transitions to the win/lose snapshot and character unlocks tick.
        // SubscribeRunEndIntegration is idempotent (no-op on second call).
        SubscribeRunEndIntegration(bgmDriver, characterUnlock);

        // ADR-0009: scan [BraveRegister] mechanics + raise GameContextReady.
        Bootstrapper.Complete(ctx);
    }

    // ---- Wave 7A helpers ----

    /// <summary>
    /// Build the slug → UnlockCondition registry that CharacterUnlockService consumes.
    /// Translates each CharacterDefinition.unlockCondition (inspector raw struct) to the
    /// runtime POCO via the Systems-side extension method. Returns an empty dict when no
    /// catalogue is wired (every IsUnlocked() query then resolves to false except for
    /// slugs whose save-file Owned/Unlocked flag was already set by a prior session).
    /// </summary>
    private static IReadOnlyDictionary<string, UnlockCondition?> BuildCharacterUnlockRegistry(
        CharacterDefinition[]? catalogue)
    {
        var dict = new Dictionary<string, UnlockCondition?>(System.StringComparer.Ordinal);
        if (catalogue == null) return dict;
        foreach (var def in catalogue)
        {
            if (def == null || string.IsNullOrEmpty(def.slug)) continue;
            dict[def.slug] = def.unlockCondition.ToRuntime();
        }
        return dict;
    }

    // Latched at Awake; subscriber re-entrancy on domain reload would dupe handlers.
    private bool _runEndBridgeSubscribed;

    private void SubscribeRunEndIntegration(BgmGameplayDriver bgm, CharacterUnlockService unlocks)
    {
        if (_runEndBridgeSubscribed) return;
        RunEndIntegrationBridge.RunEnded += OnRunEndedForMetaServices;
        _runEndBridgeSubscribed = true;

        // Capture refs in a closure-free way via fields so OnRunEndedForMetaServices doesn't
        // need to re-resolve from Context (Context may be torn down during shutdown).
        _bgmForRunEnd = bgm;
        _unlocksForRunEnd = unlocks;
    }

    private BgmGameplayDriver? _bgmForRunEnd;
    private CharacterUnlockService? _unlocksForRunEnd;

    private void OnRunEndedForMetaServices(RunEndReport report)
    {
        DispatchRunEndToMetaServices(report, _bgmForRunEnd, _unlocksForRunEnd);
    }

    /// <summary>
    /// Pure function: route a finished <see cref="RunEndReport"/> to the BGM driver
    /// (win/lose snapshot) and the character-unlock service (run completion + boss
    /// defeat tally). Public so EditMode tests can drive it with fakes for both
    /// meta services without spinning up the full <see cref="GameContextBootstrap"/>
    /// MonoBehaviour.
    /// </summary>
    public static void DispatchRunEndToMetaServices(
        RunEndReport report,
        BgmGameplayDriver? bgm,
        ICharacterUnlockService? unlocks)
    {
        if (report == null) return;

        // BGM: run-end win/lose snapshot.
        bgm?.EnterRunEnd(win: report.outcome == RunOutcome.Win);

        // Character unlocks: record the run completion for the active character, then
        // record the boss-defeat if this run cleared the boss. Graceful no-op when slug
        // is empty (e.g. naked-run tests or pre-loadout dev scenes).
        if (unlocks != null && !string.IsNullOrEmpty(report.characterId))
        {
            unlocks.RecordRunCompletion(
                report.characterId,
                waveReached: report.wavesCleared,
                bossesDefeatedThisRun: report.bossesKilled);

            if (report.deathCause == RunEndCause.BossDefeated && report.bossesKilled > 0)
            {
                // No boss-slug field on the report — we use "boss" as a sentinel slug.
                // UnlockCondition.DefeatBoss conditions referencing a specific boss slug
                // (e.g. "old-boar-king") therefore need to be re-checked via a dedicated
                // boss-kill subscription in a follow-up wave. The lifetime tally
                // (CharacterProfile.BossesDefeated) is already incremented inside
                // RecordRunCompletion above.
                unlocks.RecordBossDefeated("boss", report.characterId);
            }
        }
    }

    private void OnDestroy()
    {
        if (_runEndBridgeSubscribed)
        {
            RunEndIntegrationBridge.RunEnded -= OnRunEndedForMetaServices;
            _runEndBridgeSubscribed = false;
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // 03-save-system.md trigger: "App goes to background → single safety write".
        if (pause && Context != null && Context.TryGet<ISaveService>(out var save)) save.Save();
    }
}
