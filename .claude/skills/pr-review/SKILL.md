---
name: pr-review
description: Run the independent review that Lucid requires before any pull request is opened. Two reviewers with different lenses, reproduce-before-fix, mutation-check every test touched, then triage in public. Use before opening a PR, or when asked to review a branch, a diff or an open PR in this repository.
---

# Independent review before a pull request

Lucid has no CI (`docs/SPEC.md` §18), so a pull request's claims rest entirely
on what the author checked. This protocol exists because reading the diff has
repeatedly missed the defects that mattered — see `docs/DECISIONS.md` for the
record that motivated it.

Run this **before** opening the pull request. Reviewing your own work after
announcing it is how a wrong claim reaches the owner.

## 1. Dispatch two reviewers, in parallel

One message, two `Agent` calls, so they run concurrently and neither sees the
other's conclusions. Independence is the point: on #36 both found the same
critical defect separately, which is what made it credible without argument.

Give each the branch, the diff command, and the documents that are normative
for it. Never ask for a verdict on "quality" — ask for defects with evidence.

**Reviewer A — adversarial correctness.** Hunt logic errors, violated
invariants, ways the safety properties can be broken, null and boundary
behaviour, and *tests that pass for the wrong reason*. Require a concrete
failure scenario for each finding: exact inputs, expected versus actual.
Require it to separate PROVEN from SUSPECTED.

**Reviewer B — specification conformance.** Audit against the normative
documents only, not personal taste. For `Lucid.Core` that is
`docs/CORE-API.md` — its §12 lists required tests by group, so demand an
explicit COVERED / MISSING checklist naming the test for each bullet, and
treat a test whose *body* checks something else as MISSING. Also check §11
invariants, signature fidelity, scope creep from later issues, and
`CLAUDE.md` conventions.

Tell both: **do not modify any file**, and say plainly when a category is
clean rather than manufacturing findings.

## 2. Triage every finding yourself

Reviewers are wrong sometimes. A Copilot review of #30 reported a committed
credential that was a Unity template placeholder present in every Unity
project; a review of #36 diagnosed a missing null guard as test-order
nondeterminism when the tests were order-independent.

For each finding: **reproduce it before you fix it.** Write the failing test,
watch it fail, then fix. If you cannot reproduce it, say so and explain why
rather than fixing something speculatively.

Where a claim is empirically checkable, check it — compare the suspect value
against another project, print the actual bytes, run the scenario. Do not
reason your way to a verdict you could measure.

## 3. Mutation-check every test you fixed or added

Break the thing the test covers, run the suite, and confirm *that* test fails.
Restore, confirm green.

This is not optional. It has caught four tests across #35 and #36 that
asserted nothing while appearing to be coverage:

- a test that overwrote a Straight with a Corner and passed on an unrelated
  invariant throw rather than the guard it named;
- a "wall meets wall" test whose candidate cube sat *diagonally* from the
  only other cube, so no wall ever met a wall;
- an "attached after explore" test that read derived state, which is computed
  from the neighbour existing and so held regardless of the bug;
- a duplicate that passed the helper's own default value.

A test that cannot fail is worse than no test: it is a false claim of
coverage in a repository where the test summary is the only evidence.

## 4. Post the triage as a pull request comment

State which findings were real, which were false positives **and why**, and
what changed. When you disagree with a reviewer, say so and show the
reasoning — a wrong diagnosis left standing gets re-raised later.

Correct any claim in the pull request body that the review disproved. On #36
the description claimed "wall-to-wall accepted" was covered; it was not.

## 5. File what is out of scope

Findings in adjacent or already-merged code become issues with the repro,
not a wider pull request. Rule 1 is one issue, one branch, one pull request.
Design gaps where the code follows the spec faithfully get labelled
`question` — that is the owner's call, not yours.

## Then

Paste the `tools/run-tests.sh` summary into the pull request, per rule 6, and
open it. Stop there: the owner reviews and merges.
