# Audio Source Shortlist — Brave Bunny

> Owner: art-director (audio sub-role). Cross-refs: `core/docs/asset-policy.md` (CC0/OFL/MIT/CC-BY only), `00-audio-overview.md` (no paid services), `01-bgm-spec.md` (12-track list), `02-sfx-spec.md` (~50 SFX catalog). Single-source procurement list for asset-curator + (deferred) sound-designer. **Zero paid music or SFX libraries.**

## License legend

| Code | Meaning | Attribution required? |
|---|---|---|
| **CC0** | Public-domain dedication | No |
| **CC-BY** | Creative Commons Attribution | Yes — record in `LICENSES.md` + in-game credits screen |
| **royalty-free** | Pixabay terms (no attribution required, no resale) | No |
| **OFL** | (fonts — not audio) | n/a here |

## BGM source mapping (12 tracks)

| Track | Primary source | License | Attribution? | Notes |
|---|---|---|---|---|
| 1. Home | **Pixabay** (search: "cozy acoustic loop", "happy ukulele") | royalty-free | no | Filter by mood "happy" + duration ≤ 2:00 |
| 2. Lobby | **Pixabay** | royalty-free | no | Tempo-up variant of Home; can author from same source |
| 3. Run — Meadow | **incompetech.com** (Kevin MacLeod — search: "Mister Exposition", "Carefree") | CC-BY 4.0 | yes | Record artist + URL in LICENSES.md + credits |
| 4. Run — Beach | **Pixabay** + **incompetech** fallback | mixed | depends | Ukulele/marimba style |
| 5. Run — Forest | **incompetech** (search: "Dreamer", "Sneaky Snitch") | CC-BY 4.0 | yes | Wooden percussion bias |
| 6. Run — Cavern | **Free Music Archive** (search: "ambient", "ethereal" + CC0 filter) | CC0 | no | Glow-pad feel |
| 7. Run — Snow | **Pixabay** | royalty-free | no | Bell-layer biased |
| 8. Boss — Meadow | **incompetech** (search: "Volatile Reaction" tempo, retoned major) | CC-BY 4.0 | yes | Suspended-chord motif |
| 9. Run-end win | **Pixabay** stinger pack | royalty-free | no | 8-sec fanfare |
| 10. Run-end lose | **Pixabay** or **custom (LMMS)** | royalty-free / CC0 | no | Gentle descending pluck |
| 11. Battle pass | **Pixabay** | royalty-free | no | Soft pad-heavy reward loop |
| 12. Cold-start splash | **custom (LMMS)** | CC0 (we author) | no | 5-sec signature stinger — brand-load |

**Custom-authored tracks**: 1-2 expected (cold-start splash + maybe run-end lose). Total audio composition effort: ~4 hr if needed.

## SFX source mapping (~50 SFX, ~92 files with round-robin)

| Bucket | Primary source | License | Attribution? |
|---|---|---|---|
| UI clicks / pops / chimes | **Kenney UI Audio pack (CC0)** | CC0 | no |
| Combat hits / thuds / poofs | **Freesound** (CC0 filter — search: "cartoon hit", "poof", "thump") | CC0 | no |
| Pickup chimes / sparkles | **Freesound** CC0 + **Kenney Interface Sounds** | CC0 | no |
| Enemy death poofs | **Freesound** CC0 (search: "puff", "cartoon pop") + pitch-shift variants | CC0 | no |
| Boss stingers (intro, phase, death) | **Freesound** CC0 (search: "orchestral hit", "cute fanfare") | CC0 | no |
| Hero voice (cute pip, "ouch") | **Freesound** CC0 (search: "cute creature", "cartoon animal") — NOT human voice | CC0 | no |
| Weapon SFX (carrot fire, sunbeam hum, daisy explode) | **Freesound** CC0 + custom layered in Audacity | CC0 | no |
| Ambient biome beds | **Freesound** CC0 (search: "meadow ambient loop", "cavern drip", "beach wave loop") | CC0 | no |
| Endgame fanfares / wind-downs | **Freesound** CC0 + **Pixabay** | CC0 / royalty-free | no |
| Meta unlocks (character, weapon, pass) | **Kenney Interface Sounds** + **Freesound** | CC0 | no |

## Specific Freesound pack shortlist (for asset-curator)

These Freesound user-pages have multiple usable CC0 SFX for our tone (asset-curator filters by **CC0 only** within each):

| Source | Why |
|---|---|
| **Kenney.nl Audio packs**: UI Audio, Interface Sounds, Impact Sounds, Sci-Fi Sounds (cartoon subset) | Highest signal — Kenney curates CC0 cartoon-friendly SFX |
| **Freesound user `LittleRobotSoundFactory`** | High-quality CC0 retro/cartoon SFX |
| **Freesound user `Sonic-Sentinel`** | CC0 ambient + impact library |
| **Freesound search "cartoon" + CC0 + duration ≤ 1 s** | Bulk source for round-robin variant pulls |
| **Freesound search "ukulele" / "pluck" / "bell" + CC0** | Pickup chime variants |
| **Freesound search "poof" + CC0** | Enemy-death variants |

## Voice acting

**NONE at launch.** No voice library required. Future-proof: AudioMixer reserves a Voice bus (see `04-mixer-routing.md`); when added, only CC0 / OFL-equivalent voice libraries will be considered (recorded in-house = CC0).

## Fonts (audio doc cross-ref — handled by `06-ui-visual-direction.md`)

| Font | License |
|---|---|
| Fredoka | SIL OFL |
| Nunito | SIL OFL |
| Baloo 2 | SIL OFL |

All via Google Fonts; no paid font ever.

## Attribution recording

For all **CC-BY** assets (incompetech tracks especially):

1. Record in `LICENSES.md` at repo root: artist, work title, license, source URL.
2. Add to in-game credits screen (route under Settings → Credits).
3. Include in any future PR description that adds CC-BY content.

## Vertical-slice subset source shortlist

For the ~7 BGM + ~25 SFX vertical slice (per `01-bgm-spec.md` + `02-sfx-spec.md`):

- **BGM (7)**: Home, Lobby, Run-Meadow, Boss-Meadow, Run-end win, Run-end lose, Cold-start splash → mix of Pixabay (4) + incompetech (2) + custom (1).
- **SFX (~50 files with RR)**: 80% from Kenney UI/Interface CC0 + ~10 Freesound CC0 pulls for specific hits.

## Hand-off

- Audio files land in `unity/Assets/Audio/{BGM,SFX}/<bucket>/<slug>.ogg`.
- Asset-curator owns sourcing per this list; (deferred) sound-designer owns Audacity edits + variant authoring.
- `LICENSES.md` updated atomically with any new CC-BY addition.
- **Open question for asset-curator**: confirm Freesound API can be used unauthenticated for bulk-discovery, or whether sourcing happens manually through the web UI (no paid API access).
