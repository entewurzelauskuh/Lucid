# LUCID — Chicane Component Library

**Version 1.0 — 2026-08-24 (consolidated).** Design context: spec §8 (cubes and mobs), §9 (kit limits), §10 (triggers, possession, effects), §17 (pipeline). Cube specs reference these components by name in `chicane.component`; the work plan's M1.7 implements the core set and M2 the mobs.

Everything here lives in `Lucid.Runtime.Chicanes` and runs **inside one dream, on one machine**. No component is networked. The host only ever sends two things into a dream that concern this library: a trigger (`kind`, from the Nightmare) and a possession session (for the Eye and the Shade). Everything else is local physics, local timers, local damage.

---

## 1. The three laws of traps **[D]**

1. **Every harm has a tell.** At least 250 ms of warning that is both audible and visible before anything can hurt. Audio is mandatory because Dark hides the visual and Fog hides distance.
2. **Every cube is crossable with the basic kit and no shooting.** The validator walks at least one intended path per connector pair and proves it survivable at full health. Jamming a weak point makes a cube easier; it is never required.
3. **A manual trigger skips the wait, never the warning.** Triggers jump a cycle to its Warning phase, not to its Active phase. The Nightmare gains timing, not ambushes.

And two courtesies: a source that damages gives 500 ms of immunity from the same source; traps do not slow down under Molasses, so the Sleeper's right answer is to wait, which is exactly the time the effect buys.

## 2. The shared contract

```csharp
namespace Lucid.Runtime.Chicanes;

public enum ChicaneState : byte { Idle, Warning, Active, Cooling, Jammed }

[AttributeUsage(AttributeTargets.Class)]
public sealed class ChicaneAttribute : Attribute
{
    public ChicaneAttribute(string name, string paramsSchema);   // name used in cube.spec.json; schema fragment for params
}

public abstract class Chicane : MonoBehaviour
{
    public string CubeTypeId { get; internal set; }
    public DreamContext Dream { get; internal set; }        // dream id, clock, the Sleeper, telemetry sink, audio bus
    public ActorRig Actors { get; internal set; }           // name → Transform, resolved by the builder
    public ChicaneState State { get; protected set; }
    public bool Jammed => State == ChicaneState.Jammed;

    public abstract void Configure(ChicaneParams p);         // called once at instantiate, from CubeDefinition
    public virtual void OnSleeperEntered(Sleeper s) { }      // cube trigger volume
    public virtual void OnSleeperLeft(Sleeper s) { }
    public virtual void OnTrigger(string kind) { }           // from the Nightmare, already validated by the host
    public virtual void OnJam(string effect) { }             // from WeakPoint
    public abstract string Peek();                            // one line for the Nightmare's hover-peek
    protected void SetState(ChicaneState s);                  // raises telemetry deltas
}

public interface IChicaneValidator
{
    ValidationReport Validate(CubeSpec spec, GameObject builtPrefab, PathProbe probe);
}

public interface IPossessable
{
    bool CanPossess { get; }
    void BeginPossession(PossessionInput input);
    void EndPossession();
}
```

Every component class carries `[Chicane("Name", "Name.params.schema.json")]`, implements `IChicaneValidator` as a nested static class or a sibling, and is discovered by reflection. The builder validates `chicane.params` against the fragment, resolves `chicane.actors` into the `ActorRig`, attaches the component, wires `WeakPoint` and `TriggerReceiver`, and asks the validator for its report.

### Shared building blocks

| Block | What it does |
|---|---|
| `Cycler` | Deterministic phase machine: `periodMs`, `warningMs`, `activeMs`, `phaseMs`. Phase is computed from `Dream.Clock`, so every dream runs the same rhythm until a targeted trigger re-phases one of them. `Rephase(toWarningAt: now)` implements law 3 |
| `Tell` | Emits a warning: an audio cue on the dream bus plus a visual (flash, twitch, glow). Enforces the 250 ms minimum |
| `DamageVolume` | Trigger volume with damage, cause, knockback and the 500 ms per-source immunity |
| `KillVolume` | Instant death with a cause (`pit`, `spikes`, `crush`); also created from `killVolumes` in the spec |
| `PressurePlate` | Sleeper-on-surface detection with a small debounce |
| `Mover` | Kinematic mover along waypoints; carries the Sleeper standing on it; ping-pong or cycle; dwell at ends |
| `ActorRig` | Resolves actor names to transforms; throws at build time if one is missing |
| `WeakPoint` | HP, hit flash, the crosshair ring feedback, `OnDestroyed → chicane.OnJam(effect)`, sets the telemetry `jammed` flag |
| `TriggerReceiver` | Receives `kind` from the dream's power router; the host has already checked cooldown and target |
| `PathProbe` | Validator helper: samples an intended path every 0.25 m, raycasts for floor, measures gaps, rises, clearance, and intersections with harm volumes; can also sweep a moving harm over time |

### Telemetry

A `ChicaneSummary { coord, component, state, jammed }` goes into the 10 Hz packet only when it changes. The Nightmare's hover-peek shows `Peek()` per cube for the selected dream.

## 3. Core components

Params are shown with defaults; times are integer milliseconds; positions are cube-local metres (`docs/CUBE-SPEC.md` §1).

### SpikePit

Static. The floor is spikes; ledges and pillars from `props` are the way across.

- **Params:** `{}`
- **Actors:** none. Harm comes from `killVolumes` with `cause: spikes`.
- **Behaviour:** none. The tell is the sight of the spikes; there is no timing to read.
- **Weak point / trigger:** none.
- **Validator:** the safe path never enters a kill volume; every gap along it ≤ 3.5 m; every landing ≥ 1.0 m wide; every rise ≤ 1.1 m.
- **Peek:** `Spike pit`.

### Gap

Static chasm with platforms. Two variants share the code.

- **Params:** `{ "variant": "pit" | "drop" }` (default `pit`)
- **Actors:** none. `pit` uses a kill volume; `drop` has an open floor and a `down` connector, so a miss lands in the cube below and the exit moves beyond it (spec §7, drops are funnels).
- **Validator:** platform-to-platform edge distance ≤ 3.5 m along the path; rises ≤ 1.1 m; `drop` variant must declare `down` in `connectors` and `openFloor: true`.
- **Peek:** `Gap` / `Gap (drop)`.

### Trapdoor

A floor panel that opens under a Sleeper.

- **Params:** `{ "variant": "pit" | "drop", "openDelayMs": 250, "resetMs": 4000, "hinge": "south" | "north" | "east" | "west" | "split" }`
- **Actors:** `panel` (non-static). Optional `latch` prop for the weak point.
- **Behaviour:** `Idle` → Sleeper on the panel → `Warning` for `openDelayMs` (creak, panel shudders) → `Active`: the panel swings down; anything on it falls (kill volume beneath for `pit`, the cube below for `drop`) → `Cooling` for `resetMs` (panel closes) → `Idle`.
- **Trigger:** `dropNow` — jumps to `Warning` immediately, then opens. Law 3 keeps the 250 ms.
- **Weak point:** `lockShut` (panel never opens; trigger inert in this dream) or `lockOpen` (panel stays open, forcing the ledge route). One per cube spec.
- **Validator:** if `openDelayMs` < panel length ÷ 6 m/s, a path avoiding the panel must exist and be safe; the panel's swing must not clip the safe route.
- **Peek:** `Trapdoor: armed` / `open` / `resetting` / `jammed shut` / `jammed open`.

### TimedSpikes

Rows of spikes on a rhythm.

- **Params:** `{ "periodMs": 2000, "warningMs": 400, "activeMs": 600, "phaseMs": 0, "damage": 34, "groups": [ { "actors": ["spikes_a"], "offsetMs": 0 } ] }`
- **Actors:** one or more spike rows per group (raised and lowered by the component).
- **Behaviour:** per group, `Cycler`: `Safe` → `Warning` (rattle, spikes twitch) → `Active` (damage on contact) → `Safe`. Groups with different `offsetMs` make a rhythm to read.
- **Trigger:** `fireNow` — every group re-phases to `Warning` now, then the cycle continues from there; the dream is now out of step with the others.
- **Weak point:** `retract` — all groups stay safe.
- **Validator:** for every group zone the path crosses, safe window (`periodMs − warningMs − activeMs`) ≥ zone length ÷ 6 m/s + 200 ms; with offsets, the sequence must be crossable by waiting at most one period per zone; `damage` < 100.
- **Peek:** `Timed spikes: cycling` / `jammed`.

### Crusher

Pistons that slam on a cycle.

- **Params:** `{ "periodMs": 3000, "warningMs": 500, "slamMs": 250, "holdMs": 400, "closedGap": 0.3, "pistons": [ { "actor": "piston_a", "offsetMs": 0, "axis": "x" | "y" | "z", "travel": 3.5 } ] }`
- **Actors:** the pistons (non-static), moved along `axis` by `travel`.
- **Behaviour:** `Open` → `Warning` (hydraulic hiss, piston shivers) → `Active` (slam in `slamMs`; the space between piston and stop is a crush kill volume while closed) → `Hold` → retract → `Open`.
- **Trigger:** `slamNow` — re-phase to `Warning` now.
- **Weak point:** `stallRetracted` — pistons stay open.
- **Validator:** open window (`periodMs − warningMs − slamMs − holdMs`) ≥ crush-zone length ÷ 6 m/s + 200 ms; crush zone ≤ 3 m along the path; `closedGap` < 1 m so a crouching Sleeper cannot survive inside it (no false safety).
- **Peek:** `Crusher: cycling` / `jammed open`.

### Pendulum

Swinging blades over a walkway.

- **Params:** `{ "periodMs": 2400, "swingDeg": 120, "damage": 50, "knockback": 3, "blades": [ { "actor": "blade_a", "pivot": [0, 7.5, 0], "axis": "x" | "z", "phaseDeg": 0 } ] }`
- **Actors:** the blades (non-static), rotated about `pivot` and `axis`.
- **Behaviour:** continuous swing from `Dream.Clock`; the blade carries a `DamageVolume` (damage plus knockback away from the blade). The tell is the swing itself plus a whoosh that rises as the blade nears the walkway.
- **Trigger:** `hold` — the blade freezes at the bottom of its arc for 1500 ms, blocking the walkway, then resumes. The freeze is announced by a chain clank; it never damages by itself.
- **Weak point:** `dropBlade` — the chain breaks, the blade falls onto the walkway and becomes a static hurdle no taller than 1.1 m, so it is jumpable.
- **Validator:** the blade's sweep zone width ÷ 6 m/s + 200 ms ≤ half period minus the time the blade occupies the walkway; the fallen-blade geometry ≤ 1.1 m high; `damage` < 100.
- **Peek:** `Pendulum: swinging` / `held` / `fallen`.

### Turret (the Eye)

A static shooter covering the cube; the only component the Nightmare can possess.

- **Params:** `{ "actor": "eye", "fireIntervalMs": 1200, "boltSpeed": 10, "damage": 20, "acquireMs": 400, "fovDeg": 120, "yaw": [-90, 90], "pitch": [-30, 30], "loseTargetMs": 1000 }`
- **Actors:** `eye` (rotates to aim), optional `core` prop for the weak point.
- **Behaviour:** `Idle` → Sleeper in FOV with line of sight → `Warning` (`acquireMs`: iris glow, hum rises) → `Active` (a bolt every `fireIntervalMs`, `boltSpeed` m/s, collides with geometry, `damage` on hit) → loses the target after `loseTargetMs` without line of sight → `Idle`.
- **Trigger:** none. Possession replaces it.
- **Possession:** `IPossessable`. The Nightmare aims within `yaw`/`pitch` and fires on click at the same interval; no acquire delay, no automatic tracking. Hit detection runs on the Nightmare's client against the streamed Sleeper transform (spec §14).
- **Weak point:** `disable` — the Eye is dead.
- **Validator:** total line-of-sight exposure along the safe path ≤ 3 s at run speed (≤ 2 bolts, 40 damage), or a cover segment exists; bolts cannot reach the doorway the Sleeper enters through within `acquireMs`.
- **Peek:** `Eye: idle` / `tracking` / `possessed` / `dead`.

### Vent

A crouch-only crawl. Slow, safe, and a good place for a Nest next door.

- **Params:** `{ "duct": "duct" }` (the actor that defines the crawl; optional `dark: false`)
- **Behaviour:** none. The clearance forces the crouch; crouch speed is 2.5 m/s.
- **Validator:** clearance along the crawl segment between 1.1 m and 1.6 m (must force a crouch, must not trap), crawl length ≤ 8 m, no harm volumes.
- **Peek:** `Vent`.

### MovingPlatforms

Platforms over a pit.

- **Params:** `{ "carry": true, "platforms": [ { "actor": "plat_a", "waypoints": [[-3, 0, -2], [3, 0, 2]], "speed": 2.0, "dwellMs": 800, "mode": "pingpong" | "cycle", "phase": 0.0 } ] }`
- **Actors:** the platforms (non-static), driven by `Mover`.
- **Behaviour:** continuous from `Dream.Clock`. A Sleeper standing on a platform moves with it. The pit is a `killVolumes` entry.
- **Weak point / trigger:** none.
- **Validator:** at some phase every consecutive pair (ledge → platform → platform → ledge) comes within 3.5 m edge to edge and stays there ≥ 400 ms; `speed` ≤ 4 m/s; `dwellMs` ≥ 500; the combined path is survivable at full speed with waits.
- **Peek:** `Moving platforms`.

### LittleMaze

A folded corridor inside one cube. Costs time, never lives.

- **Params:** `{ "seed": 0, "grid": [4, 4], "corridorWidth": 1.4, "wallHeight": 3 }`
- **Actors:** none; the builder generates the walls from `seed` (a perfect maze on the grid, so it is the same in every dream and every session).
- **Behaviour:** none.
- **Validator:** a path exists between every pair of connectors; its length ≤ 40 m; corridors ≥ 1.2 m wide; no dead end deeper than 6 m.
- **Peek:** `Maze`.

### Nest

Spawns Shades when a Sleeper comes close.

- **Params:** `{ "nest": "nest", "mob": "Shade", "waveSize": 3, "waveIntervalMs": 20000, "maxAlive": 3, "leashCubes": 1, "spawnPoints": [[-2, 0, -2], [2, 0, 2]], "aggroOnEnter": true }`
- **Actors:** `nest` (the shootable thing; also the weak point).
- **Behaviour:** `Dormant` → Sleeper enters the cube (or a neighbour within `leashCubes`) → `Active`: spawn a wave up to `maxAlive` (chitter tell, 300 ms, then the Shades appear) → `Cooling` for `waveIntervalMs` while the Sleeper stays within the leash → another wave → `Dormant` when the Sleeper leaves the leash. Mob density (lobby) scales `waveSize` and `maxAlive`.
- **Trigger:** `spawnWave` — a wave now, still capped by `maxAlive`, still with the 300 ms tell.
- **Weak point:** `stopWaves` — no more waves; Shades already out remain.
- **Validator:** spawn points lie on the baked navmesh; the navmesh links across every connector so the leash region is walkable; at least one spawn point is ≥ 4 m from every doorway (no spawn-on-entry deaths).
- **Peek:** `Nest: dormant` / `2 Shades out` / `jammed`.

## 4. Mobs **[D]**

Mobs are not chicanes, but the Nest and the Eye own them, so they live next door in `Lucid.Runtime.Mobs`.

### Shade

| | |
|---|---|
| Stats | 20 HP (two Nightlight shots), 5.5 m/s, lunge 15 damage |
| States | `Idle` at the nest → `Alert` (chitter, 300 ms) → `Chase` on the navmesh → `Lunge` (300 ms wind-up, 1.2 m reach, 1000 ms cooldown) → `Return` when the leash is exceeded → `Idle` |
| Leash | Nest cube plus `leashCubes` neighbours; beyond it the Shade turns back |
| Death | Dissolves in a burst of light; no corpse, no ragdoll |
| Possession | `IPossessable`: direct control at 5.5 m/s, lunge on click with the same wind-up and a forgiving hitbox, no leash, no navmesh. The body dies like any Shade; the Nightmare gets the 2 s blackout |

### Eye

The Turret's mob half is the Turret itself (§3). Bolts are pooled projectiles: 10 m/s, 20 damage, destroyed on any contact, lit so they read in Dark.

Later mobs (Crawler, Ghost, Mimic) follow the same shape: stats, a small state machine, a tell before every attack, a leash unless possessed.

## 5. Verticals

Not chicanes either, but the library carries the two pieces the Sleeper controller needs.

- **Ladder** — a `Climbable` volume the builder generates from `generated:ladder`. The Sleeper enters it and climbs at 3 m/s without gravity; leaves at the top through the ceiling hole or at the bottom. Validator: the ladder reaches the `up` connector; the cube declares `climbable: true`.
- **Stairs** — `generated:stairs`, plain geometry; steps ≤ 0.3 m so the controller walks them. Validator: the top step is within 0.3 m of the ceiling hole edge; the cube declares `climbable: true`.
- **Drop** — no component; `openFloor: true` and a `down` connector.

## 6. Effects and possession seen from a trap

- **Dark:** components keep working; every `Tell` plays its audio cue; spikes, bolts and blades carry a faint glow so they remain visible up close.
- **Fog:** no change.
- **Molasses:** no change to any cycle (law 1's corollary). The Sleeper is slower; the traps are not kinder.
- **Possession:** only `IPossessable` components respond; the dream's power router refuses everything else.

## 7. Testing harness

`ChicaneTestRig` (PlayMode) instantiates a built cube prefab in an empty dream, spawns a scripted Sleeper and a `PathRunner` that walks an intended path at run speed, jumping where the probe found gaps. Every component ships four tests:

1. **Crossing:** the safe path from every intended pair is survivable at full health.
2. **Harm:** the naive straight line takes damage or dies where the design says it should.
3. **Jam:** after the weak point is destroyed, the harm does not happen and `Peek()` says so.
4. **Tell:** the first harm never precedes the first tell by less than 250 ms, including right after a manual trigger.

Cyclers are tested against a fake `Dream.Clock`, so rhythm tests run in milliseconds of wall time.

## 8. Adding a component (packs)

1. `MyTrap.cs` in `Lucid.Runtime.Chicanes` (or a pack assembly that references it) with `[Chicane("MyTrap", "MyTrap.params.schema.json")]`.
2. The params schema fragment beside it; the builder validates against it.
3. `Configure`, the state machine using `Cycler`/`Tell`/`DamageVolume` where possible, `OnTrigger`, `OnJam`, `Peek`.
4. The validator: actors exist, params in range, and the path checks that prove law 2 for this trap.
5. The four PlayMode tests.
6. A `CHANGELOG` line in the pack and a row in this document's table of components.

A component without a validator does not build. A component without a tell does not pass review.

## 9. Component table

| Name | Category | Trigger | Jam effects | Possessable | Harm |
|---|---|---|---|---|---|
| SpikePit | chicane | – | – | no | kill |
| Gap | chicane | – | – | no | kill / drop |
| Trapdoor | chicane | dropNow | lockShut, lockOpen | no | kill / drop |
| TimedSpikes | chicane | fireNow | retract | no | 34 |
| Crusher | chicane | slamNow | stallRetracted | no | kill |
| Pendulum | chicane | hold | dropBlade | no | 50 + knockback |
| Turret (Eye) | chicane | – | disable | yes | 20 per bolt |
| Vent | chicane | – | – | no | – |
| MovingPlatforms | chicane | – | – | no | kill (pit) |
| LittleMaze | chicane | – | – | no | – |
| Nest | mob | spawnWave | stopWaves | via Shade | 15 per lunge |
