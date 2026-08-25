# CLAUDE.md — Lucid

Lucid is an asymmetric online party game: one **Nightmare** builds a dream out of cubes in real time, 1–4 **Sleepers** each run the same maze alone to reach the deepest edge before dawn. Unity 6, URP, Netcode for GameObjects, Steam. MIT.

Read in this order before doing anything: `docs/SPEC.md` (what the game is), `docs/UI.md` (every screen, key and string), `docs/CORE-API.md` (the rules engine, implement it as written), `docs/CUBE-SPEC.md` + `docs/cube-spec.schema.json` (the cube format), `docs/CHICANES.md` (trap components, their laws and tests), `docs/NETCODE.md` (every message on the wire), `docs/WORKPLAN.md` (what to do next and when it is done), `docs/DECISIONS.md` (recorded deviations). If code and spec disagree, the spec wins unless DECISIONS says otherwise.

## Status

**M0.1 through M0.4 are merged; M0.5 (#5) is next** (`docs/WORKPLAN.md` §4). Unity **6000.3.11f1** at `Lucid/`.

- `Lucid.Core` implements `docs/CORE-API.md` in full — lattice, derivation, the placement and exploration rules, round, budget, powers and scoring. Every item of its §12 test list is covered.
- The cube pipeline runs end to end: `tools/build-cube.sh core` builds Straight, Corner, T, Cross and the Bedroom from `cube.spec.json`, validates each, and renders three previews apiece. Rebuilding changes nothing on disk.
- The Sleeper moves. `SleeperMotor` is the whole kit of `docs/SPEC.md` §9 and nothing else, and `tools/build-gauntlet.sh` writes the course its tests measure it on. Gravity is **derived** from the spec's rise and reach rather than set, which lands at 2.4 g; movement refuses to climb above its own feet, because a `CharacterController` mantles by itself (`docs/DECISIONS.md`).
- Still no fog-door behaviour, no dream instance, no networking. M0.5 onwards builds those.
- **PlayMode works and is proven to fail when it should** — before M0.4 the platform had never run a test, so `0/0 passed` and "nothing ran" looked identical. It carries the Sleeper's tests now; `Lucid.Netcode` remains a stub.

Things the tree does not tell you:

- The editor path comes from `UNITY_PATH`. `tools/run-tests.sh`, `tools/build-cube.sh` and `tools/build-gauntlet.sh` all refuse to run while the editor holds the project, so ask the owner to close it rather than working around them.
- `build-cube.sh` deliberately omits `-nographics`: previews need a graphics device, and the renderer degrades to writing no images rather than failing.
- Driving Unity through the MCP bridge instead? Read the console for compile errors between every refresh and test run — the bridge has no compile guard and will happily run stale assemblies (`.claude/skills/pr-review/SKILL.md` §3).
- The git remote is HTTPS. This machine has two GitHub accounts and the SSH key is the wrong one.
- Steam is deferred, not blocked; see the pinned #28.

## Rules that do not bend

1. **One issue, one branch, one pull request.** Branch `m<N>/<NN>-<slug>`. Conventional commits. Open the PR and stop; the owner reviews and merges. Never push to `main`, never merge your own PR.
2. **Spec items marked [S] are settled.** Do not reinterpret them. Ambiguity → comment on the issue, label it `question`, move on.
3. **`Lucid.Core` is pure C#.** No `UnityEngine` anywhere in `Assets/_Lucid/Core/`. Every rule (fit, frontier, depth, exits, explored, leak, budget) is a pure function with a test. Determinism is part of the contract: no floating point in rules, time in integer milliseconds, sets iterated in a defined order — two machines must derive a bit-identical `Derived.Hash` (`docs/CORE-API.md` §1, §11).
4. **Never hand-edit prefab or scene YAML.** Cubes come from `CubeBuilder`; scenes are set up by editor scripts. Commit the generated files.
5. **The asset rule.** Only CC0 / CC-BY assets are committed, each with a line in that cube's `assets/LICENSES.md` and in the root `THIRD_PARTY_NOTICES.md`. Anything else goes in `assets.manifest.json` and is fetched, never committed. The pre-commit hook enforces this; do not bypass it.
6. **Run `tools/run-tests.sh` before every PR** and paste the summary line into the PR description. There is no CI.
7. **Decisions leave a trace.** Promoting a [D] to [S] or deviating from the spec means editing `docs/SPEC.md` and adding a `docs/DECISIONS.md` entry in the same PR.
8. **Every PR is independently reviewed before it is opened.** Run the `pr-review` skill: two reviewers with different lenses, reproduce each finding before fixing it, mutation-check every test you touched, then post the triage on the PR. There is no CI, so an unchecked claim reaches the owner as fact.

## Commands

```
tools/run-tests.sh                 EditMode + PlayMode tests in Unity batch mode
tools/run-tests.sh editmode        Core tests only, fastest loop
tools/run-tests.sh editmode <name> one fixture or test by name, the inner loop
LUCID_ALL_ASSEMBLIES=1 …         include tests that ship inside packages
tools/build-cube.sh <pack>/<cube>  build one cube from cube.spec.json, validate, render previews
tools/build-cube.sh <pack>         rebuild a whole pack (after template changes)
tools/build-gauntlet.sh            regenerate the movement gauntlet scene
tools/fetch-assets.py <cube dir>   download manifest assets and verify hashes
tools/check-licenses.py            what the pre-commit hook runs
tools/playtest-report.py <log>     per-round metrics from a .lucidlog (see docs/playtests/PLAN-M0.md §5)
tools/build-dev.sh                 zipped UTP dev build for a second machine
```

Unity is invoked in batch mode by these scripts: `Unity -batchmode -nographics -quit -projectPath Lucid -executeMethod <Class.Method> -logFile -` plus script-specific arguments. Test runs are the exception: `-runTests -testPlatform EditMode|PlayMode [-testFilter <name>] -testResults <file>` and **no `-quit`**, which would end the editor before results are written. The Unity version is **6000.3.11f1**, pinned in `lucid/ProjectSettings/ProjectVersion.txt`; the editor path is read from `UNITY_PATH` (ask the owner if unset).

## How it fits together

- **Each dream is a single-player simulation on its own Sleeper's machine.** The network carries only the maze, round state, thin telemetry and the Nightmare's actions (spec §14). Traps, mobs, physics, damage and respawns are plain local gameplay — never `NetworkObject`s.
- **The host owns the rules**: lobby, phase and clock, the lattice event log, budget, cooldowns, and adjudication of placement, exploration and waking. It runs `Lucid.Core.Round`; the loop is one page at `docs/CORE-API.md` §10. The Nightmare's client only requests, and applies nothing locally until the host confirms.
- **The lattice is replicated as an ordered event log**, `CubePlaced` and `CubeExplored`, never as per-door state. Every client re-derives depth, exits and solid doors from the log and reports `Derived.Hash` back; a mismatch is a bug and auto-saves a `.lucidlog`. This is why rule 3 exists.
- **Clients never talk to each other.** Spectating and possession look like exceptions but are relays the host performs (`docs/NETCODE.md` §1, §8).

## Repository map

| Path | What lives there |
|---|---|
| `Lucid/Assets/_Lucid/Core/` | `Lucid.Core` — lattice, cube types, event log, rules, `Validate`, `Derive` |
| `Lucid/Assets/_Lucid/Runtime/` | `Lucid.Runtime` — dream instance, Sleeper controller, Nightmare view, fog doors, traps, mobs, UI; `Input/` holds the action maps and `Dev/` the gauntlet, which is in Runtime so the editor script and the PlayMode tests build one course rather than two |
| `Lucid/Assets/_Lucid/Netcode/` | `Lucid.Netcode` — `Approval`, `SessionState`, `RoundSync`, `LatticeMirror`, `DreamRelay`, transports (UTP dev, Facepunch Steam); message IDs from `docs/NETCODE.md` §12 |
| `Lucid/Assets/_Lucid/Editor/` | `Lucid.Editor` — `CubeBuilder`, `CubeValidator`, `AssetNormalizer`, scene setup scripts |
| `Lucid/Assets/_Lucid/Scenes/` | generated scenes; `Gauntlet.unity` from `tools/build-gauntlet.sh` |
| `Lucid/Assets/_Lucid/Templates/` | `CubeTemplate.prefab`, `FogDoor.prefab`, the Start cube |
| `Lucid/Assets/_Lucid/Packs/<Pack>/` | content: `Cubes/<Name>/` folders, `Skins/`, `Mobs/`, the `DreamPack` asset |
| `Lucid/Assets/_Lucid/Tests/` | `EditMode/` for Core and builder, `PlayMode/` for runtime and netcode |
| `docs/` | `SPEC.md`, `UI.md`, `CORE-API.md`, `CUBE-SPEC.md`, `CHICANES.md`, `NETCODE.md`, `WORKPLAN.md`, `DECISIONS.md`, `HISTORY.md`, `cube-spec.schema.json`, `playtests/` (plans, template, session notes, csv) |
| `tools/` | the scripts above |

Assembly references: Runtime → Core; Netcode → Runtime, Core; Editor → everything; Tests → what they test. Core references nothing.

## Making a cube (short form; long form in `docs/SPEC.md` §17)

1. Read `Cubes/<Name>/brief.md` and list `assets/` and `assets.manifest.json`.
2. Write `cube.spec.json` against `docs/cube-spec.schema.json` (worked examples in `docs/CUBE-SPEC.md`): connector mask, climbable, cost, category, shell material roles, props with transforms, chicane component and parameters, weak point, trigger, intended path, skins, nav.
3. `tools/build-cube.sh <pack>/<Name>` → read `Previews/*.png` and the validator report `Previews/report.json`.
4. Fix until the report is clean and the previews look right. Do not tune numbers outside the spec's kit limits (gaps ≤ 3.5 m, ledges ≤ 1.1 m, crawl ≥ 1.1 m).
5. Commit the spec, the generated prefab, the `CubeDefinition`, previews and the license ledger. Never the fetched assets.

A new mechanic = a new component in the chicane library (`docs/CHICANES.md` §8: attribute, params schema, validator, four tests, a tell) in its own PR, before any cube uses it.

## Conventions

- C# 10-ish as Unity allows; namespaces `Lucid.Core`, `Lucid.Runtime`, `Lucid.Netcode`, `Lucid.Editor`; one type per file; no static singletons in Runtime except a small `Services` locator set up by the scene bootstrap.
- Coordinates are `Vector3Int`-shaped `Coord` in Core (x east, y north, z = layer up); Unity world position is `(X·8, Z·8, Y·8)` — the axes swap, because Unity's up is y — and the mapping lives only in Runtime.
- Tests: NUnit. Core tests live in EditMode and run in seconds; keep them that way. PlayMode tests use scripted input, never wall-clock sleeps longer than a few frames.
- Telemetry and RPC payloads are small structs; no strings on the hot path.
- Comments explain *why*; the spec explains *what*.

## PR description template

```
## What
## Why  (issue #, spec §)
## How tested  (paste tools/run-tests.sh summary)
## Generated files touched
## Spec / DECISIONS changes
```

## Do not

- Do not add packages without a DECISIONS entry.
- Do not open Unity in interactive mode to "fix" a prefab by hand.
- Do not start the next milestone while the current one has open issues (unless `parallel-ok`).
- Do not ask for permission on things this file already answers.
