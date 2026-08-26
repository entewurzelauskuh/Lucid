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


def repo_root() -> Path | None:
    try:
        out = subprocess.run(
            ["git", "rev-parse", "--show-toplevel"],
            capture_output=True, text=True, check=True,
        ).stdout.strip()
        return Path(out) if out else None
    except (subprocess.CalledProcessError, FileNotFoundError):
        return None


def staged_paths() -> list[str]:
    out = subprocess.run(
        ["git", "diff", "--cached", "--name-only", "--diff-filter=ACMR"],
        capture_output=True, text=True, check=True,
    ).stdout
    return [line for line in out.splitlines() if line.strip()]


def absolute(path: str, base: Path | None) -> Path:
    """Anchor a path so sibling lookups do not depend on the caller's cwd.

    git reports staged paths relative to the repo root, which is only the
    working directory by luck; arguments on the command line are relative to
    wherever the user is standing.
    """
    p = Path(path)
    if p.is_absolute():
        return p
    return (base / p) if base is not None else (Path.cwd() / p).resolve()


def ledger_entry(ledger: Path, name: str) -> str | None:
    """The first line of the ledger naming this file, if any.

    The filename must appear as a whole token. A substring test would let
    an unlisted wall.png match a ledger line for stonewall.png and sail
    through the gate.
    """
    if not ledger.is_file():
        return None
    token = re.compile(rf"(?<![\w.\-]){re.escape(name)}(?![\w.\-])")
    for line in ledger.read_text(encoding="utf-8", errors="replace").splitlines():
        if token.search(line):
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


def check(paths: list[str], base: Path | None = None) -> list[str]:
    problems: list[str] = []
    for path in paths:
        resolved = absolute(path, base)
        match = ASSET_DIR.match(resolved.as_posix())
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
    root = repo_root()
    if sys.argv[1:]:
        # Command-line paths are relative to the caller, not the repo root.
        paths, base = sys.argv[1:], None
    else:
        paths, base = staged_paths(), root
    problems = check(paths, base)
    if problems:
        print("Asset rule violations (CLAUDE.md rule 5):\n", file=sys.stderr)
        for problem in problems:
            print(f"  - {problem}", file=sys.stderr)
        print(
            "\nRedistributable assets (CC0 / CC-BY) are committed with a ledger "
            "line.\nEverything else belongs in assets.manifest.json and is fetched "
            "at build time.\nThe fetcher itself is not written yet (CLAUDE.md "
            "Status), so an asset that\ncannot be committed cannot be used yet "
            "either.",
            file=sys.stderr,
        )
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
