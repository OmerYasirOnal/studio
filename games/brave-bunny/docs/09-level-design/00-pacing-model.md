# Pacing Model — Brave Bunny

> Owner: level-designer. Defines the **named beats** of a 7-10 minute run, the intensity curve they trace, and the per-biome variation knobs. Sister docs: `02-gdd/01-core-loop.md` (15-25 level-ups, 7-10 min target), `02-gdd/05-enemies.md` (per-minute density curve + 200-enemy perf cap), `02-gdd/06-biomes.md` (biome roster + hazard escalation), `01-biomes/<biome>/waves.json` (per-biome instantiation of these beats).

All frame counts in this and sibling docs assume **60 fps**. All time anchors are in **mm:ss** unless otherwise noted.

## Anchor numbers (carried forward)

| Anchor | Value | Source |
|---|---|---|
| Run length | 7-10 min (8 min canonical for boss-at-7:00 cadence) | `01-core-loop.md` |
| Level-ups per run | 15-25 | `01-core-loop.md` |
| Max concurrent enemies | 200 hard cap | `brave-bunny/CLAUDE.md` |
| Mid-run elite cadence | 1 elite per ~60 s peak | `05-enemies.md` |
| Boss event | 1 per run (end of run) | `05-enemies.md` |
| First kill latency | ≤ 5 s | This doc, calibration beat |
| First level-up latency | ~25 s (target ≤ 45 s) | `01-core-loop.md` cycle length |

## Named beats (canonical 8-minute run)

The run is partitioned into 9 named beats. Each beat has a **density band** (min/max concurrent enemies), a **level-up cadence target**, and an **agent contract** (what the beat exists to do for the player).

| # | Beat name | Window | Density band | Level-ups (cumulative) | Agent contract |
|---|---|---|---|---|---|
| 1 | **Calm intro** | 0:00 - 0:30 | 3-5 | 0 | Learn input, first kill ≤ 5 s, no pressure |
| 2 | **First swarm** | 0:30 - 1:30 | 10-20 | 1-2 (first at ~0:45) | Feel the loop, first draft event |
| 3 | **Build phase** | 1:30 - 2:30 | 20-40 | 3-5 | Identify a build direction, first elite at ~2:00 |
| 4 | **Escalation 1** | 2:30 - 3:30 | 40-80 | 5-8 | Second elite (~3:00), first weapon-evolve possible |
| 5 | **Mid swarm** | 3:30 - 5:00 | 60-120 | 8-13 | Steady pressure, identity locked in |
| 6 | **Escalation 2** | 5:00 - 6:00 | 80-150 | 13-17 | Third elite, optional mid-boss |
| 7 | **Pre-boss** | 6:00 - 7:00 | 100-160 (taper from 6:30) | 17-20 | Boss-approach signal at 6:30, density drops to clear visual field |
| 8 | **Boss fight** | 7:00 - 8:00 | 30-80 (boss + minion adds) | 20-22 (level-ups still possible) | 3-phase boss fight, swarm density drops so player can read tells |
| 9 | **Outro** | 8:00 - end | 8-15 | 22-25 | Post-boss minor swarm, run-end tally trigger when arena clears |

Total: **22-25 level-ups in a clean run** (top of the GDD band). A struggling player who dies early in beat 5 still banks 8-13 level-ups, which clears the 15-25 floor expectation (the floor is *per surviving 7-min run*; deaths bank what's earned to that point per `01-core-loop.md`).

## Intensity chart (ASCII, vertical = on-screen enemy count)

The curve is **double-peaked**: peak 1 at ~6:00 (pre-boss density), brief dip at 6:30 (boss-approach signal — game tells the player something is coming by *quieting*), then boss-fight at 7:00 with adds capping at ~80 to keep boss tells readable.

```
                                                 boss
                                                 v
160 |                                  ##                              tally
140 |                              ####  ##                              v
120 |                          ####        ##
100 |                      ####              ##
 80 |                  ####                    ##    B
 60 |              ####                          ## B B  B               (B = boss + adds)
 40 |          ####                                # B  B   B
 20 |   ####                                                  ####
  0 +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    0:00    1:00    2:00    3:00    4:00    5:00    6:00    7:00   8:00

  ^                ^         ^         ^         ^         ^         ^
calm           build       esc1      mid       esc2     pre-boss   boss  outro
intro          phase                 swarm                  +taper  fight
+1st-swarm
```

Read the chart: density climbs steadily from beat 1 through beat 6, peaks in beat 7 at ~150-160 concurrent (the *anticipation* peak), drops sharply at 6:30 (visual breath before boss), then the boss fight runs at a *deliberately low ambient density* (boss + 30-80 adds) so the player can read the boss telegraphs without the swarm camouflaging them.

## Beat-by-beat detail

### 1. Calm intro (0:00 - 0:30)

- **Density**: 3-5 swarmers (hop-slimes only in Meadow).
- **Spawn pattern**: ring at radius 35 u, staggered by 2 s.
- **First kill latency target**: ≤ 5 s (one swarmer must reach auto-attack range within 5 s of joystick movement).
- **No level-up here.** The player is learning the joystick + the auto-fire cadence.
- **No elites, no tanks, no ranged.** Pure swarmer baseline.

### 2. First swarm (0:30 - 1:30)

- **Density**: ramps from 5 → 20.
- **First level-up at ~0:45.** Triggers the very first draft modal — this is the player's "oh, *that's* the game" moment.
- **Spawn pattern**: alternating ring + stream; introduces the first flank (bee-buzz from east at ~1:00).
- **Still no elites, no tanks.** The player is learning the draft, not the enemy taxonomy.

### 3. Build phase (1:30 - 2:30)

- **Density**: 20 → 40.
- **Level-ups 3-5 (cumulative).** Player picks a build direction by level 3.
- **First tank enters** at ~1:45 (sleepy boar in Meadow).
- **First elite enters at ~2:00** (Big Onion in Meadow). The elite is a **scripted spawn**, telegraphed by a 1.5 s "ground rumble" VFX + dimmed-screen-edge.

### 4. Escalation 1 (2:30 - 3:30)

- **Density**: 40 → 80.
- **Level-ups 5-8 cumulative.** First weapon-evolve becomes mathematically possible (~3:00) for players who picked aggressively in the early drafts.
- **Second elite at ~3:00.**
- **Ranged enemies enter** at ~2:45 (archer-mole in Meadow).
- **Pattern variety**: scatter spawns introduced (random angular positions on the ring) to break the "stand-still-and-cleave" pattern.

### 5. Mid swarm (3:30 - 5:00)

- **Density**: 60 → 120.
- **Level-ups 8-13 cumulative** (cadence tightens — every ~20 s).
- **No new enemy archetypes** here; this beat is about **letting the player feel their build land**.
- Spawn variety pushed through *pattern* changes (multi-direction flanks, double-ring) not new units.

### 6. Escalation 2 (5:00 - 6:00)

- **Density**: 80 → 150.
- **Level-ups 13-17 cumulative.**
- **Third elite at ~5:30.**
- **Optional mid-boss event** (post-vertical-slice; Meadow ships without one — see `05-enemies.md` per-minute curve which notes mid-boss spike at min 5).
- This is the *peak normal density* per `05-enemies.md` (~120 at min 5 baseline; we push to 150 here for the climactic feel).

### 7. Pre-boss (6:00 - 7:00)

- **Density**: 100 → 160 (peak at ~6:25), then **taper to 60-80 by 7:00**.
- **Boss-approach signal at 6:30**: screen edge tints biome-boss-color (warm orange for Old Boar King), low rumble SFX, music swap to boss-stinger.
- **Spawn rate drops sharply** from 6:30 onward — the game *quiets* before the boss enters. This breathing room is the single most important pacing trick in the document.

### 8. Boss fight (7:00 - 8:00)

- **Boss spawns at 7:00** (center-spawn, 1.5 s entrance animation, time-dilate to 0.4x for 800 ms during entrance).
- **Ambient density during boss**: 30-80. This is **deliberately low** — the player needs visual clarity to read boss telegraphs.
- **Boss-fight minor adds** at intervals (every 25-30 s) refresh pressure without crowding tells.
- **Level-ups 20-22 cumulative** are still possible — XP from adds + the boss itself drops XP through the fight.

### 9. Outro (8:00 - end)

- **Boss defeat triggers**: chest drop + soul-shard burst + slow-mo (0.3x for 1.2 s) + tally-screen approach VFX.
- **Post-boss minor swarm** (8-15 enemies) for ~15 s to give the player a "victory lap" — they sweep the stragglers and the run-end tally activates.
- **Run-end tally**: triggered when (a) boss dead AND (b) all on-screen enemies cleared OR (c) 30 s elapsed since boss death, whichever comes first.

## Per-biome pacing variation

The named beats above are the **canonical curve**. Each biome modulates the curve along two axes: **enemy variety** and **hazard pressure**. The total intensity envelope (max 160 at pre-boss peak, ~80 during boss) stays constant across biomes for perf reasons; the *feel* varies.

| Biome | Density bias | Variety knob | Distinctive feel |
|---|---|---|---|
| **Meadow** | Standard curve, no modifiers | 4 swarmer skins, 2 tanks, 1 ranged, 1 elite | **Gentle** — calibration baseline; rascals walk in straight lines |
| **Beach** | +5% swarmer count (sideways crab motion eats sight lines) | Sand-puff minions during Crab Captain boss | **Skittery** — sideways crab gait throws off straight-line dodge |
| **Forest** | Density taper at 6:30 is **softer** (still 80-100 at boss approach) — underbrush hides enemies | Vine-trip minions summoned by Mama Oak | **Noisier** — root snares + low-vision adds cognitive load |
| **Cavern** | Density slightly *lower* throughout (-10%) because stalactites + reveal radius are doing the pressure work | Bat-mini hover-swarmers | **Claustrophobic** — low reveal radius, ceiling hazards, hushed mood |
| **Snow** | Longer line-of-sight (overcast bright), so density feels lower at same count; rebalance by +10% spawn count | Ice-puff drifts visible from across the arena | **Open** — longer sight lines, ice-slide drift adds positional uncertainty |

## Perf cross-check

Peak density anchor: **160 concurrent in beat 7 pre-boss**, **80 during boss fight**. Both well under the 200-enemy hard cap (`CLAUDE.md`). The cap is **a design feature, not a constraint to fight** (per `05-enemies.md`).

Assumption used for waves.json kill-rate math: **~6 s average enemy lifetime** (player auto-attacks kill ~10 swarmers per sec at peak build, swarmer HP scales per `05-enemies.md` formula). This bound is *validated against the curve* — if peak spawn rate × 6 s lifetime exceeds 160 in beat 7, we re-pace. Balance-engineer owns the actual TTK ladder; level-designer owns the spawn schedule.

## Cross-references

- Per-biome wave schedule source of truth: `01-biomes/<biome>/waves.json`.
- Boss spec source of truth: `02-bosses/<boss>/mechanics.md`.
- Hazard tuning numbers: `data/balance/biomes.json` (balance-engineer).
- HP/DMG scaling: `data/balance/scaling.json` (balance-engineer).
- Perf contract: `brave-bunny/CLAUDE.md` (200 enemies, 80 DC, 250k tris).
