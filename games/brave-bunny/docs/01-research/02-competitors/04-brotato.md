# Brotato — Deconstruction

> Researcher fetch date: 2026-05-12. Revenue/units estimates are third-party trade-press / Boxleiter; treat as order-of-magnitude.

## At a glance
- Developer / publisher: **Blobfish** (solo dev Thomas Gervraud); ongoing live-ops handed to **Evil Empire** (ex-Motion Twin, *Dead Cells*) in late 2025; mobile port published by **Erabit Studios**.
- Release year: **Steam Early Access Sep 27, 2022**; iOS/Android Mar 28, 2023; Steam 1.0 Jun 23, 2023; Switch Aug 3, 2023; PS4/PS5/Xbox Jan 30, 2024.
- Platforms: PC (Steam, Epic, macOS, Linux), iOS, Android, Switch, PS4/PS5, Xbox One/Series — fully cross-platform premium release.
- Genre tags: action-roguelite, **wave-survival** auto-shooter, "Vampire-Survivors-like" with explicit shop/loadout phase.
- Estimated revenue (source): Steam alone ~**$28M gross** across ~96K reviews ([Steam Revenue Calculator, Boxleiter](https://steam-revenue-calculator.com/app/1942280/brotato), fetched 2026-05-12). **10M+ players across PC/console/mobile** by Oct 2025 ([Saving Content, 2025-10-27](https://www.savingcontent.com/2025/10/27/its-a-new-dawn-as-evil-empire-rolls-out-first-content-update-for-blobfishs-brotato-on-pc-today/), fetched 2026-05-12). 1M+ in Early Access by Sep 2022.
- Notable awards / press: Nominated **Best Steam Deck Game**, 2023 Steam Awards. Metacritic PC 76. Overwhelmingly Positive on Steam (~97% across 96K reviews). Acquisition/co-dev deal with Evil Empire treated as the indie-genre success story of 2025 ([Wikipedia](https://en.wikipedia.org/wiki/Brotato), fetched 2026-05-12).

## Core loop
Pick a potato character (each is a hard-coded build archetype with stat modifiers + starting weapon class), drop into a single square arena, auto-shoot at the nearest enemy while moving with a single stick. Survive a 20–90s wave, then enter a **shop phase**: spend collected Materials on weapons (up to 6 slots, with 1–6 star tiers and on-rarity items), items (passives), level-ups (3-pick from a randomized pool), and re-rolls. Repeat for **20 waves**, defeat a final boss, walk away with a character/Danger-level unlock — no permanent stat carry-over.

## Session structure
- Run length: **A *wave* is 20s ramping to 60s (cap), with a 90s wave-20 boss = ~17 min of combat; with shop phases a full successful run = ~20–25 min.** The "super-short" feel comes from each *wave* being bite-sized, not the run itself ([Brotato Wiki – Waves](https://brotato.wiki.spellsandguns.com/Waves), fetched 2026-05-12).
- Energy/timer system: **None.** Pure premium — play as much as you want, no stamina, no daily cap. Free mobile version (`Brotato`) has rewarded-ad gates on extra spins/currency; Premium (`Brotato: Premium`) has zero gates.
- Onboarding length: No tutorial scene — first character (Well-Rounded) drops you into wave 1 in <30s. Mechanical depth (stat tooltips, shop strategy, evolutions, Danger levels) is discovered across the first 2–3 runs (~45–60 min).

## Progression
- Meta progression layers: **Deliberately thin.** No persistent stats. Meta is *unlock-only*: (1) **44+ characters** unlocked by clearing Danger levels & specific runs; (2) **60+ items + 78 weapons** (with Abyssal Terrors DLC) added to the in-run pool as you first-clear with each character; (3) **6 Danger levels (0–5)** of difficulty, each granting a new character + acting as the player's skill ladder ([Brotato Wiki – Danger Levels](https://brotato.wiki.spellsandguns.com/Danger_Levels), fetched 2026-05-12).
- Resource economy: **Materials** (in-run currency for shop) drop from kills + trees; consumed completely each run. No soft/hard currency on PC/Premium-mobile. Free-mobile adds **Spuds** (premium in-run currency) and **Totems** sold for real money or rewarded ads.
- Soft/hard currency: PC/Premium = **none**. Free-mobile = Spuds (hard), Totems (semi-hard pull currency), Materials (soft, in-run only). The split is a clean A/B between the audiences.

## Monetization
- IAP price points (USD, observed 2026-05-12 from US App Store): **Brotato: Premium = $4.99 one-time** + Abyssal Terrors DLC $2.99 + "Abyssal Terrors 01" $2.99. **Brotato (free)** carries the entire mobile-monetization stack: **Starter Pack $0.49, Beginner Totem Pack $0.49, spuds_30 $0.49, month_vip $0.99, spuds_170 $2.99, Abyssal Terrors DLC $2.99, 5-character bundle $3.99, permanent_vip $15.99, "No ads" $19.99**. Source: [Apple App Store – Brotato](https://apps.apple.com/us/app/brotato/id6445884925) and [Brotato: Premium](https://apps.apple.com/us/app/brotato-premium/id1668755109) (fetched 2026-05-12).
- Ad placements: **PC/Premium = zero ads, ever.** Free-mobile = rewarded video on **extra shop re-rolls, free-Totem pulls, daily Spuds, double-XP/double-Materials end-of-wave, extra revives**. TouchArcade and 148Apps reviews note ads are aggressive enough that the **$19.99 "No ads" SKU exists as a standalone purchase** — a clear signal Erabit's targeting two distinct buyer profiles.
- DLC strategy: **Single paid expansion (Abyssal Terrors, $2.99)** adding 10 characters, 10 weapons, 30 items, 20 waves, a Curses mechanic. Free major updates ("New Dawn" Oct 2025) keep retention alive; DLC keeps premium ARPU sleek without subscription.
- Premium vs F2P split: **Brotato is a premium game with an F2P mobile shadow port.** Steam/console/Premium-mobile are flat $4.99 premium. Free-mobile is a separately-built freemium SKU run by Erabit — same gameplay, *very* different economy (subscription `month_vip`, lifetime VIP, hard currency, gacha-style Totems). Most of the franchise's ~$28M+ Steam revenue is pure premium; mobile likely doubles that across both SKUs (no first-party figure published).

## Art direction
2D top-down, single-screen arena. Painted, slightly grunged ground tile, hand-drawn potato sprites (the "mascot moat"), readable enemy silhouettes against high-contrast palettes that shift per stage. Stylized, slightly absurdist — a **literal anthropomorphic potato** dual-wielding chainsaws is the whole pitch. UI is bone-dry functional: white-on-dark numbers, no juice beyond hitstop + crit pops. Crucially **not** a "Vampire Survivors gothic" or "Survivor.io zombie-gray" — Brotato owns *playful absurdity* in the genre.

## What works
- **Premium price + zero-gate on PC/Premium-mobile** is the cleanest UX in the genre. $4.99 buys a full game with **10M+ players** validating the model — proves auto-shooters don't *have* to be F2P to scale.
- **Shop phase = real strategic depth.** Unlike continuous-progress survivors, the wave/shop rhythm gives players a deterministic decision point every 20–60s, dramatically lowering "I died and don't know why" frustration.
- **Character-as-build identity.** 44+ characters are *not* cosmetic — each is a hard build constraint (Crazy = +%damage/–accuracy, Wildling = melee-only, Engineer = turrets). Replayability is bottomless without any stat-creep meta.
- **Danger level ladder** doubles as both difficulty and unlock pacing — the skill ramp *is* the meta. No grind, just "get better."
- **Free mobile as a funnel for Premium** — the free SKU's aggressive ads are the ad — *players who hate ads pay $4.99 for Premium*. Smart two-SKU segmentation that competitors (Survivor.io, Archero) can't easily mirror without cannibalizing their core IAP.

## What doesn't
- **No persistent power progression** turns off the Diablo/Survivor.io retention crowd — D7/D30 numbers on Premium are reportedly lower than F2P peers (no first-party data, inferred from genre norms and a thinner live-ops cadence pre-Evil-Empire).
- **Single-screen, single-arena visual sameness** across the base game — Abyssal Terrors added biomes specifically to address this 2+ years post-launch.
- **Free-mobile monetization stack is messy and review-bombed** — "Brotato (free)" sits below "Brotato: Premium" in reviewer regard; the $19.99 "No ads" SKU above premium price is the canary.
- **Onboarding is non-existent** — works for indie/PC, but the mobile free SKU's TouchArcade review explicitly flags it as a barrier vs Survivor.io / Archero.
- **No social / live-ops hook on launch** — leaderboards, daily challenges, events were absent until Evil Empire took over in late 2025. Genre rivals beat it on retention infrastructure.

## Lessons for brave-bunny
1. **The wave/shop rhythm is the design's real moat — adopt it for 7–10 min runs.** Brotato proves players *want* a strategic pause every ~30–60s. Brave-bunny's 7–10 min target maps cleanly to a **~6–8 wave structure (60–90s per wave) with a short ~10–15s shop**, vs Survivor.io's continuous single-pick stream. The pause cadence is also what makes Brotato runs *clippable in a different way* — the shop screen is a discussion moment, not just death-clutch montages. This is open lane for mascot-friendly TikTok creative.
2. **Character-as-build is higher leverage than gear-merge meta.** Brotato hits 10M players with **zero persistent stats** — replayability comes entirely from "what wild constraint am I playing this run?" For brave-bunny's TR/PH/ID audience and 8-week ship window, a roster of **8–12 hard-archetype bunny characters** (Hopper = sprint-builds, Sniper Bun = single-target only, Glass Cannon = 1-HP-max-damage) is far cheaper to ship and balance than Survivor.io's 5-layer gear/merge/tech-parts stack — and converts equally well into TikTok creative ("which bunny survived Danger 5?!"). Build the lethality-and-identity loop first; bolt on a thin meta layer (skin unlocks, cosmetic pets) only if D7 retention demands it.
3. **Run *two* SKUs from launch: Premium ($4.99) + Free (rewarded-ad heavy).** Brotato + Brotato: Premium prove the segmentation works and *reinforce each other* — the Premium SKU's existence is itself the ad-removal upsell in the free version. For brave-bunny, this means designing the data layer (saves, balance JSONs, character unlocks) to be **economy-agnostic from day one** so the same Unity build ships as a $3.99 premium SKU (low-friction Western/Steam-deck-style buy) and a freemium SKU with the Survivor.io ad-bonus surfaces. The two-SKU split also de-risks the genre's biggest unknown: whether TR/PH/ID converts to IAP at all.

## Sources (fetched 2026-05-12)
- Wikipedia: [Brotato](https://en.wikipedia.org/wiki/Brotato)
- HandWiki: [Software:Brotato](https://handwiki.org/wiki/Software:Brotato)
- Saving Content: [Evil Empire rolls out first content update for Blobfish's Brotato](https://www.savingcontent.com/2025/10/27/its-a-new-dawn-as-evil-empire-rolls-out-first-content-update-for-blobfishs-brotato-on-pc-today/)
- Steam Revenue Calculator: [Brotato Steam estimate](https://steam-revenue-calculator.com/app/1942280/brotato)
- Apple App Store: [Brotato (free)](https://apps.apple.com/us/app/brotato/id6445884925) · [Brotato: Premium](https://apps.apple.com/us/app/brotato-premium/id1668755109)
- TouchArcade: [Brotato mobile review (2023)](https://toucharcade.com/2023/04/04/brotato-iphone-ipad-android-review/) · [Brotato premium mobile launch](https://toucharcade.com/2023/03/28/top-down-roguelite-shooter-brotato-is-out-now-on-ios-and-android-as-a-premium-release/) · [Abyssal Terrors announce](https://toucharcade.com/2024/04/11/brotato-abyssal-terrors-dlc-gameplay-local-coop-multiplayer-mode-update-summer-2024-pc-release-date-mobile/)
- Pocket Gamer: [Abyssal Terrors release](https://www.pocketgamer.com/brotato/abyssal-terrors-release/)
- Brotato Wiki (spellsandguns): [Waves](https://brotato.wiki.spellsandguns.com/Waves) · [Danger Levels](https://brotato.wiki.spellsandguns.com/Danger_Levels) · [Weapons](https://brotato.wiki.spellsandguns.com/Weapons) · [Characters](https://brotato.wiki.spellsandguns.com/Characters)

## Gaps / caveats
- **No first-party revenue.** Steam ~$28M is Boxleiter; mobile revenue (both SKUs) is undisclosed by Erabit. 10M+ "players" is the only published cross-platform figure.
- **Free-mobile ad surface counts** are inferred from reviews — not measured per-session.
- **Premium vs F2P split-of-revenue is unknowable** publicly; the framing above is informed inference.
- **TR/PH/ID localized performance** is not in any public report for either SKU.
- The user's brief said Brotato runs are "~3-5 min waves" — that's the *per-wave* feel; **a full run is ~20–25 min**. Lessons above use the corrected number.
