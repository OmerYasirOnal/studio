// Brave Bunny — Systems / Settings
// Tech spec: docs/06-tech-spec/03-save-system.md (settings schema block)
//            docs/06-tech-spec/07-audio.md (3 sliders: master/music/sfx, linear → dB)

#nullable enable

namespace Brave.Systems.Settings;

/// <summary>
/// In-memory POCO that mirrors <c>save.settings</c>. The settings service
/// owns the canonical instance and writes it back into the save POCO on every
/// change for atomic persistence (see 03-save-system.md trigger list:
/// "Settings changed").
/// </summary>
public sealed class SettingsData
{
    public float AudioMaster = 0.8f;
    public float AudioMusic = 0.7f;
    public float AudioSfx = 0.9f;
    public bool HapticsEnabled = true;
    public bool LowPowerMode;
    public bool TapToMove;
    public LanguageCode Language = LanguageCode.En;
    // Wave 10 QoL — gates the FPS counter overlay (no UI yet, cheat-only).
    public bool DevModeEnabled;
}
