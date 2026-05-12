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

### ios release

```sh
[bundle exec] fastlane ios release
```

App Store submission (manual gate — binary upload only, no auto-submit)

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
