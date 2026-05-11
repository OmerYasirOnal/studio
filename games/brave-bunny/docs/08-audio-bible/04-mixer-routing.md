# Mixer Routing — Brave Bunny

> Owner: art-director (audio sub-role). Cross-refs: `00-audio-overview.md` (master ceiling, voice cap), `01-bgm-spec.md` (12 BGM tracks → snapshots), `02-sfx-spec.md` (per-SFX bus assignment), `docs/02-gdd/11-feel-pillars.md` (Pillar 8 mix discipline), `games/brave-bunny/CLAUDE.md` (perf — AudioMixer adds negligible CPU at 12-voice cap). The **Unity AudioMixer asset structure** with bus groups, snapshots, ducking rules, and per-platform tweaks.

## Bus hierarchy

```
Master (ceiling −6 dBFS, integrated target −23 LUFS)
├── Music
│   ├── BGM (state-driven snapshots: Home, Lobby, Run-Meadow, Run-Beach,
│   │       Run-Forest, Run-Cavern, Run-Snow, Boss-Meadow, BattlePass)
│   └── Stingers (run_levelup, boss_intro_sting, boss_phase_change,
│                 run_end_win, run_end_lose, cold_start_splash)
├── SFX
│   ├── UI               (all ui_* slugs)
│   ├── Combat
│   │   ├── Player       (weapon_*_fire, hero_dash, hero_heal)
│   │   ├── Enemies      (enemy_swarmer_*, enemy_elite_*, enemy_boss_*,
│   │   │                 boss_telegraph_warn)
│   │   └── Impact       (enemy_*_hit, hero_hit — hit-stops trigger 5 ms
│   │                     duck on Impact bus only)
│   ├── World            (ambient_<biome>_bed per biome)
│   └── Pickup           (run_pickup_xp_*, run_pickup_gold, run_pickup_heart)
└── Voice                (UNUSED at launch — reserved for future)
```

## Per-bus default levels

| Bus | Default level | Ceiling | Notes |
|---|---|---|---|
| Master | 0 dB | −6 dBFS hard | True-peak limiter on this bus only |
| Music | −3 dB | n/a | Sits below SFX so combat reads |
| Music / BGM | 0 dB | n/a | Per-state snapshot may override |
| Music / Stingers | +0 dB | n/a | Fanfares need full energy |
| SFX | 0 dB | n/a | |
| SFX / UI | −3 dB | n/a | UI ticks never overwhelm gameplay |
| SFX / Combat / Player | 0 dB | n/a | |
| SFX / Combat / Enemies | −3 dB | n/a | Lots of enemies — pull back to avoid mud |
| SFX / Combat / Impact | 0 dB | n/a | Impact pops are the gameplay-feel layer |
| SFX / World | −12 dB | n/a | Ambient is *bed*, not foreground |
| SFX / Pickup | −3 dB | n/a | Soft chimes |
| Voice | −∞ dB (muted) | n/a | Reserved |

## Snapshot list (state-driven BGM transitions)

Each BGM state has a named snapshot. Snapshot stores: which BGM track is unmuted, ambient world bed selection, music bus level adjustments.

| Snapshot | Active BGM | Active ambient bed | Music level |
|---|---|---|---|
| `Snapshot_Splash` | Cold-start splash stinger | none | 0 dB |
| `Snapshot_Home` | Home loop | none | −3 dB |
| `Snapshot_Lobby` | Lobby loop | none | −3 dB |
| `Snapshot_Run_Meadow` | Run — Meadow | ambient_meadow_bed | 0 dB |
| `Snapshot_Run_Beach` | Run — Beach | ambient_beach_bed | 0 dB |
| `Snapshot_Run_Forest` | Run — Forest | ambient_forest_bed | 0 dB |
| `Snapshot_Run_Cavern` | Run — Cavern | ambient_cavern_bed | 0 dB |
| `Snapshot_Run_Snow` | Run — Snow | ambient_snow_bed | 0 dB |
| `Snapshot_Boss_Meadow` | Boss — Meadow | ambient_meadow_bed at −18 dB | 0 dB |
| `Snapshot_BattlePass` | Battle pass loop | none | −3 dB |
| `Snapshot_Run_End_Win` | Run-end win stinger | fade-out bed over 1.5 s | +3 dB |
| `Snapshot_Run_End_Lose` | Run-end lose stinger | fade-out bed over 1.5 s | 0 dB |

## Snapshot transition rules

| Transition | Crossfade duration | Notes |
|---|---|---|
| Default snapshot-to-snapshot | **400 ms** | Cubic cross-fade |
| Splash → Home | 400 ms | After splash stinger completes |
| Lobby → Run start | **800 ms** (with `run_start` SFX stinger overlaid at 0 ms) | Anticipation lift |
| Run → Boss snapshot | **600 ms** | + `boss_intro_sting` overlaid at 200 ms in |
| Boss → Run-end Win/Lose | **400 ms** | + `run_end_win` / `run_end_lose` stinger immediate |
| Run-end → Home | **800 ms** | After tally screen fully visible |

## Ducking rules

| Trigger | Affected bus | Amount | Duration |
|---|---|---|---|
| SFX peak > **−6 dBFS** | Music | **−4 dB** | 80 ms ease-in-out (Pillar 8) |
| `run_levelup` fires | Music | **−4 dB** | 200 ms (Pillar 8) |
| `enemy_boss_die` fires | Music | **−4 dB** | 600 ms (boss-death fanfare cuts through) |
| Hitstop active (60 ms elite / 120 ms boss) | Impact bus | small 5 ms duck on Impact bus only | Preserves hit feel |
| Modal open | UI bus | unaffected | Modal SFX rides on existing UI mix |

## Concurrent voice cap

| Bus | Voice cap |
|---|---|
| Master (global) | **12 voices** (Pillar 8) |
| Combat / Enemies | 6 voices (priority-cull lowest) |
| Combat / Player | 3 voices |
| Combat / Impact | 4 voices |
| UI | 2 voices |
| World | 1 voice (ambient bed only) |
| Pickup | 4 voices |

Lowest-priority voice culls when over-cap. Priority order: Boss > Player > Pickup > Impact > Enemies > UI > Ambient.

## Per-platform mix tweaks

| Platform | Bus | Adjustment | Why |
|---|---|---|---|
| iOS (speaker) | Master EQ | **+2 dB at 3 kHz** (broad bell, Q ≈ 1.0) | Compensates iPhone tinny speaker; voices/chimes read better |
| iOS (headphones) | Master EQ | none (neutral) | Detected via `AVAudioSession.routeChange`; gameplay-engineer drives parameter swap |
| Android | Master EQ | +1 dB at 3 kHz | Less aggressive than iOS — varied speaker quality |
| Bluetooth speaker (any platform) | Master EQ | none + soft-knee compressor on Master | A2DP latency makes ducking less precise — compressor smooths |

## Mixer parameter exposes (gameplay-engineer reads these)

| Parameter | Type | Range | Use |
|---|---|---|---|
| `_RunIntensity` | float | 0.0 → 1.0 | Cross-fades BGM `base`/`high` stems |
| `_MusicDuck` | float | -12 → 0 dB | Driven by SFX peak detector |
| `_MasterPause` | bool | true / false | True = pause AudioMixer; used during in-run pause modal |
| `_PlatformEQ_3k` | float | 0 → +3 dB | Set at startup based on output route |
| `_HeadphoneMode` | bool | true / false | Drives `_PlatformEQ_3k` |
| `_MusicVolume`, `_SFXVolume` | float | -80 → 0 dB | User settings sliders |

## Hand-off

- Mixer asset path: `unity/Assets/Audio/Mixers/BraveBunny.mixer`
- Snapshot list authored at content time; transitions triggered by gameplay-engineer via `AudioMixerSnapshot.TransitionTo(duration)`
- All ducking is **parameter-driven**, not snapshot-driven (avoids snapshot ping-pong)
- **Open question for tech-architect**: Unity AudioMixer snapshot transition cost on iPhone SE 3 at 400 ms — measure baseline; if > 1 ms CPU spike, consider reducing transition to 200 ms.
- **Open question for sound-designer (deferred)**: verify the dual-stem `base`/`high` cross-fade works seamlessly at the 4-second cross-fade — needs phase-coherent stems.
