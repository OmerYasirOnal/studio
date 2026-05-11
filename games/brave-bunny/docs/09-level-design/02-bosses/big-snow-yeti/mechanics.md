# Big Snow-yeti — Boss Mechanics

> Owner: level-designer (mechanics + telegraphs); balance-engineer (TTK + damage tuning). Snow biome boss (biome 5 — **launch endgame boss**). Sister docs: `../../01-biomes/snow/layout.md` (arena), `../../01-biomes/snow/waves.json` (boss-event spawn at t=420), `../../00-pacing-model.md` (beat 8 boss-fight window), `../../../02-gdd/05-enemies.md` (boss role baseline), `../../../02-gdd/07-bosses.md` (Big Snow-yeti concept), `../../../02-gdd/narrative/04-boss-intros.md` (intro line).

All frame counts assume **60 fps**. 60 fps is the single canonical anchor for the whole boss spec.

## Concept

The Big Snow-yeti is enormous, fluffy, and **profoundly cold**. He is so cold that the air around him is colder; he is so cold that he does not remember being warm. He does not hate the bunny — he simply does not believe the bunny will survive his neighborhood, and is sad about that in advance. He stomps not in anger but in **shivers**. His cold-aura is involuntary. Per tone bible: tall, broad, fluffy white silhouette with a small dark Charlie-Brown-style face high in the chest area; oversized soft paws (no claws); his breath is visible (small puffs of pale cyan steam); defeat is a cross-eyed snowflake-on-nose, a slow contented exhale, and a curl-up nap.

**Key fight-shape note**: Big Snow-yeti is the **highest-HP boss** (~7500 hp per `07-bosses.md`), the **largest arena** (16×16 u in `07-bosses.md`, the 90×90 Snow arena bounding it per `../../01-biomes/snow/layout.md`), and the **only fight where ambient + boss hazards stack fully**. He has a **passive cold-aura** that is always on — the first persistent passive in the boss roster.

## Phases

| Phase | HP gate | Name | Mood / behavior |
|---|---|---|---|
| 1 | 100-66% | Plodding-shiver | Ice-stomp + snowball-toss + passive cold-aura (4.0 u) |
| 2 | 66-33% | Frost-charging | Adds snowball-volley (3-shot fan) + frost-charge (5 u dash); cold-aura expands to 5.0 u |
| 3 | 33-0% | Blizzard-roused | Ice-stomp upgrades to ice-stomp-quake (6.0 u + 4 ice-patches); snowball-volley upgrades to 5-shot; adds blizzard-howl (arena-wide DOT amp) |

Phase transitions: HP gate hit → boss briefly invulnerable (1.0 s) → yeti shivers visibly, snow-puff bursts from his fur, low rumble SFX → cold-aura radius and snowball/stomp scaling update → boss resumes.

## Attack pattern catalog

All telegraph windows ≥ 0.6 s (well above the 0.8 s boss minimum — the yeti is slow and readable; difficulty is in *managing* the cold-aura + ice-slides + multiple simultaneous threats, not in dodging fast attacks).

### Phase 1 — Plodding-shiver

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Ice-stomp** (yeti stomps; 3.0 u shockwave radiates over 0.6 s; ice patches form along the wave path, persist 6 s) | 48 (high lift of paw, frost-puff under sole) | 30 — paw-down recovery | 40 inside expanding wave; ice patches enforce 0.4 s drift | 0.5 s recovery — DPS during, mind new ice patches |
| **Snowball-toss** (yeti lobs large snowball in 1.0 s arc at player's predicted position) | 36 (arm wind-up) | 24 — arm reset | 28 + 1.0 s slow (movespeed × 0.6) | 0.4 s arm-reset |
| **Cold-aura** (passive — 1.5 hp/sec DOT inside 4.0 u of boss, always-on, no telegraph) | n/a (passive) | n/a | 1.5 hp/sec | Maintain distance; **double the Snow biome cold-tick** |

Cadence: Ice-stomp every ~5 s, Snowball-toss every ~3 s, with 1.5 s recovery between attacks. Cold-aura is constant.

### Phase 2 — Frost-charging

Inherits phase 1 attacks (modified) plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Ice-stomp** | unchanged | unchanged | shockwave upgrades to 4.0 u | Tighter dodge window |
| **Snowball-volley** (3 snowballs in fan, 0.5 s apart) | 48 (wind-up) | 30 — reset | 28 each | Dodge laterally between volleys |
| **Frost-charge** (yeti drops to all fours and charges 5 u forward over 1.0 s; trails ice patches behind) | 60 (squat-down + frost-puff burst) | 36 — slide-stop | 50 contact | 0.6 s slide-stop — guaranteed punish if positioned right |
| **Cold-aura** | expanded to 5.0 u | n/a | 2.0 hp/sec | Wider safe-distance requirement |

Cadence: Ice-stomp every ~6 s, Snowball-volley every ~5 s, Frost-charge every ~10 s.

### Phase 3 — Blizzard-roused

Inherits phases 1+2 attacks (modified) plus the set-piece:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Ice-stomp-quake** (upgraded ice-stomp — 6.0 u shockwave + 4 secondary ice patches scatter at preset positions) | 60 (both paws lift simultaneously) | 42 — heavy recovery | 50 AOE | **0.7 s recovery — biggest DPS window of the fight** |
| **Snowball-volley (5-shot)** | 48 | 30 | 28 each | Reposition during volley |
| **Frost-charge** | unchanged | unchanged | unchanged | unchanged |
| **Blizzard-howl** (yeti howls; for 4.0 s the entire arena cold-tick rate doubles to 4.0 hp/sec, suppressed only at igloo prop) | 48 (head-tilt-back + intake breath) | 60 — howl sustains | n/a direct (ambient DOT spike) | Retreat to igloo during howl; full 1.0 s window after howl ends |
| **Cold-aura** | unchanged from phase 2 (5.0 u, 2.0 hp/sec) | n/a | n/a | n/a |

Cadence: Ice-stomp-quake every ~7 s, Snowball-volley-5shot every ~6 s, Frost-charge every ~8 s, Blizzard-howl every ~20 s (set-piece moment).

## Arena (summary)

- **Biome**: Snow, 90 × 90 u (per `../../01-biomes/snow/layout.md` — largest arena in the game).
- **Igloo prop** at (-14, -16) — **critical for player survival during blizzard-howl** (4 s cold-tick suppression on exit per `06-biomes.md`).
- **Pine props** at (+18, +16) and corner — 2 s cold-tick suppression auras.
- **Ice formation** at (-18, +12) — decorative only.
- **Frozen pond** at (+12, -10) — permanent 0.4 s drift.
- **Phase 1**: 2 fixed snowdrift patches at (+10, +6) and (-8, +10); ambient ice-slides persist.
- **Phase 2**: yeti adds 1-2 ice patches per ice-stomp (via the trailing patch mechanic); frost-charge adds 3 more along its path.
- **Phase 3**: ice-stomp-quake adds 4 fixed ice patches per stomp; blizzard-howl amplifies cold-tick to 4.0 hp/sec arena-wide except within 3.0 u of igloo/pine.

**This is the only boss fight where all 3 ambient hazard classes are active simultaneously** (ice-slides + cold-tick + snowdrift) AND the boss adds its own ice patches + cold-aura amplification. Balance-engineer must validate TTK under maximum hazard pressure per `data/balance/biomes.json` cross-check.

## Telegraph color cues

Consistent palette per `../old-boar-king/mechanics.md` §Telegraph color cues.

| Attack | Telegraph color | Telegraph shape | Lead time |
|---|---|---|---|
| Ice-stomp | Orange | Expanding-ring decal (cyan-tinted) | 0.6 s before attack |
| Snowball-toss | Yellow | Predictive ring at projected landing | 0.4 s before |
| Snowball-volley | Yellow | 3 (or 5) predictive rings in fan | 0.5 s before |
| Frost-charge | Red | Dashed straight line from yeti, cyan-tinted | 0.8 s before |
| Cold-aura | Pale cyan (passive indicator) | Faint pulsing ring around yeti at aura radius | always-on (no lead time — it's passive) |
| Ice-stomp-quake | Red | Larger expanding-ring decal + 4 small predictive rings at secondary patch positions | 0.8 s before |
| Blizzard-howl | Pale cyan | Screen-edge frost-vignette fade-in over 0.8 s; cold-tick indicator amplifies | 0.8 s before howl starts |

The yeti's palette skews toward **red on his most dangerous attacks** (frost-charge contact, ice-stomp-quake AOE) — fitting for the endgame boss. The pale-cyan passive ring for cold-aura is a new color cue, intentionally distinct from the yellow/red/orange warn/damage/AOE-land triad — it reads as "atmosphere," not "incoming attack."

## Intro line (per `narrative/04-boss-intros.md`)

**Intro card (≤ 12 words):** "Big Snow-yeti's grumpy. Keep your paws warm."
**TR seed:** "Koca Kar-yetisi huysuz. Patilerin sıcak kalsın."

Plays as a 1.2 s text overlay during the boss entrance animation. Localizer key: `BOSS_INTRO_YETI` per `narrative/05-localization-keys.md`.

The intro line primes the player on **prop-aware pathing** ("keep your paws warm" = stay near igloo/pines). Critical for surviving phase 3.

## Win condition

Player reduces boss HP to 0 within **150 seconds** target TTK (longest of the launch roster — endgame boss). Approx 7500 hp split 40/30/30 across phases per `07-bosses.md`. Balance-engineer tunes per `data/balance/bosses.json`.

On defeat:
1. Yeti stops attacking; cold-aura fades (1.0 s — visible frost-ring shrinks to nothing).
2. He sits down in the snow (1.5 s); his shivers slow.
3. A tiny snowflake VFX lands on his nose; he goes cross-eyed looking at it (0.8 s).
4. Slow contented exhale (visible pale-cyan steam puff) → curls up in the snow (1.2 s).
5. Carrot-burst VFX from his last position (**5 Soul Shards** — end-boss budget per `05-enemies.md`; 1 guaranteed chest + 5 coins + **1 character-shard pull** per `07-bosses.md` end-boss drop table).
6. Slow-mo to 0.3x for 1.5 s (slightly longer than other bosses — endgame moment).
7. Bunny pats his paw before scampering off (animation note for art-director).
8. Outro beat begins (per pacing model beat 9). **Run-end tally is the credits-roll moment for the launch arc.**

He is **going for a nap**, not dying. This is the most emotional defeat in the launch roster per tone bible.

## Loss condition

Player HP reaches 0 → revive offer modal per `01-core-loop.md`. Decline → run-end tally with banked currencies. Because the yeti is the launch endgame boss, a Snow-failure run still grants progression (banked Soul Shards) — the player isn't punished for not clearing the hardest content first try.

## Telegraph audio

| Attack | Audio cue | Timing |
|---|---|---|
| Ice-stomp | Heavy paw-lift creak → soft *thump-crunch* | At telegraph onset + impact |
| Snowball-toss | Snow-pack squish + arm-whoosh | At wind-up |
| Snowball-volley | 3 (or 5) sequential snow-packs in tempo | At wind-up |
| Frost-charge | Deep growl-shiver + ice-creak | During squat-down telegraph |
| Cold-aura | Continuous low pale-cyan ambient hum, fades with distance | always-on (intensity scales with proximity to yeti) |
| Ice-stomp-quake | Heavy double-paw creak + ice-crack pulse | At impact |
| Blizzard-howl | Long low howl with wind-rise underlay | 0.8 s wind-up + 4 s sustained |
| Defeat | Soft contented exhale + curling-up rustle | At HP-0 trigger |

No vocal lines (the yeti is non-verbal; growls, shivers, and the howl are SFX). The howl is the **most expressive SFX in the launch roster** — it's the sound of cold-as-emotion, not cold-as-aggression.

## Notes on tone-bible adherence

- Small dark Charlie-Brown face — no glowing eyes, no fangs visible.
- Oversized **soft** paws (no claws). Frost-charge is "yeti drops to all fours" — endearing-clumsy, not predatory.
- Cold-aura is involuntary — the yeti is sad about it. Animation note: he occasionally looks at his own paws apologetically.
- Howl is melancholic, not threatening (per `07-bosses.md`: "the largest, slowest, gentlest tragedy in the game").
- Defeat is a **nap, not a death** — the cross-eyed snowflake-on-nose moment is the launch's emotional peak per tone bible.
- The bunny pats the yeti's paw before leaving — explicit in `07-bosses.md` cartoon-flavor section.

## Cross-references

- Arena layout + phase prop staging: `../../01-biomes/snow/layout.md` §Boss-arena delta.
- Boss spawn in waves: `../../01-biomes/snow/waves.json` at `t=420`.
- Boss role baseline (HP, damage): `../../../02-gdd/05-enemies.md` boss row.
- Boss concept narrative: `../../../02-gdd/07-bosses.md` §5.
- Tone bible intro-line voice: `../../../02-gdd/narrative/00-tone-bible.md`.
- Balance-engineer tuning lands in: `data/balance/bosses.json`.
- Hitstop spec for boss kill: `../../../02-gdd/11-feel-pillars.md` pillar 4.
- End-boss drop table (5 Soul Shards + character-shard pull): `data/balance/drops.json` + `02-meta-loop.md`.
