# 10-Balance 01 — Tuning Philosophy

> Owner: balance-engineer. The **target bands** every tuning JSON serves. If a future change drifts outside these bands, re-justify in an ADR before merging. Sister docs: `00-formulas.md`, `02-character-tuning.md`, `04-enemy-tuning.md`, `06-monte-carlo-notes.md`.

## Anchor truths (from GDD)

- **Run length: 7-10 minutes** (8 min canonical with boss at 7:00). Source: `02-gdd/01-core-loop.md`.
- **Level-ups per run: 15-25** (target 22 on clean run). Source: `02-gdd/01-core-loop.md`.
- **Median player carrots per run: ~400** (100-300 base, up to 2500 with build). Source: `02-gdd/08-economy.md`.
- **Median player soul shards per run: 4-10** (vertical slice spec). Source: `02-gdd/08-economy.md`.
- **Max concurrent enemies: 200** (hard perf cap). Source: `brave-bunny/CLAUDE.md`.

## TTK ladder targets

The Time-To-Kill ladder is the spine of every weapon tuning decision. Targets are written in **seconds-to-kill at mid-build** (player at character level 10 + weapons at level 3). All values for **Bunny** (calibration anchor); other characters fall within ±25% per their `dmg_mult`.

| Enemy archetype | Min 1 (intro) | Min 3 (build) | Min 5 (mid) | Min 7 (pre-boss) | Min 10 (extended) |
|---|---|---|---|---|---|
| **Swarmer** | 0.3 s (1 hit) | 0.6 s (1-2 hits) | 1.2 s (2-3 hits) | 2.0 s (3-4 hits) | 3.5 s (5-7 hits) |
| **Tank** | 6 s | 12 s | 18 s | 24 s | 32 s |
| **Ranged** | 2 s | 3 s | 4.5 s | 6 s | 8 s |
| **Elite** | n/a (none) | n/a (none) | 15 s | 22 s | 35 s |
| **Boss** | n/a | n/a | 90 s (mid-boss, post-launch) | n/a | 120 s (end-boss) |

**Why these numbers.** A swarmer at minute 3 should die in **1-2 hits** with a mid-build — this is the "I feel my draft choice landing" beat. If a swarmer needs 4 hits to die at minute 3, the player feels like they're failing the build. If it dies in 1 hit at minute 7, the build is over-tuned and the late-game becomes a victory lap. The ladder is calibrated for **escalating threat at a sustainable kill rate**.

**Sub-cap on swarmer TTK at minute 7.** Even at peak swarm density (~120 swarmers on screen), **kill rate must exceed spawn rate**, or the player is overwhelmed by accumulation rather than threat. Math: `120 enemies × 6 s lifetime = ~20 kills/sec sustained`. Player DPS at mid-build is ~25 hits/sec. The 25% margin is the safety budget; reduce it only if the design wants the "I'm drowning" beat.

## Run-length target

**Median player survives to 7:00 (boss) in 80% of runs.** Median player **beats the boss in 50%** of runs. Skilled-player target: beat boss in 90%, post-boss survival 5+ minutes.

If Monte Carlo (see `06-monte-carlo-notes.md`) shows median survival <70% reaching boss → reduce minute-3 enemy HP or boost starter weapon DMG. If >90% beat boss → the boss is too easy; bump boss HP 15%.

## Run-end currency target

- **Median carrots per run: 200** (200-400 band; the GDD economy doc says ~400; we calibrate lower for the **median** because the high-end of the band is build-dependent).
- **Median soul shards per run: 5** (within the 4-10 GDD band).
- **Stars per run: 0** (Stars are not earned in-run; they come from daily streak, achievements, BP).

Out-of-run rolling targets:

- **Daily F2P income:** ~1200 carrots, ~12 stars, ~50 soul shards (`02-gdd/08-economy.md`).
- **Time to max one character (L30):** ~30 runs, ~10 days. (Cumulative carrot cost: ~31,000.)

## Anti-bloat law

**Max DPS at character L30 = 40x DPS at L1.** Not 400x, not 4000x. This is the structural defense against the Survivor.io late-game where one tap clears the screen.

Math budget:

| Component | Multiplier band |
|---|---|
| Weapon-level scaling (L1 → L5) | up to **8x** (per `04-weapons.md`, every 2 levels doubles effective DPS) |
| Evolution unlock (L5 base + L5 charm) | up to **2x** vs L5 base (so cumulative up to 16x) |
| Character `dmg_mult` × character level perks (L1 → L30) | up to **1.25x** |
| Crit (default rate × default damage) | **1.05x** expected (no big swings without dedicated passive) |
| Cumulative ceiling | **~40x** |

**Power-of-2 rule.** Each rarity level (or full weapon-level) doubles effective DPS. **No more.** If a new passive proposal triples DPS at one rarity, reject — it breaks the curve.

## Weapon balance band

At weapon level 5, **all 12 weapons must fall within a ±20% DPS band**. The DPS ceiling is ~ same for all weapons; the floor differs only by 20%. No dominant weapon. Builds differentiate on **synergy + identity**, not raw DPS lead.

How to enforce: see `03-weapon-tuning.md` per-weapon DPS table at L1..L5. If any weapon's L5 DPS column falls outside `[0.8 × median, 1.2 × median]`, re-tune that weapon's `dmg_base` or `rate`.

## Character balance band

At character level 10, **all 8 characters must clear a baseline boss in 90-110 s** (10% band). The DPS-archetype characters (Fox, Hedgehog) sit at the **lower bound** (faster kill = 90 s); the tank/sustain (Tortoise, Panda) sit at the upper bound (~110 s). Owl is calibrated to sit in the middle at L10 but **scales higher** at L30 via XP throughput — see below.

## Owl-scaling watch (cross-check)

**Flagged at wave-2 review** by game-designer: Owl's 4x magnet + 15% XP-per-gem combine multiplicatively to effective +27.8% XP throughput vs. baseline (assuming baseline pickup rate ~90% of gems on screen; Owl pickups ~100%; gem value 1.15x → `(100/90) × 1.15 = 1.278`).

**Threshold:** if Owl over-DPS by >15% at L25+ vs the median character, scale down. Current model puts Owl at +27.8% XP throughput → **expected over-DPS of ~+10% at L25 and ~+18% at L30** (because Owl reaches L30 faster than peers).

**Fix.** In `characters.json` set `owl.xp_gem_value_bonus = 0.10` (reduced from designed 0.15) **and** `owl.magnet_mult = 3.0` (reduced from 4.0). Combined effective XP throughput = `(100/95) × 1.10 = 1.158` = +15.8%, just on the threshold. Re-run Monte Carlo before merge.

**Alternative fix considered.** Leave magnet at 4.0 but cap XP-per-gem at +5%. Rejected — magnet is Owl's identity ability, and the larger the magnet, the more visible the difference.

## Reroll budget

- Per `08-economy.md`, extra-banish is a **rewarded-ad surface**. Balance budget: **max 1 extra-banish per run**, hard cap (no stacking from multiple ads in a session).
- Free re-roll: 1 per draft event after level 5.

## Enemies-per-minute density vs perf

| Minute | Target concurrent | Hard cap | Headroom |
|---|---|---|---|
| 1 | 10 | 200 | 190 |
| 3 | 40 | 200 | 160 |
| 5 | 74 (incl. mid-boss adds) | 200 | 126 |
| 7 | 120 | 200 | 80 |
| 10 | 178 | 200 | **22** |

The minute-10 row is **the danger zone**. If anything pushes density above 178 at min 10, gameplay breaks (frame drops, AI degrades, hitboxes miss). Re-pace before adding any new enemy variant past launch.

## Cross-references

- `00-formulas.md` — math layer.
- `02-character-tuning.md`, `03-weapon-tuning.md`, `04-enemy-tuning.md`, `05-economy-tuning.md` — per-system tables.
- `06-monte-carlo-notes.md` — simulation walkthrough that validates these bands.
- ADR-0003 — feel timings (locked, not subject to balance shifts).
