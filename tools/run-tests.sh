#!/usr/bin/env bash
# Run Unity tests headlessly and print a summary line for the pull request.
#
#   tools/run-tests.sh                    EditMode + PlayMode
#   tools/run-tests.sh editmode           EditMode only, the fastest loop
#   tools/run-tests.sh editmode <filter>  one fixture or test by name
#   tools/run-tests.sh playmode [filter]
#
# Only Lucid.Tests.* assemblies run; set LUCID_ALL_ASSEMBLIES=1 to include
# tests that ship inside packages.
#
# The editor binary comes from $UNITY_PATH, or is guessed from the version
# pinned in Lucid/ProjectSettings/ProjectVersion.txt.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/Lucid"
RESULTS="$ROOT/.test-results"

MODE="${1:-all}"
FILTER="${2:-}"

case "$MODE" in
  all|editmode|playmode) ;;
  *) echo "usage: tools/run-tests.sh [all|editmode|playmode] [testFilter]" >&2; exit 2 ;;
esac

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

# Batch mode cannot open a project the editor already holds.
if [[ -e "$PROJECT/Temp/UnityLockfile" ]] && pgrep -f "Unity.*-projectPath.*$PROJECT" >/dev/null 2>&1; then
  echo "error: the Unity editor has $PROJECT open. Close it, or run the tests from the editor." >&2
  exit 1
fi

mkdir -p "$RESULTS"

run_platform() {
  local platform="$1" out="$RESULTS/$1.xml"
  local args=(-batchmode -nographics -projectPath "$PROJECT"
              -runTests -testPlatform "$platform" -testResults "$out"
              -logFile - )
  # Scope to Lucid's own assemblies. Without this the run also collects test
  # stubs that ship inside packages (Addressables contributes one), which is
  # noise in the summary line and grows with every package added.
  # LUCID_ALL_ASSEMBLIES=1 runs everything, for debugging a package.
  [[ -z "${LUCID_ALL_ASSEMBLIES:-}" ]] && args+=(-assemblyNames "Lucid.Tests.$platform")
  # -quit must NOT be passed with -runTests: it ends the editor before results
  # are written.
  [[ -n "$FILTER" ]] && args+=(-testFilter "$FILTER")

  set +e
  "$UNITY" "${args[@]}" >"$RESULTS/$platform.log" 2>&1
  local code=$?
  set -e

  if [[ ! -f "$out" ]]; then
    echo "$platform: no results written (exit $code). Tail of the log:" >&2
    tail -20 "$RESULTS/$platform.log" >&2
    return 1
  fi

  python3 - "$out" "$platform" <<'PY'
import sys, xml.etree.ElementTree as ET
path, platform = sys.argv[1], sys.argv[2]
r = ET.parse(path).getroot()
g = lambda k, d="0": r.get(k, d)
total, passed = int(g("total")), int(g("passed"))
failed, skipped = int(g("failed")), int(g("skipped"))
dur = float(g("duration", "0"))
print(f"{platform}: {passed}/{total} passed, {failed} failed, {skipped} skipped ({dur:.1f}s)")
sys.exit(1 if failed else 0)
PY
}

status=0
[[ "$MODE" == "all" || "$MODE" == "editmode" ]] && { run_platform EditMode || status=1; }
[[ "$MODE" == "all" || "$MODE" == "playmode" ]] && { run_platform PlayMode || status=1; }

[[ $status -eq 0 ]] && echo "OK" || echo "FAILED"
exit $status
