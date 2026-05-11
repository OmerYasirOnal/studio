# Positioning — Brave Bunny

> Where brave-bunny sits in the action-roguelite landscape, and what makes it different from the three deconstructed competitors.

## One-line positioning

> *"Survivor.io's combat with Crossy Road's smile."*

A no-energy, cartoon-mascot, build-crafting action-roguelite for players who want the Habby auto-battler loop without the grim aesthetic or the pay-to-win taste.

## Unique value proposition (UVP)

1. **Cartoon low-poly mascot register** — the only top-grossing-tier auto-battler with a saturated, animal-led look. Family-safe, TikTok-screenshot-friendly, kid-tolerable.
2. **No hard-energy gate** — runs are uncapped at launch. We replace energy-pressure with daily-streak bonuses and rewarded-ad-positive surfaces.
3. **No gear gacha** — gear is deterministic. Monetization runs through cosmetics + battle pass + character unlocks + rewarded ads. GAME.md's `no_pay_to_win` is enforced by design, not by promise.

## Feature matrix

|  | brave-bunny | Survivor.io | Vampire Survivors | Archero |
|---|---|---|---|---|
| Camera | Top-down 3/4 | Top-down 3/4 | Top-down 2D | Top-down 3/4 |
| Visual register | Low-poly saturated cartoon | Pixel-realistic, grim | Pixel art, gothic | Stylized 3D, dungeon-themed |
| Run length | 7-10 min | 10-15 min | 30 min free / 5-10 chapter | 1-2 min per room (~10-15 min total) |
| Energy gate | **None** | Soft (per-mode) | None | **Hard (universally disliked)** |
| Auto-attack | Yes | Yes | Yes | Aim + auto-shoot (twin-stick) |
| Level-up draft | 3-of-N | 3-of-N | 3-of-N | 2-of-N |
| Weapon evolutions | Yes | Yes | Yes | No (gear-based instead) |
| Meta-progression | Character unlocks + (planned) runes | Characters + tech tree | Characters + PowerUps | Hero + gear + talents |
| Gear gacha | **No** (deterministic) | No | No | **Yes (criticized)** |
| Battle pass | Yes (planned) | Yes | No | Yes |
| Rewarded ads | Yes (4-6 surfaces) | Yes (extensive) | Minimal | Yes (revive / 2x / chest) |
| Soft-launch markets | TR / PH / ID | TR / PH / ID + others | n/a (global launch) | Global |
| Engine | Unity 6 LTS URP | Unity | GameMaker (mobile via wrapper) | Unity |

## What we take from each competitor

### From Survivor.io
- 7-10 min run length as the pacing anchor
- Weapon evolution recipes as build-crafting depth
- Habby-style monetization triad: Monthly Card + Battle Pass + Growth Fund
- TikTok creator marketing playbook for soft launch

### From Vampire Survivors
- The level-up draft cadence (15-25 draft events per run target)
- Loss-banks-gold dignity loop — every run pays meta-currency
- The reverence for tight, satisfying numbers (no inflated damage bloat)

### From Archero
- Single-stick / touch-only control feel as the genre default
- Rewarded-ad ladder: revive / 2x rewards / daily chest / extra pull
- The lesson: **avoid** hard energy + gear gacha; preserve **rewarded-ad-positive** monetization

## What we explicitly do *not* do

- **No bullet hell density that requires squinting.** brave-bunny's silhouette-readability bar is "playable on iPhone SE 3 at arm's length." If a visual test fails that, it doesn't ship.
- **No realistic gore or skulls.** This is a soft-family-safe game.
- **No login-gated content** beyond a daily check-in for streak bonus.
- **No premium currency packs above $19.99.** Top-end SKU cap for trust.
- **No region-priced "exploit" SKUs** that show different prices in IN vs US (per Apple guideline + brave-bunny brand).

## Differentiation risk matrix

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| "Cute cartoon" tag pre-filters the high-ARPU audience | Med | High | Lean on Crossy Road precedent + global Pixar-tier appeal; rely on TR/PH/ID broader-age audience first |
| No-energy hurts D7 retention (no daily reason to return) | Med | High | Replace with daily streak bonus + limited-time biome rotation; A/B in soft launch |
| No-gear-gacha caps ARPPU vs Archero | High | Med | Battle pass + cosmetic skins + character unlocks compensate; target ARPPU bracket between VS and Survivor.io |
| Habby's own Capybara Go! occupies the cute-mascot lane | Med | High | Differentiate on animal **roster diversity** (8 characters min at launch) and biome variety; faster live-ops cadence than Habby's |
| iOS-first delays SEA Android volume | Low | Med | Android ships within 6 weeks of iOS soft launch |
| Custom-asset production blows schedule | Med | High | CC0 Quaternius Animated Animals + recolor pipeline removes most of the risk (this is the framework's whole bet) |

## North-star and guardrails

| | Metric | Target at vertical-slice |
|---|---|---|
| **North star** | D1 retention | ≥ 40% in TR/PH/ID |
| Secondary | D7 retention | ≥ 20% |
| Secondary | Median run length | 7-10 min |
| Guardrail | Crash-free sessions | ≥ 99.5% |
| Guardrail | Avg session ad load | ≤ 4 ads in 20 min |
| Guardrail | 60 fps frame budget hit rate (iPhone 12) | ≥ 95% |

## Audience pitch (one paragraph)

For Habby-fans who like Survivor.io's loop but bounced off its grim look, and for casual mobile players who used to play Crossy Road or Cat Quest, brave-bunny is a cartoon-mascot auto-battler with the depth of weapon evolutions and the dignity of every-run-pays — no energy, no gear gacha, just sit down for 8 minutes and feel competent.
