// Brave Bunny — Systems / Audio
// Tech spec: docs/06-tech-spec/07-audio.md (BGM streamed from disk, 400 ms crossfade)
//            docs/08-audio-bible/04-mixer-routing.md (BgmPool = 2 sources for crossfade)
// One playing AudioSource + one for crossfade target; ping-pong on every Play().
// Loop-point metadata flagged for ADR-0012 (BGM loop format on iOS).

#nullable enable

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Brave.Systems.Audio;

/// <summary>
/// Two-source BGM crossfader. Bound to the <c>BgmPool</c> (2 AudioSources on
/// the Music/BGM bus, pre-warmed at scene load per 07-audio.md). Default fade
/// is 400 ms (cubic snapshot-transition default); RunIntro and Run-end use
/// 800 ms — caller passes the duration.
/// </summary>
public sealed class BgmController : MonoBehaviour
{
    [SerializeField] private AudioSource? _sourceA;
    [SerializeField] private AudioSource? _sourceB;
    [SerializeField] private AudioMixerGroup? _bgmGroup;

    private bool _bIsActive;
    private Coroutine? _activeFade;

    /// <summary>The source currently considered the foreground BGM.</summary>
    public AudioSource? CurrentSource => _bIsActive ? _sourceB : _sourceA;

    private void Awake()
    {
        EnsureSources();
    }

    private void EnsureSources()
    {
        if (_sourceA == null) _sourceA = NewSource("BgmA");
        if (_sourceB == null) _sourceB = NewSource("BgmB");
    }

    private AudioSource NewSource(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, worldPositionStays: false);
        var src = go.AddComponent<AudioSource>();
        src.loop = true;
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D per 07-audio.md spatial table.
        if (_bgmGroup != null) src.outputAudioMixerGroup = _bgmGroup;
        return src;
    }

    /// <summary>
    /// Crossfade from the current source to <paramref name="next"/>. If no track
    /// is currently playing, this fades the new clip in from silence.
    /// </summary>
    public void Play(AudioClip next, float durationSeconds = 0.4f, float targetVolume = 1f)
    {
        EnsureSources();
        if (next == null) return;

        var incoming = _bIsActive ? _sourceA! : _sourceB!;
        var outgoing = _bIsActive ? _sourceB! : _sourceA!;

        if (incoming.clip == next && incoming.isPlaying) return; // already current

        incoming.clip = next;
        incoming.volume = 0f;
        incoming.time = 0f;
        incoming.Play();

        if (_activeFade != null) StopCoroutine(_activeFade);
        _activeFade = StartCoroutine(CrossfadeRoutine(outgoing, incoming, durationSeconds, targetVolume));
        _bIsActive = !_bIsActive;
    }

    /// <summary>Stop both sources immediately. Used on hard scene transitions / app pause.</summary>
    public void StopAll()
    {
        if (_activeFade != null) { StopCoroutine(_activeFade); _activeFade = null; }
        if (_sourceA != null) _sourceA.Stop();
        if (_sourceB != null) _sourceB.Stop();
    }

    private IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming, float duration, float targetVolume)
    {
        if (duration <= 0f)
        {
            incoming.volume = targetVolume;
            outgoing.Stop();
            outgoing.volume = 0f;
            _activeFade = null;
            yield break;
        }

        var startOutVol = outgoing.volume;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            // Equal-power crossfade for smoother perceived loudness during overlap.
            var inGain = Mathf.Sin(t * 0.5f * Mathf.PI);
            var outGain = Mathf.Cos(t * 0.5f * Mathf.PI);
            incoming.volume = inGain * targetVolume;
            outgoing.volume = outGain * startOutVol;
            yield return null;
        }

        incoming.volume = targetVolume;
        outgoing.Stop();
        outgoing.volume = 0f;
        _activeFade = null;
    }
}
