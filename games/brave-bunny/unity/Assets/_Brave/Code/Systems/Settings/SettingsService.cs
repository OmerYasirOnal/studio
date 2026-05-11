// Brave Bunny — Systems / Settings
// Tech spec: docs/06-tech-spec/03-save-system.md (Settings save trigger; never PlayerPrefs)
//            docs/06-tech-spec/07-audio.md (linear → dB conversion mandate)

#nullable enable

using System;
using Brave.Systems.Context;
using Brave.Systems.Save;

namespace Brave.Systems.Settings;

public interface ISettingsService : IService
{
    SettingsData Current { get; }
    event Action<SettingsData>? OnChanged;
    void SetAudioMaster(float linear);
    void SetAudioMusic(float linear);
    void SetAudioSfx(float linear);
    void SetHaptics(bool on);
    void SetLanguage(LanguageCode code);
    void Commit();
}

/// <summary>
/// User-facing settings (3 audio sliders + haptics + low-power + tap-to-move
/// + language). One save write per modal-close per 03-save-system.md — sliders
/// do NOT trigger a save on every tick.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly ISaveService _save;

    public SettingsData Current { get; }

    public event Action<SettingsData>? OnChanged;

    public SettingsService(ISaveService save)
    {
        _save = save;
        Current = Hydrate(save.Data.Settings);
    }

    public void SetAudioMaster(float linear) { Current.AudioMaster = Clamp01(linear); RaiseChanged(); }
    public void SetAudioMusic(float linear)  { Current.AudioMusic  = Clamp01(linear); RaiseChanged(); }
    public void SetAudioSfx(float linear)    { Current.AudioSfx    = Clamp01(linear); RaiseChanged(); }
    public void SetHaptics(bool on)          { Current.HapticsEnabled = on; RaiseChanged(); }
    public void SetLanguage(LanguageCode code) { Current.Language = code; RaiseChanged(); }

    /// <summary>Flush settings into the save POCO and trigger a save write. Called on modal close.</summary>
    public void Commit()
    {
        var s = _save.Data.Settings;
        s.AudioMaster = Current.AudioMaster;
        s.AudioMusic = Current.AudioMusic;
        s.AudioSfx = Current.AudioSfx;
        s.HapticsEnabled = Current.HapticsEnabled;
        s.LowPowerMode = Current.LowPowerMode;
        s.TapToMove = Current.TapToMove;
        s.Language = Current.Language.ToIso();
        _save.Save(); // 03-save-system.md trigger: "Settings changed"
    }

    private static SettingsData Hydrate(SaveData.SettingsSection raw) => new()
    {
        AudioMaster = raw.AudioMaster,
        AudioMusic = raw.AudioMusic,
        AudioSfx = raw.AudioSfx,
        HapticsEnabled = raw.HapticsEnabled,
        LowPowerMode = raw.LowPowerMode,
        TapToMove = raw.TapToMove,
        Language = LanguageCodeExtensions.FromIso(raw.Language),
    };

    private void RaiseChanged() => OnChanged?.Invoke(Current);

    private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
}
