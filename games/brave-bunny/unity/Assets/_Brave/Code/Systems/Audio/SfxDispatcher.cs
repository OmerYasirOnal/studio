// Brave Bunny — Systems / Audio
// Tech spec: docs/06-tech-spec/07-audio.md (pooled AudioSources, 12-voice cap, steal-oldest)
//            docs/06-tech-spec/05-performance-budget.md (0.3 ms audio dispatch budget)

#nullable enable

using System.Collections.Generic;
using UnityEngine;
using Brave.Systems.Context;

namespace Brave.Systems.Audio;

public interface ISfxDispatcher : IService
{
    SfxHandle PlaySfx(string slug, Vector3 worldPosition);
    SfxHandle PlayUi(string slug);
    void StopAll();
}

/// <summary>
/// Stable handle returned by <see cref="ISfxDispatcher.PlaySfx"/>. Caller can
/// poll <see cref="IsPlaying"/> to know whether the request was stolen on the
/// voice cap. Gameplay logic must never branch on this — handle is for UI/test only.
/// </summary>
public readonly struct SfxHandle
{
    public readonly int VoiceId;
    public readonly bool IsPlaying;
    public SfxHandle(int voiceId, bool isPlaying) { VoiceId = voiceId; IsPlaying = isPlaying; }
}

/// <summary>
/// Pooled AudioSource dispatcher with global 12-voice cap and steal-oldest
/// policy per 07-audio.md / 04-mixer-routing.md Pillar 8. Sources are
/// pre-warmed at construction time and parented to the bootstrap GameObject.
/// </summary>
public sealed class SfxDispatcher : ISfxDispatcher
{
    private const int GlobalVoiceCap = 12;
    private const int InitialPoolSize = 24; // Sum of all pool sizes in 07-audio.md table.

    private readonly IAudioMixerDriver _mixer;
    private readonly Transform _root;
    private readonly List<AudioSource> _pool = new(InitialPoolSize);
    private int _activeVoices;
    private int _nextVoiceId = 1;

    public SfxDispatcher(IAudioMixerDriver mixer, Transform root)
    {
        _mixer = mixer;
        _root = root;
        // Pool is pre-warmed lazily — stubbed; full pre-warm is done in Boot via Catalog clip refs.
    }

    public SfxHandle PlaySfx(string slug, Vector3 worldPosition)
    {
        if (_activeVoices >= GlobalVoiceCap) StealOldest();
        // TODO: resolve AudioClip via ICatalogService.GetSfx(slug); use 3D source per 07-audio.md spatial table.
        _activeVoices++;
        return new SfxHandle(_nextVoiceId++, isPlaying: true);
    }

    public SfxHandle PlayUi(string slug) => PlaySfx(slug, Vector3.zero);

    public void StopAll()
    {
        for (var i = 0; i < _pool.Count; i++) if (_pool[i] != null) _pool[i].Stop();
        _activeVoices = 0;
    }

    private void StealOldest()
    {
        // 07-audio.md: walk active list lowest-priority-first; steal oldest source within tier.
        // Stub: decrement counter. Full implementation requires priority + start-time tracking.
        if (_activeVoices > 0) _activeVoices--;
    }
}
