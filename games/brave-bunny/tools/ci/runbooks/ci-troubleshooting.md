# CI Troubleshooting Runbook — Brave Bunny iOS pipeline

> Owner: build-engineer. Wave 11 deliverable.
> Cross-ref: `tech-spec/10-build-and-ci.md`, `runbooks/first-build.md`, `runbooks/unity-license.md`.
>
> This document is the **first stop** when `bb-ios-build.yml` fails in GitHub
> Actions. Each section is keyed on a recognisable error signature and resolves
> in 1-5 minutes. If a fix is not here, append it after solving — the runbook
> is the long-term memory for this pipeline.

## How to read this runbook

1. Open the failed workflow run in GitHub Actions.
2. Search the failing step's log for the *signature* in the table below.
3. Jump to the matching section. Apply the fix. Re-run the workflow.
4. If the signature is new, add a row here with the fix you used.

## Quick-jump table

| Failing step                 | Signature                                                | Section |
|------------------------------|----------------------------------------------------------|---------|
| Verify Unity version pin     | `UNITY_VERSION env (...) != ProjectVersion.txt (...)`    | [1](#1-unity-version-pin-mismatch) |
| Cache Unity Library          | (cache miss is OK — slow but not fatal)                  | [2](#2-cache-misses--slow-first-run) |
| Run EditMode tests           | `Licensing::Module: License is not valid`                | [3](#3-unity-license-failure) |
| Run EditMode tests           | `Cannot find file: ...Brave.Boot.Editor.asmdef`          | [4](#4-asmdef-resolution-failures) |
| Run EditMode tests           | `failed="N"` with N>0 in NUnit XML                       | [5](#5-test-failures) |
| Run PlayMode smoke tests     | `category 'Smoke' matched 0 tests`                       | [6](#6-playmode-smoke-test-zero-match) |
| Build with Unity             | `BuildResult.Failed`                                     | [7](#7-unity-build-failure) |
| Run fastlane preview/beta    | `match: could not decrypt`                               | [8](#8-fastlane-match-decrypt-failure) |
| Run fastlane preview/beta    | `xcodebuild: error: Code signing is required`            | [9](#9-xcodebuild-signing-error) |
| Run fastlane beta            | `Cannot generate token: ASC API`                         | [10](#10-asc-api-key-error) |
| Run fastlane beta            | `Upload to TestFlight: timeout`                          | [11](#11-asc-upload-timeout) |
| (any)                        | mac-minutes budget exhausted                             | [12](#12-mac-minutes-budget-exhausted) |

## 1. Unity version pin mismatch

**Signature:**
```
::error::UNITY_VERSION env (6000.0.74f1) != ProjectVersion.txt (6000.0.31f1). Bump both together.
```

**Cause:** The workflow's `UNITY_VERSION` env was edited but `games/brave-bunny/unity/ProjectSettings/ProjectVersion.txt` wasn't — or vice-versa.

**Fix:** Update both to the same `m_EditorVersion`. The `Verify Unity version pin` step is the canary; treat it as a hard gate.
```bash
grep '^m_EditorVersion:' games/brave-bunny/unity/ProjectSettings/ProjectVersion.txt
grep 'UNITY_VERSION:' .github/workflows/bb-ios-build.yml
```

## 2. Cache misses — slow first run

**Signature:**
```
Cache not found for input keys: Library-brave-bunny-iOS-6000.0.74f1-...
```

**Cause:** First run after a Unity version bump, manifest change, or workflow edit that altered the cache key.

**Fix:** Nothing required — the workflow runs cold (~25-30 min extra). The next run on the same key will warm the cache. To force a cold start, change `cache-key-prefix` or delete the cache via `gh api -X DELETE /repos/:owner/:repo/actions/caches/:id`.

## 3. Unity license failure

**Signature:**
```
Licensing::Module: License is not valid
```
or
```
[Licensing::Client] Returning license server: ...
[Licensing::Client] Error: No license token
```

**Cause:** `UNITY_LICENSE` secret missing or expired. Game-CI's `unity-test-runner` / `unity-builder` reads it from the secret of that name.

**Fix:**
1. Re-acquire a Personal license from <https://license.unity3d.com/manual> using the runner's machine ID (game-ci has a `request-activation-file` action you can run once locally).
2. Set the new license on the repo:
   ```bash
   gh secret set UNITY_LICENSE --repo OmerYasirOnal/studio < Unity_v6.x.ulf
   ```
3. Full details in `runbooks/unity-license.md`.

## 4. asmdef resolution failures

**Signature:**
```
Assembly 'Brave.Tests.EditMode' will not be loaded due to errors:
Unable to resolve reference 'Brave.Boot.Editor'.
```

**Cause:** A new test references a code asmdef that isn't listed in the test asmdef's `references` array.

**Fix:** Edit `games/brave-bunny/unity/Assets/_Brave/Code/Tests/EditMode/Brave.Tests.EditMode.asmdef` and add the missing asmdef name to `references`. Re-import in Unity (or just push — the runner does it).

## 5. Test failures

**Signature:** the NUnit XML reports `failed="N"` for N > 0; `run-edit-mode-tests.sh` exits 1.

**Fix:**
1. Download the `unity-test-results-<run>` artifact from the failed workflow.
2. Open `artifacts/editmode/*.xml` or `artifacts/playmode/*.xml`.
3. Each failure has `<failure><message>...</message><stack-trace>...</stack-trace></failure>` — paste into a local test re-run to reproduce.
4. Re-run only the failing test locally:
   ```bash
   TEST_FILTER='Brave.Tests.EditMode.Boot.BuildScriptsTests' \
     ./games/brave-bunny/tools/ci/scripts/run-edit-mode-tests.sh
   ```

## 6. PlayMode smoke test zero-match

**Signature:**
```
Test category 'Smoke' matched 0 tests.
```

**Cause:** PlayMode tests were renamed or had their `[Category("Smoke")]` attribute removed.

**Fix:** Confirm `VerticalSliceSmokeTest.cs` (and friends under `Tests/PlayMode/Smoke/`) carry `[Category("Smoke")]`. If you intentionally renamed the category, update the workflow's `testCategory: "Smoke"` line.

## 7. Unity build failure

**Signature:**
```
[BuildScripts] build failed: Failed
```
or
```
BuildPipeline.BuildPlayer error: ...
```

**Cause:** Many — but the top three are:

1. **IL2CPP compile error on an iOS-only path.** Open the Unity build log artifact (`unity-build-logs-<run>`) and search for `error CS` or `il2cpp: error`. Reproduce locally with:
   ```bash
   GIT_COMMIT_SHA=$(git rev-parse HEAD) \
     ./games/brave-bunny/tools/ci/scripts/build-ios-headless.sh
   ```
2. **Missing scene in EditorBuildSettings.** `BuildScripts.BuildIOS` aborts when zero enabled scenes. Open the project and re-add the scenes — they live in `games/brave-bunny/unity/Assets/_Brave/Scenes/`.
3. **PlayerSettings drift.** A merge accidentally flipped `applicationIdentifier` away from `com.omeryasir.bravebunny`, or `targetOSVersionString`, or `Architecture` (must be ARM64=1). `BuildScripts.ApplyPlatformSettings` re-applies these every run — but only AFTER the build options are resolved, so the EditorBuildSettings scene list must be correct first.

## 8. fastlane match decrypt failure

**Signature:**
```
match: could not decrypt the cert files. wrong MATCH_PASSWORD?
```

**Cause:** `MATCH_PASSWORD` secret is wrong, or the studio-certs repo was re-keyed and the new password wasn't pushed.

**Fix:**
1. Locally: `fastlane match decrypt` with the new password to confirm it works.
2. Rotate on the repo:
   ```bash
   gh secret set MATCH_PASSWORD --repo OmerYasirOnal/studio --body "$NEW_PW"
   ```
3. Last-resort nuke:
   ```bash
   fastlane match nuke distribution
   fastlane match appstore --app_identifier com.omeryasir.bravebunny
   ```

## 9. xcodebuild signing error

**Signature:**
```
error: Code signing is required for product type 'Application' in SDK 'iOS X.X'
```
or
```
No profile for 'Unity-iPhone' matching 'match AppStore com.omeryasir.bravebunny' found
```

**Cause:** Provisioning profile mismatch — either the Unity-generated Xcode project flipped back to Automatic signing, or the profile name doesn't match the one match installed.

**Fix:** The `preview` and `beta` lanes already call `update_code_signing_settings` to force Manual signing. If those lines were edited, restore them. If the profile genuinely is missing, force a refresh:
```bash
cd games/brave-bunny/tools/ci/fastlane
bundle exec fastlane ios refresh_profile
```

## 10. ASC API key error

**Signature:**
```
Cannot generate token: ASC API key invalid
```
or
```
[!] Could not load App Store Connect API key
```

**Cause:** One of `ASC_API_KEY_KEY_ID`, `ASC_API_KEY_ISSUER_ID`, `ASC_API_KEY_CONTENT_B64` is missing or stale. The `Fastfile` resolves the key from `~/.appstoreconnect/private_keys/AuthKey_<key_id>.p8` locally; on CI it expects base64-decoded content in `ASC_API_KEY_CONTENT_B64`.

**Fix:**
1. Verify the three secrets exist:
   ```bash
   ./games/brave-bunny/tools/ci/scripts/verify-secrets.sh
   ```
2. If `ASC_API_KEY_CONTENT_B64` is missing on the repo, regenerate from the local `.p8`:
   ```bash
   base64 < ~/.appstoreconnect/private_keys/AuthKey_93HFBMV3MA.p8 | \
     gh secret set ASC_API_KEY_CONTENT_B64 --repo OmerYasirOnal/studio
   ```
3. Confirm the key role on ASC is at least App Manager — Developer role can't upload binaries.

## 11. ASC upload timeout

**Signature:**
```
Upload to TestFlight: timeout after 600 seconds
```

**Cause:** Apple's ingest is slow; first uploads of a build can take 15-20 min for Apple to process.

**Fix:** The `beta` lane passes `skip_waiting_for_build_processing: true`, so the workflow should NOT actually wait. If you see this timeout, the lane was edited — check the Fastfile. If the upload itself times out, retry the workflow: `gh run rerun <run-id>`.

## 12. mac-minutes budget exhausted

**Signature:**
```
You've used all included macOS minutes...
```

**Cause:** GitHub Free tier private repo gets ~200 mac-minutes/month. A full iOS build burns ~25-35 min, tests add 10-15 min. ~4-5 full runs/month.

**Fix:**
1. Run locally instead — `bundle exec fastlane ios preview` works on the dev machine.
2. Set `skip_tests: true` on workflow_dispatch when iterating on the build step only.
3. If this happens recurrently, escalate to the developer: budget review needed.

---

## Diagnostic artifacts checklist

When a build fails, **all four** of these artifacts should be downloaded for the post-mortem:

- `unity-test-results-<run>` (NUnit XML for EditMode + PlayMode)
- `unity-build-logs-<run>` (`Logs/build-ios-*.log`, batchmode Unity output)
- `xcode-build-logs-<run>` (`~/Library/Logs/gym/`, fastlane archive logs)
- the workflow's own `Run summary` page (commit SHA + lane + skip_tests flags)

If any of these is missing on a failure, the `Upload * (on failure)` step itself failed — check that step's log first.

## Local repro pattern

Every CI failure should be reproducible with a single shell command. The Wave 11 scripts are deliberately env-driven so dev machines can match CI:

```bash
# EditMode tests
./games/brave-bunny/tools/ci/scripts/run-edit-mode-tests.sh

# PlayMode smoke tests
TEST_CATEGORY=Smoke \
  ./games/brave-bunny/tools/ci/scripts/run-play-mode-tests.sh

# Full iOS headless build (Wave 11 entry point)
GIT_COMMIT_SHA=$(git rev-parse HEAD) \
  ./games/brave-bunny/tools/ci/scripts/build-ios-headless.sh

# Local archive (no upload)
cd games/brave-bunny/tools/ci/fastlane
MATCH_PASSWORD=... bundle exec fastlane ios preview
```

If a step fails on CI but passes locally, the difference is almost always one of:

- Unity license source (Personal `.ulf` vs Pro seat)
- Xcode version (CI pinned 16.0 via `ensure_xcode_version`)
- Match profile readonly flag (`true` on CI, `false` locally)
- Missing secret on the repo (`verify-secrets.sh`)
