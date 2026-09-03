#!/usr/bin/env bash
# Rebuild every generated file and fail if any of them changed.
#
#   tools/verify-generated.sh
#
# What this proves, exactly:
#
#   1. every generator runs to completion on this machine, and
#   2. each generator, asked to produce its artefact again, decides the
#      committed one is already current.
#
# The second is narrower than "the committed file is correct", and the
# difference matters. Neither generator overwrites: CubeBuilder writes only
# when CubeEquivalence says the cube differs, and GeneratedScene only when
# SceneSignature says the scene does. So this check is exactly as strong as
# those two comparators, and both have known blind spots — CubeEquivalence
# compares a hand-written list of fields (#66), SceneSignature sees only root
# GameObjects (#67) and only inspector-visible properties (#72). A hand edit
# to something none of them compares survives this check.
#
# Demonstrated rather than assumed: setting m_Fog in Gauntlet.unity by hand
# passes with "OK". Setting m_Text does not. The gap is the comparators', and
# closing it is what #66, #67 and #72 are for.
#
# The project has no CI (docs/SPEC.md §18), so without this nothing at all
# would notice a generator that had stopped running, or an artefact left
# behind by a generator that changed.
#
# Needs the Unity editor closed, like every script it calls, and a graphics
# device: build-cube.sh deliberately omits -nographics because previews are
# rendered with a camera, so this cannot run on a headless box (#70).
#
# Exit codes: 0 nothing changed; 1 something drifted; 2 the check could not run.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

CANNOT_CHECK=2
DRIFTED=1

status() {
  local out
  if ! out="$(git status --porcelain 2>&1)"; then
    echo "error: git status failed: $out" >&2
    exit $CANNOT_CHECK
  fi
  printf '%s' "$out"
}

# A dirty tree would be reported as drift, which is both wrong and alarming.
# Only committing helps: stashing would hide the very generator edit you are
# trying to verify and quietly check the committed one instead.
before="$(status)"
if [[ -n "$before" ]]; then
  echo "error: the working tree has changes, so drift could not be told from your work." >&2
  echo "       commit them first — do not stash, or this would verify the committed" >&2
  echo "       generator rather than the one you are working on." >&2
  git status --short >&2
  exit $CANNOT_CHECK
fi

# A generator that fails part way leaves rebuilt files behind, and the next run
# would refuse without saying why.
trap 'left="$(git status --porcelain 2>/dev/null || true)"
      if [[ -n "$left" ]]; then
        echo >&2
        echo "note: the run left the tree modified:" >&2
        git status --short >&2
      fi' ERR

packs=()
for dir in "$ROOT"/Lucid/Assets/_Lucid/Packs/*/; do
  [[ -d "$dir" ]] || continue
  # A pack may exist for its skins or mobs before it has any cubes; build-cube.sh
  # treats that as an error, which would stop the whole verification.
  if ! compgen -G "$dir/Cubes/*/cube.spec.json" >/dev/null; then
    echo "verify-generated: skipping $(basename "$dir") — no cube specs yet"
    continue
  fi
  packs+=("$(basename "$dir")")
done

echo "verify-generated: rebuilding ${#packs[@]} pack(s) and every generated scene"

# Each generator prints its own diagnosis; set -e stops us on the first that
# fails, with its output already on the terminal.
for pack in "${packs[@]:-}"; do
  [[ -n "$pack" ]] || continue
  echo "--- cubes: $pack"
  tools/build-cube.sh "$pack"
done

echo "--- scenes"
tools/build-scenes.sh

trap - ERR

drift="$(status)"
if [[ -n "$drift" ]]; then
  echo >&2
  echo "FAILED: generated files differ from what their generators produce." >&2
  echo >&2
  git status --short >&2
  echo >&2
  git --no-pager diff --stat >&2
  echo >&2
  echo "The rebuilt files are left in place so they can be inspected." >&2
  echo "Commit them if the generator is right. To discard, note that a rebuild can" >&2
  echo "add files as well as change them, so 'git checkout -- .' alone is not enough:" >&2
  echo "  git checkout -- . && git clean -fd Lucid/Assets/_Lucid" >&2
  exit $DRIFTED
fi

echo "OK: every generator ran, and each says its committed artefact is current"
