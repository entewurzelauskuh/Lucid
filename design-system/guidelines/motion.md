# Motion

Five durations, two easings, and a short list of things that must hold still.
Everything here is expressible in USS: `transition-property`, `transition-duration`,
`transition-timing-function` with Unity's predefined easings. No arbitrary curves,
no spring physics, no shader-driven UI motion.

## The durations

| Token | Value | Used by |
|---|---|---|
| `--dur-instant` | 90 ms | button press, chip and tab select, the rejection label appearing |
| `--dur-fast` | 140 ms | hover, focus, toggle knob, role card lighting up |
| `--dur-base` | 220 ms | panel in/out, toast in/out, ghost cube green↔red, health ring redraw |
| `--dur-slow` | 400 ms | screen fade, layer cut-away, Tab overlay |
| `--dur-veil` | 900 ms | the white-out on waking, the sink to black on being consumed |

Easing is `ease-out` for anything arriving and `ease-in-out` for anything that
loops. Nothing uses `linear` except a cooldown ring, which must be linear because
a player reads time off it.

## What pulses

Four things, and only four.

1. **The dawn arc in the last 30 s** — `lucid-pulse`, 1200 ms, ease-in-out, opacity 1 → 0.45. The arc and its glow pulse. **The digits do not.**
2. **The last crescent moon** — `lucid-breathe`, 2600 ms, opacity 1 → 0.62. The only always-on animation in the Sleeper HUD. Spent moons never move.
3. **The exit, in the world and on the god view** — a slow radiance, 2400 ms. This is the one thing allowed to draw the eye across a whole screen.
4. **The Title screen's fog door** — `lucid-breathe` at `--pulse-door`, purely decorative, gone the moment the Lobby opens.

Health regen adds a fifth, conditional one: `lucid-shimmer` on the health ring
for the four seconds it is refilling, then it stops.

## What fades

- **Toasts**: in over `--dur-base`, hold 4 s, out over `--dur-base`. Newest on top; the ones behind step down to 0.78 and 0.56 opacity rather than moving.
- **Damage arc**: appears instantly at the hit, fades over `--dur-base`. Instant in, because a hit you did not notice is a rule you did not see.
- **Panels and modals**: opacity only, `--dur-base`. No slide, no scale, no bounce. A panel that flies in costs 200 ms of reading time.
- **Screens**: cross-fade through black over `--dur-slow`.

## What must never animate

These carry rules. If a player has to wait for a value to settle before they can
trust it, the animation has cost them the round.

- **The dawn numerals.** The arc pulses; `3:12` is always exactly `3:12`, tabular, same position, same size.
- **The budget number.** It steps 11 → 12 in one frame. The *trickle ring* animates; the integer never tweens.
- **A cost badge.** Never counts up or down.
- **The placement rejection label.** Appears in the same frame as the red ghost, no fade. The Nightmare is reading it mid-drag.
- **Door state.** A door hardening is a cut, not a dissolve — the toast explains it, the geometry changes at once.
- **Scoreboard and Results figures.** No count-up. The screen is being read over a screen share by four people at once; a rolling number is unreadable to all of them.
- **The weak-point ring.** It tracks damage 1:1 and linearly. It is a gauge, not a flourish.
- **Sleeper marker colour, number or name.** Position interpolates; identity does not.

## Reduce motion

The Options toggle stops 1, 2 and 4 above, plus the regen shimmer. It changes
nothing else — the exit keeps its radiance because it is a game rule, and toasts
keep their fade because instantly-appearing text is harder to read, not easier.
