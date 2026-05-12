# First-build runbook — Brave Bunny iOS TestFlight

> Owner: build-engineer (procedure); developer (one-time interactive steps).
> Cross-ref: tech-spec `10-build-and-ci.md`, repo-root `CLAUDE.md` ("Escalation triggers").
>
> This runbook walks through the interactive steps that **cannot** be automated
> (Apple Developer agreement screens, App Store Connect app creation when API
> key role is insufficient, secrets registration). Run this once. After that,
> `fastlane beta` is hands-off.

## Pre-flight check — what's already DONE for brave-bunny

The autonomous setup pass has already completed:

| Step | Status | How |
|---|---|---|
| Apple Developer Program enrollment | ✅ active | Yasir's pre-existing account (Team `9X8FDSW5D8`) |
| Apple Distribution cert | ✅ live | `K83U6UWWN4` — created by `fastlane match appstore` |
| Provisioning profile | ✅ live | `match AppStore com.omeryasir.bravebunny` |
| Apple Developer bundle ID `com.omeryasir.bravebunny` | ✅ active (`SL5GAXYB7T`) | `fastlane register_app` |
| ASC API key | ✅ at `~/.appstoreconnect/api_key.json` | Pre-existing |
| match-encrypted cert repo `OmerYasirOnal/studio-certs` | ✅ created + populated | `gh repo create` + `fastlane match` |
| `MATCH_PASSWORD` | ✅ generated | See `/tmp/match_password.txt` — **save to GH Actions secret + 1Password before tmp is wiped** |

## What still needs the developer to do

### 1. Create the App Store Connect app entry (one-time, ~2 min)

The autonomous run created the Apple Developer bundle ID but the ASC API key role on this account is `Developer`, which **does not allow CREATE on apps**. Two paths:

**Path A — Upgrade API key role (preferred, 30 sec):**
1. Open <https://appstoreconnect.apple.com/access/api>
2. Find the key with Key ID `93HFBMV3MA` ("EAS submit" or similar)
3. Click → change role from `Developer` to `App Manager`
4. Re-run `cd games/brave-bunny/tools/ci/fastlane && fastlane register_app` — will create the ASC app automatically

**Path B — Manual ASC create (one click, ~2 min):**
1. Sign in at <https://appstoreconnect.apple.com/>
2. My Apps → `+` → New App
3. Fill exactly:
   - **Platform:** iOS
   - **Name:** `Brave Bunny`
   - **Primary Language:** English (U.S.)
   - **Bundle ID:** `com.omeryasir.bravebunny` (dropdown shows it — already exists)
   - **SKU:** `bravebunny`
   - **User Access:** Full Access (or per your preference)

### 2. Save `MATCH_PASSWORD` to GitHub Actions secrets

CI builds need the same passphrase that encrypted the certs in `studio-certs`.

```bash
gh secret set MATCH_PASSWORD --repo OmerYasirOnal/studio --body "$(cat /tmp/match_password.txt)"
```

Also save to 1Password / your preferred password manager — losing this passphrase means re-running match (which rotates the cert).

### 3. Save ASC API key parts as GitHub Actions secrets

```bash
gh secret set ASC_KEY_ID --repo OmerYasirOnal/studio --body "93HFBMV3MA"
gh secret set ASC_ISSUER_ID --repo OmerYasirOnal/studio --body "3894e346-c886-4ca5-91b7-773aaa6e85bd"
gh secret set ASC_KEY_P8 --repo OmerYasirOnal/studio < ~/.appstoreconnect/private_keys/AuthKey_93HFBMV3MA.p8
```

### 4. Unity license activation (after Unity install completes)

The autonomous run installed Unity 6 LTS 6000.0.74f1 via Unity Hub. License activation is the one interactive step:

```bash
# Open Unity Hub once — sign in with Unity ID, activate Personal license (free).
open -a "Unity Hub"
```

For CI builds, generate `UNITY_LICENSE` content:

```bash
# Get the .ulf file path after activating
ls ~/Library/Application\ Support/Unity/Unity_lic.ulf
gh secret set UNITY_LICENSE --repo OmerYasirOnal/studio < ~/Library/Application\ Support/Unity/Unity_lic.ulf
```

## Running the lanes

```bash
cd games/brave-bunny/tools/ci/fastlane

export MATCH_PASSWORD="$(cat /tmp/match_password.txt)"   # or sourced from 1Password

# Smoke: archive without uploading
fastlane preview

# TestFlight upload (after step 1 above)
fastlane beta

# Read-only audit
fastlane list_apps
```

All lanes use the ASC API key automatically — no Apple ID 2FA prompts.

## Escalation triggers (when to surface to the developer)

- **Cert expired / Apple agreement changed:** match will surface an error; re-run `fastlane match appstore --readonly false` locally with `MATCH_PASSWORD` exported.
- **iOS build size > 200 MB:** see `docs/07-art-bible/08-asset-budget.md`. Triage assets.
- **TestFlight processing > 1h:** investigate via `pilot list`.
