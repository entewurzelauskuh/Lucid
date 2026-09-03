#!/usr/bin/env bash
# Build cubes from their cube.spec.json.
#
#   tools/build-cube.sh <pack>/<cube>   one cube
#   tools/build-cube.sh <pack>          the whole pack, after a template change
#
# Prefabs and CubeDefinitions are generated, never hand-edited (CLAUDE.md
# rule 4). The editor path comes from $UNITY_PATH, or is guessed from the
# version pinned in Lucid/ProjectSettings/ProjectVersion.txt.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/Lucid"

if [[ $# -ne 1 ]]; then
  echo "usage: tools/build-cube.sh <pack>[/<cube>]" >&2
  exit 2
fi
TARGET="$1"

version="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt")"

if [[ -n "${UNITY_PATH:-}" ]]; then
  UNITY="$UNITY_PATH"
else
  for candidate in \
    "/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity" \
    "$HOME/Unity/Hub/Editor/$version/Editor/Unity" \
    "/opt/unity/editors/$version/Editor/Unity"
  do
    [[ -x "$candidate" ]] && UNITY="$candidate" && break
  done
fi

if [[ -z "${UNITY:-}" || ! -x "$UNITY" ]]; then
  echo "error: no Unity $version found. Set UNITY_PATH to the editor binary." >&2
  exit 1
fi

# True when some Unity already has this project open.
#
# Matched as fixed strings, not a pattern: the project path is interpolated by
# the caller, and a clone under a path containing a regex metacharacter — a
# "C++" or a "(new)" — made pgrep fail to compile the pattern and silently
# turn the guard off. Case-insensitive because the editor is launched with
# -projectpath and the Hub may have recorded the folder with another case;
# matching -projectPath exactly is what let #68 through.
unity_holds_project() {
  ps -Ao command= \
    | grep -iF -- "-projectpath" \
    | grep -qiF -- "$PROJECT"
}

# Batch mode cannot open a project the editor already holds; it aborts instead.
if [[ -e "$PROJECT/Temp/UnityLockfile" ]] && unity_holds_project; then
  echo "error: the Unity editor has $PROJECT open. Close it, or build from the editor." >&2
  exit 1
fi

mkdir -p "$ROOT/.test-results"
LOG="$ROOT/.test-results/build-cube.log"

set +e
# No -nographics: the previews are rendered with a camera, and a null
# graphics device silently produces no images. The renderer degrades to
# "no previews" rather than failing, so without this the report would be
# green and the Previews folder empty.
"$UNITY" -batchmode -quit \
  -projectPath "$PROJECT" \
  -executeMethod Lucid.Editor.Cubes.CubeBuildCommand.Run \
  -cubeTarget "$TARGET" \
  -logFile - >"$LOG" 2>&1
code=$?
set -e

# A compile error does not stop Unity running whatever it already had, so a
# build can report success over stale editor code (see #35, #49).
# Unity can abort before running anything — most often because the editor has
# the project open — and does not always exit non-zero when it does. Without
# this the script reports success over a build that never started.
if grep -q "Aborting batchmode due to" "$LOG"; then
  echo "Unity aborted before running anything:" >&2
  grep -A4 "Aborting batchmode due to" "$LOG" >&2
  exit 1
fi

if grep -qE "error CS[0-9]+" "$LOG"; then
  echo "COMPILATION FAILED - the build below would be from stale assemblies" >&2
  grep -oE "[^ ]+\.cs\([0-9]+,[0-9]+\): error CS[0-9]+: .*" "$LOG" | sort -u | head -15 >&2
  exit 1
fi

# Exactly two spaces then content: that is a problem line from Describe().
# Unity indents its own log six, which the looser pattern let through.
grep -E "^(built |unchanged |rebuilt |build-cube: |Assets/[^ ]|  [^ ])" "$LOG" || true

if [[ $code -ne 0 ]]; then
  echo "FAILED (exit $code); full log at $LOG" >&2
  exit $code
fi
echo "OK"
