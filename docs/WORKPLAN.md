# LUCID — Work Plan

**Version 1.0 — 2026-08-24 (consolidated).** Companion to `docs/SPEC.md` and the documents it lists in §21; `CLAUDE.md` holds the operating rules.

This is the document Claude Code works from. The spec says *what the game is*; this says *what to build next, in what order, and when it is done*. Sized for one developer at hobby pace: every task is an evening to a weekend; anything larger gets split.






---

## 1. How Claude Code uses this document

1. Sources of truth, in order: `docs/SPEC.md` (design), `docs/WORKPLAN.md` (order and acceptance), `CLAUDE.md` (conventions and commands), `docs/DECISIONS.md` (recorded deviations). If code and spec disagree, the spec wins unless a DECISIONS entry says otherwise.
2. Pick the lowest-numbered open issue in the current milestone. Do not start the next milestone while the current one has open issues, unless an issue is labelled `parallel-ok`.
3. One issue → one branch → one pull request. Branch names `m0/03-cube-pipeline`; conventional commits (`feat:`, `fix:`, `docs:`, `test:`, `chore:`).
4. **Claude opens the pull request and stops.** The owner reviews, requests changes or merges (squash). Claude never merges its own work and never pushes to `main`. Requested changes go on the same branch.
5. Keep pull requests reviewable: one concern, roughly under 500 changed lines of hand-written code; generated prefabs, assets and previews do not count but are listed in the description.
6. The PR description states what changed, why, how it was tested, and which spec sections it touches. If the work promotes a **[D]** to **[S]** or deviates from the spec, edit the spec in the same PR and add a DECISIONS entry.
7. Tests before PR: `tools/run-tests.sh` must pass locally. `Lucid.Core` has a test for every rule; PlayMode tests cover the controller and the dream runtime. There is no CI; the PR description quotes the test summary line.
8. Never hand-edit prefab or scene YAML. Prefabs come from the cube builder; scenes are set up by editor scripts where practical, and the resulting files are committed.
9. Blocked or ambiguous: write the question as an issue comment, label the issue `question`, move to the next issue. Do not guess on **[S]** items.
10. Keep `CLAUDE.md` short and link out to `docs/` for anything long.

## 2. Repository layout

```
lucid/
  README.md                 pitch, how to play, how to build, screenshots later
  LICENSE                   MIT
  CLAUDE.md                 conventions, commands, spec schema pointer, do/don't
  CONTRIBUTING.md           how to make a cube; written for real in M4
  CODE_OF_CONDUCT.md        Contributor Covenant
  THIRD_PARTY_NOTICES.md    every third-party asset and package with license
  docs/
    SPEC.md                 the design spec
    UI.md                   screens, HUDs, keys, copy glossary, edge flows
    CORE-API.md             Lucid.Core types, rules, algorithms, invariants, required tests
    CUBE-SPEC.md            cube.spec.json guide with worked examples
    CHICANES.md             chicane component library: contract, laws, components, mobs, tests
    NETCODE.md              message catalogue: approval, lobby state, round, lattice sync, streams, reconnect
    WORKPLAN.md             this file
    DECISIONS.md            ADR-lite: date, decision, reason, spec sections changed
    cube-spec.schema.json   JSON schema for cube.spec.json
    playtests/              PLAN-M0.md, TEMPLATE.md, one note per session, m0-rounds.csv
  Lucid/                    Unity project (Unity 6, URP)
    Assets/_Lucid/
      Core/                 Lucid.Core.asmdef — pure C#, no UnityEngine references
      Runtime/              Lucid.Runtime.asmdef — dream, sleeper, nightmare, traps, mobs, UI
      Netcode/              Lucid.Netcode.asmdef — NGO glue, transports, telemetry
      Editor/               Lucid.Editor.asmdef — CubeBuilder, CubeValidator, AssetNormalizer, scene setup
      Templates/            CubeTemplate.prefab, FogDoor.prefab, StartCube
      Packs/Core/           the base game's cubes, skins, mobs
      Tests/EditMode/       Core rule tests, builder tests
      Tests/PlayMode/       controller, dream runtime, netcode smoke tests
    Packages/manifest.json
  tools/
    run-tests.sh            Unity batch mode test runner, EditMode + PlayMode
    build-cube.sh           wraps CubeBuilder.BuildFromSpec for one cube or a whole pack
    fetch-assets.py         downloads assets.manifest.json entries, verifies hashes
    check-licenses.py       pre-commit hook: no unlisted or non-redistributable assets
    playtest-report.py      per-round metrics from a .lucidlog, appends to docs/playtests/*.csv
    build-dev.sh            zips a UTP dev build for a friend to run
  .github/
    ISSUE_TEMPLATE/         bug, feature, cube proposal
    PULL_REQUEST_TEMPLATE.md
  .gitignore                Unity standard + Library/ + fetched assets
  .gitattributes            Git LFS for .png .jpg .fbx .wav .ogg .psd .exr
```

No `.github/workflows/` for now; see spec §18.

## 3. Definition of done

A task is done when: the acceptance criteria below are met; `tools/run-tests.sh` passes locally; new rules have tests; new cubes pass the validator with previews committed; the independent review of `CLAUDE.md` rule 8 has run and its triage is on the pull request; the spec is updated if a default was decided; the owner has merged the pull request; the issue is closed with a one-line summary.

A milestone is done when: every issue is closed, a play-test note exists in `docs/playtests/`, and `README.md` reflects what now works.

---

## 4. M0 — the loop and the foundation

Goal: one Nightmare and one Sleeper play a full round over the network, with every lattice rule enforced by the host, and the cube pipeline proven on the simplest cubes. If this is not at least a little fun, stop and rethink before M1.

**M0.1 Repository and project skeleton**
- Create the public repo under MIT, default branch `main`, branch protection: pull request required, one approving review from the owner, no direct pushes. LFS enabled. Labels (`milestone:M0`…`M4`, `area:core|runtime|netcode|editor|content|ui|docs`, `good first issue`, `question`, `parallel-ok`), GitHub milestones M0–M4, issues for every M0 and M1 task below.
- Unity 6 URP project under `Lucid/` (ask the owner which 6000.x version is installed and pin it), the four asmdefs, `.gitignore`, `.gitattributes`, `README.md`, `LICENSE`, `CLAUDE.md` from the draft, `CONTRIBUTING.md` stub, `CODE_OF_CONDUCT.md`, `THIRD_PARTY_NOTICES.md`, `docs/` with SPEC, WORKPLAN, DECISIONS, issue and PR templates, the pre-commit license hook.
- `tools/run-tests.sh` runs EditMode tests headless and passes one placeholder test.
- Accept: a fresh clone opens in Unity 6 without errors; `tools/run-tests.sh` is green; the pre-commit hook rejects an unlisted asset.

**M0.2 `Lucid.Core`: lattice model and rules** — implement `docs/CORE-API.md` as written; its §12 is the test list.
- Types: `Face`, `ConnectorMask`, `CubeType` (id, mask, climbable, cost, category), `CubeInstance` (type, rotation), `Lattice` (coord → instance), `ConnectorState` (Attached, Fog, Exit, Solid), `Budget`, `EventLog` with `CubePlaced` and `CubeExplored`, `SleeperState` (cube, alive).
- Rules as pure functions: rotation of masks; fit rule; frontier rule; depth BFS; exit set with ties; explored solidification with exit and start exemptions; leak rule with one-way drops and two-way climbables; budget; `Validate(PlaceRequest)` returning a reason enum; deterministic `Derive(log)` → connector states.
- Tests: every rule; loop closure rejected by the leak rule; a drop region forces the exit beyond it; explore-then-place rejects, place-then-explore attaches; ties produce several exits; replaying a log yields identical derived state; a content hash of derived state for netcode checks.
- Accept: all tests green; no `UnityEngine` reference in the assembly.

**M0.3 Cube pipeline v0** — build to `docs/cube-spec.schema.json`; `docs/CUBE-SPEC.md` has the two reference specs.
- `CubeTemplate.prefab` with sockets at the connector standard; `Connector` and `FogDoor` components; the builder validates specs against the schema before doing anything else.
- `CubeBuilder.BuildFromSpec`: shell generation from the connector mask (walls, floor, ceiling, door frames, material roles), sockets, `CubeDefinition` asset, registration in `DreamPack`, previews from three fixed cameras; `tools/build-cube.sh`.
- `CubeValidator`: bounds, ≥ 2 connectors at standard positions, fog doors present, shell collision, license ledger; JSON report.
- Accept: Straight, Corner, T, Cross and the Start cube are built from spec files by the CLI; validator green; previews committed; the builder is idempotent (rebuilding changes nothing).

**M0.4 Sleeper controller**
- `CharacterController`-based first person: run 6 m/s, jump 1.2 m rise / ~4 m gap, crouch 1 m at 2.5 m/s; Input System `Sleeper` map, KBM only for now.
- A test gauntlet scene built by an editor script: gaps of 3.0, 3.5, 4.5 m and ledges of 1.0, 1.1, 1.4 m.
- Accept: PlayMode test with scripted input clears 3.5 m and 1.1 m, fails 4.5 m and 1.4 m.

**M0.5 Fog doors**
- Mist shader (layered scrolling noise on a quad stack), states Fog (grey) / Exit (white) / Solid (condenses into the cube's wall material) / Attached (dissolves), transitions animated; collision and the wake trigger driven by state.
- Accept: a `FogDoor` bound to a Core connector state plays the right transition for every state change; verified in a test scene and by a PlayMode test on the collider and trigger.

**M0.6 Dream runtime (local)**
- `DreamInstance`: instantiates cubes from the event log at lattice positions and rotations, wires fog doors to derived states, detects first entry per cube (trigger volume) and exit touches, respawns in the start cube.
- Accept: PlayMode test: replaying a log builds the expected geometry; entering a cube raises `Explored` exactly once; walking into a white door raises `TouchedExit`.

**M0.7 Nightmare god view and palette**
- Orbit / top-down camera with a layer slider and cut-away; fog and exit door highlights; palette (connectors only), ghost placement with rotation, red ghost with the Core reason on rejection; budget and timer HUD, all driven by Core.
- Accept: every connector type can be placed on any fog door; every rejection reason renders; placements go through `Validate`.

**M0.8 Netcode v0** — messages 001, 101–103, 201–203, 301–304, 311, 401, 402, 408 of `docs/NETCODE.md`.
- NGO host mode over Unity Transport; `Approval` with the `Hello` payload; `RoundSync` (host applies validated events through `Round`, broadcasts `LatticeEvent`, answers requests); `LatticeMirror` on clients with `HashReport`; 10 Hz `Telemetry`; Multiplayer Play Mode setup for the dev loop.
- Accept: host as Nightmare, a virtual player as Sleeper: placement, exploration, exit shift and waking work across the network; both sides report the same derived-state hash after every event; an explore/place race resolves by host order; a deliberately corrupted client hash produces a `DesyncNotice` and an automatic log save.

**M0.9 Round flow v0**
- Head start with the start door misted and a countdown; run phase; dawn; wake and consume; a minimal results overlay; return to a bare "again" screen.
- Accept: a full round can be played end to end with one Nightmare and one Sleeper, both outcomes reachable.

**M0.9b Sandbox (dev tool)**
- Offline mode from the title: unlimited budget, no timer, F5 switches between the god view and a Sleeper in the current lattice (UI doc §12). Reuses `DreamInstance` and the god view; no netcode.
- Accept: build ten cubes, drop in, walk to an exit, return to the god view, all without a network session.

**M0.9c Tuning console, log report, dev build**
- `F8` dev panel on the host: head start, round length, starting budget, trickle interval, exit hysteresis, Sleeper speed multiplier; values stamped into the `.lucidlog` header. Observer overlay: elapsed, cubes placed, exit depth, Sleeper depth. `PlaceRequest`s logged even when rejected, with timestamps.
- `tools/playtest-report.py`: the metrics in `docs/playtests/PLAN-M0.md` §5 from one `.lucidlog`; appends a row to `docs/playtests/m0-rounds.csv`.
- `tools/build-dev.sh`: a zipped UTP build for a second machine.
- Accept: a scripted round produces a report with every §5 column filled; changing a knob in the panel changes the next round and the header.

**M0.10 M0 play-test and retro** (owner)
- Run `docs/playtests/PLAN-M0.md`: warm-up plus ten rounds in three blocks, questionnaire after each, note from `TEMPLATE.md`, decisions written to DECISIONS the same day. Go / no-go for M1 by the plan's §7.

---

## 5. M1 — Steam, parallel dreams and cubes

Goal: friends can play from the Steam friends list, 2–4 Sleepers each in their own dream, with the MVP cube set and the Nightmare's triggers.

- **M1.1 Steam spike.** Facepunch.Steamworks plus the community Facepunch transport against the NGO version in use; if incompatible, a thin transport over SteamNetworkingSockets. One-page DECISIONS entry with the verdict. Needs the owner's Steam app ID.
- **M1.2 Transport selection.** `--transport=utp|steam` and an in-editor switch; the game runs fully without Steam for contributors.
- **M1.3 Steam lobby and invites.** Create/join, friends-only and invite-only, overlay invites, rich presence "Join game"; `SessionState` replication and messages 002–005, 105 (`docs/NETCODE.md` §3); lobby screen with roles, ready, host settings, start conditions, random Nightmare resolution, roles cleared after each round; version rejection with the Steam update hint.
- **M1.4 Parallel dreams.** One local dream per Sleeper; host tracks N Sleepers; telemetry per Sleeper; `Markers` fan-out (409); coloured markers in lobby order; hover-peek; reconnect within grace via `ResumeRequest`/`ResumeSnapshot` (204, 205) and consumption after it.
- **M1.5 Lives and respawn.** Lobby setting, default 1; consume on last life; the leak rule reads living Sleepers only.
- **M1.6 Results and leaderboard.** Per-round outcomes and times, scoring per spec §12, session leaderboard with "rounds as Nightmare".
- **M1.7a Chicane framework.** `Chicane` base, the `[Chicane]` attribute and discovery, `ChicaneParams`, and the shared blocks from `docs/CHICANES.md` §2 (`Cycler`, `Tell`, `DamageVolume`, `KillVolume`, `PressurePlate`, `Mover`, `ActorRig`, `WeakPoint`, `TriggerReceiver`, `PathProbe`), plus the `ChicaneTestRig` and `PathRunner`. Accept: a dummy component passes the four standard tests.
- **M1.7 Chicane components.** `SpikePit`, `Gap`, `Trapdoor`, `TimedSpikes`, `Crusher`, `Pendulum`, `Vent`, `MovingPlatforms`, `LittleMaze`, each to `docs/CHICANES.md` §3 with its params schema fragment, validator and four tests. `Turret` and `Nest` follow in M2 with the mobs.
- **M1.8 Cube set.** Verticals (Drop, Ladder shaft, Stairwell, Landing) and the nine chicanes above, each built through the pipeline from a brief and a spec; navmesh baked per cube.
- **M1.9 Manual triggers and the target selector.** Trigger button and `T` hotkey; target selector All / 1–4; per-trap-per-dream cooldowns; host validation and forwarding.
- **M1.10 Skins.** `SkinSet` with material roles, light preset and LUT slot; two skins (childhood bedroom, castle dungeon); Dream shuffle.
- **M1.11 Settings plumbing.** Every lobby setting in spec §6 applied end to end.
- **M1.12 Replay files.** Save the event log plus telemetry as `.lucidlog`; replay in the editor; the format contributors attach to bug reports; `ReplayRequest`/`ReplayChunk` (601, 602) so clients can pull the host's file at results.
- **M1.12b UI v1.** Title, lobby (players, leaderboard, settings with presets, invite, start reasons, Nightmare reveal), Sleeper panel and hover-peek, toasts with the copy glossary, spectator view, results v1, pause menu, options v0, edge flows (UI doc §3–§13). UI Toolkit throughout.
- **M1.13 M1 play-test and retro** (owner). First session with three or more friends over Steam.

---

## 6. M2 — the shooter

- Nightlight: hitscan, 4 shots/s, 10 damage, flashlight beam, muzzle and impact FX.
- Weak points and jamming: `WeakPoint` component, per-chicane jam effects, HUD feedback.
- Nest and Shade: `docs/CHICANES.md` §3 Nest and §4 Shade; spawn waves, leash, per-cube navmesh and links, the lunge with a forgiving hitbox.
- Eye turret: `docs/CHICANES.md` §3 Turret; bolts, core weak point, `IPossessable` ready for M3.
- Health, regen, damage table, kill volumes, death and respawn feedback.
- Gamepad for Sleepers: `Sleeper` map on gamepad, aim assist, rebinding screen for both roles, gamepad navigation of every menu.
- Sleeper HUD v2: health ring, moons, damage direction, weak-point ring (UI doc §6).
- Audio pass 1: mist, doors, traps, mobs, Nightlight; placeholder music.
- Mist VFX polish.
- M2 play-test and retro.

## 7. M3 — the haunting and the look

- Dream view streams and possession: `DreamRelay` with `SubscribeDream`, `DreamView`, `PossessInput`, `PossessedHit` and messages 314–317, 405–407, 501–504 (`docs/NETCODE.md` §8); the Nightmare's rendering copy of a dream; spectator tabs on the same streams; Eye aiming with local hit detection; blackout on death; DECISIONS entry on whether to move to distributed authority.
- Global effects: Dark, Fog, Molasses with per-dream cooldowns and costs; effect hotkeys.
- The dream filter: post volume, painterly render feature as a quality setting, per-skin LUTs, lighter Nightmare variant.
- Per-cube reflection probes, SSAO, performance pass with a target of 60 fps on mid-range hardware at 1080p.
- Polish: possession overlay and effects bar (UI doc §8), accessibility options, results badges, options v1, Steam Input glyphs.
- M3 play-test and retro.

## 8. M4 — the first Dream Pack

- Pick the cheapest theme to prove the pipeline (*Endless Office Hours*: corridors, a little maze, vents, a flickering-fluorescent Dark variant, one office-themed mob skin).
- 6–8 cubes, one skin and one mob built entirely through briefs and specs; nothing hand-made beyond shell textures.
- `CONTRIBUTING.md` written while doing it, including the asset rule and the license ledger.
- Stock `good first issue` with small cubes and skins for contributors.
- Ship as a free update on the Steam build.

---

## 9. First GitHub session for Claude Code

1. Ask the owner for the GitHub account or organisation, the repository name (default `lucid`), and the installed Unity 6 version.
2. Create the repository (public, MIT) and the layout in §2; commit the `docs/` folder and `CLAUDE.md` exactly as delivered in the consolidated document set.
3. Protect `main`: pull request required, one approving review, no force pushes.
4. Create labels and milestones M0–M4.
5. Open one issue per M0 and M1 task with the acceptance criteria copied in; later milestones get one epic issue each.
6. Open the pinned issue "Owner decisions needed" listing the spec's open questions (§19).
7. Start M0.1 on branch `m0/01-skeleton`, open the pull request, stop.

Owner actions Claude cannot do: GitHub account and repository permissions; installing Unity and telling Claude the version; Steam Direct sign-up and app ID (needed by M1.1); reviewing and merging pull requests.

## 10. Risks and spikes

| Risk | Where it bites | Plan |
|---|---|---|
| Community Facepunch transport lags NGO 2.x | M1.1 | Spike first; thin SteamNetworkingSockets transport as fallback |
| Possession latency feels bad | M3 | Local hit detection for the Eye, forgiving Shade lunge; distributed-authority spike if still bad |
| Painterly render feature too expensive | M3 | Quality setting; the LUT-and-DoF look without it is acceptable |
| Realtime lighting too heavy with many cubes | M3 | Light budget per cube; prefab-level lightmaps as escape hatch |
| Free assets not redistributable | Every cube | Manifest plus license gate from M0.3 onward |
| No CI means a green claim is unverified | Every PR | PR template asks for the pasted test summary; owner spot-checks locally |
| The yo-yo dominates play | M0.10 | Hysteresis is a one-line change in Core |
