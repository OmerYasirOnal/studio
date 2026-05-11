# GDD 08 — Economy

> Design-level currency, source, sink, and SKU-ladder spec for Brave Bunny. Owner: game-designer. **Numbers in this doc are design intent (ranges + ladders); the exact tuning values live in `data/balance/economy.json` (balance-engineer owns).** Per `CLAUDE.md` principle 6, no magic numbers in scripts. Sister docs: `02-meta-loop.md` (currency baseline + earn rates), `09-monetization-design.md` (IAP catalog + subscription stack), `03-characters.md` (unlock costs mirror), `00-overview.md` (`no_pay_to_win` pillar).

## Design philosophy

Three principles drive every economy decision:

1. **Every death pays.** Per `00-overview.md` pillar 3, no run is wasted. Carrots and Soul Shards bank on death; only Stars are not earned in-run.
2. **One sink per currency.** A player should never have to ask "what is this for?" Each currency has a **single primary sink** and at most one secondary; ambiguous economies bleed engagement.
3. **No raw-power purchase.** Cosmetics and quality-of-life only. Damage multipliers, HP ceilings, and crit-chance are **never on a price tag** — only on a play-time ladder. This is the structural enforcement of the `no_pay_to_win` flag.

## Currency model

Three currencies. Two earned in-game (Carrots, Soul Shards); one earned in-game and via IAP (Stars).

| Currency | Type | Earned from | Primary sink | Secondary sink |
|---|---|---|---|---|
| **Carrots** | Soft gold | Every run (100-300 base per run, up to ~2500 with build); daily streak (100-400/day); achievements (50-500 per achievement); shop fallback for Soul Shards | Character meta-level upgrades (perma stats; level 1-30 ladder) | Cosmetic shop (common skins, color variants); daily-streak fills |
| **Stars** | Premium | IAP packs ($0.99 → 50 Stars baseline); Battle Pass (5/tier premium, sparse on free); rare rewarded-ad chest (1 per day cap, 1-3 Stars); achievements (1-5 per achievement) | Character unlocks (200-1500 Stars ladder, 8-character roster) | Battle Pass tier skip (30 Stars per tier, capped at 10/season); rare cosmetic skins; founder bundle |
| **Soul Shards** | Run-banked | Elite kills (1-3 per elite); boss kills (10-30 per boss); event rewards (5× rate in Boss Rush event) | **Rune system (v1.1 post-launch)** — at launch banks visibly with **no spend** | **Interim cosmetic exchange** — convert Soul Shards → Carrots at fixed 1:50 rate at the wallet screen (mitigates "bait-and-switch" risk per `13-risks-and-cuts.md` item 5) |

### Carrot earn baseline

- Average run: **100-300 Carrots** from base clears + 50-200 from achievement triggers + up to ~2000 from full-build runs with multiplier passives → **mean ~400 Carrots per session** at calibration baseline.
- Daily-streak ladder: 100 / 150 / 250 / 0 / 0 / 400 / 0 (Carrots on days 1, 2, 3, 6 — other days reward Stars/shards per `02-meta-loop.md`).
- A skill-floor player should fully meta-level a character (1 → 30) in **~30 runs / ~5 hours / ~10 days** at average earn rate.

### Stars earn baseline

- Average F2P earnings: **8-12 Stars/day** (achievements + battle pass drip + rare ad chest + daily streak day-4/6/7).
- IAP base SKU: **$0.99 → 50 Stars** (anchor; per `09-monetization-design.md` catalog).
- F2P player unlocks the full 8-character roster in **~6-8 months** without spending; a spender hits the same milestone in days.

### Soul Shards earn baseline

- Elite drop: **1-3 Soul Shards per elite** (~1-2 elites per run pre-min-10, ~4-6 elites in extended runs).
- Boss drop: **10 Soul Shards mid-boss, 30 Soul Shards end-boss** (revised upward from `02-meta-loop.md`'s 3/5 baseline — `02-meta-loop.md` to be reconciled; the higher numbers here account for the interim Carrot-conversion exchange rate).
- Per run average: **~4-10 Soul Shards** banked.
- Wallet exchange rate at launch: **1 Soul Shard → 50 Carrots** at the wallet screen, max 200 Shards exchanged per day (caps player's ability to dump the entire bank in one sitting; preserves rune-spend hook).

## Sources / sinks tables

### Sources by frequency

| Source | Avg per run | Per session (3 runs) | Per day (full daily routine) |
|---|---|---|---|
| Carrots from clears (base + drops) | 200 | 600 | 600 |
| Carrots from achievement triggers | 50 (sporadic) | 50 | ~75 (with daily missions) |
| Carrots from daily-streak claim | n/a | n/a | ~150 avg |
| Carrots from daily missions (×3) | n/a | n/a | ~400 |
| Stars from battle pass tier-up | n/a | 5 (per tier) | 5-15 (depending on pass progress) |
| Stars from achievement triggers | n/a | sporadic | 1-3 |
| Stars from rare ad chest | n/a | n/a | 1-3 (1 chest/day cap) |
| Stars from IAP | n/a | n/a | by SKU (see `09-monetization-design.md`) |
| Soul Shards from elites | 4 | 12 | 12 |
| Soul Shards from bosses | 10-30 | 30-60 | 30-60 |

**Total daily F2P income (typical session):** ~1200 Carrots + ~12 Stars + ~50 Soul Shards.

### Sinks by frequency

| Sink | Cost | Frequency |
|---|---|---|
| **Character upgrade (1 level)** | 50 Carrots → 1500 Carrots (linear ramp across levels 1-30) | progressive; primary Carrot sink |
| **Character unlock — Tortoise** | 200 Stars | one-time |
| **Character unlock — Hedgehog** | 400 Stars | one-time |
| **Character unlock — Fox** | 600 Stars | one-time |
| **Character unlock — Otter** | 800 Stars | one-time |
| **Character unlock — Panda** | 1000 Stars | one-time |
| **Character unlock — Badger** | 1200 Stars | one-time |
| **Character unlock — Owl** | 1500 Stars | one-time |
| **Cosmetic skin (common)** | 50 Carrots | one-time |
| **Cosmetic skin (uncommon)** | 200 Carrots | one-time |
| **Cosmetic skin (rare)** | 200 Stars | one-time |
| **Cosmetic skin (legendary)** | 500 Stars | one-time, rotating |
| **Battle pass tier skip** | 30 Stars per tier (cap 10/season) | sparingly |
| **Loadout slot 3 unlock** | 750 Stars OR Battle Pass S3 premium | one-time |
| **Founder Pass (lifetime)** | $19.99 / 1800 Stars | one-time, launch window only |
| **Soul Shard → Carrot exchange** | 1 Soul Shard → 50 Carrots (cap 200/day) | interim, removed when rune system ships |

### Character upgrade cost curve (the primary Carrot sink)

The level 1 → 30 cost curve is **linear** to avoid the late-game wall pattern that punishes casual returners. Approximate:

| Level | Carrot cost | Cumulative |
|---|---|---|
| 1 | 50 | 50 |
| 5 | 200 | ~700 |
| 10 | 450 | ~3000 |
| 15 | 700 | ~7000 |
| 20 | 950 | ~13000 |
| 25 | 1225 | ~21000 |
| 30 | 1500 | ~31000 |

Total to max one character: **~31,000 Carrots** (~30 runs at 1000 Carrots/run with build, ~10 days at typical daily routine). Calibrated so a player can max **2-3 characters in the first 30 days** — enough to feel progression, not enough to short-circuit it. Exact per-level costs live in `data/balance/economy.json`.

## SKU price ladder (cross-reference to `09-monetization-design.md`)

| SKU | Price | Stars granted | Stars per dollar |
|---|---|---|---|
| Starter Sprout | $0.99 | 50 | 50.5 |
| Carrot Basket | $4.99 | 300 | 60.1 |
| Hearty Hamper | $9.99 | 700 | 70.1 |
| Founder Pass (lifetime) | $19.99 | 1800 + permanent cosmetic + permanent +5% all rewards | 90.0 (best value, one-time) |

**Hard cap: no SKU above $19.99.** Per `00-overview.md` differentiation bullet 10 and `03-positioning.md` "what we explicitly do not do" section 4.

**No region-priced exploits.** Per `03-positioning.md` differentiation bullet 5: all regional pricing follows Apple/Google standard regional conversion. No special TR / PH / ID lift-up or push-down beyond platform default.

## No-pay-to-win enforcement

### Allowed monetization surfaces (cosmetic + quality-of-life only)

| Surface | Sells | Why it's allowed |
|---|---|---|
| Character cosmetic skins | Visual only | No stat change; visible-only |
| Weapon cosmetic skins | Visual only | No DPS change; muzzle-flash recolor |
| Emotes | Visual only | Social signaling; no run effect |
| Founder Pass +5% all rewards | A reward-rate multiplier, not a stat | Affects **out-of-run currency throughput only**; in-run combat math is untouched. Maps to "throughput" lever per Capybara Go! lesson 3 (`01-research/02-competitors/05-capybara-go.md`). |
| Run Bonus Card (+20% Carrots, +20% XP for 30 days) | Throughput | Same justification — accelerates meta unlock pace, not in-run combat |
| Auto-collect range expansion (post-launch quality-of-life cosmetic) | Quality-of-life | Reduces tedium; not a power lever — the player would have collected the same gold by walking over it |
| Extra-banish (rewarded ad surface: reroll a draft option mid-run) | Quality-of-life | Re-roll already exists as a draft mechanic; the ad adds 1 extra; ceiling on power is the same |

### Banned monetization surfaces (the structural promise)

| Forbidden surface | Why we will not ship it |
|---|---|
| **Raw stat-buff IAP** (sell +DMG, +HP, +CRIT directly) | Inverts the no-P2W promise; immediate breach. |
| **Gear gacha** | Per `00-overview.md` differentiation bullet 3 and `03-positioning.md` UVP 3 — deterministic gear is the brand. |
| **Energy timer skip** | No energy exists per `00-overview.md` differentiation bullet 2. Cannot sell a skip for a meter that does not exist. |
| **Pet/mount gacha** | Per `01-research/02-competitors/05-capybara-go.md` differentiation table — Capybara Go has this; we explicitly do not. |
| **Region-priced exploit SKUs** | Per `03-positioning.md` "what we do not do" §5 + brand trust commitment. |
| **SKUs above $19.99** | Per `00-overview.md` differentiation bullet 10. |
| **Boss-skip purchases** | Skipping content is not a monetization surface; bosses are the cap of a biome run. |
| **Character power level via Stars** | Stars unlock the character; **Carrots earned in-game level the character.** Stars cannot be used to level a character. This is the cleanest possible separation between "buy access" (OK) and "buy power" (banned). |

### Audit hook for monetization-spec author

Every new monetization surface proposed post-launch must answer this 3-line audit:

1. Does it grant a stat that affects in-run combat math (DMG, HP, CRIT, MOVE)? **If yes → reject.**
2. Does it grant a currency that levels a character or upgrades a weapon? **If yes → reject (use Carrots, which are earned in-run only).**
3. Is it purely cosmetic, throughput, or quality-of-life? **If yes → review for player-trust impact, then approve.**

Per `13-risks-and-cuts.md` risk 7 (ARPPU ceiling from no-gear-gacha), the battle pass over-performs per payer because the gacha lever is closed; the audit hook prevents drift back toward gacha under revenue pressure.

## Wallet UI requirements (handoff to ui-engineer)

- **3 wallet tiles**, always visible on home screen: Carrots, Stars, Soul Shards.
- Soul Shards tile shows **"Coming Soon: Runes"** tooltip on tap until v1.1 ships (per `02-meta-loop.md` cross-reference); **interim Carrot-exchange button** lives on the wallet detail modal.
- Tap any wallet tile → wallet detail modal: 7-day earn graph, top 3 sinks, current cap if any.
- No pop-up "buy more" prompts at wallet zero state — players who hit zero see the earn-rate panel, not the IAP store.

## Cross-references

- **Currency baseline + daily streak** source of truth: `02-meta-loop.md`. This doc owns ranges and sink ladders, `02-meta-loop.md` owns the streak day-by-day reward.
- **Character unlock costs**: mirrored from `02-meta-loop.md` + `03-characters.md`. **`02-meta-loop.md` is source of truth**; this doc echoes for sink-table completeness.
- **IAP catalog detail** (more SKUs, subscription stack): `09-monetization-design.md`.
- **Battle Pass economics**: `02-meta-loop.md` battle pass section.
- **Exact per-level costs + earn-rate tuning numbers**: `data/balance/economy.json` (balance-engineer owns).
- **No-P2W structural promise**: `00-overview.md` pillar 3 + `03-positioning.md` UVP 3.
