#!/usr/bin/env python3
"""validate_balance.py — sanity-check balance JSONs against schema docs.

Each JSON file in games/<active>/data/balance/ should have a sibling .schema.md
documenting field names and units. This script:

1. Parses each JSON (fails on parse errors)
2. Cross-references top-level keys against the field table in the schema doc
3. Reports unknown / missing keys

Run from anywhere; resolves the active game from .active-game by default.

Usage:
  python core/tools/balance-tools/validate_balance.py
  python core/tools/balance-tools/validate_balance.py --game brave-bunny
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[validate-balance] no .active-game; pass --game", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def _read_schema_fields(schema_md: Path) -> set[str]:
    """Parse the schema doc and return a set of recognized field names.

    Expected format: a markdown table with a 'field' or 'name' column.
    """
    if not schema_md.exists():
        return set()
    rows = set()
    in_table = False
    fields_col = 0
    for line in schema_md.read_text(encoding="utf-8").splitlines():
        if re.match(r"^\s*\|\s*-+", line):
            in_table = True
            continue
        if not in_table:
            if line.strip().startswith("|"):
                # Header row — find the column index of 'field' or 'name'.
                cells = [c.strip().lower() for c in line.strip().strip("|").split("|")]
                if "field" in cells:
                    fields_col = cells.index("field")
                elif "name" in cells:
                    fields_col = cells.index("name")
            continue
        if not line.strip().startswith("|"):
            in_table = False
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if fields_col < len(cells):
            rows.add(cells[fields_col])
    return rows


def _top_level_keys(obj) -> set[str]:
    if isinstance(obj, dict):
        return set(obj.keys())
    if isinstance(obj, list) and obj and isinstance(obj[0], dict):
        return set(obj[0].keys())
    return set()


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--game")
    args = ap.parse_args()
    g = active_game(args.game)
    bal_dir = repo_root() / "games" / g / "data" / "balance"
    if not bal_dir.is_dir():
        print(f"[validate-balance] no balance dir: {bal_dir}", file=sys.stderr)
        return 1

    errors: list[str] = []
    for jpath in sorted(bal_dir.glob("*.json")):
        try:
            data = json.loads(jpath.read_text(encoding="utf-8"))
        except json.JSONDecodeError as e:
            errors.append(f"{jpath.name}: JSON parse error: {e}")
            continue
        schema = jpath.with_suffix(".schema.md")
        expected = _read_schema_fields(schema)
        if not expected:
            errors.append(f"{jpath.name}: no schema fields parsed from {schema.name} (file missing or no table?)")
            continue
        actual = _top_level_keys(data)
        unknown = actual - expected
        missing = expected - actual
        if unknown:
            errors.append(f"{jpath.name}: unknown fields not in schema: {sorted(unknown)}")
        if missing:
            errors.append(f"{jpath.name}: schema fields not present in JSON: {sorted(missing)}")

    if errors:
        for e in errors:
            print(f"[validate-balance] {e}", file=sys.stderr)
        return 1
    print(f"[validate-balance] OK — game={g}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
