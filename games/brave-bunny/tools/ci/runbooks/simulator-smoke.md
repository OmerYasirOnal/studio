# Simulator smoke-test runbook

> Wave 12: pre-TestFlight verification pipeline. Owner: build-engineer.

## Why

TestFlight processing burns 15+ minutes and a build number per upload. If we
ship a regression that paints a pink screen (missing shader bundle / broken
render-pipeline asset / IL2CPP-stripped script), the entire cycle is wasted.

This pipeline runs Unity → Xcode → iPhone Simulator → screenshot → pink-pixel
inspection locally in ~3-4 minutes — and on CI in `.github/workflows/bb-simulator-test.yml`.

## Files

| File | Purpose |
|---|---|
| `scripts/unity-build-ios.sh` | Headless Unity build. Now accepts `--target iOS_Simulator`. |
| `scripts/build-for-simulator.sh` | Unity sim build → xcodebuild iphonesimulator → copy `.app` to `Builds/BraveBunny-sim.app`. |
| `scripts/test-in-simulator.sh` | Boot device → install → launch → screenshot → pink-pixel check. |
| `fastlane/Fastfile` (`:simulator` lane) | Wraps build + test scripts. |
| `.github/workflows/bb-simulator-test.yml` | CI entry point (push, PR, manual). |

## Local invocation

```bash
# Full pipeline (Unity build + xcodebuild + sim test):
bash games/brave-bunny/tools/ci/scripts/build-for-simulator.sh
bash games/brave-bunny/tools/ci/scripts/test-in-simulator.sh

# Or via fastlane:
cd games/brave-bunny/tools/ci/fastlane
bundle exec fastlane ios simulator
bundle exec fastlane ios simulator device:"iPhone 17 Pro"
bundle exec fastlane ios simulator skip_unity:true   # reuse last Unity output
```

## Pink-pixel threshold

The test fails when `> 30%` of screenshot pixels match the pink-shader-error
heuristic (R>200 ∧ G<100 ∧ B>200). Override with `BB_PINK_THRESHOLD=0.5`.

## Device selection

Default device: **iPhone 17**. Override with `BB_DEVICE="iPhone 17 Pro"` or
the `device:` fastlane parameter. The original task description specified
"iPhone 15" but the Xcode 16 / macos-latest runner ships iPhone 17 family
simulators only.

## iOS target switch — the most common local failure

If you run `build-for-simulator.sh` with `BB_SKIP_UNITY=1` against an existing
Xcode project that was generated for the **device** SDK, xcodebuild will fail
with:

```
xcodebuild: error: Unable to find a destination matching the provided destination specifier:
    { generic:1, platform:iOS Simulator }
  Available destinations for the "Unity-iPhone" scheme:
    { platform:iOS, id:dvtdevice-DVTiPhonePlaceholder-iphoneos:placeholder, name:Any iOS Device }
```

That means the Unity-generated `Unity-iPhone.xcodeproj` has
`SUPPORTED_PLATFORMS = iphoneos` (device-only). You **must** rebuild via Unity
with `--target iOS_Simulator` — that flips `PlayerSettings.iOS.sdkVersion` to
`SimulatorSDK`, which makes Unity stamp `SUPPORTED_PLATFORMS = iphonesimulator`
in the regenerated project and recompile IL2CPP for `arm64-apple-ios-simulator`.

There is no Xcode-side workaround. The IL2CPP-compiled C++ sources (under
`Build/iOS/Il2CppOutputProject/`) are pre-compiled per the Unity sdkVersion
setting; you cannot swap a device build to simulator after the fact.

Practical workflow:

```bash
# Full rebuild for simulator (clean, ~15-20 min first time, ~5 min warm)
bash games/brave-bunny/tools/ci/scripts/build-for-simulator.sh

# Reuse the simulator Xcode output for a fast xcodebuild iteration
BB_SKIP_UNITY=1 bash games/brave-bunny/tools/ci/scripts/build-for-simulator.sh
```

If the device-pipeline (`fastlane beta`) was the last Unity build, the next
`build-for-simulator.sh` MUST do a full Unity rebuild — the cached Xcode
project is stuck on device SDK until `--target iOS_Simulator` overwrites it.

## iOS target-version note

If `xcodebuild` fails with `iOS Simulator destination not supported` even
after a fresh simulator Unity build, the project may have its
`IPHONEOS_DEPLOYMENT_TARGET` set to a version newer than what the runtime
supports. Fix in Unity: `Edit → Project Settings → Player → Other Settings →
Target minimum iOS Version` → set to 14.0 (matches
`IOSBuilder.ConfigureIOSPlayerSettings`).

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `Unity-iPhone.xcodeproj` missing after build | Unity license expired / asmdef compile error | Inspect `unity/Logs/build-ios-simulator-*.log` |
| `xcodebuild` exit 65 with code-signing errors | Stale `update_code_signing_settings` config from device pipeline | The sim script bypasses signing via `CODE_SIGNING_ALLOWED=NO`; if it still fails, manually toggle the target's "Signing & Capabilities" tab. |
| `simctl install` fails with `MIInstallerErrorDomain` | App built for device, not simulator | Confirm `PlayerSettings.iOS.sdkVersion == SimulatorSDK` in IOSBuilder.cs. Check that `--target iOS_Simulator` was passed. |
| Pink ratio ~1.0 | Render pipeline asset missing or shader bundle stripped | `unity/ProjectSettings/GraphicsSettings.asset` → ensure `m_CustomRenderPipeline` is set; rebuild. |
| `simctl boot` hangs > 60s | Stale simulator state | `xcrun simctl shutdown all && xcrun simctl erase all` |
