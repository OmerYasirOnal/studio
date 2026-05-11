---
description: Scaffold a new game project from a template.
argument-hint: <game-name> [--template action-roguelite|endless-runner|puzzle]
---

# /new-game

Pass `$ARGUMENTS` directly to:

```bash
./core/scripts/new-game.sh $ARGUMENTS
```

The script will:

1. Verify `games/<name>/` does not exist
2. Copy `core/templates/<template>/` and `core/templates/_common/` into `games/<name>/`
3. Replace `__GAME_NAME__` placeholders in `GAME.md`
4. Write `.active-game` = `<name>`

After it returns: tell the user to edit `games/<name>/GAME.md` and then run `/phase-status` to begin Game Phase 1.
