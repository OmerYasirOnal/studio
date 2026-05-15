# Brave Bunny — Soft-Launch Readiness Checklist

> **Target markets:** Turkey (TR), Philippines (PH), Indonesia (ID)
> **Target date:** 2026-07-15
> **Owner:** sole developer + orchestrator agent
> **Build target at launch:** TestFlight build #N (placeholder — to be filled before submission)

This checklist is the single page that decides "we ship soft-launch this week / we don't." Every item must be `[x]` or have an ADR explaining the deferral.

---

## 1. Build & store

- [ ] **Latest production build #N uploaded to TestFlight** and processed without ITMS warnings.
- [ ] **Latest production build uploaded to Google Play Internal Testing** track.
- [ ] **App Store Connect metadata complete in EN, TR, FIL, ID** — title, subtitle, keywords, description, what's new.
- [ ] **Google Play store listing localised** for TR, FIL, ID.
- [ ] **Screenshots** generated for all required device sizes (6.9", 6.5", 5.5", iPad if iPad-supported, Android phone, Android tablet).
- [ ] **App icon final** at all required sizes.
- [ ] **Promo video** uploaded (optional, but lifts CVR ~ 15%) or explicit ADR deferring it.
- [ ] **In-app rating prompt** wired (SKStoreReviewController / Play In-App Review) and gated to >= 3rd completed run.
- [ ] **App Privacy questionnaire (App Store)** completed and matches `docs/legal/privacy-policy.md` exactly.
- [ ] **Data Safety form (Google Play)** completed and matches privacy policy.
- [ ] **Age rating** confirmed: 9+ Apple / Everyone 10+ Google / PEGI 7. Cartoon violence only, no blood, no real-world themes.

## 2. Legal & compliance

- [ ] **Privacy policy URL live** at `https://bravebunny.example/privacy` (or hosted equivalent) and **linked from App Store Connect + Google Play console**.
- [ ] **Terms of Service URL live** at `https://bravebunny.example/terms`.
- [ ] **Support URL live** at `https://bravebunny.example/support` (can be a static "email us" page).
- [ ] **In-game Settings → Legal** screen links to both URLs.
- [ ] **Credits screen** lists every CC0/OFL/MIT/CC-BY asset with attribution (per `core/docs/asset-policy.md`).
- [ ] **Encryption export compliance** answered on App Store Connect (TLS-only via mail = standard exempt).
- [ ] **Apple Developer Program agreements** all accepted; payment + tax forms complete (Paid Apps agreement required for IAP).
- [ ] **Google Play developer agreements** all accepted; payment profile verified.

## 3. IAP

- [ ] **All SKUs created** in App Store Connect & Google Play Console with localised TR/PH/ID prices.
- [ ] **No SKU above $19.99** (positioning pledge).
- [ ] **No tier-skip pack above $4.99**.
- [ ] **Restore Purchases** flow tested on iOS and Android with a fresh install on a second device.
- [ ] **Subscription Monthly Card** auto-renew tested + cancel-from-settings tested.
- [ ] **Receipt validation** runs locally on app and survives offline relaunch.
- [ ] **Refund webhook** (Apple S2S, Google RTDN) — **deferred for soft-launch**, ADR-NNNN documenting deferral.

## 4. Telemetry & crash

- [ ] **Local telemetry JSONL** writes correctly on iOS and Android sandboxes.
- [ ] **Telemetry rolls over** at 5 MB (oldest entries dropped, not data loss to file system).
- [ ] **Settings → Send anonymous telemetry** opens mail-compose sheet with file attached.
- [ ] **Settings → Report a crash** opens mail-compose sheet with latest crash log.
- [ ] **Crash report mailbox** `crashes@bravebunny.example` set up and monitored daily.
- [ ] **Telemetry inbox** `telemetry@bravebunny.example` set up.
- [ ] **No third-party analytics SDK** in build (verified via `Find Linked Frameworks` or APK analyser).

> **TODO (post-launch):** evaluate adding a privacy-preserving analytics SDK (e.g. self-hosted Plausible-style) before global launch. Track in `docs/11-roadmap/current-phase.md`.

## 5. Stability

- [ ] **No P0/P1 bugs open** in `docs/qa/`.
- [ ] **Crash-free session rate ≥ 99.5%** on internal QA pool (50+ devices) for last 7 days.
- [ ] **60 fps on iPhone 12** during 200-enemy + 50-projectile + 30-VFX worst case (per perf budget `docs/06-tech-spec/05-performance-budget.md`).
- [ ] **Game cold-launch < 4 s** on Android mid-tier (Snapdragon 7 Gen 1 reference).
- [ ] **Memory ceiling < 800 MB** during 10-minute run.
- [ ] **Battery: < 8% drain per 10-min session** on iPhone 12.
- [ ] **Save migration test** from previous soft-launch build → new build verified.

## 6. Tester pool

- [ ] **≥ 200 active testers per market** recruited (TR / PH / ID). Recruit channels:
  - Reddit r/iosbeta + r/playstoreapps + r/Survivorio
  - TestFlight public link distributed via Discord communities
  - Local mobile-gaming Discords / Telegrams for TR/PH/ID
- [ ] **Beta-tester onboarding doc** (`docs/marketing/beta-tester-onboarding.md`) published and linked in TestFlight description.
- [ ] **Feedback channel** (email or Discord) live, monitored daily, with auto-reply confirming receipt.
- [ ] **First-week reply SLA: 48 h** committed.

## 7. Marketing

- [ ] **Marketing one-pager** finalised (`docs/marketing/one-pager.md`).
- [ ] **Press kit** finalised (`docs/marketing/press-kit.md`).
- [ ] **Social media accounts** created (Twitter/X, Bluesky, TikTok). Soft-launch teaser scheduled.
- [ ] **Indie press outreach list** built (placeholder — TouchArcade, AppSpy, PocketTactics, regional outlets in TR/PH/ID).
- [ ] **5 launch-day creator outreach DMs** drafted and ready to send (Survivor-like content creators in TR/PH/ID).

## 8. Observer & operations

- [ ] **Local observer dashboard** (`/observer` slash command) green for last 24 h before submission.
- [ ] **Daily crash-email triage habit** scheduled in calendar (daily 09:00 local).
- [ ] **Live ops doc** drafted with response-time targets and incident playbook.
- [ ] **Rollback plan** documented: previous build SHA tagged, expedited-review request template drafted.

## 9. Post-soft-launch roadmap (next phase, gated by metrics)

- [ ] D1 ≥ 40% on at least 2 of 3 markets → green-light global launch prep.
- [ ] D7 ≥ 18% → balance team unblocked for difficulty curve revisits.
- [ ] Crash-free ≥ 99.5% sustained 14 days → confidence to widen tester pool to early-access public.
- [ ] If any KPI misses by > 20%, trigger **`docs/11-roadmap/post-soft-launch-pivot.md`** (to be drafted upon trigger).

---

## Sign-offs

- [ ] **Tech-architect agent** has reviewed `06-tech-spec/` against shipping build — initials + date.
- [ ] **QA-engineer agent** has signed off on `docs/qa/` smoke + regression matrix — initials + date.
- [ ] **Balance-engineer agent** has signed off on TTK ladder freeze — initials + date.
- [ ] **Sole developer (human)** has tapped through 10 full runs on retail device — initials + date.

---

*This checklist is the contract. If an item is unticked, soft-launch does not ship.*
