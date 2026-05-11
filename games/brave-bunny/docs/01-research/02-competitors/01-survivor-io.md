# Survivor.io — Deconstruction

> Researcher fetch date: 2026-05-12. All revenue/install figures are third-party trade-press estimates, not first-party disclosures.

## At a glance
- Developer / publisher: Habby (Singapore)
- Release year: 2022 (soft-launched late 2020 in select regions; global launch August 2022)
- Platforms: iOS, Android (mobile-only)
- Genre tags: action-roguelite, auto-battler, swarm-shooter, "bullet-heaven"
- Estimated revenue (source): **>$500M lifetime IAP by mid-2024**; sustained ~**$5–6M / month** across 2024–early 2025; cooling to ~**$3M / month combined iOS+Android** by Q1 2026. Sources: [Gamesforum / Global Games Forum, 2025](https://www.globalgamesforum.com/news/how-survivor.io-continues-to-pull-in-5-million-a-month-three-years-later) (fetched 2026-05-12); [Sensor Tower app overview](https://app.sensortower.com/overview/1528941310?country=US) (fetched 2026-05-12); early traction $75M / 37M downloads in first 2 months per [Mobilegamer.biz, 2022](https://mobilegamer.biz/two-months-in-survivor-io-passes-75m-from-37m-downloads/).
- Notable awards / press: TikTok virality break-out — Habby moved ~50% of UA budget to TikTok, scaling daily installs from 11K → 400K in days (see [Apptamin case study](https://www.apptamin.com/blog/survivor-io-a-tiktok-success-story-for-mobile-games-%E2%8E%AE-case-study/) and [Lancaric UA case study](https://lancaric.me/survivor-io-global-launch-ua-case-study/), fetched 2026-05-12). Top-grossing iOS shoot'em up multiple quarters per [Sensor Tower, Q1 2024](https://sensortower.com/blog/2024-q1-ios-top-5-shoot'em%20up%20games-units-us-6019ed4d241bc16eb8623896).

## Core loop
Player drops into a top-down arena, moves with a single virtual stick while the character auto-fires at the nearest enemy. Every level-up presents three randomized weapon / passive upgrades, building toward weapon evolutions; survive ~15 minutes, kill the chapter boss, walk away with gold, gear chests, and EXP that feed an out-of-run meta layer (hero levels, gear tiers, gear merge, skill nodes).

## Session structure
- Run length: **~10–15 minutes per chapter** (boss spawns at the timer; clear = full rewards) — [BlueStacks guide](https://www.bluestacks.com/blog/game-guides/survivor-io/sio-beginner-guide-en.html), [LDPlayer guide](https://www.ldplayer.net/blog/survivor-io-beginners-guide.html).
- Energy/timer system: Hard energy gate on main chapter play (≈1 hour of continuous play before depletion); refill via timer regen, gems, or rewarded video ads. Side modes (Daily Challenges, time-limited events) bypass energy to keep retention sticky once mainline is locked.
- Onboarding length: First chapter + tutorial ≈ **3–5 minutes** to first power fantasy moment (auto-attack + first three upgrades), then a paced 20–30 minute first session that drip-feeds gear, hero, and pass UI.

## Progression
- Meta progression layers: (1) Hero unlock + level-up, (2) Equipment crafting and **gear merging** (combine duplicates to tier up), (3) Skill tree / stat nodes, (4) Chapter progression, (5) Collectible "Tech Parts" / set bonuses, (6) Time-gated event currencies feeding limited skins / heroes.
- Resource economy: Gold (run drops + idle "Quick Patrol" passive), gear pieces & keys (chest-driven), EXP shards (heroes), event tokens (rotating), pass EXP from daily missions. Multiple bottlenecks force diversified spend.
- Soft currency / hard currency: **Soft = Gold**; **Hard = Gems** (purchase or sparingly earned). Pass EXP and gear keys behave as semi-hard intermediate currencies.

## Monetization
- IAP price points (USD-equivalent, observed across stores): tier-style ladder anchored at **$0.99 / $4.99 / $9.99 / $19.99 / $49.99 / $99.99**. Flagship offers: **Growth Fund ≈ $7.99–$9.99**, **Monthly Card 2 ≈ $9.99–$14.99**, **Survivor Pass ≈ $19.99**. Regional pricing varies (e.g., Singapore Growth Fund S$9.98; Egypt EGP 399.99). Source: [Survivor.io App Store listings, multiple regions](https://apps.apple.com/lu/app/survivor-io/id1528941310) (fetched 2026-05-12).
- Ad placements (count per session, types): Rewarded video is the workhorse — present on **energy refills, Quick Patrol bonus pulls (5/day), Gold Piggy "Watch to Crack," daily/weekly equipment-key packs, gacha free pulls, double-rewards on run completion**. Practical session count: a free player can trigger **8–15 rewarded videos per active day**. Almost no interstitials in-run; ads gate bonuses, never block. Source: [Gamigion monetization breakdown](https://www.gamigion.com/survivor-io-the-progressive-monetization-masterclass/) (fetched 2026-05-12).
- Battle pass / subscription: **Survivor Pass** (~$19.99) with free + premium tracks; pass EXP comes from daily missions, encouraging daily logins. **Monthly Card** subscription-style item delivers gems on a 30-day drip. Hybrid mix is widely reported as **≈50% IAP / 50% ad revenue** ([Gamigion](https://www.gamigion.com/survivor-io-the-progressive-monetization-masterclass/)).

## Art direction
- 2D / 3D: **2.5D** — 3D characters and weapons over 2D-feel tile environments, with flat lighting and crisp sprite-like silhouettes.
- Camera angle: Locked top-down, mild perspective tilt, fixed zoom, no rotation.
- Visual signature: Zombie-apocalypse mall/urban palette (grays, neon greens, blood reds), readable swarm silhouettes against muted ground, juicy crit numbers and explosion juice. Functional first, mood second — readability of 200+ enemies on screen is the win.

## What works
- **Sub-30s time-to-fun**: one stick, auto-fire, three-pick level-up — the simplest possible action-roguelite onramp.
- **Layered meta**: 5+ progression vectors (hero, gear, merge, skills, pass) keep daily logins meaningful for years post-launch — [80M+ installs, $500M+ IAP](https://www.globalgamesforum.com/news/how-survivor.io-continues-to-pull-in-5-million-a-month-three-years-later).
- **TikTok-native creative loop**: gameplay is inherently clippable (last-second clutch, evolution drops), feeding a self-perpetuating UA machine ([Apptamin](https://www.apptamin.com/blog/survivor-io-a-tiktok-success-story-for-mobile-games-%E2%8E%AE-case-study/)).
- **Rewarded ads everywhere, blocking ads nowhere**: ads feel like *bonuses* the player chooses, raising eCPM acceptance without hurting D1/D7.
- **Live-ops drumbeat**: limited heroes, gacha events, anniversary content sustain a 3+ year tail almost no other viral hit has matched.

## What doesn't
- **Visual fatigue**: the gray-zombie palette is identikit and has spawned a wave of indistinguishable clones; new players bounce off the genre's sameness.
- **Late-game gear grind feels paywall-heavy**: gear-merge RNG and tech-part sets are widely cited in community guides as the point where F2P stalls (BlueStacks / Pocket Gamer beginner guides).
- **Energy gate frustrates committed players** and pushes them to clones with no gate (e.g., Vampire Survivors mobile).
- **Story / world-building is near zero** — no narrative hook to anchor 12-month retention beyond systems.
- **UI density** balloons over time; the home screen has 15+ tappable systems, hostile to returning lapsed users.

## Lessons for brave-bunny
1. **Visual differentiation is the biggest open lane.** Survivor.io's gray zombie aesthetic is the genre's weakest moat — a saturated low-poly cartoon mascot read (Brave Bunny's direction) is genuinely under-supplied in TR/PH/ID stores and is *more* TikTok-native (cute-character clips out-engage gore clips in those markets). Lean hard into the silhouette + palette differentiation in the first 6 seconds of every UA creative.
2. **Copy the loop, not the gate.** Adopt the 10–15 min run, three-pick level-up, auto-fire, weapon-evolution structure verbatim — it is the proven hook. But **soft-launch without a hard energy gate** (or with a very generous one); test as a North-Star-friendly retention driver against the 40% D1 target before reintroducing energy when monetization tuning starts. Replace the gate's monetization role with the **Monthly Card + Pass + Growth Fund triad**, which has higher ARPU lift and less D7 churn.
3. **Bake rewarded video into 6–8 bonus surfaces from day one.** Survivor.io's ad strategy is "every action can be sped up by an ad" — never blocking, always optional. For TR/PH/ID with lower IAP ARPU, this ad-stack is *more* important than IAP tuning at launch. Plan the bonus surfaces (energy refill, double end-of-run, free-pull, idle-loot bonus, chest unlock skip, revive) before tech-spec freezes.

## Sources (fetched 2026-05-12)
- Gamesforum: [How Survivor.io continues to pull in $5M/month three years later](https://www.globalgamesforum.com/news/how-survivor.io-continues-to-pull-in-5-million-a-month-three-years-later)
- Mobilegamer.biz: [$75M / 37M downloads in two months](https://mobilegamer.biz/two-months-in-survivor-io-passes-75m-from-37m-downloads/)
- Mobilegamer.biz: [TikTok drives Survivor.io past $15M / 12M downloads](https://mobilegamer.biz/tiktok-drives-habbys-survivor-io-past-15m-iap-revenue-12m-downloads/)
- Gamigion: [Survivor.io — The "Progressive" Monetization Masterclass](https://www.gamigion.com/survivor-io-the-progressive-monetization-masterclass/)
- Sensor Tower: [App overview – Survivor!.io (iOS US)](https://app.sensortower.com/overview/1528941310?country=US)
- Apptamin: [Survivor.io TikTok success case study](https://www.apptamin.com/blog/survivor-io-a-tiktok-success-story-for-mobile-games-%E2%8E%AE-case-study/)
- Lancaric: [Survivor.io Global Launch UA Case Study](https://lancaric.me/survivor-io-global-launch-ua-case-study/)
- BlueStacks: [Beginner's Guide for Survivor.io](https://www.bluestacks.com/blog/game-guides/survivor-io/sio-beginner-guide-en.html)
- LDPlayer: [Survivor.io Beginner's Guide](https://www.ldplayer.net/blog/survivor-io-beginners-guide.html)
- Pocket Gamer: [Survivor.io guide](https://www.pocketgamer.com/survivor-io/guide/)
- Survivor.io Wiki – Fandom: [Survivor Pass](https://survivorio.fandom.com/wiki/Survivor_Pass)
- App Store listings (regional pricing): [LU](https://apps.apple.com/lu/app/survivor-io/id1528941310), [SG](https://apps.apple.com/sg/app/survivor-io/id1528941310), [EG](https://apps.apple.com/eg/app/survivor-io/id1528941310)

## Gaps / caveats
- **No first-party financials** — all revenue numbers are Sensor Tower / AppMagic / trade-press estimates; treat as order-of-magnitude.
- **IAP price ladder** is observed from store listings + community spending guides, not a Habby disclosure; exact SKU-by-SKU pricing rotates with promos.
- **Ad-count-per-session** is a practical-play estimate (8–15 rewarded videos/day), not a measured Sensor Tower figure.
- **TR/PH/ID localized ARPU** for Survivor.io is not in public reports — only China / KR / US splits are quoted; carry over with caution.
