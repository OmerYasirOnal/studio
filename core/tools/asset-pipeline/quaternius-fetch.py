#!/usr/bin/env python3
"""quaternius-fetch.py — guided downloader for Quaternius (CC0) asset packs.

Quaternius.com hosts ZIPs of CC0 3D-model packs (rigged + animated). Because the
site uses Patreon-style download links that may redirect, this script does NOT
hard-code URLs — instead it accepts a URL on stdin (or via --url) and verifies
the host before downloading.

Usage:
  python core/tools/asset-pipeline/quaternius-fetch.py \
      --url https://quaternius.com/some-pack.zip \
      --target-subdir 3d/characters/animals \
      --notes "Ultimate Animated Animals — base mesh for bunny"

After download, the script appends a row to LICENSES.md (CC0 1.0). It does
NOT extract the zip — that's a manual step so you can pick which files matter.
"""

from __future__ import annotations

import argparse
import hashlib
import sys
import urllib.request
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse

ALLOWED_HOSTS = {"quaternius.com", "www.quaternius.com", "cdn.quaternius.com"}


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[quaternius] no .active-game; pass --game", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def fetch(url: str, dst: Path) -> str:
    print(f"[quaternius] downloading {url} -> {dst}")
    dst.parent.mkdir(parents=True, exist_ok=True)
    req = urllib.request.Request(url, headers={"User-Agent": "studio-asset-curator/0.1"})
    sha = hashlib.sha256()
    with urllib.request.urlopen(req, timeout=60) as r, dst.open("wb") as out:
        while True:
            chunk = r.read(1 << 16)
            if not chunk:
                break
            sha.update(chunk)
            out.write(chunk)
    return sha.hexdigest()


def append_license_row(licenses_md: Path, rel_path: str, url: str, notes: str) -> None:
    fetched = datetime.now().strftime("%Y-%m-%d")
    row = f"| {rel_path} | Quaternius | CC0 1.0 | {url} | Quaternius (Patrick Adams) | {fetched} |  <!-- {notes} -->\n"
    if not licenses_md.exists():
        print(f"[quaternius] LICENSES.md missing at {licenses_md}", file=sys.stderr)
        sys.exit(3)
    text = licenses_md.read_text(encoding="utf-8")
    # Insert before the "_none yet_" placeholder, else append before the trailing sections.
    if "_none yet_" in text:
        text = text.replace("| _none yet_ | | | | | |\n", row)
    else:
        # Append just after the header table separator
        text = text + ("\n" if not text.endswith("\n") else "") + row
    licenses_md.write_text(text, encoding="utf-8")
    print(f"[quaternius] LICENSES.md updated: + {rel_path}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--url", required=True)
    ap.add_argument("--target-subdir", required=True, help="Relative path inside assets-raw/")
    ap.add_argument("--notes", default="", help="Short note recorded in LICENSES.md")
    ap.add_argument("--game")
    args = ap.parse_args()

    parsed = urlparse(args.url)
    if parsed.hostname not in ALLOWED_HOSTS:
        print(f"[quaternius] host {parsed.hostname} is not in ALLOWED_HOSTS — refusing", file=sys.stderr)
        return 1

    game = active_game(args.game)
    base = repo_root() / "games" / game / "assets-raw"
    sub = base / args.target_subdir
    filename = Path(parsed.path).name or "download.zip"
    dst = sub / filename

    if dst.exists():
        print(f"[quaternius] skip — already present: {dst}")
        return 0

    sha = fetch(args.url, dst)
    print(f"[quaternius] sha256={sha}")
    append_license_row(base / "LICENSES.md", str(dst.relative_to(base)), args.url, args.notes)
    return 0


if __name__ == "__main__":
    sys.exit(main())
