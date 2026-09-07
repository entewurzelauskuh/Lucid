# Docs change request — `entewurzelauskuh/Lucid`

**For Claude Code.** Fourteen edits the repo's own documentation needs before the UI
implementation can be consistent with itself. Every item came out of actually building
the screens: each one is a place where the docs are silent, ambiguous, or — in one case —
ask for something UI Toolkit cannot do.

Work top to bottom. **Item 1 is a rule marked `[S]` that the platform cannot honour as
written**; it needs a human decision, not a patch. Items 4 and 4a also need sign-off.
Everything else is safe to write down as-is.

## Resolution (2026-09-07, #85)

Applied to `docs/` on branch `m0/85-design-system-docs`, one commit per file.
Where a "currently says" quote did not match the live doc, the doc had not
moved — the quote had misread its marker:

| Item | Outcome |
|---|---|
| 1 | **Applied, not escalated.** `docs/UI.md` §15 is `[D]`, not `[S]`; the lifetime rule is written in. |
| 2, 3, 5, 6, 9 | Applied. §1, §9 and §14 are unmarked and §15 is `[D]`, so none of these touched settled text. |
| 4, 4a | **Held**, per the owner. Dark's two levels land in `docs/DECISIONS.md` as *proposed*. |
| 7 | **Reconciled.** §11 already listed both toggles, with the hatch on *fog*, not `solid` (the guide's own §7 agrees). Added: both default off, and the two-control rule. |
| 8 | Applied. The three "gaps" were mostly already in §14 — the lobby blockers and the placement reasons are there verbatim. §14 gained the nine strings `LucidStrings.cs` had harvested from §2, §4, §7–§10 and §13, and five section captions. The UXML skeletons' twenty-two invented strings were **not** added; they are the skeletons' bug. |
| 10, 11 | Applied. 11 is the one genuine `[S]` edit and has a DECISIONS entry. |
| 12 | Rule 5 amended for OFL fonts; the ledger rows go in `THIRD_PARTY_NOTICES.md` when the files land, since rule 5's ledger is per cube. |
| 13 | The nineteen entries land as one entry in that file's format; Dark marked proposed. |
| 14 | Written: `docs/UI-TOOLKIT-LIMITS.md`. |

Also corrected in this folder: the guide's §5 put `depth · exit` on the Sleeper
HUD (it is the Nightmare's row); §1's layout would have landed the C# outside
every assembly; §4's `connectorMask` field does not exist.

## How to use this file

1. **Re-read each section before editing it.** The "currently says" quotes below are from
   a read of `docs/` at branch `main` on **2026-09-05**, with no commit sha recorded. If a
   quote does not match, the doc moved — reconcile against the *intent* stated in "Why",
   and say so in your PR rather than forcing the text.
2. **Respect the markers.** `[S]` is settled: never silently reinterpret one. Where an
   item below touches `[S]` text it says **ESCALATE** and gives you the exact question to
   put to the owner. `[D]` is open — those you may simply write down.
3. **One PR, one commit per file**, so a rejected item can be dropped without unpicking
   the rest.
4. Items marked **SIGN-OFF** carry a number this design system invented. Do not implement
   them as settled until a human agrees; quote the alternative given.

| File | Items |
|---|---|
| `docs/UI.md` | 1, 2, 3, 4, 5, 6, 7, 8, 9 |
| `docs/SPEC.md` | 10, 11 |
| `CLAUDE.md` | 12 |
| `docs/DECISIONS.md` | 13 |
| `docs/UI-TOOLKIT-LIMITS.md` *(new, optional)* | 14 |

---

## 1. §15 — "panels are translucent blue-black mist" cannot be built as a blanket rule · **ESCALATE**

**Currently says** (§15, marked `[S]`): panels are translucent blue-black mist with thin
light borders.

**The problem.** USS has no `backdrop-filter`, and it cannot be added: the UI is composited
in a Screen Space Overlay pass *after* the camera, so a panel has nothing to sample from.
"Translucent mist" therefore renders as flat 72 %-opaque navy over the 3D view — which is
not the intent, and on the Nightmare's permanent docks it is actively bad: a busy god view
reads straight through the palette the player is trying to aim at.

Getting true blur means a Render Texture and a custom URP blit for every panel, re-run
whenever the view moves. That is a rendering feature, not a UI one, and the brief scopes
the 3D post-process out.

**Proposed amendment** — translucency allocated by *lifetime*, not by surface type:

- **Permanent chrome** (Nightmare docks, powers bar, scoreboard panel) → opaque `--ink-800`.
- **Small transient surfaces** (toasts, hover peeks, the reveal card, hint cards) → keep
  the 72 % mist. They are small, brief, and never sit over anything being aimed at.
- **Modal scrims** → one full-screen captured frame, blurred once on open. Splits in two:
  `.scrim--frozen` for modals the player has stepped away from (Pause, Options, Round
  start) and `.scrim--live`, never blurred, for modals over a running round (Tab overlay,
  target selector) — a frozen frame behind live play misreports the game state.

This keeps the *look* of §15 everywhere it is affordable and **removes any need for a
custom URP render feature in the UI**.

**The question for the owner:** may §15's translucency rule be re-marked from `[S]` to
"`[S]` in spirit, `[D]` in execution", with the lifetime rule above written in beneath it?
If the answer is no, the alternative is a Render Texture blur pass and someone owning it.

---

## 2. §15 — write down the type scale and define the 22 px floor

**Currently says:** suggests Cormorant Garamond for titles and Inter for body; no sizes.

**Add:** five sizes, no exceptions — **64 / 34 / 22 / 16 / 13 px**, display family at 64 and
34, body family throughout. Then the floor, stated carefully, because the obvious phrasing
is unimplementable:

> **22 px is the floor for every *value* on Results and the scoreboard**, because those
> screens are read over a Discord screen share at 720p — 0.667×, so 22 px authored is
> 14.7 px on a friend's monitor. A *value* is anything a viewer reads off the screen: a
> name, a Sleeper number, a score, a delta, a time, a depth. Static column labels and
> section captions ("THE NIGHTMARE", "fastest wake") may sit at 13 px, because they name a
> thing rather than report one.

**Why the distinction matters:** written as "nothing below 22 px", the rule fails the
moment you add a column header, and an implementer will then ignore all of it. Sleeper
numbers are explicitly values — they are what makes the four colours colour-blind-safe, so
they may never be the smallest text on the screen.

---

## 3. §15 — three missing rules about the Sleeper colours

**Currently says** (`[S]`): the four colour-blind-safe values, always paired with a number
and a name.

**Add all three:**

1. **Chips are filled with the raw colour, never darkened toward the numeral.** A
   `color-mix(… 88 %, black)` fill put `--sleeper-3` (#009E73) at **4.46:1** against the
   near-black number inside it. Raw, all four clear AA: **8.52 / 8.32 / 5.61 / 6.27**. The
   number in the chip is the one element that may never fail, since it is what carries the
   identity when hue does not.
2. **None of the four is ever used as text.** No Sleeper colour clears 4.5:1 as type on
   mist. They appear only as chips, markers and strokes; the name beside them is `--fg-1`.
3. **The Nightmare has no number, but still reserves the chip's footprint.** Numbers go to
   Sleepers only — so on a scoreboard row the Nightmare's 32 px chip slot is emitted
   *unpainted*. Omitting the element outdents that one name by chip + gap, and one player
   is the Nightmare every round, so the ragged column would show in every session.

---

## 4. §6 — Dark needs two dim levels, not one · **SIGN-OFF**

**Currently says:** Dark dims the HUD except the dawn timer.

**The problem.** Implemented as a flat dim, the natural value (32 %) puts the health ring
and the crescent lives at roughly **1.7:1** — they are not dimmed, they are erased. §1.3
forbids hiding anything a rule depends on reading, and how much health you have left is
exactly that. So "dims the HUD" and §1.3 conflict, and the doc does not say which wins.

**Proposed:** two levels.

| Layer | Under Dark |
|---|---|
| Key hints, toasts, cluster scrims | 32 % |
| Health ring, crescent lives | **60 %** |
| Dawn timer | untouched |

**These numbers are ours, not yours** — §6 only says "dims". If you want Dark to bite
harder, the honest alternative is hints at 32 % with the two rule-carrying readouts held at
60 %, which is what is implemented now; going below ~55 % on those two reopens the
contrast failure. Please confirm or replace the numbers.

### 4a. §1 — add the general form of that rule · **SIGN-OFF**

The Dark conflict is one instance of a rule §1 implies but never states. Suggested wording:

> **Anything a rule depends on reading stays readable.** No effect, overlay or dim may take
> a rule-carrying element below 4.5:1, and nothing a rule depends on may animate while the
> player needs to read it. This outranks any atmospheric effect, including Dark.

That single clause resolves item 4, the "never animate" list in the motion notes, and every
future power that wants to obscure the HUD.

---

## 5. §15 — four font files, not two, and the licence lines

**Currently says:** Cormorant Garamond for titles, Inter for body.

**Add:** Unity exposes **no OpenType feature switches** — you cannot ask TextCore for
`tnum` or `lnum` from a style property. Tabular figures matter here: any value that ticks
(the dawn timer, budget, health, scores) jitters horizontally with proportional figures.

So the features are frozen into two extra files offline, and the shipped set is four:

| File | Features | Used for |
|---|---|---|
| `Inter[…].ttf` | as published | all prose |
| `LucidInter-Tabular.ttf` | `tnum` frozen | anything that ticks — class `.tabular` |
| `CormorantGaramond-Light.ttf` | as published | display prose |
| `LucidCormorant-Lining.ttf` | `lnum` + `tnum` frozen | display numerals — class `.lining` |

Both source families are **OFL**, so the derivatives are fine to ship provided the licence
travels with them and the reserved-name rule is respected — hence the `Lucid…` prefix
rather than "Inter Tabular". Command lines are in `CLAUDE-CODE-UI-GUIDE.md` §3.

---

## 6. §1 — state the crisp-UI rule as a compositing rule

**Currently says** (`[S]`): the world is painted and the UI is crisp.

**Add one sentence**, because it is the thing most likely to get quietly broken: *the UI is
composited in a Screen Space Overlay pass and receives none of the world's painterly pass,
bloom, grain or depth of field — no UI element may be blurred, tinted or post-processed to
"match" the world.* The contrast between soft world and sharp interface is the look; a
softened HUD reads as a rendering bug.

---

## 7. §11 — Options is missing two accessibility toggles

**Currently says:** the Options categories, with the accessibility contents left open
(`[D]`).

**Add**, both defaulting off, both implemented:

- **Colour-blind marker shapes** — gives each Sleeper index its own chip corner radius as
  well as its colour. *Off* by default: the four §15 colours are already colour-blind-safe
  and always carry a number and a name, so this is a second belt rather than the first one.
- **High-contrast doors** — adds a faint hatch pattern to `solid` and thickens the `exit`
  rim, for players who want the four door states separated by more than mist density.

Also worth recording in §11: Options is built from exactly two controls, `.toggle` and
`.slider`. Every setting must express itself as one of those or it does not belong here.

---

## 8. §14 — confirm one glossary string and add three that the screens needed

**§14 is `[S]` and verbatim**, so nothing here is a change — these are gaps found by
building screens that needed strings §14 does not contain.

1. **Confirm `Possessing(mob, owner)`'s mob argument.** The mockups render it as
   *"Possessing a Shade in Anna's dream — P to let go"*. A Shade is the only mob a Nest
   spawns, so that was the assumption; confirm it is the right noun and that the em-dash
   hint is part of the string rather than layout.
2. **Blocked-button reasons.** A button that cannot be pressed replaces its own label with
   the reason ("Waiting for Dev to ready up") rather than greying out — §1.3 applied to
   controls instead of only to placements. Those reason strings need to live in §14.
3. **Placement-rejection reasons.** The Nightmare's rejected-placement label quotes the
   reason verbatim; the full set of reasons (out of budget, no connection, overlaps, too
   close to a Sleeper) should be enumerated in §14 rather than composed at the call site.

Anything not in §14 is a string an implementer will invent, and then a designer will
translate. `Runtime/LucidStrings.cs` in this folder is §14 transcribed and is the only
place copy may live in the Unity project.

---

## 9. §9 — Results: say that the figures must derive from one record

**Currently says:** the screen's contents, `[S]`, with layout `[D]`.

**Add an implementation note.** The player cards, the leaderboard deltas and the badges are
three views of one round; written as three separate blocks of numbers they *will* drift.
Building it that way here, three of four players disagreed with themselves in the first
pass. So:

> Results is computed from a single session record with SPEC §12 applied in code — a
> Sleeper who wakes scores 100 + remaining seconds, a consumed Sleeper scores 0, the
> Nightmare scores 100 per Sleeper consumed. Every figure on the screen, including the
> badges, derives from that record. Two invariants hold for any session and are worth
> asserting in a test: Nightmare stints across all players equals rounds played, and each
> player's woke + consumed equals rounds played minus their own stints.

Worth also naming in §9 what the Nightmare's card carries — Sleepers consumed, points,
cubes placed, deepest cube reached — since it is the one card with no wake time.

---

## 10. SPEC §15 — record the render split and the hardware target

**Add**, so nobody plans a UI feature that needs a render pass:

- The painterly pass, bloom, grain and depth of field are **world-only**. The UI is a
  Screen Space Overlay composited afterwards and is deliberately untouched by all of it.
- **No custom URP render feature is required for the UI** (see item 1). If one is
  introduced for the world, the UI must not start depending on it.
- The two views have genuinely different renderers and should be described that way:
  **the Nightmare's god view is 2.5D, near-top-down**, and **the Sleeper's view is full 3D
  first person**. They share the colour grade and the painterly pass, not a camera rig.
- Development target is a **Ryzen 7 7800X3D / RX 9060 XT** class machine. Nothing in the UI
  is close to a budget concern at that spec — the constraint driving item 1 is API surface,
  not performance.

---

## 11. SPEC §7 — spell out the non-hue channel for each door state

**Currently says** (`[S]`): door states never rely on hue alone.

**Add the channel**, so four implementers do not invent four encodings. Each state must be
identifiable in a greyscale screenshot:

| State | Hue | Non-hue channel |
|---|---|---|
| `fog` | grey-blue | a drifting mist **sheet** filling the opening |
| `exit` | white-gold | the same sheet, **blazing** — the only warm light in the frame |
| `solid` | room material | opening **condensed to blank wall**, faintly hatched |
| `attached` | none | an **open** passage, nothing in it |

The rule is really "state is legible in greyscale". Adding that sentence makes it testable:
desaturate a screenshot and all four are still distinguishable.

---

## 12. `CLAUDE.md` rule 5 — the asset ledger is missing five files

Rule 5 requires a ledger line per asset. These exist in the design system and need ledger
entries with maker, tool and licence:

| Asset | Format | Licence |
|---|---|---|
| `LucidInter-Tabular.ttf` | TTF | OFL derivative (`pyftfeatfreeze`) |
| `LucidCormorant-Lining.ttf` | TTF | OFL derivative (`pyftfeatfreeze`) |
| `dash-border.png` | 9-sliced sprite | CC0, made in-house |
| `icons.png` + sub-rect map | sprite atlas, 29 icons | CC0, made in-house |
| `moon.png` | sprite | CC0, made in-house |

`guidelines/asset-checklist.md` in the design system has the full list with effort
estimates and flags anything exceeding "one person, vector tools, a weekend".

---

## 13. `docs/DECISIONS.md` — nineteen entries ready to paste

`guidelines/decisions-draft.md` in the design system holds one line per deviation from a
`[D]` suggestion, each saying what it replaces and why. Paste them in. Two of them —
Dark's two dim levels, and the general readability clause from item 4a — are the SIGN-OFF
items above and should land as "proposed" until confirmed.

---

## 14. Optional: `docs/UI-TOOLKIT-LIMITS.md`

Three platform limits shaped several decisions above, and they will be rediscovered by
whoever next tries the obvious thing. A short doc saves that:

- **No `backdrop-filter`**, and it cannot be added → item 1.
- **No OpenType feature switches** → item 5.
- **No dash array in Painter2D and no `border-style` in USS** → dashed boxes use one
  9-sliced sprite; the connector net distinguishes a wall by a fainter stroke and a missing
  dot, which reads better at 12 px than dashes anyway.
- Also worth listing: **no `@keyframes`** — the four pulses in the motion notes are small
  C# tweens on `style.opacity`.

---

## Do not change

- Anything `[S]` except where an item above says **ESCALATE**, and then only by asking.
- §14 strings, in any way, for any reason. They are the glossary; the UI quotes them.
- The four Sleeper colours, the three accents, the door-state semantics, the 8 m cube.
- Gameplay, cube inventory, mobs, powers, costs, the 3D post-process — out of scope for
  this request and for the design system that raised it.

## Done when

- [ ] Item 1 answered by a human, and §15 reads consistently with whatever they chose
- [ ] Items 4 and 4a either confirmed with numbers or replaced with the owner's
- [ ] §15 states the type scale, the value floor, and the four font files
- [ ] §15 carries the three Sleeper-colour rules from item 3
- [ ] §6 states two dim levels; §1 carries the readability clause
- [ ] §11 lists both accessibility toggles and the two-control rule
- [ ] §14's three gaps closed and `Possessing`'s mob confirmed
- [ ] §9 requires one session record; SPEC §7 lists the non-hue channels
- [ ] SPEC §15 records the render split, the two view types and the hardware target
- [ ] `CLAUDE.md` rule 5 has the five ledger lines
- [ ] `DECISIONS.md` carries all nineteen entries, two marked proposed
- [ ] Every quote above re-checked against the live doc; mismatches reported, not forced
