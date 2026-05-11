# BGM Spec — Brave Bunny

> Owner: art-director (audio sub-role). Cross-refs: `00-audio-overview.md` (mood mapping + LUFS strategy), `04-mixer-routing.md` (snapshot transitions), `docs/02-gdd/11-feel-pillars.md` (Pillar 8 mix headroom), `03-source-shortlist.md` (where to fetch). The **12 launch BGM tracks** + per-track tempo/key/loop structure. All tracks **CC0 / CC-BY / royalty-free** — no paid music.

## Track table (12 launch BGM tracks)

| # | State / scene | Tempo (BPM) | Key (suggested) | Loop length | Mood descriptor |
|---|---|---|---|---|---|
| 1 | **Home** (lobby / main menu) | 90 BPM | C major | 1:30 | cozy / inviting / acoustic-pluck |
| 2 | **Lobby / loadout** (pre-run) | 100 BPM | F major | 1:00 | anticipation / sparse percussion |
| 3 | **Run — Meadow** | 120 BPM | G major | 2:30 | bright / pluck / bouncy |
| 4 | **Run — Beach** | 115 BPM | C major | 2:30 | warm-breeze / ukulele-feel |
| 5 | **Run — Forest** | 125 BPM | D minor (modal-bright) | 2:30 | playful-mystery / wooden percussion |
| 6 | **Run — Cavern** | 110 BPM | A minor (sus-chord biased) | 2:30 | wonder / glow / ethereal pad |
| 7 | **Run — Snow** | 120 BPM | F# minor | 2:30 | cool-bounce / bell layer |
| 8 | **Boss — Meadow** (template; alt bosses re-key per biome) | 140 BPM | G major (suspended) | 1:30 + 4-bar intro stinger | playful-tense / half-time bass |
| 9 | **Run-end win** (stinger, not loop) | 100 BPM | C major | 0:08 | fanfare / triumphant |
| 10 | **Run-end lose** (stinger, not loop) | 80 BPM | C major | 0:10 | gentle / dignified (Pillar 6) |
| 11 | **Battle pass screen** | 90 BPM | F major | 1:00 | reward / soft / pad-heavy |
| 12 | **Cold-start splash** (stinger, not loop) | 90 BPM | C major | 0:05 | brand / signature pluck |

## Loop technique

| Element | Spec | Why |
|---|---|---|
| Phrase length | 8 bars | Half-loop natural breath point |
| Run music structure | **A-B-A-B-C-B** | Adds variation without doubling track length |
| Intro stinger | 1-4 bars, plays once on state entry | Smooths snapshot transition |
| Outro variant | 1-2 bars, plays on state exit | Cuts off cleanly into next BGM |
| Loop point metadata | Authored in OGG `LOOPSTART` / `LOOPLENGTH` tags | Unity AudioSource respects via custom importer |
| Crossfade between BGM | 400 ms (per `04-mixer-routing.md`) | Snapshot transition default |

## Per-state intensity layers (run BGM only)

For each **Run — <Biome>** track, sound-designer authors **2 stems**:

| Stem | When mixed in | Mix level |
|---|---|---|
| `base` (pluck + bass + light percussion) | Always — plays from wave 1 | 0 dB |
| `high` (additional percussion + counter-melody) | Mixed in when on-screen enemy count > 50 | +0 dB cross-fade over 4 s |

The `high` stem mixes in via AudioMixer parameter `_RunIntensity` (0.0 → 1.0). Gameplay-engineer drives this parameter from the enemy-count signal. This gives the moment-to-moment feel of "the music is keeping up with me" without requiring 2 separate tracks per biome (saves ~7 MB on-disk).

## Sample budget per track

| Track type | Target on-disk size (OGG 96 kbps stereo) |
|---|---|
| 1:30 loop (Home, Lobby, BP screen) | ~1.1 MB |
| 2:30 run loop (5 biomes) | ~1.8 MB |
| 1:30 boss loop | ~1.1 MB |
| 0:05-0:10 stinger | ~80 KB |

Estimated total BGM on-disk: **~12 tracks × ~600 KB avg = ~7.2 MB**. Matches `08-asset-budget.md` BGM row.

## Source candidates (per `03-source-shortlist.md`)

| Tier | Source | License | Notes |
|---|---|---|---|
| Primary | **Pixabay royalty-free music** | royalty-free (no attribution) | Best fit for cartoon-cozy mood; large library |
| Secondary | **Kevin MacLeod / incompetech.com** | CC-BY 4.0 | High quality; attribution recorded in `LICENSES.md` + credits screen |
| Tertiary | **Free Music Archive** (CC0 filter) | CC0 | Sparser pickings for our specific mood; fallback only |
| Custom | Compose in **LMMS / Bitwig demo / GarageBand** | engine-authored = CC0 for our project | If gap, art-director composes |

## Track shortlist for vertical slice

Per `08-asset-budget.md` vertical-slice list, **7 tracks ship in the slice**:

1. Home (track 1)
2. Lobby (track 2)
3. Run — Meadow (track 3)
4. Boss — Meadow (track 8)
5. Run-end win (track 9)
6. Run-end lose (track 10)
7. Cold-start splash (track 12)

## Vertical-slice authoring order

1. Home (sells the brand on cold start)
2. Run — Meadow (the dominant track during gameplay)
3. Run-end win + lose (closing emotional beat per Pillar 6)
4. Boss — Meadow
5. Lobby
6. Cold-start splash
7. (Post-slice) Battle pass + 4 other biomes + 4 other bosses

## Hand-off

- Tracks land in `unity/Assets/Audio/BGM/<state>_<biome>.ogg`.
- Loop-point metadata authored via `oggz-tools` or Audacity; Unity custom importer parses `LOOPSTART` + `LOOPLENGTH`.
- Snapshot mappings per state in `04-mixer-routing.md`.
- Asset-curator + sound-designer (deferred) own track sourcing per `03-source-shortlist.md`.
- **Open question for tech-architect:** confirm OGG seamless loop on iOS AVAudioEngine (occasional reports of 1-frame click on Vorbis); fallback is WAV-PCM with 2× on-disk cost.
