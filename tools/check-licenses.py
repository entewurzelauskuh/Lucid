#!/usr/bin/env python3
"""Enforce the asset rule (CLAUDE.md rule 5, spec §17, §18).

Only redistributable assets may be committed, and each needs a line in its
cube's assets/LICENSES.md naming a CC0 or CC-BY licence. Anything listed in
a cube's assets.manifest.json is fetched at build time and must never be
committed.

Run with no arguments to check what is staged (this is what the pre-commit
hook does), or pass paths to check them directly.
"""
from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

ASSET_DIR = re.compile(r"(?P<cube>(?:.*/)?Packs/[^/]+/Cubes/[^/]+)/assets/(?P<rel>.+)")
ALLOWED = re.compile(r"\bCC0\b|\bCC-?BY\b", re.IGNORECASE)
# Text that lives with the assets rather than being one.
EXEMPT = {"LICENSES.md"}


def staged_paths() -> list[str]:
    out = subprocess.run(
        ["git", "diff", "--cached", "--name-only", "--diff-filter=ACMR"],
        capture_output=True, text=True, check=True,
    ).stdout
    return [line for line in out.splitlines() if line.strip()]


def ledger_entry(ledger: Path, name: str) -> str | None:
    """The first line of the ledger naming this file, if any."""
    if not ledger.is_file():
        return None
    for line in ledger.read_text(encoding="utf-8", errors="replace").splitlines():
        if name in line:
            return line
    return None


def manifest_names(cube: Path) -> set[str]:
    """Files the manifest says are fetched, so must never be committed."""
    manifest = cube / "assets.manifest.json"
    if not manifest.is_file():
        return set()
    try:
        data = json.loads(manifest.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        print(f"  {manifest}: not valid JSON ({exc})", file=sys.stderr)
        return set()
    entries = data.get("assets", data if isinstance(data, list) else [])
    names = set()
    for entry in entries:
        if isinstance(entry, dict):
            for key in ("file", "path", "name", "dest"):
                if entry.get(key):
                    names.add(Path(str(entry[key])).name)
                    break
    return names


def check(paths: list[str]) -> list[str]:
    problems: list[str] = []
    for path in paths:
        match = ASSET_DIR.match(path)
        if not match:
            continue

        rel = match.group("rel")
        name = Path(rel).name
        if name in EXEMPT:
            continue

        # A .meta rides along with its asset; judge the asset instead.
        subject = name[:-5] if name.endswith(".meta") else name
        if subject in EXEMPT:
            continue

        cube = Path(match.group("cube"))

        if subject in manifest_names(cube):
            problems.append(
                f"{path}: listed in {cube}/assets.manifest.json, so it is fetched "
                f"and must not be committed (CLAUDE.md rule 5)"
            )
            continue

        entry = ledger_entry(cube / "assets" / "LICENSES.md", subject)
        if entry is None:
            problems.append(
                f"{path}: no entry for {subject!r} in {cube}/assets/LICENSES.md. "
                f"Add source URL and licence, or move it to assets.manifest.json"
            )
        elif not ALLOWED.search(entry):
            problems.append(
                f"{path}: ledger entry for {subject!r} is not CC0 or CC-BY, so it "
                f"cannot be redistributed here -> {entry.strip()}"
            )
    return problems


def main() -> int:
    paths = sys.argv[1:] or staged_paths()
    problems = check(paths)
    if problems:
        print("Asset rule violations (CLAUDE.md rule 5):\n", file=sys.stderr)
        for problem in problems:
            print(f"  - {problem}", file=sys.stderr)
        print(
            "\nRedistributable assets (CC0 / CC-BY) are committed with a ledger "
            "line.\nEverything else belongs in assets.manifest.json and is fetched "
            "by tools/fetch-assets.py.",
            file=sys.stderr,
        )
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
