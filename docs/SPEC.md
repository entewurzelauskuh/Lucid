# LUCID — Game Design Spec

**Version 1.0 — 2026-08-24 (consolidated)**

Legend: **[S]** settled · **[D]** default assumption, overrule freely · **[O]** open question

How the design evolved: `docs/HISTORY.md`. Decisions taken during implementation: `docs/DECISIONS.md`. Companion documents: §21.






---

## 1. One-liner

Lucid is an online party game for 2–5 friends. One player, the **Nightmare**, builds a dream in real time out of cube-shaped rooms and haunts it. The others, the **Sleepers**, each run the same maze alone, jumping and shooting their way to the deepest edge of the dream to wake up before dawn.

## 2. Snapshot

| | |
|---|---|
| Genre | Asymmetric online multiplayer: first-person platformer/shooter vs. real-time level builder |
| Players | 1 Nightmare + 1–4 Sleepers, 2–5 per lobby **[S]** |
| Session | Lobby → timed rounds (default 5 min **[D]**) → results → lobby |
| Setting | A shared dream. Any theme, style or genre may appear; dream logic excuses the mix. Not necessarily scary. **[S]** |
| Look | Painterly, soft-focus, dreamy, over realistic-ish assets **[S]** |
| Audience | Trusted friends. No anti-cheat; the host owns the maze, each Sleeper's machine owns its dream. **[S]** |
| Platform | PC (Windows first) on Steam, Unity 6 **[S]** |
| Input | Keyboard + mouse for both roles; gamepad for Sleepers **[S]** |
| Growth | Core game first, then themed Dream Packs as free updates, built with a Claude Code pipeline **[S]** |
| Development | Solo developer with Claude Code, hobby pace; public GitHub repository under MIT, contributions welcome **[S]** |

## 3. Design pillars

1. **The dream is built live, then haunted.** No level exists when the round starts. It grows as fast as the Nightmare can place cubes, and the Nightmare can trigger, possess and dim what it has built.
2. **The deepest edge is the exit.** The light shines only through the unused doors of the cube farthest from the start. The Nightmare must keep pushing that point ahead of the Sleepers; the Sleepers must reach it before dawn.
3. **Exploring is waking up.** Every cube a Sleeper enters loses its unused doors to the Nightmare. The dream can only grow ahead of the Sleepers, never behind them.
4. **Anything goes.** A castle corridor can open onto a subway platform onto a giant's kitchen. Every cube is *geometry × theme skin*, and one dream filter makes the mix read as one world.
5. **Party first.** Short rounds, readable rules, per-player results, nothing that needs a dedicated server or anti-cheat.
6. **Everything is a cube.** One lattice, one placement rule, one authoring pipeline. Scope stays sane, and new content is a folder and a brief.
7. **The Nightmare's attention is the scarcest resource.** Every build or power is one click or one key.

## 4. Core loop

1. Host opens a lobby; players join, pick a role, ready up.
2. Round starts. **Head start:** the Nightmare builds alone for 30 s (lobby-adjustable) while each Sleeper waits in their own start cube. **[S]**
3. **Run phase:** the Sleepers are released. The Nightmare keeps building and starts haunting; budget trickles in.
4. A Sleeper who walks through a white exit door **wakes** (safe, spectates). A Sleeper who loses their last life is **consumed** (out, spectates).
5. The round ends when every Sleeper has woken or been consumed, or when the timer reaches **dawn**: everyone still inside is consumed and sleeps restlessly for all eternity. **[S]**
6. Results, then back to the lobby. Roles are cleared; selection starts anew. **[S]**

**Win conditions [S]:** each Sleeper who wakes wins individually. The Nightmare wins the round if no Sleeper wakes.

## 5. Parallel dreams **[S]**

- The Nightmare builds **one** maze. Every Sleeper runs through their **own instance** of it, a private dream with the same layout. Sleepers cannot see or touch each other.
- **Each dream runs on its Sleeper's own machine.** The network carries the maze, the round, thin telemetry and the Nightmare's actions; nothing inside a dream is simulated anywhere else (§14).
- **Lattice state is global:** which cubes exist, which doors are attached, fog, exit or solid. It is the same in every dream, and every Sleeper's exploration shapes it (§7).
- **Dream state is private:** trap positions, jammed weak points, live mobs, the Sleeper's health and lives. A trapdoor one Sleeper jammed still works in another Sleeper's dream.
- Consequences: no leader to follow, no body-blocking, every Sleeper faces the whole maze; the Nightmare faces up to four independent runs at different points of the same maze; the player count can scale well beyond four later.
- Interaction between dreams (echoes, fractures of another Sleeper's run) is a future idea (§20).

## 6. Lobby

- The host creates a Steam lobby (friends-only or invite-only) and owns the maze and the round. Friends join through the Steam friends list, an overlay invite, or "Join game" on the host's rich presence. **[S]**
- Every player picks **Nightmare** or **Sleeper** and toggles **Ready**. **[S]**
- Start requires: all ready, at least one Nightmare, 1–4 Sleepers after resolution. **[S]**
- If several players picked Nightmare, one is chosen at random at start; the rest play as Sleepers. **[S]** The lobby is capped at 5 so this can never produce more than 4 Sleepers. **[D]**
- No automatic rotation. After each round the lobby reopens with roles cleared. **[S]**
- A session leaderboard persists in the lobby. **[D]**

### Lobby settings (host)

| Setting | Default | Range | |
|---|---|---|---|
| Head start | 30 s | 5–120 s | **[S]** |
| Round length (dawn) | 5:00 | 2–15 min | **[D]** |
| Sleeper lives | 1 | 1–5 | **[S]** |
| Starting budget | 12 | 0–40 | **[D]** |
| Budget trickle | 1 per 4 s | off – 1 per 1 s | **[D]** |
| Layers | −3 … +3 | configurable | **[D]** |
| Footprint | 24 × 24 cubes | 12–48 | **[D]** |
| Enabled packs | All installed | any subset | **[D]** |
| Theme set | Dream shuffle (random skins) | all / shuffle / pick one | **[D]** |
| Nightmare powers | Triggers, possession, effects all on | each on/off | **[D]** |
| Mob density | Normal | off / low / normal / high | **[D]** |
| Exit sense | Off | off / faint glow through walls | **[D]** |
| Gamepad aim assist | On | on/off | **[D]** |

## 7. The lattice

### Cubes and connectors

- The dream is a 3D grid of **cubes**: uniform, edge **C = 8 m** **[D]**, addressed (x, y, z). z is the **layer**; the start cube sits at (0, 0, 0). Layers extend downward **[S]** and upward **[D]**.
- A cube is a bounding box; interiors may be narrower (a corridor cube is a corridor inside an 8 m cube, not an 8 m hall).
- Each cube has six faces. A face is either a **connector** (a doorway) or **wall**. Every cube type has at least two connectors. One connector is the one the Sleepers come in through; the others are where the Nightmare may attach the next cube.
- Vertical connectors are always traversable **downward** (a drop). They are traversable **upward** only if the lower cube contains a climbable (ladder, stairs) reaching its ceiling. Climbability belongs to the lower cube. **[D]**
- The **start cube** is fixed, exempt from all rules below, and cannot be edited. **[S]** Proposal: the dreamer's bedroom with exactly one connector, the door, sealed by "dream mist" until the head start ends; Sleepers spawn and respawn there. **[D]**

### Fog doors **[S]**

Every connector that has no cube attached is a **fog door**: a wall of drifting mist in the doorway. There are four connector states.

| State | Look | Passable | Nightmare may attach |
|---|---|---|---|
| **Attached** | Open passage; the mist dissolved when the cube was placed | yes | – |
| **Fog** | Dark grey mist, solid to the touch | no | yes |
| **Exit** | Bright white light; same mist effect | yes → the Sleeper wakes | yes, which pushes the exit deeper |
| **Solid** | Wall in the cube's skin; the mist condensed into it | no | never again |

Transitions: Fog ↔ Exit whenever the depth ranking changes; Fog or Exit → Attached when the Nightmare builds on it; Fog → Solid when a Sleeper explores the cube. Exit doors never become Solid.

### The exit rule **[S]**

- **Depth** of a cube = number of cubes walked from the start cube along attached connectors, shortest path, ignoring direction. Recomputed on every lattice change.
- Among all cubes that still have at least one fog door, take the deepest. **All fog doors on those cubes are exits.** Ties are allowed, so several white doors may exist at once.
- When the Nightmare attaches a cube to an exit door, the new cube is deeper, so the exit moves onto its fog doors. Every other white door in the dream turns grey at that moment; Sleepers see the light shift.
- No hysteresis by default; see the yo-yo note below. **[O]** tuning.

### The explored rule **[S]**

- When a Sleeper enters a cube for the first time, every fog door of that cube that is **not** an exit **solidifies into wall**. The Nightmare has lost those connectors for good.
- This is **global** (§5): one Sleeper's foothold in the dream closes those doors in every dream. Otherwise the Nightmare would be building different mazes for different Sleepers.
- Exit doors never solidify; the start cube is exempt.
- Fantasy: each room you set foot in becomes real, and the dream can only keep growing where nobody has been yet.

### Placement rules

1. **Instant.** A placed cube exists immediately; the fog door it covers clears with a short dissolve. A Sleeper standing at that door can run straight in. **[S]**
2. **Fog doors only.** A cube may only be attached to a Fog or Exit door, never to a Solid one or into empty space, so every cube is reachable from the start. **[S]**
3. **Fit rule.** Every face of the new cube that touches an existing cube must match it: connector ↔ connector or wall ↔ wall. No doorway into solid wall. **[D]**
4. **Leak rule.** After the placement, every living Sleeper must still be able to reach at least one exit door from their current cube by legal movement (drops one-way, climbables two-way). Otherwise the placement is rejected. **[D]**
5. **Add only.** Cubes are never removed or swapped. **[D]**
6. **Budget.** The cube's cost must be affordable (§10).

Rejected placements show the Nightmare a red ghost with the reason.

### Why the rules hold together

- Every cube type has at least two connectors and the fit rule forbids doorway-to-wall contact, so each placement removes one fog door and creates at least one, except when it closes a loop. The leak rule handles loops. The Nightmare can never seal the dream; it can only push the exit deeper and put chicanes in front of it.
- **Drops are funnels.** Once a Sleeper falls through a drop, the leak rule forces the deepest point to stay beyond it: the deeper you fall, the deeper the exit. The rule is checked on **placement**, from where each Sleeper stands, so it constrains what the Nightmare may build — it does not constrain where a Sleeper may walk. A Sleeper who drops into a place with no way out has stranded herself, and that is a legitimate way to lose a life rather than a failure of the rule. **[S]**
- **Moving exits invite a yo-yo.** With two branches near maximum depth, the Nightmare can extend whichever branch the leading Sleeper is *not* in, for a cube or two per flip. The counter is the explored rule: a Sleeper who reaches a branch end after it turned grey kills that branch for good, so the Nightmare must keep spare branch ends out of reach, and every spare branch costs budget. Watch this in play-tests; if the yo-yo dominates, add hysteresis (a new deepest cube must beat the current exit depth by 2). **[O]**
- Running out of budget is the natural end of the Nightmare's ability to stall. The timer is the Nightmare's only way to win.

## 8. Cubes

A cube type is the unit of content.

| Field | Meaning |
|---|---|
| Connector mask | Which of the 6 faces are connectors; always ≥ 2 |
| Climbable | Whether the top connector can be climbed from inside (ladder/stairs) |
| Interior | Collision geometry + chicane logic; identical across skins |
| Weak point | Optional shootable part; destroyed = trap jammed in that dream for the rest of the round |
| Trigger | Optional manual trigger the Nightmare can fire (§10) |
| Cost | Budget points |
| Category | Connector / Vertical / Chicane / Mob / Gimmick |
| Pack | The Dream Pack it ships in (§16) |
| Skins | One or more theme skins (materials, props, lighting); cosmetic only |
| Rotation | 4 yaw rotations; top/bottom faces unaffected |

**Geometry × skin is the variety engine.** A Spike Pit exists once as collision and logic and appears as a castle dungeon, a subway maintenance pit, or a drawer full of pencils in a giant's desk.

**Connector standard [D].** Every doorway is centred on its face at floor level, 2.5 m wide × 3 m high, so any cube fits any other. Vertical connectors are a 2.5 m square hole centred on the face.

### MVP cube set **[D]**

**Connectors** (cost 1): Straight (2 opposite), Corner (2 adjacent), T (3), Cross (4).

**Vertical** (cost 1–2):
- **Drop** — floor hole, bottom connector, no climbable. One-way down; a funnel (§7).
- **Ladder shaft** — top and bottom connectors, climbable; stackable.
- **Stairwell** — one side connector, top connector, climbable stairs. Pairs with any cube that has a bottom connector.
- **Landing** — bottom connector plus side connectors; the receiving cube above a stairwell or ladder.

**Chicanes** (cost 2–4). Each is a jump-and-run problem beatable with the basic movement kit (§9). The weak point offers the Sleepers a trade: stand still and shoot it out, or run it with skill. Every chicane obeys the three laws of traps **[S]**: every harm has a tell of at least 250 ms, audible and visible; every cube is crossable with the basic kit and no shooting; a manual trigger skips the wait but never the warning. Components, parameters and validator rules are in `docs/CHICANES.md`.

| Cube | The problem | Weak point (jam effect) | Manual trigger |
|---|---|---|---|
| Spike Pit | Floor is spikes; cross via ledges and pillars | none | none |
| Gap | Chasm with platforms; a miss kills, or drops a layer in the drop variant | none | none |
| Trapdoor | Floor panel opens when stepped on; pit variant kills, drop variant sends you down | latch (panel locks shut) | drop it now |
| Timed spikes | Spikes on a rhythm | control box (spikes retract) | fire now, off-rhythm |
| Crusher | Pistons on a cycle | hydraulic line (pistons stall retracted) | slam now |
| Pendulum | Swinging blades over a walkway | chain (blade falls, becomes a static hurdle) | hold (freezes the blade at the bottom for 1.5 s) |
| Turret ("Eye") | Static shooter covering the cube | core (dead) | possess to aim by hand |
| Vent | Crouch crawl; slow but safe | none | none |
| Moving platforms | Platforms over a pit | none | none |
| Little maze | Folded corridor in one cube; costs time, not lives | none | none |

**Mob** (cost 3–5):
- **Nest** — spawns a wave of Shades when a Sleeper enters, and again every 20 s while the Sleeper is in the cube or a neighbour; at most 3 Shades alive per nest per dream. Weak point: the nest itself (no more waves; Shades already out remain). Trigger: spawn a wave now.

**Gimmick** (later): low gravity, flipped gravity, mirror room, dark room, giant furniture, conveyor floor, wind tunnel, a "gift" cube holding a bait pickup guarded by a Mimic.

### Mobs **[D]**

| Mob | Behaviour | HP | Damage | Notes |
|---|---|---|---|---|
| Shade | Melee chaser | 20 (2 shots) | 15 per hit | 5.5 m/s, slightly slower than a Sleeper. Leashed to its nest cube and neighbours; unleashed when possessed |
| Eye (turret) | Static; fires slow bolts (10 m/s) every 1.2 s | 60 (6 shots) | 20 per bolt | Lives in the Turret cube; possessable for manual aim |
| Later | Crawler (ceilings), Ghost (through walls, slow), Mimic (fake pickup) | | | |

Mob AI is deliberately local: each cube type ships a pre-baked navmesh piece, linked across connectors; the leash keeps pathing within one cube of home. Possession is how a mob leaves home.

### Theme skins, initial list **[D]**

Childhood bedroom, castle dungeon, subway, office at night, forest, giant's kitchen, hospital, spaceship. Skins are materials, props and lighting on the same collision. Later skins mostly arrive with Dream Packs (§16).

## 9. Sleepers

- **Movement [S]:** run, jump, crouch. No sprint, double jump, mantle or wall-jump. Defaults **[D]**: run 6 m/s; jump clears 1.2 m rise and ~4 m gap at full speed; crouch 1 m tall at 2.5 m/s. **Chicane design guideline:** every chicane must be beatable with exactly this kit — ledges ≤ 1.1 m, gaps ≤ 3.5 m, crawl spaces ≥ 1.1 m.
- **Input [S].** Keyboard + mouse or gamepad, fully rebindable. Gamepad gets gentle aim assist (magnetism on mobs and weak points, lobby-toggleable) since the Nightlight is hitscan. **[D]**
- **Health and lives.** 100 HP; regen starts 4 s after the last hit at 10 HP/s so a single life is not a slow bleed. **[D]** Pits, spike floors and crusher squishes kill outright; Shades, Eyes, timed spikes and pendulums deal damage. Zero HP = lose a life, respawn in the start cube after 3 s. Lives per lobby setting, default 1. Last life lost = consumed. **[S]**
- **Weapon: the Nightlight [D].** A flashlight that shoots. Infinite ammo, hitscan, semi-auto around 4 shots/s, 10 damage. It is also the only light source during Dark (§10).
- **Targets [S]:** mobs and trap weak points, in the Sleeper's own dream only.
- **Jamming.** A weak point takes about 6 shots, roughly 1.5 s of sustained fire while standing still. That pause is the Nightmare's window: a Shade wave or a manual trigger punishes hesitation, which is the whole point of giving the Nightmare both.
- **Reading the dream.** Grey mist: dead end for now. White light: the way out. Mist that hardens into wall as you enter: you are making the dream real. A door that suddenly clears: the Nightmare just built there. No minimap; *exit sense* is off by default (§6). **[D]**
- **Waking.** Walk through a white door. The Sleeper leaves the dream and spectates any dream with a free camera. **[S]** Spectators cannot act (future ideas in §20). **[S]**
- **Co-op.** Sleepers never meet, but they share a side: every cube one of them explores is a branch the Nightmare cannot use against the others. **[S]**

## 10. Nightmare

- **Input [S].** Keyboard + mouse; the god view, palette and target selector are mouse-driven. Gamepad support for the Nightmare is a later feature (§20).
- **View.** Free orbit/top-down camera over the one lattice with a layer slider (cut-away above the selected layer). Each Sleeper is a marker in their own colour; hovering a marker peeks at that dream's state (mobs alive, jammed traps). Fog and exit doors highlighted as placement targets. **[D]**
- **Palette.** GUI panel with one tab per enabled pack and category filters, cost badges, hotkeys. Select a type → hover a fog door → rotate → confirm. **[S]**
- **Budget.** Starting pool plus steady trickle. **[S]** Defaults: 12 points at start, +1 point every 4 s. **[D]** Tuning intent: a corridor costs the Nightmare more budget-time than it costs a Sleeper to run through, so the Nightmare *must* use chicanes to stay ahead.
- **Constraints.** Placement rules §7. Cannot touch the start cube. Cannot remove cubes. Cannot build on solid doors.

### Live powers **[S]**, details **[D]**

One economy for everything: cubes and global effects both spend budget points, so every effect is a room not built. Triggers and possession cost no budget; their cost is the Nightmare's attention.

**Target selector [S].** The Nightmare's HUD has a target selector: **All** (default at round start) or one Sleeper's dream. **Tab** cycles All → Sleeper 1 → … → All, **0** returns to All, and clicking a Sleeper's marker or panel row selects that dream; the number row **1–9** is reserved for the palette, which is used far more often (UI doc §8). **[S]** The selected dream and its mob markers are highlighted in the god view. Triggers and effects apply to the selection; possession always enters one dream. **[D]** Costs are flat per use; cooldowns are per power per dream, and All consumes them everywhere. So All is the efficient choice and targeting is the precise one: Molasses on the one Sleeper mid-gap, Dark on the one shooting out a weak point, then the same effect on another dream a moment later at full price.

**Manual triggers.** Hover a trap cube and press T, or click its button in the god view. Free, per-trap-per-dream cooldown 6 s. A jammed trap does not fire in the dream where it was jammed. Trigger list in §8.

**Possession.** Click a Shade or Eye marker (coloured by dream) and press P. The Nightmare drops into first person inside that Sleeper's dream and controls the body until it presses P again or the body dies (2 s blackout, then back to the god view). While possessed: no placement, no triggers, no effects; the budget trickle continues. A possessed mob is unleashed and may roam that whole dream. Possession is a gamble by design: the maze stops growing for everyone while you play monster in one dream, so it pays when you have built a lead or need to finish a wounded Sleeper who is shooting out a weak point.

**Global effects.** Hotkeys Q / W / E, each with its own 30 s cooldown per dream.

| Effect | Duration | Cost | What happens |
|---|---|---|---|
| Dark | 8 s | 3 | All lights out; Sleepers see only by Nightlight. Fog doors still glow |
| Fog | 10 s | 2 | Sight radius ~6 m; door colours unreadable beyond it |
| Molasses | 6 s | 4 | Sleepers move at 70 %, which also shortens their jump. A gap that needs full speed becomes lethal: the right play is to wait it out, which is the time the Nightmare bought |

## 11. Round timeline **[D]**

| Time | Event |
|---|---|
| 0:00 | Round starts. Nightmare builds. Sleepers wait in their start cubes, door misted, countdown visible to all. |
| 0:30 | Mist drops. Sleepers run. Trickle continues. |
| … | Sleepers explore, wake or are consumed. Nightmare builds ahead and haunts behind. |
| 5:00 | Dawn. Remaining Sleepers consumed. |
| +10 s | Results screen → lobby. |

## 12. Scoring **[D]**

- Sleeper wakes: 100 + remaining seconds. Consumed: 0.
- Nightmare: 100 per consumed Sleeper.
- The session leaderboard sums across rounds. Because Nightmares are chosen by selection rather than rotation, show "rounds as Nightmare" next to each score so uneven role counts are visible.

## 13. Fair play

Trusted lobby, no anti-cheat. The host validates the §7 rules, so even a modified client cannot place illegal cubes. Each Sleeper's machine is trusted about everything that happens inside its own dream: health, deaths, jams, and having reached a door.

## 14. Technical notes — Unity **[S]** engine and Steam, **[D]** details

**Stack.** Unity 6 (6000.x), URP, Input System, AI Navigation, Addressables. Netcode for GameObjects 2.x in host mode. Facepunch.Steamworks for the Steam API; the community Facepunch transport for NGO from Unity's multiplayer-community-contributions repository (community-maintained, so pin it against the NGO version in use; the fallback is a thin transport over SteamNetworkingSockets, or Unity Transport with UGS Relay). Steam lobbies, invites and rich presence for joining; Steam's relay handles NAT traversal.

**Steam practicalities.** A Steam app (Steam Direct) is needed early, since lobbies and invites are the join mechanism from M1 on. Use a private beta branch for friends' play-tests; dev builds carry `steam_appid.txt`. Cloud saves, achievements and stats are later extras.

**Dev loop and Steam-free mode.** The transport is selectable: Unity Transport on LAN/localhost for development and for contributors without access to the Steam app, Facepunch for real sessions. Unity's Multiplayer Play Mode (virtual players in one editor) is the day-to-day loop for one Nightmare plus one or two Sleepers. **Sandbox** is a single-player, offline mode: build with unlimited budget and no timer, press F5 to drop into the lattice as a Sleeper and F5 to return; it is the tool for trying cubes and for contributors without a second player (UI doc §12). **[S]**

**Architecture in one sentence [S].** Each dream is a single-player simulation running on its Sleeper's machine; the network carries only the maze, the round state, thin telemetry and the Nightmare's actions.

**What the host owns.** Lobby and roles; round timer and phase; the lattice event log; budget, cooldowns and power validation; scores; adjudication of exploration and waking against the lattice.

**What each Sleeper's client owns.** Its whole dream: physics, traps, mobs, damage, health, lives, respawns. None of it is networked as NetworkObjects; it is plain local gameplay. It sends the host `Explored(coord)`, `Died`, `TouchedExit(coord, face)`, and a 10 Hz telemetry packet (position, health, lives, live mobs, jammed traps) that feeds the Nightmare's markers and hover-peek.

**What the Nightmare's client owns.** The god view and the palette. It sends requests — `Place(coord, type, rotation)`, `Trigger(coord, target)`, `Effect(kind, target)`, `Possess(dream, mobId)` — and applies nothing locally until the host confirms. Placement latency is one round trip, tens of milliseconds between friends.

**Lattice replication.** An ordered event log, `CubePlaced(coord, type, rotation, skin)` and `CubeExplored(coord)`, from which every client derives connector states deterministically: depth BFS → exits, solidified doors. No per-door state is replicated; a few hundred nodes recomputed per event. Host ordering resolves races: an exploration that arrives before a placement on the same door solidifies it and the placement is rejected; a placement that arrives first attaches the cube and the exploration finds no fog door left. A `TouchedExit` is confirmed only if the door is still an exit when it arrives; if a placement beat it, the client rolls the Sleeper back into the new cube's doorway. The log doubles as a replay file for bug reports and balancing.

**The rules live in a pure C# assembly.** `Lucid.Core` holds the lattice, cube definitions, the event log and every rule (fit, leak, exits, explored, budget) with no Unity dependency, so each rule is unit-tested and Claude Code can develop and verify them without opening the editor. Unity code only renders and transports what Core decides. The API surface, algorithms, invariants and required tests are in `docs/CORE-API.md`, accepted as written. **[S]**

**Powers across dreams.** The host validates cost and cooldown, then forwards `Trigger` and `Effect` to the targeted dream or to all. Dark and Fog are client-side rendering; Molasses is a movement modifier the dream applies locally.

**Spectating and possession share one relay.** A client subscribes to a dream; the host asks that dream's owner to publish a 20 Hz view stream (Sleeper pose, mob poses, re-phased movers, chicane state deltas) and relays it to every subscriber. Nobody streams a dream nobody watches. The round clock is NGO's synchronised server time, so a spectator's copy of every cycling trap runs in step without streaming it. Message by message in `docs/NETCODE.md`.

**Possession, the one client-to-client feature.** The Nightmare's client loads a local rendering copy of the target dream: static geometry from the lattice, identical by construction, plus dynamic state streamed from the Sleeper's client at 20–30 Hz (Sleeper transform, mob transforms, trap phases, jammed flags). The Nightmare sends inputs at 30 Hz; the Sleeper's client simulates the possessed mob authoritatively and echoes its transform. For the Eye, aiming and hit detection run on the Nightmare's client against the streamed Sleeper transform and damage is sent as an event, which hides the round trip. Expect 50–150 ms of movement latency; acceptable for a melee chaser if the Shade's lunge is forgiving. Alternative to evaluate at M3: NGO's distributed-authority topology, where each Sleeper's client owns its dream objects natively and possession becomes an ownership transfer. **[O]**

**Input.** Input System action maps: `Sleeper` (KBM + gamepad), `Nightmare` (KBM), `UI` (both). Rebinding screen for both. Steam Input glyphs and configs once the Steam build exists.

**Rendering.** URP Forward+, the dream filter as a global post-processing volume with per-skin overrides (§15), realtime lights, per-cube reflection probes baked into prefabs, SSAO. No scene lightmaps: cubes are instantiated at runtime.

**Content.** `CubeDefinition` and `SkinSet` ScriptableObjects, `DreamPack` registries, one Addressables group per pack. Per-cube navmesh instantiated with `NavMesh.AddNavMeshData` at the cube's transform, `NavMeshLink`s across connectors, leash as a radius check. Authoring pipeline in §17.

**Character controller.** Unity's `CharacterController` covers run, jump, crouch; swap in a kinematic controller only if platforming feel demands it.

**Scaling.** The host's cost per Sleeper is one telemetry stream, so the design supports many more than four Sleepers.

## 15. Art and audio direction **[S]** direction, **[D]** execution

**Direction.** Painterly, soft-focus, dreamy, on top of realistic-ish assets. The two solve each other's problem: free store assets arrive in many styles and qualities, and one strong, consistent post-process is what makes a subway platform, a castle corridor and a giant's kitchen read as one dream.

**The dream filter.** One URP post-processing profile applied everywhere, with per-skin overrides:
- Soft bloom, gentle depth of field (focus follows the look direction; softness grows toward the screen edges), light film grain, a touch of lens distortion, vignette.
- A painterly pass as a custom render feature (Kuwahara / oil-paint at low strength), exposed as a quality setting. It is the difference between "photo" and "painting" and it costs GPU time.
- Per-skin colour grading LUT, fog colour and light temperature: these make the Backrooms skin sickly yellow and the castle skin candle-warm.
- The Nightmare's god view gets a lighter filter so the lattice stays readable; the Sleeper's first person gets the full treatment.

**Lighting.** Realtime lights, 2–4 per cube, colour and intensity set by the skin; per-cube reflection probe; SSAO. If this proves too expensive on target hardware, prefab-level lightmap baking is the escape hatch. **[O]**

**Signature VFX: the mist.** Fog doors are the one effect the whole game hangs on: a sheet of drifting mist in the doorway (layered scrolling noise on a stack of quads), dark grey or bright white, dissolving on attach, condensing into the wall material on solidify. Worth real polish time.

**Own art.** Only what is simple: shell geometry (walls, floors, ceilings, door frames) generated from the connector mask and textured with CC0 PBR sets. Everything else — props, furniture, machinery, creatures — comes from free asset sources through the pipeline in §17.

### Audio

**SFX first, music much later.** The soundscape carries the round before any score exists: the mist (a low airy loop per fog door, brighter for exits), the door dissolve and the door hardening (the one sound every Sleeper learns to dread), the trickle tick and placement thud for the Nightmare, trap tells (a warning tick before timed spikes, a hydraulic hiss before a crusher), the Nightlight's snap, Shade chitter, and the dawn swell. Every trap has an audible tell so the chicanes are fair with the basic movement kit. Music, when it comes, is one ambient bed per skin and a dawn cue; it gets its own slider then and not before.

## 16. Content strategy: Dream Packs **[S]**

- **Base game** ships the core loop, lobby and UI, the MVP cube set (§8) and 2–3 skins.
- After that, growth is mostly **Dream Packs**: themed bundles of cubes, skins, mobs and occasionally a new mechanic, built through the pipeline in §17. Working titles: *Cthulhu Dream* (drowned chapel, tentacle pendulum, cultist nest), *Endless Office Hours* (Backrooms: buzzing yellow corridors, little maze, vents, fluorescent Dark), *Giant's Kitchen*, *Night Shift* (hospital).
- A pack is a `DreamPack` asset listing its cubes, skins and mobs, shipped as one Addressables group. The palette shows one tab per enabled pack; the host chooses enabled packs in the lobby. **[D]** mechanics.
- **Packs are free updates inside the base build.** Everyone on the current Steam build has every pack; nothing to buy, no version mismatches between friends. Cosmetic DLC, if ever, is a separate question.
- The other growth line is overarching features: fractures between dreams, woken-Sleeper powers, Nightmare gamepad support (§20).

## 17. Cube authoring pipeline with Claude Code **[S]** intent, **[D]** design

Goal: drop free assets into a folder, write a short brief, and have Claude Code produce a validated, registered cube prefab the Nightmare can place. The same path is how contributors add cubes (§18).

**Principle: Claude writes specs and code, never prefab YAML.** Prefab files are GUID-and-fileID soup. A declarative spec plus an editor-side builder is deterministic, reviewable in git, and re-runnable whenever the template changes.

**Folder convention**

```
Assets/_Lucid/Packs/<Pack>/Cubes/<CubeName>/
  brief.md              what this cube is for, the chicane idea, references
  assets/               redistributable assets (CC0, CC-BY), committed
  assets/LICENSES.md    source URL + license per asset; the builder refuses unlisted assets
  assets.manifest.json  non-redistributable assets: source URL, license, expected hash; fetched, never committed
  cube.spec.json        written by Claude from the brief and the assets
  <CubeName>.prefab     generated
  <CubeName>.asset      generated CubeDefinition
  Previews/             generated screenshots for review
```

**The spec** (`cube.spec.json`; schema in `docs/cube-spec.schema.json`, guide and worked examples in `docs/CUBE-SPEC.md`): id, pack, category, cost; connector mask and climbable; shell (material roles for wall / floor / ceiling / trim, door-frame style); props (asset path, position, rotation, scale, collider mode); chicane (a component from the library plus parameters, e.g. `TimedSpikes { periodMs: 2000, warningMs: 400 }`); weak point (which prop, HP, jam effect); trigger; intended path (a polyline from the entrance to each connector, used by the validator); skins (which SkinSets the cube supports, default all); nav (agent radius, links).

**The template.** `CubeTemplate.prefab`: root with an 8 m bounds gizmo; `Shell` (generated); `Sockets/` with six `Connector` sockets at the standard positions, each carrying a `FogDoor`; `Interior/`; `Logic/`; `Nav/` with a `NavMeshSurface`. Versioned; rebuilding every cube after a template change is one command.

**The builder.** `Lucid.Editor.CubeBuilder.BuildFromSpec`, run in Unity batch mode from the command line (`Unity -batchmode -quit -projectPath . -executeMethod … -spec <path>`), or through a Unity MCP bridge if editor round trips get tedious. Steps: instantiate the template → generate the shell from the connector mask → apply skin roles → place props → add and configure the chicane, weak point and trigger components → bake the navmesh and links → save the prefab and `CubeDefinition` → register in the `DreamPack` → run the validator → render previews from three fixed cameras.

**The validator** writes a JSON report that Claude reads and acts on: geometry inside the cube bounds; at least two connectors at standard positions, each with a fog door; shell collision present; navmesh baked and linked across connectors; declared weak point and trigger wired to live components; intended path within the kit limits (gaps ≤ 3.5 m, ledges ≤ 1.1 m, crawl ≥ 1.1 m); triangle budget (≤ 60 k per cube **[D]**); every asset has a license entry and every committed asset is redistributable. Later: a scripted agent that actually runs the intended path.

**Asset normalization.** `AssetNormalizer` editor script for anything dropped into `assets/` or fetched from the manifest: scale to metres, pivot to base centre, URP material upgrade, lightmap UVs off, simplified colliders (convex or box), sensible import settings, triangle count recorded. Preferred sources: CC0 PBR texture and model libraries (Poly Haven, ambientCG) for shells; the Unity Asset Store free section, Sketchfab CC0/CC-BY, itch.io and similar for props; the license ledger records each.

**CLAUDE.md** at the repo root carries all of this as instructions: conventions, commands, the spec schema, the component library and how to extend it, the rule about never hand-editing prefab YAML, and how to read the validator report and preview images. Adding a new mechanic means adding a component to the library plus its validator hooks; from then on any cube can use it.

**The loop in practice:** you write `brief.md` and drop assets → Claude writes the spec → builds → reads the report and the previews → fixes → you play it.

## 18. Open source and contributions **[S]**

- **Public repository on GitHub** from day one. Claude Code creates and maintains it: repository, issues, milestones, branches, pull requests (see the work plan).
- **Licensing.** Code under **MIT**; original text, briefs, specs and generated shell textures under **CC-BY-4.0**; third-party assets under their own licenses, listed in `THIRD_PARTY_NOTICES.md`.
- **Review flow.** Every change is one pull request for one issue, opened by Claude Code and reviewed by the owner. Nothing reaches `main` that the owner has not looked at. The owner decides; the merge itself is performed on their word, for the pull request they name (`docs/DECISIONS.md`, 2026-09-03). Claude Code never merges unasked and never pushes to `main`.
- **Tests run locally.** `tools/run-tests.sh` before every pull request; no CI at hobby pace. Adding GitHub Actions later is a one-file change once contributors appear and a Unity license secret is worth setting up.
- **The asset rule.** Free does not mean redistributable. Unity Asset Store assets, including free ones, are typically licensed for use in builds, not for redistribution in source form, so they cannot live in a public repository. CC0 and CC-BY assets can — **plain CC0 or plain CC-BY only. NonCommercial, NoDerivatives and ShareAlike are all refused.** NC would make the whole artefact non-commercial, which an MIT repository is not; ND forbids the rescaling, re-pivoting and material upgrades §17's pipeline performs on everything it ingests; and SA, though it is redistributable and so passes the test the other two fail, propagates its own terms through that same pipeline: what it adapts is Adapted Material, which would have to ship SA instead of the CC-BY-4.0 this section grants for generated shell textures (`docs/DECISIONS.md`, 2026-09-03). The gate reads the licence column of the ledger row and nothing else, so a row is `| file | source | licence |`. Hence the split in §17: redistributable assets are committed with a license entry; everything else is referenced by `assets.manifest.json` and fetched locally by `tools/fetch-assets`. A pre-commit hook enforces it.
- **Contribution path = the cube pipeline.** A contributor adds a folder with a brief, assets or a manifest, and a spec; the owner builds, validates, reviews previews and merges. `CONTRIBUTING.md` walks through making one cube end to end, and it is written by doing exactly that (work plan M4).
- **Contributors need no Steam access.** The Unity Transport dev mode (§14) runs the whole game on LAN or localhost; the Facepunch path needs the owner's Steam app.
- **Community hygiene.** Contributor Covenant code of conduct, issue templates (bug, feature, cube proposal), a `good first issue` label kept stocked with small cubes and skins.
- **No in-game chat.** No voice, no text; a friends' lobby already lives on Discord. Steam voice or a lobby text box can be revisited if strangers ever play together.

## 19. Open questions

1. Exit hysteresis, only if the yo-yo dominates in play-tests.
2. Possession transport: custom remote control (default) or NGO distributed authority.
3. Lightmapping, only if realtime lighting proves too expensive.
4. Tuning: round length, budget, mob and weapon numbers all need play-testing.

## 20. Future ideas (parking lot)

- Woken Sleepers acting from the outside: disabling a trap, calling from beyond a door.
- Fractures between dreams: a Sleeper glimpses an echo of another Sleeper's run, or finds a door the other opened.
- More than four Sleepers; parallel dreams already allow it.
- Gamepad support for the Nightmare.
- Gimmick cubes and bait "gift" cubes.
- Steam Workshop for community cubes, once the pipeline is solid.
- Cosmetic DLC, if ever.
- GitHub Actions test runs, once contributors show up.

## 21. Companion documents

| Document | Path | What it settles |
|---|---|---|
| UI screens and flows | `docs/UI.md` | Every screen, HUD, key binding, toast string and edge flow. Its rules: the world is painted, the UI is crisp; one glance for the essentials; never hide a rule; results readable across a Discord call; door states never depend on hue alone |
| Core API | `docs/CORE-API.md` | `Lucid.Core` types, rules as pure functions, algorithms, invariants and the required tests; accepted as written, M0.2 implements it |
| Cube spec | `docs/CUBE-SPEC.md`, `docs/cube-spec.schema.json` | The declarative cube format with worked examples; M0.3 builds the pipeline to the schema |
| Chicane library | `docs/CHICANES.md` | Trap components, their parameters, tells, weak points, triggers, validator rules and tests; mobs and verticals |
| Netcode | `docs/NETCODE.md` | Every message between host, Nightmare and Sleepers; approval, reconnect, failure handling, tests |
| Playtests | `docs/playtests/PLAN-M0.md` | The M0 session: hypotheses, protocol, metrics, decision rules. Later milestones end with a session planned the same way; the tunables live only in the dev tuning console, never in the shipped lobby |
| Work plan | `docs/WORKPLAN.md` | Milestones, tasks, acceptance criteria, GitHub conventions, the first session for Claude Code |
| Operating instructions | `CLAUDE.md` | What Claude Code reads first and the rules that do not bend |
| History and decisions | `docs/HISTORY.md`, `docs/DECISIONS.md` | How the design evolved; deviations taken during implementation |

## 22. Milestones

Summary only. The task-level plan with acceptance criteria and GitHub conventions is `docs/WORKPLAN.md`; the operating instructions for Claude Code are in `CLAUDE.md`.

- **M0 — the loop and the foundation.** Repo, `CLAUDE.md`, `Lucid.Core` rules with tests, the cube pipeline, fog doors, one Nightmare + one Sleeper over a local transport, a full round end to end.
- **M1 — Steam, parallel dreams and cubes.** Steam lobby and invites, roles and settings, 2–4 Sleepers in local dreams, results, the MVP cube set, manual triggers with the target selector, lives, 2 skins.
- **M2 — the shooter.** Nightlight, weak points, Nest + Shade, Eye, health, gamepad, first audio pass, mist polish.
- **M3 — the haunting and the look.** Possession, global effects, the dream filter, performance and polish.
- **M4 — the first Dream Pack.** One themed pack built entirely through the pipeline, and `CONTRIBUTING.md` written by doing it.
