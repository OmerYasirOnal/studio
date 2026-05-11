# GDD 00 — Overview

> Phase 2 entry doc. Owner: game-designer. Companion docs in this folder cover the core loop, combat, draft, meta, monetization, and feel pillars. The brief for this document is `docs/01-research/03-positioning.md`.

## High-concept (≤150 words)

Brave Bunny is a top-down 3/4 cartoon-mascot **action-roguelite** for iOS and Android. Players steer a brave animal hero through 7–10 minute survival runs in saturated low-poly biomes, swatting ever-thickening swarms with auto-firing weapons. Every level-up offers a 3-of-N upgrade draft; weapons evolve into screen-filling finishers when their synergy ingredients line up. Death banks every coin earned — runs always pay. Between runs, players unlock new animal heroes, push a free + premium Battle Pass, and chase daily-streak bonuses (never an energy meter). The pitch in a sentence: *Survivor.io's combat with Crossy Road's smile.* The thesis: the bullet-heaven shelf is dominated by grim pixel and gothic art; a saturated, family-safe, animal-led look is an open lane in TR / PH / ID and on TikTok — and a CC0-recolor asset pipeline lets one developer ship it in eight weeks.

## North-star metric

| | Metric | Target |
|---|---|---|
| **North star** | D1 retention | **≥ 40%** in soft-launch (TR / PH / ID) at vertical-slice gate |
| Secondary | D7 retention | ≥ 20% |
| Secondary | Median run length | 7–10 min |
| Guardrail | Crash-free sessions | ≥ 99.5% |
| Guardrail | 60 fps frame-budget hit-rate (iPhone 12) | ≥ 95% |
| Guardrail | Session ad load | ≤ 4 ads / 20 min |

All numbers cross-link to `docs/01-research/03-positioning.md` so the GDD doesn't drift.

## Pillars (3)

1. **Saturated joy** — every frame is loud, warm, and cartoon-bright. The game must read as "fun" before the player sees a number. The hard test: a 0.5 s clip at 60 fps should make a stranger smile. Owners: art-director, gameplay-engineer (feel-pillars).
2. **Build crafting depth** — the run is a build. Three random offers per level-up, weapon evolution recipes, and synergy adjacencies turn a 7-minute run into a small puzzle. Owners: balance-engineer, game-designer.
3. **Dignity-by-design (no paywalls)** — no energy gate; no gear gacha; every death banks gold; cosmetic + battle pass + character-unlock monetization only. The `no_pay_to_win` flag in `GAME.md` is enforced structurally, not promised in a TOS. Owners: game-designer, monetization-spec authors.

## Audience pitch (one paragraph)

For Habby-fans who like Survivor.io's loop but bounced off its grim look, and for casual mobile players who used to play Crossy Road, Cat Quest, or Crash Bandicoot N. Sane, Brave Bunny is a cartoon-mascot auto-battler with the depth of weapon evolutions and the dignity of every-run-pays — no energy meter, no gear gacha, just sit down for 8 minutes with one thumb and feel competent. Soft-launch audience skews 18–35 in TR / PH / ID with a broad-age halo (10–50) thanks to the family-safe register.

## Mode list

- **Run** — the core 7–10 min survival session. Single biome, escalating waves, mid-run boss, end-run boss. Solo only at launch.
- **Lobby (Home)** — the always-on hub. Hosts daily-streak claim, mailbox, biome selector, loadout selector, social/leaderboard preview, and entry points to the other modes.
- **Meta** — character roster (8 at launch), character-level upgrades (perma stats, locked-rare unlocks for the draft pool), and the (post-launch) rune system. Spend point for Stars + Soul-shards.
- **Store** — cosmetic skins for characters and weapons, starter packs (no power), Monthly Card, Growth Fund. No premium currency pack above $19.99.
- **Battle Pass** — 30-day free + premium track, ~50 tiers, rewards mostly cosmetic + character shards + soft currency. One pass live at a time.

## Production scope

### Vertical slice (8-week milestone)

- **1 character** — Bunny (the eponymous mascot; melee-leaning balance baseline).
- **1 biome** — Carrot Fields (open green meadow with low-poly fences, palette baseline for the art bible).
- **3 weapons** — Carrot Spear (front-cone melee), Pebble Sling (auto-targeting projectile), Honey Aura (radial DOT). One evolution recipe across the trio (Carrot Spear + Pebble Sling → Bouncing Cob).
- **1 boss** — the Big Bad Wolf (mid-run miniboss + end-run boss, cosmetic-only variants).
- **15 enemy archetypes** across 4 wave-pattern templates.
- **1 full meta loop** — XP banking, gold banking, soul-shard banking, character-level upgrade (Bunny only).
- **Monetization skeleton** — store screen wired, IAP stubs only, no live receipts.

### Launch scope (Q3 2026)

- **8 characters** — distinct stat baselines (movespeed, HP, weapon affinity) and 1 signature weapon each. Animal roster diversity is a UVP — see positioning risk matrix on Capybara Go!
- **5 biomes** — Carrot Fields, Honey Swamp, Sky Garden, Frost Burrow, Volcano Hop.
- **12 weapons** — including the 3 from the slice; ≥ 6 evolution recipes.
- **1 boss roster** — one boss family with 5 cosmetic biome-skins (cut-list item #2 from `GAME.md`: if behind schedule, the cosmetic variants are the cut).
- **Full meta** — character-level upgrades for all 8 heroes, 30-tier battle pass live, daily-streak system live.
- **Live-ops cadence** — first event 2 weeks post-launch, balance pass every 2 weeks (per `GAME.md` live_ops block).

## What makes brave-bunny different (10 bullets)

1. **Visual register is the moat** — saturated low-poly cartoon in a genre dominated by grim pixel + gothic art.
2. **No energy gate, ever** — pacing is daily-streak-driven, not gated.
3. **Deterministic gear, no gacha** — gear pulls are not in the monetization graph at all.
4. **Animal-roster diversity** — 8 characters at launch, animal-themed, designed for TikTok screenshots and merch.
5. **Every death pays** — gold and soul-shards bank on death; loss-banks-gold dignity loop borrowed from Vampire Survivors.
6. **CC0-asset pipeline** — Quaternius Animated Animals + recolor lets one dev ship 8 heroes in 8 weeks.
7. **Family-safe by design** — no skulls, no blood, no realistic gore; kid-tolerable, parent-tolerable.
8. **TR / PH / ID-first** — soft-launch is the first-class audience, not a test market.
9. **Habby monetization triad without the paywall** — Monthly Card + Battle Pass + Growth Fund, no pay-to-win.
10. **Rewarded-ad-positive surfaces** — 4–6 surfaces (revive, 2x gold, daily chest, draft re-roll, extra pickup magnet, character-shard pull) — ads as a *feature*, not a tax.
