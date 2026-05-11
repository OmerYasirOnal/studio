"""Studio Observer — minimal FastAPI dashboard for multi-game agent orchestration.

Localhost-only by design. No authentication, no external network calls.
Tails JSONL logs and renders static HTML in `static/`. Frontend polls the JSON
endpoints every few seconds.

Endpoints:
  GET  /                   redirect to /static/index.html
  GET  /health             liveness check
  GET  /api/games          list of games on disk
  GET  /api/active-game    contents of /.active-game
  GET  /api/agents?game=X  recent agent-status events (default tail = 200)
  GET  /api/handoffs?game=X most recent handoff files (returns metadata only)
  GET  /api/handoff?game=X&path=...  contents of a single handoff
  GET  /api/decisions?game=X  list of ADRs
  GET  /api/decision?game=X&path=...  contents of a single ADR
  GET  /api/phase?game=X   current phase pointer + status
  GET  /api/commits        last 50 framework commits

Static files served at /static.
"""

from __future__ import annotations

import json
import os
from pathlib import Path
from typing import Any

from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse, JSONResponse, RedirectResponse
from fastapi.staticfiles import StaticFiles

# Repo root = three levels up from this file: core/observer/server.py -> core/observer -> core -> repo
ROOT = Path(__file__).resolve().parent.parent.parent
LOGS = ROOT / "logs"
GAMES = ROOT / "games"
STATIC = Path(__file__).resolve().parent / "static"

app = FastAPI(title="Studio Observer", version="0.1.0", docs_url=None, redoc_url=None)
app.mount("/static", StaticFiles(directory=str(STATIC)), name="static")


# ---------- helpers -----------------------------------------------------------


def _tail_jsonl(path: Path, n: int) -> list[dict[str, Any]]:
    """Return the last n parseable JSONL records from path. Tolerant of bad lines."""
    if not path.exists():
        return []
    try:
        with path.open("rb") as fh:
            fh.seek(0, os.SEEK_END)
            size = fh.tell()
            block = min(size, 64 * 1024)
            fh.seek(size - block)
            chunk = fh.read().decode("utf-8", errors="replace")
    except OSError:
        return []
    lines = chunk.splitlines()
    # If we cut mid-line, drop the first.
    if size > 64 * 1024:
        lines = lines[1:]
    out: list[dict[str, Any]] = []
    for line in lines[-n:]:
        line = line.strip()
        if not line:
            continue
        try:
            out.append(json.loads(line))
        except json.JSONDecodeError:
            continue
    return out


def _active_game() -> str:
    f = ROOT / ".active-game"
    if not f.exists():
        return ""
    return f.read_text(encoding="utf-8").strip()


def _validate_game(game: str) -> Path:
    """Resolve a game directory or raise 400."""
    if not game:
        raise HTTPException(400, "missing 'game' query param")
    if "/" in game or game.startswith("."):
        raise HTTPException(400, "invalid game name")
    g = GAMES / game
    if not g.is_dir():
        raise HTTPException(404, f"no game directory: games/{game}")
    return g


def _list_games() -> list[dict[str, Any]]:
    items: list[dict[str, Any]] = []
    if not GAMES.exists():
        return items
    for child in sorted(GAMES.iterdir()):
        if not child.is_dir():
            continue
        gm = child / "GAME.md"
        title = child.name
        if gm.exists():
            for line in gm.read_text(encoding="utf-8", errors="replace").splitlines():
                if line.strip().startswith("display_name:"):
                    title = line.split(":", 1)[1].strip().strip('"').strip("'")
                    break
        items.append({"slug": child.name, "display_name": title})
    return items


# ---------- routes ------------------------------------------------------------


@app.get("/", include_in_schema=False)
def root() -> RedirectResponse:
    return RedirectResponse(url="/static/index.html")


@app.get("/health")
def health() -> dict[str, Any]:
    return {"ok": True, "active": _active_game(), "games": len(_list_games())}


@app.get("/api/games")
def games() -> JSONResponse:
    return JSONResponse({"games": _list_games(), "active": _active_game()})


@app.get("/api/active-game")
def active_game() -> JSONResponse:
    return JSONResponse({"active": _active_game()})


@app.get("/api/agents")
def agents(game: str = "", tail: int = 200) -> JSONResponse:
    records = _tail_jsonl(LOGS / "agent-status.jsonl", n=max(1, min(tail, 2000)))
    if game:
        records = [r for r in records if r.get("game") == game]
    # Group by (agent) for last-known status.
    by_agent: dict[str, dict[str, Any]] = {}
    for r in records:
        a = r.get("agent")
        if not a:
            continue
        by_agent[a] = r
    return JSONResponse({"events": records, "by_agent": by_agent})


@app.get("/api/handoffs")
def handoffs(game: str = "", limit: int = 25) -> JSONResponse:
    g = _validate_game(game) if game else None
    if g is None:
        return JSONResponse({"handoffs": []})
    h_dir = g / "docs" / "handoffs"
    items: list[dict[str, Any]] = []
    if h_dir.exists():
        for p in sorted(h_dir.glob("*.md"), key=lambda p: p.stat().st_mtime, reverse=True)[:limit]:
            items.append({
                "filename": p.name,
                "rel_path": str(p.relative_to(ROOT)),
                "mtime": int(p.stat().st_mtime),
                "size_bytes": p.stat().st_size,
                "agent": p.name.rsplit("-", 1)[0] if "-" in p.name else p.stem,
            })
    return JSONResponse({"handoffs": items})


@app.get("/api/handoff")
def handoff(game: str, path: str) -> JSONResponse:
    g = _validate_game(game)
    p = (ROOT / path).resolve()
    if not str(p).startswith(str(g.resolve())):
        raise HTTPException(400, "path escapes game dir")
    if not p.is_file():
        raise HTTPException(404, "handoff not found")
    return JSONResponse({"content": p.read_text(encoding="utf-8", errors="replace")})


@app.get("/api/decisions")
def decisions(game: str) -> JSONResponse:
    g = _validate_game(game)
    d_dir = g / "docs" / "decisions"
    items: list[dict[str, Any]] = []
    if d_dir.exists():
        for p in sorted(d_dir.glob("*.md")):
            if p.name.upper() == "INDEX.md":
                continue
            title = p.stem
            try:
                head = p.read_text(encoding="utf-8", errors="replace").splitlines()[0:5]
                for line in head:
                    if line.startswith("# "):
                        title = line[2:].strip()
                        break
            except OSError:
                pass
            items.append({"filename": p.name, "rel_path": str(p.relative_to(ROOT)), "title": title})
    return JSONResponse({"decisions": items})


@app.get("/api/decision")
def decision(game: str, path: str) -> JSONResponse:
    g = _validate_game(game)
    p = (ROOT / path).resolve()
    if not str(p).startswith(str(g.resolve())):
        raise HTTPException(400, "path escapes game dir")
    if not p.is_file():
        raise HTTPException(404, "decision not found")
    return JSONResponse({"content": p.read_text(encoding="utf-8", errors="replace")})


@app.get("/api/phase")
def phase(game: str) -> JSONResponse:
    g = _validate_game(game)
    cur = g / "docs" / "11-roadmap" / "current-phase.md"
    body = ""
    if cur.exists():
        body = cur.read_text(encoding="utf-8", errors="replace")
    return JSONResponse({"current": body, "exists": cur.exists()})


@app.get("/api/commits")
def commits(tail: int = 50) -> JSONResponse:
    return JSONResponse({"commits": _tail_jsonl(LOGS / "commits.jsonl", n=tail)})


@app.get("/favicon.ico", include_in_schema=False)
def favicon() -> FileResponse:
    return FileResponse(STATIC / "favicon.svg", media_type="image/svg+xml")
