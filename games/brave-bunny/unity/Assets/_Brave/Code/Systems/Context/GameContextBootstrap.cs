// Brave Bunny — Systems / Context
// Tech spec: docs/06-tech-spec/08-state-machine.md (Boot entry actions)
//            docs/06-tech-spec/09-event-bus.md (service registry table)
// Lives on the [Bootstrap] GameObject in _Brave/Scenes/Boot.unity.

#nullable enable

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

        Context = ctx;
        Loc.Bind(localization);

        // ADR-0009: scan [BraveRegister] mechanics + raise GameContextReady.
        Bootstrapper.Complete(ctx);
    }

    private void OnApplicationPause(bool pause)
    {
        // 03-save-system.md trigger: "App goes to background → single safety write".
        if (pause && Context != null && Context.TryGet<ISaveService>(out var save)) save.Save();
    }
}
