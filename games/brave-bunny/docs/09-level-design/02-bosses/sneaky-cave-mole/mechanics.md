# Sneaky Cave Mole — Boss Mechanics

> Owner: level-designer (mechanics + telegraphs); balance-engineer (TTK + damage tuning). Cavern biome boss (biome 4). Sister docs: `../../01-biomes/cavern/layout.md` (arena), `../../01-biomes/cavern/waves.json` (boss-event spawn at t=420), `../../00-pacing-model.md` (beat 8 boss-fight window), `../../../02-gdd/05-enemies.md` (boss role baseline), `../../../02-gdd/07-bosses.md` (Sneaky Cave Mole concept), `../../../02-gdd/narrative/04-boss-intros.md` (intro line).

All frame counts assume **60 fps**. 60 fps is the single canonical anchor for the whole boss spec.

## Concept

The Sneaky Cave Mole has been digging in this cavern for about three weeks (which for a mole is "as long as anyone can remember"). He does not like to be seen — being seen is the worst thing that can happen to a mole — so he pops up, jabs with his shovel-paw, and pops back down. He is not menacing; he is **shy and twitchy**, and the bunny scares him as much as he scares the bunny. Per tone bible: round chubby mole, tiny dark glasses askew (he says they help him see in the light — they do not), small "eep!" SFX every time he surfaces, defeat is a clean-the-glasses-blink-twice-burrow-away with a small white flag.

**Key fight-shape note**: the mole spends **~60% of fight underground**. The fight is not about sustained DPS — it's about reading telegraphed dig-mounds and punishing the exposed windows. This is the **shortest-phase-count boss** (2 phases instead of 3) because the burrow/teleport mechanic carries its own complexity budget.

## Phases

| Phase | HP gate | Name | Mood / behavior |
|---|---|---|---|
| 1 | 100-50% | Twitchy-digger | Dig-strike + dirt-spray + surface-skitter; long exposed windows (0.6 s) |
| 2 | 50-0% | Sneaky-trickster | Adds triple-mound (decoy mind-game) + stalactite-shake; exposed windows shorten to 0.4 s |

Phase transitions: HP gate hit → boss briefly invulnerable (1.0 s) → mole pops up, removes glasses, polishes them, puts them back on crooked → "eep!" SFX → burrows. Phase 2 attacks pool unlocks.

## Attack pattern catalog

All telegraph windows ≥ 0.4 s for dig-strike (the simplest tell) and 0.6-0.8 s for more complex attacks. The 0.4 s minimum is below the boss-tier 0.8 s baseline per `05-enemies.md`, but is **deliberate** — the mole's complexity is in *guessing which mound is real*, not in dodging fast attacks. Compensated by long mole-exposed windows that give the player generous DPS opportunities.

### Phase 1 — Twitchy-digger

| Attack | Telegraph (frames @ 60 fps) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Dig-strike** (mole pops up at a telegraphed dig-mound, jabs in a 1.0 u forward cone, burrows back 0.6 s later) | 36 (dirt-mound rises at target spot, dust-puff) | 36 — **exposed atop ground** | 35 inside cone | **0.6 s exposed window** — primary DPS opportunity; bunny stands ready adjacent to mound |
| **Dirt-spray** (mole surfaces briefly, spits 2.0 u radius dirt cloud, burrows 0.4 s later) | 30 (small mound + small cough SFX) | 24 — exposed | 18 + 0.5 s vision-obscure inside cloud | 0.4 s exposed; vision-obscure adds difficulty mid-fight |
| **Surface-skitter** (mole emerges and scampers a visible 4 u line, re-burrows; not an attack but exposes boss) | 24 (mound) | 60 — scampering visible | 0 from attack; 20 contact if bunny touches him | Full 1.0 s window — pursuit opportunity but contact-risky |

Cadence: Dig-strike / Dig-strike / Dirt-spray / Surface-skitter / Dig-strike with 1.0-1.5 s underground time between surfacings.

### Phase 2 — Sneaky-trickster

Inherits phase 1 attacks (modified) plus:

| Attack | Telegraph (frames) | Wind-down (frames) | Damage | Player opening |
|---|---|---|---|---|
| **Dig-strike** | unchanged | shortened to 24 — exposed | 35 | Tighter window, faster reflex required |
| **Triple-mound** (3 mounds rise simultaneously; one is real, two are decoys, all telegraphed identically) | 48 (three mounds, identical visuals) | 36 — real mole strikes from one | 35 from real mole only | **Guess correctly = full 0.6 s window**; guess wrong = take the strike. **Mind-game mechanic.** |
| **Dirt-spray (double)** (upgrade — two surfacings, 0.8 s apart at different positions) | 30 per surfacing | 24 each | 18 + 0.5 s obscure each | Reposition between sprays |
| **Stalactite-shake** (mole stays underground; shakes ceiling; 3 stalactite drop zones telegraph + fall over 2.0 s) | 48 per zone | n/a | 25 + 0.3 s stagger per zone | Dodge zones; **boss is not exposed during this attack** — pure dodge phase |
| **Surface-skitter** | REMOVED in phase 2 — replaced by triple-mound | n/a | n/a | n/a |

Cadence: Triple-mound every ~8 s, Dig-strike every ~3 s, Stalactite-shake every ~12 s, Dirt-spray-double every ~10 s. Phase 2 is **read-the-mound-or-die** as the central tension.

### How to read the triple-mound decoy

Per `07-bosses.md` flavor, the three mounds are visually identical — but there's a **tone-respecting tell** the player can learn:
- The real mole-mound has a faint **dirt-puff** VFX emanating ~6 frames before the others (0.1 s before the strike). Subtle but learnable.
- This tell is **not** documented in the in-game tutorial — it's a "git gud" signal. Reduces frustration ceiling without removing the mind-game floor.

## Arena (summary)

- **Biome**: Cavern, 60 × 60 u (per `../../01-biomes/cavern/layout.md`).
- **Reveal radius**: 6.5 u baseline, locally extended to 8.0 u near torch props.
- **Phase 1**: open dirt floor; 4 fixed dig-mound marker positions at (±8, ±8) — mole surfaces from one per dig-strike pattern.
- **Phase 2**: same 4 dig-mound positions + stalactite-shake-zone pool spawns over 2 s (3 zones from a 6-position pool).
- **Torch props** at 4 corners — extend reveal radius locally; bunny pathing between them maintains visibility.
- **Ambient stalactite hazards remain active** — the only fight where ambient hazards stack with boss hazards in Cavern (boss riffs on the mechanic).

## Telegraph color cues

Consistent palette per `../old-boar-king/mechanics.md` §Telegraph color cues.

| Attack | Telegraph color | Telegraph shape | Lead time |
|---|---|---|---|
| Dig-strike | Yellow | Brown dirt-mound rising + dust ring | 0.6 s before attack (P1), 0.4 s (P2) |
| Dirt-spray | Yellow | Small mound + cough-dust ring | 0.5 s before |
| Surface-skitter | Yellow (movement-tell only, not damage) | Mound + visible movement trail | 0.4 s before |
| Triple-mound | Yellow | 3 identical brown mounds (real one has faint pre-strike puff) | 0.8 s before strike |
| Stalactite-shake | Orange | 3 expanding-dust-circle decals at zone positions | 0.8 s per zone (matches ambient stalactite telegraph) |

The mole's palette is **almost entirely yellow** — he's tricky (warn), not deadly (red). The single orange is the AOE landing of the stalactite drops. This skews the boss's read toward "puzzle" not "danger."

## Intro line (per `narrative/04-boss-intros.md`)

**Intro card (≤ 12 words):** "Cave Mole's in the floor. Listen for the rumble."
**TR seed:** "Sinsi Köstebek yerin altında. Sesini dinle."

Plays as a 1.2 s text overlay during the boss entrance animation. Localizer key: `BOSS_INTRO_MOLE` per `narrative/05-localization-keys.md`.

The intro line specifically primes the player on the **audio tell** — the dirt-rumble before each surfacing is the most reliable read.

## Win condition

Player reduces boss HP to 0 within **100 seconds** target TTK. Approx 5500 hp split 60/40 across phases per `07-bosses.md` (phase 1 longer because teleport defends his HP). Balance-engineer tunes per `data/balance/bosses.json`.

On defeat:
1. Mole surfaces normally for what looks like another dig-strike.
2. He sits up on his haunches, removes his glasses, polishes them with both shovel-paws (1.5 s), blinks twice in confusion (0.5 s).
3. He pulls a tiny white flag from his shovel-paw and waves it once.
4. Burrows down slowly with an "eep!" — disappears below the dirt.
5. Carrot-burst VFX from his last position (3 Soul Shards + 1 chest + 5 coins per `05-enemies.md`).
6. Slow-mo to 0.3x for 1.2 s.
7. Outro beat begins.

He is **not defeated in the violent sense** — he has yielded. The bunny scoots past the mound and keeps going.

## Loss condition

Player HP reaches 0 → revive offer modal per `01-core-loop.md`. Decline → run-end tally.

## Telegraph audio

The **audio tell is critical** for this boss — the mole surfaces at telegraphed positions but a 0.3 s pre-surface rumble can give attentive players an extra read.

| Attack | Audio cue | Timing |
|---|---|---|
| Dig-strike | Soft dirt-rumble (0.3 s) → "eep!" → shovel-jab whoosh | 0.3 s pre-surface rumble + onset SFX |
| Dirt-spray | Cough + soft puff | At onset |
| Triple-mound | 3 identical rumbles (real one ~6 frames offset) | 0.8 s before strike |
| Stalactite-shake | Underground "thrum" + ceiling-creak | At onset |
| Surface-skitter | Visible scamper + paw-patter | During scampering |
| Defeat | Glass-polish "skritch" + small "eep" + dirt-fountain | At HP-0 trigger |

The "eep!" is the **mole's signature SFX** — every surfacing gets one. Per tone bible: small, high, slightly mortified — not aggressive.

## Notes on tone-bible adherence

- Tiny dark glasses always askew — never sharp, never reflective in a menacing way.
- Burrow VFX is a goofy dirt-fountain, NOT a sinister shadow or void.
- The triple-mound decoy is "tricksy hobbits" not "horror movie" — the mole is mischievous, not malicious.
- Stalactite-shake is the mole shaking the ceiling FROM UNDERGROUND with a small "umph!" — comedic effort, not menacing power.
- Defeat with white-flag-wave is the gentlest defeat animation in the roster.
- No fangs, no claws — shovel-paws are oversized and rounded.

## Cross-references

- Arena layout + phase prop staging: `../../01-biomes/cavern/layout.md` §Boss-arena delta.
- Boss spawn in waves: `../../01-biomes/cavern/waves.json` at `t=420`.
- Boss role baseline (HP, damage): `../../../02-gdd/05-enemies.md` boss row.
- Boss concept narrative: `../../../02-gdd/07-bosses.md` §4.
- Tone bible intro-line voice: `../../../02-gdd/narrative/00-tone-bible.md`.
- Balance-engineer tuning lands in: `data/balance/bosses.json`.
- Hitstop spec for boss kill: `../../../02-gdd/11-feel-pillars.md` pillar 4.
