# LUCID — Netcode Message Catalogue

**Version 1.0 — 2026-08-24 (consolidated).** Design context: spec §5 (parallel dreams), §14 (architecture), `docs/CORE-API.md` §7–§10 (events, round, powers), `docs/UI.md` §13 (edge flows). The work plan's M0.8 implements the round core, M1.3–M1.4 the session and dreams, M3 the view streams.

The whole protocol fits one sentence from the spec: *each dream is a single-player simulation on its Sleeper's machine; the network carries the maze, the round state, thin telemetry and the Nightmare's actions.* This document is the list of exactly what that means on the wire.

---

## 1. Topology and channels

- **Netcode for GameObjects 2.x, client-server, host mode.** The host is a player like any other (usually the lobby creator) and runs `Lucid.Core.Round`. There is no dedicated server and no distributed authority.
- **Every message goes through the host.** Clients never talk to each other; the two client-to-client features (spectating a dream, possession) are relays the host performs.
- **Transport:** Facepunch (Steam) in sessions, Unity Transport on LAN or localhost in development and for contributors (spec §14).
- **Channels:** *reliable ordered* for everything that changes state (events, requests, replies, status), *unreliable sequenced* for streams (telemetry, dream views, possession input), where a late packet is dropped by sequence number rather than delivered.
- **Time:** NGO's synchronised server time is the round clock. `RoundStart` carries the server-time stamp of t = 0; `Dream.Clock` on every machine is `ServerTime − roundStartTime`, which is what chicane cyclers read (`docs/CHICANES.md` §2). No custom clock message is needed.
- **Identity:** `PlayerId` is the NGO client id for the session; the approval payload maps it to a Steam id and display name. `DreamId` is the Sleeper's lobby index 0–3; `DreamId = −1` means "all dreams" in power targets.

## 2. Connection and approval

NGO's `ConnectionApprovalCallback` receives a `Hello` payload and answers with approve or reject plus a reason string shown on the client.

```
Hello         C → H   buildId (string), contentHash (uint64), steamId (uint64), displayName (string), protocolVersion (uint16)
```

Rejections: `"Different version: update Lucid on Steam"` (buildId or contentHash differ; packs are part of the build, spec §16, so the hashes must match exactly), `"Lobby is full"`, `"Protocol mismatch"`. A player who reconnects during their disconnect grace is recognised by `steamId` and resumed (§10). A player joining while a round runs is approved into the lobby and told `RoundInProgress`.

## 3. Lobby state

Lobby state is replicated, not messaged: one `SessionState` NetworkBehaviour owned by the host.

| Replicated | Type | Content |
|---|---|---|
| `Players` | `NetworkList<PlayerEntry>` | playerId, steamId, name, role (None / Nightmare / Sleeper), ready, dreamId (−1 until a round starts), connected |
| `Settings` | `NetworkVariable<LobbySettings>` | every host setting from spec §6, plus the preset name |
| `Leaderboard` | `NetworkList<ScoreEntry>` | playerId, points, roundsAsNightmare, woke, consumed |
| `Phase` | `NetworkVariable<SessionPhase>` | Lobby, Starting, HeadStart, Running, Dawn, Results |

Client requests that change it:

```
RoleSelect       C → H   role
ReadySet         C → H   ready
```

The host validates start conditions locally (spec §6) and writes `Phase = Starting`. Late joiners receive `RoundInProgress (remainingMs)` once and wait.

## 4. Round lifecycle

```
RoundStart       H → all   settingsSnapshot, seed (uint32), nightmarePlayerId, sleepers[dreamId → playerId],
                           startTypeIndex, startRotation, registryHash (uint64), roundStartServerTime (double)
PhaseChanged     H → all   phase (HeadStart | Running | Dawn), atServerTime
SleeperStatus    H → all   dreamId, status (InDream | Awake | Consumed | Disconnected), livesLeft, atClockMs, cause
RoundEnded       H → all   outcomes[dreamId → (status, atClockMs)], scores[playerId → points], badges, consumedByNightmare,
                           replayAvailable (bool)
ReturnToLobby    H → all   (no payload; clients clear roles and ready)
```

`RoundStart` is what lets every client build its own `Lattice.New(...)` and `Round` mirror. `registryHash` is the ordered hash of every registered cube type id; it must equal the host's or the client disconnects itself with a version error (it cannot differ if `contentHash` matched, so this is a guard).

## 5. Lattice replication and sync checks

The lattice is an ordered event log (`docs/CORE-API.md` §7). The host applies a validated event to its `Round`, then broadcasts it.

```
LatticeEvent     H → all   seq (uint32), kind (Placed | Explored), payload, postHash (uint64)
HashReport       C → H     seq, hash
DesyncNotice     H → all   seq, playerId
```

- Clients apply events in `seq` order; a gap means a lost reliable message, which NGO does not allow, so a gap is a bug and triggers `ResumeRequest` (§10) as recovery.
- After applying an event a client computes `Derived.Hash` and sends `HashReport`. The host compares it with `postHash`; on mismatch it broadcasts `DesyncNotice`, saves the `.lucidlog` automatically and shows a toast. Events are rare (single digits per second at most), so a report per event costs nothing.
- Payloads use registry indices instead of strings: `typeIndex (uint16)`, `skinIndex (uint16)`. `Coord` is three `int8` (footprint half ≤ 48, layers ±3).

## 6. Sleeper ↔ host

Up, from the Sleeper's client (the dream's owner):

```
DreamReady       S → H   dreamId                                     reliable, once the dream scene is built
Telemetry        S → H   seq (uint16), cube (3×int8), localPos (3×uint16, 1/256 m), yaw (uint8), health (uint8),
                         lives (uint8), status (uint8), flags (uint8)   unreliable, 10 Hz, 16 bytes
Explored         S → H   cube                                        reliable
TouchedExit      S → H   cube, face                                  reliable
Died             S → H   cause (uint8)                               reliable
ChicaneDelta     S → H   cube, componentIndex (uint8), state (uint8), jammed (bool)   reliable, on change only
```

Down, to the Sleeper's client:

```
WakeVerdict      H → S   accepted (bool), reason (uint8: NotAnExit | NotInDream | HeadStart)   reliable; on reject the client rolls the Sleeper back into the doorway
DeathVerdict     H → S   lostLife (bool), livesLeft, respawnDelayMs  reliable; consumed = !lostLife
TriggerFire      H → S   cube, kind (uint8)                           reliable; routed from a Nightmare request
EffectStart      H → S   kind (uint8), durationMs                     reliable; routed
ViewSubscribers  H → S   count (uint8)                                reliable; > 0 means "publish DreamView" (§8)
PossessionBegin  H → S   mobId (uint16), nightmarePlayerId            reliable
PossessionEnd    H → S   reason (Released | Died | Disconnected)      reliable
```

The host uses `Telemetry.cube` to keep `SleeperState.Cube` current for the leak rule (`Round.UpdateSleeperCube`) and fans the pose out as markers (§7). It answers `Explored` by running `Round.TryExplore`, `TouchedExit` by `Round.TryWake`, `Died` by `Round.ReportDeath`.

## 7. Nightmare ↔ host

Up:

```
PlaceRequest     N → H   reqId (uint16), targetCube, targetFace, typeIndex, rotation, skinIndex     reliable
PowerRequest     N → H   reqId, kind (Trigger | Effect), cube or effectKind, targetDreamId (int8)  reliable
PossessRequest   N → H   dreamId, mobId                                                             reliable
ReleaseRequest   N → H   (none)                                                                     reliable
```

Down:

```
PlaceReply       H → N   reqId, verdict (uint8 PlaceError), trappedDreamId (int8)    reliable
PowerReply       H → N   reqId, error (uint8 PowerError)                             reliable
PossessReply     H → N   accepted, mobId, reason                                     reliable
BudgetState      H → N   points, msUntilNextPoint, effectCooldowns[dream][kind] (ms), possessionActive   reliable, after every change and at 1 Hz
Markers          H → N   count, per Sleeper: dreamId, cube, localPos, yaw, health, lives, status   unreliable, 10 Hz, ≈ 60 bytes
ChicaneSummary   H → N   forwarded ChicaneDelta with dreamId                                       reliable
```

The Nightmare client applies nothing locally until `PlaceReply` and the matching `LatticeEvent` arrive; the ghost turns red with the reason on rejection. Trigger cooldowns are not sent separately: the Nightmare's trap buttons show the cooldown from `PowerReply` timestamps, and `BudgetState` carries effect cooldowns because those have a cost.

## 8. Dream view streams: spectating and possession

Watching a dream and possessing a mob in it use one mechanism. A client **subscribes** to a dream; the host asks that dream's owner to publish a view stream and relays it to every subscriber.

```
SubscribeDream   C → H          dreamId (int8, −1 = none)                       reliable
DreamView        S → H → subs   seq (uint16), clockMs (uint32),
                                sleeperPose (cube, localPos, yaw, pitch),
                                mobs[count ≤ 12]: mobId (uint16), localPos, yaw (uint8), state (uint8),
                                movers[count ≤ 8]: actorIndex (uint8), phase (uint16),
                                chicaneStates delta                             unreliable, 20 Hz, ≈ 150–300 bytes
PossessInput     N → H → S      seq (uint16), move (2×int8), lookYaw (uint8), lookPitch (int8), buttons (uint8)   unreliable, 30 Hz, 8 bytes
PossessedHit     N → H → S      mobId, damage (uint8), hitPos (localPos)         reliable
```

- A Sleeper's client publishes `DreamView` only while `ViewSubscribers > 0`. Nobody pays for streams nobody watches.
- Subscribers rebuild the dream's static geometry from the lattice they already have and animate the streamed poses. Trap actors driven by `Cycler` need no streaming at all: they read `Dream.Clock`, so a spectator's copy runs the same rhythm; only the `movers` that a targeted trigger re-phased are corrected by the `phase` field.
- Spectators see; the possessing Nightmare also sends `PossessInput`. The owner's client simulates the possessed body and echoes its pose in the next `DreamView`. The Eye's hits are decided on the Nightmare's client against the streamed Sleeper pose and delivered as `PossessedHit` (spec §14).
- The Nightmare's hover-peek does not subscribe; it reads `ChicaneSummary` and `Markers`.

## 9. Power routing

1. Nightmare sends `PowerRequest` with an explicit target (`docs/UI.md` §8: Tab selects, the request carries the selection).
2. Host validates through `Powers` and `Budget` (`docs/CORE-API.md` §9), spends, starts cooldowns in every targeted dream, replies `PowerReply`, updates `BudgetState`.
3. Host sends `TriggerFire` or `EffectStart` to the targeted dream owners, and `EffectStart` also to subscribers of those dreams and to the Nightmare for toasts.
4. Each dream applies the effect locally: Dark and Fog as rendering, Molasses as a movement modifier, a trigger as `Chicane.OnTrigger(kind)`.

Effect start times are stamped with `clockMs`, so a dream that receives the message 80 ms late still ends the effect at the right moment.

## 10. Reconnect and late join

```
ResumeRequest    C → H   lastSeq (uint32)
ResumeSnapshot   H → C   chunked: chunkIndex, chunkCount, bytes   reliable
```

- **Sleeper disconnect:** the host marks the dream `Disconnected` and starts a 30 s grace (`docs/UI.md` §13). If a `Hello` with the same `steamId` arrives in time, approval resumes the player: `RoundStart` again, then a `ResumeSnapshot` holding every `LatticeEvent` from `seq 1` (a few kilobytes), the current `SleeperState` (lives, status) and the round clock. The client rebuilds the dream and respawns in the bedroom; its exploration history is irrelevant because solidification is global and already in the log. Grace expired → `SleeperStatus Consumed`.
- **Nightmare disconnect (not the host):** the host ends the round without scores, `RoundEnded` with `consumedByNightmare = false` and an outcome of `Aborted`, then `ReturnToLobby`.
- **Host disconnect:** NGO tears the session down; clients show "The dream collapsed" and return to the title.
- **Late join:** approved into the lobby with `RoundInProgress`; no `RoundStart` until the next round.
- **Lost events:** cannot happen on the reliable channel, but a client that detects a `seq` gap sends `ResumeRequest` anyway and rebuilds; belt and braces cost nothing.

## 11. Wire formats

- All payloads are `INetworkSerializable` structs with fixed layouts; no strings after `Hello` and `RoundStart`. Names are resolved from the `Players` list, cube types and skins from the shared registry order, which `contentHash` guarantees is identical.
- `Coord`: `int8 x, y, z`. `localPos`: `uint16 × 3` at 1/256 m inside the 8 m cube. `yaw`: `uint8` (1.4°). `pitch`: `int8` (±90° in 0.7° steps). Times inside a round: `uint32` milliseconds.
- Lattice event payloads are written by `Lucid.Core.EventLog.Write` and are byte-identical on the wire and in `.lucidlog`.
- Unreliable messages stay under 1 KB so they never fragment; reliable messages may exceed the MTU (NGO fragments them) but `ResumeSnapshot` is chunked at 16 KB regardless.

### Bandwidth budget (4 Sleepers, worst case)

| Flow | Rate | Bytes/s |
|---|---|---|
| Telemetry, each Sleeper → host | 10 Hz × 16 B | 160 |
| Markers, host → Nightmare | 10 Hz × ~60 B | 600 |
| DreamView, one dream → host → 4 subscribers | 20 Hz × 300 B × 5 | 30 000 |
| PossessInput, Nightmare → host → owner | 30 Hz × 8 B × 2 | 480 |
| Lattice events and replies | a few per second | < 500 |

Even a lobby where everyone spectates the same dream stays around 30 KB/s at the host, well inside what Steam's relay hands out for free.

## 12. Message table

| ID | Message | From → To | Delivery | Rate | Milestone |
|---|---|---|---|---|---|
| 001 | Hello (approval payload) | C → H | reliable | once | M0.8 |
| 002 | SessionState (replicated) | H → all | NetworkVariable / List | on change | M1.3 |
| 003 | RoleSelect | C → H | reliable | on click | M1.3 |
| 004 | ReadySet | C → H | reliable | on click | M1.3 |
| 005 | RoundInProgress | H → C | reliable | on late join | M1.3 |
| 101 | RoundStart | H → all | reliable | once per round | M0.8 |
| 102 | PhaseChanged | H → all | reliable | 2 per round | M0.8 |
| 103 | SleeperStatus | H → all | reliable | on change | M0.8 |
| 104 | RoundEnded | H → all | reliable | once | M0.9 |
| 105 | ReturnToLobby | H → all | reliable | once | M1.3 |
| 201 | LatticeEvent | H → all | reliable ordered | on event | M0.8 |
| 202 | HashReport | C → H | reliable | per event | M0.8 |
| 203 | DesyncNotice | H → all | reliable | rare | M0.8 |
| 204 | ResumeRequest | C → H | reliable | rare | M1.4 |
| 205 | ResumeSnapshot | H → C | reliable, chunked | rare | M1.4 |
| 301 | DreamReady | S → H | reliable | once | M0.8 |
| 302 | Telemetry | S → H | unreliable seq | 10 Hz | M0.8 |
| 303 | Explored | S → H | reliable | on event | M0.8 |
| 304 | TouchedExit | S → H | reliable | on event | M0.8 |
| 305 | Died | S → H | reliable | on event | M1.5 |
| 306 | ChicaneDelta | S → H | reliable | on change | M1.9 |
| 311 | WakeVerdict | H → S | reliable | on event | M0.8 |
| 312 | DeathVerdict | H → S | reliable | on event | M1.5 |
| 313 | TriggerFire | H → S | reliable | on request | M1.9 |
| 314 | EffectStart | H → S, subs, N | reliable | on request | M3 |
| 315 | ViewSubscribers | H → S | reliable | on change | M3 |
| 316 | PossessionBegin | H → S | reliable | on request | M3 |
| 317 | PossessionEnd | H → S | reliable | on event | M3 |
| 401 | PlaceRequest | N → H | reliable | on click | M0.8 |
| 402 | PlaceReply | H → N | reliable | per request | M0.8 |
| 403 | PowerRequest | N → H | reliable | on click | M1.9 |
| 404 | PowerReply | H → N | reliable | per request | M1.9 |
| 405 | PossessRequest | N → H | reliable | on click | M3 |
| 406 | PossessReply | H → N | reliable | per request | M3 |
| 407 | ReleaseRequest | N → H | reliable | on key | M3 |
| 408 | BudgetState | H → N | reliable | on change + 1 Hz | M0.8 |
| 409 | Markers | H → N, spectators | unreliable seq | 10 Hz | M1.4 |
| 410 | ChicaneSummary | H → N | reliable | on change | M1.9 |
| 501 | SubscribeDream | C → H | reliable | on tab switch | M3 |
| 502 | DreamView | S → H → subs | unreliable seq | 20 Hz | M3 |
| 503 | PossessInput | N → H → S | unreliable seq | 30 Hz | M3 |
| 504 | PossessedHit | N → H → S | reliable | on hit | M3 |
| 601 | ReplayRequest | C → H | reliable | on click | M1.12 |
| 602 | ReplayChunk | H → C | reliable, chunked | on request | M1.12 |

The ID is the `protocolVersion`-scoped identifier used in logs and in the `Lucid.Netcode.Messages` enum; adding a message appends to its hundred block, never renumbers.

## 13. Mapping to Netcode for GameObjects

- `Lucid.Netcode.SessionState` — a `NetworkBehaviour` on a host-owned object holding the four replicated fields of §3.
- `Lucid.Netcode.RoundSync` — RPCs for §4, §5, §6, §7, §9: `[Rpc(SendTo.Server)]` for client requests, `[Rpc(SendTo.ClientsAndHost)]` for broadcasts, `[Rpc(SendTo.SpecifiedInParams)]` for per-client replies and routed messages. Streams use `RpcDelivery.Unreliable` with a sequence number in the payload; a receiver ignores anything older than its last seen sequence.
- `Lucid.Netcode.DreamRelay` — §8: keeps the subscriber list per dream, forwards `DreamView` and `PossessInput`.
- `Lucid.Netcode.Transports` — the `--transport=utp|steam` switch (spec §14).
- Connection approval in `Lucid.Netcode.Approval`, reading the `Hello` payload from `NetworkManager.ConnectionApprovalCallback`.
- The host process owns exactly one `Lucid.Core.Round`; every RPC handler on the server side is a thin call into it. Clients own a `LatticeMirror` that applies `LatticeEvent`s through Core's replay path and reports hashes.

## 14. Failure handling

| Failure | Response |
|---|---|
| Client's `Derived.Hash` ≠ `postHash` | `DesyncNotice`, automatic `.lucidlog` save on the host, toast; the client requests a `ResumeSnapshot` and rebuilds |
| `PlaceRequest` for an unknown `typeIndex` | `PlaceReply UnknownType`; cannot happen with matching `contentHash` |
| `TouchedExit` on a door that just got a cube | `WakeVerdict false`; the client walks the Sleeper into the new cube (spec §14 race) |
| Telemetry stops for 2 s | The marker greys out; the leak rule keeps using the last cube |
| Telemetry stops for 30 s | Treated as a disconnect: grace, then consumed |
| `DreamView` stalls while possessed | The Nightmare's view freezes with a "signal lost" vignette; after 3 s the host ends the possession |
| Any reliable RPC throws on the host | Logged with the message ID and `seq`; the request is answered with an error rather than dropped, so the client never waits forever |

## 15. Tests

- **M0.8 smoke (Multiplayer Play Mode, host + one virtual client):** `RoundStart` builds identical lattices; a placement round-trips with `PlaceReply` and `LatticeEvent`; the client's `HashReport` matches; explore-then-place and place-then-explore resolve by host order; `TouchedExit` wakes; an intentionally corrupted client hash produces a `DesyncNotice`.
- **M1 session:** approval rejects a wrong `contentHash` with the version string; `RoleSelect`/`ReadySet` drive start conditions; a disconnected Sleeper resumes within grace from a `ResumeSnapshot` and is consumed after it; a late joiner receives `RoundInProgress` and no events.
- **M3 streams:** `DreamView` publishes only with subscribers; a second subscriber receives the same stream; `PossessInput` at 30 Hz moves the body in the owner's dream and the pose returns within two frames on localhost; `PossessedHit` damages the Sleeper; owner disconnect ends the possession.
- **Bandwidth assertion:** a scripted 5-minute round with four Sleepers and one spectator stays under 40 KB/s at the host in the test harness.
