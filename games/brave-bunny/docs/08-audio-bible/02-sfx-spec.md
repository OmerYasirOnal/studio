# SFX Spec — Brave Bunny

> Owner: art-director (audio sub-role). Cross-refs: `00-audio-overview.md` (mix headroom + voice cap), `04-mixer-routing.md` (per-SFX bus assignment), `docs/02-gdd/11-feel-pillars.md` (Pillars 1, 3, 4, 5, 6 specify exact dB and timing), `docs/02-gdd/04-weapons.md` (per-weapon flavor descriptions). The **~50-SFX launch catalog** with character notes, durations, dB targets, and round-robin policy. Actual `.ogg` files come from `03-source-shortlist.md`.

## Round-robin policy

Every SFX that fires more than once per **3 seconds** of gameplay has **≥ 3 variants**. Audio engine picks pseudo-random non-repeat. Hard rule per Pillar 1 violation flag (no enemy dies silently — also no enemy dies with the *exact same* sound 3× in a row).

| Frequency | Required variants |
|---|---|
| > 1 / second sustained | 5 variants |
| 1 / 1-3 s | 3 variants |
| < 1 / 3 s | 1 variant acceptable |

## SFX catalog

| Slug | Where | Character | Duration | Target dB | Variants |
|---|---|---|---|---|---|
| `ui_button_press` | UI | soft pop | 80 ms | −6 dBFS | 1 |
| `ui_button_back` | UI | reverse pop | 80 ms | −8 dBFS | 1 |
| `ui_modal_open` | UI | swoosh up | 200 ms | −10 dBFS | 1 (low-pass to 8 kHz) |
| `ui_modal_close` | UI | swoosh down | 150 ms | −10 dBFS | 1 |
| `ui_tab_switch` | UI | tap-tick | 50 ms | −12 dBFS | 1 |
| `ui_tap_tick` | UI | soft pointer-up tick | 60 ms | −12 dBFS | 1 (Pillar 5) |
| `ui_locked_shake` | UI | dull thunk + soft chord | 180 ms | −9 dBFS | 1 |
| `ui_toast_in` | UI | soft sweep | 200 ms | −12 dBFS | 1 |
| `ui_purchase_confirm` | UI | bright bell | 400 ms | −6 dBFS | 1 |
| `ui_streak_claim` | UI | warm sparkle | 600 ms | −6 dBFS | 1 |
| `ui_achievement_pop` | UI | golden chime | 800 ms | −6 dBFS | 1 |
| `ui_store_browse` | UI | soft tick | 60 ms | −12 dBFS | 1 |
| `ui_iap_confirm` | UI | clear positive bell | 400 ms | −6 dBFS | 1 |
| `run_start` | Gameplay | flute riff (brand cue) | 600 ms | −3 dBFS | 1 |
| `run_levelup` | Gameplay | 4-note arpeggio (Pillar 2) | 800 ms (fanfare body 600 ms) | −3 dBFS | 1 |
| `run_pickup_xp_small` | Gameplay | tiny chime, +200 cents pitch | 60 ms | −3 dBFS | 4 (Pillar 3) |
| `run_pickup_xp_large` | Gameplay | chime, −200 cents pitch | 80 ms | −3 dBFS | 4 |
| `run_pickup_gold` | Gameplay | softer chime, distinct from xp | 80 ms | −6 dBFS | 3 |
| `run_pickup_heart` | Gameplay | warm pip | 120 ms | −3 dBFS | 2 |
| `weapon_carrot_fire` | Combat | woody thud + "wahoo" | 80 ms | −6 dBFS | 3 |
| `weapon_carrot_return` | Combat | softer woody whoosh | 60 ms | −9 dBFS | 3 |
| `weapon_sunbeam_loop` | Combat | warm hum (continuous) | loop | −10 dBFS | 1 |
| `weapon_sunbeam_start` | Combat | sparkle-rise | 200 ms | −9 dBFS | 1 |
| `weapon_daisy_drop` | Combat | soft thump + flower-rustle | 120 ms | −9 dBFS | 3 |
| `weapon_daisy_explode` | Combat | popcorn-pop + petal sweep | 200 ms | −6 dBFS | 3 |
| `enemy_swarmer_hit` | Combat | wet thump | 70 ms | −6 dBFS | 5 (Pillar 4) |
| `enemy_swarmer_die` | Combat | poof / air-puff | 120 ms | −9 dBFS | 3 (Pillar 1 trash death) |
| `enemy_elite_hit` | Combat | heavier thump (hitstop coincident) | 100 ms | −6 dBFS | 3 |
| `enemy_elite_die` | Combat | bigger poof + faint chime | 200 ms | −6 dBFS | 3 (Pillar 1 elite death) |
| `enemy_boss_hit` | Combat | resonant hit with subtle echo | 200 ms | −6 dBFS | 3 |
| `enemy_boss_die` | Combat | drum hit + fanfare cap | 600 ms | −3 dBFS | 1 (Pillar 1 boss death) |
| `boss_intro_sting` | Combat | reveal swell | 600 ms | −3 dBFS | 1 (mood shift) |
| `boss_phase_change` | Combat | drum hit + chime | 400 ms | −3 dBFS | 1 (tonal emphasis) |
| `boss_telegraph_warn` | Combat | warning chord (danger-red telegraph) | 300 ms | −6 dBFS | 1 |
| `hero_hit` | Combat | cute "ouch" pip | 150 ms | −6 dBFS | 3 |
| `hero_death` | Combat | dignified descending pluck (Pillar 6) | 800 ms | −6 dBFS | 1 |
| `hero_levelup_fanfare` | Gameplay | layered with `run_levelup` | 600 ms | −3 dBFS | 1 |
| `hero_dash` | Combat | air whoosh | 150 ms | −9 dBFS | 3 |
| `hero_heal` | Combat | sparkle-up | 400 ms | −9 dBFS | 2 |
| `run_end_win` | Endgame | full fanfare | 1500 ms | −3 dBFS | 1 |
| `run_end_lose` | Endgame | gentle fade (Pillar 6) | 1200 ms | −6 dBFS | 1 |
| `tally_count_tick` | Endgame | tick-tick during tally count | 30 ms (per tick) | −9 dBFS | 1 |
| `tally_slam` | Endgame | slam each tally line lands | 250 ms | −9 dBFS | 1 (Pillar 6) |
| `revive_offer_in` | Endgame | hopeful chime | 600 ms | −6 dBFS | 1 |
| `unlock_character` | Meta | character-specific signature + golden chime | 1200 ms | −3 dBFS | 1 |
| `unlock_weapon` | Meta | weapon-specific cue + chime | 800 ms | −3 dBFS | 1 |
| `pass_tier_up` | Meta | rising 3-note + sparkle | 600 ms | −3 dBFS | 1 |
| `daily_streak_chime` | Meta | warm sparkle (escalating per day) | 600 ms | −6 dBFS | 1 |
| `ambient_meadow_bed` | Environment | crickets + light breeze + birdsong (loop) | loop | −18 dBFS | 1 (looping) |
| `ambient_beach_bed` | Environment | waves + gulls (loop) | loop | −18 dBFS | 1 |
| `ambient_forest_bed` | Environment | wind in leaves + owl hoot (loop) | loop | −18 dBFS | 1 |
| `ambient_cavern_bed` | Environment | drip + low hum (loop) | loop | −18 dBFS | 1 |
| `ambient_snow_bed` | Environment | wind + distant ice crack (loop) | loop | −18 dBFS | 1 |

## Counts

| Bucket | SFX slugs | With round-robin variants |
|---|---|---|
| UI | 13 | 13 files (mostly 1 variant each) |
| Gameplay (non-combat) | 6 | ~14 files (xp + gold + heart have RR) |
| Combat (weapons + enemies + hero) | 19 | ~50 files (heavy RR) |
| Endgame | 6 | 6 files |
| Meta | 4 | 4 files |
| Environment (ambient beds) | 5 | 5 files |
| **Total slugs** | **~53** | **~92 files** |

Storage: ~92 files × ~30 KB avg = ~2.8 MB. With BGM (7.2 MB) the audio bundle stays ~10 MB on-disk — within the 12 MB `08-asset-budget.md` allocation.

## Round-robin enforcement

The sound designer authors RR variants by **micro-pitch + envelope** variation on the same source sample. The Unity audio system picks `(lastIndex + random(1..N-1)) % N` so the same variant never plays twice in a row.

## Vertical slice subset (~25 SFX)

| Bucket | Vertical-slice ships |
|---|---|
| UI | `ui_button_press`, `ui_button_back`, `ui_modal_open`, `ui_modal_close`, `ui_tap_tick`, `ui_locked_shake` (6) |
| Gameplay | `run_start`, `run_levelup`, `run_pickup_xp_small`, `run_pickup_xp_large`, `run_pickup_gold`, `run_pickup_heart` (6) |
| Combat | `weapon_carrot_fire`, `weapon_carrot_return`, `weapon_sunbeam_loop`, `weapon_sunbeam_start`, `weapon_daisy_drop`, `weapon_daisy_explode`, `enemy_swarmer_hit`, `enemy_swarmer_die`, `enemy_elite_hit`, `enemy_elite_die`, `enemy_boss_hit`, `enemy_boss_die`, `boss_intro_sting`, `boss_phase_change`, `hero_hit` (15) |
| Endgame | `run_end_win`, `run_end_lose`, `tally_count_tick`, `tally_slam` (4) |
| Environment | `ambient_meadow_bed` (1) |
| **Vertical slice total** | **~32 slugs (~50 files with RR)** |

This exceeds the "~25" sketch in the brief because pillar 1 + 4 + 6 acceptance criteria require minimum SFX density — RR variants are non-optional for ship quality.

## Hand-off

- SFX files land in `unity/Assets/Audio/SFX/<bucket>/<slug>_<variant>.ogg`.
- Mixer bus assignment per slug specified in `04-mixer-routing.md`.
- Asset-curator + sound-designer (deferred) own sourcing per `03-source-shortlist.md`.
- **Open question for ui-engineer**: confirm whether Unity UI Toolkit emits pointer-up events with the < 16 ms latency required for `ui_tap_tick` to land inside Pillar 5's window.
