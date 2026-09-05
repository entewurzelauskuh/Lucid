# Contributing to Lucid

> **Stub.** The real guide is written in M4 by making a Dream Pack and recording
> what it actually took (`docs/WORKPLAN.md` §8). Until then, this is the short form.

## Before you start

Read `docs/SPEC.md` for what the game is, then `docs/WORKPLAN.md` for what is
being built now. `CLAUDE.md` holds the conventions — they apply to everyone, not
just to Claude Code. If code and spec disagree, the spec wins unless
`docs/DECISIONS.md` says otherwise.

## Setting up

```bash
git clone https://github.com/entewurzelauskuh/Lucid.git
cd Lucid
tools/install-hooks.sh      # asset-rule gate; run once per clone
```

Open `Lucid/` with the Unity version pinned in
`Lucid/ProjectSettings/ProjectVersion.txt`. You do **not** need Steam: the Unity
Transport path runs the whole game on localhost or LAN, and Sandbox mode runs it
with no second player at all.

```bash
tools/run-tests.sh              # EditMode + PlayMode
tools/run-tests.sh editmode     # the fast loop
```

## The rules that matter

**One issue, one branch, one pull request.** Branch `m<N>/<NN>-<slug>`,
conventional commits. Say what changed, why, how you tested it, and paste the
`tools/run-tests.sh` summary line — there is no CI, so that line is the only
evidence a reviewer has.

**Never hand-edit prefab or scene YAML.** Cubes are generated from
`cube.spec.json` by `CubeBuilder`; scenes are set up by editor scripts. Commit
the generated files, but change them by changing their inputs.

**The asset rule.** Free does not mean redistributable. Only CC0 and CC-BY
assets may be committed, each with a line in its cube's `assets/LICENSES.md`
giving the source URL and licence. Plain CC0 or plain CC-BY: **NonCommercial,
NoDerivatives and ShareAlike are refused**, even though SA material is
redistributable — `docs/SPEC.md` §18 says why. The gate reads the licence
column of that row, so the row has to be `| file | source | licence |`. Everything else — including free Unity Asset
Store content — goes in `assets.manifest.json` and is fetched locally, never
committed. The pre-commit hook enforces this; do not bypass it.

## Adding a cube

The cube pipeline is the contribution path (`docs/SPEC.md` §17). Write
`brief.md`, add assets or a manifest, write `cube.spec.json` against
`docs/cube-spec.schema.json`, then build and read the validator report and the
generated previews. `docs/CUBE-SPEC.md` has worked examples.

A new *mechanic* is not a cube: it is a new component in the chicane library
(`docs/CHICANES.md` §8 — attribute, params schema, validator, four tests, a
tell), in its own pull request, before any cube uses it.

## Questions

Open an issue and label it `question`. Do not guess on spec items marked **[S]**;
they are settled.
