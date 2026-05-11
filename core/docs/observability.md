# Observability

Studio is observable by default. Everything important emits a JSONL line.

## Event streams

| File | Schema | Producer |
|---|---|---|
| `logs/agent-status.jsonl` | `{ts, game, agent, status, action, detail, task?, session?, log?}` | `spawn-agent.sh`, agents themselves |
| `logs/commits.jsonl` | `{ts, sha, short, author, msg, files, game}` | post-commit git hook |
| `logs/framework-events.jsonl` | `{ts, event, ...}` | `new-game.sh` and friends |
| `logs/observer.log` | plain text (uvicorn) | observer process |
| `games/<g>/logs/<agent>/*.log` | tmux pipe-pane raw | per-agent terminal capture |

## Agent status taxonomy

```
spawning  → tmux session opened, claude not yet attached
working   → agent actively producing output
idle      → agent waiting (e.g., orchestrator between phase boundaries)
blocked   → agent surfaced a blocker (escalation candidate)
done      → agent finished, hand-off note written
```

## Observer dashboard

`./core/scripts/observer-start.sh` starts a FastAPI server on `localhost:7777`. Reads:

- `/api/games` → list of games on disk
- `/api/active-game` → contents of `.active-game`
- `/api/agents?game=<g>` → tail of agent-status events, plus the latest event per agent
- `/api/handoffs?game=<g>` → recent hand-off files
- `/api/handoff?game=<g>&path=<rel>` → single hand-off body
- `/api/decisions?game=<g>` → ADR list
- `/api/decision?game=<g>&path=<rel>` → single ADR body
- `/api/phase?game=<g>` → contents of `current-phase.md`
- `/api/commits` → tail of `commits.jsonl`
- `/health` → liveness

The dashboard polls these every 4-5 s. Localhost-only by design — `--host 127.0.0.1`.

## Tailing manually

```bash
tail -f logs/agent-status.jsonl | jq .
tmux attach -t studio-<game>-<agent>      # detach: ctrl-b d
```

## How agents log

From inside a tmux Claude Code session, the agent uses the `filesystem` MCP to append a JSON line to `logs/agent-status.jsonl`. Pattern:

```python
# pseudo — agents do this via Bash tool
echo '{"ts": <unix>, "game": "<g>", "agent": "<a>", "status": "working", "action": "writing", "detail": "<doc>"}' >> logs/agent-status.jsonl
```

## Privacy

The observer **never** sends data anywhere. It binds to localhost. There is no telemetry, no analytics, no crash reporter. If you want to share state with a teammate, push the relevant `logs/` files to git.
