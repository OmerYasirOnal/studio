---
description: Create a new ADR for the active game.
argument-hint: "<short topic>"
---

# /decide

Read `.active-game`. Locate the next ADR number under `games/<active>/docs/decisions/` (highest existing `NNNN-` + 1, zero-padded to 4 digits, starting from `0001`).

Create `games/<active>/docs/decisions/<NNNN>-<topic-slugified>.md` with this template:

```markdown
# ADR <NNNN> — <topic>

**Date:** <YYYY-MM-DD>
**Status:** proposed
**Owner:** <agent or "human">

## Context

<1-2 paragraphs: what forced this decision?>

## Decision

<single sentence: what did we decide?>

## Consequences

<3-5 bullets: what changes downstream?>

## Alternatives considered

- **<alt 1>** — <why rejected>
- **<alt 2>** — <why rejected>
- **<alt 3>** — <why rejected>

## References

- <links to handoffs, docs, code, external sources>
```

Open the file ready for editing. After save, append to `games/<active>/docs/decisions/INDEX.md` (create if missing).
