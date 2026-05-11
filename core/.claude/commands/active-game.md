---
description: Show or change the active game.
argument-hint: [game-name]
---

# /active-game

If `$ARGUMENTS` is empty: print the contents of `.active-game` and the GAME.md `display_name` field. If `.active-game` is missing, list all directories under `games/` as available candidates and stop.

If `$ARGUMENTS` names a directory under `games/`:

1. Verify `games/$ARGUMENTS/GAME.md` exists.
2. Overwrite `.active-game` with `$ARGUMENTS`.
3. Print the new active game's display name + current phase from `games/$ARGUMENTS/docs/11-roadmap/current-phase.md` (or "Phase 0 — not yet started").

If `$ARGUMENTS` names a directory that doesn't exist: suggest `./core/scripts/new-game.sh $ARGUMENTS --template <t>` and stop.
