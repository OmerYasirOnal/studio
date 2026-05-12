# Orchestrator hand-off — 2026-05-12 (Apple integration session)

**Status:** Apple Developer integration complete. Bundle id + cert + profile live. GitHub Actions secrets set. Unity 6 LTS installing.

## What changed this session

### Apple Developer side (real-world side effects)

- **Bundle id created** on Apple Developer Portal: `com.omeryasir.bravebunny` (id=SL5GAXYB7T)
- **Apple Distribution cert** created: `K83U6UWWN4`
- **Provisioning profile** created: `match AppStore com.omeryasir.bravebunny`
- **Both encrypted + pushed** to new private repo `OmerYasirOnal/studio-certs`
- **Both installed** in local login keychain

### Naming convention rename

Bundle id changed across the entire repo:
- `com.yasironal.brave-bunny` → `com.omeryasir.bravebunny`

Rationale: Yasir's Apple Developer account has 37 existing bundle ids, all under `com.omeryasir.*`. The framework's previous placeholder `com.yasironal.*` wouldn't have matched on a real cert. Sed-renamed across GAME.md, all Fastlane configs, tech specs, runbooks, IOSBuilder.cs, ProjectSettings.asset.

### GitHub Actions secrets

5 secrets set on `OmerYasirOnal/studio`:

| Secret | Purpose |
|---|---|
| `MATCH_PASSWORD` | Decrypts studio-certs cert+profile bundle |
| `ASC_KEY_ID` | App Store Connect API key id (`93HFBMV3MA`) |
| `ASC_ISSUER_ID` | ASC issuer UUID (`3894e346-c886-4ca5-91b7-773aaa6e85bd`) |
| `ASC_KEY_P8` | ASC .p8 PEM body (materialized to `~/.appstoreconnect/private_keys/` at CI runtime) |
| `MATCH_GIT_BASIC_AUTHORIZATION` | base64(user:token) for HTTPS clone of studio-certs |

Verify via:

```bash
./games/brave-bunny/tools/ci/scripts/verify-secrets.sh
```

### Fastlane lane additions

`games/brave-bunny/tools/ci/fastlane/Fastfile`:

- `private_lane :asc_api_key` — single token factory (loads `.p8` once per lane)
- `lane :register_app` — idempotent bundle id + ASC app entry creation (one-time)
- `lane :list_apps` — read-only audit of all apps + bundle ids on the dev account
- All existing lanes (`preview`/`beta`/`release`) now thread `api_key:` through every ASC call — no Apple ID 2FA dependency on the build path

### CI helper scripts

- `tools/ci/scripts/verify-secrets.sh` — 5 required secrets present check, prints missing-secret install commands
- `tools/ci/scripts/apply-pending-workflow.sh` — applies the .pending workflow update once `gh auth refresh --scopes workflow` runs

### Editor scripting

`games/brave-bunny/unity/Assets/_Brave/Code/Boot/Editor/`:

- `Brave.Boot.Editor.asmdef` — Editor-only asmdef
- `BalanceJsonImporter.cs` — menu item `Brave > Generate Balance SOs from JSON` that reads `data/balance/*.json` and creates/updates ScriptableObject .asset files

### Unity install (in progress)

- Unity Hub 3.18.0 installed via brew
- Unity 6 LTS 6000.0.74f1 + iOS Build Support downloading via `Unity Hub --headless install`
- Editor pkg ~3.9 GB downloaded (of ~4 GB total), iOS module 356 MB complete
- License activation still needs user (open Unity Hub once for Personal license)
- ProjectVersion.txt updated to `6000.0.74f1`

## Pending — only 2 things require user input

### 1. ASC app entry (~30 sec)

The autonomous run created the bundle id but couldn't create the ASC app entry because the API key role is `Developer` (needs `App Manager` for app create).

**Path A — upgrade API key role:**
1. Visit <https://appstoreconnect.apple.com/access/api>
2. Find key `93HFBMV3MA`, change role to **App Manager**
3. Re-run: `cd games/brave-bunny/tools/ci/fastlane && fastlane register_app`

**Path B — manual web UI (one click):**
1. Visit <https://appstoreconnect.apple.com/apps>
2. Click `+` → New App
3. Fill: iOS, name "Brave Bunny", language English (U.S.), bundle id `com.omeryasir.bravebunny` (dropdown), SKU `bravebunny`

### 2. Workflow scope on gh token (~30 sec)

The `.github/workflows/bb-ios-build.yml` update is staged at `games/brave-bunny/tools/ci/github-actions/ios-build.yml.pending`. Apply via:

```bash
gh auth refresh --hostname github.com --scopes workflow
./games/brave-bunny/tools/ci/scripts/apply-pending-workflow.sh
```

### 3. (Once Unity finishes installing) license activation + UNITY_LICENSE secret

```bash
open -a "Unity Hub"                                 # Sign in once
# After activation:
gh secret set UNITY_LICENSE --repo OmerYasirOnal/studio < ~/Library/Application\ Support/Unity/Unity_lic.ulf
```

## What now works fully

| Action | Status |
|---|---|
| `gh secret list --repo OmerYasirOnal/studio` | ✅ shows 5 ASC-related secrets |
| `./core/scripts/verify-framework.sh --game brave-bunny` | ✅ 38 framework + 26 game = 64/64 |
| `python3 core/tools/asset-pipeline/licenses.py --validate --game brave-bunny` | ✅ 24 files, all permissively licensed |
| `python3 core/tools/balance-tools/validate_balance.py --game brave-bunny` | ✅ 0 advisories |
| `python3 core/tools/code-tools/check_link_xml.py --game brave-bunny` | ✅ all 8 [BraveRegister] preserved |
| `fastlane list_apps` (locally) | ✅ lists 2 apps + 37 bundle ids via ASC API |
| `fastlane match appstore --readonly true` | ✅ pulls cert from studio-certs |

## Reading order for next session

1. `current-phase.md` — phase status
2. This handoff
3. `runbooks/first-build.md` — finish the 2 manual steps
4. `verify-framework.sh --game brave-bunny` — confirm nothing drifted

## Closing observation

The framework can now demonstrate every layer of "one developer ships in 8 weeks": agent dispatch, observer dashboard, asset pipeline, balance + license validation, IL2CPP stripping rules, real Apple Developer integration (cert+profile in a private cert sync repo), GH Actions CI with real secrets. The next session's work is real Unity engineering: open the project, fix any Phase-5 compile drift, get the joystick + auto-attack running, hit 60fps with 200 enemies — then trigger the first `fastlane beta` to TestFlight.
