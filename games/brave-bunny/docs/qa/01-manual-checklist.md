# QA 01 — Manual Pre-Release Checklist

> Owner: qa-engineer. Run this checklist on every TestFlight candidate before submitting to soft-launch markets. Sister docs: `00-test-plan.md`, `03-device-matrix.md`. Cross-references: `docs/03-user-stories/02-run.md` (US-13..US-30), `docs/03-user-stories/04-monetization.md` (US-43..US-54), `docs/06-tech-spec/03-save-system.md`, `decisions/0008-save-format-newtonsoft-json.md`.

## How to use this checklist

- Run all sections on the **iPhone 12** (perf baseline) and the **iPhone SE 3** (small-screen smoke).
- Tick each box only after observing the behavior live; don't fill out from memory.
- Record any failing item in `04-known-issues.md` with severity per `02-bug-triage.md`.
- Sweep takes ~75 minutes on a clean build; budget 2h for the first pass per device.

---

## 1. Boot path (5)

- [ ] Cold boot from Springboard reaches Home in ≤ 8 s on iPhone 12.
- [ ] Logo splash respects `Application.targetFrameRate` (no jank at 60 fps).
- [ ] First-launch creates a fresh save (`save_0.dat`) and `bunny` is owned + `carrot-boomerang` equipped (per `03-save-system.md` defaults).
- [ ] Subsequent launches load the saved profile (currency, level, achievements all restore).
- [ ] Force-quit during boot does not corrupt the save file (next launch loads cleanly).

## 2. FTUE — first-time user experience (8)

- [ ] Welcome string is in player's device language if `tr` / `en`; falls back to `en` otherwise.
- [ ] FTUE never blocks the player in a modal with no "skip" affordance after first run.
- [ ] Joystick tutorial overlay appears once and never returns after first run.
- [ ] Auto-attack tutorial overlay appears within the first 10 s of the first run.
- [ ] Level-up draft tutorial appears at the first level-up, then never again.
- [ ] No FTUE step references unimplemented features.
- [ ] FTUE copy uses tone-bible voice (no banned vocabulary: "killed", "died", "Game Over").
- [ ] All FTUE strings localize through `LocalizationService` (no hardcoded strings).

## 3. Run loop (10)

- [ ] **US-13** Joystick: first-touch to hero-velocity update is visibly within one frame (no skating).
- [ ] **US-13** Floating joystick spawns at thumb-down inside the bottom-left quadrant.
- [ ] **US-13** Dead-zone at center (≤ 8% of stick radius) — no jitter when thumb is centered.
- [ ] **US-14** Level-up draft cards fit on iPhone SE 3 portrait without scroll.
- [ ] **US-14** Each card shows icon, name, description, current → next delta on one row.
- [ ] **US-15** First banish per run is free; second requires rewarded-ad opt-in.
- [ ] **US-16** Pause button is in top-right safe area; modal halts gameplay + ducks audio −12 dB.
- [ ] **US-19** Each weapon has distinct visual signature (Carrot Spear / Pebble Sling / Honey Aura).
- [ ] **US-19** HUD weapon icons pulse on each fire event (alpha 0.6 → 1.0 over 80 ms).
- [ ] **US-20** Every boss attack has a ≥ 600 ms wind-up + yellow AoE telegraph.

## 4. Run end (5)

- [ ] **US-26** Death triggers 0.3x time-dilate for 300 ms then pause (no "Game Over" string).
- [ ] **US-26** Gold particle burst from hero corpse (~60 particles, ~600 ms lifetime).
- [ ] **US-18** Run-end tally slides in over 400 ms; first 800 ms is non-skippable.
- [ ] **US-18** No IAP banner / rewarded-ad offer / battle-pass nag appears before tally is banked.
- [ ] **US-30** "Play again" boots a new run in ≤ 4 s with same biome + character + loadout.

## 5. Meta / progression (8)

- [ ] Character unlock costs the documented Star price (per `08-economy.md`).
- [ ] Carrots from a run appear in the wallet within 200 ms of run end.
- [ ] Daily streak claim increments by 1 the first time on a new UTC day.
- [ ] Daily streak survives a 2-day skip; resets to 1 on a 3+ day skip.
- [ ] Achievement claim grants Stars exactly once (no double-claim per `AchievementServiceTests`).
- [ ] Quit-out via pause-modal banks 100% of in-run gold / Soul Shards / pass-XP.
- [ ] Save file size stays under 200 KB at typical mature profile.
- [ ] Re-rolled drafts (US-29) never produce the same 3-card set twice in a row.

## 6. Monetization (8)

- [ ] **US-46** No interstitial ad fires between joystick-down and run-end (20 consecutive runs).
- [ ] **US-22** Revive offer's "Head home" decline button is the larger and pre-focused control.
- [ ] **US-44** Every rewarded-ad CTA shows reward type + quantity inline before the ad starts.
- [ ] **US-45** IAP confirm screen shows local-currency price + one-time/subscription label.
- [ ] **US-43** Shop preview never auto-confirms a purchase; tap-outside dismisses without buy.
- [ ] **US-49** Starter pack appears once at run-end of run #3 and never re-surfaces if declined.
- [ ] **US-50** Gift inbox messages use `{IAP_GIFT_BANNER}` tone copy (no paid offer disguised).
- [ ] **US-54** Settings → "Restore purchases" triggers the platform-native restore flow.

## 7. Settings + accessibility (6)

- [ ] Audio master / music / SFX sliders persist across app restart.
- [ ] Language switch (en ↔ tr) is hot — no app restart required.
- [ ] All HUD numbers ≥ 28 pt on iPhone SE 3 logical resolution (per US-21).
- [ ] HP bar contrast ≥ 4:1 against the busiest biome background (WCAG AA per US-21).
- [ ] Haptics toggle disables vibration on every haptic-triggering action.
- [ ] Low-power-mode toggle applies the iPhone SE 3 degrade plan (per `05-performance-budget.md`).

## 8. Cross-platform (X)

- [ ] **iPhone 12 (A14 Bionic)** — pass entire checklist; 60 fps sustained in 200-enemy stress.
- [ ] **iPhone SE 3 (A15 Bionic, 4.7")** — pass with degrade plan applied (50 fps acceptable).
- [ ] **iPhone 15 / 16 (A16/A17 Pro)** — smoke pass only (boot, one run, run-end).
- [ ] **iPad Air (M1, landscape)** — smoke pass only (boot, one run; safe area + UI scaling).
- [ ] **Android baseline (Pixel 5)** — DEFERRED to v0.2 per `03-device-matrix.md`.

---

## Sign-off

| Build | Sweep date | Engineer | Result |
|---|---|---|---|
|   |   |   |   |
