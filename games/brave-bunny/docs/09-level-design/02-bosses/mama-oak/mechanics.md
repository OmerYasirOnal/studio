# Mama Oak — Boss Mechanics

> Owner: level-designer (mechanics + telegraphs); balance-engineer (TTK + damage tuning). Forest biome boss (biome 3). Sister docs: `../../01-biomes/forest/layout.md` (arena), `../../01-biomes/forest/waves.json` (boss-event spawn at t=420), `../../00-pacing-model.md` (beat 8 boss-fight window), `../../../02-gdd/05-enemies.md` (boss role baseline), `../../../02-gdd/07-bosses.md` (Mama Oak concept), `../../../02-gdd/narrative/04-boss-intros.md` (intro line).

All frame counts assume **60 fps**. 60 fps is the single canonical anchor for the whole boss spec.

## Concept

Mama Oak is the oldest tree in the forest and the gentlest soul on the path. She has been growing acorns for two hundred seasons and does not appreciate strangers near her seedlings. When the bunny wanders into her glade, she politely requests they leave by gently snaring their feet with roots. When the bunny does not leave, she politely requests again with more roots. By phase 3 she is dropping acorns at speed, which is the most aggressive thing she knows how to do. Per tone bible: kindly wrinkled-bark face, warm amber face-knot eyes (not glowing red), smooth root vines (not gnarled or thorny).

**Key fight-shape note**: Mama Oak is the **only stationary boss in the roster**. She does not move — the arena rotates around her. This is a deliberate variety beat: the player has been kiting bosses in straight lines (Boar, Crab); Mama Oak forces a circling pattern with no kite-the-boss option.

## Phases

| Phase | HP gate | Name | Mood / behavior |
|---|---|---|---|
| 1 | 100-66% | Politely-rooting | Root-spike + acorn-toss + leaf-flurry. Stationary, swiveling face. |
| 2 | 66-33% | Firmly-rooting | Adds root-snare patches (3 persistent); upgrades acorn-toss to double-toss |
| 3 | 33-0% | Done-being-polite | Adds acorn-rain (the big set-piece); root-snare patches up to 5; root-spike becomes triple; leaf-flurry removed |

Phase transitions: HP gate hit → boss briefly invulnerable (1.0 s) → canopy-shake VFX + leaf-cascade decal + low creak SFX → root-snare patch positions update per `arena_mods` → boss resumes.

## Attack pattern catalog

All telegraph windows ≥ 0.5 s. Damage values are minute-7 baseline; balance-engineer tunes per `data/balance/bosses.json`. Mama Oak compensates for being stationary with sustained-pressure attacks (multiple simultaneous threat layers).

### Phase 1 — Politely-rooting

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Root-spike** (single root erupts at player's last position) | 42 (ground-crack VFX at target spot, brown decal pulses) | 18 — root retracts visibly | 30 inside 1.0 u | Dodge the marker, hit the trunk during retraction |
| **Acorn-toss** (lobs a single oversized acorn in 0.8 s arc, predictive aim) | 36 (canopy branch winds back) | 24 — branch resets | 22 + 0.8 u AOE on impact | 0.4 s between tosses |
| **Leaf-flurry** (1.5 u cone of leaves blows outward, gentle pushback) | 30 (canopy rustles) | 30 — settle | 10 + pushback 2 u | 0.5 s settle — push the bunny away; reposition timer |

Cadence: Root-spike / Acorn-toss / Acorn-toss / Leaf-flurry / Root-spike with 1.0-1.5 s recovery between attacks. Faster rotation than other bosses because Mama Oak can't reposition.

### Phase 2 — Firmly-rooting

Inherits phase 1 attacks (modified) plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Root-snare patch** (3 stationary root-trap circles appear, lasting 8 s; on contact, root player 0.6 s) | 60 (three ground-crack VFX, brown root-knot decals) | n/a — patches persist | 0 direct (root sets up follow-up) | Navigate around; patches do NOT damage boss either, can be used as line-of-sight cover from acorn arc |
| **Acorn-toss (double)** | 36 | 24 | 22 per acorn | Dodge laterally; spread is 30° |
| **Root-spike** | unchanged | unchanged | unchanged | unchanged |
| **Leaf-flurry** | unchanged | unchanged | unchanged | unchanged |

Cadence: phase 1 cadence + Root-snare-patch every ~12 s + Acorn-toss replaces with double-toss.

### Phase 3 — Done-being-polite

Inherits phases 1+2 attacks (modified) plus the set-piece:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Acorn-rain** (full canopy shakes; 8 acorns drop across arena over 3.0 s, each telegraphed individually 0.6 s before impact) | 60 (canopy shake + leaf shower) | 60 — canopy droops | 22 per hit | **1.0 s droop window after rain ends — DPS window**; this is the fight's MVP opening |
| **Root-snare patch (5)** | 60 | n/a | 0 | Tighter pathing required |
| **Root-spike (triple)** (3 spikes in sequence at predicted positions, 0.4 s apart) | 42 per spike, staggered | 18 each | 30 per spike | Predict and reposition between spikes |
| **Leaf-flurry** | REMOVED in phase 3 — Mama Oak has stopped trying to be polite | n/a | n/a | n/a |
| **Acorn-toss (double)** | unchanged | unchanged | unchanged | unchanged |

Cadence: Acorn-rain every ~15 s, Root-spike-triple every ~6 s, Root-snare-patch every ~10 s, Acorn-toss fills gaps.

## Arena (summary)

- **Biome**: Forest, 70 × 90 u oval (per `../../01-biomes/forest/layout.md`).
- **Mama Oak position**: dead center (0, 0). 3.0 u radius trunk collider — bunny must circle.
- **Ambient root-snares suppressed during fight** (boss owns snares; ambient would double-punish).
- **Phase 1**: open arena; bunny circles.
- **Phase 2**: 3 root-snare patches at scripted positions (N, E, S of boss at ~6 u offset).
- **Phase 3**: 5 root-snare patches at scripted positions (adds 2 more at W and center-of-arena radius); acorn-rain shadow markers pool spawns over 3 s.
- **2 mushroom-ring props** persist at arena east/west (per `../../01-biomes/forest/layout.md` boss-arena delta) — give player +5% pickup-magnet refuge zones.

## Telegraph color cues

Consistent palette per `../old-boar-king/mechanics.md` §Telegraph color cues.

| Attack | Telegraph color | Telegraph shape | Lead time |
|---|---|---|---|
| Root-spike | Yellow | Brown ground-crack circle | 0.5 s before attack |
| Acorn-toss | Orange | Small shadow-circle at landing | 0.4 s before |
| Acorn-toss double | Orange | 2 shadow-circles | 0.4 s before |
| Leaf-flurry | Yellow | Wide cone arc decal | 0.3 s before |
| Root-snare patch | Yellow | 3 (or 5) pulsing root-knot decals | 0.6 s before patches arm; patches stay yellow while live |
| Root-spike triple | Yellow | 3 sequential brown ground-crack circles | 0.4 s per spike |
| Acorn-rain | Orange | 8 progressive shadow-circles across arena | 0.6 s per acorn (staggered) |

Color rule per art-bible: yellow = warn, red = damage, orange = AOE landing. Mama Oak skews heavily yellow/orange (no red attacks — fits her "polite, then firmer, then exasperated" arc; she never reaches "lethal-furious" register).

## Intro line (per `narrative/04-boss-intros.md`)

**Intro card (≤ 12 words):** "Mama Oak's roots are up. Step lightly."
**TR seed:** "Koca Meşe'nin kökleri uyandı. Hafif bas."

Plays as a 1.2 s text overlay during the boss entrance animation. Localizer key: `BOSS_INTRO_OAK` per `narrative/05-localization-keys.md`.

## Win condition

Player reduces boss HP to 0 within **120 seconds** target TTK (longer than Crab — stationary boss has more total HP per `07-bosses.md`: ~6000 hp split 33/33/34). Balance-engineer tunes actual HP per `data/balance/bosses.json`.

On defeat:
1. Boss stops attacking; 2.0 s "long sigh" animation — canopy droops, leaves cascade down.
2. A single oversized acorn pops out and rolls to the bunny's feet (literal Carrot pickup per `07-bosses.md`).
3. Carrot-burst VFX from the trunk base (3 Soul Shards + 1 guaranteed chest + 5 gold coins per `05-enemies.md` drop table).
4. Slow-mo to 0.3x for 1.2 s.
5. The trunk does NOT fall — Mama Oak is fine; she's just tired and not dropping more acorns. Bunny scampers off with the basket.
6. Outro beat begins (per pacing model beat 9).

## Loss condition

Player HP reaches 0 → revive offer modal per `01-core-loop.md`. Decline → run-end tally.

## Telegraph audio

| Attack | Audio cue | Timing |
|---|---|---|
| Root-spike | Low wooden creak + dirt-thump | At telegraph onset |
| Acorn-toss | Branch-creak + soft whoosh | During branch wind-back |
| Leaf-flurry | Soft canopy-rustle | At telegraph onset |
| Root-snare patch | 3 sequential vine-snap pops | At telegraph onset |
| Acorn-rain | Heavy canopy-shake + cascading wood-knock | During canopy shake |
| Defeat | Long warm exhale + leaf-cascade rustle | At HP-0 trigger |

No vocal lines. Mama Oak is non-verbal; her register is wooden, soft, kindly.

## Notes on tone-bible adherence

- Face-knot eyes are **warm amber**, never glowing red.
- Roots are smooth visible vines, NOT gnarled, thorny, or barbed.
- Acorns are oversized and round with little hats — toy-like.
- The phase-3 transition reads as "firm now, not furious." She does not roar.
- Defeat is a sigh, not a fall — the tree stands; the bunny is the one who leaves.
- No red telegraph colors in her catalog (her register stops at "orange/exasperated").

## Cross-references

- Arena layout + phase prop staging: `../../01-biomes/forest/layout.md` §Boss-arena delta.
- Boss spawn in waves: `../../01-biomes/forest/waves.json` at `t=420`.
- Boss role baseline (HP, damage): `../../../02-gdd/05-enemies.md` boss row.
- Boss concept narrative: `../../../02-gdd/07-bosses.md` §3.
- Tone bible intro-line voice: `../../../02-gdd/narrative/00-tone-bible.md`.
- Balance-engineer tuning lands in: `data/balance/bosses.json`.
- Hitstop spec for boss kill: `../../../02-gdd/11-feel-pillars.md` pillar 4.
