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
