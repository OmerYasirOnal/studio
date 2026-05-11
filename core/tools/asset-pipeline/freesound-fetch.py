#!/usr/bin/env python3
"""freesound-fetch.py — guided downloader for freesound.org (CC0 filter only).

Freesound has an OAuth API but most CC0 SFX can be downloaded directly via the
sound-page URL with no authentication. This script requires a direct download URL
(the "Download File" link on a sound's page) and validates the license string in
its accompanying metadata file.

Usage:
  python core/tools/asset-pipeline/freesound-fetch.py \
      --url https://freesound.org/data/previews/.../file.wav \
      --license-page-url https://freesound.org/people/.../sounds/123456/ \
      --target-subdir audio/sfx/melee \
      --notes "wet thump 80ms, weapon hit"

Important: --license-page-url is the **sound's page URL**, which we record as the
attribution URL. We trust the user to have manually confirmed the page shows
"License: Creative Commons 0" before passing the args.
"""

from __future__ import annotations

import argparse
import hashlib
import sys
import urllib.request
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse

ALLOWED_HOSTS = {"freesound.org", "www.freesound.org"}


def repo_root() -> Path:
    return Path(__file__).resolve().parent.parent.parent.parent


def active_game(specified: str | None) -> str:
    if specified:
        return specified
    f = repo_root() / ".active-game"
    if not f.exists():
        print("[freesound] no .active-game; pass --game", file=sys.stderr)
        sys.exit(2)
    return f.read_text(encoding="utf-8").strip()


def fetch(url: str, dst: Path) -> str:
    print(f"[freesound] downloading {url} -> {dst}")
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


def append_license_row(licenses_md: Path, rel_path: str, url: str, author: str, notes: str) -> None:
    fetched = datetime.now().strftime("%Y-%m-%d")
    row = f"| {rel_path} | Freesound | CC0 1.0 | {url} | {author} | {fetched} |  <!-- {notes} -->\n"
    text = licenses_md.read_text(encoding="utf-8") if licenses_md.exists() else ""
    if "_none yet_" in text:
        text = text.replace("| _none yet_ | | | | | |\n", row)
    else:
        text = text + ("\n" if text and not text.endswith("\n") else "") + row
    licenses_md.write_text(text, encoding="utf-8")
    print(f"[freesound] LICENSES.md updated: + {rel_path}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--url", required=True, help="Direct download URL")
    ap.add_argument("--license-page-url", required=True, help="Sound page URL (for attribution)")
    ap.add_argument("--target-subdir", required=True)
    ap.add_argument("--author", default="Freesound contributor", help="Display name of the uploader")
    ap.add_argument("--notes", default="")
    ap.add_argument("--game")
    args = ap.parse_args()

    if urlparse(args.license_page_url).hostname not in ALLOWED_HOSTS:
        print("[freesound] license-page-url must be on freesound.org", file=sys.stderr)
        return 1

    game = active_game(args.game)
    base = repo_root() / "games" / game / "assets-raw"
    sub = base / args.target_subdir
    filename = Path(urlparse(args.url).path).name or "audio.wav"
    dst = sub / filename

    if dst.exists():
        print(f"[freesound] skip — already present: {dst}")
        return 0

    sha = fetch(args.url, dst)
    print(f"[freesound] sha256={sha}")
    append_license_row(base / "LICENSES.md", str(dst.relative_to(base)), args.license_page_url, args.author, args.notes)
    return 0


if __name__ == "__main__":
    sys.exit(main())
