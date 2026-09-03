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

**Reviewer B — the lens that fits the change.** Two reviewers only pay off if
their lenses genuinely differ; a second one covering the same ground is an echo,
not a check.

Pick by **where the change's new risk sits**, not by whether a spec surface
exists. "It touches the spec, so use the spec lens" is the wrong test, and this
repository paid for it: #60 touched `docs/SPEC.md` §9 *and* the test harness,
the spec lens was the obvious pick, and the operational half went unreviewed —
**#64, #65, #68 and #69 are all downstream of that one omission.**

When a change carries both kinds of risk, give Reviewer B the half Reviewer A
is least likely to reach, and **say in the triage which lens B took and which
half nobody covered.** A wrong choice is then visible instead of silent.

*Specification conformance* — where the change makes a normative claim: new
behaviour under a spec section, a document marked **[S]**, a rule in
`CLAUDE.md`. It caught a deviation owing a DECISIONS
entry on #71 and seven wrong document citations. Audit against the normative
documents only, not personal taste. For `Lucid.Core` that is
`docs/CORE-API.md` — its §12 lists required tests by group, so demand an
explicit COVERED / MISSING checklist naming the test for each bullet, and
treat a test whose *body* checks something else as MISSING. Also check §11
invariants, signature fidelity, scope creep from later issues, and
`CLAUDE.md` conventions.

*Operational* — where the change makes an environmental claim: a script, a
generator, the harness, anything that has to run on another machine, on a fresh
clone, with LFS unsmudged, on a headless box, with the editor open, or fail
cleanly half way. That is where the remaining risk in `tools/` lives, and it is
the lens that found a rule-7 breach in the process rules themselves — a change
having a spec surface does not mean the spec lens is the one that will find the
defect.

Keep them **parallel**, and never let the second read the first. Independent
convergence is the strongest signal available — when both land on one defect
separately it settles the matter without argument — and it is worth nothing if
one has seen the other. Anchoring is the other half: the most valuable findings
in this project have come from one reviewer only, because neither was primed by
the other's framing.

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

### Never trust a green run you did not prove compiled

`tools/run-tests.sh` fails the run if the Unity log contains `error CS`,
because it once reported `OK, 2/2 passed` over a build that did not compile.
**The MCP bridge has no such guard.** `refresh_unity` returns
`resulting_state: "idle"` whether compilation succeeded or not, and
`run_tests` will then execute the *previously built* assemblies and report a
cheerful green.

That bites hardest here, because mutation testing deliberately breaks code
and is therefore the workflow most likely to break a compile. A false green
during a mutation pass does not merely miss a bug — it manufactures evidence
that the tests bite when nothing was even rebuilt. This has happened: a
mutation deleted a member a test referenced, the test assembly failed to
compile, and the pass reported 183/183 with nine mutations applied.

So, when driving Unity through the bridge:

- Call `read_console` for errors **between** every `refresh_unity` and
  `run_tests`. No errors is the only evidence the run means anything.
- Mutate by changing behaviour, not by deleting members the tests name — a
  renamed `[JsonProperty]` or an inverted condition keeps the test assembly
  compiling, so the mutant actually reaches the run.
- Do not try to infer freshness from file timestamps. `git checkout` rewrites
  source mtimes, and an assembly with no changes legitimately does not
  rebuild, so the naive comparison reports stale assemblies that are fine.

When the editor can be closed, `tools/run-tests.sh` is the better instrument
for a mutation pass: it carries the guard, and its summary line is the one
rule 6 asks for.

## 4. Post the triage as a pull request comment

State which findings were real, which were false positives **and why**, and
what changed. When you disagree with a reviewer, say so and show the
reasoning — a wrong diagnosis left standing gets re-raised later.

Correct any claim in the pull request body that the review disproved. On #36
the description claimed "wall-to-wall accepted" was covered; it was not.

Say which lens Reviewer B took, and if the change had two kinds of risk, which
half nobody covered. A lens chosen badly is then visible rather than silent.

If a reviewer returned nothing — an error, an empty report — say so and
dispatch again. A pull request opened on one review says that in the triage.

When the two reviewers **contradict each other**, that is a finding about the
change, not noise to average away. It has happened here: one wanted a fix
widened to the whole file, the other wanted it narrowed. Put both positions in
the triage with the call and the reason. Where the disagreement is about what
the design *should* be rather than what it *is*, §5 applies — label it
`question` and leave it to the owner.

## 4b. Re-review the fixes, once, when they are substantial

The triage and the repairs are written by the author, who is by definition not
independent, and nothing else looks at them. Fixes written in response to
review currently reach the owner unreviewed, which is the one gap the parallel
protocol does not close.

So when the fixes are a defect class rather than a typo — new behaviour, a
changed invariant, a test rewritten because it could not fail — dispatch **one**
reviewer on the fix commits alone before opening. This is the only place
sequential review earns its keep: there is nothing to anchor to yet, because
the diff it reads did not exist when the first pair ran.

## 5. File what is out of scope

Findings in adjacent or already-merged code become issues with the repro,
not a wider pull request. Rule 1 is one issue, one branch, one pull request.
Design gaps where the code follows the spec faithfully get labelled
`question` — that is the owner's call, not yours.

## Then

Paste the `tools/run-tests.sh` summary into the pull request, per rule 6, and
open it. Stop there: the owner reviews it and says whether to merge (CLAUDE.md rule 1).
