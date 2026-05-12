# First-build runbook — Brave Bunny iOS TestFlight

> Owner: build-engineer (procedure); developer (one-time interactive steps).
> Cross-ref: tech-spec `10-build-and-ci.md`, repo-root `CLAUDE.md` ("Escalation triggers").
>
> This runbook walks through the interactive steps that **cannot** be automated
> (Apple Developer agreement screens, App Store Connect app creation, cert
> repo bootstrap). Run this once. After that, `fastlane beta` is hands-off.

## 1. Apple Developer Program enrollment (one-time, ~24-48h)

1. Enroll at <https://developer.apple.com/programs/> ($99/year).
2. Accept the latest Program License Agreement at <https://developer.apple.com/account/>
   (Apple changes the agreement periodically — fastlane fails until accepted).
3. Note down your **Team ID** (10-char string, top-right of the developer portal).

## 2. App Store Connect app creation (one-time, ~10 min)

1. Sign in at <https://appstoreconnect.apple.com/>.
2. My Apps → "+" → New App.
3. Platform: iOS. Bundle ID: `com.omeryasir.bravebunny` (must match `Appfile`).
4. SKU: `brave-bunny`. Primary language: English (US) — TR added later via localization tab.
5. Note the **ITC Team ID** (App Store Connect team id, distinct from Developer team id).

## 3. App-specific password (one-time, ~2 min)

fastlane uploads via Apple's `iTunes Transporter`; this requires either:

- **Recommended:** an app-specific password generated at <https://account.apple.com/account/manage> → Sign-In and Security → App-Specific Passwords → "+". Label it `fastlane-brave-bunny`. Store in `FASTLANE_APP_SPECIFIC_PASSWORD`.
- Or your Apple ID password directly (less secure; breaks on 2FA prompts).

## 4. Bootstrap the cert repo (one-time, ~5 min)

```bash
# Create the SEPARATE private repo for cert storage. Never make this public.
gh repo create OmerYasirOnal/studio-certs --private --description "Fastlane match cert storage"
```

Then locally, with a strong passphrase ready (store in 1Password etc.):

```bash
cd /tmp
git clone git@github.com:OmerYasirOnal/studio-certs.git
cd studio-certs
fastlane match appstore     # interactive — generates certs + provisioning profiles
# fastlane asks for: Apple ID, team id, MATCH_PASSWORD passphrase
```

Verify the repo now contains encrypted `certs/distribution/*.p12` and
`profiles/appstore/*.mobileprovision`.

## 5. Configure GitHub Actions secrets (one-time, ~5 min)

Repo settings → Secrets and variables → Actions → New repository secret.

| Secret | Source | Notes |
|---|---|---|
| `UNITY_LICENSE` | Unity Hub Personal license activated locally, then `~/.local/share/unity3d/Unity/Unity_v6000.x.ulf` | Or use `UNITY_EMAIL` + `UNITY_PASSWORD` for activation fallback |
| `UNITY_EMAIL` | Unity account email | Fallback if `UNITY_LICENSE` is unset |
| `UNITY_PASSWORD` | Unity account password | Fallback if `UNITY_LICENSE` is unset |
| `MATCH_PASSWORD` | Passphrase from step 4 | Decrypts cert repo |
| `MATCH_GIT_AUTHOR` | GitHub username with access to `studio-certs` | Usually `OmerYasirOnal` |
| `FASTLANE_USER` | Apple ID email | e.g. `omeryasir.onal@stu.fsm.edu.tr` |
| `FASTLANE_PASSWORD` | Apple ID password | Avoid if possible — use app-specific |
| `FASTLANE_APP_SPECIFIC_PASSWORD` | App-specific password from step 3 | Preferred over `FASTLANE_PASSWORD` |
| `FASTLANE_TEAM_ID` | Apple Developer team id from step 1 | 10-char string |
| `FASTLANE_ITC_TEAM_ID` | App Store Connect team id from step 2 | Different from above |

## 6. Local smoke test (one-time, ~15 min on developer mac)

```bash
cd games/brave-bunny/tools/ci/fastlane
gem install bundler
bundle install
export MATCH_PASSWORD=...          # passphrase from step 4
export MATCH_GIT_AUTHOR=OmerYasirOnal
bundle exec fastlane ios preview   # local archive, NO upload — proves the pipeline
```

A successful run drops `games/brave-bunny/Builds/BraveBunny-preview-*.ipa`.

## 7. First TestFlight push (one-time, ~25 min)

After step 6 passes locally:

```bash
export FASTLANE_USER=omeryasir.onal@stu.fsm.edu.tr
export FASTLANE_APP_SPECIFIC_PASSWORD=...   # from step 3
export FASTLANE_TEAM_ID=...
export FASTLANE_ITC_TEAM_ID=...
bash games/brave-bunny/tools/ci/scripts/upload-testflight.sh
```

Or kick off the GitHub Action manually:

1. Repo → Actions → ios-build → Run workflow → lane: `beta`.

After fastlane finishes, the build appears in App Store Connect → My Apps →
Brave Bunny → TestFlight tab. Apple's "processing" step takes ~10-20 min.

## Escalation triggers (per repo-root `CLAUDE.md`)

Stop and surface to the human if:

- Apple's web UI requires accepting a new agreement mid-run.
- `fastlane match` fails with "no matching cert" — needs manual `match nuke` + regenerate.
- Xcode version on the runner is newer than what tech-spec 00 declares.
