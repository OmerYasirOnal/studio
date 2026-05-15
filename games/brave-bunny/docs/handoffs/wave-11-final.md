# Wave 11 — Final session handoff (2026-05-16)

> 50-line handoff for the next orchestrator session. Full context in
> `docs/11-roadmap/wave-7-through-11-summary.md`.

## What's done

- **Phase 5 closed.** 5-wave / 40-agent push (Waves 7A → 11) merged in one day. ~30k LOC across 402 files, commits `c200dd5` → `aa846cb` on `main`.
- Vertical-slice gameplay (18 weapons, 24+ enemies across 5 biomes, Old Boar King 3-phase boss, 8 characters, status effects, crit, combo, evolution).
- Meta + liveops scaffolds (achievements × 20, lifetime profile, daily login, daily quests, battle pass 30-tier, shop, IAP catalog).
- Infrastructure (TR/EN loc parity ~400 keys, local JSONL telemetry, tutorial overlay, focus-pause, run QoL, SceneSetup wires everything into Run scene).
- 1 new ADR landed: **ADR-0022** (boss HP recalibration). ADR-0006 partially superseded. `decisions/INDEX.md` is current.

## What's blocked

1. **iOS 26.5 SDK install on the build mac** — user-side, one-time. Until done, `fastlane beta` won't archive build #3. The last TestFlight binary (`0.1.0(2)`, 2026-05-12) predates all of Wave 6→10.
2. **Untested-in-Editor risk** — none of Waves 7A→10 was compiled inside Unity this session. First Editor open of `main` will almost certainly need a compile-cleanup pass (namespace / asmdef / duplicate-type issues). See `wave-7-through-11-summary.md` § Honesty.

## Unambiguous next step

```bash
# Step 1 — verify the merged main actually compiles.
./core/scripts/spawn-agent.sh qa-engineer "Open the Unity project at games/brave-bunny/unity. Compile main (commit aa846cb). If compile fails, fix the smallest set of errors needed to reach green. Run all EditMode and PlayMode tests. Report: pass count vs. the 47/47 baseline at cb36929, plus a list of any compile fixes you had to make. Single chore commit."
```

After Step 1 is green, the remaining sequence (in priority order, from `current-phase.md`):

1. `fastlane beta` build #3 → TestFlight (build-engineer + human, requires iOS 26.5 SDK).
2. Replace shop / battle-pass / daily-login mock receipts with real Unity IAP + sandbox validation (systems-engineer).
3. Replace local JSONL telemetry with a real analytics SDK; keep JSONL as dev fallback (systems-engineer).
4. AdMob SDK + ATT consent + rewarded-revive wiring (build-engineer).
5. Battle Pass Season 1 reward authoring (level-designer).
6. 3-day device QA gate (qa-engineer).
7. Soft-launch TR / PH / ID rollout (build-engineer + human).

## Reading order

1. `docs/11-roadmap/current-phase.md` (live status board)
2. `docs/11-roadmap/wave-7-through-11-summary.md` (canonical session record + honesty section + per-wave commit table)
3. `docs/decisions/INDEX.md` (22 ADRs)
4. `docs/10-balance/wave7-ttk-pass.md` (boss/weapon balance follow-ups)
5. Subsystem-specific docs only as needed.

## Carry-over follow-ups (full list in wave-7-through-11-summary.md § d)

- **RunEndReport missing `boss_defeated` slug** — needs to be added for achievement + analytics triggers.
- L5 DPS band on Carrot Boomerang & Acorn Cannon — re-verify on next balance pass.
- Asset stub shortlists from Wave 8 — real CC0 fetches still TODO for ~half of Wave 9's new enemies + weapons.
- Battle pass season-1 reward content not authored; daily-quest pool is only 5 templates.

## Sign-off

This handoff exists because Phase 5 was closed by a 40-agent parallel push that nobody ran in the Editor. The next session's first commit must be the Editor smoke + compile-clean pass before any new feature work begins.
