# GDD 10 — Onboarding

> The first-60-seconds, first-session, and first-3-sessions experience for Brave Bunny. Sister docs: `00-overview.md` (pillars + north-star D1 retention target), `01-core-loop.md` (run-start contract), `06-biomes.md` (Meadow as the calibration biome), `11-feel-pillars.md` (the 0.5 s smile-test for kill feel), `narrative/00-tone-bible.md` (voice for all on-screen copy), `02-meta-loop.md` (daily streak claim mechanic). Owner: game-designer with hand-off to ui-engineer for tutorial pop-up shape and ux-designer for the flow diagram in `04-ux-flows/`.

## Design philosophy

D1 retention ≥ 40% in TR / PH / ID is the **north-star metric** (per `00-overview.md`). Every onboarding decision points back to that number. Three rules:

1. **Get to the kill before the lesson.** The player's first auto-attack fires within 15 seconds of cold-start. The first enemy dies within 18 seconds. The first smile happens before the first tutorial pop-up.
2. **Withhold complexity until earned.** No draft explanation until the first level-up triggers. No daily streak explanation until the second session. No battle pass explanation until session 3.
3. **Permission-deferral is sacred.** Notification permission is NEVER asked in the first session. It is asked **only on session 4 launch**, after the player has chosen to return.

## First 60 seconds — second-by-second script

Cold-start from app-icon tap to first enemy kill. Total: **18 seconds to first kill, 60 seconds to first draft.**

```
00:00  splash (Unity logo)                              ≤ 2 s
00:02  splash (Brave Bunny logo)                        ≤ 2 s
00:04  title screen — "Tap to start"                    auto-jump after 3 s if untapped
00:07  narrative card (skippable on tap, ≤ 4 s)
       Copy: "Bunny hops out of the burrow.
              The carrots are missing."
00:11  first run begins — Meadow, day, calm
       Camera: top-down 3/4, default zoom
       Joystick: visible in bottom-left, idle (subtle pulse VFX)
       Bunny: spawned arena center, idle animation
00:12  silent on-screen hint: joystick pulses brighter, "[move]" label fades in
       (no modal — diegetic UI hint only)
00:14  first auto-attack fires (Carrot Spear cone) — visible to player
       even if no enemies in range (the swoosh teaches "the bunny attacks for you")
00:16  first hop-slime swarmer appears at arena edge, walks toward bunny
00:18  first kill — spear connects, slime puffs into a small green cloud,
       kill-shake (50 ms screen-shake) + +1 XP gem pops
       SFX: soft "bop" + chime
       This is the 0.5-second smile-test moment.
00:19  on-screen hint fades: "[walk over the gem to pick it up]"
00:21  bunny picks up first XP gem — UI XP bar advances ~10%
00:25  3 more swarmers spawn from arena edges
       Player has now learned: move + auto-attack + pickup
00:30  swarmer count climbs to ~8; player starts dodging and farming kills
00:40  XP bar reaches first level-up threshold
       Game pauses, screen dims, **first draft modal appears**
00:42  draft modal: 3 options presented vertically (per `05-wireframes/` iPhone SE 3 fit)
       Copy on each card: weapon name + 1-line effect ("Pebble Sling — fires
       small stones at nearest enemies")
       Above the cards: "You feel pluckier. Choose your gift."
       (from `narrative/00-tone-bible.md` §5 — {LEVEL_UP_PICK})
00:45  player taps an option — modal dismisses, run resumes,
       new weapon visible (Pebble Sling fires immediately)
00:50  swarmer pressure continues at calibration density (10-15 on screen)
00:60  player has cleared first wave, has 2 weapons firing,
       feels in control — "I get it"
```

### Critical timing gates

| Gate | Target | Why |
|---|---|---|
| Cold-start to first input | ≤ 11 s | Anything longer and the player puts the phone down |
| First auto-attack | ≤ 15 s | The auto-attack IS the game's first promise |
| First kill | ≤ 18 s | First "feel" moment; per `11-feel-pillars.md` smile-test |
| First draft | ≤ 45 s | First strategic choice; the "build crafting" pillar lands |
| Second draft | ≤ 90 s | Confirms pacing (drafts at ~30-45 s intervals early run) |
| First mid-boss preview | ≤ 5 min | Player sees there is escalation to chase |

### Diegetic UI vs modal pop-ups

- **00:12 joystick hint** — diegetic (subtle pulse + text overlay on joystick). NOT a modal.
- **00:19 pickup hint** — diegetic (text floats above first gem). NOT a modal.
- **00:40 first draft** — modal (game pauses; cannot be diegetic because the draft requires a choice).
- After first draft, **no more modals until run-end summary.** The player has the controls; do not interrupt them.

## First session (5-7 minutes)

Beyond the 60-second mark, the first session should land:

| Time | Beat | Player learns |
|---|---|---|
| 0:45 | First weapon picked from draft | "I choose my build" |
| 1:30 | Second draft fires | "There will be many of these" |
| 2:00 | First tank enemy spawns, telegraphs charge | "Some enemies hit hard, watch the wind-up" |
| 3:00 | Third draft offers a passive (e.g., +10% movespeed) | "Passives stack into builds" |
| 5:00 | First mid-boss event — Old Boar King preview appearance (cosmetic-only at minute 5 in first run; he just walks across the arena and exits) | "Bigger fights are coming" |
| 7:00 | Player dies or wins (~70% of first runs end at minute 5-7 per pacing intent) | run-end summary screen |

### Run-end summary screen (first session)

The run-end summary is the **first time the player sees the meta-loop.** It must read as a reward, not a wall.

| Element | Behavior on first run-end |
|---|---|
| Top banner | "Tuckered out — but you banked 247 carrots." (per `narrative/00-tone-bible.md` {RUN_END_LOSE}) |
| Carrots earned | Counter animates up from 0 (1.5 s) — the dopamine bar |
| Soul Shards earned | Counter animates up (1.0 s); tooltip: "Bank these for runes — coming soon" (per `08-economy.md`) |
| XP earned | Character XP bar visible, advances |
| Rewarded ad surface | "Double rewards?" button (per `09-monetization-design.md` surface 2) — visible but not pulse-pushed |
| Primary CTA | "Off we go" (start next run, per `narrative/00-tone-bible.md` {BTN_CONFIRM_START_RUN}) |
| Secondary CTA | "Head home for now" (return to home screen) |

**Withheld from first run-end summary:** battle pass progress (introduced session 3), daily mission completion (introduced session 2), share card (introduced after session 3 with at least one win).

### What is withheld in session 1

- Daily streak system (introduced day 2, on return)
- Battle pass tab (visible but greyed with "First runs first" tooltip)
- Character roster screen (visible but only Bunny selectable; tap on Tortoise shows unlock cost without a sales push)
- Notification permission (deferred per the sacred rule)
- IAP store (visible only as a wallet tile; no pop-ups, no banners)

## First 3 sessions

A "session" = an app launch followed by ≥1 run. The 3-session arc is engineered to drip-feed every meta system **once the player has chosen to return**.

### Session 1 (the cold-start session)

Covered above. Withhold everything that isn't run + draft + run-end summary.

### Session 2 (first return)

The single highest-leverage retention moment. If the player returns once, D2 conversion is ~70%; if they return twice, D7 jumps materially.

| Beat | Behavior |
|---|---|
| Cold-start | Splash + logo (skippable), no narrative card this time |
| Home screen | Daily streak modal fires **first** — day 2 reward (150 Carrots + 1 random shard per `02-meta-loop.md`) |
| Streak modal copy | "Three days running. Sturdy little adventurer." (per `narrative/00-tone-bible.md` {DAILY_STREAK_HOOK} — note: day 2 uses a softer variant: "Back again. Welcome.") |
| Streak modal CTA | "Take it." (per {BTN_CONFIRM_UPGRADE}) — single button, no upsell |
| Post-claim | Home screen, biome selector shows Meadow only (locked biomes visible but greyed) |
| First run | Identical to session 1 — Meadow, default loadout |
| Run-end summary | Daily mission completion appears (1 of 3 missions auto-completes from this run) — first introduction to daily missions |
| Tutorial pop-up | One small toast: "Daily missions reset every day. Three quick goals." (auto-dismiss after 4 s) |

### Session 3 (the conversion gate)

| Beat | Behavior |
|---|---|
| Cold-start | Splash + logo, no narrative |
| Home screen | Daily streak day 3 (250 Carrots) |
| First run | Player likely now has enough Carrots to make first character meta-level purchase — character upgrade tab gets a single pulse indicator |
| Mid-session | Battle pass tab UN-greys; first tutorial pop-up explains the pass shape: "30 tiers, 4 weeks. Free track + premium track. Play and earn." Auto-dismiss after 6 s. No purchase prompt. |
| Run-end summary | Battle pass progress bar advances visibly (~1 tier per ~2 runs at calibration earn rate) |
| Withheld | Subscription stack still not pitched; IAP store still no banner; Founder Pass still on home screen as a single static banner (no animation, no countdown) |

### Notification permission deferral

**Notification permission is requested on session 4 launch, not earlier.**

| Trigger | Behavior |
|---|---|
| Session 1-3 launch | No permission request, no system dialog, no in-game prompt |
| Session 4 launch | **Pre-prompt modal** appears: "Want a friendly nudge when your daily streak resets? We'll never spam you." Two options: "Sure, send nudges" → triggers system permission dialog. "Not now" → defers indefinitely; never re-asked unless player opts in via Settings. |
| Pre-prompt copy ownership | `narrative/00-tone-bible.md` voice — soft, opt-in, no FOMO |

Per `03-positioning.md` "no login-gated content beyond a daily check-in" — notifications are likewise gated by **player invitation**, not by app gate.

## Tutorial pop-up policy (text-light)

| Rule | Why |
|---|---|
| Max 1 modal pop-up per minute of play | Anything denser breaks the "no interruption" feel |
| All tutorial pop-ups auto-dismiss after 4-6 s | Player should not be required to tap to continue play |
| All tutorial pop-ups are diegetic where possible (text floats over the relevant UI element, not centered) | Modal-center pop-ups feel like a quiz; diegetic feels like a friend pointing |
| No "Continue / Next" stepped tutorials | One pop-up, one idea, then back to play |
| Withhold = default; reveal = on first encounter | The player never sees a tutorial for a system they have not yet touched |

### Gates removed when learned

Some onboarding gates remain until the player completes their first instance of an action:

| Gate | Removed when |
|---|---|
| "Daily streak" home-screen highlight | Player claims streak once |
| Battle pass tab grey-out | Player completes session 3 |
| Character roster unlock-cost-only display | Player earns first Stars |
| Founder Pass home-screen banner | Player either dismisses (cosmetic X) or buys; never re-spawns unless season changes |

## Returning user flow

### 7-day returner (gap < 7 days)

- Daily streak: still within 2-day skip tolerance per `02-meta-loop.md`; if returned within 2 days of last claim, streak day continues from where it left off. If returned 3-6 days later, streak resets to day 1 but no UI shaming — just "Welcome back. Day 1 ready."
- Mailbox: 1 small "We missed you" mail with 100 Carrots gift. One-shot per gap event.
- Home screen: identical to active-user home; no special "win you back" hard sells.
- First run: identical pacing; difficulty unchanged.

### 30-day returner (gap ≥ 30 days)

- Daily streak: reset to day 1, treated as a fresh start.
- Mailbox: "Welcome back" pack — 500 Carrots + 50 Stars + 1 free common cosmetic shard. One-shot.
- Home screen: "Catch-up panel" appears once — a single screen summarizing what they missed: any biome unlocks they would have earned by playing (now available for unlock at half the usual carrots-cost — but **never** at zero, and **never** by paying Stars), current battle pass season info, current Founder Pass status.
- Catch-up panel copy: "Welcome home, adventurer. Here's what's new." Per `narrative/00-tone-bible.md` voice.
- First run: identical Meadow start; the game does not assume they remember the controls.
- After first returned-session run, a soft tutorial refresher fires for **draft mechanics only** ("Three offers. Pick one.") — auto-dismiss in 5 s.
- Notification re-prompt: at the END of their first returned-session, a single soft re-prompt for notifications if they had previously declined: "Want a friendly nudge so you don't lose another month?" Once-only per return event.

### 90+ day returner

Treated as 30-day returner mechanically. Catch-up panel adds a one-paragraph "What's new since you last played" — biomes added, characters added, current event. Pulled from a curated changelog (live-ops + narrative-designer).

## Cross-references

- D1 retention target: `00-overview.md` north-star metric.
- Voice for all copy: `narrative/00-tone-bible.md`.
- Daily streak day-by-day rewards: `02-meta-loop.md`.
- Smile-test on first kill: `11-feel-pillars.md` (existing) — kill-shake, hitstop, gem pop.
- Meadow as calibration biome: `06-biomes.md`.
- Run-start contract (cold-start to first input flow): `01-core-loop.md`.
- Run-end summary screen wireframe: `05-wireframes/` (ux-designer, to be authored).
- Onboarding flow diagram: `04-ux-flows/onboarding.mmd` (ux-designer, to be authored — Mermaid).
- Notification permission integration: `06-tech-spec/` (tech-architect, to be authored).
- IAP store visibility rules during onboarding: `09-monetization-design.md`.
