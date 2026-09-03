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
  echo "error: the Unity editor has $PROJECT open. Close it, or run the tests from the editor." >&2
  exit 1
fi

mkdir -p "$RESULTS"

run_platform() {
  local platform="$1" out="$RESULTS/$1.xml"
  # A results file left by an earlier run is indistinguishable from one this
  # run wrote, and Unity leaves the old one in place when it aborts before
  # starting. That reported "OK, 282/282" over a run that never began.
  #
  # Checked, because an unchecked rm reopens exactly that hole: if the results
  # directory is not writable, the delete fails, the log redirect fails, and
  # every grep below reads as "no match" — leaving the stale file to be parsed.
  if ! rm -f "$out"; then
    echo "$platform: cannot remove $out; results would be from an earlier run" >&2
    return 1
  fi
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

  # Every check below reads the log, and grep exits 2 — which an `if` reads as
  # "no match" — when the file is not there. So the log's existence is the
  # precondition for trusting any of them.
  if [[ ! -s "$RESULTS/$platform.log" ]]; then
    echo "$platform: no log written (exit $code); the run did not start" >&2
    return 1
  fi

  # A compile error does not stop Unity from running whatever assemblies it
  # already had, so the run can report a cheerful green over a broken build.
  # Rule 6 makes this summary line the only evidence a reviewer gets, so treat
  # any compiler error as a failure of the run.
  if grep -qE "error CS[0-9]+" "$RESULTS/$platform.log"; then
    echo "$platform: COMPILATION FAILED - results below would be from stale assemblies" >&2
    grep -oE "[^ ]+\.cs\([0-9]+,[0-9]+\): error CS[0-9]+: .*" "$RESULTS/$platform.log" \
      | sort -u | head -15 >&2
    return 1
  fi

  # Unity exits 0 in some abort paths, so the log is the tell.
  if grep -q "Aborting batchmode due to" "$RESULTS/$platform.log"; then
    echo "$platform: Unity aborted before running anything:" >&2
    grep -A4 "Aborting batchmode due to" "$RESULTS/$platform.log" >&2
    return 1
  fi

  if [[ ! -f "$out" ]]; then
    echo "$platform: no results written (exit $code). Tail of the log:" >&2
    tail -20 "$RESULTS/$platform.log" >&2
    return 1
  fi

  # Unity's exit code is deliberately not a verdict here: -runTests exits
  # non-zero for ordinary test failures, so failing on it would replace the
  # "N failed" summary with a vaguer message. The results file is the
  # authority, and the checks above establish that it belongs to this run.

    python3 - "$out" "$platform" <<'PY'
import sys, xml.etree.ElementTree as ET
path, platform = sys.argv[1], sys.argv[2]
r = ET.parse(path).getroot()
g = lambda k, d="0": r.get(k, d)
total, passed = int(g("total")), int(g("passed"))
failed, skipped = int(g("failed")), int(g("skipped"))
inconclusive, state = int(g("inconclusive")), r.get("result", "")
dur = float(g("duration", "0"))
print(f"{platform}: {passed}/{total} passed, {failed} failed, {skipped} skipped ({dur:.1f}s)")

# A run that collected nothing is the other way of never starting, and it used
# to print this same line and exit 0. CLAUDE.md's Status names that confusion
# by hand; the runner should not need it named.
problems = []
if failed: problems.append(f"{failed} failed")
if total == 0: problems.append("no tests ran at all")
if inconclusive: problems.append(f"{inconclusive} inconclusive")
if state and state != "Passed": problems.append(f"NUnit reported result={state}")
if problems:
    print(f"{platform}: " + "; ".join(problems), file=sys.stderr)
    sys.exit(1)
PY
}

status=0
[[ "$MODE" == "all" || "$MODE" == "editmode" ]] && { run_platform EditMode || status=1; }
[[ "$MODE" == "all" || "$MODE" == "playmode" ]] && { run_platform PlayMode || status=1; }

[[ $status -eq 0 ]] && echo "OK" || echo "FAILED"
exit $status
