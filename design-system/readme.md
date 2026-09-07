# Lucid — UI design system

Lucid is an open-source (MIT) asymmetric online party game built in Unity 6.
One **Nightmare** builds a maze of 8 m cubes in real time from a god view; one to
four **Sleepers** each run that same maze alone in first person, trying to reach
the deepest fog door before dawn. It is a shared dream, so any theme may appear
and dream logic excuses the mix — it is not necessarily scary.

This project is the UI design system for that game: tokens, components, an icon
language, and every screen at 1920×1080.

## Sources

Built entirely from the design documentation in the game repository. No UI code
existed at the time of writing (`Lucid.Netcode` is a stub and the UI milestone
had not started), so nothing here is a recreation — it is the first pass.

- **Repository:** https://github.com/entewurzelauskuh/Lucid — explore it directly for anything below that has gone stale
- `docs/UI.md` — every screen, key and string; §1 principles, §6 Sleeper HUD, §8 Nightmare view, §9 Results, §14 copy glossary, §15 visual style
- `docs/SPEC.md` — §7 door states, §9 combat and health, §10 powers and the target selector, §12 scoring, §15 art direction
- `CLAUDE.md` — how the project works, and rule 5 on asset licensing
- See `github.md` for the sync record.

Markers in those docs matter: **[S]** is settled and was not reinterpreted here;
**[D]** is open, and every deviation was recorded in `guidelines/decisions-draft.md`;
they are now in `docs/DECISIONS.md` (2026-09-07) and the draft is the record of
what was proposed.

## What is settled

The world is painted and soft-focus; the UI over it is crisp and renders after
post-processing on a Screen Space Overlay. **Door states never rely on hue alone**
— Fog is dark and matte, Exit is bright and radiant, Solid is a wall, Attached is
an opening (`docs/SPEC.md` §7, marked `[S]`). The four Sleeper colours are
`#E69F00` `#56B4E9` `#009E73` `#CC79A7`, always paired with a number *and* a name.
Every string comes from the §14 glossary verbatim. Titles are a soft serif, body a
humanist sans, both open-licence. Implementation is UI Toolkit (UXML/USS).

A correction to the first draft of this section: `docs/UI.md` §15 — panels,
accents, type — is marked `[D]`, and §1, §6 and §14 carry no marker. Only the
door-state rule above is `[S]`. Everything else here is a default this system
chose and the owner then wrote into `docs/UI.md` (`docs/DECISIONS.md`,
2026-09-07), not a rule it inherited.

---

# Content fundamentals

**Voice.** Lucid narrates. Copy tells you what happened in the dream, in plain
words, in the past tense, as a friend describing it afterwards: *"The exit moved."*
*"A door hardened."* *"Ben was consumed."* *"Everyone woke up."* Nothing is a
system message. There is no "Error", no "Invalid", no "Success".

**Person.** Second person for what happened to you — *"You were consumed"*, *"You
lost a life — 1 moon left"*, *"You are the mob"*. Third person and the player's
own name for everyone else — *"Cara was consumed"*, never *"Player 3 eliminated"*.
Never first person; the game does not have a personality that speaks.

**Casing.** Sentence case everywhere. Titles too: *"Who is dreaming"*, *"The
dream"*, *"Who is left"*, *"Who feels it"*. Micro labels are the single exception
— uppercase at 13 px with 0.16em tracking, for section markers only (`PALETTE`,
`SLEEPERS`, `SESSION LEADERBOARD`), never for anything a player must read quickly.

**Punctuation.** No exclamation marks. No ellipsis except in the reveal card's
*"Tonight's Nightmare is…"*, where the pause is the point. Em dashes for a beat:
*"Molasses — don't jump"*. Numbers in parentheses when a rule is being shown:
*"Not enough budget (3 / 4)"*.

**Blocked things say why.** This is §1.3 as a writing rule, not just a UI one. A
rejected placement is never a red outline — it is *"Would trap Ben"*. A Start
button that cannot start is not greyed out — its label becomes *"Waiting for Dev
to ready up"*. If a player cannot do something, the interface has already told
them the reason in the place they were looking.

**Length.** A toast is one line and fits on one line at 1280 px. If it needs two,
it is two events.

**No emoji.** None, anywhere. The icon language covers everything emoji would.

**Verbs to keep.** wake, sleep, dream, run, build, place, harden, consume, jam,
possess. **Verbs to avoid.** spawn, deploy, eliminate, respawn, activate, execute.
The glossary in UI.md §14 is the authority — when it names a thing, that is the
name, in every screen, forever.

---

# Visual foundations

**Colour.** One family of blue-blacks (`--ink-900` through `--mist-400`) and three
accents. Exit white-gold is the only warm colour in the system and it means one
thing: the way out. Fog grey-blue is neutral state. Danger red appears only where
something is refused or lost. Text runs `--fg-1` (13.4:1) down to `--fg-3` (5.3:1);
`--fg-4` is 3.1:1 and is **decorative or ≥24 px only**. The four Sleeper colours
are never used as text — none of them clears 4.5:1 on mist.

**Type.** Cormorant Garamond Light for titles, the outcome line and the reveal
card — soft, slightly literary, the dream half of the pairing. Inter for
everything else — every control, every string, every numeral, tabular so a
ticking timer does not shift. Five sizes: 64 / 34 / 22 / 16 / 13. **22 px is the
floor for every value on Results or the scoreboard**, because at a 720p screen
share that is 14.7 px on your friend's monitor. A value is anything read off the
screen — name, Sleeper number, score, delta, time, depth; static column labels
and captions may go smaller, since they name a thing rather than report one.

**Spacing.** A 4 px ladder, 2 → 64. Panels pad at 24, list rows at 12, clusters
gap at 8. Every HUD cluster sits 48 px from the screen edge, all four corners the
same, so the eye learns four fixed places and stops searching.

**Backgrounds.** There are no images. The background is either the live 3D view
(blurred, dark, with an inset vignette) or flat `--ink-800`. No illustrations, no
photography, no full-bleed art — that is a hard project constraint, and the mock
`DreamView` in the UI kit is deliberately just gradients and blurred rectangles so
nobody mistakes it for a plan.

**Transparency and blur.** Translucency is allocated by how long a surface
lives, not by what kind of surface it is. Permanent chrome — the Nightmare's
340px palette dock, the 320px Sleeper panel, the powers bar — is **opaque**
`--ink-800`, because USS has no `backdrop-filter` and the Screen Space Overlay
cannot sample behind itself, so a 72 % fill over a bright dream drops `--fg-3`
below AA. Only small, short-lived surfaces keep the 72 % mist: toasts, the trap
hover-peek, the reveal card, hint cards. Modal scrims are 72 % `--ink-900` — the
live one 82 %, since it cannot blur and has to carry the separation alone — and
split in two — blurred with one captured frame where the player has stepped away
from the round (Pause, Options, Round start), never blurred where the round is
still running behind them (Tab overlay, target selector). Nothing else is
transparent.

**Borders and radii.** One hairline, 1 px, in three strengths
(`--line-strong` / `--line` / `--line-faint`). Corners are 2 / 3 / 6 px — small,
because the UI is the crisp thing over a soft world. `--r-full` exists only for
rings, moons, markers and the toggle. **There are no shadows in the interior**:
depth comes from the hairline and the fill, not from a drop shadow. Panels carry
one soft outer `--shadow-panel` to lift them off the view, and glows
(`--glow-exit`, `--glow-danger`) are light, not shadow — used on the exit, the
toggle knob, the last moon and the rejection label.

**Cards.** A card is a hairline, a 3 or 6 px radius and a 32 % `--ink-900` fill.
Selected means the border goes white-gold and the fill picks up 14 % of it.
Unaffordable means 45 % opacity *and* a red badge edge — two signals, never one.

**Hover, press, focus.** Hover lightens the fill (secondary buttons pick up
`--mist-500` at 60 %, primary goes 12 % → 22 % white-gold and gains its glow).
Press deepens the fill further; **nothing scales or moves** — a control that
shrinks under the cursor is 90 ms the player did not ask for. Focus is a 2 px
white-gold border, never an outer ring.

**Animation.** Opacity and colour, ease-out on arrival, ease-in-out on loops.
Five durations, 90 ms to 900 ms. Four things pulse and nothing else does; every
value a rule depends on is forbidden from animating. The full list is in
`guidelines/motion.md`.

**Layout rules.** The HUD is four fixed corners plus a top-centre timer and a
centre crosshair — six positions, never more. The Nightmare view docks the palette
left (340 px) and the Sleepers right (320 px), full height, with the powers bar on
the bottom edge between them; the god view is the letterbox that remains. Full
screens (Lobby, Results, Options) use a 48 px page margin and a two-column split.

**Imagery vibe.** Cool, desaturated, dark, low-contrast — the 3D view is a *bed*
for the UI, not a subject. The only warm pixels on any screen belong to the exit.

---

# Iconography

**One set, drawn for this project.** 29 thin-line icons on a 24 px grid, 1.5 px
stroke, round caps and joins, `currentColor`, no fills except two intentional dots
(the target-selector centre and the connector-net doorways). They live in
`assets/icons/` as SVG and are mirrored in `components/core/Icon.jsx` so the
components are self-contained. Stroke weight stays 1.5 up to 40 px; only above
that does it go to 2.

No icon font, no CDN set, no Lucide or Heroicons substitution — the source repo
had no icons and the door states, the six powers and the connector net have no
equivalent in any general-purpose library. Nothing was borrowed and nothing was
approximated.

**Groups.** Four door states (`door-attached`, `door-fog`, `door-exit`,
`door-solid`); six powers and effects (`power-dark`, `power-fog`,
`power-molasses`, `power-trigger`, `power-possess`, `power-target`); five pack
categories (`cat-connector`, `cat-vertical`, `cat-chicane`, `cat-mob`,
`cat-gimmick`); and state glyphs (`ready`, `unready`, `role-nightmare`,
`role-sleeper`, `moon`, `crown`, `weak-point`, `depth`).

**The door states share a silhouette.** All four are the same arched doorway
frame, and only the interior changes — an opening, drifting dashes, radiating
light, or hatching. That is the §1.5 rule drawn: you can tell them apart with the
colour removed.

**The connector net** is the one piece of real information design here. A cube has
six faces, each either a doorway or a wall, and the net unfolds them into a cross:
top at the top, then west / north / east across the middle, then south, then
bottom at the bottom. A filled dot is a doorway; a dashed empty square is a wall.
It is generated from the same six-bit mask the cube data uses (`010100` is a
straight), so a new cube type gets its glyph for free — no drawing, ever.

**Emoji and Unicode:** never. **Logo:** the source repository contains no logo or
brand mark, and none was invented. Wherever a mark would go, LUCID is set in
Cormorant Garamond Light at 0.22em tracking. If someone makes a mark later, this
is the one place the system will need updating.

---

# Components

Grouped by concern under `components/`. Every one has a `.d.ts` props contract and
a `.prompt.md`; each directory has a card in the Design System tab.

**core/** — `Icon`, `MistPanel`, `Button`, `Toggle`, `Slider`, `CostBadge`

**hud/** — `TimerArc`, `HealthRing`, `MoonLives`, `Crosshair`, `DamageArc`

**nightmare/** — `CooldownRing`, `PaletteTile` (with `ConnectorNet`),
`SleeperMarker`, `RejectionLabel`

**lobby/** — `PlayerRow`, `ScoreboardRow`

**feedback/** — `Toast`, `ToastStack`

That is every component the brief asked for. `Icon`, `DamageArc` and
`ConnectorNet` are **intentional additions**: `Icon` because the glyph set needs a
single wrapper, `DamageArc` because UI.md §6 describes directional damage at the
screen edges and nothing else covered it, and `ConnectorNet` because the net is
needed outside a palette tile.

# Index

| Path | What |
|---|---|
| `styles.css` | the entry point — imports every token file |
| `tokens/` | `fonts`, `colors`, `typography`, `spacing`, `shape`, `motion` |
| `components/` | the 18 components above, by group |
| `ui_kits/lucid-game/` | all ten screens at 1920×1080 + the 1280×720 Results check ([README](ui_kits/lucid-game/README.md)) |
| `ui_kits/lucid-game/mockups.html` | six annotated in-game situations, three per role |
| `unity/DOCS-CHANGE-REQUEST.md` | fourteen edits the Lucid repo's docs needed; applied in #85, with a resolution table |
| `unity/CLAUDE-CODE-UI-GUIDE.md` | **the implementation guide for Claude Code** — read this first in Unity |
| `assets/icons/` | 29 SVGs, 24 px grid, plus the six connector nets |
| `guidelines/concept-art-prompts.md` | ten paste-ready image prompts for generating concept art |
| `guidelines/motion.md` | durations, what pulses, what must never animate |
| `guidelines/asset-checklist.md` | every file the UI needs, who makes it, how long, what is over budget |
| `guidelines/decisions-draft.md` | the defaults this system chose, now recorded in `docs/DECISIONS.md` |
| `guidelines/*.card.html` | the foundation specimen cards |
| `unity/lucid-tokens.uss` | the tokens as USS custom properties, ready to paste |
| `unity/lucid-components.uss` | USS classes for every component |
| `unity/uxml/` | a UXML skeleton per screen |
| `unity/Runtime/*.cs` | `LucidRing`, `LucidConnectorNet`, `LucidStrings` — the parts USS cannot do |
| `github.md` | source repository and sync record |
| `SKILL.md` | use this system as an Agent Skill |

# Using this in Unity

Start at **[unity/CLAUDE-CODE-UI-GUIDE.md](unity/CLAUDE-CODE-UI-GUIDE.md)**. It is
the design authority for the Unity implementation: the rules that do not bend,
the file layout, a component-to-class table, the six fixed HUD positions, the
motion contract with its never-animate list, per-screen notes, and a
pre-pull-request checklist. `unity/Runtime/LucidStrings.cs` is `docs/UI.md` §14
transcribed, so no screen ever improvises a string.


`unity/lucid-tokens.uss` and `unity/lucid-components.uss` are written against what
USS actually supports: flexbox, borders, radii, background images, opacity,
transitions with the five predefined easings. There is **no** grid, no `calc()`,
no `color-mix()`, no pseudo-element content, no web fonts by URL, no runtime SVG
filters. Every alpha is an explicit `rgba()` literal and every duration is in
seconds. Arcs and rings are the one thing USS cannot express — they are a single
`VisualElement` subclass using `Painter2D.Arc`, reading the `--ring-*` properties
the stylesheet sets.
