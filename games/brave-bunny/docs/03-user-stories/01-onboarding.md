# User Stories — Onboarding

> Epic: first-launch experience, returning-user shortcuts, permission flows, and the cold-start path from app icon tap to first kill. Owner: ux-designer. Consumers: ui-engineer, gameplay-engineer, systems-engineer. Sources cross-referenced: `docs/02-gdd/00-overview.md` (mode list, no-paywall pillar), `docs/02-gdd/01-core-loop.md` (session loop), `docs/02-gdd/11-feel-pillars.md` (Pillar 5 UI responsiveness), `docs/02-gdd/narrative/00-tone-bible.md` (voice + copy keys), `docs/01-research/03-positioning.md` (TR/PH/ID soft launch; no login gate).

## Personas referenced

- **Casual Carla** — 32, TR commuter, 5-min sessions
- **Habby-fan Hakim** — 27, PH, ex-Survivor.io, depth-seeker
- **Family Fadia** — 38, ID, plays with toddler watching, low IAP propensity
- **Returning Rina** — 24, churned 7 days ago, returns via push

---

### US-01 First-launch FTUE under 60 seconds

**As a** Casual Carla
**I want** the first-launch tutorial to end and drop me into a real run in under 60 seconds
**So that** I do not abandon before I see the game I downloaded for

**Acceptance criteria**
- [ ] Cold-start to interactive Home screen completes in ≤ 8 s on iPhone SE 3 (per `GAME.md` target devices)
- [ ] FTUE consists of ≤ 3 coach-mark steps, each auto-dismissing on the demonstrated action (move, pick up gem, level-up draft)
- [ ] Total elapsed time from app icon tap to "first kill" telemetry event is ≤ 60 s in the median session
- [ ] No narrative cutscene blocks the first run; world premise reveal is deferred to between-run mailbox

**Wireframe links:** TBD (`docs/05-wireframes/01-ftue-coachmarks.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** —
**Notes:** Tone bible — coach-mark copy uses friendly-older-sibling voice; e.g. `{FTUE_MOVE}: "Drag your thumb. Bunny will follow."`. Record all FTUE strings as `{FTUE_*}` keys.

---

### US-02 Skip onboarding for returning users

**As a** Returning Rina
**I want** the game to recognize me as a returning player and skip the FTUE entirely
**So that** I am not insulted by being re-taught controls I already know

**Acceptance criteria**
- [ ] If `playerProfile.firstRunCompleted == true`, the FTUE coach-mark layer is never instantiated
- [ ] Returning-user path on cold start goes: splash → Home (with daily-streak modal) in ≤ 6 s
- [ ] The daily-streak modal does not show coach marks; it is a single-tap dismiss per `01-core-loop.md` step contract
- [ ] A "re-tour" option exists in Settings for users who want to replay the FTUE voluntarily

**Wireframe links:** TBD (`docs/05-wireframes/02-home-returning.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-01
**Notes:** Settings entry copy: `{SETTINGS_REPLAY_FTUE}: "Show me the basics again."`

---

### US-03 Permission requests deferred until value is demonstrated

**As a** Family Fadia
**I want** the game to wait until I have completed at least one run before asking for notification or ATT permissions
**So that** I can evaluate whether the game is worth giving permission to

**Acceptance criteria**
- [ ] No iOS ATT (App Tracking Transparency) prompt fires on first launch
- [ ] No notifications-permission prompt fires before the first run-end tally
- [ ] ATT prompt is presented at the earliest after the second run-end tally, with a preamble explaining the "ads stay relevant" value
- [ ] Notification prompt is presented after the user opts in to the daily-streak reminder toggle in Home

**Wireframe links:** TBD (`docs/05-wireframes/03-permission-preamble.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-01
**Notes:** Preamble copy uses tone bible; e.g. `{ATT_PREAMBLE}: "We use this to keep ads friendly. Up to you."`

---

### US-04 Cold-start to first kill in 30 seconds

**As a** Habby-fan Hakim
**I want** to be killing enemies within 30 seconds of tapping the app icon
**So that** I feel competent before I have to learn anything

**Acceptance criteria**
- [ ] First-launch path is: splash (2 s) → 1 coach-mark "Drag thumb" → auto-spawned 8-enemy meadow → first kill within 30 s p95
- [ ] Enemy density in the FTUE run starts at 8 (per Feel Pillar 7 minimum) and the first 3 enemies are within 4 units of the player spawn
- [ ] No menu screen sits between splash and the FTUE run on first launch
- [ ] The first kill triggers a screenshake of 2 px and a kill-stinger SFX (Feel Pillar 1) — no FTUE text covers this moment

**Wireframe links:** TBD (`docs/05-wireframes/04-ftue-first-kill.html`)
**Owners:** gameplay-engineer, ui-engineer
**Depends on:** US-01
**Notes:** This is the single most important onboarding KPI; track as `ftue_first_kill_seconds` in analytics.

---

### US-05 Joystick coach mark demonstrates and dismisses

**As a** Casual Carla
**I want** the joystick coach mark to appear, demonstrate a drag motion, and dismiss the moment I copy it
**So that** I am not forced to read a wall of tutorial text

**Acceptance criteria**
- [ ] On first run, a pulsing ring + ghost-thumb animation appears over the bottom-left joystick region within 1 s of run start
- [ ] The coach mark dismisses the instant the player's first input-down event registers
- [ ] If the player does nothing for 8 s, the coach mark re-pulses (not re-explains) at 2x amplitude
- [ ] Coach-mark copy uses only `{FTUE_MOVE}` key; no inline English text in UXML

**Wireframe links:** TBD (`docs/05-wireframes/05-coachmark-joystick.html`)
**Owners:** ui-engineer
**Depends on:** US-01, US-04
**Notes:** Pillar 5 — tap-acknowledgment within 1 frame applies to coach-mark dismissal too.

---

### US-06 Level-up draft coach mark teaches the 3-of-N pick

**As a** Habby-fan Hakim
**I want** the first level-up to be unmistakable — even if I have played a Survivor-like before
**So that** I understand the genre contract within Brave Bunny's specific UI

**Acceptance criteria**
- [ ] First level-up uses the standard draft pause (0.4x time-dilate for 200 ms, then full pause per Feel Pillar 2)
- [ ] A 1-line coach mark `{FTUE_DRAFT}` appears above the 3 cards on first level-up only, never on subsequent level-ups in the run
- [ ] The 3 cards are pre-filtered to easy-to-read upgrades on the first level-up (no rare-tier silhouettes shown)
- [ ] On card tap, the coach mark fades over 120 ms and the run resumes per the standard contract

**Wireframe links:** TBD (`docs/05-wireframes/06-coachmark-draft.html`)
**Owners:** ui-engineer, gameplay-engineer
**Depends on:** US-04
**Notes:** Tone — `{FTUE_DRAFT}: "Pick a gift. You can only take one."`

---

### US-07 Pickup behavior is taught implicitly, not narrated

**As a** Family Fadia
**I want** XP gems to magnetize to the player without me being told what they are
**So that** my toddler watching can also follow what is happening on screen

**Acceptance criteria**
- [ ] First 3 enemy kills in the FTUE drop a small XP gem within 1.5 unit radius of the player (per `01-core-loop.md` magnet radius)
- [ ] Gem magnetize + scale-pulse + chime fires per Feel Pillar 3 spec on first contact
- [ ] No text overlay or coach mark explains "this is XP" — the bar fill is the explanation
- [ ] If the player has not picked up a gem within 15 s of the first enemy spawn, a 2-particle sparkle ping highlights the nearest gem

**Wireframe links:** TBD (`docs/05-wireframes/07-pickup-implicit.html`)
**Owners:** gameplay-engineer
**Depends on:** US-04
**Notes:** Implicit teaching is a positioning bet — family-safe + low-literacy-tolerant.

---

### US-08 No login wall — local profile by default

**As a** Returning Rina
**I want** to play without creating an account or logging into a social platform
**So that** I can re-engage in seconds when a push notification pulls me back

**Acceptance criteria**
- [ ] First launch creates a local-anonymous profile keyed to device ID; no email, no password, no third-party login required
- [ ] Cloud-save linkage (Apple ID / Google Play Games) is offered later in Settings as opt-in, never on first launch
- [ ] All meta progress (gold, soul-shards, character XP) writes to local profile from session 1
- [ ] Account-link offer appears in Settings with copy `{ACCOUNT_LINK_OFFER}`, no nag modal on Home

**Wireframe links:** TBD (`docs/05-wireframes/08-settings-account.html`)
**Owners:** systems-engineer, ui-engineer
**Depends on:** —
**Notes:** Per positioning brief: "No login-gated content beyond a daily check-in for streak bonus."

---

### US-09 Home-screen tour after first run-end

**As a** Casual Carla
**I want** a short, opt-skip tour of the Home screen after my first run ends
**So that** I know where the shop, battle pass, and biome picker live

**Acceptance criteria**
- [ ] After the first run-end tally dismisses, a 3-step pulse tour highlights: biome selector, character/loadout slot, "Play" button
- [ ] Each step auto-advances on tap of the highlighted region OR auto-dismisses after 4 s of dwell
- [ ] A "Skip tour" button is always visible at top-right; tapping it cancels remaining steps and never re-shows them
- [ ] Tour does not cover the battle pass or store icons — those are taught the first time the player taps near them

**Wireframe links:** TBD (`docs/05-wireframes/09-home-tour.html`)
**Owners:** ui-engineer
**Depends on:** US-01
**Notes:** Tone — `{TOUR_SKIP}: "Skip the tour."`, `{TOUR_STEP_BIOME}: "Pick where to go next."`

---

### US-10 Soft-language detection with TR-first fallback

**As a** Casual Carla (TR)
**I want** the game to launch in Turkish by default if my device is set to TR
**So that** I do not have to dig in Settings to find my language

**Acceptance criteria**
- [ ] On first launch, the game reads `NSLocale.preferredLanguages` (iOS) / `Locale.getDefault()` (Android) and maps to: TR → Turkish, * → English
- [ ] A language-confirm modal appears once on first launch: "Devam et / Continue", showing both options
- [ ] Selected language persists to local profile and is changeable in Settings any time
- [ ] All onboarding copy is available in both TR and EN at launch per tone bible section 6

**Wireframe links:** TBD (`docs/05-wireframes/10-language-confirm.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-01
**Notes:** PH/ID ship in EN at launch per positioning; do not show TR as an option there.

---

### US-11 Audio + haptic preference set early but not blocking

**As a** Family Fadia
**I want** to be able to mute the game on first launch in one tap
**So that** I can play with my toddler asleep next to me

**Acceptance criteria**
- [ ] A small speaker icon is visible in the top-right of the FTUE coach-mark layer and in Home
- [ ] Tapping the speaker icon toggles master mute and persists; no modal, no confirm
- [ ] Mute state is reflected in the icon glyph immediately (per Feel Pillar 5: 1-frame response)
- [ ] Haptics default to ON for iPhone, OFF for Android; togglable in Settings

**Wireframe links:** TBD (`docs/05-wireframes/11-mute-toggle.html`)
**Owners:** ui-engineer
**Depends on:** US-01
**Notes:** Tone bible Pillar 8 — audio mix has 6 dB headroom; mute must not skip the fanfare logic, only silence output.

---

### US-12 First run is winnable (boss not in FTUE)

**As a** Casual Carla
**I want** my first run to end in a clear, friendly result — not a hard boss fight that murders me
**So that** I leave session 1 feeling competent and want to return

**Acceptance criteria**
- [ ] FTUE run timer is fixed at 3 minutes (not 7–10) and ends in a "Whew. Worth a carrot." win-screen, not a boss encounter
- [ ] Wave density in FTUE caps at 25 enemies on-screen; no mid-run miniboss
- [ ] Player cannot die in FTUE — HP floor is 1, with a brief "Bunny got a scrape!" coach-mark on first damage taken
- [ ] FTUE run-end tally banks ~50 gold and 0 soul-shards; soul-shards are introduced in run 2

**Wireframe links:** TBD (`docs/05-wireframes/12-ftue-runend.html`)
**Owners:** gameplay-engineer, level-designer
**Depends on:** US-04
**Notes:** Tone — `{FTUE_END}: "Nice work. Off home for some carrots."`. Banked currency animation per Feel Pillar 6 spec.

---

**Total stories in this file: 12**
