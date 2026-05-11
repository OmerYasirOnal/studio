# Crab Captain — Boss Mechanics

> Owner: level-designer (mechanics + telegraphs); balance-engineer (TTK + damage tuning). Beach biome boss (biome 2). Sister docs: `../../01-biomes/beach/layout.md` (arena), `../../01-biomes/beach/waves.json` (boss-event spawn at t=420), `../../00-pacing-model.md` (beat 8 boss-fight window), `../../../02-gdd/05-enemies.md` (boss role baseline: 4000 hp at min 5, 35 contact / 25 ranged / 50 AOE damage), `../../../02-gdd/07-bosses.md` (Crab Captain concept), `../../../02-gdd/narrative/04-boss-intros.md` (intro line).

All frame counts assume **60 fps** (24 frames = 0.4 s, 48 frames = 0.8 s, etc.). 60 fps is the single canonical anchor for the whole boss spec.

## Concept

The Crab Captain has guarded this stretch of shoreline since long before the bunny was born. He wears a bottle-cap as a hat and is **territorial about his sand**, particularly the patch where his sand-puff children play. When the bunny gets too close, he sweeps a giant pincer to shoo. He summons sand-puffs because he genuinely believes they can help, even though they are tiny and confused. Per tone bible: he is grumpy, not menacing; the bottle-cap tips comically forward when he sits down at defeat.

## Phases

| Phase | HP gate | Name | Mood / behavior |
|---|---|---|---|
| 1 | 100-66% | Patrolling | Side-scuttling lateral pacing; pincer sweeps + bubble lobs |
| 2 | 66-33% | Summoning | Adds sand-puff summon (boss roots, big DPS window); triple bubble-spit |
| 3 | 33-0% | Cornered | Breaks the lateral rule with forward dashes; pincer becomes AOE slam |

Phase transitions: HP gate hit → boss briefly invulnerable (1.0 s) → bottle-cap-tip animation + screen shake → arena props update per `../../01-biomes/beach/layout.md` boss-arena delta → boss resumes with new attack pool.

## Attack pattern catalog

All telegraph windows ≥ 0.6 s (well above the 0.4 s boss minimum). Damage values are minute-7 baseline; balance-engineer tunes per `data/balance/bosses.json`.

### Phase 1 — Patrolling

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Pincer-sweep** (90° front-cone with the oversized claw) | 54 (pincer rears + shadow lengthens on sand) | 30 | 40 contact inside cone, 2.5 u reach | Step around to the small-pincer side during 0.5 s wind-down |
| **Bubble-spit** (single bubble lobs in 0.6 s arc, predictive aim) | 36 (captain inhales, throat puffs out) | 18 | 18 + 1.0 u AOE on impact | 0.3 s between spits; side-step laterally |
| **Skitter-strafe** (rapid lateral move; not an attack) | n/a | n/a | 0 | Reposition window for the bunny too |

Cadence: Sweep / Bubble / Skitter / Bubble / Sweep with 1.5-2.0 s recovery between attacks.

### Phase 2 — Summoning

Inherits phase 1 attacks plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Sand-puff summon** (3 sand-puff swarmers spawn at boss feet) | 48 (captain rears, stomps both back legs) | 60 — boss is **rooted and exposed** | 0 from boss; sand-puffs use swarmer contact (5 hp/touch) | **1.0 s wind-down** — biggest DPS window of fight. Kill sand-puffs during the boss recovery to clear adds while burning HP |
| **Bubble-spit (triple)** (upgrade — 3 bubbles in 45° fan, 0.4 s apart) | 36 | 24 | 18 per bubble | Dodge laterally; gap between volleys is the opening |
| **Pincer-sweep** | unchanged | unchanged | unchanged | unchanged |

Cadence: Phase 1 mix + Summon every ~18 s (2 summons total during phase 2) + Triple-spit replaces single-spit.

### Phase 3 — Cornered

Inherits phases 1+2 attacks plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Pincer-slam** (sweep upgrades to AOE — pincer slams ground, 2.0 u radial pulse) | 60 (pincer overhead, shadow grows) | 36 — pincer is stuck in the sand | 50 AOE 2.0 u | 0.6 s **guaranteed-hit** window while pincer is stuck |
| **Forward dash** (breaks lateral rule — 6 u forward charge over 0.8 s, leaves dust trail) | 48 (wide leg-spread + sand-spray) | 24 — wall-skid stagger | 50 contact | 0.4 s post-dash; if dashed into the arena soft-boundary, +1.0 s self-stagger (designer-rewarded clever-play opening) |
| **Sand-puff summon** | unchanged | unchanged | unchanged | unchanged |

Cadence: Pincer-slam every ~5 s, Forward-dash every ~8 s, Summon every ~12 s, Bubble fills gaps.

## Arena (summary)

- **Biome**: Beach, 80 × 80 u (per `../../01-biomes/beach/layout.md`).
- **Phase 1**: 2 always-active sand-trap patches at fixed positions (+8, +5) and (-8, -6). Boss immune to his own sand; bunny is not.
- **Phase 2**: same as phase 1 + sand-puff summons spawn at boss feet (anchored to a summon-marker decal that pulses during the wind-up telegraph).
- **Phase 3**: same as phase 2 + 2 pincer-slam-marker decals + 1 dash-edge marker that hints at the new dash mechanic.

## Telegraph color cues

Consistent with Old Boar King's palette per `../old-boar-king/mechanics.md` §Telegraph color cues: yellow = warn, red = damage, orange = AOE landing.

| Attack | Telegraph color | Telegraph shape | Lead time |
|---|---|---|---|
| Pincer-sweep | Yellow | 90° arc decal on sand | 0.5 s before attack |
| Bubble-spit | Yellow | Small predictive ring at projected landing | 0.4 s before |
| Bubble-spit triple | Yellow | 3 small predictive rings in fan | 0.4 s before |
| Sand-puff summon | Orange | Pulsing circle decal at boss feet | 0.6 s before |
| Pincer-slam | Orange | Filled circle decal (impact zone) | 0.6 s before |
| Forward dash | Red | Dashed straight line from boss in dash direction | 0.6 s before |

## Intro line (per `narrative/04-boss-intros.md`)

**Intro card (≤ 12 words):** "Crab Captain's on shore patrol. Watch the pincer."
**TR seed:** "Yengeç Kaptan kıyıda nöbette. Kıskaca dikkat."

Plays as a 1.2 s text overlay during the boss entrance animation (90% screen-width banner, fades after 1.5 s total). Localizer key: `BOSS_INTRO_CRAB` per `narrative/05-localization-keys.md`.

## Win condition

Player reduces boss HP to 0 within **90 seconds** target TTK from spawn. Balance-engineer tunes actual HP value against the minute-7 build per `data/balance/bosses.json`. Approx 5500 hp split 40/35/25 across phases per `07-bosses.md`.

On defeat:
1. Boss stops attacking; 1.5 s "harrumph" animation — bottle-cap tips forward.
2. Boss sits down slowly (1.0 s); pincer drops to sand with a soft *thud*.
3. Carrot-burst VFX from where the boss sat (3 Soul Shards per mid-boss budget + 1 guaranteed chest + 5 gold coins per `05-enemies.md` boss drop table; one bonus Carrot pickup pops out from beneath him).
4. Slow-mo to 0.3x for 1.2 s.
5. Outro beat begins (per pacing model beat 9).

## Loss condition

Player HP reaches 0 → revive offer modal per `01-core-loop.md` (rewarded ad, once per run). Decline → run-end tally with banked currencies.

## Telegraph audio

Per `08-audio-bible/` (when authored; level-designer specs the cue, art-director audio sub-role owns asset):

| Attack | Audio cue | Timing |
|---|---|---|
| Pincer-sweep | Wood-creak + sand-shuffle | At telegraph onset |
| Bubble-spit | Small inhale + bubble-pop launch | During inhale telegraph |
| Sand-puff summon | Captain stomp + small whoosh | At stomp wind-up |
| Pincer-slam | Heavy creak + sand-impact | At impact (matched to slam landing) |
| Forward dash | Sand-skitter + low whoosh | At telegraph onset (short cue) |
| Defeat | "Harrumph" + bottle-cap tip clack | At HP-0 trigger |

No vocal lines (the captain is non-verbal; harrumphs and clacks only). All cues are SFX layers.

## Notes on tone-bible adherence

- No menace: bottle-cap is silly, not military.
- Sand-puffs are clearly his kids — they look up at him when summoned (animation note for art-director).
- No fangs visible; pincers are rounded and toy-like.
- The harrumph at phase transitions is **affronted, not aggressive** — same register as the boar's snore.
- Forward dash in phase 3 reads as "alright that's it, I'm losing my temper" — comedic, not threatening.

## Cross-references

- Arena layout + phase prop staging: `../../01-biomes/beach/layout.md` §Boss-arena delta.
- Boss spawn in waves: `../../01-biomes/beach/waves.json` at `t=420`.
- Boss role baseline (HP, damage): `../../../02-gdd/05-enemies.md` boss row.
- Boss concept narrative: `../../../02-gdd/07-bosses.md` §2.
- Tone bible intro-line voice: `../../../02-gdd/narrative/00-tone-bible.md`.
- Balance-engineer tuning lands in: `data/balance/bosses.json`.
- Hitstop spec for boss kill: `../../../02-gdd/11-feel-pillars.md` pillar 4.
