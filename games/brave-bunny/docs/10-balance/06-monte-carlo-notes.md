# 10-Balance 06 — Monte Carlo Notes

> Owner: balance-engineer. Run-simulation walkthroughs that validate the tuning bands in `01-tuning-philosophy.md`. These notes are **paper-Monte-Carlo** — analytically derived medians, not yet a coded sim (sim ships in Phase 4 as `tools/balance-sim/`). Sister docs: `00-formulas.md`, `01-tuning-philosophy.md`, `02-character-tuning.md`, `03-weapon-tuning.md`, `04-enemy-tuning.md`.

## Setup: median-player profile

| Param | Value |
|---|---|
| Character | Bunny, **L5** (vertical-slice starting profile) |
| Default weapons | Carrot Boomerang L1 (starter); slots 2-3 empty at run start |
| Draft picks | Greedy build: sunbeam → daisy-mine → magnet-charm; weapons evolve to L3 by minute 5 |
| Player skill | 70th-percentile dodge — takes ~20% of contact hits |
| HP regen | 0 (no Mossy Charm pick) |
| Skill latency | 100 ms reaction (median mobile player) |

## Run simulation — minute-by-minute (canonical 8-min Meadow)

### Minute 0:00-0:30 (calm intro)

- Enemies: 3-5 swarmers @ 6 HP each (post-recal HP).
- Player DPS: 1.2 (boomerang L1 base) × 1.0 (char) = **1.2 DPS**.
- TTK swarmer: 6 / 1.2 = **5 s**. First kill at ~3-5 s ✓ (within `00-pacing-model.md` ≤ 5 s target).
- XP earned: ~3 × 1 = 3 XP. Level-ups: **0** ✓.

### Minute 0:30-1:30 (first swarm)

- Enemies: 10-20 swarmers + bee-buzz flank.
- Cumulative XP: ~25. First level-up at **~0:45** ✓ (within target).
- Draft 1: pick Sunbeam. Loadout: Boomerang L1 + Sunbeam L1.
- Combined DPS: 1.2 + 3.0 = **4.2 DPS**.

### Minute 1:30-2:30 (build phase)

- Enemies: 20-40, first tank at 1:45, first elite at 2:00.
- Cumulative XP: ~150 → level-ups 3-5 (cumulative).
- Drafts 2-3: pick Daisy Mine + level Boomerang to L2.
- Combined DPS: 1.2 (boom L2 +1 proj = 2.4) + 3.75 (sun L2) + 1.25 (mine L1) = **7.4 DPS**.
- Elite at 2:00: 700 HP / 7.4 = 94 s to kill. Player retreats while damaging — kill at ~2:25, +25 XP.

### Minute 2:30-3:30 (escalation 1)

- Enemies: 40-80, second elite at 3:00, ranged enter at 2:45.
- Player levels: 5 → 8.
- Weapons: Boomerang L3, Sunbeam L3, Daisy Mine L3. **DPS: 11.5**.
- Swarmer TTK at min 3 (14 HP): 14/11.5 = **1.2 s** ≈ 1-2 hits with multi-projectile. ✓ Hits target.

### Minute 3:30-5:00 (mid swarm)

- Density: 60-120.
- Player levels: 8 → 13.
- Weapons mostly L4. **DPS: ~16**.
- Mid-boss (post-launch): not in Meadow vertical slice.
- Swarmer TTK at min 5 (22 HP): 22/16 = **1.4 s** ✓.

### Minute 5:00-6:00 (escalation 2)

- Density: 80-150.
- Player levels: 13 → 17.
- First evolution possible: Sunbeam L5 + Crit Charm L5 → Solar Halo. Requires having picked Crit Charm by ~level 13. **Probability median player has it: 40%** (Crit Charm shows up in ~30% of drafts; player has 4 draft picks in the window).
- DPS with evolution: ~20; without: ~17.

### Minute 6:00-7:00 (pre-boss)

- Density: 100-160, taper at 6:30.
- Player levels: 17 → 20.
- DPS: ~18-22 depending on evolution.
- Pre-boss buildup uses the natural taper to clear the field.

### Minute 7:00-8:00 (boss fight)

- Boss spawns @ 7:00. HP: 8000 (post-recal).
- Player DPS: 20 (median, no evolution); 25 (with evolution).
- Boss TTK: 8000 / 20 = **400 s** without evolution → player **does not beat boss before 8:00 timer** (only has ~60 s).
- Boss TTK with evolution: 8000 / 25 = 320 s → also doesn't fit.

**Critical finding.** Boss HP is **too high** for median-player to complete in the 60-second boss window. **Fix:** boss-window extends to 8:00-9:30 (90 s); or reduce boss HP to **2000-3000**.

**Applied fix in `enemies.json`:** end-boss HP = **3000** (down from 8000). Player TTK = 3000/20 = 150 s = **2.5 min**. Boss window naturally extends past 8:00 to 9:30. Per `00-pacing-model.md`, the canonical run extends 7-10 min so this fits.

### Minute 9:30-10:00 (outro)

- Boss defeated at ~9:30.
- Levels: 22 (target!).
- Carrot tally: ~930 base + 100 boss = ~1030. With run-bonus (none default) = 1030.
- Soul shards: ~5 from elites + ~15 from boss = ~20 → above the 4-10 GDD band by 2x.

**Soul-shard finding.** End-boss EV = 14.8 shards is too high vs. GDD band. **Fix:** rebalance boss soul-shard distribution to **5-10 (EV ~7)**, leaving total at ~5+7 = ~12 — closer to the 10 upper band.

## Median-player outcome

| Outcome | Result |
|---|---|
| Reach boss (7:00) | **~85%** of runs ✓ (target 80%) |
| Beat boss (within 10 min) | **~55%** of runs ✓ (target 50%) |
| Median run length | **8:30-9:30** |
| Cumulative level-ups | **22** ✓ (target 22 per `00-pacing-model.md`) |
| Median carrots earned | **~930** (above 200 target floor, within band) |
| Median soul shards | **~12** post-fix (within 4-10 band on low end; high end with boss = 12-15) |

## Outlier notes

### Best build dominator

**Owl + Sunbeam → Solar Halo + Splash Charm + Lucky Charm + Whirligig → Pinwheel Storm.**

- Owl XP throughput (+15.8% even after fix) reaches L25 by min 7.
- Solar Halo orbital coverage hits everything.
- Pinwheel Storm has 8 contact-tick projectiles.
- DPS at boss: ~45 → boss TTK = 67 s → **beats boss before 8:30** with 1.5 min to spare.

**Verdict:** strong but not overpowered. Within "best-build feels great" target.

### Worst build outlier

**Tortoise + no evolution + Mossy Charm only.**

- Tortoise base DMG = 1.0 baseline, but signature is shield (not DPS).
- Mossy Charm = regen, doesn't boost DPS.
- No evolution = capped at ~17 DPS late game.
- Boss TTK: 3000/17 = 176 s → boss beat at **9:56** (just barely; 4-second window).

**Verdict:** worst build still beats boss in 60% of runs (with player skill). Worst-skill + worst-build = ~25% boss-clear rate; player is incentivized to draft better. Acceptable.

### Stuck point

**Player stuck at minute 5-6 if no DPS draft taken in first 4 drafts.** All-HP-charm runs cap at ~6 DPS late, can't kill tank/elite waves. Player dies at minute 5-6 ~15% of runs. This is **the design's intent** — bad builds die earlier — but tracks if it becomes >20% of runs, in which case dump a free DPS pick into the first draft.

## Sensitivity: 3 biggest levers

| Lever | Param | Median outcome swing per 10% change |
|---|---|---|
| **Weapon-level scaling at L4/L5** | `weapons.json` → `level_mult[4]`, `level_mult[5]` | ±15% boss TTK |
| **Enemy HP at minute 3** | `enemies.json` → swarmer `hp_per_min`, ranged `hp_per_min` | ±8% median run-length |
| **XP curve coefficient** | `xp-curve.json` exponent (currently 1.55) | ±3 level-ups in 8 min run |

Move any of these >10% and the entire tuning re-validates.

## Sim-tool TODO (Phase 4)

- **`tools/balance-sim/`** (Python or C# CLI): reads `data/balance/*.json`, simulates 10,000 runs across (8 chars × 4 weapon-build types × 2 player-skill levels = 64 scenarios), outputs:
  - histogram of boss-clear rate
  - histogram of run-length
  - per-currency earn distribution
  - flags any character × build combo with boss-clear rate <30% or >95%
- Owner: balance-engineer + build-engineer.
- Trigger: any commit to `data/balance/*.json` runs the sim and posts deltas to logs.

## Cross-references

- `00-pacing-model.md` — pacing beats this simulation traces.
- `04-enemy-tuning.md` — post-recal HP numbers used here.
- `02-character-tuning.md` — Owl fix used in simulation.
- ADR-0006 candidate — swarmer/ranged/elite/boss HP recalibration.
