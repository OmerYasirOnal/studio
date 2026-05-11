# Getting started

## Prerequisites

- macOS or Linux (Windows works for the framework, but iOS builds require macOS)
- [Claude Code](https://claude.com/claude-code) installed and authenticated
- `python3 ≥ 3.11`
- `tmux ≥ 3.0`
- `git ≥ 2.40`
- For engine work (later): **Unity 6 LTS** with iOS build module
- For asset work (later): **Blender 4.x**

## First-run

```bash
git clone <studio-repo-url> studio
cd studio

# Install python deps for the observer (uses a per-tool venv automatically)
./core/scripts/observer-start.sh

# (in another terminal — or after Ctrl-C of the observer if foreground)
./core/scripts/new-game.sh brave-bunny --template action-roguelite
$EDITOR games/brave-bunny/GAME.md
```

Now open Claude Code in the repo root:

```bash
claude --dangerously-skip-permissions
```

Inside Claude Code, type `/phase-status` to see Game Phase 0 status, then `/spawn researcher "begin Phase 1 — competitor deconstruction"`.

## What you should see

- `localhost:7777` shows the observer dashboard
- A "studio-brave-bunny-researcher" tmux session appears
- After ~10 minutes, `games/brave-bunny/docs/01-research/` starts to fill in
- `logs/agent-status.jsonl` accumulates events the dashboard tails

## When something breaks

1. Check `logs/observer.log` for observer failures
2. Check `games/<active>/logs/<agent>/*.log` for individual agent transcripts
3. Run `./core/scripts/status.sh` for a one-shot health summary
4. Read `core/docs/observability.md` for the full event taxonomy

## Mental model

- `core/` is the **framework** — never write game-specific code here
- `games/<name>/` is your **product**
- Agents are **stateless** between sessions — they only know what's in files
- Communication between agents = filesystem + `memory` MCP only
