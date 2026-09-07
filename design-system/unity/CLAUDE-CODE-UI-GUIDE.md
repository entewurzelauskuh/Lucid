# Lucid UI — implementation guide for Claude Code

You are implementing Lucid's interface in Unity 6 with UI Toolkit. This file is
the design authority for **how it looks and how it reads**. `docs/UI.md` remains
the authority for **what is on each screen and what every string says**; where
this file and `docs/UI.md` disagree, `docs/UI.md` wins and this file is the bug.

Read in this order: `docs/UI.md` §1 and §14 → this file → `lucid-tokens.uss` →
the screen you are building.

---

## 0. The rules that do not bend

1. **Never invent a player-facing string.** Every one comes from
   `Runtime/LucidStrings.cs`, which is `docs/UI.md` §14 transcribed. No literals
   in screen code. If you need a string that is not there, stop and ask for a
   glossary entry — do not improvise one.
2. **Never let hue carry a rule.** Door state, ready state, affordability and
   health all change *shape, fill and light* as well as colour. Test every state
   in greyscale before you call it done.
3. **A blocked action states its blocker.** A rejected placement shows its
   reason; a Start button that cannot start puts the reason in its own label.
   Greying out alone is a bug.
4. **Nothing a rule depends on animates.** See §6. If a player must wait for a
   number to settle before trusting it, the animation cost them the round.
5. **The UI never gets the dream filter.** Screen Space Overlay, rendered after
   post-processing. World markers are projected into that same overlay from
   `WorldToScreenPoint`, never drawn as world-space geometry.
6. **All styling in USS, no inline C# styles.** A contributor must be able to
   restyle the whole game by editing two stylesheets. If you find yourself
   setting `element.style.backgroundColor`, add a class instead.
7. **No VALUE below 22 px on Results or the scoreboard.** Those screens are read
   over a Discord screen share at 720p, which is 0.667× — 22 px authored becomes
   14.7 px on your friend's monitor. A *value* is anything a viewer reads off the
   screen: a name, a Sleeper number, a score, a delta, a time, a depth. Static
   column labels and section captions ("THE NIGHTMARE", "fastest wake") may sit at
   `--fs-micro`, because they name a thing rather than report one. Sleeper numbers
   are values — they are what makes the four colours safe for a colour-blind
   viewer, so they may never be the smallest text on the screen.

---

## 1. Files and where they go

```
Assets/_Lucid/Runtime/UI/          (inside Lucid.Runtime; see below)
  Styles/
    lucid-tokens.uss          every token; import first
    lucid-components.uss      component classes; depends on tokens
  Screens/
    Title.uxml  Lobby.uxml  PlayerRow.uxml  RoundStart.uxml
    SleeperHud.uxml  Spectator.uxml  NightmareView.uxml
    Results.uxml  Pause.uxml  Options.uxml
  Runtime/
    LucidRing.cs              every ring and arc
    LucidConnectorNet.cs      the six-face cube net, generated
    LucidStrings.cs           the copy glossary
    <Screen>Controller.cs     one per screen, written by you
  Fonts/
    CormorantGaramond-Light.ttf  CormorantGaramond-SemiBold.ttf
    Inter-Regular.ttf  Inter-Medium.ttf  Inter-SemiBold.ttf
    *.asset                   the FontAsset per weight
  Icons/
    lucid-icons.png           sprite atlas, or the individual SVGs
```

Everything sits under `Runtime/` because every script in this project lives in
an assembly definition: a folder outside them lands in `Assembly-CSharp`, which
`Lucid.Runtime` cannot reference, so a screen controller in the scene flow could
never see `LucidStrings`. The C# namespace is `Lucid.Runtime.UI` and the UXML
declares `xmlns:lucid="Lucid.Runtime.UI"` (`docs/DECISIONS.md`, 2026-09-07).

Attach both stylesheets once on the root `PanelSettings` theme rather than per
UXML, so a restyle is one file. Every UXML in `Screens/` still carries its
`<Style src=…>` lines so it previews correctly in the UI Builder.

---

## 2. Tokens

`lucid-tokens.uss` is the whole palette, type scale, spacing ladder, radii and
durations. Read values from it; never hard-code a hex, a px size or a duration
in USS or C#.

Three things about it that will bite you if you skip them:

- **Alphas are literals.** USS has no `color-mix()` and no `rgba(var(--x), .5)`.
  Every translucent value is a written-out `rgba()` — `--panel-fill` is
  `rgba(34, 49, 74, 0.72)`. If you need a new alpha of an existing colour, add a
  named token; do not compute it.
- **Durations are seconds.** `--dur-base` is `0.22s`, not `220ms`.
- **Letter-spacing is px.** USS has no `em` for it. `--ls-caps` is `2px`, which is
  0.16em at the 13px micro size. If you use the caps style at another size, you
  need a different token.

### The colour rules that matter

| Token | Contrast on mist | Use |
|---|---|---|
| `--fg-1` | 13.4:1 | primary text, numerals, titles |
| `--fg-2` | 9.0:1 | secondary text, labels |
| `--fg-3` | 5.3:1 | tertiary — the smallest legal size for body text |
| `--fg-4` | 3.1:1 | **decorative or ≥ 24 px only.** Never body text |

`--sleeper-1` … `--sleeper-4` are the four colour-blind-safe values and **none of
them is legible as text on mist**. A Sleeper's colour is only ever a chip, a
marker or a stroke; their name next to it is always `--fg-1`. Colour, number and
name travel together, every time (`docs/UI.md` §1.5).

---

## 3. Type

Two families, five sizes, and no exceptions.

- **`--font-display`** (Cormorant Garamond) — screen titles, the Results outcome
  line, the reveal card, the wordmark. Light 300 and SemiBold 600 only.
- **`--font-body`** (Inter) — every control, every string, every numeral.

```
--fs-display 64px   Results outcome, LUCID, the reveal
--fs-title   34px   screen titles, the dawn timer, the budget number
--fs-heading 22px   panel headings, player names, scoreboard rows  ← the 720p floor
--fs-body    16px   everything ordinary
--fs-micro   13px   hotkeys, costs, build string. Never carries a rule.
```

### Numerals — four font files, not two

Unity exposes **no OpenType feature switches**: you cannot ask TextCore for
tabular (`tnum`) or lining (`lnum`) figures at runtime. Both problems are real:

- **Proportional digits jitter.** `1` is narrower than `0`, so `3:12 → 3:11`
  shifts the dawn timer sideways every second — on the centred, most-looked-at
  element in the game. Scoreboard columns also stop aligning.
- **Cormorant defaults to oldstyle figures** — an O-shaped zero, a short 1, a
  descending 2 — so `0:12` reads as `O:I2`.

**The decision: freeze the features into the source files offline.** Because the
constraint is at file level, so is the fix. Two extra files, built once:

```bash
pip install opentype-feature-freezer
pyftfeatfreeze -f tnum      Inter-Regular.ttf            LucidInter-Tabular.ttf
pyftfeatfreeze -f lnum,tnum CormorantGaramond-Light.ttf  LucidCormorant-Lining.ttf
```

The frozen files have those figures as their **default** glyphs, so the generated
FontAsset needs no runtime switch. Both families are OFL; the OFL permits
modification, so check each for a Reserved Font Name and rename if present —
the `Lucid` prefix above does that unconditionally, which is the safe default.
Add a ledger line per `CLAUDE.md` rule 5 recording the source file, the licence
and the fact that features were frozen.

**Then use them by class, not by default.** Prose keeps proportional figures,
because that is typographically correct for running text:

```xml
<ui:Label class="tabular" text="3:12" />        <!-- ticks or aligns in a column -->
<ui:Label class="lining"  text="0:12" />        <!-- display font, has digits -->
```

`.tabular` on: the dawn timer, the budget number, cost badges, depth readouts,
every scoreboard cell, the trickle rate, cooldown labels.
`.lining` on: the bedroom countdown, the 3-2-1 count, any display-font title
containing a digit. Build one and look at it before you generate the rest.

---

## 4. Components

Each of these has a class block in `lucid-components.uss` and a matching React
reference implementation in this design system under `components/`. When a value
is ambiguous, the reference implementation is the tie-breaker.

| Component | USS class | Notes |
|---|---|---|
| Mist panel | `.mist-panel` | `--sunken` opaque for full screens, `--docked` drops the radius |
| Button | `.btn` + `--primary` / `--secondary` / `--lg` | one primary per screen |
| Toggle | `.toggle` + `--on` | knob glows white-gold on |
| Slider | `.slider` | value always shown as a number |
| Player row | `.player-row`, `.role-card`, `.ready-chip` | ready is green **and** a filled check ring |
| Toast | `.toast` + tone | three at a time, 4 s each, left-edge accent |
| Cost badge | `.cost-badge` + `--over` | over budget = edge, fill and text together |
| Palette tile | `.palette-tile` | category icon, name, cost, hotkey, connector net |
| Sleeper marker | `.sleeper-chip--1..4` | shape variants under `.shapes-on` |
| Rejection label | `.rejection` | rides the red ghost, appears in the same frame |
| Scoreboard row | `.score-row` | everything ≥ 22 px, tabular |
| Rings and arcs | `.ring`, `.timer-arc`, `.health-ring`, `.crosshair` | `LucidRing`, see below |
| Connector net | `.connector-net` | `LucidConnectorNet`, generated from the mask |

### Rings

`LucidRing` is the only custom-painted element. It reads `--ring-track`,
`--ring-fill` and `--ring-width` from USS, so the classes above configure it:

```xml
<lucid:LucidRing class="timer-arc" sweep-angle="180" start-angle="180" progress="0.6" />
<lucid:LucidRing class="health-ring" progress="0.82" />
<lucid:LucidRing class="ring ring--cooling" progress="0.4" reverse="true" round-caps="false" />
```

Set `round-caps="false"` on anything a player reads a value off — a rounded cap
overstates the fill by half a stroke width, which matters on the weak-point gauge.

### Dashes

Painter2D has no dash array and USS supports **no `border-style`** — there are no
dashed or dotted borders in UI Toolkit at all. Two consequences:

- **Dashed rectangles** (the empty lobby seat, god-view fog-door markers if
  drawn rather than sprited) use `.dashed-box`: one 9-sliced tiling
  `dash-border.png`, tinted per use. One asset covers every dashed box.
- **The connector net keeps its own fallback** and does not use dashes at all: a
  wall is a fainter stroke with no dot, a doorway is a full stroke with a filled
  dot. That is two channels, so it satisfies §1.5, and it is *clearer* than
  dashes at the 12 px the net is usually drawn at.

Every dashed *icon* — the fog door's drifting dashes, the unready ring — is baked
into the sprite atlas and unaffected.

### The connector net

Never author a per-cube glyph. `LucidConnectorNet` takes the six-bit mask the
`CubeDefinition` already has:

```csharp
net.SetMask(cubeDefinition.Connectors);      // Lucid.Core.FaceMask
// or net.mask = "010100";
```

The net's own order is `[top, west, north, east, south, bottom]`; `SetMask`
takes Core's `FaceMask` (North, East, South, West, Up, Down) and reorders it, so
no caller does. A new cube type gets its glyph for free, forever. This is why
there is no cube icon in `Icons/`.

---

## 5. Layout

**The Sleeper HUD has exactly six positions.** Six, not seven. Adding one costs
the player a place to look. Depth is not one of them: `depth 7 · exit 11` is the
Nightmare's Sleeper row (`docs/UI.md` §8), and §1.2 gives the Sleeper the timer,
health, lives and the door language — the way out is read off the doors.

| Position | Contents |
|---|---|
| Top centre | dawn timer + phase banner |
| Top left | active effect chips (Molasses' 70 %, Dark, Fog) |
| Top right | toast stack |
| Centre | crosshair |
| Bottom left | health ring, crescent moons |
| Bottom right | key reminders |

Every cluster sits `--hud-margin` (48 px) from its screen edge. There is **no
panel behind a HUD cluster** — each gets a faint radial scrim
(`.cluster-scrim`, one tinted `radial-soft.png`) so text stays legible without
covering the maze. This was a deliberate call over framing them in mist panels;
it is recorded in `docs/DECISIONS.md`.

**The Nightmare view docks both side panels full height.** Palette left at
340 px, Sleeper panel right at 320 px, powers bar centred on the bottom edge
between them, god view the letterbox that remains. Budget is **top left**, per
`docs/UI.md` §8's region table. Layer cut-away buttons run up the left edge of
the god view, labelled PgUp / PgDn.

**Full screens** (Lobby, Results, Options) use a 48 px page margin and a
two-column split. Results is the one screen with a hard constraint: one screen,
no scrolling, nothing under 22 px.

---

## 6. Motion

Five durations, two easings — USS supports only its predefined easings, so there
are no custom curves anywhere.

| Token | Value | Used by |
|---|---|---|
| `--dur-instant` | 0.09s | press, chip select, the rejection label |
| `--dur-fast` | 0.14s | hover, focus, toggle knob |
| `--dur-base` | 0.22s | panel and toast in/out, ghost colour, health redraw |
| `--dur-slow` | 0.4s | screen fade, layer cut-away, Tab overlay |
| `--dur-veil` | 0.9s | the white-out on waking, the sink to black |

**Four things pulse, and nothing else.** USS has no `@keyframes`, so each is a
small C# tween on `style.opacity` — never a shader, never a material animation.

1. The dawn arc in the last 30 s — 1.2 s, opacity 1 → 0.45. **The digits do not.**
2. The last crescent moon — 2.6 s, opacity 1 → 0.62.
3. The exit, in the world and on the god view — 2.4 s radiance.
4. The Title screen's fog door — decorative, gone once the Lobby opens.

Health regen adds a conditional fifth for the 4 s it is refilling.

### Never animate

Each of these carries a rule:

- the dawn numerals (the arc pulses; `3:12` is always exactly `3:12`)
- the budget number — it steps 11 → 12 in one frame; the *trickle ring* animates
- any cost badge — never counts up or down
- the placement rejection label — same frame as the red ghost, no fade
- door state — hardening is a cut, not a dissolve
- Results and scoreboard figures — no count-up, ever
- the weak-point ring — a linear 1:1 gauge, six shots
- a Sleeper marker's colour, number or name — position interpolates, identity does not

The **Reduce motion** option stops the four pulses and the regen shimmer, and
nothing else. Toasts keep their fade (instant text is harder to read) and the
exit keeps its radiance (it is a game rule).

---

## 7. Accessibility

- **High-contrast doors** (on by default in spirit, a toggle in Options): a faint
  hatch on fog, rays on exits. Use `hatch-45.png` for the hatch.
- **Colour-blind marker shapes** — an option, **off by default**, because the four
  Sleeper colours are already colour-blind-safe and always carry a number and a
  name. When on, add `.shapes-on` to the panel root; the USS varies each index's
  corner radius.
- **Larger HUD text** moves everything one step up the scale.
- **Reduce motion** as above. **Screen shake** is a 0–100 % slider.
- Minimum interactive height is `--hit-min` (44 px). No exceptions in menus.

---

## 8. Per-screen notes

**Title.** Wordmark in `--font-display` Light at 0.22em tracking — *there is no
logo asset and you must not create one*. Menu is Host a dream / Join / Sandbox /
Options / Quit; in M0 only Sandbox and Quit exist (`docs/UI.md` §16) and the
other three are absent rather than greyed, since rule 0.3 would demand a blocker
string §14 does not have. Build for five so nothing moves when they arrive. The
bedroom's fog door breathes behind the wordmark — the real start cube, through
`DreamInstance`, not an image (`docs/UI.md` §3).

**Lobby.** Three columns and a bottom bar. Only the local player's row is
interactive. Start's label is its blocker, from `LucidStrings`. Roles and ready
clear when the lobby reopens. If more than one player picked Nightmare, show
`WantToBeNightmare(n)` under the list.

**Round start.** 3-2-1, then the reveal card. Hint cards appear along the bottom
for the first three rounds only, teaching the three door states in the player's
own words: *grey mist — closed for now*, *white light — the way out*,
*hardened wall — you've been here*.

**Sleeper HUD.** Lives default to **1**, so most rounds show a single moon; do not
build the layout assuming three. Dark dims the HUD at **two levels, not one**:
non-rule chrome (key hints, toasts, cluster scrims) to 32 %, the rule-carrying
health ring and crescent lives to **60 %**, and the dawn timer not at all. A flat
32 % put the health ring at ~1.7:1, which is §1.3 hiding a rule rather than
dimming it. §6 says only "dims" — the two levels are this system's proposal, and
item 4 of `DOCS-CHANGE-REQUEST.md` asks for them to be written down. Fog doors
still glow. Molasses shows a 70 % chip top left.

**Spectator.** `YoureAwake` at the top, tabs for each Sleeper *and* the
Nightmare's god view, default to the dream you left. The watched Sleeper's vitals
are quoted at reduced opacity so nobody mistakes them for their own.

**Nightmare view.** See §5. The ghost cube is green when legal and red with the
verbatim reason when not. A trap under the cursor shows its trigger, its weak
point and its 6 s cooldown ring. Hovering a Sleeper row expands it to that
dream's live mobs and jammed traps.

**Possession.** `Possessing(mob, owner)` across the top, `BuildingPaused` under
the budget, thin red vignette, P to let go. On the body's death: two seconds of
black, `YourBodyDied`, back to the god view.

**Results.** The title is exactly one of `ResultEveryoneWoke`, `ResultDawn`,
`ResultConsumed`. Sleeper cards read `WokeAt` / `ConsumedAt` / `ConsumedByDawn`.
The Nightmare card carries Sleepers consumed, points, cubes placed and deepest
cube reached. Three badges, one line each. Back to lobby is automatic after 10 s.

**Pause.** There is no pausing a multiplayer round — say so. Options, Leave, Quit
to desktop. Leaving as the Nightmare confirms with `DreamWillCollapse`.

**Options.** Video / Audio / Controls / Accessibility / Gameplay, in that order,
built from exactly two controls: `.toggle` and `.slider`.

---

## 9. Before you open a pull request

- [ ] No player-facing string literal outside `LucidStrings.cs`
- [ ] No hex, px size or duration literal outside `lucid-tokens.uss`
- [ ] No `element.style.*` assignment for anything a class could do
- [ ] Every state distinguishable with the screen in greyscale
- [ ] Every blocked control shows its reason
- [ ] Nothing in §6's never-animate list moves
- [ ] Results legible at 1280×720 — check it, do not assume it
- [ ] All interactive elements ≥ 44 px tall
- [ ] Numerals use `.tabular` / `.lining` wherever a value ticks, aligns, or is set in the display face
- [ ] No large permanent panel is translucent; no `.scrim--frozen` over live play
- [ ] Every new asset has a ledger line per `CLAUDE.md` rule 5 (OFL / CC0 / CC-BY only)

## 10. Reference

**Before you touch `docs/`:** `DOCS-CHANGE-REQUEST.md` in this folder lists every
edit the repo's own documentation needed for the implementation to be consistent.
It has been applied (#85); its resolution table says what became of each item.
The one rule it called `[S]` and unhonourable was `[D]` all along.

The visual source of truth is the design system this file ships in:

- `ui_kits/lucid-game/index.html` — all ten screens, click-through
- `ui_kits/lucid-game/mockups.html` — six annotated in-game situations, each
  captioned with the rules in play. Start here to understand *why* a screen
  looks the way it does in a given moment.
- `ui_kits/lucid-game/results-720.html` — the screen-share legibility check
- `guidelines/motion.md` — the full motion contract
- `guidelines/asset-checklist.md` — every asset, who makes it, how long
- `components/` — a React reference implementation of every component
