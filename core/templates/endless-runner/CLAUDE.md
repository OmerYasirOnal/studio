# Game-level rules — extends `/CLAUDE.md`

- Slug: `__GAME_NAME__`
- Genre: endless-runner
- Template: `__TEMPLATE__`

## Genre-specific rules

- **Procedural generation chunks** must be deterministic given a seed for replay debugging.
- **Death is the input event** — every death must analytically log distance + cause for balance-engineer.
- **Hand-feel target**: input-to-visible-response latency ≤ 1 frame at 60 fps.
