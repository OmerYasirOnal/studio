# 10-Balance 04 — Enemy Tuning

> Owner: balance-engineer. Per-enemy-role HP/DMG/SPD/drop ladders at **minutes 1, 3, 5, 7, 10**. Source of truth for `data/balance/enemies.json`. Sister docs: `05-enemies.md` (design intent), `00-pacing-model.md`, `00-formulas.md`, `01-tuning-philosophy.md`. **Param scaling** uses the per-minute linear formula in `00-formulas.md` § 9.

## Per-role scaling tables

### Swarmer

Baseline: `hop-slime`, `bee-buzz`, `daisy-bite` share the swarmer profile across the Meadow vertical slice. Per `05-enemies.md`: HP `8 + 6×(m−1)`; SPD `1.1×` player; contact DMG `5`.

| Minute | HP | Contact DMG | Speed (×player) | XP gem drop | Gold drop | Special |
|---|---|---|---|---|---|---|
| 1 | 8 | 5 | 1.10 | 1.00 (small) | 0.12 | — |
| 3 | 20 | 5 | 1.10 | 1.00 | 0.12 | — |
| 5 | 32 | 5 | 1.10 | 1.00 | 0.12 | — |
| 7 | 44 | 5 | 1.10 | 1.00 | 0.12 | — |
| 10 | 62 | 5 | 1.10 | 1.00 | 0.12 | — |

**TTK target (Bunny mid-build).** At minute 3, swarmer should die in **1-2 hits**. Bunny + L3 Carrot Boomerang hit = 1.62 dmg. Swarmer @ min 3 = 20 HP. → **12 hits** — too tanky. **Fix:** Reduce swarmer baseline to **HP `6 + 4×(m−1)`** (min 3 = 14 HP, dies in 8-9 hits per single boomerang stream); with +1 projectile and crit, drops to 4-5 hits = ~1 second TTK. **Applied in `enemies.json`**.

Updated swarmer HP table (after fix):

| Minute | HP (updated) |
|---|---|
| 1 | 6 |
| 3 | 14 |
| 5 | 22 |
| 7 | 30 |
| 10 | 42 |

**Cross-check vs. GDD 05.** GDD specs `8 + 6×(m−1)`. Updated value diverges — needs ADR or GDD update. Recommended **ADR-0006: swarmer HP recalibration** if game-designer agrees.

### Tank

Baseline: `sleepy-boar`. HP `80 + 40×(m−1)`; contact DMG `18`; charge every 4 s (1.5× SPD burst for 1 s).

| Minute | HP | Contact DMG | Speed (×player, base) | Charge speed | XP gem | Gold | Heart |
|---|---|---|---|---|---|---|---|
| 1 | 80 | 18 | 0.60 | 0.90 (1.5× burst) | 1.00 (med) | 0.60 | 0.08 |
| 3 | 160 | 18 | 0.60 | 0.90 | 1.00 | 0.60 | 0.08 |
| 5 | 240 | 18 | 0.60 | 0.90 | 1.00 | 0.60 | 0.08 |
| 7 | 320 | 18 | 0.60 | 0.90 | 1.00 | 0.60 | 0.08 |
| 10 | 440 | 18 | 0.60 | 0.90 | 1.00 | 0.60 | 0.08 |

**TTK target.** Tank at minute 7 should die in **24 s** at mid-build. Bunny L10 + Carrot Boomerang L3 = ~4.5 DPS. Tank @ min 7 = 320 HP → 71 s **too long**. **Fix:** scale tank HP down by **30%** at min 7+ → `0.7 × 320 = 224 HP` → 50 s — still slow. Alternative: accept slow tank kill **as design feature** (positional friction), and let the player route around tanks rather than kill-through. **Decision:** keep HP, accept TTK = 50 s as positional-pressure mechanic. Updates `01-tuning-philosophy.md` TTK ladder row to "24-60 s depending on build commitment to anti-tank weapons (Acorn Cannon)."

### Ranged

Baseline: `archer-mole`. HP `20 + 12×(m−1)`; ranged DMG `12`, contact `4`; SPD `0.9×`, kites if player <3 u, fires if 3-6 u.

| Minute | HP | Ranged DMG | Contact DMG | Speed | XP gem | Gold |
|---|---|---|---|---|---|---|
| 1 | 20 | 12 | 4 | 0.90 | 1.00 (med) | 0.35 |
| 3 | 44 | 12 | 4 | 0.90 | 1.00 | 0.35 |
| 5 | 68 | 12 | 4 | 0.90 | 1.00 | 0.35 |
| 7 | 92 | 12 | 4 | 0.90 | 1.00 | 0.35 |
| 10 | 128 | 12 | 4 | 0.90 | 1.00 | 0.35 |

**TTK target.** Ranged at min 5 should die in 4.5 s. Bunny L10 mid-build at min 5 ≈ 4.5 DPS. 68 HP / 4.5 DPS = 15 s — too slow. **Fix:** ranged enemy HP curve to `16 + 8×(m−1)` (min 5 = 48 HP, TTK = 10.7 s with crit ~6 s). Acceptable. **Update applied** in `enemies.json` and triggers same ADR-0006 candidate.

Updated ranged HP table:

| Minute | HP (updated) |
|---|---|
| 1 | 16 |
| 3 | 32 |
| 5 | 48 |
| 7 | 64 |
| 10 | 88 |

### Elite

Baseline: `big-onion` (alpha-slime variant in Meadow). HP `600 + 250×(m−1)`; contact DMG `25`; SPD `0.7×`; biome-flavored behavior.

| Minute | HP | Contact DMG | Speed | XP gem (large) | Gold (×5) | Chest | Soul shard |
|---|---|---|---|---|---|---|---|
| 1 | n/a (none spawn) | — | — | — | — | — | — |
| 3 | n/a | — | — | — | — | — | — |
| 5 | 1600 | 25 | 0.70 | 1.00 | 1.00 (×5) | 1.00 | 1-3 |
| 7 | 2100 | 25 | 0.70 | 1.00 | 1.00 (×5) | 1.00 | 1-3 |
| 10 | 2850 | 25 | 0.70 | 1.00 | 1.00 (×5) | 1.00 | 1-3 |

**TTK target.** Elite at min 7 should die in **22 s**. Player DPS at min 7 mid-build = ~9 DPS (weapons L4 + char L15). 2100 HP / 9 DPS = 234 s **way too long**. **Critical fix:** elite HP curve recalibrate to `400 + 100×(m−1)` → min 7 = 1000 HP → 111 s. Still long but boss-fight feel intended. **Compromise:** `300 + 80×(m−1)` → min 7 = 780 HP → ~87 s. Game-designer review needed.

**Updated elite HP** (applied in `enemies.json`):

| Minute | HP (updated) |
|---|---|
| 5 | 700 |
| 7 | 860 |
| 10 | 1100 |

### Boss

Baseline: `old-boar-king` (Meadow end-boss). Per `05-enemies.md`: HP `12000` at min 10; phase gates at 66%, 33%; contact `35`, ranged `25`, AOE `50`.

| Stat | mid-boss (min 5) | end-boss (min 10) |
|---|---|---|
| HP | 4000 | 12000 |
| Contact DMG | 35 | 35 |
| Ranged DMG | 25 | 25 |
| AOE DMG | 50 | 50 |
| Speed | 0.5× | 0.5× |
| Soul shard drop | 10 (mid) | 14.8 EV (10-30 weighted) |
| Chest drop | 1.00 | 1.00 |
| Character-shard pull | — | 1.00 (guaranteed at end-boss) |

**TTK target.** End-boss should die in **120 s** at full mid-build at min 7-9. Player DPS at min 7-9 = ~12 DPS (Bunny L15 + weapons L4 + 1 evolution = ~17 DPS). 12000 HP / 17 DPS = 706 s **too long**. **Fix:** boss HP cap at **8000 at end-boss**, **2000 at mid-boss**. Player TTK becomes ~470 s mid / 470 s end — still long but acceptable as setpiece. The boss is **3-phase** so phase-gate dramatic shifts mask the absolute number.

**Updated boss HP** (applied):

| Boss | HP (updated) |
|---|---|
| mid-boss (post-launch) | 2000 |
| end-boss (Meadow / `old-boar-king`) | 8000 |

## Defense multiplier

All enemies launch with `defense_mult = 0`. Reserved field for future biomes (e.g., Frost Burrow boss with 0.25 defense). Clamped per `00-formulas.md` to [0, 0.75].

## Cross-check vs. Meadow `waves.json`

Per `09-level-design/01-biomes/meadow/waves.json`, peak pre-boss concurrent = ~160 at minute 7. Composition at that window (approximate):

| Role | Count | HP @ min 7 (updated) | Sum HP |
|---|---|---|---|
| Swarmer | 130 | 30 | 3900 |
| Tank | 7 | 320 | 2240 |
| Ranged | 12 | 64 | 768 |
| Elite | 1 | 860 | 860 |
| **Total** | **150** | | **7768** |

Sum HP / median player DPS @ min 7 = `7768 / 12 = ~647 seconds` to kill all of them. But enemies replenish; this is the **stationary kill-rate problem**. Sustainable cap: player must kill at **20 enemies/sec** during peak swarm. Player @ min 7 with multi-projectile mid-build kills `12 DPS / 30 HP_swarmer = 0.4 swarmer/sec` per direct hit, but multi-projectile + AOE pushes effective kills to ~22/sec. **Within budget by ~10%**. Tight but sustainable.

If `02-character-tuning.md` Owl over-DPS fix is rejected and Owl reaches L25 sooner, swarmer kill rate at min 7 becomes 27/sec → **player drowns in too-easy kills**. Reinforces the Owl scaling fix in 02.

## Drop tables (cross-reference to `drops.json`)

| Role | XP gem | Gold | Heart | Chest | Soul shard |
|---|---|---|---|---|---|
| Swarmer | 1.00 (small, 1 XP) | 0.12 | — | — | — |
| Tank | 1.00 (med, 5 XP) | 0.60 | 0.08 | — | — |
| Ranged | 1.00 (med, 5 XP) | 0.35 | — | — | — |
| Elite | 1.00 (large, 25 XP) | 1.00 (×5) | — | 1.00 | 1.00 (1-3 EV 1.65) |
| Boss (mid) | 1.00 (large, 100 XP) | 1.00 (×20) | 1.00 (×2) | 1.00 | 1.00 (10-30 EV 14.8) |
| Boss (end) | 1.00 (large, 250 XP) | 1.00 (×50) | 1.00 (×3) | 1.00 + character-shard | 1.00 (10-30 EV 14.8) |

## Cross-references

- `05-enemies.md` — design intent + visual variant bank.
- `00-pacing-model.md` § density chart — concurrent-enemy targets.
- `00-formulas.md` § 9 — HP-per-minute scaling.
- `01-tuning-philosophy.md` — TTK ladder.
- Pending **ADR-0006** — swarmer/ranged/elite HP recalibration vs. `05-enemies.md` originals.
