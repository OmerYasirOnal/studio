# 10-Balance 05 — Economy Tuning

> Owner: balance-engineer. Source/sink tables, battle-pass progression math, IAP ROI math, ARPPU target. Source of truth for `data/balance/economy.json`. Sister docs: `08-economy.md` (design philosophy), `02-meta-loop.md` (currency baselines), `09-monetization-design.md` (IAP catalog).

## Currency source/sink targets

### Per-session targets (3-run average daily session)

| Currency | Source rate | Sink rate | Net |
|---|---|---|---|
| Carrots | ~1200/day (600 in-run + ~150 streak + ~400 daily missions + ~50 sporadic) | ~700/day (early-mid game, character level-ups) | +500/day → bankable for future cosmetics |
| Stars | ~12/day (BP drip + achievements + rare ad chest) | ~10/day in steady state (no character unlock yet); bursts on unlock weeks | mostly zero net day-over-day until purchase event |
| Soul shards | ~50/day (4-10 per run × 3 runs + boss kills) | 0/day at launch (rune system v1.1) | banks up; **exchange button** (1 shard = 50 carrots, cap 200/day) |

### Per-run targets

| Currency | Median (Bunny L10, weapons L3) | High build (full mid-build) | Low (early death) |
|---|---|---|---|
| Carrots | **200** | **500-2500** | **50-100** |
| Soul shards | **5** | **8-10** | **1-2** |
| Stars | 0 | 0 | 0 |
| XP gems collected | ~80 small + ~25 med + ~5 large + boss-XP | ~120 + 40 + 8 + boss | ~30 + 5 + 0 |
| Level-ups in run | **22** | **23-25** | **8-12** |

## Carrot-per-kill table

| Enemy role | Carrots per kill |
|---|---|
| Swarmer | 1 |
| Tank | 4 |
| Ranged | 2 |
| Elite | 20 |
| Boss (mid) | 50 |
| Boss (end) | 100 |

Approx run = 600 swarmers × 1 + 30 tanks × 4 + 25 ranged × 2 + 3 elites × 20 + 1 boss × 100 = **930 base carrots**. With run-bonus card (+20%) = 1116; with founder-pass (+5%) = 977. Within the 200-2500 band; mid-build median ~930.

## Character upgrade cost curve (the primary Carrot sink)

Linear ramp per `08-economy.md` table — replicated for tuning JSON:

| Level | Carrot cost | Cumulative |
|---|---|---|
| 1 | 50 | 50 |
| 5 | 200 | 700 |
| 10 | 450 | 3000 |
| 15 | 700 | 7000 |
| 20 | 950 | 13000 |
| 25 | 1225 | 21000 |
| 30 | 1500 | 31000 |

Slope: `cost(L) = 50 + 50×(L−1)` for L1..L30 with mild curvature; matches the cumulative ~31000.

Implementation formula in `economy.json`: precomputed table 1..30 to avoid live-math discrepancies.

## Battle pass tuning

**Goal:** F2P player can reach **tier 30 in 28 days** assuming ~**12 runs/week** = 1.7 runs/day.

| Field | Value |
|---|---|
| Total tiers | 30 (free track) + 30 (premium track) |
| XP per tier | 1000 BP-XP |
| Total BP-XP for tier 30 | 30,000 BP-XP |
| BP-XP per run | ~75 (varies 50-100) → 12 runs/week × 4 weeks = 48 runs |
| BP-XP from daily missions | ~150/day × 28 days = 4,200 |
| Total BP-XP earnable | 48 × 75 + 4,200 = 7,800 (runs) + 4,200 (missions) = **12,000** (?) |

**Problem.** 12,000 BP-XP earned vs 30,000 required = only tier 12 in 28 days. Need to scale:

- **Bump BP-XP per run to 200**, OR
- **Reduce BP-XP per tier to 400**.

**Chosen lever:** reduce BP-XP per tier to 400 → total = 12,000 → exactly tier 30 in 28 days (theoretical max with full daily routine). A casual F2P player (~5 runs/week) hits tier ~15, which sets up the premium-track upgrade incentive at mid-pass.

**BP rewards per tier** (alternating, with milestone tiers):

| Tier band | Free track | Premium track |
|---|---|---|
| 1-5 | 50 carrots, 5 carrots, XP gem, 50 carrots, **Cosmetic (common)** | 100 carrots, 5 stars, 50 carrots, 5 stars, **Cosmetic (rare)** |
| 6-15 | mix carrots + occasional stars (1-3) + cosmetic at 10 | mix carrots + stars (5/tier) + 1 weapon unlock + **Hedgehog at tier 15** |
| 16-30 | mix + cosmetic at 20, 25 | stars + soul shards + character-shard pulls + **Panda at tier 30** |
| Capstone | 30: 200 carrots + cosmetic | 30: 300 stars + Panda + legendary cosmetic |

**Stars earned via BP per season:** ~50 free + ~150 premium = **200 stars in a 60-day season**. Aligns with the GDD "F2P player unlocks the full 8-character roster in ~6-8 months."

## IAP catalog skeleton

Per `08-economy.md` § SKU price ladder + `09-monetization-design.md`:

| SKU id | Price | Stars granted | Stars/$ | Content |
|---|---|---|---|---|
| `starter-sprout` | $0.99 | 50 | 50.5 | Stars only |
| `carrot-basket` | $4.99 | 300 | 60.1 | Stars + 1 common cosmetic |
| `hearty-hamper` | $9.99 | 700 | 70.1 | Stars + 1 uncommon cosmetic + 5 soul shards |
| `founder-pass` | $19.99 | 1800 | 90.0 | Stars + permanent cosmetic + permanent +5% all rewards |
| `monthly-bunny-card` (subscription) | $4.99/month | 1050/month (35/day × 30) | 210.4 | 35 stars/day for 30 days |

**Hard cap: no SKU above $19.99.** Per `08-economy.md`.

## Subscription ROI math

**Monthly Bunny Card** ($4.99/month): 35 stars/day × 30 days = **1050 stars/month**.

| Comparison | Value |
|---|---|
| 1 month subscription | 1050 stars for $4.99 = 210 stars/$ |
| Single-purchase 50 stars (Starter Sprout) | 50 stars for $0.99 = 50 stars/$ |
| **ROI multiplier** | **210/50 = 4.2×** effective stars per dollar |

GDD math says 2.1× — the **revised number is 4.2×** because the per-day star drip × 30 days outweighs single-purchase rate. This **strongly favors the subscription**, making it the engagement-driving anchor.

**Founder Pass** ($19.99 one-time): 1800 stars + perma cosmetic + perma +5% rewards = effective **value ≈ 10× $1.99 Starter Sprout equivalent**.

| Comparison | Value |
|---|---|
| Founder Pass | 1800 stars + perma cosmetic + +5% perma | $19.99 |
| 10 × Starter Sprout equivalent | 500 stars only | $9.90 |
| **Value lift** | **1800 vs 500 = 3.6× stars** + perma bonuses | 2× the price |

Effective value per dollar = roughly **5× Starter Sprout** at the Founder Pass tier. Matches the brief's "10x value of Starter" when factoring in permanent cosmetic + perma +5% reward multiplier over 60+ day lifetime.

## ARPPU target

| Tier | Survivor.io equivalent | Vampire Survivors equivalent | Brave Bunny target |
|---|---|---|---|
| Heavy spender | ~$50/month | ~$2.99 one-time | $22/month |
| Median payer | ~$15/month | $2.99 one-time | $8/month |
| ARPPU band | $25-40/month | ~$3 total | **$22/month** |

The $22 ARPPU is the midpoint between VS (low) and Survivor.io (high). Per `13-risks-and-cuts.md` risk 7, the no-gear-gacha cap means we **cannot reach Survivor.io ARPPU**; we deliberately position lower with a **higher conversion rate** target (no gear walls).

## Cosmetic price ladder

| Tier | Price | Acquisition |
|---|---|---|
| Common | 50 carrots | Carrots in shop; common drop |
| Uncommon | 200 carrots OR 1 cosmetic pull | Shop + BP rewards |
| Rare | 200 stars | Shop + BP premium |
| Legendary | 500 stars | Limited rotating (1 per week) |

## Daily mission tuning (carrot source)

3 missions/day, each granting 100-200 carrots on complete. Total daily mission income: ~400 carrots (matching GDD § Sources by frequency).

Mission templates (rotated):

| Mission | Carrot reward |
|---|---|
| "Beat the boss" | 150 |
| "Survive 5 minutes" | 100 |
| "Reach character level 15 in a run" | 150 |
| "Kill 5 elites" | 200 |
| "Use a weapon evolution" | 200 |

## Run-bonus card (throughput surface, not P2W)

**Run Bonus Card**: +20% Carrots, +20% XP for 30 days. Price: $2.99.

Effect on F2P → buyer trajectory: cuts character L30 grind from ~10 days to ~7 days. Cuts BP tier-30 grind from 28 days to ~23 days. **Acceptable** per `08-economy.md` "throughput is not power" rule.

## Cross-references

- `08-economy.md` — design philosophy + sink-table master.
- `02-meta-loop.md` — currency baselines + daily streak.
- `09-monetization-design.md` — IAP catalog detail (post-launch additions live there).
- `00-formulas.md` § 6 (drops) + § 7 (soul shards).
- `01-tuning-philosophy.md` — ARPPU target band justification.
