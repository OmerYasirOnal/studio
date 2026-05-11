#!/usr/bin/env python3
"""kenney-fetch.py — guided downloader for Kenney.nl (CC0) packs.

Same pattern as quaternius-fetch.py but enforces kenney.nl host.

Usage:
  python core/tools/asset-pipeline/kenney-fetch.py \
      --url https://kenney.nl/media/pages/.../nature-kit.zip \
      --target-subdir 3d/environment/meadow \
      --notes "Nature Kit — trees, rocks, mushrooms"
"""

from __future__ import annotations

import argparse
import hashlib
import sys
import urllib.request
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse

ALLOWED_HOSTS = {"kenney.nl", "www.kenney.nl"}


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[kenney] no .active-game; pass --game", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def fetch(url: str, dst: Path) -> str:
    print(f"[kenney] downloading {url} -> {dst}")
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
    row = f"| {rel_path} | Kenney | CC0 1.0 | {url} | Kenney (Asbjørn Thirslund) | {fetched} |  <!-- {notes} -->\n"
    text = licenses_md.read_text(encoding="utf-8") if licenses_md.exists() else ""
    if "_none yet_" in text:
        text = text.replace("| _none yet_ | | | | | |\n", row)
    else:
        text = text + ("\n" if text and not text.endswith("\n") else "") + row
    licenses_md.write_text(text, encoding="utf-8")
    print(f"[kenney] LICENSES.md updated: + {rel_path}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--url", required=True)
    ap.add_argument("--target-subdir", required=True)
    ap.add_argument("--notes", default="")
    ap.add_argument("--game")
    args = ap.parse_args()

    parsed = urlparse(args.url)
    if parsed.hostname not in ALLOWED_HOSTS:
        print(f"[kenney] host {parsed.hostname} is not in ALLOWED_HOSTS — refusing", file=sys.stderr)
        return 1

    game = active_game(args.game)
    base = repo_root() / "games" / game / "assets-raw"
    sub = base / args.target_subdir
    filename = Path(parsed.path).name or "download.zip"
    dst = sub / filename

    if dst.exists():
        print(f"[kenney] skip — already present: {dst}")
        return 0

    sha = fetch(args.url, dst)
    print(f"[kenney] sha256={sha}")
    append_license_row(base / "LICENSES.md", str(dst.relative_to(base)), args.url, args.notes)
    return 0


if __name__ == "__main__":
    sys.exit(main())
