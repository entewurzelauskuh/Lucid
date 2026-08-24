# LUCID — `Lucid.Core` API

**Version 1.0 — 2026-08-24 (consolidated).** Design context: spec §7 (lattice rules), §10 (budget, powers), §14 (architecture). The work plan's M0.2 implements this document; M0.6–M0.8 consume it.

`Lucid.Core` is a plain C# assembly with no `UnityEngine` reference. It owns every rule of the dream and nothing else: no rendering, no physics, no networking. Everything in it is deterministic and unit-tested, so Claude Code can build and verify it from the command line before the editor is ever opened.

---

## 1. Conventions

- **Coordinates.** `Coord(X, Y, Z)`: x east, y north, z layer up. The start cube is `(0, 0, 0)`. The runtime maps a coord to a Unity position as `(X·8, Z·8, Y·8)`; Core never knows about metres.
- **Faces.** `North (+Y), East (+X), South (−Y), West (−X), Up (+Z), Down (−Z)`. `Opposite(Face)` pairs them.
- **Rotation.** `R0, R90, R180, R270`, clockwise seen from above: `North → East → South → West`. `Up` and `Down` are fixed.
- **Determinism.** No floating point in rules; time is integer milliseconds; iteration over sets is in a defined order (sorted by coord, then face) so hashes match across machines.
- **Immutability at the edges.** `Lattice` and `Derived` are read-only to callers; changes happen only through `Rules.Apply…`, which return new state. Cloning a few hundred cubes per placement is cheap and keeps validation side-effect free.
- **Ids.** Cube types, skins and packs are strings (`core.trapdoor_pit`); Sleepers are `int` lobby indices 0–3; players are `PlayerId` opaque ints assigned by the lobby.

## 2. Types

```csharp
namespace Lucid.Core;

public readonly record struct Coord(int X, int Y, int Z)
{
    public Coord Offset(Face f);                       // neighbour through a face
    public static IComparer<Coord> Ordering { get; }   // (Z, Y, X)
}

public enum Face : byte { North, East, South, West, Up, Down }
public enum Rotation : byte { R0, R90, R180, R270 }

[Flags] public enum FaceMask : byte
{ None = 0, North = 1, East = 2, South = 4, West = 8, Up = 16, Down = 32 }

public static class Faces
{
    public static Face Opposite(Face f);
    public static Coord Offset(Face f);                // unit step
    public static Face Rotate(Face f, Rotation r);     // Up/Down unchanged
    public static FaceMask Rotate(FaceMask m, Rotation r);
    public static bool IsVertical(Face f);
    public static IEnumerable<Face> Of(FaceMask m);    // in enum order
}

public enum CubeCategory : byte { Connector, Vertical, Chicane, Mob, Gimmick, Start }

public sealed record CubeType(
    string Id, string Pack, CubeCategory Category,
    FaceMask Connectors,      // unrotated
    bool Climbable,           // Up connector climbable from inside
    int Cost);

public sealed class CubeRegistry
{
    public void Register(CubeType t);   // throws unless ≥ 2 connectors, or Category == Start with exactly 1
    public CubeType Get(string id);     // throws KeyNotFound
    public bool TryGet(string id, out CubeType t);
    public IReadOnlyCollection<CubeType> All { get; }
}

public sealed record CubeInstance(string TypeId, Rotation Rotation, string SkinId, long PlacedSeq);

public readonly record struct ConnectorRef(Coord Cube, Face Face);   // one doorway, seen from one cube

public enum ConnectorState : byte { Attached, Fog, Exit, Solid }

public sealed record Limits(int FootprintHalf, int LayerMin, int LayerMax);   // defaults 12, −3, +3
```

### Lattice

```csharp
public sealed class Lattice
{
    public Coord Start { get; }
    public string StartTypeId { get; }
    public IReadOnlyDictionary<Coord, CubeInstance> Cubes { get; }
    public IReadOnlySet<ConnectorRef> Solidified { get; }   // doors turned Solid by exploration
    public IReadOnlySet<Coord> Explored { get; }

    public static Lattice New(CubeRegistry reg, string startTypeId, Rotation startRotation);
    public bool Has(Coord c);
    public FaceMask ConnectorsAt(Coord c, CubeRegistry reg);   // rotated mask
    public bool HasConnector(Coord c, Face f, CubeRegistry reg);
    public Lattice Clone();
}
```

Internally a `Dictionary<Coord, CubeInstance>` plus two hash sets. Nothing else. The start cube is a cube like any other in the dictionary; its exemptions live in the rules.

### Derived state

```csharp
public sealed record Derived(
    IReadOnlyDictionary<Coord, int> Depth,                       // BFS hops from Start
    IReadOnlyDictionary<ConnectorRef, ConnectorState> Connectors, // every connector of every cube
    IReadOnlyList<ConnectorRef> Exits,                           // the white doors, ordered
    int ExitDepth,                                               // −1 if no fog door exists
    ulong Hash);                                                 // FNV-1a over Depth and Connectors, ordered
```

### Sleepers, budget, clock

```csharp
public enum SleeperStatus : byte { InDream, Awake, Consumed, Disconnected }

public sealed record SleeperState(int Id, PlayerId Player, SleeperStatus Status, Coord Cube, int Lives);

public sealed class Budget
{
    public int Points { get; }
    public int TrickleIntervalMs { get; }      // 0 = no trickle
    public int MsUntilNextPoint { get; }
    public Budget(int startingPoints, int trickleIntervalMs);
    public void Advance(int deltaMs);          // integer accumulation, never loses remainder
    public bool CanAfford(int cost);
    public bool TrySpend(int cost);
}

public sealed record RoundSettings(
    int HeadStartMs = 30_000, int RoundLengthMs = 300_000, int Lives = 1,
    int StartingBudget = 12, int TrickleIntervalMs = 4_000,
    int ExitHysteresis = 0,                    // 0 = off; 2 = a new deepest cube must beat the current exit depth by 2
    Limits? Limits = null);
```

## 3. Deriving connector states

```csharp
public static class Deriver
{
    public static Derived Derive(Lattice l, CubeRegistry reg, int exitHysteresis = 0, int previousExitDepth = -1);
}
```

Algorithm, in order:

1. **Attachment.** For every cube and every connector face `f` of its rotated mask: let `n = c.Offset(f)`. The door is **Attached** if `n` exists and `n` has the connector `Opposite(f)`. If `n` exists without that connector the lattice is corrupt: throw `LatticeInvariantViolation` (the fit rule makes this unreachable).
2. **Depth.** Breadth-first from `Start` over Attached doors in both directions, vertical included. Depth is hop count. Cubes not reached from Start cannot exist (placement is frontier-only); assert it.
3. **Fog and Solid.** A non-attached door is **Solid** if it is in `Solidified`, else **Fog**.
4. **Exits.** Among cubes with at least one Fog door, take the maximum depth `d`. Every Fog door on a cube at depth `d` becomes **Exit**. If no cube has a Fog door, `Exits` is empty and `ExitDepth = −1` (the leak rule guarantees this never happens after a valid placement).
   With hysteresis `h > 0` and a `previousExitDepth`: if `d < previousExitDepth + h` and the previous exit cubes still have Fog doors, keep the exits where they were. Off by default; the knob exists for M0.10.
5. **Hash.** FNV-1a 64 over `(coord, depth)` in `Coord.Ordering`, then `(coord, face, state)` in the same order. Two machines that agree on the hash agree on the dream.

Cost: O(cubes + doors). A few hundred cubes derive in well under a millisecond.

## 4. Traversal (for the leak rule)

```csharp
public static class Traversal
{
    public static HashSet<Coord> Reachable(Lattice l, CubeRegistry reg, Derived d, Coord from);
    public static bool CanReachExit(Lattice l, CubeRegistry reg, Derived d, Coord from);
}
```

Directed reachability over Attached doors:

- horizontal door: both directions;
- `Down` door from cube `c` to `c.Offset(Down)`: always allowed (a drop);
- `Up` door from cube `c` to `c.Offset(Up)`: allowed only if `reg.Get(c.TypeId).Climbable`.

`CanReachExit` is true if any reachable cube owns an Exit door. Exits are doors, not cubes, so a Sleeper standing in the exit cube itself trivially reaches one.

## 5. Placement

```csharp
public sealed record PlaceRequest(ConnectorRef Target, string TypeId, Rotation Rotation, string SkinId);

public enum PlaceError : byte
{ None, UnknownType, NotADoor, DoorIsSolid, DoorOccupied, OutOfBounds, DoesNotFit, NotEnoughBudget, WouldTrap, StartProtected }

public readonly record struct PlaceVerdict(PlaceError Error, int TrappedSleeper = -1)
{ public bool Ok => Error == PlaceError.None; }

public sealed record RuleContext(
    Lattice Lattice, Derived Derived, CubeRegistry Registry,
    IReadOnlyList<SleeperState> Sleepers, Budget Budget, RoundSettings Settings);

public static class Rules
{
    public static PlaceVerdict ValidatePlace(RuleContext ctx, PlaceRequest req);
    public static (Lattice lattice, Derived derived) ApplyPlace(RuleContext ctx, PlaceRequest req, long seq);
}
```

`ValidatePlace` checks in this order and returns the first failure:

1. `UnknownType` — `req.TypeId` not registered, or its category is `Start`.
2. `NotADoor` — `Target.Cube` does not exist, or has no connector on `Target.Face`, or the door is Attached.
3. `DoorIsSolid` — the door is in `Solidified`.
4. `DoorOccupied` — `Target.Cube.Offset(Target.Face)` already holds a cube (cannot happen if 2 passed; kept as a guard).
5. `OutOfBounds` — the new coord violates `Limits`.
6. `DoesNotFit` — **fit rule**: let `m = Rotate(type.Connectors, req.Rotation)`. `m` must contain `Opposite(Target.Face)`. For every face `f` of the new cube whose neighbour exists: `m` has `f` ⇔ the neighbour has `Opposite(f)`. Connector to connector, wall to wall, nothing else.
7. `NotEnoughBudget` — `!Budget.CanAfford(type.Cost)`.
8. `WouldTrap` — **leak rule**: clone the lattice, add the cube, derive, and for every Sleeper with `Status == InDream` check `CanReachExit(from: sleeper.Cube)`. The first Sleeper who could not reach an exit is reported in `TrappedSleeper`.

`ApplyPlace` assumes a passing verdict: adds the `CubeInstance` with `PlacedSeq = seq`, derives, and returns both. Budget is spent by the caller (the host) so that validation stays pure.

Placing on an Exit door is legal and is how the exit moves deeper. Placing anything on the start cube's single door is legal too; the start cube is protected only from exploration and from ever being replaced.

## 6. Exploration

```csharp
public enum ExploreError : byte { None, NoCube, AlreadyExplored, StartCube }

public static class Rules
{
    public static ExploreError ValidateExplore(RuleContext ctx, Coord cube);
    public static (Lattice lattice, Derived derived) ApplyExplore(RuleContext ctx, Coord cube, long seq);
}
```

`ApplyExplore`: mark the cube explored; for every door of that cube whose current state is **Fog** (not Exit, not Attached, not already Solid), add it to `Solidified`; derive. `AlreadyExplored` and `StartCube` are reported so the host can drop the event silently rather than log it. Exploration is global (spec §5): the host applies it once no matter which Sleeper reported it.

Because the host serialises events, races resolve themselves: an `Explored` applied before a `Place` on the same door leaves the placement with `DoorIsSolid`; a `Place` applied first leaves the exploration with nothing to solidify on that face.

## 7. Events and the log

```csharp
public abstract record LatticeEvent(long Seq);
public sealed record CubePlaced(long Seq, Coord Cube, string TypeId, Rotation Rotation, string SkinId) : LatticeEvent(Seq);
public sealed record CubeExplored(long Seq, Coord Cube, int SleeperId) : LatticeEvent(Seq);

public sealed class EventLog
{
    public IReadOnlyList<LatticeEvent> Events { get; }
    public long NextSeq { get; }
    public void Append(LatticeEvent e);                                  // Seq must equal NextSeq
    public static (Lattice, Derived) Replay(EventLog log, CubeRegistry reg, string startTypeId, Rotation startRotation, RoundSettings s);
    public void Write(Stream s);                                         // compact binary, for the wire and .lucidlog
    public static EventLog Read(Stream s);
}
```

Replay re-applies events in sequence through `ApplyPlace` and `ApplyExplore` without validation and without a budget; it must produce the same `Derived.Hash` as the live host. A `.lucidlog` file is a header (format version, settings, players, cube registry ids) followed by the event stream and, optionally, telemetry lines; the runtime owns the header and telemetry, Core owns the event encoding.

Only these two event kinds touch the lattice. Round events (`SleeperWoke`, `SleeperConsumed`, `EffectFired`, `TriggerFired`) are recorded by the runtime for replays and results and never feed `Derive`.

## 8. Round, waking, lives, scoring

```csharp
public enum Phase : byte { HeadStart, Running, Dawn }

public sealed class Round
{
    public Round(RoundSettings settings, CubeRegistry reg, string startTypeId, Rotation startRotation, IReadOnlyList<PlayerId> sleepers);
    public Phase Phase { get; }
    public int ClockMs { get; }
    public Lattice Lattice { get; }
    public Derived Derived { get; }
    public Budget Budget { get; }
    public EventLog Log { get; }
    public IReadOnlyList<SleeperState> Sleepers { get; }
    public bool IsOver { get; }

    public void Advance(int deltaMs);                               // clock, budget trickle, HeadStart → Running → Dawn
    public PlaceVerdict TryPlace(PlaceRequest r);                   // validate, apply, spend, log
    public ExploreError TryExplore(int sleeperId, Coord cube);      // validate, apply, log
    public void UpdateSleeperCube(int sleeperId, Coord cube);       // from telemetry; feeds the leak rule
    public WakeVerdict TryWake(int sleeperId, ConnectorRef door);   // Woke | NotAnExit | NotInDream | HeadStart
    public DeathVerdict ReportDeath(int sleeperId);                 // LostLife(livesLeft) | Consumed
    public void ReportDisconnect(int sleeperId, bool reconnected);
}

public static class Scoring
{
    public static IReadOnlyDictionary<PlayerId, int> Compute(Round r, PlayerId nightmare);
    // Sleeper woke: 100 + remaining seconds at wake. Consumed: 0. Nightmare: 100 per consumed Sleeper.
}
```

`Round` is the host's single object; the netcode layer only forwards requests into it and broadcasts what it appends to the log. `IsOver` becomes true when every Sleeper is Awake or Consumed, or when `Phase == Dawn`, at which point every `InDream` Sleeper is consumed.

## 9. Powers (budget and cooldowns only)

```csharp
public enum EffectKind : byte { Dark, Fog, Molasses }
public readonly record struct PowerTarget(int DreamId)   // −1 = All
{ public static PowerTarget All => new(-1); }

public sealed record EffectSpec(EffectKind Kind, int Cost, int DurationMs, int CooldownMs);
// defaults: Dark (3, 8000, 30000), Fog (2, 10000, 30000), Molasses (4, 6000, 30000)

public enum PowerError : byte { None, OnCooldown, NotEnoughBudget, NoSuchDream, Possessed, Disabled }

public sealed class Powers
{
    public Powers(IReadOnlyList<EffectSpec> effects, int triggerCooldownMs, int dreamCount);
    public PowerError ValidateEffect(EffectKind k, PowerTarget t, Budget b, int clockMs);
    public void ApplyEffect(EffectKind k, PowerTarget t, Budget b, int clockMs);   // spends once; starts the cooldown in every targeted dream
    public PowerError ValidateTrigger(Coord cube, PowerTarget t, int clockMs);
    public void ApplyTrigger(Coord cube, PowerTarget t, int clockMs);
    public int CooldownRemainingMs(EffectKind k, int dreamId, int clockMs);
    public int TriggerCooldownRemainingMs(Coord cube, int dreamId, int clockMs);
    public bool PossessionActive { get; set; }   // while true every other power is Disabled
}
```

What a trigger or effect *does* inside a dream is runtime code; Core only answers "may the Nightmare do this now, and what does it cost".

## 10. The host loop, in one page

```
on tick(deltaMs):            round.Advance(deltaMs); if round.IsOver → results
on Place(req) from Nightmare: v = round.TryPlace(req); reply(v); if v.Ok → broadcast(log.Last)
on Telemetry(s):             round.UpdateSleeperCube(s.Id, s.Cube); forward markers to Nightmare
on Explored(s, cube):        e = round.TryExplore(s, cube); if e == None → broadcast(log.Last)
on TouchedExit(s, door):     w = round.TryWake(s, door); reply(w); if Woke → broadcast(SleeperWoke)
on Died(s):                  d = round.ReportDeath(s); broadcast(d)
on Effect(k, t) / Trigger(c, t): validate via powers + budget; apply; forward to targeted dreams
```

Every client keeps its own `Lattice` by applying broadcast events through `EventLog.Replay` semantics and checks `Derived.Hash` against the host's after each event; a mismatch is a bug report with a `.lucidlog` attached.

## 11. Invariants

1. Every cube is reachable from Start over Attached doors (frontier-only placement).
2. Every Attached door has an Attached partner on the neighbour (fit rule).
3. No door is both Solidified and Attached.
4. After any valid placement at least one Exit exists and every InDream Sleeper can reach one (leak rule).
5. Exit doors are never in `Solidified`; the start cube is never in `Explored`.
6. `Replay(log).Hash == live.Hash` at every sequence number.
7. `Derive` is a pure function of `(Lattice, registry, hysteresis, previousExitDepth)`.

## 12. Tests M0.2 must include

- Faces: opposite, offset, rotation of faces and masks, all four rotations round-trip.
- Registry: rejects a one-connector non-start type; accepts the start type.
- Derive: depth on a line, a loop and across layers; Fog vs Solid; exits with ties; hysteresis holds and releases; hash stable across insertion order.
- Fit rule: connector-to-wall rejected; wall-to-wall accepted; two-sided fit when the new cube touches two neighbours.
- Frontier: placement into empty space rejected; onto Solid rejected; onto Attached rejected; onto Exit accepted and moves the exit.
- Leak rule: closing a loop that removes the last fog door rejected; a drop region keeps the exit beyond it; a placement that would strand a Sleeper below a drop rejected with the right `TrappedSleeper`; Sleepers not InDream ignored.
- Explore: solidifies only Fog doors; never Exit doors; start cube exempt; idempotent; explore-then-place gives `DoorIsSolid`, place-then-explore attaches.
- Budget: trickle accumulates integer remainders; cannot overspend.
- Round: head start blocks waking; dawn consumes; lives count down; `IsOver` in both endings; scoring matches spec §12.
- Powers: cooldown per dream; All consumes all; possession disables the rest.
- Log: write/read round-trip; replay hash equals live hash for a scripted 50-event game.

## 13. What Core deliberately does not do

Health, damage, trap states, mob AI, jams, door effects, rendering of door states, transport. Those live in `Lucid.Runtime` and `Lucid.Netcode`, and they read from Core, never write into it except through the `Round` methods above.
