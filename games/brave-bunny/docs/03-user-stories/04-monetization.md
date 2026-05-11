# User Stories — Monetization

> Epic: shop, IAP, rewarded ads, battle pass purchase, gifts, starter packs. **CRITICAL**: every story in this file must honor `docs/01-research/03-positioning.md` no-pay-to-win + no-gear-gacha + no-energy-gate constraints. Owner: ux-designer. Consumers: ui-engineer, systems-engineer. Sources cross-referenced: `docs/02-gdd/00-overview.md` (store + battle pass mode contracts), `docs/02-gdd/01-core-loop.md` (revive offer, ad placement), `docs/02-gdd/narrative/00-tone-bible.md` (no aggressive sales copy), `docs/01-research/03-positioning.md` (price caps, no region-priced exploit SKUs).

## Personas referenced

- **Casual Carla** — 32, TR commuter, 5-min sessions
- **Habby-fan Hakim** — 27, PH, ex-Survivor.io, depth-seeker
- **Family Fadia** — 38, ID, plays with toddler watching, low IAP propensity
- **Returning Rina** — 24, churned 7 days ago, returns via push

---

### US-43 Cosmetic shop browsable without IAP friction

**As a** Casual Carla
**I want** to browse the cosmetic shop without being forced through purchase confirms
**So that** I can window-shop and decide later

**Acceptance criteria**
- [ ] Shop opens from Home in ≤ 2 taps; entry button never has a notification badge unless a new free item is available
- [ ] Tapping any item shows a preview screen (item on character or icon for weapons) with price + "Buy" — never auto-confirm
- [ ] Tapping outside the preview or "Back" closes without purchase
- [ ] No item in the shop unlocks combat power or stats; all items are visual-only

**Wireframe links:** TBD (`docs/05-wireframes/43-shop-main.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Positioning enforcement: cosmetic-only. Audit at QA: any shop SKU that affects gameplay stats blocks ship.

---

### US-44 Rewarded ad opt-in with clear reward preview

**As a** Casual Carla
**I want** every rewarded-ad surface to show me exactly what I get before I watch
**So that** I do not waste 30 seconds on a reward I do not want

**Acceptance criteria**
- [ ] Every rewarded-ad CTA shows the reward type + quantity inline (e.g. "Watch ad: +50 gold", "Watch ad: 1 free re-roll")
- [ ] CTA includes a play-icon glyph and the word "Watch" (not "Free!"); no surprise reveals after ad completes
- [ ] If the ad SDK fails (no fill), a friendly fallback shows: `{AD_NO_FILL}: "No friendly sponsor right now. Try again in a bit."`
- [ ] No reward is power-gated to ad-watchers; every ad reward is also obtainable in 1-2 runs of normal play

**Wireframe links:** TBD (`docs/05-wireframes/44-rewarded-ad-cta.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Rewarded-ad-positive but not coercive. Track `rewarded_ad_attach_rate` per surface.

---

### US-45 IAP confirmation shows price, value, and "no auto-renew" status

**As a** Family Fadia
**I want** the IAP confirm screen to clearly show what I am buying, the price, and whether it renews
**So that** I do not accidentally subscribe to something

**Acceptance criteria**
- [ ] IAP confirm displays: item name + visual, price in local currency from App Store / Play Store, "One-time purchase" or "Renews monthly" label
- [ ] Subscription items (Monthly Card) require an additional "I understand this renews" checkbox before the system confirm
- [ ] All prices come from store-server APIs; no hardcoded prices in UI
- [ ] No bundle is sold above $19.99 USD equivalent (positioning cap)

**Wireframe links:** TBD (`docs/05-wireframes/45-iap-confirm.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-43
**Notes:** Per `GAME.md` monetization block — IAP true, no_pay_to_win true. Store SKUs need tech-architect approval.

---

### US-46 No interstitial ads during gameplay — ever

**As a** Habby-fan Hakim
**I want** the game to never show me a forced interstitial ad mid-run
**So that** the combat flow is never interrupted by an ad I cannot skip

**Acceptance criteria**
- [ ] No `AdMob.ShowInterstitial()` call exists in the run code path; lint rule enforces this in `Scripts/Gameplay/`
- [ ] No ad of any type appears between joystick-down and run-end
- [ ] No interstitial appears on the run-end tally; only opt-in rewarded ads (revive, 2x gold)
- [ ] QA test: 20 consecutive runs with no interstitial firing

**Wireframe links:** N/A (anti-pattern enforcement)
**Owners:** systems-engineer, ui-engineer
**Depends on:** —
**Notes:** Positioning explicit. This is a QA blocker — any interstitial in gameplay fails release gate.

---

### US-47 Run-end ad surface is opt-in only with larger decline

**As a** Family Fadia
**I want** the "2x gold" rewarded-ad offer at run-end to have its decline button bigger than its accept button
**So that** I do not accidentally watch ads when I am tired

**Acceptance criteria**
- [ ] Run-end "Double your gold!" modal has 2 buttons: "Watch ad" (≥ 88 pt) and "Take what I earned" (≥ 120 pt, pre-focused)
- [ ] Modal auto-dismisses to decline after 8 s of no input
- [ ] Surface appears at most once per run-end; no re-prompt if declined
- [ ] Accept path: ad → 2x gold animation → tally update; decline path: direct to Home

**Wireframe links:** TBD (`docs/05-wireframes/47-2x-gold-offer.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-18
**Notes:** Identical decline-friendly pattern to US-22 (revive). Per `01-core-loop.md` failure loop.

---

### US-48 Battle pass purchase — one screen, free vs premium clearly split

**As a** Habby-fan Hakim
**I want** the battle pass purchase screen to show exactly which rewards are free vs premium across all 50 tiers
**So that** I can decide whether the premium track is worth $9.99 to me

**Acceptance criteria**
- [ ] BP screen is a single scrollable list of 50 tier rows; each row shows free reward (left) + premium reward (right)
- [ ] Owned premium rewards show in color; un-owned premium rewards show in greyscale with a small lock glyph
- [ ] "Unlock premium pass" CTA appears at the top with a single price (no upsells, no tier-skip packs above $4.99)
- [ ] Free rewards remain claimable regardless of premium ownership

**Wireframe links:** TBD (`docs/05-wireframes/48-battle-pass.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-35
**Notes:** Tier-skip packs cap at $4.99 per positioning brief. No premium-currency-pack above $19.99 anywhere.

---

### US-49 Starter pack offer appears once, then never again

**As a** Returning Rina
**I want** the "starter pack" offer to appear once around the first run-end and never again
**So that** I am not nagged session after session

**Acceptance criteria**
- [ ] Starter pack offers exactly once per profile, at run-end of run #3 (telemetry-validated)
- [ ] Offer modal contains: cosmetic skin preview + soft-currency amount + price + "Maybe later" decline button
- [ ] If declined, the offer never re-surfaces as a modal (still browsable in the shop)
- [ ] Starter pack contents are cosmetic + currency only — never a power upgrade

**Wireframe links:** TBD (`docs/05-wireframes/49-starter-pack.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-43
**Notes:** Tone — `{STARTER_DECLINE}: "Maybe later."`. Positioning: no pay-to-win, even in starter packs.

---

### US-50 Gift inbox — friendly sponsor framing

**As a** Family Fadia
**I want** dev-team gifts to feel like a kindness, not a sales tactic
**So that** the game's voice stays warm even when monetization surfaces appear

**Acceptance criteria**
- [ ] Dev-gift inbox messages use `{IAP_GIFT_BANNER}: "A friendly sponsor sent you a gift."` per tone bible
- [ ] No gift message contains a paid offer disguised as a gift
- [ ] Each gift shows contents + expiry timer; claim is single-tap
- [ ] Inbox UI is identical to mailbox (US-40); gift is just a message type

**Wireframe links:** TBD (`docs/05-wireframes/50-gift-inbox.html`)
**Owners:** ui-engineer, narrative-designer
**Depends on:** US-40
**Notes:** Anti-pattern test: any gift message linking directly to a paid SKU fails review.

---

### US-51 Monthly Card — opt-in subscription with daily rewards

**As a** Habby-fan Hakim
**I want** the Monthly Card subscription to give me a small daily reward and a one-time bonus
**So that** I get steady value across 30 days without paying for power

**Acceptance criteria**
- [ ] Monthly Card SKU: $4.99 / month, auto-renew, cancellable in store
- [ ] Rewards: 1× one-time premium currency bundle + 30× daily gold drops (claimed from Home daily strap)
- [ ] Daily drop never expires; missed days bank up to a 7-day cap
- [ ] Card screen clearly shows "Renews monthly. Cancel anytime in your store." per US-45 requirements

**Wireframe links:** TBD (`docs/05-wireframes/51-monthly-card.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-45
**Notes:** Positioning: Habby triad (Monthly Card + BP + Growth Fund) without the paywall — all cosmetic + soft-currency.

---

### US-52 Growth Fund — milestone-paced cumulative reward

**As a** Casual Carla
**I want** the Growth Fund to give me cosmetic + soft-currency rewards as I hit gameplay milestones
**So that** my one-time $9.99 buy stays valuable for weeks

**Acceptance criteria**
- [ ] Growth Fund SKU: one-time $9.99, unlocks a 10-milestone reward track keyed to gameplay (e.g. "Reach Bunny Lv 10", "Clear 50 runs")
- [ ] Each milestone reward is cosmetic + currency; never a stat upgrade
- [ ] Progress is visible on a Fund screen accessible from Home / Shop
- [ ] Rewards persist forever; no expiry

**Wireframe links:** TBD (`docs/05-wireframes/52-growth-fund.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-43
**Notes:** Positioning: Habby triad. Milestones must be earnable in pure-F2P play within ~3 weeks.

---

### US-53 No region-priced "exploit" SKUs

**As a** All personas (especially Family Fadia in ID)
**I want** the prices I see to be honest store prices, not artificially-inflated region-exploit SKUs
**So that** I trust the game and its publisher

**Acceptance criteria**
- [ ] All SKU prices are pulled from Apple App Store Connect / Google Play Console at runtime; no hardcoded pricing
- [ ] No SKU is "country-locked" to higher prices in specific regions (e.g. IN, ID, PH)
- [ ] Store-server config asserts price-tier parity across TR / PH / ID / global at publish time
- [ ] QA test in soft-launch: verify TR / PH / ID prices match Apple's tier table

**Wireframe links:** N/A (config + audit)
**Owners:** systems-engineer
**Depends on:** US-45
**Notes:** Positioning explicit: "No region-priced exploit SKUs." Brand trust pillar.

---

### US-54 Refund + restore-purchases path is always accessible

**As a** Family Fadia
**I want** a clear "restore purchases" button in Settings for when I switch devices or am charged in error
**So that** I am not stuck contacting support for a basic store action

**Acceptance criteria**
- [ ] Settings screen has "Restore purchases" button under an "Account" section
- [ ] Tap triggers the platform-native restore flow (App Store / Play Store)
- [ ] On success, confirmation modal lists restored SKUs and updates entitlements immediately
- [ ] On failure, copy explains why ("No purchases found on this account") and offers a "Contact us" mailto link

**Wireframe links:** TBD (`docs/05-wireframes/54-restore-purchases.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-45
**Notes:** Apple App Store guideline requirement; also a trust-building UX win.

---

**Total stories in this file: 12**
