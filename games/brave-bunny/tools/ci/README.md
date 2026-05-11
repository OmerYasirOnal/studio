# CI tooling — `Brave Bunny`

Owned by build-engineer. Populated in Phase 6 (Vertical Slice) when the first TestFlight build is needed.

```
ci/
  fastlane/         # Fastfile, Appfile, Matchfile, Pluginfile
  github-actions/   # ios-build.yml, unity-test.yml, lint.yml
  scripts/          # unity-build-ios.sh, archive.sh, upload-testflight.sh
  runbooks/         # manual-step recipes when CI hits an escalation trigger
```

No secrets in this directory. GitHub Actions secrets only.
