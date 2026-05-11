# Changelog

All notable changes to the Studio framework will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Game-specific changelogs live in `games/<name>/CHANGELOG.md`. This file only tracks `core/`.

## [Unreleased]

## [0.1.0] - 2026-05-11

### Added
- Framework directory layout (`core/`, `games/`, `shared/`)
- 16 game-agnostic agent definitions under `core/.claude/agents/`
- 9 slash commands under `core/.claude/commands/`
- 4 pre/post hooks under `core/.claude/hooks/`
- MCP config with 5 zero-key servers (filesystem, git, sequential-thinking, memory, fetch)
- CLI scripts: `new-game.sh`, `spawn-agent.sh`, `observer-start.sh`, `ralph.sh`, `status.sh`
- Observer dashboard (FastAPI + vanilla HTML) on `localhost:7777`
- Game templates: `action-roguelite`, `endless-runner`, `puzzle`, shared `_common`
- Asset pipeline scripts for Quaternius / Kenney / Freesound (CC0 only)
- Framework documentation (`getting-started`, `creating-a-game`, `agent-system`, `observability`, `asset-policy`, `architecture`)
- GitHub Actions CI + observer smoke test
- First game `games/brave-bunny/` scaffolded with the `action-roguelite` template
