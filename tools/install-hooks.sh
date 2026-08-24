#!/usr/bin/env bash
# Install Lucid's git hooks. Run once per clone.
#
# Shims .git/hooks/pre-commit rather than setting core.hooksPath, because
# core.hooksPath would take .git/hooks out of the picture entirely and
# git-lfs keeps its post-checkout, post-commit, post-merge and pre-push
# hooks there.
set -euo pipefail
root="$(git rev-parse --show-toplevel)"
target="$root/.git/hooks/pre-commit"

cat > "$target" <<'SHIM'
#!/usr/bin/env bash
# Skip quietly when the versioned hook is absent -- e.g. after checking out a
# branch that predates it -- so a missing file can never block a commit.
hook="$(git rev-parse --show-toplevel)/tools/hooks/pre-commit"
[ -x "$hook" ] || exit 0
exec "$hook" "$@"
SHIM
chmod +x "$target"
echo "installed $target -> tools/hooks/pre-commit"
