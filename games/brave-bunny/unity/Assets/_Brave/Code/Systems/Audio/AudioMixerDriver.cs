// Brave Bunny — Systems / Audio
// Tech spec: docs/06-tech-spec/07-audio.md (linear → dB conversion, snapshot transition)
//            docs/08-audio-bible/04-mixer-routing.md (bus hierarchy + ducking — source of truth)

#nullable enable

using System;
using UnityEngine;
using UnityEngine.Audio;
using Brave.Systems.Context;

namespace Brave.Systems.Audio;

public interface IAudioMixerDriver : IService
{
    void SetMusicVolume(float linear);
    void SetSfxVolume(float linear);
    void SetUiVolume(float linear);
    void SetMasterVolume(float linear);
    void SnapshotTransition(string snapshotName, float seconds);
    void SetDuck(float dbAttenuation, float attackSeconds, float holdSeconds, float releaseSeconds);
}

/// <summary>
/// Thin wrapper over Unity <see cref="AudioMixer"/>. Linear-to-dB conversion is
/// done here once per setter call (per 07-audio.md: <c>dB = log10(linear) * 20</c>,
/// clamp linear==0 → −80 dB). Snapshot transition delegates to Unity's built-in
/// cubic crossfade; durations are sourced from 07-audio.md snapshot table.
/// </summary>
public sealed class AudioMixerDriver : IAudioMixerDriver
{
    private const float MinDb = -80f;

    // Mixer exposed parameter names (authored on BraveBunny.mixer per 07-audio.md).
    private const string MasterParam = "_MasterVolume";
    private const string MusicParam = "_MusicVolume";
    private const string SfxParam = "_SFXVolume";
    private const string UiParam = "_UIVolume";
    private const string MusicDuckParam = "_MusicDuck";

    private readonly AudioMixer? _mixer;

    public AudioMixerDriver(AudioMixer? mixer) { _mixer = mixer; }

    public void SetMasterVolume(float linear) => WriteDb(MasterParam, linear);
    public void SetMusicVolume(float linear) => WriteDb(MusicParam, linear);
    public void SetSfxVolume(float linear) => WriteDb(SfxParam, linear);
    public void SetUiVolume(float linear) => WriteDb(UiParam, linear);

    public void SnapshotTransition(string snapshotName, float seconds)
    {
        if (_mixer == null) return;
        var snap = _mixer.FindSnapshot(snapshotName);
        if (snap == null) { Debug.LogWarning($"AudioMixerDriver: missing snapshot {snapshotName}"); return; }
        snap.TransitionTo(seconds);
    }

    /// <summary>
    /// Parameter-driven duck per 07-audio.md (avoids snapshot ping-pong).
    /// Stub: writes the attenuation immediately; full attack/hold/release shape
    /// is driven by <c>MusicStateMachine</c> coroutine via UniTask.
    /// </summary>
    public void SetDuck(float dbAttenuation, float attackSeconds, float holdSeconds, float releaseSeconds)
    {
        if (_mixer == null) return;
        _mixer.SetFloat(MusicDuckParam, dbAttenuation);
        // TODO: drive an actual easing curve through UniTask per 07-audio.md "Recovery shape".
    }

    /// <summary>Read-only helper for tests / debug HUD.</summary>
    public float ReadDb(string param)
    {
        if (_mixer == null) return MinDb;
        return _mixer.GetFloat(param, out var v) ? v : MinDb;
    }

    private void WriteDb(string param, float linear)
    {
        if (_mixer == null) return;
        var db = linear <= 0f ? MinDb : Mathf.Log10(Mathf.Clamp01(linear)) * 20f;
        _mixer.SetFloat(param, db);
    }
}
