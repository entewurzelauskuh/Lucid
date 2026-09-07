# Lucid — game UI kit

Every screen in `docs/UI.md`, at 1920×1080, built from the components in
`components/`. Open `index.html` and use the bar at the bottom to move between
them; the picker remembers where you were.

| Screen | File | Notes |
|---|---|---|
| Title | `TitleScreen.jsx` | wordmark over the bedroom's breathing fog door |
| Lobby | `LobbyScreen.jsx` | role + ready per player, host settings, session leaderboard. Try clicking your own row's role cards and ready chip — the Start button's label is the current blocker |
| Round start | `RoundStartScreen.jsx` | 3-2-1 then the reveal card |
| Sleeper HUD | `SleeperHud.jsx` | three states: running, Dark active, last 30 s (plus the Tab overlay) |
| Spectator | `SpectatorScreen.jsx` | consumed, watching someone else; their vitals are quoted, not owned |
| Nightmare view | `NightmareView.jsx` | idle, placing with a rejected ghost, and the Possession overlay. The target selector opens from "Who feels it" |
| Results | `ResultsScreen.jsx` | `compact` renders the 1280×720 check in `results-720.html` |
| Pause | `PauseScreen.jsx` | |
| Options | `OptionsScreen.jsx` | the Accessibility tab is live — toggle marker shapes and watch the four Sleepers |

`Shared.jsx` holds `DreamView` (the neutral blurred stand-in for the 3D view —
gradients and blurred rectangles, never an illustration), `Scrim`,
`ClusterScrim` (the "light chrome" HUD wash) and `PhaseBanner`.

The maze in the god view is drawn from a hand-written cell list, not from real
generation output. It exists to show the marker, ghost-cube and door language at
the right density — do not read anything into its shape.

## In-game mockups

`mockups.html` holds six situations, three per role, each captioned with the
rules that produce it. They are the reference for *why* a screen looks a
particular way at a particular moment, and every number in them is a real value
from `docs/SPEC.md` — costs, cooldowns, HP, lives, depth.

| | Situation |
|---|---|
| **N1** | Head start, 0:12 in. Starting budget, three cubes, a legal green ghost, every Sleeper still at depth 0. |
| **N2** | Budget 3 against a Crusher costing 4 while Anna stands at an exit door. Red ghost with the verbatim reason; Molasses mid-cooldown. |
| **N3** | Dawn in 0:24, one Sleeper left at depth 13. Trapdoor trigger off cooldown under the cursor, his row expanded to the hover-peek. |
| **S1** | The bedroom during the head start. Countdown projected on the misted door, hint cards along the bottom, one moon. |
| **S2** | Molasses at 70 % with 4 s left, shooting out a Trapdoor's latch — four of six shots. |
| **S3** | Dark in the last 0:18. HUD at 32 % except the timer, Nightlight the only source, exit white and close, 24 HP. |

All six are also in `index.html`'s nav.

### Corrections applied from a second read of the docs

The first pass had copy and rule errors, now fixed: lives default to **1** (not
3), budget sits **top left** per §8, Results titles are exactly *Everyone woke
up* / *Dawn.* / *Consumed*, possession reads *Possessing a Shade in Anna's dream
— P to let go* with *Building paused* under the budget, the spectator line is
*You're awake. Watch the others.*, Title includes **Sandbox**, Pause offers
Options / Leave / Quit to desktop, and the palette now carries the real MVP cube
set with its real costs and connector masks.
