# GDD 13 — Risks and Cuts

> The top-10 risks to Brave Bunny's launch, with mitigations, and the ordered cut-list for when schedule pressure forces tradeoffs. Owner: game-designer (with input from all agents). Sister docs: `GAME.md` (root cut-list — this doc extends it), `03-positioning.md` (differentiation risk matrix — partially mirrored), `09-monetization-design.md` (monetization risk matrix), `00-overview.md` (pillars + scope to cut against), `01-research/02-competitors/05-capybara-go.md` (Capybara Go competitive risk).

## Risk matrix (top 10)

Risks scored **Impact (Low / Med / High)** × **Likelihood (Low / Med / High)**. Top of the list = highest priority for mitigation effort.

### 1. Capybara Go! occupies the cute-mascot lane

| Field | Value |
|---|---|
| Impact | High |
| Likelihood | Medium |
| Cross-ref | `01-research/02-competitors/05-capybara-go.md` (full deconstruction); `03-positioning.md` risk matrix row 4 |
| Mitigation | Lean on **roster diversity** (8 distinct animal heroes per `03-characters.md`) — Capybara Go has 1 hero, brave-bunny has 8 silhouettes. Use 8 characters as TikTok creative leads. Match Habby's live-ops cadence (weekly + monthly drops per `12-content-roadmap.md`). Faster balance cadence (bi-weekly vs Habby's monthly) reads as "indie cares more" — a positioning weapon. |
| Surfacing | Re-evaluate at 30-day post-launch retro: if Capybara Go counter-launches a roster expansion, escalate. |

### 2. iPhone SE 3 doesn't fit 3-card draft horizontally

| Field | Value |
|---|---|
| Impact | High |
| Likelihood | High |
| Cross-ref | ux-designer wave 2 flag (handoff to `04-ux-flows/` + `05-wireframes/`); `GAME.md` target_devices includes iphone-se-3 explicitly |
| Mitigation | Vertical card stack on iPhone SE 3 width (3 cards stacked vertically with thumb-reach optimization). Horizontal layout reserved for iPhone 12+ width and tablet. UI conditionally branches at runtime on safe-area width threshold (375pt or below = vertical). ux-designer to lock the threshold and authoritative wireframe in `05-wireframes/draft-screen.md`. |
| Surfacing | Vertical-slice gate hard requirement: draft screen passes on both iPhone SE 3 and iPhone 12 in physical-device test. |

### 3. Owl character may overscale late

| Field | Value |
|---|---|
| Impact | Medium |
| Likelihood | Medium |
| Cross-ref | game-designer wave 2 flag (per `03-characters.md` Owl row — Far Sight grants 4× magnet + 15% XP value); `10-balance/` (TTK ladder — to be authored) |
| Mitigation | Balance-engineer runs Owl-specific TTK ladder at vertical-slice gate. Specifically validate that Owl + a stacking-XP build does not break the wave-density curve at minute 10. If Owl out-levels by > 1 TTK tier vs Bunny baseline, nerf the +15% XP value before the magnet radius (magnet radius is the more visible / fun half of the signature mechanic). |
| Surfacing | Owl is the 8th and hardest unlock per `03-characters.md` — the small N of Owl-playing players in soft launch means the issue may not surface until post-launch weeks 6+. Telemetry should flag any Owl run where minute-10 character level ≥ 1.5× the median minute-10 level across all heroes. |

### 4. CC0 recolor pipeline blows schedule on Quaternius coverage gaps

| Field | Value |
|---|---|
| Impact | Medium |
| Likelihood | Low (with Blender custom-prop fallback) |
| Cross-ref | `core/docs/asset-policy.md`; `00-overview.md` differentiation bullet 6 (CC0 pipeline is the framework bet); `07-bosses.md` Mama Oak (no animal-pack tree-creature — flagged Blender custom) |
| Mitigation | 7 of 8 characters confirmed from Quaternius Animated Animals (Bunny, Tortoise, Hedgehog, Fox, Otter, Panda, Badger). 8th (Owl) has a coverage option; if not, a Blender custom rig is the fallback (1-2 days of work). Mama Oak boss requires Blender custom either way — pre-allocated in art-director schedule. Maintain a **CC0 sourcing log** so coverage gaps are caught before week 6. |
| Surfacing | Asset audit gate at end of week 3: every launch character + boss has a confirmed CC0 source or a scheduled Blender custom block. Escalate if any character is unfound and unblocked. |

### 5. Soul Shards bank with no v1 spend feels bait-and-switch

| Field | Value |
|---|---|
| Impact | Low |
| Likelihood | Medium |
| Cross-ref | `02-meta-loop.md` Soul Shards launch-state caveat; `08-economy.md` interim Carrot-exchange mechanic |
| Mitigation | Interim cosmetic exchange at the wallet screen (1 Soul Shard → 50 Carrots, cap 200/day per `08-economy.md`). Soul Shard wallet tile shows persistent "Coming Soon: Runes" tooltip. Public roadmap mention of v1.1 rune system on the store page, in the press kit, and in any launch trailer. |
| Surfacing | Monitor app reviews + Reddit / TikTok comments in first 14 days for "what are these things for" sentiment. Two confirmed instances of that complaint in any week → trigger an in-game pop-up clarifying the rune roadmap. |

### 6. D7 retention without an energy gate

| Field | Value |
|---|---|
| Impact | High |
| Likelihood | Medium |
| Cross-ref | `00-overview.md` north-star (D7 ≥ 20%); `02-meta-loop.md` daily streak system; `03-positioning.md` risk matrix row 2; `12-content-roadmap.md` live-ops cadence |
| Mitigation | Daily streak system with 7-day capstone + 2-day skip tolerance (per `02-meta-loop.md` — forgiving by design). Bi-weekly biome rotation events (per `12-content-roadmap.md` weeks 1-2 + 9-10 — Meadow night/harvest variants). Push notification opt-in nudges (per `10-onboarding.md` session-4 deferred prompt). Monthly hero spotlight gives lapsed users a free-trial week. |
| Surfacing | D7 retention is the second-most-important number after D1. Soft-launch dashboard alarms at D7 < 15% (target is ≥ 20%). |

### 7. ARPPU ceiling from no-gear-gacha

| Field | Value |
|---|---|
| Impact | Medium |
| Likelihood | High |
| Cross-ref | `03-positioning.md` risk matrix row 3; `09-monetization-design.md` monetization risk matrix row 1; `01-research/02-competitors/05-capybara-go.md` (gacha-driven ARPPU benchmark) |
| Mitigation | Battle pass over-performs per payer because gacha is closed (the spender's spend is concentrated on pass + Founder Pass + Hero Spotlight Bundles). Founder Pass scarcity (90-day window) drives one-time $19.99 spend. Hero Spotlight Bundle monthly cadence gives a recurring $4.99 surface for engaged spenders. Run Bonus Card ($4.99/month) catches throughput-spenders who would otherwise hit no other surface. ARPPU target bracket: between Vampire Survivors (low) and Survivor.io (high) — explicitly not in Habby/gacha range. |
| Surfacing | Monthly ARPPU vs bracket target at 30-day post-launch retro. If ARPPU < Vampire Survivors benchmark, ADR on adding 1 more cosmetic Bundle SKU (not gacha). |

### 8. Soft-launch market currency volatility (TR) distorts ARPU

| Field | Value |
|---|---|
| Impact | Low |
| Likelihood | High |
| Cross-ref | `03-positioning.md` (TR is priority soft-launch market); `09-monetization-design.md` (no region-priced exploit SKUs) |
| Mitigation | Monitor **PH and ID as cleaner proxies** for ARPU signal — both have more stable currency vs USD than TR. Report ARPU in three buckets in soft-launch dashboard: TR-only, PH-only, ID-only. Cross-reference Capybara Go's KR/TW/JP regional breakouts as a parallel-market sanity check. Resist temptation to region-price up in TR to "fix" the ARPU number — that would breach `03-positioning.md` "what we explicitly do not do" §5. |
| Surfacing | If TR ARPU diverges from PH/ID ARPU by > 2× in either direction for 2 consecutive weeks, escalate to product retro. |

### 9. Habby UA cost in soft-launch markets

| Field | Value |
|---|---|
| Impact | Medium |
| Likelihood | Medium |
| Cross-ref | `01-research/02-competitors/05-capybara-go.md` (Habby scaled to 200k daily installs leveraging organic + meme-leverage); `03-positioning.md` |
| Mitigation | **TikTok creator partnerships before paid UA** — leverage the 8-character roster as creator content (1 character per creator = 8 distinct creator angles, vs Capybara Go's 1-character meme). Build creator-kit assets (character renders, animated short clips, soundtrack snippets) before launch — ready for distribution day 1. Defer paid UA until organic CPI baseline is established; expectation is CPI > Capybara Go's (we don't have the meme) but < genre median if creator playbook works. |
| Surfacing | Track creator-driven install attribution in first 30 days. If creator channel < 5% of installs by day 14, escalate to UA strategy review. |

### 10. Apple Arcade as parallel channel

| Field | Value |
|---|---|
| Impact | Low |
| Likelihood | Low |
| Cross-ref | `GAME.md` priority_platform: ios; `03-positioning.md` |
| Mitigation | **Revisit at vertical-slice gate.** Apple Arcade requires no-IAP, no-ads — totally different monetization shape (Apple pays a guaranteed share). For an no-energy-no-gacha game with strong art bona fides, Arcade is a plausible parallel ship. But it requires a fork in the monetization spec and an Apple business deal. Decision: do not pursue pre-launch; reconsider only if vertical-slice review + Apple business contact aligns. |
| Surfacing | One-time decision gate at vertical-slice review. If declined, do not re-surface for 12 months. |

## Risk-by-impact summary

| Impact tier | Risks | Total |
|---|---|---|
| High | #1 Capybara Go lane, #2 iPhone SE 3 draft, #6 D7 retention | 3 |
| Medium | #3 Owl overscale, #4 CC0 pipeline, #7 ARPPU ceiling, #9 Habby UA | 4 |
| Low | #5 Soul Shards UX, #8 TR currency, #10 Apple Arcade | 3 |

The three highest-impact risks (#1, #2, #6) are owned across game-designer + ux-designer + balance-engineer and must have mitigation evidence at the vertical-slice gate review.

## Cut list (ordered)

Cuts apply in order. **The first cut is the cheapest; the last cut is the most painful.** If schedule pressure forces a cut, take the first remaining unchecked item.

### From `GAME.md` root (re-listed for reference)

1. **Meta-progression beyond character unlocks** — ship with only character-level upgrades, defer rune system to v1.1. (Already planned per `02-meta-loop.md` Soul Shards launch-state caveat — this is the **baseline** plan, not a contingency cut.)
2. **Boss roster beyond 1** — ship vertical slice with only Old Boar King; reuse cosmetically across all 5 launch biomes if launch happens with only 1 boss model.
3. **Localization beyond TR/EN** — drop PH/ID Tagalog/Bahasa, soft-launch with English in those markets (already planned per `narrative/00-tone-bible.md` §6 — PH ships in English at launch).

### Extension cut list (game-designer additions)

4. **Cut new-biome general-availability rollouts past Beach** — if biomes 3 (Forest), 4 (Cavern), 5 (Snow) cannot ship within the launch schedule, ship with Meadow + Beach only. Forest / Cavern / Snow can be a post-launch quarterly drop (per `12-content-roadmap.md` quarterly cadence).
5. **Cut Run Bonus Card subscription** — if subscription stack complexity blows the monetization-spec implementation timeline, drop Run Bonus Card. Monthly Bunny Card + Founder Pass alone is a defensible 2-product subscription stack.
6. **Cut the daily-streak 2-day skip tolerance feature** — if forgiveness-detection mechanism is too complex for launch, ship with hard reset on missed day. UX takes a hit; cut is recoverable in week 1 patch.
7. **Cut Boss Rush monthly event** — highest-effort event surface per `12-content-roadmap.md`. Replace with bi-weekly cosmetic drop cadence (already the floor cadence; no incremental dev-hours required).
8. **Cut Owl character (delay to post-launch v1.1)** — Owl is the 8th unlock (1500 Stars, ~180 days for F2P per `03-characters.md`). Most players will not reach Owl in the first 90 days. Shipping with 7 characters at launch + Owl at v1.1 is structurally fine, especially if Owl's overscale risk (risk #3) is unresolved.
9. **Cut Founder Pass +5% all-rewards permanent multiplier** — ship Founder Pass as cosmetic + Stars-pack only, no permanent throughput multiplier. Reduces no-P2W audit complexity at launch (some players will read the +5% as soft pay-to-win even though it is throughput-only).
10. **Cut 3rd loadout slot (post-launch unlock)** — `02-meta-loop.md` already gates this behind 750 Stars OR Battle Pass S3. If loadout system can only ship with 2 slots at launch, that's fine; the 3rd-slot gate doesn't fire until S3 anyway, so the cut is invisible to launch users.

## Cut-list rule

Take cuts **in order**, top to bottom. Skipping a cut to take a later one requires an ADR. The order is informed by: lowest-cost-first, lowest-player-impact-first, easiest-to-re-add-later-first.

Cuts 1-3 are pre-planned. Cuts 4-10 are **contingency** — only fire under explicit schedule pressure validated at a phase-gate review.

## Risk re-evaluation cadence

- **Weekly during build phase** (phases 3-6): risk matrix re-scored. Any risk moved from Medium → High triggers an ADR.
- **At vertical-slice gate**: all 10 risks must have current mitigation evidence + an owner sign-off.
- **30 / 60 / 90 days post-launch**: retro re-scores all risks; new risks added; closed risks archived.

## Cross-references

- Root cut list: `GAME.md` §Cut list.
- Differentiation risk matrix (overlaps with risks #1, #6, #7): `03-positioning.md`.
- Monetization risk matrix (overlaps with risk #7): `09-monetization-design.md`.
- D1 / D7 retention targets (informs risk #6): `00-overview.md` north-star.
- iPhone SE 3 wireframe contract (risk #2): `05-wireframes/draft-screen.md` (ux-designer, to be authored).
- Owl TTK ladder (risk #3): `10-balance/ttk-ladder.md` (balance-engineer, to be authored).
- CC0 sourcing audit log (risk #4): `07-art-bible/` (art-director, to be authored).
- Soul Shards interim exchange (risk #5): `08-economy.md` wallet exchange rate.
- TR / PH / ID priority markets (risks #8, #9): `03-positioning.md`.
