# QA 04 — Known Issues (Living Document)

> Owner: qa-engineer. Append-only log of currently-open defects + recently-closed ones (kept for release-notes triangulation). Severity definitions in `02-bug-triage.md`. Cross-reference `00-test-plan.md` for the live-monitor SLOs that promote latent issues into this list.

## How to use this file

- Add a row when a defect is opened. Update the **Status** column as it moves.
- Severity per `02-bug-triage.md` (P0..P4). Status: `open` / `in-progress` / `fixed` / `wontfix` / `cantrepro`.
- "Workaround" is what the player or QA does today; "Fix tracked in" is the PR / commit / ADR ref.
- Closed-but-recent issues stay here for 1 release cycle, then archive to `logs/qa/archive-<sprint>.md`.

## Open issues

| ID | Sev | Title | Status | First seen | Workaround | Fix tracked in |
|---|---|---|---|---|---|---|
|   |   |   |   |   |   |   |

## Recently closed (kept for release notes)

| ID | Sev | Title | Closed on | Fixed in | Notes |
|---|---|---|---|---|---|
|   |   |   |   |   |   |

## Convention

- **ID** format: `BB-<NNN>` (Brave Bunny + 3-digit number, monotonic).
- **First seen** is the build SHA or TestFlight build number where QA first observed it.
- **Fix tracked in** is the merging PR URL (preferred) or commit SHA.
- New rows go at the **top** of the Open table for visibility.
