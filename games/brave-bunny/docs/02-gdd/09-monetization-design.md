# GDD 09 — Monetization Design

> Design-level monetization spec for Brave Bunny. **No implementation here** — receipts, IAP product IDs, validation, store integration all live in `06-tech-spec/` (tech-architect) and `unity/Assets/Scripts/Systems/` (systems-engineer). This doc owns the **SKU ladder, subscription stack, rewarded ad surface design, battle pass shape, and the explicit "what we won't sell" list.** Sister docs: `08-economy.md` (currency model + no-P2W audit hook), `02-meta-loop.md` (battle pass tier rewards), `03-positioning.md` (no-paywall positioning weapon), `01-research/02-competitors/05-capybara-go.md` (Habby subscription stack lessons), `00-overview.md` (pillar 3 — dignity-by-design).

## Design philosophy

Three rules, in priority order:

1. **No structural pay-to-win.** Per `08-economy.md` "no-pay-to-win enforcement" — Stars never level characters, never buy gear, never buy gacha pulls. Stars only **unlock content** (characters, premium battle pass, cosmetic skins) and grant **throughput** (Founder Pass +5%, Run Bonus Card +20% rewards) — never raw combat stats.
2. **Subscription stack is opt-in throughput, not a paywall.** Per `01-research/02-competitors/05-capybara-go.md` lesson 3, we steal Habby's stack idea but **cap at 3 simultaneous products** (Habby has 6). A new player can ignore every subscription and still unlock the full 8-character roster in 6-8 months.
3. **Rewarded ads are a feature, not a tax.** 4-6 player-positive surfaces per `00-overview.md` differentiation bullet 10. The session ad load cap is **4 ads per 20 minutes** (per `00-overview.md` guardrail). Ads never block, never auto-play, never interrupt a run.

## IAP catalog (SKU ladder)

Anchor SKU is **$0.99 → 50 Stars** (Starter Sprout). Hard cap is **$19.99** (Founder Pass / Hearty Hamper tier). No SKU above $19.99 per `03-positioning.md` "what we explicitly do not do" §4.

### Stars packs (one-time / repeatable)

| SKU | Price | Stars | Bonus | Stars/$ | Notes |
|---|---|---|---|---|---|
| Starter Sprout | $0.99 | 50 | — | 50.5 | Anchor SKU. Always-available. |
| Carrot Basket | $4.99 | 300 | +20% first-time bonus (60 extra) → 360 first purchase | 60.1 (72.1 first) | Best mid-tier; common buy. |
| Hearty Hamper | $9.99 | 700 | +1 random uncommon cosmetic skin shard | 70.1 | Cosmetic-flavored mid-high tier. |
| Founder Pass (lifetime) | $19.99 | 1800 | +permanent cosmetic frame, +permanent +5% all reward currency throughput, "Founder" name tag | 90.0 | **One-time, launch-window only**. Top SKU. |

### Cosmetic-bundle SKUs (rotating, time-limited)

| SKU | Price | Contents | Cadence |
|---|---|---|---|
| Hero Spotlight Bundle | $4.99 | 1 character's premium skin + matching weapon skin + 100 Stars | Monthly rotation, aligns with `02-meta-loop.md` monthly hero spotlight |
| Biome Drop Bundle | $4.99 | 5 character skins recolored for that biome + 1 emote | Per new biome unlock event |
| Seasonal Bundle | $9.99 | 8-character outfit set + 4 emotes + 200 Stars | Quarterly |

### Anti-pattern SKUs we do not ship

- **No "double-XP for 7 days"** (would warp the upgrade ladder per `08-economy.md` no-P2W §2 — Stars cannot level characters even indirectly).
- **No "10× pull packs"** (no gacha exists; nothing to multi-pull).
- **No "instant-finish boss"** (skipping content is not a monetization surface).
- **No "premium currency mega-pack" above $19.99.**
- **No "starter pack with a +10% DMG charm."** Stats are not purchasable.

### IAP first-time-buyer hook

Every Stars pack offers a **+20% bonus on first purchase** (one-time per SKU per account). This is the **only** time-limited / one-shot price treatment in the catalog. No flash sales, no countdown timers, no fear-of-missing-out price drops beyond the standard first-purchase bonus and the Founder Pass launch window.

## Subscription stack (3 simultaneous products max)

Per `01-research/02-competitors/05-capybara-go.md` lesson 3 — Habby ships 6 simultaneous subscriptions (Monthly + Ad-Free + Auto-Mine + Lifetime + 2 funds + 4 passes). That stack is monetization-effective but **player-hostile**: the "every microtransaction hook in the books" review complaint is the predictable cost. Brave Bunny ships **3 simultaneous products** so the choice is legible.

### 1. Monthly Bunny Card — $4.99 / month (recurring)

| Field | Value |
|---|---|
| Price | $4.99 / month |
| Renewal | Auto-renew (player-cancellable per platform rules) |
| Daily Stars drip | 10 Stars / day claimable (300 Stars / month total) |
| Ad-free | All interstitial ads removed; rewarded ads remain opt-in (player-positive surfaces) |
| Cosmetic | Permanent "Monthly Member" cosmetic frame around player avatar |
| Value vs raw Stars | 300 Stars at Carrot Basket rate = ~$5.00 → **Monthly Card is net-zero on Stars alone**, the ad-free + frame are the bonus |
| Hook | "Skip the ads, drip the Stars." |

### 2. Run Bonus Card — $4.99 / month (recurring)

| Field | Value |
|---|---|
| Price | $4.99 / month |
| Renewal | Auto-renew |
| In-run effect | **+20% Carrots earned per run, +20% XP earned per run** for the subscription period |
| In-combat effect | **None** (does not affect DMG, HP, CRIT, MOVE — preserves no-P2W structural promise) |
| Cosmetic | "Bonus Bunny" avatar frame |
| Value vs raw Carrots | A typical daily player earns ~1200 Carrots / day; +20% = ~240 extra Carrots / day = ~7200 / month — enough to fully meta-level a character ~25% faster |
| Hook | "Throughput, not power." |

### 3. Founder Pass (lifetime, one-time) — $19.99

| Field | Value |
|---|---|
| Price | $19.99 (one-time, lifetime) |
| Stars granted | 1800 (one-shot) |
| Permanent effect | **+5% all reward currency throughput, forever** (Carrots, Soul Shards, battle pass XP — does NOT affect in-run combat math) |
| Permanent cosmetic | "Founder" name tag + exclusive avatar frame + Bunny "Founder" skin variant |
| Window | **Launch + 90 days only**, then retired forever (the founder-window scarcity is the only fear-of-missing-out mechanic we ship) |
| Value vs raw Stars | 1800 Stars at Carrot Basket rate = ~$30 → Founder Pass is the best per-Star value at lifetime |
| Hook | "Founders only. Once." |

### The 3-product cap is structural

A new product cannot ship without first **retiring or replacing an existing one**. ADR-required. This prevents subscription-creep — the player should always be able to see the entire subscription stack on one screen without scrolling.

## Battle Pass

Per `02-meta-loop.md`: **30 tiers × 4 weeks** (one season). Free + premium tracks. **Only one pass live at a time** — not Capybara Go's 4 parallel passes.

| Field | Value |
|---|---|
| Length | 30 tiers / 4 weeks |
| Free track | Carrots, cosmetic shards, occasional Stars, character unlock at tier 30 (rotates per season) |
| Premium track | All free + premium skin at tier 5, premium weapon skin at tier 20, premium-exclusive character skin at tier 30 |
| Premium price | $9.99 OR 800 Stars (Stars-for-pass reinforces non-paywall promise — per `02-meta-loop.md`) |
| Tier skip | 30 Stars per tier, capped at 10 skips per season |
| Pass-XP earn | Tied to run length + daily missions completion (calibrated by balance-engineer in `data/balance/battle-pass.json`) |
| Rollover | No tier rollover between seasons; unclaimed rewards forfeit at season end (player must claim during season) |

### Pass design rules

1. **Tier 30 free-track always grants a character unlock** (rotates: Panda S1, Otter S2, Badger S3 per `02-meta-loop.md`). The pass is a **second earn path** for every character.
2. **Premium track rewards are cosmetic + accelerated content access**, never raw power.
3. **No "pay to skip the entire pass."** The 10-skip cap is the structural limit.

## Rewarded ad surfaces (4-6 surfaces, all player-positive)

Per `00-overview.md` differentiation bullet 10 — "ads as a feature, not a tax." 4-6 surfaces, never blocking, always with a clear player benefit. Session cap of **4 ads / 20 minutes** per `00-overview.md` guardrail.

| # | Surface | What the player gets | What we get | Session frequency cap |
|---|---|---|---|---|
| 1 | **Revive** (on-death modal) | One-time revive at the dropped run, restores 50% HP, keeps current build | Ad impression + retention save | 1 per run |
| 2 | **Double end-rewards** (post-run summary screen) | 2× Carrots + 2× Soul Shards from the just-ended run | Ad impression + per-run engagement | 1 per run |
| 3 | **Daily chest** (home screen) | 1 daily reward bundle: ~50 Carrots + chance of 1 Star + chance of 1 cosmetic shard | Ad impression + daily check-in | 1 per UTC day |
| 4 | **Free pull** (cosmetic shard shop) | One free common cosmetic shard pull / day | Ad impression + cosmetic engagement loop | 1 per UTC day |
| 5 | **Magnet boost** (pre-run loadout) | Pickup magnet radius +50% for the next run only | Ad impression + run-frequency boost | 1 per run |
| 6 | **Extra-banish** (in-run, on draft screen) | One free draft re-roll mid-run (the draft normally allows 0 re-rolls) | Ad impression + in-run engagement | 1 per run |

**Total surfaces: 6.** All are player-positive. None of these gate progression — every reward they provide is also earnable through normal play within the same week.

### Ad-load contract (hard limits)

- **4 ads per 20-minute session maximum** (per `00-overview.md` guardrail).
- **0 interstitial ads ever.** Per Capybara Go contrast in `01-research/02-competitors/05-capybara-go.md` — Habby uses light IAA mix with no interstitials; we match that posture from day 1.
- **All ads are rewarded, all are opt-in.**
- **Monthly Bunny Card removes the "ad-required" prefix on the daily chest** (claim still requires a tap; the ad watch is skipped).

## Habby-style subscription stack reference

Per `01-research/02-competitors/05-capybara-go.md` — Habby's 6-product stack:
1. Monthly Card ($5/mo)
2. Ad-Free Card ($10 lifetime)
3. Auto-Mine Card ($10/mo)
4. Lifetime Card ($30 one-time)
5. Talent Fund / Growth Funds ($20-$30)
6. 4 parallel Battle Passes ($10 each)

**Brave Bunny's 3-product stack maps to Habby's stack as:**
- Monthly Bunny Card = Habby Monthly Card **+** Habby Ad-Free Card (combined).
- Run Bonus Card = Habby Growth Fund **but cosmetic/throughput only, no power**.
- Founder Pass = Habby Lifetime Card **with stricter scarcity (launch + 90 days only)**.

We do not ship Auto-Mine (no idle layer to mine in), do not ship parallel passes (one pass at a time), do not ship power-funds.

## "What we won't sell" — enumerated

Per `03-positioning.md` + `08-economy.md` "banned monetization surfaces":

1. **No raw stat-buff IAP** (no "+10% DMG", "+200 HP", "+5% CRIT" purchase).
2. **No gacha pulls for gear** (gear is deterministic — per UVP 3).
3. **No timer skip on energy** (no energy exists — per UVP 2).
4. **No region-priced exploit SKUs** (all regional pricing follows Apple/Google standard conversion — per positioning §5).
5. **No SKU above $19.99** (top-end cap for trust — per positioning §4).
6. **No pet / mount gacha** (we explicitly differentiate from Capybara Go on this).
7. **No boss-skip / chapter-skip purchases** (content cannot be bought past).
8. **No starter pack with stat charms** (cosmetics + Stars only in any starter pack).
9. **No 4-parallel-battle-passes** (1 pass at a time; the only pass).
10. **No interstitial ads** (rewarded only).
11. **No subscriptions above 3 simultaneous products** (structural cap).
12. **No fear-of-missing-out flash sales** (the only time-limited offers are first-purchase-bonus and the Founder Pass launch window).

## Monetization risk matrix

Cross-referenced with `03-positioning.md` no-paywall pillar. Risks specific to monetization design — broader risks aggregate in `13-risks-and-cuts.md`.

| # | Risk | Likelihood | Impact | Cross-ref | Mitigation |
|---|---|---|---|---|---|
| 1 | No-gear-gacha caps ARPPU vs Archero / Capybara Go | High | Medium | `03-positioning.md` risk matrix row 3 | Battle pass over-performs per payer; Founder Pass scarcity drives one-time spend; Hero Spotlight Bundle monthly cadence |
| 2 | Run Bonus Card's "+20% Carrots" feels like soft P2W to vocal players, even though it's throughput-only | Medium | Medium | `08-economy.md` no-P2W audit | Crystal-clear store copy: "Throughput, not power." In-store badge: "Does not affect combat." Press kit explicitly addresses the line. |
| 3 | Founder Pass launch-window scarcity reads as "manipulative FOMO" to anti-FOMO audience | Low | Medium | `00-overview.md` pillar 3 | Honest framing in store: "Founders only. Once. Here's why." 90-day window is generous, not a 7-day pressure cooker. |
| 4 | Subscription stack of 3 still feels "too much" for the no-paywall positioning brand | Low | High | `03-positioning.md` UVP 3 | Structural 3-product cap is the differentiator vs Capybara Go's 6. Marketing copy leads with "one pass at a time, max three subs." |
| 5 | Rewarded-ad surface fatigue if all 6 surfaces fire daily | Medium | Low | `00-overview.md` guardrail (4 ads / 20 min) | Per-surface cap (most are 1/run or 1/day), session cap of 4/20min, and Monthly Card removes the daily-chest ad — escape valve exists. |

## Cross-references

- Currency model + no-P2W audit hook: `08-economy.md`.
- Battle pass tier-by-tier rewards: `02-meta-loop.md` (source of truth for tier rewards; this doc owns the price + structure).
- Character unlock costs: `02-meta-loop.md` + `03-characters.md`.
- No-paywall positioning weapon: `03-positioning.md`.
- Capybara Go 6-product stack contrast: `01-research/02-competitors/05-capybara-go.md`.
- Pillar 3 (dignity-by-design): `00-overview.md`.
- Implementation (IAP integration, receipt validation, store glue): `06-tech-spec/` (tech-architect, to be authored) + `unity/Assets/Scripts/Systems/` (systems-engineer).
- Risk reconciliation across all systems: `13-risks-and-cuts.md`.
