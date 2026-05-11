# Old Boar King — Boss Mechanics

> Owner: level-designer (mechanics + telegraphs); balance-engineer (TTK + damage tuning). Vertical-slice boss for the Meadow biome. Sister docs: `arena.md` (per-phase arena layout), `../../01-biomes/meadow/waves.json` (boss-event spawn at t=420), `../../00-pacing-model.md` (beat 8 boss-fight window), `../../../02-gdd/05-enemies.md` (boss role baseline: 4000 hp at min 5, 35 contact / 25 ranged / 50 AOE damage), `../../../02-gdd/06-biomes.md` (Meadow boss callout), `../../../02-gdd/narrative/00-tone-bible.md` (boss intro line voice).

All frame counts assume **60 fps** (24 frames = 0.4 s, 36 frames = 0.6 s, etc.). This is the single canonical frame-rate anchor for the whole boss spec.

## Concept

He is not a villain. He is a sleepy old boar who has been napping on the carrot patch for as long as anyone can remember, and the bunny has come to take their carrots back. He is grumpy. He is plodding. He wants his nap back, and he will charge any small fluffy creature standing between him and that nap. The fight reads like a child convincing a stubborn grandparent to move from a comfy chair — slow, sweepy, with bursts of "alright, enough!" rage.

Per tone bible: rascals are not malevolent, bosses are not menacing. Old Boar's defeat animation is a **deep sigh and a slow lie-down**, not a death throes. The carrots roll free from under him and the bunny scampers off with the basket.

## Phases

Three phases gated by HP thresholds. Each phase swaps the attack pattern catalog and modifies the arena (per `arena.md`).

| Phase | HP gate | Name | Mood / behavior |
|---|---|---|---|
| 1 | 100-66% | Awake-and-grumpy | Charges in straight lines, sweeps with tusks. Predictable. |
| 2 | 66-33% | Furrowed-brow | Adds hop attack; summons 4 minions twice across the phase. |
| 3 | 33-0% | Fully-cross | Adds stomp shockwave; rage charges at 2x speed. |

Phase transitions: HP gate hit → boss briefly invulnerable (1.0 s) → roar VFX + screen shake → arena props spawn for the new phase (per `arena.md`) → boss resumes with new attack pool.

## Attack pattern catalog

All telegraph windows are minimum **0.4 s (24 frames)** per `05-enemies.md` boss spec ("Every attack telegraphed with **0.8 s minimum window**" — Old Boar's tells start at 0.4 s for the simpler attacks in phase 1 and grow to 0.6-0.8 s in phase 3). All damage values are minute-5 baseline; balance-engineer tunes per `data/balance/scaling.json`.

### Phase 1 — Awake-and-grumpy

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Charge (linear)** | 30 (24 visual rumble + 6 prep stance) | 18 | 15 contact along path | Side-step early, hit flank during wind-down |
| **Sweep tusk (frontal arc)** | 18 | 12 | 10 contact in 90° front arc, 2 u reach | Step back during wind-up; re-engage during wind-down |

Cadence: alternates Charge / Sweep / Sweep / Charge with 1.5-2.0 s recovery between attacks.

### Phase 2 — Furrowed-brow

Inherits phase 1 attacks plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Hop attack (AOE landing)** | 24 (orange landing-circle decal) | 20 | 12 + 8 AOE inside 2 u landing radius | Run to landing zone edge during airtime |
| **Summon minions** | 30 (boss roots in place, ground rumble) | 60 (recovery animation; boss is *open*) | 0 | **Kill minions during boss recovery; massive damage window** |

Summon spawns 4 hop-slimes from the boss position in a 4-direction burst (N/E/S/W). Boss summons **twice** during phase 2, at the start of the phase and at ~50% HP through the phase.

Cadence: phase 1 mix + 1 hop attack every ~6 s + 2 scripted summons across the phase.

### Phase 3 — Fully-cross

Inherits phases 1 + 2 attacks plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Stomp shockwave (radial)** | 36 (red expanding-ring decal grows from boss outward) | 24 | 18 + 0.6 s slow on hit | Diagonal-dodge through one of the 2 gap windows in the ring (the ring has gaps; not a 360° wall) |
| **Rage charge (2x speed)** | 12 (very short tell — read this one early or eat it) | 30 | 18 contact | Tight side-step + counter-attack into the long wind-down |

Note on stomp shockwave: there is **no jump button** in Brave Bunny. The shockwave is **not a full ring** — it has 2 gaps positioned at 90° from boss-facing. Player reads the gap pattern from the red telegraph decal and diagonal-dodges through it.

Cadence: aggressive mix of all attacks; stomp every ~5 s, rage charge every ~7 s, with phase 1 attacks filling gaps.

## Arena (summary; expanded in `arena.md`)

- **Biome**: Meadow, 80 × 80 u (per `../../01-biomes/meadow/layout.md`).
- **Phase 1**: no hazards added. Boss occupies center; arena is the standard Meadow layout.
- **Phase 2**: 2 tree-stumps spawn as cover (props from Kenney Nature Kit). Player can use them to break boss line-of-sight.
- **Phase 3**: 4 ground-crack decals appear to telegraph the radial stomp pattern (they pulse 0.4 s before each stomp). Cosmetic-only between stomps.

## Telegraph color cues

Each attack has a unique color + shape language so the player can read incoming attacks at a glance during peak chaos.

| Attack | Telegraph color | Telegraph shape | Lead time |
|---|---|---|---|
| Sweep tusk | Yellow | Arc decal on ground (90° front-arc) | 0.3 s before attack |
| Charge (linear) | Red | Dashed line from boss in charge direction | 0.5 s before |
| Hop attack | Orange | Filled circle on ground at landing zone | 0.4 s before |
| Stomp shockwave | Red | Expanding-ring decal with 2 visible gaps | 0.6 s before |
| Rage charge | Bright red | Solid line + boss-edge red rim-light | 0.2 s before (intentionally short) |

Color rule per art-bible (no per-biome dramatic deviation): yellow = warn, red = damage, orange = AOE landing. Consistent across the launch boss roster.

## Intro line (tone-bible-correct, family-safe)

Per `02-gdd/narrative/00-tone-bible.md` sample copy: **"Old Boar's awake. Mind your tail."**

Plays as a 1.2 s text overlay during the boss entrance animation. Localized per the tone-bible TR translation: **"Koca Yaban uyandı. Kuyruğuna dikkat."**

## Win condition

Player reduces boss HP to 0 within **90 seconds** (target TTK from spawn to defeat). Balance-engineer to tune actual HP value against the typical minute-7 player build per `data/balance/scaling.json`.

On defeat:
1. Boss stops attacking; 1.5 s "deep sigh" animation.
2. Boss slowly lies down (1.0 s).
3. Carrot-burst VFX from where the boss was (3 Soul Shards + 1 guaranteed chest + 5 gold coins per `05-enemies.md` boss drop table; for the vertical slice "mid-boss-equivalent" budget = 3 Soul Shards).
4. Slow-mo to 0.3x for 1.2 s.
5. Outro beat begins (per pacing model beat 9): post-boss minor swarm, then run-end tally activates.

## Loss condition

Player HP reaches 0 → revive offer modal per `01-core-loop.md` (rewarded ad, once per run). Decline → run-end tally with banked currencies.

## Telegraph audio

Per `08-audio-bible/` (when authored; level-designer specs the cue, art-director (audio sub-role) owns the asset):

| Attack | Audio cue | Timing |
|---|---|---|
| Charge | Low rumble, growing | Starts 0.3 s before charge launch |
| Sweep | Wood-creak + whoosh | Starts at telegraph onset |
| Hop attack | Whoosh + cymbal swell | During airtime (boss in air) |
| Stomp shockwave | Low thud + ring-expansion swoosh | At impact (matched to ring expansion) |
| Rage charge | Sharp snort + drum-hit | At telegraph onset (very short cue, matches short tell) |
| Defeat | Gentle sigh + slow exhale | At HP-0 trigger; transitions into tally screen ambient |

No vocal lines (boar is non-verbal). All cues are SFX layers, no voice.

## Cross-references

- Arena layout + phase prop staging: `arena.md` (sibling file).
- Boss spawn in waves: `../../01-biomes/meadow/waves.json` at `t=420`.
- Boss role baseline (HP, damage): `../../../02-gdd/05-enemies.md` boss row.
- Boss roster context: `../../../02-gdd/06-biomes.md` Meadow section.
- Tone bible intro-line voice: `../../../02-gdd/narrative/00-tone-bible.md`.
- Balance-engineer tuning lands in: `data/balance/bosses.json` (TBA).
- Hitstop spec for boss kill: `../../../02-gdd/11-feel-pillars.md` pillar 4.
