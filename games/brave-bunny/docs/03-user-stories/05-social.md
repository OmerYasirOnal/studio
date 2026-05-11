# User Stories — Social

> Epic: share run-result cards, leaderboards, friend invites / referrals. Lightweight social per `docs/01-research/03-positioning.md` (TikTok-screenshot-friendly + soft-launch markets that play in friend-groups). Owner: ux-designer. Consumers: ui-engineer, systems-engineer. Sources cross-referenced: `docs/02-gdd/00-overview.md` (mode list — lobby includes social/leaderboard preview), `docs/02-gdd/narrative/00-tone-bible.md` (share-card copy key), `docs/01-research/03-positioning.md` (TikTok creator marketing playbook).

## Personas referenced

- **Casual Carla** — 32, TR commuter, 5-min sessions
- **Habby-fan Hakim** — 27, PH, ex-Survivor.io, depth-seeker
- **Family Fadia** — 38, ID, plays with toddler watching, low IAP propensity
- **Returning Rina** — 24, churned 7 days ago, returns via push

---

### US-55 Share run-result card as image to social platforms

**As a** Casual Carla
**I want** to share a clean, branded image of my run result to my socials
**So that** my friends see the game and I get to brag a little

**Acceptance criteria**
- [ ] Run-end screen has a "Share run" button; tapping it renders a 1080×1080 PNG with: hero portrait, biome, time, kills, "carrots earned", and game logo
- [ ] Card uses `{SHARE_CARD}: "{HERO} cleared the {BIOME} in {TIME}. Beat that?"` per tone bible
- [ ] Share opens the platform-native share sheet (iOS UIActivityViewController / Android Intent.ACTION_SEND)
- [ ] No personal info (player name, email, device ID) appears on the card

**Wireframe links:** TBD (`docs/05-wireframes/55-share-card.html`)
**Owners:** ui-engineer, art-director
**Depends on:** US-30
**Notes:** Positioning brief: "TikTok-screenshot-friendly". Card layout owned by art-director; technical render owned by ui-engineer.

---

### US-56 Weekly leaderboard with anonymized-by-default display

**As a** Habby-fan Hakim
**I want** a weekly leaderboard where I can see how my best run compares to others
**So that** I have a competitive long-tail goal

**Acceptance criteria**
- [ ] Leaderboard accessible from Home with a "Top runs this week" entry; resets every Monday 00:00 UTC
- [ ] Default display shows anonymized handles ("Brave Player 1234") with hero portrait; opt-in profile name in Settings
- [ ] Top 100 globally + player's own rank + ±5 neighbors visible without scroll-to-end
- [ ] No paid boost or "buy higher rank" option; leaderboard is gameplay-determined only

**Wireframe links:** TBD (`docs/05-wireframes/56-leaderboard.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Anonymized-by-default supports family-safe positioning + reduces moderation burden at launch.

---

### US-57 Referral code — both inviter and invitee get cosmetic

**As a** Casual Carla
**I want** to invite a friend with a code that gives us both a small cosmetic
**So that** I can bring my friends in without feeling like I am selling them ads

**Acceptance criteria**
- [ ] Settings → "Invite a friend" screen shows a 6-character referral code unique to the player
- [ ] Share button opens platform-native share sheet with copy: `{REFERRAL_SHARE}: "Try Brave Bunny with me — use code {CODE} for a free carrot hat."`
- [ ] Invitee enters code in Settings → "Got an invite?"; both players unlock a referral-exclusive cosmetic
- [ ] No tier-based referral rewards (no "invite 10, get premium" — keeps it gentle)

**Wireframe links:** TBD (`docs/05-wireframes/57-referral.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** —
**Notes:** Cosmetic-only reward per no-pay-to-win positioning. Per tone — "carrot hat" framing keeps voice warm.

---

### US-58 Weekly leaderboard reward — top 100 get cosmetic shard

**As a** Habby-fan Hakim
**I want** a small cosmetic reward for placing in the top 100 each week
**So that** the leaderboard climb has a tangible (but not power-affecting) payoff

**Acceptance criteria**
- [ ] On weekly reset, top 100 players receive a cosmetic shard via mailbox (US-40)
- [ ] Reward is identical for ranks 1–100; no top-3 power-up tier (cosmetic-only)
- [ ] Reward message uses tone bible voice: `{LB_REWARD}: "You ran a sturdy week. Here's a shiny ribbon."`
- [ ] No reward expiry; claim from mailbox at leisure

**Wireframe links:** TBD (`docs/05-wireframes/58-lb-reward.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-56, US-40
**Notes:** Encourages weekly return without creating pay-to-win pressure.

---

### US-59 Friends list — see who you invited and their best run

**As a** Casual Carla
**I want** to see a friends list of people I invited or who invited me
**So that** the leaderboard feels personal, not just a global rank

**Acceptance criteria**
- [ ] Friends list accessible from Home → "Friends"; lists referred + referring players with handle + best-run-this-week
- [ ] Friends list never auto-pulls contacts or device info
- [ ] Add by referral code; remove via long-press → "Stop following"
- [ ] No DM / chat at launch; this is a leaderboard-context list only

**Wireframe links:** TBD (`docs/05-wireframes/59-friends-list.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-57
**Notes:** Family-safe positioning: no free-form chat at launch reduces moderation burden.

---

### US-60 Hero-of-the-day spotlight on Home

**As a** Returning Rina
**I want** to see a "Hero of the day" community spotlight on Home (e.g. "@Brave Player 9821 ran the Carrot Fields in 6m 12s")
**So that** the game feels alive and other players exist

**Acceptance criteria**
- [ ] Home screen shows a Hero-of-the-day strip below the daily-streak strap, rotating once per day
- [ ] Featured run is anonymized handle + biome + time + hero portrait; cycled from top weekly leaderboard
- [ ] Tapping the strip deep-links to the leaderboard (US-56), pre-scrolled to that player's row
- [ ] Featured players cannot be friend-requested or messaged from the spotlight (read-only)

**Wireframe links:** TBD (`docs/05-wireframes/60-hero-of-day.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-56
**Notes:** Lightweight community presence; complements the lone-wolf loop without forcing social interaction.

---

### US-61 Block / report user — minimal moderation surface

**As a** Family Fadia
**I want** to be able to block / report a leaderboard handle that I find inappropriate
**So that** my game stays family-safe even with public displays

**Acceptance criteria**
- [ ] Long-press on any leaderboard row shows "Hide this player" + "Report handle" options
- [ ] "Hide" persists locally and removes that handle from all leaderboard views for this profile
- [ ] "Report" sends handle + timestamp to a moderation queue; confirmation modal: `{REPORT_SENT}: "Thanks. We'll take a look."`
- [ ] No public retaliation (player who blocked is not notified)

**Wireframe links:** TBD (`docs/05-wireframes/61-report-block.html`)
**Owners:** ui-engineer, systems-engineer
**Depends on:** US-56
**Notes:** Required for App Store guidelines compliance on any UGC surface (handles are UGC).

---

### US-62 Share milestone — auto-prompt to share on big wins

**As a** Casual Carla
**I want** the game to gently offer me a share option when I hit a real milestone (first evolution, first boss kill, top-10 weekly placement)
**So that** I do not miss the natural moment to brag

**Acceptance criteria**
- [ ] After first-time milestones, a small "Share this moment?" strip slides in on the run-end tally
- [ ] Strip is dismissable with no nag; appears at most once per milestone per profile (lifetime)
- [ ] Share renders a milestone-themed card variant (US-55 base + milestone badge overlay)
- [ ] Strip never appears during gameplay or before the tally finishes banking (per US-18)

**Wireframe links:** TBD (`docs/05-wireframes/62-share-milestone.html`)
**Owners:** ui-engineer
**Depends on:** US-55, US-18
**Notes:** Tone — `{SHARE_MILESTONE}: "First evolution! Worth a brag?"`. Per positioning brief, supports the TikTok creator playbook.

---

**Total stories in this file: 8**
