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
# Rule 5 admits CC0 and a *bare* attribution CC-BY, and nothing else. Two
# patterns rather than one clever one, because the clause forms are the part
# that keeps going wrong:
#
#   ALLOWED   the licence has to be one of the two families at all
#   DENIED    ...and must carry none of the extra clauses
#
# Both are matched against the licence *cell* of the ledger row, never the whole
# line. Matching the line let anything through that merely mentioned a licence
# somewhere else on it: an asset named cc0-hero.png, or a source URL on
# cc0-textures.com — which is ambientCG, a source docs/SPEC.md §17 recommends —
# opened the gate for an Asset Store EULA.
#
# DENIED matches a clause only as a whole token, and repeated. A trailing \b
# let CC-BY-NC4.0 and CC-BY-NC_4.0 past, because a digit and an underscore are
# word characters and the boundary never fired; forbidding any following letter
# instead keeps "CC BY 4.0 - SAmple pack" and "CC0 - NDA cleared" accepted,
# where SA and ND are the first letters of an ordinary word. The + is for
# CC-BY-NCSA, where one clause abuts the next.
#
# Neither pattern uses \s, and the cell is trimmed of an explicit set. Python
# and .NET disagree about \s: Python's matches U+001C-001F, .NET's does not, so
# "CC-BY\x1cNC 4.0" was rejected here and accepted by the validator — a cube
# building clean and failing at commit, which is the one thing sharing the
# pattern is meant to prevent. str.strip() and String.Trim() differ over the
# same characters.
#
# Kept character-for-character identical to CubeValidator.AllowedLicence and
# CubeValidator.DeniedLicence; LicenceRuleTests runs this script and compares
# its verdicts against the validator's, which is the only check that proves the
# two agree rather than merely look alike.
ALLOWED_PATTERN = r"\bCC0\b|\bCC[- ]?BY\b"
DENIED_PATTERN = (
    r"(?:^|[^A-Za-z0-9])(?:NC|ND|SA)+(?![A-Za-z])"
    r"|Non[^A-Za-z0-9]*Commercial|No[^A-Za-z0-9]*Deriv|Share[^A-Za-z0-9]*Alike"
)
ALLOWED = re.compile(ALLOWED_PATTERN, re.IGNORECASE)
DENIED = re.compile(DENIED_PATTERN, re.IGNORECASE)
# Text that lives with the assets rather than being one.
EXEMPT = {"LICENSES.md"}


def licence_cell(entry: str) -> str | None:
    """The licence column of a ledger row, or None if the line is not a row.

    docs/SPEC.md §18 fixes the shape: | file | source | licence |. A line
    that is not a table row cannot be read for a licence, and guessing from the
    whole line is what let a URL decide the verdict.
    """
    # By position, not "the last non-empty cell". Taking the last one read a
    # fourth column when a ledger had one — so a note saying "CC0 base" beside
    # a CC-BY-NC licence opened the gate — and dropping empty cells refused a
    # row whose source column was blank.
    cells = entry.split("|")
    return cells[3].strip(" \t") if len(cells) == 5 else None


def is_redistributable(entry: str) -> bool:
    """Whether a ledger row names a licence rule 5 admits."""
    cell = licence_cell(entry)
    if cell is None:
        return False
    return bool(ALLOWED.search(cell)) and not DENIED.search(cell)


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
    # split("\n"), not splitlines(). Python's splitlines() also breaks on
    # U+001C-001E, U+0085, U+2028 and U+2029, and C# String.Split('\n') does
    # not — so a ledger line containing one of those was two lines here and one
    # line to the validator. The row then looked malformed to the hook and fine
    # to the build: a cube building clean and failing at commit, which is the
    # one thing sharing the rule is meant to prevent.
    for line in ledger.read_text(encoding="utf-8", errors="replace").split("\n"):
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
    entries = data if isinstance(data, list) else (
        data.get("assets", []) if isinstance(data, dict) else []
    )
    if not isinstance(entries, list):
        entries = []
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

        # A .meta is Unity's own import settings, not third-party content, and
        # the asset it rides with is judged on its own account. CubeValidator
        # skips them too: judging the subject here made a stray wall.png.meta
        # with no wall.png block the commit and pass the build.
        if name.endswith(".meta"):
            continue
        subject = name
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
        elif licence_cell(entry) is None:
            problems.append(
                f"{path}: the ledger line for {subject!r} is not a "
                f"| file | source | licence | row, so it names no licence "
                f"-> {entry.strip()}"
            )
        elif not is_redistributable(entry):
            problems.append(
                f"{path}: {subject!r} is not CC0 or a bare CC-BY. NonCommercial, "
                f"NoDerivatives and ShareAlike are not accepted (CLAUDE.md rule 5, "
                f"docs/SPEC.md §18) -> {licence_cell(entry)}"
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
