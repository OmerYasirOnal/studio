# User Stories — Meta (between-run progression)

> Epic: home screen surfacing, character unlock, daily streak, missions, achievements, battle pass progression. Owner: ux-designer. Consumers: ui-engineer, systems-engineer. Sources cross-referenced: `docs/02-gdd/00-overview.md` (meta mode, no-paywall pillar), `docs/02-gdd/01-core-loop.md` (session loop, run-end currency banking), `docs/02-gdd/narrative/00-tone-bible.md` (voice for meta strings), `docs/01-research/03-positioning.md` (no energy gate, daily-streak replaces).

## Personas referenced

- **Casual Carla** — 32, TR commuter, 5-min sessions
- **Habby-fan Hakim** — 27, PH, ex-Survivor.io, depth-seeker
- **Family Fadia** — 38, ID, plays with toddler watching, low IAP propensity
- **Returning Rina** — 24, churned 7 days ago, returns via push

---

### US-31 Home screen surfaces "what to do next"

**As a** Returning Rina
**I want** the home screen to tell me what I should do this session
**So that** I do not stare at the menu wondering what is new

**Acceptance criteria**
- [ ] Home screen shows a single "Next thing" strap above the Play button (e.g. "Day 3 streak — claim it", "Battle pass tier 4 ready", "New character unlock available")
- [ ] Strap content prioritizes (in order): unclaimed daily streak → claimable battle-pass tier → unspent character-shard threshold → daily mission progress
- [ ] If nothing is pending, strap shows `{NEXT_NUDGE_PLAY}: "Off we go. Carrots await."`
- [ ] Tapping the strap deep-links to the relevant surface (streak modal, BP screen, character roster, mission tray)

**Wireframe links:** TBD (`docs/05-wireframes/31-home-next-thing.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** This is a re-engagement KPI driver; track `home_strap_tap_rate` per surface.

---

### US-32 Character unlock cost is clear and not RNG

**As a** Habby-fan Hakim
**I want** to know exactly how many soul-shards I need to unlock the next character — no gacha, no luck
**So that** I trust the progression and can plan my runs

**Acceptance criteria**
- [ ] Character roster screen shows each locked hero with a direct shard cost (e.g. "200 / 200" or "84 / 200 — needs 116 more")
- [ ] No spin animation, no "pull" button, no probability text — unlock is a single "Unlock" button when threshold met
- [ ] Locked heroes show the unlock cost in soul-shards; never in real-money currency
- [ ] Unlock action triggers a celebratory animation (1.2 s) and the hero appears in the loadout selector immediately

**Wireframe links:** TBD (`docs/05-wireframes/32-char-roster.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Positioning UVP #3: "No gear gacha". Cosmetic character recolors are separate from base unlocks; clarity is the pillar.

---

### US-33 Daily mission tray with 3 rotating goals

**As a** Casual Carla
**I want** 3 small daily missions I can clear in a single run
**So that** my 5-minute commute session feels accomplished

**Acceptance criteria**
- [ ] Daily mission tray surfaces on Home screen with 3 missions, each ≤ 1 line description, with progress bar
- [ ] All 3 missions are achievable within a single 7-minute run by an average-skill player
- [ ] Missions rotate at 04:00 local time; remaining time visible as "Resets in HH:MM"
- [ ] Each mission completion grants pass-XP + a small cosmetic shard reward; total daily reward bundle is fixed, not RNG

**Wireframe links:** TBD (`docs/05-wireframes/33-daily-missions.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Mission examples must use tone bible: `{MISSION_KILL_50}: "Send 50 rascals packing."`, not "Kill 50 enemies."

---

### US-34 Achievements list — lifetime stats and trophies

**As a** Habby-fan Hakim
**I want** a long-running achievements list with lifetime stats
**So that** I have a meta-goal beyond the current battle pass

**Acceptance criteria**
- [ ] Achievements screen lists ≥ 30 lifetime achievements at launch (e.g. "1000 rascals sent home", "First evolution discovered", "5-day streak")
- [ ] Each achievement shows: icon, name, 1-line description, progress bar (if numeric), claimed/unclaimed state
- [ ] Claim grants a one-time reward (cosmetic shards or gold); never a power upgrade
- [ ] Achievements section is sub-screen of Profile, accessible in ≤ 2 taps from Home

**Wireframe links:** TBD (`docs/05-wireframes/34-achievements.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Names use tone bible; no "destroy", "slay", or "kill". Use "send packing", "see off", "bonk".

---

### US-35 Battle pass progression visible after every run

**As a** Casual Carla
**I want** to see my battle pass progress update right after the run-end tally
**So that** the pass feels alive, not buried in a menu

**Acceptance criteria**
- [ ] After the run-end tally banks pass-XP, a 2-second mini-strip slides in showing "Tier X → Tier X+1" with the next 3 tiers' rewards
- [ ] If a tier was crossed during the run, the new tier's free + premium rewards animate "claim ready" with a soft pulse
- [ ] Strip is dismissable on tap; auto-advances to Home after 4 s if no input
- [ ] Premium track rewards remain visible but greyed if the pass is not owned, with a single "See pass" CTA

**Wireframe links:** TBD (`docs/05-wireframes/35-bp-progress-strip.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-18
**Notes:** Per `01-core-loop.md` — none of these gate "Play Again". The strip is informational, never a wall.

---

### US-36 No energy gate — Play button is always live

**As a** Habby-fan Hakim
**I want** the Play button to be enabled at all times — no energy, no stamina, no cooldown
**So that** I can play 10 runs back-to-back if I want

**Acceptance criteria**
- [ ] Home screen "Play" button is always tappable; no energy bar UI exists anywhere
- [ ] No modal, dialog, or counter blocks consecutive runs
- [ ] No "wait X minutes" cooldown text appears between any two runs
- [ ] Settings screen does not mention energy or stamina in any context

**Wireframe links:** TBD (`docs/05-wireframes/36-home-play-cta.html`)
**Owners:** ui-engineer
**Depends on:** —
**Notes:** Positioning brief is explicit: "No energy gate, ever." This is a top-3 differentiation pillar.

---

### US-37 Daily streak claim is 0-input

**As a** Returning Rina
**I want** my daily streak claim to be a "tap anywhere" gesture, not a maze of confirms
**So that** I can claim and return to the game in under 5 seconds

**Acceptance criteria**
- [ ] On first session of the day, a streak modal appears over Home with the day's reward animated front-and-center
- [ ] Single tap anywhere on the modal claims the reward and dismisses
- [ ] Missing a day resets the streak to day 1 but does NOT lock any content (per `01-core-loop.md` step contract)
- [ ] Day-7 milestone grants a guaranteed cosmetic shard with a 1.2 s celebratory animation

**Wireframe links:** TBD (`docs/05-wireframes/37-daily-streak.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Tone — `{DAILY_STREAK_HOOK}: "Three days running. Sturdy little adventurer."`

---

### US-38 Character-level upgrades — perma stats per hero

**As a** Habby-fan Hakim
**I want** to spend character XP on perma-stat upgrades for each hero
**So that** my favorite character grows with me run-over-run

**Acceptance criteria**
- [ ] Each hero has a character-level screen showing XP progress, current level, and 3 stat upgrade trees (HP, ATK, Move Speed)
- [ ] Upgrades cost gold + character XP, both deterministic
- [ ] Upgrade preview shows "current → next" delta identically to in-run draft cards (US-23 pattern)
- [ ] Upgrades unlock rare-tier draft pool entries at specific level thresholds (Bunny Lv 3 unlocks Carrot Bomb, Lv 5 unlocks Bouncing Cob)

**Wireframe links:** TBD (`docs/05-wireframes/38-char-level-upgrades.html`)
**Owners:** ui-engineer, balance-engineer
**Depends on:** US-32
**Notes:** Per `01-core-loop.md` draft details — locked rares unlock by character meta-level.

---

### US-39 Biome selector — locked biomes show requirement, not paywall

**As a** Casual Carla
**I want** to see exactly what I need to do to unlock a new biome
**So that** progression feels earnable, not gatekept by my wallet

**Acceptance criteria**
- [ ] Biome selector shows 5 horizontal tiles (single row, scrollable on small screens)
- [ ] Locked biomes display a single requirement string (e.g. "Reach Bunny Lv 5", "Clear 3 Big Bad Wolf bosses")
- [ ] No locked biome shows a "$ unlock now" option — unlock is always gameplay-gated
- [ ] Tapping a locked biome shows a non-blocking tooltip with the requirement; never a paywall modal

**Wireframe links:** TBD (`docs/05-wireframes/39-biome-selector.html`)
**Owners:** ui-engineer
**Depends on:** —
**Notes:** Positioning: "Locked biomes show a single requirement, never a paywall." This is a no-pay-to-win pillar.

---

### US-40 Mailbox for live-ops gifts and patch-note nudges

**As a** Returning Rina
**I want** a mailbox where catch-up rewards, event notes, and patch notes live
**So that** I have a single place to "catch up" after my churn period

**Acceptance criteria**
- [ ] Mailbox icon on Home shows a numeric badge of unread items
- [ ] Mailbox supports 3 message types: rewards (with claim button), event notices (info only), patch notes (text only)
- [ ] Reward messages show contents + expiry timer; claim is single-tap
- [ ] Read messages persist 14 days then auto-purge; rewards never auto-expire without 24h push warning

**Wireframe links:** TBD (`docs/05-wireframes/40-mailbox.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Tone — `{MAIL_GIFT_FROM_DEV}: "A small basket from the team. Thanks for sticking around."`

---

### US-41 Loadout slot for cosmetic skins (preview without purchase)

**As a** Family Fadia
**I want** to preview a cosmetic skin on my character before buying
**So that** I do not buy something that looks weird in-game

**Acceptance criteria**
- [ ] Loadout slot for character cosmetics opens a horizontal carousel of owned + previewable skins
- [ ] Previewable (un-owned) skins show on the 3D model rotating against a neutral backdrop with a "Preview" tag
- [ ] Locked skins show price + "Buy" button or shard cost; no auto-buy on tap
- [ ] Equipped skin saves immediately to local profile; no confirm dialog needed

**Wireframe links:** TBD (`docs/05-wireframes/41-loadout-cosmetic.html`)
**Owners:** ui-engineer
**Depends on:** US-43
**Notes:** Cosmetics are the primary monetization vector per `00-overview.md` mode list — preview must be friction-free.

---

### US-42 Profile screen — lifetime stats and "Hero of the day"

**As a** Habby-fan Hakim
**I want** a profile screen with my lifetime numbers and a "best of recent" highlight
**So that** my long-haul play is visible and shareable

**Acceptance criteria**
- [ ] Profile shows: total runs, total rascals sent home, longest run time, total gold banked, top character by playtime
- [ ] "Hero of the day" card highlights the player's best run in the last 24h (DPS / longest / most kills)
- [ ] All stats use tone-bible vocabulary ("rascals sent home", never "kills")
- [ ] Share-card export uses Profile + Hero-of-the-day data (links to US-55)

**Wireframe links:** TBD (`docs/05-wireframes/42-profile-stats.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Lifetime stat keys must match `data/balance/` schema once balance-engineer authors them.

---

**Total stories in this file: 12**
