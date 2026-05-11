---
description: Trigger an iOS build for the active game.
argument-hint: [preview|beta|release]
---

# /build-ios

Resolve `.active-game`. Verify `games/<active>/tools/ci/fastlane/Fastfile` exists. If not, spawn `build-engineer` agent first with the task "Bootstrap Fastlane for iOS build" and stop.

Otherwise, run the requested lane (default `preview`):

```bash
cd games/<active>/tools/ci/fastlane && fastlane $ARGUMENTS
```

Stream output. After completion, append the build result (success/fail + artifact path) to `logs/agent-status.jsonl` under agent `build-engineer`.

If any escalation trigger is hit (interactive cert UI, App Store Connect agreement), surface to the human with a runbook link.
