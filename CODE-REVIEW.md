# Code review — Lucid, whole tree at `830f736` (branch `m0/15-merge-authority`, 2026-09-03)

Written for an agent to work from. Three independent reviewers read the tree in full with different lenses:
Core rules and determinism against `docs/CORE-API.md`; Unity Runtime/Editor code and spec fidelity; tooling,
hooks and documentation consistency. Nothing was run and nothing was changed. Every finding below was
verified against the quoted lines by its reviewer; treat them as claims to reproduce before fixing (rule 8).

## How to work this file

- One finding, or one coherent cluster, per issue/branch/PR (CLAUDE.md rule 1). Do not batch unrelated fixes.
- Reproduce first: write the failing test or the failing command, then fix. Mutation-check every test you touch.
- Anything that changes a spec sentence needs the `docs/SPEC.md`/`docs/CORE-API.md` edit **and** a
  `docs/DECISIONS.md` entry in the same PR (rule 7). Several findings below are exactly that.
- Findings marked *question* are not code changes: comment on the issue, label `question`, move on (rule 2).
- `tools/run-tests.sh` before every PR; the editor must be closed for the batch tools.

## Totals

| Lens | Critical | High | Medium | Low | Nit |
|---|---|---|---|---|---|
| Core | 0 | 0 | 4 | 10 | 8 |
| Runtime / Editor | 0 | 1 | 4 | 6 | 5 |
| Tooling / docs | 0 | 2 | 6 | 11 | 3 |

Verified clean and not worth re-checking: no `UnityEngine`, float, `DateTime` or culture-sensitive call in
Core; every hash-feeding walk is ordered; `SleeperMotor` matches SPEC §9's kit; `FogDoorTransitions` matches
SPEC §7's table; the asmdef graph is exactly the rule; every package has a DECISIONS entry; every asset has
its `.meta`; all previews are LFS pointers; `run-tests.sh` cannot report green on a run that never started.

## Suggested work order

1. **Licence gate accepts CC-BY-NC/ND/SA and rejects "CC BY 4.0"** (High, T1). Same regex in
   `tools/check-licenses.py:21` and `CubeValidator.cs:239`. The one gate rule 5 relies on has a hole.
2. **`CubeEquivalence` compares almost nothing the template emits** (High, R1). `verify-generated.sh` says
   OK over stale prefabs after generator changes. Fix by reusing `SceneSignature.Of` for prefabs.
3. **#66/#67/#72/#70 closed with no code landed** (High, T2). Reopen or replace, then fix the CLAUDE.md
   Status paragraph and `verify-generated.sh` header that still point at them. Do this alongside 2.
4. **Hysteresis pins a depth number, not the doors** (Medium, C1). Off by default, but the planned M0.10
   knob yields zero exits and `WouldTrap` on a legal loop-closing placement. Pair with C3 (no loop test).
5. **`Derived.Hash` ignores type and rotation** (Medium, C2). Spec change + DECISIONS entry.
6. **Fog door per-frame cost** (Medium, R2), **overlap buffer truncation** (Medium, R4), **`Configure` after
   `Build`** (Medium, R5), **`SceneSignature` blind spots** (Medium, R3).
7. Tooling Mediums: hook scope and sub-folder `.meta` false positive (T3, T4), `.gitignore` anchoring (T5),
   three-way licence-ledger contradiction (T6), WORKPLAN merge rule drift (T7), README (T8).
8. Test-strength Lows in Core (C12, C13, C14) are cheap and each closes a mutation hole; do them in one PR.
   C4 (the 50-event replay is a straight line) belongs with them.
9. Everything else as time allows. Nits can ride along with a PR that touches the same file.

Finding IDs: C = Core, R = Runtime/Editor, T = Tooling/docs, numbered in the order they appear below.

---

# Part 1 — Core (C)


Scope: every file in `Lucid/Assets/_Lucid/Core/` and every test in `Lucid/Assets/_Lucid/Tests/EditMode/Core/`, read in full. Grep for `UnityEngine|float|double|decimal|DateTime|CultureInfo|ToLower|ToUpper|string.Compare|OrderBy|Random` over Core returned nothing; `Lucid.Core.asmdef` has `noEngineReferences: true` and no references. Rule 3 holds. Every hash-feeding walk goes through `Lattice.CoordsInOrder()` / `Faces.All`; the one unordered dictionary iteration in `Deriver` (step 3, `Deriver.cs:81-86`) only computes a maximum, which is order-independent. No determinism hazard found in the rules themselves.

All paths below are absolute under ``.

---

- **C1 [Medium] Hysteresis can leave a valid lattice with no exit, and refuses legal placements as `WouldTrap` — `Lucid/Assets/_Lucid/Core/Deriver.cs:88-117`, `Lucid/Assets/_Lucid/Core/RulesValidate.cs:88`.**
  When hysteresis holds, the code pins the exit *depth number* (`exitDepth = previousExitDepth;` line 96) and then marks Fog doors on cubes at that depth (`if (depth[c] != exitDepth) continue;` line 104). But a placement that closes a loop can *lower* the depth of the previous exit cube (BFS finds a shorter route), so after the hold no cube sits at `previousExitDepth` any more, `exits` stays empty and line 117 sets `exitDepth = -1` — while Fog doors still exist. `CheckLeak` then returns `WouldTrap` (`RulesValidate.cs:88`) for a placement that is legal under the spec's own §3 step 4 ("take the maximum depth d ... every Fog door on a cube at depth d becomes Exit"; with hysteresis, "keep the exits where they were" — the doors, not a depth number). Concrete repro with the test registry, `ExitHysteresis: 2`: Start→Tee R0 (0,1,0)→Straight (0,2,0)→Corner R90 (0,3,0)→Straight R90 (1,3,0)→Corner R180 (2,3,0)→Straight (2,2,0)→Tee R270 (2,1,0) [depth 7, exits (2,1,0).E and .W]. Placing Straight R90 at (1,1,0) via (0,1,0).E attaches both sides; (2,1,0) drops to depth 3; `deepest = 3 < 7 + 2` and `StillHasFog` is true (the E door is still Fog) → held at 7 → no cube at depth 7 → `Exits.Count == 0` → `WouldTrap`. `StillHasFog` also checks that a previous exit is still `Fog` (line 148) *after* step 1 but *before* Exit marking, so it is correct as far as it goes; the defect is pinning by depth. Off by default (`RoundSettings.ExitHysteresis = 0`), so no live impact today, but `docs/SPEC.md` §7 and §16 plan to switch it on, and the test `HysteresisHoldsTheLightThenReleasesIt` cannot see it (no loop is closed).
  *Fix:* when the hold condition is true, compute `exitDepth` as the current depth of the previous exit cubes that still have Fog (`depth[door.Cube]` for the doors that pass `StillHasFog`), not `previousExitDepth`; and, belt and braces, if the held pass yields `exits.Count == 0`, fall back to `deepest` before setting `exitDepth = -1`. Add a test that closes a loop under hysteresis and asserts `Exits.Count > 0` and that the placement validates.

- **C2 [Medium] `Derived.Hash` covers only depths and connector states, so two machines can agree on the hash while disagreeing on which cube stands where — `Lucid/Assets/_Lucid/Core/Deriver.cs:119-136`.**
  The hash folds `(coord, depth)` then `(coord, face, state)`. `CubeInstance.TypeId`, `Rotation` and `SkinId` are not in it. This matches CORE-API §3 step 5 literally, but not its stated purpose ("Two machines that agree on the hash agree on the dream"), nor the sync check in §10 / NETCODE §5 where type ids travel as registry *indices*: a client that maps an index to a different type with the same connector mask (Straight vs. a corridor chicane, both `N|S`) derives the identical hash and the mismatch is never reported. Rotation is likewise invisible whenever the rotated masks coincide (a Straight at R0 and R180). `TheHashIgnoresInsertionOrder` and `TheHashChangesWhenTheDreamDoes` cannot catch this because they only vary geometry.
  *Fix:* in step 5 also feed `Fnv.String(hash, instance.TypeId)` and `Fnv.Int32(hash, (int)instance.Rotation)` per cube in `CoordsInOrder()` (SkinId is cosmetic; include it or not, but decide). This is a spec change to CORE-API §3 step 5 → needs a `docs/DECISIONS.md` entry in the same PR (rule 7). Add a test: same geometry, different type id (register a second `N|S` type) → different hash.

- **C3 [Medium] §12 "Derive: depth on a loop" is not covered — `Lucid/Assets/_Lucid/Tests/EditMode/Core/DeriverTests.cs` (whole file), `docs/CORE-API.md` §12.**
  CLAUDE.md states every §12 item is covered. `DepthCountsHopsAlongALine` and `DepthCrossesLayers` exist; nothing closes a loop. `AlmostClosedLoop` (LeakRuleTests.cs:16-23) deliberately stops one cube short, `TheHashIgnoresInsertionOrder` builds a cross with two arms (a tree), and `SolidDoorWithAWayAround` (SolidifiedDoorTests.cs:18-30) runs a corridor round a wall without reconnecting. So the BFS's "shortest hop count" property when two routes exist (`Deriver.cs:55-70`) is never asserted, and neither is the `depth.Count != l.Cubes.Count` guard on line 72. A mutation that made depth "first route found in coord order" rather than "shortest" would pass every test.
  *Fix:* add a test that builds a ring (Start→Cross; Corner R270 at (1,1,0); Corner R180 at (1,2,0); Tee R0 at (0,2,0) attaching to both (0,1,0) and (1,2,0)) and asserts `DepthOf((1,2,0)) == 3` via either route, plus a longer detour variant where closing the loop *reduces* a depth (which also serves finding 1).

- **C4 [Medium] The "scripted 50-event game" replay test is a straight corridor with no-op explores — `Lucid/Assets/_Lucid/Tests/EditMode/Core/EventLogTests.cs:98-117`.**
  The walk only ever places `TestLattice.Straight` on an exit door. A Straight built on an exit has exactly one fog door, so `derived.Exits.Count` is always 1, `rng.Next(1)` is always 0, and the "dream" is a line of ~43 cubes heading north (unvalidated, so it also runs past `Limits.FootprintHalf = 12`). The `step % 7 == 6` explores target `door.Cube`, which is the exit cube, whose only fog door *is* the exit — so no door ever solidifies and `Solidified` stays empty for the whole replay. The §12 item exists to prove `Replay` reproduces the host across placements, solidification and exit movement; this covers only the first. `ReplaySurvivesTheFileFormat` (lines 140-164) covers one solidify, which is the only reason the explore path in `Replay` is exercised at all.
  *Fix:* draw the cube type from a small list (Straight, Tee, Cross, Corner with a fitting rotation) and let the walk use `Rules.ValidatePlace` to skip illegal picks; explore a *non-exit* cube (e.g. the previous placement's cube once a deeper one exists) so `Solidified` grows; assert `live.Solidified.Count > 0` and `derived.Exits.Count > 1` at least once during the walk as fixture preconditions.

- **C5 [Low] `CheckLeak` dereferences `ctx.Settings` that `ValidatePlace` treats as optional — `Lucid/Assets/_Lucid/Core/RulesValidate.cs:45` vs `:81`.**
  Line 45: `Limits limits = ctx.Settings?.EffectiveLimits ?? Limits.Default;` tolerates a null `Settings`; line 81: `ctx.Settings.ExitHysteresis` throws `NullReferenceException` on the same context once steps 1-7 pass. `Rules.Rederive` (Rules.cs:57) also uses `?.`. Either the null is supported or it is not; today a null `Settings` fails only on the rarest path.
  *Fix:* pick one: `ctx.Settings?.ExitHysteresis ?? 0` on line 81, or validate `Settings != null` in the `RuleContext` constructor and drop the `?.`s.

- **C6 [Low] Deriver's Solidified handling deviates from CORE-API §3 step 1 and §5 step 6 without a DECISIONS entry — `Lucid/Assets/_Lucid/Core/Deriver.cs:32-36,44`, `Lucid/Assets/_Lucid/Core/RulesValidate.cs:62`, `Lucid/Assets/_Lucid/Core/Lattice.cs:75-76`.**
  Spec step 1: "Attached if `n` exists and `n` has the connector `Opposite(f)`. If `n` exists without that connector ... throw." The code checks `IsSolidified(door)` *first* (line 32) and uses `HasOpenConnector` for the neighbour (line 44); the fit rule likewise uses `HasOpenConnector` (RulesValidate.cs:62). This is the right behaviour (SPEC §7 "never again") and `SolidifiedDoorTests` pins it, but CORE-API still describes the pre-`HasOpenConnector` algorithm, and `grep -i solid docs/DECISIONS.md` finds only the M0.5 rendering entry. Rule 7: a deviation edits the spec and adds a DECISIONS entry in the same PR.
  *Fix:* amend CORE-API §3 step 1 ("Attached if `n` exists and `n` has an *open* (not Solidified) connector `Opposite(f)`; a Solidified door is Solid regardless of `n`") and §5 step 6 ("the neighbour has an open `Opposite(f)`"), and add the DECISIONS entry retroactively.

- **C7 [Low] `EventLog.Read` accepts any rotation byte, and `Faces.Rotate` silently normalises it — `Lucid/Assets/_Lucid/Core/EventLog.cs:133`, `Lucid/Assets/_Lucid/Core/Faces.cs:51,56`.**
  `(Rotation)r.ReadByte()` is unchecked. `Rotate(Face, r)` does `((int)f + steps) & 3`, so `Rotation = 4` behaves as `R0` in every rule while `CubeInstance.Rotation` carries the bogus value into the runtime (which will spin the prefab by `4 * 90°` or index out of a table) and back out through `Write`. Kind bytes are checked (line 142); rotation is not. A wire/log corruption that should fail loudly instead changes a rendered cube without changing the hash.
  *Fix:* `if (rotation > Rotation.R270) throw new InvalidDataException(...)` in `Read`, and consider making `Faces.Rotate` throw on `(int)r > 3` like `Opposite` does on a bad face.

- **C8 [Low] Dawn refusals reuse unrelated error codes — `Lucid/Assets/_Lucid/Core/Round.cs:111,130`.**
  `TryPlace` at Dawn returns `PlaceError.NotADoor`; `TryExplore` returns `ExploreError.NoCube`. `PlaceError` is shown on the red ghost (UI.md §8, per the comment in PlaceError.cs), so the Nightmare is told the exit door "is not a door" at the moment the round ends. The DECISIONS entry of 2026-08-25 records that Dawn closes these methods but not which code they answer with.
  *Fix:* add `PlaceError.RoundOver` (and `ExploreError.RoundOver`, or document that `NoCube` is the silent-drop code) with a CORE-API §5/§6 edit; or at minimum document the choice in DECISIONS.

- **C9 [Low] Spec question: an Exit on the Up face of a non-climbable cube counts as reachable — `Lucid/Assets/_Lucid/Core/Traversal.cs:42-52`, `docs/CORE-API.md` §4.**
  `CanReachExit` returns true if any reachable cube *owns* an Exit door, and §4 says explicitly "a Sleeper standing in the exit cube itself trivially reaches one". For a type like the test registry's `Pit` (`N|Up`, not climbable) the Up door can be an Exit, yet the Sleeper cannot touch a ceiling doorway without a climbable. The leak rule therefore lets the Nightmare leave a Sleeper in a pit whose only white door is overhead. Code matches spec; the spec line is not marked [S].
  *Fix:* not a code change. Comment on the issue with label `question`: should `CanReachExit` require `CanPass(c, f)` for the Exit door's own face (so an Up exit counts only from a climbable cube)? If yes, that is a one-line change in the `CanReachExit` loop plus a test.

- **C10 [Low] Spec ambiguity: `All` is refused if any single dream is on cooldown — `Lucid/Assets/_Lucid/Core/Powers.cs:56-59`, `Lucid/Assets/_Lucid/Tests/EditMode/Core/PowersTests.cs:64-72`.**
  SPEC §10 says "cooldowns are per power per dream, and All consumes them everywhere" and §9 of CORE-API says "starts the cooldown in every targeted dream"; neither says All *requires* every dream to be ready. The implementation does (`if (_effectReadyAt[k][dream] > clockMs) return OnCooldown;` for every dream in `Dreams(All)`), and the test `AllIsRefusedIfAnySingleDreamIsStillOnCooldown` locks that in. The alternative reading — All fires into the dreams that are ready and starts their cooldowns — is at least as consistent with "All is the efficient choice". Targeting one dream then being locked out of All for 30 s is a real gameplay consequence.
  *Fix:* comment on the issue with label `question`; whichever answer, write the sentence into CORE-API §9 so the test is pinned to text.

- **C11 [Low] `Lattice.WithExplored` has no guard, so a corrupt log replays silently into an invariant-5 violation — `Lucid/Assets/_Lucid/Core/Lattice.cs:118-124`, `Lucid/Assets/_Lucid/Core/EventLog.cs:58-60`.**
  `WithCube` throws `LatticeInvariantViolation` on a duplicate coord precisely because "replay runs unvalidated" (line 105-107). `WithExplored` accepts any coord: a `CubeExplored` for a coord with no cube marks thin air explored, and a `CubeExplored` for `Start` solidifies the bedroom door (`ApplyExplore` reads `ConnectorsAt(Start)` and its N door is Fog whenever something deeper exists — no wait, it is Attached once anything is built; but on an empty dream it is the Exit, so nothing solidifies). The nonexistent-coord case is the live one: it survives replay, diverges `Explored` from the host without touching the hash, and the next `ValidateExplore` on the host answers differently from the client.
  *Fix:* in `WithExplored`, throw `LatticeInvariantViolation` if `!_cubes.ContainsKey(c)` or `c == Start`; add a replay test mirroring `ReplayRefusesToOverwriteACube`.

- **C12 [Low] The Dawn gate on `TryExplore` is asserted by a line that passes for three other reasons — `Lucid/Assets/_Lucid/Tests/EditMode/Core/RoundTests.cs:336`.**
  `Assert.That(r.TryExplore(0, new Coord(0, 1, 0)), Is.Not.EqualTo(ExploreError.None))`: at this point Sleeper 0 is Consumed by dawn (line 136 returns `NoCube`), (0,1,0) was never placed (`ValidateExplore` returns `NoCube`), and only then would the Dawn check matter. Deleting `if (Phase == Phase.Dawn) return ExploreError.NoCube;` from `Round.cs:130` leaves this test green. The `TryPlace` half of the test is sound (a mutation there makes the placement succeed).
  *Fix:* build a corridor and explore it *before* dawn to prove the path works, or drop the assertion and state in the test that dawn's consume is what closes exploration, as `NobodyWakesAfterDawn` does for waking.

- **C13 [Low] `StillHasFog` is not exercised: removing it leaves every hysteresis test green — `Lucid/Assets/_Lucid/Tests/EditMode/Core/DeriverTests.cs:114-132`, `Lucid/Assets/_Lucid/Core/Deriver.cs:141-151`.**
  The only hysteresis test holds (previous exits keep their fog) and then releases by depth (3 ≥ 1 + 2). The release-because-the-old-exits-are-gone branch — explore the T so its spare door condenses, or build on it — is never taken, so `StillHasFog(previousExits, connectors)` could be replaced by `true` (or `previousExits` dropped from the signature) without a failure. §12 says "hysteresis holds *and releases*"; only the depth release is covered.
  *Fix:* add a test: hold at depth 1, then explore (0,1,0) so its N door solidifies and the E door is attached; assert `ExitDepth` moves to the actual deepest fog door.

- **C14 [Low] Ties across *different* cubes at the exit depth are never asserted — `Lucid/Assets/_Lucid/Tests/EditMode/Core/DeriverTests.cs:69-79`, `Lucid/Assets/_Lucid/Tests/EditMode/Core/LeakRuleTests.cs:116`.**
  `TiesProduceSeveralExitsAtOnce` is two doors on one T. `StrandingASleeperBelowADropIsRejectedAndNamesThem` sets up a tie between the pit (0,1,-1) and (-1,1,0) but only asserts `ExitDepth == 2`. A mutation of `Deriver.cs:102-114` that stops after the first cube at `exitDepth` in `CoordsInOrder()` (the pit, z = -1, sorts first) passes both. SPEC §7 exit rule [S]: "**All** fog doors on those cubes are exits. Ties are allowed".
  *Fix:* in the tie fixture, assert both `(0,1,-1).N` and `(-1,1,0).W` are `Exit` and `Exits.Count == 2`.

- **C15 [Nit] The start cube's `PlacedSeq` collides with the first event's `Seq` — `Lucid/Assets/_Lucid/Core/Lattice.cs:48`, `Lucid/Assets/_Lucid/Core/EventLog.cs:26`.**
  `new CubeInstance(startTypeId, startRotation, null, 0)` and `NextSeq` starts at 0, so the bedroom and the first placed cube both carry `PlacedSeq = 0`. Nothing reads it yet; when the runtime does (e.g. "which cube appeared last"), it will be ambiguous. *Fix:* `-1` for the start cube, or document that seq 0 is the bedroom and `EventLog.NextSeq` starts at 1.

- **C16 [Nit] `PlaceError.StartProtected` is unreachable by spec — `Lucid/Assets/_Lucid/Core/PlaceError.cs:19`.**
  CORE-API §5: "Placing anything on the start cube's single door is legal too". No code path returns `StartProtected` (grep confirms). A dead enum member on a wire enum is a small hazard for the HUD string table (UI.md). *Fix:* remove it from both the enum and CORE-API §5, or note it as reserved.

- **C17 [Nit] A round with zero Sleepers is over before it begins — `Lucid/Assets/_Lucid/Core/Round.cs:57-73`.**
  `IsOver` loops over an empty array and returns true. SPEC §5 requires 1–4 Sleepers at start, so the lobby prevents it; the constructor could enforce `sleepers.Count >= 1` so a host bug fails at construction rather than on the first tick.

- **C18 [Nit] `EventLog.Read` with a truncated stream throws `EndOfStreamException`, not `InvalidDataException` — `Lucid/Assets/_Lucid/Core/EventLog.cs:122-145`.**
  `ReadRejectsSomethingThatIsNotALog` covers the magic; a valid header followed by `count = 1000` and no bytes fails inside `BinaryReader`. Callers that catch `InvalidDataException` to reject a bad `.lucidlog` will let this one propagate. *Fix:* wrap the body in `try { … } catch (EndOfStreamException e) { throw new InvalidDataException("truncated event log", e); }`.

- **C19 [Nit] `Derived.Depth` and `Derived.Connectors` are unordered dictionaries on the public surface — `Lucid/Assets/_Lucid/Core/Derived.cs:11-12`.**
  Core itself never iterates them into anything order-sensitive, but Runtime/Netcode will (door rendering order, per-door messages). A consumer that serialises `Connectors` in enumeration order will be nondeterministic across machines. *Fix:* either document "iterate via `Lattice.CoordsInOrder()` + `Faces.All`" on the record, or expose an ordered `IReadOnlyList<KeyValuePair<...>>` view alongside.

- **C20 [Nit] `Deriver.Derive` grew a `previousExits` parameter and `Lattice.Solidified/Explored` are `IReadOnlyCollection`, neither recorded — `Lucid/Assets/_Lucid/Core/Deriver.cs:17`, `Lucid/Assets/_Lucid/Core/Lattice.cs:34-35`.**
  CORE-API §2/§3 say `IReadOnlySet` (absent from .NET Standard 2.1, so the change is forced) and a four-argument `Derive`. Both are reasonable; both are undocumented drift. *Fix:* one-line edits to CORE-API §2 and §3, with a DECISIONS line for `IReadOnlySet` next to the existing record-struct entry.

- **C21 [Nit] `Budget.MsUntilNextPoint` is 0 both for "no trickle" and would be 0 for "point due now" — `Lucid/Assets/_Lucid/Core/Budget.cs:29-30`.**
  `TrickleIntervalMs == 0 ? 0 : …` makes the HUD unable to tell "trickle off" from "next point imminent" (the latter cannot actually occur since `Advance` consumes whole intervals, but the API reads as if it could). *Fix:* return `-1` (or `int.MaxValue`) when the trickle is off, and say so in the doc comment.

- **C22 [Nit] `SkinId` of `""` reads back as `null`; `TypeId` of `null` reads back as `""` — `Lucid/Assets/_Lucid/Core/EventLog.cs:89-91,132-136`.**
  Asymmetric normalisation: `BinaryRoundTripPreservesEveryEvent` only tests `null` and a non-empty skin. A `CubePlaced` with `SkinId = ""` does not round-trip by record equality, and a `TypeId = null` becomes `""` which then throws `KeyNotFoundException` in `reg.Get` during replay rather than a clear error. *Fix:* treat both fields the same way (empty ⇒ null) and reject a null/empty `TypeId` in `Read` with `InvalidDataException`.

---

## What was checked and found sound

- Fit rule (`RulesValidate.cs:50-64`) checks all six faces including Up/Down and requires the target's opposite; two-sided fit tested (`FitIsCheckedAgainstEveryNeighbourNotJustTheTarget`).
- Frontier: empty space, wall, Attached, Solid, Exit all tested with the right codes; validation is side-effect free (`ValidationNeverChangesAnything`).
- Leak rule: both halves (no exit at all; a Sleeper cut off) tested, `TrappedSleeper` id asserted, non-InDream statuses ignored, climbable vs pit both directions tested.
- Traversal direction (`CanPass`) matches §4 and SPEC §7 "climbability belongs to the lower cube".
- Explore: solidifies only Fog, never Exit; start exempt; idempotent; both race orders; invariant 3 checked on the set itself, not just the derived state.
- Budget integer remainder: coarse vs fine, awkward step sizes, boundary spend.
- Round: head start blocks waking, dawn consumes InDream and Disconnected, lives count down and respawn at Start, `IsOver` in both endings, clock and trickle clamp at dawn independent of tick size, `Replay(log).Hash == live.Hash` for the round's own log, dead telemetry dropped (#38), only a live Sleeper may report exploration.
- Scoring: `100 + remaining whole seconds`, consumed = 0 and +100 to the Nightmare, Nightmare always present, accumulation guards duplicate `PlayerId`s.
- Powers: per-dream cooldown, All consumes all, flat cost, possession vs disabled distinct, unaffordable effect neither fires nor burns cooldown, refused trigger does not extend cooldown.
- FNV-1a is fed little-endian bytes and UTF-16 code units; no platform dependence.

---

# Part 2 — Runtime / Editor (R)


Scope read in full: `Lucid/Assets/_Lucid/Runtime/**` (Sleeper, fog doors, Dev gauntlet, cube types, Mist shader), `Lucid/Assets/_Lucid/Editor/**` (CubeBuilder, CubeTemplateBuilder, ShellBuilder, CubeGeometry, CubeValidator, CubeSpecChecks/Reader, CubePreviewRenderer, CubeEquivalence, GeneratedScene(s), Gauntlet/FogDoor scene builders, SceneSignature), all asmdefs and AssemblyInfo files, `Tests/EditMode/{AssemblyBoundaryTests,Cubes,Doors,Scenes,Sleeper}`, `Tests/PlayMode/{Doors,Sleeper}`, and `tools/{run-tests,build-scenes,verify-generated}.sh`. Nothing was run and nothing was changed.

**Spec fidelity, checked line by line and found correct:**
- `SleeperMotorSettings.cs:37-42` carries exactly SPEC §9's kit (run 6, crouch 2.5, rise 1.2, crouch height 1.0) and `SleeperMotor.cs` implements run/jump/crouch and nothing else; gravity derivation matches `docs/DECISIONS.md` ("2.4 g"), and the no-mantle probe is the recorded deviation.
- `FogDoorTransitions.cs:25-53` is SPEC §7's table exactly: Fog↔Exit, Fog|Exit→Attached, Fog→Solid, Exit→Solid refused, Attached and Solid terminal; `IsPassable`/`Wakes` match the §7 table's "Passable" column. `FogDoorTransitionsTests.cs` names all sixteen pairs.
- Assembly graph matches CLAUDE.md (Runtime→Core; Netcode→Runtime,Core; Editor→everything; Core references nothing, guarded by `AssemblyBoundaryTests`). No `UnityEngine` in Core.
- No wall-clock sleeps in PlayMode; every test steps `Tick`/`Physics.Simulate` by hand. `run-tests.sh` refuses `total == 0`, compile errors and aborted runs, so a PlayMode run cannot report green without running.

There are no Critical findings. The findings below are ordered by severity.

---

- **R1 [High] `CubeEquivalence` is the staleness oracle for both cube prefabs and the template, and it compares almost none of what the template now emits** — `Lucid/Assets/_Lucid/Editor/Cubes/CubeEquivalence.cs:57-88`, used by `CubeBuilder.cs:64` and `CubeTemplateBuilder.cs:45`.
  `SameComponents` compares the sorted type list plus `MaterialRole.Role`, `Connector.{Face,IsDoorway,Door==null}`, `FogDoor.{Face,State}` and `CubeBounds.{Size,FloorDrop}`; nothing else. The template (`CubeTemplateBuilder.cs:82-118`) also emits a `NavMeshSurface` (every field uncompared), a `FogDoorVisual` (`_layers` uncompared, `FogDoorVisual.cs:36`), a `FogDoor` with `_transitionSeconds` (uncompared, `FogDoor.cs:59`), and every shell piece from `ShellBuilder.Box` (`ShellBuilder.cs:150-158`) carries `MeshFilter.sharedMesh`, `MeshRenderer.sharedMaterials` and a `BoxCollider` whose size/centre/isTrigger/enabled are never read. Because `CubeTemplateBuilder.IsStale()` is this same comparator, a code change to any of those (say `_layers = 6`, or a NavMeshSurface agent type, or making walled-face doors start disabled) rebuilds *nothing*: the template is judged current, every cube is judged current, and `tools/verify-generated.sh` prints "OK" over stale artefacts. The remark at `CubeEquivalence.cs:22-23` admits the list must grow by hand, which is exactly the silent failure `SceneSignature.cs:20-28` was written to avoid. CLAUDE.md's Status calls this "#66", but the concrete gap is wider than "a hand edit": it also hides *generator* changes, which is the case `verify-generated.sh` exists for.
  *Fix:* replace the hand list with the property walk that already exists: `CubeEquivalence.Matches(built, existing) => existing != null && SceneSignature.Of(new[]{built}) == SceneSignature.Of(new[]{existing})`. `SceneSignature.Reference` already maps in-hierarchy references to paths and assets to GUID+localId, so prefab fileIDs do not leak in; `Ignored` already drops the prefab bookkeeping properties. Keep `CubeEquivalenceTests` and add one that mutates `FogDoorVisual._layers` and one that changes a `BoxCollider.size` on a shell piece, both expected `Matches == false`.

- **R2 [Medium] Every fog door does full per-frame work whether or not anything changed, on all six sockets including walled faces and dissolved doors** — `Lucid/Assets/_Lucid/Runtime/FogDoorVisual.cs:54, 80-97, 133-156`.
  `LateUpdate() => Refresh()` and `Refresh()` unconditionally calls `Apply()`, which for each of the (default 4) layers does `GetPropertyBlock` + seven `SetFloat/SetColor` by **string name** + `SetPropertyBlock` (`:146-154`). With the template putting a `FogDoorVisual` on all six sockets (`CubeTemplateBuilder.cs:115`, "walled faces included"), that is 24 property-block rebuilds and ~170 string hashes per cube per frame while nothing is transitioning, and 24 unbatched transparent draws per cube (the remark at `:16-18` accepts the draws for *animating* mist, but the stack stays enabled for `Attached` at `Density 0 / Dissolve 1` (`FogDoorLook.cs:57-58`) and for walled faces where the mist sits inside a 0.3 m wall). A 40-cube dream is ~1000 property-block writes and ~1000 transparent draws a frame for doors that are not visible.
  *Fix:* (1) cache `static readonly int TintId = Shader.PropertyToID("_Tint")` etc.; (2) in `Refresh`, return early when `_rendered.Matches(_door.State) && _door.Progress >= 1f && _applied` (set `_applied` after `Apply`, clear on state change); (3) after a transition ends in `Attached`, set `renderer.enabled = false` for the stack (and re-enable on any later state); (4) let the builder or M0.6's wiring disable `FogDoor`/`FogDoorVisual` on sockets whose `Connector.IsDoorway` is false, or have `FogDoorVisual.EnsureReady` read the sibling `Connector` on its parent and not build the stack for a walled face.

- **R3 [Medium] `SceneSignature` cannot tell two same-typed components on one object apart, and never sees scene-level settings** — `Lucid/Assets/_Lucid/Editor/Scenes/SceneSignature.cs:233-246, 64`.
  `Reference()` renders a component reference as `"scene:" + Path(c.transform) + ":" + c.GetType().Name`. `FogDoor` serializes two `BoxCollider` references on the same GameObject (`FogDoor.cs:66-67`), so `_barrier` and `_wake` both sign as `scene:.../FogDoor-Fog:BoxCollider`; swapping which collider is the trigger is invisible to the signature except through the side effects `ApplyState` happens to leave. Separately, `Of()` walks only root GameObjects, so `RenderSettings` (fog, ambient, skybox), `LightmapSettings` and NavMesh data are outside the signature: a generator that starts setting `RenderSettings.fog` stops rewriting its scene (CLAUDE.md's Status records this as #67 and the `m_Fog` demonstration). Also `Components()` at `:270-271` drops `null` entries, so a "missing script" component signs the same as no component.
  *Fix:* for the reference case, append the component's index among siblings of the same type (`Array.IndexOf(c.GetComponents(c.GetType()), c)`); for scene settings, sign a `SerializedObject` over `RenderSettings`/`LightmapSettings` (`UnityEditor.SceneManagement.EditorSceneManager` exposes them via `Unmanaged` objects; `new SerializedObject(RenderSettings...)` is obtainable with `Resources.FindObjectsOfTypeAll<RenderSettings>()` filtered by scene) and append it to `Of`; count null components explicitly (`"C <path> <missing>"`).

- **R4 [Medium] The `SleeperMotor` mantle probe silently switches itself off when more than eight colliders overlap the foot box** — `Lucid/Assets/_Lucid/Runtime/Sleeper/SleeperMotor.cs:24, 178-180, 192-194, 207-213`.
  `_overlap` is `new Collider[8]`; `Physics.OverlapBoxNonAlloc`/`OverlapCapsuleNonAlloc` truncate at the buffer size and return 8 with no signal that more were present. `NothingButOurselves` then only inspects the eight it got. In a chicane with many small colliders (grates, props, kill volumes are triggers and ignored, but rubble/steps are not) the ninth collider — possibly the one lip that should stop the mantle — is never seen, and the remark at `:203-205` describes exactly this failure mode ("the no-mantle rule would switch itself off in silence") for a different cause. `HasRoomToStand` has the same truncation and would let a Sleeper stand up into a ceiling.
  *Fix:* treat `hits == _overlap.Length` as "obstructed / unknown" and either grow the buffer (`Collider[32]`) and re-query, or return `false` from `HasFootRoomAt`/`HasRoomToStand` when the buffer is full. Add a PlayMode test that surrounds the foot with nine thin boxes and asserts the 1.4 m ledge still fails.

- **R5 [Medium] `FogDoor.Configure(face)` after `Build()` leaves colliders sized for the previous face** — `Lucid/Assets/_Lucid/Runtime/FogDoor.cs:162, 176-203`.
  `Build()` returns early once `_barrier` and `_wake` exist and is the only place `OpeningSize(_face)`/`OpeningCentre(_face)` are applied. `Configure` only assigns `_face`. Today every caller configures before `Awake` (the template in edit mode, `CubeValidatorTests.Valid`), so it is latent, but the first runtime caller — M0.6 building a door and then telling it its face, or any test doing `AddComponent<FogDoor>()` (Awake runs) followed by `Configure(Face.Up)` — gets a 2.5×3 doorway collider lying in a 2.5×2.5 floor hole, which is the exact bug the remark at `:38-45` says was fixed.
  *Fix:* have `Configure` re-apply size and centre when the colliders exist: `internal void Configure(Face face) { _face = face; if (_barrier != null) { _barrier.size = _wake.size = OpeningSize(face); _barrier.center = _wake.center = OpeningCentre(face); } }`, and add an EditMode test that builds, then configures `Face.Up`, and asserts `_barrier.size.y == CubeMetrics.VerticalHole`.

- **R6 [Low] `GetComponent<ISleeperInputSource>()` runs every frame on any body without a source** — `Lucid/Assets/_Lucid/Runtime/Sleeper/SleeperMotor.cs:48, 72` and `SleeperLook.cs:35`.
  `EnsureReady()` is called from `Update` and from every `Tick`; with `_source == null` it does an interface-typed `GetComponent` each call. Every PlayMode rig (`SleeperRig.Create` adds no source) and any body whose source is added late pays this per frame, and `_source == null` on an interface reference does not use Unity's destroyed-object comparison, so a destroyed `SleeperInputSource` keeps being read (Unity returns a fake-null object whose `Read` throws once the native side is gone).
  *Fix:* resolve once with a `_sourceResolved` flag and provide `internal void Bind(ISleeperInputSource)` that `SleeperRig`/the scene builder call after `AddComponent<SleeperInputSource>()`; compare with `_source is Object o && o == null` when the source is a `MonoBehaviour`.

- **R7 [Low] A frame hitch at an exit can skip the wake trigger** — `Lucid/Assets/_Lucid/Runtime/FogDoor.cs:33, 46-48`; `SleeperMotor.cs:46-50`.
  The wake box is `Depth = 0.25` m and the body moves in `Update` by `RunSpeed * Time.deltaTime`; PhysX only sees the positions the body occupies at frame boundaries. Overlap with the capsule (radius 0.4) lasts 0.25 + 2·0.4 = 1.05 m of travel, so a single frame with `dt > 0.175 s` (Unity caps `Time.deltaTime` at `maximumDeltaTime = 0.333 s`, i.e. up to 2 m of travel) carries the Sleeper straight through without `OnTriggerEnter`. Hitches of that size are exactly what a cube instantiation on placement produces. `FogDoorTests.WalkInto` steps at 1/60 s so cannot see this.
  *Fix:* make the wake volume deeper than the barrier (e.g. `_wake.size.z = 1.5f`, centred half inside the cube), or detect the wake in `SleeperMotor` via `CollisionFlags`/a `Physics.BoxCast` along the frame's step instead of relying on a trigger enter.

- **R8 [Low] Static `Mesh`/`Material` are created without `HideAndDontSave` and are built in EditMode by the tests** — `Lucid/Assets/_Lucid/Runtime/FogDoorVisual.cs:38-39, 169, 177`.
  `FogDoorPlaybackTests` calls `visual.Refresh()` in EditMode (e.g. `FogDoorPlaybackTests.cs:82, 93`), which runs `Build()` and instantiates `new Material(shader)` and `new Mesh` with default hide flags in the editor. Unity reports these as leaked objects on the next scene save/play transition and they survive as statics until a domain reload; they are also the only static mutable state in Runtime besides the sanctioned `Services` locator (CLAUDE.md Conventions).
  *Fix:* set `hideFlags = HideFlags.HideAndDontSave` on both and destroy them in a `[RuntimeInitializeOnLoadMethod]`/domain-reload hook, or move them into a small `MistResources` ScriptableObject referenced by the door.

- **R9 [Low] `CubeBuilder` passes a result through a static field** — `Lucid/Assets/_Lucid/Editor/Cubes/CubeBuilder.cs:148, 165-177, 93`.
  `WriteDefinition` sets `static bool _definitionChanged` and `BuildFromSpec` reads it back; a re-entrant or interleaved build (the command builds a whole pack in a loop) reports the previous cube's value if `WriteDefinition` throws before assigning. It is also the kind of hidden state the rest of the builder is careful to avoid.
  *Fix:* return a tuple/out parameter: `static CubeDefinition WriteDefinition(..., out bool changed)`.

- **R10 [Low] `CubeValidator` reports one stray piece twice and mis-sizes non-box colliders** — `Lucid/Assets/_Lucid/Editor/Cubes/CubeValidator.cs:285-289, 291-299`.
  `Occupants` yields every `Renderer` *and* every `Collider`, so a primitive outside bounds produces two identical "bounds" problems (the test at `CubeValidatorTests.cs:94` only looks at `Problems[0]`). `LocalBounds` handles `MeshFilter`, `BoxCollider` and `SphereCollider`; a `CapsuleCollider` or a `MeshCollider` without a `MeshFilter` on the same object falls to `new Bounds(Vector3.zero, Vector3.one)` and passes any bounds check.
  *Fix:* de-duplicate by GameObject in `Occupants` (or take `Collider.bounds` once per object), and either add `CapsuleCollider`/`MeshCollider.sharedMesh.bounds` arms or report "unsupported collider type" as a problem rather than assuming a unit cube.

- **R11 [Low] `SleeperRig.Create` with a vertical `facing` feeds `LookRotation` a zero vector** — `Lucid/Assets/_Lucid/Runtime/Sleeper/SleeperRig.cs:24-26`.
  The guard checks `facing.sqrMagnitude > 0f` but then flattens to `(facing.x, 0, facing.z).normalized`; for `Vector3.up` that is `Vector3.zero` and `Quaternion.LookRotation` logs "Look rotation viewing vector is zero" and returns identity.
  *Fix:* guard on the flattened vector: `var flat = new Vector3(facing.x, 0f, facing.z); if (flat.sqrMagnitude > 1e-6f) rotation = LookRotation(flat.normalized, up);`.

- **R12 [Nit] Unused assembly references** — `Lucid/Assets/_Lucid/Runtime/Lucid.Runtime.asmdef:6-7` lists `Unity.AI.Navigation` and `Unity.RenderPipelines.Universal.Runtime`; `grep -rn "Rendering.Universal\|Unity.AI\|UnityEngine.AI" Runtime/` finds no use (the `NavMeshSurface` lives in the Editor template). CLAUDE.md's rule is "Runtime → Core"; the extra references are harmless today but widen what Runtime may quietly grow into.
  *Fix:* drop both until a Runtime type needs them (the shader uses the URP include path, not the assembly).

- **R13 [Nit] Documentation names a type that does not exist** — CLAUDE.md "Repository map" and the task brief list `AssetNormalizer` in `Lucid.Editor`; `ls Lucid/Assets/_Lucid/Editor/Cubes` has no such file (46 files, none normalising assets). Related: `CubeBuilder.Unhandled` (`CubeBuilder.cs:133-145`) reports `props`/`chicane`/`lighting`/... as ignored, so no asset is ever imported yet.
  *Fix:* remove `AssetNormalizer` from the map or mark it "planned (M1)".

- **R14 [Nit] Nested helper types versus "one type per file"** — `FogDoorVisual.cs:198` (`ConnectorStateCache`), `ShellBuilder.cs:162` (`Bounds3`), `CubeBuildResult.cs:39` (`Builder`, whose setters mutate an already-constructed result). All private/internal and small; noting only because CLAUDE.md states the convention flatly.
  *Fix:* none required; if the convention is meant literally, `ConnectorStateCache` collapses to a `ConnectorState?` field and `Bounds3` could be `Editor/Cubes/Bounds3.cs`.

- **R15 [Nit] `SleeperInput.Move` is documented as clamped but nothing clamps it** — `Lucid/Assets/_Lucid/Runtime/Sleeper/SleeperInput.cs:13-14`; the motor normalises only when `sqrMagnitude > 1` (`SleeperMotor.cs:88`), which is the right behaviour, so the comment is the thing to change ("normalised by the motor when longer than 1").

- **R16 [Nit] `SleeperReachTests.Cleared` leaks its scene on an exception** — `Tests/PlayMode/Sleeper/SleeperReachTests.cs:27-41` destroys the motor and gauntlet after the run, but not in a `try/finally`; a throw inside `RunForward` leaves a gauntlet and a camera behind for the next test. `GauntletTests`/`SleeperMotorTests` use `_spawned` + `[TearDown]` correctly.
  *Fix:* wrap in `try { … } finally { DestroyImmediate(...) }`.

---

**Things checked that are not findings (so the coordinator need not re-check):**
- `Lucid/Mist` *is* in `ProjectSettings/GraphicsSettings.asset` `m_AlwaysIncludedShaders` (guid `e439ae66…` = `Mist.shader.meta`), so `Shader.Find` at `FogDoorVisual.cs:162` survives a player build.
- The committed `FogDoors.unity` contains no serialized "Mist"/"Layer" children (Awake does not run in edit mode), so the quad stack is not duplicated on load; the four frozen doors carry their `_barrier`/`_wake` references and the cycler carries `{fileID: 0}` and builds at Awake, both deterministic across rebuilds.
- `FogDoor.Build()` adoption logic (`:185-190`) is correct because the template puts the door on its own child (`CubeTemplateBuilder.cs:108`), so no foreign `BoxCollider` can be adopted.
- Editor scripts write only via `PrefabUtility.SaveAsPrefabAsset`/`EditorSceneManager.SaveScene` behind a comparator, use only built-in resources (`Default-Material`, `LegacyRuntime.ttf`), and never touch YAML text; `GeneratedScene.Write` reopens the committed scene when a populate throws.
- `run-tests.sh` scoping (`-assemblyNames Lucid.Tests.PlayMode`) matches the asmdef name exactly; `UNITY_INCLUDE_TESTS` keeps `Lucid.Tests.PlayMode` (`includePlatforms: []`) out of player builds.

---

# Part 3 — Tooling / process / docs (T)


Scope: `tools/*.sh`, `tools/check-licenses.py`, `tools/hooks/`, `tools/install-hooks.sh`, `.gitignore`, `.gitattributes`, `Lucid/Packages/manifest.json` (+ lock), `ProjectVersion.txt`, all six asmdefs, `THIRD_PARTY_NOTICES.md`, `Packs/**` (no `assets/`, `LICENSES.md` or `assets.manifest.json` exists yet in any cube), `docs/*.md`, `CLAUDE.md`, `README.md`, `CONTRIBUTING.md`, `.github/`, cross-checked against the tree, `git log` and `gh issue/pr list`. Every finding below was reproduced against the files quoted; nothing was modified and Unity was not run.

Clean categories, stated plainly: all five shell scripts have `set -euo pipefail` and quote every variable; the asmdef graph is exactly the rule (Core → nothing with `noEngineReferences: true`; Runtime → Core; Netcode → Runtime, Core; Editor → everything; Tests → what they test); every package in `manifest.json` is either from the Unity 6 URP template (commit 9cb063d) or has a DECISIONS entry (NGO, Addressables, MCP bridge, Newtonsoft); `packages-lock.json` agrees with the manifest; every committed file under `Assets/_Lucid` has its `.meta` and no `.meta` is orphaned; all 15 PNGs are LFS pointers; no `.DS_Store`, `.csproj` or test output is tracked; `docs/CORE-API.md` §12's thirteen bullets each have a named test (hysteresis, insertion-order hash, `TrappedSleeper`, trickle remainder, 50-event replay hash all present); `run-tests.sh`'s stale-results / no-log / `error CS` / `Aborting batchmode` / `total == 0` guards are all real and in the right order; `CubeBuildCommand` and `GeneratedScenes` both `EditorApplication.Exit(code)` with a non-zero code on failure so the wrappers' `$code` check is meaningful.

---

## High

- **T1 [High] `tools/check-licenses.py:21` and `Lucid/Assets/_Lucid/Editor/Cubes/CubeValidator.cs:239` — the licence regex accepts CC-BY-NC / ND / SA and rejects the canonical "CC BY 4.0".**
  Both gates use `\bCC0\b|\bCC-?BY\b`. Because `\b` matches between `Y` and `-`, `CC-BY-NC 4.0`, `CC-BY-ND`, and `CC-BY-SA 4.0` all satisfy it (reproduced: `True | a.png | url | CC-BY-NC 4.0 |`). NonCommercial and NoDerivatives are exactly the licences rule 5 and `docs/SPEC.md` §18 exist to keep out of a public MIT repository, and the ledger line is the only thing the gate reads. The converse is also wrong: Creative Commons' own spelling is `CC BY 4.0` with a space, and that is rejected (`False | a.png | url | CC BY 4.0 |`; end-to-end run of the script printed `ledger entry for 'wall.png' is not CC0 or CC-BY`). `LicenceRuleTests.ANonRedistributableLicenceIsRejected` only tests the Asset Store EULA string, so the NC hole has no test. Both gates carry the same regex "so the two agree", which means they agree on the same bug.
  *Fix:* in both places use an anchored, explicit form that permits only a bare attribution licence, e.g. `(?i)\bCC0\b|\bCC[- ]BY(?:[- ]?\d\.\d)?\b(?![- ]?(?:NC|ND|SA)\b)`, and add a test on each side with `CC-BY-NC 4.0` (must reject) and `CC BY 4.0` (must accept). Keep the two regexes literally identical and cite each from the other.

- **T2 [High] Process: #66, #67 and #72 are closed as COMPLETED with no fix in the tree, while `CLAUDE.md` and `tools/verify-generated.sh:17-23` still describe them as the open work that will close the comparators' blind spots.**
  `CLAUDE.md` Status: "`verify-generated.sh` … is exactly as strong as the two comparators … `CubeEquivalence` (#66) and `SceneSignature` (#67, #72) — so a hand edit to a field neither compares survives it". `tools/verify-generated.sh:22-23`: "The gap is the comparators', and closing it is what #66, #67 and #72 are for." `gh issue view` shows all three `CLOSED`, `stateReason=COMPLETED`, `closedByPullRequestsReferences=[]`, closed 2026-09-03 08:28 / 08:28 / 11:01. No commit mentions them (`git log --all --grep` returns only the #65/#71/#73 commits, which predate or do not touch them). The code confirms nothing changed: `SceneSignature.cs:139` still walks `property.NextVisible(true)` (the #72 hole), `SceneSignature.Of` still takes root GameObjects only (#67), and `CubeEquivalence.cs` was last touched by #52. So the blind spots the docs warn about are real, and nothing tracks them any more — the worst combination for a check whose whole value is knowing where it is weak. Same shape for #70: closed COMPLETED, and `grep -n timeout tools/run-tests.sh` still finds nothing, so the runner's "no timeout" half of that issue was closed without landing.
  *Fix:* either reopen #66/#67/#72 (and the timeout half of #70) or file one replacement issue and cite it; then update the `CLAUDE.md` Status paragraph and the `verify-generated.sh` header to point at whatever is actually open. Closing an issue without a PR reference should say why in a comment; these have none.

## Medium

- **T3 [Medium] `tools/check-licenses.py:20` — the hook only guards `Packs/<p>/Cubes/<c>/assets/`; skins, mobs and every other asset location are unchecked.**
  `ASSET_DIR = re.compile(r"(?P<cube>(?:.*/)?Packs/[^/]+/Cubes/[^/]+)/assets/(?P<rel>.+)")`. `CLAUDE.md`'s map says `Packs/<Pack>/` holds "`Cubes/<Name>/` folders, `Skins/`, `Mobs/`", and M1.10 (#23) and M2 will put textures and meshes there; nothing under `Templates/`, `Scenes/` or a future `Skins/` will ever reach the ledger check. The hook therefore cannot deliver rule 5's "Only CC0 / CC-BY assets are committed" — only the per-cube subset of it.
  *Fix:* match any binary asset extension (the LFS list in `.gitattributes` is a ready-made definition) anywhere under `Lucid/Assets/`, and require a ledger at the nearest `assets/LICENSES.md` or, failing that, a line in `THIRD_PARTY_NOTICES.md`. Decide the skin/mob ledger location now and record it in DECISIONS.

- **T4 [Medium] `tools/check-licenses.py:108-110` — every sub-folder `.meta` inside `assets/` is a false positive.**
  A folder `assets/textures/` carries `assets/textures.meta`; the hook strips `.meta`, gets `textures`, finds no ledger token and blocks the commit (reproduced: `assets/textures.meta: no entry for 'textures' in .../LICENSES.md`). `LicenceRuleTests.AnAssetInASubfolderIsStillChecked` proves sub-folders are the intended layout, so the first cube that uses one will either be blocked or teach its author to add a bogus `| textures | - | CC0 |` line to get past the gate.
  *Fix:* when `name.endswith(".meta")` and `(cube/"assets"/rel[:-5]).is_dir()`, skip it (`CubeValidator.cs:218` already skips all `.meta`s, so the two gates currently disagree here too — the test file's remark says they must agree).

- **T5 [Medium] `.gitignore:44-47` — the Addressables patterns are root-anchored and never match the actual project path; the header comment says the opposite.**
  Lines 2-3: "Patterns are unanchored … so they match whether the Unity project folder is `lucid/` or `Lucid/`." Lines 45-46: `/[Aa]ssets/[Aa]ddressable[Aa]ssets[Dd]ata/*/*.bin*` and `/[Aa]ssets/[Ss]treamingAssets/aa/*`. The leading `/` anchors them to the repository root, but the project lives at `Lucid/Assets/`. Reproduced: `git check-ignore -v Lucid/Assets/AddressableAssetsData/x/catalog.bin Lucid/Assets/StreamingAssets/aa/foo` matches nothing (only `ServerData/` and `Library/` matched). `com.unity.addressables` 2.11.2 is installed, so the first content build will offer multi-megabyte bundles and catalogues for commit. `!/[Aa]ssets/**/*.meta` on line 17 is dead for the same reason (harmless today, since nothing ignores `.meta`).
  *Fix:* drop the leading `/` (or prefix `[Ll]ucid/`) on lines 17, 45 and 46; add a `git check-ignore` assertion to whatever tooling test eventually covers `.gitignore`.

- **T6 [Medium] Three documents disagree on where a third-party asset's licence line lives.**
  `CLAUDE.md` rule 5: "each with a line in that cube's `assets/LICENSES.md` **and in the root `THIRD_PARTY_NOTICES.md`**". `THIRD_PARTY_NOTICES.md:6`: "Per-cube art assets are **not** listed here." `docs/SPEC.md` §18: "third-party assets under their own licenses, listed in `THIRD_PARTY_NOTICES.md`." The hook checks only the per-cube ledger, so the rule as CLAUDE.md states it is unenforced, and the rule as THIRD_PARTY_NOTICES states it contradicts the spec.
  *Fix:* pick one (the per-cube ledger, with `THIRD_PARTY_NOTICES.md` pointing to them, is what the tooling implements), edit `CLAUDE.md` rule 5 and SPEC §18 to match, and note it in DECISIONS since §18 is **[S]**.

- **T7 [Medium] `docs/WORKPLAN.md:19` still forbids what `CLAUDE.md` rule 1 and SPEC §18 now require.**
  WORKPLAN §1 item 4: "The owner reviews, requests changes or merges (squash). **Claude never merges its own work** and never pushes to `main`." `CLAUDE.md` rule 1 (830f736): "On that word — naming the PR, for that PR — merge it yourself with `gh pr merge <n> --squash --admin --delete-branch`." The 2026-09-03 DECISIONS entry says only "`docs/WORKPLAN.md` §1's branch-protection line still describes the setting", which is true of line 88 (M0.1) but not of line 19, and WORKPLAN §1.1 ranks WORKPLAN above CLAUDE.md as a source of truth, so a reader following the stated precedence gets the old rule. `.claude/skills/pr-review/SKILL.md:145` ("Stop there: the owner reviews and merges") has the same drift, milder.
  *Fix:* edit WORKPLAN §1 item 4 to the same sentence as SPEC §18 and cite the DECISIONS entry; touch SKILL.md's last line.

- **T8 [Medium] `README.md:9-11` — "Design complete, implementation not started. Milestone M0 is next."**
  Twenty-two PRs are merged, M0.1–M0.5 are closed, and the README is the first thing a visitor reads. WORKPLAN §3 defers the README to milestone end, but "implementation not started" is false today, not merely incomplete, and CONTRIBUTING.md points people at `tools/run-tests.sh` as if the README were current.
  *Fix:* one paragraph mirroring CLAUDE.md's Status (what runs, what does not), updated in each milestone-closing PR as §3 already requires.

## Low

- **T9 [Low] `tools/verify-generated.sh:33` documents exit codes it cannot keep.** "Exit codes: 0 nothing changed; 1 something drifted; 2 the check could not run." A generator failure propagates `build-cube.sh`'s own exit code through `set -e` (1 for a validator failure, 2 for "no cube.spec.json"), so a broken generator reports as "drifted" and a bad target as "could not run". *Fix:* wrap each generator call: `tools/build-cube.sh "$pack" || exit $CANNOT_CHECK` (or a distinct 3 for "generator failed").

- **T10 [Low] `tools/verify-generated.sh` reports false drift on an unsmudged LFS checkout.** `CubePreviewRenderer.cs:257` compares the rendered PNG bytes to the file on disk; with `git lfs` not installed the file is a 130-byte pointer, so every preview is "different", gets rewritten, and `git status` shows 15 modified PNGs with no hint why. The pr-review skill names "LFS unsmudged" as a lens, so this is anticipated but not handled. *Fix:* before building, `git lfs ls-files --long` or a `head -c 40 | grep -q 'version https://git-lfs'` probe on one preview, and exit `$CANNOT_CHECK` with "run `git lfs pull`".

- **T11 [Low] `tools/install-hooks.sh:10` assumes `.git` is a directory; it fails inside a git worktree.** `target="$root/.git/hooks/pre-commit"` — in a linked worktree `.git` is a file, so `cat > "$target"` fails with "Not a directory". This repository is worked on with worktrees (the local branch list and the Agent tool's `isolation: worktree` both show it). *Fix:* `target="$(git rev-parse --git-path hooks)/pre-commit"` (resolves to the shared hooks dir in both layouts); also refuse to overwrite an existing non-shim hook rather than clobbering it silently.

- **T12 [Low] `tools/install-hooks.sh:17` — the shim turns the gate off silently when the hook loses its executable bit.** `[ -x "$hook" ] || exit 0`. `core.fileMode=false` (Windows, some network filesystems, a zip download) makes every commit pass. The comment defends the "absent" case; the "present but not executable" case is a different failure and should not be quiet. *Fix:* `[ -e "$hook" ] || exit 0; exec bash "$hook" "$@"` — run it via the interpreter so the bit does not matter, and fail if it exists but cannot run.

- **T13 [Low] Editor-lock guard is duplicated verbatim in three scripts and races the launch.** `run-tests.sh:47-65`, `build-cube.sh:40-58`, `build-scenes.sh:39-57` are byte-identical 19-line blocks (plus the identical 20-line editor-discovery block above each). The #68 fix had to be applied three times; the next one will be missed once. The guard also needs *both* the lockfile and a `-projectpath` process, so an editor opened by double-clicking a scene (no `-projectpath` on its command line) or via a symlinked clone (`pwd -L` vs the Hub's physical path) passes it and only the downstream `Aborting batchmode` grep catches it — which is fine as a backstop but is not what the comment promises. *Fix:* extract `tools/lib/unity.sh` with `find_unity` and `refuse_if_editor_holds_project`, sourced by all three; match on the lockfile alone plus a process whose command line contains the project path *or* `Unity.app` with the lockfile mtime newer than boot, or simply document that the abort grep is the real guard.

- **T14 [Low] `tools/check-licenses.py:37-42` — deletions and edits to the ledger are not re-checked.** The hook lists `--diff-filter=ACMR` staged paths and judges only assets among them. Deleting an asset's line from `LICENSES.md`, or adding an already-committed asset to `assets.manifest.json`, is a staged change to a non-asset file and passes; the tree is then in violation with the gate green. *Fix:* when a `LICENSES.md` or `assets.manifest.json` is staged, re-check every tracked file under that cube's `assets/` (`git ls-files <cube>/assets`).

- **T15 [Low] `docs/DECISIONS.md:54` cites "the `lucid/` versus `Lucid/` project folder in `docs/DECISIONS.md`'s earlier history" — there is no such entry.** `git log --all -p -- docs/DECISIONS.md | grep 'lucid/\`'` finds only this sentence. The defect it refers to is visible elsewhere (`docs/WORKPLAN.md:30` still draws the root as `lucid/` while the repo is `Lucid`, and `.gitignore:2-3` explains the case-bracketing for the same reason). *Fix:* point at WORKPLAN §2 / `.gitignore` instead, or write the missing entry.

- **T16 [Low] `docs/WORKPLAN.md` §2 layout is stale in five places and ranks above `CLAUDE.md` as a source of truth.** Line 54: `Templates/ CubeTemplate.prefab, FogDoor.prefab, StartCube` — CLAUDE.md says there is no `FogDoor.prefab` and the start cube lives in the pack (true: `Packs/core/Cubes/start/`). Lines 59-65 omit `build-scenes.sh` and `verify-generated.sh` and list three scripts that do not exist. No `Scenes/` entry. Line 30 `lucid/`. `CubeBuildCommand.cs:42-43` says "The template is generated, so a fresh clone has none" while `Templates/CubeTemplate.prefab` is committed. *Fix:* regenerate §2 from `git ls-files` once per milestone, or delete it and point at CLAUDE.md's map (which is current).

- **T17 [Low] `CLAUDE.md` repository map names `AssetNormalizer` as living in `Lucid.Editor`; no such file exists.** `grep -rn AssetNormalizer Lucid/Assets/_Lucid` returns nothing; SPEC §17 describes it as future work. The other forward-looking names in the map (`DreamInstance`, the Netcode classes) are covered by the Status paragraph saying they are stubs; `AssetNormalizer` is not. *Fix:* mark it "(planned, SPEC §17)" or drop it until it exists.

- **T18 [Low] `THIRD_PARTY_NOTICES.md:24` — "com.unity.nuget.newtonsoft-json | 3.2.2 (transitive)".** DECISIONS 2026-08-25 made it a direct dependency and `manifest.json` lists it; the lock shows `depth 0`. Also the table header claims every package is under the Unity Companion License "unless the package states otherwise"; Newtonsoft.Json is MIT. *Fix:* drop "(transitive)" and add a Licence column.

- **T19 [Low] `.gitignore:57-60` — "The download destination is settled when that tool is written (M0.3); add its pattern here then."** M0.3 is merged and `fetch-assets.py` was not written (CLAUDE.md Status says so). The comment sends a reader to a milestone that already passed. *Fix:* reference CLAUDE.md's Status / the issue that will own the fetcher, or pick the destination now (`assets/fetched/` is the obvious one) and ignore it.

## Nit

- **T20 [Nit] `tools/run-tests.sh:136-158`** requires `python3` but nothing checks for it; under `set -e` a missing interpreter exits 127 with "command not found", which is loud enough, but the usage header could say so.
- **T21 [Nit] `tools/build-scenes.sh:87`** greps for `^(gauntlet|fogdoors|scenes): `; a third generated scene will print a line the wrapper silently drops. Consider a shared prefix (`scene: <name> …`) so the grep does not need editing per scene.
- **T22 [Nit] `.github/PULL_REQUEST_TEMPLATE.md`** and CLAUDE.md's template differ slightly ("Why (issue #, spec §)" vs "Why (Closes #N — the keyword …)"); the GitHub one is what a contributor sees and lacks the closing-keyword reminder that rule 1 depends on.
- **T23 [Nit] `CONTRIBUTING.md:23-25`** says the UTP path "runs the whole game on localhost or LAN, and Sandbox mode runs it with no second player"; neither exists yet. It is labelled a stub, so this is a note, not a defect.
- **T24 [Nit] `.gitattributes`** LFS list omits `.glb`/`.gltf`/`.hdr`, which Poly Haven and Sketchfab (the sources SPEC §17 recommends) hand out; the first such asset will land in plain git.
