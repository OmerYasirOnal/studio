# Audio Overview — Brave Bunny

> Owner: art-director (audio sub-role). Cross-refs: `docs/02-gdd/11-feel-pillars.md` (Pillar 8 mix discipline — master ceiling −6 dB, 12-voice cap, fanfare ducking), `docs/07-art-bible/00-style-overview.md` (tone parity — cartoon, warm, never grim), `games/brave-bunny/CLAUDE.md` (no paid services — Pixabay royalty-free + Freesound CC0 + incompetech CC-BY only). All values authoritative for sound-designer (deferred role) and ui-engineer / gameplay-engineer when wiring AudioMixer.

## Audio thesis

Brave Bunny's audio is **warm, bouncy, and cozy**. Every BGM track is consonant and major-key-biased; every SFX has a cartoon "pop" quality (no thuds, no grit, no industrial textures). The mix has **6 dB of headroom** at all times so fanfares (level-up, boss death, run-end) genuinely cut through. We use a state-machine BGM system so transitions between home / lobby / run-low / run-high / boss feel curated rather than jarring.

## Mood mapping

| Game state | Mood descriptor | BGM characteristic |
|---|---|---|
| Cold-start splash | brand, gentle | 5 s C-major stinger |
| Home / lobby | warm, calm, inviting | Acoustic-pluck + light pad, ~90-100 BPM |
| Loadout / pre-run | anticipation, soft hype | Slight tempo bump, sparse percussion |
| Run low-intensity (waves 1-3) | upbeat, bouncy | Pluck melody, gentle bass, 115-120 BPM |
| Run high-intensity (waves 4+) | driving, exciting | Same key/tempo, +percussion + counter-melody, parallel layer added |
| Boss fight | tense-but-cute | Lower-register pluck, half-time feel, suspended-chord motif; never minor-grim |
| Run-end win | fanfare, triumphant | 4-note arpeggio + sparkle layer, 1.5 s |
| Run-end lose | gentle, dignified | Soft descending pluck, 1.2 s — per Pillar 6 dignity |
| Battle pass screen | reward, soft | Mid-tempo F-major loop |

## Voice acting

**NONE at launch.** Rationale:

1. **Cost**: voice acting requires paid talent or paid TTS — both forbidden per CLAUDE.md zero-paid-API rule.
2. **Localization burden**: launching with voice means re-recording per language; we target 4+ languages at launch.
3. **Narrative-by-text + ambient-SFX is enough**: Brave Bunny is action-roguelite, not narrative-driven. Characters express through animation + signature SFX cues (Bunny hop, Fox dash whoosh, Owl coo).

Reserved AudioMixer bus exists for post-launch addition. See `04-mixer-routing.md`.

## Dynamic range strategy

| Metric | Target | Why |
|---|---|---|
| Integrated loudness (LUFS) | **−23 LUFS** | EBU R128 target — matches Apple Music / Spotify loudness norm; safe on mobile speakers |
| True-peak ceiling | **−1 dBFS** | Prevents inter-sample peaks from clipping on AAC re-encode |
| Master ceiling | **−6 dBFS** | Per Pillar 8 — leaves 6 dB headroom for fanfares |
| Ducking trigger | SFX peak > **−6 dBFS** | Triggers music duck −4 dB for 80 ms |
| Concurrent voice cap | **12** | Lowest-priority voice culls when over-cap (per Pillar 8) |

## Format + compression

| Asset class | Format | Bitrate | Loop strategy |
|---|---|---|---|
| BGM | OGG Vorbis | 96 kbps stereo | Seamless loop point in file metadata; Unity AudioSource loop-on |
| SFX | OGG Vorbis | 128 kbps mono | One-shot; round-robin variants per slug |
| UI SFX | OGG Vorbis | 128 kbps mono | One-shot |
| Boss intro stinger | OGG Vorbis | 128 kbps stereo | One-shot, plays into looping BGM |

Total audio on-disk budget: **~12 MB** (12 BGM × ~600 KB + ~150 SFX files × ~30 KB avg). Well within the 200 MB app-size hot zone (`08-asset-budget.md`).

## Pillar cross-references

- **Pillar 1 — Every kill must shake the room**: 1-of-3 round-robin enemy-death stinger at −9 dB trash / −6 dB elite / −3 dB boss. Spec'd in `02-sfx-spec.md`.
- **Pillar 2 — Level-up celebration**: 4-note arpeggio at −3 dB, 600 ms; music ducks −4 dB for 200 ms. Spec'd in `02-sfx-spec.md` (`run_levelup`) + ducking rules in `04-mixer-routing.md`.
- **Pillar 3 — Pickup satisfying**: soft chime at −3 dB, pitch-shifted by tier. Round-robin 4 variants.
- **Pillar 4 — Auto-attack has impact**: per-weapon SFX with round-robin variants; hit SFX at −6 dB.
- **Pillar 5 — UI taps responsive**: soft "tick" SFX at −12 dB within 120 ms of pointer-up.
- **Pillar 6 — Death is dignified**: 800 ms wind-down stinger at −6 dB.
- **Pillar 7 — Screen always alive**: per-biome ambient bed plays under BGM.
- **Pillar 8 — Mix never crushes**: master ceiling −6 dB; ducking on fanfares.

## Hand-off

- BGM track specs: `01-bgm-spec.md`
- SFX catalog: `02-sfx-spec.md`
- Source procurement: `03-source-shortlist.md`
- AudioMixer routing: `04-mixer-routing.md`
- Open question for tech-architect: confirm Unity AudioMixer snapshot transition latency on iPhone SE 3 — we spec 400 ms but want measured baseline.
