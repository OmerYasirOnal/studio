# Contributing to Studio

Studio is an opinionated multi-agent game-development framework. Right now it is optimized for one developer (Yasir) and one game (Brave Bunny), but contributions are welcome — especially:

- New game templates under `core/templates/`
- New game-agnostic agent definitions under `core/.claude/agents/`
- CC0 asset pipeline scripts under `core/tools/asset-pipeline/`
- Observer dashboard improvements

## Non-negotiables

1. **Zero external paid APIs.** No ElevenLabs, no Meshy, no OpenAI, no Replicate. The framework must run with `claude --dangerously-skip-permissions` and nothing else. See `core/docs/asset-policy.md`.
2. **Framework / Game separation.** Any code that is specific to a single game belongs in `games/<name>/`, never in `core/`. The litmus test: *Would this be useful in a different game?*
3. **CC0 / OFL / MIT / CC-BY licenses only** for assets and dependencies. No exceptions.
4. **Conventional Commits.** `feat:`, `fix:`, `docs:`, `refactor:`, `chore:`. Atomic commits.
5. **No magic numbers.** Balance and tuning lives in JSON + ScriptableObjects.

## Development workflow

1. Fork or branch from `main`.
2. Make atomic commits.
3. Pre-commit hooks must pass (markdownlint, ruff, shellcheck).
4. CI must be green (`.github/workflows/ci.yml`).
5. Open a PR with a clear scope statement and a link to the user story or issue.

## Adding a new agent

1. Create `core/.claude/agents/<name>.md` using the template described in `core/docs/agent-system.md`.
2. Use `<active>` placeholder for paths — never hard-code `games/brave-bunny`.
3. Add the agent to the roster in `README.md`.
4. Update `core/docs/agent-system.md` agent table.

## Adding a new game template

1. Copy `core/templates/_common/` as the base.
2. Add genre-specific docs scaffolding under `docs/`.
3. Add an entry to `core/scripts/new-game.sh` `--template` validation.
4. Document the template in `core/docs/creating-a-game.md`.

## Bug reports & feature requests

Use `.github/ISSUE_TEMPLATE/bug.md` or `.github/ISSUE_TEMPLATE/feature.md`.
