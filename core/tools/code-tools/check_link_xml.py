#!/usr/bin/env python3
"""check_link_xml.py — detect drift between [BraveRegister(...)] declarations
in C# source and <type> preservation entries in unity/Assets/_Brave/link.xml.

Per ADR-0009 (polymorphic mechanics via type-name registry), runtime reflection
finds classes with [BraveRegister] attribute. IL2CPP can strip these in iOS
builds unless they're explicitly preserved in link.xml. This script catches
the case where a developer adds a new [BraveRegister] class but forgets to
update link.xml.

Usage:
  python core/tools/code-tools/check_link_xml.py             # active game
  python core/tools/code-tools/check_link_xml.py --game brave-bunny

Exit codes:
  0 — no drift
  1 — drift detected (missing entries in link.xml)
  2 — fatal (no game specified, missing files)
"""

from __future__ import annotations

import argparse
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


REGISTER_RE = re.compile(r"\[\s*BraveRegister\s*\(\s*\"([^\"]+)\"\s*\)\s*\]")
CLASS_RE = re.compile(r"(?:public|internal|sealed|abstract|partial|\s)*\bclass\s+(\w+)")
NAMESPACE_RE = re.compile(r"^\s*namespace\s+([\w.]+)\s*[;{]")


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[check-link] no .active-game; pass --game", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def find_register_attributes(code_root: Path) -> list[tuple[str, str, str]]:
    """Return [(token, namespace, class_name), ...] for every [BraveRegister(...)] class."""
    out: list[tuple[str, str, str]] = []
    if not code_root.exists():
        return out
    for cs in code_root.rglob("*.cs"):
        try:
            text = cs.read_text(encoding="utf-8")
        except OSError:
            continue
        if "[BraveRegister" not in text:
            continue
        namespace = ""
        lines = text.splitlines()
        # Resolve namespace (first match in file).
        for line in lines:
            m = NAMESPACE_RE.match(line)
            if m:
                namespace = m.group(1)
                break
        # Walk lines looking for [BraveRegister(...)] followed (after the line)
        # by the first class declaration.
        for i, line in enumerate(lines):
            m = REGISTER_RE.search(line)
            if not m:
                continue
            token = m.group(1)
            # Find the next `class Name` declaration within the next 4 lines.
            class_name = ""
            for j in range(i, min(i + 6, len(lines))):
                cm = CLASS_RE.search(lines[j])
                if cm:
                    class_name = cm.group(1)
                    break
            if not class_name:
                # Probably misformatted; still record token for awareness
                class_name = "<unknown>"
            out.append((token, namespace, class_name))
    return out


def find_preserved_types(link_xml: Path) -> set[str]:
    if not link_xml.exists():
        return set()
    try:
        tree = ET.parse(str(link_xml))
    except ET.ParseError as e:
        print(f"[check-link] link.xml parse error: {e}", file=sys.stderr)
        sys.exit(2)
    types: set[str] = set()
    for asm in tree.findall(".//assembly"):
        for t in asm.findall("./type"):
            full = t.get("fullname", "")
            if full:
                types.add(full)
    return types


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--game")
    ap.add_argument("--report-only", action="store_true",
                    help="Just print the table; never exit non-zero")
    args = ap.parse_args()

    g = active_game(args.game)
    root = repo_root()
    code_root = root / "games" / g / "unity" / "Assets" / "_Brave" / "Code"
    link_xml = root / "games" / g / "unity" / "Assets" / "_Brave" / "link.xml"

    if not code_root.exists():
        print(f"[check-link] no Unity code dir at {code_root} (Phase 5 not started?)")
        return 0  # not an error — pre-Phase-5 games legitimately have no code yet

    registered = find_register_attributes(code_root)
    if not registered:
        print(f"[check-link] no [BraveRegister] attributes found in {code_root}")
        return 0  # no mechanics yet — no drift possible

    preserved = find_preserved_types(link_xml)

    missing: list[tuple[str, str, str]] = []
    for token, ns, cls in registered:
        if cls == "<unknown>":
            missing.append((token, ns, cls))
            continue
        fullname = f"{ns}.{cls}" if ns else cls
        if fullname not in preserved:
            missing.append((token, ns, cls))

    print(f"[check-link] game={g}")
    print(f"[check-link] [BraveRegister] classes found: {len(registered)}")
    print(f"[check-link] <type> entries in link.xml: {len(preserved)}")

    if not missing:
        print("[check-link] OK — every [BraveRegister] class is preserved")
        return 0

    print(f"[check-link] DRIFT — {len(missing)} class(es) missing from link.xml:", file=sys.stderr)
    for token, ns, cls in missing:
        full = f"{ns}.{cls}" if ns else cls
        print(f"  [BraveRegister(\"{token}\")] {full}", file=sys.stderr)
    print(file=sys.stderr)
    print("Add the following block(s) to link.xml inside the matching <assembly>:", file=sys.stderr)
    for token, ns, cls in missing:
        full = f"{ns}.{cls}" if ns else cls
        print(f'    <type fullname="{full}" preserve="all"/>', file=sys.stderr)

    return 0 if args.report_only else 1


if __name__ == "__main__":
    sys.exit(main())
