# LUCID — Decisions

ADR-lite. One entry per decision taken during implementation that promotes a **[D]** to **[S]**, deviates from `SPEC.md`, adds a package, or settles an open question. Newest first. Edit the spec in the same pull request.

Format:

```
## YYYY-MM-DD — short title
Context: what forced the decision (issue #, playtest, spike).
Decision: what was decided.
Reason: why this over the alternatives.
Spec: sections changed.
```

## 2026-09-03 — The owner's word merges the pull request, not the owner's hand

Context: rule 1 said "Open the PR and stop; the owner reviews and merges. Never push to `main`, never merge your own PR." Every pull request in this repository is opened by the `entewurzelauskuh` account, which is also the owner's, and **GitHub will not let an account approve its own pull request** — so branch protection's "one approving review" can never be satisfied here, and every merge is an admin override regardless of whose hand performs it.
Decision: the owner still reviews every PR and still decides; the merge itself is performed on their word, per pull request, named. `gh pr merge <n> --squash --admin --delete-branch`. Branch protection stays as it is.
Reason: the part of rule 1 that was load-bearing is that nothing reaches `main` without the owner having looked at it, and that is untouched — the gate moved from a button to a sentence, and the sentence is the thing that was always doing the work. Keeping the protection rule in place while overriding it is deliberate: it is unenforceable for a sole maintainer using one account, and it becomes real the day a second collaborator can approve. Setting `required_approving_review_count` to 0 was rejected because it would state the truth about today and remove the rule that should apply tomorrow. Two other routes were raised in review and not taken now: a **repository ruleset with the owner as a bypass actor**, which would enforce the rule against everyone else rather than nobody and is the better shape once there is anyone else; and a **separate identity opening pull requests**, which would make the owner's approval genuinely possible. Both are worth revisiting at M1.1, when contributors arrive. The authorisation is explicitly per-PR and does not generalise: merging is outward-facing and hard to undo, and "I reviewed #73, merge it" is a different sentence from "merge things from now on".

What actually permits the merge is `enforce_admins: false` on `main`; `--admin` is only a `gh`-side flag that skips its own mergeability check and records nothing. If that setting is ever turned on, rule 1's command starts failing and the ruleset route above is the answer.
Spec: **`docs/SPEC.md` §18** — its Review flow bullet said "reviewed and merged by the owner. Claude Code never merges its own work", which this contradicts, so it is edited in the same pull request. `docs/WORKPLAN.md` §1 item 4, §3's definition of done and §9's owner-actions list said the same and are updated too. The branch-protection lines in §4 and §9.3 describe a setting that is unchanged.

## 2026-09-03 — Two reviewers, in parallel, with lenses chosen for the change

Context: rule 8 asked for "two reviewers with different lenses" without saying whether they run together, and I ran one on #73 on the grounds that a shell script did not need two.
Decision: always two, always in parallel, never letting the second read the first. Reviewer A stays adversarial correctness. Reviewer B takes the lens the change's **risk** sits under — specification conformance where the change makes a normative claim, *operational* where it makes an environmental one, and when it does both, the half Reviewer A is least likely to reach, said out loud in the triage.
Reason: independent convergence is the strongest signal this protocol produces. Two reviewers landing on one defect separately settles it without argument, and that is worth nothing if one has read the other. Anchoring is the other half: the findings that mattered most here came from a single reviewer each — a serialization bug on one side, a missing DECISIONS entry on the other — because neither was primed by the other's framing. Two rather than one is not about small changes being risky; it is that **nobody knows in advance which lens will find the thing.** On #73 the one lens dispatched happened to be the one that showed the change's premise was false; had it gone the other way, nothing would have.

Choosing the lens by *risk* rather than by "does a spec surface exist" was the review's correction, and the evidence is in this repository: #60 touched both `docs/SPEC.md` §9 and the harness, the spec lens was the obvious pick, and the operational half went unreviewed — #64, #65, #68 and #69 are all downstream of that one omission.

Sequential review of the same diff is rejected for the reasons above. Sequential review of the **fixes** is not, and is the gap that remains: a triage is written by the author, who is by definition not independent, so fixes currently reach the owner unreviewed. The skill now dispatches one reviewer on the fix commits when the fixes are a defect class rather than a typo.
Spec: none. `CLAUDE.md` rule 8 and `.claude/skills/pr-review/SKILL.md` §§1, 2 and 4 carry it.

## 2026-09-03 — A solidified door stops moving instead of becoming the wall

Context: #5. `docs/SPEC.md` §7 marks the fog-door block **[S]** and says a Solid connector is a "Wall in the cube's skin; the mist condensed into it"; `docs/WORKPLAN.md` §4 repeats it as an M0.5 deliverable, "Solid (condenses into the cube's wall material)".
Decision: a Solid door renders as mist that is dark, opaque and **completely still**, rather than taking on the cube's wall material. The target is unchanged and the delivery is deferred to M0.6.
Reason: a door cannot reach its cube's wall material yet. `FogDoor` sits on a socket under a cube prefab and knows its `Face` and nothing else; the skin that decides a wall's material is chosen per cube by the `DreamPack`, and nothing assembles the two until M0.6 instantiates cubes from an event log and wires their doors to derived states. Faking it — sampling a neighbouring wall, or hard-coding a material — would put a second answer to "what does this cube look like" beside the skin system that owns it. The substitute keeps the property that matters, which `docs/UI.md` §1 states as a principle rather than a preference: door states must not differ by hue alone, and a wall that does not breathe is a tell no colour-blind viewer can miss. Recorded rather than left in a code comment because a reader of §7 or of M0.6's acceptance would otherwise have no way to know the debt exists — the same reason the doorFrame-styles entry below was written.
Spec: none. §7 stays the target; only the delivery moves.

## 2026-08-25 — The dream pulls 2.4 g, and the Sleeper has flat feet

Context: #4. `docs/SPEC.md` §9 sets three numbers that are not independent: run 6 m/s, jump 1.2 m of rise, and clear "~4 m gap at full speed". Fixing two of them fixes gravity.
Decision: the tunables are the spec's own numbers — `RunSpeed`, `JumpRise` and `JumpTravel` (3.8 m of capsule-centre flight, which measures out as a 4.15 m gap) — and gravity is derived from them at **23.9 m/s², about 2.4 g**. Separately, `SleeperMotor` refuses to move horizontally into anything standing above its feet while airborne.
Reason: a 1.2 m jump under Earth gravity hangs for almost a second and carries 5.9 m, half again the reach §9 allows, so a heavy dream is forced rather than chosen; it is recorded here because it is a thing the M0 play-test should feel for rather than discover. The flat-feet rule implements §9's "no mantle", which a `CharacterController` breaks on its own: its foot is a hemisphere, so once the sphere's centre rises past a lip the corner lies outside the capsule and the body slides on and over it. Measured on the gauntlet, a 1.2 m jump mounted a **1.4 m** ledge — the very height the M0.4 acceptance requires it to fail — arriving 0.2 m higher than it ever rose. With the rule the tallest ledge is the rise itself. Tuning the jump down to make the acceptance pass would have satisfied the test while breaking the spec: a Sleeper would still have mantled, just from lower down.
Spec: none. §9's kit is unchanged and its `**[D]**` defaults stay open for the play-test; `JumpTravel` and the derived gravity are implementation of "~4 m", not a new promise.

## 2026-08-25 — Three doorFrame styles generate the same geometry for now

Context: #47. `docs/cube-spec.schema.json` defines `doorFrame` as `none | plain | arch | industrial`. The builder distinguishes two behaviours: `none` emits no trim, and the other three emit the same jambs and head.
Decision: ship it, and say so. `plain`, `arch` and `industrial` are accepted and produce identical geometry until skins exist.
Reason: the three differ in how they *look*, and looking like anything needs a SkinSet to resolve the trim role to a material — that is M1.10 (#23). Generating three different box arrangements now would be inventing an art direction the spec does not state, and it would have to be thrown away. Recording it matters because a cube author reading `docs/CUBE-SPEC.md` today would otherwise have no way to know that `"doorFrame": "industrial"` currently does nothing beyond `"plain"`.
Spec: none. The schema's four values all remain valid; only the rendering is deferred.

## 2026-08-25 — A pack's folder is named for its id, in lower case

Context: #47. Three sources disagreed. `docs/WORKPLAN.md` §2's layout said `Packs/Core/`; `docs/CUBE-SPEC.md` §3 and §4 both write `Packs/core/` in their worked example paths; and `docs/cube-spec.schema.json` constrains a pack id to `^[a-z0-9]+$`, which admits only lower case. A `Core/` folder from the M0.1 skeleton was already committed, so on a case-insensitive filesystem the builder's `Packs/core` resolved into it and nothing looked wrong.
Decision: the folder is named exactly for the pack id, so `core`. `docs/WORKPLAN.md` §2 is corrected.
Reason: the builder derives the folder from `spec.Pack`, which the schema forces to lower case, so any other convention means the builder cannot find a pack from its spec alone. Left as it was, a Linux checkout would carry `Packs/Core/` from git while the builder wrote `Packs/core/` — two packs, and `tools/build-cube.sh core/straight` finding neither. This is the same defect as the `lucid/` versus `Lucid/` project folder in `docs/DECISIONS.md`'s earlier history, and it stayed invisible for the same reason.
Spec: `docs/WORKPLAN.md` §2. `docs/CUBE-SPEC.md` was already right.

## 2026-08-25 — The floor slab hangs below the origin plane

Context: #47. `docs/CUBE-SPEC.md` §1 says the cube spans y in [0, 8] with "origin at the centre of the floor", and that doorways are "centred on their face at floor level, 2.5 m wide x 3 m high" with face centres at y = 0. Those two statements together fix where the floor slab can go, and it is not inside the stated span.
Decision: the generated floor slab occupies y in [-thickness, 0]. The walkable surface is exactly the origin plane, and a doorway occupies y in [0, 3] as §1 states.
Reason: the alternative — a slab at [0, thickness] — raises the walkable surface above the origin, so either the doorway starts 0.3 m up a step, or its lower 0.3 m is blocked by the floor. Both contradict §1's numbers.

That leaves the cube one thickness taller than its 8 m unless the ceiling gives the room back, so the ceiling slab stops at y = 8 - thickness rather than at 8. A cube at layer n then occupies world y in [8n - 0.3, 8n + 7.7]: exactly 8 m, with its floor slab filling the gap the cube below left above its ceiling. The two abut.

An earlier draft of this entry said the ceiling ran to 8 and that the two slabs "coincide rather than collide", at no cost. That was wrong, and the independent review of #47 caught it: coinciding is the failure, not the escape from it — two solid boxes in one volume, two overlapping colliders, and co-planar faces at every vertical join. It was also wrong about the cube actually shipped, whose `interior.height` of 4 makes its ceiling the block [4, 8], so the floor above was buried inside it rather than merely touching.
Spec: `docs/CUBE-SPEC.md` §1 and §2, edited in the same pull request to say which 8 m the cube occupies. `docs/WORKPLAN.md` §4's validator rule "everything inside the 8 m bounds" is read as x, z in [-4, 4] and y in [-thickness, 8 - thickness]; #48 implements it that way.

## 2026-08-25 — Newtonsoft.Json becomes a direct dependency, and the spec checker is hand-written

Context: #46. The cube pipeline reads `cube.spec.json`, so `Lucid.Editor` needs a JSON parser. Unity's own `JsonUtility` cannot express the format — no dictionaries, and `vec3` is an array of arrays.

Decision: `com.unity.nuget.newtonsoft-json` at **3.2.2** as a direct entry in `Lucid/Packages/manifest.json`.
Reason: it was already resolving, but only as a *transitive* dependency of `com.coplaydev.unity-mcp` at depth 1. That bridge is a development-only tool, and the entry above it in this file promises `tools/build-cube.sh` keeps working without it — so a contributor who never installs the bridge, or an owner who removes it, would have found the cube pipeline broken with no obvious cause. Naming it directly makes the dependency real rather than incidental.

Second decision: the builder's spec check is **hand-written against** `docs/cube-spec.schema.json`, not a JSON-Schema engine running that file.
Reason: there is no JSON-Schema validator available in either environment — Newtonsoft's schema package is not part of Unity's distribution, and the local Python has no `jsonschema` module. `docs/WORKPLAN.md` §4's "validates specs against the schema" is therefore delivered as a checker that enforces the schema's rules: Newtonsoft's `MissingMemberHandling.Error` and `Required.Always` cover unknown and missing members, and the ranges, patterns and the six cross-field rules in the schema's `allOf` are coded explicitly. The honest cost is drift: nothing makes the checker follow the schema if the schema changes. Revisit if the format grows enough that keeping them in step stops being obvious — vendoring a C# JSON-Schema library or requiring `pip install jsonschema` are both open.
Spec: none. `docs/WORKPLAN.md` §4's wording is satisfied in substance; the difference is recorded here.

## 2026-08-25 — A Sleeper may strand herself, and that is a way to lose

Context: #40, raised by the independent review of #36. The leak rule is evaluated on placement, from each Sleeper's current cube, and nothing re-evaluates when a Sleeper moves. A fuzz run over roughly 6000 accepted placements found 29 stranding events, every one caused by a Sleeper move and none by a placement or an exploration — so the rule itself is sound; this is a case it was never asked about.
Decision: accept it. Falling into a place you cannot climb out of is a legible way to lose a life, and lives (M1.5) are already the recovery path. Neither of the alternatives is worth its cost: detecting and respawning would need a Core API and host work for an outcome players can already read, and guaranteeing a reachable exit from every cube would constrain the Nightmare so heavily that drops would stop being funnels, which `docs/SPEC.md` §7 relies on.
Reason: the rule's job is to stop the *Nightmare* sealing a Sleeper away. It was never meant to stop a Sleeper walking into a pit, and the spec's wording had promised more than the rule delivers.
Spec: `docs/SPEC.md` §7, "Why the rules hold together" — the drops-are-funnels bullet now says the check is on placement and that self-stranding is a way to lose.

## 2026-08-25 — Round closes at dawn, and the powers' Apply methods report failure

Context: #34, and the independent review of that branch (CLAUDE.md rule 8). Four deviations from `docs/CORE-API.md` §8 and §9 were needed, each because the specification's listing was thinner than the behaviour it implies elsewhere in the document set.

Decision, and the reason for each:

1. **`Disconnected` no longer ends the round.** §8 said "every Sleeper is Awake or Consumed"; the implementation had treated any non-`InDream` status as finished, which ended the round on a two-second network blip and un-ended it on the player's return. `docs/NETCODE.md` §10 gives disconnects a reconnect grace, so the literal reading of §8 is the correct one. Dawn now consumes the disconnected as well, otherwise pulling the cable beat being eaten: the Sleeper survived dawn and the Nightmare was denied the 100 points.
2. **`Dawn` closes `TryPlace`, `TryExplore` and `TryWake`.** Only `Advance` respected the end of the round, so the maze kept growing and a Sleeper could still wake and score on the results screen. Every client re-derives from the broadcast log, so those events would have reached them.
3. **`Powers.ApplyEffect` and `ApplyTrigger` return `bool` instead of `void`.** They cannot assume a passing verdict the way `Rules.ApplyPlace` does: §10's host loop validates, may spend budget on a placement, and only then applies. The `void` versions spent whatever they could and started the cooldown regardless, so an effect the Nightmare could no longer afford fired for free.
4. **`SleeperState` carries `WokeAtMs`, and `DeathOutcome` has an `Ignored` value.** §8's `Scoring` contract is "100 + remaining seconds *at wake*", which cannot be computed at results time without recording the instant. `Ignored` answers a death arriving just after a wake, which is ordinary on a 10 Hz link and must not cost a life.

Also: `PowerError.Possessed` now has a meaning. It is returned while `PossessionActive` is set, and `Disabled` is reserved for an effect that is not in this round's list. Previously both returned `Disabled` and `Possessed` was dead, which is the defect #39 records against `PlaceError.StartProtected`.

Spec: `docs/CORE-API.md` §2 (SleeperState), §8 (IsOver, ReportDeath, the dawn gate) and §9 (Apply signatures, Possessed vs Disabled) edited in the same pull request.

## 2026-08-25 — Every pull request gets an independent review before it is opened

Context: #41. There is no CI (`docs/SPEC.md` §18), so a pull request's claims rest on what the author checked by hand, and four consecutive reviews showed that reading the diff was not enough. #31 carried a bypass in the asset-rule gate itself; #35 a silent-corruption path; #36 a rule that let the Nightmare re-open a door a Sleeper had sealed, plus three tests that asserted nothing while appearing to be coverage. Each was caught in review before merging, which is the point: none was visible by reading the diff.
Decision: `CLAUDE.md` rule 8 and the `pr-review` skill. Two independent reviewers with different lenses (adversarial correctness, and conformance against the spec's own required-test list), every finding reproduced before it is fixed, every test touched mutation-checked, the triage posted on the pull request, and out-of-scope findings filed as issues.
Reason: the two habits that actually found things were independence and mutation testing. On #36 two reviewers converged on the same critical defect separately, which settled it without argument; and mutation testing has now caught four tests that could not fail. Neither is discoverable by reading. This is cheaper than CI and does not need a Unity licence secret; `docs/SPEC.md` §18 still holds that GitHub Actions is a one-file change once contributors appear, and this protocol is what stands in for it until then.
Spec: none. `docs/WORKPLAN.md` §3's definition of done now names the review.

## 2026-08-25 — Core value types are hand-written structs, not record structs

Context: #32. `docs/CORE-API.md` §2 declares `Coord`, `ConnectorRef` and `PlaceVerdict` as `readonly record struct`, and uses file-scoped namespaces throughout. Unity 6000.3 compiles at **C# 9.0** (`<LangVersion>9.0</LangVersion>` in the generated projects); record structs and file-scoped namespaces are C# 10.
Decision: value types are hand-written `readonly struct`s implementing `IEquatable<T>`, `==`, `!=`, `GetHashCode`, `Deconstruct` and `ToString` — exactly what the compiler would have generated. Namespaces are block-scoped. Reference records (`CubeType`, `Derived`, the events) stay records, which C# 9 does support.
Reason: semantics are unchanged — these remain value types with value equality, which matters because `Coord` is a dictionary key on every hot path. Making them classes instead would have changed allocation and equality behaviour throughout Core. Raising the language version is not supported by Unity and would risk the IL2CPP and Burst toolchains for a syntactic convenience.
Spec: none. `docs/CORE-API.md` §2's listings are illustrative of shape, not of syntax; the API surface is identical.

## 2026-08-25 — IsExternalInit is declared per assembly

Context: #32. C# 9 records compile positional members to `init`-only setters, which require `System.Runtime.CompilerServices.IsExternalInit`. That type arrived with .NET 5; Unity's .NET Standard 2.1 profile predates it, so every record in `Lucid.Core` failed with CS0518.
Decision: declare an internal `IsExternalInit` in each assembly that defines records. `Lucid.Core/IsExternalInit.cs` is the first.
Reason: the documented workaround for this exact combination. The alternative — abandoning records for plain classes with hand-written equality — would cost value semantics that the event log's round-trip test and the rules' comparisons depend on.
Spec: none.

## 2026-08-24 — MCP for Unity bridge added, pinned to v10.1.2

Context: issue #29. Driving the editor only through `-batchmode` makes small round trips (reading the console, running one test) cost a full editor launch. Spec §17 already allows "a Unity MCP bridge if editor round trips get tedious".
Decision: `com.coplaydev.unity-mcp` from `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity`, pinned to the **v10.1.2** tag.
Reason: pinned to a tag rather than the project's `#beta` default branch, so the dependency cannot move under us — the same reasoning spec §14 applies to the community Facepunch transport. A dev-only tool does ship in the public manifest as a result; the alternative, an embedded copy under `Packages/`, would vendor several thousand lines of third-party source into the repository, which is worse. Batch mode stays the contract: `tools/run-tests.sh` and `tools/build-cube.sh` must keep working without the bridge, so contributors are never required to install it.
Spec: none — §17 already anticipates the bridge.

## 2026-08-24 — Netcode for GameObjects and Addressables pinned

Context: M0.1. The Unity 6 URP project was generated from the 3D cross-platform template, whose manifest carries neither package, but spec §14 requires both.
Decision: `com.unity.netcode.gameobjects` at **2.13.1** and `com.unity.addressables` at **2.11.2** in `Lucid/Packages/manifest.json`.
Reason: both declare `unity=6000.0` and so are compatible with the pinned 6000.3.11f1. NGO 2.13.1 is the registry's `latest` and satisfies §14's "2.x in host mode". For Addressables the registry's `latest` dist-tag is 2.11.2 even though 3.0–4.0.1 are published; taking the tagged release is what Package Manager offers by default and avoids adopting the newer build pipeline before anything depends on it. Revisit if a pack ever needs 4.x.
Spec: none — §14 already names both packages; this only pins versions.
