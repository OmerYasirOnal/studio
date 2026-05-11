# GDD 12 — Content Roadmap (First 90 Days Post-Launch)

> The live-ops content cadence for Brave Bunny's first 90 days after launch. Owner: game-designer with live-ops handoff to monetization-spec author and ux-designer for event-tray UI. Sister docs: `02-meta-loop.md` (live-ops cadence target table — this doc operationalizes it), `09-monetization-design.md` (event pricing rules), `06-biomes.md` (which biomes can host events), `07-bosses.md` (boss-rush event roster), `GAME.md` (live_ops block: first event = +2 weeks, balance cadence = every 2 weeks), `01-research/02-competitors/05-capybara-go.md` (Habby cadence benchmark).

## Design philosophy

Per `01-research/02-competitors/05-capybara-go.md` lesson 5 — Habby ships **1 weekly event + 4 passes + Capy Gacha + Tower Fund + Talent Fund + 2 Growth events** on a weekly drumbeat. Brave Bunny does not need (and cannot staff) 4 parallel passes, but it **does need** to match Habby's cadence on the surfaces that drive DAU retention:

1. **One scheduled drop every 1-2 weeks**, never longer.
2. **No "dead weeks"** where home-screen content does not change.
3. **Live-ops drops are recolors + JSON edits, not new code.** This is the CC0 pipeline thesis applied to live-ops.
4. **Every drop must be cuttable.** The cut order is enumerated in `13-risks-and-cuts.md`.

## Cadence rules (the steady-state contract)

These rules apply **for the entire 90-day window and beyond.** Any breach requires an ADR.

| Cadence | Drop type | Owner | Effort budget |
|---|---|---|---|
| **Weekly (or every 2 weeks)** | 1 cosmetic drop (CC0 + Blender recolor pack) | art-director + live-ops | ≤ 8 dev-hours / drop |
| **Bi-weekly** | 1 balance pass (TTK ladder re-validation) | balance-engineer | ≤ 4 dev-hours / pass |
| **Monthly** | 1 boss-rush event (uses existing 5 launch bosses, no new content) | live-ops + level-designer | ≤ 6 dev-hours / event |
| **Every 4 weeks** | 1 character spotlight (free trial week + −25% unlock cost) | game-designer + ui-engineer | ≤ 4 dev-hours / spotlight |
| **Every 4 weeks** | 1 battle pass season turnover | game-designer + monetization-spec | ≤ 12 dev-hours / season |
| **Quarterly** | 1 new biome OR new character (cut-list decides) | full team | ≤ 80 dev-hours / drop |

Total: **~25 dev-hours / week** sustainable cadence for live-ops by a one-developer team.

## Week-by-week roadmap

### Weeks 1-2 — Stability + tuning

| Beat | Owner | Notes |
|---|---|---|
| Daily missions tray launches | ui-engineer | Was withheld from session 1-2 per `10-onboarding.md` for new users; goes live for all users in week 1 |
| First weekly event #1: **Meadow — Night Variant** | live-ops + art-director | Palette swap of Meadow (warm noon → cool moonlit); +20% Carrots; +30% elite spawn; 7-day window |
| Balance pass #1 | balance-engineer | TTK ladder re-validation on Bunny + first-week telemetry — most-likely tuning: nerf Carrot Spear if it dominates draft, buff Honey Aura if it under-picks |
| Crash-rate audit | qa-engineer | Hard-target: crash-free sessions ≥ 99.5% (per `00-overview.md` guardrail) |
| Push notification: 1 message at the end of week 1 ("Your bunny misses you.") | live-ops | Only sent to users who opted-in per `10-onboarding.md` session-4 prompt |

### Weeks 3-4 — Character Spotlight #1 + Battle Pass turnover

| Beat | Owner | Notes |
|---|---|---|
| Character Spotlight #1: **Tortoise** | game-designer + ui-engineer | Tortoise free for 7 days (any player can play as Tortoise without unlock); -25% unlock cost (200 → 150 Stars) for spotlight week + 1 |
| Spotlight bundle SKU: **Tortoise Spotlight Bundle** $4.99 | monetization-spec | Per `09-monetization-design.md` Hero Spotlight Bundle template: Tortoise premium skin + weapon skin + 100 Stars |
| Battle Pass Season 1 **finale** | game-designer | Tier 30 capstone fires: free track grants Panda unlock for Season 1 holders |
| Battle Pass Season 2 **start** | game-designer + monetization-spec | New 30-tier pass; tier 30 free-track now grants Otter unlock |
| Weekly event #2 (alternates from Event #1): **Boss Rush — Old Boar King only** | live-ops + level-designer | 5-minute mode; fight 3 Old Boar Kings back-to-back at scaled HP; 5× Soul Shards reward + leaderboard |
| Balance pass #2 | balance-engineer | Bi-weekly cadence |

### Weeks 5-6 — Boss Rush event + cosmetic drop

| Beat | Owner | Notes |
|---|---|---|
| **Monthly Boss Rush #1** — first full boss-rush (all 5 launch bosses if 5-boss launch shipped; **only Old Boar King** if cut-list item #2 fired and we shipped vertical-slice-only bosses) | live-ops + level-designer | 5-minute mode; sequential boss fights; 5× Soul Shards; leaderboard for top 100 |
| Cosmetic drop: **CC0-recolor pack** (1 outfit per launch character — pumpkin / autumn theme) | art-director | 8 outfits (1 per character); price: 50 Carrots common, 200 Stars rare |
| Daily mission pool expansion +5 new missions | game-designer | Pool now 35 (vs launch 30); reduces repeat frequency |
| Balance pass #3 | balance-engineer | |
| TR localization audit | narrative-designer + localizer | First post-launch TR copy audit per `narrative/00-tone-bible.md` §6 |

### Weeks 7-8 — New biome unlock window for non-founders

| Beat | Owner | Notes |
|---|---|---|
| Biome **Beach** unlocked for non-Founder users | live-ops | Beach is launch-day for Founder Pass holders (per `09-monetization-design.md` Founder Pass perm cosmetic + early-access framing); week 7 it becomes generally available |
| Beach launch event: **Sand Festival** — Beach-only run, +50% Carrots, all swarmer spawns are sand-puffs (cosmetic-only enemy reskin) | live-ops + art-director | 14-day window |
| Spotlight bundle SKU: **Beach Drop Bundle** $4.99 per `09-monetization-design.md` Biome Drop Bundle template | monetization-spec | 5 character skins recolored for Beach + 1 sandals emote |
| Battle pass mid-season check-in | game-designer | Telemetry: % of free-track completers vs premium-track completers; if free-completion < 50%, ease Pass-XP curve in next balance pass |
| Balance pass #4 | balance-engineer | |

### Weeks 9-10 — Character Spotlight #2 + Soft-launch readout

| Beat | Owner | Notes |
|---|---|---|
| Character Spotlight #2: **Fox** | game-designer + ui-engineer | Fox free for 7 days; -25% unlock cost |
| Spotlight bundle SKU: **Fox Spotlight Bundle** $4.99 | monetization-spec | Fox premium skin + Cunning Strike weapon skin + 100 Stars |
| **TR localized launch decision gate** | game-designer + product (foreground orchestrator) | Review TR soft-launch numbers (D1, D7, ARPU); GO/NO-GO on broader TR push and TR-localized marketing |
| Weekly event #3: **Meadow — Harvest Moon** | live-ops + art-director | Palette swap, +30% elite spawn, +20% Carrots; rerun of week-1 night variant template with new palette |
| Balance pass #5 | balance-engineer | |
| Push notification: scheduled mid-engagement nudge for opted-in users | live-ops | "There's a Spotlight on the Fox this week. Want to try her?" |

### Weeks 11-12 — Run-Rush leaderboard + Founder window closes

| Beat | Owner | Notes |
|---|---|---|
| **Run-Rush leaderboard event** — global leaderboard for fastest Meadow + Beach combined clear time over a 7-day window | live-ops + ui-engineer | Cosmetic-only rewards (top 100: exclusive cosmetic frame; top 10: exclusive emote). Per `09-monetization-design.md` no-P2W audit — no rewards that affect combat math |
| **Founder Pass launch-window closes (90-day mark)** | monetization-spec | Per `09-monetization-design.md` — Founder Pass retired forever after launch + 90 days. One-time scarcity event. Last-week soft reminder banner on home screen |
| Battle Pass Season 2 finale | game-designer | Tier 30 free-track now grants Badger unlock |
| Battle Pass Season 3 start | game-designer | Tier 30 free-track grants Hedgehog unlock (rotation continues) |
| Daily missions pool expansion +5 | game-designer | Pool now 40 |
| Balance pass #6 | balance-engineer | |
| 90-day post-launch retrospective: D1 / D7 / D30 / ARPU vs targets | game-designer + foreground orchestrator | Documented in `decisions/` as ADR |

## Cadence summary table

| Cadence type | Total drops in 90 days |
|---|---|
| Cosmetic drops | 6 (1 every 2 weeks) |
| Balance passes | 6 (bi-weekly) |
| Monthly boss-rush events | 3 |
| Character spotlights | 2 (one every 4 weeks; Tortoise + Fox) |
| Battle Pass season turnovers | 2 (S1→S2, S2→S3) |
| New biome general-availability unlocks | 1 (Beach for non-Founders) |
| One-shot events | 2 (Sand Festival, Run-Rush leaderboard) |
| Monetization scarcity events | 1 (Founder Pass window close) |

Total live-ops surfaces fired in 90 days: **23 distinct beats**, with **no week containing zero drops.**

## Live-ops content pipeline (the CC0 + Blender thesis applied)

Every weekly cosmetic drop is shippable in ≤ 8 dev-hours because:

1. **CC0 source asset** from Quaternius Animated Animals (already on disk per `core/docs/asset-policy.md`).
2. **Blender recolor pass** (palette swap; 30-60 min per character mesh).
3. **JSON edit** in `data/cosmetics/<season>.json` to register the new skin (15 min).
4. **Store-screen art** auto-rendered from the in-game mesh + a 256×256 thumbnail render template (15 min per item).
5. **QA pass** — visual diff against the previous build (1-2 hours).

The pipeline survives **one developer + one weekend** per cosmetic drop. No paid asset purchases. No third-party APIs.

## Telemetry contract

Each event drop must report **at minimum** the following metrics to `logs/<game>/live-ops/`:

| Metric | Target |
|---|---|
| Event participation rate (DAU touching event surface) | ≥ 60% |
| Event completion rate (DAU completing event reward path) | ≥ 35% |
| Event-driven D2 lift vs control week | ≥ 5% |
| Cosmetic drop attach rate (% of DAU acquiring at least 1 drop item) | ≥ 25% |
| Balance pass churn (% of users churning within 3 days of patch) | ≤ 8% |

If any metric breaches for 2 consecutive drops, ADR-triggered review of cadence and pricing.

## Cuts (per `13-risks-and-cuts.md`)

If 90-day cadence cannot be sustained, cuts apply in this order:

1. **Drop monthly boss-rush** (high effort vs DAU lift; replace with another cosmetic drop)
2. **Reduce balance pass cadence to monthly** (bi-weekly is the ideal; monthly is the floor)
3. **Cut second character spotlight** (Tortoise spotlight stays; Fox spotlight defers)
4. **Cut Run-Rush leaderboard** (highest dev-effort one-shot; cuttable without retention loss)
5. **Cut new biome general-availability for non-Founders** (Beach stays Founder-only longer; pushes Founder Pass value)

Cosmetic drop cadence is **never cut**. If everything else falls, the cosmetic drop is the last drop standing — it is the cheapest, the lowest-risk, and the most reliable DAU-retention beat.

## Cross-references

- Live-ops cadence target (the contract this doc operationalizes): `02-meta-loop.md` §Live-ops cadence target.
- Monthly hero spotlight definition (free trial + discount mechanic): `02-meta-loop.md` + `09-monetization-design.md` (pricing on Spotlight Bundles).
- Boss roster for boss-rush events: `07-bosses.md`.
- Biome list for biome events: `06-biomes.md`.
- Battle pass tier 30 character rotation source of truth: `02-meta-loop.md`.
- CC0 asset pipeline policy: `core/docs/asset-policy.md`.
- Cut list reconciliation: `13-risks-and-cuts.md`.
- Telemetry pipeline (logs structure): `core/docs/observability.md` (framework, if it exists) + `06-tech-spec/` (tech-architect).
- Soft-launch market readouts: `01-research/03-positioning.md` (TR/PH/ID priority).
