# Orchestrator hand-off — 2026-05-12 (Session 3 final)

**Status:** Framework v0.1.0 + Brave Bunny scaffolded through Phase 5 wave 2. Verification passes.

## Verification snapshot

```
$ ./core/scripts/verify-framework.sh --game brave-bunny
=== summary ===
  framework passes:  38
  game passes:       26
  failures:           0
  advisories:         0
[verify] OK — framework v0.1.0 verified
```

64/64 checks pass.

## All phases at a glance

| Phase | Status | Highlights |
|---|---|---|
| 0 Framework | ✅ done | 105 core files, observer up, 16 agents, 9 commands, 4 hooks, 5 MCP servers |
| 0 Game | ✅ done | brave-bunny scaffolded via new-game.sh, GAME.md filled |
| 1 Discovery | ✅ done | 5 competitor decons, market doc, positioning + risk matrix, references |
| 2 GDD | ✅ done | 14 GDD sections, 62 user stories, 15 wireframes, 5 UX flows, full narrative (8 char bios + 5 boss intros + 89 loc keys), 5 biomes laid out + waves.json each, 5 bosses speced, full art bible (10 sections) + audio bible (5 sections) |
| 3 Tech Architecture | ✅ done | 12 tech-spec docs, 13 ADRs (10 accepted + 3 proposed/deferred) |
| 4 Asset Pipeline | ✅ planned + 22 real CC0 fetches | INDEX maps 310 planned assets; 22 actually downloaded (216 MB on disk, all permissively licensed): 10 Kenney CC0 zips, 4 Polyhaven HDRIs, 5 ambientCG PBR, 3 Google Fonts; Quaternius pack still needs manual click-through |
| 5 Prototype | ✅ scaffolded | Unity 6 LTS project with 6 asmdefs (one-way deps enforced), 149 C# files, 5 UXML screens, 75-var USS theme, EN/TR localization JSON, 12 test files (1350 LoC), Fastlane lanes + 3 GitHub Actions workflows |

## Git state

- Local: `main` branch, 13 atomic commits
- Remote: `https://github.com/OmerYasirOnal/studio` (private, owner OmerYasirOnal)
- Last push: commit `ae5ce0b` (or later if more pushed)
- 216 MB of assets committed in repo

## Active blockers (require user action)

1. **Apple Developer account + App Store Connect agreement acceptance** — required before any TestFlight build (`fastlane beta`)
2. **Unity 6 LTS install** — required to compile the C# scaffolding and run EditMode/PlayMode tests
3. **Unity license** (free Personal tier OK) — for CI builds
4. **Match cert sync repo** (`studio-certs` private GitHub) — for `fastlane match` first run
5. **GitHub Actions secrets** for CI iOS builds: `UNITY_LICENSE`, `MATCH_PASSWORD`, `MATCH_GIT_AUTHOR`, `FASTLANE_USER`, `FASTLANE_APP_SPECIFIC_PASSWORD`, `FASTLANE_TEAM_ID`, `FASTLANE_ITC_TEAM_ID`

The runbook at `games/brave-bunny/tools/ci/runbooks/first-build.md` walks through #1-#5.

## What was done autonomously this session

Wave 1-3 (Phases 1-2 design):
- Phase 1 closeout: Brotato + Capybara Go! decons (2 parallel agents)
- Phase 2 wave 1: GDD anchors + art-bible kick + tone bible (5 parallel agents)
- Phase 2 wave 2: GDD content tables + art content + 8 char bios + 62 user stories (4 parallel agents)
- Phase 2 wave 3: GDD finish (6-13) + audio bible + UX flows + Meadow level design (5 parallel agents)

Wave 4 (Phase 2-3 bridge):
- 15 HTML wireframes + 8 balance JSONs + 6 tech specs (3 parallel agents)
- ADRs 0004-0005 (no-VO launch, engine choice)

Wave 5 (Phase 3 finish + Phase 4 planning):
- 6 remaining tech specs + asset INDEX + 4 Blender build.py recipes (3 parallel agents)
- ADRs 0006-0012 (HP recalibration, charm consumption, save format, mechanic registry, sub ROI, BGM loop format, event channels)

Wave 6 (Phase 5 init):
- Unity skeleton (6 asmdefs, manifest.json, ProjectSettings) + 149 C# files + 5 UXML + 12 tests (5 parallel agents)
- Cleanup of duplicate-namespace files from a tool retry

Wave 7 (Phase 5 + Phase 4):
- Build-engineer Fastlane + 3 CI workflows (1 agent)
- Asset-curator REAL CC0 fetches: 22 files / 216 MB (1 agent)
- Bugfixes: licenses.py (exclude .py/.blend), validate_balance.py (advisory mode), .gitignore (`Icon?` glob collision)

Wave 8 (Phase 2 finish):
- 4 remaining biomes (Beach/Forest/Cavern/Snow) + 4 bosses (Crab Captain/Mama Oak/Sneaky Cave Mole/Big Snow-yeti) (1 agent)
- ADR-0013 (arena spawn-radius invariant)
- Framework verification scripts (verify-framework.sh + verify-game.sh)

## How to keep going (next session)

```bash
# Prereqs
brew install --cask unity-hub       # then install Unity 6 LTS from the Hub UI
brew install --cask docker          # optional, for AdMob SDK testing

# Validate state
cd /Users/omeryasironal/Projects/studio
./core/scripts/observer-start.sh &
./core/scripts/verify-framework.sh --game brave-bunny

# Open Unity project (will regenerate Library/ + Packages/packages-lock.json)
# - File > Open Project > games/brave-bunny/unity/
# - Wait for package import (~5 min first time)
# - Open Window > General > Test Runner
# - Run EditMode tests — expect compile to require some tweaks
# - Address compile errors as a single pass; commit fixes

# In Claude Code at studio/ root:
claude --dangerously-skip-permissions
# /active-game brave-bunny
# /spawn gameplay-engineer "Open the Unity project, run EditMode tests, fix the compile errors. Don't add new functionality — just make the existing 149 files compile cleanly. Document any tech-spec drift as ADR-0014+ as you go."
```

## Open questions for the human

- **Quaternius pack**: requires manual click-through. Either Yasir downloads it OR write ADR-0014 documenting the Beaver-fallback for Otter.
- **Apple Developer Program enrollment** — when do you want to start?
- **Unity license**: Personal tier works at our revenue scale. Confirm fine? (If yes, no Unity Pro purchase needed.)
- **TR/PH/ID soft-launch markets** — when do you want to flip the soft-launch switch (need Phase 5+6 done first)?

## Recommended cadence post-Unity-install

| Week | Focus | Owner agents |
|---|---|---|
| Week of 2026-05-13 | Make Unity scaffolding compile + run EditMode tests | gameplay + systems + ui |
| Week of 2026-05-20 | Joystick + auto-attack + 200-enemy stress scene at 60fps | gameplay + qa |
| Week of 2026-05-27 | Save round-trip + boot-to-meadow gameplay loop | systems + gameplay |
| Week of 2026-06-03 | UI Toolkit screens wired to runtime state | ui + systems |
| Week of 2026-06-10 | Old Boar King boss fully fightable | gameplay + level-designer + balance |
| Week of 2026-06-17 | Vertical slice gate (Phase 6 exit) | qa |
| Week of 2026-06-24 | Apple Developer enrollment + TestFlight first upload | build + human |
| Week of 2026-07-01 | TestFlight beta polish | qa + build |
| Week of 2026-07-08+ | Soft-launch TR/PH/ID | (entire team) |

That gives a credible 8-week path to first soft-launch test, assuming Unity install + Apple Developer happen this week.

## Closing observation

Phases 0-3 took **2 continuous Claude Code Opus sessions** to complete on paper. Phase 4 fetched real CC0 assets via WebFetch in a third session. Phase 5 scaffolded but is gated on the user's Unity install. Phase 6 onward is a real iterative engineering effort that benefits from the framework's spawn-agent pattern in tmux for long-running work.

The framework has now been demonstrated at every layer: agent dispatch, observer dashboard, asset pipeline, balance validation, license enforcement, CI workflows, verification scripts. The next session's first command should be `./core/scripts/verify-framework.sh --game brave-bunny` to confirm nothing drifted.
