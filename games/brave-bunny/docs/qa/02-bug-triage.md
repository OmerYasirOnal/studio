# QA 02 — Bug Triage & SLA

> Owner: qa-engineer. Cross-references: `00-test-plan.md`, `04-known-issues.md`, repo-root `CLAUDE.md` (escalation triggers).

## Severity definitions

| Sev | Name | Definition | SLA |
|---|---|---|---|
| **P0** | Crash / data loss | App crashes on boot, run, or run-end. Save file becomes unreadable. Player-paid IAP entitlement lost. | Fix within **4 hours** of report. Hot-fix release. Stop-the-line. |
| **P1** | Block | Core feature unusable for ≥ 50% of users on a supported device. Examples: joystick dead, run never ends, shop fails to load, rewarded ad always errors. | Fix before next release. Target **≤ 48 hours**. |
| **P2** | Major | Feature degraded but workaround exists. Examples: occasional draft card UI overlap, wave 7 over-spawns by 20, audio bus level off by ±3 dB. | Schedule in current sprint. |
| **P3** | Minor | Cosmetic but visible. Examples: localized string overflow on iPhone SE 3, particle puff missing on one enemy type, tooltip clipped. | Backlog. Pick up opportunistically. |
| **P4** | Cosmetic / nitpick | Off-pixel alignment, tone-bible voice drift in one string, particle puff one frame late. | Backlog, no SLA. |

## Pillar-violation flags (auto-promote to P1)

These bugs auto-promote to **P1** even if their visible symptom is minor — they violate a "feel pillar" or positioning brief:

- Any "Game Over" string in production copy (Pillar 6 violation per US-26).
- Any tap with > 1-frame response on joystick / pause / draft pick (Pillar 5 violation per US-13).
- Any interstitial ad firing during gameplay (positioning violation per US-46).
- Any shop SKU that affects gameplay stats (positioning violation per US-43, US-49).
- Any save file write that exceeds 200 KB at typical mature profile (ADR-0008 size budget).
- Any unfair death — boss attack without ≥ 600 ms telegraph (US-20).

## Triage workflow

```
report → triage (qa-engineer) → assign severity → route to owner → fix → verify → close
```

- **Triage owner**: qa-engineer for the first 24 h; can escalate to tech-architect for ADR-level questions.
- **Routing**: by file ownership map in repo-root `CLAUDE.md` (e.g., `Scripts/Gameplay/` → gameplay-engineer).
- **Verification**: qa-engineer reproduces fix on the affected device + signs off on the issue card.
- **Logging**: every closed bug gets a row in `04-known-issues.md` for the public-facing release notes.

## Escalation path

Per repo-root `CLAUDE.md` § Escalation triggers, raise to the human orchestrator only when:

- A P0 cannot be reproduced in 3 different approaches and 2+ hours.
- Apple Developer interactive UI is needed (cert, App Store Connect agreement).
- The test suite has been broken across the last 5 commits and revert doesn't fix it.

Otherwise: write an ADR, decide, proceed.

## Areas + triage owners

| Area | Code path | Primary owner | Backup |
|---|---|---|---|
| Run-loop combat | `Scripts/Gameplay/Combat/` | gameplay-engineer | tech-architect |
| Save / data | `Scripts/Systems/Save/` | systems-engineer | tech-architect |
| HUD / UI | `Scripts/UI/` | ui-engineer | art-director |
| Wave timing | `data/waves/*.json` | level-designer | balance-engineer |
| Balance tuning | `data/balance/*.json` | balance-engineer | game-designer |
| Localization | `_Brave/Localization/Tables/` | ui-engineer | narrative-designer |
| Build / CI | `tools/ci/` | build-engineer | tech-architect |

## Cross-references

- `00-test-plan.md` — strategy.
- `04-known-issues.md` — running defect log.
- ADR-0008 / ADR-0009 — invariants whose violation auto-promotes to P1.
- `docs/03-user-stories/` — feel-pillar definitions cited above.
