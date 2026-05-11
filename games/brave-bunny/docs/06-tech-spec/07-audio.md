# Tech Spec 07 — Audio

> Owner: tech-architect. The Unity Audio Mixer wiring, voice-management policy, streaming/decompression strategy, ducking implementation, and per-platform audio tweaks for Brave Bunny. Authoritative cross-refs: `08-audio-bible/04-mixer-routing.md` (bus hierarchy + snapshots + ducking rules — single source of truth), `00-engine-and-version.md` (OGG Vorbis source, BGM streamed / SFX in-memory), `05-performance-budget.md` (12-voice cap, 0.3 ms audio dispatch budget, 12 MB SFX RAM cap). Sister doc: `06-rendering.md` for the parallel rendering pipeline.

## Mixer asset

Single asset at `unity/Assets/_Brave/Audio/Mixers/BraveBunny.mixer`. Bus hierarchy authored to **match `08-audio-bible/04-mixer-routing.md` exactly** — that doc is the source of truth; this doc only specifies the *implementation* details Unity needs.

```
Master (ceiling −6 dBFS true-peak limiter)
├── Music
│   ├── BGM
│   └── Stingers
├── SFX
│   ├── UI
│   ├── Combat
│   │   ├── Player
│   │   ├── Enemies
│   │   └── Impact
│   ├── World
│   └── Pickup
└── Voice (muted, reserved)
```

Default per-bus levels per `04-mixer-routing.md` table (`Music = −3 dB`, `Enemies = −3 dB`, `World = −12 dB`, etc.). Reproduced in the Mixer asset, **not** here, so changes go through the audio-bible doc.

## AudioSource architecture

Pooled per-channel, **never per-emitter**. An enemy prefab does not own an `AudioSource`; instead, when it needs to play `enemy_swarmer_hit`, it calls `IAudioService.PlaySfx(slug, worldPosition)` and the service borrows a pooled source from the appropriate bus channel.

### Pool sizes (pre-warmed at scene load)

| Pool | Source count | Bus assigned | Reason |
|---|---|---|---|
| `BgmPool` | 2 | Music / BGM | One playing + one for crossfade |
| `StingerPool` | 2 | Music / Stingers | Stinger overlap on rare boss-intro + level-up overlap |
| `UiPool` | 2 | SFX / UI | 2-voice cap from `04-mixer-routing.md` |
| `PlayerSfxPool` | 3 | SFX / Combat / Player | 3-voice cap |
| `EnemySfxPool` | 6 | SFX / Combat / Enemies | 6-voice cap (priority-cull lowest) |
| `ImpactSfxPool` | 4 | SFX / Combat / Impact | 4-voice cap |
| `WorldPool` | 1 | SFX / World | 1-voice ambient bed |
| `PickupPool` | 4 | SFX / Pickup | 4-voice cap |

**Total pooled `AudioSource` count = 24**, all parented to a single `[AudioRoot]` GameObject in `Boot.unity` and `DontDestroyOnLoad`'d. Memory: ~24 × 200 bytes engine overhead = ~5 KB; negligible.

## Voice cap and stealing

Global cap **12 concurrent SFX voices** per `04-mixer-routing.md` (Pillar 8). Enforced by the `IAudioService` implementation, not by Unity (Unity's per-platform polyphony cap is higher but inconsistent).

Steal policy: **steal-oldest on overflow**. Priority order from `04-mixer-routing.md`: Boss > Player > Pickup > Impact > Enemies > UI > Ambient. When over cap, the audio service walks the active source list lowest-priority-first and finds the oldest source within that priority tier to interrupt.

`IAudioService.PlaySfx()` returns a handle even when the request was stolen at queue time (no exception, no allocation) — the handle's `IsPlaying` flag reads `false`. Gameplay-engineer code never branches on whether a sound played; gameplay logic is deterministic regardless of audio outcomes.

## BGM streaming

BGM tracks are **streamed from disk** (Vorbis), not decoded into RAM:

- Unity import setting: `Load Type = Streaming`, `Compression Format = Vorbis`, `Quality = 50%`.
- File size per BGM track: ~1.5 MB on disk per 90-second loop.
- Memory cost: ~200 KB streaming buffer per active stream — under the 12 MB SFX RAM cap from `05-performance-budget.md`.
- Loop-point metadata: encoded as **Vorbis comment `LOOPSTART` / `LOOPEND` sample-frame offsets**. Read at clip-import time by an `AssetPostprocessor` and exposed on a sibling `BgmClipMeta.asset` ScriptableObject (Unity doesn't expose Vorbis comments on `AudioClip` directly).
- Playback uses two `AudioSource`s on the `BgmPool` and a 400 ms crossfade per `04-mixer-routing.md` snapshot transition default.

### iOS BGM loop click

Unity on iOS exhibits a 1-frame audible click at Vorbis loop boundaries when `LOOPSTART != 0`. **ADR-NEEDED-AUDIO-FORMAT** flagged here for tech-architect + sound-designer decision.

**Tentative decision pending QA validation:** ship Vorbis with `LOOPSTART/LOOPEND` pinned, accept the 1-frame click. If QA validates the click as audibly distracting on production hardware (iPhone 12 speaker + headphones, AAC Bluetooth), switch to **WAV-PCM 22 kHz mono for run-time BGM only** (Home/Lobby/BattlePass BGM stay Vorbis):

- Cost: +12 MB on disk across 5 biome BGMs + 1 boss-Meadow BGM (currently ~6 MB Vorbis, would become ~18 MB WAV).
- Frees us from the click; WAV-PCM has no loop-point ambiguity.
- Still fits the 200 MB App Store hot-zone budget from `05-performance-budget.md` (~192 MB total with this change).

This is filed as **ADR-0012 — BGM loop format on iOS** by tech-architect before Phase 5 build target validation.

## SFX in-memory

SFX clips are **fully decompressed at scene load** so they cost zero IO latency on play:

- Import setting per duration:

| Clip duration | Compression Format | Load Type | Reason |
|---|---|---|---|
| **< 0.5 s** | **ADPCM** | Decompress On Load | Tiny clips (UI ticks, hit puffs, gem pickups) — ADPCM beats Vorbis for short clips |
| **0.5 s – 3 s** | Vorbis (quality 70%) | Decompress On Load | Mid SFX (weapon fire, enemy death) |
| **> 3 s** | Vorbis (quality 50%) | Compressed In Memory | Long ambients, stingers — keep compressed in RAM, decode on demand |

- **Sample rate:** 22 kHz mono (iPhone 12) / **16 kHz mono** (iPhone SE 3, low-power per `05-performance-budget.md` degrade plan #5).
- Mono only — survivor-like camera does not have meaningful stereo content; saves half the RAM.
- Total decompressed SFX RAM budget: **12 MB** per `05-performance-budget.md` — leaves room for ~120 unique clips at average 100 KB decompressed each.

## Spatial audio

| Bucket | Spatial mode | Settings |
|---|---|---|
| **Combat SFX** | **3D (volume + slight pan)** | `spatialBlend = 0.6` (mostly 2D for clarity, slight 3D pan to give directional cue); `dopplerLevel = 0` (no Doppler — survivor camera doesn't need it); `rolloffMode = Linear`, `minDistance = 2`, `maxDistance = 12` |
| **No HRTF** | — | Unity's HRTF spatializer is CPU-heavy and survivor-likes don't need it; disabled across all bus channels |
| **UI SFX** | **2D** | `spatialBlend = 0` |
| **BGM** | **2D** | `spatialBlend = 0` |
| **Ambient bed (World)** | **2D** | `spatialBlend = 0` — bed is omnipresent |
| **Pickup SFX** | **2D** | `spatialBlend = 0` — pickup feedback is UI-feel, not spatial |

## Ducking implementation

Ducking is **parameter-driven**, not snapshot-driven (per `04-mixer-routing.md` mandate — avoids snapshot ping-pong).

| Trigger | Mixer parameter | Target | Easing |
|---|---|---|---|
| SFX peak > **−6 dBFS** | `_MusicDuck` | Music bus, **−4 dB** for 80 ms | 80 ms ease-in-out (Pillar 8 + `04-mixer-routing.md`) |
| `run_levelup` fires | `_MusicDuck` | **−4 dB** for 200 ms | 80 ms ease-in, 120 ms ease-out |
| `enemy_boss_die` fires | `_MusicDuck` | **−4 dB** for 600 ms | 80 ms in / 120 ms out |
| Hitstop active (elite 60 ms / boss 120 ms) | `_ImpactDuck` | small −2 dB on Impact bus only | 5 ms ramp |

`_MusicDuck` is exposed on the mixer; the audio service writes the parameter on a `Coroutine`-free animation curve (UniTask `UniTask.Delay` per `11-third-party.md`) so the run hot loop allocates zero per `05-performance-budget.md`.

### Recovery shape

After duck attack:

- **Attack:** 80 ms ease-in to −4 dB (per `04-mixer-routing.md` Pillar 8).
- **Hold:** duration above per trigger.
- **Recovery:** **120 ms ease-out** back to 0 dB. (`04-mixer-routing.md` does not specify recovery shape; tech-architect locks it here.)

## Snapshot crossfade

Mood states transition via `AudioMixerSnapshot.TransitionTo(duration)`:

| Transition | Duration | Notes |
|---|---|---|
| Splash → Home | **400 ms** | Default cubic crossfade |
| Home / Lobby → Run start | **800 ms** | Anticipation lift; `run_start` stinger overlaid at t=0 |
| Run → Boss snapshot | **600 ms** | `boss_intro_sting` overlaid at t=200 ms |
| Boss → Run-end Win/Lose | **400 ms** | Stinger immediate |
| Run-end → Home | **800 ms** | After tally fully visible |
| Default snapshot-to-snapshot | **400 ms** | Cubic crossfade per `04-mixer-routing.md` |

Open question from `04-mixer-routing.md`: snapshot transition CPU cost on iPhone SE 3 at 400 ms. **Mitigation:** if Profiler shows > 1 ms CPU spike, drop default to 200 ms; tracked in `05-performance-budget.md` failure-mode triage.

## Volume controls

User settings exposes 3 sliders (Master / Music / SFX) per `03-save-system.md` settings schema. Each slider:

- **UI** maps linear 0.0 → 1.0.
- **Internal** converts to **logarithmic dB** via `dB = log10(linear) * 20` (clamped at `linear = 0` → −80 dB → muted).
- Written to mixer parameters `_MasterVolume`, `_MusicVolume`, `_SFXVolume`.

## iOS system-silent-switch behavior

iOS mute switch **respected** at launch. `AVAudioSession.setCategory(.ambient)` so the silent switch mutes the app — matches user expectation for a mobile game (vs. games that ignore the switch like YouTube). Implementation in `Brave.Boot.IosAudioSessionSetup` runs once at boot.

Headphone route detection: subscribe to `AVAudioSession.routeChangeNotification`; on change, flip `_HeadphoneMode` parameter, which drives `_PlatformEQ_3k` per `04-mixer-routing.md`:

- **Speaker:** +2 dB at 3 kHz (broad bell, Q ≈ 1.0).
- **Headphones / Bluetooth:** 0 dB (neutral).

## Cross-references

- `08-audio-bible/04-mixer-routing.md` — bus hierarchy, default levels, snapshots, ducking rules (single source of truth).
- `00-engine-and-version.md` — OGG Vorbis source; BGM stream / SFX in-memory.
- `05-performance-budget.md` — 12-voice cap, 12 MB SFX RAM, 0.3 ms CPU audio dispatch, SE 3 degrade plan.
- `03-save-system.md` — `settings.audioMaster/Music/Sfx` persistence.
- **ADR-0012 (pending)** — BGM loop format on iOS.
