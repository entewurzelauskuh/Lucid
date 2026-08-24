# LUCID — UI Screens and Flows

**Version 1.0 — 2026-08-24 (consolidated).** Companion to `docs/SPEC.md`. Same legend: **[S]** settled · **[D]** default, overrule freely · **[O]** open.

Settled alongside this document: audio is SFX first, music much later **[S]**; no in-game voice or text chat, friends use Discord **[S]**.

---

## 1. Principles

1. **The world is painted, the UI is crisp.** The dream filter (spec §15) applies to the 3D view only; UI renders after post-processing. Panels are translucent mist, text is sharp.
2. **One glance.** A Sleeper needs the timer, health, lives and the door language. The Nightmare needs budget, timer, where the Sleepers are, the palette and cooldowns. Everything else is on hover or behind Tab.
3. **Never hide a rule.** Every rejected placement shows its reason. Every state change that matters gets a toast. If a player asks "why?", the answer was on screen.
4. **Party first.** Results are readable across a Discord call: big names, big outcomes, one screen.
5. **Door states never depend on hue alone.** Fog is dark and matte, Exit is bright and radiant, Solid is a wall, Attached is an opening. Sleeper colours are always paired with a number and a name.

## 2. Screen map

```
Boot ─► Title ─┬─ Host a dream ─────────► Lobby ─► (Start) ─► Round ─► Results ─► Lobby …
               ├─ Join (Steam friends) ─► Lobby                │
               ├─ Sandbox ──────────────► Sandbox              ├─ Nightmare view
               ├─ Options                                      ├─ Sleeper view ─► Spectator view
               └─ Quit                                         └─ Pause (Esc) ─► Options / Leave

Steam "Join game" / overlay invite ─► Lobby directly
```

No mid-round joining: latecomers wait in the lobby with "Round in progress, 2:13 left". **[D]**

## 3. Title

- Background: the start cube, a bedroom at night, its door a grey fog door that slowly pulses white. The door language is taught before the first round is played.
- Menu: **Host a dream**, **Join** (opens the Steam friends overlay; also "you can accept invites from the overlay at any time"), **Sandbox**, **Options**, **Quit**.
- Corner: Steam avatar and name, build version and branch.
- Launched through a Steam invite → straight to the lobby.

## 4. Lobby **[S]** rules, **[D]** layout

Three columns and a bottom bar.

**Left: players.** One row per player: Steam avatar, name, host crown, role choice as two large cards (**Nightmare**: an eye in mist · **Sleeper**: a crescent moon), a ready check. If more than one player picked Nightmare, a line under the list: "3 players want to be the Nightmare — one will be chosen at random."

**Centre: session leaderboard.** Name, score, rounds as Nightmare, woke / consumed counts. Empty state on the first round: "The first dream begins."

**Right: settings.** Host edits, others see a read-only summary. Groups: *Round* (head start, dawn length, lives), *Nightmare* (starting budget, trickle, powers toggles, mob density), *Dream* (enabled packs, theme set, layers, footprint), *Sleepers* (exit sense, gamepad aim assist). Presets at the top **[D]**: **Default**, **Gentle** (2 lives, 8 min, low mobs, slower trickle), **Cruel** (1 life, 4 min, high mobs, fast trickle).

**Bottom bar.** *Invite friends* (Steam overlay), *Ready* toggle, *Start* (host only). Start is disabled with the reason shown on the button: "Nobody picked Nightmare", "Need at least one Sleeper", "Waiting for Ben to ready up".

**Start sequence.** 3-2-1 countdown, then the reveal card: "Tonight's Nightmare is… **Anna**". Roles and ready state are cleared when the lobby reopens after a round. **[S]**

## 5. Round start

- **Nightmare:** builds immediately. Top banner: "The Sleepers stir in 0:30", counting down, then "The Sleepers are running".
- **Sleeper:** stands in the bedroom; the door is mist with the countdown projected onto it. First three rounds only (option to disable), three small cards fade in along the bottom: *grey mist — closed for now* · *white light — the way out* · *hardened wall — you've been here*.

## 6. Sleeper HUD

| Position | Element | Notes |
|---|---|---|
| Top centre | Dawn timer | `mm:ss` with a thin arc that fills toward dawn; pulses in the last 30 s. Head-start countdown uses the same element |
| Bottom left | Health | A soft glow ring that dims as health drops; regen shimmers. Number on hover-hold only |
| Bottom left | Lives | Crescent moons, one per life; the last moon glows |
| Centre | Crosshair | A small dot. Over a weak point it grows a ring that drains as the part takes damage; over a mob it brightens |
| Centre edges | Damage direction | A short vignette arc on the side the hit came from |
| Top right | Toasts | Stack of three, four seconds each: "The exit moved", "A door hardened", "Anna woke up", "Ben was consumed", "Dark", "Fog", "Molasses — don't jump" |
| Whole screen | Effects | Dark: HUD dims except the timer. Fog: none. Molasses: a viscous vignette and a small "70 %" icon while it lasts |

**Death.** Screen drops and blurs. "You lost a life — 2 moons left", respawn countdown 3-2-1 in the bedroom. Last life: "Consumed" and the screen sinks to black, then the spectator view.

**Waking.** White-out, "You woke up — 3:12", then the spectator view.

**Tab:** scoreboard overlay — every player, status (in the dream / awake / consumed), the timer, the Nightmare's name.

**Esc:** pause menu (§10).

## 7. Spectator view

Free camera in any dream. Tabs for each Sleeper (1–4) and the Nightmare's god view; the default is the dream you left. A line at the top: "You're awake. Watch the others." No powers, no chat; voice is Discord. Toasts continue so the audience follows the story.

## 8. Nightmare view **[S]** contents, **[D]** layout and keys

### Layout

| Region | Element |
|---|---|
| Centre | God view: orbit / top-down camera over the lattice with a layer cut-away |
| Top centre | Dawn timer and phase banner |
| Top left | Budget: the number, a ring showing the next trickle point, the trickle rate |
| Left | Palette: one tab per enabled pack, category chips, cube cards |
| Right | Sleeper panel: one row per Sleeper |
| Bottom centre | Powers bar: target selector, effect buttons with cost and cooldown rings, possession hint |
| Top right | Toasts |

**Cube card.** Icon, name, cost, hotkey number, a mini cube net showing which faces are connectors. Hover: the chicane in one line, weak point, trigger, and the preview render from the pipeline (spec §17), reused here.

**Sleeper row.** Colour chip with number, name, status, health bar, moons, "depth 7 · exit 11", last event. Click selects the row as the power target and focuses the camera. Expanding a row shows that dream's live mobs and jammed traps (the hover-peek).

**In-world overlays.** Fog doors highlighted as buildable; exits pulsing white; Sleeper markers with number, colour and facing; mob markers small and coloured by dream; trap cubes show a trigger icon on hover with its cooldown ring; the ghost cube green when valid, red with a label when not: "Door is solid", "Doesn't fit here", "Would trap Ben", "Not enough budget (3 / 4)", "Not a door".

### Keys

| Action | Key |
|---|---|
| Select cube type | click a card, or **1–9** for the visible cards; **Ctrl+Tab** cycles the category chips |
| Place / rotate ghost / cancel | left click / **R** / **Esc** or right click |
| Camera pan / orbit / zoom | edge scroll or arrows, middle-drag / right-drag / wheel |
| Layer up / down | **PgUp / PgDn** (also **[ ]**) |
| Focus start cube / focus target | **Home** / **F** |
| Toggle top-down and orbit | **V** |
| Trigger the trap under the cursor | **T** |
| Possess the mob under the cursor | **P**; **P** again to let go |
| Global effects | **Q** Dark · **W** Fog · **E** Molasses |
| Power target | **Tab** cycles All → Sleeper 1 → … → All; clicking a marker or row selects; **0** returns to All |
| Pause menu | **Esc** with no ghost active |

This revises spec §10: the target selector moves from 1–4 to Tab and click, so the number row can serve the palette, which is used far more often. **[D]**

### Possession overlay

First person through the mob, thin red vignette, top line "Possessing a Shade in Anna's dream — P to let go", timer and budget still visible, "Building paused" under the budget. On the body's death: two seconds of black, "Your body died", back to the god view.

## 9. Results **[D]**

One screen. Title by outcome: "Dawn." if anyone was consumed by the timer, "Everyone woke up" if all escaped, "Consumed" if nobody did.

- Sleeper cards in outcome order: avatar, name, "Woke at 3:12" / "Consumed at 2:40" / "Consumed by dawn", points.
- Nightmare card: name, Sleepers consumed, points, cubes placed, deepest cube reached.
- Badges, one line each: fastest wake, longest survived, most cubes built.
- Leaderboard deltas.
- Buttons: *Back to lobby* (host; auto after 10 s), *Save replay* (writes the `.lucidlog`).

## 10. Pause menu

There is no pausing a multiplayer round. The menu offers *Options*, *Leave*, *Quit to desktop*. Leaving as the Nightmare ends the round for everyone, with a confirm: "The dream will collapse for all Sleepers." Leaving as a Sleeper counts as consumed, with a confirm.

## 11. Options **[D]**

- **Video:** resolution, fullscreen, vsync, quality preset, painterly pass (off / low / high), depth of field strength, film grain, motion blur (off by default), field of view for Sleepers (70–110), head bob.
- **Audio:** master, SFX, UI. A music slider appears when music exists.
- **Controls:** rebinding for the Sleeper map (keyboard and gamepad) and the Nightmare map; invert Y; sensitivity; crouch hold / toggle; gamepad aim-assist strength.
- **Accessibility:** text size; high-contrast doors (a faint hatch on fog, rays on exits); reduced flashes (softer Dark and Fog transitions); screen shake off; colour-blind marker shapes.
- **Gameplay:** hint cards on / off; toast verbosity.

## 12. Sandbox **[D]**

Single player, local, no network. Build as the Nightmare with unlimited budget and no timer, press **F5** to drop into the current lattice as a Sleeper, **F5** again to return to the god view. For trying cubes, tuning chicanes and for contributors who have no second player. Costs almost nothing after M0: the local dream and the god view already exist; the sandbox only switches which one drives the camera and input.

## 13. Edge flows **[D]**

| Situation | Behaviour |
|---|---|
| Host leaves | "The dream collapsed" for everyone → title |
| Nightmare (not host) disconnects mid-round | Round ends without scores, "The Nightmare fled" → lobby |
| Sleeper disconnects | 30 s grace; on return with the same Steam ID they respawn in the bedroom with the current lattice; otherwise consumed when the grace ends |
| Player joins during a round | Waits in the lobby with the remaining time shown |
| Steam offline | Title shows "Steam is offline"; Sandbox still works; Host and Join disabled |

## 14. Copy glossary

Strings are the rules made visible; keep them exact so every screen says the same thing.

- Phase: "The Sleepers stir in 0:30" · "The Sleepers are running" · "Dawn in 0:30" · "Dawn."
- Sleeper events: "{name} woke up" · "{name} was consumed" · "You lost a life — {n} moons left" · "You woke up — {time}" · "Consumed"
- Doors, Sleeper side: "The exit moved" · "A door hardened"
- Doors, Nightmare side: "{name} hardened {n} doors in the {cube}" · "{name} reached the exit"
- Placement: "Door is solid" · "Doesn't fit here" · "Would trap {name}" · "Not enough budget ({have} / {cost})" · "Not a door"
- Effects: "Dark" · "Fog" · "Molasses — don't jump"
- Possession: "Possessing a {mob} in {name}'s dream — P to let go" · "Your body died"
- Lobby: "Nobody picked Nightmare" · "Need at least one Sleeper" · "Waiting for {name} to ready up" · "Tonight's Nightmare is… {name}"

## 15. Visual style of the UI **[D]**

- Panels: translucent blue-black with soft, misty edges and thin light borders. Titles in a soft serif, body in a humanist sans; both under open licenses (for example Cormorant Garamond and Inter, both OFL).
- Accents: exit white-gold, fog grey-blue, danger red.
- Sleeper colours from a colour-blind-safe set, always with a number: orange `#E69F00`, sky blue `#56B4E9`, green `#009E73`, purple-pink `#CC79A7`.
- Icons: thin line icons; connectors drawn as a mini cube net.
- Implementation: UI Toolkit (UXML / USS) for every screen and HUD, so contributors can restyle without touching code; a Screen Space Overlay so the post filter never touches the UI; world markers projected from world positions into the same overlay.

## 16. Milestone mapping

- **M0:** Sleeper HUD (timer, lives placeholder), Nightmare HUD (budget, timer, palette v0, ghost with reasons), head-start banner, results overlay v0, sandbox switch (dev tool).
- **M1:** Title, lobby with Steam avatars and settings presets, Sleeper panel, target selector, toasts, spectator view, results v1, pause menu, options v0, edge flows.
- **M2:** health, lives and damage feedback, weak-point ring, gamepad UI navigation, rebinding, aim-assist option.
- **M3:** possession overlay, effects bar, accessibility set, results badges, options v1 (painterly pass and friends).
