# GDD 02 — Meta Loop

> The out-of-run progression systems for Brave Bunny: daily streak, unlock ladder, currencies, battle pass, achievements, daily missions, weekly events, and live-ops cadence. Sister docs: `00-overview.md` (pillars), `01-core-loop.md` (run-end tally contract), `03-characters.md` (unlock costs sourced from this doc).

## Daily streak system

The streak system replaces the energy gate. It is the **only** mechanism that rewards "show up tomorrow." Per `00-overview.md` pillar 3, it is forgiving by design — missing days never locks content.

| Streak day | Reward | Notes |
|---|---|---|
| 1 | 100 Carrots | Always claimable on first launch of UTC day |
| 2 | 150 Carrots + 1 random passive shard | Soft pull from 6-passive pool |
| 3 | 250 Carrots | |
| 4 | 1 Stars | First premium drip — proves the streak pays |
| 5 | 1 random weapon shard | Soft pull from owned-weapons pool |
| 6 | 400 Carrots + 1 Stars | |
| 7 | **1 cosmetic shard + 3 Stars** | Day-7 capstone; cosmetic shard is a guaranteed unowned skin shard |

After day 7 the cycle repeats from day 1. The capstone is the only "rare-feeling" reward; it must always feel earned, never trivial.

### Reset rules

- Streak day increments on the **first claim** within a new UTC day.
- A missed day **does not reset to 0**; instead, a **2-day skip tolerance** applies. The player can miss up to 2 consecutive UTC days and pick up where they left off on the third day. Missing 3+ days resets to day 1.
- The 2-day tolerance is invisible to the player (no countdown UI); we soft-detect re-engagement and award silently. A/B candidate post-launch: surface the tolerance as a "freeze token" cosmetic.

### Hard rule

> No streak reward is ever required to progress meta unlocks. The streak is a bonus channel, not a prerequisite. If a player ignores the streak entirely, they still unlock the full character roster via Carrots and Stars earned in-run + IAP.

## Persistent unlocks ladder

Three ladders run in parallel: characters, biomes, starter-loadout slots.

### Character ladder (8 total)

Authoritative cost list — sourced into `03-characters.md` unlock-cost column. Sequenced cheap-to-expensive so a daily player hits a new unlock every ~10 days at average earn rate.

| Order | Character | Unlock cost | Alternative path |
|---|---|---|---|
| 1 | Bunny | Free (starter) | n/a |
| 2 | Tortoise | 200 Stars | "Survive 100 elite kills" achievement (free) |
| 3 | Hedgehog | 400 Stars | Battle Pass Tier 15 (premium) |
| 4 | Fox | 600 Stars | "Reach wave 30" achievement |
| 5 | Otter | 800 Stars | Limited event "Splash Festival" (free, week 6) |
| 6 | Panda | 1000 Stars | Battle Pass Tier 30 (free track capstone) |
| 7 | Badger | 1200 Stars | "Defeat the Wolf boss 25 times" achievement |
| 8 | Owl | 1500 Stars | "Reach wave 50" achievement (hardest in game) |

### Biome ladder (5 total)

| Order | Biome | Unlock condition |
|---|---|---|
| 1 | Carrot Fields | Free (starter, vertical-slice biome) |
| 2 | Honey Swamp | Reach Bunny Lv 5 (any character) |
| 3 | Sky Garden | Defeat the Carrot Fields boss 3 times |
| 4 | Frost Burrow | Reach any character Lv 10 |
| 5 | Volcano Hop | Defeat 4 biome bosses |

All biome unlocks are **gameplay-gated, never paywalled**. The biome list ships in the build; only the unlock flag flips.

### Starter-loadout slots

The loadout pick (per `01-core-loop.md`) currently exposes 2 starting-item slots. A third (cosmetic) slot post-launch.

| Slot | Unlock | Effect |
|---|---|---|
| 1 | Free | Pick 1 starting weapon |
| 2 | Free | Pick 1 starting passive |
| 3 | 750 Stars OR Battle Pass Season 3 premium | Pick a second starting passive |

## Currency model

Three currencies. Each has a **single primary sink** to avoid the "what is this for" trap.

| Currency | Type | Earned from | Primary sink | Secondary sink |
|---|---|---|---|---|
| **Carrots** | Soft gold | Every run (1–5k per run); daily streak; achievements | Character meta-level upgrades (perma stats) | Cosmetic shop (skin shards, color variants) |
| **Stars** | Premium | IAP packs; Battle Pass; rare rewarded-ad chest (1 per day cap); achievements (1–3 per achievement) | Character unlocks (8-character ladder) | Battle Pass tier skip (50 Stars per tier; cap of 10 skips/season) |
| **Soul Shards** | Run-banked | Drop from elites (1 shard) and bosses (3 shards) | **Rune system (v1.1 post-launch)** — placeholder sink at launch | n/a at launch (banks visibly, told "for runes coming soon") |

### Carrot earn baseline

- Average run: **2500 Carrots**
- A skill-floor player should be able to fully meta-level a character in **~30 runs** (~5 hours of play across ~10 days).
- Cosmetic shop items priced **500–4000 Carrots**; new player can afford their first cosmetic on day 1.

### Stars earn baseline

- Average earnings without IAP: **8–12 Stars/day** (achievements + battle pass + daily streak + rare ad chest).
- IAP packs: $0.99 → 60 Stars; $4.99 → 350 Stars; $9.99 → 800 Stars; $19.99 → 1800 Stars. **No SKU above $19.99** per `03-positioning.md`.
- F2P player can unlock the full 8-character roster in ~6–8 months without spending a dollar.

### Soul Shards launch-state caveat

Soul Shards bank at launch but have no spend until the rune system ships (target: v1.1, week 8 post-launch). This is **flagged for monetization-spec author and balance-engineer**: the visible-but-unspendable currency is a deliberate hook for the rune drop, but must not feel like a bait-and-switch. Show "Coming Soon: Runes" tooltip on the wallet tile.

## Battle pass structure

Per `00-overview.md`: **30 tiers × 4 weeks** (one season). Free + premium tracks.

### Track contents

| Tier | Free reward | Premium reward |
|---|---|---|
| 1 | 50 Carrots | 100 Carrots + 1 Star |
| 5 | 1 cosmetic shard | Premium skin (Bunny: "Carnival") |
| 10 | 200 Carrots | 3 Stars + cosmetic skin (random owned character) |
| 15 | **Hedgehog unlock shard ×5** | Hedgehog unlock shard ×10 (premium accelerates) |
| 20 | 1 Stars | Premium weapon skin |
| 25 | 500 Carrots | 5 Stars + cosmetic emote |
| **30** | **1 character unlock** (rotates: Panda S1, Otter S2, Badger S3) | All free + premium-exclusive character skin |

### Battle Pass economics

- Premium track price: **$9.99** (or 800 Stars equivalent — yes, Stars-for-pass is allowed; reinforces non-paywall promise).
- A diligent player completes all 30 tiers in 28 days at ~2 hr/day. Pass-XP earn rate **calibrated by balance-engineer**, sourced from run length + missions completed.
- Tier skip: **50 Stars per tier**, capped at **10 skips per season** to prevent whales from skipping the whole pass on day 1 (preserves dev-driven pacing).

### Pass tier 30 capstone

Tier 30 free-track grants a **rotating character unlock** (Panda Season 1, Otter Season 2, Badger Season 3, etc.). This is the only place where a character unlocks via the pass; the rotation ensures every character has 2 earn paths (Stars + 1 pass season).

## Achievement framework

**50 achievements** at launch, grouped into 4 categories. Each grants 1–5 Stars and a one-time Carrots payout.

| Group | Count | Examples | Reward range |
|---|---|---|---|
| **Run** | 20 | "Survive 5 minutes", "Reach wave 20", "Kill 1000 enemies in one run" | 1 Star + 100–500 Carrots each |
| **Mastery** | 15 | "Reach character Lv 10", "Evolve 3 weapons in one run", "Unlock all weapons" | 2 Stars + 200–800 Carrots each |
| **Cosmetic** | 8 | "Own 5 character skins", "Equip a full outfit", "Use 3 emotes" | 1 Star + 100 Carrots each |
| **Discovery** | 7 | "Visit all 5 biomes", "Trigger every weapon evolution", "Find the hidden chest in Honey Swamp" | 3 Stars + 500 Carrots each |

Total achievement Stars at full clear: **~140 Stars** (roughly one mid-cost character unlock).

## Daily missions

**3 missions per day**, cycled from a pool of **30** so no player sees the same trio twice in a 10-day window.

| Mission type | Example | Reward |
|---|---|---|
| Kill quota | "Defeat 200 swarmers" | 100 Carrots + 50 Pass-XP |
| Survive quota | "Survive 7 minutes in any run" | 150 Carrots + 75 Pass-XP |
| Weapon flavor | "Evolve any weapon" | 200 Carrots + 100 Pass-XP |
| Character flavor | "Complete a run as Fox" (only triggers for owned characters) | 250 Carrots + 100 Pass-XP |
| Pickup flavor | "Collect 50 hearts in one day" | 100 Carrots + 50 Pass-XP |

Missions reset at **00:00 UTC**. Uncompleted missions do not roll over (avoids hoarding).

## Weekly events

Two recurring event slots, run alternating weeks:

### Limited biome variant

- One existing biome receives a **palette + enemy-mix swap** for 7 days (e.g., "Carrot Fields: Harvest Moon" — orange palette, +30% elite spawn rate, double Carrots drop).
- Available to all players regardless of biome unlock state (lets new players preview locked biomes).

### Boss rush

- A 5-minute mode where players fight all 5 biome bosses back-to-back, no draft.
- Rewards: **5 Soul Shards** (5x normal rate), 1000 Carrots, leaderboard placement.
- Free re-entry; no energy.

## Live-ops cadence target

Aligns with `GAME.md` `live_ops` block.

| Cadence | Drop type | Owner |
|---|---|---|
| **Weekly** | New cosmetic in store + 1 limited event slot rotation | live-ops + art-director |
| **Bi-weekly** | Balance patch (TTK ladder re-validation) | balance-engineer |
| **Monthly** | "Hero spotlight": one character gets a free-trial week + a discounted unlock cost (−25%) | game-designer + monetization-spec |
| **Seasonal (every 4 weeks)** | New battle pass season + tier 30 character rotation | game-designer + ui-engineer |
| **Quarterly** | New biome OR new character (cut-list determines which) | full team |

## Cross-references

- Currencies wired into the run-end tally in `01-core-loop.md` (gold = Carrots, soul-shards = Soul Shards, pass-XP banks here).
- Character unlock costs duplicated in `03-characters.md` table — **this doc is source of truth**.
- Soul Shards rune system spec deferred to `02-gdd/06-runes.md` (v1.1).
- Battle Pass and IAP pricing audited by monetization-spec author at `02-gdd/07-monetization.md` (TBD).
