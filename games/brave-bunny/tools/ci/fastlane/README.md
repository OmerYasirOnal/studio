fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## iOS

### ios register_app

```sh
[bundle exec] fastlane ios register_app
```

Create Apple Developer bundle ID + App Store Connect app entry (idempotent)

### ios enable_iap

```sh
[bundle exec] fastlane ios enable_iap
```

Enable IAP capability on the bundle id (idempotent)

### ios refresh_profile

```sh
[bundle exec] fastlane ios refresh_profile
```

Force-refresh the appstore provisioning profile

### ios list_apps

```sh
[bundle exec] fastlane ios list_apps
```

List all apps and bundle IDs on the Apple Developer account (read-only)

### ios preview

```sh
[bundle exec] fastlane ios preview
```

Local archive without uploading (smoke test the pipeline)

### ios beta

```sh
[bundle exec] fastlane ios beta
```

Build and upload to TestFlight

### ios beta_local

```sh
[bundle exec] fastlane ios beta_local
```

Build and upload to TestFlight without match (sigh-based)

### ios upload_existing

```sh
[bundle exec] fastlane ios upload_existing
```

Upload an existing app-store-signed .ipa to TestFlight (validation shortcut)

### ios rearchive

```sh
[bundle exec] fastlane ios rearchive
```

Re-archive the existing Xcode project + upload to TestFlight (skips Unity rebuild)

### ios release

```sh
[bundle exec] fastlane ios release
```

App Store submission (manual gate — binary upload only, no auto-submit)

### ios simulator

```sh
[bundle exec] fastlane ios simulator
```

Build for iOS Simulator, launch, screenshot, pink-pixel regression check

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
