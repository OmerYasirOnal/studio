#!/usr/bin/env python3
"""licenses.py — validate that every asset under games/<active>/assets-raw/ has a row in LICENSES.md.

Usage:
  python core/tools/asset-pipeline/licenses.py --validate
  python core/tools/asset-pipeline/licenses.py --validate --game brave-bunny
  python core/tools/asset-pipeline/licenses.py --report
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

ALLOWED_LICENSES = {"CC0", "CC0 1.0", "CC-BY", "CC-BY 4.0", "MIT", "SIL OFL", "SIL OFL 1.1"}

EXCLUDED_NAMES = {"LICENSES.md", "README.md", ".DS_Store", "INDEX.md"}


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[licenses] no .active-game and --game not provided", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def parse_license_table(licenses_md: Path) -> dict[str, dict[str, str]]:
    """Return {relative_path: {source, license, url, author, fetched}}."""
    if not licenses_md.exists():
        return {}
    rows: dict[str, dict[str, str]] = {}
    in_table = False
    for line in licenses_md.read_text(encoding="utf-8").splitlines():
        if re.match(r"^\s*\|\s*-+", line):
            in_table = True
            continue
        if not in_table:
            continue
        if not line.strip().startswith("|"):
            in_table = False
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if len(cells) < 6:
            continue
        if cells[0].lower().startswith("file") or cells[0].startswith("_"):
            continue
        rows[cells[0]] = {
            "source": cells[1],
            "license": cells[2],
            "url": cells[3],
            "author": cells[4],
            "fetched": cells[5],
        }
    return rows


def list_asset_files(assets_raw: Path) -> list[Path]:
    if not assets_raw.exists():
        return []
    out: list[Path] = []
    for p in assets_raw.rglob("*"):
        if not p.is_file():
            continue
        if p.name in EXCLUDED_NAMES:
            continue
        if any(part.startswith(".") for part in p.relative_to(assets_raw).parts):
            continue
        out.append(p)
    return out


def cmd_validate(game: str) -> int:
    root = repo_root()
    assets_raw = root / "games" / game / "assets-raw"
    licenses_md = assets_raw / "LICENSES.md"
    rows = parse_license_table(licenses_md)
    files = list_asset_files(assets_raw)

    errors: list[str] = []
    for f in files:
        rel = str(f.relative_to(assets_raw))
        if rel not in rows:
            errors.append(f"missing row in LICENSES.md: {rel}")
            continue
        lic = rows[rel]["license"].strip()
        # Accept anything that contains an allowed string.
        if not any(allowed in lic for allowed in ALLOWED_LICENSES):
            errors.append(f"disallowed license '{lic}' for {rel}")

    # Also: every row should point to a real file.
    for rel in rows:
        if not (assets_raw / rel).is_file():
            errors.append(f"row points to missing file: {rel}")

    if errors:
        for e in errors:
            print(f"[licenses] FAIL: {e}", file=sys.stderr)
        print(f"[licenses] {len(errors)} problem(s) — game={game}", file=sys.stderr)
        return 1

    print(f"[licenses] OK — {len(files)} files, {len(rows)} rows, all licensed permissively (game={game})")
    return 0


def cmd_report(game: str) -> int:
    root = repo_root()
    licenses_md = root / "games" / game / "assets-raw" / "LICENSES.md"
    rows = parse_license_table(licenses_md)
    by_license: dict[str, int] = {}
    by_source: dict[str, int] = {}
    for r in rows.values():
        by_license[r["license"]] = by_license.get(r["license"], 0) + 1
        by_source[r["source"]] = by_source.get(r["source"], 0) + 1
    print(f"# Asset license report — {game}\n")
    print(f"Total assets logged: {len(rows)}\n")
    print("## By license\n")
    for k, v in sorted(by_license.items(), key=lambda x: -x[1]):
        print(f"- {k}: {v}")
    print("\n## By source\n")
    for k, v in sorted(by_source.items(), key=lambda x: -x[1]):
        print(f"- {k}: {v}")
    return 0


def main() -> int:
    ap = argparse.ArgumentParser(description="License validator for studio asset pipeline.")
    ap.add_argument("--validate", action="store_true", help="Run validation; exit non-zero on error.")
    ap.add_argument("--report", action="store_true", help="Print a summary report.")
    ap.add_argument("--game", help="Game slug. Defaults to .active-game.")
    args = ap.parse_args()
    g = active_game(args.game)
    if args.validate:
        return cmd_validate(g)
    if args.report:
        return cmd_report(g)
    ap.print_help()
    return 0


if __name__ == "__main__":
    sys.exit(main())
