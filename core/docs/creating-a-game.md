# Creating a new game

```bash
./core/scripts/new-game.sh <slug> --template <action-roguelite|endless-runner|puzzle>
```

The script does five things:

1. Verifies `games/<slug>/` does not exist
2. Copies the chosen template tree from `core/templates/<template>/`
3. Layers `core/templates/_common/` on top
4. Replaces `__GAME_NAME__`, `__DISPLAY_NAME__`, `__TEMPLATE__`, `__SCAFFOLD_DATE__` placeholders
5. Writes `.active-game` = `<slug>`

## After scaffolding

1. Edit `games/<slug>/GAME.md` — fill in `inspiration`, refine `art_style`, `target_devices`, `bundle_id_pattern`.
2. Edit `games/<slug>/CLAUDE.md` if you have genre-specific rules to add.
3. From Claude Code: `/phase-status` to verify Phase 0 is complete.
4. Then `/spawn researcher "<first task>"` to kick off Phase 1.

## Template selection

| Template | Use for |
|---|---|
| `action-roguelite` | Survivor.io / Vampire Survivors / Archero style — auto-attack, swarms, builds |
| `endless-runner` | Subway Surfers / Temple Run / Crossy Road — procedural infinite, swipe controls |
| `puzzle` | Candy Crush / Royal Match — level-based match-3 or tile-puzzle |

Different genres warrant new templates. Adding one is straightforward:

1. Copy `core/templates/action-roguelite/` as a base
2. Rename to your genre slug
3. Customize `GAME.md`, `CLAUDE.md` with genre-specific defaults
4. Update `core/scripts/new-game.sh` arg validation (currently the script accepts any template directory, but keep `core/templates/<name>` to match `--template <name>`)
5. Add a row to the table above
6. Open a PR

## Switching active game

```bash
echo "<other-slug>" > .active-game
```

…or use `/active-game <slug>` from Claude Code. Subsequent `spawn-agent.sh` calls operate on the new active game.
