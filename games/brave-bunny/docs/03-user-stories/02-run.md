# User Stories — Run (in-game moment-to-moment)

> Epic: the 7–10 minute survival session, from biome-pick confirm through run-end tally. The most important user-story set — this is where retention is won or lost. Owner: ux-designer. Consumers: gameplay-engineer, ui-engineer, systems-engineer. Sources cross-referenced: `docs/02-gdd/01-core-loop.md` (run loop, auto-attack, pickups, draft), `docs/02-gdd/11-feel-pillars.md` (all 8 pillars), `docs/02-gdd/narrative/00-tone-bible.md` (in-run copy), `docs/01-research/03-positioning.md` (silhouette readability on iPhone SE 3).

## Personas referenced

- **Casual Carla** — 32, TR commuter, 5-min sessions
- **Habby-fan Hakim** — 27, PH, ex-Survivor.io, depth-seeker
- **Family Fadia** — 38, ID, plays with toddler watching, low IAP propensity
- **Returning Rina** — 24, churned 7 days ago, returns via push

---

### US-13 Joystick responsiveness within 1 frame

**As a** Habby-fan Hakim
**I want** the hero to begin moving within 1 frame of my thumb-down on the joystick
**So that** the run feels tight, like Survivor.io's gold-standard input

**Acceptance criteria**
- [ ] Pointer-down event to first hero-velocity update is ≤ 16.6 ms (1 frame at 60 fps) p99
- [ ] Joystick is a floating control: it spawns at thumb-down position within the bottom-left quadrant
- [ ] Joystick visual feedback (knob translates with thumb) renders within the same frame as the velocity update
- [ ] Dead-zone is 8% of stick radius; no jitter when thumb is centered

**Wireframe links:** TBD (`docs/05-wireframes/13-hud-joystick.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** —
**Notes:** Pillar 5 violation flag: any tap with no 1-frame response auto-fails QA. Test on iPhone SE 3 in landlocked grip.

---

### US-14 Level-up draft picks in under 2 seconds

**As a** Casual Carla
**I want** to read all 3 draft cards and pick one within 2 seconds
**So that** the run does not feel interrupted by a long menu

**Acceptance criteria**
- [ ] Draft cards display: icon, name (1 line), description (1 line), "current → next" stat delta in a single readable row
- [ ] Cards are sized ≥ 88 pt tap target (iOS HIG) per `01-core-loop.md` card-layout spec
- [ ] All 3 cards are visible on iPhone SE 3 (4.7" screen) in portrait without scroll
- [ ] Median pick time in playtests is ≤ 2.0 s (telemetry event: `draft_pick_seconds`)

**Wireframe links:** TBD (`docs/05-wireframes/14-levelup-draft.html`)
**Owners:** ui-engineer
**Depends on:** US-15
**Notes:** Tone — `{LEVEL_UP_PICK}: "You feel pluckier. Choose your gift."`. iPhone SE 3 fit is a known UX risk (flagged in summary).

---

### US-15 Banish one draft option per run

**As a** Habby-fan Hakim
**I want** to be able to remove a draft option from the future pool of this run
**So that** I can shape my build instead of being rerolled into the same junk

**Acceptance criteria**
- [ ] Each draft card has a small "🚫" affordance (icon only, no emoji per tone bible — use SVG ban glyph) in its top-right
- [ ] First banish per run is free; second costs a rewarded ad opt-in (per `01-core-loop.md`)
- [ ] Hard cap of 2 banishes per run total; the affordance hides on the 3rd draft
- [ ] Banished upgrade name is logged in run-end stats screen ("Sent home: Pebble Sling Lv 2")

**Wireframe links:** TBD (`docs/05-wireframes/15-draft-banish.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** US-14
**Notes:** Copy — `{DRAFT_BANISH}: "Send this gift home."`. The "🚫" placeholder maps to a custom SVG icon per art-bible — emoji are banned in copy.

---

### US-16 Pause, resume, and quit a run mid-session

**As a** Family Fadia
**I want** to be able to pause the run instantly when my toddler interrupts
**So that** I do not lose my progress to a real-life interruption

**Acceptance criteria**
- [ ] A pause button (≥ 88 pt) sits in the top-right safe area, not overlapping the joystick or HUD
- [ ] Pause halts game time, audio fades −12 dB over 200 ms, and a 3-option modal appears: Resume / Settings / Head home
- [ ] "Head home for now" confirms via secondary tap and banks all earned currency (per `01-core-loop.md` failure loop: 100% of gold/soul-shards/pass-XP preserved on quit-out)
- [ ] Resume restores audio and game speed over 200 ms ease-out

**Wireframe links:** TBD (`docs/05-wireframes/16-pause-modal.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** —
**Notes:** Tone — `{BTN_CONFIRM_QUIT_RUN}: "Head home for now."`. Quit-out treats the run as "death" for banking purposes.

---

### US-17 Boss intro card respects slow-mo dwell with skip

**As a** Habby-fan Hakim
**I want** the boss intro to feel like a moment, but to be skippable if I have seen it before
**So that** I get the cinematic vibe on first encounter without it slowing later runs

**Acceptance criteria**
- [ ] Boss intro triggers a 0.4x time-dilate for 600 ms, then a card slides in from the bottom with boss portrait + name + tone copy
- [ ] Card auto-dismisses after 1.8 s OR on any tap after the first 600 ms (no skip during the dwell — Feel Pillar 6 spirit)
- [ ] On subsequent encounters of the same boss in the same player profile, dwell is 200 ms and auto-dismiss is 800 ms
- [ ] Audio fanfare ducks combat SFX by −4 dB per Feel Pillar 8

**Wireframe links:** TBD (`docs/05-wireframes/17-boss-intro.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** —
**Notes:** Tone — `{BOSS_INTRO_BOAR}: "Old Boar's awake. Mind your tail."`. Vary copy per boss per tone bible section 5.

---

### US-18 Run-end tally shows banked currency before any IAP/ad surface

**As a** Casual Carla
**I want** to see how much gold, soul-shards, and pass-XP I banked before the game asks me for anything
**So that** I feel rewarded for the run before the game tries to monetize me

**Acceptance criteria**
- [ ] Run-end tally slides in from bottom over 400 ms (Feel Pillar 6) showing 3 lines: Gold, Soul-shards, Pass-XP
- [ ] Each line slams in series with a 250 ms count animation and tick-tick SFX at −9 dB
- [ ] First 800 ms of the tally is non-skippable (positioning brief: dopamine lands first)
- [ ] No IAP banner, no rewarded-ad offer, and no battle-pass nag appears until the tally is fully banked

**Wireframe links:** TBD (`docs/05-wireframes/18-runend-tally.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-26
**Notes:** Pillar 6 violation: tally skippable < 800 ms auto-fails QA. Copy — `{RUN_END_LOSE}: "Tuckered out — but you banked {GOLD} carrots."`

---

### US-19 Auto-attack visibility — player understands what is firing

**As a** Casual Carla
**I want** to see which weapon is firing at what target
**So that** I do not feel like the game is playing itself

**Acceptance criteria**
- [ ] Every weapon has a distinct visual signature: Carrot Spear (arc swipe), Pebble Sling (visible projectile), Honey Aura (radial pulse)
- [ ] Auto-target lock-on briefly highlights the target enemy with a 1-frame outline at −30% luminance
- [ ] Weapon icons in the HUD pulse on each fire event (alpha 0.6 → 1.0 over 80 ms)
- [ ] On iPhone SE 3, all weapon VFX read clearly at arm's-length distance per positioning silhouette test

**Wireframe links:** TBD (`docs/05-wireframes/19-hud-weapon-pulses.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** US-13
**Notes:** Feel Pillar 4 — auto-attack must have impact, including hit-flash and knockback.

---

### US-20 Boss attack telegraphs are readable

**As a** Habby-fan Hakim
**I want** to see clear "this is about to happen" indicators on boss attacks
**So that** my deaths feel fair, not random

**Acceptance criteria**
- [ ] Every boss attack has a ≥ 600 ms wind-up with a yellow zone indicator showing damage AoE
- [ ] Wind-up indicator scales from alpha 0 → 0.7 over the first half, holds, then flashes red in the final 200 ms
- [ ] AoE indicator stays on the ground for the full attack duration so the player can read it under VFX
- [ ] No boss attack is "true unavoidable" within the first 3 boss encounters of a new player profile

**Wireframe links:** TBD (`docs/05-wireframes/20-boss-telegraph.html`)
**Owners:** gameplay-engineer, level-designer
**Depends on:** US-17
**Notes:** This is a fairness pillar — depends on level-designer's boss attack spec for AoE shapes.

---

### US-21 HUD readability on iPhone SE 3

**As a** Family Fadia (ID, mid-tier device)
**I want** the HUD to be clearly readable on my older phone
**So that** I can play without squinting

**Acceptance criteria**
- [ ] All HUD numbers (HP, gold counter, kill counter, timer) render at ≥ 28 pt on iPhone SE 3 (4.7", 750×1334 logical)
- [ ] HP bar is the topmost HUD element with high-contrast outline (≥ 4:1 against background per WCAG AA)
- [ ] Joystick, pause button, and weapon icons live in fixed-position safe-area frames; never overlap during gameplay
- [ ] HUD adapts to notched devices (iPhone 12+) with safe-area insets; never clips behind the notch

**Wireframe links:** TBD (`docs/05-wireframes/21-hud-safe-area.html`)
**Owners:** ui-engineer
**Depends on:** —
**Notes:** iPhone SE 3 is a `GAME.md` target device. Silhouette + HUD legibility is a positioning bet.

---

### US-22 Revive offer is opt-in with larger decline button

**As a** Family Fadia
**I want** the revive offer to be one tap, with the decline option being the larger, easier button
**So that** I do not accidentally watch an ad I did not want

**Acceptance criteria**
- [ ] Revive modal appears at HP = 0 once per run only (per `01-core-loop.md`)
- [ ] Two buttons: "Watch ad to revive" (smaller, ≥ 88 pt) and "Head home" (larger, ≥ 120 pt, pre-focused)
- [ ] 5 s countdown auto-dismisses to "Head home" if no input
- [ ] Decline path skips directly to the run-end tally; accept path triggers rewarded-ad SDK with respawn at 50% HP + 1.5 s i-frames

**Wireframe links:** TBD (`docs/05-wireframes/22-revive-offer.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-18
**Notes:** Tone — `{HERO_REVIVE}: "Bunny got knocked silly. Want a quick nap and one more try?"`. Positioning: rewarded-ad-positive but not coercive.

---

### US-23 Upgrade descriptions show clear current → next deltas

**As a** Habby-fan Hakim
**I want** every draft card to show me exactly what the upgrade changes numerically
**So that** I can min-max my build instead of guessing

**Acceptance criteria**
- [ ] Each card body shows a numeric delta in the format: "Damage: 12 → 18 (+6)"
- [ ] For upgrades with no current level, the format is: "New: Honey Aura (radial DOT 4/s)"
- [ ] Stats use abbreviated keys (DMG, ATK SPD, CRIT %) with a long-press info tooltip for definitions
- [ ] Tooltip on long-press shows a 1-line description + stat formula reference; auto-dismisses on release

**Wireframe links:** TBD (`docs/05-wireframes/23-draft-tooltip.html`)
**Owners:** ui-engineer, balance-engineer
**Depends on:** US-14
**Notes:** Stat keys must match `data/balance/weapons.json` field names once balance-engineer authors them.

---

### US-24 Currency drop visibility — gold and hearts are unmistakable

**As a** Casual Carla
**I want** to instantly see when a gold coin or heart drops
**So that** I move toward it intentionally (gold auto-magnetizes, heart needs proximity)

**Acceptance criteria**
- [ ] Gold coins render at 1.5x the size of XP gems with a yellow shimmer particle effect
- [ ] Hearts render with a red pulse glow visible against any biome palette per art-bible
- [ ] On enemy death, gold auto-magnetizes from full screen with a 300 ms sweep arc (per `01-core-loop.md`)
- [ ] Heart magnetize radius is 2.5 units; visible "needs proximity" cue is its slow bob, not the gold sweep

**Wireframe links:** TBD (`docs/05-wireframes/24-currency-drops.html`)
**Owners:** gameplay-engineer, art-director
**Depends on:** —
**Notes:** Feel Pillar 3 — pickup is satisfying; chime pitch keyed by gem tier.

---

### US-25 Evolution recipe surfacing — "two gifts want to become one"

**As a** Habby-fan Hakim
**I want** the game to tell me when I have the prerequisites for a weapon evolution
**So that** I can plan toward the screen-filling finishers Survivor.io promised

**Acceptance criteria**
- [ ] When the player owns both ingredients for an evolution (e.g. Carrot Spear + Pebble Sling at Lv ≥ 5), the next draft surfaces the evolution card with a glowing border
- [ ] The evolution card uses the `{LEVEL_UP_EVOLVE}` copy key in its header strap
- [ ] Evolution card never appears if prerequisites are unmet; no "tease" cards
- [ ] On pick, the evolution plays a 1.2 s celebratory animation (golden burst + boss-tier screenshake at 4 px, 160 ms recovery)

**Wireframe links:** TBD (`docs/05-wireframes/25-evolution-card.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** US-14
**Notes:** Tone — `{LEVEL_UP_EVOLVE}: "Two gifts want to become one. Pick the pair."`

---

### US-26 Death feels dignified — celebration, not punishment

**As a** Casual Carla
**I want** my death to feel like a friendly end-of-run, not a "Game Over" screen
**So that** I return for another run instead of quitting the app

**Acceptance criteria**
- [ ] On HP = 0: time-dilates to 0.3x for 300 ms, then pause (Feel Pillar 6)
- [ ] Gold particle burst from hero corpse (60 particles, 2.0 unit radius, 600 ms lifetime)
- [ ] Camera dollies in 5% over 300 ms
- [ ] No string contains banned vocabulary ("killed", "died", "dead"); use `{RUN_END_LOSE}` from tone bible

**Wireframe links:** TBD (`docs/05-wireframes/26-death-celebration.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** —
**Notes:** Pillar 6 violation flag: any "Game Over" string auto-fails QA.

---

### US-27 Mid-run chest opens a free draft

**As a** Habby-fan Hakim
**I want** mid-run chests from elites to give me a free draft pick
**So that** elite kills feel meaningfully rewarded beyond gold

**Acceptance criteria**
- [ ] Chest pickup pauses the run identically to a level-up (0.4x dilate, 200 ms, then pause)
- [ ] Free draft shows 3 cards from the standard pool; banish and re-roll affordances apply
- [ ] Chest draft does NOT consume the banish or re-roll quota (they refresh per level-up only)
- [ ] Chest reward UI is visually distinguished by a chest-icon strap in the card header

**Wireframe links:** TBD (`docs/05-wireframes/27-chest-draft.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** US-14
**Notes:** Per `01-core-loop.md` pickup behaviors — chests guaranteed from elites and mid-run miniboss.

---

### US-28 Wave-pressure cue — player feels density increase

**As a** Habby-fan Hakim
**I want** to feel when a new wave or density spike is starting
**So that** I can react with positioning instead of being blindsided

**Acceptance criteria**
- [ ] Each wave change is announced by a 600 ms low rumble SFX at −9 dB + brief "wave X" strap on HUD
- [ ] Strap auto-fades over 800 ms; no tap required
- [ ] Density ramp follows `level-designer/waves.json` schedule; visible enemy count per Feel Pillar 7 minimums (8 at min 1, 80 at min 5)
- [ ] No wave change can occur during a level-up draft pause (waves resume on resume)

**Wireframe links:** TBD (`docs/05-wireframes/28-wave-strap.html`)
**Owners:** gameplay-engineer, level-designer, ui-engineer
**Depends on:** —
**Notes:** waves.json is owned by level-designer per `CLAUDE.md`; gameplay-engineer never modifies it.

---

### US-29 Re-roll affordance is single-tap, ad-gated on second use

**As a** Casual Carla
**I want** to re-roll a bad draft once for free
**So that** I can recover from a roll that does not fit my build

**Acceptance criteria**
- [ ] A re-roll "🎲" button (custom SVG, no emoji per tone bible) appears at the bottom of the draft UI
- [ ] First re-roll per run is free; second re-roll requires rewarded-ad opt-in
- [ ] Hard cap of 2 re-rolls per run; affordance hides after cap is reached
- [ ] Re-rolled cards animate out (top) and in (bottom) over 280 ms with the standard overshoot bezier

**Wireframe links:** TBD (`docs/05-wireframes/29-draft-reroll.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** US-14
**Notes:** Re-roll never produces the same 3-card set; balance-engineer must define duplicate-avoidance rules.

---

### US-30 Run-end "Play again" path is at most 2 taps

**As a** Casual Carla
**I want** to start a new run with at most 2 taps from the run-end tally
**So that** my next 5-minute session is friction-free

**Acceptance criteria**
- [ ] Run-end tally screen has 3 buttons: "Head home" (default), "Play again", "Share run"
- [ ] "Play again" boots a new run with the same biome + character + loadout in ≤ 4 s
- [ ] "Head home" returns to Home and the "Play" button is pre-focused
- [ ] No interstitial ad ever appears between run-end and "Play again"

**Wireframe links:** TBD (`docs/05-wireframes/30-runend-buttons.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-18
**Notes:** Positioning: "No interstitial ads ever during gameplay." Frictionless retry is a D1 retention driver.

---

**Total stories in this file: 18**
