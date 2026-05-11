# ADR 0010 — Subscription ROI policy

**Date:** 2026-05-12
**Status:** accepted
**Owner:** orchestrator (synthesizing balance-engineer wave-4 flag)

## Context

Balance-engineer's wave-4 economy tuning (`docs/10-balance/05-economy-tuning.md`) computed the Monthly Bunny Card ROI at **4.2x** vs a single $0.99 SKU (35 stars/day × 30 = 1050 stars for $4.99 ≈ 6.3x the per-star rate of $0.99 → 50 stars). The original GDD `docs/02-gdd/09-monetization-design.md` framed the value prop as "~2.1x". Two reasons the math drifted:

1. The GDD's 2.1x assumed 17 stars/day; balance proposed 35/day to feel more generous
2. The ad-free + cosmetic frame perks were not monetized in the math

Question: at 4.2x ROI, is the Monthly Card overly generous and a margin risk?

## Decision

**Keep the 4.2x effective ROI for Monthly Bunny Card.**

Rationale:

1. **Habby competitor analysis** (`docs/01-research/02-competitors/05-capybara-go.md`) shows subscription ROI in the 3-5x range is standard for the genre; we are competitive, not over-generous
2. **Brand reinforcement** — the "no pay to win" positioning needs to be visibly generous on the *one* monetization product that *is* premium. Wholesome generosity > pinch-pennies on subscriptions
3. **LTV math** — subscribers retain 3-4x longer than non-subscribers (Habby data). 4.2x ROI on a 4-month median subscription life is still 17x LTV vs 6x for non-subscribers, profitable
4. **Soft launch lever** — if soft-launch numbers in TR/PH/ID show subscription ARPU is too low, we'll dial back to 25 stars/day (3x ROI), not push above 35

## Constraints lock

- **Monthly Bunny Card**: 35 stars/day × 30 days = 1050 stars + ad-free + cosmetic frame for $4.99
- **Run Bonus Card**: +20% carrots + +20% XP for 30 days for $4.99 — stacks with Monthly Bunny Card (different lane)
- **Founder Pass (lifetime)**: permanent +5% all rewards + permanent cosmetic for $19.99 — top-end SKU cap

Update `docs/02-gdd/09-monetization-design.md` and `docs/10-balance/05-economy-tuning.md` to reference this ADR as the source of truth.

## Consequences

- systems-engineer's IAP service uses these prices as defaults; A/B testing in soft launch may vary them
- balance-engineer monitors ARPPU during soft launch — if Monthly Card cannibalizes one-shot SKU sales below 30% of subscriber revenue, revisit
- Analytics event taxonomy needs `subscription_start`, `subscription_renew`, `subscription_cancel`, `subscription_lapse` events for ROI tracking
- ui-engineer's IAP confirmation screen shows the subscription's *per-day* equivalent ($0.17/day) prominently

## Alternatives considered

- **Lower to 3x ROI (25 stars/day)** — rejected for v0.1 launch. May adopt post-launch if margin pressure surfaces.
- **Add a $2.99 weekly tier** — rejected. Subscription product proliferation is a Capybara Go! anti-pattern (6 products is too many). Stick to 3.
- **Make Monthly Card include the Run Bonus Card** — rejected. Two clear lanes is clearer for the player; bundling hides value.

## References

- `docs/02-gdd/09-monetization-design.md`
- `docs/10-balance/05-economy-tuning.md`
- `docs/01-research/02-competitors/05-capybara-go.md` (subscription stack analysis)
- `data/balance/economy.json`
