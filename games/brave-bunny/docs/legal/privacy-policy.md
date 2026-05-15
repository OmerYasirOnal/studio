# Brave Bunny — Privacy Policy

> **Effective date:** 2026-07-01 (soft-launch target)
> **Last updated:** 2026-05-16
> **Applies to:** Brave Bunny mobile game on iOS (App Store) and Android (Google Play)
> **Operator:** Brave Bunny Studio (sole developer)
> **Contact:** privacy@bravebunny.example

This Privacy Policy describes the data Brave Bunny ("the game", "we", "our") collects, why we collect it, who it is shared with, and the controls you have over it. We wrote this in plain language. If anything is unclear, email **privacy@bravebunny.example** and we will explain.

We follow the spirit of the **App Store App Privacy guidelines**, the **Google Play Data Safety guidelines**, and the **GDPR** principle of data minimization. Brave Bunny is built solo and ships with **no third-party analytics SDK** and **no advertising SDK** at launch.

---

## 1. Summary (TL;DR)

- The game runs **fully offline**. Your save file lives on your device.
- We collect **anonymous gameplay telemetry locally** (a file on your device). It is **never uploaded** without your explicit, in-game opt-in.
- If you opt in to **crash reports**, a single email containing a stack trace is composed in your mail app for you to send manually — we do not auto-upload.
- We process **in-app purchases (IAP)** through Apple App Store and Google Play. We never see your card details.
- We do **not** collect: name, address, phone number, email, location, contacts, photos, microphone, camera.
- We do **not** use: third-party analytics, advertising IDs, fingerprinting, social-login.
- You can wipe everything by deleting the app.

---

## 2. Data we collect

### 2.1 Local-only data (stored on your device, never leaves it)

| Data | Why | Storage |
|---|---|---|
| Save file (progression, currencies, unlocks, settings) | Lets you keep your progress | `~/Documents/save.json` (iOS sandbox), `Android/data/.../save.json` (Android) |
| Telemetry events (run start, run end, deaths, wave reached, weapon picks, framerate samples) | So **you** — and we, if you opt in to share — can see how the game is playing | Local JSONL file `telemetry.jsonl` in app sandbox |
| Crash logs (stack trace, device model, OS version) | Diagnose crashes | Local file `crash-<timestamp>.txt` in app sandbox |

**None of the above is transmitted off-device by default.** No background upload. No "phone home" on launch.

### 2.2 Data processed by Apple / Google for IAP

When you buy a cosmetic, battle pass, or subscription, **Apple App Store** (iOS) or **Google Play Billing** (Android) processes the transaction. We receive:

- An anonymous purchase receipt token (used to validate that you paid).
- The product identifier (e.g. `cosmetic_bunny_hat_01`).
- The transaction status (success / refund / pending).

We do **not** receive: your name, email, payment method, billing address, or country beyond the storefront the receipt came from.

For Apple's and Google's own data practices, see:
- Apple: https://www.apple.com/legal/privacy/
- Google Play: https://policies.google.com/privacy

### 2.3 Data we ask before sending (opt-in only)

If you tap **Settings → Send anonymous telemetry**, your `telemetry.jsonl` file is attached to an email composed in your default mail app. **You** review it. **You** press send. We never auto-upload.

If you tap **Settings → Report a crash**, your latest `crash-<timestamp>.txt` is attached to an email composed in your mail app, sent to `crashes@bravebunny.example`. Same rule: you press send.

You can decline both at any time. The game is fully playable without either.

---

## 3. What we do NOT collect

To be fully explicit (Apple's App Privacy questionnaire grades us on this):

- No name, email, phone, address, government ID.
- No location (precise or coarse).
- No advertising identifier (IDFA / GAID).
- No microphone, camera, photo library, contacts, calendar.
- No social-login (no Facebook SDK, no Google Sign-In SDK, no Apple Sign-In — game-internal account only, none required).
- No third-party advertising SDK.
- No third-party analytics SDK (no Firebase, no GameAnalytics, no Adjust, no AppsFlyer, no Unity Analytics).
- No biometric data.
- No health, fitness, or sensor data.

---

## 4. Purposes of processing

| Purpose | Data used | Legal basis (GDPR) |
|---|---|---|
| Let you play and save progress | Local save file | Contractual necessity (Art. 6(1)(b)) |
| Let you review your own gameplay stats | Local telemetry | Legitimate interest (Art. 6(1)(f)) — data stays on your device |
| Sell you optional cosmetic content | IAP receipt | Contractual necessity (Art. 6(1)(b)) |
| Improve the game using crash reports | Crash log, only if you opt in & send | Consent (Art. 6(1)(a)) |
| Improve the game using telemetry | Telemetry, only if you opt in & send | Consent (Art. 6(1)(a)) |

---

## 5. Data sharing

We do **not** sell, rent, or share your personal data with third parties for marketing, advertising, profiling, or any other purpose.

The only third parties involved are:

- **Apple App Store / Google Play** — processes your IAP. Bound by their own privacy policies.
- **Apple TestFlight / Google Play Internal Testing** (soft-launch period only) — handles beta distribution. Apple/Google may collect crash diagnostics per their normal platform policy; that data is governed by Apple/Google, not us.
- **Your email provider** (Gmail, Apple Mail, etc.) — only if **you** send a telemetry or crash email. We have no control over what your email provider stores.

No data brokers. No ad networks. No "partners". No "affiliates".

---

## 6. Data retention

| Data | Retention |
|---|---|
| Local save | Until you delete the app or clear app data |
| Local telemetry | Trimmed automatically; oldest entries dropped when file exceeds 5 MB |
| Local crash logs | Kept for last 10 crashes, oldest dropped |
| IAP receipts | Stored by Apple / Google; we keep a one-way hash to validate restored purchases |
| Crash emails you send to us | Stored in our mailbox for max **180 days**, then deleted |
| Telemetry emails you send to us | Stored in our mailbox for max **180 days**, then deleted, aggregated stats kept indefinitely with no per-user link |

---

## 7. Your rights (GDPR, CCPA, KVKK)

You have the right to:

- **Access** any data we hold about you. Email **privacy@bravebunny.example**. Because we hold almost nothing (only support-email content if you wrote to us), turnaround is fast.
- **Delete** any data we hold. We will delete within 30 days of request.
- **Object** to processing. Don't opt in to telemetry/crash, and we hold nothing.
- **Portability**: your save file is a plain JSON file in the app sandbox — copy it freely.
- **Withdraw consent** for telemetry/crash sharing at any time in Settings.
- **Lodge a complaint** with your local data-protection authority. Examples:
  - Turkey (KVKK): https://www.kvkk.gov.tr
  - Philippines (NPC): https://www.privacy.gov.ph
  - Indonesia (Kominfo): https://www.kominfo.go.id
  - EU: https://edpb.europa.eu/about-edpb/board/members_en

**California residents** (CCPA): we do not sell your personal information. We do not share it for cross-context behavioural advertising. You may still email us to confirm.

---

## 8. Children's privacy

Brave Bunny is rated **9+ (Apple) / Everyone 10+ (Google) / PEGI 7** equivalent. We do not knowingly collect personal data from anyone, of any age, beyond the offline local-storage described above. The game does not contain chat, user-generated-content, location, or any feature that would gather child personal data. If you are a parent and believe data was collected, email **privacy@bravebunny.example** — we will respond within 7 days.

---

## 9. Security

- Save and telemetry files live in the app's OS-protected sandbox.
- IAP validation uses Apple / Google receipt verification.
- Crash & telemetry emails travel over standard TLS-encrypted mail transport.
- We do not host a server, database, or web API at soft-launch. Reduced attack surface by design.

---

## 10. International transfers

Because the game does not transmit data off-device at default, no international transfer occurs. If you choose to send a support email, it travels through your email provider's infrastructure (which may be international); we receive it at our mailbox hosted in the European Union.

---

## 11. Changes to this policy

If we add an analytics SDK, advertising SDK, or any new data collection, we will:

1. Update this document and the "Last updated" date.
2. Show an in-game banner on next launch describing the change.
3. Require fresh opt-in for anything that is not strictly necessary to play.

You can always see the current policy at `https://bravebunny.example/privacy` (placeholder URL — final URL will be set before soft-launch).

---

## 12. Contact

- **Privacy questions:** privacy@bravebunny.example
- **Crash reports:** crashes@bravebunny.example
- **General support:** support@bravebunny.example
- **Postal address (data controller):** placeholder — to be added before soft-launch submission

> This policy was written in plain English. Translated versions for Turkish, Filipino, and Bahasa Indonesia will be published before each market's launch.
