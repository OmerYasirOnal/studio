# Capybara Go! — Deconstruction

> Researcher fetch date: 2026-05-12. All revenue/install figures are third-party trade-press estimates, not first-party disclosures. This is the highest-risk neighbour to brave-bunny because it owns the cute-mascot lane Habby just proved was a $100M+ pocket.

## At a glance
- Developer / publisher: Habby (Singapore) — same publisher as Survivor.io and Archero.
- Release year: **2024** — iOS soft launch August 30, 2024; Korea / Taiwan soft launch October 2024; global / US launch **November 20, 2024**. Sources: [Lancaric UA case study](https://lancaric.me/capybara-go-global-launch-ua-case-study/), [PocketGamer.biz](https://www.pocketgamer.biz/habbys-capybara-go-surpasses-100m-in-gross-player-spending/) (fetched 2026-05-12).
- Platforms: iOS, Android (mobile-only).
- Genre tags: text-based roguelike, idle RPG, auto-battler, mascot-led, "social-casino RPG".
- Estimated revenue (source): **>$109M lifetime gross player spend by Feb 10, 2025** — i.e., 3 months from global launch. Peak month: **$33.3M (Dec 2024)**, **$32.1M (Jan 2025)**, single-day record **$1.4M on Jan 24, 2025**. Reached **~$140M in 2025 full-year** per Sensor Tower China publisher report. Daily IAP $600k–$850k + ~$100–$150k/day IAA. Steady state late 2025 ≈ **$19M / month**. Sources: [PocketGamer.biz, Feb 2025](https://www.pocketgamer.biz/habbys-capybara-go-surpasses-100m-in-gross-player-spending/), [Gamigion, 2024](https://www.gamigion.com/capybara-go-by-habby-makes-800k-day-without-the-u-s/), [Deconstructor of Fun, 2025](https://www.deconstructoroffun.com/blog/2025/7/31/habbys-hybridcasual-empire-the-template-that-built-a-powerhouse), [TechNode, Dec 2024](https://technode.com/2024/12/19/capybara-go-takes-40-million-in-overseas-revenue-within-two-months-of-launch/).
- Top-grossing rank trajectory: Top-20 grossing in KR/TW/JP throughout Q4 2024–Q1 2025; charted #22 on Sensor Tower's overseas Chinese-publisher revenue rankings for 2025 alongside Archero 2 (#17). [Sensor Tower / itiger, 2026](https://www.itiger.com/news/1143976939).
- Notable awards / press: Hit **$10M iOS revenue one month post-launch** ([Sensor Tower news-feed](https://app.sensortower.com/news-feed/habbys-capybara-go-hits-10m-all-time-ios-revenue-one-month-after-launch/6743fb327bcfcd12834eb471)); cited by Deconstructor of Fun as the new template for Habby's "hybridcasual empire" ([DoF, Jul 2025](https://www.deconstructoroffun.com/blog/2025/7/31/habbys-hybridcasual-empire-the-template-that-built-a-powerhouse)).

## Core loop
Tap-to-advance text-roguelike adventure: each "Day" is a tile in a Mario Party-style track. Tapping next-day triggers a randomized event — a battle (resolved automatically with the capybara, pets, and gear), a 3-choice skill draft, a shop, a buff, or a narrative beat. Battles are **fully auto** — no skill stick, no aiming — outcome is a function of gear, pets, talents, and the skill build assembled mid-run. Players "play the build", not the fight. Outside of runs, an **idle layer** accrues gold, spirit stones, and pickaxe charges while offline; check-ins claim AFK rewards, daily energy refills, and timed events.

## Session structure
- Active vs idle balance: **~70% idle / 30% active.** Active = chapter advancement and event-mode runs; idle = AFK gold pile + 7+ daily "chore" surfaces (Goblin Miner, Tower, Arena, Dungeon Raid, Dungeon Dive, Guild donation, AFK claim) — [Game-Vault wiki beginner guide](https://capybara-go.game-vault.net/wiki/Guide:Ultimate_Beginners_Guide).
- Run length: **3–8 minutes per chapter push or dungeon**; auto-battle x2/x4 speed-up via Monthly Card subscription cuts that to ~1–2 min. Daily-chore loop end-to-end ≈ **8–12 minutes**.
- Energy/timer system: **Hard energy gate** (~50–100 cap). Energy refills via timer regen, gem purchase (90 gems → 15 energy, max ~2x/day for F2P), Monthly Card daily grant, or Lifetime Card. [P2W spending guide](https://capybara-go.game-vault.net/wiki/Guide:Real_Money_Spending_P2W_Guide).
- Onboarding length: First chapter ≈ **2–3 minutes** to the first auto-battle and first 3-choice skill draft; first session paced **15–25 minutes** to introduce gear, pets, talents, and the AFK panel.

## Progression
- Meta progression layers: (1) **Equipment** (Bronze→Mythic, free downgrade, 6 slots), (2) **Skill builds** (in-run drafts; Physical Shield / Skill Rage archetypes), (3) **Pets** (egg-hatch gacha, leveling, evolution), (4) **Mounts** (Gold Horseshoe currency), (5) **Talents** (permanent passive tree), (6) **Inheritance / Legacy** rebirth-style permanent stats, (7) **Chapter Adventure / Tower / Dungeon Dive** parallel ladders, (8) **Guild** social layer.
- Resource economy: Gold (run + AFK), Gems (premium), Spirit Stones (gear), Power Stones (gear enhance), Gold Horseshoes (mounts), Lucky Silver Coins (Capy Gacha), Pickaxes (mining), Energy (stamina), Dungeon Vouchers, multiple event tokens (4 rotating Growth events).
- Soft/hard currency: **Soft = Gold + Spirit Stones**, **Hard = Gems**, with **Lucky Silver Coins** as a third intermediate currency dedicated to the gacha sink.

## Monetization
- IAP price points: Standard tier ladder **$0.99 / $4.99 / $9.99 / $19.99 / $29.99 / $99.99**. Chapter packs scale $2.99 → $99.99 with chapter progression. Source: [P2W spending guide](https://capybara-go.game-vault.net/wiki/Guide:Real_Money_Spending_P2W_Guide).
- Ad placements: **Light IAA mix (~15–20% of revenue)** — daily 3+1 rewarded videos in the "three-chest shop", plus rewarded surfaces for extra AFK claim, free pulls, and dungeon retries. The **$10 Ad-Free Card permanently removes ads + grants 50 daily gems**, which is rare for the genre and shows ads are an opt-out chore, not a load-bearing pillar. ([Lancaric](https://lancaric.me/capybara-go-global-launch-ua-case-study/)).
- Battle pass: **4 simultaneous battle passes** ($10 each per season — main pass + tower/talent/dungeon themed funds), creating a "stack of passes" rather than a single track. ([Lancaric](https://lancaric.me/capybara-go-global-launch-ua-case-study/)).
- Gear gacha: Yes — **Cappy King chest** with multi-click rarity ramp-up before reveal (a slot-machine "press to upgrade chest" UI), plus weekly **Capy Gacha** event using Lucky Silver Coins (2-day window; 500-coin guarantee floor). Pets via separate egg-hatch gacha. ([Theria Games guide](https://theriagames.com/guide/capybara-go-capy-gacha-event/), [Game-Vault Capy Gacha wiki](https://capybara-go.game-vault.net/wiki/Capy_Gacha)).
- Habby's signature triad — **confirmed present and expanded**:
  - **Monthly Card** — $5/mo (Speed Travel, 4x battle speed, +50 energy & 200 gems daily).
  - **Growth Fund** equivalents — Talent Fund ($20), Dungeon Fund ($30), Tower Challenge Fund ($10/bracket), Black Market growth packs.
  - **Battle Pass** — $10/season × 4 parallel passes.
  - **Plus** two lifetime cards Survivor.io doesn't have: **Ad-Free Card ($10 lifetime)** and **Lifetime Card ($30 one-time)** — perpetual daily drip. This is Habby's triad evolved into a **6-product subscription stack**, which is the single biggest monetization innovation vs Survivor.io / Archero.

## Art direction
- 2D / 3D: **Pure 2D**, hand-drawn flat illustration; chibi capybara sprites over storybook-paper backgrounds.
- Camera: **No camera** — UI-first layout. Side-scrolling Mario-Party board for the day-track; static portrait framing for battles (capybara left vs enemy right, damage numbers float).
- Visual signature — **cute-mascot-led, single-hero**: ONE capybara protagonist with cosmetic mounts, hats, and pet companions doing the visual variety work. Pastel palette (mint, butter, soft pink), thick-line cartoon illustration, AI-narrated voiceovers selling personality. Compared to brave-bunny's planned **low-poly cartoon 3D + 8 distinct animal heroes**: Capybara Go is a 2D storybook with a single mascot whose iconography (capybara meme) does the recognizability lifting; brave-bunny will read instead as a **3D toy-shelf cast** — closer to a Crossy Road / Disney Magic Kingdoms register, with silhouette variety across a roster instead of accessory variety on one hero.

## What works
- **Capybara meme = free distribution.** The #capybara hashtag had 400k+ TikTok posts pre-launch and 114M-view hits — Habby leased an existing internet affection instead of building a new IP. UA CPI fell well below genre average; Lancaric reports the game scaled to 200k daily installs without paying Applovin at all ([Lancaric](https://lancaric.me/capybara-go-global-launch-ua-case-study/)).
- **Idle-first design opens older / casual demos.** No skill stick, no aim, no dodge — anyone who can read can play. This unlocked the **KR/TW/JP RPG-spending whales** Survivor.io never fully captured (KR alone = 28% / $30M of first-quarter revenue per [PocketGamer.biz](https://www.pocketgamer.biz/habbys-capybara-go-surpasses-100m-in-gross-player-spending/)).
- **6-product subscription stack** (Monthly + Ad-Free + Auto-Mine + Lifetime + 4 passes + funds) drives multi-vector spend; ARPPU dwarfs Survivor.io's pass+monthly+fund triad.
- **Parallel-progression overload (8 layers)** is monetization gold: every layer is a separate paywall and a separate event to FOMO. Daily check-in sticky factor is extreme.
- **Three-chest daily UI** ([Lancaric](https://lancaric.me/capybara-go-global-launch-ua-case-study/)) bundles ad watches, gacha pulls, and IAP offers into one screen — single highest-converting surface in the game.

## What doesn't
- **Auto-battle = zero skill expression.** Hardcore Survivor.io / Archero players bounce; the game is review-bombed as "a slot machine dressed up as an RPG" ([Gamigion](https://www.gamigion.com/capybara-go-by-habby-makes-800k-day-without-the-u-s/)).
- **Monetization-stack overload.** "Every microtransaction hook in the books" is a recurring player complaint; UI density on the home screen (8+ chore tiles + 4 passes + 6 subscriptions) is hostile to lapsed-user re-entry.
- **Energy gate frustrates session-length-hungry players** — the gate is what *forces* the Monthly Card upsell, but it is also the #1 cited F2P churn point in reviews.
- **Single-hero ceiling.** Once a player tires of the capybara meme, there is no roster of new heroes to chase. Habby's answer is more pets / mounts / cosmetics, which dilute the original IP.
- **2D text-roguelike has low "watch-me-play" virality** vs Survivor.io's clip-friendly mass-mob carnage — UA is heavily UGC-meme-leverage, not gameplay-spectacle leverage.

## Lessons for brave-bunny
1. **Cute-mascot is now a proven, $100M+ lane — but it is single-hero-saturated.** Capybara Go owns "the one cute animal." brave-bunny's defensible white space is **roster diversity**: 8 distinct animal heroes (bunny, fox, owl, hedgehog, etc.) creates 8 silhouettes, 8 personalities, 8 collection vectors, 8 UA creative leads. Lean into this in every store-screenshot and every TikTok thumbnail — Habby cannot match it without re-architecting their IP.
2. **Hold the no-energy / no-gear-gacha line as a positioning weapon.** Capybara Go's worst-reviewed friction is precisely energy + gear gacha. brave-bunny's "play as much as you want, gear is earned not pulled" stance is a hard, advertisable promise that targets the exact F2P demographic Capybara Go is bleeding. Tag it explicitly in store copy: "No energy. No gacha. Just runs."
3. **Steal the 6-product subscription stack idea, but trim and re-skin.** Don't ship a single Monthly Card — ship a **Hero Pass (cosmetic-only), Run Bonus Card ($5/mo: +25% gold), and Lifetime Founder's Pack ($30)**. Habby proved players will buy 3+ subs in parallel if each maps to a different anxiety; brave-bunny's twist is that none of these subs sell *power*, only *throughput and cosmetics* — preserving the no-pay-to-win promise.
4. **3D camera + active gameplay is the dual differentiator.** Capybara Go is 2D UI-first and auto-battle; brave-bunny's **low-poly 3D toy-shelf + skill-stick active runs** lands halfway between Survivor.io's gore-grind and Capybara Go's tap-only idle. That middle is currently empty in TR/PH/ID stores and reads native on TikTok (3D cute > 2D cute for short-form virality in those markets).
5. **Match Habby's live-ops cadence (weekly event + rotating pass) on day 1.** Capybara Go ships 4 passes + Capy Gacha + Tower Fund + Talent Fund + Black Market + 2 Growth events on a weekly drumbeat. brave-bunny does not need 4 passes, but it does need **one weekly limited event + one monthly hero spotlight from launch** — or its DAU tail will look anemic next to Habby's. Bake the live-ops calendar into the tech spec, not as a phase-3 add-on.

## Differentiation table

| Axis | brave-bunny | Capybara Go! |
|---|---|---|
| Visual register | Low-poly cartoon 3D, toy-shelf | 2D storybook flat illustration |
| Camera | 3D top-down with mild tilt | UI-first, no camera (static portraits + board track) |
| Hero variety | **8 distinct animals** (bunny, fox, owl, hedgehog, etc.) | **1 capybara** + cosmetic mounts/pets |
| Combat | Active skill-stick, auto-fire, dodge | Fully auto-battle (tap to advance days) |
| Run length | 10–15 min active chapter | 3–8 min auto-resolved chapter (x4 with Monthly Card) |
| Energy gate | **None** (or generous soft cap) | Hard cap ~50–100, refill via gems / Monthly Card |
| Gear gacha | **None — deterministic crafting/merge** | Cappy King chest + weekly Capy Gacha (Lucky Silver Coins) |
| Pet/mount gacha | None (cosmetic unlocks only) | Egg-hatch pets + horseshoe mounts |
| Battle passes | 1 (cosmetic-only) | **4 parallel passes** ($10 each/season) |
| Subscription stack | Monthly + Lifetime Founder's (cosmetic + throughput) | **6 products**: Monthly $5, Ad-Free $10, Auto-Mine $10, Lifetime $30, plus funds |
| Ad model | Rewarded-heavy (8–15/day, never blocking) | Light rewarded (3+1/day); $10 lifetime ad-removal |
| Pay-to-win posture | **Explicit no-P2W; gear deterministic** | Heavy P2W via gear gacha + funds |
| Live-ops cadence | Weekly event + monthly hero spotlight (target) | Weekly Capy Gacha + 4 passes + Growth events stacked |
| Primary IP hook | Roster + silhouette variety | Capybara meme leverage |
| TikTok creative lead | 3D cute hero + clutch run clips | Capybara meme reaction + UGC voiceovers |

## Sources (fetched 2026-05-12)
- PocketGamer.biz: [Habby's Capybara Go surpasses $100m in gross player spending](https://www.pocketgamer.biz/habbys-capybara-go-surpasses-100m-in-gross-player-spending/)
- Sensor Tower news-feed: [Capybara Go! hits $10M iOS revenue one month after launch](https://app.sensortower.com/news-feed/habbys-capybara-go-hits-10m-all-time-ios-revenue-one-month-after-launch/6743fb327bcfcd12834eb471)
- Sensor Tower news-feed: [Habby soft launches Capybara Go!](https://app.sensortower.com/news-feed/habby-soft-launches-capybara-go/66f86faf9479e9529478de2b)
- Sensor Tower / itiger: [Top 30 Chinese Mobile Game Publishers 2025](https://www.itiger.com/news/1143976939)
- Deconstructor of Fun: [Habby's Hybridcasual Empire](https://www.deconstructoroffun.com/blog/2025/7/31/habbys-hybridcasual-empire-the-template-that-built-a-powerhouse)
- TechNode: [Capybara Go! takes $40m overseas in two months](https://technode.com/2024/12/19/capybara-go-takes-40-million-in-overseas-revenue-within-two-months-of-launch/)
- Gamigion: [Capybara GO makes $800k/day without the U.S.](https://www.gamigion.com/capybara-go-by-habby-makes-800k-day-without-the-u-s/)
- Lancaric: [Capybara GO Global Launch UA Case Study](https://lancaric.me/capybara-go-global-launch-ua-case-study/)
- Capybara Go! Game-Vault wiki: [Ultimate Beginners Guide](https://capybara-go.game-vault.net/wiki/Guide:Ultimate_Beginners_Guide)
- Capybara Go! Game-Vault wiki: [Real Money P2W Spending Guide](https://capybara-go.game-vault.net/wiki/Guide:Real_Money_Spending_P2W_Guide)
- Capybara Go! Game-Vault wiki: [Capy Gacha](https://capybara-go.game-vault.net/wiki/Capy_Gacha)
- Theria Games: [Capy Gacha Event guide](https://theriagames.com/guide/capybara-go-capy-gacha-event/)
- BlueStacks: [Capybara Go! Beginner's Guide](https://www.bluestacks.com/blog/game-guides/capybara-go/cbg-beginners-guide-en.html)
- PocketGamer.biz: [Habby soft-launches text-based idle RPG Capybara Go](https://www.pocketgamer.biz/after-1bn-in-player-spending-hybridcasual-specialist-habby-soft-launches-text-based-idle-rpg-capybara-go/)

## Gaps / caveats
- **No first-party financials** — all revenue is Sensor Tower / AppMagic / trade-press estimates; treat as order-of-magnitude. The $109M / $33M-month figures are gross player spend (pre-store-cut), not net to Habby.
- **2025 full-year revenue** ($140M) sourced via secondary Chinese-publisher report aggregating Sensor Tower data; the underlying breakout (iOS-only vs cross-platform) is not fully transparent.
- **IAP price ladder** is observed from community spending guides + store listings, not a Habby disclosure; promo SKUs rotate.
- **Ad share** (15–20% of revenue) and **3+1 rewarded videos/day** are practical-play estimates from one UA case study, not a measured Sensor Tower figure.
- **TR-specific revenue** is not in any public source for Capybara Go!; KR/TW/JP/US splits are the only published regional cuts.
- **brave-bunny differentiation claims** are designed-intent positioning, not yet validated by playtest — re-test after first soft-launch cohort.
