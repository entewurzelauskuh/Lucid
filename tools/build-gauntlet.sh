#!/usr/bin/env bash
# Write the movement gauntlet scene from GauntletLayout.
#
#   tools/build-gauntlet.sh
#
# The scene is generated, never hand-edited (CLAUDE.md rule 4). Its geometry
# comes from Lucid.Runtime.Dev.GauntletBuilder, which is also what the PlayMode
# tests run against, so the course a human walks is the course they measure.
#
# The editor path comes from $UNITY_PATH, or is guessed from the version
# pinned in Lucid/ProjectSettings/ProjectVersion.txt.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/Lucid"

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

if [[ -e "$PROJECT/Temp/UnityLockfile" ]] && pgrep -f "Unity.*-projectPath.*$PROJECT" >/dev/null 2>&1; then
  echo "error: the Unity editor has $PROJECT open. Close it, or use Lucid > Build Gauntlet Scene." >&2
  exit 1
fi

mkdir -p "$ROOT/.test-results"
LOG="$ROOT/.test-results/build-gauntlet.log"

set +e
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$PROJECT" \
  -executeMethod Lucid.Editor.Scenes.GauntletSceneBuilder.BuildFromCommandLine \
  -logFile - >"$LOG" 2>&1
code=$?
set -e

# A compile error does not stop Unity running whatever it already had, so a
# build can report success over stale editor code (see #35, #49).
if grep -qE "error CS[0-9]+" "$LOG"; then
  echo "COMPILATION FAILED - the build below would be from stale assemblies" >&2
  grep -oE "[^ ]+\.cs\([0-9]+,[0-9]+\): error CS[0-9]+: .*" "$LOG" | sort -u | head -15 >&2
  exit 1
fi

grep -E "^gauntlet: " "$LOG" || true

if [[ $code -ne 0 ]]; then
  echo "FAILED (exit $code); full log at $LOG" >&2
  exit $code
fi
echo "OK"
