# Brave Bunny — Beta Tester Onboarding

> Welcome, hopper. You are one of the first to play **Brave Bunny** before it goes wide. This page tells you how to install, how to give us useful feedback, and what is broken (so you can stop reporting the bugs we already know).
> Estimated reading time: 3 minutes.

---

## 1. Install

### iPhone / iPad (TestFlight)

1. Install **TestFlight** from the App Store: https://apps.apple.com/app/testflight/id899247664
2. Tap the invite link we sent you: `https://testflight.apple.com/join/BB-PLACEHOLDER` *(placeholder — final link in your invite email)*
3. Tap **Accept**, then **Install** in TestFlight.
4. Brave Bunny will appear on your home screen with a small orange dot — that means it is a beta build.

> **Minimum iOS:** 16.0. We target iPhone 12 and later for performance. Older devices will still run the game but may dip below 60 fps in dense waves.

### Android (Google Play Internal Testing)

1. Open this link on the same Google account you use for the Play Store: `https://play.google.com/apps/internaltest/PLACEHOLDER` *(placeholder)*
2. Tap **Become a tester**.
3. Wait 5 minutes for the listing to propagate, then install from the Play Store as normal.

> **Minimum Android:** 10 (API 29). 64-bit ARM only.

---

## 2. Soft-launch markets

We are soft-launching in **Turkey (TR)**, **Philippines (PH)**, and **Indonesia (ID)** first. If your App Store / Play account is set to a different country, the build may not appear — email **beta@bravebunny.example** and we will route you to the right invite.

---

## 3. How to give feedback

You have three channels. Pick whichever is easiest.

### 3.1 Shake to report (in-game)

Shake your phone in the main menu (or hold three fingers on the screen for 1 second). A feedback form opens. Type your thought. Tap Send. Done. The form is pre-filled with build number, device, and recent telemetry — no PII.

### 3.2 TestFlight feedback (iOS)

Screenshot anything weird → in Photos, tap **Share → TestFlight Feedback**. Add a sentence of context. This goes straight to our dashboard.

### 3.3 Email

For anything longer than three sentences, email **beta@bravebunny.example**. Please include:

- Build number (Settings → About → Build).
- Device + OS version.
- What you did, what you expected, what actually happened.
- A short video or screenshot if visual.

---

## 4. Known issues (do not report these)

| # | Issue | Status |
|---|---|---|
| 1 | Some boss-arena spawn cones appear oversized | known — Wave 7 telemetry analysis pending |
| 2 | TR localisation has a few English strings on debug builds | known — full localisation arrives at v0.9 |
| 3 | Battle pass placeholder art shows in some tier rewards | known — final art in v0.8 |
| 4 | Crash on app launch if device storage is < 200 MB | known — error dialog in v0.8 |
| 5 | Game audio occasionally clips when switching between Bluetooth and speaker mid-run | known — audio agent investigating |

If you find something **not** on this list, that is gold to us. Please send it.

---

## 5. Save warning

Soft-launch builds **may wipe progress** between releases. We will tell you in patch notes. Do not get attached to your gold count. We hate it as much as you do; it is part of beta.

---

## 6. Privacy reminder

By default Brave Bunny stores telemetry **only on your device**. The shake-to-report and TestFlight-feedback flows attach a telemetry snippet to your message; you can review and remove it before sending. Full details in `docs/legal/privacy-policy.md`. Short version: we collect nothing automatically.

---

## 7. Contact

- **Beta program:** beta@bravebunny.example
- **Bug reports:** bugs@bravebunny.example
- **Anything else:** hello@bravebunny.example

Thank you for hopping in early. Tell us when it feels wrong; tell us when it feels right. Both make the game better.

— Brave Bunny Studio
