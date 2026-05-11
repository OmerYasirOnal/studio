# Tech Spec 10 — Build and CI

> Owner: tech-architect (spec); build-engineer (implementation). The iOS-first build pipeline, Fastlane lanes, signing strategy, secret handling, and CI cadence for Brave Bunny. Cross-refs: `00-engine-and-version.md` (IL2CPP, .NET Standard 2.1, Unity 6 LTS pin), `01-project-layout.md` (Unity project path), `05-performance-budget.md` (200 MB App Store hot-zone budget), `GAME.md` (priority platform: iOS, bundle id pattern).

## Build target priority

**iOS first.** Android is a build target but deferred until iOS soft-launch validates per `GAME.md` thesis. The Unity build is platform-agnostic; only signing + store delivery diverges.

| Platform | Status at launch | Bundle / Package id |
|---|---|---|
| **iOS** | **Primary** | `com.yasironal.brave-bunny` |
| Android | Deferred until iOS soft-launch | `com.yasironal.bravebunny` (Android disallows hyphens) |

## Build pipeline ownership

| Path | Owner | Notes |
|---|---|---|
| `unity/` Unity project | gameplay-engineer + systems-engineer + ui-engineer | Day-to-day |
| `tools/ci/fastlane/` | **build-engineer** | Fastlane lanes, Match config, ipa-upload glue |
| `.github/workflows/` | **build-engineer** | GitHub Actions YAML |
| `tools/ci/perf-smoke.sh` | build-engineer | Perf-smoke planned per `05-performance-budget.md` |
| `tools/ci/unity-build-ios.sh` | build-engineer | Headless Unity build invocation |

This doc spec'd by tech-architect; implementation handed to build-engineer in Phase 5.

## Headless Unity build

`tools/ci/unity-build-ios.sh` wraps the standard Unity batchmode invocation:

```bash
#!/usr/bin/env bash
set -euo pipefail
UNITY_PATH="/Applications/Unity/Hub/Editor/$(cat unity/ProjectSettings/ProjectVersion.txt | grep m_EditorVersion | awk '{print $2}')/Unity.app/Contents/MacOS/Unity"

"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath ./unity \
    -buildTarget iOS \
    -executeMethod Brave.Build.IOSBuilder.Build \
    -logFile ./build-logs/unity-ios-$(date +%Y%m%d-%H%M%S).log

# Resulting Xcode project lives at unity/Build/iOS/
```

The `Brave.Build.IOSBuilder.Build` method (in `Brave.Boot` asmdef, `#if UNITY_EDITOR` gated):

- Reads version from `GAME.md` (`semver: ...`) + framework `core/VERSION`.
- Sets `PlayerSettings.iOS.buildNumber = $(CI run id)` monotonic.
- Sets `PlayerSettings.applicationIdentifier = "com.yasironal.brave-bunny"`.
- Forces `iOS Target SDK = Device SDK`, `Architecture = ARM64`, `Scripting Backend = IL2CPP`.
- Strips Editor-only define `UNITY_EDITOR` and any `UNITY_INCLUDE_TESTS`.
- Strips desktop input bindings (`UNITY_IOS` define) per `04-input-system.md`.
- Generates Xcode project at `unity/Build/iOS/`.

Android variant `unity-build-android.sh` uses `-buildTarget Android` and writes APK + AAB to `unity/Build/Android/`.

## Versioning

| Field | Source | Format |
|---|---|---|
| **Marketing version** (`CFBundleShortVersionString` / `versionName`) | `GAME.md` SemVer + framework `core/VERSION` | `1.0.0` (decided at v1 launch; vertical slice ships `0.1.0`) |
| **Build number** (`CFBundleVersion` / `versionCode`) | CI run id (`GITHUB_RUN_NUMBER` monotonic) | integer, never decrements |

`IOSBuilder` reads both at build time; rejects any merge to `main` that downgrades the SemVer (CI guard owned by build-engineer).

## Signing

**Fastlane Match** (free, MIT) for certificate + provisioning profile sync across machines:

- Match repo: **private GitHub repo** under the developer's account (storage cost: free under 1 GB; never public).
- Encrypted with a passphrase stored in **GitHub Actions secrets** (`MATCH_PASSWORD`), never in the repo, never echoed in logs.
- Apple Developer Program account: solo developer account (no team). Apple ID + app-specific password lives in GitHub Actions secrets (`APPLE_ID`, `FASTLANE_APP_SPECIFIC_PASSWORD`).
- Cert types: **Development** (for local + TestFlight internal), **Distribution** (for App Store).

## Fastlane lanes

`tools/ci/fastlane/Fastfile`:

```ruby
default_platform(:ios)

platform :ios do
  desc "Local archive without upload (preview)"
  lane :preview do
    match(type: "development", readonly: true)
    sync_code_signing(type: "development")
    build_app(scheme: "Unity-iPhone",
              workspace: "../../unity/Build/iOS/Unity-iPhone.xcworkspace",
              export_method: "development",
              output_directory: "../../build-output/preview")
  end

  desc "Upload to TestFlight (beta)"
  lane :beta do
    match(type: "appstore", readonly: true)
    build_app(scheme: "Unity-iPhone",
              workspace: "../../unity/Build/iOS/Unity-iPhone.xcworkspace",
              export_method: "app-store",
              output_directory: "../../build-output/beta")
    upload_to_testflight(skip_waiting_for_build_processing: true)
  end

  desc "Submit to App Store (release)"
  lane :release do
    match(type: "appstore", readonly: true)
    build_app(scheme: "Unity-iPhone",
              workspace: "../../unity/Build/iOS/Unity-iPhone.xcworkspace",
              export_method: "app-store",
              output_directory: "../../build-output/release")
    upload_to_app_store(submit_for_review: false, automatic_release: false)
  end
end
```

Invocations:

```bash
cd tools/ci/fastlane && fastlane preview     # local
cd tools/ci/fastlane && fastlane beta        # CI TestFlight
cd tools/ci/fastlane && fastlane release     # CI App Store
```

## Secrets management

| Secret | Where | Used in |
|---|---|---|
| `MATCH_PASSWORD` | GitHub Actions secrets | `match` lane (decrypt certs) |
| `APPLE_ID` | GitHub Actions secrets | `upload_to_testflight`, `upload_to_app_store` |
| `FASTLANE_APP_SPECIFIC_PASSWORD` | GitHub Actions secrets | Same |
| `UNITY_LICENSE` | GitHub Actions secrets | Unity license activation in CI |
| `UNITY_EMAIL` / `UNITY_PASSWORD` | GitHub Actions secrets | Unity activation fallback |

**Never committed** to the repo. Per repo-root `CLAUDE.md` forbidden-pattern: committing an API key fails the pre-commit hook (build-engineer wires this).

## CI: GitHub Actions

`.github/workflows/build-ios.yml` (build-engineer to author). Skeleton:

- **Runners:** macOS GitHub-hosted runners (`macos-14` or newer). Required for Xcode + iOS SDK.
- **Trigger model:** **Manual `workflow_dispatch`** at first (Mac minutes cost ~10× Linux minutes; we don't run on every PR).
- **Nightly trigger:** Once Phase 5 stabilizes, `schedule: cron '0 6 * * *'` runs the `preview` lane on `main`.
- **TestFlight cadence:** Manual `workflow_dispatch` with input `lane: beta`.
- **App Store cadence:** Manual `workflow_dispatch` with input `lane: release`, gated on a `release/*` branch.

Workflow steps:

```yaml
1. checkout (with submodules false)
2. cache (~/Library/Caches/UnityHub + unity/Library)
3. setup ruby + bundler (Fastlane)
4. activate Unity license (game-ci/unity-activate)
5. run tools/ci/unity-build-ios.sh
6. cd tools/ci/fastlane && fastlane $LANE
7. archive logs + ipa as workflow artifact
8. (release lane only) post a GitHub Release with the ipa attached
```

## Build cadence

| Cadence | Trigger | Lane |
|---|---|---|
| **On-demand (Phase 5)** | `workflow_dispatch` | `preview` |
| **Nightly (Phase 5 stable)** | `cron '0 6 * * *'` on `main` | `preview` |
| **TestFlight uploads** | `workflow_dispatch` manual | `beta` |
| **App Store submissions** | `workflow_dispatch` manual + tag `vX.Y.Z` | `release` |
| **Perf smoke** | Pre-merge on `main` (Phase 5+) | `perf-smoke.sh` (build-engineer) |

Mac-minutes budget: GitHub Free tier gives **2,000 min/month** for private repos at 10× multiplier on macOS = effectively **200 min/month**. Plan: ~30 nightly preview builds at ~5 min each = 150 min, leaves margin for beta + release.

## Android deferral

When iOS soft-launch validates per `GAME.md`:

- Same Unity build invocation with `-buildTarget Android`.
- Sign with a single keystore generated locally, encrypted via `git-crypt` (free) into `tools/ci/signing/`.
- Fastlane `supply` plugin for Play Console upload.
- AAB (Android App Bundle) preferred over APK for Play Store delivery.
- Android Fastlane lane TBD when build-engineer scopes Android phase.

## Asset packaging

| Bucket | Packaging | Phase |
|---|---|---|
| **Core game assets** | In-build (default Resources / Assets) | Vertical slice (Phase 5) |
| **Cosmetic packs** | AssetBundles | Deferred to v1.1 (post-launch live-ops) |
| **Localized text** | Unity Localization tables in-build | All phases |

AssetBundles will use Unity Addressables in v1.1; the dependency is already listed in `11-third-party.md`. Vertical slice ships everything in-build — keeps the pipeline simple while we converge on a content rhythm.

## Cross-references

- `00-engine-and-version.md` — IL2CPP, .NET Standard 2.1, ProjectVersion pin.
- `01-project-layout.md` — Unity project path, asmdef structure.
- `05-performance-budget.md` — 200 MB App Store hot-zone budget gates `release` lane.
- `11-third-party.md` — package manifest for Unity Packages.
- `GAME.md` — iOS priority platform, bundle id pattern, version policy.
- Repo-root `CLAUDE.md` — no API keys committed (pre-commit hook).
