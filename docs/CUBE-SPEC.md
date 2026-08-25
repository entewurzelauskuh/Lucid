# LUCID — Cube Spec Guide

**Version 1.0 — 2026-08-24 (consolidated).** Companion to `docs/cube-spec.schema.json`. Design context: spec §8 and §17.

`cube.spec.json` is the only thing a cube author writes by hand (or Claude Code writes from a brief). The builder turns it into a prefab, a `CubeDefinition` and previews; the validator turns it into a report. Prefab YAML is never edited.

## 1. The frame

Cube-local coordinates: origin at the centre of the floor, **x east, y up, z north**, metres. The cube spans x, z ∈ [−4, 4] and y ∈ [−t, 8 − t], where t is the shell thickness: the origin is the walkable surface, so the floor slab hangs just below it and the ceiling stops the same distance short of the top. That is still exactly 8 m, and it is what lets a cube's floor slab sit in the gap the cube below leaves above its ceiling rather than in the same volume. **[S]** Doorways are centred on their face at floor level, 2.5 m wide × 3 m high. Vertical connectors are a 2.5 m square hole centred on the floor or ceiling.

Face centres at floor level: north (0, 0, 4), east (4, 0, 0), south (0, 0, −4), west (−4, 0, 0); up is the ceiling hole at (0, 8, 0), down the floor hole at (0, 0, 0).

`Lucid.Core` uses lattice coordinates (x east, y north, z layer up); the runtime maps a lattice coord to a world position as `(x·8, z·8, y·8)`. Rotation R90 turns the cube 90° clockwise seen from above: north → east → south → west. Up and down never rotate.

## 2. What goes where

| Part of the spec | What it decides |
|---|---|
| `connectors`, `climbable`, `category`, `cost` | Everything `Lucid.Core` needs; copied into the `CubeDefinition` |
| `shell` | Generated walls, floor, ceiling and door frames; material *roles* that a SkinSet resolves |
| `props` | Dressing and mechanism parts: committed assets, manifest assets, or builder primitives |
| `chicane`, `weakPoint`, `trigger`, `killVolumes` | The gameplay of the cube, wired to library components |
| `intendedPaths` | How a Sleeper is meant to cross; the validator measures gaps, rises and clearance along them |
| `nav`, `lighting`, `skins`, `preview` | Navmesh bake, lights, which skins apply, which previews to render |

Rules the schema enforces: at least two connectors unless the category is `start`; `climbable` needs `up`; chicane and mob cubes need a `chicane` block and intended paths; vertical cubes need intended paths; a trigger or weak point needs a chicane.

Rules the validator enforces on the built prefab: everything inside the 8 m bounds — x, z ∈ [−4, 4] and y ∈ [−t, 8 − t] per §1, not y ∈ [0, 8]; connectors at the standard positions with fog doors; shell collision present; navmesh baked and linked; declared weak point and trigger resolve to live components; intended paths within the kit limits (gap ≤ 3.5 m, rise ≤ 1.1 m, clearance ≥ 1.1 m); ≤ 60 k triangles; every asset licensed, every committed asset redistributable.

## 3. Example: the simplest connector

`Assets/_Lucid/Packs/core/Cubes/straight/cube.spec.json`

```json
{
  "specVersion": 1,
  "id": "core.straight",
  "name": "Straight",
  "pack": "core",
  "category": "connector",
  "cost": 1,
  "connectors": ["north", "south"],
  "shell": {
    "materials": { "wall": "wall", "floor": "floor", "ceiling": "ceiling", "trim": "trim" },
    "doorFrame": "plain",
    "interior": { "width": 4, "height": 4 }
  },
  "intendedPaths": [
    { "from": "south", "to": "north", "points": [[0, 0, -4], [0, 0, 4]] }
  ],
  "skins": ["*"],
  "notes": "A 4 m wide corridor inside the 8 m cube. No props; the skin provides all the character."
}
```

Corner, T and Cross differ only in `connectors` (`["south", "east"]`, `["south", "east", "west"]`, all four) and their intended paths.

## 4. Example: a full chicane

`Assets/_Lucid/Packs/core/Cubes/trapdoor_pit/cube.spec.json`

```json
{
  "specVersion": 1,
  "id": "core.trapdoor_pit",
  "name": "Trapdoor",
  "pack": "core",
  "category": "chicane",
  "cost": 3,
  "connectors": ["south", "north"],
  "shell": {
    "materials": { "wall": "wall", "floor": "floor", "ceiling": "ceiling", "trim": "metal" },
    "doorFrame": "industrial",
    "openFloor": true
  },
  "props": [
    { "name": "floor_south", "asset": "generated:box", "position": [0, -0.15, -3], "size": [8, 0.3, 2], "material": "floor" },
    { "name": "floor_north", "asset": "generated:box", "position": [0, -0.15, 3], "size": [8, 0.3, 2], "material": "floor" },
    { "name": "ledge_west", "asset": "generated:box", "position": [-3.5, -0.15, 0], "size": [1, 0.3, 4], "material": "floor" },
    { "name": "panel", "asset": "generated:box", "position": [0, -0.15, 0], "size": [7, 0.3, 4], "material": "metal", "static": false },
    { "name": "latch", "asset": "assets/latch_box.fbx", "position": [3.6, 1.2, 0], "rotation": [0, -90, 0], "collider": "box" },
    { "name": "pit_walls", "asset": "generated:box", "position": [0, -4, 0], "size": [8, 8, 8], "material": "wall", "collider": "none" }
  ],
  "chicane": {
    "component": "Trapdoor",
    "params": { "variant": "pit", "openDelayMs": 250, "resetMs": 4000, "hinge": "south" },
    "actors": ["panel"]
  },
  "weakPoint": { "prop": "latch", "hp": 60, "jamEffect": "lockShut" },
  "trigger": { "kind": "dropNow", "cooldownMs": 6000, "label": "Drop" },
  "killVolumes": [
    { "center": [0, -3.5, 0], "size": [7, 1, 4], "cause": "pit" }
  ],
  "intendedPaths": [
    { "from": "south", "to": "north", "points": [[0, 0, -4], [-3.5, 0, -2], [-3.5, 0, 2], [0, 0, 4]], "notes": "Safe route along the west ledge." },
    { "from": "south", "to": "north", "points": [[0, 0, -4], [0, 0, -2], [0, 0, 2], [0, 0, 4]], "notes": "Straight across the panel; only safe once the latch is shot or if you are fast." }
  ],
  "nav": { "agentRadius": 0.4, "links": true, "exclude": ["panel"] },
  "lighting": {
    "preset": "skin",
    "lights": [ { "type": "spot", "position": [3.2, 3, 0], "direction": [0, -1, 0], "role": "accent", "intensity": 8, "color": "skin" } ]
  },
  "skins": ["*"],
  "notes": "The pit variant kills; the drop variant is the same spec with variant: drop, openFloor kept, no killVolumes, and a down connector."
}
```

What the builder does with it: generates the shell without a floor slab; instantiates the four generated boxes and the latch mesh from `assets/`; attaches the `Trapdoor` component to the cube and hands it the `panel` actor; attaches `WeakPoint` to the latch with 60 HP mapped to the component's `lockShut` effect; registers the `dropNow` trigger with a 6 s cooldown; adds the kill volume; bakes the navmesh over the ledge and floors, excluding the moving panel; adds links at both doorways; renders three previews; runs the validator, which walks both intended paths and confirms the ledge route has no gap, no rise and full clearance.

## 5. Writing a spec from a brief

1. Read `brief.md`. Decide the connector layout first; it fixes cost and category.
2. Choose shell roles over literal materials unless the cube only makes sense in one skin.
3. Prefer `generated:` primitives for anything structural; use assets for dressing and for the one prop the Sleeper must recognise (the latch, the pendulum blade, the turret).
4. Give every prop a stable `name`; the chicane, weak point and nav sections refer to it.
5. Draw the intended paths as you would walk them; put the safe route first.
6. Build, read `Previews/report.json`, fix, rebuild. Do not tune the kit limits.

## 6. Versioning

`specVersion` is 1. Breaking changes to the schema bump it; the builder keeps reading older versions and `tools/build-cube.sh <pack>` rebuilds everything after a template or schema change.
